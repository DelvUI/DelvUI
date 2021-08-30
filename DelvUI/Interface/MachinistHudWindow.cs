using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Interface.Bars;
using ImGuiNET;

namespace DelvUI.Interface
{
    public class MachinistHudWindow : HudWindow
    {
        public override uint JobId => 31;

        private int OverheatHeight => PluginConfiguration.MCHOverheatHeight;

        private int OverheatWidth => PluginConfiguration.MCHOverheatWidth;

        private new int XOffset => PluginConfiguration.MCHBaseXOffset;

        private new int YOffset => PluginConfiguration.MCHBaseYOffset;

        private int HeatGaugeHeight => PluginConfiguration.MCHHeatGaugeHeight;

        private int HeatGaugeWidth => PluginConfiguration.MCHHeatGaugeWidth;

        private int HeatGaugePadding => PluginConfiguration.MCHHeatGaugePadding;

        private int HeatGaugeXOffset => PluginConfiguration.MCHHeatGaugeXOffset;

        private int HeatGaugeYOffset => PluginConfiguration.MCHHeatGaugeYOffset;

        private int BatteryGaugeHeight => PluginConfiguration.MCHBatteryGaugeHeight;

        private int BatteryGaugeWidth => PluginConfiguration.MCHBatteryGaugeWidth;

        private int BatteryGaugePadding => PluginConfiguration.MCHBatteryGaugePadding;

        private int BatteryGaugeXOffset => PluginConfiguration.MCHBatteryGaugeXOffset;

        private int BatteryGaugeYOffset => PluginConfiguration.MCHBatteryGaugeYOffset;

        private bool WildfireEnabled => PluginConfiguration.MCHWildfireEnabled;

        private int WildfireHeight => PluginConfiguration.MCHWildfireHeight;

        private int WildfireWidth => PluginConfiguration.MCHWildfireWidth;

        private int WildfireXOffset => PluginConfiguration.MCHWildfireXOffset;

        private int WildfireYOffset => PluginConfiguration.MCHWildfireYOffset;

        private Dictionary<string, uint> HeatColor => PluginConfiguration.JobColorMap[Jobs.MCH * 1000];

        private Dictionary<string, uint> BatteryColor => PluginConfiguration.JobColorMap[Jobs.MCH * 1000 + 1];

        private Dictionary<string, uint> RobotColor => PluginConfiguration.JobColorMap[Jobs.MCH * 1000 + 2];

        private Dictionary<string, uint> OverheatColor => PluginConfiguration.JobColorMap[Jobs.MCH * 1000 + 3];

        private Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.MCH * 1000 + 4];

        private Dictionary<string, uint> WildfireColor => PluginConfiguration.JobColorMap[Jobs.MCH * 1000 + 5];
        private int InterBarOffset => PluginConfiguration.MCHInterBarOffset;
        // TODO: Rook auto-turret differences?
        private readonly float[] _robotDuration = {12.450f, 13.950f, 15.450f, 16.950f, 18.450f, 19.950f};
        
        public MachinistHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _) 
        {
            var nextHeight = DrawOverheatBar(0);
            nextHeight = DrawHeatGauge(nextHeight);
            nextHeight = DrawBatteryGauge(nextHeight);
            if (WildfireEnabled)
            {
                DrawWildfireBar(nextHeight);
            }
        }

        protected override void DrawPrimaryResourceBar()
        {
        }

        private int DrawHeatGauge(int initialHeight)
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<MCHGauge>();
            
            var xPos = CenterX - XOffset + HeatGaugeXOffset;
            var yPos = CenterY + YOffset + initialHeight + HeatGaugeYOffset;
            
            var bar = BarBuilder.Create(xPos, yPos, HeatGaugeHeight, HeatGaugeWidth)
                .SetChunks(2)
                .SetChunkPadding(HeatGaugePadding)
                .AddInnerBar(gauge.Heat, 100, HeatColor, EmptyColor)
                .SetTextMode(BarTextMode.EachChunk)
                .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                .Build();
            
            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);

            return HeatGaugeHeight + initialHeight + InterBarOffset;
        }

        private int DrawBatteryGauge(int initialHeight)
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<MCHGauge>();
            
            var xPos = CenterX - XOffset + BatteryGaugeXOffset;
            var yPos = CenterY + YOffset + initialHeight + BatteryGaugeYOffset;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, BatteryGaugeHeight, BatteryGaugeWidth)
                .SetChunks(new[] {.5f, .1f, .1f, .1f, .1f, .1f})
                .SetChunkPadding(BatteryGaugePadding)
                .AddInnerBar(gauge.Battery, 100, BatteryColor, EmptyColor);

            if (gauge.IsRobotActive())
            {
                builder.AddInnerBar(gauge.RobotTimeRemaining / 1000f, _robotDuration[gauge.LastRobotBatteryPower / 10 - 5], RobotColor, null)
                    .SetTextMode(BarTextMode.Single)
                    .SetText(BarTextPosition.CenterLeft, BarTextType.Current);
            }

            var drawList = ImGui.GetWindowDrawList();
            var bar = builder.Build();
            bar.Draw(drawList);

            return BatteryGaugeHeight + initialHeight + InterBarOffset;
        }

        private int DrawOverheatBar(int initialHeight)
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<MCHGauge>();
            
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset + initialHeight;

            var builder = BarBuilder.Create(xPos, yPos, OverheatHeight, OverheatWidth);

            if (gauge.IsOverheated())
            {
                builder.AddInnerBar(gauge.OverheatTimeRemaining / 1000f, 8, OverheatColor, null)
                    .SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }
            
            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);

            return OverheatHeight + initialHeight + InterBarOffset;
        }

        private int DrawWildfireBar(int initialHeight)
        {
            var wildfireBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1946);
        
            var xPos = CenterX - XOffset + WildfireXOffset;
            var yPos = CenterY + YOffset + initialHeight + WildfireYOffset;

            var builder = BarBuilder.Create(xPos, yPos, WildfireHeight, WildfireWidth);

            if (wildfireBuff.Any())
            {
                var duration = wildfireBuff.First().Duration;
                builder.AddInnerBar(duration, 10, WildfireColor, null)
                    .SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);

            return WildfireHeight + initialHeight + InterBarOffset;
        }
    }
}