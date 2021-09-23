﻿using Dalamud.Plugin;
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
        private PartyFramesManaBarConfig _manaBarConfig;
        private PartyFramesCastbarConfig _castbarConfig;
        private PartyFramesRoleIconConfig _roleIconConfig;
        private PartyFramesBuffsConfig _buffsConfig;
        private PartyFramesDebuffsConfig _debuffsConfig;

        private LabelHud _labelHud;
        private LabelHud _manaLabelHud;
        private CastbarHud _castbarHud;
        private StatusEffectsListHud _buffsListHud;
        private StatusEffectsListHud _debuffsListHud;

        public IPartyFramesMember Member;
        public bool Visible = false;
        public Vector2 Position;

        public PartyFramesBar(
            string id,
            PartyFramesHealthBarsConfig config,
            PartyFramesManaBarConfig manaBarConfig,
            PartyFramesCastbarConfig castbarConfig,
            PartyFramesRoleIconConfig roleIconConfig,
            PartyFramesBuffsConfig buffsConfig,
            PartyFramesDebuffsConfig debuffsConfig
        )
        {
            _config = config;
            _manaBarConfig = manaBarConfig;
            _castbarConfig = castbarConfig;
            _roleIconConfig = roleIconConfig;
            _buffsConfig = buffsConfig;
            _debuffsConfig = debuffsConfig;

            _labelHud = new LabelHud("partyFramesBar_label_" + id, config.NameLabelConfig);
            _manaLabelHud = new LabelHud("partyFramesBar_manaLabel_" + id, _manaBarConfig.ValueLabelConfig);
            _castbarHud = new CastbarHud("partyFramesBar_castbar_" + id, _castbarConfig, "");
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
            var buffsPos = Utils.GetAnchoredPosition(Position, -_config.Size, _buffsConfig.HealthBarAnchor);
            _buffsListHud.Actor = actor;
            _buffsListHud.Draw(buffsPos);

            var debuffsPos = Utils.GetAnchoredPosition(Position, -_config.Size, _debuffsConfig.HealthBarAnchor);
            _debuffsListHud.Actor = actor;
            _debuffsListHud.Draw(debuffsPos);

            // mana
            if (_manaBarConfig.Enabled && Member.MaxHP > 0 &&
                (!_manaBarConfig.ShowOnlyForHealers || JobsHelper.IsJobHealer(Member.JobId)))
            {
                var parentPos = Utils.GetAnchoredPosition(Position, -_config.Size, _manaBarConfig.HealthBarAnchor);
                var manaBarPos = Utils.GetAnchoredPosition(parentPos + _manaBarConfig.Position, _manaBarConfig.Size, _manaBarConfig.Anchor);

                drawList.AddRectFilled(manaBarPos, manaBarPos + _manaBarConfig.Size, _manaBarConfig.BackgroundColor.Base);

                var scale = (float)Member.MP / (float)Member.MaxMP;
                var fillSize = new Vector2(Math.Max(1, _config.Size.X * scale), _manaBarConfig.Size.Y);

                DrawHelper.DrawGradientFilledRect(manaBarPos, fillSize, _manaBarConfig.Color, drawList);

                _manaLabelHud.Draw(manaBarPos, _manaBarConfig.Size, actor);
            }

            // castbar
            var castbarPos = Utils.GetAnchoredPosition(Position, -_config.Size, _castbarConfig.HealthBarAnchor);
            _castbarHud.Actor = actor;
            _castbarHud.Draw(castbarPos);

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
    }
}
