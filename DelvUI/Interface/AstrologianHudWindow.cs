using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Interface.Bars;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace DelvUI.Interface
{
    class AstrologianHudWindow : HudWindow
    {
        public override uint JobId => 33;
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

        private new Vector2 BarSize { get; set; }
        private Vector2 BarCoords { get; set; }

        public AstrologianHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _)
        {
            if (ShowDivinationBar)
            {
                DrawDivinationBar();
            }
            if (ShowDrawBar)
            {
                DrawDraw();
            }
            if (ShowDotBar)
            {
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
            if (!ShowPrimaryResourceBar)
            {
                return;
            }

            base.DrawPrimaryResourceBar();
        }

        protected new void DrawOutlinedText(string text, Vector2 pos)
        {
            DrawOutlinedText(text, pos, Vector4.One, new Vector4(0f, 0f, 0f, 1f));
        }
        
        private void DrawDivinationBar()
        {
            List<Dictionary<string, uint>> chunkColors = new List<Dictionary<string, uint>>();
            if ((PluginInterface.ClientState.LocalPlayer?.ClassJob.Id) != 33)
            {
                return;
            }
            unsafe
            {
                var gauge = PluginInterface.ClientState.JobGauges.Get<ASTGauge>();
                var field = typeof(ASTGauge).GetField("seals", BindingFlags.NonPublic | BindingFlags.GetField |
                                                               BindingFlags.Instance);
                var textSealReady = "";
                var sealNumbers = 0;
                var result = field.GetValue(gauge);
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
                            chunkColors.Add(SealLunarColor);
                            break;
                        case SealType.SUN:
                            chunkColors.Add(SealSunColor);
                            break;
                        case SealType.CELESTIAL:
                            chunkColors.Add(SealCelestialColor);
                            break;
                    }
                    if (!gauge.ContainsSeal(SealType.NONE))
                    {
                        sealNumbers = 0;
                        if (gauge.ContainsSeal(SealType.SUN)) { sealNumbers++; };
                        if (gauge.ContainsSeal(SealType.MOON)) { sealNumbers++; };
                        if (gauge.ContainsSeal(SealType.CELESTIAL)) { sealNumbers++; };
                        textSealReady = sealNumbers.ToString();
                    }
                }
                hdl.Free();
                var xPos = CenterX - XOffset + DivinationBarX;
                var yPos = CenterY + YOffset + DivinationBarY;

                var bar = BarBuilder.Create(xPos, yPos, DivinationHeight, DivinationWidth)
                    .SetBackgroundColor(EmptyColor["background"])
                    .SetChunks(3)
                    .SetChunkPadding(DivinationBarPad)
                    .AddInnerBar(chunkColors.Count(n => n != EmptyColor), 3, chunkColors.ToArray())
                    .SetTextMode(BarTextMode.Single)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, textSealReady);
                
                if (!ShowDivinationTextBar)
                {
                    textSealReady = "";
                };

                bar.SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, textSealReady);

                if (ShowDivinationGlowBar)
                {
                    var chucksToGlow = new bool[3];
                    for (int i = 0; i < sealNumbers; i++)
                    {
                        chucksToGlow[i] = true;
                    }
                    bar.SetGlowChunks(chucksToGlow);
                    bar.SetGlowColor(DivinationGlowColor["background"]);
                };

                var drawList = ImGui.GetWindowDrawList();
                bar.Build().Draw(drawList);
            }

        }

        private void DrawDraw()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<ASTGauge>();

            var xPos = CenterX - XOffset + DrawBarX;
            var yPos = CenterY + YOffset + DrawBarY;

            var cardJob = "";
            var cardColor = EmptyColor;
            var builder = BarBuilder.Create(xPos, yPos, DrawHeight, DrawWidth);

            switch (gauge.DrawnCard())
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

            var castInfo = 0f;
            var cardPresent = 0f;
            var cardMax = 0f;
            var drawList = ImGui.GetWindowDrawList();
            unsafe
            {
                var actionManager = ActionManager.Instance();
                var adjustedId = actionManager->GetAdjustedActionId(3590);
                var timeElapsed = actionManager->GetRecastTimeElapsed(ActionType.Spell, adjustedId);
                var timeTotal = actionManager->GetRecastTime(ActionType.Spell, adjustedId);
                castInfo = Math.Abs(timeTotal - timeElapsed);
            }
            
            if (cardJob != "")
            {
                cardPresent = 1f;
                cardMax = 1f;
            }
            else
            {
                cardPresent = castInfo > 0 ? castInfo: 1f;
                cardJob = castInfo > 0 ? Math.Abs(castInfo).ToString("N1") : "READY";
                cardColor = castInfo> 0 ? DrawCDColor : DrawCDReadyColor;
                cardMax = castInfo > 0 ? 30f : 1f;
            }

            var bar = builder.AddInnerBar(Math.Abs(cardPresent), cardMax, cardColor)
                .SetBackgroundColor(EmptyColor["background"])
                .SetTextMode(BarTextMode.Single);

            if (ShowDrawGlowBar)
            {
                switch (cardJob)
                {
                    case "RANGED":
                        bar.SetGlowColor(DrawRangedGlowColor["gradientRight"]);
                        break;
                    case "MELEE":
                        bar.SetGlowColor(DrawMeleeGlowColor["gradientRight"]);
                        break;
                };
            }

            if (!ShowDrawTextBar)
            {
                cardJob = "";
            }

            bar.SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, cardJob);

            

            bar.Build().Draw(drawList);
        }

        private void DrawDot()
        {
            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;
            var xPos = CenterX - XOffset + DotBarX;
            var yPos = CenterY + YOffset + DotBarY;
            var drawList = ImGui.GetWindowDrawList();
            var builder = BarBuilder.Create(xPos, yPos, DotHeight, DotWidth);
            
            if (target is not Chara)
            {
                Bar barNoTarget = builder.AddInnerBar(0, 30f, DotColor)
                    .SetBackgroundColor(EmptyColor["background"])
                    .SetTextMode(BarTextMode.Single)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                    .Build();
                barNoTarget.Draw(drawList);
                return;
            };
            var dot = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1881 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId ||
                                                               o.EffectId == 843 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId ||
                                                               o.EffectId == 838 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId);
            var dotCooldown = dot.EffectId == 838 ? 18f : 30f;
            var dotDuration = dot.Duration;


            var bar = builder.AddInnerBar(System.Math.Abs(dotDuration), dotCooldown, DotColor)
                .SetBackgroundColor(EmptyColor["background"])
                .SetTextMode(BarTextMode.Single)
                .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                .Build();

            bar.Draw(drawList);
        }

        private void DrawLightspeed()
        {
            var lightspeedBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 841);
            var lightspeedDuration = 0f;
            const float lightspeedMaxDuration = 15f;

            var xPos = CenterX - XOffset + LightspeedBarX;
            var yPos = CenterY + YOffset + LightspeedBarY;

            if (lightspeedBuff.Any())
            {
                lightspeedDuration = Math.Abs(lightspeedBuff.First().Duration);
            }

            var builder = BarBuilder.Create(xPos, yPos, LightspeedHeight, LightspeedWidth);

            var bar = builder.AddInnerBar(lightspeedDuration, lightspeedMaxDuration, EmptyColor, LightspeedColor)               
                .SetTextMode(BarTextMode.Single)
                .SetBackgroundColor(EmptyColor["background"])
                .SetFlipDrainDirection(true)
                .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        private void DrawStar()
        {
            var starPreCookingBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1224);
            var starPostCookingBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1248);
            var starDuration = 0f;
            const float starMaxDuration = 10f;

            var xPos = CenterX - XOffset + StarBarX;
            var yPos = CenterY + YOffset + StarBarY;
            var starColorSelector = EmptyColor;

            if (starPreCookingBuff.Any())
            { 
                starDuration = starMaxDuration - Math.Abs(starPreCookingBuff.First().Duration);
                starColorSelector = StarEarthlyColor;
            }

            if (starPostCookingBuff.Any())
            {
                starDuration = Math.Abs(starPostCookingBuff.First().Duration);
                starColorSelector = StarGiantColor;
            }

            var builder = BarBuilder.Create(xPos, yPos, StarHeight, StarWidth);

            var bar = builder.AddInnerBar(starDuration, starMaxDuration, EmptyColor, starColorSelector)
                .SetTextMode(BarTextMode.Single)
                .SetBackgroundColor(EmptyColor["background"])
                .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);

            if (starColorSelector == StarGiantColor && ShowStarGlowBar)
            {
                bar.SetGlowColor(StarGlowColor["background"]);
            };

            var drawList = ImGui.GetWindowDrawList();
            bar.Build().Draw(drawList);
        }
    }
}