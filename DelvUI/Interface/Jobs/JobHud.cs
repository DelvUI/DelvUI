using Dalamud.Plugin;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;

namespace DelvUI.Interface.Jobs
{
    public class JobHud : DraggableHudElement, IHudElementWithActor
    {
        protected DalamudPluginInterface PluginInterface => Plugin.PluginInterface;

        public JobConfig Config => (JobConfig)_config;

        public GameObject? Actor { get; set; } = null;

        public JobHud(string id, JobConfig config, string? displayName = null) : base(id, config, displayName)
        {
        }

        public override void DrawChildren(Vector2 origin)
        {
        }
    }
}
