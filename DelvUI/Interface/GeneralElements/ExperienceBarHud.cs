using Dalamud.Game.ClientState.Objects.Types;
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

        private BarHud? ExpBar { get; set; }

        public ExperienceBarHud(string ID, ExperienceBarConfig config, string displayName) : base(ID, config, displayName) { }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            return (new List<Vector2>() { Config.Position }, new List<Vector2>() { Config.Size });
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (!Config.Enabled || Actor is null)
            {
                return;
            }

            ExpBar ??= new BarHud(Config, Actor);
            ExpBar.Draw(origin, ExperienceHelper.Instance.CurrentExp, ExperienceHelper.Instance.RequiredExp);
        }
    }


}
