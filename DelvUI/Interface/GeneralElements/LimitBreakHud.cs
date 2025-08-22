using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace DelvUI.Interface.GeneralElements
{
    public class LimitBreakHud : DraggableHudElement, IHudElementWithActor, IHudElementWithVisibilityConfig
    {
        private LimitBreakConfig Config => (LimitBreakConfig)_config;
        public VisibilityConfig VisibilityConfig => Config.VisibilityConfig;

        public IGameObject? Actor { get; set; } = null;

        public LimitBreakHud(LimitBreakConfig config, string displayName) : base(config, displayName) { }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            return (new List<Vector2>() { Config.Position }, new List<Vector2>() { Config.Size });
        }

        public override unsafe void DrawChildren(Vector2 origin)
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

            LimitBreakController* lbController = LimitBreakController.Instance();
            int currentLimitBreak = helper.LimitBreakActive ? helper.LimitBreakBarWidth.Sum() : 0;
            int maxLimitBreak = helper.LimitBreakMaxLevel * helper.MaxLimitBarWidth;
            int limitBreakChunks = helper.LimitBreakActive ? helper.LimitBreakMaxLevel : 3;

            Plugin.Logger.Info($"Helper Limit Break: {currentLimitBreak}, Max Limit Break: {maxLimitBreak}, Chunks: {limitBreakChunks}");
            Plugin.Logger.Info($"Controller Limit Break: {lbController->CurrentUnits}, Max Limit Break: {lbController->BarUnits}, Chunks: {lbController->BarCount}");

            Config.Label.SetValue(helper.LimitBreakLevel / limitBreakChunks);

            BarHud[] bars = BarUtilities.GetChunkedProgressBars(Config, limitBreakChunks, currentLimitBreak, maxLimitBreak);

            foreach (BarHud bar in bars)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.StrataLevel));
            }
        }
    }
}
