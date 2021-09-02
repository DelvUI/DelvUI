using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;
using DelvUI.Helpers;

namespace DelvUI.Interface
{
    public class SamuraiHudWindow : HudWindow
    {

        public override uint JobId => 34;

        private int SamBuffsBarHeight => PluginConfiguration.SamBuffsBarHeight;
        private int SamBuffsBarWidth => PluginConfiguration.SamBuffsBarWidth;
        private int SamBuffsBarX => PluginConfiguration.SamBuffsBarX;
        private int SamBuffsBarY => PluginConfiguration.SamBuffsBarY;		
		
        private int SamHiganbanaBarHeight => PluginConfiguration.SamHiganbanaBarHeight;
        private int SamHiganbanaBarWidth => PluginConfiguration.SamHiganbanaBarWidth;
        private int SamHiganbanaBarX => PluginConfiguration.SamHiganbanaBarX;
        private int SamHiganbanaBarY => PluginConfiguration.SamHiganbanaBarY;
		
		private int SamKenkiBarHeight => PluginConfiguration.SamKenkiBarHeight;
        private int SamKenkiBarWidth => PluginConfiguration.SamKenkiBarWidth;
        private int SamKenkiBarX => PluginConfiguration.SamKenkiBarX;
        private int SamKenkiBarY => PluginConfiguration.SamKenkiBarY;
		
		private int SamMeditationBarHeight => PluginConfiguration.SamMeditationBarHeight;
        private int SamMeditationBarWidth => PluginConfiguration.SamMeditationBarWidth;
        private int SamMeditationBarX => PluginConfiguration.SamMeditationBarX;
        private int SamMeditationBarY => PluginConfiguration.SamMeditationBarY;
		
        private int SamSenBarHeight => PluginConfiguration.SamSenBarHeight;
        private int SamSenBarWidth => PluginConfiguration.SamSenBarWidth;
        private int SamSenBarX => PluginConfiguration.SamSenBarX;
        private int SamSenBarY => PluginConfiguration.SamSenBarY;
		
        private int SamTimeJinpuXOffset => PluginConfiguration.SamTimeJinpuXOffset;
        private int SamTimeJinpuYOffset => PluginConfiguration.SamTimeJinpuYOffset;

        private int SamTimeShifuXOffset => PluginConfiguration.SamTimeShifuXOffset;
        private int SamTimeShifuYOffset => PluginConfiguration.SamTimeShifuYOffset;
		
        private int BuffsPadding => PluginConfiguration.SAMBuffsPadding;
        private int MeditationPadding => PluginConfiguration.SAMMeditationPadding;
        private int SenPadding => PluginConfiguration.SAMSenPadding;
		
        private bool BuffsEnabled => PluginConfiguration.SAMBuffsEnabled;
        private bool GaugeEnabled => PluginConfiguration.SAMGaugeEnabled;
        private bool HiganbanaEnabled => PluginConfiguration.SAMHiganbanaEnabled;
        private bool MeditationEnabled => PluginConfiguration.SAMMeditationEnabled;
        private bool SenEnabled => PluginConfiguration.SAMSenEnabled;

        private bool HiganbanaText => PluginConfiguration.SAMHiganbanaText;
        private bool KenkiText => PluginConfiguration.SAMKenkiText;
        private bool BuffText => PluginConfiguration.SAMBuffText;

        private Dictionary<string, uint> SamHiganbanaColor => PluginConfiguration.JobColorMap[Jobs.SAM * 1000];
        private Dictionary<string, uint> SamShifuColor => PluginConfiguration.JobColorMap[Jobs.SAM * 1000 + 1];
        private Dictionary<string, uint> SamJinpuColor => PluginConfiguration.JobColorMap[Jobs.SAM * 1000 + 2];
        private Dictionary<string, uint> SamSetsuColor => PluginConfiguration.JobColorMap[Jobs.SAM * 1000 + 3];
        private Dictionary<string, uint> SamGetsuColor => PluginConfiguration.JobColorMap[Jobs.SAM * 1000 + 4];
        private Dictionary<string, uint> SamKaColor => PluginConfiguration.JobColorMap[Jobs.SAM * 1000 + 5];
        private Dictionary<string, uint> SamMeditationColor => PluginConfiguration.JobColorMap[Jobs.SAM * 1000 + 6];
        private Dictionary<string, uint> SamKenkiColor => PluginConfiguration.JobColorMap[Jobs.SAM * 1000 + 7];
        private Dictionary<string, uint> SamEmptyColor => PluginConfiguration.JobColorMap[Jobs.SAM * 1000 + 8]; 
        private Dictionary<string, uint> SamExpiryColor => PluginConfiguration.JobColorMap[Jobs.SAM * 1000 + 9];




        public SamuraiHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _)
        {
            if (GaugeEnabled) {
                DrawKenkiBar();
            }
            if (SenEnabled) {
                DrawSenResourceBar();
            }
            if (MeditationEnabled) {
                DrawMeditationResourceBar();
            }
            if (HiganbanaEnabled) {
                DrawHiganbanaBar();
            }
            if (BuffsEnabled) {
                DrawActiveBuffs();
            }
        }
        protected override void DrawPrimaryResourceBar() {
        }
        private void DrawKenkiBar()
        {
            if (!GaugeEnabled) {
                return;
            }

            var gauge = PluginInterface.ClientState.JobGauges.Get<SAMGauge>();


            var xPos = CenterX - SamKenkiBarX;
            var yPos = CenterY + SamKenkiBarY;
            var cursorPos = new Vector2(xPos, yPos);
            const int chunkSize = 100;
            var barSize = new Vector2(SamKenkiBarWidth, SamKenkiBarHeight);

            // Kenki Gauge
            var kenki = Math.Min((int)gauge.Kenki, chunkSize);
            var scale = (float)kenki / chunkSize;
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, SamEmptyColor["background"]);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(SamKenkiBarWidth * scale, SamKenkiBarHeight),
                SamKenkiColor["gradientLeft"], SamKenkiColor["gradientRight"], SamKenkiColor["gradientRight"], SamKenkiColor["gradientRight"]
            );

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            var textSize = ImGui.CalcTextSize(gauge.Kenki.ToString());
            if(KenkiText)
                DrawOutlinedText(gauge.Kenki.ToString(), new Vector2(cursorPos.X + SamKenkiBarWidth / 2f - textSize.X / 2f, cursorPos.Y + (barSize.Y / 2) - 12));
        }

        private void DrawHiganbanaBar()
        {
            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;

            if (target is not Chara) {
                return;
            }

            var actorId = PluginInterface.ClientState.LocalPlayer.ActorId;
            var higanbana = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1228 && o.OwnerId == actorId || o.EffectId == 1319 && o.OwnerId == actorId);
            var higanbanaDuration = higanbana.Duration;

            var higanbanaColor = higanbanaDuration > 5 ? SamHiganbanaColor["base"] : SamExpiryColor["base"];

            var xOffset = CenterX - SamHiganbanaBarX;
            var yOffset = CenterY + SamHiganbanaBarY;
            var cursorPos = new Vector2(xOffset, yOffset);
            var barSize = new Vector2(SamHiganbanaBarWidth, SamHiganbanaBarHeight);
            var drawList = ImGui.GetWindowDrawList();

            var dotStart = new Vector2(xOffset + SamHiganbanaBarWidth - (barSize.X / 60) * higanbanaDuration, yOffset);

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, SamEmptyColor["background"]);
            drawList.AddRectFilled(dotStart, cursorPos + new Vector2(barSize.X, barSize.Y), higanbanaColor);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            var textSize = ImGui.CalcTextSize(Math.Round(higanbanaDuration).ToString(CultureInfo.InvariantCulture));

            if (HiganbanaText) {
                DrawOutlinedText(Math.Round(higanbanaDuration).ToString(CultureInfo.InvariantCulture), new Vector2(cursorPos.X + SamHiganbanaBarWidth / 2f - textSize.X / 2f, cursorPos.Y + (barSize.Y / 2) - 12));
            }
        }

        private void DrawActiveBuffs()
        {
            var target = PluginInterface.ClientState.LocalPlayer;

            var buffsBarWidth = (SamBuffsBarWidth / 2) - 1;
            var shifu = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1299);
            var jinpu = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1298);

            var shifuDuration = shifu.Duration;
            var jinpuDuration = jinpu.Duration;

            var xOffset = CenterX - SamBuffsBarX;
            var cursorPos = new Vector2(CenterX - SamBuffsBarX, CenterY + SamBuffsBarY);
            var barSize = new Vector2(buffsBarWidth, SamBuffsBarHeight);
            var drawList = ImGui.GetWindowDrawList();
            var shifuXOffset = CenterX - SamTimeShifuXOffset;
            var shifuYOffset = CenterY + SamTimeShifuYOffset;
            var shifuTextSize = ImGui.CalcTextSize(Math.Round(shifuDuration).ToString(CultureInfo.InvariantCulture));
            var jinpuTextSize = ImGui.CalcTextSize(Math.Round(jinpuDuration).ToString(CultureInfo.InvariantCulture));

            var buffStart = new Vector2(xOffset + buffsBarWidth - (barSize.X / 40) * shifuDuration, CenterY + SamBuffsBarY);

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, SamEmptyColor["background"]);
            drawList.AddRectFilledMultiColor(
                    buffStart, cursorPos + new Vector2(barSize.X, barSize.Y),
                    SamShifuColor["gradientLeft"], SamShifuColor["gradientRight"], SamShifuColor["gradientRight"], SamShifuColor["gradientLeft"]
                );

            if (!PluginConfiguration.ShowBuffTime)
            {
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                cursorPos = new Vector2(cursorPos.X + buffsBarWidth + BuffsPadding, cursorPos.Y);

                drawList.AddRectFilled(cursorPos, cursorPos + barSize, SamEmptyColor["background"]);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2((barSize.X / 30) * jinpuDuration, barSize.Y),
                    jinpuDuration > 0 ? SamJinpuColor["gradientLeft"] : 0x00202E3,
                    jinpuDuration > 0 ? SamJinpuColor["gradientRight"] : 0x00202E3,
                    jinpuDuration > 0 ? SamJinpuColor["gradientRight"] : 0x00202E3,
                    jinpuDuration > 0 ? SamJinpuColor["gradientLeft"] : 0x00202E3
                );
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            }
            else
            {
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                if (BuffText) {
                    DrawOutlinedText(Math.Round(shifuDuration).ToString(CultureInfo.InvariantCulture), new Vector2(cursorPos.X + buffsBarWidth / 2f - shifuTextSize.X / 2f, cursorPos.Y + (barSize.Y / 2) - 12));
                }

                cursorPos = new Vector2(cursorPos.X + buffsBarWidth + BuffsPadding, cursorPos.Y);
                var jinpuXOffset = CenterX - SamTimeJinpuXOffset;
                var jinpuYOffset = CenterY + SamTimeJinpuYOffset;

                drawList.AddRectFilled(cursorPos, cursorPos + barSize, SamEmptyColor["background"]);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2((barSize.X / 40) * jinpuDuration, barSize.Y),
                    jinpuDuration > 0 ? SamJinpuColor["gradientLeft"] : 0x00202E3,
                    jinpuDuration > 0 ? SamJinpuColor["gradientRight"] : 0x00202E3,
                    jinpuDuration > 0 ? SamJinpuColor["gradientRight"] : 0x00202E3,
                    jinpuDuration > 0 ? SamJinpuColor["gradientLeft"] : 0x00202E3
                );
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                if (BuffText) {
                    DrawOutlinedText(Math.Round(jinpuDuration).ToString(CultureInfo.InvariantCulture), new Vector2(cursorPos.X + buffsBarWidth / 2f - jinpuTextSize.X / 2f, cursorPos.Y + (barSize.Y / 2) - 12));
                }
            }
        }

        private void DrawSenResourceBar() {
            var gauge = PluginInterface.ClientState.JobGauges.Get<SAMGauge>();
            
            var senBarWidth = (SamSenBarWidth - SenPadding * 2) / 3f;
            var senBarSize = new Vector2(senBarWidth, SamSenBarHeight);
            var xPos = CenterX - SamSenBarX;
            var yPos = CenterY + SamSenBarY;
            var cursorPos = new Vector2(xPos - SenPadding - senBarWidth, yPos);
            
            var drawList = ImGui.GetWindowDrawList();

            // Ka Bar
            cursorPos = new Vector2(cursorPos.X + SenPadding + senBarWidth, cursorPos.Y);
            drawList.AddRectFilled(cursorPos, cursorPos + senBarSize, gauge.HasSetsu() ? SamSetsuColor["base"] : SamEmptyColor["background"]);
            drawList.AddRect(cursorPos, cursorPos + senBarSize, 0xFF000000);

            // Getsu Bar
            cursorPos = new Vector2(cursorPos.X + SenPadding + senBarWidth, cursorPos.Y);
            drawList.AddRectFilled(cursorPos, cursorPos + senBarSize, gauge.HasGetsu() ? SamGetsuColor["base"] : SamEmptyColor["background"]);
            drawList.AddRect(cursorPos, cursorPos + senBarSize, 0xFF000000);

            // Setsu Bar
            cursorPos = new Vector2(cursorPos.X + SenPadding + senBarWidth, cursorPos.Y);
            drawList.AddRectFilled(cursorPos, cursorPos + senBarSize, gauge.HasKa() ? SamKaColor["base"] : SamEmptyColor["background"]);
            drawList.AddRect(cursorPos, cursorPos + senBarSize, 0xFF000000);
        }


        private void DrawMeditationResourceBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<SAMGauge>();

            var meditationBarWidth = (SamMeditationBarWidth - MeditationPadding * 2) / 3f;
            var meditationBarSize = new Vector2(meditationBarWidth, SamMeditationBarHeight);
            var xPos = CenterX - SamMeditationBarX;
            var yPos = CenterY + SamMeditationBarY;
            var cursorPos = new Vector2(xPos - MeditationPadding - meditationBarWidth, yPos);

            var drawList = ImGui.GetWindowDrawList();

            // Meditation Stacks
            for (var i = 1; i < 4; i++) {
                cursorPos = new Vector2(cursorPos.X + MeditationPadding + meditationBarWidth, cursorPos.Y);

                drawList.AddRectFilled(cursorPos, cursorPos + meditationBarSize, gauge.MeditationStacks >= i ? SamMeditationColor["base"] : SamEmptyColor["background"]);
                drawList.AddRect(cursorPos, cursorPos + meditationBarSize, 0xFF000000);
            }
        }
    }
}
