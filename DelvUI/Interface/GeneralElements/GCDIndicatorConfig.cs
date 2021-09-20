using DelvUI.Config;
using DelvUI.Config.Attributes;
using System;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Section("Misc")]
    [SubSection("GCD Indicator", 0)]
    public class GCDIndicatorConfig : MovablePluginConfigObject
    {
        [DragInt2("Size", min = 1, max = 4000)]
        [Order(15)]
        public Vector2 Size;

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
        [CollapseControl(35, 0)]
        public bool CircularMode = false;
        
        [Checkbox("Anchor To Mouse")]
        [CollapseWith(40,0)]
        public bool AnchorToMouse = false;
        
        [Checkbox("Change Position (0,0 = centered with cursor point)")]
        [CollapseWith(45,0)]
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
        [Order(65)]
        public bool ShowGCDQueueIndicator = true;

        [ColorEdit4("GCD Queue Color")]
        [Order(70)]
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
