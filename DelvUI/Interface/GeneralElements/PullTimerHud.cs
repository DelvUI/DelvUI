using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    public class PullTimerHud : DraggableHudElement, IHudElementWithActor
    {
        private PullTimerConfig Config => (PullTimerConfig)_config;

        public GameObject? Actor { get; set; } = null;

        public PullTimerHud(PullTimerConfig config, string displayName) : base(config, displayName) { }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            return (new List<Vector2>() { Config.Position }, new List<Vector2>() { Config.Size });
        }

        public override void DrawChildren(Vector2 origin)
        {
            PullTimerState helper = PullTimerHelper.Instance.PullTimerState;

            Config.Label.SetValue(helper.CountDownValue);

            if (!helper.CountingDown)
            {
                Config.Label.SetText("");
            }

            if (!Config.Enabled || Actor is null)
            {
                return;
            }

            if (Config.HideWhenInactive && !helper.CountingDown)
            {
                return;
            }

            PluginConfigColor? fillColor = Config.UseJobColor ? Utils.ColorForActor(Actor) : null;

            BarHud bar = BarUtilities.GetProgressBar(Config,
                helper.CountDownValue,
                helper.CountDownMax, 0F, Actor, fillColor);

            AddDrawActions(bar.GetDrawActions(origin, Config.StrataLevel));
        }
    }
}
