using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Section("Misc")]
    [SubSection("Primary Resource Bar", 0)]
    public class PrimaryResourceConfig : AnchorablePluginConfigObject
    {
        [ColorEdit4("Color")]
        [Order(20)]
        public PluginConfigColor Color = new(new(0 / 255f, 162f / 255f, 252f / 255f, 100f / 100f));

        [Checkbox("Use Job Color")]
        [Order(25)]
        public bool UseJobColor = false;

        [NestedConfig("Label", 30)]
        public LabelConfig ValueLabelConfig;

        [Checkbox("Threshold Marker", spacing = true)]
        [Order(35)]
        public bool ShowThresholdMarker = false;

        [DragInt("Value", min = 1, max = 10000)]
        [Order(40, collapseWith = nameof(ShowThresholdMarker))]
        public int ThresholdMarkerValue = 7000;

        [ColorEdit4("Color" + "##Threshold")]
        [Order(45, collapseWith = nameof(ShowThresholdMarker))]
        public PluginConfigColor BelowThresholdColor = new(new Vector4(190 / 255f, 28f / 255f, 57f / 255f, 100f / 100f));

        [Checkbox("Hide When Full", spacing = true)]
        [Order(50)]
        public bool HidePrimaryResourceWhenFull = false;

        public PrimaryResourceConfig(Vector2 position, Vector2 size, LabelConfig valueLabelConfig)
        {
            Position = position;
            Size = size;
            ValueLabelConfig = valueLabelConfig;
        }

        public static new PrimaryResourceConfig DefaultConfig()
        {
            var size = new Vector2(254, 20);
            var pos = new Vector2(0, HUDConstants.BaseHUDOffsetY - 37);

            var labelConfig = new LabelConfig(Vector2.Zero, "", DrawAnchor.Center, DrawAnchor.Center);

            return new PrimaryResourceConfig(pos, size, labelConfig);
        }
    }
}