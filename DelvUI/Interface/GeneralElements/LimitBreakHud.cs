using System;
using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;

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
            Config.Label.SetText("");

            if (!Config.Enabled)
            {
                return;
            }

            LimitBreakController* lbController = LimitBreakController.Instance();
            AddonHWDAetherGauge* caGauge = (AddonHWDAetherGauge*) Plugin.GameGui.GetAddonByName("HWDAetherGauge", 1).Address;

            int currentLimitBreak = lbController->CurrentUnits;
            int maxLimitBreak = lbController->BarUnits * lbController->BarCount;
            int limitBreakChunks = lbController->BarCount;

            Plugin.Logger.Info($"LB: {currentLimitBreak}/{maxLimitBreak} ({limitBreakChunks})");

            if (caGauge != null)
            {
                currentLimitBreak = caGauge->MaxGaugeValue;
                maxLimitBreak = 1000;
                limitBreakChunks = 5;
            }

            int valuePerChunk = limitBreakChunks == 0 ? 0 : maxLimitBreak / limitBreakChunks;
            int currentChunksFilled = valuePerChunk == 0 ? 0 : currentLimitBreak / valuePerChunk;

            if (Config.HideWhenInactive && limitBreakChunks == 0)
            {
                return;
            }

            Config.Label.SetValue(currentChunksFilled);

            BarHud[] bars = BarUtilities.GetChunkedProgressBars(Config, limitBreakChunks, currentLimitBreak, maxLimitBreak);
            foreach (BarHud bar in bars)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.StrataLevel));
            }
        }
    }
}
