using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUI.Interface
{
    public class GunbreakerHudWindow : HudWindow {
        public override uint JobId => 37;

        private new int XOffset => PluginConfiguration.GNBHorizontalOffset;
        private new int YOffset => PluginConfiguration.GNBVerticalOffset;

        private bool CartridgeBarEnabled => PluginConfiguration.GNBCartridgeBarEnabled;
        private int CartridgeBarHeight => PluginConfiguration.GNBCartridgeBarHeight;
        private int CartridgeBarWidth => PluginConfiguration.GNBCartridgeBarWidth;
        public int CartridgeBarPadding => PluginConfiguration.GNBCartridgeBarPadding;

        private Dictionary<string, uint> CartridgeColorLeft => PluginConfiguration.JobColorMap[Jobs.GNB * 1000];
        private Dictionary<string, uint> CartridgeColorRight => PluginConfiguration.JobColorMap[Jobs.GNB * 1000 + 1];

        public GunbreakerHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _) {
            if (CartridgeBarEnabled)
            {
                DrawPowderGauge();
            }
        }
        
        protected override void DrawPrimaryResourceBar()
        {
        }

        private void DrawPowderGauge() {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");

            var gauge = PluginInterface.ClientState.JobGauges.Get<GNBGauge>();

            var barWidth = (CartridgeBarWidth - CartridgeBarPadding * 2)  / 2;
            var barSize = new Vector2(barWidth, CartridgeBarHeight);
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset - 33;
            var cursorPos = new Vector2(xPos, yPos);

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            if (gauge.NumAmmo > 0) {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barSize.X, barSize.Y),
                    CartridgeColorLeft["gradientLeft"], CartridgeColorLeft["gradientRight"], CartridgeColorLeft["gradientRight"], CartridgeColorLeft["gradientLeft"]
                );
            }
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            cursorPos = new Vector2(cursorPos.X + barWidth + CartridgeBarPadding, cursorPos.Y);
            
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            if(gauge.NumAmmo > 1) {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barSize.X, barSize.Y),
                    CartridgeColorRight["gradientLeft"], CartridgeColorRight["gradientRight"], CartridgeColorRight["gradientRight"], CartridgeColorRight["gradientLeft"]
                );
            }
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }
    }
}