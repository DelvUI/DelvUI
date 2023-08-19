using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using DelvUI.Interface.PartyCooldowns;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.Party
{
    public class PartyFramesCooldownListHud : ParentAnchoredDraggableHudElement, IHudElementWithActor, IHudElementWithAnchorableParent, IHudElementWithPreview
    {
        private PartyFramesCooldownListConfig Config => (PartyFramesCooldownListConfig)_config;
        private PartyCooldownsDataConfig _dataConfig = null!;

        private LabelHud _timeLabel;
        private bool _needsUpdate = true;
        private LayoutInfo _layoutInfo;

        private List<PartyCooldown> _cooldowns = new List<PartyCooldown>();
        private List<PartyCooldown>? _fakeCooldowns = new List<PartyCooldown>();

        public GameObject? Actor { get; set; }

        protected override bool AnchorToParent => true;
        protected override DrawAnchor ParentAnchor => Config is PartyFramesCooldownListConfig config ? config.HealthBarAnchor : DrawAnchor.Center;

        public PartyFramesCooldownListHud(PartyFramesCooldownListConfig config, string? displayName = null) : base(config, displayName)
        {
            _timeLabel = new LabelHud(config.TimeLabel);

            _dataConfig = ConfigurationManager.Instance.GetConfigObject<PartyCooldownsDataConfig>();

            _config.ValueChangeEvent += OnConfigPropertyChanged;
            _dataConfig.CooldownsDataChangedEvent += OnCooldownsDataChanged;
            PartyCooldownsManager.Instance.CooldownsChangedEvent += OnCooldownsChanged;
        }

        ~PartyFramesCooldownListHud()
        {
            _config.ValueChangeEvent -= OnConfigPropertyChanged;
            _dataConfig.CooldownsDataChangedEvent += OnCooldownsDataChanged;
            PartyCooldownsManager.Instance.CooldownsChangedEvent += OnCooldownsChanged;
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
                _fakeCooldowns = null;
                return;
            }

            var RNG = new Random((int)ImGui.GetTime());

            _fakeCooldowns = new List<PartyCooldown>();

            for (int i = 0; i < 10; i++)
            {
                int index = RNG.Next(0, _dataConfig.Cooldowns.Count);

                PartyCooldown cooldown = new PartyCooldown(_dataConfig.Cooldowns[index], 0, 90, null);
                
                int rng = RNG.Next(100);
                if (rng > 80)
                {
                    cooldown.LastTimeUsed = ImGui.GetTime() - 30;
                }
                else if (rng > 50)
                {
                    cooldown.LastTimeUsed = ImGui.GetTime() + 1;
                }

                _fakeCooldowns.Add(cooldown);
            }
        }

        public void StopPreview()
        {
            Config.Preview = false;
            UpdatePreview();
        }

        private void OnCooldownsDataChanged(PartyCooldownsDataConfig sender)
        {
            _needsUpdate = true;
        }

        private void OnCooldownsChanged(PartyCooldownsManager sender)
        {
            _needsUpdate = true;
        }

        private void UpdateCooldowns()
        {
            _cooldowns.Clear();

            if (Actor == null || PartyCooldownsManager.Instance?.CooldownsMap == null) { return; }

            if (PartyCooldownsManager.Instance.CooldownsMap.TryGetValue(Actor.ObjectId, out Dictionary<uint, PartyCooldown>? dict) && dict != null)
            {
                _cooldowns = dict.Values.Where(o => o.Data.IsEnabledForPartyFrames()).ToList();
            }

            _cooldowns.Sort((a, b) =>
            {
                int aOrder = a.Data.Column * 1000 + a.Data.Priority;
                int bOrder = b.Data.Column * 1000 + b.Data.Priority;

                return aOrder.CompareTo(bOrder);
            });

            _needsUpdate = false;
        }

        private void CalculateLayout(uint count)
        {
            if (count <= 0) { return; }

            _layoutInfo = LayoutHelper.CalculateLayout(
                Config.Size,
                Config.IconSize,
                count,
                Config.IconPadding,
                Config.FillRowsFirst
            );
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (!Config.Enabled) { return; }

            if (_needsUpdate)
            {
                UpdateCooldowns();
            }

            List<PartyCooldown> list = _fakeCooldowns != null ? _fakeCooldowns : _cooldowns;

            if (list.Count == 0) { return; }

            // area
            GrowthDirections growthDirections = LayoutHelper.GrowthDirectionsFromIndex(Config.Directions);
            Vector2 position = origin + GetAnchoredPosition(Config.Position, Config.Size, DrawAnchor.TopLeft);
            Vector2 areaPos = LayoutHelper.CalculateStartPosition(position, Config.Size, growthDirections);
            Vector2 margin = new Vector2(14, 10);

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            // calculate icon positions
            uint count = (uint)list.Count;
            CalculateLayout(count);
            var (iconPositions, minPos, maxPos) = LayoutHelper.CalculateIconPositions(
                growthDirections,
                count,
                position,
                Config.Size,
                Config.IconSize,
                Config.IconPadding,
                Config.FillRowsFirst,
                _layoutInfo
            );

            // window
            // imgui clips the left and right borders inside windows for some reason
            // we make the window bigger so the actual drawable size is the expected one
            Vector2 windowPos = minPos - margin;
            Vector2 windowSize = maxPos - minPos;

            AddDrawAction(Config.StrataLevel, () =>
            {
                DrawHelper.DrawInWindow(ID, windowPos, windowSize + margin * 2, false, false, (drawList) =>
                {
                    // area
                    if (Config.Preview)
                    {
                        drawList.AddRectFilled(areaPos, areaPos + Config.Size, 0x88000000);
                    }

                    for (int i = 0; i < count; i++)
                    {
                        Vector2 iconPos = iconPositions[i];
                        PartyCooldown cooldown = list[i];

                        float cooldownTime = cooldown.CooldownTimeRemaining();
                        float effectTime = cooldown.EffectTimeRemaining();

                        // icon
                        bool recharging = effectTime == 0 && cooldownTime > 0;
                        uint color = recharging ? 0xAAFFFFFF : 0xFFFFFFFF;
                        DrawHelper.DrawIcon(cooldown.Data.IconId, iconPos, Config.IconSize, false, color, drawList);

                        if (effectTime == 0 && cooldownTime > 0)
                        {
                            DrawHelper.DrawIconCooldown(iconPos, Config.IconSize, cooldownTime, cooldown.Data.CooldownDuration, drawList);
                        }

                        // border
                        if (Config.DrawBorder)
                        {
                            bool active = effectTime > 0 && Config.ChangeIconBorderWhenActive;
                            uint iconBorderColor = active ? Config.IconActiveBorderColor.Base : Config.BorderColor.Base;
                            int thickness = active ? Config.IconActiveBorderThickness : Config.BorderThickness;
                            drawList.AddRect(iconPos, iconPos + Config.IconSize, iconBorderColor, 0, ImDrawFlags.None, thickness);
                        }
                    }
                });
            });

            PartyCooldown? hoveringCooldown = null;
            GameObject? character = Actor;

            // labels need to be drawn separated since they have their own window for clipping
            for (var i = 0; i < count; i++)
            {
                Vector2 iconPos = iconPositions[i];
                PartyCooldown cooldown = list[i];

                float cooldownTime = cooldown.CooldownTimeRemaining();
                float effectTime = cooldown.EffectTimeRemaining();

                // time
                AddDrawAction(Config.TimeLabel.StrataLevel, () =>
                {
                    if (effectTime > 0)
                    {
                        Config.TimeLabel.SetValue(effectTime);
                    }
                    else if (cooldownTime > 0)
                    {
                        Config.TimeLabel.SetText(Utils.DurationToFullString(cooldownTime));
                    }
                    else
                    {
                        Config.TimeLabel.SetText("");
                    }

                    _timeLabel.Draw(iconPos, Config.IconSize, character);
                });

                // tooltips / interaction
                if (ImGui.IsMouseHoveringRect(iconPos, iconPos + Config.IconSize))
                {
                    hoveringCooldown = cooldown;
                }
            }

            if (hoveringCooldown != null)
            {
                // tooltip
                if (Config.ShowTooltips)
                {
                    TooltipsHelper.Instance.ShowTooltipOnCursor(
                        hoveringCooldown.TooltipText(),
                        hoveringCooldown.Data.Name,
                        hoveringCooldown.Data.ActionId
                    );
                }
            }
        }
    }
}
