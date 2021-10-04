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

        [Checkbox("Threshold Color")]
        [Order(55, collapseWith = nameof(Threshold))]
        public bool UseThresholdColor = false;

        [Combo("Activate Above/Below Threshold", "Above", "Below")]
        [Order(60, collapseWith = nameof(UseThresholdColor))]
        public ThresholdType ThresholdType = ThresholdType.Below;

        [ColorEdit4("Threshold Color")]
        [Order(65, collapseWith = nameof(UseThresholdColor))]
        public PluginConfigColor ThresholdColor = new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [Checkbox("Show Threshold Marker")]
        [Order(70, collapseWith = nameof(Threshold))]
        public bool DrawThresholdMarker = false;

        [ColorEdit4("Threshold Marker Color")]
        [Order(75, collapseWith = nameof(DrawThresholdMarker))]
        public PluginConfigColor ThresholdMarkerColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

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
            return IsThresholdActive(current) && UseThresholdColor ? ThresholdColor : FillColor;
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
