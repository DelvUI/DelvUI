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
            
            Config.Label.SetText($"{helper.CountDownValue.ToString(Config.ShowDecimals ? "N2" : "N0", CultureInfo.InvariantCulture),0}");

            if (!helper.CountingDown)
            {
                Config.Label.SetText("");
            }
            
            if (!Config.Enabled)
            {
                return;
            }

            if (Config.HideWhenInactive && !helper.CountingDown)
            {
                return;
            }
            
            BarUtilities.GetProgressBar(Config, 
                helper.CountDownValue,
                helper.CountDownMax).Draw(origin);
        }
    }


}
