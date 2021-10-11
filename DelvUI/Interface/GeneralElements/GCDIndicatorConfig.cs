using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Interface.Bars;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [DisableParentSettings("Size")]
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

        [ColorEdit4("Background Color")]
        [Order(16)]
        public PluginConfigColor BackgroundColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 50f / 100f));

        [ColorEdit4("Color")]
        [Order(17)]
        public PluginConfigColor FillColor = new PluginConfigColor(new(220f / 255f, 220f / 255f, 220f / 255f, 100f / 100f));

        [Checkbox("Show Border")]
        [Order(18)]
        public bool ShowBorder = true;

        [Checkbox("Show GCD Queue Indicator", spacing = true)]
        [Order(20)]
        public bool ShowGCDQueueIndicator = true;

        [ColorEdit4("GCD Queue Color")]
        [Order(25, collapseWith = nameof(ShowGCDQueueIndicator))]
        public PluginConfigColor QueueColor = new PluginConfigColor(new(13f / 255f, 207f / 255f, 31f / 255f, 100f / 100f));

        [Checkbox("Circular Mode", spacing = true)]
        [Order(30)]
        public bool CircularMode = false;

        [DragInt("Radius")]
        [Order(35, collapseWith = nameof(CircularMode))]
        public int CircleRadius = 40;

        [DragInt("Thickness")]
        [Order(40, collapseWith = nameof(CircularMode))]
        public int CircleThickness = 10;

        [NestedConfig("Bar Mode", 45, separator = false, spacing = true, nest = true)]
        public GCDBarConfig Bar = new GCDBarConfig(
            new Vector2(0, HUDConstants.BaseHUDOffsetY + 21),
            new Vector2(254, 8),
            new PluginConfigColor(Vector4.Zero)
        );

        public new static GCDIndicatorConfig DefaultConfig() { return new GCDIndicatorConfig(); }
    }

    [DisableParentSettings("Position", "Anchor", "HideWhenInactive", "FillColor", "BackgroundColor", "DrawBorder")]
    [Exportable(false)]
    public class GCDBarConfig : BarConfig
    {
        public GCDBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor, BarDirection fillDirection = BarDirection.Right)
            : base(position, size, fillColor, fillDirection)
        {
        }
    }
}
