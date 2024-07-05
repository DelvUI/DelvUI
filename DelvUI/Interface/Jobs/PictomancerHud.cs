using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;

namespace DelvUI.Interface.Jobs
{
    public class PictomancerHud : JobHud
    {
        private new PictomancerConfig Config => (PictomancerConfig)_config;
        
        private static PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;

        public PictomancerHud(PictomancerConfig config, string? displayName = null) : base(config, displayName)
        {

        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.PaletteBar.Enabled)
            {
                positions.Add(Config.Position + Config.PaletteBar.Position);
                sizes.Add(Config.PaletteBar.Size);
            }

            if (Config.PaintBar.Enabled)
            {
                positions.Add(Config.Position + Config.PaintBar.Position);
                sizes.Add(Config.PaintBar.Size);
            }

            if (Config.CreatureCanvasBar.Enabled)
            {
                positions.Add(Config.Position + Config.CreatureCanvasBar.Position);
                sizes.Add(Config.CreatureCanvasBar.Size);
            }

            if (Config.WeaponCanvasBar.Enabled)
            {
                positions.Add(Config.Position + Config.WeaponCanvasBar.Position);
                sizes.Add(Config.WeaponCanvasBar.Size);
            }

            if (Config.LandscapeCanvasBar.Enabled)
            {
                positions.Add(Config.Position + Config.LandscapeCanvasBar.Position);
                sizes.Add(Config.LandscapeCanvasBar.Size);
            }

            if (Config.HammerTimeBar.Enabled)
            {
                positions.Add(Config.Position + Config.HammerTimeBar.Position);
                sizes.Add(Config.HammerTimeBar.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, IPlayerCharacter player)
        {
            Vector2 pos = origin + Config.Position;

            if (Config.PaletteBar.Enabled)
            {
                DrawPaletteBar(pos, player);
            }

            if (Config.PaintBar.Enabled)
            {
                DrawPaintBar(pos, player);
            }

            if (Config.CreatureCanvasBar.Enabled)
            {
                DrawCreatureCanvasBar(pos, player);
            }

            if (Config.WeaponCanvasBar.Enabled)
            {
                DrawWeaponCanvasBar(pos, player);
            }

            if (Config.LandscapeCanvasBar.Enabled)
            {
                DrawLandscapeCanvasBar(pos, player);
            }

            if (Config.HammerTimeBar.Enabled)
            {
                DrawHammerTimeBar(pos, player);
            }
        }

        protected unsafe void DrawPaletteBar(Vector2 origin, IPlayerCharacter player)
        {
            PictomancerPaletteBarConfig config = Config.PaletteBar;
            PCTGauge gauge = Plugin.JobGauges.Get<PCTGauge>();

            if (config.HideWhenInactive && gauge.PalleteGauge == 0)
            {
                return;
            }

            config.Label.SetValue(gauge.PalleteGauge);

            bool isSubstractive = Utils.StatusListForBattleChara(player).FirstOrDefault(o => o.StatusId is 3674) != null;
            PluginConfigColor fillColor = isSubstractive ? config.SubtractiveColor : config.FillColor;

            BarHud[] bars = BarUtilities.GetChunkedProgressBars(
                config,
                2,
                gauge.PalleteGauge,
                100,
                0,
                player,
                fillColor: fillColor
            );

            foreach (BarHud bar in bars)
            {
                AddDrawActions(bar.GetDrawActions(origin, config.StrataLevel));
            }
        }

        private unsafe void DrawPaintBar(Vector2 origin, IPlayerCharacter player)
        {
            PictomancerPaintBarConfig config = Config.PaintBar;
            PCTGauge gauge = Plugin.JobGauges.Get<PCTGauge>();
            
            bool hasBlackPaint = Utils.StatusListForBattleChara(player).FirstOrDefault(o => o.StatusId is 3691) != null;
            
            var empty = new Tuple<PluginConfigColor, float, LabelConfig?>(EmptyColor, 1, null);
            var white = new Tuple<PluginConfigColor, float, LabelConfig?>(config.WhitePaintColor, 1, null);
            var black = new Tuple<PluginConfigColor, float, LabelConfig?>(config.BlackPaintColor, 1, null);
            
            List<Tuple<PluginConfigColor, float, LabelConfig?>> chunks = new List<Tuple<PluginConfigColor, float, LabelConfig?>>();
            
            for (int i = 1; i <= 5; i++)
            {
                if (i < gauge.Paint)
                {
                    chunks.Add(white);
                }
                else if (i == gauge.Paint)
                {
                    chunks.Add(hasBlackPaint ? black : white);
                }
                else
                {
                    chunks.Add(empty);
                }
            }

            if (config.HideWhenInactive && gauge.Paint == 0)
            {
                return;
            }
            BarHud[] bars = BarUtilities.GetChunkedBars(
                Config.PaintBar, 
                chunks.ToArray(), 
                player
            );
            /*
            BarHud[] bars = BarUtilities.GetChunkedBars(
                Config.PaintBar,
                5,
                gauge.Paint,
                5,
                0,
                player,
                fillColor: config.WhitePaintColor
            );
            */
            foreach (BarHud bar in bars)
            {
                AddDrawActions(bar.GetDrawActions(origin, config.StrataLevel));
            }
        }

        private unsafe void DrawCreatureCanvasBar(Vector2 origin, IPlayerCharacter player)
        {
            PictomancerCreatureCanvasBarConfig config = Config.CreatureCanvasBar;
            PCTGauge gauge = Plugin.JobGauges.Get<PCTGauge>();

            if (config.HideWhenInactive && !gauge.CreatureMotifDrawn && gauge.CreatureFlags != 0)
            {
                return;
            }

            // canvas
            PluginConfigColor? canvasColor = null;
            if (gauge.CanvasFlags.HasFlag(CanvasFlags.Pom))
            {
                canvasColor = config.PomColor;
            }
            else if (gauge.CanvasFlags.HasFlag(CanvasFlags.Wing))
            {
                canvasColor = config.WingsColor;
            }
            else if (gauge.CanvasFlags.HasFlag(CanvasFlags.Claw))
            {
                canvasColor = config.ClawColor;
            }
            else if (gauge.CanvasFlags.HasFlag(CanvasFlags.Maw))
            {
                canvasColor = config.FangsColor;
            }
            Tuple<PluginConfigColor, float, LabelConfig?> canvas = new(
                canvasColor ?? new(Vector4.Zero),
                canvasColor != null ? 1 : 0,
                null
            );

            // part drawing
            PluginConfigColor? drawingColor = null;
            if (gauge.CreatureFlags.HasFlag(CreatureFlags.Pom))
            {
                drawingColor = config.PomColor;
            }
            else if (gauge.CreatureFlags.HasFlag(CreatureFlags.Wings))
            {
                drawingColor = config.WingsColor;
            }
            else if (gauge.CreatureFlags.HasFlag(CreatureFlags.Claw))
            {
                drawingColor = config.ClawColor;
            }
            Tuple<PluginConfigColor, float, LabelConfig?> drawing = new(
                drawingColor ?? new(Vector4.Zero),
                drawingColor != null ? 1 : 0,
                null
            );

            // portrait
            PluginConfigColor? portraitColor = null;
            if (gauge.CreatureFlags.HasFlag(CreatureFlags.MooglePortait))
            {
                portraitColor = config.MoogleColor;
            }
            else if (gauge.CreatureFlags.HasFlag(CreatureFlags.MadeenPortrait))
            {
                portraitColor = config.MadeenColor;
            }
            Tuple<PluginConfigColor, float, LabelConfig?> portrait = new(
                portraitColor ?? new(Vector4.Zero),
                portraitColor != null ? 1 : 0,
                null
            );

            var chunks = new Tuple<PluginConfigColor, float, LabelConfig?>[3];
            chunks[0] = canvas;
            chunks[1] = drawing;
            chunks[2] = portrait;

            BarHud[] bars = BarUtilities.GetChunkedBars(config, chunks, player);
            foreach (BarHud bar in bars)
            {
                AddDrawActions(bar.GetDrawActions(origin, config.StrataLevel));
            }
        }

        private unsafe void DrawWeaponCanvasBar(Vector2 origin, IPlayerCharacter player)
        {
            ChunkedBarConfig config = Config.WeaponCanvasBar;
            PCTGauge gauge = Plugin.JobGauges.Get<PCTGauge>();

            bool active = gauge.CanvasFlags.HasFlag(CanvasFlags.Weapon);

            if (config.HideWhenInactive && !active)
            {
                return;
            }

            BarHud[] bars = BarUtilities.GetChunkedBars(config, 1, active ? 1 : 0, 1, 0, player);
            foreach (BarHud bar in bars)
            {
                AddDrawActions(bar.GetDrawActions(origin, config.StrataLevel));
            }
        }

        private unsafe void DrawLandscapeCanvasBar(Vector2 origin, IPlayerCharacter player)
        {
            ChunkedBarConfig config = Config.LandscapeCanvasBar;
            PCTGauge gauge = Plugin.JobGauges.Get<PCTGauge>();

            bool active = gauge.CanvasFlags.HasFlag(CanvasFlags.Landscape);

            if (config.HideWhenInactive && !active)
            {
                return;
            }

            BarHud[] bars = BarUtilities.GetChunkedBars(config, 1, active ? 1 : 0, 1, 0, player);
            foreach (BarHud bar in bars)
            {
                AddDrawActions(bar.GetDrawActions(origin, config.StrataLevel));
            }
        }

        private unsafe void DrawHammerTimeBar(Vector2 origin, IPlayerCharacter player)
        {
            ChunkedBarConfig config = Config.HammerTimeBar;
            int stacks = Utils.StatusListForBattleChara(player).FirstOrDefault(o => o.StatusId is 3680)?.StackCount ?? 0;

            if (config.HideWhenInactive && stacks == 0)
            {
                return;
            }

            BarHud[] bars = BarUtilities.GetChunkedBars(config, 3, stacks, 3, 0, player);
            foreach (BarHud bar in bars)
            {
                AddDrawActions(bar.GetDrawActions(origin, config.StrataLevel));
            }
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Caster", 0)]
    [SubSection("Pictomancer", 1)]
    public class PictomancerConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.PCT;

        public new static PictomancerConfig DefaultConfig()
        {
            var config = new PictomancerConfig();
            config.PaletteBar.UseChunks = false;
            config.PaletteBar.Label.Enabled = true;

            return config;
        }

        [NestedConfig("Palette Bar", 30)]
        public PictomancerPaletteBarConfig PaletteBar = new PictomancerPaletteBarConfig(
            new Vector2(0, -10),
            new Vector2(152, 18),
            PluginConfigColor.FromHex(0xFFC8C89F)
        );

        [NestedConfig("Paint Bar", 35)]
        public PictomancerPaintBarConfig PaintBar = new PictomancerPaintBarConfig(
            new(0, -26),
            new(254, 10)
        );

        [NestedConfig("Creature Canvas Bar", 40)]
        public PictomancerCreatureCanvasBarConfig CreatureCanvasBar = new PictomancerCreatureCanvasBarConfig(
            new(0, -38),
            new(254, 10)
        );

        [NestedConfig("Weapon Canvas Bar", 40)]
        public ChunkedBarConfig WeaponCanvasBar = new ChunkedBarConfig(
            new(102.5f, -10),
            new(49, 18),
            PluginConfigColor.FromHex(0xFFC5616C)
        );

        [NestedConfig("Landscape Canvas Bar", 40)]
        public ChunkedBarConfig LandscapeCanvasBar = new ChunkedBarConfig(
            new(102.5f, -10),
            new(49, 18),
            PluginConfigColor.FromHex(0xFF8690E5)
        );

        [NestedConfig("Hammer Time Bar", 40)]
        public ChunkedBarConfig HammerTimeBar = new ChunkedBarConfig(
            new(0, -50),
            new(254, 10),
            PluginConfigColor.FromHex(0xFFFFFFFF)
        );
    }

    [Exportable(false)]
    public class PictomancerPaletteBarConfig : ChunkedProgressBarConfig
    {
        [ColorEdit4("Subtractive Fill Color")]
        [Order(27)]
        public PluginConfigColor SubtractiveColor = PluginConfigColor.FromHex(0xFFAF6BAE);

        public PictomancerPaletteBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
             : base(position, size, fillColor)
        {
        }
    }

    [DisableParentSettings("FillColor")]
    [Exportable(false)]
    public class PictomancerPaintBarConfig : ChunkedBarConfig
    {
        [ColorEdit4("White Paint Color")]
        [Order(26)]
        public PluginConfigColor WhitePaintColor = PluginConfigColor.FromHex(0xFF00FFFF);

        [ColorEdit4("Black Paint Color")]
        [Order(27)]
        public PluginConfigColor BlackPaintColor = PluginConfigColor.FromHex(0xFFDB57DB);

        public PictomancerPaintBarConfig(Vector2 position, Vector2 size)
             : base(position, size, new(Vector4.Zero))
        {
        }
    }

    [DisableParentSettings("FillColor")]
    [Exportable(false)]
    public class PictomancerCreatureCanvasBarConfig : ChunkedBarConfig
    {
        [ColorEdit4("Pom Color")]
        [Order(17)]
        public PluginConfigColor PomColor = PluginConfigColor.FromHex(0xFFE69378);

        [ColorEdit4("Wings Color")]
        [Order(18)]
        public PluginConfigColor WingsColor = PluginConfigColor.FromHex(0xFFD38BE4);

        [ColorEdit4("Claw Color")]
        [Order(19)]
        public PluginConfigColor ClawColor = PluginConfigColor.FromHex(0xFFA16854);

        [ColorEdit4("Fangs Color")]
        [Order(20)]
        public PluginConfigColor FangsColor = PluginConfigColor.FromHex(0xFF80BFBD);

        [ColorEdit4("Moogle Color")]
        [Order(21)]
        public PluginConfigColor MoogleColor = PluginConfigColor.FromHex(0xFFA745C7);

        [ColorEdit4("Madeen Color")]
        [Order(22)]
        public PluginConfigColor MadeenColor = PluginConfigColor.FromHex(0xFF93Cf7D);

        public PictomancerCreatureCanvasBarConfig(Vector2 position, Vector2 size)
             : base(position, size, new(Vector4.Zero))
        {
        }
    }
}
