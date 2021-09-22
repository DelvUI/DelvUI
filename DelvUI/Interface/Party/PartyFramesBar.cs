using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Enums;
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
        private PartyFramesHealthBarsConfig _config;
        private PartyFramesManaBarConfig _manaBarsConfig;
        private PartyFramesRoleIconConfig _roleIconConfig;
        private PartyFramesBuffsConfig _buffsConfig;
        private PartyFramesDebuffsConfig _debuffsConfig;

        private LabelHud _labelHud;
        private LabelHud _manaLabelHud;
        private StatusEffectsListHud _buffsListHud;
        private StatusEffectsListHud _debuffsListHud;

        public IPartyFramesMember Member;
        public bool Visible = false;
        public Vector2 Position;

        public PartyFramesBar(
            string id,
            PartyFramesHealthBarsConfig config,
            PartyFramesManaBarConfig manaBarsConfig,
            PartyFramesRoleIconConfig roleIconConfig,
            PartyFramesBuffsConfig buffsConfig,
            PartyFramesDebuffsConfig debuffsConfig
        )
        {
            _config = config;
            _manaBarsConfig = manaBarsConfig;
            _roleIconConfig = roleIconConfig;
            _buffsConfig = buffsConfig;
            _debuffsConfig = debuffsConfig;

            _labelHud = new LabelHud("partyFramesBar_label_" + id, config.NameLabelConfig);
            _manaLabelHud = new LabelHud("partyFramesBar_manaLabel_" + id, _manaBarsConfig.ValueLabelConfig);
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

            // border
            var borderPos = Position - Vector2.One;
            var borderSize = _config.Size + Vector2.One * 2;
            drawList.AddRect(borderPos, borderPos + borderSize, 0xFF000000);

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
            if (_manaBarsConfig.Enabled && Member.MaxHP > 0 &&
                (!_manaBarsConfig.ShowOnlyForHealers || JobsHelper.IsJobHealer(Member.JobId)))
            {
                var parentPos = Utils.GetAnchoredPosition(Position, -_config.Size, _manaBarsConfig.HealthBarAnchor);
                var manaBarPos = Utils.GetAnchoredPosition(parentPos + _manaBarsConfig.Position, _manaBarsConfig.Size, _manaBarsConfig.Anchor);

                drawList.AddRectFilled(manaBarPos, manaBarPos + _manaBarsConfig.Size, _manaBarsConfig.BackgroundColor.Base);

                var scale = (float)Member.MP / (float)Member.MaxMP;
                var fillSize = new Vector2(Math.Max(1, _config.Size.X * scale), _manaBarsConfig.Size.Y);

                DrawHelper.DrawGradientFilledRect(manaBarPos, fillSize, _manaBarsConfig.Color, drawList);

                _manaLabelHud.Draw(manaBarPos, _manaBarsConfig.Size, actor);
            }

            // name
            var name = Member.Name ?? "";

            if (actor != null)
            {
                _labelHud.Draw(Position, _config.Size, actor);
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

                _labelHud.Draw(Position, _config.Size);

                _config.NameLabelConfig.SetText(previousText);
                _config.NameLabelConfig.Color = previousColor;
                _config.NameLabelConfig.OutlineColor = previousOutlineColor;
            }

            // icon
            if (_roleIconConfig.Enabled && isClose)
            {
                var iconId = _roleIconConfig.UseRoleIcons ?
                    JobsHelper.RoleIconIDForJob(Member.JobId, _roleIconConfig.UseSpecificDPSRoleIcons) :
                    JobsHelper.IconIDForJob(Member.JobId) + (uint)_roleIconConfig.Style * 100;

                var parentPos = Utils.GetAnchoredPosition(Position, -_config.Size, _roleIconConfig.HealthBarAnchor);
                var iconPos = Utils.GetAnchoredPosition(parentPos + _roleIconConfig.Position, _roleIconConfig.Size, _roleIconConfig.Anchor);

                DrawHelper.DrawIcon(iconId, iconPos, _roleIconConfig.Size, false, drawList);
            }

            // highlight
            if (_config.ColorsConfig.ShowHighlight && isHovering)
            {
                drawList.AddRectFilled(Position, Position + _config.Size, _config.ColorsConfig.HighlightColor.Base);
            }

        }

        private Vector2 CalculatePositionForAnchor(DrawAnchor anchor)
        {
            switch (anchor)
            {
                case DrawAnchor.TopLeft: return Position;
                case DrawAnchor.TopRight: return Position + new Vector2(_config.Size.X, 0);
                case DrawAnchor.BottomLeft: return Position + new Vector2(0, _config.Size.Y);
            }

            return Position + _config.Size;
        }
    }
}
