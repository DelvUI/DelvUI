using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin;
using DelvUI.Helpers;
using System.Numerics;

namespace DelvUI.Interface.Jobs
{
    public class JobHud : DraggableHudElement, IHudElementWithActor
    {
        protected DalamudPluginInterface PluginInterface => Plugin.PluginInterface;

        public JobConfig Config => (JobConfig)_config;

        public GameObject? Actor { get; set; } = null;
        protected PlayerCharacter? Player => Actor is PlayerCharacter ? (PlayerCharacter)Actor : null;

        public JobHud(JobConfig config, string? displayName = null) : base(config, displayName)
        {
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (Player == null || !_config.Enabled)
            {
                return;
            }

            //DrawJobHud(origin, Player);
        }

        public virtual void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            // override
        }
    }
}
