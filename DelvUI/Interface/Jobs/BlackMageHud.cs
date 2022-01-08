using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
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
        private static readonly List<float> ThunderDoTDurations = new() { 21, 18, 30, 18 };

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

            if (Config.StacksBar.Enabled)
            {
                positions.Add(Config.Position + Config.StacksBar.Position);
                sizes.Add(Config.StacksBar.Size);
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

            if (Config.ParadoxBar.Enabled)
            {
                positions.Add(Config.Position + Config.ParadoxBar.Position);
                sizes.Add(Config.ParadoxBar.Size);
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

            if (Config.ManaBar.Enabled)
            {
                DrawManaBar(pos, player);
            }

            if (Config.StacksBar.Enabled)
            {
                DrawStacksBar(pos);
            }

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

            if (Config.ParadoxBar.Enabled)
            {
                DrawParadoxBar(pos, player);
            }

            if (Config.EnochianBar.Enabled)
            {
                DrawEnochianBar(pos, player);
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
            BLMGauge gauge = Plugin.JobGauges.Get<BLMGauge>();

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

            PluginConfigColor fillColor = config.FillColor;
            PluginConfigColor bgColor = config.BackgroundColor;
            if (config.UseElementColor)
            {
                fillColor = gauge.InAstralFire ? config.FireColor : gauge.InUmbralIce ? config.IceColor : config.FillColor;
                bgColor = gauge.InAstralFire ? config.FireBackgroundColor : gauge.InUmbralIce ? config.IceBackgroundColor : config.BackgroundColor;
            }

            BarHud bar = BarUtilities.GetProgressBar(
                config,
                drawTreshold ? config.ThresholdConfig : null,
                new LabelConfig[] { config.ValueLabelConfig, config.ElementTimerLabelConfig },
                player.CurrentMp,
                player.MaxMp,
                0,
                player,
                fillColor: fillColor,
                backgroundColor: bgColor
            );

            AddDrawActions(bar.GetDrawActions(origin, config.StrataLevel));
        }

        protected void DrawStacksBar(Vector2 origin)
        {
            BLMGauge gauge = Plugin.JobGauges.Get<BLMGauge>();
            if (Config.StacksBar.HideWhenInactive && gauge.UmbralIceStacks == 0 && gauge.AstralFireStacks == 0)
            {
                return;
            };

            PluginConfigColor color = gauge.UmbralIceStacks > 0 ? Config.StacksBar.IceColor : Config.StacksBar.FireColor;
            byte stacks = gauge.UmbralIceStacks > 0 ? gauge.UmbralIceStacks : gauge.AstralFireStacks;

            BarHud[] bars = BarUtilities.GetChunkedBars(Config.StacksBar, 3, stacks, 3f, fillColor: color);
            foreach (BarHud bar in bars)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.StacksBar.StrataLevel));
            }
        }

        protected void DrawUmbralHeartBar(Vector2 origin)
        {
            BLMGauge gauge = Plugin.JobGauges.Get<BLMGauge>();
            if (Config.UmbralHeartBar.HideWhenInactive && gauge.UmbralHearts == 0)
            {
                return;
            };

            BarHud[] bars = BarUtilities.GetChunkedBars(Config.UmbralHeartBar, 3, gauge.UmbralHearts, 3f);
            foreach (BarHud bar in bars)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.UmbralHeartBar.StrataLevel));
            }
        }

        protected void DrawTripleCastBar(Vector2 origin, PlayerCharacter player)
        {
            byte stackCount = player.StatusList.FirstOrDefault(o => o.StatusId is 1211)?.StackCount ?? 0;

            if (Config.TriplecastBar.HideWhenInactive && stackCount == 0)
            {
                return;
            };

            BarHud[] bars = BarUtilities.GetChunkedBars(Config.TriplecastBar, 3, stackCount, 3f);
            foreach (BarHud bar in bars)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.TriplecastBar.StrataLevel));
            }
        }

        protected void DrawEnochianBar(Vector2 origin, PlayerCharacter player)
        {
            BLMGauge gauge = Plugin.JobGauges.Get<BLMGauge>();

            if (Config.EnochianBar.HideWhenInactive && !gauge.IsEnochianActive)
            {
                return;
            }

            float timer = gauge.IsEnochianActive ? (30000f - gauge.EnochianTimer) : 0f;
            Config.EnochianBar.Label.SetValue(timer / 1000);

            BarHud bar = BarUtilities.GetProgressBar(Config.EnochianBar, timer / 1000, 30, 0f, player);
            AddDrawActions(bar.GetDrawActions(origin, Config.EnochianBar.StrataLevel));
        }

        protected void DrawPolyglotBar(Vector2 origin, PlayerCharacter player)
        {
            BLMGauge gauge = Plugin.JobGauges.Get<BLMGauge>();

            if (Config.PolyglotBar.HideWhenInactive && gauge.PolyglotStacks == 0)
            {
                return;
            }

            // only 1 stack before level 80
            if (player.Level < 80)
            {
                BarGlowConfig? glow = gauge.PolyglotStacks == 1 && Config.PolyglotBar.GlowConfig.Enabled ? Config.PolyglotBar.GlowConfig : null;
                BarHud bar = BarUtilities.GetBar(Config.PolyglotBar, gauge.PolyglotStacks, 1, 0, glowConfig: glow);
                AddDrawActions(bar.GetDrawActions(origin, Config.PolyglotBar.StrataLevel));
            }
            // 2 stacks for level 80+
            else
            {
                BarGlowConfig? glow = Config.PolyglotBar.GlowConfig.Enabled ? Config.PolyglotBar.GlowConfig : null;
                BarHud[] bars = BarUtilities.GetChunkedBars(Config.PolyglotBar, 2, gauge.PolyglotStacks, 2f, 0, glowConfig: glow);
                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.PolyglotBar.StrataLevel));
                }
            }
        }

        protected void DrawParadoxBar(Vector2 origin, PlayerCharacter player)
        {
            BLMGauge gauge = Plugin.JobGauges.Get<BLMGauge>();

            if (Config.ParadoxBar.HideWhenInactive && !gauge.IsParadoxActive)
            {
                return;
            };

            PluginConfigColor color = Config.ParadoxBar.FillColor;
            if (Config.ParadoxBar.UseElementColor)
            {
                color = gauge.InUmbralIce ? Config.ParadoxBar.IceColor : (gauge.InAstralFire ? Config.ParadoxBar.FireColor : color);
            }

            BarGlowConfig? glow = gauge.IsParadoxActive && Config.ParadoxBar.GlowConfig.Enabled ? Config.ParadoxBar.GlowConfig : null;
            BarHud bar = BarUtilities.GetBar(Config.ParadoxBar, gauge.IsParadoxActive ? 1 : 0, 1, 0, fillColor: color, glowConfig: glow);
            AddDrawActions(bar.GetDrawActions(origin, Config.ParadoxBar.StrataLevel));
        }

        protected void DrawThundercloudBar(Vector2 origin, PlayerCharacter player)
        {
            BarHud? bar = BarUtilities.GetProcBar(Config.ThundercloudBar, player, 164, 40f);
            if (bar != null)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.ThundercloudBar.StrataLevel));
            }
        }

        protected void DrawFirestarterBar(Vector2 origin, PlayerCharacter player)
        {
            BarHud? bar = BarUtilities.GetProcBar(Config.FirestarterBar, player, 165, 30f);
            if (bar != null)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.FirestarterBar.StrataLevel));
            }
        }

        protected void DrawThunderDoTBar(Vector2 origin, PlayerCharacter player)
        {
            GameObject? target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;

            BarHud? bar = BarUtilities.GetDoTBar(Config.ThunderDoTBar, player, target, ThunderDoTIDs, ThunderDoTDurations);
            if (bar != null)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.ThunderDoTBar.StrataLevel));
            }
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
            new Vector2(254, 18),
            new PluginConfigColor(new Vector4(234f / 255f, 95f / 255f, 155f / 255f, 100f / 100f))
        );

        [NestedConfig("Umbral Ice / Astral Fire Bar", 31)]
        public BlackMageStacksBarConfig StacksBar = new BlackMageStacksBarConfig(
            new(-67, -27),
            new(120, 10)
        );

        [NestedConfig("Umbral Heart Bar", 32)]
        public ChunkedBarConfig UmbralHeartBar = new ChunkedBarConfig(
            new(67, -27),
            new(120, 10),
            new PluginConfigColor(new Vector4(125f / 255f, 195f / 255f, 205f / 255f, 100f / 100f))
        );

        [NestedConfig("Paradox Bar", 33)]
        public BlackMageParadoxBarConfig ParadoxBar = new BlackMageParadoxBarConfig(
            new(0, -27),
            new(10, 10),
            new PluginConfigColor(new Vector4(123f / 255f, 66f / 255f, 177f / 255f, 100f / 100f))
        );

        [NestedConfig("Enochian Bar", 40)]
        public ProgressBarConfig EnochianBar = new ProgressBarConfig(
            new(-16, -41),
            new(222, 14),
            new PluginConfigColor(new Vector4(234f / 255f, 95f / 255f, 155f / 255f, 100f / 100f))
        );

        [NestedConfig("Polyglot Bar", 45)]
        public BlackMagePolyglotBarConfig PolyglotBar = new BlackMagePolyglotBarConfig(
            new(112, -41),
            new(30, 14),
            new PluginConfigColor(new Vector4(234f / 255f, 95f / 255f, 155f / 255f, 100f / 100f))
        );

        [NestedConfig("Triplecast Bar", 50)]
        public ChunkedBarConfig TriplecastBar = new ChunkedBarConfig(
            new(0, -55),
            new(254, 10),
            new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f))
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
        [Checkbox("Use Element Color" + "##MP", spacing = true)]
        [Order(50)]
        public bool UseElementColor = true;

        [ColorEdit4("Ice Color" + "##MP")]
        [Order(51, collapseWith = nameof(UseElementColor))]
        public PluginConfigColor IceColor = new PluginConfigColor(new Vector4(69f / 255f, 115f / 255f, 202f / 255f, 100f / 100f));

        [ColorEdit4("Ice Background Color" + "##MP")]
        [Order(52, collapseWith = nameof(UseElementColor))]
        public PluginConfigColor IceBackgroundColor = new PluginConfigColor(new Vector4(50f / 255f, 80f / 255f, 130f / 255f, 50f / 100f));

        [ColorEdit4("Fire Color" + "##MP")]
        [Order(53, collapseWith = nameof(UseElementColor))]
        public PluginConfigColor FireColor = new PluginConfigColor(new Vector4(204f / 255f, 40f / 255f, 40f / 255f, 100f / 100f));

        [ColorEdit4("Fire Background Color" + "##MP")]
        [Order(54, collapseWith = nameof(UseElementColor))]
        public PluginConfigColor FireBackgroundColor = new PluginConfigColor(new Vector4(120f / 255f, 30f / 255f, 30f / 255f, 50f / 100f));

        [NestedConfig("Value Label", 60, separator = false, spacing = true)]
        public NumericLabelConfig ValueLabelConfig = new NumericLabelConfig(new Vector2(2, 0), "", DrawAnchor.Left, DrawAnchor.Left);

        [NestedConfig("Element Timer Label", 65, separator = false, spacing = true)]
        public NumericLabelConfig ElementTimerLabelConfig = new NumericLabelConfig(Vector2.Zero, "", DrawAnchor.Center, DrawAnchor.Center);

        [NestedConfig("Threshold", 70, separator = false, spacing = true)]
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

    [DisableParentSettings("FillColor", "FillDirection")]
    [Exportable(false)]
    public class BlackMageStacksBarConfig : ChunkedBarConfig
    {
        [ColorEdit4("Ice Color" + "##MP")]
        [Order(26)]
        public PluginConfigColor IceColor = new PluginConfigColor(new Vector4(69f / 255f, 115f / 255f, 202f / 255f, 100f / 100f));

        [ColorEdit4("Fire Color" + "##MP")]
        [Order(27)]
        public PluginConfigColor FireColor = new PluginConfigColor(new Vector4(204f / 255f, 40f / 255f, 40f / 255f, 100f / 100f));

        public BlackMageStacksBarConfig(Vector2 position, Vector2 size)
             : base(position, size, new(Vector4.Zero))
        {
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

    [Exportable(false)]
    public class BlackMageParadoxBarConfig : BarConfig
    {
        [Checkbox("Use Element Color" + "##Paradox", spacing = true)]
        [Order(50)]
        public bool UseElementColor = true;

        [ColorEdit4("Ice Color" + "##Paradox")]
        [Order(51, collapseWith = nameof(UseElementColor))]
        public PluginConfigColor IceColor = new PluginConfigColor(new Vector4(69f / 255f, 115f / 255f, 202f / 255f, 100f / 100f));

        [ColorEdit4("Fire Color" + "##Paradox")]
        [Order(52, collapseWith = nameof(UseElementColor))]
        public PluginConfigColor FireColor = new PluginConfigColor(new Vector4(204f / 255f, 40f / 255f, 40f / 255f, 100f / 100f));

        [NestedConfig("Show Glow", 60, separator = false, spacing = true)]
        public BarGlowConfig GlowConfig = new BarGlowConfig();

        public BlackMageParadoxBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
             : base(position, size, fillColor)
        {
        }
    }
}
