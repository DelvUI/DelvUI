using System;
using System.Numerics;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Interface.Bars;

namespace DelvUI.Interface.GeneralElements
{
    [DisableParentSettings("HideWhenInactive")]
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

            return config;
        }
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

        [Combo("Blend Mode", "LAB", "LChab", "XYZ", "RGB", "LChuv", "Luv", "Jzazbz", "JzCzhz")]
        [Order(90, collapseWith = nameof(UseColorBasedOnHealthValue))]
        public BlendMode blendMode = BlendMode.LAB;
        
        [Checkbox("Tank Invulnerability")]
        [Order(95)]
        public bool ShowTankInvulnerability = true;

        [Checkbox("Tank Invulnerability Custom Color")]
        [Order(100, collapseWith = nameof(ShowTankInvulnerability))]
        public bool UseCustomInvulnerabilityColor = true;

        [ColorEdit4("Tank Invulnerability Color ##TankInvulnerabilityCustom")]
        [Order(105, collapseWith = nameof(UseCustomInvulnerabilityColor))]
        public PluginConfigColor CustomInvulnerabilityColor = new PluginConfigColor(new Vector4(100f / 255f, 100f / 255f, 100f / 255f, 100f / 100f));
        
        [NestedConfig("Use Smooth Transitions", 110, separator = false, nest = true)]
        public SmoothHealthConfig SmoothHealthConfig;
        
        [NestedConfig("Left Text", 115)]
        public EditableLabelConfig LeftLabelConfig;

        [NestedConfig("Right Text", 120)]
        public EditableLabelConfig RightLabelConfig;

        [NestedConfig("Shields", 125)]
        public ShieldConfig ShieldConfig = new ShieldConfig();

        public UnitFrameConfig(Vector2 position, Vector2 size, EditableLabelConfig leftLabelConfig, EditableLabelConfig rightLabelConfig)
            : base(position, size, new PluginConfigColor(new(40f / 255f, 40f / 255f, 40f / 255f, 100f / 100f)))
        {
            Position = position;
            Size = size;
            LeftLabelConfig = leftLabelConfig;
            RightLabelConfig = rightLabelConfig;
            BackgroundColor = new PluginConfigColor(new(0f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));
            SmoothHealthConfig = new SmoothHealthConfig();
        }
    }

    public enum BlendMode
    {
        LAB,
        LChab,
        XYZ,
        RGB,
        LChuv,
        Luv,
        Jzazbz,
        JzCzhz
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
        
        private float? _startHp;
        private float? _targetHp;
        private float? _lastHp;

        public uint GetNextHp(int currentHp, int maxHp)
        {
            if (!_startHp.HasValue || !_targetHp.HasValue || !_lastHp.HasValue)
            {
                _lastHp = currentHp;
                _startHp = currentHp;
                _targetHp = currentHp;
            }

            if (currentHp != _lastHp)
            {
                _startHp = _lastHp;
                _targetHp = currentHp;
            }

            if (_startHp.HasValue && _targetHp.HasValue)
            {
                float delta = _targetHp.Value - _startHp.Value;
                float offset = delta * Velocity / 100f;
                _startHp = Math.Clamp(_startHp.Value + offset, 0, maxHp);
            }
            
            _lastHp = currentHp;
            return _startHp.HasValue ? (uint)_startHp.Value : (uint)currentHp;
        }
    }
}
