using Dalamud.Plugin;
using DelvUI.Config.Tree;
using DelvUI.Interface;
using ImGuiScene;

namespace DelvUI.Config
{
    public class ConfigurationManager
    {
        private static ConfigurationManager _instance;

        public TextureWrap BannerImage;

        public string ConfigDirectory;

        public BaseNode ConfigBaseNode;
        public bool DrawConfigWindow;

        public ConfigurationManager(TextureWrap bannerImage, string configDirectory, BaseNode configBaseNode)
        {
            BannerImage = bannerImage;
            ConfigDirectory = configDirectory;
            ConfigBaseNode = configBaseNode;
            _instance = this;
            LoadConfigurations();
        }

        public static ConfigurationManager Initialize(DalamudPluginInterface pluginInterface)
        {
            var configObjects = new PluginConfigObject[]
            {
                new AstrologianHudConfig(),
                new BardHudConfig(),
                new BlackMageHudConfig(),
                new DancerHudConfig(),
                new DarkKnightHudConfig(),
                new DragoonHudConfig(),
                new GunbreakerHudConfig(),
                new NinjaHudConfig(),
                new PaladinHudConfig(),
                new RedMageHudConfig(),
                new SummonerHudConfig(),
                new WarriorHudConfig(),
                new WhiteMageHudConfig(),
                new MonkHudConfig(),
            };

            return Initialize(pluginInterface, configObjects);
        }

        public static ConfigurationManager Initialize(DalamudPluginInterface pluginInterface, params PluginConfigObject[] configObjects)
        {
            BaseNode node = new BaseNode();

            foreach (PluginConfigObject configObject in configObjects)
            {
                node.GetOrAddConfig(configObject);
            }

            TextureWrap banner = BuildBanner(pluginInterface);

            return new ConfigurationManager(banner, pluginInterface.GetPluginConfigDirectory(), node);
        }

        public static ConfigurationManager GetInstance() => _instance;

        private static TextureWrap BuildBanner(DalamudPluginInterface pluginInterface) =>
            // var bannerImage = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "Media", "Images", "banner_short_x150.png");
            //
            // if (File.Exists(bannerImage)) {
            //     try {
            //         return pluginInterface.UiBuilder.LoadImage(bannerImage);
            //     } catch (Exception ex) {
            //         PluginLog.Log($"Image failed to load. {bannerImage}");
            //         PluginLog.Log(ex.ToString());
            //     }
            // } else {
            //     PluginLog.Log($"Image doesn't exist. {bannerImage}");
            // }
            null;

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
