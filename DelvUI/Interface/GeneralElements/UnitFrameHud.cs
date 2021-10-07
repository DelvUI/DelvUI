using Dalamud.Game.ClientState.Objects.Types;
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
        private LabelHud _leftLabel;
        private LabelHud _rightLabel;

        private readonly OpenContextMenuFromTarget _openContextMenuFromTarget;

        public GameObject? Actor { get; set; } = null;

        public UnitFrameHud(string id, UnitFrameConfig config, string displayName) : base(id, config, displayName)
        {
            // labels
            _leftLabel = new LabelHud(id + "_leftLabel", Config.LeftLabelConfig);
            _rightLabel = new LabelHud(id + "_rightLabel", Config.RightLabelConfig);

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
            PluginConfigColor fillColor = Config.UseJobColor ? Utils.ColorForActor(character) : Config.FillColor;

            if (Config.UseColorBasedOnHealthValue)
            {
                var scale = (float)character.CurrentHp / Math.Max(1, character.MaxHp);
                fillColor = Utils.ColorByHealthValue(scale, Config.LowHealthColorThreshold / 100f, Config.FullHealthColorThreshold / 100f, Config.FullHealthColor, Config.LowHealthColor);
            }

            var background = new Rect(Config.Position, Config.Size, BackgroundColor(character));
            var healthFill = BarUtilities.GetFillRect(Config.Position, Config.Size, Config.FillDirection, fillColor, character.CurrentHp, character.MaxHp);
            var bar = new BarHud(ID, Config, character).Background(background).Foreground(healthFill).Labels(Config.LeftLabelConfig, Config.RightLabelConfig);

            if (Config.UseMissingHealthBar)
            {
                var healthMissingSize = Config.Size - BarUtilities.GetFillDirectionOffset(healthFill.Size, Config.FillDirection);
                var healthMissingPos = Config.FillDirection.IsInverted() ? Config.Position : Config.Position + BarUtilities.GetFillDirectionOffset(healthFill.Size, Config.FillDirection);
                bar.Foreground(new Rect(healthMissingPos, healthMissingSize, Config.HealthMissingColor));
            }

            if (Config.ShieldConfig.Enabled)
            {
                float shield = Utils.ActorShieldValue(Actor);
                var shieldPos = Config.FillDirection.IsInverted() ? Config.Position : Config.Position + BarUtilities.GetFillDirectionOffset(healthFill.Size, Config.FillDirection);
                var shieldSize = Config.Size - BarUtilities.GetFillDirectionOffset(healthFill.Size, Config.FillDirection);
                Rect shieldFill = BarUtilities.GetFillRect(shieldPos, shieldSize, Config.FillDirection, Config.ShieldConfig.Color, shield, character.MaxHp, character.CurrentHp);
                bar.Foreground(shieldFill);
            }

            bar.Draw(pos);
        }

        private void DrawFriendlyNPC(Vector2 pos, GameObject? Actor)
        {
            var bar = new BarHud(ID, Config, Actor);
            bar.Foreground(new Rect(Config.Position, Config.Size, Config.UseJobColor? GlobalColors.Instance.NPCFriendlyColor : Config.FillColor));
            bar.Labels(Config.LeftLabelConfig, Config.RightLabelConfig);
            bar.Draw(pos);
        }

        private void DrawTankStanceIndicator(ImDrawListPtr drawList, Vector2 startPos)
        {
            if (Actor is not BattleChara battleChara || Config.TankStanceIndicatorConfig == null)
            {
                return;
            }

            var tankStanceBuff = battleChara.StatusList.Where(
                o => o.StatusId is 79 or 91 or 392 or 393 or 743 or 1396 or 1397 or 1833
            );

            var thickness = Config.TankStanceIndicatorConfig.Thickness + 1;
            var barSize = new Vector2(Config.Size.Y > Config.Size.X ? Config.Size.X : Config.Size.Y, Config.Size.Y);
            var cursorPos = startPos + new Vector2(-thickness, thickness);

            var color = !tankStanceBuff.Any() ? Config.TankStanceIndicatorConfig.InactiveColor : Config.TankStanceIndicatorConfig.ActiveColor;

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, color.Base);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
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
