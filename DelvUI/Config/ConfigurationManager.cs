using Dalamud.Logging;
using DelvUI.Config.Profiles;
using DelvUI.Config.Tree;
using DelvUI.Helpers;
using DelvUI.Interface;
using DelvUI.Interface.GeneralElements;
using DelvUI.Interface.Jobs;
using DelvUI.Interface.Party;
using DelvUI.Interface.StatusEffects;
using ImGuiScene;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DelvUI.Config
{
    public delegate void ConfigurationManagerEventHandler(ConfigurationManager configurationManager);

    public class ConfigurationManager : IDisposable
    {
        public static ConfigurationManager Instance { get; private set; } = null!;

        public readonly TextureWrap? BannerImage;
        public BaseNode ConfigBaseNode;

        public GradientDirection GradientDirection
        {
            get
            {
                var config = Instance.GetConfigObject<MiscColorConfig>();
                return config != null ? config.GradientDirection : GradientDirection.None;
            }
        }

        public string ConfigDirectory;

        private bool _drawConfigWindow;
        public bool DrawConfigWindow
        {
            get => _drawConfigWindow;
            set
            {
                if (_drawConfigWindow == value)
                {
                    return;
                }

                _drawConfigWindow = value;

                if (!_drawConfigWindow)
                {
                    if (ConfigBaseNode.NeedsSave)
                    {
                        SaveConfigurations();
                    }

                    if (_needsProfileUpdate)
                    {
                        UpdateCurrentProfile();
                        _needsProfileUpdate = false;
                    }
                }
            }
        }

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

        public ConfigurationManager(
            TextureWrap? bannerImage,
            string configDirectory,
            BaseNode configBaseNode,
            ConfigurationManagerEventHandler? resetEvent = null,
            ConfigurationManagerEventHandler? lockEvent = null)
        {
            BannerImage = bannerImage;
            ConfigDirectory = configDirectory;
            ConfigBaseNode = configBaseNode;
            ConfigBaseNode.ConfigObjectResetEvent += OnConfigObjectReset;

            LoadOrInitializeFiles();

            LockEvent = lockEvent;

            ResetEvent = resetEvent;
            ResetEvent?.Invoke(this);

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

        public static void Initialize()
        {
            BaseNode node = new();
            InitializeBaseNode(node);

            TextureWrap? banner = Plugin.BannerTexture;

            var currentResetEvent = (ConfigurationManagerEventHandler?)Instance?.ResetEvent?.Clone();
            var currentLockEvent = (ConfigurationManagerEventHandler?)Instance?.LockEvent?.Clone();

            Instance = new ConfigurationManager(
                banner,
                Plugin.PluginInterface.GetPluginConfigDirectory(),
                node,
                currentResetEvent,
                currentLockEvent
            );
        }

        private void OnConfigObjectReset(BaseNode sender)
        {
            ResetEvent?.Invoke(this);
        }

        private void OnLogout(object? sender, EventArgs? args)
        {
            SaveConfigurations();
            ProfilesManager.Instance.SaveCurrentProfile();
        }

        public void Draw()
        {
            if (DrawConfigWindow)
            {
                if (LockHUD)
                {
                    ConfigBaseNode.Draw();
                }
                else
                {
                    DraggablesHelper.DrawGridWindow();
                }

            }
        }

        public void AddExtraSectionNode(SectionNode node)
        {
            ConfigBaseNode.AddExtraSectionNode(node);
        }

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
            if (_drawConfigWindow)
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
            typeof(HideHudConfig),
            typeof(TooltipsConfig),
            typeof(ExperienceBarConfig),
            typeof(GCDIndicatorConfig),
            typeof(MPTickerConfig),
            typeof(GridConfig),
            typeof(PullTimerConfig),

            typeof(ImportConfig)
        };
        #endregion
    }
}