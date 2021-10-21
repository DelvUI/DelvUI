﻿using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
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
    public class BlackMageHud : JobHud
    {
        private new BlackMageConfig Config => (BlackMageConfig)_config;

        private static readonly List<uint> ThunderDoTIDs = new() { 161, 162, 163, 1210 };
        private static readonly List<float> ThunderDoTDurations = new() { 18, 12, 24, 18 };

        public BlackMageHud(BlackMageConfig config, string? displayName = null) : base(config, displayName)
        {

        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.ManaBar.Enabled)
            {
                positions.Add(Config.Position + Config.ManaBar.Position);
                sizes.Add(Config.ManaBar.Size);
            }

            if (Config.UmbralHeartBar.Enabled)
            {
                positions.Add(Config.Position + Config.UmbralHeartBar.Position);
                sizes.Add(Config.UmbralHeartBar.Size);
            }

            if (Config.TriplecastBar.Enabled)
            {
                positions.Add(Config.Position + Config.TriplecastBar.Position);
                sizes.Add(Config.TriplecastBar.Size);
            }

            if (Config.EnochianBar.Enabled)
            {
                positions.Add(Config.Position + Config.EnochianBar.Position);
                sizes.Add(Config.EnochianBar.Size);
            }

            if (Config.PolyglotBar.Enabled)
            {
                positions.Add(Config.Position + Config.PolyglotBar.Position);
                sizes.Add(Config.PolyglotBar.Size);
            }

            if (Config.ThundercloudBar.Enabled && !Config.ThundercloudBar.HideWhenInactive)
            {
                positions.Add(Config.Position + Config.ThundercloudBar.Position);
                sizes.Add(Config.ThundercloudBar.Size);
            }

            if (Config.ThunderDoTBar.Enabled && !Config.ThunderDoTBar.HideWhenInactive)
            {
                positions.Add(Config.Position + Config.ThunderDoTBar.Position);
                sizes.Add(Config.ThunderDoTBar.Size);
            }

            if (Config.FirestarterBar.Enabled && !Config.FirestarterBar.HideWhenInactive)
            {
                positions.Add(Config.Position + Config.FirestarterBar.Position);
                sizes.Add(Config.FirestarterBar.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            Vector2 pos = origin + Config.Position;

            if (Config.UmbralHeartBar.Enabled)
            {
                DrawUmbralHeartBar(pos);
            }

            if (Config.TriplecastBar.Enabled)
            {
                DrawTripleCastBar(pos, player);
            }

            if (Config.PolyglotBar.Enabled)
            {
                DrawPolyglotBar(pos, player);
            }

            if (Config.ManaBar.Enabled)
            {
                DrawManaBar(pos, player);
            }

            if (Config.EnochianBar.Enabled)
            {
                DrawEnochianBar(pos);
            }

            if (Config.ThundercloudBar.Enabled)
            {
                DrawThundercloudBar(pos, player);
            }

            if (Config.ThunderDoTBar.Enabled)
            {
                DrawThunderDoTBar(pos, player);
            }

            if (Config.FirestarterBar.Enabled)
            {
                DrawFirestarterBar(pos, player);
            }
        }

        protected void DrawManaBar(Vector2 origin, PlayerCharacter player)
        {
            BlackMageManaBarConfig config = Config.ManaBar;
            var gauge = Plugin.JobGauges.Get<BLMGauge>();

            if (config.HideWhenInactive && !gauge.InAstralFire && !gauge.InUmbralIce && player.CurrentMp == player.MaxMp)
            {
                return;
            }

            // value
            config.ValueLabelConfig.SetValue(player.CurrentMp);

            // element timer
            if (gauge.InAstralFire || gauge.InUmbralIce)
            {
                float time = gauge.ElementTimeRemaining > 10 ? gauge.ElementTimeRemaining / 1000f : 0;
                config.ElementTimerLabelConfig.SetValue(time);
            }
            else
            {
                config.ElementTimerLabelConfig.SetText("");
            }

            bool drawTreshold = gauge.InAstralFire || !config.ThresholdConfig.ShowOnlyDuringAstralFire;

            BarHud bar = BarUtilities.GetProgressBar(
                config,
                drawTreshold ? config.ThresholdConfig : null,
                new LabelConfig[] { config.ValueLabelConfig, config.ElementTimerLabelConfig },
                player.CurrentMp,
                player.MaxMp,
                0,
                player,
                gauge.InAstralFire ? config.FireColor : gauge.InUmbralIce ? config.IceColor : config.FillColor,
                gauge.IsEnochianActive && config.GlowConfig.Enabled ? config.GlowConfig : null
            );

            bar.Draw(origin);
        }

        protected void DrawUmbralHeartBar(Vector2 origin)
        {
            var gauge = Plugin.JobGauges.Get<BLMGauge>();
            if (Config.UmbralHeartBar.HideWhenInactive && gauge.UmbralHearts == 0)
            {
                return;
            };

            BarUtilities.GetChunkedBars(Config.UmbralHeartBar, 3, gauge.UmbralHearts, 3f)
                .Draw(origin);
        }

        protected void DrawTripleCastBar(Vector2 origin, PlayerCharacter player)
        {
            byte stackCount = player.StatusList.FirstOrDefault(o => o.StatusId is 1211)?.StackCount ?? 0;

            if (Config.TriplecastBar.HideWhenInactive && stackCount == 0)
            {
                return;
            };

            BarUtilities.GetChunkedBars(Config.TriplecastBar, 3, stackCount, 3f)
                .Draw(origin);
        }

        protected void DrawEnochianBar(Vector2 origin)
        {
            var gauge = Plugin.JobGauges.Get<BLMGauge>();

            if (Config.EnochianBar.HideWhenInactive && !gauge.IsEnochianActive)
            {
                return;
            }

            float timer = gauge.IsEnochianActive ? (30000f - gauge.EnochianTimer) : 0f;
            Config.EnochianBar.Label.SetValue(timer / 1000);
            BarUtilities.GetProgressBar(Config.EnochianBar, timer, 30000, 0f)
                .Draw(origin);
        }

        protected void DrawPolyglotBar(Vector2 origin, PlayerCharacter player)
        {
            var gauge = Plugin.JobGauges.Get<BLMGauge>();

            if (Config.PolyglotBar.HideWhenInactive && gauge.PolyglotStacks == 0)
            {
                return;
            }

            // only 1 stack before level 80
            if (player.Level < 80)
            {
                var glow = gauge.PolyglotStacks == 1 && Config.PolyglotBar.GlowConfig.Enabled ? Config.PolyglotBar.GlowConfig : null;
                BarUtilities.GetBar(Config.PolyglotBar, gauge.PolyglotStacks, 1, 0, glowConfig: glow)
                    .Draw(origin);
            }
            // 2 stacks for level 80+
            else
            {
                var glow = Config.PolyglotBar.GlowConfig.Enabled ? Config.PolyglotBar.GlowConfig : null;
                BarUtilities.GetChunkedBars(Config.PolyglotBar, 2, gauge.PolyglotStacks, 2f, 0, glowConfig: glow)
                    .Draw(origin);
            }
        }

        protected void DrawThundercloudBar(Vector2 origin, PlayerCharacter player)
        {
            BarUtilities.GetProcBar(Config.ThundercloudBar, player, 164, 18f)?
                .Draw(origin);
        }

        protected void DrawFirestarterBar(Vector2 origin, PlayerCharacter player)
        {
            BarUtilities.GetProcBar(Config.FirestarterBar, player, 165, 18f)?
                .Draw(origin);
        }

        protected void DrawThunderDoTBar(Vector2 origin, PlayerCharacter player)
        {
            var target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;

            BarUtilities.GetDoTBar(Config.ThunderDoTBar, player, target, ThunderDoTIDs, ThunderDoTDurations)?.
                Draw(origin);
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Caster", 0)]
    [SubSection("Black Mage", 1)]
    public class BlackMageConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.BLM;

        public new static BlackMageConfig DefaultConfig()
        {
            var config = new BlackMageConfig();

            config.EnochianBar.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.EnochianBar.Label.TextAnchor = DrawAnchor.Left;
            config.EnochianBar.Label.FrameAnchor = DrawAnchor.Left;
            config.EnochianBar.Label.Position = new Vector2(2, 0);

            config.ThundercloudBar.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.ThundercloudBar.Label.TextAnchor = DrawAnchor.Right;
            config.ThundercloudBar.Label.FrameAnchor = DrawAnchor.Right;
            config.ThundercloudBar.Label.Position = new Vector2(-2, 0);

            config.ThunderDoTBar.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.ThunderDoTBar.Label.TextAnchor = DrawAnchor.Left;
            config.ThunderDoTBar.Label.FrameAnchor = DrawAnchor.Left;
            config.ThunderDoTBar.Label.Position = new Vector2(2, 0);

            config.FirestarterBar.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.FirestarterBar.Label.TextAnchor = DrawAnchor.Left;
            config.FirestarterBar.Label.FrameAnchor = DrawAnchor.Left;
            config.FirestarterBar.Label.Position = new Vector2(2, 0);

            return config;
        }

        [NestedConfig("Mana Bar", 30)]
        public BlackMageManaBarConfig ManaBar = new BlackMageManaBarConfig(
            new Vector2(0, -10),
            new Vector2(254, 20),
            new PluginConfigColor(new Vector4(234f / 255f, 95f / 255f, 155f / 255f, 100f / 100f))
        );

        [NestedConfig("Umbreal Heart Bar", 35)]
        public ChunkedBarConfig UmbralHeartBar = new ChunkedBarConfig(
            new(0, -27),
            new(254, 10),
            new PluginConfigColor(new Vector4(125f / 255f, 195f / 255f, 205f / 255f, 100f / 100f))
        );

        [NestedConfig("Triplecast Bar", 40)]
        public ChunkedBarConfig TriplecastBar = new ChunkedBarConfig(
            new(0, -39),
            new(254, 10),
            new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f))
        );

        [NestedConfig("Enochian Bar", 45)]
        public ProgressBarConfig EnochianBar = new ProgressBarConfig(
            new(-16, -53),
            new(222, 14),
            new PluginConfigColor(new Vector4(234f / 255f, 95f / 255f, 155f / 255f, 100f / 100f))
        );

        [NestedConfig("Polyglot Bar", 50)]
        public BlackMagePolyglotBarConfig PolyglotBar = new BlackMagePolyglotBarConfig(
            new(112, -53),
            new(30, 14),
            new PluginConfigColor(new Vector4(234f / 255f, 95f / 255f, 155f / 255f, 100f / 100f))
        );

        [NestedConfig("Thundercloud Bar", 55)]
        public ProgressBarConfig ThundercloudBar = new ProgressBarConfig(
            new(-64, -69),
            new(126, 14),
            new PluginConfigColor(new Vector4(240f / 255f, 163f / 255f, 255f / 255f, 100f / 100f)),
            BarDirection.Left
        );

        [NestedConfig("Thunder DoT Bar", 60)]
        public ProgressBarConfig ThunderDoTBar = new ProgressBarConfig(
            new(64, -69),
            new(126, 14),
            new PluginConfigColor(new Vector4(67f / 255f, 187 / 255f, 255f / 255f, 100f / 100f))
        );

        [NestedConfig("Firestarter Bar", 65)]
        public ProgressBarConfig FirestarterBar = new ProgressBarConfig(
            new(0, -85),
            new(254, 14),
            new PluginConfigColor(new Vector4(255f / 255f, 136f / 255f, 0 / 255f, 100f / 100f))
        );
    }

    [Exportable(false)]
    public class BlackMageManaBarConfig : BarConfig
    {
        [ColorEdit4("Ice Color" + "##MP")]
        [Order(26)]
        public PluginConfigColor IceColor = new PluginConfigColor(new Vector4(69f / 255f, 115f / 255f, 202f / 255f, 100f / 100f));

        [ColorEdit4("Fire Color" + "##MP")]
        [Order(27)]
        public PluginConfigColor FireColor = new PluginConfigColor(new Vector4(204f / 255f, 40f / 255f, 40f / 255f, 100f / 100f));

        [NestedConfig("Value Label", 45, separator = false, spacing = true)]
        public NumericLabelConfig ValueLabelConfig = new NumericLabelConfig(new Vector2(2, 0), "", DrawAnchor.Left, DrawAnchor.Left);

        [NestedConfig("Element Timer Label", 50, separator = false, spacing = true)]
        public NumericLabelConfig ElementTimerLabelConfig = new NumericLabelConfig(Vector2.Zero, "", DrawAnchor.Center, DrawAnchor.Center);

        [NestedConfig("Glow When Enochian Is Active", 55, separator = false, spacing = true)]
        public BarGlowConfig GlowConfig = new BarGlowConfig();

        [NestedConfig("Threshold", 65, separator = false, spacing = true)]
        public BlackMakeManaBarThresholdConfig ThresholdConfig = new BlackMakeManaBarThresholdConfig();

        public BlackMageManaBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
             : base(position, size, fillColor)
        {
        }
    }

    [Exportable(false)]
    public class BlackMakeManaBarThresholdConfig : ThresholdConfig
    {
        [Checkbox("Show Only During Astral Fire")]
        [Order(5)]
        public bool ShowOnlyDuringAstralFire = true;

        public BlackMakeManaBarThresholdConfig()
        {
            Enabled = true;
            Value = 2400;
            Color = new PluginConfigColor(new Vector4(240f / 255f, 120f / 255f, 10f / 255f, 100f / 100f));
            ShowMarker = true;
            MarkerColor = new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));
        }
    }

    [Exportable(false)]
    public class BlackMagePolyglotBarConfig : ChunkedBarConfig
    {
        [NestedConfig("Show Glow", 60, separator = false, spacing = true)]
        public BarGlowConfig GlowConfig = new BarGlowConfig();

        public BlackMagePolyglotBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
             : base(position, size, fillColor)
        {
        }
    }
}
