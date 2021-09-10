using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface;
using DelvUI.Interface.GeneralElements;
using FFXIVClientStructs;
using ImGuiNET;
using System;
using System.IO;
using System.Reflection;

namespace DelvUI
{
    public class Plugin : IDalamudPlugin
    {
        private bool _fontBuilt;
        private bool _fontLoadFailed;
        private HudWindow _hudWindow;
        private HudManager _hudManager;
        private PluginConfiguration _pluginConfiguration;
        private SystemMenuHook _menuHook;

        private static DalamudPluginInterface _pluginInterface;

        public static ImGuiScene.TextureWrap bannerTexture;

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public string AssemblyLocation { get; set; } = Assembly.GetExecutingAssembly().Location;
        public string Name => "DelvUI";
        public static string Version = "";

        public static DalamudPluginInterface GetPluginInterface() => _pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            _pluginInterface = pluginInterface;

            Version = Assembly.GetExecutingAssembly()?.GetName().Version.ToString() ?? "";

            LoadBanner();

            // initialize a not-necessarily-defaults configuration
            ConfigurationManager.Initialize(false);

            // load a configuration with default parameters and write it to file
            _pluginConfiguration = new PluginConfiguration();
            PluginConfiguration.WriteConfig("default", _pluginConfiguration);

            // if a previously used configuration exists, use it instead
            var oldConfiguration = PluginConfiguration.ReadConfig(Name);

            if (oldConfiguration != null)
            {
                _pluginConfiguration = oldConfiguration;
            }

            _pluginConfiguration.BannerImage = bannerTexture;
            _pluginConfiguration.Init();

            _pluginInterface.UiBuilder.OnBuildUi += Draw;
            _pluginInterface.UiBuilder.OnBuildFonts += BuildFont;
            _pluginInterface.UiBuilder.OnOpenConfigUi += OpenConfigUi;

            if (!_fontBuilt && !_fontLoadFailed)
            {
                _pluginInterface.UiBuilder.RebuildFonts();
            }

            _pluginInterface.CommandManager.AddHandler(
                "/delvui",
                new CommandInfo(PluginCommand)
                {
                    HelpMessage = "Opens the DelvUI configuration window.\n"
                                + "/delvui toggle → Toggles HUD visibility.\n"
                                + "/delvui show → Shows HUD.\n"
                                + "/delvui hide → Hides HUD.",
                    ShowInHelp = true
                }
            );

            _menuHook = new SystemMenuHook(_pluginInterface);

            _pluginInterface.CommandManager.AddHandler("/delvuireloadconfig", new CommandInfo(ReloadConfigCommand));

            TexturesCache.Initialize(pluginInterface);
            GlobalColors.Initialize();
            Resolver.Initialize();

            _hudManager = new HudManager(_pluginInterface, _pluginConfiguration);
        }

        public void Dispose()
        {
            _menuHook.Dispose();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void BuildFont()
        {
            string fontFile = Path.Combine(Path.GetDirectoryName(AssemblyLocation) ?? "", "Media", "Fonts", "big-noodle-too.ttf");
            _fontBuilt = false;

            if (File.Exists(fontFile))
            {
                try
                {
                    _pluginConfiguration.BigNoodleTooFont = ImGui.GetIO().Fonts.AddFontFromFileTTF(fontFile, 24);
                    _fontBuilt = true;
                }
                catch (Exception ex)
                {
                    PluginLog.Log($"Font failed to load. {fontFile}");
                    PluginLog.Log(ex.ToString());
                    _fontLoadFailed = true;
                }
            }
            else
            {
                PluginLog.Log($"Font doesn't exist. {fontFile}");
                _fontLoadFailed = true;
            }
        }

        private void LoadBanner()
        {
            string bannerImage = Path.Combine(Path.GetDirectoryName(AssemblyLocation) ?? "", "Media", "Images", "banner_short_x150.png");

            if (File.Exists(bannerImage))
            {
                try
                {
                    bannerTexture = _pluginInterface.UiBuilder.LoadImage(bannerImage);
                }
                catch (Exception ex)
                {
                    PluginLog.Log($"Image failed to load. {bannerImage}");
                    PluginLog.Log(ex.ToString());
                }
            }
            else
            {
                PluginLog.Log($"Image doesn't exist. {bannerImage}");
            }
        }

        private void PluginCommand(string command, string arguments)
        {
            ConfigurationManager.GetInstance().DrawConfigWindow = !ConfigurationManager.GetInstance().DrawConfigWindow;

            //switch (arguments)
            //{
            //    case "toggle":
            //        _configurationWindow.ToggleHud();

            //        break;

            //    case "show":
            //        _configurationWindow.ShowHud();

            //        break;

            //    case "hide":
            //        _configurationWindow.HideHud();

            //        break;

            //    default:
            //        ConfigurationManager.GetInstance().DrawConfigWindow = false;

            //        break;
            //}
        }

        private void ReloadConfigCommand(string command, string arguments) { ConfigurationManager.GetInstance().LoadConfigurations(); }

        private void Draw()
        {
            bool hudState = _pluginInterface.ClientState.Condition[ConditionFlag.WatchingCutscene]
                         || _pluginInterface.ClientState.Condition[ConditionFlag.WatchingCutscene78]
                         || _pluginInterface.ClientState.Condition[ConditionFlag.OccupiedInCutSceneEvent]
                         || _pluginInterface.ClientState.Condition[ConditionFlag.CreatingCharacter]
                         || _pluginInterface.ClientState.Condition[ConditionFlag.BetweenAreas]
                         || _pluginInterface.ClientState.Condition[ConditionFlag.BetweenAreas51];

            _pluginInterface.UiBuilder.OverrideGameCursor = false;

            ConfigurationManager.GetInstance().Draw();

            if (_hudWindow?.JobId != _pluginInterface.ClientState.LocalPlayer?.ClassJob.Id)
            {
                SwapJobs();
            }

            if (_fontBuilt)
            {
                ImGui.PushFont(_pluginConfiguration.BigNoodleTooFont);
            }

            if (!hudState)
            {
                _hudWindow?.Draw();
                _hudManager?.Draw();
            }

            if (_fontBuilt)
            {
                ImGui.PopFont();
            }
        }

        private void SwapJobs()
        {
            _hudWindow = _pluginInterface.ClientState.LocalPlayer?.ClassJob.Id switch
            {
                //Tanks
                //JobIDs.DRK => new DarkKnightHud(_pluginInterface, _pluginConfiguration),
                //JobIDs.GNB => new GunbreakerHudWindow(_pluginInterface, _pluginConfiguration),
                //JobIDs.WAR => new WarriorHud(_pluginInterface, _pluginConfiguration),
                //JobIDs.PLD => new PaladinHud(_pluginInterface, _pluginConfiguration),

                //Healers
                //JobIDs.WHM => new WhiteMageHud(_pluginInterface, _pluginConfiguration),
                //JobIDs.SCH => new ScholarHudWindow(_pluginInterface, _pluginConfiguration),
                //JobIDs.AST => new AstrologianHud(_pluginInterface, _pluginConfiguration),

                //Melee DPS
                //JobIDs.DRG => new DragoonHud(_pluginInterface, _pluginConfiguration),
                //JobIDs.SAM => new SamuraiHudWindow(_pluginInterface, _pluginConfiguration),
                JobIDs.MNK => new MonkHudWindow(_pluginInterface, _pluginConfiguration),
                //JobIDs.NIN => new NinjaHud(_pluginInterface, _pluginConfiguration),

                //Ranged DPS
                JobIDs.BRD => new BardHudWindow(_pluginInterface, _pluginConfiguration),
                JobIDs.DNC => new DancerHudWindow(_pluginInterface, _pluginConfiguration),
                JobIDs.MCH => new MachinistHudWindow(_pluginInterface, _pluginConfiguration),

                //Caster DPS
                //JobIDs.RDM => new RedMageHudWindow(_pluginInterface, _pluginConfiguration),
                //JobIDs.SMN => new SummonerHudWindow(_pluginInterface, _pluginConfiguration),
                //JobIDs.BLM => new BlackMageHud(_pluginInterface, _pluginConfiguration),

                //Low jobs
                //JobIDs.MRD => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                //JobIDs.GLD => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                //JobIDs.CNJ => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                //JobIDs.PGL => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                //JobIDs.LNC => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                //JobIDs.ROG => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                //JobIDs.ARC => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                //JobIDs.THM => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                //JobIDs.ACN => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),

                //Hand
                //JobIDs.CRP => new HandHudWindow(_pluginInterface, _pluginConfiguration),
                //JobIDs.BSM => new HandHudWindow(_pluginInterface, _pluginConfiguration),
                //JobIDs.ARM => new HandHudWindow(_pluginInterface, _pluginConfiguration),
                //JobIDs.GSM => new HandHudWindow(_pluginInterface, _pluginConfiguration),
                //JobIDs.LTW => new HandHudWindow(_pluginInterface, _pluginConfiguration),
                //JobIDs.WVR => new HandHudWindow(_pluginInterface, _pluginConfiguration),
                //JobIDs.ALC => new HandHudWindow(_pluginInterface, _pluginConfiguration),
                //JobIDs.CUL => new HandHudWindow(_pluginInterface, _pluginConfiguration),

                ////Land
                //JobIDs.MIN => new LandHudWindow(_pluginInterface, _pluginConfiguration),
                //JobIDs.BOT => new LandHudWindow(_pluginInterface, _pluginConfiguration),
                //JobIDs.FSH => new LandHudWindow(_pluginInterface, _pluginConfiguration),

                //dont have packs yet
                //JobIDs.BLU => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                _ => _hudWindow
            };
        }

        private void OpenConfigUi(object sender, EventArgs e)
        {
            ConfigurationManager.GetInstance().DrawConfigWindow = !ConfigurationManager.GetInstance().DrawConfigWindow;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            ConfigurationManager.GetInstance().DrawConfigWindow = false;

            if (_hudWindow != null)
            {
                _hudWindow.IsVisible = false;
            }

            _pluginInterface.CommandManager.RemoveHandler("/delvui");
            _pluginInterface.CommandManager.RemoveHandler("/delvuireloadconfig");
            _pluginInterface.UiBuilder.OnBuildUi -= Draw;
            _pluginInterface.UiBuilder.OnBuildFonts -= BuildFont;
            _pluginInterface.UiBuilder.OnOpenConfigUi -= OpenConfigUi;
            _pluginInterface.UiBuilder.RebuildFonts();
        }
    }
}
