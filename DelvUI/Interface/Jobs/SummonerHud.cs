using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
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
    public class SummonerHud : JobHud
    {
        private bool _bahamutFinished = true;

        private new SummonerConfig Config => (SummonerConfig)_config;

        public SummonerHud(SummonerConfig config, string? displayName = null) : base(config, displayName)
        {
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new();
            List<Vector2> sizes = new();

            if (Config.AetherflowBar.Enabled)
            {
                positions.Add(Config.Position + Config.AetherflowBar.Position);
                sizes.Add(Config.AetherflowBar.Size);
            }

            if (Config.DemiStatusIndicatorBar.Enabled)
            {
                positions.Add(Config.Position + Config.DemiStatusIndicatorBar.Position);
                sizes.Add(Config.DemiStatusIndicatorBar.Size);
            }

            if (Config.DreadwyrmAetherBar.Enabled)
            {
                positions.Add(Config.Position + Config.DreadwyrmAetherBar.Position);
                sizes.Add(Config.DreadwyrmAetherBar.Size);
            }

            if (Config.TranceBar.Enabled)
            {
                positions.Add(Config.Position + Config.TranceBar.Position);
                sizes.Add(Config.TranceBar.Size);
            }

            if (Config.RuinBar.Enabled)
            {
                positions.Add(Config.Position + Config.RuinBar.Position);
                sizes.Add(Config.RuinBar.Size);
            }

            if (Config.MiasmaBar.Enabled)
            {
                positions.Add(Config.Position + Config.MiasmaBar.Position);
                sizes.Add(Config.MiasmaBar.Size);
            }

            if (Config.BioBar.Enabled)
            {
                positions.Add(Config.Position + Config.BioBar.Position);
                sizes.Add(Config.BioBar.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            Vector2 pos = origin + Config.Position;

            if (Config.AetherflowBar.Enabled)
            {
                DrawAetherBar(pos, player);
            }

            if (Config.DemiStatusIndicatorBar.Enabled)
            {
                DrawDemiStatusIndicatorBar(pos);
            }

            if (Config.DreadwyrmAetherBar.Enabled)
            {
                DrawDreadwyrmAetherBar(pos);
            }

            if (Config.TranceBar.Enabled)
            {
                DrawTranceBar(pos);
            }

            if (Config.RuinBar.Enabled)
            {
                DrawRuinBar(pos, player);
            }

            if (Config.MiasmaBar.Enabled)
            {
                DrawMiasmaBar(pos, player);
            }

            if (Config.BioBar.Enabled)
            {
                DrawBioBar(pos, player);
            }
        }

        private void DrawAetherBar(Vector2 origin, PlayerCharacter player)
        {
            byte stackCount = player.StatusList.FirstOrDefault(o => o.StatusId == 304)?.StackCount ?? 0;

            if (Config.AetherflowBar.HideWhenInactive && stackCount == 0)
            {
                return;
            };

            BarUtilities.GetChunkedBars(Config.AetherflowBar, 2, stackCount, 2)
                .Draw(origin);
        }

        private void DrawDemiStatusIndicatorBar(Vector2 origin)
        {
            SMNGauge gauge = Plugin.JobGauges.Get<SMNGauge>();
            byte stacks = (byte)gauge.AetherFlags;

            if (Config.DemiStatusIndicatorBar.HideWhenInactive && stacks == 0)
            {
                return;
            }

            PluginConfigColor color = stacks >= 16 ? Config.DemiStatusIndicatorBar.PhoenixColor : Config.DemiStatusIndicatorBar.BahamutColor;
            int value = stacks < 8 ? 0 : 1;

            BarUtilities.GetBar(Config.DemiStatusIndicatorBar, value, 1, fillColor: color)
                .Draw(origin);
        }

        private void DrawDreadwyrmAetherBar(Vector2 origin)
        {
            SMNGauge gauge = Plugin.JobGauges.Get<SMNGauge>();
            byte stacks = (byte)gauge.AetherFlags;

            if (Config.DreadwyrmAetherBar.HideWhenInactive && stacks == 0)
            {
                return;
            }

            var value = 0;

            if (stacks >= 4 && stacks < 8)
            {
                value = 1;
            }
            else if (stacks >= 8 && stacks < 16)
            {
                value = 2;
            }

            BarUtilities.GetChunkedBars(Config.DreadwyrmAetherBar, 2, value, 2)
                .Draw(origin);
        }

        private void DrawTranceBar(Vector2 origin)
        {
            SMNGauge gauge = Plugin.JobGauges.Get<SMNGauge>();

            PluginConfigColor tranceColor;
            float maxDuration;
            float tranceDuration = gauge.SummonTimerRemaining;

            if (!_bahamutFinished && tranceDuration < 1)
            {
                _bahamutFinished = true;
            }

            if (Config.TranceBar.HideWhenInactive && tranceDuration == 0)
            {
                return;
            }

            byte flags = (byte)gauge.AetherFlags;
            switch (flags)
            {
                case >= 16:
                    tranceColor = Config.TranceBar.PhoenixColor;
                    maxDuration = 20000f;
                    break;

                case >= 8:
                    tranceColor = Config.TranceBar.BahamutColor;
                    maxDuration = 20000f;
                    _bahamutFinished = false;

                    break;

                default:
                    // This is needed because as soon as you summon Bahamut the flag goes back to 0-2
                    tranceColor = _bahamutFinished ? Config.TranceBar.FillColor : Config.TranceBar.BahamutColor;
                    maxDuration = _bahamutFinished ? 15000f : 20000f;

                    break;
            }

            Config.TranceBar.Label.SetValue(tranceDuration / 1000f);
            BarUtilities.GetProgressBar(Config.TranceBar, tranceDuration, maxDuration, fillColor: tranceColor)
                .Draw(origin);
        }

        private void DrawRuinBar(Vector2 origin, PlayerCharacter player)
        {
            byte stackCount = player.StatusList.FirstOrDefault(o => o.StatusId == 1212)?.StackCount ?? 0;

            if (Config.RuinBar.HideWhenInactive && stackCount == 0)
            {
                return;
            };

            BarUtilities.GetChunkedBars(Config.RuinBar, 4, stackCount, 4)
                .Draw(origin);
        }

        private static List<uint> MiasmaIDs = new List<uint> { 1215, 180 };
        private static List<float> MiasmaDurations = new List<float> { 30, 30 };

        protected void DrawMiasmaBar(Vector2 origin, PlayerCharacter player)
        {
            var target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;

            BarUtilities.GetDoTBar(Config.MiasmaBar, player, target, MiasmaIDs, MiasmaDurations)?.
                Draw(origin);
        }

        private static List<uint> BioIDs = new List<uint> { 1214, 179, 189 };
        private static List<float> BioDurations = new List<float> { 30, 30, 30 };

        protected void DrawBioBar(Vector2 origin, PlayerCharacter player)
        {
            var target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;

            BarUtilities.GetDoTBar(Config.BioBar, player, target, BioIDs, BioDurations)?.
                Draw(origin);
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Caster", 0)]
    [SubSection("Summoner", 1)]
    public class SummonerConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.SMN;
        public new static SummonerConfig DefaultConfig()
        {
            var config = new SummonerConfig();

            config.TranceBar.Label.FontID = FontsConfig.DefaultMediumFontKey;

            config.MiasmaBar.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.MiasmaBar.Label.TextAnchor = DrawAnchor.Right;
            config.MiasmaBar.Label.FrameAnchor = DrawAnchor.Right;
            config.MiasmaBar.Label.Position = new Vector2(-2, 0);

            config.BioBar.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.BioBar.Label.TextAnchor = DrawAnchor.Left;
            config.BioBar.Label.FrameAnchor = DrawAnchor.Left;
            config.BioBar.Label.Position = new Vector2(2, 0);

            return config;
        }

        [NestedConfig("Aetherflow Bar", 40)]
        public ChunkedBarConfig AetherflowBar = new ChunkedBarConfig(
            new(-67, -6),
            new(120, 10),
            new(new Vector4(0f / 255f, 255f / 255f, 0f / 255f, 100f / 100f))
        );

        [NestedConfig("Demi Status Indicator Bar", 45)]
        public SummonerDemiStatusIndicatorBarConfig DemiStatusIndicatorBar = new SummonerDemiStatusIndicatorBarConfig(
            new(0, -6),
            new(10, 10)
        );

        [NestedConfig("Dreadwyrm Aether Bar", 50)]
        public ChunkedBarConfig DreadwyrmAetherBar = new ChunkedBarConfig(
            new(67, -6),
            new(120, 10),
            new(new Vector4(128f / 255f, 255f / 255f, 255f / 255f, 100f / 100f))
        );

        [NestedConfig("Trance Bar", 55)]
        public SummonerTranceBarConfig TranceBar = new SummonerTranceBarConfig(
            new(0, -20),
            new(254, 14),
            new(new Vector4(128f / 255f, 255f / 255f, 255f / 255f, 100f / 100f))
        );

        [NestedConfig("Ruin Bar", 60)]
        public ChunkedBarConfig RuinBar = new ChunkedBarConfig(
            new(0, -34),
            new(254, 10),
            new PluginConfigColor(new Vector4(94f / 255f, 250f / 255f, 154f / 255f, 100f / 100f))
        );

        [NestedConfig("Miasma Bar", 65)]
        public ProgressBarConfig MiasmaBar = new ProgressBarConfig(
            new(-64, -48),
            new(126, 14),
            new PluginConfigColor(new Vector4(106f / 255f, 237f / 255f, 241f / 255f, 100f / 100f)),
            BarDirection.Left
        );

        [NestedConfig("Bio Bar", 70)]
        public ProgressBarConfig BioBar = new ProgressBarConfig(
            new(64, -48),
            new(126, 14),
            new PluginConfigColor(new Vector4(50f / 255f, 93f / 255f, 37f / 255f, 100f / 100f))
        );
    }

    [DisableParentSettings("FillColor")]
    [Exportable(false)]
    public class SummonerDemiStatusIndicatorBarConfig : BarConfig
    {
        [ColorEdit4("Bahamut Color")]
        [Order(26)]
        public PluginConfigColor BahamutColor = new(new Vector4(128f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [ColorEdit4("Phoenix Color")]
        [Order(27)]
        public PluginConfigColor PhoenixColor = new(new Vector4(255f / 255f, 128f / 255f, 0f / 255f, 100f / 100f));

        public SummonerDemiStatusIndicatorBarConfig(Vector2 position, Vector2 size)
            : base(position, size, new(Vector4.Zero))
        {
        }
    }

    [Exportable(false)]
    public class SummonerTranceBarConfig : ProgressBarConfig
    {
        [ColorEdit4("Bahamut Color")]
        [Order(26)]
        public PluginConfigColor BahamutColor = new(new Vector4(128f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [ColorEdit4("Phoenix Color")]
        [Order(27)]
        public PluginConfigColor PhoenixColor = new(new Vector4(255f / 255f, 128f / 255f, 0f / 255f, 100f / 100f));

        public SummonerTranceBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
            : base(position, size, fillColor)
        {
        }
    }
}