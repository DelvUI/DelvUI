using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using ImGuiNET;

namespace DelvUI.Interface.GeneralElements
{
    public enum WindowClippingMode
    {
        Full,
        Hide,
        Performance
    }

    [Exportable(false)]
    [Disableable(false)]
    [Section("Misc")]
    [SubSection("Window Clipping", 0)]
    public class WindowClippingConfig : PluginConfigObject
    {
        public new static WindowClippingConfig DefaultConfig() => new WindowClippingConfig();

        public WindowClippingMode Mode = WindowClippingMode.Full;

        public bool NameplatesClipRectsEnabled = true;
        public bool TargetCastbarClipRectEnabled = false;
        public bool HotbarsClipRectsEnabled = false;
        public bool ChatBubblesClipRectsEnabled = true;

        public bool ThirdPartyClipRectsEnabled = true;

        private bool _showConfirmationDialog = false;

        [ManualDraw]
        public bool Draw(ref bool changed)
        {
            ImGuiHelper.NewLineAndTab();

            if (ImGui.Checkbox("Enabled", ref Enabled))
            {
                if (Enabled)
                {
                    Enabled = false;
                    _showConfirmationDialog = true;
                }
                else
                {
                    changed = true;
                }
            }

            // confirmation dialog
            if (_showConfirmationDialog)
            {
                string[] lines = new string[] { "THIS FEATURE IS KNOWN TO CAUSE RANDOM", "CRASHES TO A SMALL PORTION OF USERS!!!", "Are you sure you want to enable it?" };
                var (didConfirm, didClose) = ImGuiHelper.DrawConfirmationModal("WARNING!", lines);

                if (didConfirm)
                {
                    Enabled = true;
                    changed = true;
                }

                if (didConfirm || didClose)
                {
                    _showConfirmationDialog = false;
                }
            }

            if (!Enabled) { return changed; }

            // mode
            ImGuiHelper.NewLineAndTab();
            ImGui.SameLine();
            ImGui.Text("Mode: ");

            ImGui.SameLine();
            if (ImGui.RadioButton("Full", Mode == WindowClippingMode.Full))
            {
                Mode = WindowClippingMode.Full;
            }

            ImGui.SameLine();
            if (ImGui.RadioButton("Hide", Mode == WindowClippingMode.Hide))
            {
                Mode = WindowClippingMode.Hide;
            }

            ImGui.SameLine();
            if (ImGui.RadioButton("Performance", Mode == WindowClippingMode.Performance))
            {
                Mode = WindowClippingMode.Performance;
            }

            // nameplates
            ImGui.NewLine();
            ImGuiHelper.NewLineAndTab();
            changed |= ImGui.Checkbox("Enable special clipping for Nameplates", ref NameplatesClipRectsEnabled);
            ImGuiHelper.SetTooltip("When enabled, Nameplates will get covered by game UI elements that wouldn't normally cover DelvUI elements.");

            if (NameplatesClipRectsEnabled)
            {
                ImGuiHelper.Tab(); ImGuiHelper.Tab();
                changed |= ImGui.Checkbox("Default Target Castbar", ref TargetCastbarClipRectEnabled);
                ImGuiHelper.SetTooltip("When enabled, the game's target castbar will not be covered by DelvUI Nameplates.\nFor players that prefer to use the default target cast bar over DelvUI's.");

                ImGuiHelper.Tab(); ImGuiHelper.Tab();
                changed |= ImGui.Checkbox("Hotbars", ref HotbarsClipRectsEnabled);
                ImGuiHelper.SetTooltip("When enabled, active hotbar will not be covered by DelvUI Nameplates.\nNote that the way this is calculated is not perfect and it might not work well for hotbars that have empty slots.");

                ImGuiHelper.Tab(); ImGuiHelper.Tab();
                changed |= ImGui.Checkbox("Chat Bubbles", ref ChatBubblesClipRectsEnabled);
            }

            // third party
            ImGui.NewLine();
            ImGuiHelper.NewLineAndTab();
            changed |= ImGui.Checkbox("Enable clipping for other plugins", ref ThirdPartyClipRectsEnabled);
            ImGuiHelper.SetTooltip("When enabled, other plugins' windows can also be clipped so DelvUI elements don't cover them.\nPlease note that this requires the developer of each third party plugin to implement the feature.");

            // text
            ImGui.NewLine();
            ImGuiHelper.NewLineAndTab();
            ImGui.SameLine();

            switch (Mode)
            {
                case WindowClippingMode.Full:
                    ImGui.Text("DelvUI will attempt to not cover game windows in this mode by clipping around them.");
                    break;

                case WindowClippingMode.Hide:
                    ImGui.Text("DelvUI will attempt to not cover game windows in this mode by not drawing an element if its touching a game window.");
                    break;

                case WindowClippingMode.Performance:
                    ImGui.Text("Window Clipping functionallity will be reduced in favor of performance.\nOnly one game window will be clipped at a time. This might yield unexpected / ugly results.\n\nNote: This mode won't work well with Nameplates.");
                    break;
            }

            ImGuiHelper.NewLineAndTab();
            ImGui.Text("If you're exepriencing random crashes or bad performance, we recommend you try a different mode\nor disable Window Clipping alltogether");

            return false;
        }
    }
}
