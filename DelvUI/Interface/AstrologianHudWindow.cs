using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DelvUI.Interface
{
    public class AstrologianHudWindow : HudWindow
    {
        public override uint JobId => Jobs.AST;
        private readonly AstrologianHudConfig _config;
        private readonly SpellHelper _spellHelper = new();
        private Dictionary<string, uint> EmptyColor => PluginConfiguration.MiscColorMap["empty"];

        public AstrologianHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration,
            AstrologianHudConfig config) :
            base(pluginInterface, pluginConfiguration)
        {
            _config = config;
        }

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
                var format = _config.EnableDecimalRedrawBar ? "N1" : "N0";
                return _config.ShowRedrawTextBar ? redrawCastInfo.ToString(format) + " [" + redrawStacks + "]" : redrawCastInfo.ToString(format);
            }

            return _config.ShowRedrawTextBar ? redrawStacks.ToString("N0") : "";
        }

        private void DrawDivinationBar()
        {
            var chunkColors = new List<Dictionary<string, uint>>();

            unsafe
            {
                var gauge = PluginInterface.ClientState.JobGauges.Get<ASTGauge>();

                var field = typeof(ASTGauge).GetField(
                    "seals",
                    BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance
                );

                var textSealReady = "";
                var sealNumbers = 0;
                var result = field?.GetValue(gauge);
                var hdl = GCHandle.Alloc(result, GCHandleType.Pinned);
                var p = (byte*)hdl.AddrOfPinnedObject();
                for (var ix = 0; ix < 3; ++ix)
                {
                    var seal = *(p + ix);
                    var type = (SealType)seal;
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

                    if (gauge.ContainsSeal(SealType.SUN)) { sealNumbers++; }
                    if (gauge.ContainsSeal(SealType.MOON)) { sealNumbers++; }
                    if (gauge.ContainsSeal(SealType.CELESTIAL)) { sealNumbers++; }

                    textSealReady = sealNumbers.ToString();
                }

                hdl.Free();
                var xPos = CenterX - XOffset + _config.BaseOffset.X + _config.DivinationBarPosition.X;
                var yPos = CenterY + YOffset + _config.BaseOffset.Y + _config.DivinationBarPosition.Y;

                var bar = BarBuilder.Create(xPos, yPos, _config.DivinationBarSize.X, _config.DivinationBarSize.Y)
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
                    var chucksToGlow = new bool[3];

                    for (var i = 0; i < sealNumbers; i++)
                    {
                        chucksToGlow[i] = true;
                    }

                    bar.SetGlowChunks(chucksToGlow);
                    bar.SetGlowColor(_config.DivinationGlowColor.Map["base"]);
                }

                var drawList = ImGui.GetWindowDrawList();
                bar.Build().Draw(drawList, PluginConfiguration);
            }
        }

        private void DrawDraw()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<ASTGauge>();

            var xPos = CenterX - XOffset + _config.BaseOffset.X + _config.DrawBarPosition.X;
            var yPos = CenterY + YOffset + _config.BaseOffset.Y + _config.DrawBarPosition.Y;

            var cardJob = "";
            var cardColor = EmptyColor;
            var builder = BarBuilder.Create(xPos, yPos, _config.DrawBarSize.X, _config.DrawBarSize.Y);

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
            var drawList = ImGui.GetWindowDrawList();
            var drawCastInfo = _spellHelper.GetSpellCooldown(3590);
            var redrawCastInfo = _spellHelper.GetSpellCooldown(3593);
            var redrawStacks = _spellHelper.GetStackCount(3, 3593);

            if (cardJob != "")
            {
                cardPresent = 1f;
                cardMax = 1f;
            }
            else
            {
                cardPresent = drawCastInfo > 0 ? drawCastInfo : 1f;
                cardJob = drawCastInfo > 0
                    ? Math.Abs(drawCastInfo).ToString(_config.EnableDecimalDrawBar ? "N1" : "N0")
                    : "READY";
                cardColor = drawCastInfo > 0 ? _config.DrawCdColor.Map : _config.DrawCdReadyColor.Map;
                cardMax = drawCastInfo > 0 ? 30f : 1f;
            }

            var bar = builder.AddInnerBar(Math.Abs(cardPresent), cardMax, cardColor)
                .SetBackgroundColor(EmptyColor["background"])
                .SetTextMode(BarTextMode.Single)
                .SetText(BarTextPosition.CenterLeft, BarTextType.Custom,
                    _config.ShowDrawCooldownTextBar ? Math.Abs(cardPresent).ToString("G") : "");

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
                    bar.SetText(BarTextPosition.CenterLeft, BarTextType.Custom,
                        _config.ShowDrawCooldownTextBar ? cardJob : "");
                    break;
            }

            if (_config.ShowRedrawBar)
            {
                var redrawText = RedrawText(redrawCastInfo, redrawStacks);
                bar.AddPrimaryText(new BarText(BarTextPosition.CenterRight, BarTextType.Custom, redrawText));
            }

            bar.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawDot()
        {
            var target = PluginInterface.ClientState.Targets.SoftTarget ??
                         PluginInterface.ClientState.Targets.CurrentTarget;
            var xPos = CenterX - XOffset + _config.BaseOffset.X + _config.DotBarPosition.X;
            var yPos = CenterY + YOffset + _config.BaseOffset.Y + _config.DotBarPosition.Y;
            var drawList = ImGui.GetWindowDrawList();
            var builder = BarBuilder.Create(xPos, yPos, _config.DotBarSize.X, _config.DotBarSize.Y);

            if (target is not Chara)
            {
                var barNoTarget = builder.AddInnerBar(0, 30f, _config.DotColor.Map)
                    .SetBackgroundColor(EmptyColor["background"])
                    .SetTextMode(BarTextMode.Single)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, _config.ShowDotTextBar ? !_config.EnableDecimalDotBar ? "0" : "0.0" : "")
                    .Build();
                barNoTarget.Draw(drawList, PluginConfiguration);

                return;
            }


            var dot = target.StatusEffects.FirstOrDefault(o =>
                o.EffectId == 1881 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId ||
                o.EffectId == 843 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId ||
                o.EffectId == 838 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId);
            var dotCooldown = dot.EffectId == 838 ? 18f : 30f;
            var dotDuration = _config.EnableDecimalDotBar ? dot.Duration : Math.Abs(dot.Duration);

            var bar = builder.AddInnerBar(dotDuration, dotCooldown, _config.DotColor.Map)
                .SetBackgroundColor(EmptyColor["background"])
                .SetTextMode(BarTextMode.Single)
                .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom,
                    _config.ShowDotTextBar
                        ? !_config.EnableDecimalDotBar
                            ? dot.Duration.ToString("N0")
                            : Math.Abs(dot.Duration).ToString("N1")
                        : "")
                .Build();

            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawLightspeed()
        {
            var lightspeedBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 841).ToList();
            var lightspeedDuration = 0f;
            const float lightspeedMaxDuration = 15f;

            var xPos = CenterX - XOffset + _config.BaseOffset.X + _config.LightspeedBarPosition.X;
            var yPos = CenterY + YOffset + _config.BaseOffset.Y + _config.LightspeedBarPosition.Y;

            if (lightspeedBuff.Any())
            {
                lightspeedDuration = Math.Abs(lightspeedBuff.First().Duration);
            }

            var builder = BarBuilder.Create(xPos, yPos, _config.LightspeedBarSize.X, _config.LightspeedBarSize.Y);

            var bar = builder.AddInnerBar(lightspeedDuration, lightspeedMaxDuration, EmptyColor, _config.LightspeedColor.Map)
                .SetTextMode(BarTextMode.Single)
                .SetBackgroundColor(EmptyColor["background"])
                .SetFlipDrainDirection(true)
                .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom,
                    _config.ShowLightspeedTextBar
                        ? !_config.EnableDecimalLightspeedBar
                            ? lightspeedDuration.ToString("N0")
                            : Math.Abs(lightspeedDuration).ToString("N1")
                        : "")
                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawStar()
        {
            var starPreCookingBuff =
                PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1224).ToList();
            var starPostCookingBuff =
                PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1248).ToList();
            var starDuration = 0f;
            const float starMaxDuration = 10f;

            var xPos = CenterX - XOffset + _config.BaseOffset.X + _config.StarBarPosition.X;
            var yPos = CenterY + YOffset + _config.BaseOffset.Y + _config.StarBarPosition.Y;
            var starColorSelector = EmptyColor;

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

            var builder = BarBuilder.Create(xPos, yPos, _config.StarBarSize.X, _config.StarBarSize.Y);

            var bar = builder.AddInnerBar(starDuration, starMaxDuration, EmptyColor, starColorSelector)
                .SetTextMode(BarTextMode.Single)
                .SetBackgroundColor(EmptyColor["background"])
                .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom,
                    _config.ShowStarTextBar
                        ? !_config.EnableDecimalStarBar
                            ? starDuration.ToString("N0")
                            : Math.Abs(starDuration).ToString("N1")
                        : "");

            if (starColorSelector == _config.StarGiantColor.Map && _config.ShowStarGlowBar)
            {
                bar.SetGlowColor(_config.StarGlowColor.Map["base"]);
            }

            var drawList = ImGui.GetWindowDrawList();
            bar.Build().Draw(drawList, PluginConfiguration);
        }
    }

    [Serializable]
    public class AstrologianHudConfig : PluginConfigObject
    {
        public Vector2 BaseOffset = new(0, 0);
        public Vector2 DrawBarSize = new(20, 254);
        public Vector2 DrawBarPosition = new(33, -43);
        public Vector2 DivinationBarSize = new(10, 254);
        public Vector2 DivinationBarPosition = new(33, -77);
        public Vector2 DotBarSize = new(20, 84);
        public Vector2 DotBarPosition = new(118, -65);
        public Vector2 StarBarSize = new(20, 84);
        public Vector2 StarBarPosition = new(33, -65);
        public Vector2 LightspeedBarSize = new(20, 84);
        public Vector2 LightspeedBarPosition = new(203, -65);
        public int DivinationBarPad = 1;
        public bool ShowDivinationBar = true;
        public bool ShowDrawBar = true;
        public bool ShowDotBar = true;
        public bool ShowStarBar = true;
        public bool ShowLightspeedBar = true;
        public bool ShowRedrawBar = true;
        public bool ShowPrimaryResourceBar = true;
        public bool ShowStarGlowBar = true;
        public bool ShowDivinationGlowBar = true;
        public bool ShowDrawGlowBar;
        public bool EnableRedrawCooldownCumulated;
        public bool EnableDecimalDrawBar;
        public bool EnableDecimalRedrawBar;
        public bool EnableDecimalDotBar;
        public bool EnableDecimalStarBar;
        public bool EnableDecimalLightspeedBar;
        public bool ShowDivinationTextBar;
        public bool ShowDrawTextBar = true;
        public bool ShowDrawCooldownTextBar = true;
        public bool ShowRedrawTextBar = true;
        public bool ShowRedrawCooldownTextBar = true;
        public bool ShowDotTextBar = true;
        public bool ShowStarTextBar = true;
        public bool ShowLightspeedTextBar = true;

        public PluginConfigColor SealSunColor =
            new(new Vector4(213f / 255f, 124f / 255f, 97f / 255f, 100f / 100f));

        public PluginConfigColor SealLunarColor =
            new(new Vector4(241f / 255f, 217f / 255f, 125f / 255f, 100f / 100f));

        public PluginConfigColor SealCelestialColor =
            new(new Vector4(100f / 255f, 207f / 255f, 211f / 255f, 100f / 100f));

        public PluginConfigColor DotColor =
            new(new Vector4(20f / 255f, 80f / 255f, 168f / 255f, 100f / 100f));

        public PluginConfigColor StarEarthlyColor =
            new(new Vector4(37f / 255f, 181f / 255f, 177f / 255f, 100f / 100f));

        public PluginConfigColor StarGiantColor =
            new(new Vector4(198f / 255f, 154f / 255f, 199f / 255f, 100f / 100f));

        public PluginConfigColor LightspeedColor =
            new(new Vector4(255f / 255f, 255f / 255f, 173f / 255f, 100f / 100f));

        public PluginConfigColor StarGlowColor =
            new(new Vector4(255f / 255f, 199f / 255f, 62f / 255f, 100f / 100f));

        public PluginConfigColor DivinationGlowColor =
            new(new Vector4(255f / 255f, 199f / 255f, 62f / 255f, 100f / 100f));

        public PluginConfigColor DrawMeleeGlowColor =
            new(new Vector4(83f / 255f, 34f / 255f, 120f / 255f, 100f / 100f));

        public PluginConfigColor DrawRangedGlowColor =
            new(new Vector4(124f / 255f, 34f / 255f, 120f / 255f, 100f / 100f));

        public PluginConfigColor DrawCdColor =
            new(new Vector4(26f / 255f, 167f / 255f, 109f / 255f, 100f / 100f));

        public PluginConfigColor DrawCdReadyColor =
            new(new Vector4(137f / 255f, 26f / 255f, 42f / 255f, 100f / 100f));


        public new bool Draw()
        {
            var changed = false;

            if (ImGui.CollapsingHeader("Base Offset", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Move the entire job bars pack");
                }
                changed |= ImGui.DragFloat2("Base Offset Position", ref BaseOffset, 1f, -2000, 2000, "%.f");
            }

            if (ImGui.CollapsingHeader("Draw Bar"))
            {
                changed |= ImGui.Checkbox("Show Draw Bar", ref ShowDrawBar);
                changed |= ImGui.DragFloat2("Draw Bar Size", ref DrawBarSize, 1f, 1, 2000, "%.f");
                changed |= ImGui.DragFloat2("Draw Bar Position", ref DrawBarPosition, 1f, -2000, 2000, "%.f");
                ImGui.Text("Color");
                changed |= ColorEdit4("Draw on CD Color", ref DrawCdColor);
                changed |= ColorEdit4("Draw Ready Color", ref DrawCdReadyColor);
                changed |= ColorEdit4("Draw Melee Glow Color", ref DrawMeleeGlowColor);
                changed |= ColorEdit4("Draw Ranged Glow Color", ref DrawRangedGlowColor);
                ImGui.Text("Options");
                changed |= ImGui.Checkbox("Show card preferred target with glow", ref ShowDrawGlowBar);
                changed |= ImGui.Checkbox("Show card preferred target with text", ref ShowDrawTextBar);
                changed |= ImGui.Checkbox("Show Draw Timer", ref ShowDrawCooldownTextBar);
                changed |= ImGui.Checkbox("Enable Redraw Stacks & Cooldown", ref ShowRedrawBar);
                if (ShowRedrawBar)
                {
                    changed |= ImGui.Checkbox("Show Redraw Timer", ref ShowRedrawCooldownTextBar);
                    changed |= ImGui.Checkbox("Show Redraw Stacks", ref ShowRedrawTextBar);
                }
                changed |= ImGui.Checkbox("Change next Redraw cooldown to total Redraw cooldown", ref EnableRedrawCooldownCumulated);
                changed |= ImGui.Checkbox("Change Draw timer to decimal", ref EnableDecimalDrawBar);
                changed |= ImGui.Checkbox("Change Redraw timer to decimal", ref EnableDecimalRedrawBar);
            }

            if (ImGui.CollapsingHeader("Divination Bar"))
            {
                changed |= ImGui.Checkbox("Show Divination Bar", ref ShowDivinationBar);

                changed |= ImGui.DragFloat2("Divination Bar Size", ref DivinationBarSize, 1f, 1, 2000, "%.f");
                changed |= ImGui.DragFloat2("Divination Bar Position", ref DivinationBarPosition, 1f, -2000, 2000, "%.f");
                changed |= ImGui.DragInt("Divination Bar Padding Offset", ref DivinationBarPad, .1f, -1000, 1000);

                ImGui.Text("Color");
                changed |= ColorEdit4("Seal Sun Color", ref SealSunColor);
                changed |= ColorEdit4("Seal Lunar Color", ref SealLunarColor);
                changed |= ColorEdit4("Seal Celestial Color", ref SealCelestialColor);
                changed |= ColorEdit4("Divination Glow Color", ref DivinationGlowColor);

                ImGui.Text("Options");
                changed |= ImGui.Checkbox("Show numbers of different seals for Divination with glow", ref ShowDivinationGlowBar);
                changed |= ImGui.Checkbox("Show numbers of different seals for Divination with text", ref ShowDivinationTextBar);

            }

            if (ImGui.CollapsingHeader("Dot Bar"))
            {
                changed |= ImGui.Checkbox("Show Dot Bar", ref ShowDotBar);

                changed |= ImGui.DragFloat2("Dot Bar Size", ref DotBarSize, 1f, 1, 2000, "%.f");
                changed |= ImGui.DragFloat2("Dot Bar Position", ref DotBarPosition, 1f, -2000, 2000, "%.f");

                ImGui.Text("Color");
                changed |= ColorEdit4("Dot Color", ref DotColor);

                ImGui.Text("Options");
                changed |= ImGui.Checkbox("Show Dot timer", ref ShowDotTextBar);
                changed |= ImGui.Checkbox("Change Dot timer to decimal", ref EnableDecimalDotBar);

            }

            if (ImGui.CollapsingHeader("Star Bar"))
            {
                changed |= ImGui.Checkbox("Show Star Bar", ref ShowStarBar);
                changed |= ImGui.DragFloat2("Star Bar Size", ref StarBarSize, 1f, 1, 2000, "%.f");
                changed |= ImGui.DragFloat2("Star Bar Position", ref StarBarPosition, 1f, -2000, 2000, "%.f");


                ImGui.Text("Color");
                changed |= ColorEdit4("Star Earthly Color", ref StarEarthlyColor);
                changed |= ColorEdit4("Star Giant Color", ref StarGiantColor);
                changed |= ColorEdit4("Star Glow Color", ref StarGlowColor);

                ImGui.Text("Options");
                changed |= ImGui.Checkbox("Show Star timer", ref ShowStarTextBar);
                changed |= ImGui.Checkbox("Change Star timer to decimal", ref EnableDecimalStarBar);
                changed |= ImGui.Checkbox("Enable Star bar glow when Giant Dominance is ready", ref ShowStarGlowBar);

            }

            if (ImGui.CollapsingHeader("Lightspeed Bar"))
            {
                changed |= ImGui.Checkbox("Show Lightspeed Bar", ref ShowLightspeedBar);

                changed |= ImGui.DragFloat2("Lightspeed Bar Size", ref LightspeedBarSize, 1f, 1, 2000, "%.f");
                changed |= ImGui.DragFloat2("Lightspeed Bar Position", ref LightspeedBarPosition, 1f, -2000, 2000, "%.f");

                ImGui.Text("Color");
                changed |= ColorEdit4("Lightspeed Color", ref LightspeedColor);

                ImGui.Text("Options");
                changed |= ImGui.Checkbox("Show Lightspeed timer", ref ShowLightspeedTextBar);
                changed |= ImGui.Checkbox("Change Lightspeed timer to decimal", ref EnableDecimalLightspeedBar);

            }

            if (ImGui.CollapsingHeader("Others Options"))
            {
                changed |= ImGui.Checkbox("Show Primary Resource Bar", ref ShowPrimaryResourceBar);
                //changed |= ColorEdit4("Bar Empty Color", ref EmptyColor);
            }

            return changed;
        }
    }
}
