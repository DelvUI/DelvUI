using Dalamud.Game.ClientState.Actors.Types;
using DelvUI.Config;
using DelvUI.Helpers;
using ImGuiNET;
using System;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    public unsafe class UnitFrameHud : HudElement, IHudElementWithActor
    {
        private PluginConfiguration _pluginConfiguration;

        private UnitFrameConfig Config => (UnitFrameConfig)_config;
        private LabelHud _leftLabel;
        private LabelHud _rightLabel;

        public Actor Actor { get; set; } = null;

        public UnitFrameHud(UnitFrameConfig config, PluginConfiguration pluginConfiguration) : base(config)
        {
            // NOTE: Temporary. Have to do this for now for job colors.
            // Ideally hud elements shouldna't need a reference to PluginConfiguration
            _pluginConfiguration = pluginConfiguration;

            _leftLabel = new LabelHud(Config.LeftLabelConfig);
            _rightLabel = new LabelHud(Config.RightLabelConfig);
        }

        public override void Draw(Vector2 origin)
        {
            if (!Config.Enabled || Actor == null)
            {
                return;
            }

            if (Actor is not Chara)
            {
                DrawFriendlyNPC(origin);
            }
            else
            {
                DrawChara(origin, (Chara)Actor);
            }

            _leftLabel.DrawWithActor(origin + Config.Position, Actor);
            _rightLabel.DrawWithActor(origin + Config.Position, Actor);
        }

        private void DrawChara(Vector2 origin, Chara chara)
        {
            var scale = (float)chara.CurrentHp / Math.Max(1, chara.MaxHp);

            if (Config.TankStanceIndicatorConfig != null && Config.TankStanceIndicatorConfig.Enabled && JobsHelper.IsJobTank(chara.ClassJob.Id))
            {
                DrawTankStanceIndicator(origin);
            }

            var startPos = new Vector2(origin.X + Config.Position.X - Config.Size.X / 2f, origin.Y + Config.Position.Y - Config.Size.Y / 2f);
            var endPos = startPos + Config.Size;
            var color = Config.UseCustomColor ? Config.CustomColor.Map : Utils.ColorForActor(_pluginConfiguration, chara);
            var bgColor = BackgroundColor(chara);

            var drawList = ImGui.GetWindowDrawList();

            // Basically make an invisible box for BeginChild to work properly.
            ImGuiWindowFlags windowFlags = 0;
            windowFlags |= ImGuiWindowFlags.NoBackground;
            windowFlags |= ImGuiWindowFlags.NoTitleBar;
            windowFlags |= ImGuiWindowFlags.NoMove;
            windowFlags |= ImGuiWindowFlags.NoDecoration;

            ImGui.SetNextWindowPos(startPos);
            ImGui.SetNextWindowSize(Config.Size);

            ImGui.Begin("health_bar", windowFlags);

            if (ImGui.BeginChild("health_bar", Config.Size))
            {
                // background
                drawList.AddRectFilled(startPos, endPos, bgColor);

                // health
                drawList.AddRectFilledMultiColor(
                    startPos,
                    endPos,
                    color["gradientLeft"],
                    color["gradientRight"],
                    color["gradientRight"],
                    color["gradientLeft"]
                );

                // border
                drawList.AddRect(startPos, endPos, 0xFF000000);

                // Check if mouse is hovering over the box properly
                if (ImGui.GetIO().MouseClicked[0] && ImGui.IsMouseHoveringRect(startPos, endPos))
                {
                    //PluginInterface.ClientState.Targets.SetCurrentTarget(actor);
                }
            }

            ImGui.EndChild();
            ImGui.End();


            // shield
            if (Config.ShieldConfig.Enabled)
            {
                var shield = Utils.ActorShieldValue(Actor);

                if (Config.ShieldConfig.FillHealthFirst)
                {
                    DrawHelper.DrawShield(shield, scale, startPos, endPos, Config.ShieldConfig.Height, Config.ShieldConfig.HeightInPixels, Config.ShieldConfig.Color.Map);
                }
                else
                {
                    DrawHelper.DrawOvershield(shield, startPos, endPos, Config.ShieldConfig.Height, Config.ShieldConfig.HeightInPixels, Config.ShieldConfig.Color.Map);
                }
            }
        }

        private void DrawFriendlyNPC(Vector2 origin)
        {
            var startPos = new Vector2(origin.X + Config.Position.X - Config.Size.X / 2f, origin.Y + Config.Position.Y - Config.Size.Y / 2f);
            var endPos = startPos + Config.Size;
            var color = _pluginConfiguration.NPCColorMap["friendly"];

            var drawList = ImGui.GetWindowDrawList();
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
    }
}
