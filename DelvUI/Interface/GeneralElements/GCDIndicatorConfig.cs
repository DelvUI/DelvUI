using DelvUI.Config;
using DelvUI.Config.Attributes;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Section("Misc")]
    [SubSection("GCD Indicator", 0)]
    public class GCDIndicatorConfig : AnchorablePluginConfigObject
    {
        [Checkbox("Always Show")]
        [Order(1)]
        public bool AlwaysShow = false;

        [Checkbox("Anchor To Mouse")]
        [Order(2)]
        public bool AnchorToMouse = false;

        [ColorEdit4("Color", spacing = true)]
        [Order(20)]
        public PluginConfigColor Color = new PluginConfigColor(new(220f / 255f, 220f / 255f, 220f / 255f, 100f / 100f));

        [Checkbox("Show Border")]
        [Order(25)]
        public bool ShowBorder = true;

        [Checkbox("Vertical Mode")]
        [Order(35)]
        public bool VerticalMode = false;

        [Checkbox("Circular Mode", separator = true)]
        [Order(50)]
        public bool CircularMode = false;

        [DragInt("Radius")]
        [Order(55, collapseWith = nameof(CircularMode))]
        public int CircleRadius = 40;

        [DragInt("Thickness")]
        [Order(60, collapseWith = nameof(CircularMode))]
        public int CircleThickness = 10;

        [Checkbox("Show GCD Queue Indicator", separator = true)]
        [Order(65)]
        public bool ShowGCDQueueIndicator = true;

        [ColorEdit4("GCD Queue Color")]
        [Order(70, collapseWith = nameof(ShowGCDQueueIndicator))]
        public PluginConfigColor QueueColor = new PluginConfigColor(new(13f / 255f, 207f / 255f, 31f / 255f, 100f / 100f));

        public GCDIndicatorConfig(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
        }

        public new static GCDIndicatorConfig DefaultConfig()
        {
            var size = new Vector2(254, 8);
            var pos = new Vector2(0, HUDConstants.BaseHUDOffsetY + 21);

            var config = new GCDIndicatorConfig(pos, size);
            config.Enabled = false;

            return config;
        }
    }
}
