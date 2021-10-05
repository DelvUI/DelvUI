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
    public class ExperienceBarConfig : BarConfigBase
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

        public override bool IsActive(float current, float max, float min)
        {
            return (Plugin.ClientState.LocalPlayer?.Level ?? 0) != 80;
        }

        public override Bar2[] GetBars(float current, float max, float min = 0f, GameObject? actor = null)
        {
            Rect background = new Rect(Vector2.Zero, Size, BackgroundColor);

            // Exp progress bar
            PluginConfigColor expFillColor = UseJobColor ? Utils.ColorForActor(actor) : FillColor;
            Rect expBar = Rect.GetFillRect(Vector2.Zero, Size, FillDirection, expFillColor, current, max, min);

            // Rested exp bar
            uint rested = ShowRestedExp ? ExperienceHelper.Instance.RestedExp : 0;
            var restedPos = (FillDirection == BarDirection.Right || FillDirection == BarDirection.Down) ? Rect.GetFillDirectionOffset(expBar.Size, FillDirection) : Vector2.Zero;
            var restedSize = Size - Rect.GetFillDirectionOffset(expBar.Size, FillDirection);
            Rect restedBar = Rect.GetFillRect(restedPos, restedSize, FillDirection, RestedExpColor, rested, max, min);

            return new Bar2[] { new Bar2(background, new[] { expBar, restedBar }, new[] { LeftLabelConfig, RightLabelConfig }) };
        }
    }
}
