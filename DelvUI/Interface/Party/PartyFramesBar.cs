﻿using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using DelvUI.Interface.StatusEffects;
using ImGuiNET;
using ImGuiScene;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

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
        private PartyFramesCooldownListHud _cooldownListHud;

        private TextureWrap? _readyCheckTexture = null;

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
            _orderLabelHud = new LabelHud(_configs.HealthBar.OrderNumberConfig);
            _statusLabelHud = new LabelHud(PlayerStatus.Label);
            _raiseLabelHud = new LabelHud(RaiseTracker.Icon.NumericLabel);
            _invulnLabelHud = new LabelHud(InvulnTracker.Icon.NumericLabel);

            _manaBarHud = new PrimaryResourceHud(_configs.ManaBar);
            _castbarHud = new CastbarHud(_configs.CastBar);
            _buffsListHud = new StatusEffectsListHud(_configs.Buffs);
            _debuffsListHud = new StatusEffectsListHud(_configs.Debuffs);

            _cooldownListHud = new PartyFramesCooldownListHud(_configs.CooldownList);

            _readyCheckTexture = TexturesCache.Instance.GetTextureFromPath("ui/uld/ReadyCheck_hr1.tex") ?? TexturesCache.Instance.GetTextureFromPath("ui/uld/ReadyCheck.tex");
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
                if (_configs.HealthBar.ColorsConfig.ColorByHealth.UseJobColorAsMaxHealth)
                {
                    return ColorUtils.GetColorByScale(scale, _configs.HealthBar.ColorsConfig.ColorByHealth.LowHealthColorThreshold / 100f, _configs.HealthBar.ColorsConfig.ColorByHealth.FullHealthColorThreshold / 100f, _configs.HealthBar.ColorsConfig.ColorByHealth.LowHealthColor, _configs.HealthBar.ColorsConfig.ColorByHealth.FullHealthColor,
                        GlobalColors.Instance.SafeColorForJobId(Member.JobId), _configs.HealthBar.ColorsConfig.ColorByHealth.UseMaxHealthColor, _configs.HealthBar.ColorsConfig.ColorByHealth.BlendMode);
                }
                else if (_configs.HealthBar.ColorsConfig.ColorByHealth.UseRoleColorAsMaxHealth)
                {
                    return ColorUtils.GetColorByScale(scale, _configs.HealthBar.ColorsConfig.ColorByHealth.LowHealthColorThreshold / 100f, _configs.HealthBar.ColorsConfig.ColorByHealth.FullHealthColorThreshold / 100f, _configs.HealthBar.ColorsConfig.ColorByHealth.LowHealthColor, _configs.HealthBar.ColorsConfig.ColorByHealth.FullHealthColor,
                        GlobalColors.Instance.SafeRoleColorForJobId(Member.JobId), _configs.HealthBar.ColorsConfig.ColorByHealth.UseMaxHealthColor, _configs.HealthBar.ColorsConfig.ColorByHealth.BlendMode);
                }
                return ColorUtils.GetColorByScale(scale, _configs.HealthBar.ColorsConfig.ColorByHealth);
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
            _cooldownListHud.StopPreview();
            _configs.HealthBar.MouseoverAreaConfig.Preview = false;
        }

        public void StopMouseover()
        {
            if (_wasHovering)
            {
                InputsHelper.Instance.ClearTarget();
                _wasHovering = false;
            }
        }

        public List<(StrataLevel, Action)> GetBarDrawActions(Vector2 origin, PluginConfigColor? borderColor = null)
        {
            List<(StrataLevel, Action)> drawActions = new List<(StrataLevel, Action)>();

            if (!Visible || Member is null)
            {
                StopMouseover();
                return drawActions;
            }

            // click
            var (areaStart, areaEnd) = _configs.HealthBar.MouseoverAreaConfig.GetArea(Position, _configs.HealthBar.Size);
            bool isHovering = ImGui.IsMouseHoveringRect(areaStart, areaEnd);
            bool ignoreMouseover = _configs.HealthBar.MouseoverAreaConfig.Enabled && _configs.HealthBar.MouseoverAreaConfig.Ignore;
            Character? character = Member.Character;

            if (isHovering)
            {
                _wasHovering = true;
                InputsHelper.Instance.SetTarget(character, ignoreMouseover);

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
                        Plugin.TargetManager.Target = character;
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
            else if (_configs.HealthBar.ColorsConfig.UseJobColorAsBackgroundColor && character is BattleChara)
            {
                bgColor = GlobalColors.Instance.SafeColorForJobId(character.ClassJob.Id);
            }
            else if (_configs.HealthBar.ColorsConfig.UseRoleColorAsBackgroundColor && character is BattleChara)
            {
                bgColor = _configs.HealthBar.RangeConfig.Enabled
                    ? GetDistanceColor(character, GlobalColors.Instance.SafeRoleColorForJobId(character.ClassJob.Id))
                    : GlobalColors.Instance.SafeRoleColorForJobId(character.ClassJob.Id);
            }

            Rect background = new Rect(Position, _configs.HealthBar.Size, bgColor);

            // hp
            uint currentHp = Member.HP;
            uint maxHp = Member.MaxHP;

            if (_configs.HealthBar.SmoothHealthConfig.Enabled)
            {
                currentHp = _smoothHPHelper.GetNextHp((int)currentHp, (int)maxHp, _configs.HealthBar.SmoothHealthConfig.Velocity);
            }

            float hpScale = maxHp > 0 ? (float)currentHp / (float)maxHp : 1;
            PluginConfigColor? hpColor = _configs.HealthBar.RangeConfig.Enabled && character != null
            ? GetDistanceColor(character, GetColor(hpScale))
            : GetColor(hpScale);

            Rect healthFill = BarUtilities.GetFillRect(Position, _configs.HealthBar.Size, _configs.HealthBar.FillDirection, hpColor, currentHp, maxHp);

            // bar
            int thickness = borderColor != null ? _configs.HealthBar.ColorsConfig.ActiveBorderThickness : _configs.HealthBar.ColorsConfig.InactiveBorderThickness;

            if (WhosTalkingIcon.ChangeBorders && Member.WhosTalkingState != WhosTalkingState.None)
            {
                thickness = WhosTalkingIcon.BorderThickness;
            }

            borderColor = borderColor ?? GetBorderColor(character);

            BarHud bar = new BarHud(
                _configs.HealthBar.ID,
                _configs.HealthBar.ColorsConfig.ShowBorder,
                borderColor,
                thickness,
                actor: character,
                current: currentHp,
                max: maxHp,
                shadowConfig: _configs.HealthBar.ShadowConfig,
                barTextureName: _configs.HealthBar.BarTextureName,
                barTextureDrawMode: _configs.HealthBar.BarTextureDrawMode
            );

            bar.SetBackground(background);
            bar.AddForegrounds(healthFill);

            // missing health
            if (_configs.HealthBar.ColorsConfig.UseMissingHealthBar)
            {
                Vector2 healthMissingSize = _configs.HealthBar.Size - BarUtilities.GetFillDirectionOffset(healthFill.Size, _configs.HealthBar.FillDirection);
                Vector2 healthMissingPos = _configs.HealthBar.FillDirection.IsInverted() ? Position : Position + BarUtilities.GetFillDirectionOffset(healthFill.Size, _configs.HealthBar.FillDirection);

                PluginConfigColor? missingHealthColor = _configs.HealthBar.ColorsConfig.UseJobColorAsMissingHealthColor && character is BattleChara
                    ? GlobalColors.Instance.SafeColorForJobId(character!.ClassJob.Id)
                    : _configs.HealthBar.ColorsConfig.UseRoleColorAsMissingHealthColor && character is BattleChara
                        ? GlobalColors.Instance.SafeRoleColorForJobId(character!.ClassJob.Id)
                        : _configs.HealthBar.ColorsConfig.HealthMissingColor;

                if (_configs.HealthBar.ColorsConfig.UseDeathIndicatorBackgroundColor && Member.HP <= 0 && character != null)
                {
                    missingHealthColor = _configs.HealthBar.ColorsConfig.DeathIndicatorBackgroundColor;
                }

                if (_configs.Trackers.Invuln.ChangeBackgroundColorWhenInvuln && character is BattleChara battleChara)
                {
                    Status? tankInvuln = Utils.GetTankInvulnerabilityID(battleChara);
                    if (tankInvuln is not null)
                    {
                        missingHealthColor = _configs.Trackers.Invuln.BackgroundColor;
                    }
                }

                if (_configs.HealthBar.RangeConfig.Enabled)
                {
                    missingHealthColor = GetDistanceColor(character, missingHealthColor);
                }

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
            bool isSoftTarget = character != null && character == Plugin.TargetManager.SoftTarget;
            if (_configs.HealthBar.ColorsConfig.ShowHighlight && (isHovering || isSoftTarget))
            {
                Rect highlight = new Rect(Position, _configs.HealthBar.Size, _configs.HealthBar.ColorsConfig.HighlightColor);
                bar.AddForegrounds(highlight);
            }

            drawActions = bar.GetDrawActions(Vector2.Zero, _configs.HealthBar.StrataLevel);

            // mouseover area
            BarHud? mouseoverAreaBar = _configs.HealthBar.MouseoverAreaConfig.GetBar(
                Position,
                _configs.HealthBar.Size,
                _configs.HealthBar.ID + "_mouseoverArea"
            );

            if (mouseoverAreaBar != null)
            {
                drawActions.AddRange(mouseoverAreaBar.GetDrawActions(Vector2.Zero, StrataLevel.HIGHEST));
            }

            return drawActions;
        }

        private PluginConfigColor GetBorderColor(Character? character)
        {
            GameObject? target = Plugin.TargetManager.Target ?? Plugin.TargetManager.SoftTarget;

            return character != null && character == target ? _configs.HealthBar.ColorsConfig.TargetBordercolor : _configs.HealthBar.ColorsConfig.BorderColor;
        }

        private PluginConfigColor GetDistanceColor(Character? character, PluginConfigColor color)
        {
            byte distance = character != null ? character.YalmDistanceX : byte.MaxValue;
            float currentAlpha = color.Vector.W * 100f;
            float alpha = _configs.HealthBar.RangeConfig.AlphaForDistance(distance, currentAlpha) / 100f;

            return color.WithAlpha(alpha);
        }

        // need to separate elements that have their own window so clipping doesn't get messy
        public List<(StrataLevel, Action)> GetElementsDrawActions(Vector2 origin)
        {
            List<(StrataLevel, Action)> drawActions = new List<(StrataLevel, Action)>();

            PlayerCharacter? player = Plugin.ClientState.LocalPlayer;
            if (!Visible || Member is null || player == null)
            {
                StopMouseover();
                return drawActions;
            }

            Character? character = Member.Character;

            // who's talking
            bool drawingWhosTalking = false;
            if (WhosTalkingIcon.Enabled && WhosTalkingIcon.Icon.Enabled && WhosTalkingIcon.EnabledForState(Member.WhosTalkingState))
            {
                TextureWrap? texture = WhosTalkingHelper.Instance.GetTextureForState(Member.WhosTalkingState);

                if (texture != null)
                {
                    Vector2 parentPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, WhosTalkingIcon.Icon.FrameAnchor);
                    Vector2 iconPos = Utils.GetAnchoredPosition(parentPos + WhosTalkingIcon.Icon.Position, WhosTalkingIcon.Icon.Size, WhosTalkingIcon.Icon.Anchor);

                    drawActions.Add((WhosTalkingIcon.Icon.StrataLevel, () =>
                    {
                        DrawHelper.DrawInWindow(WhosTalkingIcon.Icon.ID, iconPos, WhosTalkingIcon.Icon.Size, false, (drawList) =>
                        {
                            ImGui.SetCursorPos(iconPos);
                            ImGui.Image(texture.ImGuiHandle, WhosTalkingIcon.Icon.Size);
                        });
                    }
                    ));

                    drawingWhosTalking = true;
                }
            }

            // role/job icon
            if (RoleIcon.Enabled && (!drawingWhosTalking || !WhosTalkingIcon.ReplaceRoleJobIcon))
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
                        JobsHelper.IconIDForJob(Member.JobId, (uint)RoleIcon.Style);
                }

                if (iconId > 0)
                {
                    Vector2 parentPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, RoleIcon.FrameAnchor);
                    Vector2 iconPos = Utils.GetAnchoredPosition(parentPos + RoleIcon.Position, RoleIcon.Size, RoleIcon.Anchor);

                    drawActions.Add((RoleIcon.StrataLevel, () =>
                    {
                        DrawHelper.DrawInWindow(RoleIcon.ID, iconPos, RoleIcon.Size, false, (drawList) =>
                        {
                            DrawHelper.DrawIcon(iconId, iconPos, RoleIcon.Size, false, drawList);
                        });
                    }
                    ));
                }
            }

            // sign icon
            if (SignIcon.Enabled)
            {
                uint? iconId = SignIcon.IconID(character);
                if (iconId.HasValue)
                {
                    Vector2 parentPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, SignIcon.FrameAnchor);
                    Vector2 iconPos = Utils.GetAnchoredPosition(parentPos + SignIcon.Position, SignIcon.Size, SignIcon.Anchor);

                    drawActions.Add((SignIcon.StrataLevel, () =>
                    {
                        DrawHelper.DrawInWindow(SignIcon.ID, iconPos, SignIcon.Size, false, (drawList) =>
                        {
                            DrawHelper.DrawIcon(iconId.Value, iconPos, SignIcon.Size, false, drawList);
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
                    DrawHelper.DrawInWindow(LeaderIcon.ID, iconPos, LeaderIcon.Size, false, (drawList) =>
                    {
                        DrawHelper.DrawIcon(61571, iconPos, LeaderIcon.Size, false, drawList);
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
                        DrawHelper.DrawInWindow(PlayerStatus.Icon.ID, iconPos, PlayerStatus.Icon.Size, false, (drawList) =>
                        {
                            DrawHelper.DrawIcon(iconId.Value, iconPos, PlayerStatus.Icon.Size, false, drawList);
                        });
                    }
                    ));
                }
            }

            // ready check status icon
            if (Member.ReadyCheckStatus != ReadyCheckStatus.None && ReadyCheckIcon.Enabled && ReadyCheckIcon.Icon.Enabled && _readyCheckTexture != null)
            {
                Vector2 parentPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, ReadyCheckIcon.Icon.FrameAnchor);
                Vector2 iconPos = Utils.GetAnchoredPosition(parentPos + ReadyCheckIcon.Icon.Position, ReadyCheckIcon.Icon.Size, ReadyCheckIcon.Icon.Anchor);

                drawActions.Add((ReadyCheckIcon.Icon.StrataLevel, () =>
                {
                    DrawHelper.DrawInWindow(ReadyCheckIcon.Icon.ID, iconPos, ReadyCheckIcon.Icon.Size, false, (drawList) =>
                    {
                        Vector2 uv0 = new Vector2(0.5f * (int)Member.ReadyCheckStatus, 0f);
                        Vector2 uv1 = new Vector2(0.5f + 0.5f * (int)Member.ReadyCheckStatus, 1f);
                        drawList.AddImage(_readyCheckTexture.ImGuiHandle, iconPos, iconPos + ReadyCheckIcon.Icon.Size, uv0, uv1);
                    });
                }
                ));
            }

            // raise icon
            bool showingRaise = ShowingRaise();
            if (showingRaise && RaiseTracker.Icon.Enabled)
            {
                Vector2 parentPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, RaiseTracker.Icon.FrameAnchor);
                Vector2 iconPos = Utils.GetAnchoredPosition(parentPos + RaiseTracker.Icon.Position, RaiseTracker.Icon.Size, RaiseTracker.Icon.Anchor);

                drawActions.Add((RaiseTracker.Icon.StrataLevel, () =>
                {
                    DrawHelper.DrawInWindow(RaiseTracker.Icon.ID, iconPos, RaiseTracker.Icon.Size, false, (drawList) =>
                    {
                        DrawHelper.DrawIcon(411, iconPos, RaiseTracker.Icon.Size, true, drawList);
                    });
                }
                ));
            }

            // invuln icon
            bool showingInvuln = ShowingInvuln();
            if (showingInvuln && InvulnTracker.Icon.Enabled)
            {
                Vector2 parentPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, InvulnTracker.Icon.FrameAnchor);
                Vector2 iconPos = Utils.GetAnchoredPosition(parentPos + InvulnTracker.Icon.Position, InvulnTracker.Icon.Size, InvulnTracker.Icon.Anchor);

                drawActions.Add((InvulnTracker.Icon.StrataLevel, () =>
                {
                    DrawHelper.DrawInWindow(InvulnTracker.Icon.ID, iconPos, InvulnTracker.Icon.Size, false, (drawList) =>
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
                drawActions.Add((_configs.ManaBar.StrataLevel, () =>
                {
                    _manaBarHud.Actor = character;
                    _manaBarHud.PartyMember = Member;
                    _manaBarHud.PrepareForDraw(parentPos);
                    _manaBarHud.Draw(parentPos);
                }
                ));
            }

            // buffs / debuffs
            Vector2 buffsPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, _configs.Buffs.HealthBarAnchor);
            drawActions.Add((_configs.Buffs.StrataLevel, () =>
            {
                _buffsListHud.Actor = character;
                _buffsListHud.PrepareForDraw(buffsPos);
                _buffsListHud.Draw(buffsPos);
            }
            ));

            Vector2 debuffsPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, _configs.Debuffs.HealthBarAnchor);
            drawActions.Add((_configs.Debuffs.StrataLevel, () =>
            {
                _debuffsListHud.Actor = character;
                _debuffsListHud.PrepareForDraw(debuffsPos);
                _debuffsListHud.Draw(debuffsPos);
            }
            ));

            // cooldown list
            Vector2 cooldownListPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, _configs.CooldownList.HealthBarAnchor);
            drawActions.Add((_configs.CooldownList.StrataLevel, () =>
            {
                _cooldownListHud.Actor = character;
                _cooldownListHud.PrepareForDraw(cooldownListPos);
                _cooldownListHud.Draw(cooldownListPos);
            }
            ));

            // castbar
            Vector2 castbarPos = Utils.GetAnchoredPosition(Position, -_configs.HealthBar.Size, _configs.CastBar.HealthBarAnchor);
            drawActions.Add((_configs.CastBar.StrataLevel, () =>
            {
                _castbarHud.Actor = character;
                _castbarHud.PrepareForDraw(castbarPos);
                _castbarHud.Draw(castbarPos);
            }
            ));

            // name
            bool drawName = ShouldDrawName(character, showingRaise, showingInvuln);
            if (drawName)
            {
                drawActions.Add((_configs.HealthBar.NameLabelConfig.StrataLevel, () =>
                {
                    bool? playerName = null;
                    if (character == null || character.ObjectKind == ObjectKind.Player)
                    {
                        playerName = true;
                    }

                    _nameLabelHud.Draw(Position, _configs.HealthBar.Size, character, Member.Name, isPlayerName: playerName);
                }
                ));
            }

            // health label
            if (Member.MaxHP > 0)
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
                string str = char.ConvertFromUtf32(0xE090 + order - 1).ToString();

                drawActions.Add((_configs.HealthBar.OrderNumberConfig.StrataLevel, () =>
                {
                    _configs.HealthBar.OrderNumberConfig.SetText(str);
                    _orderLabelHud.Draw(Position, _configs.HealthBar.Size, character);
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
                float duration = Math.Abs(Member.RaiseTime!.Value);

                drawActions.Add((RaiseTracker.Icon.NumericLabel.StrataLevel, () =>
                {
                    RaiseTracker.Icon.NumericLabel.SetValue(duration);
                    _raiseLabelHud.Draw(Position, _configs.HealthBar.Size);
                }
                ));
            }

            // invuln label
            if (showingInvuln)
            {
                float duration = Math.Abs(Member.InvulnStatus!.InvulnTime);

                drawActions.Add((InvulnTracker.Icon.NumericLabel.StrataLevel, () =>
                {
                    InvulnTracker.Icon.NumericLabel.SetValue(duration);
                    _invulnLabelHud.Draw(Position, _configs.HealthBar.Size);
                }
                ));
            }

            return drawActions;
        }

        private bool ShouldDrawName(Character? character, bool showingRaise, bool showingInvuln)
        {
            if (showingRaise && RaiseTracker.HideNameWhenRaised)
            {
                return false;
            }

            if (showingInvuln && InvulnTracker.HideNameWhenInvuln)
            {
                return false;
            }

            if (Member != null && PlayerStatus.Enabled && PlayerStatus.HideName && Member.Status != PartyMemberStatus.None)
            {
                return false;
            }

            if (Member != null && ReadyCheckIcon.Enabled && ReadyCheckIcon.HideName && Member.ReadyCheckStatus != ReadyCheckStatus.None)
            {
                return false;
            }

            if (Utils.IsActorCasting(character) && _configs.CastBar.Enabled && _configs.CastBar.HideNameWhenCasting)
            {
                return false;
            }

            return true;
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
        private SignIconConfig SignIcon => _configs.Icons.Sign;
        private PartyFramesLeaderIconConfig LeaderIcon => _configs.Icons.Leader;
        private PartyFramesPlayerStatusConfig PlayerStatus => _configs.Icons.PlayerStatus;
        private PartyFramesReadyCheckStatusConfig ReadyCheckIcon => _configs.Icons.ReadyCheckStatus;
        private PartyFramesWhosTalkingConfig WhosTalkingIcon => _configs.Icons.WhosTalking;
        private PartyFramesRaiseTrackerConfig RaiseTracker => _configs.Trackers.Raise;
        private PartyFramesInvulnTrackerConfig InvulnTracker => _configs.Trackers.Invuln;
        private PartyFramesCleanseTrackerConfig CleanseTracker => _configs.Trackers.Cleanse;
        #endregion
    }
}
