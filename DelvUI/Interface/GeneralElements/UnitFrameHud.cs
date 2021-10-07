using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DelvUI.Interface.GeneralElements
{
    public unsafe class UnitFrameHud : DraggableHudElement, IHudElementWithActor
    {
        private UnitFrameConfig Config => (UnitFrameConfig)_config;

        private readonly OpenContextMenuFromTarget _openContextMenuFromTarget;

        public GameObject? Actor { get; set; } = null;

        public UnitFrameHud(string id, UnitFrameConfig config, string displayName) : base(id, config, displayName)
        { 
            // interaction stuff
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
                if (ImGui.GetIO().MouseClicked[0])
                {
                    Plugin.TargetManager.SetTarget(Actor);
                }
                else if (ImGui.GetIO().MouseClicked[1])
                {
                    var agentHud = new IntPtr(Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalID(4));
                    _openContextMenuFromTarget(agentHud, Actor.Address);
                }

                MouseOverHelper.Instance.Target = Actor;
            }
        }

        private void DrawCharacter(Vector2 pos, Character character)
        {
            float currentHp = character.CurrentHp;
            float maxHp = character.MaxHp;

            if (Config.TankStanceIndicatorConfig is { Enabled: true } && JobsHelper.IsJobTank(character.ClassJob.Id))
            {
                DrawTankStanceIndicator(pos);
            }

            PluginConfigColor fillColor = Config.UseJobColor ? Utils.ColorForActor(character) : Config.FillColor;

            if (Config.UseColorBasedOnHealthValue)
            {
                var scale = currentHp / Math.Max(1, maxHp);
                fillColor = Utils.ColorByHealthValue(scale, Config.LowHealthColorThreshold / 100f, Config.FullHealthColorThreshold / 100f, Config.FullHealthColor, Config.LowHealthColor);
            }

            var background = new Rect(Config.Position, Config.Size, BackgroundColor(character));
            var healthFill = BarUtilities.GetFillRect(Config.Position, Config.Size, Config.FillDirection, fillColor, currentHp, maxHp);
            var bar = new BarHud(Config, character).Background(background).Foreground(healthFill).Labels(Config.LeftLabelConfig, Config.RightLabelConfig);

            if (Config.UseMissingHealthBar)
            {
                var healthMissingSize = Config.Size - BarUtilities.GetFillDirectionOffset(healthFill.Size, Config.FillDirection);
                var healthMissingPos = Config.FillDirection.IsInverted() ? Config.Position : Config.Position + BarUtilities.GetFillDirectionOffset(healthFill.Size, Config.FillDirection);
                bar.Foreground(new Rect(healthMissingPos, healthMissingSize, Config.HealthMissingColor));
            }

            if (Config.ShieldConfig.Enabled)
            {
                float shield = Utils.ActorShieldValue(Actor) * maxHp;
                float overshield = Config.ShieldConfig.FillHealthFirst ? Math.Max(shield + currentHp - maxHp, 0f) : shield;
                Rect overshieldFill = BarUtilities.GetFillRect(Config.Position, Config.Size, Config.FillDirection, Config.ShieldConfig.Color, overshield, maxHp);
                bar.Foreground(overshieldFill);

                if (Config.ShieldConfig.FillHealthFirst && currentHp < maxHp)
                {
                    var shieldPos = Config.FillDirection.IsInverted() ? Config.Position : Config.Position + BarUtilities.GetFillDirectionOffset(healthFill.Size, Config.FillDirection);
                    var shieldSize = Config.Size - BarUtilities.GetFillDirectionOffset(healthFill.Size, Config.FillDirection);
                    Rect shieldFill = BarUtilities.GetFillRect(shieldPos, shieldSize, Config.FillDirection, Config.ShieldConfig.Color, shield - overshield, maxHp - currentHp, 0f);
                    bar.Foreground(shieldFill);
                }
            }

            bar.Draw(pos);
        }

        private void DrawFriendlyNPC(Vector2 pos, GameObject? Actor)
        {
            var bar = new BarHud(Config, Actor);
            bar.Foreground(new Rect(Config.Position, Config.Size, Config.UseJobColor? GlobalColors.Instance.NPCFriendlyColor : Config.FillColor));
            bar.Labels(Config.LeftLabelConfig, Config.RightLabelConfig);
            bar.Draw(pos);
        }

        private void DrawTankStanceIndicator(Vector2 pos)
        {
            if (Actor is not BattleChara battleChara || Config.TankStanceIndicatorConfig == null)
            {
                return;
            }

            var tankStanceBuff = battleChara.StatusList.Where(
                o => o.StatusId is 79 or 91 or 392 or 393 or 743 or 1396 or 1397 or 1833
            );

            var thickness = Config.TankStanceIndicatorConfig.Thickness + 1;
            var barSize = new Vector2(Math.Min(Config.Size.X, Config.Size.Y), Config.Size.Y);
            var cursorPos = Utils.GetAnchoredPosition(Config.Position + new Vector2(-thickness, thickness), Config.Size, Config.Anchor);

            var color = !tankStanceBuff.Any() ? Config.TankStanceIndicatorConfig.InactiveColor : Config.TankStanceIndicatorConfig.ActiveColor;
            var bar = new BarHud("DelvUI_TankStance").Background(new Rect(cursorPos, barSize)).Foreground(new Rect(cursorPos, barSize, color));
            bar.Draw(pos);
        }

        private PluginConfigColor BackgroundColor(Character? chara)
        {
            if (Config.ShowTankInvulnerability && chara is BattleChara battleChara && Utils.HasTankInvulnerability(battleChara))
            {
                if (Config.UseCustomInvulnerabilityColor)
                {
                    return Config.CustomInvulnerabilityColor;
                }
                else
                {
                    return new PluginConfigColor(GlobalColors.Instance.SafeColorForJobId(chara.ClassJob.Id).Vector.AdjustColor(-.8f));
                }
            }

            if (chara is BattleChara)
            {
                if (Config.UseJobColorAsBackgroundColor)
                {
                    return GlobalColors.Instance.SafeColorForJobId(chara.ClassJob.Id);
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
