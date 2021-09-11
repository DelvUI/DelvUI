using DelvUI.Config;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;

namespace DelvUI.Interface
{
    public abstract class HudElement
    {
        protected MovablePluginConfigObject _config;
        public MovablePluginConfigObject GetConfig() { return _config; }

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
        public GameObject? Actor { get; set; }
    }
}
