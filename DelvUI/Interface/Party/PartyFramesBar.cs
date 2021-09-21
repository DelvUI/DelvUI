using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using DelvUI.Interface.StatusEffects;
using ImGuiNET;
using System;
using System.Numerics;


namespace DelvUI.Interface.Party
{
    public class PartyFramesBar
    {
        private PartyFramesBarsConfig _config;
        private PartyFramesBuffsConfig _buffsConfig;
        private PartyFramesDebuffsConfig _debuffsConfig;

        private LabelHud _labelHud;
        private StatusEffectsListHud _buffsListHud;
        private StatusEffectsListHud _debuffsListHud;

        public IPartyFramesMember Member;
        public bool Visible = false;
        public Vector2 Position;

        public PartyFramesBar(string id, PartyFramesBarsConfig config, PartyFramesBuffsConfig buffsConfig, PartyFramesDebuffsConfig debuffsConfig)
        {
            _config = config;
            _buffsConfig = buffsConfig;
            _debuffsConfig = debuffsConfig;

            _labelHud = new LabelHud("partyFramesBar_Label_" + id, config.NameLabelConfig);
            _buffsListHud = new StatusEffectsListHud("partyFramesBar_Buffs_" + id, buffsConfig, "");
            _debuffsListHud = new StatusEffectsListHud("partyFramesBar_Debuffs_" + id, debuffsConfig, "");
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
            var actor = Member.GetActor();
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
                var color = GetColor();

                if (_config.RangeConfig.Enabled && actor != null)
                {
                    var alpha = _config.RangeConfig.AlphaForDistance(actor.YalmDistanceX) / 100f;
                    PluginLog.Log(alpha.ToString());
                    color = new(color.Vector.WithNewAlpha(alpha));
                }

                DrawHelper.DrawGradientFilledRect(Position, fillSize, color, drawList);
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
            ImGui.BeginChild("child_" + _buffsListHud.ID);
            var buffsPos = CalculatePositionForAnchor(_buffsConfig.Anchor);
            _buffsListHud.Actor = Member.GetActor();
            _buffsListHud.Draw(buffsPos);
            ImGui.EndChild();

            ImGui.BeginChild("child_" + _debuffsListHud.ID);
            var debuffsPos = CalculatePositionForAnchor(_debuffsConfig.Anchor);
            _debuffsListHud.Actor = Member.GetActor();
            _debuffsListHud.Draw(debuffsPos);
            ImGui.EndChild();

            // mana
            if (_config.ManaBarConfig.Enabled && Member.MaxHP > 0 &&
                (!_config.ManaBarConfig.ShowOnlyForHealers || JobsHelper.IsJobHealer(Member.JobId)))
            {
                var manaBarPos = Position + new Vector2(0, _config.Size.Y - _config.ManaBarConfig.Height);
                var manaBarSize = new Vector2(_config.Size.X, _config.ManaBarConfig.Height);

                drawList.AddRectFilled(manaBarPos, manaBarPos + manaBarSize, _config.ManaBarConfig.BackgroundColor.Base);

                var scale = (float)Member.MP / (float)Member.MaxMP;
                var fillSize = new Vector2(Math.Max(1, _config.Size.X * scale), manaBarSize.Y);

                DrawHelper.DrawGradientFilledRect(manaBarPos, fillSize, _config.ManaBarConfig.Color, drawList);
            }

            // name
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

            // icon
            if (_config.RoleIconConfig.Enabled && isClose)
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

        private Vector2 CalculatePositionForAnchor(HudElementAnchor anchor)
        {
            switch (anchor)
            {
                case HudElementAnchor.TopLeft: return Position;
                case HudElementAnchor.TopRight: return Position + new Vector2(_config.Size.X, 0);
                case HudElementAnchor.BottomLeft: return Position + new Vector2(0, _config.Size.Y);
            }

            return Position + _config.Size;
        }
    }
}
