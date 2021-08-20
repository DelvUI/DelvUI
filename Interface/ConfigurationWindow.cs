using System.Numerics;
using ImGuiNET;

namespace DelvUIPlugin.Interface {
    public class ConfigurationWindow {
        public bool IsVisible;
        private readonly Plugin _plugin;
        private readonly PluginConfiguration _pluginConfiguration;

        public ConfigurationWindow(Plugin plugin, PluginConfiguration pluginConfiguration) {
            _plugin = plugin;
            _pluginConfiguration = pluginConfiguration;
        }

        public void Draw() {
            if (!IsVisible) {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(0, 0), ImGuiCond.Always);

            if (ImGui.Begin("DelvUI configuration", ref IsVisible, ImGuiWindowFlags.NoCollapse)) {
                var changed = false;
                changed |= ImGui.Checkbox("Hide HUD", ref _pluginConfiguration.HideHud);

                if (changed) {
                    _pluginConfiguration.Save();
                }

                ImGui.End();
            }
        }
    }
}