using DelvUI.Config;
using DelvUI.Config.Attributes;
using System;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Serializable]
    [Section("Misc")]
    [SubSection("Primary Resource Bar", 0)]
    public class PrimaryResourceConfig : MovablePluginConfigObject
    {
        [DragInt2("Size", min = 1, max = 4000)]
        [Order(15)]
        public Vector2 Size;

        [Checkbox("Show Value")]
        [Order(20)]
        public bool ShowValue = true;

        [NestedConfig("Label", 25)]
        public LabelConfig ValueLabelConfig;

        [Checkbox("Show Threshold Marker")]
        [CollapseControl(30, 0)]
        public bool ShowThresholdMarker = false;

        [DragInt("Threshold Marker Value", min = 1, max = 10000)]
        [CollapseWith(0, 0)]
        public int ThresholdMarkerValue = 7000;

        [ColorEdit4("Color")]
        [Order(35)]
        public PluginConfigColor Color = new PluginConfigColor(new(0 / 255f, 205f / 255f, 230f / 255f, 100f / 100f));

        public PrimaryResourceConfig(Vector2 position, Vector2 size, LabelConfig valueLabelConfig)
        {
            Position = position;
            Size = size;
            ValueLabelConfig = valueLabelConfig;
        }

        public new static PrimaryResourceConfig DefaultConfig()
        {
            var size = new Vector2(254, 20);
            var pos = new Vector2(0, HUDConstants.BaseHUDOffsetY - 37);

            var labelConfig = new LabelConfig(Vector2.Zero, "", LabelTextAnchor.Center);

            return new PrimaryResourceConfig(pos, size, labelConfig);
        }
    }
}
