using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using System.Collections.Generic;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    public class ExperienceBarHud : DraggableHudElement, IHudElementWithActor, IHudElementWithVisibilityConfig
    {
        private ExperienceBarConfig Config => (ExperienceBarConfig)_config;
        public VisibilityConfig VisibilityConfig => Config.VisibilityConfig;

        public GameObject? Actor { get; set; } = null;

        private ExperienceHelper _helper = new ExperienceHelper();
        private IconLabelHud _sanctuaryLabel;

        public ExperienceBarHud(ExperienceBarConfig config, string displayName) : base(config, displayName)
        {
            Config.SanctuaryLabel.IconId = FontAwesomeIcon.Moon;
            _sanctuaryLabel = new IconLabelHud(Config.SanctuaryLabel);
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            return (new List<Vector2>() { Config.Position }, new List<Vector2>() { Config.Size });
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (!Config.Enabled || Actor is null || Config.HideWhenInactive && (Plugin.ClientState.LocalPlayer?.Level ?? 0) >= 90 || (Config.HideWhenInactive && Config.HideWhenDownsynced && _helper.IsMaxLevel()))
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

            BarHud bar = new BarHud(Config, Actor);
            bar.AddForegrounds(expBar, restedBar);
            bar.AddLabels(Config.LeftLabel, Config.RightLabel);

            AddDrawActions(bar.GetDrawActions(origin, Config.StrataLevel));

            // sanctuary icon
            AddDrawAction(Config.SanctuaryLabel.StrataLevel, () =>
            {
                var pos = Utils.GetAnchoredPosition(origin, Config.Size, Config.Anchor);
                _sanctuaryLabel.Draw(pos + Config.Position, Config.Size, Actor);
            });
        }
    }
}
