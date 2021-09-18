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

            var leftLabelConfig = new EditableLabelConfig(new Vector2(5, 0), "[name:abbreviate]", LabelTextAnchor.TopLeft, LabelTextAnchor.BottomLeft);
            var rightLabelConfig = new EditableLabelConfig(new Vector2(-5, 0), "[health:current-short] | [health:percent]", LabelTextAnchor.TopRight, LabelTextAnchor.BottomRight);

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

            var leftLabelConfig = new EditableLabelConfig(new Vector2(5, 0), "[health:current-short] | [health:percent]", LabelTextAnchor.TopLeft, LabelTextAnchor.BottomLeft);
            var rightLabelConfig = new EditableLabelConfig(new Vector2(-5, 0), "[name:abbreviate]", LabelTextAnchor.TopRight, LabelTextAnchor.BottomRight);

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

            var leftLabelConfig = new EditableLabelConfig(new Vector2(0, 0), "[name:abbreviate]", LabelTextAnchor.Top, LabelTextAnchor.Bottom);
            var rightLabelConfig = new EditableLabelConfig(new Vector2(0, 0), "", LabelTextAnchor.Center, LabelTextAnchor.TopLeft);

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

            var leftLabelConfig = new EditableLabelConfig(new Vector2(0, 0), "[name:abbreviate]", LabelTextAnchor.Top, LabelTextAnchor.Bottom);
            var rightLabelConfig = new EditableLabelConfig(new Vector2(0, 0), "", LabelTextAnchor.Center, LabelTextAnchor.Center);

            return new FocusTargetUnitFrameConfig(pos, size, leftLabelConfig, rightLabelConfig);
        }
    }

    [Serializable]
    public class UnitFrameConfig : MovablePluginConfigObject
    {
        [DragInt2("Size", min = 1, max = 4000)]
        [Order(15)]
        public Vector2 Size;
        
        [Checkbox("Custom Frame Color", separator = true)]
        [CollapseControl(20, 0)]
        public bool UseCustomColor = false;

        [ColorEdit4("Color ##CustomFrame")]
        [CollapseWith(0, 0)]
        public PluginConfigColor CustomColor = new PluginConfigColor(new Vector4(0f / 255f, 145f / 255f, 6f / 255f, 100f / 100f));

        [Checkbox("Custom Background Color")]
        [CollapseControl(25, 1)]
        public bool UseCustomBackgroundColor = false;

        [ColorEdit4("Color ##CustomBackground")]
        [CollapseWith(0, 1)]
        public PluginConfigColor CustomBackgroundColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        [Checkbox("Tank Invulnerability")]
        [Order(30)]
        public bool ShowTankInvulnerability = true;

        [Checkbox("Tank Invulnerability Custom Color")]
        [CollapseControl(35, 2)]
        public bool UseCustomInvulnerabilityColor = true;

        [ColorEdit4("Color ##TankInvulnerabilityCustom")]
        [CollapseWith(0, 2)]
        public PluginConfigColor CustomInvulnerabilityColor = new PluginConfigColor(new Vector4(100f / 255f, 100f / 255f, 100f / 255f, 100f / 100f));

        [NestedConfig("Left Text", 40)]
        public EditableLabelConfig LeftLabelConfig;

        [NestedConfig("Right Text", 45)]
        public EditableLabelConfig RightLabelConfig;

        
        [NestedConfig("Shields", 50)]
        public ShieldConfig ShieldConfig = new ShieldConfig();

        [NestedConfig("Tank Stance", 51)]
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
        public int Height = 26;

        [Checkbox("Height in Pixels")]
        [Order(10)]
        public bool HeightInPixels = false;

        [Checkbox("Fill Health First")]
        [Order(15)]
        public bool FillHealthFirst = true;

        [ColorEdit4("Color ##Shields")]
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

        [ColorEdit4("Active")]
        [Order(10)]
        public PluginConfigColor ActiveColor = new PluginConfigColor(new Vector4(0f / 255f, 205f / 255f, 230f / 255f, 100f / 100f));

        [ColorEdit4("Inactive")]
        [Order(15)]
        public PluginConfigColor InactiveColor = new PluginConfigColor(new Vector4(255f / 255f, 0f / 255f, 32f / 255f, 100f / 100f));
    }
}
