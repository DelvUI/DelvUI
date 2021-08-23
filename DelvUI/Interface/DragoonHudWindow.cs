using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui;
using Dalamud.Interface;
using Dalamud.Plugin;
using ImGuiNET;
using DelvUI.Config;

namespace DelvUI.Interface
{
    public class DragoonHudWindow : HudWindow
    {
        public override uint JobId => Jobs.DRG;

        protected new int XOffset => PluginConfiguration.DRGBaseXOffset;
        protected new int YOffset => PluginConfiguration.DRGBaseYOffset;
        protected int ChaosThrustBarWidth => PluginConfiguration.DRGChaosThrustBarWidth;
        protected int ChaosThrustBarHeight => PluginConfiguration.DRGChaosThrustBarHeight;
        protected int ChaosThrustXOffset => PluginConfiguration.DRGChaosThrustXOffset;
        protected int ChaosThrustYOffset => PluginConfiguration.DRGChaosThrustYOffset;
        protected int DisembowelBarWidth => PluginConfiguration.DRGDisembowelBarWidth;
        protected int DisembowelBarHeight => PluginConfiguration.DRGDisembowelBarHeight;
        protected int DisembowelXOffset => PluginConfiguration.DRGDisembowelBarXOffset;
        protected int DisembowelYOffset => PluginConfiguration.DRGDisembowelBarYOffset;
        protected int EyeOfTheDragonBarHeight => PluginConfiguration.DRGEyeOfTheDragonHeight;
        protected int EyeOfTheDragonBarWidth => PluginConfiguration.DRGEyeOfTheDragonBarWidth;
        protected int EyeOfTheDragonPadding => PluginConfiguration.DRGEyeOfTheDragonPadding;
        protected int EyeOfTheDragonXOffset => PluginConfiguration.DRGEyeOfTheDragonXOffset;
        protected int EyeOfTheDragonYOffset => PluginConfiguration.DRGEyeOfTheDragonYOffset;
        protected int BloodBarWidth => PluginConfiguration.DRGBloodBarWidth;
        protected int BloodBarHeight => PluginConfiguration.DRGBloodBarHeight;
        protected int BloodBarXOffset => PluginConfiguration.DRGBloodBarXOffset;
        protected int BloodBarYOffset => PluginConfiguration.DRGBloodBarYOffset;
        protected bool ShowChaosThrustTimer => PluginConfiguration.DRGShowChaosThrustTimer;
        protected bool ShowDisembowelTimer => PluginConfiguration.DRGShowDisembowelBuffTimer;
        protected bool ShowEyeOfTheDragon => PluginConfiguration.DRGShowEyeOfTheDragon;
        protected bool ShowBloodBar => PluginConfiguration.DRGShowBloodBar;
        protected bool ShowChaosThrustText => PluginConfiguration.DRGShowChaosThrustText;
        protected bool ShowBloodText => PluginConfiguration.DRGShowBloodText;
        protected bool ShowDisembowelText => PluginConfiguration.DRGShowDisembowelText;
        protected Dictionary<string, uint> EyeOfTheDragonColor => PluginConfiguration.JobColorMap[Jobs.DRG * 1000];
        protected Dictionary<string, uint> BloodOfTheDragonColor => PluginConfiguration.JobColorMap[Jobs.DRG * 1000 + 1];
        protected Dictionary<string, uint> LifeOfTheDragonColor => PluginConfiguration.JobColorMap[Jobs.DRG * 1000 + 2];
        protected Dictionary<string, uint> DisembowelColor => PluginConfiguration.JobColorMap[Jobs.DRG * 1000 + 3];
        protected Dictionary<string, uint> ChaosThrustColor => PluginConfiguration.JobColorMap[Jobs.DRG * 1000 + 4];
        protected Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.DRG * 1000 + 5];

        public DragoonHudWindow(
            ClientState clientState,
            DalamudPluginInterface pluginInterface,
            DataManager dataManager,
            Framework framework,
            GameGui gameGui,
            JobGauges jobGauges,
            ObjectTable objectTable, 
            PluginConfiguration pluginConfiguration,
            SigScanner sigScanner,
            TargetManager targetManager,
            UiBuilder uiBuilder
        ) : base(
            clientState,
            pluginInterface,
            dataManager,
            framework,
            gameGui,
            jobGauges,
            objectTable,
            pluginConfiguration,
            sigScanner,
            targetManager,
            uiBuilder
        ) { }

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

        // Never draw the mana bar for Dragoons as it's useless.
        protected override void DrawPrimaryResourceBar() {
        }

        private void DrawChaosThrustBar() {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var actor = TargetManager.SoftTarget ?? TargetManager.Target;
            var scale = 0f;
            var duration = 0;

            if (actor is BattleChara target) {
                var chaosThrust = target.StatusList.FirstOrDefault(o => o.StatusId is 1312 or 118 && o.SourceID == ClientState.LocalPlayer.ObjectId);
                scale = chaosThrust?.RemainingTime ?? 0f / 24f;
                duration = (int) Math.Round(chaosThrust?.RemainingTime ?? 0f);
                if (scale < 0f) {
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
                cursorPos, cursorPos + chaosThrustBarSize,
                ChaosThrustColor["gradientLeft"], ChaosThrustColor["gradientRight"], ChaosThrustColor["gradientRight"], ChaosThrustColor["gradientLeft"]);

            if (ShowChaosThrustText && duration > 0f) {
                var durationText = duration.ToString();
                var textSize = ImGui.CalcTextSize(durationText);
                DrawOutlinedText(duration.ToString(), new Vector2(cursorPos.X + 5f, cursorPos.Y + ChaosThrustBarHeight / 2f - textSize.Y / 2f));
            }
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }

        private void DrawEyeOfTheDragonBars() {
            var gauge = JobGauges.Get<DRGGauge>();

            var barSize = new Vector2(EyeOfTheDragonBarWidth, EyeOfTheDragonBarHeight);
            var xPos = CenterX - XOffset + EyeOfTheDragonXOffset;
            var yPos = CenterY + YOffset + EyeOfTheDragonYOffset;
            var cursorPos = new Vector2(xPos, yPos);
            var eyeCount = gauge.EyeCount;
            var drawList = ImGui.GetWindowDrawList();

            for (byte i = 0; i < 2; i++) {
                cursorPos = new Vector2(cursorPos.X + (EyeOfTheDragonBarWidth + EyeOfTheDragonPadding) * i, cursorPos.Y);
                if (eyeCount >= i + 1) {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + barSize,
                        EyeOfTheDragonColor["gradientLeft"], EyeOfTheDragonColor["gradientRight"], EyeOfTheDragonColor["gradientRight"], EyeOfTheDragonColor["gradientLeft"]
                    );
                }
                else {
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, EmptyColor["background"]);
                }
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            }
        }

        private void DrawBloodOfTheDragonBar() {
            var gauge = JobGauges.Get<DRGGauge>();

            var xPos = CenterX - XOffset + BloodBarXOffset;
            var yPos = CenterY + YOffset + BloodBarYOffset;
            var cursorPos = new Vector2(xPos, yPos);
            var barSize = new Vector2(BloodBarWidth, BloodBarHeight);

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, EmptyColor["background"]);

            const int maxTimerMs = 30 * 1000;
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
                    cursorPos, cursorPos + botdBarSize,
                    LifeOfTheDragonColor["gradientLeft"], LifeOfTheDragonColor["gradientRight"], LifeOfTheDragonColor["gradientRight"], LifeOfTheDragonColor["gradientLeft"]);
            }
            else {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + botdBarSize,
                    BloodOfTheDragonColor["gradientLeft"], BloodOfTheDragonColor["gradientRight"], BloodOfTheDragonColor["gradientRight"], BloodOfTheDragonColor["gradientLeft"]);
            }
            
            if (ShowBloodText) {
                var durationText = ((int)(currTimerMs / 1000f)).ToString();
                var textSize = ImGui.CalcTextSize(durationText);
                DrawOutlinedText(durationText, new Vector2(cursorPos.X + 5f, cursorPos.Y + BloodBarHeight / 2f - textSize.Y / 2f));
            }
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }

        private void DrawDisembowelBar() {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var xPos = CenterX - XOffset + DisembowelXOffset;
            var yPos = CenterY + YOffset + DisembowelYOffset;
            var barSize = new Vector2(DisembowelBarWidth, DisembowelBarHeight);
            var cursorPos = new Vector2(xPos, yPos);
            var drawList = ImGui.GetWindowDrawList();
            var disembowelBuff = ClientState.LocalPlayer.StatusList.Where(o => o.StatusId is 1914 or 121);
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, EmptyColor["background"]);

            if (disembowelBuff.Count() == 0)
            {
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                return;
            }
            
            var buff = disembowelBuff.First();
            if (buff.RemainingTime <= 0)
            {
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                return;
            }

            var scale = buff.RemainingTime / 30f;
            var disembowelBarSize = new Vector2(DisembowelBarWidth * scale, DisembowelBarHeight);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + disembowelBarSize,
                DisembowelColor["gradientLeft"], DisembowelColor["gradientRight"], DisembowelColor["gradientRight"], DisembowelColor["gradientLeft"]);

            if (ShowDisembowelText) {
                var durationText = ((int)buff.RemainingTime).ToString();
                var textSize = ImGui.CalcTextSize(durationText);
                DrawOutlinedText(durationText, new Vector2(cursorPos.X + 5f, cursorPos.Y + BloodBarHeight / 2f - textSize.Y / 2f));
            }
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }
    }
}