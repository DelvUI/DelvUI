using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Statuses;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Principal;

namespace DelvUI.Interface.Jobs
{
    public class PaladinHud : JobHud
    {
        private new PaladinConfig Config => (PaladinConfig)_config;

        public PaladinHud(PaladinConfig config, string? displayName = null) : base(config, displayName)
        {
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new();
            List<Vector2> sizes = new();

            if (Config.OathGauge.Enabled)
            {
                positions.Add(Config.Position + Config.OathGauge.Position);
                sizes.Add(Config.OathGauge.Size);
            }

            if (Config.FightOrFlightBar.Enabled)
            {
                positions.Add(Config.Position + Config.FightOrFlightBar.Position);
                sizes.Add(Config.FightOrFlightBar.Size);
            }

            if (Config.RequiescatStacksBar.Enabled)
            {
                positions.Add(Config.Position + Config.RequiescatStacksBar.Position);
                sizes.Add(Config.RequiescatStacksBar.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, IPlayerCharacter player)
        {
            Vector2 pos = origin + Config.Position;

            if (Config.OathGauge.Enabled)
            {
                DrawOathGauge(pos, player);
            }

            if (Config.FightOrFlightBar.Enabled)
            {
                DrawFightOrFlightBar(pos, player);
            }

            if (Config.RequiescatStacksBar.Enabled)
            {
                DrawRequiescatBar(pos, player);
            }
        }

        private void DrawOathGauge(Vector2 origin, IPlayerCharacter player)
        {
            PLDGauge gauge = Plugin.JobGauges.Get<PLDGauge>();

            if (!Config.OathGauge.HideWhenInactive || gauge.OathGauge > 0)
            {
                Config.OathGauge.Label.SetValue(gauge.OathGauge);

                BarHud[] bars = BarUtilities.GetChunkedProgressBars(Config.OathGauge, 2, gauge.OathGauge, 100, 0, player);
                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.OathGauge.StrataLevel));
                }
            }
        }

        private void DrawFightOrFlightBar(Vector2 origin, IPlayerCharacter player)
        {
            float fightOrFlightDuration = Utils.StatusListForBattleChara(player).FirstOrDefault(o => o.StatusId is 76)?.RemainingTime ?? 0f;

            if (!Config.FightOrFlightBar.HideWhenInactive || fightOrFlightDuration > 0)
            {
                Config.FightOrFlightBar.Label.SetValue(fightOrFlightDuration);

                BarHud bar = BarUtilities.GetProgressBar(Config.FightOrFlightBar, fightOrFlightDuration, 20f, 0f, player);
                AddDrawActions(bar.GetDrawActions(origin, Config.FightOrFlightBar.StrataLevel));
            }
        }

        private void DrawRequiescatBar(Vector2 origin, IPlayerCharacter player)
        {
            IStatus? requiescatBuff = Utils.StatusListForBattleChara(player).FirstOrDefault(o => o.StatusId is 1368);
            float requiescatDuration = Math.Max(0f, requiescatBuff?.RemainingTime ?? 0f);
            int stacks = requiescatBuff?.Param ?? 0;

            if (!Config.RequiescatStacksBar.HideWhenInactive || requiescatDuration > 0)
            {
                Config.RequiescatStacksBar.Label.SetValue(requiescatDuration);

                LabelConfig[] labels = new LabelConfig[4];
                labels[2] = Config.RequiescatStacksBar.Label;

                BarHud[] bars = BarUtilities.GetChunkedBars(Config.RequiescatStacksBar, 4, stacks, 4, labels: labels);
                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.RequiescatStacksBar.StrataLevel));
                }
            }
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Tank", 0)]
    [SubSection("Paladin", 1)]
    public class PaladinConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.PLD;

        public new static PaladinConfig DefaultConfig()
        {
            var config = new PaladinConfig();

            config.UseDefaultPrimaryResourceBar = true;
            config.OathGauge.UsePartialFillColor = true;
            config.RequiescatStacksBar.Label.Enabled = true;

            return config;
        }

        [NestedConfig("Oath Gauge", 35)]
        public ChunkedProgressBarConfig OathGauge = new ChunkedProgressBarConfig(
            new Vector2(0, -54),
            new Vector2(254, 20),
            new PluginConfigColor(new Vector4(24f / 255f, 80f / 255f, 175f / 255f, 100f / 100f)),
            2,
            new PluginConfigColor(new Vector4(180f / 255f, 180f / 255f, 180f / 255f, 100f / 100f))
        );

        [NestedConfig("Fight or Flight Bar", 40)]
        public ProgressBarConfig FightOrFlightBar = new ProgressBarConfig(
            new Vector2(-64, -32),
            new Vector2(126, 20),
            new PluginConfigColor(new Vector4(240f / 255f, 50f / 255f, 0f / 255f, 100f / 100f))
        );

        [NestedConfig("Requiescat Bar", 45)]
        public ChunkedProgressBarConfig RequiescatStacksBar = new ChunkedProgressBarConfig(
            new Vector2(64, -32),
            new Vector2(126, 20),
            new PluginConfigColor(new Vector4(61f / 255f, 61f / 255f, 255f / 255f, 100f / 100f))
        );
    }
}
