using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Interface.Bars;
using ImGuiNET;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Section("Misc")]
    [SubSection("Experience Bar", 0)]
    public class ExperienceBarConfig : BarConfig
    {
        [Checkbox("Hide When Downsynced")]
        [Order(44, collapseWith = nameof(HideWhenInactive))]
        public bool HideWhenDownsynced = false;

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
        public EditableLabelConfig LeftLabel;

        [NestedConfig("Right Text", 65)]
        public EditableLabelConfig RightLabel;

        public ExperienceBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor) : base(position, size, fillColor)
        {
            LeftLabel = new EditableLabelConfig(new Vector2(5, 0), "[job]  Lv[level]  EXP [exp:current-short]/[exp:required-short]", DrawAnchor.BottomLeft, DrawAnchor.TopLeft);
            RightLabel = new EditableLabelConfig(new Vector2(-5, 0), "([exp:percent]%)", DrawAnchor.BottomRight, DrawAnchor.TopRight);
        }

        public new static ExperienceBarConfig DefaultConfig()
        {
            return new ExperienceBarConfig(
                new Vector2(0, -ImGui.GetMainViewport().Size.Y * 0.45f),
                new Vector2(860, 10),
                new PluginConfigColor(new Vector4(211f / 255f, 166f / 255f, 79f / 255f, 100f / 100f)));
        }
    }
}
