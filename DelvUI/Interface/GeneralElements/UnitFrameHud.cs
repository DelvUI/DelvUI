using Dalamud.Game.ClientState.Objects.SubKinds;
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
    public unsafe class UnitFrameHud : DraggableHudElement, IHudElementWithActor
    {
        public UnitFrameConfig Config => (UnitFrameConfig)_config;

        private readonly OpenContextMenuFromTarget _openContextMenuFromTarget;

        private SmoothHPHelper _smoothHPHelper = new SmoothHPHelper();

        private GameObject? _actor = null;
        public GameObject? Actor
        {
            get => _actor;
            set
            {
                if (_actor == value) { return; }

                _actor = value;
                _smoothHPHelper.Reset();
            }
        }

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

        public override void DrawChildren(Vector2 origin)
        {
            if (!Config.Enabled || Actor == null)
            {
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
                MouseOverHelper.Instance.Target = Actor;

                if (MouseOverHelper.Instance.LeftButtonClicked)
                {
                    Plugin.TargetManager.SetTarget(Actor);
                }
                else if (MouseOverHelper.Instance.RightButtonClicked)
                {
                    var agentHud = new IntPtr(Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalID(4));
                    _openContextMenuFromTarget(agentHud, Actor.Address);
                }
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

            PluginConfigColor fillColor = Config.UseJobColor ? Utils.ColorForActor(character) : Config.FillColor;

            if (Config.UseColorBasedOnHealthValue)
            {
                var scale = (float)currentHp / Math.Max(1, maxHp);
                fillColor = Utils.GetColorByScale(scale, Config.LowHealthColorThreshold / 100f, Config.FullHealthColorThreshold / 100f, Config.LowHealthColor, Config.FullHealthColor, Config.blendMode);
            }

            var background = new Rect(Config.Position, Config.Size, BackgroundColor(character));
            var healthFill = BarUtilities.GetFillRect(Config.Position, Config.Size, Config.FillDirection, fillColor, currentHp, maxHp);
            var bar = new BarHud(Config, character).SetBackground(background).AddForegrounds(healthFill).AddLabels(Config.LeftLabelConfig, Config.RightLabelConfig);

            if (Config.UseMissingHealthBar)
            {
                var healthMissingSize = Config.Size - BarUtilities.GetFillDirectionOffset(healthFill.Size, Config.FillDirection);
                var healthMissingPos = Config.FillDirection.IsInverted() ? Config.Position : Config.Position + BarUtilities.GetFillDirectionOffset(healthFill.Size, Config.FillDirection);
                PluginConfigColor? color = character.CurrentHp is 0 ? Config.DeadBackdrop : Config.HealthMissingColor;
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
        }

        private void DrawFriendlyNPC(Vector2 pos, GameObject? actor)
        {
            var bar = new BarHud(Config, actor);
            bar.AddForegrounds(new Rect(Config.Position, Config.Size, Config.UseJobColor ? GlobalColors.Instance.NPCFriendlyColor : Config.FillColor));
            bar.AddLabels(Config.LeftLabelConfig, Config.RightLabelConfig);
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
                else if (chara.CurrentHp is 0)
                {
                    return Config.DeadBackdrop;
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
