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
    public class PartyCooldownsHud : DraggableHudElement
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
            int columnCount = PartyCooldownsDataConfig.ColumnCount;
            int rowCount = 4; // hardcoded just for draggable area purposes

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

                    PluginConfigColor fillColor = effectTime > 0 ? _barConfig.AvailableColor : _barConfig.RechargingColor;
                    PluginConfigColor bgColor = effectTime > 0 || cooldownTime == 0 ? _barConfig.AvailableBackgroundColor : _barConfig.RechargingBackgroundColor;

                    Rect background = new Rect(pos, size, bgColor);
                    Rect fill = BarUtilities.GetFillRect(pos, size, _barConfig.FillDirection, fillColor, current, max);

                    string barId = _barConfig.ID + $"_{offsetX + i}";
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
                    bar.Draw(origin);

                    // icon
                    Vector2 iconPos = origin + new Vector2(Config.Position.X + offsetX, y);
                    Vector2 iconSize = new Vector2(size.Y);

                    DrawHelper.DrawInWindow(barId + "_enmityIcon", iconPos, iconSize, false, false, (drawList) =>
                    {
                        DrawHelper.DrawIcon(cooldown.Data.IconId, iconPos, iconSize, false, drawList);

                        if (_barConfig.DrawBorder)
                        {
                            drawList.AddRect(iconPos, iconPos + iconSize, _barConfig.BorderColor.Base, 0, ImDrawFlags.None, _barConfig.BorderThickness);
                        }
                    });

                    // name
                    Character? character = cooldown.Member?.Character ?? cooldown.SourceId == player.ObjectId ? player : null;
                    string? name = character == null ? "Fake Name" : null;
                    _nameLabelHud.Draw(origin + pos, size, character, name);

                    // time
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
                    _timeLabelHud.Draw(origin + pos, size, character);

                    i++;
                }

                addedOffset = false;
            }
        }
    }
}
