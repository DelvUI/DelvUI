using System;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Interface;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUIPlugin.Interface {
    public class DarkKnightHudWindow : HudWindow {
        public DarkKnightHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        public override void Draw() {
            if (!ShouldBeVisible() || PluginInterface.ClientState.LocalPlayer == null) {
                return;
            }

            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.SetNextWindowSize(ImGui.GetMainViewport().Size);
            
            var begin = ImGui.Begin(
                "DelvUI",
                ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysAutoResize | 
                ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoBringToFrontOnFocus
            );

            if (!begin) {
                return;
            }
            
            DrawHealthBar();
            DrawPrimaryResourceBar();
            DrawSecondaryResourceBar();
            DrawTargetBar();
            
            ImGui.End();
        }

        // Move the positioning offsets and such into config and clean up these Draw methods
        private void DrawPrimaryResourceBar() {
            var actor = PluginInterface.ClientState.LocalPlayer;
            float scale;

            const int xPadding = 5;
            var barWidth = PluginConfiguration.PrimaryBarBackgroundImage.Width / 3.0f;
            var barHeight = PluginConfiguration.PrimaryBarBackgroundImage.Height;
            var xPos = ImGui.GetMainViewport().Size.X / 2f - barWidth - barWidth / 2f;
            var yPos = ImGui.GetMainViewport().Size.Y / 2f + 490;
            
            var cursorPos = new Vector2(xPos, yPos);
            const int chunkSize = 3000;
            int currentResource;
            
            // Chunk 1
            currentResource = Math.Min(actor.CurrentMp, chunkSize); 
            scale = (float) currentResource / chunkSize;
            ImGui.SetCursorPos(cursorPos);

            ImGui.Image(
                PluginConfiguration.PrimaryBarBackgroundImage.ImGuiHandle,
                new Vector2(barWidth, barHeight),
                Vector2.One,
                Vector2.Zero
            );

            ImGui.SetCursorPos(cursorPos);

            ImGui.Image(
                PluginConfiguration.PrimaryBarImage.ImGuiHandle,
                new Vector2(barWidth * scale, barHeight),
                new Vector2(scale, 1f),
                Vector2.Zero
            );
            
            // Chunk 2
            currentResource = Math.Max(Math.Min(actor.CurrentMp, chunkSize * 2) - chunkSize, 0); 
            scale = (float) currentResource / chunkSize;
            ImGui.SetCursorPos(new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y));

            ImGui.Image(
                PluginConfiguration.PrimaryBarBackgroundImage.ImGuiHandle,
                new Vector2(barWidth, barHeight),
                Vector2.One,
                Vector2.Zero
            );

            ImGui.SetCursorPos(new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y));

            ImGui.Image(
                PluginConfiguration.PrimaryBarImage.ImGuiHandle,
                new Vector2(barWidth * scale, barHeight),
                new Vector2(scale, 1f),
                Vector2.Zero
            );
            
            // Chunk 3
            currentResource = Math.Max(Math.Min(actor.CurrentMp, chunkSize * 3) - chunkSize * 2, 0); 
            scale = (float) currentResource / chunkSize;
            ImGui.SetCursorPos(new Vector2(cursorPos.X + barWidth + xPadding + barWidth + xPadding, cursorPos.Y));

            ImGui.Image(
                PluginConfiguration.PrimaryBarBackgroundImage.ImGuiHandle,
                new Vector2(barWidth, barHeight),
                Vector2.One,
                Vector2.Zero
            );

            ImGui.SetCursorPos(new Vector2(cursorPos.X + barWidth + xPadding + barWidth + xPadding, cursorPos.Y));

            ImGui.Image(
                PluginConfiguration.PrimaryBarImage.ImGuiHandle,
                new Vector2(barWidth * scale, barHeight),
                new Vector2(scale, 1f),
                Vector2.Zero
            );
        }
        
        private void DrawSecondaryResourceBar() {
            var gauge = PluginInterface.ClientState.JobGauges.Get<DRKGauge>();
            float scale;

            const int xPadding = 5;
            const int yPadding = 5;
            var barWidth = PluginConfiguration.SecondaryBarBackgroundImage.Width / 2.0f;
            var barHeight = PluginConfiguration.SecondaryBarBackgroundImage.Height;
            var xPos = ImGui.GetMainViewport().Size.X / 2f - barWidth - xPadding + 5;
            var yPos = ImGui.GetMainViewport().Size.Y / 2f + 490 + barHeight + yPadding;
            
            var cursorPos = new Vector2(xPos, yPos);
            const int chunkSize = 50;

            // Chunk 1
            int blood = gauge.Blood;
            var currentResource = Math.Min(blood, chunkSize); 
            scale = (float) currentResource / chunkSize;
            ImGui.SetCursorPos(cursorPos);

            ImGui.Image(
                PluginConfiguration.SecondaryBarBackgroundImage.ImGuiHandle,
                new Vector2(barWidth, barHeight),
                Vector2.One,
                Vector2.Zero
            );

            ImGui.SetCursorPos(cursorPos);

            ImGui.Image(
                scale >= 1.0f ? PluginConfiguration.SecondaryBarImage.ImGuiHandle : PluginConfiguration.SecondaryBarDimImage.ImGuiHandle,
                new Vector2(barWidth * scale, barHeight),
                new Vector2(scale, 1f),
                Vector2.Zero
            );
            
            // Chunk 2
            currentResource = Math.Max(Math.Min(blood, chunkSize * 2) - chunkSize, 0); 
            scale = (float) currentResource / chunkSize;
            ImGui.SetCursorPos(new Vector2(cursorPos.X + barWidth + xPadding + 5, cursorPos.Y));

            ImGui.Image(
                PluginConfiguration.SecondaryBarBackgroundImage.ImGuiHandle,
                new Vector2(barWidth, barHeight),
                Vector2.One,
                Vector2.Zero
            );

            ImGui.SetCursorPos(new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y));

            ImGui.Image(
                scale >= 1.0f ? PluginConfiguration.SecondaryBarImage.ImGuiHandle : PluginConfiguration.SecondaryBarDimImage.ImGuiHandle,
                new Vector2(barWidth * scale, barHeight),
                new Vector2(scale, 1f),
                Vector2.Zero
            );
        }
    }
}