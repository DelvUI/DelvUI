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
using DelvUI.Interface.Bars;

namespace DelvUI.Interface.Party
{
    public class PartyFramesBar
    {
        public delegate void PartyFramesBarEventHandler(PartyFramesBar bar);
        public PartyFramesBarEventHandler? MovePlayerEvent;
        public PartyFramesBarEventHandler? OpenContextMenuEvent;

        private readonly PartyFramesConfigs _configs;

        private readonly LabelHud _nameLabelHud;
        private readonly LabelHud _healthLabelHud;
        private readonly LabelHud _orderLabelHud;
        private readonly LabelHud _statusLabelHud;
        private readonly LabelHud _raiseLabelHud;
        private readonly LabelHud _invulnLabelHud;
        private readonly PrimaryResourceHud _manaBarHud;
        private readonly CastbarHud _castbarHud;
        private readonly StatusEffectsListHud _buffsListHud;
        private readonly StatusEffectsListHud _debuffsListHud;

        public bool Visible = false;
        public Vector2 Position;

        private readonly SmoothHPHelper _smoothHPHelper = new();

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
            if (Member is not { MaxHP: > 0 })
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

        public void Draw(Vector2 origin, PluginConfigColor? borderColor = null)
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

            // hp
            uint currentHp = Member.HP;
            uint maxHp = Member.MaxHP;

            if (_configs.HealthBar.SmoothHealthConfig.Enabled)
            {
                currentHp = _smoothHPHelper.GetNextHp((int)currentHp, (int)maxHp, _configs.HealthBar.SmoothHealthConfig.Velocity);
            }

            float hpScale = maxHp > 0 ? (float)currentHp / (float)maxHp : 1;
            Vector2 hpFillSize = new(_configs.HealthBar.Size.X * hpScale, _configs.HealthBar.Size.Y);
            Rect background = new(Position, Position + _configs.HealthBar.Size, bgColor);
            PluginConfigColor? hpColor = GetColor(hpScale);

            if (_configs.HealthBar.RangeConfig.Enabled)
            {
                hpColor = GetDistanceColor(character, hpColor);
            }
            
            Rect healthFill = BarUtilities.GetFillRect(Position, hpFillSize, _configs.HealthBar.FillDirection, hpColor, currentHp, maxHp);
            PluginConfigColor color = borderColor ?? GetBorderColor(character);

            BarHud bar = new(_configs.HealthBar.ID,
                _configs.HealthBar.DrawBorder,
                color,
                _configs.HealthBar.BorderThickness,
                actor: character,
                current: currentHp,
                max: maxHp);

            bar.SetBackground(background);
            bar.AddForegrounds(healthFill);

            if (_configs.HealthBar.ColorsConfig.UseMissingHealthBar)
            {
                Vector2 healthMissingSize = _configs.HealthBar.Size - BarUtilities.GetFillDirectionOffset(healthFill.Size, _configs.HealthBar.FillDirection);
                Vector2 healthMissingPos = _configs.HealthBar.FillDirection.IsInverted() ? Position : Position + BarUtilities.GetFillDirectionOffset(healthFill.Size, _configs.HealthBar.FillDirection);
                PluginConfigColor? missingHealthColor = _configs.HealthBar.ColorsConfig.HealthMissingColor;
                bar.AddForegrounds(new Rect(healthMissingPos, healthMissingSize, missingHealthColor));
            }

            // shield
            if (_configs.HealthBar.ShieldConfig.Enabled)
            {
                if (Member.Shield > 0f)
                {
                    bar.AddForegrounds(
                        BarUtilities.GetShieldForeground(
                            _configs.HealthBar.ShieldConfig,
                            Position,
                            _configs.HealthBar.Size,
                            healthFill.Size,
                            _configs.HealthBar.FillDirection,
                            Member.Shield,
                            currentHp,
                            maxHp
                        )
                    );
                }
            }

            // highlight
            if (_configs.HealthBar.ColorsConfig.ShowHighlight && isHovering)
            {
                Rect highlight = new(Position, Position + _configs.HealthBar.Size, _configs.HealthBar.ColorsConfig.HighlightColor);
                bar.AddForegrounds(highlight);
            }
        }

        private PluginConfigColor GetBorderColor(Character? character)
        {
            GameObject? target = Plugin.TargetManager.Target ?? Plugin.TargetManager.SoftTarget;

            return character != null && character == target ? _configs.HealthBar.ColorsConfig.TargetBordercolor : _configs.HealthBar.BorderColor;
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
                if (character is BattleNpc { BattleNpcKind: BattleNpcSubKind.Chocobo })
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
                    Vector2 parentPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, RoleIcon.FrameAnchor);
                    Vector2 iconPos = Utils.GetAnchoredPosition(parentPos + RoleIcon.Position, RoleIcon.Size, RoleIcon.Anchor);

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
                Vector2 parentPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, LeaderIcon.FrameAnchor);
                Vector2 iconPos = Utils.GetAnchoredPosition(parentPos + LeaderIcon.Position, LeaderIcon.Size, LeaderIcon.Anchor);

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
                    Vector2 parentPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, PlayerStatus.Icon.FrameAnchor);
                    Vector2 iconPos = Utils.GetAnchoredPosition(parentPos + PlayerStatus.Icon.Position, PlayerStatus.Icon.Size, PlayerStatus.Icon.Anchor);

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
                Vector2 parentPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, RaiseTracker.Icon.FrameAnchor);
                Vector2 iconPos = Utils.GetAnchoredPosition(parentPos + RaiseTracker.Icon.Position, RaiseTracker.Icon.Size, RaiseTracker.Icon.Anchor);

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
                Vector2 parentPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, InvulnTracker.Icon.FrameAnchor);
                Vector2 iconPos = Utils.GetAnchoredPosition(parentPos + InvulnTracker.Icon.Position, InvulnTracker.Icon.Size, InvulnTracker.Icon.Anchor);

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
                Vector2 parentPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, _configs.ManaBar.HealthBarAnchor);
                _manaBarHud.Actor = character;
                _manaBarHud.PartyMember = Member;

                drawActions.Add((_configs.ManaBar.StrataLevel, () =>
                {
                    _manaBarHud.Draw(parentPos);
                }
                ));
            }

            // buffs / debuffs
            Vector2 buffsPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, _configs.Buffs.HealthBarAnchor);
            _buffsListHud.Actor = character;
            drawActions.Add((_configs.Buffs.StrataLevel, () =>
            {
                _buffsListHud.Draw(buffsPos);
            }
            ));

            Vector2 debuffsPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, _configs.Debuffs.HealthBarAnchor);
            _debuffsListHud.Actor = character;
            drawActions.Add((_configs.Debuffs.StrataLevel, () =>
            {
                _debuffsListHud.Draw(debuffsPos);
            }
            ));

            // castbar
            Vector2 castbarPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, _configs.CastBar.HealthBarAnchor);
            _castbarHud.Actor = character;
            drawActions.Add((_configs.CastBar.StrataLevel, () =>
            {
                _castbarHud.Draw(castbarPos);
            }
            ));

            // name
            bool showingRaise = ShowingRaise();
            bool showingInvuln = ShowingInvuln();

            bool drawName = true;

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
                int order = Member.ObjectId == player.ObjectId ? 1 : Member.Order;
                _configs.HealthBar.OrderLabelConfig.SetText("[" + order + "]");

                drawActions.Add((_configs.HealthBar.OrderLabelConfig.StrataLevel, () =>
                {
                    _orderLabelHud.Draw(Position, _configs.HealthBar.Size);
                }
                ));
            }

            // status
            string? statusString = StringForStatus(Member.Status);
            if (PlayerStatus.Enabled && PlayerStatus.Label.Enabled && statusString != null)
            {
                PlayerStatus.Label.SetText(statusString);

                drawActions.Add((PlayerStatus.Label.StrataLevel, () =>
                {
                    _statusLabelHud.Draw(Position, _configs.HealthBar.Size);
                }
                ));
            }

            // raise label
            if (showingRaise)
            {
                float duration = Math.Abs(Member.RaiseTime!.Value);
                string text = duration < 10 ? duration.ToString("N1", CultureInfo.InvariantCulture) : Utils.DurationToString(duration);
                RaiseTracker.Icon.Label.SetText(text);

                drawActions.Add((RaiseTracker.Icon.Label.StrataLevel, () =>
                {
                    _raiseLabelHud.Draw(Position, _configs.HealthBar.Size);
                }
                ));
            }

            // invuln label
            if (showingInvuln)
            {
                float duration = Math.Abs(Member.InvulnStatus!.InvulnTime);
                string text = duration < 10 ? duration.ToString("N1", CultureInfo.InvariantCulture) : Utils.DurationToString(duration);
                InvulnTracker.Icon.Label.SetText(text);

                drawActions.Add((InvulnTracker.Icon.Label.StrataLevel, () =>
                {
                    _invulnLabelHud.Draw(Position, _configs.HealthBar.Size);
                }
                ));
            }

            return drawActions;
        }

        private bool ShowingRaise() =>
            Member is { RaiseTime: { } } && RaiseTracker.Enabled && (Member.RaiseTime.Value > 0 || RaiseTracker.KeepIconAfterCastFinishes);

        private bool ShowingInvuln() => Member is { InvulnStatus: { } } && InvulnTracker.Enabled && Member.InvulnStatus.InvulnTime > 0;

        private bool ShowMana()
        {
            if (Member == null)
            {
                return false;
            }

            bool isHealer = JobsHelper.IsJobHealer(Member.JobId);

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
                _ => null
            };
        }

        private static string? StringForStatus(PartyMemberStatus status)
        {
            return status switch
            {
                PartyMemberStatus.ViewingCutscene => "[Viewing Cutscene]",
                PartyMemberStatus.Offline => "[Offline]",
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
