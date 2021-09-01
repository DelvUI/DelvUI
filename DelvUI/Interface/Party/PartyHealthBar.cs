using System.Collections.Generic;
using ImGuiNET;
using System.Numerics;
using DelvUI.Helpers;
using System;
using Lumina.Excel.GeneratedSheets;


namespace DelvUI.Interface.Party
{
    class PartyHealthBar
    {
        private PluginConfiguration pluginConfiguration;

        private IGroupMember _member = null;
        public IGroupMember Member
        {
            get { return _member; }
            set { 
                _member = value;
                UpdateColor();
            }
        }

        public bool Visible = false;
        public Vector2 Position;
        public Vector2 Size;
        private Dictionary<string, uint> color;

        public PartyHealthBar(PluginConfiguration pluginConfiguration)
        {
            this.pluginConfiguration = pluginConfiguration;
            color = pluginConfiguration.NPCColorMap["friendly"];
        }

        public void UpdateColor()
        {
            color = pluginConfiguration.NPCColorMap["friendly"];
            if (Member == null) return;

            if (pluginConfiguration.PartyListUseRoleColors)
            {
                color = ColorForJob(Member.JobId);
            }
            else
            {
                pluginConfiguration.JobColorMap.TryGetValue(Member.JobId, out color);
            }
        }

        private Dictionary<string, uint> ColorForJob(uint jodId)
        {
            var role = JobsHelper.RoleForJob(jodId);

            switch (role) {
                case JobRoles.Tank: return pluginConfiguration.PartyListColorMap["tank"];
                case JobRoles.DPS: return pluginConfiguration.PartyListColorMap["dps"];
                case JobRoles.Healer: return pluginConfiguration.PartyListColorMap["healer"];
            }

            return pluginConfiguration.PartyListColorMap["generic_role"];
        }

        public void Draw(ImDrawListPtr drawList, Vector2 origin)
        {
            if (!Visible) return;

            // bg
            var isClose = Member.MaxHP > 0;
            var bgColorMap = pluginConfiguration.PartyListColorMap["background"];
            drawList.AddRectFilled(Position, Position + Size, isClose ? (uint)bgColorMap["default"] : (uint)bgColorMap["outOfRange"]);

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
            if (pluginConfiguration.PartyListShieldEnabled)
            {
                if (pluginConfiguration.PartyListShieldFillHealthFirst && Member.MaxHP > 0)
                {
                    DrawHelper.DrawShield(Member.Shield, (float)Member.HP / Member.MaxHP, Position, Size,
                        pluginConfiguration.PartyListShieldHeight, !pluginConfiguration.PartyListShieldHeightPixels,
                        pluginConfiguration.PartyListColorMap["shield"]);
                }
                else
                {
                    DrawHelper.DrawShield(Member.Shield, Position, Size,
                        pluginConfiguration.PartyListShieldHeight, !pluginConfiguration.PartyListShieldHeightPixels,
                        pluginConfiguration.PartyListColorMap["shield"]);
                }
            }

            // buffs / debuffs
            var statusEffects = Member.StatusEffects;
            var pos = Position + Size - origin + new Vector2(6, 1);

            for (int s = 0; s < statusEffects.Length; s++)
            {
                var id = (uint)statusEffects[s].EffectId;
                if (id == 0) continue;

                var texture = TexturesCache.Instance.GetTexture<Status>(id, (uint)Math.Max(0, statusEffects[s].StackCount - 1));
                if (texture == null) continue;

                var size = new Vector2(texture.Width, texture.Height);
                ImGui.SetCursorPos(pos - size);
                ImGui.Image(texture.ImGuiHandle, size);

                pos.X = pos.X - size.X - 1;
            }

            // name
            var actor = Member.GetActor();
            var name = Member.Name.Abbreviate() ?? "";

            if (actor != null)
            {
                name = TextTags.GenerateFormattedTextFromTags(actor, pluginConfiguration.PartyListHealthBarText);
            }

            var textSize = ImGui.CalcTextSize(name);
            var textPos = new Vector2(Position.X + Size.X / 2f - textSize.X / 2f, Position.Y + Size.Y / 2f - textSize.Y / 2f);
            drawList.AddText(textPos, actor == null ? 0x44FFFFFF : 0xFFFFFFFF, name);
        }
    }
}
