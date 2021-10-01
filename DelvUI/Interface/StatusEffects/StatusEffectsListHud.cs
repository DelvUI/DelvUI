using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using LuminaStatus = Lumina.Excel.GeneratedSheets.Status;
using StatusStruct = FFXIVClientStructs.FFXIV.Client.Game.Status;

namespace DelvUI.Interface.StatusEffects
{
    public class StatusEffectsListHud : DraggableHudElement, IHudElementWithActor
    {
        protected new StatusEffectsListConfig Config => (StatusEffectsListConfig)base.Config;

        private LayoutInfo _layoutInfo;
        private bool _showingTooltip = false;

        internal static int StatusEffectListsSize = 30;
        private StatusStruct[]? _fakeEffects = null;

        private readonly LabelHud _durationLabel;
        private readonly LabelHud _stacksLabel;

        public GameObject? Actor { get; set; } = null;

        public StatusEffectsListHud(string id, StatusEffectsListConfig config, string displayName) : base(id, config, displayName)
        {
            base.Config.ValueChangeEvent += OnConfigPropertyChanged;

            _durationLabel = new LabelHud(id + "_duration", config.IconConfig.DurationLabelConfig);
            _stacksLabel = new LabelHud(id + "_stacks", config.IconConfig.StacksLabelConfig);

            UpdatePreview();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            
            if (!disposing)
            {
                base.Config.ValueChangeEvent -= OnConfigPropertyChanged;
                return;
            }
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            Vector2 pos = CalculateStartPosition(Config.Position, Config.Size, Config.GetGrowthDirections());
            return (new List<Vector2>() { pos + Config.Size / 2f }, new List<Vector2>() { Config.Size });
        }

        private uint CalculateLayout(List<StatusEffectData> list)
        {
            uint effectCount = (uint)list.Count;
            uint count = Config.Limit >= 0 ? Math.Min((uint)Config.Limit, effectCount) : effectCount;

            if (count <= 0)
            {
                return 0;
            }

            _layoutInfo = LayoutHelper.CalculateLayout(
                Config.Size,
                Config.IconConfig.Size,
                count,
                Config.IconPadding,
                Config.FillRowsFirst
            );

            return count;
        }

        protected virtual List<StatusEffectData> StatusEffectsData()
        {
            List<StatusEffectData>? list = StatusEffectDataList(Actor);

            // show mine first
            if (Config.ShowMineFirst)
            {
                OrderByMineFirst(list);
            }

            return list;
        }

        protected unsafe List<StatusEffectData> StatusEffectDataList(GameObject? actor)
        {
            var list = new List<StatusEffectData>();

            BattleChara? character = null;

            if (_fakeEffects == null)
            {
                if (actor == null || actor is not BattleChara)
                {
                    return list;
                }

                character = (BattleChara)actor;
            }

            Dalamud.Game.ClientState.Objects.SubKinds.PlayerCharacter? player = Plugin.ClientState.LocalPlayer;
            int count = StatusEffectListsSize;
            if (_fakeEffects != null)
            {
                count = Config.Limit == -1 ? _fakeEffects.Length : Math.Min(Config.Limit, _fakeEffects.Length);
            }

            for (int i = 0; i < count; i++)
            {
                // status
                StatusStruct* status = null;

                if (_fakeEffects != null)
                {
                    StatusStruct fakeStruct = _fakeEffects![i];
                    status = &fakeStruct;
                }
                else
                {
                    status = (StatusStruct*)character!.StatusList[i]?.Address;
                }

                if (status == null || status->StatusID == 0)
                {
                    continue;
                }

                // data
                LuminaStatus? data = null;

                if (_fakeEffects != null)
                {
                    data = Plugin.DataManager.GetExcelSheet<LuminaStatus>()?.GetRow(status->StatusID);
                }
                else
                {
                    data = character!.StatusList[i]?.GameData;
                }
                if (data == null)
                {
                    continue;
                }

                // dont filter anything on preview mode
                if (_fakeEffects != null)
                {
                    list.Add(new StatusEffectData(*status, data));
                    continue;
                }

                // buffs
                if (!Config.ShowBuffs && data.Category == 1)
                {
                    continue;
                }

                // debuffs
                if (!Config.ShowDebuffs && data.Category != 1)
                {
                    continue;
                }

                // permanent
                if (!Config.ShowPermanentEffects && data.IsPermanent)
                {
                    continue;
                }

                // only mine
                if (Config.ShowOnlyMine && player?.ObjectId != status->SourceID)
                {
                    continue;
                }

                // blacklist
                if (Config.BlacklistConfig.Enabled && !Config.BlacklistConfig.StatusAllowed(data))
                {
                    continue;
                }

                list.Add(new StatusEffectData(*status, data));
            }

            return list;
        }

        protected void OrderByMineFirst(List<StatusEffectData> list)
        {
            Dalamud.Game.ClientState.Objects.SubKinds.PlayerCharacter? player = Plugin.ClientState.LocalPlayer;
            if (player == null)
            {
                return;
            }

            list.Sort((a, b) =>
            {
                bool isAFromPlayer = a.Status.SourceID == player.ObjectId;
                bool isBFromPlayer = b.Status.SourceID == player.ObjectId;

                if (isAFromPlayer && !isBFromPlayer)
                {
                    return -1;
                }

                if (!isAFromPlayer && isBFromPlayer)
                {
                    return 1;
                }

                return 0;
            });
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (!Config.Enabled)
            {
                return;
            }

            if (_fakeEffects == null && (Actor == null || Actor.ObjectKind != ObjectKind.Player && Actor.ObjectKind != ObjectKind.BattleNpc))
            {
                return;
            }

            // calculate layout
            List<StatusEffectData>? list = StatusEffectsData();

            // area
            GrowthDirections growthDirections = Config.GetGrowthDirections();
            Vector2 position = origin + Config.Position;
            Vector2 areaPos = CalculateStartPosition(position, Config.Size, growthDirections);
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            if (Config.Preview)
            {
                drawList.AddRectFilled(areaPos, areaPos + Config.Size, 0x88000000);
            }

            // no need to do anything else if there are no effects
            if (list.Count == 0)
            {
                return;
            }

            // calculate icon positions
            uint count = CalculateLayout(list);
            var iconPositions = new List<Vector2>();
            var minPos = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 maxPos = Vector2.Zero;

            int row = 0;
            int col = 0;

            for (int i = 0; i < count; i++)
            {
                CalculateAxisDirections(growthDirections, row, count, out Vector2 direction, out Vector2 offset);

                var pos = new Vector2(
                    position.X + offset.X + Config.IconConfig.Size.X * col * direction.X + Config.IconPadding.X * col * direction.X,
                    position.Y + offset.Y + Config.IconConfig.Size.Y * row * direction.Y + Config.IconPadding.Y * row * direction.Y
                );

                minPos.X = Math.Min(pos.X, minPos.X);
                minPos.Y = Math.Min(pos.Y, minPos.Y);
                maxPos.X = Math.Max(pos.X + Config.IconConfig.Size.X, maxPos.X);
                maxPos.Y = Math.Max(pos.Y + Config.IconConfig.Size.Y, maxPos.Y);

                iconPositions.Add(pos);

                // rows / columns
                if (Config.FillRowsFirst || (growthDirections & GrowthDirections.Centered) != 0)
                {
                    col += 1;
                    if (col >= _layoutInfo.TotalColCount)
                    {
                        col = 0;
                        row += 1;
                    }
                }
                else
                {
                    row += 1;
                    if (row >= _layoutInfo.TotalRowCount)
                    {
                        row = 0;
                        col += 1;
                    }
                }
            }

            // window
            ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoTitleBar;
            windowFlags |= ImGuiWindowFlags.NoMove;
            windowFlags |= ImGuiWindowFlags.NoDecoration;
            windowFlags |= ImGuiWindowFlags.NoBackground;
            windowFlags |= ImGuiWindowFlags.NoBackground;
            if (!Config.ShowBuffs)
            {
                windowFlags |= ImGuiWindowFlags.NoInputs;
            }

            // imgui clips the left and right borders inside windows for some reason
            // we make the window bigger so the actual drawable size is the expected one
            var margin = new Vector2(14, 10);
            Vector2 windowPos = minPos - margin;
            Vector2 windowSize = maxPos - minPos;
            ImGui.SetNextWindowPos(windowPos, ImGuiCond.Always);
            ImGui.SetNextWindowSize(windowSize + margin * 2);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
            ImGui.Begin("statusEffectList " + ID, windowFlags);

            // draw
            drawList = ImGui.GetWindowDrawList();
            bool showingTooltip = false;

            for (int i = 0; i < count; i++)
            {
                Vector2 iconPos = iconPositions[i];
                StatusEffectData statusEffectData = list[i];

                StatusEffectIconDrawHelper.DrawStatusEffectIcon(drawList, iconPos, statusEffectData, Config.IconConfig, _durationLabel, _stacksLabel);

                if (Actor != null && ImGui.IsMouseHoveringRect(iconPos, iconPos + Config.IconConfig.Size))
                {
                    // tooltip
                    if (Config.ShowTooltips)
                    {
                        TooltipsHelper.Instance.ShowTooltipOnCursor(statusEffectData.Data.Description, statusEffectData.Data.Name);
                        showingTooltip = true;
                    }

                    // remove buff on right click
                    bool isFromPlayer = statusEffectData.Status.SourceID == Plugin.ClientState.LocalPlayer?.ObjectId;

                    if (statusEffectData.Data.Category == 1 && isFromPlayer && ImGui.GetIO().MouseClicked[1])
                    {
                        ChatHelper.SendChatMessage("/statusoff \"" + statusEffectData.Data.Name + "\"");
                    }

                    // automatic add to black list with ctrl+alt+shift click
                    if (Config.BlacklistConfig.Enabled &&
                        ImGui.GetIO().KeyCtrl && ImGui.GetIO().KeyAlt && ImGui.GetIO().KeyShift && ImGui.GetIO().MouseClicked[0])
                    {
                        Config.BlacklistConfig.AddNewEntry(statusEffectData.Data);
                    }
                }

            }

            ImGui.End();
            ImGui.PopStyleVar();

            if (_showingTooltip && !showingTooltip)
            {
                TooltipsHelper.Instance.RemoveTooltip();
            }
            _showingTooltip = showingTooltip;
        }

        private void CalculateAxisDirections(GrowthDirections growthDirections, int row, uint elementCount, out Vector2 direction, out Vector2 offset)
        {
            if ((growthDirections & GrowthDirections.Centered) != 0)
            {
                int elementsPerRow = (int)(Config.Size.X / (Config.IconConfig.Size.X + Config.IconPadding.X));
                long elementsInRow = Math.Min(elementsPerRow, elementCount - (elementsPerRow * row));

                direction.X = 1;
                direction.Y = (growthDirections & GrowthDirections.Down) != 0 ? 1 : -1;
                offset.X = -(Config.IconConfig.Size.X + Config.IconPadding.X) * elementsInRow / 2f;
                offset.Y = direction.Y == 1 ? 0 : -Config.IconConfig.Size.Y;
            }
            else
            {
                direction.X = (growthDirections & GrowthDirections.Right) != 0 ? 1 : -1;
                direction.Y = (growthDirections & GrowthDirections.Down) != 0 ? 1 : -1;
                offset.X = direction.X == 1 ? 0 : -Config.IconConfig.Size.X;
                offset.Y = direction.Y == 1 ? 0 : -Config.IconConfig.Size.Y;
            }
        }

        private Vector2 CalculateStartPosition(Vector2 position, Vector2 size, GrowthDirections growthDirections)
        {
            Vector2 area = size;
            if ((growthDirections & GrowthDirections.Left) != 0)
            {
                area.X = -area.X;
            }

            if ((growthDirections & GrowthDirections.Up) != 0)
            {
                area.Y = -area.Y;
            }

            Vector2 startPos = position;
            if ((growthDirections & GrowthDirections.Centered) != 0)
            {
                startPos.X = position.X - size.X / 2f;
            }

            Vector2 endPos = position + area;

            if (endPos.X < position.X)
            {
                startPos.X = endPos.X;
            }

            if (endPos.Y < position.Y)
            {
                startPos.Y = endPos.Y;
            }

            return startPos;
        }

        private void OnConfigPropertyChanged(object? sender, OnChangeBaseArgs args)
        {
            if (args.PropertyName == "Preview")
            {
                UpdatePreview();
            }
        }

        private unsafe void UpdatePreview()
        {
            if (!Config.Preview)
            {
                _fakeEffects = null;
                return;
            }

            var RNG = new Random((int)ImGui.GetTime());
            _fakeEffects = new StatusStruct[StatusEffectListsSize];

            for (int i = 0; i < StatusEffectListsSize; i++)
            {
                var fakeStruct = new StatusStruct();
                fakeStruct.RemainingTime = RNG.Next(1, 30);
                fakeStruct.StatusID = (ushort)RNG.Next(1, 200);
                fakeStruct.StackCount = (byte)RNG.Next(0, 3);
                fakeStruct.SourceID = 0;

                _fakeEffects[i] = fakeStruct;
            }
        }
    }

    public struct StatusEffectData
    {
        public StatusStruct Status;
        public LuminaStatus Data;

        public StatusEffectData(StatusStruct status, LuminaStatus data)
        {
            Status = status;
            Data = data;
        }
    }

    [Flags]
    public enum GrowthDirections : short
    {
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8,
        Centered = 16,
    }
}
