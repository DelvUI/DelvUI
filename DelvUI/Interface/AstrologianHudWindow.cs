using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using Dalamud.Game.ClientState.Structs.JobGauge;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUI.Interface
{
    class AstrologianHudWindow : HudWindow
    {
        public override uint JobId => 33;

        private int DivinationHeight => PluginConfiguration.AstDivinationHeight;

        private int DivinationWidth => PluginConfiguration.AstDivinationWidth;

        private int DivinationBarX => PluginConfiguration.AstDivinationBarX;

        private int DivinationBarY => PluginConfiguration.AstDivinationBarY;

        private int DivinationBarPad => PluginConfiguration.AstDivinationBarPad;

        private int DrawHeight => PluginConfiguration.AstDrawBarHeight;

        private int DrawWidth => PluginConfiguration.AstDrawBarWidth;

        private int DrawBarX => PluginConfiguration.AstDrawBarX;

        private int DrawBarY => PluginConfiguration.AstDrawBarY;

        private Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000];

        private Dictionary<string, uint> SealSunColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000 + 1];

        private Dictionary<string, uint> SealLunarColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000 + 2];

        private Dictionary<string, uint> SealCelestialColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000 + 3];

        private new Vector2 BarSize { get; set; }

        private Vector2 BarCoords { get; set; }

        public AstrologianHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _)
        {
            DrawHealthBar();
            DrawPrimaryResourceBar();
            DrawDivinationBar();
            DrawDraw();
            DrawTargetBar();
            DrawFocusBar();
            DrawCastBar();
        }
        protected new void DrawOutlinedText(string text, Vector2 pos)
        {
            DrawOutlinedText(text, pos, Vector4.One, new Vector4(0f, 0f, 0f, 1f));
        }
        private void DrawDivinationBar()
        {
            //NOTE: Using FFXIVClientStructs may be a better solution to detect duplicate seals
            //https://github.com/aers/FFXIVClientStructs/blob/dd3ec0485395e23592303311bb29d0447e03f06d/FFXIVClientStructs/FFXIV/Client/Game/Gauge/JobGauges.cs#L33

            var gauge = PluginInterface.ClientState.JobGauges.Get<ASTGauge>();

            var barWidth = (DivinationWidth / 3);
            BarSize = new Vector2(barWidth, DivinationHeight);
            BarCoords = new Vector2(DivinationBarX, DivinationBarY);
            var cursorPos = new Vector2(CenterX + BarCoords.X, CenterY + BarCoords.Y - 58);

            var drawList = ImGui.GetWindowDrawList();

            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["gradientRight"]);
            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
            cursorPos = new Vector2(cursorPos.X + barWidth + DivinationBarPad, cursorPos.Y);

            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["gradientRight"]);
            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
            cursorPos = new Vector2(cursorPos.X - barWidth * 2 - DivinationBarPad * 2, cursorPos.Y);

            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["gradientRight"]);
            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);

            if (gauge.ContainsSeal(SealType.SUN)) 
                {
                drawList.AddRectFilled(cursorPos, cursorPos + BarSize, SealSunColor["gradientRight"]);
                drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
            }
            cursorPos = new Vector2(cursorPos.X + barWidth + DivinationBarPad, cursorPos.Y);
            if (gauge.ContainsSeal(SealType.MOON))
            {
                drawList.AddRectFilled(cursorPos, cursorPos + BarSize, SealLunarColor["gradientRight"]);
                drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
            }
            cursorPos = new Vector2(cursorPos.X + barWidth + DivinationBarPad, cursorPos.Y);
            if (gauge.ContainsSeal(SealType.CELESTIAL))
            {
                drawList.AddRectFilled(cursorPos, cursorPos + BarSize, SealCelestialColor["gradientRight"]);
                drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
            }      
        }

        private void DrawDraw()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<ASTGauge>();

            BarSize = new Vector2(DrawWidth, DrawHeight);
            BarCoords = new Vector2(DrawBarX, DrawBarY);
            var cursorPos = new Vector2(CenterX - BarCoords.X, CenterY + BarCoords.Y - 37);

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
    }
}