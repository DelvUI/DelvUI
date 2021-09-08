using Dalamud.Plugin;
using DelvUI.Config.Tree;
using DelvUI.Interface;
using ImGuiScene;

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
                new BlackMageHudConfig(), new SummonerHudConfig(), new RedMageHudConfig()
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
    }
}
