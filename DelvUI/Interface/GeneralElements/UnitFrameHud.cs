﻿using DelvUI.Helpers;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Objects.Types;
using static System.Globalization.CultureInfo;

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

            var startPos = Utils.GetAnchoredPosition(origin + Config.Position, Config.Size, Config.Anchor);
            var endPos = startPos + Config.Size;

            DrawHelper.DrawInWindow(ID, startPos, Config.Size, true, false, (drawList) =>
            {
                // health bar
                if (Actor is not Character)
                {
                    DrawFriendlyNPC(drawList, startPos, endPos);
                }
                else
                {
                    DrawChara(drawList, startPos, (Character)Actor);
                }

                // Check if mouse is hovering over the box properly
                if (ImGui.IsMouseHoveringRect(startPos, endPos) && !DraggingEnabled)
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
            });

            // labels            
            string actorName = Actor.Name.ToString().CheckForUpperCase();
            _leftLabel.Draw(startPos, Config.Size, Actor, actorName);
            _rightLabel.Draw(startPos, Config.Size, Actor, actorName);
        }

        private void DrawChara(ImDrawListPtr drawList, Vector2 startPos, Character chara)
        {
            if (Config.TankStanceIndicatorConfig is { Enabled: true } && JobsHelper.IsJobTank(chara.ClassJob.Id))
            {
                DrawTankStanceIndicator(drawList, startPos);
            }

            var endPos = startPos + Config.Size;
            var scale = (float)chara.CurrentHp / Math.Max(1, chara.MaxHp);
            var color = Config.UseCustomColor ? Config.CustomColor : Utils.ColorForActor(chara);
            var bgColor = BackgroundColor(chara);

            if (Config.UseCustomColor && Config.UseColorBasedOnHealthValue)
            {
                color = Utils.ColorByHealthValue(scale, Config.LowHealthColorThreshold / 100f, Config.FullHealthColorThreshold / 100f, Config.FullHealthColor, Config.LowHealthColor);
            }

            // background
            drawList.AddRectFilled(startPos, endPos, bgColor);

            // health
            DrawHelper.DrawGradientFilledRect(startPos, new Vector2(Config.Size.X * scale, Config.Size.Y), color, drawList);

            // shield
            if (Config.ShieldConfig.Enabled)
            {
                var shield = Utils.ActorShieldValue(Actor);

                if (Config.ShieldConfig.FillHealthFirst)
                {
                    DrawHelper.DrawShield(shield, scale, startPos, Config.Size,
                        Config.ShieldConfig.Height, Config.ShieldConfig.HeightInPixels, Config.ShieldConfig.Color, drawList);
                }
                else
                {
                    DrawHelper.DrawOvershield(shield, startPos, Config.Size,
                        Config.ShieldConfig.Height, Config.ShieldConfig.HeightInPixels, Config.ShieldConfig.Color, drawList);
                }
            }

            // border
            drawList.AddRect(startPos, endPos, 0xFF000000);
        }

        private void DrawFriendlyNPC(ImDrawListPtr drawList, Vector2 startPos, Vector2 endPos)
        {
            var color = Config.UseCustomColor ? Config.CustomColor : GlobalColors.Instance.NPCFriendlyColor;

            drawList.AddRectFilled(startPos, endPos, GlobalColors.Instance.EmptyUnitFrameColor.Base);

            DrawHelper.DrawGradientFilledRect(startPos, new Vector2(Config.Size.X, Config.Size.Y), color, drawList);

            drawList.AddRect(startPos, endPos, 0xFF000000);
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

        private uint BackgroundColor(Character? chara)
        {
            if (Config.ShowTankInvulnerability && chara is BattleChara battleChara && Utils.HasTankInvulnerability(battleChara))
            {
                uint color;
                if (Config.UseCustomInvulnerabilityColor)
                {
                    color = Config.CustomInvulnerabilityColor.Base;
                }
                else
                {
                    color = ImGui.ColorConvertFloat4ToU32(GlobalColors.Instance.SafeColorForJobId(chara.ClassJob.Id).Vector.AdjustColor(-.8f));
                }

                return color;
            }

            if (Config.UseCustomBackgroundColor && chara is BattleChara)
            {
                if (Config.UseJobColorAsBackgroundColor)
                {
                    return GlobalColors.Instance.SafeColorForJobId(chara.ClassJob.Id).Base;
                }
                else
                {
                    return Config.CustomBackgroundColor.Base;
                }
            }

            return GlobalColors.Instance.EmptyUnitFrameColor.Base;
        }

        private delegate void OpenContextMenuFromTarget(IntPtr agentHud, IntPtr gameObject);
    }
}
