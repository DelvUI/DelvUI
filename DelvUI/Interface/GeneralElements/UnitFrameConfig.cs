﻿using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Interface.Bars;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [DisableParentSettings("HideWhenInactive", "HideHealthIfPossible")]
    [Section("Unit Frames")]
    [SubSection("Player", 0)]
    public class PlayerUnitFrameConfig : UnitFrameConfig
    {
        [NestedConfig("Tank Stance Indicator", 122, spacing = true)]
        public TankStanceIndicatorConfig TankStanceIndicatorConfig = new TankStanceIndicatorConfig();

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

            return config;
        }
    }

    public enum TankStanceCorner
    {
        TopLeft = 0,
        TopRight,
        BottomLeft,
        BottomRight
    }

    [Exportable(false)]
    public class TankStanceIndicatorConfig : PluginConfigObject
    {
        [Combo("Corner", "Top Left", "Top Right", "Bottom Left", "Bottom Right")]
        [Order(5)]
        public TankStanceCorner Corner = TankStanceCorner.BottomLeft;

        [DragFloat2("Size", min = 1, max = 500)]
        [Order(10)]
        public Vector2 Size = new Vector2(HUDConstants.DefaultBigUnitFrameSize.Y - 20, HUDConstants.DefaultBigUnitFrameSize.Y - 20);

        [DragInt("Thickness", min = 2, max = 20)]
        [Order(15)]
        public int Thickess = 4;

        [ColorEdit4("Active Color")]
        [Order(20)]
        public PluginConfigColor ActiveColor = new PluginConfigColor(new Vector4(0f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [ColorEdit4("Inactive Color")]
        [Order(25)]
        public PluginConfigColor InactiveColor = new PluginConfigColor(new Vector4(255f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));
    }

    [DisableParentSettings("HideWhenInactive")]
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

    [DisableParentSettings("HideWhenInactive")]
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

    [DisableParentSettings("HideWhenInactive")]
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

    [DisableParentSettings("HideWhenInactive")]
    public class UnitFrameConfig : BarConfig
    {
        [Checkbox("Use Job Color", spacing = true)]
        [Order(45)]
        public bool UseJobColor = true;

        [Checkbox("Use Role Color")]
        [Order(46)]
        public bool UseRoleColor = false;

        [NestedConfig("Color Based On Health Value", 50, collapsingHeader = false)]
        public ColorByHealthValueConfig ColorByHealth = new ColorByHealthValueConfig();

        [Checkbox("Job Color As Background Color", spacing = true)]
        [Order(50)]
        public bool UseJobColorAsBackgroundColor = false;

        [Checkbox("Missing Health Color")]
        [Order(55)]
        public bool UseMissingHealthBar = false;

        [ColorEdit4("Color" + "##MissingHealth")]
        [Order(60, collapseWith = nameof(UseMissingHealthBar))]
        public PluginConfigColor HealthMissingColor = new PluginConfigColor(new Vector4(255f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        [Checkbox("Death Indicator Background Color", spacing = true)]
        [Order(61)]
        public bool UseDeathIndicatorBackgroundColor = false;

        [ColorEdit4("Color" + "##DeathIndicator")]
        [Order(62, collapseWith = nameof(UseDeathIndicatorBackgroundColor))]
        public PluginConfigColor DeathIndicatorBackgroundColor = new PluginConfigColor(new Vector4(204f / 255f, 3f / 255f, 3f / 255f, 50f / 100f));

        [Checkbox("Tank Invulnerability", spacing = true)]
        [Order(95)]
        public bool ShowTankInvulnerability = true;

        [Checkbox("Tank Invulnerability Custom Color")]
        [Order(100, collapseWith = nameof(ShowTankInvulnerability))]
        public bool UseCustomInvulnerabilityColor = true;

        [ColorEdit4("Tank Invulnerability Color ##TankInvulnerabilityCustom")]
        [Order(105, collapseWith = nameof(UseCustomInvulnerabilityColor))]
        public PluginConfigColor CustomInvulnerabilityColor = new PluginConfigColor(new Vector4(211f / 255f, 235f / 255f, 215f / 245f, 50f / 100f));

        [Checkbox("Walking Dead Custom Color")]
        [Order(110, collapseWith = nameof(ShowTankInvulnerability))]
        public bool UseCustomWalkingDeadColor = true;

        [ColorEdit4("Walking Dead Color ##TankWalkingDeadCustom")]
        [Order(115, collapseWith = nameof(UseCustomWalkingDeadColor))]
        public PluginConfigColor CustomWalkingDeadColor = new PluginConfigColor(new Vector4(158f / 255f, 158f / 255f, 158f / 255f, 50f / 100f));

        [NestedConfig("Use Smooth Transitions", 120, collapsingHeader = false)]
        public SmoothHealthConfig SmoothHealthConfig = new SmoothHealthConfig();

        [Checkbox("Hide Health if Possible", spacing = true, help = "This will hide any label that has a health tag if the character doesn't have health (ie minions, friendly npcs, etc)")]
        [Order(121)]
        public bool HideHealthIfPossible = true;

        [NestedConfig("Left Text", 125)]
        public EditableLabelConfig LeftLabelConfig = null!;

        [NestedConfig("Right Text", 130)]
        public EditableLabelConfig RightLabelConfig = null!;

        [NestedConfig("Role/Job Icon", 135)]
        public RoleJobIconConfig RoleIconConfig = new RoleJobIconConfig(
            new Vector2(5, 0),
            new Vector2(30, 30),
            DrawAnchor.Left,
            DrawAnchor.Left
        );

        [NestedConfig("Shields", 140)]
        public ShieldConfig ShieldConfig = new ShieldConfig();

        public UnitFrameConfig(Vector2 position, Vector2 size, EditableLabelConfig leftLabelConfig, EditableLabelConfig rightLabelConfig)
            : base(position, size, new PluginConfigColor(new(40f / 255f, 40f / 255f, 40f / 255f, 100f / 100f)))
        {
            Position = position;
            Size = size;
            LeftLabelConfig = leftLabelConfig;
            RightLabelConfig = rightLabelConfig;
            BackgroundColor = new PluginConfigColor(new(0f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));
            RoleIconConfig.Enabled = false;
            ColorByHealth.Enabled = false;
        }

        public UnitFrameConfig() : base(Vector2.Zero, Vector2.Zero, new(Vector4.Zero)) { } // don't remove
    }

    [Exportable(false)]
    public class ShieldConfig : PluginConfigObject
    {
        [DragInt("Thickness")]
        [Order(5)]
        public int Height = 26; // Should be 'Size' instead of 'Height' but leaving as is to avoid breaking configs

        [Checkbox("Thickness in Pixels")]
        [Order(10)]
        public bool HeightInPixels = false;

        [Checkbox("Fill Health First")]
        [Order(15)]
        public bool FillHealthFirst = true;

        [ColorEdit4("Color ##Shields")]
        [Order(20)]
        public PluginConfigColor Color = new PluginConfigColor(new Vector4(198f / 255f, 210f / 255f, 255f / 255f, 70f / 100f));
    }

    [Exportable(false)]
    public class SmoothHealthConfig : PluginConfigObject
    {
        [DragFloat("Velocity", min = 1f, max = 100f)]
        [Order(5)]
        public float Velocity = 25f;
    }
}
