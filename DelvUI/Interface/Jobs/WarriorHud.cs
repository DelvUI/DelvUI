﻿using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
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
    public class WarriorHud : JobHud
    {
        private new WarriorConfig Config => (WarriorConfig)_config;

        public WarriorHud(WarriorConfig config, string? displayName = null) : base(config, displayName)
        {
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new();
            List<Vector2> sizes = new();

            if (Config.StormsEyeBar.Enabled)
            {
                positions.Add(Config.Position + Config.StormsEyeBar.Position);
                sizes.Add(Config.StormsEyeBar.Size);
            }

            if (Config.BeastGauge.Enabled)
            {
                positions.Add(Config.Position + Config.BeastGauge.Position);
                sizes.Add(Config.BeastGauge.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            Vector2 pos = origin + Config.Position;

            if (Config.StormsEyeBar.Enabled)
            {
                DrawStormsEyeBar(pos, player);
            }

            if (Config.BeastGauge.Enabled)
            {
                DrawBeastGauge(pos, player);
            }
        }

        private void DrawStormsEyeBar(Vector2 origin, PlayerCharacter player)
        {
            float innerReleaseDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 1177 or 86)?.RemainingTime ?? 0f;
            float stormsEyeDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 90)?.RemainingTime ?? 0f;

            if ((!Config.StormsEyeBar.HideWhenInactive || stormsEyeDuration > 0) && innerReleaseDuration is 0)
            {
                Config.StormsEyeBar.Label.SetText(Math.Truncate(stormsEyeDuration).ToString());
                BarUtilities.GetProgressBar(Config.StormsEyeBar, stormsEyeDuration, 60f, 0f, player, Config.StormsEyeBar.StormsEyeColor).Draw(origin);
            }
            if (innerReleaseDuration > 0)
            {
                Config.StormsEyeBar.Label.SetText(Math.Truncate(innerReleaseDuration).ToString());
                BarUtilities.GetProgressBar(Config.StormsEyeBar, innerReleaseDuration, 10f, 0f, player, Config.StormsEyeBar.InnerReleaseColor).Draw(origin);
            }
        }

        private void DrawBeastGauge(Vector2 origin, PlayerCharacter player)
        {
            WARGauge gauge = Plugin.JobGauges.Get<WARGauge>();
            var nascentChaosDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 1897)?.RemainingTime ?? 0f;

            if (!Config.BeastGauge.HideWhenInactive || gauge.BeastGauge > 0 || nascentChaosDuration > 0)
            {
                Config.BeastGauge.Label.SetText(gauge.BeastGauge.ToString("N0"));
                BarUtilities.GetProgressBar(Config.BeastGauge, gauge.BeastGauge, 100, 0f, player, nascentChaosDuration > 0 ? Config.BeastGauge.NascentChaosColor : Config.BeastGauge.BeastGaugeColor).Draw(origin);
            }
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Tank", 0)]
    [SubSection("Warrior", 1)]
    public class WarriorConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.WAR;
        public new static WarriorConfig DefaultConfig()
        {
            var config = new WarriorConfig();

            config.StormsEyeBar.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.BeastGauge.Label.FontID = FontsConfig.DefaultMediumFontKey;

            return config;
        }

        [NestedConfig("Storm's Eye Bar", 30)]
        public StormsEyeBarConfig StormsEyeBar = new StormsEyeBarConfig(
            new(0, -32),
            new(254, 20),
            new PluginConfigColor(new Vector4(0, 0, 0, 0))
        );

        [NestedConfig("Beast Gauge", 35)]
        public BeastGaugeConfig BeastGauge = new BeastGaugeConfig(
            new(0, -10),
            new(254, 20),
            new PluginConfigColor(new Vector4(0, 0, 0, 0))
        );
    }

    [DisableParentSettings("FillColor")]
    [Exportable(false)]
    public class StormsEyeBarConfig : ProgressBarConfig
    {
        [ColorEdit4("Storm's Eye Color", spacing = true)]
        [Order(55)]
        public PluginConfigColor StormsEyeColor = new(new Vector4(255f / 255f, 136f / 255f, 146f / 255f, 100f / 100f));

        [ColorEdit4("Inner Release")]
        [Order(60)]
        public PluginConfigColor InnerReleaseColor = new(new Vector4(255f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        public StormsEyeBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor) : base(position, size, fillColor)
        {
        }
    }

    [DisableParentSettings("FillColor")]
    [Exportable(false)]
    public class BeastGaugeConfig : ProgressBarConfig
    {
        [ColorEdit4("Beast Gauge Color", spacing = true)]
        [Order(65)]
        public PluginConfigColor BeastGaugeColor = new(new Vector4(201f / 255f, 13f / 255f, 13f / 255f, 100f / 100f));

        [ColorEdit4("Nascent Chaos Color")]
        [Order(70)]
        public PluginConfigColor NascentChaosColor = new(new Vector4(240f / 255f, 176f / 255f, 0f / 255f, 100f / 100f));

        public BeastGaugeConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor) : base(position, size, fillColor)
        {
        }
    }
}
