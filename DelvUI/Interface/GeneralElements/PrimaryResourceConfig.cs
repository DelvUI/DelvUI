using DelvUI.Config;
using DelvUI.Config.Attributes;
using System;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Section("Misc")]
    [SubSection("Primary Resource Bar", 0)]
    public class PrimaryResourceConfig : MovablePluginConfigObject
    {
        [DragInt2("Size", min = 1, max = 4000)]
        [Order(15)]
        public Vector2 Size;

        [ColorEdit4("Color")]
        [Order(20)]
        public PluginConfigColor Color = new PluginConfigColor(new(0 / 255f, 162f / 255f, 252f / 255f, 100f / 100f));

        [Checkbox("Use Job Color")]
        [Order(25)]
        public bool UseJobColor = false;

        [NestedConfig("Label", 30)]
        public LabelConfig ValueLabelConfig;

        [Checkbox("Threshold Marker", spacing = true)]
        [CollapseControl(35, 0)]
        public bool ShowThresholdMarker = false;

        [DragInt("Value", min = 1, max = 10000)]
        [CollapseWith(0, 0)]
        public int ThresholdMarkerValue = 7000;
        //TODO ADD BELOW THRESHOLD COLOR


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

            var labelConfig = new LabelConfig(Vector2.Zero, "", HudElementAnchor.Center, HudElementAnchor.Center);

            return new PrimaryResourceConfig(pos, size, labelConfig);
        }
    }
}