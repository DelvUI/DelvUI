using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Interface.GeneralElements;
using System.Numerics;

namespace DelvUI.Interface.Bars
{
    [Portable(false)]
    public class ThresholdBarConfig : BarConfig
    {
        [Checkbox("Threshold")]
        [Order(45)]
        public bool Threshold = false;

        [DragFloat("Threshold Value", min = 0f, max = 10000f)]
        [Order(50, collapseWith = nameof(Threshold))]
        public float ThresholdValue = 0f;

        [Combo("Activate Above/Below Threshold", "Above", "Below")]
        [Order(60, collapseWith = nameof(Threshold))]
        public ThresholdType ThresholdType = ThresholdType.Below;

        [ColorEdit4("Threshold Color")]
        [Order(65, collapseWith = nameof(Threshold))]
        public PluginConfigColor ThresholdColor = new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [NestedConfig("Bar Text", 80, separator = false, spacing = true)]
        public LabelConfig LabelConfig;

        public ThresholdBarConfig(
            Vector2 position, 
            Vector2 size,
            PluginConfigColor fillColor, 
            PluginConfigColor? threshHoldColor = null,
            float threshold = 0f) : base(position, size, fillColor)
        {
            LabelConfig = new LabelConfig(Vector2.Zero, "", DrawAnchor.Center, DrawAnchor.Center);
            ThresholdColor = threshHoldColor ?? fillColor;
            ThresholdValue = threshold;
        }

        public override PluginConfigColor GetBarColor(float current, GameObject? actor = null)
        {
            return Threshold && IsThresholdActive(current) ? ThresholdColor : FillColor;
        }

        public bool IsThresholdActive(float current)
        {
            return ThresholdType == ThresholdType.Below && current < ThresholdValue ||
                    ThresholdType == ThresholdType.Above && current > ThresholdValue;
        }
    }

    public enum ThresholdType
    {
        Above,
        Below
    }
}
