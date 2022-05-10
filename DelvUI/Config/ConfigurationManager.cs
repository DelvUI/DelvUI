using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using DelvUI.Config.Profiles;
using DelvUI.Config.Tree;
using DelvUI.Config.Windows;
using DelvUI.Helpers;
using DelvUI.Interface;
using DelvUI.Interface.EnemyList;
using DelvUI.Interface.GeneralElements;
using DelvUI.Interface.Jobs;
using DelvUI.Interface.Party;
using DelvUI.Interface.PartyCooldowns;
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
    public delegate void StrataLevelsEventHandler(ConfigurationManager configurationManager, PluginConfigObject config);

    public class ConfigurationManager : IDisposable
    {
        public static ConfigurationManager Instance { get; private set; } = null!;

        public readonly TextureWrap? BannerImage;

        private BaseNode _configBaseNode;
        private Dictionary<string, BaseNode> _configBaseNodeByProfile;
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

        public string CurrentVersion => Plugin.Version;
        public string? PreviousVersion { get; private set; } = null;

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
                    ConfigBaseNode.NeedsSave = true;
                }
            }
        }

        public bool ShowHUD = true;

        public event ConfigurationManagerEventHandler? ResetEvent;
        public event ConfigurationManagerEventHandler? LockEvent;
        public event ConfigurationManagerEventHandler? ConfigClosedEvent;
        public event StrataLevelsEventHandler? StrataLevelsChangedEvent;

        public ConfigurationManager()
        {
            BannerImage = Plugin.BannerTexture;
            ConfigDirectory = Plugin.PluginInterface.GetPluginConfigDirectory();

            _configBaseNodeByProfile = new Dictionary<string, BaseNode>();
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

            Plugin.ClientState.Logout += OnLogout;
            Plugin.JobChangedEvent += OnJobChanged;
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
            ProfilesManager.Instance?.SaveCurrentProfile();
        }

        private void OnJobChanged(uint jobId)
        {
            UpdateCurrentProfile();
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
                    PreviousVersion = File.ReadAllText(path);
                    if (PreviousVersion != Plugin.Version)
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

        #region strata
        public void OnStrataLevelChanged(PluginConfigObject config)
        {
            StrataLevelsChangedEvent?.Invoke(this, config);
        }
        #endregion

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
        public void LoadOrInitializeFiles()
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

                    // gotta save after initial load store possible version update changes right away
                    SaveConfigurations(true);
                }
            }
            catch (Exception e)
            {
                PluginLog.Error("Error initializing configurations: " + e.Message);

                if (e.StackTrace != null)
                {
                    PluginLog.Error(e.StackTrace);
                }
            }
        }

        public void ForceNeedsSave()
        {
            ConfigBaseNode.NeedsSave = true;
        }

        public void LoadConfigurations()
        {
            ConfigBaseNode.Load(ConfigDirectory, CurrentVersion, PreviousVersion);
        }

        public void SaveConfigurations(bool forced = false)
        {
            if (!forced && !ConfigBaseNode.NeedsSave)
            {
                return;
            }

            ConfigBaseNode.Save(ConfigDirectory);

            ProfilesManager.Instance?.SaveCurrentProfile();

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

            ProfilesManager.Instance?.UpdateCurrentProfile();
        }

        public string? ExportCurrentConfigs()
        {
            return ConfigBaseNode.GetBase64String();
        }

        public bool ImportProfile(string oldProfileName, string profileName, string rawString)
        {
            // cache old profile
            _configBaseNodeByProfile[oldProfileName] = ConfigBaseNode;

            // load profile from cache or from rawString
            if (!_configBaseNodeByProfile.TryGetValue(profileName, out BaseNode? maybeNode)
                && !ImportProfileNonCached(rawString, out maybeNode))
            {
                return false;
            }

            BaseNode node = maybeNode!;
            if(IsConfigWindowOpened || string.IsNullOrEmpty(node.SelectedOptionName))
            {
                node.SelectedOptionName = ConfigBaseNode.SelectedOptionName;
                node.RefreshSelectedNode();
            }

            ConfigBaseNode.ConfigObjectResetEvent -= OnConfigObjectReset;
            ConfigBaseNode = node;
            ConfigBaseNode.ConfigObjectResetEvent += OnConfigObjectReset;

            ResetEvent?.Invoke(this);

            return true;
        }

        private bool ImportProfileNonCached(string rawString, out BaseNode? node)
        {
            List<string> importStrings = new List<string>(rawString.Trim().Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries));
            ImportData[] imports = importStrings.Select(str => new ImportData(str)).ToArray();

            node = new BaseNode();
            InitializeBaseNode(node);

            Dictionary<Type, PluginConfigObject> oldConfigObjects = new Dictionary<Type, PluginConfigObject>();

            foreach (ImportData importData in imports)
            {
                PluginConfigObject? config = importData.GetObject();
                if (config == null)
                {
                    return false;
                }

                if (!node.SetConfigObject(config))
                {
                    oldConfigObjects.Add(config.GetType(), config);
                }
            }

            try
            {
                // handle imports for breaking changes in the config
                if (UnmergeableConfigTypesPerVersion.TryGetValue(CurrentVersion, out List<Type>? types) && types != null)
                {
                    foreach (Type type in types)
                    {
                        var genericMethod = node.GetType().GetMethod("GetConfigObject");
                        var method = genericMethod?.MakeGenericMethod(type);
                        PluginConfigObject? config = (PluginConfigObject?)method?.Invoke(node, null);

                        if (config != null)
                        {
                            config.ImportFromOldVersion(oldConfigObjects, CurrentVersion, PreviousVersion);
                            node.SetConfigObject(config); // needed to refresh nodes
                        }
                    }
                }

                node.Save(ConfigDirectory);
            }
            catch
            {
                return false;
            }

            if (ProfilesManager.Instance != null)
            {
                node.AddExtraSectionNode(ProfilesManager.Instance.ProfilesNode);
            }

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
            foreach (Type type in ConfigObjectTypes)
            {
                var genericMethod = node.GetType().GetMethod("GetConfigPageNode");
                var method = genericMethod?.MakeGenericMethod(type);
                method?.Invoke(node, null);
            }
        }

        private static Type[] ConfigObjectTypes = new Type[]
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
            typeof(PartyFramesIconsConfig),
            typeof(PartyFramesBuffsConfig),
            typeof(PartyFramesDebuffsConfig),
            typeof(PartyFramesTrackersConfig),

            typeof(PartyCooldownsConfig),
            typeof(PartyCooldownsBarConfig),
            typeof(PartyCooldownsDataConfig),

            typeof(EnemyListConfig),
            typeof(EnemyListHealthBarConfig),
            typeof(EnemyListEnmityIconConfig),
            typeof(EnemyListCastbarConfig),
            typeof(EnemyListBuffsConfig),
            typeof(EnemyListDebuffsConfig),

            typeof(PaladinConfig),
            typeof(WarriorConfig),
            typeof(DarkKnightConfig),
            typeof(GunbreakerConfig),

            typeof(WhiteMageConfig),
            typeof(ScholarConfig),
            typeof(AstrologianConfig),
            typeof(SageConfig),

            typeof(MonkConfig),
            typeof(DragoonConfig),
            typeof(NinjaConfig),
            typeof(SamuraiConfig),
            typeof(ReaperConfig),

            typeof(BardConfig),
            typeof(MachinistConfig),
            typeof(DancerConfig),

            typeof(BlackMageConfig),
            typeof(SummonerConfig),
            typeof(RedMageConfig),
            typeof(BlueMageConfig),

            typeof(TanksColorConfig),
            typeof(HealersColorConfig),
            typeof(MeleeColorConfig),
            typeof(RangedColorConfig),
            typeof(CastersColorConfig),
            typeof(RolesColorConfig),
            typeof(MiscColorConfig),

            typeof(FontsConfig),
            typeof(HUDOptionsConfig),
            typeof(WindowClippingConfig),
            typeof(TooltipsConfig),
            typeof(ExperienceBarConfig),
            typeof(GCDIndicatorConfig),
            typeof(PullTimerConfig),
            typeof(LimitBreakConfig),
            typeof(MPTickerConfig),
            typeof(GridConfig),

            typeof(ImportConfig)
        };

        private static Dictionary<string, List<Type>> UnmergeableConfigTypesPerVersion = new Dictionary<string, List<Type>>()
        {
            ["0.4.0.0"] = new List<Type>() {
                typeof(PartyFramesIconsConfig),
                typeof(PartyFramesTrackersConfig)
            },
            ["0.6.2.0"] = new List<Type>() {
                typeof(PartyFramesHealthBarsConfig)
            },

        };
        #endregion
    }
}