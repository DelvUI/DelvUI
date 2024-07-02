using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin;
using System.Numerics;

namespace DelvUI.Interface.Jobs
{
    public class JobHud : DraggableHudElement, IHudElementWithActor, IHudElementWithVisibilityConfig
    {
        protected IDalamudPluginInterface PluginInterface => Plugin.PluginInterface;

        public JobConfig Config => (JobConfig)_config;
        public VisibilityConfig VisibilityConfig => Config.VisibilityConfig;

        public IGameObject? Actor { get; set; } = null;
        protected IPlayerCharacter? Player => Actor is IPlayerCharacter ? (IPlayerCharacter)Actor : null;

        public JobHud(JobConfig config, string? displayName = null) : base(config, displayName)
        {
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (Player == null || !_config.Enabled)
            {
                return;
            }

            DrawJobHud(origin, Player);
        }

        public virtual void DrawJobHud(Vector2 origin, IPlayerCharacter player)
        {
            // override
        }
    }
}
