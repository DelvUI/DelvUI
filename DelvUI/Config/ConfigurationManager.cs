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
using Dalamud.Logging;

namespace DelvUI.Config
{
    public delegate void ConfigurationManagerEventHandler(ConfigurationManager configurationManager);

    public class ConfigurationManager
    {
        private static ConfigurationManager _instance = null!;

        public readonly TextureWrap? BannerImage;

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
            bool defaultConfig,
            TextureWrap? bannerImage,
            string configDirectory,
            BaseNode configBaseNode,
            ConfigurationManagerEventHandler? resetEvent = null,
            ConfigurationManagerEventHandler? lockEvent = null)
        {
            BannerImage = bannerImage;
            ConfigDirectory = configDirectory;
            ConfigBaseNode = configBaseNode;
            _instance = this;

            if (!defaultConfig)
            {
                LoadConfigurations();
            }

            LockEvent = lockEvent;

            ResetEvent = resetEvent;
            ResetEvent?.Invoke(this);
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

                typeof(ImportExportConfig)
            };

            return Initialize(defaultConfig, configObjects);
        }

        public static ConfigurationManager Initialize(bool defaultConfig, params Type[] configObjectTypes)
        {
            BaseNode node = new();

            foreach (Type type in configObjectTypes)
            {
                var genericMethod = node.GetType().GetMethod("GetOrAddConfig");
                var method = genericMethod?.MakeGenericMethod(type);
                method?.Invoke(node, null);
            }

            TextureWrap? banner = Plugin.BannerTexture;

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

        public void SaveConfigurations() { ConfigBaseNode.Save(ConfigDirectory); }

        public PluginConfigObject GetConfigObjectForType(Type type)
        {
            MethodInfo? genericMethod = GetType().GetMethod("GetConfigObject");
            MethodInfo? method = genericMethod?.MakeGenericMethod(type);
            return (PluginConfigObject)method?.Invoke(this, null)!;
        }
        public T GetConfigObject<T>() where T : PluginConfigObject => ConfigBaseNode.GetConfigObject<T>()!;
        public ConfigPageNode GetConfigPageNode<T>() where T : PluginConfigObject => ConfigBaseNode.GetConfigPageNode<T>()!;

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

        public static string GenerateExportString(PluginConfigObject configObject)
        {
            var jsonString = JsonConvert.SerializeObject(configObject, Formatting.Indented,
                new JsonSerializerSettings { TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple, TypeNameHandling = TypeNameHandling.Objects });

            return CompressAndBase64Encode(jsonString);
        }

        public static string ExportBaseNode(BaseNode baseNode)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                TypeNameHandling = TypeNameHandling.Objects
            };
            var jsonString = JsonConvert.SerializeObject(baseNode, Formatting.Indented, settings);
            ImGui.SetClipboardText(jsonString);
            return CompressAndBase64Encode(jsonString);
        }

        public static void LoadImportedConfiguration(string importString, ConfigPageNode configPageNode)
        {
            // see comments on ConfigPageNode's Load
            MethodInfo? methodInfo = typeof(ConfigurationManager).GetMethod("LoadImportString");
            MethodInfo? function = methodInfo?.MakeGenericMethod(configPageNode.ConfigObject.GetType());
            PluginConfigObject? importedConfigObject = (PluginConfigObject?)function?.Invoke(GetInstance(), new object[] { importString });

            if (importedConfigObject != null)
            {
                PluginLog.Log($"Importing {importedConfigObject.GetType()}");
                // update the tree 
                configPageNode.ConfigObject = importedConfigObject;
                // but also update the dictionary
                GetInstance().ConfigBaseNode.configPageNodesMap[configPageNode.ConfigObject.GetType()] = configPageNode;
                GetInstance().SaveConfigurations();

                _instance.ResetEvent?.Invoke(_instance);
            }
            else
            {
                PluginLog.Log($"Could not load from import string (of type {configPageNode.ConfigObject.GetType()})");
            }
        }

        public static void LoadTotalConfiguration(string[] importStrings)
        {
            _instance.ConfigBaseNode.LoadBase64String(importStrings);
            _instance.ResetEvent?.Invoke(_instance);
        }

        public static T? LoadImportString<T>(string importString) where T : PluginConfigObject
        {
            try
            {
                var jsonString = Base64DecodeAndDecompress(importString);
                return JsonConvert.DeserializeObject<T>(jsonString);
            }
            catch (Exception ex)
            {
                PluginLog.Log(ex.Message + "\n" + ex.StackTrace);

                return default;
            }
        }
    }
}