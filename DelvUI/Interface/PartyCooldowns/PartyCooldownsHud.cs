﻿using Dalamud.Game.ClientState.Objects.SubKinds;
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
    public class PartyCooldownsHud : DraggableHudElement, IHudElementWithPreview, IHudElementWithVisibilityConfig
    {
        private PartyCooldownsConfig Config => (PartyCooldownsConfig)_config;
        public VisibilityConfig VisibilityConfig => Config.VisibilityConfig;
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
                    if (!cooldown.Data.IsEnabledForPartyCooldowns()) { continue; }

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

            IPlayerCharacter? player = Plugin.ClientState.LocalPlayer;
            if (player == null) { return; }

            float offset = 0;
            bool addedOffset = true;
            bool isVertical = Config.GrowthDirection == PartyCooldownsGrowthDirection.Up || Config.GrowthDirection == PartyCooldownsGrowthDirection.Down;

            foreach (List<PartyCooldown> list in _cooldowns)
            {
                if (list.Count == 0) { continue; }

                int i = 0;

                foreach (PartyCooldown cooldown in list)
                {
                    if (!addedOffset)
                    {
                        offset += isVertical ? Config.Padding.X + _barConfig.Size.X : Config.Padding.Y + _barConfig.Size.Y;
                        addedOffset = true;
                    }

                    string barId = _barConfig.ID + $"_{offset + i}";

                    // cooldown bar
                    float cooldownTime = cooldown.CooldownTimeRemaining();
                    float effectTime = cooldown.EffectTimeRemaining();

                    float max = effectTime > 0 ? cooldown.GetEffectDuration() : cooldown.GetCooldown();
                    float current = effectTime > 0 ? effectTime : cooldownTime;
                    
                    float sizeX = Math.Max(1, _barConfig.Size.X - _barConfig.Size.Y);
                    Vector2 size = new Vector2(sizeX, _barConfig.Size.Y);

                    Vector2 pos;

                    if (isVertical)
                    {
                        int direction = Config.GrowthDirection == PartyCooldownsGrowthDirection.Down ? 1 : -1;
                        pos = new Vector2(
                            Config.Position.X + size.Y + offset - 1,
                            Config.Position.Y + i * direction * size.Y + i * direction * Config.Padding.Y
                        );
                    }
                    else
                    {
                        int direction = Config.GrowthDirection == PartyCooldownsGrowthDirection.Right ? 1 : -1;
                        pos = new Vector2(
                            size.Y + Config.Position.X + i * direction * size.X + i * direction * size.Y + i * direction * Config.Padding.X,
                            Config.Position.Y + offset - 1
                        );
                    }

                    if (_barConfig.ShowBar)
                    {
                        PluginConfigColor fillColor = effectTime > 0 ? _barConfig.AvailableColor : _barConfig.RechargingColor;
                        PluginConfigColor bgColor = effectTime > 0 || cooldownTime == 0 ? _barConfig.AvailableBackgroundColor : _barConfig.RechargingBackgroundColor;

                        if (_barConfig.UseJobColors)
                        {
                            uint? jobId = GetJobId(cooldown, player);
                            if (jobId.HasValue)
                            {
                                PluginConfigColor jobColor = GlobalColors.Instance.SafeColorForJobId(jobId.Value);
                                PluginConfigColor bgJobColor = jobColor.WithAlpha(40f / 100f);
                                PluginConfigColor rechargeJobColor = jobColor.WithAlpha(25f / 100f);
                                PluginConfigColor nonActive = PluginConfigColor.FromHex(0x88000000);
                                fillColor = effectTime > 0 ? jobColor : rechargeJobColor;
                                bgColor = effectTime > 0 || cooldownTime == 0 ? bgJobColor : nonActive;
                            }
                        }

                        Rect background = new Rect(pos, size, bgColor);
                        Rect fill = BarUtilities.GetFillRect(pos, size, _barConfig.FillDirection, fillColor, current, max);

                        BarHud bar = new BarHud(
                            barId,
                            _barConfig.DrawBorder,
                            _barConfig.BorderColor,
                            _barConfig.BorderThickness,
                            DrawAnchor.TopLeft,
                            current: current,
                            max: max,
                            barTextureName: _barConfig.BarTextureName,
                            barTextureDrawMode: _barConfig.BarTextureDrawMode
                        );

                        bar.SetBackground(background);
                        bar.AddForegrounds(fill);

                        AddDrawActions(bar.GetDrawActions(origin, _barConfig.StrataLevel));
                    }

                    // icon
                    if (_barConfig.ShowIcon)
                    {
                        Vector2 iconPos = origin + new Vector2(pos.X - size.Y + 1, pos.Y);
                        Vector2 iconSize = new Vector2(size.Y);
                        bool recharging = effectTime == 0 && cooldownTime > 0;
                        bool shouldDrawCooldown = ClipRectsHelper.Instance.GetClipRectForArea(iconPos, iconSize) == null;

                        AddDrawAction(_barConfig.StrataLevel, () =>
                        {
                            DrawHelper.DrawInWindow(barId + "_icon", iconPos, iconSize, false, (drawList) =>
                            {
                                uint color = recharging ? 0xAAFFFFFF : 0xFFFFFFFF;
                                DrawHelper.DrawIcon(cooldown.Data.IconId, iconPos, iconSize, false, color, drawList);

                                // cooldown
                                if (shouldDrawCooldown && 
                                    _barConfig.ShowIconCooldownAnimation && 
                                    effectTime == 0 && 
                                    cooldownTime > 0)
                                {
                                    DrawHelper.DrawIconCooldown(iconPos, iconSize, cooldownTime, cooldown.GetCooldown(), drawList);
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
                    PluginConfigColor? labelColor = effectTime > 0 && _barConfig.ChangeLabelsColorWhenActive ? _barConfig.LabelsActiveColor : null;

                    ICharacter? character = cooldown.Member?.Character;
                    if (character == null && cooldown.SourceId == player.GameObjectId)
                    {
                        character = player;
                    }

                    Vector2 labelPos = origin + pos;
                    AddDrawAction(_barConfig.NameLabel.StrataLevel, () =>
                    {
                        PluginConfigColor realColor = _barConfig.NameLabel.Color;
                        _barConfig.NameLabel.Color = labelColor ?? realColor;

                        string? name = character == null ? "Fake Name" : null;
                        _nameLabelHud.Draw(labelPos, size, character, name);

                        _barConfig.NameLabel.Color = realColor;
                    });

                    // time
                    AddDrawAction(_barConfig.TimeLabel.StrataLevel, () =>
                    {
                        PluginConfigColor realColor = _barConfig.TimeLabel.Color;
                        _barConfig.TimeLabel.Color = labelColor ?? realColor;
                        _barConfig.TimeLabel.SetText("");

                        if (effectTime > 0)
                        {
                            if (_barConfig.TimeLabel.ShowEffectDuration)
                            {
                                _barConfig.TimeLabel.SetValue(effectTime);
                            }
                        }
                        else if (cooldownTime > 0)
                        {
                            if (_barConfig.TimeLabel.ShowRemainingCooldown)
                            {
                                _barConfig.TimeLabel.SetText(Utils.DurationToString(cooldownTime, _barConfig.TimeLabel.NumberFormat));
                            }
                        }

                        _timeLabelHud.Draw(labelPos, size, character);
                        _barConfig.TimeLabel.Color = realColor;
                    });

                    // tooltip
                    pos = origin + new Vector2(pos.X - size.Y + 1, pos.Y);
                    if (Config.ShowTooltips && ImGui.IsMouseHoveringRect(pos, pos + _barConfig.Size))
                    {
                        TooltipsHelper.Instance.ShowTooltipOnCursor(
                            cooldown.TooltipText(),
                            cooldown.Data.Name,
                            cooldown.Data.ActionId
                        );
                    }

                    i++;
                }

                addedOffset = false;
            }
        }

        private uint? GetJobId(PartyCooldown cooldown, IPlayerCharacter player)
        {
            uint jobId = cooldown.Data.JobId;
            if (jobId != 0) { return jobId; }

            if (cooldown.Member != null) { return cooldown.Member.JobId; }

            if (cooldown.SourceId == player.GameObjectId) { return player.ClassJob.RowId; }

            ICharacter? chara = Plugin.ObjectTable.SearchById(cooldown.SourceId) as ICharacter;
            return chara?.ClassJob.RowId;
        }
    }
}
