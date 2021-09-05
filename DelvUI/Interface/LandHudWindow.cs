using Dalamud.Plugin;
using DelvUI.Config;
using ImGuiNET;
using System.Diagnostics;
using System.Numerics;

namespace DelvUI.Interface
{
    public class LandHudWindow : HudWindow
    {
        public LandHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) :
            base(pluginInterface, pluginConfiguration)
        {
            JobId = pluginInterface.ClientState.LocalPlayer.ClassJob.Id;
        }

        public override uint JobId { get; }

        protected override void Draw(bool _) { }

        protected override void DrawPrimaryResourceBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var barSize = new Vector2(PrimaryResourceBarWidth, PrimaryResourceBarHeight);
            var actor = PluginInterface.ClientState.LocalPlayer;
            var scale = (float)actor.CurrentGp / actor.MaxGp;
            var cursorPos = new Vector2(CenterX - PrimaryResourceBarXOffset + 33, CenterY + PrimaryResourceBarYOffset - 16);

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);

            drawList.AddRectFilledMultiColor(
                cursorPos,
                cursorPos + new Vector2(barSize.X * scale, barSize.Y),
                0xFFE6CD00,
                0xFFD8Df3C,
                0xFFD8Df3C,
                0xFFE6CD00
            );

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            if (ShowPrimaryResourceBarThresholdMarker)
            {
                // threshold
                var position = new Vector2(cursorPos.X + PrimaryResourceBarThresholdValue / 10000f * barSize.X - 3, cursorPos.Y);
                var size = new Vector2(2, barSize.Y);
                drawList.AddRect(position, position + size, 0xFF000000);
            }

            if (!ShowPrimaryResourceBarValue)
            {
                return;
            }

            // text
            var currentGp = PluginInterface.ClientState.LocalPlayer.CurrentGp;
            var text = $"{currentGp,0}";
            DrawOutlinedText(text, new Vector2(cursorPos.X + 2 + PrimaryResourceBarTextXOffset, cursorPos.Y - 3 + PrimaryResourceBarTextYOffset));
        }
    }
}
