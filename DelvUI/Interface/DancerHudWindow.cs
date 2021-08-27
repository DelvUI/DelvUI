using System;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUI.Interface {
    public class DancerHudWindow : HudWindow {
        public override uint JobId => 38;
        
        private static int BarHeight => 13;
        private static int BarWidth => 254;
        private new static int XOffset => 127;
        private new static int YOffset => 476;
        
        public DancerHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _) {
            DrawHealthBar();
            DrawPrimaryResourceBar();
            DrawSecondaryResourceBar();
            DrawTargetBar();
            DrawFocusBar();
            DrawCastBar();
        }

        protected override void DrawPrimaryResourceBar() {
            var gauge = PluginInterface.ClientState.JobGauges.Get<DNCGauge>();
            
            const int xPadding = 2;
            var barWidth = (BarWidth - xPadding) / 2;
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset - 46;
            var cursorPos = new Vector2(xPos, yPos);
            const int chunkSize = 50;
            var barSize = new Vector2(barWidth, BarHeight);
            
            // Chunk 1
            var esprit = Math.Min((int)gauge.Esprit, chunkSize); 
            var scale = (float) esprit / chunkSize;
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            
            if (scale >= 1.0f) {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barWidth * scale, BarHeight), 
                    0xFF3DD8FE, 0xFF3BF3FF, 0xFF3BF3FF, 0xFF3DD8FE
                );
            }
            else {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barWidth * scale, BarHeight), 
                    0xFF90827C, 0xFF8E8D8F, 0xFF8E8D8F, 0xFF90827C
                );
            }
            
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            
            // Chunk 2
            esprit = Math.Max(Math.Min((int)gauge.Esprit, chunkSize * 2) - chunkSize, 0); 
            scale = (float) esprit / chunkSize;
            cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
            
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            
            if (scale >= 1.0f) {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barWidth * scale, BarHeight), 
                    0xFF3DD8FE, 0xFF3BF3FF, 0xFF3BF3FF, 0xFF3DD8FE
                );
            }
            else {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barWidth * scale, BarHeight), 
                    0xFF90827C, 0xFF8E8D8F, 0xFF8E8D8F, 0xFF90827C
                );
            }
            
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }
        
        private void DrawSecondaryResourceBar() {
            var gauge = PluginInterface.ClientState.JobGauges.Get<DNCGauge>();

            const int xPadding = 2;
            var barWidth = (BarWidth - xPadding * 3) / 4;
            var barSize = new Vector2(barWidth, BarHeight);
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset - 30;
            var cursorPos = new Vector2(xPos, yPos);

            var drawList = ImGui.GetWindowDrawList();
            for (var i = 0; i <= 4 - 1; i++)
            {
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                if (gauge.NumFeathers > i)
                {
                    drawList.AddRectFilled(cursorPos, cursorPos + new Vector2(barSize.X, barSize.Y), 0xFF4FD29B);
                }

                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
            }
        }
    }
}

