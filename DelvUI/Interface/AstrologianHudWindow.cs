using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Structs;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using ImGuiNET;
using Actor = Dalamud.Game.ClientState.Actors.Types.Actor;

namespace DelvUI.Interface
{
    public class AstrologianHudWindow : HudWindow
    {
        private readonly SpellHelper _spellHelper = new();

        public override uint JobId => Jobs.AST;
        private AstrologianHudConfig _config => (AstrologianHudConfig)ConfigurationManager.GetInstance().GetConfiguration(new AstrologianHudConfig());

        private float OriginX => CenterX + _config.Position.X;
        private float OriginY => CenterY + _config.Position.Y;

        private Dictionary<string, uint> EmptyColor => PluginConfiguration.MiscColorMap["empty"];

        public AstrologianHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _)
        {
            if (_config.ShowDivinationBar)
            {
                DrawDivinationBar();
            }

            if (_config.ShowDrawBar)
            {
                DrawDraw();
            }

            if (_config.ShowDotBar)
            {
                DrawDot();
            }

            if (_config.ShowLightspeedBar)
            {
                DrawLightspeed();
            }

            if (_config.ShowStarBar)
            {
                DrawStar();
            }
        }

        protected override void DrawPrimaryResourceBar()
        {
            if (!_config.ShowPrimaryResourceBar)
            {
                return;
            }

            base.DrawPrimaryResourceBar();
        }

        protected string RedrawText(float redrawCastInfo, int redrawStacks)
        {
            if (!_config.ShowRedrawBar)
            {
                return "";
            }

            if (redrawCastInfo <= 0)
            {
                return _config.ShowRedrawTextBar ? redrawStacks.ToString("N0") : "";
            }

            if (!_config.EnableRedrawCooldownCumulated)
            {
                if (redrawCastInfo % 30 == 0)
                {
                    return "30";
                }

                redrawCastInfo %= 30;
            }

            if (_config.ShowRedrawCooldownTextBar)
            {
                string format = _config.EnableDecimalRedrawBar ? "N1" : "N0";

                return _config.ShowRedrawTextBar ? redrawCastInfo.ToString(format) + " [" + redrawStacks + "]" : redrawCastInfo.ToString(format);
            }

            return _config.ShowRedrawTextBar ? redrawStacks.ToString("N0") : "";
        }

        private void DrawDivinationBar()
        {
            List<Dictionary<string, uint>> chunkColors = new();

            unsafe
            {
                ASTGauge gauge = PluginInterface.ClientState.JobGauges.Get<ASTGauge>();

                FieldInfo field = typeof(ASTGauge).GetField("seals", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);

                string textSealReady = "";
                int sealNumbers = 0;
                object result = field?.GetValue(gauge);
                GCHandle hdl = GCHandle.Alloc(result, GCHandleType.Pinned);
                byte* p = (byte*)hdl.AddrOfPinnedObject();

                for (int ix = 0; ix < 3; ++ix)
                {
                    byte seal = *(p + ix);
                    SealType type = (SealType)seal;

                    switch (type)
                    {
                        case SealType.NONE:
                            chunkColors.Add(EmptyColor);

                            break;

                        case SealType.MOON:
                            chunkColors.Add(_config.SealLunarColor.Map);

                            break;

                        case SealType.SUN:
                            chunkColors.Add(_config.SealSunColor.Map);

                            break;

                        case SealType.CELESTIAL:
                            chunkColors.Add(_config.SealCelestialColor.Map);

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

                hdl.Free();
                float xPos = OriginX + _config.DivinationBarPosition.X - _config.DivinationBarSize.X / 2f;
                float yPos = OriginY + _config.DivinationBarPosition.Y - _config.DivinationBarSize.Y / 2f;

                BarBuilder bar = BarBuilder.Create(xPos, yPos, _config.DivinationBarSize.Y, _config.DivinationBarSize.X)
                                           .SetBackgroundColor(EmptyColor["background"])
                                           .SetChunks(3)
                                           .SetChunkPadding(_config.DivinationBarPad)
                                           .AddInnerBar(chunkColors.Count(n => n != EmptyColor), 3, chunkColors.ToArray())
                                           .SetTextMode(BarTextMode.Single)
                                           .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, textSealReady);

                if (!_config.ShowDivinationTextBar)
                {
                    textSealReady = "";
                }

                bar.SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, textSealReady);

                if (_config.ShowDivinationGlowBar)
                {
                    bool[] chucksToGlow = new bool[3];

                    for (int i = 0; i < sealNumbers; i++)
                    {
                        chucksToGlow[i] = true;
                    }

                    bar.SetGlowChunks(chucksToGlow);
                    bar.SetGlowColor(_config.DivinationGlowColor.Map["base"]);
                }

                ImDrawListPtr drawList = ImGui.GetWindowDrawList();
                bar.Build().Draw(drawList, PluginConfiguration);
            }
        }

        private void DrawDraw()
        {
            ASTGauge gauge = PluginInterface.ClientState.JobGauges.Get<ASTGauge>();

            float xPos = OriginX + _config.DrawBarPosition.X - _config.DrawBarSize.X / 2f;
            float yPos = OriginY + _config.DrawBarPosition.Y - _config.DrawBarSize.Y / 2f;

            string cardJob = "";
            Dictionary<string, uint> cardColor = EmptyColor;
            BarBuilder builder = BarBuilder.Create(xPos, yPos, _config.DrawBarSize.Y, _config.DrawBarSize.X);

            switch (gauge.DrawnCard())
            {
                case CardType.BALANCE:
                    cardColor = _config.SealSunColor.Map;
                    cardJob = "MELEE";

                    break;

                case CardType.BOLE:
                    cardColor = _config.SealSunColor.Map;
                    cardJob = "RANGED";

                    break;

                case CardType.ARROW:
                    cardColor = _config.SealLunarColor.Map;
                    cardJob = "MELEE";

                    break;

                case CardType.EWER:
                    cardColor = _config.SealLunarColor.Map;
                    cardJob = "RANGED";

                    break;

                case CardType.SPEAR:
                    cardColor = _config.SealCelestialColor.Map;
                    cardJob = "MELEE";

                    break;

                case CardType.SPIRE:
                    cardColor = _config.SealCelestialColor.Map;
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

                cardJob = drawCastInfo > 0 ? Math.Abs(drawCastInfo).ToString(_config.EnableDecimalDrawBar ? "N1" : "N0") : "READY";

                cardColor = drawCastInfo > 0 ? _config.DrawCdColor.Map : _config.DrawCdReadyColor.Map;
                cardMax = drawCastInfo > 0 ? 30f : 1f;
            }

            BarBuilder bar = builder.AddInnerBar(Math.Abs(cardPresent), cardMax, cardColor)
                                    .SetBackgroundColor(EmptyColor["background"])
                                    .SetTextMode(BarTextMode.Single)
                                    .SetText(BarTextPosition.CenterLeft, BarTextType.Custom, _config.ShowDrawCooldownTextBar ? Math.Abs(cardPresent).ToString("G") : "");

            if (_config.ShowDrawGlowBar)
            {
                switch (cardJob)
                {
                    case "RANGED":
                        bar.SetGlowColor(_config.DrawRangedGlowColor.Map["base"]);

                        break;

                    case "MELEE":
                        bar.SetGlowColor(_config.DrawMeleeGlowColor.Map["base"]);

                        break;
                }
            }

            if (!_config.ShowDrawTextBar)
            {
                if (!(cardJob == "RANGED" || cardJob == "MELEE" || cardJob == "READY"))
                {
                    cardJob = "";
                }
            }

            switch (cardJob)
            {
                case "RANGED":
                    bar.SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, cardJob);

                    break;

                case "MELEE":
                    bar.SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, cardJob);

                    break;

                case "READY":
                    bar.SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, cardJob);

                    break;

                default:
                    bar.SetText(BarTextPosition.CenterLeft, BarTextType.Custom, _config.ShowDrawCooldownTextBar ? cardJob : "");

                    break;
            }

            if (_config.ShowRedrawBar)
            {
                string redrawText = RedrawText(redrawCastInfo, redrawStacks);
                bar.AddPrimaryText(new BarText(BarTextPosition.CenterRight, BarTextType.Custom, redrawText));
            }

            bar.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawDot()
        {
            Actor target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;
            float xPos = OriginX + _config.DotBarPosition.X - _config.DotBarSize.X / 2f;
            float yPos = OriginY + _config.DotBarPosition.Y - _config.DotBarSize.Y / 2f;
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            BarBuilder builder = BarBuilder.Create(xPos, yPos, _config.DotBarSize.Y, _config.DotBarSize.X);

            if (target is not Chara)
            {
                Bar barNoTarget = builder.AddInnerBar(0, 30f, _config.DotColor.Map)
                                         .SetBackgroundColor(EmptyColor["background"])
                                         .SetTextMode(BarTextMode.Single)
                                         .SetText(
                                             BarTextPosition.CenterMiddle,
                                             BarTextType.Custom,
                                             _config.ShowDotTextBar
                                                 ? !_config.EnableDecimalDotBar
                                                     ? "0"
                                                     : "0.0"
                                                 : ""
                                         )
                                         .Build();

                barNoTarget.Draw(drawList, PluginConfiguration);

                return;
            }

            StatusEffect dot = target.StatusEffects.FirstOrDefault(
                o => o.EffectId == 1881 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                  || o.EffectId == 843 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                  || o.EffectId == 838 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
            );

            float dotCooldown = dot.EffectId == 838 ? 18f : 30f;
            float dotDuration = _config.EnableDecimalDotBar ? dot.Duration : Math.Abs(dot.Duration);

            Bar bar = builder.AddInnerBar(dotDuration, dotCooldown, _config.DotColor.Map)
                             .SetBackgroundColor(EmptyColor["background"])
                             .SetTextMode(BarTextMode.Single)
                             .SetText(
                                 BarTextPosition.CenterMiddle,
                                 BarTextType.Custom,
                                 _config.ShowDotTextBar
                                     ? !_config.EnableDecimalDotBar
                                         ? dot.Duration.ToString("N0")
                                         : Math.Abs(dot.Duration).ToString("N1")
                                     : ""
                             )
                             .Build();

            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawLightspeed()
        {
            List<StatusEffect> lightspeedBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 841).ToList();
            float lightspeedDuration = 0f;
            const float lightspeedMaxDuration = 15f;

            float xPos = OriginX + _config.LightspeedBarPosition.X - _config.LightspeedBarSize.X / 2f;
            float yPos = OriginY + _config.LightspeedBarPosition.Y - _config.LightspeedBarSize.Y / 2f;

            if (lightspeedBuff.Any())
            {
                lightspeedDuration = Math.Abs(lightspeedBuff.First().Duration);
            }

            BarBuilder builder = BarBuilder.Create(xPos, yPos, _config.LightspeedBarSize.Y, _config.LightspeedBarSize.X);

            Bar bar = builder.AddInnerBar(lightspeedDuration, lightspeedMaxDuration, EmptyColor, _config.LightspeedColor.Map)
                             .SetTextMode(BarTextMode.Single)
                             .SetBackgroundColor(EmptyColor["background"])
                             .SetFlipDrainDirection(true)
                             .SetText(
                                 BarTextPosition.CenterMiddle,
                                 BarTextType.Custom,
                                 _config.ShowLightspeedTextBar
                                     ? !_config.EnableDecimalLightspeedBar
                                         ? lightspeedDuration.ToString("N0")
                                         : Math.Abs(lightspeedDuration).ToString("N1")
                                     : ""
                             )
                             .Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawStar()
        {
            List<StatusEffect> starPreCookingBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1224).ToList();

            List<StatusEffect> starPostCookingBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1248).ToList();

            float starDuration = 0f;
            const float starMaxDuration = 10f;

            float xPos = OriginX + _config.StarBarPosition.X - _config.StarBarSize.X / 2f;
            float yPos = OriginY + _config.StarBarPosition.Y - _config.StarBarSize.Y / 2f;
            Dictionary<string, uint> starColorSelector = EmptyColor;

            if (starPreCookingBuff.Any())
            {
                starDuration = starMaxDuration - Math.Abs(starPreCookingBuff.First().Duration);
                starColorSelector = _config.StarEarthlyColor.Map;
            }

            if (starPostCookingBuff.Any())
            {
                starDuration = Math.Abs(starPostCookingBuff.First().Duration);
                starColorSelector = _config.StarGiantColor.Map;
            }

            BarBuilder builder = BarBuilder.Create(xPos, yPos, _config.StarBarSize.Y, _config.StarBarSize.X);

            BarBuilder bar = builder.AddInnerBar(starDuration, starMaxDuration, EmptyColor, starColorSelector)
                                    .SetTextMode(BarTextMode.Single)
                                    .SetBackgroundColor(EmptyColor["background"])
                                    .SetText(
                                        BarTextPosition.CenterMiddle,
                                        BarTextType.Custom,
                                        _config.ShowStarTextBar
                                            ? !_config.EnableDecimalStarBar
                                                ? starDuration.ToString("N0")
                                                : Math.Abs(starDuration).ToString("N1")
                                            : ""
                                    );

            if (starColorSelector == _config.StarGiantColor.Map && _config.ShowStarGlowBar)
            {
                bar.SetGlowColor(_config.StarGlowColor.Map["base"]);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Build().Draw(drawList, PluginConfiguration);
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Healer", 0)]
    [SubSection("Astrologian", 1)]
    public class AstrologianHudConfig : PluginConfigObject
    {
        #region Base Position

        [DragFloat2("Base Position", min = -2000f, max = 2000f)]
        [Order(0)]
        public Vector2 Position = new(0, 0);

        #endregion

        #region Primary Resource Bar

        [Checkbox("Show Primary Resource Bar")]
        [Order(5)]
        public bool ShowPrimaryResourceBar = true;

        #endregion

        #region Draw Bar

        [Checkbox("Show Draw Bar")]
        [CollapseControl(10, 0)]
        public bool ShowDrawBar = true;

        [DragFloat2("Draw Bar Size", min = 1f, max = 2000f)]
        [CollapseWith(0, 0)]
        public Vector2 DrawBarSize = new(254, 20);

        [DragFloat2("Draw Bar Position", min = -2000f, max = 2000f)]
        [CollapseWith(5, 0)]
        public Vector2 DrawBarPosition = new(0, 427);

        [ColorEdit4("Draw on CD Color")]
        [CollapseWith(10, 0)]
        public PluginConfigColor DrawCdColor = new(new Vector4(26f / 255f, 167f / 255f, 109f / 255f, 100f / 100f));

        [ColorEdit4("Draw Ready Color")]
        [CollapseWith(15, 0)]
        public PluginConfigColor DrawCdReadyColor = new(new Vector4(137f / 255f, 26f / 255f, 42f / 255f, 100f / 100f));

        [ColorEdit4("Draw Melee Glow Color")]
        [CollapseWith(20, 0)]
        public PluginConfigColor DrawMeleeGlowColor = new(new Vector4(83f / 255f, 34f / 255f, 120f / 255f, 100f / 100f));

        [ColorEdit4("Draw Ranged Glow Color")]
        [CollapseWith(25, 0)]
        public PluginConfigColor DrawRangedGlowColor = new(new Vector4(124f / 255f, 34f / 255f, 120f / 255f, 100f / 100f));

        [Checkbox("Show card preferred target with glow")]
        [CollapseWith(30, 0)]
        public bool ShowDrawGlowBar;

        [Checkbox("Show card preferred target with text")]
        [CollapseWith(35, 0)]
        public bool ShowDrawTextBar = true;

        [Checkbox("Show Draw Timer")]
        [CollapseWith(40, 0)]
        public bool ShowDrawCooldownTextBar = true;

        [Checkbox("Enable Redraw Stacks & Cooldown")]
        [CollapseWith(45, 0)]
        public bool ShowRedrawBar = true;

        [Checkbox("Show Redraw Timer")]
        [CollapseWith(50, 0)]
        public bool ShowRedrawCooldownTextBar = true;

        [Checkbox("Show Redraw Stacks")]
        [CollapseWith(55, 0)]
        public bool ShowRedrawTextBar = true;

        [Checkbox("Change next Redraw cooldown to total Redraw cooldown")]
        [CollapseWith(60, 0)]
        public bool EnableRedrawCooldownCumulated;

        [Checkbox("Change Draw timer to decimal")]
        [CollapseWith(65, 0)]
        public bool EnableDecimalDrawBar;

        [Checkbox("Change Redraw timer to decimal")]
        [CollapseWith(70, 0)]
        public bool EnableDecimalRedrawBar;

        #endregion

        #region Divination Bar

        [Checkbox("Show Divination Bar")]
        [CollapseControl(15, 1)]
        public bool ShowDivinationBar = true;

        [DragFloat2("Divination Bar Size", min = 1f, max = 2000f)]
        [CollapseWith(0, 1)]
        public Vector2 DivinationBarSize = new(254, 10);

        [DragFloat2("Divination Bar Position", min = -2000f, max = 2000f)]
        [CollapseWith(5, 1)]
        public Vector2 DivinationBarPosition = new(0, 388);

        [DragInt("Divination Bar Padding", min = -1000, max = 1000)]
        [CollapseWith(10, 1)]
        public int DivinationBarPad = 2;

        [ColorEdit4("Seal Sun Color")]
        [CollapseWith(15, 1)]
        public PluginConfigColor SealSunColor = new(new Vector4(213f / 255f, 124f / 255f, 97f / 255f, 100f / 100f));

        [ColorEdit4("Seal Lunar Color")]
        [CollapseWith(20, 1)]
        public PluginConfigColor SealLunarColor = new(new Vector4(241f / 255f, 217f / 255f, 125f / 255f, 100f / 100f));

        [ColorEdit4("Seal Celestial Color")]
        [CollapseWith(25, 1)]
        public PluginConfigColor SealCelestialColor = new(new Vector4(100f / 255f, 207f / 255f, 211f / 255f, 100f / 100f));

        [ColorEdit4("Divination Glow Color")]
        [CollapseWith(30, 1)]
        public PluginConfigColor DivinationGlowColor = new(new Vector4(255f / 255f, 199f / 255f, 62f / 255f, 100f / 100f));

        [Checkbox("Show numbers of different seals for Divination with glow")]
        [CollapseWith(35, 1)]
        public bool ShowDivinationGlowBar = true;

        [Checkbox("Show numbers of different seals for Divination with text")]
        [CollapseWith(40, 1)]
        public bool ShowDivinationTextBar;

        #endregion

        #region Dot Bar

        [Checkbox("Show Dot Bar")]
        [CollapseControl(20, 2)]
        public bool ShowDotBar = true;

        [DragFloat2("Dot Bar Size", min = 1f, max = 2000f)]
        [CollapseWith(0, 2)]
        public Vector2 DotBarSize = new(84, 20);

        [DragFloat2("Dot Bar Position", min = -2000f, max = 2000f)]
        [CollapseWith(5, 2)]
        public Vector2 DotBarPosition = new(-85, 405);

        [ColorEdit4("Dot Color")]
        [CollapseWith(10, 2)]
        public PluginConfigColor DotColor = new(new Vector4(20f / 255f, 80f / 255f, 168f / 255f, 100f / 100f));

        [Checkbox("Show Dot timer")]
        [CollapseWith(15, 2)]
        public bool ShowDotTextBar = true;

        [Checkbox("Change Dot timer to decimal")]
        [CollapseWith(20, 2)]
        public bool EnableDecimalDotBar;

        #endregion

        #region Star Bar

        [Checkbox("Show Star Bar")]
        [CollapseControl(25, 3)]
        public bool ShowStarBar = true;

        [DragFloat2("Star Bar Size", min = 1f, max = 2000f)]
        [CollapseWith(0, 3)]
        public Vector2 StarBarSize = new(84, 20);

        [DragFloat2("Star Bar Position", min = -2000f, max = 2000f)]
        [CollapseWith(5, 3)]
        public Vector2 StarBarPosition = new(0, 405);

        [ColorEdit4("Star Earthly Color")]
        [CollapseWith(10, 3)]
        public PluginConfigColor StarEarthlyColor = new(new Vector4(37f / 255f, 181f / 255f, 177f / 255f, 100f / 100f));

        [ColorEdit4("Star Giant Color")]
        [CollapseWith(15, 3)]
        public PluginConfigColor StarGiantColor = new(new Vector4(198f / 255f, 154f / 255f, 199f / 255f, 100f / 100f));

        [ColorEdit4("Star Glow Color")]
        [CollapseWith(20, 3)]
        public PluginConfigColor StarGlowColor = new(new Vector4(255f / 255f, 199f / 255f, 62f / 255f, 100f / 100f));

        [Checkbox("Show Star timer")]
        [CollapseWith(25, 3)]
        public bool ShowStarTextBar = true;

        [Checkbox("Change Star timer to decimal")]
        [CollapseWith(30, 3)]
        public bool EnableDecimalStarBar;

        [Checkbox("Enable Star bar glow when Giant Dominance is ready")]
        [CollapseWith(35, 3)]
        public bool ShowStarGlowBar = true;

        #endregion

        #region Lightspeed Bar

        [Checkbox("Show Lightspeed Bar")]
        [CollapseControl(30, 4)]
        public bool ShowLightspeedBar = true;

        [DragFloat2("Lightspeed Bar Size", min = 1f, max = 2000f)]
        [CollapseWith(0, 4)]
        public Vector2 LightspeedBarSize = new(84, 20);

        [DragFloat2("Lightspeed Bar Position", min = -2000f, max = 2000f)]
        [CollapseWith(5, 4)]
        public Vector2 LightspeedBarPosition = new(85, 405);

        [ColorEdit4("Lightspeed Color")]
        [CollapseWith(10, 4)]
        public PluginConfigColor LightspeedColor = new(new Vector4(255f / 255f, 255f / 255f, 173f / 255f, 100f / 100f));

        [Checkbox("Show Lightspeed timer")]
        [CollapseWith(15, 4)]
        public bool ShowLightspeedTextBar = true;

        [Checkbox("Change Lightspeed timer to decimal")]
        [CollapseWith(20, 4)]
        public bool EnableDecimalLightspeedBar;

        #endregion
    }
}
