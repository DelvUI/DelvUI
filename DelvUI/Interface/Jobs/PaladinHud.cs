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
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;

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

            if (Config.ManaBar.Enabled)
            {
                positions.Add(Config.Position + Config.ManaBar.Position);
                sizes.Add(Config.ManaBar.Size);
            }

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

            if (Config.RequiescatBar.Enabled)
            {
                positions.Add(Config.Position + Config.RequiescatBar.Position);
                sizes.Add(Config.RequiescatBar.Size);
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

            if (Config.ManaBar.Enabled)
            {
                DrawManaBar(pos, player);
            }

            if (Config.OathGauge.Enabled)
            {
                DrawOathGauge(pos);
            }

            if (Config.FightOrFlightBar.Enabled)
            {
                DrawFightOrFlightBar(pos, player);
            }

            if (Config.RequiescatBar.Enabled)
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

        private void DrawManaBar(Vector2 origin, PlayerCharacter player)
        {
            if (Config.ManaBar.HideWhenInactive && player.CurrentMp == player.MaxMp) { return; }

            Config.ManaBar.Label.SetText($"{player.CurrentMp,0}");
            BarUtilities.GetChunkedProgressBars(Config.ManaBar, 5, player.CurrentMp, player.MaxMp, 0f, player).Draw(origin);
        }

        private void DrawOathGauge(Vector2 origin)
        {
            PLDGauge gauge = Plugin.JobGauges.Get<PLDGauge>();

            if (!Config.OathGauge.HideWhenInactive || gauge.OathGauge > 0)
            {
                Config.OathGauge.Label.SetText(gauge.OathGauge.ToString("N0"));
                BarUtilities.GetProgressBar(Config.OathGauge, gauge.OathGauge, 100, 0f).Draw(origin);
            }
        }

        private void DrawFightOrFlightBar(Vector2 origin, PlayerCharacter player)
        {
            float fightOrFlightDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 76)?.RemainingTime ?? 0f;

            if (!Config.FightOrFlightBar.HideWhenInactive || fightOrFlightDuration > 0)
            {
                Config.FightOrFlightBar.Label.SetText(Math.Abs(fightOrFlightDuration).ToString("N0"));
                BarUtilities.GetProgressBar(Config.FightOrFlightBar, fightOrFlightDuration, 25f, 0f, player).Draw(origin);
            }
        }

        private void DrawRequiescatBar(Vector2 origin, PlayerCharacter player)
        {
            float requiescatDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 1368)?.RemainingTime ?? 0f;

            if (!Config.RequiescatBar.HideWhenInactive || requiescatDuration > 0)
            {
                Config.RequiescatBar.Label.SetText(Math.Abs(requiescatDuration).ToString("N0"));
                BarUtilities.GetProgressBar(Config.RequiescatBar, requiescatDuration, 12f, 0f, player).Draw(origin);
            }
        }

        private void DrawAtonementBar(Vector2 origin, PlayerCharacter player)
        {
            byte stackCount = player.StatusList.FirstOrDefault(o => o.StatusId is 1902)?.StackCount ?? 0;

            if (Config.AtonementBar.HideWhenInactive && stackCount == 0) { return; };

            BarUtilities.GetChunkedProgressBars(Config.AtonementBar, 3, stackCount, 3f)
                .Draw(origin);
        }

        private void DrawDoTBar(Vector2 origin, PlayerCharacter player)
        {
            var actor = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            if (actor is BattleChara target)
            {
                var dotDuration = target.StatusList.FirstOrDefault(o => o.StatusId is 725 && o.SourceID == player.ObjectId)?.RemainingTime ?? 0f;
                if (!Config.GoringBladeBar.HideWhenInactive || dotDuration > 0)
                {
                    Config.GoringBladeBar.Label.SetText(Math.Truncate(dotDuration).ToString());
                    BarUtilities.GetProgressBar(Config.GoringBladeBar, dotDuration, 21f, 0f, player).Draw(origin);
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

            config.OathGauge.ThresholdConfig.Enabled = true;
            config.GoringBladeBar.ThresholdConfig.Enabled = true;
            config.OathGauge.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.FightOrFlightBar.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.RequiescatBar.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.GoringBladeBar.Label.FontID = FontsConfig.DefaultMediumFontKey;

            return config;
        }

        [NestedConfig("Mana Bar", 30)]
        public ChunkedProgressBarConfig ManaBar = new ChunkedProgressBarConfig(
            new Vector2(0, -76),
            new Vector2(254, 20),
            new PluginConfigColor(new Vector4(0f / 255f, 203f / 255f, 230f / 255f, 100f / 100f))
        );

        [NestedConfig("Oath Gauge", 35)]
        public ProgressBarConfig OathGauge = new ProgressBarConfig(
            new Vector2(0, -54),
            new Vector2(254, 20),
            new PluginConfigColor(new Vector4(24f / 255f, 80f / 255f, 175f / 255f, 100f / 100f)),
            BarDirection.Right,
            new PluginConfigColor(new Vector4(180f / 255f, 180f / 255f, 180f / 255f, 100f / 100f)),
            50
        );

        [NestedConfig("Fight or Flight Bar", 40)]
        public ProgressBarConfig FightOrFlightBar = new ProgressBarConfig(
            new Vector2(-64, -32),
            new Vector2(126, 20),
            new PluginConfigColor(new Vector4(240f / 255f, 50f / 255f, 0f / 255f, 100f / 100f))
        );

        [NestedConfig("Requiescat Bar", 45)]
        public ProgressBarConfig RequiescatBar = new ProgressBarConfig(
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

        [NestedConfig("Goring Blade Bar", 55)]
        public ProgressBarConfig GoringBladeBar = new ProgressBarConfig(
            new(0, -98),
            new(254, 20),
            new PluginConfigColor(new Vector4(255f / 255f, 128f / 255f, 0f / 255f, 100f / 100f)),
            BarDirection.Right,
            new PluginConfigColor(new Vector4(233f / 255f, 33f / 255f, 33f / 255f, 53f / 100f)),
            5
        );
    }
}
