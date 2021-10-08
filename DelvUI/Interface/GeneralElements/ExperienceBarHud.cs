using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using System.Collections.Generic;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    public class ExperienceBarHud : DraggableHudElement, IHudElementWithActor
    {
        private ExperienceBarConfig Config => (ExperienceBarConfig)_config;

        public GameObject? Actor { get; set; } = null;

        public ExperienceBarHud(string ID, ExperienceBarConfig config, string displayName) : base(ID, config, displayName) { }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            return (new List<Vector2>() { Config.Position }, new List<Vector2>() { Config.Size });
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (!Config.Enabled || Actor is null || Config.HideWhenInactive && (Plugin.ClientState.LocalPlayer?.Level ?? 0) == 80)
            {
                return;
            }

            uint current = ExperienceHelper.Instance.CurrentExp;
            uint required = ExperienceHelper.Instance.RequiredExp;
            uint rested = Config.ShowRestedExp ? ExperienceHelper.Instance.RestedExp : 0;

            // Exp progress bar
            PluginConfigColor expFillColor = Config.UseJobColor ? Utils.ColorForActor(Actor) : Config.FillColor;
            Rect expBar = BarUtilities.GetFillRect(Config.Position, Config.Size, Config.FillDirection, expFillColor, current, required);

            // Rested exp bar
            var restedPos = Config.FillDirection.IsInverted() ? Config.Position : Config.Position + BarUtilities.GetFillDirectionOffset(expBar.Size, Config.FillDirection);
            var restedSize = Config.Size - BarUtilities.GetFillDirectionOffset(expBar.Size, Config.FillDirection);
            Rect restedBar = BarUtilities.GetFillRect(restedPos, restedSize, Config.FillDirection, Config.RestedExpColor, rested, required, 0f);

            var bar = new BarHud(Config, Actor).AddForegrounds(expBar, restedBar).AddLabels(Config.LeftLabel, Config.RightLabel);
            bar.Draw(origin);
        }
    }


}
