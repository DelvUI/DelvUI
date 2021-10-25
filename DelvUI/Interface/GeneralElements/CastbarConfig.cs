﻿using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Section("Castbars")]
    [SubSection("Player", 0)]
    public class PlayerCastbarConfig : UnitFrameCastbarConfig
    {
        [Checkbox("Use Job Color")]
        [Order(35)]
        public bool UseJobColor = false;

        [Checkbox("Slide Cast", separator = true)]
        [Order(40)]
        public bool ShowSlideCast = true;

        [DragInt("Time (milliseconds)", min = 0, max = 10000)]
        [Order(45, collapseWith = nameof(ShowSlideCast))]
        public int SlideCastTime = 500;

        [ColorEdit4("Color ##SlidecastColor")]
        [Order(50, collapseWith = nameof(ShowSlideCast))]
        public PluginConfigColor SlideCastColor = new PluginConfigColor(new(190f / 255f, 28f / 255f, 57f / 255f, 100f / 100f));

        public PlayerCastbarConfig(Vector2 position, Vector2 size, LabelConfig castNameConfig, LabelConfig castTimeConfig)
            : base(position, size, castNameConfig, castTimeConfig)
        {

        }

        public new static PlayerCastbarConfig DefaultConfig()
        {
            var size = new Vector2(254, 24);
            var pos = new Vector2(0, HUDConstants.PlayerCastbarY);

            var castNameConfig = new LabelConfig(new Vector2(5, 0), "", DrawAnchor.Left, DrawAnchor.Left);
            var castTimeConfig = new LabelConfig(new Vector2(-5, 0), "", DrawAnchor.Right, DrawAnchor.Right);

            return new PlayerCastbarConfig(pos, size, castNameConfig, castTimeConfig);
        }
    }

    [Section("Castbars")]
    [SubSection("Target", 0)]
    public class TargetCastbarConfig : UnitFrameCastbarConfig
    {
        [Checkbox("Interruptable Color", spacing = true)]
        [Order(35)]
        public bool ShowInterruptableColor = true;

        [ColorEdit4("Interruptable")]
        [Order(40, collapseWith = nameof(ShowInterruptableColor))]
        public PluginConfigColor InterruptableColor = new PluginConfigColor(new(255f / 255f, 87f / 255f, 113f / 255f, 100f / 100f));

        [Checkbox("Damage Type Colors", spacing = true)]
        [Order(45)]
        public bool UseColorForDamageTypes = true;

        [ColorEdit4("Physical")]
        [Order(50, collapseWith = nameof(UseColorForDamageTypes))]
        public PluginConfigColor PhysicalDamageColor = new PluginConfigColor(new(190f / 255f, 28f / 255f, 57f / 255f, 100f / 100f));

        [ColorEdit4("Magical")]
        [Order(55, collapseWith = nameof(UseColorForDamageTypes))]
        public PluginConfigColor MagicalDamageColor = new PluginConfigColor(new(0f / 255f, 72f / 255f, 179f / 255f, 100f / 100f));

        [ColorEdit4("Darkness")]
        [Order(60, collapseWith = nameof(UseColorForDamageTypes))]
        public PluginConfigColor DarknessDamageColor = new PluginConfigColor(new(188f / 255f, 19f / 255f, 254f / 255f, 100f / 100f));

        public TargetCastbarConfig(Vector2 position, Vector2 size, LabelConfig castNameConfig, LabelConfig castTimeConfig)
            : base(position, size, castNameConfig, castTimeConfig)
        {

        }
        public new static TargetCastbarConfig DefaultConfig()
        {
            var size = new Vector2(254, 24);
            var pos = new Vector2(0, HUDConstants.BaseHUDOffsetY / 2f - size.Y / 2);

            var castNameConfig = new LabelConfig(new Vector2(5, 0), "", DrawAnchor.Left, DrawAnchor.Left);
            var castTimeConfig = new LabelConfig(new Vector2(-5, 0), "", DrawAnchor.Right, DrawAnchor.Right);

            return new TargetCastbarConfig(pos, size, castNameConfig, castTimeConfig);
        }
    }

    [Section("Castbars")]
    [SubSection("Target of Target", 0)]
    public class TargetOfTargetCastbarConfig : TargetCastbarConfig
    {
        public TargetOfTargetCastbarConfig(Vector2 position, Vector2 size, LabelConfig castNameConfig, LabelConfig castTimeConfig)
            : base(position, size, castNameConfig, castTimeConfig)
        {

        }
        public new static TargetOfTargetCastbarConfig DefaultConfig()
        {
            var size = new Vector2(120, 24);
            var pos = new Vector2(0, -1);

            var castNameConfig = new LabelConfig(new Vector2(0, 0), "", DrawAnchor.Center, DrawAnchor.Center);
            var castTimeConfig = new LabelConfig(new Vector2(-5, 0), "", DrawAnchor.Right, DrawAnchor.Right);
            castTimeConfig.Enabled = false;

            var config = new TargetOfTargetCastbarConfig(pos, size, castNameConfig, castTimeConfig);
            config.Anchor = DrawAnchor.Top;
            config.AnchorToUnitFrame = true;
            config.ShowIcon = false;

            return config;
        }
    }

    [Section("Castbars")]
    [SubSection("Focus Target", 0)]
    public class FocusTargetCastbarConfig : TargetCastbarConfig
    {
        public FocusTargetCastbarConfig(Vector2 position, Vector2 size, LabelConfig castNameConfig, LabelConfig castTimeConfig)
            : base(position, size, castNameConfig, castTimeConfig)
        {

        }
        public new static FocusTargetCastbarConfig DefaultConfig()
        {
            var size = new Vector2(120, 24);
            var pos = new Vector2(0, -1);

            var castNameConfig = new LabelConfig(new Vector2(0, 0), "", DrawAnchor.Center, DrawAnchor.Center);
            var castTimeConfig = new LabelConfig(new Vector2(-5, 0), "", DrawAnchor.Right, DrawAnchor.Right);
            castTimeConfig.Enabled = false;

            var config = new FocusTargetCastbarConfig(pos, size, castNameConfig, castTimeConfig);
            config.Anchor = DrawAnchor.Top;
            config.AnchorToUnitFrame = true;
            config.ShowIcon = false;

            return config;
        }
    }

    public abstract class UnitFrameCastbarConfig : CastbarConfig
    {
        [Checkbox("Anchor to Unit Frame")]
        [Order(16)]
        public bool AnchorToUnitFrame = false;

        [Anchor("Unit Frame Anchor")]
        [Order(17, collapseWith = nameof(AnchorToUnitFrame))]
        public DrawAnchor UnitFrameAnchor = DrawAnchor.Bottom;

        public UnitFrameCastbarConfig(Vector2 position, Vector2 size, LabelConfig castNameConfig, LabelConfig castTimeConfig)
            : base(position, size, castNameConfig, castTimeConfig)
        {

        }
    }

    public abstract class CastbarConfig : AnchorablePluginConfigObject
    {
        [ColorEdit4("Color ##Castbar")]
        [Order(20)]
        public PluginConfigColor Color = new PluginConfigColor(new(0f / 255f, 162f / 255f, 252f / 255f, 100f / 100f));

        [ColorEdit4("BackgroundColor ##Castbar")]
        [Order(21)]
        public PluginConfigColor BackgroundColor = new PluginConfigColor(new(0f / 255f, 0f / 255f, 0f / 255f, 50f / 100f));

        [Checkbox("Show Ability Icon")]
        [Order(25)]
        public bool ShowIcon = true;

        [Checkbox("Preview")]
        [Order(30)]
        public bool Preview = false;

        //CHARA TYPE SPECIFIC CONFIGS SPAWN HERE

        [NestedConfig("Cast Name", 70)]
        public LabelConfig CastNameConfig;

        [NestedConfig("Cast Time", 75)]
        public LabelConfig CastTimeConfig;

        public CastbarConfig(Vector2 position, Vector2 size, LabelConfig castNameConfig, LabelConfig castTimeConfig)
        {
            Position = position;
            Size = size;
            CastNameConfig = castNameConfig;
            CastTimeConfig = castTimeConfig;
        }
    }
}
