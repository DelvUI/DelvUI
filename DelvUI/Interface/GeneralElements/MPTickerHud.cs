using DelvUI.Helpers;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Interface.Bars;
using Dalamud.Game.ClientState.Objects.SubKinds;
using System.Linq;
using Dalamud.Game.ClientState.JobGauge.Types;

namespace DelvUI.Interface.GeneralElements
{
    public class MPTickerHud : DraggableHudElement, IHudElementWithActor
    {
        private MPTickerConfig Config => (MPTickerConfig)_config;

        private MPTickHelper _mpTickHelper = null!;
        public GameObject? Actor { get; set; } = null;

        public MPTickerHud(MPTickerConfig config, string displayName) : base(config, displayName) { }

        protected override void InternalDispose()
        {
            _mpTickHelper.Dispose();
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            return (new List<Vector2>() { Config.Position + Config.Bar.Position },
                    new List<Vector2>() { Config.Bar.Size });
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (!Config.Enabled || Actor == null || Actor is not PlayerCharacter player)
            {
                return;
            }

            // full mp
            if (Config.HideOnFullMP && player.CurrentMp >= player.MaxMp)
            {
                return;
            }

            // BLM specific settings
            if (Config.EnableOnlyForBLM)
            {
                if (player.ClassJob.Id != JobIDs.BLM)
                {

                    return;
                }
                else
                {
                    var gauge = Plugin.JobGauges.Get<BLMGauge>();
                    if (Config.ShowOnlyDuringUmbralIce && !gauge.InUmbralIce)
                    {
                        return;
                    }
                }
            }

            _mpTickHelper ??= new MPTickHelper();

            var now = ImGui.GetTime();
            var scale = (float)((now - _mpTickHelper.LastTick) / MPTickHelper.ServerTickRate);

            if (scale <= 0)
            {
                return;
            }

            if (scale > 1)
            {
                scale = 1;
            }

            MPTickerFire3ThresholdConfig? thresholdConfig = GetFire3ThresholdConfig();
            BarHud bar = BarUtilities.GetProgressBar(Config.Bar, thresholdConfig, null, scale, 1, 0, fillColor: Config.Bar.FillColor);

            AddDrawActions(bar.GetDrawActions(origin + Config.Position, _config.StrataLevel));
        }

        private MPTickerFire3ThresholdConfig? GetFire3ThresholdConfig()
        {
            if (Actor is not PlayerCharacter player || player.ClassJob.Id != JobIDs.BLM)
            {
                return null;
            }

            MPTickerFire3ThresholdConfig config = Config.Bar.Fire3Threshold;
            if (!config.Enabled)
            {
                return null;
            }

            bool leyLinesActive = player.StatusList.Any(e => e.StatusId == 738);
            float castTime = config.Fire3CastTime * (leyLinesActive ? 0.85f : 1f);

            // tick rate is 3s
            // adding 0.3f as "safety net"
            config.Value = (3 - castTime + 0.3f) / 3;

            return config;
        }
    }
}
