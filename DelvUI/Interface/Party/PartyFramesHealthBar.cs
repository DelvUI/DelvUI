using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;


namespace DelvUI.Interface.Party
{
    public class PartyFramesHealthBar
    {
        private PartyFramesHealthBarsConfig _config;

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

        public PartyFramesHealthBar(PartyFramesHealthBarsConfig config)
        {
            _config = config;
        }

        public void UpdateColor()
        {
            color = _config.ColorsConfig.GenericRoleColor.Map;
            if (Member == null)
            {
                return;
            }

            if (_config.ColorsConfig.UseRoleColors)
            {
                color = ColorForJob(Member.JobId);
            }
            else
            {
                color = GlobalColors.Instance.SafeColorForJobId(Member.JobId).Map;
            }
        }

        private Dictionary<string, uint> ColorForJob(uint jodId)
        {
            var role = JobsHelper.RoleForJob(jodId);

            switch (role)
            {
                case JobRoles.Tank: return _config.ColorsConfig.TankRoleColor.Map;
                case JobRoles.DPS: return _config.ColorsConfig.DPSRoleColor.Map;
                case JobRoles.Healer: return _config.ColorsConfig.HealerRoleColor.Map;
            }

            return _config.ColorsConfig.GenericRoleColor.Map;
        }

        public void Draw(ImDrawListPtr drawList, Vector2 origin)
        {
            if (!Visible)
            {
                return;
            }

            // bg
            var isClose = Member.MaxHP > 0;
            var bgColorMap = isClose ? _config.ColorsConfig.BackgroundColor.Base : _config.ColorsConfig.UnreachableColor.Base;
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
            if (_config.ShieldConfig.Enabled)
            {
                if (_config.ShieldConfig.FillHealthFirst && Member.MaxHP > 0)
                {
                    DrawHelper.DrawShield(Member.Shield, (float)Member.HP / Member.MaxHP, Position, Size,
                        _config.ShieldConfig.Height, !_config.ShieldConfig.HeightInPixels,
                        _config.ShieldConfig.Color.Map, drawList);
                }
                else
                {
                    DrawHelper.DrawOvershield(Member.Shield, Position, Size,
                        _config.ShieldConfig.Height, !_config.ShieldConfig.HeightInPixels,
                        _config.ShieldConfig.Color.Map, drawList);
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
                name = TextTags.GenerateFormattedTextFromTags(actor, _config.TextFormat);
            }

            var textSize = ImGui.CalcTextSize(name);
            var textPos = new Vector2(Position.X + Size.X / 2f - textSize.X / 2f, Position.Y + Size.Y / 2f - textSize.Y / 2f);
            drawList.AddText(textPos, actor == null ? 0x44FFFFFF : 0xFFFFFFFF, name);
        }
    }
}
