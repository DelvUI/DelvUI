using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Interface.GeneralElements;
using System.Numerics;

namespace DelvUI.Interface.Bars
{
    [Portable(false)]
    public class BarConfig : AnchorablePluginConfigObject
    {
        [ColorEdit4("Background Color")]
        [Order(20)]
        public PluginConfigColor BackgroundColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 50f / 100f));

        [ColorEdit4("Fill Color")]
        [Order(25)]
        public PluginConfigColor FillColor = new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [Combo("Fill Direction", new string[] { "Left", "Right", "Up", "Down" })]
        [Order(30)]
        public BarDirection FillDirection = BarDirection.Right;

        [Checkbox("Draw Border")]
        [Order(35)]
        public bool DrawBorder = true;

        [Checkbox("Hide when inactive")]
        [Order(40)]
        public bool HideWhenInactive = false;

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

        [Checkbox("Chunk Bar")]
        [Order(80)]
        public bool Chunk = false;

        [DragInt("Chunk Number", min = 2, max = 10)]
        [Order(85, collapseWith = nameof(Chunk))]
        public int ChunkNum = 2;

        [DragInt("Chunk Padding", min = 0, max = 4000)]
        [Order(90, collapseWith = nameof(Chunk))]
        public int ChunkPadding = 2;

        [NestedConfig("Bar Text", 95, separator = false, spacing = true)]
        public LabelConfig BarLabelConfig;

        public BarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor, PluginConfigColor? threshHoldColor = null, float threshold = 0f)
        {
            Position = position;
            Size = size;
            BarLabelConfig = new LabelConfig(Vector2.Zero, "", DrawAnchor.Center, DrawAnchor.Center);
            FillColor = fillColor;
            ThresholdColor = threshHoldColor ?? fillColor;
            ThresholdValue = threshold;
        }

        public PluginConfigColor GetBarColor(float current)
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

    public enum BarDirection
    {
        Left,
        Right,
        Up,
        Down
    }
}
