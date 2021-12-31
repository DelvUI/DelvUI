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

namespace DelvUI.Interface.Jobs
{
    public class PaladinHud : JobHud
    {
        private new PaladinConfig Config => (PaladinConfig)_config;

        // goring blade and blade of valor
        private static readonly List<uint> DoTIDs = new() { 725, 2721 };
        private static readonly List<float> DoTDurations = new() { 21, 21 };

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

            if (Config.AtonementBar.Enabled)
            {
                positions.Add(Config.Position + Config.AtonementBar.Position);
                sizes.Add(Config.AtonementBar.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
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

            if (Config.AtonementBar.Enabled)
            {
                DrawAtonementBar(pos, player);
            }

            if (Config.GoringBladeBar.Enabled)
            {
                DrawDoTBar(pos, player);
            }
        }

        private void DrawOathGauge(Vector2 origin, PlayerCharacter player)
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

        private void DrawFightOrFlightBar(Vector2 origin, PlayerCharacter player)
        {
            float fightOrFlightDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 76)?.RemainingTime ?? 0f;

            if (!Config.FightOrFlightBar.HideWhenInactive || fightOrFlightDuration > 0)
            {
                Config.FightOrFlightBar.Label.SetValue(fightOrFlightDuration);

                BarHud bar = BarUtilities.GetProgressBar(Config.FightOrFlightBar, fightOrFlightDuration, 25f, 0f, player);
                AddDrawActions(bar.GetDrawActions(origin, Config.FightOrFlightBar.StrataLevel));
            }
        }

        private void DrawRequiescatBar(Vector2 origin, PlayerCharacter player)
        {
            Status? requiescatBuff = player.StatusList.FirstOrDefault(o => o.StatusId is 1368);
            float requiescatDuration = Math.Max(0f, requiescatBuff?.RemainingTime ?? 0f);
            byte stacks = requiescatBuff?.StackCount ?? 0;

            if (!Config.RequiescatStacksBar.HideWhenInactive || requiescatDuration > 0)
            {
                var chunks = new Tuple<PluginConfigColor, float, LabelConfig?>[5];

                for (int i = 0; i < 5; i++)
                {
                    chunks[i] = new(Config.RequiescatStacksBar.FillColor, i < stacks ? 1 : 0, i == 2 ? Config.RequiescatStacksBar.Label : null);
                }

                Config.RequiescatStacksBar.Label.SetValue(requiescatDuration);

                BarHud[] bars = BarUtilities.GetChunkedBars(Config.RequiescatStacksBar, chunks, player);
                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.RequiescatStacksBar.StrataLevel));
                }
            }
        }

        private void DrawAtonementBar(Vector2 origin, PlayerCharacter player)
        {
            byte stackCount = player.StatusList.FirstOrDefault(o => o.StatusId is 1902)?.StackCount ?? 0;

            if (Config.AtonementBar.HideWhenInactive && stackCount == 0) { return; };

            BarHud[] bars = BarUtilities.GetChunkedBars(Config.AtonementBar, 3, stackCount, 3f, 0, player);
            foreach (BarHud bar in bars)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.AtonementBar.StrataLevel));
            }
        }

        private void DrawDoTBar(Vector2 origin, PlayerCharacter player)
        {
            var target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;

            BarHud? bar = BarUtilities.GetDoTBar(Config.GoringBladeBar, player, target, DoTIDs, DoTDurations);
            if (bar != null)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.GoringBladeBar.StrataLevel));
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

        [NestedConfig("Atonement Bar", 50)]
        public ChunkedBarConfig AtonementBar = new ChunkedBarConfig(
            new Vector2(0, -10),
            new Vector2(254, 20),
            new PluginConfigColor(new Vector4(240f / 255f, 176f / 255f, 0f / 255f, 100f / 100f))
        );

        [NestedConfig("DoT Bar", 55)]
        public ProgressBarConfig GoringBladeBar = new ProgressBarConfig(
            new(0, -76),
            new(254, 20),
            new PluginConfigColor(new Vector4(255f / 255f, 128f / 255f, 0f / 255f, 100f / 100f)),
            BarDirection.Right,
            new PluginConfigColor(new Vector4(233f / 255f, 33f / 255f, 33f / 255f, 53f / 100f)),
            5
        );
    }
}
