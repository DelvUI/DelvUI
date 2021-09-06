using Dalamud.Game.ClientState.Actors.Types;
using DelvUI.Config;
using System.Numerics;

namespace DelvUI.Interface
{
    public abstract class HudElement
    {
        protected MovablePluginConfigObject _config;
        public string ID { get; private set; }

        public HudElement(string id, MovablePluginConfigObject config)
        {
            _config = config;
            ID = id;
        }

        public abstract void Draw(Vector2 origin);
    }

    public interface IHudElementWithActor
    {
        public Actor Actor { get; set; }
    }
}
