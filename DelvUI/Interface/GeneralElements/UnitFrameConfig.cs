﻿using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Interface.Bars;
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

        public new static PlayerUnitFrameConfig DefaultConfig()
        {
            var size = HUDConstants.DefaultBigUnitFrameSize;
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

        public new static TargetUnitFrameConfig DefaultConfig()
        {
            var size = HUDConstants.DefaultBigUnitFrameSize;
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

        public new static TargetOfTargetUnitFrameConfig DefaultConfig()
        {
            var size = HUDConstants.DefaultSmallUnitFrameSize;
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

        public new static FocusTargetUnitFrameConfig DefaultConfig()
        {
            var size = HUDConstants.DefaultSmallUnitFrameSize;
            var pos = new Vector2(
                -HUDConstants.UnitFramesOffsetX - HUDConstants.DefaultBigUnitFrameSize.X - 6 - size.X / 2f,
                HUDConstants.BaseHUDOffsetY - 15
            );

            var leftLabelConfig = new EditableLabelConfig(new Vector2(0, 0), "[name:abbreviate]", DrawAnchor.Top, DrawAnchor.Bottom);
            var rightLabelConfig = new EditableLabelConfig(new Vector2(0, 0), "", DrawAnchor.Center, DrawAnchor.Center);

            return new FocusTargetUnitFrameConfig(pos, size, leftLabelConfig, rightLabelConfig);
        }
    }

    public class UnitFrameConfig : BarConfig
    {
        [Checkbox("Use Job Color")]
        [Order(45)]
        public bool UseJobColor = true;

        [Checkbox("Job Color As Background Color")]
        [Order(50)]
        public bool UseJobColorAsBackgroundColor = false;

        [Checkbox("Missing Health Color")]
        [Order(55)]
        public bool UseMissingHealthBar = false;

        [ColorEdit4("Color")]
        [Order(60, collapseWith = nameof(UseMissingHealthBar))]
        public PluginConfigColor HealthMissingColor = new PluginConfigColor(new Vector4(255f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        [Checkbox("Color Based On Health Value")]
        [Order(65)]
        public bool UseColorBasedOnHealthValue = false;

        [ColorEdit4("Full Health Color ##CustomFrame")]
        [Order(70, collapseWith = nameof(UseColorBasedOnHealthValue))]
        public PluginConfigColor FullHealthColor = new PluginConfigColor(new Vector4(0f / 255f, 255f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Low Health Color ##CustomFrame")]
        [Order(75, collapseWith = nameof(UseColorBasedOnHealthValue))]
        public PluginConfigColor LowHealthColor = new PluginConfigColor(new Vector4(255f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        [DragFloat("Full Health Color Above Health %", min = 50f, max = 100f, velocity = 1f)]
        [Order(80, collapseWith = nameof(UseColorBasedOnHealthValue))]
        public float FullHealthColorThreshold = 75f;

        [DragFloat("Low Health Color Below Health %", min = 0f, max = 50f, velocity = 1f)]
        [Order(85, collapseWith = nameof(UseColorBasedOnHealthValue))]
        public float LowHealthColorThreshold = 25f;

        [Checkbox("Tank Invulnerability")]
        [Order(90)]
        public bool ShowTankInvulnerability = true;

        [Checkbox("Tank Invulnerability Custom Color")]
        [Order(95, collapseWith = nameof(ShowTankInvulnerability))]
        public bool UseCustomInvulnerabilityColor = true;

        [ColorEdit4("Tank Invulnerability Color ##TankInvulnerabilityCustom")]
        [Order(100, collapseWith = nameof(UseCustomInvulnerabilityColor))]
        public PluginConfigColor CustomInvulnerabilityColor = new PluginConfigColor(new Vector4(100f / 255f, 100f / 255f, 100f / 255f, 100f / 100f));

        [NestedConfig("Left Text", 105)]
        public EditableLabelConfig LeftLabelConfig;

        [NestedConfig("Right Text", 110)]
        public EditableLabelConfig RightLabelConfig;

        [NestedConfig("Shields", 115)]
        public ShieldConfig ShieldConfig = new ShieldConfig();

        [NestedConfig("Tank Stance", 120)]
        public TankStanceIndicatorConfig? TankStanceIndicatorConfig = null;

        public UnitFrameConfig(Vector2 position, Vector2 size, EditableLabelConfig leftLabelConfig, EditableLabelConfig rightLabelConfig)
            : base(position, size, new PluginConfigColor(new(40f / 255f, 40f / 255f, 40f / 255f, 100f / 100f)))
        {
            Position = position;
            Size = size;
            LeftLabelConfig = leftLabelConfig;
            RightLabelConfig = rightLabelConfig;
            BackgroundColor = new PluginConfigColor(new(0f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));
        }
    }

    [Exportable(false)]
    public class ShieldConfig : PluginConfigObject
    {
        [DragInt("Thickness")]
        [Order(5)]
        public int Size = 26;

        [Checkbox("Thickness in Pixels")]
        [Order(10)]
        public bool SizeInPixels = false;

        [Checkbox("Fill Health First")]
        [Order(15)]
        public bool FillHealthFirst = true;

        [ColorEdit4("Color ##Shields")]
        [Order(20)]
        public PluginConfigColor Color = new PluginConfigColor(new Vector4(198f / 255f, 210f / 255f, 255f / 255f, 70f / 100f));
    }

    [Exportable(false)]
    public class TankStanceIndicatorConfig : PluginConfigObject
    {
        [DragInt("Thickness")]
        [Order(5)]
        public int Thickness = 2;

        [ColorEdit4("Active")]
        [Order(10)]
        public PluginConfigColor ActiveColor = new PluginConfigColor(new Vector4(0f / 255f, 205f / 255f, 230f / 255f, 100f / 100f));

        [ColorEdit4("Inactive")]
        [Order(15)]
        public PluginConfigColor InactiveColor = new PluginConfigColor(new Vector4(255f / 255f, 0f / 255f, 32f / 255f, 100f / 100f));
    }
}
