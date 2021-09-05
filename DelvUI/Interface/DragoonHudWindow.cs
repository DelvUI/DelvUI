using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Config;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface
{
    public class DragoonHudWindow : HudWindow
    {

        public DragoonHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        public override uint JobId => Jobs.DRG;
        private new int XOffset => PluginConfiguration.DRGBaseXOffset;
        private new int YOffset => PluginConfiguration.DRGBaseYOffset;
        private int ChaosThrustBarWidth => PluginConfiguration.DRGChaosThrustBarWidth;
        private int ChaosThrustBarHeight => PluginConfiguration.DRGChaosThrustBarHeight;
        private int ChaosThrustXOffset => PluginConfiguration.DRGChaosThrustXOffset;
        private int ChaosThrustYOffset => PluginConfiguration.DRGChaosThrustYOffset;
        private int DisembowelBarWidth => PluginConfiguration.DRGDisembowelBarWidth;
        private int DisembowelBarHeight => PluginConfiguration.DRGDisembowelBarHeight;
        private int DisembowelXOffset => PluginConfiguration.DRGDisembowelBarXOffset;
        private int DisembowelYOffset => PluginConfiguration.DRGDisembowelBarYOffset;
        private int EyeOfTheDragonBarHeight => PluginConfiguration.DRGEyeOfTheDragonHeight;
        private int EyeOfTheDragonBarWidth => PluginConfiguration.DRGEyeOfTheDragonBarWidth;
        private int EyeOfTheDragonPadding => PluginConfiguration.DRGEyeOfTheDragonPadding;
        private int EyeOfTheDragonXOffset => PluginConfiguration.DRGEyeOfTheDragonXOffset;
        private int EyeOfTheDragonYOffset => PluginConfiguration.DRGEyeOfTheDragonYOffset;
        private int BloodBarWidth => PluginConfiguration.DRGBloodBarWidth;
        private int BloodBarHeight => PluginConfiguration.DRGBloodBarHeight;
        private int BloodBarXOffset => PluginConfiguration.DRGBloodBarXOffset;
        private int BloodBarYOffset => PluginConfiguration.DRGBloodBarYOffset;
        private bool ShowChaosThrustTimer => PluginConfiguration.DRGShowChaosThrustTimer;
        private bool ShowDisembowelTimer => PluginConfiguration.DRGShowDisembowelBuffTimer;
        private bool ShowEyeOfTheDragon => PluginConfiguration.DRGShowEyeOfTheDragon;
        private bool ShowBloodBar => PluginConfiguration.DRGShowBloodBar;
        private bool ShowChaosThrustText => PluginConfiguration.DRGShowChaosThrustText;
        private bool ShowBloodText => PluginConfiguration.DRGShowBloodText;
        private bool ShowDisembowelText => PluginConfiguration.DRGShowDisembowelText;
        private Dictionary<string, uint> EyeOfTheDragonColor => PluginConfiguration.JobColorMap[Jobs.DRG * 1000];
        private Dictionary<string, uint> BloodOfTheDragonColor => PluginConfiguration.JobColorMap[Jobs.DRG * 1000 + 1];
        private Dictionary<string, uint> LifeOfTheDragonColor => PluginConfiguration.JobColorMap[Jobs.DRG * 1000 + 2];
        private Dictionary<string, uint> DisembowelColor => PluginConfiguration.JobColorMap[Jobs.DRG * 1000 + 3];
        private Dictionary<string, uint> ChaosThrustColor => PluginConfiguration.JobColorMap[Jobs.DRG * 1000 + 4];
        private Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.DRG * 1000 + 5];
        protected override List<uint> GetJobSpecificBuffs()
        {
            uint[] ids = { 
                // Dive Ready
                1243,
                // Life Surge
                116,
                2175,
                // Lance Charge
                1864,
                // Right Eye
                1183,
                1453,
                1910,
                // Disembowel
                121,
                1914,
            };
            return new List<uint>(ids);
        }

        protected override void Draw(bool _)
        {
            if (ShowChaosThrustTimer)
            {
                DrawChaosThrustBar();
            }

            if (ShowDisembowelTimer)
            {
                DrawDisembowelBar();
            }

            if (ShowEyeOfTheDragon)
            {
                DrawEyeOfTheDragonBars();
            }

            if (ShowBloodBar)
            {
                DrawBloodOfTheDragonBar();
            }
        }

        protected override void DrawPrimaryResourceBar()
        {
            // Never draw the mana bar for Dragoons as it's useless.
        }

        private void DrawChaosThrustBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;
            var scale = 0f;
            var duration = 0;

            if (target is Chara)
            {
                var chaosThrust = target.StatusEffects.FirstOrDefault(
                    o => (o.EffectId == 1312 || o.EffectId == 118) && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                );

                scale = chaosThrust.Duration / 24f;
                duration = (int)Math.Round(chaosThrust.Duration);

                if (scale < 0f)
                {
                    scale = 0f;
                    duration = 0;
                }
            }

            var barSize = new Vector2(ChaosThrustBarWidth, ChaosThrustBarHeight);
            var xPos = CenterX - XOffset + ChaosThrustXOffset;
            var yPos = CenterY + YOffset + ChaosThrustYOffset;
            var cursorPos = new Vector2(xPos, yPos);
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, EmptyColor["background"]);
            var chaosThrustBarSize = new Vector2(ChaosThrustBarWidth * scale, ChaosThrustBarHeight);

            drawList.AddRectFilledMultiColor(
                cursorPos,
                cursorPos + chaosThrustBarSize,
                ChaosThrustColor["gradientLeft"],
                ChaosThrustColor["gradientRight"],
                ChaosThrustColor["gradientRight"],
                ChaosThrustColor["gradientLeft"]
            );

            if (ShowChaosThrustText && duration > 0f)
            {
                var durationText = duration.ToString();
                var textSize = ImGui.CalcTextSize(durationText);
                DrawOutlinedText(duration.ToString(), new Vector2(cursorPos.X + 5f, cursorPos.Y + ChaosThrustBarHeight / 2f - textSize.Y / 2f));
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }

        private void DrawEyeOfTheDragonBars()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<DRGGauge>();

            var barSize = new Vector2(EyeOfTheDragonBarWidth, EyeOfTheDragonBarHeight);
            var xPos = CenterX - XOffset + EyeOfTheDragonXOffset;
            var yPos = CenterY + YOffset + EyeOfTheDragonYOffset;
            var cursorPos = new Vector2(xPos, yPos);
            var eyeCount = gauge.EyeCount;
            var drawList = ImGui.GetWindowDrawList();

            for (byte i = 0; i < 2; i++)
            {
                cursorPos = new Vector2(cursorPos.X + (EyeOfTheDragonBarWidth + EyeOfTheDragonPadding) * i, cursorPos.Y);

                if (eyeCount >= i + 1)
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos,
                        cursorPos + barSize,
                        EyeOfTheDragonColor["gradientLeft"],
                        EyeOfTheDragonColor["gradientRight"],
                        EyeOfTheDragonColor["gradientRight"],
                        EyeOfTheDragonColor["gradientLeft"]
                    );
                }
                else
                {
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, EmptyColor["background"]);
                }

                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            }
        }

        private void DrawBloodOfTheDragonBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<DRGGauge>();

            var xPos = CenterX - XOffset + BloodBarXOffset;
            var yPos = CenterY + YOffset + BloodBarYOffset;
            var cursorPos = new Vector2(xPos, yPos);
            var barSize = new Vector2(BloodBarWidth, BloodBarHeight);

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, EmptyColor["background"]);

            var maxTimerMs = 30 * 1000;
            var currTimerMs = gauge.BOTDTimer;

            if (currTimerMs == 0)
            {
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                return;
            }

            var scale = (float)currTimerMs / maxTimerMs;
            var botdBarSize = new Vector2(BloodBarWidth * scale, BloodBarHeight);

            if (gauge.BOTDState == BOTDState.LOTD)
            {
                drawList.AddRectFilledMultiColor(
                    cursorPos,
                    cursorPos + botdBarSize,
                    LifeOfTheDragonColor["gradientLeft"],
                    LifeOfTheDragonColor["gradientRight"],
                    LifeOfTheDragonColor["gradientRight"],
                    LifeOfTheDragonColor["gradientLeft"]
                );
            }
            else
            {
                drawList.AddRectFilledMultiColor(
                    cursorPos,
                    cursorPos + botdBarSize,
                    BloodOfTheDragonColor["gradientLeft"],
                    BloodOfTheDragonColor["gradientRight"],
                    BloodOfTheDragonColor["gradientRight"],
                    BloodOfTheDragonColor["gradientLeft"]
                );
            }

            if (ShowBloodText)
            {
                var durationText = ((int)(currTimerMs / 1000f)).ToString();
                var textSize = ImGui.CalcTextSize(durationText);
                DrawOutlinedText(durationText, new Vector2(cursorPos.X + 5f, cursorPos.Y + BloodBarHeight / 2f - textSize.Y / 2f));
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }

        private void DrawDisembowelBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var xPos = CenterX - XOffset + DisembowelXOffset;
            var yPos = CenterY + YOffset + DisembowelYOffset;
            var barSize = new Vector2(DisembowelBarWidth, DisembowelBarHeight);
            var cursorPos = new Vector2(xPos, yPos);
            var drawList = ImGui.GetWindowDrawList();
            var disembowelBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1914 || o.EffectId == 121);
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, EmptyColor["background"]);

            if (disembowelBuff.Count() == 0)
            {
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                return;
            }

            var buff = disembowelBuff.First();

            if (buff.Duration <= 0)
            {
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                return;
            }

            var scale = buff.Duration / 30f;
            var disembowelBarSize = new Vector2(DisembowelBarWidth * scale, DisembowelBarHeight);

            drawList.AddRectFilledMultiColor(
                cursorPos,
                cursorPos + disembowelBarSize,
                DisembowelColor["gradientLeft"],
                DisembowelColor["gradientRight"],
                DisembowelColor["gradientRight"],
                DisembowelColor["gradientLeft"]
            );

            if (ShowDisembowelText)
            {
                var durationText = ((int)buff.Duration).ToString();
                var textSize = ImGui.CalcTextSize(durationText);
                DrawOutlinedText(durationText, new Vector2(cursorPos.X + 5f, cursorPos.Y + BloodBarHeight / 2f - textSize.Y / 2f));
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }
    }
}
