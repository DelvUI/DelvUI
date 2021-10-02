using Dalamud.Game.ClientState.Objects.Types;
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
            if (!Config.Enabled || Actor == null || Actor is not Character)
            {
                return;
            }

            Bar2 expBar = new Bar2(Config);
            float current = 3175757f;
            float max = 13881000f;
            expBar.SetBarText(string.Format("{0} Lv{1}\tEXP {2}/{3}", "MNK", 71, "3,175,757", "13,881,000"));
            expBar.Draw(origin, current, max);
        }
    }
}
