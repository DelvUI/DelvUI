using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Interface.Bars;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface
{
    public class SamuraiHudWindow : HudWindow
    {
        public SamuraiHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        public override uint JobId => 34;

        private int BaseXOffset => PluginConfiguration.SAMBaseXOffset;
        private int BaseYOffset => PluginConfiguration.SAMBaseYOffset;

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
        private bool ShowBuffTime => PluginConfiguration.ShowBuffTime;
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

        protected override void Draw(bool _)
        {
            if (GaugeEnabled)
            {
                DrawKenkiBar();
            }

            if (SenEnabled)
            {
                DrawSenResourceBar();
            }

            if (MeditationEnabled)
            {
                DrawMeditationResourceBar();
            }

            if (HiganbanaEnabled)
            {
                DrawHiganbanaBar();
            }

            if (BuffsEnabled)
            {
                DrawActiveBuffs();
            }
        }

        protected override void DrawPrimaryResourceBar() { }

        private void DrawKenkiBar()
        {
            if (!GaugeEnabled)
            {
                return;
            }

            var gauge = PluginInterface.ClientState.JobGauges.Get<SAMGauge>();

            var xPos = CenterX + BaseXOffset - SamKenkiBarX;
            var yPos = CenterY + BaseYOffset + SamKenkiBarY;

            // Kenki Gauge

            var kenkiBuilder = BarBuilder.Create(xPos, yPos, SamKenkiBarHeight, SamKenkiBarWidth).SetBackgroundColor(SamEmptyColor["background"]);
            kenkiBuilder.AddInnerBar(gauge.Kenki, 100, SamKenkiColor);

            if (KenkiText) {
                kenkiBuilder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            var drawList = ImGui.GetWindowDrawList();
            kenkiBuilder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawHiganbanaBar()
        {
            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;

            if (target is not Chara)
            {
                return;
            }

            var actorId = PluginInterface.ClientState.LocalPlayer.ActorId;
            var higanbana = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1228 && o.OwnerId == actorId || o.EffectId == 1319 && o.OwnerId == actorId);
            var higanbanaDuration = higanbana.Duration;

            var higanbanaColor = higanbanaDuration > 5 ? SamHiganbanaColor : SamExpiryColor;

            var xOffset = CenterX + BaseXOffset - SamHiganbanaBarX;
            var yOffset = CenterY + BaseYOffset + SamHiganbanaBarY;

            if (higanbanaDuration == 0)
            {
                return;
            }
            var higanbanaBuilder = BarBuilder.Create(xOffset, yOffset, SamHiganbanaBarHeight, SamHiganbanaBarWidth).SetBackgroundColor(SamEmptyColor["background"]);
            higanbanaBuilder.AddInnerBar(higanbanaDuration, 60f, higanbanaColor).SetFlipDrainDirection(false);

            if (HiganbanaText)
            {
                higanbanaBuilder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }
            var drawList = ImGui.GetWindowDrawList();
            higanbanaBuilder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawActiveBuffs()
        {
            var target = PluginInterface.ClientState.LocalPlayer;

            var buffsBarWidth = (SamBuffsBarWidth / 2);
            var shifu = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1299);
            var jinpu = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1298);

            var shifuDuration = shifu.Duration;
            var jinpuDuration = jinpu.Duration;

            var xOffset = CenterX + BaseXOffset - SamBuffsBarX;
            var shifuXOffset = xOffset;
            var jinpuXOffset = xOffset + buffsBarWidth;
            var yOffset = CenterY + BaseYOffset + SamBuffsBarY;

            var shifuBuilder = BarBuilder.Create(shifuXOffset, yOffset, SamBuffsBarHeight, buffsBarWidth).SetBackgroundColor(SamEmptyColor["background"]);
            var jinpuBuilder = BarBuilder.Create(jinpuXOffset, yOffset, SamBuffsBarHeight, buffsBarWidth).SetBackgroundColor(SamEmptyColor["background"]);

            shifuBuilder.AddInnerBar(shifuDuration, 40f, SamShifuColor).SetFlipDrainDirection(true);
            jinpuBuilder.AddInnerBar(jinpuDuration, 40f, SamJinpuColor).SetFlipDrainDirection(false);

            if (BuffText)
            {
                shifuBuilder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                jinpuBuilder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);

            }

            var drawList = ImGui.GetWindowDrawList();
            shifuBuilder.Build().Draw(drawList, PluginConfiguration);
            jinpuBuilder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawSenResourceBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<SAMGauge>();
            var senBarWidth = (int)Math.Floor((SamSenBarWidth - SenPadding * 2) / 3f);
            var senBarSize = new Vector2(senBarWidth, SamSenBarHeight);
            var xPos = CenterX + BaseXOffset - SamSenBarX;
            var yPos = CenterY + BaseYOffset + SamSenBarY;
            var cursorPos = new Vector2(xPos - SenPadding - senBarWidth, yPos);

            int[] order = new int[3] { 0, 1, 2 };  // Setsu, Getsu, Ka

            var setsuBuilder = BarBuilder.Create(xPos + order[0] * (SenPadding + senBarWidth), cursorPos.Y, SamSenBarHeight, senBarWidth);
            var getsuBuilder = BarBuilder.Create(xPos + order[1] * (SenPadding + senBarWidth), cursorPos.Y, SamSenBarHeight, senBarWidth);
            var kaBuilder = BarBuilder.Create(xPos + order[2] * (SenPadding + senBarWidth), yPos, SamSenBarHeight, senBarWidth);

            kaBuilder.AddInnerBar(gauge.HasKa() ? 1 : 0, 1, SamKaColor);
            getsuBuilder.AddInnerBar(gauge.HasGetsu() ? 1 : 0, 1, SamGetsuColor);
            setsuBuilder.AddInnerBar(gauge.HasSetsu() ? 1 : 0, 1, SamSetsuColor);

            var drawList = ImGui.GetWindowDrawList();

            kaBuilder.Build().Draw(drawList, PluginConfiguration);
            getsuBuilder.Build().Draw(drawList, PluginConfiguration);
            setsuBuilder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawMeditationResourceBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<SAMGauge>();

            var meditationBarWidth = (int)Math.Floor((SamMeditationBarWidth - MeditationPadding * 2) / 3f);
            var meditationBarSize = new Vector2(meditationBarWidth, SamMeditationBarHeight);
            var xPos = CenterX + BaseXOffset - SamMeditationBarX;
            var yPos = CenterY + BaseYOffset + SamMeditationBarY;
            var cursorPos = new Vector2(xPos - MeditationPadding - meditationBarWidth, yPos);

            var meditationBuilder = BarBuilder.Create(xPos, yPos, SamMeditationBarHeight, SamMeditationBarWidth)
                .SetChunks(3)
                .SetBackgroundColor(SamEmptyColor["background"])
                .SetChunkPadding(MeditationPadding)
                .AddInnerBar(gauge.MeditationStacks, 3, SamMeditationColor);

            var drawList = ImGui.GetWindowDrawList();
            meditationBuilder.Build().Draw(drawList, PluginConfiguration);
        }
    }
}
