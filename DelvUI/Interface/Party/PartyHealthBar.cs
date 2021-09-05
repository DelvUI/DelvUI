using DelvUI.Config;
using DelvUI.Helpers;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;


namespace DelvUI.Interface.Party
{
    public class PartyHealthBar
    {
        private PluginConfiguration _pluginConfiguration;
        private PartyHudConfig _config;

        private IGroupMember _member = null;
        public IGroupMember Member
        {
            get { return _member; }
            set
            {
                _member = value;
                UpdateColor();
            }
        }

        public bool Visible = false;
        public Vector2 Position;
        public Vector2 Size;
        private Dictionary<string, uint> color;

        public PartyHealthBar(PluginConfiguration pluginConfiguration, PartyHudConfig config)
        {
            _pluginConfiguration = pluginConfiguration;
            _config = config;
        }

        public void UpdateColor()
        {
            color = _config.SortConfig.GenericRoleColor.Map;
            if (Member == null)
            {
                return;
            }

            if (_config.SortConfig.UseRoleColors)
            {
                color = ColorForJob(Member.JobId);
            }
            else
            {
                _pluginConfiguration.JobColorMap.TryGetValue(Member.JobId, out color);
            }
        }

        private Dictionary<string, uint> ColorForJob(uint jodId)
        {
            var role = JobsHelper.RoleForJob(jodId);

            switch (role)
            {
                case JobRoles.Tank: return _config.SortConfig.TankRoleColor.Map;
                case JobRoles.DPS: return _config.SortConfig.DPSRoleColor.Map;
                case JobRoles.Healer: return _config.SortConfig.HealerRoleColor.Map;
            }

            return _config.SortConfig.GenericRoleColor.Map;
        }

        public void Draw(ImDrawListPtr drawList, Vector2 origin)
        {
            if (!Visible)
            {
                return;
            }

            // bg
            var isClose = Member.MaxHP > 0;
            var bgColorMap = isClose ? _config.HealthBarsConfig.BackgroundColor.Base : _config.HealthBarsConfig.UnreachableColor.Base;
            drawList.AddRectFilled(Position, Position + Size, bgColorMap);

            // hp
            if (isClose)
            {
                var scale = Member.MaxHP > 0 ? (float)Member.HP / (float)Member.MaxHP : 1;
                var fillPos = Position;
                var fillSize = new Vector2(Math.Max(1, Size.X * scale), Size.Y / 2f);
                drawList.AddRectFilledMultiColor(
                    fillPos, fillPos + fillSize,
                    color["gradientLeft"], color["gradientLeft"], color["gradientRight"], color["gradientRight"]
                );

                fillPos.Y = fillPos.Y + Size.Y / 2f;
                drawList.AddRectFilledMultiColor(
                    fillPos, fillPos + fillSize,
                    color["gradientRight"], color["gradientRight"], color["gradientLeft"], color["gradientLeft"]
                );
            }

            // shield
            if (_config.HealthBarsConfig.ShieldsConfig.Enabled)
            {
                if (_config.HealthBarsConfig.ShieldsConfig.FillHealthFirst && Member.MaxHP > 0)
                {
                    DrawHelper.DrawShield(Member.Shield, (float)Member.HP / Member.MaxHP, Position, Size,
                        _config.HealthBarsConfig.ShieldsConfig.Height, !_config.HealthBarsConfig.ShieldsConfig.HeightInPixels,
                        _config.HealthBarsConfig.ShieldsConfig.Color.Map);
                }
                else
                {
                    DrawHelper.DrawShield(Member.Shield, Position, Size,
                        _config.HealthBarsConfig.ShieldsConfig.Height, !_config.HealthBarsConfig.ShieldsConfig.HeightInPixels,
                        _config.HealthBarsConfig.ShieldsConfig.Color.Map);
                }
            }

            // buffs / debuffs
            //var statusEffects = Member.StatusEffects;
            //var pos = Position + Size - origin + new Vector2(6, 1);

            //for (int s = 0; s < statusEffects.Length; s++)
            //{
            //    var id = (uint)statusEffects[s].EffectId;
            //    if (id == 0) continue;

            //    var texture = TexturesCache.Instance.GetTexture<Status>(id, (uint)Math.Max(0, statusEffects[s].StackCount - 1));
            //    if (texture == null) continue;

            //    var size = new Vector2(texture.Width, texture.Height);
            //    ImGui.SetCursorPos(pos - size);
            //    ImGui.Image(texture.ImGuiHandle, size);

            //    pos.X = pos.X - size.X - 1;
            //}

            // name
            var actor = Member.GetActor();
            var name = Member.Name.Abbreviate() ?? "";

            if (actor != null)
            {
                name = TextTags.GenerateFormattedTextFromTags(actor, _config.HealthBarsConfig.TextFormat);
            }

            var textSize = ImGui.CalcTextSize(name);
            var textPos = new Vector2(Position.X + Size.X / 2f - textSize.X / 2f, Position.Y + Size.Y / 2f - textSize.Y / 2f);
            drawList.AddText(textPos, actor == null ? 0x44FFFFFF : 0xFFFFFFFF, name);
        }
    }
}
