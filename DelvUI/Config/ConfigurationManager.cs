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

                if (!_drawConfigWindow && ConfigBaseNode.NeedsSave)
                {
                    SaveConfigurations();
                }
            }
        }

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

            LoadConfigurations();

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

            // ProfilesConfig is special because its not loaded and saved like other configs
            // and its not stored in the profile's data
            // its loading and saving is handled by the config itself
            // so we need to make sure we load the "real" config once the nodes are loaded
            var profilesConfig = ProfilesConfig.Load();
            if (profilesConfig != null)
            {
                Instance.SetConfigObject(profilesConfig);
            }
            else
            {
                profilesConfig = Instance.GetConfigObject<ProfilesConfig>();
            }

            profilesConfig?.Initialize();
        }

        private void OnConfigObjectReset(BaseNode sender)
        {
            ResetEvent?.Invoke(this);
        }

        private void OnLogout(object? sender, EventArgs? args)
        {
            SaveConfigurations();
            SaveCurrentProfile();
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
            SaveCurrentProfile();

            ConfigBaseNode.NeedsSave = false;
        }

        private void SaveCurrentProfile()
        {
            var profilesConfig = GetConfigObject<ProfilesConfig>();
            profilesConfig.SaveCurrentProfile(ExportCurrentConfigs());
        }

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

        #region export
        public static string CompressAndBase64Encode(string jsonString)
        {
            using MemoryStream output = new();

            using (DeflateStream gzip = new(output, CompressionLevel.Optimal))
            {
                using StreamWriter writer = new(gzip, Encoding.UTF8);
                writer.Write(jsonString);
            }

            return Convert.ToBase64String(output.ToArray());
        }

        public static string Base64DecodeAndDecompress(string base64String)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64String);

            using MemoryStream inputStream = new(base64EncodedBytes);
            using DeflateStream gzip = new(inputStream, CompressionMode.Decompress);
            using StreamReader reader = new(gzip, Encoding.UTF8);
            var decodedString = reader.ReadToEnd();

            return decodedString;
        }

        public static string GenerateExportString(object obj)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                TypeNameHandling = TypeNameHandling.Objects
            };

            var jsonString = JsonConvert.SerializeObject(obj, Formatting.Indented, settings);
            return CompressAndBase64Encode(jsonString);
        }

        public string? ExportCurrentConfigs()
        {
            return ConfigBaseNode.GetBase64String();
        }

        public bool ImportProfile(string rawString)
        {
            List<string> importStrings = new List<string>(rawString.Trim().Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries));
            ImportData[] imports = importStrings.Select(str => new ImportData(str)).ToArray();

            ProfilesConfig profilesConfig = GetConfigObject<ProfilesConfig>();
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
            ConfigBaseNode = node;
            SetConfigObject(profilesConfig); // dont overwrite profiles config

            ResetEvent?.Invoke(this);

            return true;
        }

        public void ResetConfig()
        {
            ProfilesConfig profilesConfig = GetConfigObject<ProfilesConfig>();
            ConfigBaseNode.Reset();
            SetConfigObject(profilesConfig); // dont overwrite profiles config

            ResetEvent?.Invoke(this);
        }

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

            typeof(PartyFramesConfig),
            typeof(PartyFramesHealthBarsConfig),
            typeof(PartyFramesManaBarConfig),
            typeof(PartyFramesCastbarConfig),
            typeof(PartyFramesRoleIconConfig),
            typeof(PartyFramesLeaderIconConfig),
            typeof(PartyFramesBuffsConfig),
            typeof(PartyFramesDebuffsConfig),
            typeof(PartyFramesRaiseTrackerConfig),

            typeof(PlayerCastbarConfig),
            typeof(TargetCastbarConfig),
            typeof(TargetOfTargetCastbarConfig),
            typeof(FocusTargetCastbarConfig),

            typeof(PlayerBuffsListConfig),
            typeof(PlayerDebuffsListConfig),
            typeof(TargetBuffsListConfig),
            typeof(TargetDebuffsListConfig),
            typeof(CustomEffectsListConfig),

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
            typeof(PrimaryResourceConfig),
            typeof(TooltipsConfig),
            typeof(GCDIndicatorConfig),
            typeof(MPTickerConfig),
            typeof(GridConfig),

            typeof(ImportConfig),
            typeof(ProfilesConfig)
        };
        #endregion
    }
}