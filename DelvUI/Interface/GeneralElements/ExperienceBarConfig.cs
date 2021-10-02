using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Interface.Bars;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Section("Misc")]
    [SubSection("Experience Bar", 0)]
    public class ExperienceBarConfig : BarConfig
    { 
        public ExperienceBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor) : base(position, size, fillColor)
        {
        }

        public ExperienceBarConfig() : this(new Vector2(0, 0), new Vector2(500, 10), new PluginConfigColor(new Vector4(211f / 255f, 166f / 255f, 79f / 255f, 100f / 100f)))
        {
            BarLabelConfig = new LabelConfig(new Vector2(5, 0), "", DrawAnchor.BottomLeft, DrawAnchor.TopLeft);
        }

        public new static ExperienceBarConfig DefaultConfig() 
        {
            return new ExperienceBarConfig(); 
        }
    }
}
