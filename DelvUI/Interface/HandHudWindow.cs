using Dalamud.Plugin;
using DelvUI.Config;
using ImGuiNET;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Game.ClientState.Actors.Types;

namespace DelvUI.Interface
{
    public class HandHudWindow : HudWindow
    {
        public HandHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) :
            base(pluginInterface, pluginConfiguration)
        {
            JobId = pluginInterface.ClientState.LocalPlayer.ClassJob.Id;
        }

        public override uint JobId { get; }

        protected override void Draw(bool _) { }

        protected override void DrawPrimaryResourceBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            Vector2 barSize = new Vector2(PrimaryResourceBarWidth, PrimaryResourceBarHeight);
            PlayerCharacter actor = PluginInterface.ClientState.LocalPlayer;
            var scale = (float) actor.CurrentCp / actor.MaxCp;
            Vector2 cursorPos = new Vector2(CenterX - PrimaryResourceBarXOffset + 33, CenterY + PrimaryResourceBarYOffset - 16);

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
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
                Vector2 position = new Vector2(cursorPos.X + PrimaryResourceBarThresholdValue / 10000f * barSize.X - 3, cursorPos.Y);
                Vector2 size = new Vector2(2, barSize.Y);
                drawList.AddRect(position, position + size, 0xFF000000);
            }

            if (!ShowPrimaryResourceBarValue)
            {
                return;
            }

            // text
            var currentCp = PluginInterface.ClientState.LocalPlayer.CurrentCp;
            var text = $"{currentCp,0}";
            DrawOutlinedText(text, new Vector2(cursorPos.X + 2 + PrimaryResourceBarTextXOffset, cursorPos.Y - 3 + PrimaryResourceBarTextYOffset));
        }
    }
}
