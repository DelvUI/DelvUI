using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Dalamud.Logging;
using DelvUI.Enums;

namespace DelvUI.Interface.Jobs
{
    public class BardHud : JobHud
    {
        private readonly SpellHelper _spellHelper = new();
        private new BardConfig Config => (BardConfig)_config;
        private PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;

        public BardHud(BardConfig config, string? displayName = null) : base(config, displayName)
        {
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.SongGaugeBar.Enabled)
            {
                positions.Add(Config.Position + Config.SongGaugeBar.Position);
                sizes.Add(Config.SongGaugeBar.Size);
            }

            if (Config.SoulVoiceBar.Enabled)
            {
                positions.Add(Config.Position + Config.SoulVoiceBar.Position);
                sizes.Add(Config.SoulVoiceBar.Size);
            }

            if (Config.StacksBar.Enabled)
            {
                positions.Add(Config.Position + Config.StacksBar.Position);
                sizes.Add(Config.StacksBar.Size);
            }

            if (Config.CausticBiteDoTBar.Enabled)
            {
                positions.Add(Config.Position + Config.CausticBiteDoTBar.Position);
                sizes.Add(Config.CausticBiteDoTBar.Size);
            }

            if (Config.StormbiteDoTBar.Enabled)
            {
                positions.Add(Config.Position + Config.StormbiteDoTBar.Position);
                sizes.Add(Config.StormbiteDoTBar.Size);
            }

            if (Config.CodaBar.Enabled)
            {
                positions.Add(Config.Position + Config.CodaBar.Position);
                sizes.Add(Config.CodaBar.Size);
            }

            return (positions, sizes);
        }
        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            Vector2 pos = origin + Config.Position;

            if (Config.CausticBiteDoTBar.Enabled)
            {
                DrawCausticBiteDoTBar(pos, player);
            }

            if (Config.StormbiteDoTBar.Enabled)
            {
                DrawStormbiteDoTBar(pos, player);
            }

            HandleCurrentSong(pos, player);

            if (Config.SoulVoiceBar.Enabled)
            {
                DrawSoulVoiceBar(pos);
            }

            if (Config.CodaBar.Enabled)
            {
                DrawCodaBar(pos, player);
            }
        }

        private void DrawCodaBar(Vector2 origin, PlayerCharacter player)
        {
            BRDGauge gauge = Plugin.JobGauges.Get<BRDGauge>();
            if (!Config.CodaBar.HideWhenInactive)
            {
                var order = Config.CodaBar.CodaOrder;
                var hasCoda = new[] { gauge.Coda.Contains(Song.WANDERER) ? 1 : 0, gauge.Coda.Contains(Song.MAGE) ? 1 : 0, gauge.Coda.Contains(Song.ARMY) ? 1 : 0 };
                var colors = new[] { Config.CodaBar.WMColor, Config.CodaBar.MBColor, Config.CodaBar.APColor };

                var coda = new Tuple<PluginConfigColor, float, LabelConfig?>[3];
                for (int i = 0; i < 3; i++)
                {
                    coda[i] = new Tuple<PluginConfigColor, float, LabelConfig?>(colors[order[i]], hasCoda[order[i]], null);
                }

                BarUtilities.GetChunkedBars(Config.CodaBar, coda, player).Draw(origin);
            }
        }

        private static List<uint> CausticBiteDoTIDs = new List<uint> { 124, 1200 };
        private static List<float> CausticBiteDoTDurations = new List<float> { 45, 45 };

        protected void DrawCausticBiteDoTBar(Vector2 origin, PlayerCharacter player)
        {
            var target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;

            BarUtilities.GetDoTBar(Config.CausticBiteDoTBar, player, target, CausticBiteDoTIDs, CausticBiteDoTDurations)?.
                         Draw(origin);
        }

        private static List<uint> StormbiteDoTIDs = new List<uint> { 129, 1201 };
        private static List<float> StormbiteDoTDurations = new List<float> { 45, 45 };

        protected void DrawStormbiteDoTBar(Vector2 origin, PlayerCharacter player)
        {
            var target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;

            BarUtilities.GetDoTBar(Config.StormbiteDoTBar, player, target, StormbiteDoTIDs, StormbiteDoTDurations)?.
                         Draw(origin);
        }

        private void HandleCurrentSong(Vector2 origin, PlayerCharacter player)
        {
            BRDGauge gauge = Plugin.JobGauges.Get<BRDGauge>();
            byte songStacks = gauge.Repertoire;
            Song song = gauge.Song;
            ushort songTimer = gauge.SongTimer;

            switch (song)
            {
                case Song.WANDERER:
                    if (Config.StacksBar.Enabled && Config.StacksBar.ShowWMStacks)
                    {
                        DrawStacksBar(
                            origin,
                            player,
                            songStacks,
                            3,
                            Config.StacksBar.WMStackColor,
                            Config.StacksBar.WMGlowConfig.Enabled && songStacks == 3 ? Config.StacksBar.WMGlowConfig : null
                            );
                    }

                    DrawSongTimerBar(origin, songTimer, Config.SongGaugeBar.WMColor);

                    break;

                case Song.MAGE:
                    if (Config.StacksBar.Enabled && Config.StacksBar.ShowMBProc)
                    {
                        DrawBloodletterReady(origin, player);
                    }

                    DrawSongTimerBar(origin, songTimer, Config.SongGaugeBar.MBColor);

                    break;

                case Song.ARMY:
                    if (Config.StacksBar.Enabled && Config.StacksBar.ShowAPStacks)
                    {
                        DrawStacksBar(origin, player, songStacks, 4, Config.StacksBar.APStackColor);
                    }

                    DrawSongTimerBar(origin, songTimer, Config.SongGaugeBar.APColor);

                    break;

                case Song.NONE:
                    if (Config.StacksBar.Enabled && !Config.StacksBar.HideWhenInactive)
                    {
                        DrawStacksBar(origin, player, 0, 3, Config.StacksBar.WMStackColor);
                    }

                    DrawSongTimerBar(origin, 0, EmptyColor);

                    break;

                default:
                    if (Config.StacksBar.Enabled && !Config.StacksBar.HideWhenInactive)
                    {
                        DrawStacksBar(origin, player, 0, 3, Config.StacksBar.WMStackColor);
                    }

                    DrawSongTimerBar(origin, 0, EmptyColor);

                    break;
            }
        }

        private void DrawBloodletterReady(Vector2 origin, PlayerCharacter player)
        {
            int maxStacks = player.Level < 84 ? 2 : 3;
            int maxCooldown = maxStacks * 15;
            int cooldown = Math.Max(0, _spellHelper.GetSpellCooldownInt(110) - 15);
            int stacks = (maxCooldown - cooldown) / 15;

            DrawStacksBar(origin, player, stacks, maxStacks, Config.StacksBar.MBProcColor,
                Config.StacksBar.MBGlowConfig.Enabled ? Config.StacksBar.MBGlowConfig : null);
        }

        protected void DrawSongTimerBar(Vector2 origin, ushort songTimer, PluginConfigColor songColor)
        {

            if (Config.SongGaugeBar.HideWhenInactive && songTimer == 0 || !Config.SongGaugeBar.Enabled)
            {
                return;
            }

            float duration = Math.Abs(songTimer / 1000f);

            Config.SongGaugeBar.Label.SetValue(duration);
            BarUtilities.GetProgressBar(Config.SongGaugeBar, duration, 45f, 0f, null, songColor)
                        .Draw(origin);
        }

        protected void DrawSoulVoiceBar(Vector2 origin)
        {
            BardSoulVoiceBarConfig config = Config.SoulVoiceBar;
            byte soulVoice = Plugin.JobGauges.Get<BRDGauge>().SoulVoice;

            if (config.HideWhenInactive && soulVoice == 0)
            {
                return;
            }

            config.Label.SetValue(soulVoice);

            BarUtilities.GetProgressBar(
                config,
                config.ThresholdConfig,
                new LabelConfig[] { config.Label },
                soulVoice,
                100f,
                0f,
                null,
                config.FillColor,
                soulVoice == 100f && config.GlowConfig.Enabled ? config.GlowConfig : null
            ).Draw(origin);
        }

        private void DrawStacksBar(Vector2 origin, PlayerCharacter player, int amount, int max, PluginConfigColor stackColor, BarGlowConfig? glowConfig = null)
        {
            BardStacksBarConfig config = Config.StacksBar;

            config.FillColor = stackColor;
            BarUtilities.GetChunkedBars(Config.StacksBar, max, amount, max, 0F, glowConfig: glowConfig).
                         Draw(origin);
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Ranged", 0)]
    [SubSection("Bard", 1)]
    public class BardConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.BRD;

        public new static BardConfig DefaultConfig()
        {
            var config = new BardConfig();

            config.SoulVoiceBar.Label.FontID = FontsConfig.DefaultMediumFontKey;

            config.StormbiteDoTBar.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.StormbiteDoTBar.Label.TextAnchor = DrawAnchor.Left;
            config.StormbiteDoTBar.Label.FrameAnchor = DrawAnchor.Left;
            config.StormbiteDoTBar.Label.Position = new Vector2(2, 0);

            config.CausticBiteDoTBar.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.CausticBiteDoTBar.Label.TextAnchor = DrawAnchor.Right;
            config.CausticBiteDoTBar.Label.FrameAnchor = DrawAnchor.Right;
            config.CausticBiteDoTBar.Label.Position = new Vector2(-2, 0);
            config.CausticBiteDoTBar.FillDirection = BarDirection.Left;

            config.SoulVoiceBar.ThresholdConfig.Enabled = true;
            config.SoulVoiceBar.ThresholdConfig.Value = 80;
            config.SoulVoiceBar.ThresholdConfig.ThresholdType = ThresholdType.Above;
            config.SoulVoiceBar.ThresholdConfig.ChangeColor = true;
            config.SoulVoiceBar.ThresholdConfig.Color = new PluginConfigColor(new Vector4(150f / 255f, 0f / 255f, 255f / 255f, 100f / 100f));

            return config;
        }

        [NestedConfig("Song Gauge Bar", 30)]
        public BardSongBarConfig SongGaugeBar = new BardSongBarConfig(
            new(0, -22),
            new(254, 20),
            new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 0f / 100f))
        );

        [NestedConfig("Soul Voice Bar", 35)]
        public BardSoulVoiceBarConfig SoulVoiceBar = new BardSoulVoiceBarConfig(
            new(0, -5),
            new(254, 10),
            new PluginConfigColor(new Vector4(248f / 255f, 227f / 255f, 0f / 255f, 100f / 100f))
        );

        [NestedConfig("Stacks Bar", 40)]
        public BardStacksBarConfig StacksBar = new BardStacksBarConfig(
            new(0, -39),
            new(254, 10),
            new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 0f / 100f))
        );

        [NestedConfig("Caustic Bite Bar", 60)]
        public ProgressBarConfig CausticBiteDoTBar = new ProgressBarConfig(
            new(-64, -51),
            new(126, 10),
            new PluginConfigColor(new Vector4(182f / 255f, 68f / 255f, 235f / 255f, 100f / 100f))
        );

        [NestedConfig("Stormbite Bar", 65)]
        public ProgressBarConfig StormbiteDoTBar = new ProgressBarConfig(
            new(64, -51),
            new(126, 10),
            new PluginConfigColor(new Vector4(72f / 255f, 117f / 255f, 202f / 255f, 100f / 100f))
        );

        [NestedConfig("Coda Bar", 40)]
        public BardCodaBarConfig CodaBar = new BardCodaBarConfig(
            new(0, -63),
            new(254, 10),
            new PluginConfigColor(new Vector4(0, 0, 0, 0))
        );
    }

    [DisableParentSettings("FillColor")]
    [Exportable(false)]
    public class BardSongBarConfig : ProgressBarConfig
    {
        [ColorEdit4("Wanderer's Minuet" + "##Song")]
        [Order(31)]
        public PluginConfigColor WMColor = new(new Vector4(158f / 255f, 157f / 255f, 36f / 255f, 100f / 100f));

        [ColorEdit4("Mage's Ballad" + "##Song")]
        [Order(32)]
        public PluginConfigColor MBColor = new(new Vector4(143f / 255f, 90f / 255f, 143f / 255f, 100f / 100f));

        [ColorEdit4("Army's Paeon" + "##Song")]
        [Order(33)]
        public PluginConfigColor APColor = new(new Vector4(207f / 255f, 205f / 255f, 52f / 255f, 100f / 100f));

        public BardSongBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
            : base(position, size, fillColor)
        {
        }
    }

    [Exportable(false)]
    public class BardSoulVoiceBarConfig : ProgressBarConfig
    {
        [NestedConfig("Show Glow", 36, separator = false, spacing = true)]
        public BarGlowConfig GlowConfig = new BarGlowConfig();

        public BardSoulVoiceBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
            : base(position, size, fillColor)
        {
        }
    }

    [DisableParentSettings("FillColor")]
    [Exportable(false)]
    public class BardStacksBarConfig : ChunkedBarConfig
    {
        [Checkbox("Wanderer's Minuet Stacks", separator = false, spacing = true)]
        [Order(51)]
        public bool ShowWMStacks = true;

        [NestedConfig("Wanderer's Minuet Stacks Glow", 52, separator = false, spacing = true)]
        public BarGlowConfig WMGlowConfig = new BarGlowConfig();

        [Checkbox("Mage's Ballad Proc" + "##Stacks")]
        [Order(53)]
        public bool ShowMBProc = true;

        [NestedConfig("Mage's Ballad Proc Glow", 54, separator = false, spacing = true)]
        public BarGlowConfig MBGlowConfig = new BarGlowConfig();

        [Checkbox("Army's Paeon Stacks" + "##Stacks")]
        [Order(56)]
        public bool ShowAPStacks = true;

        [ColorEdit4("Wanderer's Minuet Stack" + "##Stacks")]
        [Order(57)]
        public PluginConfigColor WMStackColor = new(new Vector4(150f / 255f, 215f / 255f, 232f / 255f, 100f / 100f));

        [ColorEdit4("Mage's Ballad Proc" + "##Stacks")]
        [Order(58)]
        public PluginConfigColor MBProcColor = new(new Vector4(199f / 255f, 46f / 255f, 46f / 255f, 100f / 100f));

        [ColorEdit4("Army's Paeon Stack" + "##Stacks")]
        [Order(59)]
        public PluginConfigColor APStackColor = new(new Vector4(0f / 255f, 222f / 255f, 177f / 255f, 100f / 100f));

        public BardStacksBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
            : base(position, size, fillColor)
        {
        }
    }

    [DisableParentSettings("FillColor")]
    [Exportable(false)]
    public class BardCodaBarConfig : ChunkedBarConfig
    {
        [ColorEdit4("Wanderer's Minuet" + "##Coda", spacing = true)]
        [Order(71)]
        public PluginConfigColor WMColor = new(new Vector4(145f / 255f, 186f / 255f, 94f / 255f, 100f / 100f));

        [ColorEdit4("Mage's Ballad" + "##Coda")]
        [Order(72)]
        public PluginConfigColor MBColor = new(new Vector4(143f / 255f, 90f / 255f, 143f / 255f, 100f / 100f));

        [ColorEdit4("Army's Paeon" + "##Coda")]
        [Order(73)]
        public PluginConfigColor APColor = new(new Vector4(207f / 255f, 205f / 255f, 52f / 255f, 100f / 100f));

        [DragDropHorizontal("Order", "Wanderer's Minuet", "Mage's Ballad", "Army's Paeon")]
        [Order(74)]
        public int[] CodaOrder = new int[] { 0, 1, 2 };

        public BardCodaBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor) : base(position, size, fillColor, 2) { }
    }
}
