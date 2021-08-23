using Dalamud.Plugin;
using ImGuiNET;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Gui;
using Dalamud.Interface;
using DelvUI.Config;

namespace DelvUI.Interface
{
    public class LandHudWindow : HudWindow
    {

        public LandHudWindow(
            ClientState clientState,
            DalamudPluginInterface pluginInterface,
            DataManager dataManager,
            Framework framework,
            GameGui gameGui,
            JobGauges jobGauges,
            ObjectTable objectTable, 
            PluginConfiguration pluginConfiguration,
            SigScanner sigScanner,
            TargetManager targetManager,
            UiBuilder uiBuilder
        ) : base(
            clientState,
            pluginInterface,
            dataManager,
            framework,
            gameGui,
            jobGauges,
            objectTable,
            pluginConfiguration,
            sigScanner,
            targetManager,
            uiBuilder
        ) {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            JobId = ClientState.LocalPlayer.ClassJob.Id;
        }
        
        public override uint JobId { get; }

        protected override void Draw(bool _) {
        }

        protected override void DrawPrimaryResourceBar() {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var actor = ClientState.LocalPlayer;
            var barSize = new Vector2(PrimaryResourceBarWidth, PrimaryResourceBarHeight);
            var scale = (float) actor.CurrentGp / actor.MaxGp;
            var cursorPos = new Vector2(CenterX - PrimaryResourceBarXOffset + 33, CenterY + PrimaryResourceBarYOffset - 16);

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * scale, barSize.Y),
                0xFFE6CD00, 0xFFD8Df3C, 0xFFD8Df3C, 0xFFE6CD00
            );
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            if (ShowPrimaryResourceBarThresholdMarker)
            {
                // threshold
                var position = new Vector2(cursorPos.X + (PrimaryResourceBarThresholdValue / 10000f) * barSize.X - 3, cursorPos.Y);
                var size = new Vector2(2, barSize.Y);
                drawList.AddRect(position, position + size, 0xFF000000);
            }

            if (!ShowPrimaryResourceBarValue) return;

            // text
            var currentGp = ClientState.LocalPlayer.CurrentGp;
            var text = $"{currentGp,0}";
            DrawOutlinedText(text, new Vector2(cursorPos.X + 2 + PrimaryResourceBarTextXOffset, cursorPos.Y - 3 + PrimaryResourceBarTextYOffset));
        }
    }
}
