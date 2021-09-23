using Dalamud.Plugin;
using DelvUI.Config.Tree;
using DelvUI.Helpers;
using DelvUI.Interface;
using DelvUI.Interface.GeneralElements;
using DelvUI.Interface.Jobs;
using DelvUI.Interface.StatusEffects;
using ImGuiNET;
using ImGuiScene;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;

namespace DelvUI.Config
{
    public class ConfigurationManager
    {
        private static ConfigurationManager _instance;

        public readonly TextureWrap BannerImage;

        public BaseNode ConfigBaseNode;

        public GradientDirection GradientDirection
        {
            get
            {
                var config = GetInstance().GetConfigObject<MiscColorConfig>();
                return config != null ? config.GradientDirection : GradientDirection.None;
            }
        }

        public string ConfigDirectory;
        public bool DrawConfigWindow;

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

                if (LockEvent != null)
                {
                    LockEvent(this, null);
                }

                if (_lockHUD)
                {
                    SaveConfigurations();
                }
            }
        }

        public bool ShowHUD = true;

        public event EventHandler ResetEvent;
        public event EventHandler LockEvent;

        public ConfigurationManager(bool defaultConfig, TextureWrap bannerImage, string configDirectory, BaseNode configBaseNode, EventHandler resetEvent = null, EventHandler lockEvent = null)
        {
            BannerImage = bannerImage;
            ConfigDirectory = configDirectory;
            ConfigBaseNode = configBaseNode;
            _instance = this;

            ((ProfilesConfig)ConfigBaseNode.GetOrAddConfig<ProfilesConfig>().ConfigObject).GenerateDefaultsProfile(ConfigBaseNode);

            if (!defaultConfig)
            {
                LoadConfigurations();
            }

            LockEvent = lockEvent;

            ResetEvent = resetEvent;
            if (ResetEvent != null)
            {
                ResetEvent(this, null);
            }
        }


        public static ConfigurationManager Initialize(bool defaultConfig)
        {
            Type[] configObjects =
            {
                typeof(PlayerUnitFrameConfig),
                typeof(TargetUnitFrameConfig),
                typeof(TargetOfTargetUnitFrameConfig),
                typeof(FocusTargetUnitFrameConfig),

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

                typeof(ProfilesConfig)
            };

            return Initialize(defaultConfig, configObjects);
        }

        public static ConfigurationManager Initialize(bool defaultConfig, params Type[] configObjectTypes)
        {
            BaseNode node = new();

            foreach (Type type in configObjectTypes)
            {
                var genericMethod = node.GetType().GetMethod("GetOrAddConfig");
                var method = genericMethod.MakeGenericMethod(type);
                method.Invoke(node, null);
            }

            TextureWrap banner = Plugin.bannerTexture;

            var currentResetEvent = GetInstance()?.ResetEvent;
            var currentLockEvent = GetInstance()?.LockEvent;
            return new ConfigurationManager(defaultConfig, banner, Plugin.PluginInterface.GetPluginConfigDirectory(), node, currentResetEvent, currentLockEvent);
        }

        public static ConfigurationManager GetInstance() => _instance;

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

        public void LoadConfigurations() { ConfigBaseNode.Load(ConfigDirectory); }

        public void SaveConfigurations()
        {
            ConfigBaseNode.Save(ConfigDirectory);

            // update the profile dictionary
            ProfilesConfig profilesConfig = (ProfilesConfig)GetConfigObjectForType(typeof(ProfilesConfig));
            profilesConfig?.UpdateCurrentProfile(ConfigBaseNode);
        }

        public PluginConfigObject GetConfigObjectForType(Type type)
        {
            MethodInfo genericMethod = GetType().GetMethod("GetConfigObject");
            MethodInfo method = genericMethod.MakeGenericMethod(type);
            return (PluginConfigObject)method.Invoke(this, null);
        }
        public T GetConfigObject<T>() where T : PluginConfigObject => ConfigBaseNode.GetConfigObject<T>();
        public ConfigPageNode GetConfigPageNode<T>() where T : PluginConfigObject => ConfigBaseNode.GetConfigPageNode<T>();

        public static string GenerateJsonString(PluginConfigObject configObject)
        {
            var jsonString = JsonConvert.SerializeObject(configObject, Formatting.Indented,
                new JsonSerializerSettings { TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple, TypeNameHandling = TypeNameHandling.Objects });

            return jsonString;
        }

        public static void LoadImportedConfiguration(string importString, ConfigPageNode configPageNode)
        {
            // see comments on ConfigPageNode's Load
            MethodInfo methodInfo = typeof(ImportExportHelper).GetMethod("LoadImportString");
            MethodInfo function = methodInfo.MakeGenericMethod(configPageNode.ConfigObject.GetType());
            PluginConfigObject importedConfigObject = (PluginConfigObject)function.Invoke(null, new object[] { importString });

            if (importedConfigObject != null)
            {
                // update the tree 
                configPageNode.ConfigObject = importedConfigObject;
                // but also update the dictionary
                GetInstance().ConfigBaseNode.configPageNodesMap[configPageNode.ConfigObject.GetType()] = configPageNode;
                GetInstance().SaveConfigurations();
                _instance.ResetEvent(_instance, null);
            }
            else
            {
                PluginLog.Log($"Could not load from import string (of type {importedConfigObject.GetType()})");
            }
        }

        public static void LoadTotalConfiguration(string[] importStrings)
        {
            _instance.ConfigBaseNode.LoadJsonStrings(importStrings);
            _instance.ResetEvent(_instance, null);
        }

        // loads configuration from profile matching a job ID (if one exists)
        // if it's already the current profile, do nothing
        public void LoadProfile(uint jobID)
        {
            ProfilesConfig profilesConfig = (ProfilesConfig)GetConfigObjectForType(typeof(ProfilesConfig));
            if (profilesConfig == null)
            {
                return;
            }

            string profileName;
            if (!profilesConfig.JobProfileMap.TryGetValue(jobID, out profileName) || profilesConfig.CurrentProfile == profileName)
            {
                return;
            }

            string profileString;
            if (!profilesConfig.Profiles.TryGetValue(profileName, out profileString))
            {
                return;
            }

            string[] importStrings = ImportExportHelper.Base64DecodeAndDecompress(profileString).Split(new string[] { ImportExportHelper.Separator }, StringSplitOptions.RemoveEmptyEntries);
            LoadTotalConfiguration(importStrings);
            profilesConfig.CurrentProfile = profileName;
        }
    }
}
