using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Utility;
using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LuminaStatus = Lumina.Excel.GeneratedSheets.Status;
using StatusStruct = FFXIVClientStructs.FFXIV.Client.Game.Status;

namespace DelvUI.Interface.StatusEffects
{
    public class StatusEffectsListHud : ParentAnchoredDraggableHudElement, IHudElementWithActor, IHudElementWithAnchorableParent, IHudElementWithPreview, IHudElementWithMouseOver, IHudElementWithVisibilityConfig
    {
        protected StatusEffectsListConfig Config => (StatusEffectsListConfig)_config;
        public VisibilityConfig? VisibilityConfig => Config is UnitFrameStatusEffectsListConfig config ? config.VisibilityConfig : null;

        private LayoutInfo _layoutInfo;

        internal static int StatusEffectListsSize = 30;
        private StatusStruct[]? _fakeEffects = null;

        private LabelHud _durationLabel;
        private LabelHud _stacksLabel;
        public GameObject? Actor { get; set; } = null;

        private bool _wasHovering = false;
        private bool NeedsSpecialInput => !ClipRectsHelper.Instance.Enabled || ClipRectsHelper.Instance.Mode == WindowClippingMode.Performance;

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
            Vector2 pos = LayoutHelper.CalculateStartPosition(Config.Position, Config.Size, LayoutHelper.GrowthDirectionsFromIndex(Config.Directions));
            return (new List<Vector2>() { pos + Config.Size / 2f }, new List<Vector2>() { Config.Size });
        }

        public void StopMouseover()
        {
            if (_wasHovering && NeedsSpecialInput)
            {
                InputsHelper.Instance.StopHandlingInputs();
                _wasHovering = false;
            }
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
                LayoutHelper.GetFillsRowsFirst(Config.FillRowsFirst, LayoutHelper.GrowthDirectionsFromIndex(Config.Directions))
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

            // show mine or permanent first
            if (Config.ShowMineFirst || Config.ShowPermanentFirst)
            {
                return OrderByMineOrPermanentFirst(list);
            }

            return list;
        }

        protected unsafe List<StatusEffectData> StatusEffectDataList(GameObject? actor)
        {
            List<StatusEffectData> list = new List<StatusEffectData>();
            PlayerCharacter? player = Plugin.ClientState.LocalPlayer;
            BattleChara? character = null;
            int count = StatusEffectListsSize;

            if (_fakeEffects == null)
            {
                if (actor == null || actor is not BattleChara)
                {
                    return list;
                }

                character = (BattleChara)actor;
            }
            else
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
                    try
                    {
                        status = character?.StatusList[i] == null ? null : (StatusStruct*)character.StatusList[i]!.Address;
                    }
                    catch { }
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
                    try
                    {
                        data = character?.StatusList[i]?.GameData;
                    } catch { }
                }

                if (data == null)
                {
                    continue;
                }

                // filter "invisible" status effects
                if (data.Icon == 0 || data.Name.ToString().Length == 0)
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
                if (!Config.ShowBuffs && data.StatusCategory == 1)
                {
                    continue;
                }

                // debuffs
                if (!Config.ShowDebuffs && data.StatusCategory != 1)
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

        protected List<StatusEffectData> OrderByMineOrPermanentFirst(List<StatusEffectData> list)
        {
            var player = Plugin.ClientState.LocalPlayer;
            if (player == null)
            {
                return list;
            }

            if (Config.ShowMineFirst && Config.ShowPermanentFirst)
            {
                return list.OrderByDescending(x => x.Status.SourceID == player.ObjectId && x.Data.IsPermanent || x.Data.IsFcBuff)
                    .ThenByDescending(x => x.Status.SourceID == player.ObjectId)
                    .ThenByDescending(x => x.Data.IsPermanent)
                    .ThenByDescending(x => x.Data.IsFcBuff)
                    .ToList();
            }
            else if (Config.ShowMineFirst && !Config.ShowPermanentFirst)
            {
                return list.OrderByDescending(x => x.Status.SourceID == player.ObjectId)
                    .ToList();
            }
            else if (!Config.ShowMineFirst && Config.ShowPermanentFirst)
            {
                return list.OrderByDescending(x => x.Data.IsPermanent)
                    .ThenByDescending(x => x.Data.IsFcBuff)
                    .ToList();
            }

            return list;
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
            List<StatusEffectData> list = StatusEffectsData();

            // area
            GrowthDirections growthDirections = LayoutHelper.GrowthDirectionsFromIndex(Config.Directions);
            Vector2 position = origin + GetAnchoredPosition(Config.Position, Config.Size, DrawAnchor.TopLeft);
            Vector2 areaPos = LayoutHelper.CalculateStartPosition(position, Config.Size, growthDirections);
            Vector2 margin = new Vector2(14, 10);

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            // no need to do anything else if there are no effects
            if (list.Count == 0)
            {
                if (_wasHovering && NeedsSpecialInput)
                {
                    _wasHovering = false;
                    InputsHelper.Instance.StopHandlingInputs();
                }

                return;
            }

            // calculate icon positions
            uint count = CalculateLayout(list);
            var (iconPositions, minPos, maxPos) = LayoutHelper.CalculateIconPositions(
                growthDirections,
                count,
                position,
                Config.Size,
                Config.IconConfig.Size,
                Config.IconPadding,
                LayoutHelper.GetFillsRowsFirst(Config.FillRowsFirst, growthDirections),
                _layoutInfo
            );
     
            // window
            // imgui clips the left and right borders inside windows for some reason
            // we make the window bigger so the actual drawable size is the expected one
            Vector2 windowPos = minPos - margin;
            Vector2 windowSize = maxPos - minPos;

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
                        Vector2 iconPos = iconPositions[i];
                        var statusEffectData = list[i];

                        // shadow
                        if (Config.IconConfig.ShadowConfig! != null && Config.IconConfig.ShadowConfig.Enabled)
                        {
                            // Right Side
                            drawList.AddRectFilled(iconPos + new Vector2(Config.IconConfig.Size.X, Config.IconConfig.ShadowConfig.Offset), iconPos + Config.IconConfig.Size + new Vector2(Config.IconConfig.ShadowConfig.Offset, Config.IconConfig.ShadowConfig.Offset) + new Vector2(Config.IconConfig.ShadowConfig.Thickness - 1, Config.IconConfig.ShadowConfig.Thickness - 1), Config.IconConfig.ShadowConfig.Color.Base);

                            // Bottom Size
                            drawList.AddRectFilled(iconPos + new Vector2(Config.IconConfig.ShadowConfig.Offset, Config.IconConfig.Size.Y), iconPos + Config.IconConfig.Size + new Vector2(Config.IconConfig.ShadowConfig.Offset, Config.IconConfig.ShadowConfig.Offset) + new Vector2(Config.IconConfig.ShadowConfig.Thickness - 1, Config.IconConfig.ShadowConfig.Thickness - 1), Config.IconConfig.ShadowConfig.Color.Base);
                        }

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

            StatusEffectData? hoveringData = null;
            GameObject? character = Actor;

            // labels need to be drawn separated since they have their own window for clipping
            for (var i = 0; i < count; i++)
            {
                Vector2 iconPos = iconPositions[i];
                StatusEffectData statusEffectData = list[i];

                // duration
                if (Config.IconConfig.DurationLabelConfig.Enabled &&
                    !statusEffectData.Data.IsPermanent &&
                    !statusEffectData.Data.IsFcBuff)
                {
                    AddDrawAction(Config.IconConfig.DurationLabelConfig.StrataLevel, () =>
                    {
                        double duration = Math.Round(Math.Abs(statusEffectData.Status.RemainingTime));
                        Config.IconConfig.DurationLabelConfig.SetText(Utils.DurationToString(duration));
                        _durationLabel.Draw(iconPos, Config.IconConfig.Size, character);
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
                        _stacksLabel.Draw(iconPos, Config.IconConfig.Size, character);
                    });
                }

                // tooltips / interaction
                if (ImGui.IsMouseHoveringRect(iconPos, iconPos + Config.IconConfig.Size))
                {
                    hoveringData = statusEffectData;
                }
            }

            if (hoveringData.HasValue)
            {
                StatusEffectData data = hoveringData.Value;

                if (NeedsSpecialInput)
                {
                    _wasHovering = true;
                    InputsHelper.Instance.StartHandlingInputs();
                }

                // tooltip
                if (Config.ShowTooltips)
                {
                    TooltipsHelper.Instance.ShowTooltipOnCursor(
                        EncryptedStringsHelper.GetString(data.Data.Description.ToDalamudString().ToString()),
                        EncryptedStringsHelper.GetString(data.Data.Name),
                        data.Status.StatusID,
                        GetStatusActorName(data.Status)
                    );
                }

                bool leftClick = InputsHelper.Instance.HandlingMouseInputs ? InputsHelper.Instance.LeftButtonClicked : ImGui.GetIO().MouseClicked[0];
                bool rightClick = InputsHelper.Instance.HandlingMouseInputs ? InputsHelper.Instance.RightButtonClicked : ImGui.GetIO().MouseClicked[1];

                // remove buff on right click
                bool isFromPlayer = data.Status.SourceID == Plugin.ClientState.LocalPlayer?.ObjectId;
                bool isTheEcho = data.Status.StatusID is 42 or 239;

                if (data.Data.StatusCategory == 1 && (isFromPlayer || isTheEcho) && rightClick)
                {
                    ChatHelper.SendChatMessage("/statusoff \"" + data.Data.Name + "\"");

                    if (NeedsSpecialInput)
                    {
                        _wasHovering = false;
                        InputsHelper.Instance.StopHandlingInputs();
                    }
                }

                // automatic add to black list with ctrl+alt+shift click
                if (Config.BlacklistConfig.Enabled &&
                    ImGui.GetIO().KeyCtrl && ImGui.GetIO().KeyAlt && ImGui.GetIO().KeyShift && leftClick)
                {
                    Config.BlacklistConfig.AddNewEntry(data.Data);
                    ConfigurationManager.Instance.ForceNeedsSave();

                    if (NeedsSpecialInput)
                    {
                        _wasHovering = false;
                        InputsHelper.Instance.StopHandlingInputs();
                    }
                }
            }
            else if (_wasHovering && NeedsSpecialInput)
            {
                _wasHovering = false;
                InputsHelper.Instance.StopHandlingInputs();
            }
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
}
