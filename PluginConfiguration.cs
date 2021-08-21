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
        [JsonIgnore] public TextureWrap HealthBarImage = null;
        [JsonIgnore] public TextureWrap HealthBarBackgroundImage = null;
        [JsonIgnore] public TextureWrap PrimaryBarImage = null;
        [JsonIgnore] public TextureWrap PrimaryBarBackgroundImage = null;
        [JsonIgnore] public TextureWrap SecondaryBarImage = null;
        [JsonIgnore] public TextureWrap SecondaryBarBackgroundImage = null;
        [JsonIgnore] public TextureWrap SecondaryBarDimImage = null;
        [JsonIgnore] public TextureWrap TargetBarImage = null;
        [JsonIgnore] public TextureWrap TargetBarBackgroundImage = null;
        [JsonIgnore] public TextureWrap BarBorder = null;
        [JsonIgnore] public ImFontPtr BigNoodleTooFont = null;

        public void Init(DalamudPluginInterface pluginInterface) {
            _pluginInterface = pluginInterface;
        }

        public void Save() {
            _pluginInterface.SavePluginConfig(this);
        }
    }
}