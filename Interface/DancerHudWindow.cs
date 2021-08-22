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
            int esprit = gauge.Esprit;
            var currentResource = Math.Min(esprit, chunkSize); 
            var scale = (float) currentResource / chunkSize;
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(PluginConfiguration.SecondaryBarBackgroundImage.ImGuiHandle, barSize, Vector2.One, Vector2.Zero);
            
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(
                scale >= 1.0f ? PluginConfiguration.SecondaryBarImage.ImGuiHandle : PluginConfiguration.SecondaryBarDimImage.ImGuiHandle,
                new Vector2(barWidth * scale, BarHeight),
                new Vector2(scale, 1f),
                Vector2.Zero
            );
            
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(ImageBorder, barSize, Vector2.One, Vector2.Zero);
            
            // Chunk 2
            currentResource = Math.Max(Math.Min(esprit, chunkSize * 2) - chunkSize, 0); 
            scale = (float) currentResource / chunkSize;
            cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
            
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(PluginConfiguration.SecondaryBarBackgroundImage.ImGuiHandle, barSize, Vector2.One, Vector2.Zero);
            
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(
                scale >= 1.0f ? PluginConfiguration.SecondaryBarImage.ImGuiHandle : PluginConfiguration.SecondaryBarDimImage.ImGuiHandle,
                new Vector2(barWidth * scale, BarHeight),
                new Vector2(scale, 1f),
                Vector2.Zero
            );
            
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(ImageBorder, barSize, Vector2.One, Vector2.Zero);
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
            
            var imagePrimary = PluginConfiguration.PrimaryBarImage.ImGuiHandle;
            var imageSecondaryDim = PluginConfiguration.SecondaryBarDimImage.ImGuiHandle;

            var cursorPos = new Vector2(xPos - xPadding - barWidth, yPos);
            
            for (var i = 1; i < 5; i++) {
                cursorPos = new Vector2(cursorPos.X + xPadding + barWidth, cursorPos.Y);
                ImGui.SetCursorPos(cursorPos);
                ImGui.Image(gauge.NumFeathers >= i ? imagePrimary : imageSecondaryDim, barSize, Vector2.One, Vector2.Zero);

                ImGui.SetCursorPos(cursorPos);
                ImGui.Image(ImageBorder, barSize, Vector2.One, Vector2.Zero);
            }
        }
    }
}