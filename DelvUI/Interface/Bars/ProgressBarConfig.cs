using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Interface.GeneralElements;
using System.Numerics;

namespace DelvUI.Interface.Bars
{
    public class ProgressBarConfig : BarConfig
    {
        [Checkbox("Threshold")]
        [Order(45)]
        public bool Threshold = false;

        [DragFloat("Threshold Value", min = 0f, max = 10000f)]
        [Order(50, collapseWith = nameof(Threshold))]
        public float ThresholdValue = 0f;

        [Combo("Activate Above/Below Threshold", "Above", "Below")]
        [Order(55, collapseWith = nameof(Threshold))]
        public ThresholdType ThresholdType = ThresholdType.Below;

        [ColorEdit4("Threshold Color")]
        [Order(60, collapseWith = nameof(Threshold))]
        public PluginConfigColor ThresholdColor;

        [Checkbox("Show Threshold Marker")]
        [Order(65, collapseWith = nameof(Threshold))]
        public bool ThresholdMarker = false;

        [DragInt("Threshold Marker Size", min = 0, max = 10000)]
        [Order(70, collapseWith = nameof(ThresholdMarker))]
        public int ThresholdMarkerSize = 2;

        [ColorEdit4("Threshold Marker Color")]
        [Order(75, collapseWith = nameof(ThresholdMarker))]
        public PluginConfigColor ThresholdMarkerColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        [NestedConfig("Bar Text", 1000, separator = false, spacing = true)]
        public LabelConfig Label;

        public ProgressBarConfig(
            Vector2 position, 
            Vector2 size,
            PluginConfigColor fillColor, 
            PluginConfigColor? threshHoldColor = null,
            float threshold = 0f) : base(position, size, fillColor)
        {
            Label = new LabelConfig(Vector2.Zero, "", DrawAnchor.Center, DrawAnchor.Center);
            ThresholdColor = threshHoldColor ?? new PluginConfigColor(new(230f / 255f, 33f / 255f, 33f / 255f, 53f / 100f));
            ThresholdValue = threshold;
        }

        public bool IsThresholdActive(float current)
        {
            return Threshold && (ThresholdType == ThresholdType.Below && current < ThresholdValue ||
                                 ThresholdType == ThresholdType.Above && current > ThresholdValue);
        }
    }

    public enum ThresholdType
    {
        Above,
        Below
    }
}
