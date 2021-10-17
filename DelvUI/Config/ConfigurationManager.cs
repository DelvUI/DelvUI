using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using DelvUI.Config.Profiles;
using DelvUI.Config.Tree;
using DelvUI.Config.Windows;
using DelvUI.Helpers;
using DelvUI.Interface;
using DelvUI.Interface.GeneralElements;
using DelvUI.Interface.Jobs;
using DelvUI.Interface.Party;
using DelvUI.Interface.StatusEffects;
using ImGuiScene;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DelvUI.Config
{
    public delegate void ConfigurationManagerEventHandler(ConfigurationManager configurationManager);

    public class ConfigurationManager : IDisposable
    {
        public static ConfigurationManager Instance { get; private set; } = null!;

        public readonly TextureWrap? BannerImage;

        private BaseNode _configBaseNode;
        public BaseNode ConfigBaseNode
        {
            get => _configBaseNode;
            set
            {
                _configBaseNode = value;
                _mainConfigWindow.node = value;
            }
        }

        private WindowSystem _windowSystem;
        private MainConfigWindow _mainConfigWindow;
        private ChangelogWindow _changelogWindow;
        private GridWindow _gridWindow;

        public bool IsConfigWindowOpened => _mainConfigWindow.IsOpen;
        public bool ShowingModalWindow = false;

        public GradientDirection GradientDirection
        {
            get
            {
                var config = Instance.GetConfigObject<MiscColorConfig>();
                return config != null ? config.GradientDirection : GradientDirection.None;
            }
        }

        public string ConfigDirectory;

        private bool _needsProfileUpdate = false;
        private bool _lockHUD = true;

        public bool LockHUD
        {
            get => _lockHUD;
            set
            {
                if (_lockHUD == value)
                {
                    return;
                }

                _lockHUD = value;
                _mainConfigWindow.IsOpen = value;
                _gridWindow.IsOpen = !value;

                LockEvent?.Invoke(this);

                if (_lockHUD)
                {
                    SaveConfigurations();
                }
            }
        }

        public bool ShowHUD = true;

        public event ConfigurationManagerEventHandler? ResetEvent;
        public event ConfigurationManagerEventHandler? LockEvent;
        public event ConfigurationManagerEventHandler? ConfigClosedEvent;

        public ConfigurationManager()
        {
            BannerImage = Plugin.BannerTexture;
            ConfigDirectory = Plugin.PluginInterface.GetPluginConfigDirectory();

            _configBaseNode = new BaseNode();
            InitializeBaseNode(_configBaseNode);
            _configBaseNode.ConfigObjectResetEvent += OnConfigObjectReset;

            _mainConfigWindow = new MainConfigWindow("DelvUI Settings");
            _mainConfigWindow.node = _configBaseNode;
            _mainConfigWindow.CloseAction = () =>
            {
                ConfigClosedEvent?.Invoke(this);

                if (ConfigBaseNode.NeedsSave)
                {
                    SaveConfigurations();
                }

                if (_needsProfileUpdate)
                {
                    UpdateCurrentProfile();
                    _needsProfileUpdate = false;
                }
            };

            string changelog = LoadChangelog();
            _changelogWindow = new ChangelogWindow("DelvUI Changelog v" + Plugin.Version, changelog);
            _gridWindow = new GridWindow("Grid ##DelvUI");

            _windowSystem = new WindowSystem("DelvUI_Windows");
            _windowSystem.AddWindow(_mainConfigWindow);
            _windowSystem.AddWindow(_changelogWindow);
            _windowSystem.AddWindow(_gridWindow);

            CheckVersion();

            LoadOrInitializeFiles();

            Plugin.ClientState.Logout += OnLogout;
        }

        ~ConfigurationManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            ConfigBaseNode.ConfigObjectResetEvent -= OnConfigObjectReset;
            Plugin.ClientState.Logout -= OnLogout;
            BannerImage?.Dispose();

            Instance = null!;
        }

        public static void Initialize() { Instance = new ConfigurationManager(); }

        private void OnConfigObjectReset(BaseNode sender)
        {
            ResetEvent?.Invoke(this);
        }

        private void OnLogout(object? sender, EventArgs? args)
        {
            SaveConfigurations();
            ProfilesManager.Instance.SaveCurrentProfile();
        }

        private string LoadChangelog()
        {
            string path = Path.Combine(Plugin.AssemblyLocation, "changelog.md");

            try
            {
                string fullChangelog = File.ReadAllText(path);
                string versionChangelog = fullChangelog.Split("#", StringSplitOptions.RemoveEmptyEntries)[0];
                return versionChangelog.Replace(Plugin.Version, "");
            }
            catch (Exception e)
            {
                PluginLog.Error("Error loading changelog: " + e.Message);
            }

            return "";
        }

        private void CheckVersion()
        {
            string path = Path.Combine(ConfigDirectory, "version");

            try
            {
                bool needsWrite = false;

                if (!File.Exists(path))
                {
                    needsWrite = true;
                }
                else
                {
                    string version = File.ReadAllText(path);
                    if (version != Plugin.Version)
                    {
                        needsWrite = true;
                    }
                }

                _changelogWindow.IsOpen = needsWrite;

                if (needsWrite)
                {
                    File.WriteAllText(path, Plugin.Version);
                }
            }
            catch (Exception e)
            {
                PluginLog.Error("Error checking version: " + e.Message);
            }
        }

        #region windows
        public void ToggleConfigWindow()
        {
            _mainConfigWindow.Toggle();
        }

        public void OpenConfigWindow()
        {
            _mainConfigWindow.IsOpen = false;
        }

        public void CloseConfigWindow()
        {
            _mainConfigWindow.IsOpen = false;
        }

        public void OpenChangelogWindow()
        {
            _changelogWindow.IsOpen = true;
        }

        public void Draw()
        {
            _windowSystem.Draw();
        }

        public void AddExtraSectionNode(SectionNode node)
        {
            ConfigBaseNode.AddExtraSectionNode(node);
        }
        #endregion

        #region config getters and setters
        public PluginConfigObject GetConfigObjectForType(Type type)
        {
            MethodInfo? genericMethod = GetType().GetMethod("GetConfigObject");
            MethodInfo? method = genericMethod?.MakeGenericMethod(type);
            return (PluginConfigObject)method?.Invoke(this, null)!;
        }
        public T GetConfigObject<T>() where T : PluginConfigObject => ConfigBaseNode.GetConfigObject<T>()!;

        public static PluginConfigObject GetDefaultConfigObjectForType(Type type)
        {
            MethodInfo? method = type.GetMethod("DefaultConfig", BindingFlags.Public | BindingFlags.Static);
            return (PluginConfigObject)method?.Invoke(null, null)!;
        }

        public ConfigPageNode GetConfigPageNode<T>() where T : PluginConfigObject => ConfigBaseNode.GetConfigPageNode<T>()!;

        public void SetConfigObject(PluginConfigObject configObject) => ConfigBaseNode.SetConfigObject(configObject);
        #endregion

        #region load / save / profiles
        private void LoadOrInitializeFiles()
        {
            try
            {
                // detect if we need to create the config files (fresh install)
                if (Directory.GetDirectories(ConfigDirectory).Length == 0)
                {
                    SaveConfigurations(true);
                }
                else
                {
                    LoadConfigurations();
                }
            }
            catch
            {
                PluginLog.Error("Error initializing configurations!");
            }
        }

        public void ForceNeedsSave()
        {
            ConfigBaseNode.NeedsSave = true;
        }

        public void LoadConfigurations()
        {
            ConfigBaseNode.Load(ConfigDirectory);
        }

        public void SaveConfigurations(bool forced = false)
        {
            if (!forced && !ConfigBaseNode.NeedsSave)
            {
                return;
            }

            ConfigBaseNode.Save(ConfigDirectory);

            if (ProfilesManager.Instance != null)
            {
                ProfilesManager.Instance.SaveCurrentProfile();
            }

            ConfigBaseNode.NeedsSave = false;
        }

        public void UpdateCurrentProfile()
        {
            // dont update the profile on job change when the config window is opened
            if (_mainConfigWindow.IsOpen)
            {
                _needsProfileUpdate = true;
                return;
            }

            ProfilesManager.Instance.UpdateCurrentProfile();
        }

        public string? ExportCurrentConfigs()
        {
            return ConfigBaseNode.GetBase64String();
        }

        public bool ImportProfile(string rawString)
        {
            List<string> importStrings = new List<string>(rawString.Trim().Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries));
            ImportData[] imports = importStrings.Select(str => new ImportData(str)).ToArray();

            BaseNode node = new BaseNode();
            InitializeBaseNode(node);

            foreach (ImportData importData in imports)
            {
                PluginConfigObject? config = importData.GetObject();
                if (config == null)
                {
                    return false;
                }

                node.SetConfigObject(config);
            }

            try
            {
                node.Save(ConfigDirectory);
            }
            catch
            {
                return false;
            }

            string? oldSelection = ConfigBaseNode.SelectedOptionName;
            node.SelectedOptionName = oldSelection;
            node.AddExtraSectionNode(ProfilesManager.Instance.ProfilesNode);

            ConfigBaseNode.ConfigObjectResetEvent -= OnConfigObjectReset;
            ConfigBaseNode = node;
            ConfigBaseNode.ConfigObjectResetEvent += OnConfigObjectReset;

            ResetEvent?.Invoke(this);

            return true;
        }

        public void ResetConfig()
        {
            ConfigBaseNode.Reset();
            ResetEvent?.Invoke(this);
        }
        #endregion

        #region initialization
        private static void InitializeBaseNode(BaseNode node)
        {
            // creates node tree in the right order...
            foreach (Type type in configObjectTypes)
            {
                var genericMethod = node.GetType().GetMethod("GetConfigPageNode");
                var method = genericMethod?.MakeGenericMethod(type);
                method?.Invoke(node, null);
            }
        }

        private static Type[] configObjectTypes = new Type[]
        {
            typeof(PlayerUnitFrameConfig),
            typeof(TargetUnitFrameConfig),
            typeof(TargetOfTargetUnitFrameConfig),
            typeof(FocusTargetUnitFrameConfig),

            typeof(PlayerPrimaryResourceConfig),
            typeof(TargetPrimaryResourceConfig),
            typeof(TargetOfTargetPrimaryResourceConfig),
            typeof(FocusTargetPrimaryResourceConfig),

            typeof(PlayerCastbarConfig),
            typeof(TargetCastbarConfig),
            typeof(TargetOfTargetCastbarConfig),
            typeof(FocusTargetCastbarConfig),

            typeof(PlayerBuffsListConfig),
            typeof(PlayerDebuffsListConfig),
            typeof(TargetBuffsListConfig),
            typeof(TargetDebuffsListConfig),
            typeof(FocusTargetBuffsListConfig),
            typeof(FocusTargetDebuffsListConfig),
            typeof(CustomEffectsListConfig),

            typeof(PartyFramesConfig),
            typeof(PartyFramesHealthBarsConfig),
            typeof(PartyFramesManaBarConfig),
            typeof(PartyFramesCastbarConfig),
            typeof(PartyFramesRoleIconConfig),
            typeof(PartyFramesLeaderIconConfig),
            typeof(PartyFramesBuffsConfig),
            typeof(PartyFramesDebuffsConfig),
            typeof(PartyFramesRaiseTrackerConfig),
            typeof(PartyFramesInvulnTrackerConfig),

            typeof(PaladinConfig),
            typeof(WarriorConfig),
            typeof(DarkKnightConfig),
            typeof(GunbreakerConfig),

            typeof(WhiteMageConfig),
            typeof(ScholarConfig),
            typeof(AstrologianConfig),

            typeof(MonkConfig),
            typeof(DragoonConfig),
            typeof(NinjaConfig),
            typeof(SamuraiConfig),

            typeof(BardConfig),
            typeof(MachinistConfig),
            typeof(DancerConfig),

            typeof(BlackMageConfig),
            typeof(SummonerConfig),
            typeof(RedMageConfig),

            typeof(TanksColorConfig),
            typeof(HealersColorConfig),
            typeof(MeleeColorConfig),
            typeof(RangedColorConfig),
            typeof(CastersColorConfig),
            typeof(MiscColorConfig),

            typeof(FontsConfig),
            typeof(HUDOptionsConfig),
            typeof(TooltipsConfig),
            typeof(ExperienceBarConfig),
            typeof(GCDIndicatorConfig),
            typeof(PullTimerConfig),
            typeof(LimitBreakConfig),
            typeof(MPTickerConfig),
            typeof(GridConfig),

            typeof(ImportConfig)
        };
        #endregion
    }
}