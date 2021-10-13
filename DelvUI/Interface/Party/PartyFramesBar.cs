using Dalamud.Game.ClientState.Objects.Enums;
using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using DelvUI.Interface.StatusEffects;
using ImGuiNET;
using System;
using System.Globalization;
using System.Numerics;
using Dalamud.Logging;
using System.Windows.Forms;

namespace DelvUI.Interface.Party
{
    public class PartyFramesBar
    {
        public delegate void PartyFramesBarEventHandler(PartyFramesBar bar);
        public PartyFramesBarEventHandler? MovePlayerEvent;
        public PartyFramesBarEventHandler? OpenContextMenuEvent;

        private PartyFramesHealthBarsConfig _config;
        private PartyFramesManaBarConfig _manaBarConfig;
        private PartyFramesCastbarConfig _castbarConfig;
        private PartyFramesRoleIconConfig _roleIconConfig;
        private PartyFramesLeaderIconConfig _leaderIconConfig;
        private PartyFramesBuffsConfig _buffsConfig;
        private PartyFramesDebuffsConfig _debuffsConfig;
        private PartyFramesRaiseTrackerConfig _raiseTrackerConfig;
        private PartyFramesInvulnTrackerConfig _invulnTrackerConfig;

        private LabelHud _nameLabelHud;
        private LabelHud _healthLabelHud;
        private LabelHud _manaLabelHud;
        private LabelHud _orderLabelHud;
        private LabelHud _raiseLabelHud;
        private LabelHud _invulnLabelHud;
        private CastbarHud _castbarHud;
        private StatusEffectsListHud _buffsListHud;
        private StatusEffectsListHud _debuffsListHud;

        public bool Visible = false;
        public Vector2 Position;

        private SmoothHPHelper _smoothHPHelper = new SmoothHPHelper();

        private IPartyFramesMember? _member = null;
        public IPartyFramesMember? Member
        {
            get => _member;
            set
            {
                if (_member == value) { return; }

                _member = value;
                _smoothHPHelper.Reset();
            }
        }


        public PartyFramesBar(
            string id,
            PartyFramesHealthBarsConfig config,
            PartyFramesManaBarConfig manaBarConfig,
            PartyFramesCastbarConfig castbarConfig,
            PartyFramesRoleIconConfig roleIconConfig,
            PartyFramesLeaderIconConfig leaderIconConfig,
            PartyFramesBuffsConfig buffsConfig,
            PartyFramesDebuffsConfig debuffsConfig,
            PartyFramesRaiseTrackerConfig raiseTrackerConfig,
            PartyFramesInvulnTrackerConfig invulnTrackerConfig
        )
        {
            _config = config;
            _manaBarConfig = manaBarConfig;
            _castbarConfig = castbarConfig;
            _roleIconConfig = roleIconConfig;
            _leaderIconConfig = leaderIconConfig;
            _buffsConfig = buffsConfig;
            _debuffsConfig = debuffsConfig;
            _raiseTrackerConfig = raiseTrackerConfig;
            _invulnTrackerConfig = invulnTrackerConfig;

            _nameLabelHud = new LabelHud(config.NameLabelConfig);
            _healthLabelHud = new LabelHud(config.HealthLabelConfig);
            _manaLabelHud = new LabelHud(_manaBarConfig.ValueLabelConfig);
            _orderLabelHud = new LabelHud(config.OrderLabelConfig);
            _raiseLabelHud = new LabelHud(_raiseTrackerConfig.LabelConfig);
            _invulnLabelHud = new LabelHud(_invulnTrackerConfig.LabelConfig);

            _castbarHud = new CastbarHud(_castbarConfig, "");
            _buffsListHud = new StatusEffectsListHud(buffsConfig, "");
            _debuffsListHud = new StatusEffectsListHud(debuffsConfig, "");
        }

        public PluginConfigColor GetColor(float scale)
        {
            var color = _config.ColorsConfig.GenericRoleColor;

            if (Member != null && Member.Character?.ObjectKind != ObjectKind.BattleNpc)
            {
                if (_config.ColorsConfig.UseRoleColors)
                {
                    color = ColorForJob(Member.JobId);
                }
                else if (_config.ColorsConfig.UseColorBasedOnHealthValue)
                {
                    color = Utils.GetColorByScale(scale, _config.ColorsConfig.LowHealthColorThreshold / 100f, _config.ColorsConfig.FullHealthColorThreshold / 100f, _config.ColorsConfig.LowHealthColor, _config.ColorsConfig.FullHealthColor, _config.ColorsConfig.blendMode);
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
                case JobRoles.Healer: return _config.ColorsConfig.HealerRoleColor;

                case JobRoles.DPSMelee:
                case JobRoles.DPSRanged:
                case JobRoles.DPSCaster:
                    return _config.ColorsConfig.DPSRoleColor;
            }

            return _config.ColorsConfig.GenericRoleColor;
        }

        public void Draw(Vector2 origin, ImDrawListPtr drawList, PluginConfigColor? borderColor = null)
        {
            if (!Visible || Member is null)
            {
                return;
            }

            // click
            bool isHovering = ImGui.IsMouseHoveringRect(Position, Position + _config.Size);
            var character = Member.Character;

            if (isHovering)
            {
                MouseOverHelper.Instance.Target = character;

                // move player bar to this spot on ctrl+alt+shift click
                if (ImGui.GetIO().KeyCtrl && ImGui.GetIO().KeyAlt && ImGui.GetIO().KeyShift && Control.MouseButtons == MouseButtons.Left)
                {
                    MovePlayerEvent?.Invoke(this);
                }
                // target
                else if (Control.MouseButtons == MouseButtons.Left && character != null)
                {
                    Plugin.TargetManager.SetTarget(character);
                }
                // context menu
                else if (Control.MouseButtons == MouseButtons.Right)
                {
                    OpenContextMenuEvent?.Invoke(this);
                }
            }

            // bg
            PluginConfigColor bgColor;
            if (Member.RaiseTime != null && _raiseTrackerConfig.Enabled && _raiseTrackerConfig.ChangeBackgroundColorWhenRaised)
            {
                bgColor = _raiseTrackerConfig.BackgroundColor;
            }
            else if (Member.InvulnStatus?.InvulnTime != null && _invulnTrackerConfig.Enabled && _invulnTrackerConfig.ChangeBackgroundColorWhenInvuln)
            {
                bgColor = Member.InvulnStatus?.InvulnId == 811 ? _invulnTrackerConfig.WalkingDeadBackgroundColor : _invulnTrackerConfig.BackgroundColor;
            }
            else
            {
                bgColor = _config.ColorsConfig.BackgroundColor;
            }

            drawList.AddRectFilled(Position, Position + _config.Size, bgColor.Base);

            // hp
            uint currentHp = Member.HP;
            uint maxHp = Member.MaxHP;

            if (_config.SmoothHealthConfig.Enabled)
            {
                currentHp = _smoothHPHelper.GetNextHp((int)currentHp, (int)maxHp, _config.SmoothHealthConfig.Velocity);
            }

            var hpScale = maxHp > 0 ? (float)currentHp / (float)maxHp : 1;
            var hpFillSize = new Vector2(_config.Size.X * hpScale, _config.Size.Y);
            PluginConfigColor? hpColor = GetColor(hpScale);

            var distance = character != null ? character.YalmDistanceX : byte.MaxValue;
            if (_config.RangeConfig.Enabled)
            {
                var currentAlpha = hpColor.Vector.W * 100f;
                var alpha = _config.RangeConfig.AlphaForDistance(distance, currentAlpha) / 100f;
                hpColor = new(hpColor.Vector.WithNewAlpha(alpha));
            }

            DrawHelper.DrawGradientFilledRect(Position, hpFillSize, hpColor, drawList);

            // shield
            if (_config.ShieldConfig.Enabled)
            {
                if (_config.ShieldConfig.FillHealthFirst && Member.MaxHP > 0)
                {
                    DrawHelper.DrawShield(Member.Shield, (float)currentHp / maxHp, Position, _config.Size,
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
            var color = borderColor != null ? borderColor.Base : _config.ColorsConfig.BorderColor.Base;
            drawList.AddRect(borderPos, borderPos + borderSize, color);

            // mana
            if (ShowMana())
            {
                var parentPos = Utils.GetAnchoredPosition(Position, -_config.Size, _manaBarConfig.HealthBarAnchor);
                var manaBarPos = Utils.GetAnchoredPosition(parentPos + _manaBarConfig.Position, _manaBarConfig.Size, _manaBarConfig.Anchor);

                drawList.AddRectFilled(manaBarPos, manaBarPos + _manaBarConfig.Size, _manaBarConfig.BackgroundColor.Base);

                var scale = (float)Member.MP / (float)Member.MaxMP;
                var fillSize = new Vector2(Math.Max(1, _config.Size.X * scale), _manaBarConfig.Size.Y);

                DrawHelper.DrawGradientFilledRect(manaBarPos, fillSize, _manaBarConfig.Color, drawList);

                _manaLabelHud.Draw(manaBarPos, _manaBarConfig.Size, character);
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
                    var parentPos = Utils.GetAnchoredPosition(Position, -_config.Size, _roleIconConfig.HealthBarAnchor);
                    var iconPos = Utils.GetAnchoredPosition(parentPos + _roleIconConfig.Position, _roleIconConfig.Size, _roleIconConfig.Anchor);

                    DrawHelper.DrawIcon(iconId, iconPos, _roleIconConfig.Size, false, drawList);
                }
            }

            // leader icon
            if (_leaderIconConfig.Enabled && Member.IsPartyLeader)
            {
                var parentPos = Utils.GetAnchoredPosition(Position, -_config.Size, _leaderIconConfig.HealthBarAnchor);
                var iconPos = Utils.GetAnchoredPosition(parentPos + _leaderIconConfig.Position, _leaderIconConfig.Size, _leaderIconConfig.Anchor);

                DrawHelper.DrawIcon(61521, iconPos, _leaderIconConfig.Size, false, drawList);
            }

            // raise icon
            if (ShowingRaise())
            {
                var parentPos = Utils.GetAnchoredPosition(Position, -_config.Size, _raiseTrackerConfig.HealthBarAnchor);
                var iconPos = Utils.GetAnchoredPosition(parentPos + _raiseTrackerConfig.Position, _raiseTrackerConfig.IconSize, _raiseTrackerConfig.Anchor);
                DrawHelper.DrawIcon(411, iconPos, _raiseTrackerConfig.IconSize, true, drawList);
            }

            // invuln icon
            if (ShowingInvuln())
            {
                var parentPos = Utils.GetAnchoredPosition(Position, -_config.Size, _invulnTrackerConfig.HealthBarAnchor);
                var iconPos = Utils.GetAnchoredPosition(parentPos + _invulnTrackerConfig.Position, _invulnTrackerConfig.IconSize, _invulnTrackerConfig.Anchor);
                DrawHelper.DrawIcon(Member.InvulnStatus!.InvulnIcon, iconPos, _invulnTrackerConfig.IconSize, true, drawList);
            }

            // highlight
            if (_config.ColorsConfig.ShowHighlight && isHovering)
            {
                drawList.AddRectFilled(Position, Position + _config.Size, _config.ColorsConfig.HighlightColor.Base);
            }
        }

        // need to separate elements that have their own window so clipping doesn't get messy
        public void DrawElements(Vector2 origin)
        {
            if (!Visible || Member is null)
            {
                return;
            }

            var player = Plugin.ClientState.LocalPlayer;
            if (player == null)
            {
                return;
            }

            var character = Member.Character;

            // mana
            if (ShowMana())
            {
                var parentPos = Utils.GetAnchoredPosition(Position, -_config.Size, _manaBarConfig.HealthBarAnchor);
                var manaBarPos = Utils.GetAnchoredPosition(parentPos + _manaBarConfig.Position, _manaBarConfig.Size, _manaBarConfig.Anchor);
                _manaLabelHud.Draw(manaBarPos, _manaBarConfig.Size, character);
            }

            // buffs / debuffs
            var buffsPos = Utils.GetAnchoredPosition(Position, -_config.Size, _buffsConfig.HealthBarAnchor);
            _buffsListHud.Actor = character;
            _buffsListHud.Draw(buffsPos);

            var debuffsPos = Utils.GetAnchoredPosition(Position, -_config.Size, _debuffsConfig.HealthBarAnchor);
            _debuffsListHud.Actor = character;
            _debuffsListHud.Draw(debuffsPos);

            // castbar
            var castbarPos = Utils.GetAnchoredPosition(Position, -_config.Size, _castbarConfig.HealthBarAnchor);
            _castbarHud.Actor = character;
            _castbarHud.Draw(castbarPos);

            // name
            var showingRaise = ShowingRaise();
            var showingInvuln = ShowingInvuln();

            var drawName = true;

            if (showingRaise || showingInvuln)
            {
                if ((showingRaise && _raiseTrackerConfig.HideNameWhenRaised) || (showingInvuln && _invulnTrackerConfig.HideNameWhenInvuln))
                {
                    drawName = false;
                }
            }

            if (drawName)
            {
                _nameLabelHud.Draw(Position, _config.Size, character, Member.Name);
            }

            // health label
            _healthLabelHud.Draw(Position, _config.Size, character, Member.HP.ToString());

            // order
            if (character == null || character?.ObjectKind != ObjectKind.BattleNpc)
            {
                var order = Member.ObjectId == player.ObjectId ? 1 : Member.Order;
                _config.OrderLabelConfig.SetText("[" + order + "]");
                _orderLabelHud.Draw(Position, _config.Size);
            }

            // raise label
            if (showingRaise)
            {
                var duration = Math.Abs(Member.RaiseTime!.Value);
                var text = duration < 10 ? duration.ToString("N1", CultureInfo.InvariantCulture) : Utils.DurationToString(duration);
                _raiseTrackerConfig.LabelConfig.SetText(text);
                _raiseLabelHud.Draw(Position, _config.Size);
            }
            // invuln label
            if (showingInvuln)
            {
                var duration = Math.Abs(Member.InvulnStatus!.InvulnTime);
                var text = duration < 10 ? duration.ToString("N1", CultureInfo.InvariantCulture) : Utils.DurationToString(duration);
                _invulnTrackerConfig.LabelConfig.SetText(text);
                _invulnLabelHud.Draw(Position, _config.Size);
            }
        }

        private bool ShowingRaise()
        {
            return Member != null && Member.RaiseTime.HasValue && _raiseTrackerConfig.Enabled &&
                (Member.RaiseTime.Value > 0 || _raiseTrackerConfig.KeepIconAfterCastFinishes);
        }

        private bool ShowingInvuln()
        {
            return Member != null && Member.InvulnStatus != null && _invulnTrackerConfig.Enabled && Member.InvulnStatus.InvulnTime > 0;
        }

        private bool ShowMana()
        {
            if (Member == null)
            {
                return false;
            }

            return (_manaBarConfig.Enabled && Member.MaxHP > 0 &&
                (!_manaBarConfig.ShowOnlyForHealers || JobsHelper.IsJobHealer(Member.JobId)));
        }
    }
}
