using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUIPlugin.Interface {
    public class WhiteMageHudWindow : HudWindow {
        public override uint JobId => 24;
        
        private new int BarHeight => 26;
        private new int BarWidth => 357;
        private new int XOffset => 178;
        private new int YOffset => 496;
        
        public WhiteMageHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _) {
            DrawHealthBar();
            DrawPrimaryResourceBar();
            DrawSecondaryResourceBar();
            DrawTargetBar();
        }

        private void DrawSecondaryResourceBar() {
            var gauge = PluginInterface.ClientState.JobGauges.Get<WHMGauge>();

            const int xPadding = 3;
            const int yPadding = 3;
            const int numChunks = 6;
            
            var barWidth = (BarWidth - xPadding * (numChunks - 1)) / numChunks;
            var barSize = new Vector2(barWidth, BarHeight);
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset + BarHeight + yPadding;
            
            const float lilyCooldown = 30000f;
            var imagePrimary = PluginConfiguration.PrimaryBarImage.ImGuiHandle;
            var imageSecondary = PluginConfiguration.SecondaryBarImage.ImGuiHandle;
            var imageSecondaryDim = PluginConfiguration.SecondaryBarDimImage.ImGuiHandle;
            var imageSecondaryBackground = PluginConfiguration.SecondaryBarBackgroundImage.ImGuiHandle;

            var cursorPos = new Vector2(xPos, yPos);
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(imageSecondaryBackground, barSize, Vector2.One, Vector2.Zero);
            
            var scale = gauge.NumLilies == 0 ? gauge.LilyTimer / lilyCooldown : 1;
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(
                gauge.NumLilies >= 1 ? imagePrimary : imageSecondaryDim, 
                new Vector2(barWidth * scale, BarHeight), 
                new Vector2(scale, 1f), 
                Vector2.Zero
            );
            
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(ImageBorder, barSize, Vector2.One, Vector2.Zero);

            cursorPos = new Vector2(cursorPos.X + xPadding + barWidth, cursorPos.Y);
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(imageSecondaryBackground, barSize, Vector2.One, Vector2.Zero);

            if (gauge.NumLilies > 0) {
                scale = gauge.NumLilies == 1 ? gauge.LilyTimer / lilyCooldown : 1;
                ImGui.SetCursorPos(cursorPos);
                ImGui.Image(
                    gauge.NumLilies >= 2 ? imagePrimary : imageSecondaryDim,
                    new Vector2(barWidth * scale, BarHeight),
                    new Vector2(scale, 1f),
                    Vector2.Zero
                );
            }
            
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(ImageBorder, barSize, Vector2.One, Vector2.Zero);

            cursorPos = new Vector2(cursorPos.X + xPadding + barWidth, cursorPos.Y);
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(imageSecondaryBackground, barSize, Vector2.One, Vector2.Zero);

            if (gauge.NumLilies > 1) {
                scale = gauge.NumLilies == 2 ? gauge.LilyTimer / lilyCooldown : 1;
                ImGui.SetCursorPos(cursorPos);
                ImGui.Image(
                    gauge.NumLilies == 3 ? imagePrimary : imageSecondaryDim,
                    new Vector2(barWidth * scale, BarHeight),
                    new Vector2(scale, 1f),
                    Vector2.Zero
                );
            }
            
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(ImageBorder, barSize, Vector2.One, Vector2.Zero);

            // Blood Lilies
            cursorPos = new Vector2(cursorPos.X + xPadding + barWidth, cursorPos.Y);
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(gauge.NumBloodLily > 0 ? imageSecondary : imageSecondaryBackground, barSize, Vector2.One, Vector2.Zero);
            
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(ImageBorder, barSize, Vector2.One, Vector2.Zero);

            cursorPos = new Vector2(cursorPos.X + xPadding + barWidth, cursorPos.Y);
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(gauge.NumBloodLily > 1 ? imageSecondary : imageSecondaryBackground, barSize, Vector2.One, Vector2.Zero);
            
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(ImageBorder, barSize, Vector2.One, Vector2.Zero);

            cursorPos = new Vector2(cursorPos.X + xPadding + barWidth, cursorPos.Y);
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(gauge.NumBloodLily > 2 ? imageSecondary : imageSecondaryBackground, barSize, Vector2.One, Vector2.Zero);
            
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(ImageBorder, barSize, Vector2.One, Vector2.Zero);
        }
    }
}