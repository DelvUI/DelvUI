using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using ImGuiNET;
using ImGuiScene;
using DelvUIPlugin.Interface;

namespace DelvUIPlugin {
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Plugin : IDalamudPlugin {
        public string Name => "DelvUI";

        private DalamudPluginInterface _pluginInterface;
        private PluginConfiguration _pluginConfiguration;
        private HudWindow _hudWindowWindow;
        private ConfigurationWindow _configurationWindow;

        private bool _fontBuilt;
        private bool _fontLoadFailed;

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public string AssemblyLocation { get; set; } = Assembly.GetExecutingAssembly().Location;

        public void Initialize(DalamudPluginInterface pluginInterface) {
            _pluginInterface = pluginInterface;
            _pluginConfiguration = pluginInterface.GetPluginConfig() as PluginConfiguration ?? new PluginConfiguration();

            _pluginConfiguration.Init(_pluginInterface);

            // Pull this out of Initialize and instantiate hud class based on current job
            _hudWindowWindow = new DarkKnightHudWindow(_pluginInterface, _pluginConfiguration);
            _configurationWindow = new ConfigurationWindow(this, _pluginConfiguration);

            LoadTextures();

            _pluginInterface.CommandManager.AddHandler("/pdelvui", new CommandInfo(PluginCommand) {HelpMessage = "Opens configuration window", ShowInHelp = true});

            _pluginInterface.UiBuilder.OnBuildUi += BuildUi;
            _pluginInterface.UiBuilder.OnBuildFonts += BuildFont;
            _pluginInterface.UiBuilder.OnOpenConfigUi += OpenConfigUi;
        }
        
        private void BuildFont() {
            var fontFile = Path.Combine(Path.GetDirectoryName(AssemblyLocation) ?? "", "Media", "Fonts", "big-noodle-too.ttf");
            _fontBuilt = false;
            
            if (File.Exists(fontFile)) {
                try {
                    _pluginConfiguration.BigNoodleTooFont = ImGui.GetIO().Fonts.AddFontFromFileTTF(fontFile, 24);
                    _fontBuilt = true;
                } catch (Exception ex) {
                    PluginLog.Log($"Font failed to load. {fontFile}");
                    PluginLog.Log(ex.ToString());
                    _fontLoadFailed = true;
                }
            } else {
                PluginLog.Log($"Font doesn't exist. {fontFile}");
                _fontLoadFailed = true;
            }
        }

        private void LoadTextures() {
            _pluginConfiguration.HealthBarImage = LoadTexture("HealthBar.png");
            _pluginConfiguration.HealthBarBackgroundImage = LoadTexture("HealthBarBG.png");
            _pluginConfiguration.PrimaryBarImage = LoadTexture("PrimaryBar.png");
            _pluginConfiguration.PrimaryBarBackgroundImage = LoadTexture("PrimaryBarBG.png");
            _pluginConfiguration.SecondaryBarImage = LoadTexture("SecondaryBar.png");
            _pluginConfiguration.SecondaryBarBackgroundImage = LoadTexture("SecondaryBarBG.png");
            _pluginConfiguration.SecondaryBarDimImage = LoadTexture("SecondaryBarDim.png");
            _pluginConfiguration.TargetBarImage = LoadTexture("TargetBar.png");
            _pluginConfiguration.TargetBarBackgroundImage = LoadTexture("TargetBarBG.png");
        }
        
        // ReSharper disable once MemberCanBePrivate.Global
        public TextureWrap LoadTexture(string fileName) {
            var filePath = Path.Combine($@"{Path.GetDirectoryName(AssemblyLocation)}\Media\Textures", $@"{fileName}");

            if (!File.Exists(filePath)) {
                PluginLog.Error($"{filePath} was not found.");
                return null;
            }

            var texture = _pluginInterface.UiBuilder.LoadImage(filePath);
            if (texture == null) {
                PluginLog.Error($"Failed to load {filePath}.");
            }

            return texture;
        }

        private void PluginCommand(string command, string arguments) {
            _configurationWindow.IsVisible = !_configurationWindow.IsVisible;
        }

        private void BuildUi() {
            if (!_fontBuilt && !_fontLoadFailed) {
                _pluginInterface.UiBuilder.RebuildFonts();
                return;
            }
            
            if (_fontBuilt) ImGui.PushFont(_pluginConfiguration.BigNoodleTooFont);

            _configurationWindow.Draw();
            _hudWindowWindow.Draw();
            
            if (_fontBuilt) ImGui.PopFont();
        }

        private void OpenConfigUi(object sender, EventArgs e) {
            _configurationWindow.IsVisible = !_configurationWindow.IsVisible;
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposing) {
                return;
            }

            _configurationWindow.IsVisible = false;
            _hudWindowWindow.IsVisible = false;

            DisposeTextures();

            _pluginInterface.CommandManager.RemoveHandler("/pdelvui");
            _pluginInterface.UiBuilder.OnBuildUi -= BuildUi;
            _pluginInterface.UiBuilder.OnBuildFonts -= BuildFont;
            _pluginInterface.UiBuilder.OnOpenConfigUi -= OpenConfigUi;
            _pluginInterface.UiBuilder.RebuildFonts();
        }

        private void DisposeTextures() {
            _pluginConfiguration.HealthBarImage?.Dispose();
            _pluginConfiguration.HealthBarBackgroundImage?.Dispose();
            _pluginConfiguration.PrimaryBarImage?.Dispose();
            _pluginConfiguration.PrimaryBarBackgroundImage?.Dispose();
            _pluginConfiguration.SecondaryBarImage?.Dispose();
            _pluginConfiguration.SecondaryBarBackgroundImage?.Dispose();
            _pluginConfiguration.SecondaryBarDimImage?.Dispose();
            _pluginConfiguration.TargetBarImage?.Dispose();
            _pluginConfiguration.TargetBarBackgroundImage?.Dispose();
        }
        
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}