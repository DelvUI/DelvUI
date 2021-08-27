using System;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUI.Interface {
    public class SamuraiHudWindow : HudWindow {

        public override uint JobId => 34;
        private static int BarHeight => 20;
        private int BarWidth => 250;
        private new int XOffset => 127;
        private new int YOffset => 416;

        public SamuraiHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _) {
            DrawHealthBar();
            DrawPrimaryResourceBar();
            DrawSenResourceBar();
            DrawMeditationResourceBar();
            DrawTargetBar();
            DrawFocusBar();
            DrawCastBar();
        }

        protected override void DrawPrimaryResourceBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<SAMGauge>();

            const int yPadding = 3;
            var barWidth = BarWidth;
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset - BarHeight - yPadding;
            var cursorPos = new Vector2(xPos, yPos);
            const int chunkSize = 100;
            var barSize = new Vector2(barWidth, BarHeight);

            // Kenki Gauge
            var kenki = Math.Min((int)gauge.Kenki, chunkSize);
            var scale = (float)kenki / chunkSize;
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barWidth * scale, BarHeight),
                0xFF5252FF, 0xFF9C9CFF, 0xFF9C9CFF, 0xFF5252FF
            );

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            var textSize = ImGui.CalcTextSize(gauge.Kenki.ToString());
            DrawOutlinedText(gauge.Kenki.ToString(), new Vector2(cursorPos.X + BarWidth / 2f - textSize.X / 2f, cursorPos.Y-2));

        }
        
        private void DrawSenResourceBar() {
            var gauge = PluginInterface.ClientState.JobGauges.Get<SAMGauge>();

            const int xPadding = 3;
            const int numChunks = 3;
            
            var barWidth = (BarWidth - xPadding * (numChunks - 1)) / numChunks;
            var barSize = new Vector2(barWidth, BarHeight);
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset ;
            var cursorPos = new Vector2(xPos - xPadding - barWidth, yPos);
            
            var drawList = ImGui.GetWindowDrawList();
            
            // Setsu Bar
            cursorPos = new Vector2(cursorPos.X + xPadding + barWidth, cursorPos.Y);
            if (gauge.HasSetsu()) drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0xFFF7EA59);
            else drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            // Getsu Bar
            cursorPos = new Vector2(cursorPos.X + xPadding + barWidth, cursorPos.Y);
            if (gauge.HasGetsu())drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0xFFF77E59);
            else drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            // Ka Bar
            cursorPos = new Vector2(cursorPos.X + xPadding + barWidth, cursorPos.Y);
            if (gauge.HasKa())drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0XFF5959F7);
            else drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }
    

        private void DrawMeditationResourceBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<SAMGauge>();

            const int xPadding = 3;
            const int yPadding = 3;
            const int numChunks = 3;

            var barWidth = (BarWidth - xPadding * (numChunks - 1)) / numChunks;
            var barSize = new Vector2(barWidth, BarHeight);
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset + BarHeight + yPadding;
            var cursorPos = new Vector2(xPos - xPadding - barWidth, yPos);

            var drawList = ImGui.GetWindowDrawList();

            // Meditation Stacks
            for (var i = 1; i < 4; i++)
            {
                cursorPos = new Vector2(cursorPos.X + xPadding + barWidth, cursorPos.Y);

                if (gauge.MeditationStacks >= i)
                {
                    drawList.AddRectFilled(
                        cursorPos, cursorPos + barSize,
                        ImGui.ColorConvertFloat4ToU32(new Vector4(247 / 255f, 163 / 255f, 89 / 255f, 255f))
                    );
                }
                else
                {
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                }

                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            }
        }
    }
}