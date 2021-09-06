﻿using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Interface.Bars;
using DelvUI.Config.Attributes;
using ImGuiNET;
using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface
{
    public class MachinistHudWindow : HudWindow
    {
        private readonly float[] _robotDuration = { 12.450f, 13.950f, 15.450f, 16.950f, 18.450f, 19.950f };
        private readonly MachinistHudConfig _config = (MachinistHudConfig)ConfigurationManager.GetInstance().GetConfiguration(new MachinistHudConfig());

        public override uint JobId => Jobs.MCH;
        private Vector2 Origin => new(CenterX + _config.Position.X, CenterY + _config.Position.Y);
        private readonly PluginConfigColor EmptyColor;
        private readonly PluginConfigColor PartialFillColor;

        public MachinistHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration)
        {
            EmptyColor = new(PluginConfiguration.EmptyColor);
            PartialFillColor = new(PluginConfiguration.PartialFillColor);
        }

        protected override void Draw(bool _)
        {
            if (_config.ShowOverheat)
            {
                DrawOverheatBar();
            }

            if (_config.ShowHeatGauge)
            {
                DrawHeatGauge();
            }

            if (_config.ShowBatteryGauge)
            {
                DrawBatteryGauge();
            }

            if (_config.ShowWildfire)
            {
                DrawWildfireBar();
            }
        }

        protected override void DrawPrimaryResourceBar() { }
        private Vector2 CalculatePosition(Vector2 position, Vector2 size) => Origin + position - size / 2f;

        private void DrawHeatGauge()
        {
            Debug.Assert(PluginInterface.ClientState.JobGauges != null, "PluginInterface.ClientState.JobGauges != null");
            var gauge = PluginInterface.ClientState.JobGauges.Get<MCHGauge>();

            Vector2 position = CalculatePosition(_config.HeatGaugePosition, _config.HeatGaugeSize);

            var builder = BarBuilder.Create(position, _config.HeatGaugeSize)
                                    .SetChunks(2)
                                    .SetChunkPadding(_config.HeatGaugePadding)
                                    .AddInnerBar(gauge.Heat, 100, _config.HeatGaugeFillColor.Map, PartialFillColor.Map);

            if (_config.ShowHeatGaugeText)
            {
                builder.SetTextMode(BarTextMode.EachChunk)
                       .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                       .SetBackgroundColor(EmptyColor.Background);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawBatteryGauge()
        {
            Debug.Assert(PluginInterface.ClientState.JobGauges != null, "PluginInterface.ClientState.JobGauges != null");
            var gauge = PluginInterface.ClientState.JobGauges.Get<MCHGauge>();

            Vector2 position = CalculatePosition(_config.BatteryGaugePosition, _config.BatteryGaugeSize);

            var builder = BarBuilder.Create(position, _config.BatteryGaugeSize)
                                    .SetChunks(new[] { .5f, .1f, .1f, .1f, .1f, .1f })
                                    .SetChunkPadding(_config.BatteryGaugePadding)
                                    .SetBackgroundColor(EmptyColor.Background);

            if (_config.ShowBatteryGaugeBattery)
            {
                builder.AddInnerBar(gauge.Battery, 100, _config.BatteryFillColor.Map, PartialFillColor.Map);

                if (_config.ShowBatteryGaugeBatteryText)
                {
                    builder.SetTextMode(BarTextMode.Single)
                           .SetText(BarTextPosition.CenterLeft, BarTextType.Current, _config.BatteryFillColor.Vector, Vector4.UnitW, null);
                }
            }

            if (gauge.IsRobotActive() && _config.ShowBatteryGaugeRobotDuration)
            {
                builder.AddInnerBar(gauge.RobotTimeRemaining / 1000f, _robotDuration[gauge.LastRobotBatteryPower / 10 - 5], _config.RobotFillColor.Map, null);

                if (_config.ShowBatteryGaugeRobotDurationText)
                {
                    builder.SetTextMode(BarTextMode.Single)
                           .SetText(BarTextPosition.CenterRight, BarTextType.Current, _config.RobotFillColor.Vector, Vector4.UnitW, null);
                }
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            var bar = builder.Build();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawOverheatBar()
        {
            Debug.Assert(PluginInterface.ClientState.JobGauges != null, "PluginInterface.ClientState.JobGauges != null");
            var gauge = PluginInterface.ClientState.JobGauges.Get<MCHGauge>();

            Vector2 position = CalculatePosition(_config.OverheatPosition, _config.OverheatSize);

            var builder = BarBuilder.Create(position, _config.OverheatSize)
                .SetBackgroundColor(EmptyColor.Background);

            if (gauge.IsOverheated())
            {
                builder.AddInnerBar(gauge.OverheatTimeRemaining / 1000f, 8, _config.OverheatFillColor.Map, null);

                if (_config.ShowOverheatText)
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
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var wildfireBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1946);

            Vector2 position = CalculatePosition(_config.WildfirePosition, _config.WildfireSize);

            var builder = BarBuilder.Create(position, _config.WildfireSize);

            if (wildfireBuff.Any())
            {
                var duration = wildfireBuff.First().Duration;
                builder.AddInnerBar(duration, 10, _config.WildfireFillColor.Map, null);

                if (_config.ShowWildfireText)
                {
                    builder.SetTextMode(BarTextMode.EachChunk)
                           .SetBackgroundColor(EmptyColor.Background)
                           .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Ranged", 0)]
    [SubSection("Machinist", 1)]
    public class MachinistHudConfig : PluginConfigObject
    {
        [DragFloat2("Base offset", min = -4000f, max = 4000f)]
        public Vector2 Position = new(0, 0);

        /* Overheat */
        [Checkbox("Show Overheat Bar")]
        public bool ShowOverheat = true;

        [Checkbox("Show Overheat Bar Text")]
        public bool ShowOverheatText = true;

        [DragFloat2("Overheat Bar Position", min = -4000f, max = 4000f)]
        public Vector2 OverheatPosition = new(0, 395);

        [DragFloat2("Overheat Bar Size", min = 0, max = 4000f)]
        public Vector2 OverheatSize = new(254, 20);

        [ColorEdit4("Overheat Fill Color")]
        public PluginConfigColor OverheatFillColor = new(new Vector4(255f / 255f, 239f / 255f, 14f / 255f, 100f / 100f));

        /* Heat Gauge */
        [Checkbox("Show Heat Gauge")]
        public bool ShowHeatGauge = true;

        [Checkbox("Show Heat Gauge Text")]
        public bool ShowHeatGaugeText = true;

        [DragFloat2("Heat Gauge Position", min = -4000f, max = 4000f)]
        public Vector2 HeatGaugePosition = new(0, 417);

        [DragFloat2("Heat Gauge Size", min = 0, max = 4000f)]
        public Vector2 HeatGaugeSize = new(254, 20);

        [DragInt("Heat Gauge Padding", min = 0)]
        public int HeatGaugePadding = 2;

        [ColorEdit4("Heat Gauge Fill Color")]
        public PluginConfigColor HeatGaugeFillColor = new(new Vector4(201f / 255f, 13f / 255f, 13f / 255f, 100f / 100f));

        /* Battery Gauge */
        [Checkbox("Show Battery Gauge")]
        public bool ShowBatteryGauge = true;

        [Checkbox("Battery Gauge Show Battery")]
        public bool ShowBatteryGaugeBattery = true;

        [Checkbox("Battery Gauge Show Battery Text")]
        public bool ShowBatteryGaugeBatteryText = false;

        [Checkbox("Show Battery Gauge Robot Duration")]
        public bool ShowBatteryGaugeRobotDuration = true;

        [Checkbox("Show Battery Gauge Robot Duration Text")]
        public bool ShowBatteryGaugeRobotDurationText = true;

        [DragFloat2("Battery Gauge Position", min = -4000f, max = 4000f)]
        public Vector2 BatteryGaugePosition = new(0, 439);

        [DragFloat2("Battery Gauge Size", min = 0, max = 4000f)]
        public Vector2 BatteryGaugeSize = new(254, 20);

        [DragInt("Battery Gauge Padding", min = 0)]
        public int BatteryGaugePadding = 2;

        [ColorEdit4("Battery Fill Color")]
        public PluginConfigColor BatteryFillColor = new(new Vector4(106f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [ColorEdit4("Robot Fill Color")]
        public PluginConfigColor RobotFillColor = new(new Vector4(153f / 255f, 0f / 255f, 255f / 255f, 100f / 100f));

        /* Wildfire */
        [Checkbox("Show Wildfire")]
        public bool ShowWildfire = false;

        [Checkbox("Show Wildfire Text")]
        public bool ShowWildfireText = true;

        [DragFloat2("Wildfire Position", min = -4000f, max = 4000f)]
        public Vector2 WildfirePosition = new(0, 373);

        [DragFloat2("Wildfire Size", min = 0, max = 4000f)]
        public Vector2 WildfireSize = new(254, 20);

        [ColorEdit4("Wildfire Color")]
        public PluginConfigColor WildfireFillColor = new(new Vector4(255f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));
    }
}
