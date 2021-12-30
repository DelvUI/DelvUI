using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
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
        [Order(3)]
        public bool AlwaysShow = false;

        [Checkbox("Anchor To Mouse")]
        [Order(4)]
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

        [Checkbox("Instant GCDs only", spacing = true)]
        [Order(19)]
        public bool InstantGCDsOnly = false;

        [Checkbox("Only show when under GCD Threshold", spacing = true)]
        [Order(20)]
        public bool LimitGCDThreshold = false;

        [DragFloat("GCD Threshold", velocity = 0.01f)]
        [Order(21, collapseWith = nameof(LimitGCDThreshold))]
        public float GCDThreshold = 1.50f;

        [Checkbox("Show GCD Queue Indicator", spacing = true)]
        [Order(24)]
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

        [DragInt("Start Angle", min = 0, max = 359)]
        [Order(45, collapseWith = nameof(CircularMode))]
        public int CircleStartAngle = 0;

        [Checkbox("Rotate CCW")]
        [Order(50, collapseWith = nameof(CircularMode))]
        public bool RotateCCW = false;

        [NestedConfig("Bar Mode", 45, collapsingHeader = false)]
        public GCDBarConfig Bar = new GCDBarConfig(
            new Vector2(0, HUDConstants.BaseHUDOffsetY + 21),
            new Vector2(254, 8),
            new PluginConfigColor(Vector4.Zero)
        );

        public new static GCDIndicatorConfig DefaultConfig() { return new GCDIndicatorConfig() { Enabled = false, Strata = StrataLevel.MID_HIGH }; }
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
