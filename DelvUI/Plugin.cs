using System;
using System.IO;
using System.Reflection;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Buddy;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface;
using Dalamud.Logging;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Config.Profiles;
using DelvUI.Helpers;
using DelvUI.Interface;
using DelvUI.Interface.GeneralElements;
using DelvUI.Interface.Party;
using DelvUI.Interface.PartyCooldowns;
using ImGuiNET;
using ImGuiScene;
using SigScanner = Dalamud.Game.SigScanner;

namespace DelvUI
{
    public class Plugin : IDalamudPlugin
    {
        public static BuddyList BuddyList { get; private set; } = null!;
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
        public static PartyList PartyList { get; private set; } = null!;

        public static TextureWrap? BannerTexture;

        public static string AssemblyLocation { get; private set; } = "";
        public string Name => "DelvUI";

        public static string Version { get; private set; } = "";

        private HudManager _hudManager = null!;
        private SystemMenuHook _menuHook = null!;

        public delegate void JobChangedEventHandler(uint jobId);
        public static event JobChangedEventHandler? JobChangedEvent;
        private uint _jobId = 0;

        public Plugin(
            BuddyList buddyList,
            ClientState clientState,
            CommandManager commandManager,
            Condition condition,
            DalamudPluginInterface pluginInterface,
            DataManager dataManager,
            Framework framework,
            GameGui gameGui,
            JobGauges jobGauges,
            ObjectTable objectTable,
            PartyList partyList,
            SigScanner sigScanner,
            TargetManager targetManager
        )
        {
            BuddyList = buddyList;
            ClientState = clientState;
            CommandManager = commandManager;
            Condition = condition;
            PluginInterface = pluginInterface;
            DataManager = dataManager;
            Framework = framework;
            GameGui = gameGui;
            JobGauges = jobGauges;
            ObjectTable = objectTable;
            PartyList = partyList;
            SigScanner = sigScanner;
            TargetManager = targetManager;
            UiBuilder = PluginInterface.UiBuilder;

            if (pluginInterface.AssemblyLocation.DirectoryName != null)
            {
                AssemblyLocation = pluginInterface.AssemblyLocation.DirectoryName + "\\";
            }
            else
            {
                AssemblyLocation = Assembly.GetExecutingAssembly().Location;
            }

            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.6.3.0";

            FontsManager.Initialize(AssemblyLocation);
            LoadBanner();

            // initialize a not-necessarily-defaults configuration
            ConfigurationManager.Initialize();
            ProfilesManager.Initialize();
            ConfigurationManager.Instance.LoadOrInitializeFiles();

            FontsManager.Instance.LoadConfig();

            _menuHook = new SystemMenuHook(PluginInterface);

            ChatHelper.Initialize();
            ClipRectsHelper.Initialize();
            GlobalColors.Initialize();
            LimitBreakHelper.Initialize();
            InputsHelper.Initialize();
            PartyManager.Initialize();
            PartyCooldownsManager.Initialize();
            PullTimerHelper.Initialize();
            TextTagsHelper.Initialize();
            TexturesCache.Initialize();
            TooltipsHelper.Initialize();

            _hudManager = new HudManager();

            UiBuilder.Draw += Draw;
            UiBuilder.BuildFonts += BuildFont;
            UiBuilder.OpenConfigUi += OpenConfigUi;

            CommandManager.AddHandler(
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
        }

        public void Dispose()
        {
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
            var configManager = ConfigurationManager.Instance;

            if (configManager.IsConfigWindowOpened && !configManager.LockHUD)
            {
                configManager.LockHUD = true;
            }
            else
            {
                switch (arguments)
                {
                    case "toggle":
                        ConfigurationManager.Instance.ShowHUD = !ConfigurationManager.Instance.ShowHUD;
                        break;

                    case "show":
                        ConfigurationManager.Instance.ShowHUD = true;
                        break;

                    case "hide":
                        ConfigurationManager.Instance.ShowHUD = false;
                        break;

                    case { } argument when argument.StartsWith("forcejob"):
                        // TODO: Turn this into a helper function?
                        var args = argument.Split(" ");

                        if (args.Length > 0)
                        {
                            if (args[1] == "off")
                            {
                                ForcedJob.Enabled = false;

                                return;
                            }

                            var job = typeof(JobIDs).GetField(args[1].ToUpper());

                            if (job != null)
                            {
                                ForcedJob.Enabled = true;
                                ForcedJob.ForcedJobId = (uint)(job.GetValue(null) ?? JobIDs.ACN);
                            }
                        }
                        break;

                    case { } argument when argument.StartsWith("profile"):
                        // TODO: Turn this into a helper function?
                        var profile = argument.Split(" ", 2);

                        if (profile.Length > 0)
                        {
                            ProfilesManager.Instance.CheckUpdateSwitchCurrentProfile(profile[1]);
                        }

                        break;

                    default:
                        configManager.ToggleConfigWindow();

                        break;
                }
            }
        }

        private void UpdateJob()
        {
            var player = ClientState.LocalPlayer;
            if (player is null) { return; }

            var newJobId = player.ClassJob.Id;
            if (ForcedJob.Enabled)
            {
                newJobId = ForcedJob.ForcedJobId;
            }

            if (_jobId != newJobId)
            {
                _jobId = newJobId;
                JobChangedEvent?.Invoke(_jobId);
            }
        }

        private void Draw()
        {
            UpdateJob();

            bool hudState =
                Condition[ConditionFlag.WatchingCutscene] ||
                Condition[ConditionFlag.WatchingCutscene78] ||
                Condition[ConditionFlag.OccupiedInCutSceneEvent] ||
                Condition[ConditionFlag.CreatingCharacter] ||
                Condition[ConditionFlag.BetweenAreas] ||
                Condition[ConditionFlag.BetweenAreas51] ||
                Condition[ConditionFlag.OccupiedSummoningBell];

            UiBuilder.OverrideGameCursor = false;

            ConfigurationManager.Instance.Draw();

            var fontPushed = FontsManager.Instance.PushDefaultFont();

            if (!hudState)
            {
                _hudManager?.Draw(_jobId);
            }

            if (fontPushed)
            {
                ImGui.PopFont();
            }

            InputsHelper.Instance.Update();
        }

        private void OpenConfigUi()
        {
            ConfigurationManager.Instance.ToggleConfigWindow();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            _menuHook.Dispose();
            _hudManager.Dispose();

            ConfigurationManager.Instance.SaveConfigurations(true);
            ConfigurationManager.Instance.CloseConfigWindow();

            CommandManager.RemoveHandler("/delvui");

            UiBuilder.Draw -= Draw;
            UiBuilder.BuildFonts -= BuildFont;
            UiBuilder.OpenConfigUi -= OpenConfigUi;
            UiBuilder.RebuildFonts();

            ChatHelper.Instance.Dispose();
            ClipRectsHelper.Instance.Dispose();
            ExperienceHelper.Instance.Dispose();
            FontsManager.Instance.Dispose();
            GlobalColors.Instance.Dispose();
            LimitBreakHelper.Instance.Dispose();
            InputsHelper.Instance.Dispose();
            PartyCooldownsManager.Instance.Dispose();
            PartyManager.Instance.Dispose();
            PullTimerHelper.Instance.Dispose();
            ProfilesManager.Instance.Dispose();
            SpellHelper.Instance.Dispose();
            TexturesCache.Instance.Dispose();
            TooltipsHelper.Instance.Dispose();

            // This needs to remain last to avoid race conditions
            ConfigurationManager.Instance.Dispose();
        }
    }
}
