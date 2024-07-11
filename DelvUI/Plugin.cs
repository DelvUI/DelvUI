using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using DelvUI.Config;
using DelvUI.Config.Profiles;
using DelvUI.Helpers;
using DelvUI.Interface;
using DelvUI.Interface.GeneralElements;
using DelvUI.Interface.Nameplates;
using DelvUI.Interface.Party;
using DelvUI.Interface.PartyCooldowns;
using System;
using System.IO;
using System.Reflection;

namespace DelvUI
{
    public class Plugin : IDalamudPlugin
    {
        public static IBuddyList BuddyList { get; private set; } = null!;
        public static IClientState ClientState { get; private set; } = null!;
        public static ICommandManager CommandManager { get; private set; } = null!;
        public static ICondition Condition { get; private set; } = null!;
        public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        public static IDataManager DataManager { get; private set; } = null!;
        public static IFramework Framework { get; private set; } = null!;
        public static IGameGui GameGui { get; private set; } = null!;
        public static IJobGauges JobGauges { get; private set; } = null!;
        public static IObjectTable ObjectTable { get; private set; } = null!;
        public static ISigScanner SigScanner { get; private set; } = null!;
        public static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
        public static ITargetManager TargetManager { get; private set; } = null!;
        public static IUiBuilder UiBuilder { get; private set; } = null!;
        public static IPartyList PartyList { get; private set; } = null!;
        public static IPluginLog Logger { get; private set; } = null!;
        public static ITextureProvider TextureProvider { get; private set; } = null!;
        public static IAddonLifecycle AddonLifecycle { get; private set; } = null!;

        public static ISharedImmediateTexture? BannerTexture;

        public static string AssemblyLocation { get; private set; } = "";
        public string Name => "DelvUI";

        public static string Version { get; private set; } = "";

        private HudManager _hudManager = null!;

        public delegate void JobChangedEventHandler(uint jobId);
        public static event JobChangedEventHandler? JobChangedEvent;
        private uint _jobId = 0;

        public Plugin(
            IBuddyList buddyList,
            IClientState clientState,
            ICommandManager commandManager,
            ICondition condition,
            IDalamudPluginInterface pluginInterface,
            IDataManager dataManager,
            IFramework framework,
            IGameGui gameGui,
            IJobGauges jobGauges,
            IObjectTable objectTable,
            IPartyList partyList,
            ISigScanner sigScanner,
            IGameInteropProvider gameInteropProvider,
            ITargetManager targetManager,
            IPluginLog logger,
            ITextureProvider textureProvider,
            IAddonLifecycle addonLifecycle
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
            GameInteropProvider = gameInteropProvider;
            TargetManager = targetManager;
            UiBuilder = PluginInterface.UiBuilder;
            Logger = logger;
            TextureProvider = textureProvider;
            AddonLifecycle = addonLifecycle;

            if (pluginInterface.AssemblyLocation.DirectoryName != null)
            {
                AssemblyLocation = pluginInterface.AssemblyLocation.DirectoryName + "\\";
            }
            else
            {
                AssemblyLocation = Assembly.GetExecutingAssembly().Location;
            }

            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "2.2.0.3";

            FontsManager.Initialize(AssemblyLocation);
            BarTexturesManager.Initialize(AssemblyLocation);
            LoadBanner();

            // initialize a not-necessarily-defaults configuration
            ConfigurationManager.Initialize();
            ProfilesManager.Initialize();
            ConfigurationManager.Instance.LoadOrInitializeFiles();

            FontsManager.Instance.LoadConfig();
            BarTexturesManager.Instance.LoadConfig();

            ChatHelper.Initialize();
            ClipRectsHelper.Initialize();
            GlobalColors.Initialize();
            LimitBreakHelper.Initialize();
            InputsHelper.Initialize();
            NameplatesManager.Initialize();
            PartyManager.Initialize();
            PartyCooldownsManager.Initialize();
            PullTimerHelper.Initialize();
            TextTagsHelper.Initialize();
            TooltipsHelper.Initialize();
            PetRenamerHelper.Initialize();
            HonorificHelper.Initialize();
            WotsitHelper.Initialize();
            WhosTalkingHelper.Initialize();

            _hudManager = new HudManager();

            UiBuilder.Draw += Draw;
            UiBuilder.OpenConfigUi += OpenConfigUi;

            FontsManager.Instance.BuildFonts();

            CommandManager.AddHandler(
                "/delvui",
                new CommandInfo(PluginCommand)
                {
                    HelpMessage = "Opens the DelvUI configuration window.\n"
                                + "/delvui toggle → Toggles HUD visibility.\n"
                                + "/delvui show → Shows HUD.\n"
                                + "/delvui hide → Hides HUD.\n"
                                + "/delvui toggledefaulthud → Toggles the game's Job Gauges visibility.\n"
                                + "/delvui forcejob <JOB> → Forces DelvUI to show the hud for the given Job short name.\n"
                                + "/delvui profile <PROFILE> → Switch to the given profile",

                    ShowInHelp = true
                }
            );

            CommandManager.AddHandler(
                "/dui",
                new CommandInfo(PluginCommand)
                {
                    HelpMessage = "Opens the DelvUI configuration window.\n"
                                + "/dui toggle → Toggles HUD visibility.\n"
                                + "/dui show → Shows HUD.\n"
                                + "/dui hide → Hides HUD."
                                + "/dui toggledefaulthud → Toggles the game's Job Gauges visibility.\n"
                                + "/dui forcejob <JOB> → Forces DelvUI to show the hud for the given Job short name.\n"
                                + "/dui profile <PROFILE> → Switch to the given profile",

                    ShowInHelp = true
                }
            );

            WotsitHelper.Instance?.Update();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void LoadBanner()
        {
            string bannerImage = Path.Combine(Path.GetDirectoryName(AssemblyLocation) ?? "", "Media", "Images", "banner_short_x150.png");

            if (File.Exists(bannerImage))
            {
                try
                {
                    BannerTexture = TextureProvider.GetFromFile(bannerImage);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Image failed to load. {bannerImage}\n\n{ex}");
                }
            }
            else
            {
                Logger.Debug($"Image doesn't exist. {bannerImage}");
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

                    case "toggledefaulthud":
                        ConfigurationManager.Instance.GetConfigObject<HUDOptionsConfig>().HideDefaultJobGauges =
                            !ConfigurationManager.Instance.GetConfigObject<HUDOptionsConfig>().HideDefaultJobGauges;
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
                            ProfilesManager.Instance?.CheckUpdateSwitchCurrentProfile(profile[1]);
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
            PartyManager.Instance?.Update();
            WhosTalkingHelper.Instance?.Update();

            try
            {
                using (FontsManager.Instance.PushDefaultFont())
                {
                    if (!hudState)
                    {
                        _hudManager?.Draw(_jobId);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("Something went wrong!:\n" + e.Message);
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

            _hudManager?.Dispose();

            ConfigurationManager.Instance?.SaveConfigurations(true);
            ConfigurationManager.Instance?.CloseConfigWindow();

            CommandManager.RemoveHandler("/delvui");
            CommandManager.RemoveHandler("/dui");

            UiBuilder.Draw -= Draw;
            UiBuilder.OpenConfigUi -= OpenConfigUi;
            UiBuilder.FontAtlas.BuildFontsAsync();

            BarTexturesManager.Instance?.Dispose();
            ChatHelper.Instance?.Dispose();
            ClipRectsHelper.Instance?.Dispose();
            ExperienceHelper.Instance?.Dispose();
            FontsManager.Instance?.Dispose();
            GlobalColors.Instance?.Dispose();
            LimitBreakHelper.Instance?.Dispose();
            InputsHelper.Instance?.Dispose();
            NameplatesManager.Instance?.Dispose();
            PartyCooldownsManager.Instance?.Dispose();
            PartyManager.Instance?.Dispose();
            PullTimerHelper.Instance?.Dispose();
            ProfilesManager.Instance?.Dispose();
            SpellHelper.Instance?.Dispose();
            TooltipsHelper.Instance?.Dispose();
            HonorificHelper.Instance?.Dispose();
            PetRenamerHelper.Instance?.Dispose();
            WotsitHelper.Instance?.Dispose();
            WhosTalkingHelper.Instance?.Dispose();

            // This needs to remain last to avoid race conditions
            ConfigurationManager.Instance?.Dispose();
        }
    }
}
