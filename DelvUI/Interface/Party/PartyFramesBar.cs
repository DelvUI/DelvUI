using Dalamud.Game.ClientState.Actors.Types;
using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using System;
using System.Numerics;


namespace DelvUI.Interface.Party
{
    public class PartyFramesBar
    {
        private PartyFramesBarsConfig _config;
        private LabelHud _labelHud;

        public IPartyFramesMember Member;
        public bool Visible = false;
        public Vector2 Position;

        public PartyFramesBar(string id, PartyFramesBarsConfig config)
        {
            _config = config;

            _labelHud = new LabelHud("partyFramesBar_" + id, config.NameLabelConfig);
        }

        public PluginConfigColor GetColor()
        {
            var color = _config.ColorsConfig.GenericRoleColor;
            if (Member != null)
            {
                if (_config.ColorsConfig.UseRoleColors)
                {
                    color = ColorForJob(Member.JobId);
                }
                else
                {
                    color = GlobalColors.Instance.SafeColorForJobId(Member.JobId);
                }
            }

            return color;
        }

        private PluginConfigColor ColorForJob(uint jodId)
        {
            var role = JobsHelper.RoleForJob(jodId);

            switch (role)
            {
                case JobRoles.Tank: return _config.ColorsConfig.TankRoleColor;
                case JobRoles.DPS: return _config.ColorsConfig.DPSRoleColor;
                case JobRoles.Healer: return _config.ColorsConfig.HealerRoleColor;
            }

            return _config.ColorsConfig.GenericRoleColor;
        }

        public void Draw(Vector2 origin, ImDrawListPtr drawList)
        {
            if (!Visible)
            {
                return;
            }

            // click
            bool isHovering = ImGui.IsMouseHoveringRect(Position, Position + _config.Size);
            if (isHovering && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                Plugin.TargetManager.SetCurrentTarget(Member.GetActor());
            }

            // bg
            var isClose = Member.MaxHP > 0;
            var bgColorMap = isClose ? _config.ColorsConfig.BackgroundColor.Base : _config.ColorsConfig.UnreachableColor.Base;
            drawList.AddRectFilled(Position, Position + _config.Size, bgColorMap);

            // hp
            if (isClose)
            {
                var scale = Member.MaxHP > 0 ? (float)Member.HP / (float)Member.MaxHP : 1;
                var fillSize = new Vector2(Math.Max(1, _config.Size.X * scale), _config.Size.Y);

                DrawHelper.DrawGradientFilledRect(Position, fillSize, GetColor(), drawList);
            }

            // shield
            if (_config.ShieldConfig.Enabled)
            {
                if (_config.ShieldConfig.FillHealthFirst && Member.MaxHP > 0)
                {
                    DrawHelper.DrawShield(Member.Shield, (float)Member.HP / Member.MaxHP, Position, _config.Size,
                        _config.ShieldConfig.Height, !_config.ShieldConfig.HeightInPixels,
                        _config.ShieldConfig.Color, drawList);
                }
                else
                {
                    DrawHelper.DrawOvershield(Member.Shield, Position, _config.Size,
                        _config.ShieldConfig.Height, !_config.ShieldConfig.HeightInPixels,
                        _config.ShieldConfig.Color, drawList);
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
            var name = Member.Name ?? "";

            if (actor != null)
            {
                _labelHud.Draw(Position + _config.Size / 2f, _config.Size, actor);
            }
            else
            {
                var previousText = _config.NameLabelConfig.GetText();
                _config.NameLabelConfig.SetText(name);

                var previousColor = _config.NameLabelConfig.Color;
                var previousOutlineColor = _config.NameLabelConfig.OutlineColor;
                if (Member is not FakePartyFramesMember)
                {
                    _config.NameLabelConfig.Color = new(_config.NameLabelConfig.Color.Vector.AdjustColorAlpha(-.3f));
                    _config.NameLabelConfig.OutlineColor = new(_config.NameLabelConfig.OutlineColor.Vector.AdjustColorAlpha(-.3f));
                }

                _labelHud.Draw(Position + _config.Size / 2f, _config.Size);

                _config.NameLabelConfig.SetText(previousText);
                _config.NameLabelConfig.Color = previousColor;
                _config.NameLabelConfig.OutlineColor = previousOutlineColor;
            }

            //var textSize = ImGui.CalcTextSize(name);
            //var textPos = new Vector2(Position.X + _config.Size.X / 2f - textSize.X / 2f, Position.Y + _config.Size.Y / 2f - textSize.Y / 2f);
            //drawList.AddText(textPos, actor == null && Member is not FakePartyFramesMember ? 0x44FFFFFF : 0xFFFFFFFF, name);

            // icon
            if (_config.RoleIconConfig.Enabled)
            {
                var iconId = _config.RoleIconConfig.UseRoleIcons ?
                    JobsHelper.RoleIconIDForJob(Member.JobId, _config.RoleIconConfig.UseSpecificDPSRoleIcons) :
                    JobsHelper.IconIDForJob(Member.JobId) + (uint)_config.RoleIconConfig.Style * 100;

                DrawHelper.DrawIcon(iconId, Position + _config.RoleIconConfig.Position, _config.RoleIconConfig.Size, false, drawList);
            }

            // highlight
            if (_config.ColorsConfig.ShowHighlight && isHovering)
            {
                drawList.AddRectFilled(Position, Position + _config.Size, _config.ColorsConfig.HighlightColor.Base);
            }

            // border
            var borderPos = Position - Vector2.One;
            var borderSize = _config.Size + Vector2.One * 2;
            drawList.AddRect(borderPos, borderPos + borderSize, 0xFF000000);
        }
    }
}
