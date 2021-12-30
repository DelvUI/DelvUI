using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Interface.Bars;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [DisableParentSettings("HideWhenInactive", "Label")]
    [Section("Mana Bars")]
    [SubSection("Player", 0)]
    public class PlayerPrimaryResourceConfig : UnitFramePrimaryResourceConfig
    {
        public PlayerPrimaryResourceConfig(Vector2 position, Vector2 size)
            : base(position, size)
        {

        }

        public new static PlayerPrimaryResourceConfig DefaultConfig()
        {
            var size = new Vector2(HUDConstants.DefaultBigUnitFrameSize.X, 10);
            var pos = new Vector2(0, 0);

            var config = new PlayerPrimaryResourceConfig(pos, size);
            config.Anchor = DrawAnchor.Bottom;

            return config;
        }
    }

    [DisableParentSettings("HideWhenInactive", "Label")]
    [Section("Mana Bars")]
    [SubSection("Target", 0)]
    public class TargetPrimaryResourceConfig : UnitFramePrimaryResourceConfig
    {
        public TargetPrimaryResourceConfig(Vector2 position, Vector2 size)
            : base(position, size)
        {

        }

        public new static TargetPrimaryResourceConfig DefaultConfig()
        {
            var size = new Vector2(HUDConstants.DefaultBigUnitFrameSize.X, 10);
            var pos = new Vector2(0, 0);

            var config = new TargetPrimaryResourceConfig(pos, size);
            config.Anchor = DrawAnchor.Bottom;

            return config;
        }
    }

    [DisableParentSettings("HideWhenInactive", "Label")]
    [Section("Mana Bars")]
    [SubSection("Target of Target", 0)]
    public class TargetOfTargetPrimaryResourceConfig : UnitFramePrimaryResourceConfig
    {
        public TargetOfTargetPrimaryResourceConfig(Vector2 position, Vector2 size)
            : base(position, size)
        {

        }

        public new static TargetOfTargetPrimaryResourceConfig DefaultConfig()
        {
            var size = new Vector2(HUDConstants.DefaultSmallUnitFrameSize.X, 10);
            var pos = new Vector2(0, 0);

            var config = new TargetOfTargetPrimaryResourceConfig(pos, size);
            config.Anchor = DrawAnchor.Bottom;

            return config;
        }
    }

    [DisableParentSettings("HideWhenInactive", "Label")]
    [Section("Mana Bars")]
    [SubSection("Focus Target", 0)]
    public class FocusTargetPrimaryResourceConfig : UnitFramePrimaryResourceConfig
    {
        public FocusTargetPrimaryResourceConfig(Vector2 position, Vector2 size)
            : base(position, size)
        {

        }

        public new static FocusTargetPrimaryResourceConfig DefaultConfig()
        {
            var size = new Vector2(HUDConstants.DefaultSmallUnitFrameSize.X, 10);
            var pos = new Vector2(0, 0);

            var config = new FocusTargetPrimaryResourceConfig(pos, size);
            config.Anchor = DrawAnchor.Bottom;

            return config;
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

        public UnitFramePrimaryResourceConfig(Vector2 position, Vector2 size)
            : base(position, size)
        {

        }
    }

    public abstract class PrimaryResourceConfig : ProgressBarConfig
    {
        [Checkbox("Use Job Color", spacing = true)]
        [Order(19)]
        public bool UseJobColor = false;

        [Checkbox("Hide When Full", spacing = true)]
        [Order(41)]
        public bool HidePrimaryResourceWhenFull = false;

        [NestedConfig("Label", 1000, separator = false, spacing = true)]
        public EditableLabelConfig ValueLabel = new EditableLabelConfig(Vector2.Zero, "[mana:current]", DrawAnchor.Center, DrawAnchor.Center);

        public PrimaryResourceConfig(Vector2 position, Vector2 size)
            : base(position, size, new(new(0 / 255f, 162f / 255f, 252f / 255f, 100f / 100f)))
        {
            Strata = StrataLevel.LOW;
        }
    }
}