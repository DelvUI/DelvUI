using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Plugin;
using DelvUI.Config;
using System.Numerics;

namespace DelvUI.Interface.Jobs
{
    public class JobHud : HudElement, IHudElementWithActor
    {
        protected PluginConfiguration PluginConfiguration;
        protected DalamudPluginInterface PluginInterface => Plugin.GetPluginInterface();

        public JobConfig Config => (JobConfig)_config;

        public Actor Actor { get; set; } = null;

        public JobHud(string ID, JobConfig config, PluginConfiguration pluginConfiguration) : base(ID, config)
        {
            // NOTE: Temporary due to fonts
            PluginConfiguration = pluginConfiguration;
        }

        public override void Draw(Vector2 origin)
        {
        }
    }
}
