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
using DelvUI.Helpers;
using DelvUI.Interface;
using DelvUI.Interface.GeneralElements;
using FFXIVClientStructs;
using ImGuiNET;
using ImGuiScene;
using System;
using System.IO;
using System.Reflection;
using SigScanner = Dalamud.Game.SigScanner;

namespace DelvUI
{
    public class Plugin : IDalamudPlugin
    {
        public static ClientState ClientState { get; private set; } = null!;
        public static CommandManager CommandManager { get; private set; } = null!;
        public static Condition Condition { get; private set; } = null!;
        public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
        public static DataManager DataManager { get; private set; } = null!;
        public static Framework Framework { get; private set; } = null!;
        public static GameGui GameGui { get; private set; } = null!;
        public static JobGauges JobGauges { get; private set; } = null!;
        public static ObjectTable ObjectTable { get; private set; } = null!;
        public static SigScanner SigScanner { get; private set; } = null!;
        public static TargetManager TargetManager { get; private set; } = null!;
        public static UiBuilder UiBuilder { get; private set; } = null!;

        public static TextureWrap? BannerTexture;

        public string AssemblyLocation { get; }
        public string Name => "DelvUI";

        public static string Version { get; private set; } = "";

        private HudManager _hudManager = null!;
        private SystemMenuHook _menuHook = null!;

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
            TargetManager targetManager
        )
        {
            ClientState = clientState;
            CommandManager = commandManager;
            Condition = condition;
            PluginInterface = pluginInterface;
            DataManager = dataManager;
            Framework = framework;
            GameGui = gameGui;
            JobGauges = jobGauges;
            ObjectTable = objectTable;
            SigScanner = sigScanner;
            TargetManager = targetManager;
            UiBuilder = PluginInterface.UiBuilder;
            AssemblyLocation = Assembly.GetExecutingAssembly().Location;

            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "";
            FontsManager.Initialize(AssemblyLocation);

            LoadBanner();

            // initialize a not-necessarily-defaults configuration
            ConfigurationManager.Initialize(false);
            FontsManager.Instance.LoadConfig();

            UiBuilder.Draw += Draw;
            UiBuilder.BuildFonts += BuildFont;
            UiBuilder.OpenConfigUi += OpenConfigUi;

            if (!FontsManager.Instance.DefaultFontBuilt)
            {
                UiBuilder.RebuildFonts();
            }

            CommandManager.AddHandler(
                "/delvui",
                new CommandInfo(PluginCommand)
                {
                    HelpMessage = "Opens the DelvUI configuration window.\n"
                                + "/delvui toggle → Toggles HUD visibility.\n"
                                + "/delvui show → Shows HUD.\n"
                                + "/delvui hide → Hides HUD.\n"
                                + "/delvui reset → Resets HUD to default. This is irreversible!",

                    ShowInHelp = true
                }
            );

            _menuHook = new SystemMenuHook(PluginInterface);

            TexturesCache.Initialize();
            GlobalColors.Initialize();
            TooltipsHelper.Initialize();
            ChatHelper.Initialize();
            MouseOverHelper.Initialize();
            Resolver.Initialize();

            _hudManager = new HudManager();
        }

        public void Dispose()
        {
            _menuHook.Dispose();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void BuildFont()
        {
            FontsManager.Instance.BuildFonts();
        }

        private void LoadBanner()
        {
            string bannerImage = Path.Combine(Path.GetDirectoryName(AssemblyLocation) ?? "", "Media", "Images", "banner_short_x150.png");

            if (File.Exists(bannerImage))
            {
                try
                {
                    BannerTexture = UiBuilder.LoadImage(bannerImage);
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
            var configManager = ConfigurationManager.GetInstance();

            if (configManager.DrawConfigWindow && !configManager.LockHUD)
            {
                configManager.LockHUD = true;
            }
            else
            {
                switch (arguments)
                {
                    case "toggle":
                        ConfigurationManager.GetInstance().ShowHUD = !ConfigurationManager.GetInstance().ShowHUD;

                        break;

                    case "show":
                        ConfigurationManager.GetInstance().ShowHUD = true;

                        break;

                    case "hide":
                        ConfigurationManager.GetInstance().ShowHUD = false;

                        break;

                    case "reset":
                        ConfigurationManager.Initialize(true);
                        ConfigurationManager.GetInstance().SaveConfigurations();

                        break;

                    default:
                        configManager.DrawConfigWindow = !configManager.DrawConfigWindow;

                        break;
                }
            }
        }

        private void ReloadConfigCommand(string command, string arguments) { ConfigurationManager.GetInstance().LoadConfigurations(); }

        private void Draw()
        {
            bool hudState = Condition[ConditionFlag.WatchingCutscene]
                         || Condition[ConditionFlag.WatchingCutscene78]
                         || Condition[ConditionFlag.OccupiedInCutSceneEvent]
                         || Condition[ConditionFlag.CreatingCharacter]
                         || Condition[ConditionFlag.BetweenAreas]
                         || Condition[ConditionFlag.BetweenAreas51];

            UiBuilder.OverrideGameCursor = false;

            ConfigurationManager.GetInstance().Draw();

            var fontPushed = FontsManager.Instance.PushDefaultFont();

            if (!hudState)
            {
                _hudManager?.Draw();
            }

            if (fontPushed)
            {
                ImGui.PopFont();
            }
        }

        private void OpenConfigUi()
        {
            ConfigurationManager.GetInstance().DrawConfigWindow = !ConfigurationManager.GetInstance().DrawConfigWindow;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            _hudManager.Dispose();

            ConfigurationManager.GetInstance().DrawConfigWindow = false;

            CommandManager.RemoveHandler("/delvui");
            CommandManager.RemoveHandler("/delvuireloadconfig");

            UiBuilder.Draw -= Draw;
            UiBuilder.BuildFonts -= BuildFont;
            UiBuilder.OpenConfigUi -= OpenConfigUi;
            UiBuilder.RebuildFonts();
        }
    }
}
