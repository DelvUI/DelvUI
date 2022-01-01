using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Utility;
using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Buddy;
using LuminaStatus = Lumina.Excel.GeneratedSheets.Status;
using StatusStruct = FFXIVClientStructs.FFXIV.Client.Game.Status;

namespace DelvUI.Interface.StatusEffects
{
    public class StatusEffectsListHud : ParentAnchoredDraggableHudElement, IHudElementWithActor, IHudElementWithAnchorableParent, IHudElementWithPreview
    {
        protected StatusEffectsListConfig Config => (StatusEffectsListConfig)_config;

        private LayoutInfo _layoutInfo;

        internal static int StatusEffectListsSize = 30;
        private StatusStruct[]? _fakeEffects = null;

        private LabelHud _durationLabel;
        private LabelHud _stacksLabel;
        public GameObject? Actor { get; set; } = null;

        protected override bool AnchorToParent => Config is UnitFrameStatusEffectsListConfig config ? config.AnchorToUnitFrame : false;
        protected override DrawAnchor ParentAnchor => Config is UnitFrameStatusEffectsListConfig config ? config.UnitFrameAnchor : DrawAnchor.Center;

        public StatusEffectsListHud(StatusEffectsListConfig config, string? displayName = null) : base(config, displayName)
        {
            _config.ValueChangeEvent += OnConfigPropertyChanged;

            _durationLabel = new LabelHud(config.IconConfig.DurationLabelConfig);
            _stacksLabel = new LabelHud(config.IconConfig.StacksLabelConfig);

            UpdatePreview();
        }

        ~StatusEffectsListHud()
        {
            _config.ValueChangeEvent -= OnConfigPropertyChanged;
        }

        public void StopPreview()
        {
            Config.Preview = false;
            UpdatePreview();
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

        protected string GetStatusActorName(StatusStruct status)
        {
            var character = Plugin.ObjectTable.SearchById(status.SourceID);
            return character == null ? "" : character.Name.ToString();
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

            var player = Plugin.ClientState.LocalPlayer;
            var count = StatusEffectListsSize;
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
                    var fakeStruct = _fakeEffects![i];
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

                // filter "invisible" status effects
                if (data.Name.ToString().Length == 0 || data.Icon == 0)
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
                var mine = player?.ObjectId == status->SourceID;

                if (Config.IncludePetAsOwn)
                {
                    mine = player?.ObjectId == status->SourceID || IsStatusFromPlayerPet(*status);
                }

                if (Config.ShowOnlyMine && !mine)
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

        protected bool IsStatusFromPlayerPet(StatusStruct status)
        {
            var buddy = Plugin.BuddyList.PetBuddy;

            if (buddy == null)
            {
                return false;
            }

            return buddy.ObjectId == status.SourceID;
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

                if (Config.IncludePetAsOwn)
                {
                    isAFromPlayer |= IsStatusFromPlayerPet(a.Status);
                    isBFromPlayer |= IsStatusFromPlayerPet(b.Status);
                }

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
            var list = StatusEffectsData();

            // area
            GrowthDirections growthDirections = Config.GetGrowthDirections();
            Vector2 position = origin + GetAnchoredPosition(Config.Position, Config.Size, DrawAnchor.TopLeft);
            Vector2 areaPos = CalculateStartPosition(position, Config.Size, growthDirections);
            var margin = new Vector2(14, 10);

            var drawList = ImGui.GetWindowDrawList();

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
            // imgui clips the left and right borders inside windows for some reason
            // we make the window bigger so the actual drawable size is the expected one
            var windowPos = minPos - margin;
            var windowSize = maxPos - minPos;

            AddDrawAction(Config.StrataLevel, () =>
            {
                DrawHelper.DrawInWindow(ID, windowPos, windowSize + margin * 2, !Config.DisableInteraction, false, (drawList) =>
                {
                    // area
                    if (Config.Preview)
                    {
                        drawList.AddRectFilled(areaPos, areaPos + Config.Size, 0x88000000);
                    }

                    for (var i = 0; i < count; i++)
                    {
                        var iconPos = iconPositions[i];
                        var statusEffectData = list[i];

                        // icon
                        var cropIcon = Config.IconConfig.CropIcon;
                        int stackCount = cropIcon ? 1 : statusEffectData.Data.MaxStacks > 0 ? statusEffectData.Status.StackCount : 0;
                        DrawHelper.DrawIcon<LuminaStatus>(drawList, statusEffectData.Data, iconPos, Config.IconConfig.Size, false, cropIcon, stackCount);

                        // border
                        var borderConfig = GetBorderConfig(statusEffectData);
                        if (borderConfig != null && cropIcon)
                        {
                            drawList.AddRect(iconPos, iconPos + Config.IconConfig.Size, borderConfig.Color.Base, 0, ImDrawFlags.None, borderConfig.Thickness);
                        }

                        // Draw dispell indicator above dispellable status effect on uncropped icons
                        if (borderConfig != null && !cropIcon && statusEffectData.Data.CanDispel)
                        {
                            var dispellIndicatorColor = new Vector4(141f / 255f, 206f / 255f, 229f / 255f, 100f / 100f);
                            // 24x32
                            drawList.AddRectFilled(
                                    iconPos + new Vector2(Config.IconConfig.Size.X * .07f, Config.IconConfig.Size.Y * .07f),
                                    iconPos + new Vector2(Config.IconConfig.Size.X * .93f, Config.IconConfig.Size.Y * .14f),
                                    ImGui.ColorConvertFloat4ToU32(dispellIndicatorColor),
                                    8f
                                );
                        }
                    }
                });
            });

            // labels need to be drawn separated since they have their own window for clipping
            for (var i = 0; i < count; i++)
            {
                var iconPos = iconPositions[i];
                var statusEffectData = list[i];

                // duration
                if (Config.IconConfig.DurationLabelConfig.Enabled &&
                    !statusEffectData.Data.IsPermanent &&
                    !statusEffectData.Data.IsFcBuff)
                {
                    AddDrawAction(Config.IconConfig.DurationLabelConfig.StrataLevel, () =>
                    {
                        double duration = Math.Round(Math.Abs(statusEffectData.Status.RemainingTime));
                        Config.IconConfig.DurationLabelConfig.SetText(Utils.DurationToString(duration));
                        _durationLabel.Draw(iconPos, Config.IconConfig.Size);
                    });
                }

                // stacks
                if (Config.IconConfig.StacksLabelConfig.Enabled &&
                    statusEffectData.Data.MaxStacks > 0 &&
                    statusEffectData.Status.StackCount > 0 &&
                    !statusEffectData.Data.IsFcBuff)
                {
                    AddDrawAction(Config.IconConfig.StacksLabelConfig.StrataLevel, () =>
                    {
                        Config.IconConfig.StacksLabelConfig.SetText($"{statusEffectData.Status.StackCount}");
                        _stacksLabel.Draw(iconPos, Config.IconConfig.Size);
                    });
                }

                // tooltips / interaction
                if (ImGui.IsMouseHoveringRect(iconPos, iconPos + Config.IconConfig.Size))
                {
                    // tooltip
                    if (Config.ShowTooltips)
                    {
                        TooltipsHelper.Instance.ShowTooltipOnCursor(
                            statusEffectData.Data.Description.ToDalamudString().ToString(),
                            statusEffectData.Data.Name,
                            statusEffectData.Status.StatusID,
                            GetStatusActorName(statusEffectData.Status)
                        );
                    }

                    bool leftClick = InputsHelper.Instance.HandlingMouseInputs ? InputsHelper.Instance.LeftButtonClicked : ImGui.GetIO().MouseClicked[0];
                    bool rightClick = InputsHelper.Instance.HandlingMouseInputs ? InputsHelper.Instance.RightButtonClicked : ImGui.GetIO().MouseClicked[1];

                    // remove buff on right click
                    bool isFromPlayer = statusEffectData.Status.SourceID == Plugin.ClientState.LocalPlayer?.ObjectId;

                    if (statusEffectData.Data.Category == 1 && isFromPlayer && rightClick)
                    {
                        ChatHelper.SendChatMessage("/statusoff \"" + statusEffectData.Data.Name + "\"");
                    }

                    // automatic add to black list with ctrl+alt+shift click
                    if (Config.BlacklistConfig.Enabled &&
                        ImGui.GetIO().KeyCtrl && ImGui.GetIO().KeyAlt && ImGui.GetIO().KeyShift && leftClick)
                    {
                        Config.BlacklistConfig.AddNewEntry(statusEffectData.Data);
                        ConfigurationManager.Instance.ForceNeedsSave();
                    }
                }
            }
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

        public StatusEffectIconBorderConfig? GetBorderConfig(StatusEffectData statusEffectData)
        {
            StatusEffectIconBorderConfig? borderConfig = null;

            bool isFromPlayerPet = false;
            if (Config.IncludePetAsOwn)
            {
                isFromPlayerPet = IsStatusFromPlayerPet(statusEffectData.Status);
            }

            if (Config.IconConfig.OwnedBorderConfig.Enabled && (statusEffectData.Status.SourceID == Plugin.ClientState.LocalPlayer?.ObjectId || isFromPlayerPet))
            {
                borderConfig = Config.IconConfig.OwnedBorderConfig;
            }
            else if (Config.IconConfig.DispellableBorderConfig.Enabled && statusEffectData.Data.CanDispel)
            {
                borderConfig = Config.IconConfig.DispellableBorderConfig;
            }
            else if (Config.IconConfig.BorderConfig.Enabled)
            {
                borderConfig = Config.IconConfig.BorderConfig;
            }

            return borderConfig;
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

                // forcing "triplecast" buff first to always be able to test stacks
                fakeStruct.StatusID = i == 0 ? (ushort)1211 : (ushort)RNG.Next(1, 200);
                fakeStruct.RemainingTime = RNG.Next(1, 30);
                fakeStruct.StackCount = (byte)RNG.Next(1, 3);
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
