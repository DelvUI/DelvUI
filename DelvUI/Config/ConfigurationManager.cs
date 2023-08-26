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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DelvUI.Config
{
    public delegate void ConfigurationManagerEventHandler(ConfigurationManager configurationManager);
    public delegate void StrataLevelsEventHandler(ConfigurationManager configurationManager, PluginConfigObject config);
    public delegate void GlobalVisibilityEventHandler(ConfigurationManager configurationManager, VisibilityConfig config);

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

        public bool OverrideDalamudStyle
        {
            get
            {
                HUDOptionsConfig config = Instance.GetConfigObject<HUDOptionsConfig>();
                return config != null ? config.OverrideDalamudStyle : true;
            }
        }

        public CultureInfo ActiveCultreInfo
        {
            get {
                HUDOptionsConfig config = Instance.GetConfigObject<HUDOptionsConfig>();
                return config == null || config.UseRegionalNumberFormats ? CultureInfo.CurrentCulture : CultureInfo.InvariantCulture;
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
        public event GlobalVisibilityEventHandler? GlobalVisibilityEvent;

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

            _configBaseNode.CreateNodesIfNeeded();
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
                bool needsBackup = false;

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
                        needsBackup = true;
                    }
                }

                _changelogWindow.IsOpen = needsWrite;

                if (needsWrite)
                {
                    File.WriteAllText(path, Plugin.Version);
                }

                if (needsBackup && PreviousVersion != null)
                {
                    BackupFiles(PreviousVersion);
                }
            }
            catch (Exception e)
            {
                PluginLog.Error("Error checking version: " + e.Message);
            }
        }

        private void BackupFiles(string version)
        {
            string backupsRoot = Path.Combine(ConfigDirectory, "Backups");
            if (!Directory.Exists(backupsRoot))
            {
                Directory.CreateDirectory(backupsRoot);
            }

            string backupPath = Path.Combine(backupsRoot, version);

            foreach (string folderPath in Directory.GetDirectories(ConfigDirectory, "*", SearchOption.AllDirectories))
            {
                if (folderPath.Contains("Backups")) { continue; }

                Directory.CreateDirectory(folderPath.Replace(ConfigDirectory, backupPath));
            }

            foreach (string filePath in Directory.GetFiles(ConfigDirectory, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(filePath, filePath.Replace(ConfigDirectory, backupPath), true);
            }
        }

        #region strata
        public void OnStrataLevelChanged(PluginConfigObject config)
        {
            StrataLevelsChangedEvent?.Invoke(this, config);
        }
        #endregion

        #region visibility
        public void OnGlobalVisibilityChanged(VisibilityConfig config)
        {
            GlobalVisibilityEvent?.Invoke(this, config);
        }
        #endregion

        #region windows
        public void ToggleConfigWindow()
        {
            _mainConfigWindow.Toggle();
        }

        public void OpenConfigWindow()
        {
            _mainConfigWindow.IsOpen = true;
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

        public List<T> GetObjects<T>() => ConfigBaseNode.GetObjects<T>();
        #endregion

        #region load / save / profiles
        public bool IsFreshInstall()
        {
            return Directory.GetDirectories(ConfigDirectory).Length == 0;
        }

        public void LoadOrInitializeFiles()
        {
            try
            {
                if (!IsFreshInstall())
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
            PerformV2Migration();
            ConfigBaseNode.Load(ConfigDirectory);
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

        public void PerformV2Migration()
        {
            // create necessary folders
            string[] newFolders = new string[] { "Other Elements", "Customization" };
            foreach (string folder in newFolders)
            {
                string path = Path.Combine(ConfigDirectory, folder);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }

            // move files
            Dictionary<string, string> files = new Dictionary<string, string>()
            {
                ["Misc\\Experience Bar.json"] = "Other Elements\\Experience Bar.json",
                ["Misc\\GCD Indicator.json"]  = "Other Elements\\GCD Indicator.json",
                ["Misc\\Limit Break.json"]    = "Other Elements\\Limit Break.json",
                ["Misc\\MP Ticker.json"]      = "Other Elements\\MP Ticker.json",
                ["Misc\\Pull Timer.json"]     = "Other Elements\\Pull Timer.json",
                ["Misc\\Fonts.json"]          = "Customization\\Fonts.json"
            };

            foreach (string key in files.Keys)
            {
                string v1Path = Path.Combine(ConfigDirectory, key);
                string v2Path = Path.Combine(ConfigDirectory, files[key]);

                try
                {
                    if (File.Exists(v1Path) && !File.Exists(v2Path))
                    {
                        File.Move(v1Path, v2Path);
                    }
                }
                catch (Exception e)
                {
                    PluginLog.Error("Error migrating file \"" + v1Path + "\" to v2 config structure: " + e.Message);
                }
            }
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

        public void OnProfileDeleted(string profileName)
        {
            try
            {
                _configBaseNodeByProfile.Remove(profileName);
            }
            catch { }
        }

        public bool ImportProfile(string oldProfileName, string profileName, string rawString, bool forceLoad = false)
        {
            // cache old profile
            _configBaseNodeByProfile[oldProfileName] = ConfigBaseNode;

            // load profile from cache or from rawString
            BaseNode? loadedNode = null;
            if (forceLoad || !_configBaseNodeByProfile.TryGetValue(profileName, out loadedNode))
            {
                ImportProfileNonCached(rawString, out loadedNode);
            }

            if (loadedNode == null) { return false; }

            if (IsConfigWindowOpened || string.IsNullOrEmpty(loadedNode.SelectedOptionName))
            {
                loadedNode.SelectedOptionName = ConfigBaseNode.SelectedOptionName;
                loadedNode.RefreshSelectedNode();
            }

            ConfigBaseNode.ConfigObjectResetEvent -= OnConfigObjectReset;
            ConfigBaseNode = loadedNode;
            ConfigBaseNode.ConfigObjectResetEvent += OnConfigObjectReset;

            PerformV2Migration();

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

            if (ProfilesManager.Instance != null)
            {
                node.AddExtraSectionNode(ProfilesManager.Instance.ProfilesNode);
            }

            return true;
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
            // Unit Frames
            typeof(PlayerUnitFrameConfig),
            typeof(TargetUnitFrameConfig),
            typeof(TargetOfTargetUnitFrameConfig),
            typeof(FocusTargetUnitFrameConfig),

            // Mana Bars
            typeof(PlayerPrimaryResourceConfig),
            typeof(TargetPrimaryResourceConfig),
            typeof(TargetOfTargetPrimaryResourceConfig),
            typeof(FocusTargetPrimaryResourceConfig),
            
            // Castbars
            typeof(PlayerCastbarConfig),
            typeof(TargetCastbarConfig),
            typeof(TargetOfTargetCastbarConfig),
            typeof(FocusTargetCastbarConfig),

            // Buffs and Debuffs
            typeof(PlayerBuffsListConfig),
            typeof(PlayerDebuffsListConfig),
            typeof(TargetBuffsListConfig),
            typeof(TargetDebuffsListConfig),
            typeof(FocusTargetBuffsListConfig),
            typeof(FocusTargetDebuffsListConfig),
            typeof(CustomEffectsListConfig),

            // Nameplates
            typeof(NameplatesGeneralConfig),
            typeof(PlayerNameplateConfig),
            typeof(EnemyNameplateConfig),
            typeof(PartyMembersNameplateConfig),
            typeof(AllianceMembersNameplateConfig),
            typeof(FriendPlayerNameplateConfig),
            typeof(OtherPlayerNameplateConfig),
            typeof(PetNameplateConfig),
            typeof(NPCNameplateConfig),
            typeof(MinionNPCNameplateConfig),
            typeof(ObjectsNameplateConfig),

            // Party Frames
            typeof(PartyFramesConfig),
            typeof(PartyFramesHealthBarsConfig),
            typeof(PartyFramesManaBarConfig),
            typeof(PartyFramesCastbarConfig),
            typeof(PartyFramesIconsConfig),
            typeof(PartyFramesBuffsConfig),
            typeof(PartyFramesDebuffsConfig),
            typeof(PartyFramesTrackersConfig),
            typeof(PartyFramesCooldownListConfig),

            // Party Cooldowns
            typeof(PartyCooldownsConfig),
            typeof(PartyCooldownsBarConfig),
            typeof(PartyCooldownsDataConfig),

            // Enemy List
            typeof(EnemyListConfig),
            typeof(EnemyListHealthBarConfig),
            typeof(EnemyListEnmityIconConfig),
            typeof(EnemyListSignIconConfig),
            typeof(EnemyListCastbarConfig),
            typeof(EnemyListBuffsConfig),
            typeof(EnemyListDebuffsConfig),

            // Job Specific Bars
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

            // Other Elements
            typeof(ExperienceBarConfig),
            typeof(GCDIndicatorConfig),
            typeof(PullTimerConfig),
            typeof(LimitBreakConfig),
            typeof(MPTickerConfig),

            // Colors
            typeof(TanksColorConfig),
            typeof(HealersColorConfig),
            typeof(MeleeColorConfig),
            typeof(RangedColorConfig),
            typeof(CastersColorConfig),
            typeof(RolesColorConfig),
            typeof(MiscColorConfig),

            // Customization
            typeof(FontsConfig),
            typeof(BarTexturesConfig),

            // Visibility
            typeof(GlobalVisibilityConfig),
            typeof(HotbarsVisibilityConfig),

            // Misc
            typeof(HUDOptionsConfig),
            typeof(WindowClippingConfig),
            typeof(TooltipsConfig),
            typeof(GridConfig),

            // Import
            typeof(ImportConfig)
        };
        #endregion
    }
}