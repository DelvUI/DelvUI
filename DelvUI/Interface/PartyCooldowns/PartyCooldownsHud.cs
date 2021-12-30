using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DelvUI.Interface.PartyCooldowns
{
    public class PartyCooldownsHud : DraggableHudElement, IHudElementWithPreview
    {
        private PartyCooldownsConfig Config => (PartyCooldownsConfig)_config;
        private PartyCooldownsBarConfig _barConfig = null!;
        private PartyCooldownsDataConfig _dataConfig = null!;

        private List<List<PartyCooldown>> _cooldowns = new List<List<PartyCooldown>>();

        private LabelHud _nameLabelHud;
        private LabelHud _timeLabelHud;

        public PartyCooldownsHud(PartyCooldownsConfig config, string displayName) : base(config, displayName)
        {
            _barConfig = ConfigurationManager.Instance.GetConfigObject<PartyCooldownsBarConfig>();
            _dataConfig = ConfigurationManager.Instance.GetConfigObject<PartyCooldownsDataConfig>();

            _dataConfig.CooldownsDataChangedEvent += OnCooldownsDataChanged;
            PartyCooldownsManager.Instance.CooldownsChangedEvent += OnCooldownsChanged;

            _nameLabelHud = new LabelHud(_barConfig.NameLabel);
            _timeLabelHud = new LabelHud(_barConfig.TimeLabel);

            UpdateCooldowns();
        }

        protected override void InternalDispose()
        {
            _dataConfig.CooldownsDataChangedEvent -= OnCooldownsDataChanged;
            PartyCooldownsManager.Instance.CooldownsChangedEvent -= OnCooldownsChanged;
        }

        public void StopPreview()
        {
            Config.Preview = false;
            PartyCooldownsManager.Instance?.UpdatePreview();
        }

        private void OnCooldownsDataChanged(PartyCooldownsDataConfig sender)
        {
            UpdateCooldowns();
        }

        private void OnCooldownsChanged(PartyCooldownsManager sender)
        {
            UpdateCooldowns();
        }

        private void UpdateCooldowns()
        {
            _cooldowns.Clear();

            int columnCount = PartyCooldownsDataConfig.ColumnCount;
            for (int i = 0; i < columnCount; i++)
            {
                _cooldowns.Add(new List<PartyCooldown>());
            }

            foreach (Dictionary<uint, PartyCooldown> memberCooldownList in PartyCooldownsManager.Instance.CooldownsMap.Values)
            {
                foreach (PartyCooldown cooldown in memberCooldownList.Values)
                {
                    int columnIndex = Math.Min(columnCount - 1, cooldown.Data.Column - 1);
                    _cooldowns[columnIndex].Add(cooldown);
                }
            }

            foreach (List<PartyCooldown> list in _cooldowns)
            {
                list.Sort((a, b) =>
                {
                    if (a.Data.Priority == b.Data.Priority)
                    {
                        if (a.Data.ActionId == b.Data.ActionId)
                        {
                            return a.SourceId.CompareTo(b.SourceId);
                        }
                        else
                        {
                            return a.Data.ActionId.CompareTo(b.Data.ActionId);
                        }
                    }

                    if (a.Data.Priority < b.Data.Priority)
                    {
                        return -1;
                    }

                    return 1;
                });
            }
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            // hardcoded just for draggable area purposes
            const int columnCount = 3;
            const int rowCount = 4;

            Vector2 size = new Vector2(
                _barConfig.Size.X * columnCount + Config.Padding.X * (columnCount - 1),
                _barConfig.Size.Y * rowCount + Config.Padding.Y * (rowCount - 1));

            Vector2 pos = Config.GrowthDirection == PartyCooldownsGrowthDirection.Down ? Config.Position : Config.Position - new Vector2(0, size.Y);

            return (new List<Vector2>() { pos + size / 2f }, new List<Vector2>() { size });
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (!Config.Enabled) { return; }

            PlayerCharacter? player = Plugin.ClientState.LocalPlayer;
            if (player == null) { return; }

            float offsetX = 0;
            bool addedOffset = true;

            foreach (List<PartyCooldown> list in _cooldowns)
            {
                if (list.Count == 0) { continue; }

                int i = 0;

                foreach (PartyCooldown cooldown in list)
                {
                    if (!addedOffset)
                    {
                        offsetX += Config.Padding.X + _barConfig.Size.X;
                        addedOffset = true;
                    }

                    string barId = _barConfig.ID + $"_{offsetX + i}";

                    // cooldown bar
                    float cooldownTime = cooldown.CooldownTimeRemaining();
                    float effectTime = cooldown.EffectTimeRemaining();

                    float max = effectTime > 0 ? cooldown.Data.EffectDuration : cooldown.Data.CooldownDuration;
                    float current = effectTime > 0 ? effectTime : cooldownTime;

                    float sizeX = Math.Max(1, _barConfig.Size.X - _barConfig.Size.Y);
                    Vector2 size = new Vector2(sizeX, _barConfig.Size.Y);

                    int direction = Config.GrowthDirection == PartyCooldownsGrowthDirection.Down ? 1 : -1;
                    float y = Config.Position.Y + i * direction * size.Y + i * direction * Config.Padding.Y;
                    Vector2 pos = new Vector2(Config.Position.X + size.Y + offsetX - 1, y);

                    if (_barConfig.ShowBar)
                    {
                        PluginConfigColor fillColor = effectTime > 0 ? _barConfig.AvailableColor : _barConfig.RechargingColor;
                        PluginConfigColor bgColor = effectTime > 0 || cooldownTime == 0 ? _barConfig.AvailableBackgroundColor : _barConfig.RechargingBackgroundColor;

                        Rect background = new Rect(pos, size, bgColor);
                        Rect fill = BarUtilities.GetFillRect(pos, size, _barConfig.FillDirection, fillColor, current, max);

                        BarHud bar = new BarHud(
                            barId,
                            _barConfig.DrawBorder,
                            _barConfig.BorderColor,
                            _barConfig.BorderThickness,
                            DrawAnchor.TopLeft,
                            current: current,
                            max: max
                        );

                        bar.SetBackground(background);
                        bar.AddForegrounds(fill);

                        AddDrawActions(bar.GetDrawActions(origin, _barConfig.StrataLevel));
                    }

                    // icon
                    if (_barConfig.ShowIcon)
                    {
                        Vector2 iconPos = origin + new Vector2(Config.Position.X + offsetX, y);
                        Vector2 iconSize = new Vector2(size.Y);
                        bool recharging = effectTime == 0 && cooldownTime > 0;

                        AddDrawAction(_barConfig.StrataLevel, () =>
                        {
                            DrawHelper.DrawInWindow(barId + "_icon", iconPos, iconSize, false, false, (drawList) =>
                            {
                                uint color = recharging ? 0xAAFFFFFF : 0xFFFFFFFF;
                                DrawHelper.DrawIcon(cooldown.Data.IconId, iconPos, iconSize, false, color, drawList);

                                if (_barConfig.ShowIconCooldownAnimation && effectTime == 0 && cooldownTime > 0)
                                {
                                    DrawHelper.DrawIconCooldown(iconPos, iconSize, cooldownTime, cooldown.Data.CooldownDuration, drawList);
                                }

                                if (_barConfig.DrawBorder)
                                {
                                    bool active = effectTime > 0 && _barConfig.ChangeIconBorderWhenActive;
                                    uint iconBorderColor = active ? _barConfig.IconActiveBorderColor.Base : _barConfig.BorderColor.Base;
                                    int thickness = active ? _barConfig.IconActiveBorderThickness : _barConfig.BorderThickness;
                                    drawList.AddRect(iconPos, iconPos + iconSize, iconBorderColor, 0, ImDrawFlags.None, thickness);
                                }
                            });
                        });
                    }

                    // name
                    Character? character = cooldown.Member?.Character;
                    if (character == null && cooldown.SourceId == player.ObjectId)
                    {
                        character = player;
                    }

                    Vector2 labelPos = origin + pos;
                    AddDrawAction(_barConfig.NameLabel.StrataLevel, () =>
                    {
                        string? name = character == null ? "Fake Name" : null;
                        _nameLabelHud.Draw(labelPos, size, character, name);
                    });

                    // time
                    AddDrawAction(_barConfig.TimeLabel.StrataLevel, () =>
                    {
                        if (effectTime > 0)
                        {
                            _barConfig.TimeLabel.SetValue(effectTime);
                        }
                        else if (cooldownTime > 0)
                        {
                            _barConfig.TimeLabel.SetText(Utils.DurationToFullString(cooldownTime));
                        }
                        else
                        {
                            _barConfig.TimeLabel.SetText("");
                        }

                        _timeLabelHud.Draw(labelPos, size, character);
                    });

                    // tooltip
                    pos = origin + new Vector2(Config.Position.X + offsetX, y);
                    if (Config.ShowTooltips && ImGui.IsMouseHoveringRect(pos, pos + _barConfig.Size))
                    {
                        TooltipsHelper.Instance.ShowTooltipOnCursor(
                            cooldown.Data.TooltipText(),
                            cooldown.Data.Name,
                            cooldown.Data.ActionId
                        );
                    }

                    i++;
                }

                addedOffset = false;
            }
        }
    }
}
