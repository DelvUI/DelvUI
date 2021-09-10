using Dalamud.Plugin;
using DelvUI.Config.Tree;
using DelvUI.Interface.GeneralElements;
using DelvUI.Interface.Jobs;
using DelvUI.Interface.StatusEffects;
using ImGuiScene;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace DelvUI.Config
{
    public class ConfigurationManager
    {
        private static ConfigurationManager _instance;

        public readonly TextureWrap BannerImage;

        public BaseNode ConfigBaseNode;

        public string ConfigDirectory;
        public bool DrawConfigWindow;

        public bool LockHUD = true;
        public bool ShowHUD = true;

        public ConfigurationManager(bool defaultConfig, TextureWrap bannerImage, string configDirectory, BaseNode configBaseNode)
        {
            BannerImage = bannerImage;
            ConfigDirectory = configDirectory;
            ConfigBaseNode = configBaseNode;
            _instance = this;
            if (!defaultConfig)
            {
                LoadConfigurations();
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

                typeof(PlayerBuffsListConfig),
                typeof(PlayerDebuffsListConfig),
                typeof(TargetBuffsListConfig),
                typeof(TargetDebuffsListConfig),

                typeof(DarkKnightConfig),
                typeof(PaladinConfig),
                typeof(WarriorConfig),
                typeof(GunbreakerConfig),

                typeof(WhiteMageConfig),
                typeof(ScholarConfig),
                typeof(AstrologianConfig),

                typeof(MonkConfig),
                typeof(DragoonConfig),
                typeof(NinjaConfig),
                typeof(SamuraiConfig),

                typeof(MachinistConfig),
                typeof(DancerConfig),
                typeof(BardConfig),

                typeof(BlackMageConfig),
                typeof(RedMageConfig),
                typeof(SummonerConfig),

                typeof(TanksColorConfig),
                typeof(HealersColorConfig),
                typeof(MeleeColorConfig),
                typeof(RangedColorConfig),
                typeof(CastersColorConfig),
                typeof(MiscColorConfig),

                typeof(PrimaryResourceConfig),
                typeof(GCDIndicatorConfig),
                typeof(MPTickerConfig)
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

            return new ConfigurationManager(defaultConfig, banner, Plugin.GetPluginInterface().GetPluginConfigDirectory(), node);
        }

        public static ConfigurationManager GetInstance() => _instance;

        public void Draw()
        {
            if (DrawConfigWindow)
            {
                ConfigBaseNode.Draw();
            }
        }

        public void LoadConfigurations() { ConfigBaseNode.Load(ConfigDirectory); }

        public void SaveConfigurations() { ConfigBaseNode.Save(ConfigDirectory); }

        public PluginConfigObject GetConfiguration(PluginConfigObject configObject)
        {
            return GetConfigObjectForType(configObject.GetType());
        }
        public PluginConfigObject GetConfigObjectForType(Type type)
        {
            var genericMethod = GetType().GetMethod("GetConfigObject");
            var method = genericMethod.MakeGenericMethod(type);
            return (PluginConfigObject)method.Invoke(this, null);
        }
        public T GetConfigObject<T>() where T : PluginConfigObject => ConfigBaseNode.GetConfigObject<T>();

        public static string CompressAndBase64Encode(string jsonString)
        {
            using MemoryStream output = new();

            using (DeflateStream gzip = new(output, CompressionLevel.Fastest))
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

        public static T LoadImportString<T>(string importString) where T : PluginConfigObject
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
