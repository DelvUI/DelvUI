using DelvUI.Config;
using System.Numerics;

namespace DelvUI.Interface
{
    public abstract class HudElement
    {
        protected MovablePluginConfigObject _config;

        public HudElement(MovablePluginConfigObject config)
        {
            _config = config;
        }

        public abstract void Draw(Vector2 origin);
    }
}
