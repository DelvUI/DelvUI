using DelvUI.Config;
using DelvUI.Config.Attributes;
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

        [Checkbox("Always Show")]
        [Order(20)]
        public bool AlwaysShow = false;

        [Checkbox("Show Border")]
        [Order(25)]
        public bool ShowBorder = true;

        [Checkbox("Vertical Mode")]
        [Order(30)]
        public bool VerticalMode = false;

        [ColorEdit4("Color")]
        [Order(35)]
        public PluginConfigColor Color = new PluginConfigColor(new(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [Checkbox("Show GCD Queue Indicator")]
        [Order(40)]
        public bool ShowGCDQueueIndicator = true;

        [ColorEdit4("GCD Queue Color")]
        [Order(45)]
        public PluginConfigColor QueueColor = new PluginConfigColor(new(15f / 255f, 235f / 255f, 38f / 255f, 100f / 100f));

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
