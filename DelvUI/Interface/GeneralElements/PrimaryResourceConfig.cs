using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Section("Mana Bars")]
    [SubSection("Player", 0)]
    public class PlayerPrimaryResourceConfig : UnitFramePrimaryResourceConfig
    {
        public PlayerPrimaryResourceConfig(Vector2 position, Vector2 size, LabelConfig valueLabelConfig)
            : base(position, size, valueLabelConfig)
        {

        }

        public new static PlayerPrimaryResourceConfig DefaultConfig()
        {
            var size = new Vector2(HUDConstants.DefaultBigUnitFrameSize.X, 10);
            var pos = new Vector2(0, 0);

            var labelConfig = new LabelConfig(Vector2.Zero, "", DrawAnchor.Center, DrawAnchor.Center);

            return new PlayerPrimaryResourceConfig(pos, size, labelConfig);
        }
    }

    [Section("Mana Bars")]
    [SubSection("Target", 0)]
    public class TargetPrimaryResourceConfig : UnitFramePrimaryResourceConfig
    {
        public TargetPrimaryResourceConfig(Vector2 position, Vector2 size, LabelConfig valueLabelConfig)
            : base(position, size, valueLabelConfig)
        {

        }

        public new static TargetPrimaryResourceConfig DefaultConfig()
        {
            var size = new Vector2(HUDConstants.DefaultBigUnitFrameSize.X, 10);
            var pos = new Vector2(0, 0);

            var labelConfig = new LabelConfig(Vector2.Zero, "", DrawAnchor.Center, DrawAnchor.Center);

            return new TargetPrimaryResourceConfig(pos, size, labelConfig);
        }
    }

    [Section("Mana Bars")]
    [SubSection("Target of Target", 0)]
    public class TargetOfTargetPrimaryResourceConfig : UnitFramePrimaryResourceConfig
    {
        public TargetOfTargetPrimaryResourceConfig(Vector2 position, Vector2 size, LabelConfig valueLabelConfig)
            : base(position, size, valueLabelConfig)
        {

        }

        public new static TargetOfTargetPrimaryResourceConfig DefaultConfig()
        {
            var size = new Vector2(HUDConstants.DefaultSmallUnitFrameSize.X, 10);
            var pos = new Vector2(0, 0);

            var labelConfig = new LabelConfig(Vector2.Zero, "", DrawAnchor.Center, DrawAnchor.Center);

            return new TargetOfTargetPrimaryResourceConfig(pos, size, labelConfig);
        }
    }

    [Section("Mana Bars")]
    [SubSection("Focus Target", 0)]
    public class FocusTargetPrimaryResourceConfig : UnitFramePrimaryResourceConfig
    {
        public FocusTargetPrimaryResourceConfig(Vector2 position, Vector2 size, LabelConfig valueLabelConfig)
            : base(position, size, valueLabelConfig)
        {

        }

        public new static FocusTargetPrimaryResourceConfig DefaultConfig()
        {
            var size = new Vector2(HUDConstants.DefaultSmallUnitFrameSize.X, 10);
            var pos = new Vector2(0, 0);

            var labelConfig = new LabelConfig(Vector2.Zero, "", DrawAnchor.Center, DrawAnchor.Center);

            return new FocusTargetPrimaryResourceConfig(pos, size, labelConfig);
        }
    }

    public abstract class UnitFramePrimaryResourceConfig : PrimaryResourceConfig
    {
        [Checkbox("Anchor to Unit Frame")]
        [Order(16)]
        public bool AnchorToUnitFrame = true;

        [Anchor("Unit Frame Anchor")]
        [Order(17, collapseWith = nameof(AnchorToUnitFrame))]
        public DrawAnchor UnitFrameAnchor = DrawAnchor.Bottom;

        public UnitFramePrimaryResourceConfig(Vector2 position, Vector2 size, LabelConfig valueLabelConfig)
            : base(position, size, valueLabelConfig)
        {

        }
    }

    public abstract class PrimaryResourceConfig : AnchorablePluginConfigObject
    {
        [ColorEdit4("Color")]
        [Order(20)]
        public PluginConfigColor Color = new PluginConfigColor(new(0 / 255f, 162f / 255f, 252f / 255f, 100f / 100f));

        [Checkbox("Use Job Color")]
        [Order(25)]
        public bool UseJobColor = false;

        [NestedConfig("Label", 30)]
        public LabelConfig ValueLabelConfig;

        [Checkbox("Threshold Marker", spacing = true)]
        [Order(35)]
        public bool ShowThresholdMarker = false;

        [DragInt("Value", min = 1, max = 10000)]
        [Order(40, collapseWith = nameof(ShowThresholdMarker))]
        public int ThresholdMarkerValue = 7000;

        [ColorEdit4("Color" + "##Threshold")]
        [Order(45, collapseWith = nameof(ShowThresholdMarker))]
        public PluginConfigColor BelowThresholdColor = new(new Vector4(190 / 255f, 28f / 255f, 57f / 255f, 100f / 100f));

        [Checkbox("Hide When Full", spacing = true)]
        [Order(50)]
        public bool HidePrimaryResourceWhenFull = false;

        public PrimaryResourceConfig(Vector2 position, Vector2 size, LabelConfig valueLabelConfig)
        {
            Position = position;
            Size = size;
            ValueLabelConfig = valueLabelConfig;
        }
    }
}