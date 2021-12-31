using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
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

            if (Config.SurgingTempestBar.Enabled)
            {
                positions.Add(Config.Position + Config.SurgingTempestBar.Position);
                sizes.Add(Config.SurgingTempestBar.Size);
            }

            if (Config.BeastGauge.Enabled)
            {
                positions.Add(Config.Position + Config.BeastGauge.Position);
                sizes.Add(Config.BeastGauge.Size);
            }

            if (Config.InnerReleaseBar.Enabled)
            {
                positions.Add(Config.Position + Config.InnerReleaseBar.Position);
                sizes.Add(Config.InnerReleaseBar.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            Vector2 pos = origin + Config.Position;

            if (Config.SurgingTempestBar.Enabled)
            {
                DrawSurgingTempestBar(pos, player);
            }

            if (Config.BeastGauge.Enabled)
            {
                DrawBeastGauge(pos, player);
            }

            if (Config.InnerReleaseBar.Enabled)
            {
                DrawInnerReleaseBar(pos, player);
            }
        }

        private void DrawSurgingTempestBar(Vector2 origin, PlayerCharacter player)
        {
            float surgingTempestDuration = Math.Abs(player.StatusList.FirstOrDefault(o => o.StatusId is 2677)?.RemainingTime ?? 0f);

            if (!Config.SurgingTempestBar.HideWhenInactive || surgingTempestDuration > 0)
            {
                Config.SurgingTempestBar.Label.SetValue(surgingTempestDuration);

                BarHud bar = BarUtilities.GetProgressBar(Config.SurgingTempestBar, surgingTempestDuration, 60f, 0, player);
                AddDrawActions(bar.GetDrawActions(origin, Config.SurgingTempestBar.StrataLevel));
            }
        }

        private void DrawBeastGauge(Vector2 origin, PlayerCharacter player)
        {
            WARGauge gauge = Plugin.JobGauges.Get<WARGauge>();
            var nascentChaosDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 1897)?.RemainingTime ?? 0f;

            if (!Config.BeastGauge.HideWhenInactive || gauge.BeastGauge > 0 || nascentChaosDuration > 0)
            {
                Config.BeastGauge.Label.SetValue(gauge.BeastGauge);

                var color = nascentChaosDuration == 0 ? Config.BeastGauge.BeastGaugeColor : Config.BeastGauge.NascentChaosColor;
                BarHud[] bars = BarUtilities.GetChunkedProgressBars(Config.BeastGauge, 2, gauge.BeastGauge, 100, 0, player, fillColor: color);
                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.BeastGauge.StrataLevel));
                }
            }
        }

        private void DrawInnerReleaseBar(Vector2 origin, PlayerCharacter player)
        {
            var innerReleaseStatus = player.StatusList.FirstOrDefault(o => o.StatusId is 1177 or 86);

            float innerReleaseDuration = Math.Max(innerReleaseStatus?.RemainingTime ?? 0f, 0f);
            byte innerReleaseStacks = innerReleaseStatus?.StackCount ?? 0;

            BarGlowConfig? primalRendGlow = null;
            if (Config.InnerReleaseBar.PrimalRendReadyGlowConfig.Enabled)
            {
                bool isPrimalRendReady = player.StatusList.FirstOrDefault(o => o.StatusId is 2624)?.RemainingTime > 0;
                if (isPrimalRendReady)
                {
                    primalRendGlow = Config.InnerReleaseBar.PrimalRendReadyGlowConfig;
                }
            }

            if (innerReleaseStacks == 0 && Config.InnerReleaseBar.ShowCooldown)
            {
                uint spellID = 7389;
                float maxDuration = SpellHelper.Instance.GetRecastTime(spellID);
                float cooldown = SpellHelper.Instance.GetSpellCooldown(spellID);
                float currentCooldown = maxDuration - cooldown;

                Config.InnerReleaseBar.Label.SetValue(maxDuration - currentCooldown);

                if (currentCooldown == maxDuration)
                {
                    if (!Config.InnerReleaseBar.HideWhenInactive)
                    {
                        BarHud[] bars = BarUtilities.GetChunkedProgressBars(
                            Config.InnerReleaseBar,
                            1,
                            1,
                            1,
                            0,
                            player,
                            fillColor: Config.InnerReleaseBar.CooldownFinishedColor,
                            glowConfig: primalRendGlow,
                            chunksToGlow: new[] { true }
                        );

                        foreach (BarHud bar in bars)
                        {
                            AddDrawActions(bar.GetDrawActions(origin, Config.InnerReleaseBar.StrataLevel));
                        }
                    }
                }
                else
                {
                    BarHud[] bars = BarUtilities.GetChunkedProgressBars(
                        Config.InnerReleaseBar,
                        1,
                        currentCooldown,
                        maxDuration,
                        0,
                        player,
                        fillColor: Config.InnerReleaseBar.CooldownInProgressColor,
                        glowConfig: primalRendGlow,
                        chunksToGlow: new[] { true }
                    );

                    foreach (BarHud bar in bars)
                    {
                        AddDrawActions(bar.GetDrawActions(origin, Config.InnerReleaseBar.StrataLevel));
                    }
                }

                return;
            }

            if (!Config.InnerReleaseBar.HideWhenInactive || innerReleaseStacks > 0)
            {
                float innerReleaseMaxDuration = 15f;
                Config.InnerReleaseBar.Label.SetValue(innerReleaseDuration);

                if (!Config.InnerReleaseBar.ShowBuffTimerOnActiveChunk)
                {
                    innerReleaseDuration = innerReleaseMaxDuration;
                }
                else
                {
                    innerReleaseDuration = Math.Min(innerReleaseDuration, innerReleaseMaxDuration);
                }

                BarHud[] bars = BarUtilities.GetChunkedProgressBars(
                    Config.InnerReleaseBar,
                    3,
                    (innerReleaseStacks - 1) * innerReleaseMaxDuration + innerReleaseDuration,
                    3 * innerReleaseMaxDuration,
                    0,
                    player,
                    primalRendGlow,
                    chunksToGlow: new[] { true, true, true }
                );

                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.InnerReleaseBar.StrataLevel));
                }
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

            config.BeastGauge.UsePartialFillColor = true;
            config.InnerReleaseBar.LabelMode = LabelMode.ActiveChunk;
            config.InnerReleaseBar.PrimalRendReadyGlowConfig.Color = new PluginConfigColor(new Vector4(246f / 255f, 30f / 255f, 136f / 255f, 100f / 100f));

            return config;
        }

        [NestedConfig("Surging Tempest Bar", 30)]
        public ProgressBarConfig SurgingTempestBar = new ProgressBarConfig(
            new(0, -32),
            new(254, 20),
            new PluginConfigColor(new Vector4(255f / 255f, 136f / 255f, 146f / 255f, 100f / 100f))
        );

        [NestedConfig("Beast Gauge", 35)]
        public WarriorBeastGaugeConfig BeastGauge = new WarriorBeastGaugeConfig(
            new(0, -10),
            new(254, 20),
            new PluginConfigColor(new Vector4(0, 0, 0, 0))
        );

        [NestedConfig("Inner Release Bar", 40)]
        public WarriorInnerReleaseBarConfig InnerReleaseBar = new WarriorInnerReleaseBarConfig(
            new(0, -54),
            new(254, 20),
            new PluginConfigColor(new Vector4(255f / 255f, 136f / 255f, 146f / 255f, 100f / 100f))
        );
    }

    [DisableParentSettings("FillColor")]
    [Exportable(false)]
    public class WarriorBeastGaugeConfig : ChunkedProgressBarConfig
    {
        [ColorEdit4("Beast Gauge Color", spacing = true)]
        [Order(65)]
        public PluginConfigColor BeastGaugeColor = new(new Vector4(201f / 255f, 13f / 255f, 13f / 255f, 100f / 100f));

        [ColorEdit4("Nascent Chaos Color")]
        [Order(70)]
        public PluginConfigColor NascentChaosColor = new(new Vector4(240f / 255f, 176f / 255f, 0f / 255f, 100f / 100f));

        public WarriorBeastGaugeConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor) : base(position, size, fillColor)
        {
        }
    }

    [Exportable(false)]
    public class WarriorInnerReleaseBarConfig : ChunkedProgressBarConfig
    {
        [Checkbox("Show Buff Timer On Active Chunk", spacing = true)]
        [Order(80)]
        public bool ShowBuffTimerOnActiveChunk;

        [Checkbox("Show Inner Release Cooldown", spacing = true)]
        [Order(85)]
        public bool ShowCooldown;

        [ColorEdit4("Inner Release On Cooldown Color")]
        [Order(90)]
        public PluginConfigColor CooldownInProgressColor = new(new Vector4(240f / 255f, 176f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Inner Release Ready Color")]
        [Order(95)]
        public PluginConfigColor CooldownFinishedColor = new(new Vector4(38f / 255f, 192f / 255f, 94f / 255f, 100f / 100f));

        [NestedConfig("Glow Color (when Primal Rend is ready)", 100, separator = false, spacing = true)]
        public BarGlowConfig PrimalRendReadyGlowConfig = new();

        public WarriorInnerReleaseBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor) : base(position, size, fillColor)
        {
        }
    }
}
