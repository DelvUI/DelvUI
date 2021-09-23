using DelvUI.Config;
using DelvUI.Config.Attributes;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Section("Misc")]
    [SubSection("GCD Indicator", 0)]
    public class GCDIndicatorConfig : AnchorablePluginConfigObject
    {
        [Checkbox("Always Show", separator = true)]
        [Order(20)]
        public bool AlwaysShow = false;

        [Checkbox("Show Border")]
        [Order(25)]
        public bool ShowBorder = true;

        [Checkbox("Vertical Mode", spacing = true)]
        [Order(30)]
        public bool VerticalMode = false;
        
        [Checkbox("Circular Mode")]
        [Order(35)]
        public bool CircularMode = false;
        
        [Checkbox("Anchor To Mouse", spacing = true)]
        [Order(40)]
        public bool AnchorToMouse = false;
        
        [Checkbox("Change Position (0,0 = centered with cursor point)")]
        [Order(45)]
        public bool OffsetMousePosition = false;
        
        [DragInt("Radius")]
        [CollapseWith(50,0)]
        public int CircleRadius = 40;
        
        [DragInt("Thickness")]
        [CollapseWith(55,0)]
        public int CircleThickness = 10;

        [ColorEdit4("Color", spacing = true)]
        [Order(60)]
        public PluginConfigColor Color = new PluginConfigColor(new(220f / 255f, 220f / 255f, 220f / 255f, 100f / 100f));

        [Checkbox("Show GCD Queue Indicator")]
        [Order(70)]
        public bool ShowGCDQueueIndicator = true;

        [ColorEdit4("GCD Queue Color")]
        [Order(75)]
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
