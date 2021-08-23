using System;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUIPlugin.Interface {
    public class DancerHudWindow : HudWindow {
        public override uint JobId => 38;
        
        private new static int BarHeight => 26;
        private new static int BarWidth => 357;
        private new static int XOffset => 178;
        private new static int YOffset => 496;
        
        public DancerHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _) {
            DrawHealthBar();
            DrawPrimaryResourceBar();
            DrawSecondaryResourceBar();
            DrawTargetBar();
        }

        protected override void DrawPrimaryResourceBar() {
            var gauge = PluginInterface.ClientState.JobGauges.Get<DNCGauge>();
            
            const int xPadding = 5;
            var barWidth = (BarWidth - xPadding) / 2;
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset;
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

            const int xPadding = 3;
            const int yPadding = 3;
            const int numChunks = 4;
            
            var barWidth = (BarWidth - xPadding * (numChunks - 1)) / numChunks;
            var barSize = new Vector2(barWidth, BarHeight);
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset + BarHeight + yPadding;
            var cursorPos = new Vector2(xPos - xPadding - barWidth, yPos);
            
            var drawList = ImGui.GetWindowDrawList();
            
            for (var i = 1; i < 5; i++) {
                cursorPos = new Vector2(cursorPos.X + xPadding + barWidth, cursorPos.Y);
                
                if (gauge.NumFeathers >= i) {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + barSize, 
                        0xFF4FD29B, 0xFF49F6AE, 0xFF49F6AE, 0xFF4FD29B
                    );
                }
                else {
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                }
                
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            }
        }
    }
}

