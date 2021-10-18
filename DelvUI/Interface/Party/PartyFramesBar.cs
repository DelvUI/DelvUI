using Dalamud.Game.ClientState.Objects.Enums;
using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using DelvUI.Interface.StatusEffects;
using ImGuiNET;
using System;
using System.Globalization;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;

namespace DelvUI.Interface.Party
{
    public class PartyFramesBar
    {
        public delegate void PartyFramesBarEventHandler(PartyFramesBar bar);
        public PartyFramesBarEventHandler? MovePlayerEvent;
        public PartyFramesBarEventHandler? OpenContextMenuEvent;

        private PartyFramesConfigs _configs;

        private LabelHud _nameLabelHud;
        private LabelHud _healthLabelHud;
        private LabelHud _orderLabelHud;
        private LabelHud _statusLabelHud;
        private LabelHud _raiseLabelHud;
        private LabelHud _invulnLabelHud;
        private PrimaryResourceHud _manaBarHud;
        private CastbarHud _castbarHud;
        private StatusEffectsListHud _buffsListHud;
        private StatusEffectsListHud _debuffsListHud;

        public bool Visible = false;
        public Vector2 Position;

        private SmoothHPHelper _smoothHPHelper = new SmoothHPHelper();

        private bool _wasHovering = false;

        public IPartyFramesMember? Member;

        public PartyFramesBar(string id, PartyFramesConfigs configs)
        {
            _configs = configs;

            _nameLabelHud = new LabelHud(_configs.HealthBar.NameLabelConfig);
            _healthLabelHud = new LabelHud(_configs.HealthBar.HealthLabelConfig);
            _orderLabelHud = new LabelHud(_configs.HealthBar.OrderLabelConfig);
            _statusLabelHud = new LabelHud(_configs.PlayerStatus.LabelConfig);
            _raiseLabelHud = new LabelHud(_configs.RaiseTracker.LabelConfig);
            _invulnLabelHud = new LabelHud(_configs.InvulnTracker.LabelConfig);

            _manaBarHud = new PrimaryResourceHud(_configs.ManaBar, "");
            _castbarHud = new CastbarHud(_configs.CastBar, "");
            _buffsListHud = new StatusEffectsListHud(_configs.Buffs, "");
            _debuffsListHud = new StatusEffectsListHud(_configs.Debuffs, "");
        }

        public PluginConfigColor GetColor(float scale)
        {
            if (Member == null || Member.MaxHP <= 0)
            {
                return _configs.HealthBar.ColorsConfig.OutOfReachBackgroundColor;
            }

            if (Member.Character?.ObjectKind == ObjectKind.BattleNpc)
            {
                return GlobalColors.Instance.NPCFriendlyColor;
            }

            bool cleanseCheck = true;
            if (_configs.CleanseTracker.CleanseJobsOnly)
            {
                cleanseCheck = Utils.IsOnCleanseJob();
            }

            if (_configs.CleanseTracker.Enabled && _configs.CleanseTracker.ChangeHealthBarCleanseColor && Member.HasDispellableDebuff && cleanseCheck)
            {
                return _configs.CleanseTracker.HealthBarColor;
            }
            else if (_configs.HealthBar.ColorsConfig.UseColorBasedOnHealthValue)
            {
                return Utils.GetColorByScale(scale, _configs.HealthBar.ColorsConfig.LowHealthColorThreshold / 100f, _configs.HealthBar.ColorsConfig.FullHealthColorThreshold / 100f, _configs.HealthBar.ColorsConfig.LowHealthColor, _configs.HealthBar.ColorsConfig.FullHealthColor, _configs.HealthBar.ColorsConfig.blendMode);
            }
            else if (Member.JobId > 0)
            {
                if (_configs.HealthBar.ColorsConfig.UseRoleColors)
                {
                    return ColorForJob(Member.JobId);
                }

                else
                {
                    return GlobalColors.Instance.SafeColorForJobId(Member.JobId);
                }
            }

            return _configs.HealthBar.ColorsConfig.OutOfReachBackgroundColor;
        }

        private PluginConfigColor ColorForJob(uint jodId)
        {
            var role = JobsHelper.RoleForJob(jodId);

            switch (role)
            {
                case JobRoles.Tank: return _configs.HealthBar.ColorsConfig.TankRoleColor;
                case JobRoles.Healer: return _configs.HealthBar.ColorsConfig.HealerRoleColor;

                case JobRoles.DPSMelee:
                case JobRoles.DPSRanged:
                case JobRoles.DPSCaster:
                    return _configs.HealthBar.ColorsConfig.DPSRoleColor;
            }

            return _configs.HealthBar.ColorsConfig.GenericRoleColor;
        }

        public void StopPreview()
        {
            _castbarHud.StopPreview();
            _buffsListHud.StopPreview();
            _debuffsListHud.StopPreview();
        }

        public void StopMouseover()
        {
            if (_wasHovering)
            {
                InputsHelper.Instance.Target = null;
                _wasHovering = false;
            }
        }

        public void Draw(Vector2 origin, ImDrawListPtr drawList, PluginConfigColor? borderColor = null)
        {
            if (!Visible || Member is null)
            {
                StopMouseover();
                return;
            }

            // click
            bool isHovering = ImGui.IsMouseHoveringRect(Position, Position + _configs.HealthBar.Size);
            Character? character = Member.Character;

            if (isHovering)
            {
                InputsHelper.Instance.Target = character;
                _wasHovering = true;

                // left click
                if (InputsHelper.Instance.LeftButtonClicked)
                {
                    // move player bar to this spot on ctrl+alt+shift click
                    if (ImGui.GetIO().KeyCtrl && ImGui.GetIO().KeyAlt && ImGui.GetIO().KeyShift)
                    {
                        MovePlayerEvent?.Invoke(this);
                    }
                    // target
                    else if (character != null)
                    {
                        Plugin.TargetManager.SetTarget(character);
                    }
                }
                // right click (context menu)
                else if (InputsHelper.Instance.RightButtonClicked)
                {
                    OpenContextMenuEvent?.Invoke(this);
                }
            }
            else if (_wasHovering)
            {
                InputsHelper.Instance.Target = null;
                _wasHovering = false;
            }

            // bg
            PluginConfigColor bgColor;
            if (Member.RaiseTime != null && _configs.RaiseTracker.Enabled && _configs.RaiseTracker.ChangeBackgroundColorWhenRaised)
            {
                bgColor = _configs.RaiseTracker.BackgroundColor;
            }
            else if (Member.InvulnStatus?.InvulnTime != null && _configs.InvulnTracker.Enabled && _configs.InvulnTracker.ChangeBackgroundColorWhenInvuln)
            {
                bgColor = Member.InvulnStatus?.InvulnId == 811 ? _configs.InvulnTracker.WalkingDeadBackgroundColor : _configs.InvulnTracker.BackgroundColor;
            }
            else if (_configs.HealthBar.ColorsConfig.UseDeathIndicatorBackgroundColor && Member.HP <= 0)
            {
                bgColor = _configs.HealthBar.RangeConfig.Enabled
                    ? GetDistance(character, _configs.HealthBar.ColorsConfig.DeathIndicatorBackgroundColor)
                    : _configs.HealthBar.ColorsConfig.DeathIndicatorBackgroundColor;
            }
            else
            {
                bgColor = _configs.HealthBar.ColorsConfig.BackgroundColor;
            }

            drawList.AddRectFilled(Position, Position + _configs.HealthBar.Size, bgColor.Base);

            // hp
            uint currentHp = Member.HP;
            uint maxHp = Member.MaxHP;

            if (_configs.HealthBar.SmoothHealthConfig.Enabled)
            {
                currentHp = _smoothHPHelper.GetNextHp((int)currentHp, (int)maxHp, _configs.HealthBar.SmoothHealthConfig.Velocity);
            }

            var hpScale = maxHp > 0 ? (float)currentHp / (float)maxHp : 1;
            var hpFillSize = new Vector2(_configs.HealthBar.Size.X * hpScale, _configs.HealthBar.Size.Y);
            PluginConfigColor? hpColor = GetColor(hpScale);

            if (_configs.HealthBar.RangeConfig.Enabled)
            {
                hpColor = GetDistance(character, hpColor);
            }

            DrawHelper.DrawGradientFilledRect(Position, hpFillSize, hpColor, drawList);

            // shield
            if (_configs.HealthBar.ShieldConfig.Enabled)
            {
                if (_configs.HealthBar.ShieldConfig.FillHealthFirst && Member.MaxHP > 0)
                {
                    DrawHelper.DrawShield(Member.Shield, (float)currentHp / maxHp, Position, _configs.HealthBar.Size,
                        _configs.HealthBar.ShieldConfig.Height, !_configs.HealthBar.ShieldConfig.HeightInPixels,
                        _configs.HealthBar.ShieldConfig.Color, drawList);
                }
                else
                {
                    DrawHelper.DrawOvershield(Member.Shield, Position, _configs.HealthBar.Size,
                        _configs.HealthBar.ShieldConfig.Height, !_configs.HealthBar.ShieldConfig.HeightInPixels,
                        _configs.HealthBar.ShieldConfig.Color, drawList);
                }
            }

            // border
            var borderPos = Position - Vector2.One;
            var borderSize = _configs.HealthBar.Size + Vector2.One * 2;
            var color = borderColor?.Base ?? _configs.HealthBar.ColorsConfig.BorderColor.Base;
            drawList.AddRect(borderPos, borderPos + borderSize, color);

            // role/job icon
            if (_configs.RoleIcon.Enabled && Member.JobId > 0)
            {
                uint iconId;

                // chocobo icon
                if (character != null && character.ObjectKind == ObjectKind.BattleNpc)
                {
                    iconId = JobsHelper.RoleIconIDForBattleCompanion + (uint)_configs.RoleIcon.Style * 100;
                }
                // role/job icon
                else
                {
                    iconId = _configs.RoleIcon.UseRoleIcons ?
                        JobsHelper.RoleIconIDForJob(Member.JobId, _configs.RoleIcon.UseSpecificDPSRoleIcons) :
                        JobsHelper.IconIDForJob(Member.JobId) + (uint)_configs.RoleIcon.Style * 100;
                }

                if (iconId > 0)
                {
                    var parentPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, _configs.RoleIcon.HealthBarAnchor);
                    var iconPos = Utils.GetAnchoredPosition(parentPos + _configs.RoleIcon.Position, _configs.RoleIcon.Size, _configs.RoleIcon.Anchor);

                    DrawHelper.DrawIcon(iconId, iconPos, _configs.RoleIcon.Size, false, drawList);
                }
            }

            // leader icon
            if (_configs.LeaderIcon.Enabled && Member.IsPartyLeader)
            {
                var parentPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, _configs.LeaderIcon.HealthBarAnchor);
                var iconPos = Utils.GetAnchoredPosition(parentPos + _configs.LeaderIcon.Position, _configs.LeaderIcon.Size, _configs.LeaderIcon.Anchor);

                DrawHelper.DrawIcon(61521, iconPos, _configs.LeaderIcon.Size, false, drawList);
            }

            // player status icon
            if (_configs.PlayerStatus.Enabled && _configs.PlayerStatus.ShowIcon)
            {
                uint? iconId = IconIdForStatus(Member.Status);
                if (iconId.HasValue)
                {
                    var parentPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, _configs.PlayerStatus.IconFrameAnchor);
                    var iconPos = Utils.GetAnchoredPosition(parentPos + _configs.PlayerStatus.IconPosition, _configs.PlayerStatus.IconSize, _configs.PlayerStatus.IconAnchor);

                    DrawHelper.DrawIcon(iconId.Value, iconPos, _configs.PlayerStatus.IconSize, false, drawList);
                }
            }

            // raise icon
            if (ShowingRaise())
            {
                var parentPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, _configs.RaiseTracker.HealthBarAnchor);
                var iconPos = Utils.GetAnchoredPosition(parentPos + _configs.RaiseTracker.Position, _configs.RaiseTracker.IconSize, _configs.RaiseTracker.Anchor);
                DrawHelper.DrawIcon(411, iconPos, _configs.RaiseTracker.IconSize, true, drawList);
            }

            // invuln icon
            if (ShowingInvuln())
            {
                var parentPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, _configs.InvulnTracker.HealthBarAnchor);
                var iconPos = Utils.GetAnchoredPosition(parentPos + _configs.InvulnTracker.Position, _configs.InvulnTracker.IconSize, _configs.InvulnTracker.Anchor);
                DrawHelper.DrawIcon(Member.InvulnStatus!.InvulnIcon, iconPos, _configs.InvulnTracker.IconSize, true, drawList);
            }

            // highlight
            if (_configs.HealthBar.ColorsConfig.ShowHighlight && isHovering)
            {
                drawList.AddRectFilled(Position, Position + _configs.HealthBar.Size, _configs.HealthBar.ColorsConfig.HighlightColor.Base);
            }
        }

        private PluginConfigColor GetDistance(Character? character, PluginConfigColor color)
        {
            byte distance = character != null ? character.YalmDistanceX : byte.MaxValue;
            float currentAlpha = color.Vector.W * 100f;
            float alpha = _configs.HealthBar.RangeConfig.AlphaForDistance(distance, currentAlpha) / 100f;

            return new PluginConfigColor(color.Vector.WithNewAlpha(alpha));
        }

        // need to separate elements that have their own window so clipping doesn't get messy
        public void DrawElements(Vector2 origin)
        {
            var player = Plugin.ClientState.LocalPlayer;
            if (!Visible || Member is null || player == null)
            {
                StopMouseover();
                return;
            }

            var character = Member.Character;

            // mana
            if (ShowMana())
            {
                var parentPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, _configs.ManaBar.HealthBarAnchor);
                _manaBarHud.Actor = character;
                _manaBarHud.PartyMember = Member;
                _manaBarHud.Draw(parentPos);
            }

            // buffs / debuffs
            var buffsPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, _configs.Buffs.HealthBarAnchor);
            _buffsListHud.Actor = character;
            _buffsListHud.Draw(buffsPos);

            var debuffsPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, _configs.Debuffs.HealthBarAnchor);
            _debuffsListHud.Actor = character;
            _debuffsListHud.Draw(debuffsPos);

            // castbar
            var castbarPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, _configs.CastBar.HealthBarAnchor);
            _castbarHud.Actor = character;
            _castbarHud.Draw(castbarPos);

            // name
            var showingRaise = ShowingRaise();
            var showingInvuln = ShowingInvuln();

            var drawName = true;

            if (showingRaise || showingInvuln)
            {
                if ((showingRaise && _configs.RaiseTracker.HideNameWhenRaised) || (showingInvuln && _configs.InvulnTracker.HideNameWhenInvuln))
                {
                    drawName = false;
                }
            }
            else if (_configs.PlayerStatus.Enabled && _configs.PlayerStatus.HideName && Member.Status != PartyMemberStatus.None)
            {
                drawName = false;
            }

            if (drawName)
            {
                _nameLabelHud.Draw(Position, _configs.HealthBar.Size, character, Member.Name);
            }

            // health label
            if (character != null)
            {
                _healthLabelHud.Draw(Position, _configs.HealthBar.Size, character, null, Member.HP, Member.MaxHP);
            }

            // order
            if (character == null || character?.ObjectKind != ObjectKind.BattleNpc)
            {
                var order = Member.ObjectId == player.ObjectId ? 1 : Member.Order;
                _configs.HealthBar.OrderLabelConfig.SetText("[" + order + "]");
                _orderLabelHud.Draw(Position, _configs.HealthBar.Size);
            }

            // status
            string? statusString = StringForStatus(Member.Status);
            if (_configs.PlayerStatus.Enabled && _configs.PlayerStatus.LabelConfig.Enabled && statusString != null)
            {
                _configs.PlayerStatus.LabelConfig.SetText(statusString);
                _statusLabelHud.Draw(Position, _configs.HealthBar.Size);
            }

            // raise label
            if (showingRaise)
            {
                var duration = Math.Abs(Member.RaiseTime!.Value);
                var text = duration < 10 ? duration.ToString("N1", CultureInfo.InvariantCulture) : Utils.DurationToString(duration);
                _configs.RaiseTracker.LabelConfig.SetText(text);
                _raiseLabelHud.Draw(Position, _configs.HealthBar.Size);
            }
            // invuln label
            if (showingInvuln)
            {
                var duration = Math.Abs(Member.InvulnStatus!.InvulnTime);
                var text = duration < 10 ? duration.ToString("N1", CultureInfo.InvariantCulture) : Utils.DurationToString(duration);
                _configs.InvulnTracker.LabelConfig.SetText(text);
                _invulnLabelHud.Draw(Position, _configs.HealthBar.Size);
            }
        }

        private bool ShowingRaise()
        {
            return Member != null && Member.RaiseTime.HasValue && _configs.RaiseTracker.Enabled &&
                (Member.RaiseTime.Value > 0 || _configs.RaiseTracker.KeepIconAfterCastFinishes);
        }

        private bool ShowingInvuln()
        {
            return Member != null && Member.InvulnStatus != null && _configs.InvulnTracker.Enabled && Member.InvulnStatus.InvulnTime > 0;
        }

        private bool ShowMana()
        {
            if (Member == null)
            {
                return false;
            }

            return (_configs.ManaBar.Enabled && Member.MaxHP > 0 &&
                (!_configs.ManaBar.ShowOnlyForHealers || JobsHelper.IsJobHealer(Member.JobId)));
        }

        private static uint? IconIdForStatus(PartyMemberStatus status)
        {
            switch (status)
            {
                case PartyMemberStatus.ViewingCutscene: return 61508;
            }

            return null;
        }

        private static string? StringForStatus(PartyMemberStatus status)
        {
            switch (status)
            {
                case PartyMemberStatus.ViewingCutscene: return "[Viewing Cutscene]";
            }

            return null;
        }
    }
}
