using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
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
    public class DragoonHud : JobHud
    {
        private new DragoonConfig Config => (DragoonConfig)_config;

        private static readonly List<uint> ChaosThrustIDs = new() { 118, 1312, 2719 };
        private static readonly List<float> ChaosThrustDurations = new() { 24, 24, 24 };

        public DragoonHud(DragoonConfig config, string? displayName = null) : base(config, displayName)
        {
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.ChaosThrustBar.Enabled)
            {
                positions.Add(Config.Position + Config.ChaosThrustBar.Position);
                sizes.Add(Config.ChaosThrustBar.Size);
            }

            if (Config.PowerSurgeBar.Enabled)
            {
                positions.Add(Config.Position + Config.PowerSurgeBar.Position);
                sizes.Add(Config.PowerSurgeBar.Size);
            }

            if (Config.EyeOfTheDragonBar.Enabled)
            {
                positions.Add(Config.Position + Config.EyeOfTheDragonBar.Position);
                sizes.Add(Config.EyeOfTheDragonBar.Size);
            }

            if (Config.FirstmindsFocusBar.Enabled)
            {
                positions.Add(Config.Position + Config.FirstmindsFocusBar.Position);
                sizes.Add(Config.FirstmindsFocusBar.Size);
            }

            if (Config.LifeOfTheDragonBar.Enabled)
            {
                positions.Add(Config.Position + Config.LifeOfTheDragonBar.Position);
                sizes.Add(Config.LifeOfTheDragonBar.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            var position = origin + Config.Position;
            if (Config.ChaosThrustBar.Enabled)
            {
                DrawChaosThrustBar(position, player);
            }

            if (Config.PowerSurgeBar.Enabled)
            {
                DrawPowerSurgeBar(position, player);
            }

            if (Config.EyeOfTheDragonBar.Enabled)
            {
                DrawEyeOfTheDragonBars(position, player);
            }

            if (Config.FirstmindsFocusBar.Enabled)
            {
                DrawFirstmindsFocusBars(position, player);
            }

            if (Config.LifeOfTheDragonBar.Enabled)
            {
                DrawBloodOfTheDragonBar(position, player);
            }
        }

        private void DrawChaosThrustBar(Vector2 origin, PlayerCharacter player)
        {
            GameObject? target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;

            BarHud? bar = BarUtilities.GetDoTBar(Config.ChaosThrustBar, player, target, ChaosThrustIDs, ChaosThrustDurations);
            if (bar != null)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.ChaosThrustBar.StrataLevel));
            }
        }

        private void DrawEyeOfTheDragonBars(Vector2 origin, PlayerCharacter player)
        {
            DRGGauge gauge = Plugin.JobGauges.Get<DRGGauge>();

            if (!Config.EyeOfTheDragonBar.HideWhenInactive || gauge.EyeCount > 0)
            {
                BarHud[] bars = BarUtilities.GetChunkedBars(Config.EyeOfTheDragonBar, 2, gauge.EyeCount, 2, 0, player);
                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.EyeOfTheDragonBar.StrataLevel));
                }
            }
        }

        private void DrawFirstmindsFocusBars(Vector2 origin, PlayerCharacter player)
        {
            DRGGauge gauge = Plugin.JobGauges.Get<DRGGauge>();

            if (!Config.FirstmindsFocusBar.HideWhenInactive || gauge.FirstmindsFocusCount > 0)
            {
                BarHud[] bars = BarUtilities.GetChunkedBars(Config.FirstmindsFocusBar, 2, gauge.FirstmindsFocusCount, 2, 0, player);
                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.FirstmindsFocusBar.StrataLevel));
                }
            }
        }

        private void DrawBloodOfTheDragonBar(Vector2 origin, PlayerCharacter player)
        {
            DRGGauge gauge = Plugin.JobGauges.Get<DRGGauge>();
            float duration = gauge.LOTDTimer / 1000f;

            if (!Config.LifeOfTheDragonBar.HideWhenInactive || duration > 0f)
            {
                Config.LifeOfTheDragonBar.Label.SetValue(duration);

                BarHud bar = BarUtilities.GetProgressBar(Config.LifeOfTheDragonBar, duration, 30, 0f, player);
                AddDrawActions(bar.GetDrawActions(origin, Config.LifeOfTheDragonBar.StrataLevel));
            }
        }

        private void DrawPowerSurgeBar(Vector2 origin, PlayerCharacter player)
        {
            var duration = Math.Abs(player.StatusList.FirstOrDefault(o => o.StatusId is 2720)?.RemainingTime ?? 0f);
            if (!Config.PowerSurgeBar.HideWhenInactive || duration > 0f)
            {
                Config.PowerSurgeBar.Label.SetValue(duration);

                BarHud bar = BarUtilities.GetProgressBar(Config.PowerSurgeBar, duration, 30f, 0f, player);
                AddDrawActions(bar.GetDrawActions(origin, Config.PowerSurgeBar.StrataLevel));
            }
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Melee", 0)]
    [SubSection("Dragoon", 1)]
    public class DragoonConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.DRG;
        public new static DragoonConfig DefaultConfig() { return new DragoonConfig(); }

        [NestedConfig("Chaos Thrust", 30)]
        public ProgressBarConfig ChaosThrustBar = new ProgressBarConfig(
            new(0, -76),
            new(254, 20),
            new(new Vector4(217f / 255f, 145f / 255f, 232f / 255f, 100f / 100f))
        );

        [NestedConfig("Power Surge", 35)]
        public ProgressBarConfig PowerSurgeBar = new ProgressBarConfig(
            new(0, -54),
            new(254, 20),
            new(new Vector4(244f / 255f, 206f / 255f, 191f / 255f, 100f / 100f))
        );

        [NestedConfig("Eye of the Dragon", 40)]
        public ChunkedBarConfig EyeOfTheDragonBar = new ChunkedBarConfig(
            new(-64, -32),
            new(126, 20),
            new(new Vector4(255f / 255f, 125f / 255f, 125f / 255f, 100f / 100f))
        );

        [NestedConfig("Firstminds' Focus", 40)]
        public ChunkedBarConfig FirstmindsFocusBar = new ChunkedBarConfig(
            new(64, -32),
            new(126, 20),
            new PluginConfigColor(new(134f / 255f, 120f / 255f, 255f / 255f, 100f / 100f))
        );

        [NestedConfig("Life of the Dragon", 45)]
        public ProgressBarConfig LifeOfTheDragonBar = new ProgressBarConfig(
            new(0, -10),
            new(254, 20),
            new(new Vector4(185f / 255f, 0f / 255f, 25f / 255f, 100f / 100f))
        );
    }
}
