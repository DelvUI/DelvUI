using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Dalamud.Logging;

namespace DelvUI.Interface.GeneralElements
{
    public class LimitBreakHud : DraggableHudElement, IHudElementWithActor
    {
        private LimitBreakConfig Config => (LimitBreakConfig)_config;

        public GameObject? Actor { get; set; } = null;

        public LimitBreakHud(LimitBreakConfig config, string displayName) : base(config, displayName) { }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            return (new List<Vector2>() { Config.Position }, new List<Vector2>() { Config.Size });
        }

        public override void DrawChildren(Vector2 origin)
        {
            LimitBreakHelper helper = LimitBreakHelper.Instance;

            Config.Label.SetText("");

            if (!Config.Enabled)
            {
                return;
            }

            if (Config.HideWhenInactive && !helper.LimitBreakActive)
            {
                return;
            }

            int currentLimitBreak = helper.LimitBreakActive ? helper.LimitBreakBarWidth.Sum() : 0;
            int maxLimitBreak = helper.LimitBreakMaxLevel * helper.MaxLimitBarWidth;
            int limitBreakChunks = helper.LimitBreakActive ? helper.LimitBreakMaxLevel : 3;

            Config.Label.SetValue(helper.LimitBreakLevel / limitBreakChunks);

            BarUtilities.GetChunkedProgressBars(Config, limitBreakChunks, currentLimitBreak, maxLimitBreak).Draw(origin);
        }
    }


}
