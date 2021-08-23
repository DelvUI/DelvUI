using System;
using System.IO;
using System.Reflection;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface;
using Dalamud.Logging;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Interface;
using FFXIVClientStructs;
using DelvUI.Helpers; 
using ImGuiNET;
using SigScanner = Dalamud.Game.SigScanner;

namespace DelvUI
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "DelvUI";

        private readonly ClientState _clientState;
        private readonly CommandManager _commandManager;
        private readonly Condition _condition;
        private readonly ConfigurationWindow _configurationWindow;
        private readonly DalamudPluginInterface _pluginInterface;
        private readonly DataManager _dataManager;
        private readonly Framework _framework;
        private readonly GameGui _gameGui;
        private readonly JobGauges _jobGauges;
        private readonly ObjectTable _objectTable;
        private readonly SigScanner _sigScanner;
        private readonly PluginConfiguration _pluginConfiguration;
        private readonly TargetManager _targetManager;
        private readonly UiBuilder _uiBuilder;

        private HudWindow _hudWindow;

        private bool _fontBuilt;
        private bool _fontLoadFailed;

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public string AssemblyLocation { get; set; } = Assembly.GetExecutingAssembly().Location;

        public Plugin(
            ClientState clientState,
            CommandManager commandManager,
            Condition condition,
            DalamudPluginInterface pluginInterface,
            DataManager dataManager,
            Framework framework,
            GameGui gameGui,
            JobGauges jobGauges,
            ObjectTable objectTable,
            SigScanner sigScanner,
            TargetManager targetManager,
            UiBuilder uiBuilder
        ) {
            _clientState = clientState;
            _commandManager = commandManager;
            _condition = condition;
            _dataManager = dataManager;
            _framework = framework;
            _gameGui = gameGui;
            _jobGauges = jobGauges;
            _objectTable = objectTable;
            _sigScanner = sigScanner;
            _pluginInterface = pluginInterface;
            _targetManager = targetManager;
            _uiBuilder = uiBuilder;

            // load a configuration with default parameters and write it to file
            _pluginConfiguration = new PluginConfiguration();
            PluginConfiguration.WriteConfig("default", _pluginInterface, _pluginConfiguration);

            // if a previously used configuration exists, use it instead
            var oldConfiguration = PluginConfiguration.ReadConfig(this.Name, _pluginInterface);

            if (oldConfiguration != null) {
                _pluginConfiguration = oldConfiguration;
            }

            _pluginConfiguration.Init(_pluginInterface);
            _configurationWindow = new ConfigurationWindow(_pluginConfiguration, _pluginInterface);

            BuildBanner();
            _pluginInterface.UiBuilder.Draw += Draw;
            _pluginInterface.UiBuilder.BuildFonts += BuildFont;
            _pluginInterface.UiBuilder.OpenConfigUi += OpenConfigUi;

            if (!_fontBuilt && !_fontLoadFailed) {
                _pluginInterface.UiBuilder.RebuildFonts();
            }

            _commandManager.AddHandler(
                "/pdelvui", new CommandInfo(PluginCommand)
                {
                    HelpMessage = (
                        "Opens the DelvUI configuration window.\n" +
                        "/pdelvui toggle → Toggles HUD visibility.\n" +
                        "/pdelvui show → Shows HUD.\n" +
                        "/pdelvui hide → Hides HUD."
                    ),
                    ShowInHelp = true
                }
            );

            TexturesCache.Initialize(_dataManager, _uiBuilder);
            Resolver.Initialize();
        }

        private void BuildFont() {
            var fontFile = Path.Combine(Path.GetDirectoryName(AssemblyLocation) ?? "", "Media", "Fonts", "big-noodle-too.ttf");
            _fontBuilt = false;

            if (File.Exists(fontFile)) {
                try {
                    _pluginConfiguration.BigNoodleTooFont = ImGui.GetIO().Fonts.AddFontFromFileTTF(fontFile, 24);
                    _fontBuilt = true;
                }
                catch (Exception ex) {
                    PluginLog.Log($"Font failed to load. {fontFile}");
                    PluginLog.Log(ex.ToString());
                    _fontLoadFailed = true;
                }
            }
            else {
                PluginLog.Log($"Font doesn't exist. {fontFile}");
                _fontLoadFailed = true;
            }
        }

        private void BuildBanner() {
            var bannerImage = Path.Combine(Path.GetDirectoryName(AssemblyLocation) ?? "", "Media", "Images", "banner_short_x150.png");

            if (File.Exists(bannerImage)) {
                try {
                    _pluginConfiguration.BannerImage = _pluginInterface.UiBuilder.LoadImage(bannerImage);
                }
                catch (Exception ex) {
                    PluginLog.Log($"Image failed to load. {bannerImage}");
                    PluginLog.Log(ex.ToString());
                }
            }
            else {
                PluginLog.Log($"Image doesn't exist. {bannerImage}");
            }
        }

        private void PluginCommand(string command, string arguments) {
            switch (arguments) {
                case "toggle":
                    _configurationWindow.ToggleHud();

                    break;

                case "show":
                    _configurationWindow.ShowHud();

                    break;

                case "hide":
                    _configurationWindow.HideHud();

                    break;

                default:
                    _configurationWindow.IsVisible = !_configurationWindow.IsVisible;

                    break;
            }
        }

        private void Draw() {

            var hudState = _condition[ConditionFlag.WatchingCutscene]
                           || _condition[ConditionFlag.WatchingCutscene78]
                           || _condition[ConditionFlag.OccupiedInCutSceneEvent]
                           || _condition[ConditionFlag.CreatingCharacter]
                           || _condition[ConditionFlag.BetweenAreas]
                           || _condition[ConditionFlag.BetweenAreas51];

            _pluginInterface.UiBuilder.OverrideGameCursor = false;

            _configurationWindow.Draw();

            if (_hudWindow?.JobId != _clientState.LocalPlayer?.ClassJob.Id) {
                SwapJobs();
            }

            if (_fontBuilt) {
                ImGui.PushFont(_pluginConfiguration.BigNoodleTooFont);
            }


            if (!hudState) {
                _hudWindow?.Draw();
            }

            if (_fontBuilt) {
                ImGui.PopFont();
            }
        }

        private void SwapJobs() {
            _hudWindow = _clientState.LocalPlayer?.ClassJob.Id switch
            {
                //Tanks
                Jobs.DRK => new DarkKnightHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.GNB => new GunbreakerHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.WAR => new WarriorHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.PLD => new PaladinHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),

                //Healers
                Jobs.WHM => new WhiteMageHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.SCH => new ScholarHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.AST => new AstrologianHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),

                //Melee DPS
                Jobs.DRG => new DragoonHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.SAM => new SamuraiHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.MNK => new MonkHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.NIN => new NinjaHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),

                //Ranged DPS
                Jobs.BRD => new BardHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.DNC => new DancerHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.MCH => new MachinistHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),

                //Caster DPS
                Jobs.RDM => new RedMageHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.SMN => new SummonerHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.BLM => new BlackMageHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder, _pluginConfiguration.BLMConfig),

                //Low jobs
                Jobs.MRD => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.GLD => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.CNJ => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.PGL => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.LNC => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.ROG => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.ARC => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.THM => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.ACN => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),

                //Hand
                Jobs.CRP => new HandHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.BSM => new HandHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.ARM => new HandHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.GSM => new HandHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.LTW => new HandHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.WVR => new HandHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.ALC => new HandHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.CUL => new HandHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),

                //Land
                Jobs.MIN => new LandHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.BOT => new LandHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                Jobs.FSH => new LandHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),

                //dont have packs yet
                Jobs.BLU => new UnitFrameOnlyHudWindow(_clientState, _pluginInterface, _dataManager, _framework, _gameGui, _jobGauges, _objectTable, _pluginConfiguration, _sigScanner, _targetManager, _uiBuilder),
                _ => _hudWindow
            };
        }

        private void OpenConfigUi() {
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

            _commandManager.RemoveHandler("/pdelvui");
            _pluginInterface.UiBuilder.Draw -= Draw;
            _pluginInterface.UiBuilder.BuildFonts -= BuildFont;
            _pluginInterface.UiBuilder.OpenConfigUi -= OpenConfigUi;
            _pluginInterface.UiBuilder.RebuildFonts();
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}