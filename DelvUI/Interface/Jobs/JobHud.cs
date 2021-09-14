using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Plugin;
using System.Collections.Generic;
using System.Numerics;

namespace DelvUI.Interface.Jobs
{
    public class JobHud : DraggableHudElement, IHudElementWithActor
    {
        protected DalamudPluginInterface PluginInterface => Plugin.PluginInterface;

        public JobConfig Config => (JobConfig)_config;

        public Actor Actor { get; set; } = null;

        public JobHud(string ID, JobConfig config, string displayName = null) : base(ID, config, displayName)
        {
        }

        public override void DrawChildren(Vector2 origin)
        {
        }
    }
}
