﻿using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DelvUI.Interface.GeneralElements
{
    public unsafe class UnitFrameHud : DraggableHudElement, IHudElementWithActor, IHudElementWithMouseOver
    {
        public UnitFrameConfig Config => (UnitFrameConfig)_config;

        private readonly OpenContextMenuFromTarget _openContextMenuFromTarget;

        private SmoothHPHelper _smoothHPHelper = new SmoothHPHelper();

        public GameObject? Actor { get; set; }

        private bool _wasHovering = false;

        public UnitFrameHud(UnitFrameConfig config, string displayName) : base(config, displayName)
        {
            // interaction stuff

            /*
             Part of openContextMenuFromTarget disassembly signature
            .text:00007FF648523940                   Client__UI__Agent__AgentHUD_OpenContextMenuFromTarget proc near
            .text:00007FF648523940
            .text:00007FF648523940                   arg_0= qword ptr  8
            .text:00007FF648523940                   arg_8= qword ptr  10h
            .text:00007FF648523940
            .text:00007FF648523940 48 85 D2          test    rdx, rdx
            .text:00007FF648523943 74 7F             jz      short locret_7FF6485239C4
            */
            _openContextMenuFromTarget =
                Marshal.GetDelegateForFunctionPointer<OpenContextMenuFromTarget>(Plugin.SigScanner.ScanText("48 85 D2 74 7F 48 89 5C 24"));
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            return (new List<Vector2>() { Config.Position }, new List<Vector2>() { Config.Size });
        }

        public void StopMouseover()
        {
            if (_wasHovering)
            {
                InputsHelper.Instance.ClearTarget();
                _wasHovering = false;
            }
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (!Config.Enabled || Actor == null)
            {
                StopMouseover();
                return;
            }

            if (Actor is Character character)
            {
                DrawCharacter(origin, character);
            }
            else
            {
                DrawFriendlyNPC(origin, Actor);
            }

            // Check if mouse is hovering over the box properly
            var startPos = Utils.GetAnchoredPosition(origin + Config.Position, Config.Size, Config.Anchor);
            if (ImGui.IsMouseHoveringRect(startPos, startPos + Config.Size) && !DraggingEnabled)
            {
                InputsHelper.Instance.SetTarget(Actor);
                _wasHovering = true;

                if (InputsHelper.Instance.LeftButtonClicked)
                {
                    Plugin.TargetManager.SetTarget(Actor);
                }
                else if (InputsHelper.Instance.RightButtonClicked)
                {
                    var agentHud = new IntPtr(Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalID(4));
                    _openContextMenuFromTarget(agentHud, Actor.Address);
                }
            }
            else if (_wasHovering)
            {
                InputsHelper.Instance.ClearTarget();
                _wasHovering = false;
            }
        }

        private void DrawCharacter(Vector2 pos, Character character)
        {
            uint currentHp = character.CurrentHp;
            uint maxHp = character.MaxHp;

            if (Config.SmoothHealthConfig.Enabled)
            {
                currentHp = _smoothHPHelper.GetNextHp((int)currentHp, (int)maxHp, Config.SmoothHealthConfig.Velocity);
            }

            PluginConfigColor fillColor = GetColor(character, currentHp, maxHp);
            Rect background = new Rect(Config.Position, Config.Size, BackgroundColor(character));
            Rect healthFill = BarUtilities.GetFillRect(Config.Position, Config.Size, Config.FillDirection, fillColor, currentHp, maxHp);

            BarHud bar = new BarHud(Config, character);
            bar.SetBackground(background);
            bar.AddForegrounds(healthFill);
            bar.AddLabels(GetLabels(maxHp));

            if (Config.UseMissingHealthBar)
            {
                Vector2 healthMissingSize = Config.Size - BarUtilities.GetFillDirectionOffset(healthFill.Size, Config.FillDirection);
                Vector2 healthMissingPos = Config.FillDirection.IsInverted() ? Config.Position : Config.Position + BarUtilities.GetFillDirectionOffset(healthFill.Size, Config.FillDirection);
                PluginConfigColor? color = Config.UseDeathIndicatorBackgroundColor && character.CurrentHp <= 0 ? Config.DeathIndicatorBackgroundColor : Config.HealthMissingColor;
                bar.AddForegrounds(new Rect(healthMissingPos, healthMissingSize, color));
            }

            if (Config.ShieldConfig.Enabled)
            {
                float shield = Utils.ActorShieldValue(Actor);
                if (shield > 0f)
                {
                    bar.AddForegrounds(
                        BarUtilities.GetShieldForeground(
                            Config.ShieldConfig,
                            Config.Position,
                            Config.Size,
                            healthFill.Size,
                            Config.FillDirection,
                            shield,
                            character.CurrentHp,
                            character.MaxHp)
                    );
                }
            }

            bar.Draw(pos);

            // role/job icon
            if (Config.RoleIconConfig.Enabled && character is PlayerCharacter)
            {
                uint jobId = character.ClassJob.Id;
                uint iconId = Config.RoleIconConfig.UseRoleIcons ?
                        JobsHelper.RoleIconIDForJob(jobId, Config.RoleIconConfig.UseSpecificDPSRoleIcons) :
                        JobsHelper.IconIDForJob(jobId) + (uint)Config.RoleIconConfig.Style * 100;

                if (iconId > 0)
                {
                    var barPos = Utils.GetAnchoredPosition(pos, Config.Size, Config.Anchor);
                    var parentPos = Utils.GetAnchoredPosition(barPos + Config.Position, -Config.Size, Config.RoleIconConfig.FrameAnchor);
                    var iconPos = Utils.GetAnchoredPosition(parentPos + Config.RoleIconConfig.Position, Config.RoleIconConfig.Size, Config.RoleIconConfig.Anchor);

                    DrawHelper.DrawInWindow(ID + "_jobIcon", iconPos, Config.RoleIconConfig.Size, false, false, (drawList) =>
                    {
                        DrawHelper.DrawIcon(iconId, iconPos, Config.RoleIconConfig.Size, false, drawList);
                    });
                }
            }
        }

        private LabelConfig[] GetLabels(uint maxHp)
        {
            List<LabelConfig> labels = new List<LabelConfig>();

            if (Config.HideHealthIfPossible)
            {
                bool isHealthLabel = IsHealthLabel(Config.LeftLabelConfig);
                if (!isHealthLabel || maxHp > 0)
                {
                    labels.Add(Config.LeftLabelConfig);
                }

                isHealthLabel = IsHealthLabel(Config.RightLabelConfig);
                if (!isHealthLabel || maxHp > 0)
                {
                    labels.Add(Config.RightLabelConfig);
                }
            }
            else
            {
                labels.Add(Config.LeftLabelConfig);
                labels.Add(Config.RightLabelConfig);
            }

            return labels.ToArray();
        }

        private bool IsHealthLabel(LabelConfig config)
        {
            return config.GetText().Contains("[health");
        }

        private PluginConfigColor GetColor(GameObject? actor, uint currentHp = 0, uint maxHp = 0)
        {
            Character? character = actor as Character;

            if (Config.UseJobColor && character != null)
            {
                return Utils.ColorForActor(character);
            }
            else if (Config.UseRoleColor)
            {
                return character is PlayerCharacter ?
                    GlobalColors.Instance.SafeRoleColorForJobId(character.ClassJob.Id) :
                    Utils.ColorForActor(character);
            }
            else if (Config.ColorByHealth.Enabled && character != null)
            {
                var scale = (float)currentHp / Math.Max(1, maxHp);
                return Utils.GetColorByScale(scale, Config.ColorByHealth);
            }

            return Config.FillColor;
        }

        private void DrawFriendlyNPC(Vector2 pos, GameObject? actor)
        {
            var bar = new BarHud(Config, actor);
            bar.AddForegrounds(new Rect(Config.Position, Config.Size, GetColor(actor)));
            bar.AddLabels(GetLabels(0));
            bar.Draw(pos);
        }

        private PluginConfigColor BackgroundColor(Character? chara)
        {
            if (Config.ShowTankInvulnerability && chara is BattleChara battleChara)
            {
                Status tankInvuln = Utils.HasTankInvulnerability(battleChara);

                if (tankInvuln != null)
                {
                    PluginConfigColor color;
                    if (Config.UseCustomInvulnerabilityColor)
                    {
                        color = Config.CustomInvulnerabilityColor;
                    }
                    else if (tankInvuln.StatusId == 811 && Config.UseCustomWalkingDeadColor)
                    {
                        color = Config.CustomWalkingDeadColor;
                    }
                    else
                    {
                        color = new PluginConfigColor(GlobalColors.Instance.SafeColorForJobId(chara.ClassJob.Id).Vector.AdjustColor(-.8f));
                    }

                    return color;
                }

            }

            if (chara is BattleChara)
            {
                if (Config.UseJobColorAsBackgroundColor)
                {
                    return GlobalColors.Instance.SafeColorForJobId(chara.ClassJob.Id);
                }
                else if (Config.UseDeathIndicatorBackgroundColor && chara.CurrentHp <= 0)
                {
                    return Config.DeathIndicatorBackgroundColor;
                }
                else
                {
                    return Config.BackgroundColor;
                }
            }

            return GlobalColors.Instance.EmptyUnitFrameColor;
        }

        private delegate void OpenContextMenuFromTarget(IntPtr agentHud, IntPtr gameObject);
    }
}
