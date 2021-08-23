using System.Diagnostics;
using System.Linq;
using Dalamud.Plugin;
using DelvUI.Interface.Bars;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using DelvUI.Helpers;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui;
using Dalamud.Interface;
using DelvUI.Config;

namespace DelvUI.Interface
{
    class AstrologianHudWindow : HudWindow
    {
        public override uint JobId => 33;

        private int BaseXOffset => PluginConfiguration.ASTBaseXOffset;
        private int BaseYOffset => PluginConfiguration.ASTBaseYOffset;

        private int DivinationHeight => PluginConfiguration.ASTDivinationHeight;
        private int DivinationWidth => PluginConfiguration.ASTDivinationWidth;
        private int DivinationBarX => PluginConfiguration.ASTDivinationBarX;
        private int DivinationBarY => PluginConfiguration.ASTDivinationBarY;
        private int DivinationBarPad => PluginConfiguration.ASTDivinationBarPad;

        private int DrawHeight => PluginConfiguration.ASTDrawBarHeight;
        private int DrawWidth => PluginConfiguration.ASTDrawBarWidth;
        private int DrawBarX => PluginConfiguration.ASTDrawBarX;
        private int DrawBarY => PluginConfiguration.ASTDrawBarY;

        private int DotHeight => PluginConfiguration.ASTDotBarHeight;
        private int DotWidth => PluginConfiguration.ASTDotBarWidth;
        private int DotBarX => PluginConfiguration.ASTDotBarX;
        private int DotBarY => PluginConfiguration.ASTDotBarY;

        private int StarHeight => PluginConfiguration.ASTStarBarHeight;
        private int StarWidth => PluginConfiguration.ASTStarBarWidth;
        private int StarBarX => PluginConfiguration.ASTStarBarX;
        private int StarBarY => PluginConfiguration.ASTStarBarY;

        private int LightspeedHeight => PluginConfiguration.ASTLightspeedBarHeight;
        private int LightspeedWidth => PluginConfiguration.ASTLightspeedBarWidth;
        private int LightspeedBarX => PluginConfiguration.ASTLightspeedBarX;
        private int LightspeedBarY => PluginConfiguration.ASTLightspeedBarY;

        private bool ShowDivinationBar => PluginConfiguration.ASTShowDivinationBar;
        private bool ShowDrawBar => PluginConfiguration.ASTShowDrawBar;
        private bool ShowDotBar => PluginConfiguration.ASTShowDotBar;
        private bool ShowStarBar => PluginConfiguration.ASTShowStarBar;
        private bool ShowLightspeedBar => PluginConfiguration.ASTShowLightspeedBar;
        private bool ShowStarGlowBar => PluginConfiguration.ASTShowStarGlowBar;
        private bool ShowDivinationGlowBar => PluginConfiguration.ASTShowDivinationGlowBar;
        private bool ShowDrawGlowBar => PluginConfiguration.ASTShowDrawGlowBar;

        private bool ShowDivinationTextBar => PluginConfiguration.ASTShowDivinationTextBar;
        private bool ShowDrawTextBar => PluginConfiguration.ASTShowDrawTextBar;
        private bool ShowRedrawBar => PluginConfiguration.ASTShowRedrawBar;
        private bool ShowPrimaryResourceBar => PluginConfiguration.ASTShowPrimaryResourceBar;

        private Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000];

        private Dictionary<string, uint> SealSunColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000 + 1];
        private Dictionary<string, uint> SealLunarColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000 + 2];
        private Dictionary<string, uint> SealCelestialColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000 + 3];  
        private Dictionary<string, uint> StarEarthlyColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000 + 4];
        private Dictionary<string, uint> StarGiantColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000 + 5];
        private Dictionary<string, uint> LightspeedColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000 + 6];
        private Dictionary<string, uint> DotColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000 + 7];
        private Dictionary<string, uint> StarGlowColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000 + 8];
        private Dictionary<string, uint> DivinationGlowColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000 + 9];
        private Dictionary<string, uint> DrawMeleeGlowColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000 + 10];
        private Dictionary<string, uint> DrawRangedGlowColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000 + 11];
        private Dictionary<string, uint> DrawCDColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000 + 12];
        private Dictionary<string, uint> DrawCDReadyColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000 + 13];

        private readonly SpellHelper _spellHelper = new();

        public AstrologianHudWindow(
            ClientState clientState,
            DalamudPluginInterface pluginInterface,
            DataManager dataManager,
            Framework framework,
            GameGui gameGui,
            JobGauges jobGauges,
            ObjectTable objectTable, 
            PluginConfiguration pluginConfiguration,
            SigScanner sigScanner,
            TargetManager targetManager,
            UiBuilder uiBuilder
        ) : base(
            clientState,
            pluginInterface,
            dataManager,
            framework,
            gameGui,
            jobGauges,
            objectTable,
            pluginConfiguration,
            sigScanner,
            targetManager,
            uiBuilder
        ) { }

        protected override void Draw(bool _)
        {
            if (ShowDivinationBar) {
                DrawDivinationBar();
            }
            
            if (ShowDrawBar) {
                DrawDraw();
            }
            
            if (ShowDotBar) {
                DrawDot();
            }
            
            if (ShowLightspeedBar) {
                DrawLightspeed();
            }
            
            if (ShowStarBar) {
                DrawStar();
            }
        }

        protected override void DrawPrimaryResourceBar()
        {
            if (!ShowPrimaryResourceBar) {
                return;
            }

            base.DrawPrimaryResourceBar();
        }
        
        private unsafe void DrawDivinationBar()
        {
            var chunkColors = new List<Dictionary<string, uint>>();
            if (ClientState.LocalPlayer?.ClassJob.Id != 33) {
                return;
            }

            var gauge = JobGauges.Get<ASTGauge>();
            var field = typeof(ASTGauge).GetField("seals", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
            var textSealReady = "";
            var sealNumbers = 0;
            var result = field?.GetValue(gauge);
            var hdl = GCHandle.Alloc(result, GCHandleType.Pinned);
            var p = (byte*)hdl.AddrOfPinnedObject();
            
            for (var ix = 0; ix < 3; ++ix) {
                var seal = *(p + ix);
                var type = (SealType)seal;
                
                switch (type) {
                    case SealType.NONE:
                        chunkColors.Add(EmptyColor);
                        break;
                    case SealType.MOON:
                        chunkColors.Add(SealLunarColor);
                        break;
                    case SealType.SUN:
                        chunkColors.Add(SealSunColor);
                        break;
                    case SealType.CELESTIAL:
                        chunkColors.Add(SealCelestialColor);
                        break;
                }
                
                if (!gauge.ContainsSeal(SealType.NONE)) {
                    sealNumbers = 0;
                    if (gauge.ContainsSeal(SealType.SUN)) { sealNumbers++; };
                    if (gauge.ContainsSeal(SealType.MOON)) { sealNumbers++; };
                    if (gauge.ContainsSeal(SealType.CELESTIAL)) { sealNumbers++; };
                    textSealReady = sealNumbers.ToString();
                }
            }
                
            hdl.Free();
            var xPos = CenterX - XOffset + BaseXOffset + DivinationBarX;
            var yPos = CenterY + YOffset + BaseYOffset + DivinationBarY;

            var bar = BarBuilder.Create(xPos, yPos, DivinationHeight, DivinationWidth)
                .SetBackgroundColor(EmptyColor["background"])
                .SetChunks(3)
                .SetChunkPadding(DivinationBarPad)
                .AddInnerBar(chunkColors.Count(n => n != EmptyColor), 3, chunkColors.ToArray())
                .SetTextMode(BarTextMode.Single)
                .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, textSealReady);
                
            if (!ShowDivinationTextBar) {
                textSealReady = "";
            };

            bar.SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, textSealReady);

            if (ShowDivinationGlowBar) {
                var chucksToGlow = new bool[3];
                for (var i = 0; i < sealNumbers; i++) {
                    chucksToGlow[i] = true;
                }
                
                bar.SetGlowChunks(chucksToGlow);
                bar.SetGlowColor(DivinationGlowColor["base"]);
            };

            var drawList = ImGui.GetWindowDrawList();
            bar.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawDraw() {
            var gauge = JobGauges.Get<ASTGauge>();

            var xPos = CenterX - XOffset + BaseXOffset + DrawBarX;
            var yPos = CenterY + YOffset + BaseYOffset + DrawBarY;

            var cardJob = "";
            var cardColor = EmptyColor;
            var builder = BarBuilder.Create(xPos, yPos, DrawHeight, DrawWidth);

            switch (gauge.DrawnCard)
            {
                case CardType.BALANCE:
                    cardColor = SealSunColor;
                    cardJob = "MELEE";
                    break;
                case CardType.BOLE:
                    cardColor = SealSunColor;
                    cardJob = "RANGED";
                    break;
                case CardType.ARROW:
                    cardColor = SealLunarColor;
                    cardJob = "MELEE";
                    break;
                case CardType.EWER:
                    cardColor = SealLunarColor;
                    cardJob = "RANGED";
                    break;
                case CardType.SPEAR:
                    cardColor = SealCelestialColor;
                    cardJob = "MELEE";
                    break;
                case CardType.SPIRE:
                    cardColor = SealCelestialColor;
                    cardJob = "RANGED";
                    break;
            }

            var cardPresent = 0f;
            var cardMax = 0f;
            var drawList = ImGui.GetWindowDrawList();
            var drawCastInfo = _spellHelper.GetSpellCooldown(3590);
            var redrawCastInfo = _spellHelper.GetSpellCooldownInt(3593);
            var redrawStacks = _spellHelper.GetStackCount(3, 3593);

            if (cardJob != "") {
                cardPresent = 1f;
                cardMax = 1f;
            }
            else {
                cardPresent = drawCastInfo > 0 ? drawCastInfo : 1f;
                cardJob = drawCastInfo > 0 ? Math.Abs(drawCastInfo).ToString("N0") : "READY";
                cardColor = drawCastInfo > 0 ? DrawCDColor : DrawCDReadyColor;
                cardMax = drawCastInfo > 0 ? 30f : 1f;
            }

            var bar = builder.AddInnerBar(Math.Abs(cardPresent), cardMax, cardColor)
                .SetBackgroundColor(EmptyColor["background"])
                .SetTextMode(BarTextMode.Single);

            if (ShowDrawGlowBar) {
                switch (cardJob) {
                    case "RANGED":
                        bar.SetGlowColor(DrawRangedGlowColor["base"]);
                        break;
                    case "MELEE":
                        bar.SetGlowColor(DrawMeleeGlowColor["base"]);
                        break;
                };
            }

            if (!ShowDrawTextBar) {
                cardJob = "";
            }

            switch (cardJob) {
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
                    bar.SetText(BarTextPosition.CenterLeft, BarTextType.Custom, cardJob);
                    break;
            }

            if (ShowRedrawBar) {
                var redrawText = (redrawCastInfo > 0 ? redrawCastInfo.ToString("N0") + " [" + redrawStacks.ToString("N0") + "]" : redrawStacks.ToString("N0"));
                bar.AddPrimaryText(new BarText(BarTextPosition.CenterRight, BarTextType.Custom, redrawText));
            }
            
            bar.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawDot()
        {
            var actor = TargetManager.SoftTarget ?? TargetManager.Target;
            var xPos = CenterX - XOffset + BaseXOffset + DotBarX;
            var yPos = CenterY + YOffset + BaseYOffset + DotBarY;
            var drawList = ImGui.GetWindowDrawList();
            var builder = BarBuilder.Create(xPos, yPos, DotHeight, DotWidth);
            
            if (actor is not BattleChara target) {
                var barNoTarget = builder.AddInnerBar(0, 30f, DotColor)
                    .SetBackgroundColor(EmptyColor["background"])
                    .SetTextMode(BarTextMode.Single)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                    .Build();
                barNoTarget.Draw(drawList, PluginConfiguration);
                return;
            };
            
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var dot = target.StatusList.FirstOrDefault(o => o.StatusId == 1881 && o.SourceID == ClientState.LocalPlayer.ObjectId ||
                                                            o.StatusId == 843 && o.SourceID == ClientState.LocalPlayer.ObjectId ||
                                                            o.StatusId == 838 && o.SourceID == ClientState.LocalPlayer.ObjectId);
            var dotCooldown = dot?.StatusId == 838 ? 18f : 30f;
            var dotDuration = dot?.RemainingTime ?? 0f;

            var bar = builder.AddInnerBar(Math.Abs(dotDuration), dotCooldown, DotColor)
                .SetBackgroundColor(EmptyColor["background"])
                .SetTextMode(BarTextMode.Single)
                .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                .Build();

            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawLightspeed()
        {
            var lightspeedBuff = ClientState.LocalPlayer.StatusList.Where(o => o.SourceID == 841);
            var lightspeedDuration = 0f;
            const float lightspeedMaxDuration = 15f;

            var xPos = CenterX - XOffset + BaseXOffset + LightspeedBarX;
            var yPos = CenterY + YOffset + BaseYOffset + LightspeedBarY;

            if (lightspeedBuff.Any()) {
                lightspeedDuration = Math.Abs(lightspeedBuff.First().RemainingTime);
            }

            var builder = BarBuilder.Create(xPos, yPos, LightspeedHeight, LightspeedWidth);

            var bar = builder.AddInnerBar(lightspeedDuration, lightspeedMaxDuration, EmptyColor, LightspeedColor)               
                .SetTextMode(BarTextMode.Single)
                .SetBackgroundColor(EmptyColor["background"])
                .SetFlipDrainDirection(true)
                .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawStar()
        {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var starPreCookingBuff = ClientState.LocalPlayer.StatusList.Where(o => o.StatusId == 1224);
            var starPostCookingBuff = ClientState.LocalPlayer.StatusList.Where(o => o.StatusId == 1248);
            var starDuration = 0f;
            const float starMaxDuration = 10f;

            var xPos = CenterX - XOffset + BaseXOffset + StarBarX;
            var yPos = CenterY + YOffset + BaseYOffset + StarBarY;
            var starColorSelector = EmptyColor;

            if (starPreCookingBuff.Any()) { 
                starDuration = starMaxDuration - Math.Abs(starPreCookingBuff.First().RemainingTime);
                starColorSelector = StarEarthlyColor;
            }

            if (starPostCookingBuff.Any()) {
                starDuration = Math.Abs(starPostCookingBuff.First().RemainingTime);
                starColorSelector = StarGiantColor;
            }

            var builder = BarBuilder.Create(xPos, yPos, StarHeight, StarWidth);

            var bar = builder.AddInnerBar(starDuration, starMaxDuration, EmptyColor, starColorSelector)
                .SetTextMode(BarTextMode.Single)
                .SetBackgroundColor(EmptyColor["background"])
                .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);

            if (starColorSelector == StarGiantColor && ShowStarGlowBar) {
                bar.SetGlowColor(StarGlowColor["base"]);
            }

            var drawList = ImGui.GetWindowDrawList();
            bar.Build().Draw(drawList, PluginConfiguration);
        }
    }
}