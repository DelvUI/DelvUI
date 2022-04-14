using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Statuses;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.GameStructs;
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
    public class DancerHud : JobHud
    {
        private new DancerConfig Config => (DancerConfig)_config;

        public DancerHud(DancerConfig config, string? displayName = null) : base(config, displayName)
        {

        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.StandardFinishBar.Enabled)
            {
                positions.Add(Config.Position + Config.StandardFinishBar.Position);
                sizes.Add(Config.StandardFinishBar.Size);
            }

            if (Config.TechnicalFinishBar.Enabled)
            {
                positions.Add(Config.Position + Config.TechnicalFinishBar.Position);
                sizes.Add(Config.TechnicalFinishBar.Position);
            }

            if (Config.DevilmentBar.Enabled)
            {
                positions.Add(Config.Position + Config.DevilmentBar.Position);
                sizes.Add(Config.DevilmentBar.Position);
            }

            if (Config.EspritGauge.Enabled)
            {
                positions.Add(Config.Position + Config.EspritGauge.Position);
                sizes.Add(Config.EspritGauge.Size);
            }

            if (Config.FeatherGauge.Enabled)
            {
                positions.Add(Config.Position + Config.FeatherGauge.Position);
                sizes.Add(Config.FeatherGauge.Size);
            }

            if (Config.CascadeBar.Enabled)
            {
                positions.Add(Config.Position + Config.CascadeBar.Position);
                sizes.Add(Config.CascadeBar.Position);
            }

            if (Config.FountainBar.Enabled)
            {
                positions.Add(Config.Position + Config.FountainBar.Position);
                sizes.Add(Config.FountainBar.Position);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            Vector2 pos = origin + Config.Position;

            if (Config.EspritGauge.Enabled)
            {
                DrawEspritBar(pos, player);
            }

            if (Config.FeatherGauge.Enabled)
            {
                DrawFeathersBar(pos, player);
            }

            if (Config.StandardFinishBar.Enabled)
            {
                DrawStandardBar(pos, player);
            }

            if (Config.TechnicalFinishBar.Enabled)
            {
                DrawTechnicalBar(pos, player);
            }

            if (Config.DevilmentBar.Enabled)
            {
                DrawDevilmentBar(pos, player);
            }

            bool showingStepBar = false;
            if (Config.StepsBar.Enabled)
            {
                showingStepBar = DrawStepBar(pos, player);
            }

            if (!showingStepBar || !Config.StepsBar.HideProcs)
            {
                if (Config.CascadeBar.Enabled) { DrawProcBar(pos, player, Config.CascadeBar, 2693); }
                if (Config.FountainBar.Enabled) { DrawProcBar(pos, player, Config.FountainBar, 2694); }
            }
        }

        private void DrawProcBar(Vector2 origin, PlayerCharacter player, DancerProcBarConfig config, uint statusId)
        {
            BarHud? bar = BarUtilities.GetProcBar(config, player, statusId, 30f, !config.IgnoreBuffDuration);
            if (bar != null)
            {
                AddDrawActions(bar.GetDrawActions(origin, config.StrataLevel));
            }
        }

        private unsafe bool DrawStepBar(Vector2 origin, PlayerCharacter player)
        {
            var gauge = Plugin.JobGauges.Get<DNCGauge>();
            if (!gauge.IsDancing)
            {
                return false;
            }

            List<Tuple<PluginConfigColor, float, LabelConfig?>> chunks = new List<Tuple<PluginConfigColor, float, LabelConfig?>>();
            List<bool> glows = new List<bool>();
            bool danceReady = true;

            for (var i = 0; i < 4; i++)
            {
                DNCStep step = (DNCStep)gauge.Steps[i];

                if (step == DNCStep.None)
                {
                    break;
                }

                if (gauge.CompletedSteps == i)
                {
                    glows.Add(true);
                    danceReady = false;
                }
                else
                {
                    glows.Add(false);
                }

                PluginConfigColor color = new(Vector4.Zero);

                switch (step)
                {
                    case DNCStep.Emboite:
                        color = Config.StepsBar.EmboiteColor;
                        break;

                    case DNCStep.Entrechat:
                        color = Config.StepsBar.EntrechatColor;
                        break;

                    case DNCStep.Jete:
                        color = Config.StepsBar.JeteColor;
                        break;

                    case DNCStep.Pirouette:
                        color = Config.StepsBar.PirouetteColor;
                        break;
                }

                var tuple = new Tuple<PluginConfigColor, float, LabelConfig?>(color, 1, null);
                chunks.Add(tuple);
            }

            if (danceReady)
            {
                for (int i = 0; i < glows.Count; i++)
                {
                    glows[i] = true;
                }
            }

            BarHud[] bars = BarUtilities.GetChunkedBars(Config.StepsBar, chunks.ToArray(), player, Config.StepsBar.GlowConfig, glows.ToArray());
            foreach (BarHud bar in bars)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.StepsBar.StrataLevel));
            }

            return true;
        }

        private void DrawEspritBar(Vector2 origin, PlayerCharacter player)
        {
            DNCGauge gauge = Plugin.JobGauges.Get<DNCGauge>();

            if (Config.EspritGauge.HideWhenInactive && gauge.Esprit is 0) { return; }

            Config.EspritGauge.Label.SetValue(gauge.Esprit);

            BarHud[] bars = BarUtilities.GetChunkedProgressBars(Config.EspritGauge, 2, gauge.Esprit, 100, 0f, player);
            foreach (BarHud bar in bars)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.EspritGauge.StrataLevel));
            }
        }

        private void DrawFeathersBar(Vector2 origin, PlayerCharacter player)
        {
            DNCGauge gauge = Plugin.JobGauges.Get<DNCGauge>();
            bool hasFlourishingBuff = player.StatusList.FirstOrDefault(o => o.StatusId is 1820 or 2021) != null;
            bool[]? glows = null;

            if (Config.FeatherGauge.HideWhenInactive && gauge.Feathers is 0 && !hasFlourishingBuff)
            {
                return;
            }

            if (Config.FeatherGauge.GlowConfig.Enabled)
            {
                glows = new bool[] { hasFlourishingBuff, hasFlourishingBuff, hasFlourishingBuff, hasFlourishingBuff };
            }

            BarGlowConfig? config = hasFlourishingBuff ? Config.FeatherGauge.GlowConfig : null;
            BarHud[] bars = BarUtilities.GetChunkedBars(Config.FeatherGauge, 4, gauge.Feathers, 4, 0, player, glowConfig: config, chunksToGlow: glows);
            foreach (BarHud bar in bars)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.FeatherGauge.StrataLevel));
            }
        }

        private void DrawTechnicalBar(Vector2 origin, PlayerCharacter player)
        {
            IEnumerable<Status> devilmentBuff = player.StatusList.Where(o => o.StatusId is 1825 && o.SourceID == player.ObjectId);

            float technicalFinishDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 1822 or 2050 && o.SourceID == player.ObjectId)?.RemainingTime ?? 0f;

            if (!Config.TechnicalFinishBar.HideWhenInactive || technicalFinishDuration > 0)
            {
                Config.TechnicalFinishBar.Label.SetValue(Math.Abs(technicalFinishDuration));

                BarHud bar = BarUtilities.GetProgressBar(Config.TechnicalFinishBar, technicalFinishDuration, 20f, 0f, player);
                AddDrawActions(bar.GetDrawActions(origin, Config.TechnicalFinishBar.StrataLevel));
            }
        }

        private void DrawDevilmentBar(Vector2 origin, PlayerCharacter player)
        {
            float devilmentDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 1825 && o.SourceID == player.ObjectId)?.RemainingTime ?? 0f;

            if (!Config.DevilmentBar.HideWhenInactive || devilmentDuration > 0)
            {
                Config.DevilmentBar.Label.SetValue(Math.Abs(devilmentDuration));

                BarHud bar = BarUtilities.GetProgressBar(Config.DevilmentBar, devilmentDuration, 20f, 0f, player);
                AddDrawActions(bar.GetDrawActions(origin, Config.DevilmentBar.StrataLevel));
            }
        }

        private void DrawStandardBar(Vector2 origin, PlayerCharacter player)
        {
            float standardFinishDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 1821 or 2024 or 2105 or 2113 && o.SourceID == player.ObjectId)?.RemainingTime ?? 0f;

            if (!Config.StandardFinishBar.HideWhenInactive || standardFinishDuration > 0)
            {
                Config.StandardFinishBar.Label.SetValue(Math.Abs(standardFinishDuration));

                BarHud bar = BarUtilities.GetProgressBar(Config.StandardFinishBar, standardFinishDuration, 60f, 0f, player);
                AddDrawActions(bar.GetDrawActions(origin, Config.StandardFinishBar.StrataLevel));
            }
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Ranged", 0)]
    [SubSection("Dancer", 1)]
    public class DancerConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.DNC;
        public new static DancerConfig DefaultConfig()
        {
            var config = new DancerConfig();

            config.EspritGauge.UseChunks = false;
            config.EspritGauge.Label.Enabled = true;

            config.CascadeBar.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.FountainBar.Label.FontID = FontsConfig.DefaultMediumFontKey;

            return config;
        }


        [NestedConfig("Standard Finish Bar", 30)]
        public ProgressBarConfig StandardFinishBar = new ProgressBarConfig(
            new(0, -10),
            new(254, 20),
            new PluginConfigColor(new Vector4(0f / 255f, 193f / 255f, 95f / 255f, 100f / 100f))
        );

        [NestedConfig("Technical Finish Bar", 35)]
        public ProgressBarConfig TechnicalFinishBar = new ProgressBarConfig(
            new(-64, -32),
            new(126, 20),
            new PluginConfigColor(new Vector4(255f / 255f, 9f / 255f, 102f / 255f, 100f / 100f))
        );

        [NestedConfig("Devilment Bar", 40)]
        public ProgressBarConfig DevilmentBar = new ProgressBarConfig(
            new(64, -32),
            new(126, 20),
            new PluginConfigColor(new Vector4(52f / 255f, 78f / 255f, 29f / 255f, 100f / 100f))
        );

        [NestedConfig("Esprit Gauge", 45)]
        public ChunkedProgressBarConfig EspritGauge = new ChunkedProgressBarConfig(
            new(0, -54),
            new(254, 20),
            new PluginConfigColor(new Vector4(72f / 255f, 20f / 255f, 99f / 255f, 100f / 100f))
        );

        [NestedConfig("Feathers Gauge", 50)]
        public DancerFeatherGaugeConfig FeatherGauge = new DancerFeatherGaugeConfig(
            new(0, -71),
            new(254, 10),
            new PluginConfigColor(new Vector4(175f / 255f, 229f / 255f, 29f / 255f, 100f / 100f))
        );

        [NestedConfig("Flourishing Symmetry Bar", 60)]
        public DancerProcBarConfig CascadeBar = new DancerProcBarConfig(
            new(-96, -83),
            new(62, 10),
            new(new Vector4(0f / 255f, 255f / 255f, 0f / 255f, 100f / 100f))
        );

        [NestedConfig("Flourishing Flow Bar", 65)]
        public DancerProcBarConfig FountainBar = new DancerProcBarConfig(
            new(-32, -83),
            new(62, 10),
            new(new Vector4(255f / 255f, 215f / 255f, 0f / 255f, 100f / 100f))
        );

        [NestedConfig("Steps Bar", 80)]
        public DancerStepsBarConfig StepsBar = new DancerStepsBarConfig(
            new(0, -83),
            new(254, 10)
        );
    }

    [Exportable(false)]
    public class DancerFeatherGaugeConfig : ChunkedBarConfig
    {
        [NestedConfig("Glow on Flourishing Fan Dance", 1000, separator = false, spacing = true)]
        public BarGlowConfig GlowConfig = new BarGlowConfig();

        public DancerFeatherGaugeConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
             : base(position, size, fillColor)
        {
            GlowConfig.Color = new PluginConfigColor(new Vector4(255f / 255f, 215f / 255f, 0f / 255f, 100f / 100f));
        }
    }

    [Exportable(false)]
    public class DancerProcBarConfig : ProgressBarConfig
    {
        [Checkbox("Ignore Buff Duration")]
        [Order(4)]
        public bool IgnoreBuffDuration = true;

        public DancerProcBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
            : base(position, size, fillColor)
        {
        }
    }

    [DisableParentSettings("FillColor", "HideWhenInactive")]
    [Exportable(false)]
    public class DancerStepsBarConfig : ChunkedBarConfig
    {
        [Checkbox("Hide Procs When Active")]
        [Order(50)]
        public bool HideProcs = true;

        [ColorEdit4("Emboite", spacing = true)]
        [Order(55)]
        public PluginConfigColor EmboiteColor = new(new Vector4(255f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Entrechat")]
        [Order(60)]
        public PluginConfigColor EntrechatColor = new(new Vector4(0f / 255f, 0f / 255f, 255f / 255f, 100f / 100f));

        [ColorEdit4("Jete")]
        [Order(65)]
        public PluginConfigColor JeteColor = new(new Vector4(0f / 255f, 255f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Pirouette")]
        [Order(70)]
        public PluginConfigColor PirouetteColor = new(new Vector4(255f / 255f, 215f / 255f, 0f / 255f, 100f / 100f));

        [NestedConfig("Glow", 75, separator = false, spacing = true)]
        public BarGlowConfig GlowConfig = new BarGlowConfig();

        public DancerStepsBarConfig(Vector2 position, Vector2 size)
            : base(position, size, new(Vector4.Zero))
        {
        }
    }
}