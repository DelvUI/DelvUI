﻿using System;
using System.IO;
using System.Reflection;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using ImGuiNET;
using DelvUIPlugin.Interface;

namespace DelvUIPlugin {
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Plugin : IDalamudPlugin {
        public string Name => "DelvUI";

        private DalamudPluginInterface _pluginInterface;
        private PluginConfiguration _pluginConfiguration;
        private HudWindow _hudWindow;
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
            _configurationWindow = new ConfigurationWindow(this, _pluginInterface, _pluginConfiguration);

            _pluginInterface.CommandManager.AddHandler("/pdelvui", new CommandInfo(PluginCommand)
            {
                HelpMessage = "Opens the DelvUI configuration window.", 
                ShowInHelp = true
            });

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

        private void PluginCommand(string command, string arguments) {
            _configurationWindow.IsVisible = !_configurationWindow.IsVisible;
        }

        private void BuildUi() {
            _pluginInterface.UiBuilder.OverrideGameCursor = false;
            
            if (!_fontBuilt && !_fontLoadFailed) {
                _pluginInterface.UiBuilder.RebuildFonts();
                return;
            }
            
            if (_fontBuilt) ImGui.PushFont(_pluginConfiguration.BigNoodleTooFont);

            if (_hudWindow?.JobId != _pluginInterface.ClientState.LocalPlayer?.ClassJob.Id) {
                SwapJobs();
            }

            _configurationWindow.Draw();
            _hudWindow?.Draw();

            if (_fontBuilt) {
                ImGui.PopFont();
            }
        }

        private void SwapJobs() {
            _hudWindow = _pluginInterface.ClientState.LocalPlayer?.ClassJob.Id switch
            {
                Jobs.WHM => new WhiteMageHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.DRK => new DarkKnightHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.DNC => new DancerHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.SAM => new SamuraiHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.BRD => new BardHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.RDM => new RedMageHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.SMN => new SummonerHudWindow(_pluginInterface, _pluginConfiguration),
                _ => _hudWindow
            };
        }
        
        private void OpenConfigUi(object sender, EventArgs e) {
            _configurationWindow.IsVisible = !_configurationWindow.IsVisible;
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposing) {
                return;
            }

            _configurationWindow.IsVisible = false;

            if (_hudWindow != null) {
                _hudWindow.IsVisible = false;
            }

            _pluginInterface.CommandManager.RemoveHandler("/pdelvui");
            _pluginInterface.UiBuilder.OnBuildUi -= BuildUi;
            _pluginInterface.UiBuilder.OnBuildFonts -= BuildFont;
            _pluginInterface.UiBuilder.OnOpenConfigUi -= OpenConfigUi;
            _pluginInterface.UiBuilder.RebuildFonts();
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}