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
using System.Collections.Generic;
using DelvUI.Enums;

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
            _statusLabelHud = new LabelHud(PlayerStatus.Label);
            _raiseLabelHud = new LabelHud(RaiseTracker.Icon.Label);
            _invulnLabelHud = new LabelHud(InvulnTracker.Icon.Label);

            _manaBarHud = new PrimaryResourceHud(_configs.ManaBar);
            _castbarHud = new CastbarHud(_configs.CastBar);
            _buffsListHud = new StatusEffectsListHud(_configs.Buffs);
            _debuffsListHud = new StatusEffectsListHud(_configs.Debuffs);
        }

        public PluginConfigColor GetColor(float scale)
        {
            if (Member == null || Member.MaxHP <= 0)
            {
                return _configs.HealthBar.ColorsConfig.OutOfReachBackgroundColor;
            }

            bool cleanseCheck = true;
            if (CleanseTracker.CleanseJobsOnly)
            {
                cleanseCheck = Utils.IsOnCleanseJob();
            }

            if (CleanseTracker.Enabled && CleanseTracker.ChangeHealthBarCleanseColor && Member.HasDispellableDebuff && cleanseCheck)
            {
                return CleanseTracker.HealthBarColor;
            }
            else if (_configs.HealthBar.ColorsConfig.ColorByHealth.Enabled)
            {
                return Utils.GetColorByScale(scale, _configs.HealthBar.ColorsConfig.ColorByHealth);
            }
            else if (Member.JobId > 0)
            {
                return _configs.HealthBar.ColorsConfig.UseRoleColors switch
                {
                    true => GlobalColors.Instance.SafeRoleColorForJobId(Member.JobId),
                    _ => GlobalColors.Instance.SafeColorForJobId(Member.JobId)
                };
            }

            return Member.Character?.ObjectKind switch
            {
                ObjectKind.BattleNpc => GlobalColors.Instance.NPCFriendlyColor,
                _ => _configs.HealthBar.ColorsConfig.OutOfReachBackgroundColor
            };
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
                InputsHelper.Instance.ClearTarget();
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
                InputsHelper.Instance.SetTarget(character);
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
                InputsHelper.Instance.ClearTarget();
                _wasHovering = false;
            }

            // bg
            PluginConfigColor bgColor = _configs.HealthBar.ColorsConfig.BackgroundColor;
            if (Member.RaiseTime != null && RaiseTracker.Enabled && RaiseTracker.ChangeBackgroundColorWhenRaised)
            {
                bgColor = RaiseTracker.BackgroundColor;
            }
            else if (Member.InvulnStatus?.InvulnTime != null && InvulnTracker.Enabled && InvulnTracker.ChangeBackgroundColorWhenInvuln)
            {
                bgColor = Member.InvulnStatus?.InvulnId == 811 ? InvulnTracker.WalkingDeadBackgroundColor : InvulnTracker.BackgroundColor;
            }
            else if (_configs.HealthBar.ColorsConfig.UseDeathIndicatorBackgroundColor && Member.HP <= 0 && character != null)
            {
                bgColor = _configs.HealthBar.RangeConfig.Enabled
                    ? GetDistanceColor(character, _configs.HealthBar.ColorsConfig.DeathIndicatorBackgroundColor)
                    : _configs.HealthBar.ColorsConfig.DeathIndicatorBackgroundColor;
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
                hpColor = GetDistanceColor(character, hpColor);
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
            Vector2 borderPos = Position - Vector2.One;
            Vector2 borderSize = _configs.HealthBar.Size + Vector2.One * 2;
            uint color = borderColor?.Base ?? _configs.HealthBar.ColorsConfig.BorderColor.Base;
            int thickness = borderColor != null ? _configs.HealthBar.ColorsConfig.ActiveBorderThickness : _configs.HealthBar.ColorsConfig.InactiveBorderThickness;
            drawList.AddRect(borderPos, borderPos + borderSize, color, 0, ImDrawFlags.None, thickness);

            // highlight
            if (_configs.HealthBar.ColorsConfig.ShowHighlight && isHovering)
            {
                drawList.AddRectFilled(Position, Position + _configs.HealthBar.Size, _configs.HealthBar.ColorsConfig.HighlightColor.Base);
            }
        }

        private PluginConfigColor GetDistanceColor(Character? character, PluginConfigColor color)
        {
            byte distance = character != null ? character.YalmDistanceX : byte.MaxValue;
            float currentAlpha = color.Vector.W * 100f;
            float alpha = _configs.HealthBar.RangeConfig.AlphaForDistance(distance, currentAlpha) / 100f;

            return new PluginConfigColor(color.Vector.WithNewAlpha(alpha));
        }

        // need to separate elements that have their own window so clipping doesn't get messy
        public List<(StrataLevel, Action)> GetElementsDrawActions(Vector2 origin)
        {
            List<(StrataLevel, Action)> drawActions = new List<(StrataLevel, Action)>();

            var player = Plugin.ClientState.LocalPlayer;
            if (!Visible || Member is null || player == null)
            {
                StopMouseover();
                return drawActions;
            }

            var character = Member.Character;

            // role/job icon
            if (RoleIcon.Enabled)
            {
                uint iconId = 0;

                // chocobo icon
                if (character is BattleNpc battleNpc && battleNpc.BattleNpcKind == BattleNpcSubKind.Chocobo)
                {
                    iconId = JobsHelper.RoleIconIDForBattleCompanion + (uint)RoleIcon.Style * 100;
                }
                // role/job icon
                else if (Member.JobId > 0)
                {
                    iconId = RoleIcon.UseRoleIcons ?
                        JobsHelper.RoleIconIDForJob(Member.JobId, RoleIcon.UseSpecificDPSRoleIcons) :
                        JobsHelper.IconIDForJob(Member.JobId) + (uint)RoleIcon.Style * 100;
                }

                if (iconId > 0)
                {
                    var parentPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, RoleIcon.FrameAnchor);
                    var iconPos = Utils.GetAnchoredPosition(parentPos + RoleIcon.Position, RoleIcon.Size, RoleIcon.Anchor);

                    drawActions.Add((RoleIcon.StrataLevel, () =>
                    {
                        DrawHelper.DrawInWindow(RoleIcon.ID, iconPos, RoleIcon.Size, false, false, (drawList) =>
                        {
                            DrawHelper.DrawIcon(iconId, iconPos, RoleIcon.Size, false, drawList);
                        });
                    }
                    ));
                }
            }

            // leader icon
            if (LeaderIcon.Enabled && Member.IsPartyLeader)
            {
                var parentPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, LeaderIcon.FrameAnchor);
                var iconPos = Utils.GetAnchoredPosition(parentPos + LeaderIcon.Position, LeaderIcon.Size, LeaderIcon.Anchor);

                drawActions.Add((LeaderIcon.StrataLevel, () =>
                {
                    DrawHelper.DrawInWindow(LeaderIcon.ID, iconPos, LeaderIcon.Size, false, false, (drawList) =>
                    {
                        DrawHelper.DrawIcon(61521, iconPos, LeaderIcon.Size, false, drawList);
                    });
                }
                ));
            }

            // player status icon
            if (PlayerStatus.Enabled && PlayerStatus.Icon.Enabled)
            {
                uint? iconId = IconIdForStatus(Member.Status);
                if (iconId.HasValue)
                {
                    var parentPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, PlayerStatus.Icon.FrameAnchor);
                    var iconPos = Utils.GetAnchoredPosition(parentPos + PlayerStatus.Icon.Position, PlayerStatus.Icon.Size, PlayerStatus.Icon.Anchor);

                    drawActions.Add((PlayerStatus.Icon.StrataLevel, () =>
                    {
                        DrawHelper.DrawInWindow(PlayerStatus.Icon.ID, iconPos, PlayerStatus.Icon.Size, false, false, (drawList) =>
                        {
                            DrawHelper.DrawIcon(iconId.Value, iconPos, PlayerStatus.Icon.Size, false, drawList);
                        });
                    }
                    ));
                }
            }

            // raise icon
            if (ShowingRaise())
            {
                var parentPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, RaiseTracker.Icon.FrameAnchor);
                var iconPos = Utils.GetAnchoredPosition(parentPos + RaiseTracker.Icon.Position, RaiseTracker.Icon.Size, RaiseTracker.Icon.Anchor);

                drawActions.Add((RaiseTracker.Icon.StrataLevel, () =>
                {
                    DrawHelper.DrawInWindow(RaiseTracker.Icon.ID, iconPos, RaiseTracker.Icon.Size, false, false, (drawList) =>
                    {
                        DrawHelper.DrawIcon(411, iconPos, RaiseTracker.Icon.Size, true, drawList);
                    });
                }
                ));
            }

            // invuln icon
            if (ShowingInvuln())
            {
                var parentPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, InvulnTracker.Icon.FrameAnchor);
                var iconPos = Utils.GetAnchoredPosition(parentPos + InvulnTracker.Icon.Position, InvulnTracker.Icon.Size, InvulnTracker.Icon.Anchor);

                drawActions.Add((InvulnTracker.Icon.StrataLevel, () =>
                {
                    DrawHelper.DrawInWindow(InvulnTracker.Icon.ID, iconPos, InvulnTracker.Icon.Size, false, false, (drawList) =>
                    {
                        DrawHelper.DrawIcon(Member.InvulnStatus!.InvulnIcon, iconPos, InvulnTracker.Icon.Size, true, drawList);
                    });
                }
                ));
            }

            // mana
            if (ShowMana())
            {
                var parentPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, _configs.ManaBar.HealthBarAnchor);
                drawActions.Add((_configs.ManaBar.StrataLevel, () =>
                {
                    _manaBarHud.Actor = character;
                    _manaBarHud.PartyMember = Member;
                    _manaBarHud.Draw(parentPos);
                }
                ));
            }

            // buffs / debuffs
            var buffsPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, _configs.Buffs.HealthBarAnchor);
            drawActions.Add((_configs.Buffs.StrataLevel, () =>
            {
                _buffsListHud.Actor = character;
                _buffsListHud.Draw(buffsPos);
            }
            ));

            var debuffsPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, _configs.Debuffs.HealthBarAnchor);
            drawActions.Add((_configs.Debuffs.StrataLevel, () =>
            {
                _debuffsListHud.Actor = character;
                _debuffsListHud.Draw(debuffsPos);
            }
            ));

            // castbar
            var castbarPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, _configs.CastBar.HealthBarAnchor);
            drawActions.Add((_configs.CastBar.StrataLevel, () =>
            {
                _castbarHud.Actor = character;
                _castbarHud.Draw(castbarPos);
            }
            ));

            // name
            var showingRaise = ShowingRaise();
            var showingInvuln = ShowingInvuln();

            var drawName = true;

            if (showingRaise || showingInvuln)
            {
                if ((showingRaise && RaiseTracker.HideNameWhenRaised) || (showingInvuln && InvulnTracker.HideNameWhenInvuln))
                {
                    drawName = false;
                }
            }
            else if (PlayerStatus.Enabled && PlayerStatus.HideName && Member.Status != PartyMemberStatus.None)
            {
                drawName = false;
            }

            if (drawName)
            {
                drawActions.Add((_configs.HealthBar.NameLabelConfig.StrataLevel, () =>
                {
                    _nameLabelHud.Draw(Position, _configs.HealthBar.Size, character, Member.Name);
                }
                ));
            }

            // health label
            if (character != null)
            {
                drawActions.Add((_configs.HealthBar.HealthLabelConfig.StrataLevel, () =>
                {
                    _healthLabelHud.Draw(Position, _configs.HealthBar.Size, character, null, Member.HP, Member.MaxHP);
                }
                ));
            }

            // order
            if (character == null || character?.ObjectKind != ObjectKind.BattleNpc)
            {
                var order = Member.ObjectId == player.ObjectId ? 1 : Member.Order;

                drawActions.Add((_configs.HealthBar.OrderLabelConfig.StrataLevel, () =>
                {
                    _configs.HealthBar.OrderLabelConfig.SetText("[" + order + "]");
                    _orderLabelHud.Draw(Position, _configs.HealthBar.Size);
                }
                ));
            }

            // status
            string? statusString = StringForStatus(Member.Status);
            if (PlayerStatus.Enabled && PlayerStatus.Label.Enabled && statusString != null)
            {
                drawActions.Add((PlayerStatus.Label.StrataLevel, () =>
                {
                    PlayerStatus.Label.SetText(statusString);
                    _statusLabelHud.Draw(Position, _configs.HealthBar.Size);
                }
                ));
            }

            // raise label
            if (showingRaise)
            {
                var duration = Math.Abs(Member.RaiseTime!.Value);
                var text = duration < 10 ? duration.ToString("N1", CultureInfo.InvariantCulture) : Utils.DurationToString(duration);

                drawActions.Add((RaiseTracker.Icon.Label.StrataLevel, () =>
                {
                    RaiseTracker.Icon.Label.SetText(text);
                    _raiseLabelHud.Draw(Position, _configs.HealthBar.Size);
                }
                ));
            }

            // invuln label
            if (showingInvuln)
            {
                var duration = Math.Abs(Member.InvulnStatus!.InvulnTime);
                var text = duration < 10 ? duration.ToString("N1", CultureInfo.InvariantCulture) : Utils.DurationToString(duration);

                drawActions.Add((InvulnTracker.Icon.Label.StrataLevel, () =>
                {
                    InvulnTracker.Icon.Label.SetText(text);
                    _invulnLabelHud.Draw(Position, _configs.HealthBar.Size);
                }
                ));
            }

            return drawActions;
        }

        private bool ShowingRaise() =>
            Member != null && Member.RaiseTime.HasValue && RaiseTracker.Enabled &&
            (Member.RaiseTime.Value > 0 || RaiseTracker.KeepIconAfterCastFinishes);

        private bool ShowingInvuln() => Member != null && Member.InvulnStatus != null && InvulnTracker.Enabled && Member.InvulnStatus.InvulnTime > 0;

        private bool ShowMana()
        {
            if (Member == null)
            {
                return false;
            }

            var isHealer = JobsHelper.IsJobHealer(Member.JobId);

            return _configs.ManaBar.Enabled && Member.MaxHP > 0 && _configs.ManaBar.ManaBarDisplayMode switch
            {
                PartyFramesManaBarDisplayMode.Always => true,
                PartyFramesManaBarDisplayMode.HealersOnly => isHealer,
                PartyFramesManaBarDisplayMode.HealersAndRaiseJobs => isHealer || JobsHelper.IsJobWithRaise(Member.JobId, Member.Level),
                _ => true
            };
        }

        private static uint? IconIdForStatus(PartyMemberStatus status)
        {
            return status switch
            {
                PartyMemberStatus.ViewingCutscene => 61508,
                PartyMemberStatus.Offline => 61504,
                PartyMemberStatus.Dead => 61502,
                _ => null
            };
        }

        private static string? StringForStatus(PartyMemberStatus status)
        {
            return status switch
            {
                PartyMemberStatus.ViewingCutscene => "[Viewing Cutscene]",
                PartyMemberStatus.Offline => "[Offline]",
                PartyMemberStatus.Dead => "[Dead]",
                _ => null
            };
        }

        #region convenience
        private PartyFramesRoleIconConfig RoleIcon => _configs.Icons.Role;
        private PartyFramesLeaderIconConfig LeaderIcon => _configs.Icons.Leader;
        private PartyFramesPlayerStatusConfig PlayerStatus => _configs.Icons.PlayerStatus;
        private PartyFramesRaiseTrackerConfig RaiseTracker => _configs.Trackers.Raise;
        private PartyFramesInvulnTrackerConfig InvulnTracker => _configs.Trackers.Invuln;
        private PartyFramesCleanseTrackerConfig CleanseTracker => _configs.Trackers.Cleanse;
        #endregion
    }
}
