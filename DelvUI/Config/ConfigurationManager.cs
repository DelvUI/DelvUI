using Dalamud.Plugin;
using DelvUI.Config.Tree;
using DelvUI.Interface;
using ImGuiScene;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public ConfigurationWindow ConfigurationWindow { get; set; }

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
            PluginConfigObject[] configObjects =
            {
                new GeneralHudConfig(),
                new TankHudConfig(), new PaladinHudConfig(), new WarriorHudConfig(), new DarkKnightHudConfig(), new GunbreakerHudConfig(),
                new WhiteMageHudConfig(), new ScholarHudConfig(), new AstrologianHudConfig(),
                new MonkHudConfig(), new DragoonHudConfig(), new NinjaHudConfig(), new SamuraiHudConfig(),
                new BardHudConfig(), new MachinistHudConfig(), new DancerHudConfig(),
                new BlackMageHudConfig(), new SummonerHudConfig(), new RedMageHudConfig(),
                new ImportExportHudConfig()
            };

            return Initialize(defaultConfig, configObjects);
        }

        public static ConfigurationManager Initialize(bool defaultConfig, params PluginConfigObject[] configObjects)
        {
            BaseNode node = new();

            foreach (PluginConfigObject configObject in configObjects)
            {
                node.GetOrAddConfig(configObject);
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

        public PluginConfigObject GetConfiguration(PluginConfigObject configObject) => ConfigBaseNode.GetOrAddConfig(configObject).ConfigObject;

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
