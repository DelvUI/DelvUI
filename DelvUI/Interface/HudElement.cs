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

        private Dictionary<StrataLevel, List<Action>> _drawActions = new Dictionary<StrataLevel, List<Action>>();

        public HudElement(MovablePluginConfigObject config)
        {
            _config = config;
        }

        public void PrepareForDraw(Vector2 origin)
        {
            _drawActions.Clear();
            CreateDrawActions(origin);
        }

        public virtual void Draw(Vector2 origin)
        {
            // iterate like this so it goes in order
            StrataLevel[] levels = (StrataLevel[])Enum.GetValues(typeof(StrataLevel));
            foreach (StrataLevel key in levels)
            {
                _drawActions.TryGetValue(key, out List<Action>? drawActions);
                if (drawActions == null) { continue; }

                foreach (Action drawAction in _drawActions[key])
                {
                    drawAction();
                }
            }
        }

        protected void AddDrawAction(StrataLevel strataLevel, Action drawAction)
        {
            _drawActions.TryGetValue(strataLevel, out List<Action>? drawActions);

            if (drawActions == null)
            {
                drawActions = new List<Action>();
                _drawActions.Add(strataLevel, drawActions);
            }

            drawActions.Add(drawAction);
        }

        protected void AddDrawActions(List<(StrataLevel, Action)> drawActions)
        {
            foreach ((StrataLevel strataLevel, Action drawAction) in drawActions)
            {
                AddDrawAction(strataLevel, drawAction);
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
