using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using LuminaStatus = Lumina.Excel.GeneratedSheets.Status;
using Status = Dalamud.Game.ClientState.Statuses.Status;

namespace DelvUI.Interface.StatusEffects
{
    public class StatusEffectsListHud : DraggableHudElement, IHudElementWithActor
    {
        protected StatusEffectsListConfig Config => (StatusEffectsListConfig)_config;

        private uint _rowCount;
        private uint _colCount;
        private bool _showingTooltip = false;

        private LabelHud _durationLabel;
        private LabelHud _stacksLabel;

        public GameObject? Actor { get; set; } = null;

        public StatusEffectsListHud(string id, StatusEffectsListConfig config, string displayName) : base(id, config, displayName)
        {
            _durationLabel = new LabelHud(id + "_duration", config.IconConfig.DurationLabelConfig);
            _stacksLabel = new LabelHud(id + "_stacks", config.IconConfig.StacksLabelConfig);
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            var pos = CalculateStartPosition(Config.Position, Config.Size, Config.GetGrowthDirections());
            return (new List<Vector2>() { pos + Config.Size / 2f }, new List<Vector2>() { Config.Size });
        }

        private uint CalculateLayout(List<StatusEffectData> list)
        {
            var effectCount = (uint)list.Count;
            var count = Config.Limit >= 0 ? Math.Min((uint)Config.Limit, effectCount) : effectCount;

            if (Actor == null || count <= 0)
            {
                return 0;
            }

            LayoutHelper.CalculateLayout(
                Config.Size,
                Config.IconConfig.Size,
                count,
                (int)Config.IconPadding.X,
                (int)Config.IconPadding.Y,
                Config.FillRowsFirst,
                out _rowCount,
                out _colCount
            );

            return count;
        }

        protected virtual List<StatusEffectData> StatusEffectsData()
        {
            var list = StatusEffectDataList(Actor);

            // show mine first
            if (Config.ShowMineFirst)
            {
                OrderByMineFirst(list);
            }

            return list;
        }

        protected List<StatusEffectData> StatusEffectDataList(GameObject? actor)
        {
            var list = new List<StatusEffectData>();
            if (actor == null || actor is not BattleChara battleChara)
            {
                return list;
            }

            var effectCount = battleChara.StatusList.Length;
            if (effectCount == 0)
            {
                return list;
            }

            var sheet = Plugin.DataManager.GetExcelSheet<LuminaStatus>();
            if (sheet == null)
            {
                return list;
            }

            var player = Plugin.ClientState.LocalPlayer;

            for (var i = 0; i < effectCount; i++)
            {
                var status = battleChara.StatusList[i];

                if (status is not { StatusId: > 0 })
                {
                    continue;
                }

                var row = sheet.GetRow(status.StatusId);
                if (row == null)
                {
                    continue;
                }

                // buffs
                if (!Config.ShowBuffs && row.Category == 1)
                {
                    continue;
                }

                // debuffs
                if (!Config.ShowDebuffs && row.Category != 1)
                {
                    continue;
                }

                // permanent
                if (!Config.ShowPermanentEffects && row.IsPermanent)
                {
                    continue;
                }

                // only mine
                if (Config.ShowOnlyMine && player?.ObjectId != status.SourceID)
                {
                    continue;
                }

                // blacklist
                if (Config.BlacklistConfig.Enabled && !Config.BlacklistConfig.StatusAllowed(row))
                {
                    continue;
                }

                list.Add(new StatusEffectData(status, row));
            }

            return list;
        }

        protected void OrderByMineFirst(List<StatusEffectData> list)
        {
            var player = Plugin.ClientState.LocalPlayer;
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
            if (!Config.Enabled || Actor == null)
            {
                return;
            }

            if (Actor.ObjectKind != ObjectKind.Player && Actor.ObjectKind != ObjectKind.BattleNpc)
            {
                return;
            }

            // calculate layout
            var list = StatusEffectsData();

            // area
            var growthDirections = Config.GetGrowthDirections();
            var position = origin + Config.Position;
            var areaPos = CalculateStartPosition(position, Config.Size, growthDirections);
            var drawList = ImGui.GetWindowDrawList();

            if (Config.ShowArea)
            {
                drawList.AddRectFilled(areaPos, areaPos + Config.Size, 0x88000000);
            }

            // no need to do anything else if there are no effects
            if (list.Count == 0)
            {
                return;
            }

            // calculate icon positions
            var count = CalculateLayout(list);
            var iconPositions = new List<Vector2>();
            var minPos = new Vector2(float.MaxValue, float.MaxValue);
            var maxPos = Vector2.Zero;

            var row = 0;
            var col = 0;

            for (var i = 0; i < count; i++)
            {
                CalculateAxisDirections(growthDirections, row, count, out var direction, out var offset);

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
                    if (col >= _colCount)
                    {
                        col = 0;
                        row += 1;
                    }
                }
                else
                {
                    row += 1;
                    if (row >= _rowCount)
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
            if (!Config.ShowBuffs)
            {
                windowFlags |= ImGuiWindowFlags.NoInputs;
            }

            // imgui clips the left and right borders inside windows for some reason
            // we make the window bigger so the actual drawable size is the expected one
            var margin = new Vector2(14, 10);
            var windowPos = minPos - margin;
            var windowSize = maxPos - minPos;
            ImGui.SetNextWindowPos(windowPos, ImGuiCond.Always);
            ImGui.SetNextWindowSize(windowSize + margin * 2);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
            ImGui.Begin("statusEffectList " + ID, windowFlags);

            // draw
            drawList = ImGui.GetWindowDrawList();
            var showingTooltip = false;

            for (var i = 0; i < count; i++)
            {
                var iconPos = iconPositions[i];
                var statusEffectData = list[i];

                StatusEffectIconDrawHelper.DrawStatusEffectIcon(drawList, iconPos, statusEffectData, Config.IconConfig, _durationLabel, _stacksLabel);

                if (ImGui.IsMouseHoveringRect(iconPos, iconPos + Config.IconConfig.Size))
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
                var elementsPerRow = (int)(Config.Size.X / (Config.IconConfig.Size.X + Config.IconPadding.X));
                var elementsInRow = Math.Min(elementsPerRow, elementCount - (elementsPerRow * row));

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
            var area = size;
            if ((growthDirections & GrowthDirections.Left) != 0)
            {
                area.X = -area.X;
            }

            if ((growthDirections & GrowthDirections.Up) != 0)
            {
                area.Y = -area.Y;
            }

            var startPos = position;
            if ((growthDirections & GrowthDirections.Centered) != 0)
            {
                startPos.X = position.X - size.X / 2f;
            }

            var endPos = position + area;

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
    }

    public struct StatusEffectData
    {
        public Status Status;
        public LuminaStatus Data;

        public StatusEffectData(Status status, LuminaStatus data)
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
