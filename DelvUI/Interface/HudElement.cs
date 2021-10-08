using DelvUI.Config;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using System;

namespace DelvUI.Interface
{
    public abstract class HudElement : IDisposable
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

        ~HudElement()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            InternalDispose();
        }

        protected virtual void InternalDispose()
        {
            // override
        }
    }

    public interface IHudElementWithActor
    {
        public GameObject? Actor { get; set; }
    }

    public interface IHudElementWithAnchorableParent
    {
        public AnchorablePluginConfigObject? ParentConfig { get; set; }
    }
}
