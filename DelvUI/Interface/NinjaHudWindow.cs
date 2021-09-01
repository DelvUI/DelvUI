using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Interface.Bars;
using ImGuiNET;
using DelvUI.Helpers;

namespace DelvUI.Interface
{
    public class NinjaHudWindow : HudWindow
    {
        public override uint JobId => 30;

        private new int XOffset => PluginConfiguration.NINBaseXOffset;
        private new int YOffset => PluginConfiguration.NINBaseYOffset;

        private bool HutonGaugeEnabled => PluginConfiguration.NINHutonGaugeEnabled;
        private int HutonGaugeHeight => PluginConfiguration.NINHutonGaugeHeight;
        private int HutonGaugeWidth => PluginConfiguration.NINHutonGaugeWidth;
        private int HutonGaugeXOffset => PluginConfiguration.NINHutonGaugeXOffset;
        private int HutonGaugeYOffset => PluginConfiguration.NINHutonGaugeYOffset;

        private bool NinkiGaugeEnabled => PluginConfiguration.NINNinkiGaugeEnabled;
        private int NinkiGaugeHeight => PluginConfiguration.NINNinkiGaugeHeight;
        private int NinkiGaugeWidth => PluginConfiguration.NINNinkiGaugeWidth;
        private int NinkiGaugePadding => PluginConfiguration.NINNinkiGaugePadding;
        private int NinkiGaugeXOffset => PluginConfiguration.NINNinkiGaugeXOffset;
        private int NinkiGaugeYOffset => PluginConfiguration.NINNinkiGaugeYOffset;

        private Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.NIN * 1000];
        private Dictionary<string, uint> HutonColor => PluginConfiguration.JobColorMap[Jobs.NIN * 1000 + 1];
        private Dictionary<string, uint> NinkiColor => PluginConfiguration.JobColorMap[Jobs.NIN * 1000 + 2];

        public NinjaHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _)
        {
            if (HutonGaugeEnabled)
                DrawHutonGauge();
            if (NinkiGaugeEnabled)
                DrawNinkiGauge();
        }

        protected override void DrawPrimaryResourceBar()
        {
        }

        private void DrawHutonGauge()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<NINGauge>();
            var hutonDurationLeft = (int)Math.Ceiling((float) (gauge.HutonTimeLeft / (double)1000));

            var xPos = CenterX - XOffset + HutonGaugeXOffset;
            var yPos = CenterY + YOffset + HutonGaugeYOffset;

            var builder = BarBuilder.Create(xPos, yPos, HutonGaugeHeight, HutonGaugeWidth);
            float maximum = 70f;

            Bar bar = builder.AddInnerBar(Math.Abs(hutonDurationLeft), maximum, HutonColor)
                .SetTextMode(BarTextMode.EachChunk)
                .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        private void DrawNinkiGauge()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<NINGauge>();

            var xPos = CenterX - XOffset + NinkiGaugeXOffset;
            var yPos = CenterY + YOffset + NinkiGaugeYOffset;

            var bar = BarBuilder.Create(xPos, yPos, NinkiGaugeHeight, NinkiGaugeWidth)
                .SetChunks(2)
                .SetChunkPadding(NinkiGaugePadding)
                .AddInnerBar(gauge.Ninki, 100, NinkiColor, EmptyColor)
                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }
    }
}