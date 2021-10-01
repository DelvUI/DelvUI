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

        public JobHud(string id, JobConfig config, string? displayName = null) : base(id, config, displayName)
        {
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (Player == null)
            {
                return;
            }

            // TODO: Do this properly...
            // I'm reusing the same calculations for the draggable areas
            // but these usually don't take into account job bars that are not always present
            // so I can't exactly use those areas for the window otherwise some elements
            // will be drawn outside of it and they won't be visible.
            // For now, I'm taking the area and making it bigger...

            var margin = new Vector2(200, 200);
            var size = MaxPos - MinPos + margin * 2;

            DrawHelper.DrawInWindow(ID, origin + MinPos - margin, size, false, false, (drawList) =>
            {
                DrawJobHud(origin, Player);
            });
        }

        public virtual void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            // override
        }
    }
}
