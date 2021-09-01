using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using Dalamud.Game.ClientState.Structs.JobGauge;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Plugin;
using ImGuiNET;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Linq;
using Dalamud.Game.ClientState.Actors.Types;
using DelvUI.Interface.Bars;
using System;
using DelvUI.Helpers;

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

        private bool ShowPrimaryResourceBar => PluginConfiguration.ASTShowPrimaryResourceBar;

        private Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000];

        private Dictionary<string, uint> SealSunColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000 + 1];
        private Dictionary<string, uint> SealLunarColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000 + 2];
        private Dictionary<string, uint> SealCelestialColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000 + 3];  
        private Dictionary<string, uint> StarEarthlyColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000 + 4];
        private Dictionary<string, uint> StarGiantColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000 + 5];
        private Dictionary<string, uint> LightspeedColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000 + 6];
        private Dictionary<string, uint> DotColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000 + 7];

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
            var barWidth = (DivinationWidth / 3);
            BarSize = new Vector2(barWidth, DivinationHeight);
            BarCoords = new Vector2(DivinationBarX, DivinationBarY);
            var cursorPos = new Vector2(CenterX + BarCoords.X, CenterY + BarCoords.Y );

            var drawList = ImGui.GetWindowDrawList();
            if ((PluginInterface.ClientState.LocalPlayer?.ClassJob.Id) != 33)
            {
                return;
            }
            unsafe
            {
                var gauge = PluginInterface.ClientState.JobGauges.Get<ASTGauge>();
                var field = typeof(ASTGauge).GetField("seals", BindingFlags.NonPublic | BindingFlags.GetField |
                                                               BindingFlags.Instance);
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
                            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["gradientRight"]);
                            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                            cursorPos = new Vector2(cursorPos.X + barWidth + DivinationBarPad, cursorPos.Y);
                            break;
                        case SealType.MOON:
                            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, SealLunarColor["gradientRight"]);
                            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                            cursorPos = new Vector2(cursorPos.X + barWidth + DivinationBarPad, cursorPos.Y);
                            break;
                        case SealType.SUN:
                            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, SealSunColor["gradientRight"]);
                            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                            cursorPos = new Vector2(cursorPos.X + barWidth + DivinationBarPad, cursorPos.Y);
                            break;
                        case SealType.CELESTIAL:
                            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, SealCelestialColor["gradientRight"]);
                            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                            cursorPos = new Vector2(cursorPos.X + barWidth + DivinationBarPad, cursorPos.Y);
                            break;
                    }
                    if (!gauge.ContainsSeal(SealType.NONE))
                    {
                        int sealNumbers = 0;
                        if (gauge.ContainsSeal(SealType.SUN)) { sealNumbers++; };
                        if (gauge.ContainsSeal(SealType.MOON)) { sealNumbers++; };
                        if (gauge.ContainsSeal(SealType.CELESTIAL)) { sealNumbers++; };
                        var textSealReady = sealNumbers.ToString();
                        DrawOutlinedText(textSealReady, new Vector2(CenterX + DivinationBarX + (DivinationWidth / 2f) - (ImGui.CalcTextSize(textSealReady).X / 2f), CenterY + DivinationBarY + (DivinationHeight / 2f) - (ImGui.CalcTextSize(textSealReady).Y / 2f)));
                    }
                }
            }

        }

        private void DrawDraw()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<ASTGauge>();

            BarSize = new Vector2(DrawWidth, DrawHeight);
            BarCoords = new Vector2(DrawBarX, DrawBarY);
            var cursorPos = new Vector2(CenterX + BarCoords.X, CenterY + BarCoords.Y);

            var drawList = ImGui.GetWindowDrawList();

            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["gradientRight"]);
            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
            switch (gauge.DrawnCard()) {
                case CardType.BALANCE:
                    drawList.AddRectFilled(cursorPos, cursorPos + BarSize, SealSunColor["gradientRight"]);
                    drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                    DrawOutlinedText("MELEE", new Vector2(cursorPos.X + DrawWidth / 2f - ImGui.CalcTextSize("MELEE").X / 2f, cursorPos.Y - 2));
                    break;
                case CardType.BOLE:
                    drawList.AddRectFilled(cursorPos, cursorPos + BarSize, SealSunColor["gradientRight"]);
                    drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                    DrawOutlinedText("RANGED", new Vector2(cursorPos.X + DrawWidth / 2f - ImGui.CalcTextSize("RANGED").X / 2f, cursorPos.Y - 2));
                    break;
                case CardType.ARROW:
                    drawList.AddRectFilled(cursorPos, cursorPos + BarSize, SealLunarColor["gradientRight"]);
                    drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                    DrawOutlinedText("MELEE", new Vector2(cursorPos.X + DrawWidth / 2f - ImGui.CalcTextSize("MELEE").X / 2f, cursorPos.Y - 2));
                    break;
                case CardType.EWER:
                    drawList.AddRectFilled(cursorPos, cursorPos + BarSize, SealLunarColor["gradientRight"]);
                    drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                    DrawOutlinedText("RANGED", new Vector2(cursorPos.X + DrawWidth / 2f - ImGui.CalcTextSize("RANGED").X / 2f, cursorPos.Y - 2));
                    break;
                case CardType.SPEAR:
                    drawList.AddRectFilled(cursorPos, cursorPos + BarSize, SealCelestialColor["gradientRight"]);
                    drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                    DrawOutlinedText("MELEE", new Vector2(cursorPos.X + DrawWidth / 2f - ImGui.CalcTextSize("MELEE").X / 2f, cursorPos.Y - 2));
                    break;
                case CardType.SPIRE:                
                    drawList.AddRectFilled(cursorPos, cursorPos + BarSize, SealCelestialColor["gradientRight"]);
                    drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                    DrawOutlinedText("RANGED", new Vector2(cursorPos.X + DrawWidth / 2f - ImGui.CalcTextSize("RANGED").X / 2f, cursorPos.Y - 2));
                    break;
            }
        }

        private void DrawDot()
        {
            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;
            var drawList = ImGui.GetWindowDrawList();

            var xPos = CenterX - XOffset + DotBarX;
            var yPos = CenterY + YOffset + DotBarY;

            var cursorPos = new Vector2(xPos, yPos);

            if (!(target is Chara))
            {
                drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["gradientRight"]);
                drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                return;
            }
            var dot = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1881 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId ||
                                                               o.EffectId == 843 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId ||
                                                               o.EffectId == 838 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId);
            var dotCooldown = dot.EffectId == 838 ? 18f : 30f;
            var dotDuration = dot.Duration;

            var builder = BarBuilder.Create(xPos, yPos, DotHeight, DotWidth);

            Bar bar = builder.AddInnerBar(System.Math.Abs(dotDuration), dotCooldown, DotColor)
                .SetTextMode(BarTextMode.Single)
                .SetBackgroundColor(EmptyColor["gradientRight"])
                .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                .Build();

            bar.Draw(drawList);
        }

        private void DrawLightspeed()
        {
            var lightspeedBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 841);
            var lightspeedDuration = 0f;
            const float lightspeedMaxDuration = 15f;

            var drawList = ImGui.GetWindowDrawList();

            var xPos = CenterX - XOffset + LightspeedBarX;
            var yPos = CenterY + YOffset + LightspeedBarY;

            if (lightspeedBuff.Any())
            {
                lightspeedDuration = Math.Abs(lightspeedBuff.First().Duration);
            }

            var builder = BarBuilder.Create(xPos, yPos, LightspeedHeight, LightspeedWidth);

            Bar bar = builder.AddInnerBar(lightspeedDuration, lightspeedMaxDuration, EmptyColor, LightspeedColor)               
                .SetTextMode(BarTextMode.Single)
                .SetBackgroundColor(EmptyColor["gradientRight"])
                .SetFlipDrainDirection(true)
                .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                .SetVertical(true)
                .Build();

            bar.Draw(drawList);
        }

        private void DrawStar()
        {
            var starPreCookingBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1224);
            var starPostCookingBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1248);
            var starDuration = 0f;
            const float starMaxDuration = 10f;
            var drawList = ImGui.GetWindowDrawList();

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

            Bar bar = builder.AddInnerBar(starDuration, starMaxDuration, EmptyColor, starColorSelector)
                .SetTextMode(BarTextMode.Single)
                .SetBackgroundColor(EmptyColor["gradientRight"])
                .SetFlipDrainDirection(true)
                .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                .SetVertical(true)
                .Build();

            bar.Draw(drawList);
        }
    }
}