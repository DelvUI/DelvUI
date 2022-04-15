using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DelvUI.Enums;
using DelvUI.Interface.GeneralElements;

namespace DelvUI.Interface.Jobs
{
    public class AstrologianHud : JobHud
    {
        private readonly SpellHelper _spellHelper = new();
        private new AstrologianConfig Config => (AstrologianConfig)_config;
        private static PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;

        private static readonly List<uint> DotIDs = new() { 1881, 843, 838 };
        private static readonly List<float> DotDuration = new() { 30f, 30f, 18f };
        private const float STAR_MAX_DURATION = 10f;
        private const float LIGHTSPEED_MAX_DURATION = 15f;

        public AstrologianHud(JobConfig config, string? displayName = null) : base(config, displayName)
        {
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new();
            List<Vector2> sizes = new();

            if (Config.DrawBar.Enabled)
            {
                positions.Add(Config.Position + Config.DrawBar.Position);
                sizes.Add(Config.DrawBar.Size);
            }

            if (Config.MinorArcanaBar.Enabled)
            {
                positions.Add(Config.Position + Config.MinorArcanaBar.Position);
                sizes.Add(Config.MinorArcanaBar.Size);
            }

            if (Config.AstrodyneBar.Enabled)
            {
                positions.Add(Config.Position + Config.AstrodyneBar.Position);
                sizes.Add(Config.AstrodyneBar.Size);
            }

            if (Config.DotBar.Enabled)
            {
                positions.Add(Config.Position + Config.DotBar.Position);
                sizes.Add(Config.DotBar.Size);
            }

            if (Config.StarBar.Enabled)
            {
                positions.Add(Config.Position + Config.StarBar.Position);
                sizes.Add(Config.StarBar.Size);
            }

            if (Config.LightspeedBar.Enabled)
            {
                positions.Add(Config.Position + Config.LightspeedBar.Position);
                sizes.Add(Config.LightspeedBar.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            Vector2 pos = origin + Config.Position;

            if (Config.AstrodyneBar.Enabled)
            {
                DrawAstrodyneBar(pos, player);
            }

            if (Config.DrawBar.Enabled)
            {
                DrawDraw(pos, player);
            }

            if (Config.MinorArcanaBar.Enabled)
            {
                DrawMinorArcana(pos, player);
            }

            if (Config.DotBar.Enabled)
            {
                DrawDot(pos, player);
            }

            if (Config.LightspeedBar.Enabled)
            {
                DrawLightspeed(pos, player);
            }

            if (Config.StarBar.Enabled)
            {
                DrawStar(pos, player);
            }
        }

        private void DrawAstrodyneBar(Vector2 origin, PlayerCharacter player)
        {
            List<PluginConfigColor> chunkColors = new();
            ASTGauge gauge = Plugin.JobGauges.Get<ASTGauge>();
            bool[] chucksToGlow = new bool[3];

            for (int ix = 0; ix < 3; ++ix)
            {
                SealType type = gauge.Seals[ix];

                switch (type)
                {
                    case SealType.NONE:
                        chunkColors.Add(EmptyColor);

                        break;

                    case SealType.MOON:
                        chunkColors.Add(Config.AstrodyneBar.SealLunarColor);

                        break;

                    case SealType.SUN:
                        chunkColors.Add(Config.AstrodyneBar.SealSunColor);

                        break;

                    case SealType.CELESTIAL:
                        chunkColors.Add(Config.AstrodyneBar.SealCelestialColor);

                        break;
                }

                int sealNumbers = 0;
                Config.AstrodyneBar.Label.SetText("");

                if (gauge.ContainsSeal(SealType.NONE))
                {
                    continue;
                }

                if (gauge.ContainsSeal(SealType.SUN))
                {
                    sealNumbers++;
                }

                if (gauge.ContainsSeal(SealType.MOON))
                {
                    sealNumbers++;
                }

                if (gauge.ContainsSeal(SealType.CELESTIAL))
                {
                    sealNumbers++;
                }

                Config.AstrodyneBar.Label.SetText(sealNumbers.ToString());


                for (int i = 0; i < sealNumbers; i++)
                {
                    chucksToGlow[i] = true;
                }

            }

            if (chunkColors.All(n => n == EmptyColor) && Config.AstrodyneBar.HideWhenInactive)
            {
                return;
            }

            Tuple<PluginConfigColor, float, LabelConfig?>[] astrodyneChunks = {
                new(chunkColors[0], chunkColors[0] != EmptyColor ? 1f : 0f, null),
                new(chunkColors[1], chunkColors[1] != EmptyColor ? 1f : 0f, Config.AstrodyneBar.Label),
                new(chunkColors[2], chunkColors[2] != EmptyColor ? 1f : 0f, null) };

            BarHud[] bars = BarUtilities.GetChunkedBars(Config.AstrodyneBar, astrodyneChunks, player, Config.AstrodyneBar.AstrodyneGlowConfig, chucksToGlow);
            foreach (BarHud bar in bars)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.AstrodyneBar.StrataLevel));
            }
        }

        private void DrawDraw(Vector2 origin, PlayerCharacter player)
        {
            ASTGauge gauge = Plugin.JobGauges.Get<ASTGauge>();

            string cardJob = "";
            PluginConfigColor cardColor = EmptyColor;

            if (gauge.DrawnCard == CardType.NONE && Config.DrawBar.HideWhenInactive)
            {
                return;
            }

            switch (gauge.DrawnCard)
            {
                case CardType.BALANCE:
                    cardColor = Config.AstrodyneBar.SealSunColor;
                    cardJob = "MELEE";
                    Config.DrawBar.DrawGlowConfig.Color = new PluginConfigColor(Config.DrawBar.DrawMeleeGlowColor.Vector);
                    break;

                case CardType.BOLE:
                    cardColor = Config.AstrodyneBar.SealSunColor;
                    cardJob = "RANGED";
                    Config.DrawBar.DrawGlowConfig.Color = new PluginConfigColor(Config.DrawBar.DrawRangedGlowColor.Vector);
                    break;

                case CardType.ARROW:
                    cardColor = Config.AstrodyneBar.SealLunarColor;
                    cardJob = "MELEE";
                    Config.DrawBar.DrawGlowConfig.Color = new PluginConfigColor(Config.DrawBar.DrawMeleeGlowColor.Vector);
                    break;

                case CardType.EWER:
                    cardColor = Config.AstrodyneBar.SealLunarColor;
                    cardJob = "RANGED";
                    Config.DrawBar.DrawGlowConfig.Color = new PluginConfigColor(Config.DrawBar.DrawRangedGlowColor.Vector);
                    break;

                case CardType.SPEAR:
                    cardColor = Config.AstrodyneBar.SealCelestialColor;
                    cardJob = "MELEE";
                    Config.DrawBar.DrawGlowConfig.Color = new PluginConfigColor(Config.DrawBar.DrawMeleeGlowColor.Vector);
                    break;

                case CardType.SPIRE:
                    cardColor = Config.AstrodyneBar.SealCelestialColor;
                    cardJob = "RANGED";
                    Config.DrawBar.DrawGlowConfig.Color = new PluginConfigColor(Config.DrawBar.DrawRangedGlowColor.Vector);
                    break;

                case CardType.NONE:
                    Config.DrawBar.DrawGlowConfig.Color = new PluginConfigColor(Vector4.Zero);
                    break;
            }

            float cardPresent;
            float cardMax;
            float drawCastInfo = _spellHelper.GetSpellCooldown(3590);
            int drawCharges = _spellHelper.GetStackCount(2, 3590);
            float current = drawCastInfo % 30 + 0.75f;

            if (cardJob != "")
            {
                cardPresent = 1f;
                cardMax = 1f;
                Config.DrawBar.Label.SetText(cardJob);

                if (Config.DrawBar.DrawDrawLabel.Enabled)
                {
                    if (drawCastInfo > 0 && drawCharges == 0)
                    {
                        Config.DrawBar.DrawDrawLabel.SetValue(current);
                    }
                    else if (drawCastInfo > 0 && drawCharges > 0)
                    {
                        Config.DrawBar.DrawDrawLabel.SetText("READY (" + current.ToString("0") + ")");
                    }
                    else
                    {
                        Config.DrawBar.DrawDrawLabel.SetText("READY");
                    }
                }
                else
                {
                    Config.DrawBar.DrawDrawLabel.SetText("");
                }
            }
            else
            {
                cardPresent = drawCastInfo > 0 ? current : 1f;

                switch (drawCastInfo)
                {
                    case > 0 when drawCharges == 0:
                        Config.DrawBar.Label.SetValue(current);
                        break;
                    case > 0 when drawCharges > 0:
                        Config.DrawBar.Label.SetText("READY (" + current.ToString("0") + ")");
                        break;
                    default:
                        Config.DrawBar.Label.SetText("READY");
                        break;
                }

                Config.DrawBar.DrawDrawLabel.SetText("");
                cardColor = drawCharges > 0 ? Config.DrawBar.DrawCdReadyColor : Config.DrawBar.DrawCdColor;
                cardMax = drawCastInfo > 0 ? 60f : 1f;
            }

            Config.DrawBar.DrawDrawChargesLabel.SetValue(drawCharges);
            LabelConfig[] labels = new LabelConfig[] { Config.DrawBar.Label, Config.DrawBar.DrawDrawChargesLabel, Config.DrawBar.DrawDrawLabel };
            BarGlowConfig? glowConfig = Config.DrawBar.DrawGlowConfig.Enabled && Math.Abs(cardMax - 1f) == 0f ? Config.DrawBar.DrawGlowConfig : null;

            BarHud bar = BarUtilities.GetBar(Config.DrawBar, cardPresent, cardMax, 0f, player, cardColor, glowConfig, labels);
            AddDrawActions(bar.GetDrawActions(origin, Config.DrawBar.StrataLevel));
        }

        private void DrawMinorArcana(Vector2 pos, PlayerCharacter player)
        {
            ASTGauge gauge = Plugin.JobGauges.Get<ASTGauge>();

            string crownCardDrawn = "";
            PluginConfigColor crownCardColor = EmptyColor;

            if (gauge.DrawnCrownCard == CardType.NONE && Config.MinorArcanaBar.HideWhenInactive)
            {
                return;
            }

            switch (gauge.DrawnCrownCard)
            {
                case CardType.LADY:
                    crownCardColor = Config.MinorArcanaBar.LadyDrawnColor;
                    crownCardDrawn = "LADY";
                    break;

                case CardType.LORD:
                    crownCardColor = Config.MinorArcanaBar.LordDrawnColor;
                    crownCardDrawn = "LORD";
                    break;
            }

            float crownCardPresent;
            float crownCardMax;
            float cooldown = _spellHelper.GetSpellCooldown(7443);

            if (crownCardDrawn != "")
            {
                crownCardPresent = 1f;
                crownCardMax = 1f;
                Config.MinorArcanaBar.Label.SetText(crownCardDrawn);

                if (Config.MinorArcanaBar.CrownDrawTimerLabel.Enabled)
                {
                    switch (cooldown)
                    {
                        case > 0:
                            Config.MinorArcanaBar.CrownDrawTimerLabel.SetValue(cooldown);
                            break;
                        default:
                            Config.MinorArcanaBar.CrownDrawTimerLabel.SetText("READY");
                            break;
                    }
                }
                else
                {
                    Config.MinorArcanaBar.CrownDrawTimerLabel.SetText("");
                }
            }
            else
            {
                crownCardPresent = cooldown > 0 ? cooldown : 1f;

                if (cooldown > 0)
                {
                    Config.MinorArcanaBar.Label.SetValue(cooldown);
                }
                else
                {
                    Config.MinorArcanaBar.Label.SetText("READY");
                }

                Config.MinorArcanaBar.CrownDrawTimerLabel.SetText("");
                crownCardColor = cooldown > 0 ? Config.MinorArcanaBar.CrownDrawCdColor : Config.MinorArcanaBar.CrownDrawCdReadyColor;
                crownCardMax = cooldown > 0 ? 60f : 1f;
            }

            LabelConfig[] labels = { Config.MinorArcanaBar.Label, Config.MinorArcanaBar.CrownDrawTimerLabel };
            BarHud bar = BarUtilities.GetBar(Config.MinorArcanaBar, crownCardPresent, crownCardMax, 0f, player, crownCardColor, labels: labels);
            AddDrawActions(bar.GetDrawActions(pos, Config.MinorArcanaBar.StrataLevel));
        }

        private void DrawDot(Vector2 origin, PlayerCharacter player)
        {
            GameObject? target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            BarHud? bar = BarUtilities.GetDoTBar(Config.DotBar, player, target, DotIDs, DotDuration);
            if (bar != null)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.DotBar.StrataLevel));
            }
        }

        private void DrawLightspeed(Vector2 origin, PlayerCharacter player)
        {
            float lightspeedDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 841 && o.SourceID == player.ObjectId)?.RemainingTime ?? 0f;

            if (Config.LightspeedBar.HideWhenInactive && !(lightspeedDuration > 0))
            {
                return;
            }

            Config.LightspeedBar.Label.SetValue(lightspeedDuration);

            BarHud bar = BarUtilities.GetProgressBar(Config.LightspeedBar, lightspeedDuration, LIGHTSPEED_MAX_DURATION, 0, player);
            AddDrawActions(bar.GetDrawActions(origin, Config.LightspeedBar.StrataLevel));
        }

        private void DrawStar(Vector2 origin, PlayerCharacter player)
        {
            float starPreCookingBuff = player.StatusList.FirstOrDefault(o => o.StatusId is 1224 && o.SourceID == player.ObjectId)?.RemainingTime ?? 0f;
            float starPostCookingBuff = player.StatusList.FirstOrDefault(o => o.StatusId is 1248 && o.SourceID == player.ObjectId)?.RemainingTime ?? 0f;

            if (Config.StarBar.HideWhenInactive && starPostCookingBuff == 0f && starPreCookingBuff == 0f)
            {
                return;
            }

            float currentStarDuration = starPreCookingBuff > 0 ? STAR_MAX_DURATION - Math.Abs(starPreCookingBuff) : Math.Abs(starPostCookingBuff);
            PluginConfigColor currentStarColor = starPreCookingBuff > 0 ? Config.StarBar.StarEarthlyColor : Config.StarBar.StarGiantColor;

            Config.StarBar.Label.SetValue(currentStarDuration);

            // Star Countdown after Star is ready 
            BarHud bar = BarUtilities.GetProgressBar(Config.StarBar, currentStarDuration, STAR_MAX_DURATION, 0f, player, currentStarColor, Config.StarBar.StarGlowConfig.Enabled && starPostCookingBuff > 0 ? Config.StarBar.StarGlowConfig : null);
            AddDrawActions(bar.GetDrawActions(origin, Config.StarBar.StrataLevel));
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Healer", 0)]
    [SubSection("Astrologian", 1)]
    public class AstrologianConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.AST;

        public new static AstrologianConfig DefaultConfig()
        {
            var config = new AstrologianConfig();

            config.UseDefaultPrimaryResourceBar = true;
            config.AstrodyneBar.Label.FontID = FontsConfig.DefaultMediumFontKey;

            return config;
        }

        [NestedConfig("Draw Bar", 100)]
        public AstrologianDrawBarConfig DrawBar = new(
            new Vector2(0, -32),
            new Vector2(254, 20),
            new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 0f / 100f))
        );

        [NestedConfig("Minor Arcana Bar", 150)]
        public AstrologianCrownDrawBarConfig MinorArcanaBar = new(
            new Vector2(64, -71),
            new Vector2(126, 10),
            new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 0f / 100f))
        );

        [NestedConfig("Astrodyne Bar", 200)]
        public AstrologianAstrodyneBarConfig AstrodyneBar = new(
            new Vector2(-64, -71),
            new Vector2(126, 10)
        );

        [NestedConfig("Dot Bar", 300)]
        public ProgressBarConfig DotBar = new(
            new Vector2(-85, -54),
            new Vector2(84, 20),
            new PluginConfigColor(new Vector4(20f / 255f, 80f / 255f, 168f / 255f, 255f / 100f))
        );

        [NestedConfig("Star Bar", 400)]
        public AstrologianStarBarConfig StarBar = new(
            new Vector2(0, -54),
            new Vector2(84, 20)
        );

        [NestedConfig("Lightspeed Bar", 500)]
        public ProgressBarConfig LightspeedBar = new(
            new Vector2(85, -54),
            new Vector2(84, 20),
            new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 173f / 255f, 100f / 100f))
        );

        [DisableParentSettings("FillColor", "Color")]
        [Exportable(false)]
        public class AstrologianDrawBarConfig : ProgressBarConfig
        {

            [NestedConfig("Draw Side Timer Label" + "##Draw", 101, separator = false, spacing = true)]
            public NumericLabelConfig DrawDrawLabel = new(new Vector2(0, 0), "", DrawAnchor.Left, DrawAnchor.Left);

            [NestedConfig("Draw Charges Label" + "##Draw", 104, separator = false, spacing = true)]
            public NumericLabelConfig DrawDrawChargesLabel = new(new Vector2(0, 0), "", DrawAnchor.Right, DrawAnchor.Right);

            [ColorEdit4("Draw on CD" + "##Draw")]
            [Order(109)]
            public PluginConfigColor DrawCdColor = new(new Vector4(26f / 255f, 167f / 255f, 109f / 255f, 100f / 100f));

            [ColorEdit4("Draw Ready" + "##Draw")]
            [Order(110)]
            public PluginConfigColor DrawCdReadyColor = new(new Vector4(137f / 255f, 26f / 255f, 42f / 255f, 100f / 100f));


            [NestedConfig("Card Preferred Target with Glow" + "##Astrodyne", 111, separator = false, spacing = true)]
            //[DisableParentSettings("Color")]
            //TODO: Remove Color from GlowConfig
            public BarGlowConfig DrawGlowConfig = new();

            [ColorEdit4("Melee Glow" + "##Draw")]
            [Order(112)]
            public PluginConfigColor DrawMeleeGlowColor = new(new Vector4(83f / 255f, 34f / 255f, 120f / 255f, 100f / 100f));

            [ColorEdit4("Ranged Glow" + "##Draw")]
            [Order(113)]
            public PluginConfigColor DrawRangedGlowColor = new(new Vector4(124f / 255f, 34f / 255f, 120f / 255f, 100f / 100f));

            public AstrologianDrawBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
                : base(position, size, fillColor)
            {
            }
        }

        [DisableParentSettings("FillColor", "Color")]
        [Exportable(false)]
        public class AstrologianCrownDrawBarConfig : ProgressBarConfig
        {
            [NestedConfig("Minor Arcana Side Timer Label" + "##CrownDraw", 119, separator = false, spacing = true)]
            public NumericLabelConfig CrownDrawTimerLabel = new(new Vector2(0, 0), "", DrawAnchor.Left, DrawAnchor.Left);

            [ColorEdit4("Minor Arcana on CD" + "##CrownDraw")]
            [Order(120)]
            public PluginConfigColor CrownDrawCdColor = new(new Vector4(26f / 255f, 167f / 255f, 109f / 255f, 100f / 100f));

            [ColorEdit4("Minor Arcana Ready" + "##CrownDraw")]
            [Order(121)]
            public PluginConfigColor CrownDrawCdReadyColor = new(new Vector4(65f / 255f, 100f / 255f, 205f / 255f, 100f / 100f));

            [ColorEdit4("Lord Color" + "##CrownDraw")]
            [Order(112)]
            public PluginConfigColor LordDrawnColor = new(new Vector4(182f / 255f, 92f / 255f, 72f / 255f, 100f / 100f));

            [ColorEdit4("Lady Color" + "##CrownDraw")]
            [Order(113)]
            public PluginConfigColor LadyDrawnColor = new(new Vector4(252f / 255f, 209f / 255f, 239f / 255f, 100f / 100f));

            public AstrologianCrownDrawBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
                : base(position, size, fillColor)
            {
            }
        }

        [Exportable(false)]
        [DisableParentSettings("FillColor", "UsePartialFillColor", "UseChunks", "PartialFillColor", "LabelMode")]
        public class AstrologianAstrodyneBarConfig : ChunkedProgressBarConfig
        {

            [ColorEdit4("Sun" + "##Astrodyne")]
            [Order(201)]
            public PluginConfigColor SealSunColor = new(new Vector4(213f / 255f, 124f / 255f, 97f / 255f, 100f / 100f));

            [ColorEdit4("Lunar" + "##Astrodyne")]
            [Order(202)]
            public PluginConfigColor SealLunarColor = new(new Vector4(241f / 255f, 217f / 255f, 125f / 255f, 100f / 100f));

            [ColorEdit4("Celestial" + "##Astrodyne")]
            [Order(203)]
            public PluginConfigColor SealCelestialColor = new(new Vector4(100f / 255f, 207f / 255f, 211f / 255f, 100f / 100f));

            [NestedConfig("Glow" + "##Astrodyne", 205, separator = false, spacing = true)]
            public BarGlowConfig AstrodyneGlowConfig = new();

            public AstrologianAstrodyneBarConfig(Vector2 position, Vector2 size)
                : base(position, size, new PluginConfigColor(Vector4.Zero))
            {
            }
        }

        [Exportable(false)]
        [DisableParentSettings("FillColor")]
        public class AstrologianStarBarConfig : ProgressBarConfig
        {
            [ColorEdit4("Earthly" + "##Star")]
            [Order(402)]
            public PluginConfigColor StarEarthlyColor = new(new Vector4(37f / 255f, 181f / 255f, 177f / 255f, 100f / 100f));

            [ColorEdit4("Giant" + "##Star")]
            [Order(403)]
            public PluginConfigColor StarGiantColor = new(new Vector4(198f / 255f, 154f / 255f, 199f / 255f, 100f / 100f));

            [NestedConfig("Giant Dominance Glow" + "##Star", 404, separator = false, spacing = true)]
            public BarGlowConfig StarGlowConfig = new();
            public AstrologianStarBarConfig(Vector2 position, Vector2 size)
                : base(position, size, new PluginConfigColor(Vector4.Zero))
            {
            }
        }
    }
}
