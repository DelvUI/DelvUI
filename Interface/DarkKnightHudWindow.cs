using System;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUIPlugin.Interface {
    public class DarkKnightHudWindow : HudWindow {
        public override uint JobId => 32;

        private new int BarHeight => 26;
        private new int BarWidth => 357;
        private new int XOffset => 178;
        private new int YOffset => 496;
        
        public DarkKnightHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _) {
            DrawHealthBar();
            DrawPrimaryResourceBar();
            DrawSecondaryResourceBar();
            DrawTargetBar();
        }

        private void DrawPrimaryResourceBar() {
            var actor = PluginInterface.ClientState.LocalPlayer;

            const int xPadding = 6;
            var barWidth = (BarWidth - xPadding * 2)  / 3.0f;
            var barSize = new Vector2(barWidth, BarHeight);
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset;
            var cursorPos = new Vector2(xPos, yPos);
            const int chunkSize = 3000;

            // Chunk 1
            var currentResource = Math.Min(actor.CurrentMp, chunkSize); 
            var scale = (float) currentResource / chunkSize;
            
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(PluginConfiguration.PrimaryBarBackgroundImage.ImGuiHandle, barSize, Vector2.One, Vector2.Zero);

            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(PluginConfiguration.PrimaryBarImage.ImGuiHandle, new Vector2(barWidth * scale, BarHeight), new Vector2(scale, 1f), Vector2.Zero);
            
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(ImageBorder, barSize, Vector2.One, Vector2.Zero);
            
            // Chunk 2
            currentResource = Math.Max(Math.Min(actor.CurrentMp, chunkSize * 2) - chunkSize, 0); 
            scale = (float) currentResource / chunkSize;
            cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
            
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(PluginConfiguration.PrimaryBarBackgroundImage.ImGuiHandle, barSize, Vector2.One, Vector2.Zero);
            
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(PluginConfiguration.PrimaryBarImage.ImGuiHandle, new Vector2(barWidth * scale, BarHeight), new Vector2(scale, 1f), Vector2.Zero);
            
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(ImageBorder, barSize, Vector2.One, Vector2.Zero);
            
            // Chunk 3
            currentResource = Math.Max(Math.Min(actor.CurrentMp, chunkSize * 3) - chunkSize * 2, 0); 
            scale = (float) currentResource / chunkSize;
            cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image( PluginConfiguration.PrimaryBarBackgroundImage.ImGuiHandle, barSize, Vector2.One, Vector2.Zero);
            
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(PluginConfiguration.PrimaryBarImage.ImGuiHandle, new Vector2(barWidth * scale, BarHeight), new Vector2(scale, 1f), Vector2.Zero);
            
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(ImageBorder, barSize, Vector2.One, Vector2.Zero);
        }
        
        private void DrawSecondaryResourceBar() {
            var gauge = PluginInterface.ClientState.JobGauges.Get<DRKGauge>();
            
            const int xPadding = 5;
            const int yPadding = 3;
            var barWidth = (BarWidth - xPadding) / 2;
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset + BarHeight + yPadding;
            var cursorPos = new Vector2(xPos, yPos);
            const int chunkSize = 50;
            var barSize = new Vector2(barWidth, BarHeight);
            
            // Chunk 1
            int blood = gauge.Blood;
            var currentResource = Math.Min(blood, chunkSize); 
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
            currentResource = Math.Max(Math.Min(blood, chunkSize * 2) - chunkSize, 0); 
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
    }
}