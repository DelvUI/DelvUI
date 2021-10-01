using Dalamud.Game.ClientState.Objects.Enums;
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
        public delegate void MovePlayerOrderHandler(PartyFramesBar bar);
        public MovePlayerOrderHandler? MovePlayerEvent;

        private readonly PartyFramesHealthBarsConfig _config;
        private readonly PartyFramesManaBarConfig _manaBarConfig;
        private readonly PartyFramesCastbarConfig _castbarConfig;
        private readonly PartyFramesRoleIconConfig _roleIconConfig;
        private readonly PartyFramesLeaderIconConfig _leaderIconConfig;
        private readonly PartyFramesBuffsConfig _buffsConfig;
        private readonly PartyFramesDebuffsConfig _debuffsConfig;

        private readonly LabelHud _nameLabelHud;
        private readonly LabelHud _manaLabelHud;
        private readonly LabelHud _orderLabelHud;
        private readonly CastbarHud _castbarHud;
        private readonly StatusEffectsListHud _buffsListHud;
        private readonly StatusEffectsListHud _debuffsListHud;

        public IPartyFramesMember? Member;
        public bool Visible = false;
        public Vector2 Position;

        public PartyFramesBar(
            string id,
            PartyFramesHealthBarsConfig config,
            PartyFramesManaBarConfig manaBarConfig,
            PartyFramesCastbarConfig castbarConfig,
            PartyFramesRoleIconConfig roleIconConfig,
            PartyFramesLeaderIconConfig leaderIconConfig,
            PartyFramesBuffsConfig buffsConfig,
            PartyFramesDebuffsConfig debuffsConfig
        )
        {
            _config = config;
            _manaBarConfig = manaBarConfig;
            _castbarConfig = castbarConfig;
            _roleIconConfig = roleIconConfig;
            _leaderIconConfig = leaderIconConfig;
            _buffsConfig = buffsConfig;
            _debuffsConfig = debuffsConfig;

            _nameLabelHud = new LabelHud("partyFramesBar_nameLabel_" + id, config.NameLabelConfig);
            _manaLabelHud = new LabelHud("partyFramesBar_manaLabel_" + id, _manaBarConfig.ValueLabelConfig);
            _orderLabelHud = new LabelHud("partyFramesBar_orderLabel_" + id, config.OrderLabelConfig);
            _castbarHud = new CastbarHud("partyFramesBar_castbar_" + id, _castbarConfig, "");
            _buffsListHud = new StatusEffectsListHud("partyFramesBar_Buffs_" + id, buffsConfig, "");
            _debuffsListHud = new StatusEffectsListHud("partyFramesBar_Debuffs_" + id, debuffsConfig, "");
        }

        public PluginConfigColor GetColor(float scale)
        {
            PluginConfigColor? color = _config.ColorsConfig.GenericRoleColor;

            if (Member != null && Member.Character?.ObjectKind != ObjectKind.BattleNpc)
            {
                if (_config.ColorsConfig.UseRoleColors)
                {
                    color = ColorForJob(Member.JobId);
                }
                else if (_config.ColorsConfig.UseColorBasedOnHealthValue)
                {
                    color = Utils.ColorByHealthValue(scale, _config.ColorsConfig.LowHealthColorThreshold / 100f, _config.ColorsConfig.FullHealthColorThreshold / 100f, _config.ColorsConfig.FullHealthColor, _config.ColorsConfig.LowHealthColor);
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
            JobRoles role = JobsHelper.RoleForJob(jodId);

            switch (role)
            {
                case JobRoles.Tank: return _config.ColorsConfig.TankRoleColor;
                case JobRoles.DPS: return _config.ColorsConfig.DPSRoleColor;
                case JobRoles.Healer: return _config.ColorsConfig.HealerRoleColor;
            }

            return _config.ColorsConfig.GenericRoleColor;
        }

        public void Draw(Vector2 origin, ImDrawListPtr drawList, PluginConfigColor? borderColor = null)
        {
            if (!Visible || Member is null)
            {
                return;
            }

            Dalamud.Game.ClientState.Objects.SubKinds.PlayerCharacter? player = Plugin.ClientState.LocalPlayer;
            if (player == null)
            {
                return;
            }

            // click
            bool isHovering = ImGui.IsMouseHoveringRect(Position, Position + _config.Size);
            Dalamud.Game.ClientState.Objects.Types.Character? character = Member.Character;

            if (isHovering)
            {
                MouseOverHelper.Instance.Target = character;

                // move player bar to this spot on ctrl+alt+shift click
                if (ImGui.GetIO().KeyCtrl && ImGui.GetIO().KeyAlt && ImGui.GetIO().KeyShift && ImGui.GetIO().MouseClicked[0])
                {
                    MovePlayerEvent?.Invoke(this);
                }
                // target
                else if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && character != null)
                {
                    Plugin.TargetManager.SetTarget(character);
                }
            }

            // bg
            drawList.AddRectFilled(Position, Position + _config.Size, _config.ColorsConfig.BackgroundColor.Base);

            // hp
            float hpScale = Member.MaxHP > 0 ? (float)Member.HP / (float)Member.MaxHP : 1;
            var hpFillSize = new Vector2(Math.Max(1, _config.Size.X * hpScale), _config.Size.Y);
            PluginConfigColor? hpColor = GetColor(hpScale);

            byte distance = character != null ? character.YalmDistanceX : byte.MaxValue;
            if (_config.RangeConfig.Enabled)
            {
                float currentAlpha = hpColor.Vector.W * 100f;
                float alpha = _config.RangeConfig.AlphaForDistance(distance, currentAlpha) / 100f;
                hpColor = new(hpColor.Vector.WithNewAlpha(alpha));
            }

            DrawHelper.DrawGradientFilledRect(Position, hpFillSize, hpColor, drawList);

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
            Vector2 borderPos = Position - Vector2.One;
            Vector2 borderSize = _config.Size + Vector2.One * 2;
            uint color = borderColor != null ? borderColor.Base : _config.ColorsConfig.BorderColor.Base;
            drawList.AddRect(borderPos, borderPos + borderSize, color);

            // buffs / debuffs
            Vector2 buffsPos = Utils.GetAnchoredPosition(Position, -_config.Size, _buffsConfig.HealthBarAnchor);
            _buffsListHud.Actor = character;
            _buffsListHud.Draw(buffsPos);

            Vector2 debuffsPos = Utils.GetAnchoredPosition(Position, -_config.Size, _debuffsConfig.HealthBarAnchor);
            _debuffsListHud.Actor = character;
            _debuffsListHud.Draw(debuffsPos);

            // mana
            if (_manaBarConfig.Enabled && Member.MaxHP > 0 &&
                (!_manaBarConfig.ShowOnlyForHealers || JobsHelper.IsJobHealer(Member.JobId)))
            {
                Vector2 parentPos = Utils.GetAnchoredPosition(Position, -_config.Size, _manaBarConfig.HealthBarAnchor);
                Vector2 manaBarPos = Utils.GetAnchoredPosition(parentPos + _manaBarConfig.Position, _manaBarConfig.Size, _manaBarConfig.Anchor);

                drawList.AddRectFilled(manaBarPos, manaBarPos + _manaBarConfig.Size, _manaBarConfig.BackgroundColor.Base);

                float scale = (float)Member.MP / (float)Member.MaxMP;
                var fillSize = new Vector2(Math.Max(1, _config.Size.X * scale), _manaBarConfig.Size.Y);

                DrawHelper.DrawGradientFilledRect(manaBarPos, fillSize, _manaBarConfig.Color, drawList);

                _manaLabelHud.Draw(manaBarPos, _manaBarConfig.Size, character);
            }

            // castbar
            Vector2 castbarPos = Utils.GetAnchoredPosition(Position, -_config.Size, _castbarConfig.HealthBarAnchor);
            _castbarHud.Actor = character;
            _castbarHud.Draw(castbarPos);

            // name
            _nameLabelHud.Draw(Position, _config.Size, character, Member.Name);

            // order
            if (character == null || character?.ObjectKind != ObjectKind.BattleNpc)
            {
                int order = Member.ObjectId == player.ObjectId ? 1 : Member.Order;
                _config.OrderLabelConfig.SetText("[" + order + "]");
                _orderLabelHud.Draw(Position, _config.Size);
            }

            // role/job icon
            if (_roleIconConfig.Enabled && Member.JobId > 0)
            {
                uint iconId;

                // chocobo icon
                if (character != null && character.ObjectKind == ObjectKind.BattleNpc)
                {
                    iconId = JobsHelper.RoleIconIDForBattleCompanion + (uint)_roleIconConfig.Style * 100;
                }
                // role/job icon
                else
                {
                    iconId = _roleIconConfig.UseRoleIcons ?
                        JobsHelper.RoleIconIDForJob(Member.JobId, _roleIconConfig.UseSpecificDPSRoleIcons) :
                        JobsHelper.IconIDForJob(Member.JobId) + (uint)_roleIconConfig.Style * 100;
                }

                if (iconId > 0)
                {
                    Vector2 parentPos = Utils.GetAnchoredPosition(Position, -_config.Size, _roleIconConfig.HealthBarAnchor);
                    Vector2 iconPos = Utils.GetAnchoredPosition(parentPos + _roleIconConfig.Position, _roleIconConfig.Size, _roleIconConfig.Anchor);

                    DrawHelper.DrawIcon(iconId, iconPos, _roleIconConfig.Size, false, drawList);
                }
            }

            // leader icon
            if (_leaderIconConfig.Enabled && Member.IsPartyLeader)
            {
                Vector2 parentPos = Utils.GetAnchoredPosition(Position, -_config.Size, _leaderIconConfig.HealthBarAnchor);
                Vector2 iconPos = Utils.GetAnchoredPosition(parentPos + _leaderIconConfig.Position, _leaderIconConfig.Size, _leaderIconConfig.Anchor);

                DrawHelper.DrawIcon(61521, iconPos, _leaderIconConfig.Size, false, drawList);
            }

            // highlight
            if (_config.ColorsConfig.ShowHighlight && isHovering)
            {
                drawList.AddRectFilled(Position, Position + _config.Size, _config.ColorsConfig.HighlightColor.Base);
            }
        }
    }
}
