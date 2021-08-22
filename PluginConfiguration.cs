using Dalamud.Configuration;
using Dalamud.Plugin;
using ImGuiNET;
using ImGuiScene;
using Newtonsoft.Json;

namespace DelvUIPlugin {
    public class PluginConfiguration : IPluginConfiguration {
        public int Version { get; set; }

        public bool HideHud = false;
        public bool HideCombat = false;
        
        [JsonIgnore] private DalamudPluginInterface _pluginInterface;
        [JsonIgnore] public ImFontPtr BigNoodleTooFont = null;

        public void Init(DalamudPluginInterface pluginInterface) {
            _pluginInterface = pluginInterface;
        }

        public void Save() {
            _pluginInterface.SavePluginConfig(this);
        }
    }
}