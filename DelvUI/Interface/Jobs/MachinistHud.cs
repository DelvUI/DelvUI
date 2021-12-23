﻿using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.Jobs
{
    public class MachinistHud : JobHud
    {
        private bool _robotMaxDurationSet;
        private float _robotMaxDuration;

        private new MachinistConfig Config => (MachinistConfig)_config;

        public MachinistHud(MachinistConfig config, string? displayName = null) : base(config, displayName)
        {
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.OverheatGauge.Enabled)
            {
                positions.Add(Config.Position + Config.OverheatGauge.Position);
                sizes.Add(Config.OverheatGauge.Size);
            }

            if (Config.HeatGauge.Enabled)
            {
                positions.Add(Config.Position + Config.HeatGauge.Position);
                sizes.Add(Config.HeatGauge.Size);
            }

            if (Config.BatteryGauge.Enabled)
            {
                positions.Add(Config.Position + Config.BatteryGauge.Position);
                sizes.Add(Config.BatteryGauge.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            Vector2 pos = origin + Config.Position;

            if (Config.OverheatGauge.Enabled)
            {
                DrawOverheatBar(pos, player);
            }

            if (Config.HeatGauge.Enabled)
            {
                DrawHeatGauge(pos, player);
            }

            if (Config.BatteryGauge.Enabled)
            {
                DrawBatteryGauge(pos, player);
            }

            if (Config.WildfireBar.Enabled)
            {
                DrawWildfireBar(pos, player);
            }
        }

        private void DrawHeatGauge(Vector2 origin, PlayerCharacter player)
        {
            MCHGauge gauge = Plugin.JobGauges.Get<MCHGauge>();

            if (!Config.HeatGauge.HideWhenInactive || gauge.Heat > 0)
            {
                Config.HeatGauge.Label.SetValue(gauge.Heat);
                BarUtilities.GetChunkedProgressBars(Config.HeatGauge, 2, gauge.Heat, 100, 0, player)
                    .Draw(origin);
            }
        }

        private void DrawBatteryGauge(Vector2 origin, PlayerCharacter player)
        {
            MCHGauge gauge = Plugin.JobGauges.Get<MCHGauge>();

            if ((!Config.BatteryGauge.HideWhenInactive || gauge.Battery > 0) && !gauge.IsRobotActive)
            {
                Config.BatteryGauge.Label.SetValue(gauge.Battery);
                BarUtilities.GetProgressBar(Config.BatteryGauge, gauge.Battery, 100f, 0f, player, Config.BatteryGauge.BatteryColor)
                    .Draw(origin);
            }

            if (!gauge.IsRobotActive && _robotMaxDurationSet)
            {
                _robotMaxDurationSet = false;
            }
            if (gauge.IsRobotActive)
            {
                if (!_robotMaxDurationSet)
                {
                    _robotMaxDuration = gauge.SummonTimeRemaining;
                    _robotMaxDurationSet = true;
                }

                Config.BatteryGauge.Label.SetValue(gauge.SummonTimeRemaining / 1000f);
                BarUtilities.GetProgressBar(Config.BatteryGauge, gauge.SummonTimeRemaining / 1000, _robotMaxDuration / 1000, 0f, player, Config.BatteryGauge.RobotColor)
                    .Draw(origin);
            }
        }

        private void DrawOverheatBar(Vector2 origin, PlayerCharacter player)
        {
            MCHGauge gauge = Plugin.JobGauges.Get<MCHGauge>();

            if (!Config.OverheatGauge.HideWhenInactive || gauge.IsOverheated)
            {
                Config.OverheatGauge.Label.SetValue(gauge.OverheatTimeRemaining / 1000f);
                BarUtilities.GetProgressBar(Config.OverheatGauge, gauge.OverheatTimeRemaining / 1000f, 8, 0f, player)
                    .Draw(origin);
            }
        }

        private void DrawWildfireBar(Vector2 origin, PlayerCharacter player)
        {
            float wildfireDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 1946)?.RemainingTime ?? 0f;

            if (!Config.WildfireBar.HideWhenInactive || wildfireDuration > 0)
            {
                Config.WildfireBar.Label.SetValue(wildfireDuration);
                BarUtilities.GetProgressBar(Config.WildfireBar, wildfireDuration, 10, 0f, player)
                    .Draw(origin);
            }
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Ranged", 0)]
    [SubSection("Machinist", 1)]
    public class MachinistConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.MCH;
        public new static MachinistConfig DefaultConfig()
        {
            var config = new MachinistConfig();

            config.HeatGauge.UsePartialFillColor = true;

            return config;
        }

        [NestedConfig("Overheat Gauge", 30)]
        public ProgressBarConfig OverheatGauge = new ProgressBarConfig(
            new Vector2(0, -54),
            new Vector2(254, 20),
            new PluginConfigColor(new Vector4(255f / 255f, 239f / 255f, 14f / 255f, 100f / 100f))
        );

        [NestedConfig("Heat Gauge", 35)]
        public ChunkedProgressBarConfig HeatGauge = new ChunkedProgressBarConfig(
            new Vector2(0, -32),
            new Vector2(254, 20),
            new PluginConfigColor(new Vector4(201f / 255f, 13f / 255f, 13f / 255f, 100f / 100f)),
            2,
            new PluginConfigColor(new Vector4(180f / 255f, 180f / 255f, 180f / 255f, 100f / 100f))
        );

        [NestedConfig("Battery Gauge", 40)]
        public MachinistBatteryGaugeConfig BatteryGauge = new MachinistBatteryGaugeConfig(
            new Vector2(0, -10),
            new Vector2(254, 20),
            new PluginConfigColor(new Vector4(0, 0, 0, 0))
        );

        [NestedConfig("Wildfire Bar", 50)]
        public ProgressBarConfig WildfireBar = new ProgressBarConfig(
            new Vector2(0, -76),
            new Vector2(254, 20),
            new PluginConfigColor(new Vector4(255f / 255f, 0f / 255f, 0f / 255f, 100f / 100f)),
            BarDirection.Right,
            new PluginConfigColor(new Vector4(180f / 255f, 180f / 255f, 180f / 255f, 100f / 100f)),
            50
        );
    }

    [DisableParentSettings("FillColor")]
    [Exportable(false)]
    public class MachinistBatteryGaugeConfig : ProgressBarConfig
    {
        [ColorEdit4("Battery Color", spacing = true)]
        [Order(55)]
        public PluginConfigColor BatteryColor = new(new Vector4(106f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [ColorEdit4("Robot Color")]
        [Order(60)]
        public PluginConfigColor RobotColor = new(new Vector4(153f / 255f, 0f / 255f, 255f / 255f, 100f / 100f));

        public MachinistBatteryGaugeConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor) : base(position, size, fillColor)
        {
        }
    }
}
