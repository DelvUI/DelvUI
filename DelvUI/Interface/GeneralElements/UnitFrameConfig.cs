using DelvUI.Config;
using DelvUI.Config.Attributes;
using System;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Serializable]
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

            var leftLabelConfig = new EditableLabelConfig(new Vector2(-size.X / 2f + 5, -size.Y / 2f + 2), "[name:abbreviate]", LabelTextAnchor.BottomLeft);
            var rightLabelConfig = new EditableLabelConfig(new Vector2(size.X / 2 + 10, -size.Y / 2f + 2), "[health:current-short] | [health:percent]%", LabelTextAnchor.BottomRight);

            var config = new PlayerUnitFrameConfig(pos, size, leftLabelConfig, rightLabelConfig);
            config.TankStanceIndicatorConfig = new TankStanceIndicatorConfig();

            return config;
        }
    }

    [Serializable]
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

            var leftLabelConfig = new EditableLabelConfig(new Vector2(-size.X / 2f + 5, -size.Y / 2f + 2), "[health:current-short] | [health:percent]%", LabelTextAnchor.BottomLeft);
            var rightLabelConfig = new EditableLabelConfig(new Vector2(size.X / 2 - 5, -size.Y / 2 + 2), "[name:abbreviate]", LabelTextAnchor.BottomRight);

            return new TargetUnitFrameConfig(pos, size, leftLabelConfig, rightLabelConfig);
        }
    }

    [Serializable]
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

            var leftLabelConfig = new EditableLabelConfig(new Vector2(0, -size.Y / 2f + 2), "[name:abbreviate]", LabelTextAnchor.Bottom);
            var rightLabelConfig = new EditableLabelConfig(new Vector2(0, size.Y / 2f), "", LabelTextAnchor.Top);

            return new TargetOfTargetUnitFrameConfig(pos, size, leftLabelConfig, rightLabelConfig);
        }
    }

    [Serializable]
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

            var leftLabelConfig = new EditableLabelConfig(new Vector2(0, -size.Y / 2f + 2), "[name:abbreviate]", LabelTextAnchor.Bottom);
            var rightLabelConfig = new EditableLabelConfig(new Vector2(0, size.Y / 2f), "", LabelTextAnchor.Top);

            return new FocusTargetUnitFrameConfig(pos, size, leftLabelConfig, rightLabelConfig);
        }
    }

    [Serializable]
    public class UnitFrameConfig : MovablePluginConfigObject
    {
        [NestedConfig("Left Label", 20)]
        public EditableLabelConfig LeftLabelConfig;

        [NestedConfig("Right Label", 25)]
        public EditableLabelConfig RightLabelConfig;

        [Checkbox("Use Custom Color")]
        [CollapseControl(30, 0)]
        public bool UseCustomColor = false;

        [ColorEdit4("Custom Color")]
        [CollapseWith(0, 0)]
        public PluginConfigColor CustomColor = new PluginConfigColor(new Vector4(0f / 255f, 145f / 255f, 6f / 255f, 100f / 100f));

        [Checkbox("Use Custom Background Color")]
        [CollapseControl(35, 1)]
        public bool UseCustomBackgroundColor = false;

        [ColorEdit4("Custom Background Color")]
        [CollapseWith(0, 1)]
        public PluginConfigColor CustomBackgroundColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        [Checkbox("Show Tank Invulnerability")]
        [CollapseControl(40, 2)]
        public bool ShowTankInvulnerability = true;

        [ColorEdit4("Invulnerability Color")]
        [CollapseWith(0, 2)]
        public PluginConfigColor InvulnerabilityColor = new PluginConfigColor(new Vector4(100f / 255f, 100f / 255f, 1000f / 255f, 100f / 100f));

        [NestedConfig("Shields", 45)]
        public ShieldConfig ShieldConfig = new ShieldConfig();

        [NestedConfig("Tank Stance", 50)]
        public TankStanceIndicatorConfig TankStanceIndicatorConfig = null;

        public UnitFrameConfig(Vector2 position, Vector2 size, EditableLabelConfig leftLabelConfig, EditableLabelConfig rightLabelConfig)
        {
            Position = position;
            Size = size;
            LeftLabelConfig = leftLabelConfig;
            RightLabelConfig = rightLabelConfig;
        }
    }

    [Serializable]
    [Portable(false)]
    public class ShieldConfig : PluginConfigObject
    {
        [DragInt("Height")]
        [Order(5)]
        public int Height = 10;

        [Checkbox("Height in Pixels")]
        [Order(10)]
        public bool HeightInPixels = false;

        [Checkbox("Fill Health First")]
        [Order(15)]
        public bool FillHealthFirst = true;

        [ColorEdit4("Color")]
        [Order(20)]
        public PluginConfigColor Color = new PluginConfigColor(new Vector4(198f / 255f, 210f / 255f, 255f / 255f, 70f / 100f));
    }

    [Serializable]
    [Portable(false)]
    public class TankStanceIndicatorConfig : PluginConfigObject
    {
        [DragInt("Thickness")]
        [Order(5)]
        public int Thickness = 2;

        [ColorEdit4("Active Color")]
        [Order(10)]
        public PluginConfigColor ActiveColor = new PluginConfigColor(new Vector4(0f / 255f, 205f / 255f, 230f / 255f, 100f / 100f));

        [ColorEdit4("Unactive Color")]
        [Order(15)]
        public PluginConfigColor UnactiveColor = new PluginConfigColor(new Vector4(255f / 255f, 0f / 255f, 32f / 255f, 100f / 100f));
    }
}
