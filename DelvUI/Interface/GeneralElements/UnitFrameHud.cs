using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.Internal.Gui.Addon;
using DelvUI.Config;
using DelvUI.Helpers;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using ImGuiNET;
using System;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DelvUI.Interface.GeneralElements
{
    public unsafe class UnitFrameHud : HudElement, IHudElementWithActor
    {
        private PluginConfiguration _pluginConfiguration;

        private UnitFrameConfig Config => (UnitFrameConfig)_config;
        private LabelHud _leftLabel;
        private LabelHud _rightLabel;

        private ImGuiWindowFlags _childFlags = 0;
        private readonly OpenContextMenuFromTarget _openContextMenuFromTarget;

        public Actor Actor { get; set; } = null;

        public UnitFrameHud(string id, UnitFrameConfig config, PluginConfiguration pluginConfiguration) : base(id, config)
        {
            // NOTE: Temporary. Have to do this for now for job colors.
            // Ideally hud elements shouldna't need a reference to PluginConfiguration
            _pluginConfiguration = pluginConfiguration;

            // labels
            _leftLabel = new LabelHud(id + "_leftLabel", Config.LeftLabelConfig);
            _rightLabel = new LabelHud(id + "_rightLabel", Config.RightLabelConfig);

            // interaction stuff
            _openContextMenuFromTarget =
                Marshal.GetDelegateForFunctionPointer<OpenContextMenuFromTarget>(Plugin.InterfaceInstance.TargetModuleScanner.ScanText("48 85 D2 74 7F 48 89 5C 24"));

            _childFlags |= ImGuiWindowFlags.NoTitleBar;
            _childFlags |= ImGuiWindowFlags.NoScrollbar;
            _childFlags |= ImGuiWindowFlags.AlwaysAutoResize;
            _childFlags |= ImGuiWindowFlags.NoBackground;
            _childFlags |= ImGuiWindowFlags.NoBringToFrontOnFocus;
        }

        public override void Draw(Vector2 origin)
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

            var startPos = new Vector2(origin.X + Config.Position.X - Config.Size.X / 2f, origin.Y + Config.Position.Y - Config.Size.Y / 2f);
            var endPos = startPos + Config.Size;

            var drawList = ImGui.GetWindowDrawList();
            var addon = Plugin.InterfaceInstance.Framework.Gui.GetAddonByName("ContextMenu", 1);

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
                    if (ImGui.IsMouseHoveringRect(startPos, endPos))
                    {
                        if (ImGui.GetIO().MouseClicked[0])
                        {
                            Plugin.InterfaceInstance.ClientState.Targets.SetCurrentTarget(Actor);
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
            });

            // labels
            _leftLabel.DrawWithActor(origin + Config.Position, Actor);
            _rightLabel.DrawWithActor(origin + Config.Position, Actor);
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
                DrawTankStanceIndicator(origin);
            }

            var startPos = new Vector2(origin.X + Config.Position.X - Config.Size.X / 2f, origin.Y + Config.Position.Y - Config.Size.Y / 2f);
            var endPos = startPos + Config.Size;
            var scale = (float)chara.CurrentHp / Math.Max(1, chara.MaxHp);
            var color = Config.UseCustomColor ? Config.CustomColor.Map : Utils.ColorForActor(_pluginConfiguration, chara);
            var bgColor = BackgroundColor(chara);

            // background
            drawList.AddRectFilled(startPos, endPos, bgColor);

            // health
            drawList.AddRectFilledMultiColor(
                startPos,
                startPos + new Vector2(Config.Size.X * scale, Config.Size.Y),
                color["gradientLeft"],
                color["gradientRight"],
                color["gradientRight"],
                color["gradientLeft"]
            );

            // border
            drawList.AddRect(startPos, endPos, 0xFF000000);

            // shield
            if (Config.ShieldConfig.Enabled)
            {
                var shield = Utils.ActorShieldValue(Actor);

                if (Config.ShieldConfig.FillHealthFirst)
                {
                    DrawHelper.DrawShield(shield, scale, startPos, endPos, 
                        Config.ShieldConfig.Height, Config.ShieldConfig.HeightInPixels, Config.ShieldConfig.Color.Map, drawList);
                }
                else
                {
                    DrawHelper.DrawOvershield(shield, startPos, endPos, 
                        Config.ShieldConfig.Height, Config.ShieldConfig.HeightInPixels, Config.ShieldConfig.Color.Map, drawList);
                }
            }
        }

        private void DrawFriendlyNPC(ImDrawListPtr drawList, Vector2 startPos, Vector2 endPos)
        {
            var color = _pluginConfiguration.NPCColorMap["friendly"];

            drawList.AddRectFilled(startPos, endPos, ImGui.ColorConvertFloat4ToU32(_pluginConfiguration.UnitFrameEmptyColor));

            drawList.AddRectFilledMultiColor(
                startPos,
                endPos,
                color["gradientLeft"],
                color["gradientRight"],
                color["gradientRight"],
                color["gradientLeft"]
            );

            drawList.AddRect(startPos, endPos, 0xFF000000);
        }

        private void DrawTankStanceIndicator(Vector2 origin)
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

            var vectorColor = tankStanceBuff.Count() <= 0 ? Config.TankStanceIndicatorConfig.UnactiveColor : Config.TankStanceIndicatorConfig.ActiveColor;
            var color = ImGui.ColorConvertFloat4ToU32(vectorColor);

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, color);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }

        private uint BackgroundColor(Chara chara)
        {
            if (Config.ShowTankInvulnerability && Utils.HasTankInvulnerability(chara))
            {
                return _pluginConfiguration.JobColorMap[chara.ClassJob.Id]["invuln"];
            }

            if (Config.UseCustomBackgroundColor)
            {
                return Config.CustomBackgroundColor.Base;
            }

            return ImGui.ColorConvertFloat4ToU32(_pluginConfiguration.UnitFrameEmptyColor);
        }

        private delegate void OpenContextMenuFromTarget(IntPtr agentHud, IntPtr gameObject);
    }
}
