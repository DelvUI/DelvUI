using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.Jobs
{
    public class AstrologianHud : JobHud
    {
        private readonly SpellHelper _spellHelper = new();
        private new AstrologianConfig Config => (AstrologianConfig)_config;
        private PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;

        public AstrologianHud(string id, AstrologianConfig config, string? displayName = null) : base(id, config, displayName)
        {
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new();
            List<Vector2> sizes = new();

            if (Config.ShowDrawBar)
            {
                positions.Add(Config.Position + Config.DrawBarPosition);
                sizes.Add(Config.DrawBarSize);
            }

            if (Config.ShowDivinationBar)
            {
                positions.Add(Config.Position + Config.DivinationBarPosition);
                sizes.Add(Config.DivinationBarSize);
            }

            if (Config.ShowDotBar)
            {
                positions.Add(Config.Position + Config.DotBarPosition);
                sizes.Add(Config.DotBarSize);
            }

            if (Config.ShowStarBar)
            {
                positions.Add(Config.Position + Config.StarBarPosition);
                sizes.Add(Config.StarBarSize);
            }

            if (Config.ShowLightspeedBar)
            {
                positions.Add(Config.Position + Config.LightspeedBarPosition);
                sizes.Add(Config.LightspeedBarSize);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            if (Config.ShowDivinationBar)
            {
                DrawDivinationBar(origin);
            }

            if (Config.ShowDrawBar)
            {
                DrawDraw(origin);
            }

            if (Config.ShowDotBar)
            {
                DrawDot(origin, player);
            }

            if (Config.ShowLightspeedBar)
            {
                DrawLightspeed(origin, player);
            }

            if (Config.ShowStarBar)
            {
                DrawStar(origin, player);
            }
        }

        private string RedrawText(float redrawCastInfo, int redrawStacks)
        {
            if (redrawCastInfo <= 0)
            {
                return Config.ShowRedrawTextBar ? redrawStacks.ToString("N0") : "";
            }

            if (!Config.EnableRedrawCooldownCumulated)
            {
                if (redrawCastInfo % 30 == 0)
                {
                    return "30";
                }

                redrawCastInfo %= 30;
            }

            if (Config.ShowRedrawCooldownTextBar)
            {
                string format = Config.EnableDecimalRedrawBar ? "N1" : "N0";

                return Config.ShowRedrawTextBar ? redrawCastInfo.ToString(format) + " [" + redrawStacks + "]" : redrawCastInfo.ToString(format);
            }

            return Config.ShowRedrawTextBar ? redrawStacks.ToString("N0") : "";
        }

        private unsafe void DrawDivinationBar(Vector2 origin)
        {
            List<PluginConfigColor> chunkColors = new();
            ASTGauge gauge = Plugin.JobGauges.Get<ASTGauge>();
            IntPtr gaugeAddress = gauge.Address;
            byte[] sealsFromBytes = new byte[3];

            AstrologianGauge* tmp = (AstrologianGauge*)gaugeAddress;
            for (int ix = 0; ix < 3; ++ix)
            {
                sealsFromBytes[ix] = tmp->Seals[ix];
            }

            string textSealReady = "";
            int sealNumbers = 0;

            for (int ix = 0; ix < 3; ++ix)
            {
                byte seal = sealsFromBytes[ix];
                SealType type = (SealType)seal;

                switch (type)
                {
                    case SealType.NONE:
                        chunkColors.Add(EmptyColor);

                        break;

                    case SealType.MOON:
                        chunkColors.Add(Config.SealLunarColor);

                        break;

                    case SealType.SUN:
                        chunkColors.Add(Config.SealSunColor);

                        break;

                    case SealType.CELESTIAL:
                        chunkColors.Add(Config.SealCelestialColor);

                        break;
                }

                if (gauge.ContainsSeal(SealType.NONE))
                {
                    continue;
                }

                sealNumbers = 0;

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

                textSealReady = sealNumbers.ToString();
            }

            float xPos = origin.X + Config.Position.X + Config.DivinationBarPosition.X - Config.DivinationBarSize.X / 2f;
            float yPos = origin.Y + Config.Position.Y + Config.DivinationBarPosition.Y - Config.DivinationBarSize.Y / 2f;

            BarBuilder bar = BarBuilder.Create(xPos, yPos, Config.DivinationBarSize.Y, Config.DivinationBarSize.X)
                                       .SetBackgroundColor(EmptyColor.Base)
                                       .SetChunks(3)
                                       .SetChunkPadding(Config.DivinationBarPad)
                                       .AddInnerBar(chunkColors.Count(n => n != EmptyColor), 3, chunkColors.ToArray())
                                       .SetTextMode(BarTextMode.Single)
                                       .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, textSealReady);

            if (!Config.ShowDivinationTextBar)
            {
                textSealReady = "";
            }

            bar.SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, textSealReady);

            if (Config.ShowDivinationGlowBar)
            {
                bool[] chucksToGlow = new bool[3];

                for (int i = 0; i < sealNumbers; i++)
                {
                    chucksToGlow[i] = true;
                }

                bar.SetGlowChunks(chucksToGlow);
                bar.SetGlowColor(Config.DivinationGlowColor.Base);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Build().Draw(drawList);
        }

        private void DrawDraw(Vector2 origin)
        {
            ASTGauge gauge = Plugin.JobGauges.Get<ASTGauge>();

            float xPos = origin.X + Config.Position.X + Config.DrawBarPosition.X - Config.DrawBarSize.X / 2f;
            float yPos = origin.Y + Config.Position.Y + Config.DrawBarPosition.Y - Config.DrawBarSize.Y / 2f;

            string cardJob = "";
            PluginConfigColor cardColor = EmptyColor;
            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.DrawBarSize.Y, Config.DrawBarSize.X);

            switch (gauge.DrawnCard)
            {
                case CardType.BALANCE:
                    cardColor = Config.SealSunColor;
                    cardJob = "MELEE";

                    break;

                case CardType.BOLE:
                    cardColor = Config.SealSunColor;
                    cardJob = "RANGED";

                    break;

                case CardType.ARROW:
                    cardColor = Config.SealLunarColor;
                    cardJob = "MELEE";

                    break;

                case CardType.EWER:
                    cardColor = Config.SealLunarColor;
                    cardJob = "RANGED";

                    break;

                case CardType.SPEAR:
                    cardColor = Config.SealCelestialColor;
                    cardJob = "MELEE";

                    break;

                case CardType.SPIRE:
                    cardColor = Config.SealCelestialColor;
                    cardJob = "RANGED";

                    break;
            }

            float cardPresent;
            float cardMax;
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            float drawCastInfo = _spellHelper.GetSpellCooldown(3590);
            float redrawCastInfo = _spellHelper.GetSpellCooldown(3593);
            int redrawStacks = _spellHelper.GetStackCount(3, 3593);

            if (cardJob != "")
            {
                cardPresent = 1f;
                cardMax = 1f;
            }
            else
            {
                cardPresent = drawCastInfo > 0 ? drawCastInfo : 1f;

                cardJob = drawCastInfo > 0 ? Math.Abs(drawCastInfo).ToString(Config.EnableDecimalDrawBar ? "N1" : "N0") : "READY";

                cardColor = drawCastInfo > 0 ? Config.DrawCdColor : Config.DrawCdReadyColor;
                cardMax = drawCastInfo > 0 ? 30f : 1f;
            }

            BarBuilder bar = builder.AddInnerBar(Math.Abs(cardPresent), cardMax, cardColor)
                                    .SetBackgroundColor(EmptyColor.Base)
                                    .SetTextMode(BarTextMode.Single)
                                    .SetText(BarTextPosition.CenterLeft, BarTextType.Custom, Config.ShowDrawCooldownTextBar ? Math.Abs(cardPresent).ToString("G") : "");

            if (Config.ShowDrawGlowBar)
            {
                switch (cardJob)
                {
                    case "RANGED":
                        bar.SetGlowColor(Config.DrawRangedGlowColor.Base);

                        break;

                    case "MELEE":
                        bar.SetGlowColor(Config.DrawMeleeGlowColor.Base);

                        break;
                }
            }

            if (!Config.ShowDrawCooldownTextBar && cardJob is not ("RANGED" or "MELEE" or "READY"))
            {
                cardJob = "";
            }

            switch (cardJob)
            {
                case "RANGED":
                    bar.SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, Config.ShowDrawTextBar ? cardJob : "");
                    if (Config.ShowDrawCardWhileDrawn)
                    {
                        bar.AddPrimaryText(new BarText(BarTextPosition.CenterLeft, BarTextType.Custom, Config.ShowDrawCooldownTextBar ? Math.Abs(drawCastInfo).ToString(Config.EnableDecimalDrawBar ? "N1" : "N0") : ""));
                    }
                    break;

                case "MELEE":
                    bar.SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, Config.ShowDrawTextBar ? cardJob : "");
                    if (Config.ShowDrawCardWhileDrawn)
                    {
                        bar.AddPrimaryText(new BarText(BarTextPosition.CenterLeft, BarTextType.Custom, Config.ShowDrawCooldownTextBar ? Math.Abs(drawCastInfo).ToString(Config.EnableDecimalDrawBar ? "N1" : "N0") : ""));
                    }
                    break;

                case "READY":
                    bar.SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, Config.ShowDrawTextBar ? cardJob : "");
                    if (Config.ShowDrawCardWhileDrawn)
                    {
                        bar.AddPrimaryText(new BarText(BarTextPosition.CenterLeft, BarTextType.Custom, Config.ShowDrawCooldownTextBar ? Math.Abs(drawCastInfo).ToString(Config.EnableDecimalDrawBar ? "N1" : "N0") : ""));
                    }
                    break;

                default:
                    bar.SetText(BarTextPosition.CenterLeft, BarTextType.Custom, Config.ShowDrawCooldownTextBar ? cardJob : "");
                    break;
            }

            string redrawText = RedrawText(redrawCastInfo, redrawStacks);
            bar.AddPrimaryText(new BarText(BarTextPosition.CenterRight, BarTextType.Custom, redrawText));

            bar.Build().Draw(drawList);
        }

        private void DrawDot(Vector2 origin, PlayerCharacter player)
        {
            GameObject? actor = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            float xPos = origin.X + Config.Position.X + Config.DotBarPosition.X - Config.DotBarSize.X / 2f;
            float yPos = origin.Y + Config.Position.Y + Config.DotBarPosition.Y - Config.DotBarSize.Y / 2f;
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.DotBarSize.Y, Config.DotBarSize.X);

            if (actor is not BattleChara target)
            {
                Bar barNoTarget = builder.AddInnerBar(0, 30f, Config.DotColor)
                                         .SetBackgroundColor(EmptyColor.Base)
                                         .SetTextMode(BarTextMode.Single)
                                         .SetText(
                                             BarTextPosition.CenterMiddle,
                                             BarTextType.Custom,
                                             Config.ShowDotTextBar
                                                 ? Config.EnableDecimalDotBar ? "0.0" : "0"
                                                 : ""
                                         )
                                         .Build();

                barNoTarget.Draw(drawList);

                return;
            }

            Status? dot = target.StatusList.FirstOrDefault(
                o => o.StatusId == 1881 && o.SourceID == player.ObjectId
                  || o.StatusId == 843 && o.SourceID == player.ObjectId
                  || o.StatusId == 838 && o.SourceID == player.ObjectId
            );

            float dotCooldown = dot?.StatusId == 838 ? 18f : 30f;
            float dotDuration = Math.Abs(dot?.RemainingTime ?? 0f);
            string dotDurationText = "";

            if (Config.ShowDotTextBar)
            {
                dotDurationText = dotDuration.ToString(Config.EnableDecimalDotBar ? "N1" : "N0");
            }

            Bar bar = builder.AddInnerBar(dotDuration, dotCooldown, Config.DotColor)
                             .SetBackgroundColor(EmptyColor.Base)
                             .SetTextMode(BarTextMode.Single)
                             .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, dotDurationText)
                             .Build();

            bar.Draw(drawList);
        }

        private void DrawLightspeed(Vector2 origin, PlayerCharacter player)
        {
            List<Status> lightspeedBuff = player.StatusList.Where(o => o.StatusId == 841).ToList();
            float lightspeedDuration = 0f;
            const float lightspeedMaxDuration = 15f;

            float xPos = origin.X + Config.Position.X + Config.LightspeedBarPosition.X - Config.LightspeedBarSize.X / 2f;
            float yPos = origin.Y + Config.Position.Y + Config.LightspeedBarPosition.Y - Config.LightspeedBarSize.Y / 2f;

            if (lightspeedBuff.Any())
            {
                lightspeedDuration = Math.Abs(lightspeedBuff.First().RemainingTime);
            }

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.LightspeedBarSize.Y, Config.LightspeedBarSize.X);

            Bar bar = builder.AddInnerBar(lightspeedDuration, lightspeedMaxDuration, EmptyColor, Config.LightspeedColor)
                             .SetTextMode(BarTextMode.Single)
                             .SetBackgroundColor(EmptyColor.Base)
                             .SetFlipDrainDirection(true)
                             .SetText(
                                 BarTextPosition.CenterMiddle,
                                 BarTextType.Custom,
                                 Config.ShowLightspeedTextBar
                                     ? Config.EnableDecimalLightspeedBar ? Math.Abs(lightspeedDuration).ToString("N1") : lightspeedDuration.ToString("N0")
                                     : ""
                             )
                             .Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        private void DrawStar(Vector2 origin, PlayerCharacter player)
        {
            List<Status> starPreCookingBuff = player.StatusList.Where(o => o.StatusId == 1224).ToList();
            List<Status> starPostCookingBuff = player.StatusList.Where(o => o.StatusId == 1248).ToList();

            float starDuration = 0f;
            const float starMaxDuration = 10f;

            float xPos = origin.X + Config.Position.X + Config.StarBarPosition.X - Config.StarBarSize.X / 2f;
            float yPos = origin.Y + Config.Position.Y + Config.StarBarPosition.Y - Config.StarBarSize.Y / 2f;
            PluginConfigColor starColorSelector = EmptyColor;

            if (starPreCookingBuff.Any())
            {
                starDuration = starMaxDuration - Math.Abs(starPreCookingBuff.First().RemainingTime);
                starColorSelector = Config.StarEarthlyColor;
            }

            if (starPostCookingBuff.Any())
            {
                starDuration = Math.Abs(starPostCookingBuff.First().RemainingTime);
                starColorSelector = Config.StarGiantColor;
            }

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.StarBarSize.Y, Config.StarBarSize.X);

            BarBuilder bar = builder.AddInnerBar(starDuration, starMaxDuration, EmptyColor, starColorSelector)
                                    .SetTextMode(BarTextMode.Single)
                                    .SetBackgroundColor(EmptyColor.Base)
                                    .SetText(
                                        BarTextPosition.CenterMiddle,
                                        BarTextType.Custom,
                                        Config.ShowStarTextBar
                                            ? Config.EnableDecimalStarBar ? Math.Abs(starDuration).ToString("N1") : starDuration.ToString("N0")
                                            : ""
                                    );

            if (starColorSelector == Config.StarGiantColor && Config.ShowStarGlowBar)
            {
                bar.SetGlowColor(Config.StarGlowColor.Base);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Build().Draw(drawList);
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
            AstrologianConfig? config = new() { UseDefaultPrimaryResourceBar = true };

            return config;
        }

        #region Draw Bar
        [Checkbox("Draw" + "##Draw", separator = true)]
        [Order(30)]
        public bool ShowDrawBar = true;

        [DragFloat2("Position" + "##Draw", min = -2000f, max = 2000f)]
        [Order(35, collapseWith = nameof(ShowDrawBar))]
        public Vector2 DrawBarPosition = new(0, -32);

        [DragFloat2("Size" + "##Draw", min = 1f, max = 2000f)]
        [Order(40, collapseWith = nameof(ShowDrawBar))]
        public Vector2 DrawBarSize = new(254, 20);

        [ColorEdit4("Draw on CD" + "##Draw")]
        [Order(45, collapseWith = nameof(ShowDrawBar))]
        public PluginConfigColor DrawCdColor = new(new Vector4(26f / 255f, 167f / 255f, 109f / 255f, 100f / 100f));

        [ColorEdit4("Draw Ready" + "##Draw")]
        [Order(50, collapseWith = nameof(ShowDrawBar))]
        public PluginConfigColor DrawCdReadyColor = new(new Vector4(137f / 255f, 26f / 255f, 42f / 255f, 100f / 100f));

        [ColorEdit4("Melee Glow" + "##Draw")]
        [Order(55, collapseWith = nameof(ShowDrawBar))]
        public PluginConfigColor DrawMeleeGlowColor = new(new Vector4(83f / 255f, 34f / 255f, 120f / 255f, 100f / 100f));

        [ColorEdit4("Ranged Glow" + "##Draw")]
        [Order(60, collapseWith = nameof(ShowDrawBar))]
        public PluginConfigColor DrawRangedGlowColor = new(new Vector4(124f / 255f, 34f / 255f, 120f / 255f, 100f / 100f));

        [Checkbox("Card Preferred Target with Glow" + "##Draw", spacing = true)]
        [Order(65, collapseWith = nameof(ShowDrawBar))]
        public bool ShowDrawGlowBar;

        [Checkbox("Card Preferred Target with Text" + "##Draw")]
        [Order(70, collapseWith = nameof(ShowDrawBar))]
        public bool ShowDrawTextBar = true;

        [Checkbox("Draw Timer" + "##Draw", spacing = true)]
        [Order(75, collapseWith = nameof(ShowDrawBar))]
        public bool ShowDrawCooldownTextBar = true;

        [Checkbox("with Decimals" + "##Draw")]
        [Order(80, collapseWith = nameof(ShowDrawBar))]
        public bool EnableDecimalDrawBar;

        [Checkbox("Card Drawn Timer" + "##Draw")]
        [Order(85, collapseWith = nameof(ShowDrawBar))]
        public bool ShowDrawCardWhileDrawn;

        [Checkbox("Redraw Timer" + "##Redraw", spacing = true)]
        [Order(90, collapseWith = nameof(ShowDrawBar))]
        public bool ShowRedrawCooldownTextBar = true;

        [Checkbox("with Decimals" + "##Redraw")]
        [Order(95, collapseWith = nameof(ShowDrawBar))]
        public bool EnableDecimalRedrawBar;

        [Checkbox("Redraw Stacks" + "##Redraw")]
        [Order(100, collapseWith = nameof(ShowDrawBar))]
        public bool ShowRedrawTextBar = true;

        [Checkbox("Total Redraw Cooldown Instead of Next" + "##Redraw")]
        [Order(105, collapseWith = nameof(ShowDrawBar))]
        public bool EnableRedrawCooldownCumulated;



        #endregion

        #region Divination Bar
        [Checkbox("Divination" + "##Divination", separator = true)]
        [Order(110)]
        public bool ShowDivinationBar = true;

        [DragFloat2("Position" + "##Divination", min = -2000f, max = 2000f)]
        [Order(115, collapseWith = nameof(ShowDivinationBar))]
        public Vector2 DivinationBarPosition = new(0, -71);

        [DragFloat2("Size" + "##Divination", min = 1f, max = 2000f)]
        [Order(120, collapseWith = nameof(ShowDivinationBar))]
        public Vector2 DivinationBarSize = new(254, 10);

        [DragInt("Spacing" + "##Divination", min = -1000, max = 1000)]
        [Order(125, collapseWith = nameof(ShowDivinationBar))]
        public int DivinationBarPad = 2;

        [ColorEdit4("Sun" + "##Divination")]
        [Order(130, collapseWith = nameof(ShowDivinationBar))]
        public PluginConfigColor SealSunColor = new(new Vector4(213f / 255f, 124f / 255f, 97f / 255f, 100f / 100f));

        [ColorEdit4("Lunar" + "##Divination")]
        [Order(135, collapseWith = nameof(ShowDivinationBar))]
        public PluginConfigColor SealLunarColor = new(new Vector4(241f / 255f, 217f / 255f, 125f / 255f, 100f / 100f));

        [ColorEdit4("Celestial" + "##Divination")]
        [Order(140, collapseWith = nameof(ShowDivinationBar))]
        public PluginConfigColor SealCelestialColor = new(new Vector4(100f / 255f, 207f / 255f, 211f / 255f, 100f / 100f));

        [Checkbox("Seal Count Text" + "##Divination", spacing = true)]
        [Order(145, collapseWith = nameof(ShowDivinationBar))]
        public bool ShowDivinationTextBar;

        [Checkbox("Seal Count Glow" + "##Divination")]
        [Order(150, collapseWith = nameof(ShowDivinationBar))]
        public bool ShowDivinationGlowBar = true;

        [ColorEdit4("Glow" + "##Divination")]
        [Order(155, collapseWith = nameof(ShowDivinationBar))]
        public PluginConfigColor DivinationGlowColor = new(new Vector4(255f / 255f, 199f / 255f, 62f / 255f, 100f / 100f));

        #endregion

        #region Dot Bar
        [Checkbox("Combust" + "##Combust", separator = true)]
        [Order(160)]
        public bool ShowDotBar = true;

        [DragFloat2("Size" + "##Combust", min = 1f, max = 2000f)]
        [Order(165, collapseWith = nameof(ShowDotBar))]
        public Vector2 DotBarSize = new(84, 20);

        [DragFloat2("Position" + "##Combust", min = -2000f, max = 2000f)]
        [Order(170, collapseWith = nameof(ShowDotBar))]
        public Vector2 DotBarPosition = new(-85, -54);

        [ColorEdit4("Color" + "##Combust")]
        [Order(175, collapseWith = nameof(ShowDotBar))]
        public PluginConfigColor DotColor = new(new Vector4(20f / 255f, 80f / 255f, 168f / 255f, 100f / 100f));

        [Checkbox("Timer" + "##Combust", spacing = true)]
        [Order(180, collapseWith = nameof(ShowDotBar))]
        public bool ShowDotTextBar = true;

        [Checkbox("with Decimals" + "##Combust")]
        [Order(185, collapseWith = nameof(ShowDotBar))]
        public bool EnableDecimalDotBar;
        #endregion

        #region Star Bar
        [Checkbox("Star" + "##Star", separator = true)]
        [Order(190)]
        public bool ShowStarBar = true;

        [DragFloat2("Position" + "##Star", min = -2000f, max = 2000f)]
        [Order(195, collapseWith = nameof(ShowStarBar))]
        public Vector2 StarBarPosition = new(0, -54);

        [DragFloat2("Size" + "##Star", min = 1f, max = 2000f)]
        [Order(200, collapseWith = nameof(ShowStarBar))]
        public Vector2 StarBarSize = new(84, 20);

        [ColorEdit4("Earthly" + "##Star")]
        [Order(205, collapseWith = nameof(ShowStarBar))]
        public PluginConfigColor StarEarthlyColor = new(new Vector4(37f / 255f, 181f / 255f, 177f / 255f, 100f / 100f));

        [ColorEdit4("Giant" + "##Star")]
        [Order(210, collapseWith = nameof(ShowStarBar))]
        public PluginConfigColor StarGiantColor = new(new Vector4(198f / 255f, 154f / 255f, 199f / 255f, 100f / 100f));

        [Checkbox("Timer" + "##Star", spacing = true)]
        [Order(215, collapseWith = nameof(ShowStarBar))]
        public bool ShowStarTextBar = true;

        [Checkbox("with Decimals" + "##Star")]
        [Order(220, collapseWith = nameof(ShowStarBar))]
        public bool EnableDecimalStarBar;

        [Checkbox("Giant Dominance Glow" + "##Star", spacing = true)]
        [Order(225, collapseWith = nameof(ShowStarBar))]
        public bool ShowStarGlowBar = true;

        [ColorEdit4("Color" + "##Star")]
        [Order(230, collapseWith = nameof(ShowStarBar))]
        public PluginConfigColor StarGlowColor = new(new Vector4(255f / 255f, 199f / 255f, 62f / 255f, 100f / 100f));

        #endregion

        #region Lightspeed Bar
        [Checkbox("Lightspeed" + "##Lightspeed", separator = true)]
        [Order(235)]
        public bool ShowLightspeedBar = true;

        [DragFloat2("Position" + "##Lightspeed", min = -2000f, max = 2000f)]
        [Order(240, collapseWith = nameof(ShowLightspeedBar))]
        public Vector2 LightspeedBarPosition = new(85, -54);

        [DragFloat2("Size" + "##Lightspeed", min = 1f, max = 2000f)]
        [Order(245, collapseWith = nameof(ShowLightspeedBar))]
        public Vector2 LightspeedBarSize = new(84, 20);

        [ColorEdit4("Color" + "##Lightspeed")]
        [Order(250, collapseWith = nameof(ShowLightspeedBar))]
        public PluginConfigColor LightspeedColor = new(new Vector4(255f / 255f, 255f / 255f, 173f / 255f, 100f / 100f));

        [Checkbox("Timer" + "##Lightspeed", spacing = true)]
        [Order(255, collapseWith = nameof(ShowLightspeedBar))]
        public bool ShowLightspeedTextBar = true;

        [Checkbox("with Decimals" + "##Lightspeed")]
        [Order(260, collapseWith = nameof(ShowLightspeedBar))]
        public bool EnableDecimalLightspeedBar;
        #endregion
    }
}
