using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Logging;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
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

            if (Config.OverheatChunkedGauge.Enabled)
            {
                positions.Add(Config.Position + Config.OverheatChunkedGauge.Position);
                sizes.Add(Config.OverheatChunkedGauge.Size);
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

        public override void DrawJobHud(Vector2 origin, IPlayerCharacter player)
        {
            Vector2 pos = origin + Config.Position;

            if (Config.OverheatChunkedGauge.Enabled)
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

            if (Config.AutomatonBar.Enabled)
            {
                DrawAutomatonBar(pos, player);
            }

            if (Config.WildfireBar.Enabled)
            {
                DrawWildfireBar(pos, player);
            }
        }

        private void DrawHeatGauge(Vector2 origin, IPlayerCharacter player)
        {
            MCHGauge gauge = Plugin.JobGauges.Get<MCHGauge>();

            if (!Config.HeatGauge.HideWhenInactive || gauge.Heat > 0)
            {
                Config.HeatGauge.Label.SetValue(gauge.Heat);

                BarHud[] bars = BarUtilities.GetChunkedProgressBars(Config.HeatGauge, 2, gauge.Heat, 100, 0, player);
                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.HeatGauge.StrataLevel));
                }
            }
        }

        private void DrawBatteryGauge(Vector2 origin, IPlayerCharacter player)
        {
            MCHGauge gauge = Plugin.JobGauges.Get<MCHGauge>();

            if (!Config.BatteryGauge.HideWhenInactive || gauge.Battery > 0)
            {
                Config.BatteryGauge.Label.SetValue(gauge.Battery);

                BarHud bar = BarUtilities.GetProgressBar(Config.BatteryGauge, gauge.Battery, 100f, 0f, player, Config.BatteryGauge.BatteryColor);
                AddDrawActions(bar.GetDrawActions(origin, Config.BatteryGauge.StrataLevel));
            }
        }

        private void DrawAutomatonBar(Vector2 origin, IPlayerCharacter player)
        {
            MCHGauge gauge = Plugin.JobGauges.Get<MCHGauge>();

            if (!gauge.IsRobotActive && _robotMaxDurationSet)
            {
                _robotMaxDurationSet = false;
            }

            if (!Config.AutomatonBar.HideWhenInactive || gauge.IsRobotActive)
            {
                float remaining = gauge.SummonTimeRemaining / 1000f;
                if (!_robotMaxDurationSet && gauge.IsRobotActive)
                {
                    _robotMaxDuration = remaining;
                    _robotMaxDurationSet = true;
                }
                
                Config.AutomatonBar.Label.SetValue(remaining);

                BarHud bar = BarUtilities.GetProgressBar(Config.AutomatonBar, remaining, _robotMaxDuration > 0 ? _robotMaxDuration : 1f, 0f, player);
                AddDrawActions(bar.GetDrawActions(origin, Config.AutomatonBar.StrataLevel));
            }
        }

        private void DrawOverheatBar(Vector2 origin, IPlayerCharacter player)
        {
            MCHGauge gauge = Plugin.JobGauges.Get<MCHGauge>();
            ushort stackCount = Utils.StatusListForBattleChara(player).FirstOrDefault(o => o.StatusId is 2688)?.Param ?? 0;

            if (!Config.OverheatChunkedGauge.HideWhenInactive || stackCount > 0)
            {
                LabelConfig[]? labels = null;
                if (Config.OverheatChunkedGauge.DurationLabel.Enabled)
                {
                    Config.OverheatChunkedGauge.DurationLabel.SetValue(gauge.OverheatTimeRemaining / 1000f);
                    labels = new LabelConfig[5];
                    labels[2] = Config.OverheatChunkedGauge.DurationLabel;
                }

                BarHud[] bars = BarUtilities.GetChunkedBars(Config.OverheatChunkedGauge, 5, stackCount, 5, labels: labels );
                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.OverheatChunkedGauge.StrataLevel));
                }
            }
        }

        private void DrawWildfireBar(Vector2 origin, IPlayerCharacter player)
        {
            float wildfireDuration = Utils.StatusListForBattleChara(player).FirstOrDefault(o => o.StatusId is 1946)?.RemainingTime ?? 0f;

            if (!Config.WildfireBar.HideWhenInactive || wildfireDuration > 0)
            {
                Config.WildfireBar.Label.SetValue(wildfireDuration);

                BarHud bar = BarUtilities.GetProgressBar(Config.WildfireBar, wildfireDuration, 10, 0f, player);
                AddDrawActions(bar.GetDrawActions(origin, Config.WildfireBar.StrataLevel));
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
        public MachinistOverheatBarConfig OverheatChunkedGauge = new MachinistOverheatBarConfig(
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
            new Vector2(-31, -10),
            new Vector2(192, 20),
            new PluginConfigColor(new Vector4(0, 0, 0, 0))
        );

        [NestedConfig("Automaton Queen Bar", 45)]
        public ProgressBarConfig AutomatonBar = new ProgressBarConfig(
            new Vector2(97, -10),
            new Vector2(60, 20),
            new PluginConfigColor(new Vector4(153f / 255f, 0f / 255f, 255f / 255f, 100f / 100f))
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

        public MachinistBatteryGaugeConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor) : base(position, size, fillColor)
        {
        }
    }

    [Exportable(false)]
    public class MachinistOverheatBarConfig : ChunkedBarConfig
    {
        [NestedConfig("Duration Text", 1000)]
        public NumericLabelConfig DurationLabel;

        public MachinistOverheatBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
             : base(position, size, fillColor)
        {
            DurationLabel = new NumericLabelConfig(Vector2.Zero, "", DrawAnchor.Center, DrawAnchor.Center);
            DurationLabel.HideIfZero = true;
        }
    }
}