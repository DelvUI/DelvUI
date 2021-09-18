using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using System;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Serializable]
    [Section("Misc")]
    [SubSection("GCD Indicator", 0)]
    public class GCDIndicatorConfig : MovablePluginConfigObject
    {
        [DragInt2("Size", min = 1, max = 4000)]
        [Order(15)]
        public Vector2 Size;

        [Combo("Anchor", "Center", "Left", "Right", "Top", "TopLeft", "TopRight", "Bottom", "BottomLeft", "BottomRight")]
        [Order(20)]
        public DrawAnchor Anchor = DrawAnchor.Center;

        [Checkbox("Anchor To Mouse")]
        [Order(25)]
        public bool AnchorToMouse = false;
        
        [Checkbox("Offset Mouse Position")]
        [Order(30)]
        public bool OffsetMousePosition = false;
        
        [Checkbox("Always Show")]
        [Order(35)]
        public bool AlwaysShow = false;

        [Checkbox("Show Border")]
        [Order(40)]
        public bool ShowBorder = true;

        [Checkbox("Vertical Mode")]
        [Order(45)]
        public bool VerticalMode = false;
        
        [Checkbox("Circular Mode")]
        [Order(50)]
        public bool CircularMode = false;
        
        [DragInt("Radius")]
        [Order(55)]
        public int CircleRadius = 40;
        
        [DragInt("Thickness")]
        [Order(60)]
        public int CircleThickness = 10;

        [ColorEdit4("Color")]
        [Order(65)]
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
