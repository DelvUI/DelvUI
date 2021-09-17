using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.Internal.Gui.Addon;
using DelvUI.Helpers;
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

        private ImGuiWindowFlags _childFlags = 0;
        private readonly OpenContextMenuFromTarget _openContextMenuFromTarget;

        public Actor Actor { get; set; } = null;

        public UnitFrameHud(string id, UnitFrameConfig config, string displayName) : base(id, config, displayName)
        {
            // labels
            _leftLabel = new LabelHud(id + "_leftLabel", Config.LeftLabelConfig);
            _rightLabel = new LabelHud(id + "_rightLabel", Config.RightLabelConfig);

            // interaction stuff
            _openContextMenuFromTarget =
                Marshal.GetDelegateForFunctionPointer<OpenContextMenuFromTarget>(Plugin.SigScanner.ScanText("48 85 D2 74 7F 48 89 5C 24"));

            _childFlags |= ImGuiWindowFlags.NoTitleBar;
            _childFlags |= ImGuiWindowFlags.NoScrollbar;
            _childFlags |= ImGuiWindowFlags.AlwaysAutoResize;
            _childFlags |= ImGuiWindowFlags.NoBackground;
            _childFlags |= ImGuiWindowFlags.NoBringToFrontOnFocus;
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

            ImGuiWindowFlags windowFlags = 0;
            windowFlags |= ImGuiWindowFlags.NoBackground;
            windowFlags |= ImGuiWindowFlags.NoTitleBar;
            windowFlags |= ImGuiWindowFlags.NoMove;
            windowFlags |= ImGuiWindowFlags.NoDecoration;
            windowFlags |= ImGuiWindowFlags.NoInputs;

            var startPos = origin + Config.Position - Config.Size / 2f;
            var endPos = startPos + Config.Size;

            var drawList = ImGui.GetWindowDrawList();
            var addon = Plugin.GameGui.GetAddonByName("ContextMenu", 1);

            DrawHelper.ClipAround(addon, ID, drawList, (drawListPtr, windowName) =>
            {
                ImGui.SetNextWindowPos(startPos);
                ImGui.SetNextWindowSize(Config.Size);

                ImGui.Begin(windowName, windowFlags);

                UpdateChildFlags(addon);

                if (ImGui.BeginChild(windowName, Config.Size, default, _childFlags))
                {
                    // health bar
                    if (Actor is not Chara)
                    {
                        DrawFriendlyNPC(drawListPtr, startPos, endPos);
                    }
                    else
                    {
                        DrawChara(drawListPtr, origin, (Chara)Actor);
                    }

                    // Check if mouse is hovering over the box properly
                    if (ImGui.IsMouseHoveringRect(startPos, endPos) && !DraggingEnabled)
                    {
                        if (ImGui.GetIO().MouseClicked[0])
                        {
                            Plugin.TargetManager.SetCurrentTarget(Actor);
                        }
                        else if (ImGui.GetIO().MouseClicked[1])
                        {
                            var agentHud = new IntPtr(Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalID(4));
                            _openContextMenuFromTarget(agentHud, Actor.Address);
                        }
                    }
                }

                ImGui.EndChild();
                ImGui.End();

                // labels
                _leftLabel.Draw(origin + Config.Position, Config.Size, Actor);
                _rightLabel.Draw(origin + Config.Position, Config.Size, Actor);
            });
        }

        private void UpdateChildFlags(Addon addon)
        {
            if (addon is not { Visible: true })
            {
                _childFlags &= ~ImGuiWindowFlags.NoInputs;
            }
            else
            {
                if (ImGui.IsMouseHoveringRect(new Vector2(addon.X, addon.Y), new Vector2(addon.X + addon.Width, addon.Y + addon.Height)))
                {
                    _childFlags |= ImGuiWindowFlags.NoInputs;
                }
                else
                {
                    _childFlags &= ~ImGuiWindowFlags.NoInputs;
                }
            }
        }

        private void DrawChara(ImDrawListPtr drawList, Vector2 origin, Chara chara)
        {
            if (Config.TankStanceIndicatorConfig != null && Config.TankStanceIndicatorConfig.Enabled && JobsHelper.IsJobTank(chara.ClassJob.Id))
            {
                DrawTankStanceIndicator(drawList, origin);
            }

            var startPos = new Vector2(origin.X + Config.Position.X - Config.Size.X / 2f, origin.Y + Config.Position.Y - Config.Size.Y / 2f);
            var endPos = startPos + Config.Size;
            var scale = (float)chara.CurrentHp / Math.Max(1, chara.MaxHp);
            var color = Config.UseCustomColor ? Config.CustomColor : Utils.ColorForActor(chara);
            var bgColor = BackgroundColor(chara);

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
            var color = GlobalColors.Instance.NPCFriendlyColor;

            drawList.AddRectFilled(startPos, endPos, GlobalColors.Instance.EmptyUnitFrameColor.Base);

            drawList.AddRectFilledMultiColor(
                startPos,
                endPos,
                color.TopGradient,
                color.TopGradient,
                color.BottomGradient,
                color.BottomGradient
            );

            drawList.AddRect(startPos, endPos, 0xFF000000);
        }

        private void DrawTankStanceIndicator(ImDrawListPtr drawList, Vector2 origin)
        {
            var tankStanceBuff = Actor.StatusEffects.Where(
                o => o.EffectId == 79 ||    // IRON WILL
                     o.EffectId == 91 ||    // DEFIANCE
                     o.EffectId == 392 ||   // ROYAL GUARD
                     o.EffectId == 393 ||   // IRON WILL
                     o.EffectId == 743 ||   // GRIT
                     o.EffectId == 1396 ||  // DEFIANCE
                     o.EffectId == 1397 ||  // GRIT
                     o.EffectId == 1833     // ROYAL GUARD
            );

            var thickness = Config.TankStanceIndicatorConfig.Thickness + 1;
            var barSize = new Vector2(Config.Size.Y > Config.Size.X ? Config.Size.X : Config.Size.Y, Config.Size.Y);
            var cursorPos = new Vector2(
                origin.X + Config.Position.X - Config.Size.X / 2f - thickness,
                origin.Y + Config.Position.Y - Config.Size.Y / 2f + thickness
            );

            var color = tankStanceBuff.Count() <= 0 ? Config.TankStanceIndicatorConfig.UnactiveColor : Config.TankStanceIndicatorConfig.ActiveColor;

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, color.Base);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }

        private uint BackgroundColor(Chara chara)
        {
            if (Config.ShowTankInvulnerability && Utils.HasTankInvulnerability(chara))
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

            if (Config.UseCustomBackgroundColor)
            {
                return Config.CustomBackgroundColor.Base;
            }

            return GlobalColors.Instance.EmptyUnitFrameColor.Base;
        }

        private delegate void OpenContextMenuFromTarget(IntPtr agentHud, IntPtr gameObject);
    }
}
