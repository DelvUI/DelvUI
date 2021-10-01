using DelvUI.Config;
using DelvUI.Config.Attributes;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Section("Misc")]
    [SubSection("GCD Indicator", 0)]
    public class GCDIndicatorConfig : AnchorablePluginConfigObject
    {
        [ColorEdit4("Color")]
        [Order(20)]
        public PluginConfigColor Color = new(new(220f / 255f, 220f / 255f, 220f / 255f, 100f / 100f));

        [Checkbox("Show Border")]
        [Order(25)]
        public bool ShowBorder = true;

        [Checkbox("Always Show")]
        [Order(30)]
        public bool AlwaysShow = false;

        [Checkbox("Vertical Mode", spacing = true)]
        [Order(35)]
        public bool VerticalMode = false;

        [Checkbox("Anchor To Mouse", spacing = true)]
        [Order(40)]
        public bool AnchorToMouse = false;

        [DragInt2("Offset from Mouse", min = -4000, max = 4000)]
        [Order(45, collapseWith = nameof(AnchorToMouse))]
        public Vector2 MouseOffset = Vector2.Zero;

        [Checkbox("Circular Mode")]
        [Order(50)]
        public bool CircularMode = false;

        [DragInt("Radius")]
        [Order(55, collapseWith = nameof(CircularMode))]
        public int CircleRadius = 40;

        [DragInt("Thickness")]
        [Order(60, collapseWith = nameof(CircularMode))]
        public int CircleThickness = 10;

        [Checkbox("Show GCD Queue Indicator")]
        [Order(65)]
        public bool ShowGCDQueueIndicator = true;

        [ColorEdit4("GCD Queue Color")]
        [Order(70, collapseWith = nameof(ShowGCDQueueIndicator))]
        public PluginConfigColor QueueColor = new(new(13f / 255f, 207f / 255f, 31f / 255f, 100f / 100f));

        public GCDIndicatorConfig(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
        }

        public static new GCDIndicatorConfig DefaultConfig()
        {
            var size = new Vector2(254, 8);
            var pos = new Vector2(0, HUDConstants.BaseHUDOffsetY + 21);

            var config = new GCDIndicatorConfig(pos, size);
            config.Enabled = false;

            return config;
        }
    }
}
