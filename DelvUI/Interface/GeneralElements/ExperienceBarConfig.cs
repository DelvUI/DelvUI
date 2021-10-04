using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Section("Misc")]
    [SubSection("Experience Bar", 0)]
    public class ExperienceBarConfig : BarConfig
    {
        [Checkbox("Use Job Color")]
        [Order(45)]
        public bool UseJobColor = false;

        [Checkbox("Show Rested Exp")]
        [Order(50)]
        public bool ShowRestedExp = true;

        [ColorEdit4("Rested Exp Color")]
        [Order(55, collapseWith = nameof(ShowRestedExp))]
        public PluginConfigColor RestedExpColor = new PluginConfigColor(new Vector4(110f / 255f, 197f / 255f, 207f / 255f, 50f / 100f));

        [NestedConfig("Left Text", 60)]
        public EditableLabelConfig LeftLabelConfig;

        [NestedConfig("Right Text", 65)]
        public EditableLabelConfig RightLabelConfig;

        public ExperienceBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor) : base(position, size, fillColor)
        {
            LeftLabelConfig = new EditableLabelConfig(new Vector2(5, 0), "[job]  Lv[level]  EXP [exp:current-short]/[exp:required-short]", DrawAnchor.BottomLeft, DrawAnchor.TopLeft);
            RightLabelConfig = new EditableLabelConfig(new Vector2(-5, 0), "([exp:percent]%)", DrawAnchor.BottomRight, DrawAnchor.TopRight);
        }

        public ExperienceBarConfig() : this(new Vector2(0, 750), new Vector2(860, 10), new PluginConfigColor(new Vector4(211f / 255f, 166f / 255f, 79f / 255f, 100f / 100f)))
        {
        }

        public new static ExperienceBarConfig DefaultConfig() 
        {
            return new ExperienceBarConfig(); 
        }

        public override bool IsActive(float current)
        {
            return (Plugin.ClientState.LocalPlayer?.Level ?? 0) != 80;
        }

        public override PluginConfigColor GetBarColor(float current, GameObject? actor = null)
        {
            if (current == ExperienceHelper.Instance.RestedExp)
            {
                return RestedExpColor;
            }

            return UseJobColor ? Utils.ColorForActor(actor) : base.GetBarColor(current);
        }
    }
}
