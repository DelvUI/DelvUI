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
    public class WhiteMageHud : JobHud
    {
        private new WhiteMageConfig Config => (WhiteMageConfig)_config;

        private static readonly List<uint> DiaIDs = new() { 143, 144, 1871 };
        private static readonly List<float> DiaDurations = new() { 30, 30, 30 };

        public WhiteMageHud(WhiteMageConfig config, string? displayName = null) : base(config, displayName)
        {
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new();
            List<Vector2> sizes = new();

            if (Config.LilyBar.Enabled)
            {
                positions.Add(Config.Position + Config.LilyBar.Position);
                sizes.Add(Config.LilyBar.Size);
            }

            if (Config.DiaBar.Enabled)
            {
                positions.Add(Config.Position + Config.DiaBar.Position);
                sizes.Add(Config.DiaBar.Size);
            }

            if (Config.AsylumBar.Enabled)
            {
                positions.Add(Config.Position + Config.AsylumBar.Position);
                sizes.Add(Config.AsylumBar.Size);
            }

            if (Config.PresenceOfMindBar.Enabled)
            {
                positions.Add(Config.Position + Config.PresenceOfMindBar.Position);
                sizes.Add(Config.PresenceOfMindBar.Size);
            }

            if (Config.PlenaryBar.Enabled)
            {
                positions.Add(Config.Position + Config.PlenaryBar.Position);
                sizes.Add(Config.PlenaryBar.Size);
            }

            if (Config.TemperanceBar.Enabled)
            {
                positions.Add(Config.Position + Config.TemperanceBar.Position);
                sizes.Add(Config.TemperanceBar.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            Vector2 pos = origin + Config.Position;

            if (Config.LilyBar.Enabled)
            {
                DrawLilyBar(pos, player);
            }

            if (Config.DiaBar.Enabled)
            {
                DrawDiaBar(pos, player);
            }

            if (Config.AsylumBar.Enabled)
            {
                DrawAsylumBar(pos, player);
            }

            if (Config.PresenceOfMindBar.Enabled)
            {
                DrawPresenceOfMindBar(pos, player);
            }

            if (Config.PlenaryBar.Enabled)
            {
                DrawPlenaryBar(pos, player);

            }
            if (Config.TemperanceBar.Enabled)
            {
                DrawTemperanceBar(pos, player);
            }
        }

        private void DrawDiaBar(Vector2 origin, PlayerCharacter player)
        {
            var target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;

            BarHud? bar = BarUtilities.GetDoTBar(Config.DiaBar, player, target, DiaIDs, DiaDurations);
            if (bar != null)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.DiaBar.StrataLevel));
            }
        }

        private void DrawLilyBar(Vector2 origin, PlayerCharacter player)
        {
            WHMGauge gauge = Plugin.JobGauges.Get<WHMGauge>();

            const float lilyCooldown = 20000f;

            float GetScale(int num, float timer) => num + (timer / lilyCooldown);
            float lilyScale = GetScale(gauge.Lily, gauge.LilyTimer);

            if (!Config.LilyBar.HideWhenInactive || lilyScale > 0)
            {
                BarHud[] bars = BarUtilities.GetChunkedBars(Config.LilyBar, 3, lilyScale, 3, 0, player, partialFillColor: Config.LilyBar.PartialFillColor);
                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.LilyBar.StrataLevel));
                }
            }

            if (!Config.BloodLilyBar.HideWhenInactive && Config.BloodLilyBar.Enabled || gauge.BloodLily > 0)
            {
                BarGlowConfig? glow = gauge.BloodLily == 3 ? Config.BloodLilyBar.GlowConfig : null;
                BarHud[] bars = BarUtilities.GetChunkedBars(Config.BloodLilyBar, 3, gauge.BloodLily, 3, 0, player, glowConfig: glow, chunksToGlow: new[] { true, true, true });
                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.BloodLilyBar.StrataLevel));
                }
            }
        }

        private void DrawAsylumBar(Vector2 origin, PlayerCharacter player)
        {
            float asylymDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 739 or 1911 && o.SourceID == player.ObjectId)?.RemainingTime ?? 0f;

            if (!Config.AsylumBar.HideWhenInactive || asylymDuration > 0)
            {
                Config.AsylumBar.Label.SetValue(asylymDuration);

                BarHud bar = BarUtilities.GetProgressBar(Config.AsylumBar, asylymDuration, 24f, 0f, player);
                AddDrawActions(bar.GetDrawActions(origin, Config.AsylumBar.StrataLevel));
            }
        }

        private void DrawPresenceOfMindBar(Vector2 origin, PlayerCharacter player)
        {
            float presenceOfMindDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 157 && o.SourceID == player.ObjectId)?.RemainingTime ?? 0f;

            if (!Config.PresenceOfMindBar.HideWhenInactive || presenceOfMindDuration > 0)
            {
                Config.PresenceOfMindBar.Label.SetValue(presenceOfMindDuration);

                BarHud bar = BarUtilities.GetProgressBar(Config.PresenceOfMindBar, presenceOfMindDuration, 15f, 0f, player);
                AddDrawActions(bar.GetDrawActions(origin, Config.PresenceOfMindBar.StrataLevel));
            }
        }

        private void DrawPlenaryBar(Vector2 origin, PlayerCharacter player)
        {
            float plenaryDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 1219 && o.SourceID == player.ObjectId)?.RemainingTime ?? 0f;

            if (!Config.PlenaryBar.HideWhenInactive || plenaryDuration > 0)
            {
                Config.PlenaryBar.Label.SetValue(plenaryDuration);

                BarHud bar = BarUtilities.GetProgressBar(Config.PlenaryBar, plenaryDuration, 10f, 0f, player);
                AddDrawActions(bar.GetDrawActions(origin, Config.PlenaryBar.StrataLevel));
            }
        }

        private void DrawTemperanceBar(Vector2 origin, PlayerCharacter player)
        {
            float temperanceDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 1872 && o.SourceID == player.ObjectId)?.RemainingTime ?? 0f;

            if (!Config.TemperanceBar.HideWhenInactive || temperanceDuration > 0)
            {
                Config.TemperanceBar.Label.SetValue(temperanceDuration);

                BarHud bar = BarUtilities.GetProgressBar(Config.TemperanceBar, temperanceDuration, 20f, 0f, player);
                AddDrawActions(bar.GetDrawActions(origin, Config.TemperanceBar.StrataLevel));
            }
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Healer", 0)]
    [SubSection("White Mage", 1)]
    public class WhiteMageConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.WHM;
        public new static WhiteMageConfig DefaultConfig()
        {
            var config = new WhiteMageConfig();

            config.UseDefaultPrimaryResourceBar = true;

            config.AsylumBar.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.PresenceOfMindBar.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.PlenaryBar.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.TemperanceBar.Label.FontID = FontsConfig.DefaultMediumFontKey;

            return config;
        }

        [NestedConfig("Lily Bar", 30)]
        public LilyBarConfig LilyBar = new LilyBarConfig(
            new(-64, -32),
            new(126, 20),
            new PluginConfigColor(new(0f / 255f, 64f / 255f, 255f / 255f, 100f / 100f))
        );

        [NestedConfig("Blood Lily Bar", 35)]
        public BloodLilyBarConfig BloodLilyBar = new BloodLilyBarConfig(
            new(64, -32),
            new(126, 20),
            new PluginConfigColor(new(199f / 255f, 40f / 255f, 9f / 255f, 100f / 100f))
        );

        [NestedConfig("Dia Bar", 40)]
        public ProgressBarConfig DiaBar = new ProgressBarConfig(
            new(0, -10),
            new(254, 20),
            new PluginConfigColor(new(0f / 255f, 64f / 255f, 255f / 255f, 100f / 100f))
        );

        [NestedConfig("Asylum Bar", 45)]
        public ProgressBarConfig AsylumBar = new ProgressBarConfig(
            new(-96, -52),
            new(62, 15),
            new PluginConfigColor(new(241f / 255f, 217f / 255f, 125f / 255f, 100f / 100f))
        );

        [NestedConfig("Presence of Mind Bar", 50)]
        public ProgressBarConfig PresenceOfMindBar = new ProgressBarConfig(
            new(-32, -52),
            new(62, 15),
            new PluginConfigColor(new(213f / 255f, 124f / 255f, 97f / 255f, 100f / 100f))
        );

        [NestedConfig("Plenary Bar", 55)]
        public ProgressBarConfig PlenaryBar = new ProgressBarConfig(
            new(32, -52),
            new(62, 15),
            new PluginConfigColor(new(26f / 255f, 167f / 255f, 109f / 255f, 100f / 100f))
        );

        [NestedConfig("Temperance Bar", 60)]
        public ProgressBarConfig TemperanceBar = new ProgressBarConfig(
            new(96, -52),
            new(62, 15),
            new PluginConfigColor(new(100f / 255f, 207f / 255f, 211f / 255f, 100f / 100f))
        );
    }

    [Exportable(false)]
    public class LilyBarConfig : ChunkedBarConfig
    {
        [Checkbox("Use Partial Fill Color", spacing = true)]
        [Order(65)]
        public bool UsePartialFillColor = false;

        [ColorEdit4("Partial Fill Color")]
        [Order(66, collapseWith = nameof(UsePartialFillColor))]
        public PluginConfigColor PartialFillColor;

        public LilyBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
             : base(position, size, fillColor)
        {
            PartialFillColor = new PluginConfigColor(new(0f / 255f, 64f / 255f, 255f / 255f, 50f / 100f));
        }
    }

    [Exportable(false)]
    public class BloodLilyBarConfig : ChunkedBarConfig
    {
        [NestedConfig("Glow Color (when Misery ready)", 60, separator = false, spacing = true)]
        public BarGlowConfig GlowConfig = new();

        public BloodLilyBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
             : base(position, size, fillColor)
        {
            GlowConfig.Color = new PluginConfigColor(new(247f / 255f, 177f / 255f, 67f / 255f, 100f / 100f));
        }
    }
}
