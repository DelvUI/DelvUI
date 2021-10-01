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

        [Checkbox("Below Threshold Color", spacing = true)]
        [Order(45)]
        public bool UseThresholdColor = false;

        [ColorEdit4("Threshold Color")]
        [Order(50, collapseWith = nameof(UseThresholdColor))]
        public PluginConfigColor ThresholdColor = new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [Checkbox("Show Threshold Marker")]
        [Order(55)]
        public bool DrawThresholdMarker = false;

        [ColorEdit4("Threshold Marker Color")]
        [Order(50, collapseWith = nameof(DrawThresholdMarker))]
        public PluginConfigColor ThresholdMarkerColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        [Checkbox("Chunk Bar", spacing = true)]
        [Order(60)]
        public bool Chunk = false;

        [DragInt("Chunk Number", min = 2, max = 10)]
        [Order(65, collapseWith = nameof(Chunk))]
        public int ChunkNum = 2;

        [DragInt("Chunk Padding", min = 0, max = 4000)]
        [Order(70, collapseWith = nameof(Chunk))]
        public int ChunkPadding = 2;

        [NestedConfig("Bar Text", 75, separator = false, spacing = true)]
        public LabelConfig BarLabelConfig;

        public BarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor, PluginConfigColor? threshHoldColor = null)
        {
            Position = position;
            Size = size;
            BarLabelConfig = new LabelConfig(Vector2.Zero, "", DrawAnchor.Center, DrawAnchor.Center);
            FillColor = fillColor;
            ThresholdColor = threshHoldColor ?? fillColor;
        }
    }

    public enum BarChunkDirection
    {
        Horizontal,
        Vertical
    }

    public enum BarDirection
    {
        Left,
        Right,
        Up,
        Down
    }
}
