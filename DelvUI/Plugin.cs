using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Actors;
using Dalamud.Game.Command;
using Dalamud.Game.Internal;
using Dalamud.Game.Internal.Gui;
using Dalamud.Interface;
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
        private HudManager _hudManager;
        private SystemMenuHook _menuHook;

        public static ImGuiScene.TextureWrap bannerTexture;

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public string AssemblyLocation { get; set; } = Assembly.GetExecutingAssembly().Location;
        public string Name => "DelvUI";
        public static string Version = "";

        public static ClientState ClientState => PluginInterface.ClientState;

        public static CommandManager CommandManager => PluginInterface.CommandManager;
        public static Condition Condition => ClientState.Condition;
        public static DalamudPluginInterface PluginInterface { get; private set; }
        public static DataManager DataManager => PluginInterface.Data;
        public static Framework Framework => PluginInterface.Framework;
        public static GameGui GameGui => Framework.Gui;
        public static JobGauges JobGauges => ClientState.JobGauges;
        public static ActorTable ObjectTable => ClientState.Actors;
        public static Dalamud.Game.SigScanner SigScanner => PluginInterface.TargetModuleScanner;
        public static Targets TargetManager => ClientState.Targets;
        public static UiBuilder UiBuilder => PluginInterface.UiBuilder;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            PluginInterface = pluginInterface;

            Version = Assembly.GetExecutingAssembly()?.GetName().Version.ToString() ?? "";

            FontsManager.Initialize(AssemblyLocation);
            LoadBanner();

            // initialize a not-necessarily-defaults configuration
            ConfigurationManager.Initialize(false);
            FontsManager.Instance.LoadConfig();

            UiBuilder.OnBuildUi += Draw;
            UiBuilder.OnBuildFonts += BuildFont;
            UiBuilder.OnOpenConfigUi += OpenConfigUi;

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
                    bannerTexture = UiBuilder.LoadImage(bannerImage);
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

            var fontPushed = FontsManager.Instance.PushFonts();

            if (!hudState)
            {
                _hudManager?.Draw();
            }

            if (fontPushed)
            {
                foreach (var _ in FontsManager.Instance.Fonts)
                {
                    ImGui.PopFont();
                }
            }
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

            HudHelper.RestoreToGameDefaults();

            ConfigurationManager.GetInstance().DrawConfigWindow = false;

            CommandManager.RemoveHandler("/delvui");
            CommandManager.RemoveHandler("/delvuireloadconfig");
            UiBuilder.OnBuildUi -= Draw;
            UiBuilder.OnBuildFonts -= BuildFont;
            UiBuilder.OnOpenConfigUi -= OpenConfigUi;
            UiBuilder.RebuildFonts();
        }
    }
}
