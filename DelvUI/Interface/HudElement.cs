using DelvUI.Config;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using System;
using System.Collections.Generic;
using DelvUI.Enums;

namespace DelvUI.Interface
{
    public abstract class HudElement : IDisposable
    {
        protected MovablePluginConfigObject _config;
        public MovablePluginConfigObject GetConfig() { return _config; }

        public string ID => _config.ID;

        private SortedList<PluginConfigObject, Action> _drawActions = new SortedList<PluginConfigObject, Action>(new StrataLevelComparer<PluginConfigObject>());

        public HudElement(MovablePluginConfigObject config)
        {
            _config = config;
        }

        public virtual void Draw(Vector2 origin)
        {
            _drawActions.Clear();
            CreateDrawActions(origin);

            foreach (Action drawAction in _drawActions.Values)
            {
                drawAction();
            }
        }

        protected void AddDrawAction(PluginConfigObject config, Action drawAction)
        {
            _drawActions.Add(config, drawAction);
        }

        protected void AddDrawActions(List<(PluginConfigObject, Action)> drawActions)
        {
            foreach ((PluginConfigObject config, Action drawAction) in drawActions)
            {
                _drawActions.Add(config, drawAction);
            }
        }

        protected abstract void CreateDrawActions(Vector2 origin);

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

    public interface IHudElementWithMouseOver
    {
        public void StopMouseover();
    }

    public interface IHudElementWithPreview
    {
        public void StopPreview();
    }
}
