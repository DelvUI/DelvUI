using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Section("Unit Frames")]
    [SubSection("Player", 0)]
    public class PlayerUnitFrameConfig : UnitFrameConfig
    {
        public PlayerUnitFrameConfig(Vector2 position, Vector2 size, EditableLabelConfig leftLabelConfig, EditableLabelConfig rightLabelConfig)
            : base(position, size, leftLabelConfig, rightLabelConfig)
        {
        }

        public static new PlayerUnitFrameConfig DefaultConfig()
        {
            Vector2 size = HUDConstants.DefaultBigUnitFrameSize;
            var pos = new Vector2(-HUDConstants.UnitFramesOffsetX - size.X / 2f, HUDConstants.BaseHUDOffsetY);

            var leftLabelConfig = new EditableLabelConfig(new Vector2(5, 0), "[name:abbreviate]", DrawAnchor.TopLeft, DrawAnchor.BottomLeft);
            var rightLabelConfig = new EditableLabelConfig(new Vector2(-5, 0), "[health:current-short] | [health:percent]", DrawAnchor.TopRight, DrawAnchor.BottomRight);

            var config = new PlayerUnitFrameConfig(pos, size, leftLabelConfig, rightLabelConfig);
            config.TankStanceIndicatorConfig = new TankStanceIndicatorConfig();

            return config;
        }
    }

    [Section("Unit Frames")]
    [SubSection("Target", 0)]
    public class TargetUnitFrameConfig : UnitFrameConfig
    {
        public TargetUnitFrameConfig(Vector2 position, Vector2 size, EditableLabelConfig leftLabelConfig, EditableLabelConfig rightLabelConfig)
            : base(position, size, leftLabelConfig, rightLabelConfig)
        {
        }

        public static new TargetUnitFrameConfig DefaultConfig()
        {
            Vector2 size = HUDConstants.DefaultBigUnitFrameSize;
            var pos = new Vector2(HUDConstants.UnitFramesOffsetX + size.X / 2f, HUDConstants.BaseHUDOffsetY);

            var leftLabelConfig = new EditableLabelConfig(new Vector2(5, 0), "[health:current-short] | [health:percent]", DrawAnchor.TopLeft, DrawAnchor.BottomLeft);
            var rightLabelConfig = new EditableLabelConfig(new Vector2(-5, 0), "[name:abbreviate]", DrawAnchor.TopRight, DrawAnchor.BottomRight);

            return new TargetUnitFrameConfig(pos, size, leftLabelConfig, rightLabelConfig);
        }
    }

    [Section("Unit Frames")]
    [SubSection("Target of Target", 0)]
    public class TargetOfTargetUnitFrameConfig : UnitFrameConfig
    {
        public TargetOfTargetUnitFrameConfig(Vector2 position, Vector2 size, EditableLabelConfig leftLabelConfig, EditableLabelConfig rightLabelConfig)
            : base(position, size, leftLabelConfig, rightLabelConfig)
        {
        }

        public static new TargetOfTargetUnitFrameConfig DefaultConfig()
        {
            Vector2 size = HUDConstants.DefaultSmallUnitFrameSize;
            var pos = new Vector2(
                HUDConstants.UnitFramesOffsetX + HUDConstants.DefaultBigUnitFrameSize.X + 6 + size.X / 2f,
                HUDConstants.BaseHUDOffsetY - 15
            );

            var leftLabelConfig = new EditableLabelConfig(new Vector2(0, 0), "[name:abbreviate]", DrawAnchor.Top, DrawAnchor.Bottom);
            var rightLabelConfig = new EditableLabelConfig(new Vector2(0, 0), "", DrawAnchor.Center, DrawAnchor.TopLeft);

            return new TargetOfTargetUnitFrameConfig(pos, size, leftLabelConfig, rightLabelConfig);
        }
    }

    [Section("Unit Frames")]
    [SubSection("Focus Target", 0)]
    public class FocusTargetUnitFrameConfig : UnitFrameConfig
    {
        public FocusTargetUnitFrameConfig(Vector2 position, Vector2 size, EditableLabelConfig leftLabelConfig, EditableLabelConfig rightLabelConfig)
            : base(position, size, leftLabelConfig, rightLabelConfig)
        {
        }

        public static new FocusTargetUnitFrameConfig DefaultConfig()
        {
            Vector2 size = HUDConstants.DefaultSmallUnitFrameSize;
            var pos = new Vector2(
                -HUDConstants.UnitFramesOffsetX - HUDConstants.DefaultBigUnitFrameSize.X - 6 - size.X / 2f,
                HUDConstants.BaseHUDOffsetY - 15
            );

            var leftLabelConfig = new EditableLabelConfig(new Vector2(0, 0), "[name:abbreviate]", DrawAnchor.Top, DrawAnchor.Bottom);
            var rightLabelConfig = new EditableLabelConfig(new Vector2(0, 0), "", DrawAnchor.Center, DrawAnchor.Center);

            return new FocusTargetUnitFrameConfig(pos, size, leftLabelConfig, rightLabelConfig);
        }
    }

    public class UnitFrameConfig : AnchorablePluginConfigObject
    {
        [Checkbox("Custom Frame Color")]
        [Order(20)]
        public bool UseCustomColor = false;

        [ColorEdit4("Color ##CustomFrame")]
        [Order(25, collapseWith = nameof(UseCustomColor))]
        public PluginConfigColor CustomColor = new(new Vector4(0f / 255f, 145f / 255f, 6f / 255f, 100f / 100f));

        [Checkbox("Color Based On Health Value ##CustomFrame")]
        [Order(30, collapseWith = nameof(UseCustomColor))]
        public bool UseColorBasedOnHealthValue = false;

        [ColorEdit4("Full Health Color ##CustomFrame")]
        [Order(35, collapseWith = nameof(UseColorBasedOnHealthValue))]
        public PluginConfigColor FullHealthColor = new(new Vector4(0f / 255f, 255f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Low Health Color ##CustomFrame")]
        [Order(40, collapseWith = nameof(UseColorBasedOnHealthValue))]
        public PluginConfigColor LowHealthColor = new(new Vector4(255f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        [DragFloat("Full Health Color Above Health %", min = 50f, max = 100f, velocity = 1f)]
        [Order(45, collapseWith = nameof(UseColorBasedOnHealthValue))]
        public float FullHealthColorThreshold = 75f;

        [DragFloat("Low Health Color Below Health %", min = 0f, max = 50f, velocity = 1f)]
        [Order(50, collapseWith = nameof(UseColorBasedOnHealthValue))]
        public float LowHealthColorThreshold = 25f;

        [Checkbox("Custom Background Color")]
        [Order(55)]
        public bool UseCustomBackgroundColor = false;

        [ColorEdit4("Color ##CustomBackground")]
        [Order(60, collapseWith = nameof(UseCustomBackgroundColor))]
        public PluginConfigColor CustomBackgroundColor = new(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        [Checkbox("Job Color As Background Color")]
        [Order(65, collapseWith = nameof(UseCustomBackgroundColor))]
        public bool UseJobColorAsBackgroundColor = false;

        [Checkbox("Tank Invulnerability")]
        [Order(70)]
        public bool ShowTankInvulnerability = true;

        [Checkbox("Tank Invulnerability Custom Color")]
        [Order(75, collapseWith = nameof(ShowTankInvulnerability))]
        public bool UseCustomInvulnerabilityColor = true;

        [ColorEdit4("Tank Invulnerability Color ##TankInvulnerabilityCustom")]
        [Order(80, collapseWith = nameof(UseCustomInvulnerabilityColor))]
        public PluginConfigColor CustomInvulnerabilityColor = new(new Vector4(100f / 255f, 100f / 255f, 100f / 255f, 100f / 100f));

        [NestedConfig("Left Text", 85)]
        public EditableLabelConfig LeftLabelConfig;

        [NestedConfig("Right Text", 90)]
        public EditableLabelConfig RightLabelConfig;

        [NestedConfig("Shields", 95)]
        public ShieldConfig ShieldConfig = new();

        [NestedConfig("Tank Stance", 100)]
        public TankStanceIndicatorConfig? TankStanceIndicatorConfig = null;

        public UnitFrameConfig(Vector2 position, Vector2 size, EditableLabelConfig leftLabelConfig, EditableLabelConfig rightLabelConfig)
        {
            Position = position;
            Size = size;
            LeftLabelConfig = leftLabelConfig;
            RightLabelConfig = rightLabelConfig;
        }
    }

    [Portable(false)]
    public class ShieldConfig : PluginConfigObject
    {
        [DragInt("Height")]
        [Order(5)]
        public int Height = 26;

        [Checkbox("Height in Pixels")]
        [Order(10)]
        public bool HeightInPixels = false;

        [Checkbox("Fill Health First")]
        [Order(15)]
        public bool FillHealthFirst = true;

        [ColorEdit4("Color ##Shields")]
        [Order(20)]
        public PluginConfigColor Color = new(new Vector4(198f / 255f, 210f / 255f, 255f / 255f, 70f / 100f));
    }

    [Portable(false)]
    public class TankStanceIndicatorConfig : PluginConfigObject
    {
        [DragInt("Thickness")]
        [Order(5)]
        public int Thickness = 2;

        [ColorEdit4("Active")]
        [Order(10)]
        public PluginConfigColor ActiveColor = new(new Vector4(0f / 255f, 205f / 255f, 230f / 255f, 100f / 100f));

        [ColorEdit4("Inactive")]
        [Order(15)]
        public PluginConfigColor InactiveColor = new(new Vector4(255f / 255f, 0f / 255f, 32f / 255f, 100f / 100f));
    }
}
