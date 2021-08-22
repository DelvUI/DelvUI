using System.Numerics;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUIPlugin.Interface {
    public class ConfigurationWindow {
        public bool IsVisible = false;
        private readonly Plugin _plugin;
        private readonly DalamudPluginInterface _pluginInterface;
        private readonly PluginConfiguration _pluginConfiguration;

        public ConfigurationWindow(Plugin plugin, DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) {
            _plugin = plugin;
            _pluginInterface = pluginInterface;
            _pluginConfiguration = pluginConfiguration;
        }

        public void Draw() {
            if (!IsVisible) {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(0, 0), ImGuiCond.Always);

            if (!ImGui.Begin("DelvUI configuration", ref IsVisible, ImGuiWindowFlags.NoCollapse)) {
                return;
            }

            var changed = false;
            changed |= ImGui.Checkbox("Hide HUD", ref _pluginConfiguration.HideHud);
            
            changed |= ImGui.ColorEdit4("Job Color PLD", ref _pluginConfiguration.JobColorPLD);
            changed |= ImGui.ColorEdit4("Job Color WAR", ref _pluginConfiguration.JobColorWAR);
            changed |= ImGui.ColorEdit4("Job Color DRK", ref _pluginConfiguration.JobColorDRK);
            changed |= ImGui.ColorEdit4("Job Color GNB", ref _pluginConfiguration.JobColorGNB);
            changed |= ImGui.ColorEdit4("Job Color WHM", ref _pluginConfiguration.JobColorWHM);
            changed |= ImGui.ColorEdit4("Job Color AST", ref _pluginConfiguration.JobColorAST);
            changed |= ImGui.ColorEdit4("Job Color SCH", ref _pluginConfiguration.JobColorSCH);
            changed |= ImGui.ColorEdit4("Job Color MNK", ref _pluginConfiguration.JobColorMNK);
            changed |= ImGui.ColorEdit4("Job Color DRG", ref _pluginConfiguration.JobColorDRG);
            changed |= ImGui.ColorEdit4("Job Color NIN", ref _pluginConfiguration.JobColorNIN);
            changed |= ImGui.ColorEdit4("Job Color SAM", ref _pluginConfiguration.JobColorSAM);
            changed |= ImGui.ColorEdit4("Job Color BRD", ref _pluginConfiguration.JobColorBRD);
            changed |= ImGui.ColorEdit4("Job Color MCH", ref _pluginConfiguration.JobColorMCH);
            changed |= ImGui.ColorEdit4("Job Color DNC", ref _pluginConfiguration.JobColorDNC);
            changed |= ImGui.ColorEdit4("Job Color BLM", ref _pluginConfiguration.JobColorBLM);
            changed |= ImGui.ColorEdit4("Job Color SMN", ref _pluginConfiguration.JobColorSMN);
            changed |= ImGui.ColorEdit4("Job Color RDM", ref _pluginConfiguration.JobColorRDM);
            changed |= ImGui.ColorEdit4("Job Color BLU", ref _pluginConfiguration.JobColorBLU);
            
            changed |= ImGui.ColorEdit4("NPC Color Hostile", ref _pluginConfiguration.NPCColorHostile);
            changed |= ImGui.ColorEdit4("NPC Color Neutral", ref _pluginConfiguration.NPCColorNeutral);
            changed |= ImGui.ColorEdit4("NPC Color Friendly", ref _pluginConfiguration.NPCColorFriendly);

            if (changed) {
                _pluginConfiguration.BuildColorMap();
                _pluginConfiguration.Save();
            }

            ImGui.End();
        }
    }
}