using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUI.Interface {
    public class GunbreakerHudWindow : HudWindow {
        public override uint JobId => 37;

        private static int BarHeight => 13;
        private static int BarWidth => 254;
        private new static int XOffset => 127;
        private new static int YOffset => 479;
        
        public GunbreakerHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _) {
            DrawPowderGauge();
        }
        protected override void DrawPrimaryResourceBar()
        {
        }

        private void DrawPowderGauge() {
            var gauge = PluginInterface.ClientState.JobGauges.Get<GNBGauge>();
            const uint powderColor = 0xFFFEAD43;

            const int xPadding = 2;
            var barWidth = (BarWidth - xPadding * 2)  / 2;
            var barSize = new Vector2(barWidth, BarHeight);
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset - 33;
            var cursorPos = new Vector2(xPos, yPos);

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            if (gauge.NumAmmo > 0) {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barSize.X, barSize.Y), 
                    powderColor, powderColor, powderColor, powderColor
                );
            }
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
            
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            if(gauge.NumAmmo > 1) {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barSize.X, barSize.Y), 
                    powderColor, powderColor, powderColor, powderColor
                );
            }
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }
    }
}