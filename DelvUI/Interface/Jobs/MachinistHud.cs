using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.Jobs
{
    public class MachinistHud : JobHud
    {
        private readonly float[] _robotDuration = { 12.450f, 13.950f, 15.450f, 16.950f, 18.450f, 19.950f };
        private new MachinistConfig Config => (MachinistConfig)_config;

        public MachinistHud(string id, MachinistConfig config, string? displayName = null) : base(id, config, displayName)
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
                DrawOverheatBar(pos);
            }

            if (Config.HeatGauge.Enabled)
            {
                DrawHeatGauge(pos);
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

        private void DrawHeatGauge(Vector2 origin)
        {
            MCHGauge gauge = Plugin.JobGauges.Get<MCHGauge>();

            if (!Config.HeatGauge.HideWhenInactive || gauge.Heat > 0)
            {
                Config.HeatGauge.Label.SetText(gauge.Heat.ToString("N0"));
                BarUtilities.GetProgressBar(Config.HeatGauge, gauge.Heat, 100, 0f).Draw(origin);
            }
        }

        private void DrawBatteryGauge(Vector2 origin, PlayerCharacter player)
        {
            MCHGauge gauge = Plugin.JobGauges.Get<MCHGauge>();

            if ((!Config.BatteryGauge.HideWhenInactive || gauge.Battery > 0) && !gauge.IsRobotActive)
            {
                Config.BatteryGauge.Label.SetText(gauge.Battery.ToString("N0"));
                BarUtilities.GetProgressBar(Config.BatteryGauge, gauge.Battery, 100f, 0f, player, Config.BatteryGauge.BatteryColor).Draw(origin);
            }

            if (gauge.IsRobotActive)
            {
                float robotDuration = gauge.SummonTimeRemaining / 1000f;

                Config.BatteryGauge.Label.SetText(robotDuration.ToString("N0"));
                BarUtilities.GetProgressBar(Config.BatteryGauge, gauge.SummonTimeRemaining, _robotDuration[gauge.LastSummonBatteryPower / 10 - 5], 0f, player, Config.BatteryGauge.RobotColor).Draw(origin);
            }
        }

        private void DrawOverheatBar(Vector2 origin)
        {
            MCHGauge gauge = Plugin.JobGauges.Get<MCHGauge>();
            float overheatDuration = gauge.OverheatTimeRemaining / 1000f;

            if (!Config.OverheatGauge.HideWhenInactive || gauge.IsOverheated)
            {
                Config.OverheatGauge.Label.SetText(overheatDuration.ToString("N0"));
                BarUtilities.GetProgressBar(Config.OverheatGauge, overheatDuration, 8f, 0f).Draw(origin);
            }
        }

        private void DrawWildfireBar(Vector2 origin, PlayerCharacter player)
        {
            float wildfireDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 1946)?.RemainingTime ?? 0f;

            if (!Config.WildfireBar.HideWhenInactive || wildfireDuration > 0)
            {
                Config.WildfireBar.Label.SetText(wildfireDuration.ToString("N0"));
                BarUtilities.GetProgressBar(Config.WildfireBar, wildfireDuration, 10, 0f).Draw(origin);
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

            config.HeatGauge.ThresholdConfig.Enabled = true;
            config.HeatGauge.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.OverheatGauge.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.BatteryGauge.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.WildfireBar.Label.FontID = FontsConfig.DefaultMediumFontKey;

            return config;
        }

        [NestedConfig("Overheat Gauge", 30)]
        public ProgressBarConfig OverheatGauge = new ProgressBarConfig(
            new Vector2(0, -54),
            new Vector2(254, 20),
            new PluginConfigColor(new Vector4(255f / 255f, 239f / 255f, 14f / 255f, 100f / 100f))
        );

        [NestedConfig("Heat Gauge", 35)]
        public ProgressBarConfig HeatGauge = new ProgressBarConfig(
            new Vector2(0, -32),
            new Vector2(254, 20),
            new PluginConfigColor(new Vector4(201f / 255f, 13f / 255f, 13f / 255f, 100f / 100f)),
            BarDirection.Right,
            new PluginConfigColor(new Vector4(180f / 255f, 180f / 255f, 180f / 255f, 100f / 100f)),
            50
        );

        [NestedConfig("Battery Gauge", 40)]
        public BatteryGaugeConfig BatteryGauge = new BatteryGaugeConfig(
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

    [Exportable(false)]
    public class BatteryGaugeConfig : ProgressBarConfig
    {
        [ColorEdit4("Battery Color", spacing = true)]
        [Order(55)]
        public PluginConfigColor BatteryColor = new(new Vector4(106f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [ColorEdit4("Robot Color")]
        [Order(60)]
        public PluginConfigColor RobotColor = new(new Vector4(153f / 255f, 0f / 255f, 255f / 255f, 100f / 100f));

        public BatteryGaugeConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor) : base(position, size, fillColor)
        {
        }
    }

}
