using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUIPlugin.Interface {
    public class DarkKnightHudWindow : HudWindow {
        public override uint JobId => 32;

        private int BarHeight => 13;
        private int BarWidth => 254;
        private new int XOffset => 127;
        private new int YOffset => 466;
        
        public DarkKnightHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _) {
            DrawHealthBar();
            DrawPrimaryResourceBar();
            DrawSecondaryResourceBar();
            DrawTargetBar();
            DrawFocusBar();
        }

        protected override void DrawPrimaryResourceBar() {
            var actor = PluginInterface.ClientState.LocalPlayer;
            //var tbn = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1178);
            const int xPadding = 2;
            var barWidth = (BarWidth - xPadding * 2)  / 3.0f;
            var barSize = new Vector2(barWidth, BarHeight);
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset - 33;
            var cursorPos = new Vector2(xPos, yPos);
            const int chunkSize = 3000;

            // Chunk 1
            var mana = Math.Min(actor.CurrentMp, chunkSize); 
            
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * mana / chunkSize, barSize.Y), 
                0xFFE6CD00, 0xFFD8Df3C, 0xFFD8Df3C, 0xFFE6CD00
            );
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            
            // Chunk 2
            mana = Math.Max(Math.Min(actor.CurrentMp, chunkSize * 2) - chunkSize, 0); 
            cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
            
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * mana / chunkSize, barSize.Y), 
                0xFFE6CD00, 0xFFD8Df3C, 0xFFD8Df3C, 0xFFE6CD00
            );
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            
            // Chunk 3
            mana = Math.Max(Math.Min(actor.CurrentMp, chunkSize * 3) - chunkSize * 2, 0); 
            cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
            
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * mana / chunkSize, barSize.Y), 
                0xFFE6CD00, 0xFFD8Df3C, 0xFFD8Df3C, 0xFFE6CD00
            );
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }
        
        private void DrawSecondaryResourceBar() {
            var gauge = PluginInterface.ClientState.JobGauges.Get<DRKGauge>();
            
            const int xPadding = 2;
            var barWidth = (BarWidth - xPadding) / 2;
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset + BarHeight - 33;
            var cursorPos = new Vector2(xPos, yPos);
            const int chunkSize = 50;
            var barSize = new Vector2(barWidth, BarHeight);
            
            // Chunk 1
            var blood = Math.Min((int)gauge.Blood, chunkSize); 
            var scale = (float) blood / chunkSize;
            
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);

            if (scale >= 1.0f) {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barWidth * scale, BarHeight), 
                    0xFFC40B95, 0xFFFE00BF, 0xFFFE00BF, 0xFFC40B95
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
            blood = Math.Max(Math.Min((int)gauge.Blood, chunkSize * 2) - chunkSize, 0); 
            scale = (float) blood / chunkSize;
            cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
            
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);

            if (scale >= 1.0f) {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barWidth * scale, BarHeight), 
                    0xFF3D009B, 0xFF4D25DD, 0xFF4D25DD, 0xFF3D009B
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
    }
}