using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
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
    public class SageHud : JobHud
    {
        private new SageConfig Config => (SageConfig)_config;

        private static readonly List<uint> DotIDs = new() { 2614, 2615, 2616 };
        private static readonly List<float> DotDurations = new() { 30, 30, 30 };

        public SageHud(JobConfig config, string? displayName = null) : base(config, displayName)
        {
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new();
            List<Vector2> sizes = new();

            if (Config.AddersgallBar.Enabled)
            {
                positions.Add(Config.Position + Config.AddersgallBar.Position);
                sizes.Add(Config.AddersgallBar.Size);
            }

            if (Config.DotBar.Enabled)
            {
                positions.Add(Config.Position + Config.DotBar.Position);
                sizes.Add(Config.DotBar.Size);
            }

            if (Config.KeracholeBar.Enabled)
            {
                positions.Add(Config.Position + Config.KeracholeBar.Position);
                sizes.Add(Config.KeracholeBar.Size);
            }

            if (Config.PhysisBar.Enabled)
            {
                positions.Add(Config.Position + Config.PhysisBar.Position);
                sizes.Add(Config.PhysisBar.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            Vector2 pos = origin + Config.Position;

            if (Config.AddersgallBar.Enabled)
            {
                DrawAddersgallBar(pos, player);
            }

            if (Config.DotBar.Enabled)
            {
                DrawDotBar(pos, player);
            }

            if (Config.KeracholeBar.Enabled)
            {
                DrawKeracholeBar(pos, player);
            }

            if (Config.PhysisBar.Enabled)
            {
                DrawPhysisBar(pos, player);
            }
        }

        private void DrawDotBar(Vector2 origin, PlayerCharacter player)
        {
            GameObject? target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;

            BarHud? bar = BarUtilities.GetDoTBar(Config.DotBar, player, target, DotIDs, DotDurations);
            if (bar != null)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.DotBar.StrataLevel));
            }
        }

        private void DrawAddersgallBar(Vector2 origin, PlayerCharacter player)
        {
            SGEGauge gauge = Plugin.JobGauges.Get<SGEGauge>();

            const float addersgallCooldown = 20000f;

            float GetScale(int num, float timer) => num + (timer / addersgallCooldown);
            float adderScale = GetScale(gauge.Addersgall, gauge.AddersgallTimer);
            BarGlowConfig? glow = gauge.Eukrasia && Config.EukrasiaGlow ? Config.AddersgallBar.GlowConfig : null;

            if (!Config.AddersgallBar.HideWhenInactive || adderScale > 0)
            {
                BarHud[] bars = BarUtilities.GetChunkedBars(Config.AddersgallBar, 3, adderScale, 3, 0, player, glowConfig: glow, chunksToGlow: new[] { true, true, true });
                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.AddersgallBar.StrataLevel));
                }
            }

            if (!Config.AdderstingBar.HideWhenInactive && Config.AdderstingBar.Enabled || gauge.Addersting > 0)
            {
                BarHud[] bars = BarUtilities.GetChunkedBars(Config.AdderstingBar, 3, gauge.Addersting, 3, 0, player, glowConfig: glow, chunksToGlow: new[] { true, true, true });
                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.AdderstingBar.StrataLevel));
                }
            }
        }

        private void DrawPhysisBar(Vector2 origin, PlayerCharacter player)
        {
            float physisDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 2617 or 2620 && o.SourceID == player.ObjectId)?.RemainingTime ?? 0f;

            if (!Config.PhysisBar.HideWhenInactive || physisDuration > 0)
            {
                Config.PhysisBar.Label.SetValue(physisDuration);
                BarHud bar = BarUtilities.GetProgressBar(Config.PhysisBar, physisDuration, 15f, 0f, player);
                AddDrawActions(bar.GetDrawActions(origin, Config.PhysisBar.StrataLevel));
            }
        }

        private void DrawKeracholeBar(Vector2 origin, PlayerCharacter player)
        {
            float keracholeDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 2618 or 2938 && o.SourceID == player.ObjectId)?.RemainingTime ?? 0f;
            float holosDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 3003 && o.SourceID == player.ObjectId)?.RemainingTime ?? 0f;

            if (!Config.KeracholeBar.HideWhenInactive || keracholeDuration > 0 || holosDuration > 0)
            {
                float duration = holosDuration > 0 ? holosDuration : keracholeDuration;
                float maxDuration = holosDuration > 0 ? 20f : 15f;

                Config.KeracholeBar.Label.SetValue(duration);
                BarHud bar = BarUtilities.GetProgressBar(Config.KeracholeBar, duration, maxDuration, 0f, player);
                AddDrawActions(bar.GetDrawActions(origin, Config.KeracholeBar.StrataLevel));
            }
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Healer", 0)]
    [SubSection("Sage", 1)]
    public class SageConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.SGE;

        public new static SageConfig DefaultConfig()
        {
            var config = new SageConfig();

            config.UseDefaultPrimaryResourceBar = true;
            config.DotBar.Label.FontID = FontsConfig.DefaultMediumFontKey;

            return config;
        }

        [Checkbox("Enable Eukrasia Glow", spacing = true)]
        [Order(30)]
        public bool EukrasiaGlow = true;

        [NestedConfig("Addersgall Bar", 35)]
        public AddersgallBarConfig AddersgallBar = new AddersgallBarConfig(
            new(-64, -32),
            new(126, 20),
            new PluginConfigColor(new(197f / 255f, 247f / 255f, 255f / 255f, 100f / 100f))
        );

        [NestedConfig("Addersting Bar", 40)]
        public AddersgallBarConfig AdderstingBar = new AddersgallBarConfig(
            new(64, -32),
            new(126, 20),
            new PluginConfigColor(new(255f / 255f, 232f / 255f, 255f / 255f, 100f / 100f))
        );

        [NestedConfig("Eukrasian Dosis Bar", 45)]
        public ProgressBarConfig DotBar = new ProgressBarConfig(
            new(0, -10),
            new(254, 20),
            new PluginConfigColor(new(41f / 255f, 142f / 255f, 144f / 255f, 100f / 100f))
        );

        [NestedConfig("Kerachole / Holos Bar", 50)]
        public ProgressBarConfig KeracholeBar = new ProgressBarConfig(
            new(64, -52),
            new(126, 15),
            new PluginConfigColor(new(100f / 255f, 207f / 255f, 211f / 255f, 100f / 100f))
        );

        [NestedConfig("Physis Bar", 55)]
        public ProgressBarConfig PhysisBar = new ProgressBarConfig(
            new(-64, -52),
            new(126, 15),
            new PluginConfigColor(new(26f / 255f, 167f / 255f, 109f / 255f, 100f / 100f))
        );
    }

    [Exportable(false)]
    public class AddersgallBarConfig : ChunkedBarConfig
    {
        [NestedConfig("Glow Color (when Eukrasia active)", 60, separator = false, spacing = true)]
        public BarGlowConfig GlowConfig = new();

        public AddersgallBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
             : base(position, size, fillColor)
        {
            GlowConfig.Color = new PluginConfigColor(new(247f / 255f, 177f / 255f, 67f / 255f, 100f / 100f));
        }
    }
}
