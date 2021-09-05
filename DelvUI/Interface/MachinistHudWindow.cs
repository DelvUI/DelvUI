using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Interface.Bars;
using DelvUI.Helpers;
using ImGuiNET;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface
{
    public class MachinistHudWindow : HudWindow
    {
        private readonly float[] _robotDuration = { 12.450f, 13.950f, 15.450f, 16.950f, 18.450f, 19.950f };

        public MachinistHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        public override uint JobId => 31;

        private int BaseXOffset => PluginConfiguration.MCHBaseXOffset;
        private int BaseYOffset => PluginConfiguration.MCHBaseYOffset;

        private bool OverheatEnabled => PluginConfiguration.MCHOverheatEnable;
        private bool OverheatText => PluginConfiguration.MCHOverheatText;
        private int OverheatHeight => PluginConfiguration.MCHOverheatHeight;
        private int OverheatWidth => PluginConfiguration.MCHOverheatWidth;
        private int OverheatXOffset => PluginConfiguration.MCHOverheatXOffset;
        private int OverheatYOffset => PluginConfiguration.MCHOverheatYOffset;

        private bool HeatGaugeEnabled => PluginConfiguration.MCHHeatGaugeEnable;
        private bool HeatGaugeText => PluginConfiguration.MCHHeatGaugeText;
        private int HeatGaugeHeight => PluginConfiguration.MCHHeatGaugeHeight;
        private int HeatGaugeWidth => PluginConfiguration.MCHHeatGaugeWidth;
        private int HeatGaugePadding => PluginConfiguration.MCHHeatGaugePadding;
        private int HeatGaugeXOffset => PluginConfiguration.MCHHeatGaugeXOffset;
        private int HeatGaugeYOffset => PluginConfiguration.MCHHeatGaugeYOffset;

        private bool BatteryGaugeEnabled => PluginConfiguration.MCHBatteryGaugeEnable;
        private bool BatteryGaugeShowBattery => PluginConfiguration.MCHBatteryGaugeShowBattery;
        private bool BatteryGaugeBatteryText => PluginConfiguration.MCHBatteryGaugeBatteryText;
        private bool BatteryGaugeShowRobotDuration => PluginConfiguration.MCHBatteryGaugeShowRobotDuration;
        private bool BatteryGaugeRobotDurationText => PluginConfiguration.MCHBatteryGaugeRobotDurationText;
        private int BatteryGaugeHeight => PluginConfiguration.MCHBatteryGaugeHeight;
        private int BatteryGaugeWidth => PluginConfiguration.MCHBatteryGaugeWidth;
        private int BatteryGaugePadding => PluginConfiguration.MCHBatteryGaugePadding;
        private int BatteryGaugeXOffset => PluginConfiguration.MCHBatteryGaugeXOffset;
        private int BatteryGaugeYOffset => PluginConfiguration.MCHBatteryGaugeYOffset;

        private bool WildfireEnabled => PluginConfiguration.MCHWildfireEnabled;
        private bool WildfireText => PluginConfiguration.MCHWildfireText;
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

        protected override void Draw(bool _)
        {
            if (OverheatEnabled)
            {
                DrawOverheatBar();
            }

            if (HeatGaugeEnabled)
            {
                DrawHeatGauge();
            }

            if (BatteryGaugeEnabled)
            {
                DrawBatteryGauge();
            }

            if (WildfireEnabled)
            {
                DrawWildfireBar();
            }
        }

        protected override void DrawPrimaryResourceBar() { }

        private void DrawHeatGauge()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<MCHGauge>();

            var xPos = CenterX + BaseXOffset - HeatGaugeXOffset;
            var yPos = CenterY + BaseYOffset + HeatGaugeYOffset;

            var builder = BarBuilder.Create(xPos, yPos, HeatGaugeHeight, HeatGaugeWidth)
                                    .SetChunks(2)
                                    .SetChunkPadding(HeatGaugePadding)
                                    .AddInnerBar(gauge.Heat, 100, HeatColor, EmptyColor);

            if (HeatGaugeText)
            {
                builder.SetTextMode(BarTextMode.EachChunk)
                       .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                       .SetBackgroundColor(EmptyColor["background"]);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawBatteryGauge()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<MCHGauge>();

            var xPos = CenterX + BaseXOffset - BatteryGaugeXOffset;
            var yPos = CenterY + BaseYOffset + BatteryGaugeYOffset;

            var builder = BarBuilder.Create(xPos, yPos, BatteryGaugeHeight, BatteryGaugeWidth)
                                    .SetChunks(new[] { .5f, .1f, .1f, .1f, .1f, .1f })
                                    .SetChunkPadding(BatteryGaugePadding)
                                    .SetBackgroundColor(EmptyColor["background"]);

            if (BatteryGaugeShowBattery)
            {
                builder.AddInnerBar(gauge.Battery, 100, BatteryColor, EmptyColor);

                if (BatteryGaugeBatteryText)
                {
                    builder.SetTextMode(BarTextMode.Single)
                           .SetText(BarTextPosition.CenterLeft, BarTextType.Current, PluginConfiguration.MCHBatteryColor, Vector4.UnitW, null);
                }
            }

            if (gauge.IsRobotActive() && BatteryGaugeShowRobotDuration)
            {
                builder.AddInnerBar(gauge.RobotTimeRemaining / 1000f, _robotDuration[gauge.LastRobotBatteryPower / 10 - 5], RobotColor, null);

                if (BatteryGaugeRobotDurationText)
                {
                    builder.SetTextMode(BarTextMode.Single)
                           .SetText(BarTextPosition.CenterRight, BarTextType.Current, PluginConfiguration.MCHRobotColor, Vector4.UnitW, null);
                }
            }

            var drawList = ImGui.GetWindowDrawList();
            var bar = builder.Build();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawOverheatBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<MCHGauge>();

            var xPos = CenterX + BaseXOffset - OverheatXOffset;
            var yPos = CenterY + BaseYOffset + OverheatYOffset;

            var builder = BarBuilder.Create(xPos, yPos, OverheatHeight, OverheatWidth).SetBackgroundColor(EmptyColor["background"]);

            if (gauge.IsOverheated())
            {
                builder.AddInnerBar(gauge.OverheatTimeRemaining / 1000f, 8, OverheatColor, null);

                if (OverheatText)
                {
                    builder.SetTextMode(BarTextMode.EachChunk)
                           .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawWildfireBar()
        {
            var wildfireBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1946);

            var xPos = CenterX + BaseXOffset - WildfireXOffset;
            var yPos = CenterY + BaseYOffset + WildfireYOffset;

            var builder = BarBuilder.Create(xPos, yPos, WildfireHeight, WildfireWidth);

            if (wildfireBuff.Any())
            {
                var duration = wildfireBuff.First().Duration;
                builder.AddInnerBar(duration, 10, WildfireColor, null);

                if (WildfireText)
                {
                    builder.SetTextMode(BarTextMode.EachChunk)
                           .SetBackgroundColor(EmptyColor["background"])
                           .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }
    }
}
