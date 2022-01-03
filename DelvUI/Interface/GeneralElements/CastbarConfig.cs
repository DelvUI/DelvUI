using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Interface.Bars;
using System;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Section("Castbars")]
    [SubSection("Player", 0)]
    public class PlayerCastbarConfig : UnitFrameCastbarConfig
    {
        [Checkbox("Use Job Color", spacing = true)]
        [Order(19)]
        public bool UseJobColor = false;

        [Checkbox("Slide Cast", spacing = true)]
        [Order(60)]
        public bool ShowSlideCast = true;

        [DragInt("Time (milliseconds)", min = 0, max = 10000)]
        [Order(61, collapseWith = nameof(ShowSlideCast))]
        public int SlideCastTime = 500;

        [ColorEdit4("Color ##SlidecastColor")]
        [Order(62, collapseWith = nameof(ShowSlideCast))]
        public PluginConfigColor SlideCastColor = new PluginConfigColor(new(190f / 255f, 28f / 255f, 57f / 255f, 100f / 100f));

        public PlayerCastbarConfig(Vector2 position, Vector2 size, LabelConfig castNameConfig, NumericLabelConfig castTimeConfig)
            : base(position, size, castNameConfig, castTimeConfig)
        {

        }

        public new static PlayerCastbarConfig DefaultConfig()
        {
            var size = new Vector2(254, 24);
            var pos = new Vector2(0, HUDConstants.PlayerCastbarY);

            var castNameConfig = new LabelConfig(new Vector2(5, 0), "", DrawAnchor.Left, DrawAnchor.Left);
            var castTimeConfig = new NumericLabelConfig(new Vector2(-5, 0), "", DrawAnchor.Right, DrawAnchor.Right);
            castTimeConfig.NumberFormat = 1;

            return new PlayerCastbarConfig(pos, size, castNameConfig, castTimeConfig);
        }
    }

    [Section("Castbars")]
    [SubSection("Target", 0)]
    public class TargetCastbarConfig : UnitFrameCastbarConfig
    {
        [Checkbox("Interruptable Color", spacing = true)]
        [Order(50)]
        public bool ShowInterruptableColor = true;

        [ColorEdit4("Interruptable")]
        [Order(51, collapseWith = nameof(ShowInterruptableColor))]
        public PluginConfigColor InterruptableColor = new PluginConfigColor(new(255f / 255f, 87f / 255f, 113f / 255f, 100f / 100f));

        [Checkbox("Damage Type Colors", spacing = true)]
        [Order(60)]
        public bool UseColorForDamageTypes = true;

        [ColorEdit4("Physical")]
        [Order(61, collapseWith = nameof(UseColorForDamageTypes))]
        public PluginConfigColor PhysicalDamageColor = new PluginConfigColor(new(190f / 255f, 28f / 255f, 57f / 255f, 100f / 100f));

        [ColorEdit4("Magical")]
        [Order(62, collapseWith = nameof(UseColorForDamageTypes))]
        public PluginConfigColor MagicalDamageColor = new PluginConfigColor(new(0f / 255f, 72f / 255f, 179f / 255f, 100f / 100f));

        [ColorEdit4("Darkness")]
        [Order(63, collapseWith = nameof(UseColorForDamageTypes))]
        public PluginConfigColor DarknessDamageColor = new PluginConfigColor(new(188f / 255f, 19f / 255f, 254f / 255f, 100f / 100f));

        public TargetCastbarConfig(Vector2 position, Vector2 size, LabelConfig castNameConfig, NumericLabelConfig castTimeConfig)
            : base(position, size, castNameConfig, castTimeConfig)
        {

        }
        public new static TargetCastbarConfig DefaultConfig()
        {
            var size = new Vector2(254, 24);
            var pos = new Vector2(0, HUDConstants.BaseHUDOffsetY / 2f - size.Y / 2);

            var castNameConfig = new LabelConfig(new Vector2(5, 0), "", DrawAnchor.Left, DrawAnchor.Left);
            var castTimeConfig = new NumericLabelConfig(new Vector2(-5, 0), "", DrawAnchor.Right, DrawAnchor.Right);
            castTimeConfig.NumberFormat = 1;

            return new TargetCastbarConfig(pos, size, castNameConfig, castTimeConfig);
        }
    }

    [Section("Castbars")]
    [SubSection("Target of Target", 0)]
    public class TargetOfTargetCastbarConfig : TargetCastbarConfig
    {
        public TargetOfTargetCastbarConfig(Vector2 position, Vector2 size, LabelConfig castNameConfig, NumericLabelConfig castTimeConfig)
            : base(position, size, castNameConfig, castTimeConfig)
        {

        }
        public new static TargetOfTargetCastbarConfig DefaultConfig()
        {
            var size = new Vector2(120, 24);
            var pos = new Vector2(0, -1);

            var castNameConfig = new LabelConfig(new Vector2(0, 0), "", DrawAnchor.Center, DrawAnchor.Center);
            var castTimeConfig = new NumericLabelConfig(new Vector2(-5, 0), "", DrawAnchor.Right, DrawAnchor.Right);
            castTimeConfig.Enabled = false;
            castTimeConfig.NumberFormat = 1;

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
        public FocusTargetCastbarConfig(Vector2 position, Vector2 size, LabelConfig castNameConfig, NumericLabelConfig castTimeConfig)
            : base(position, size, castNameConfig, castTimeConfig)
        {

        }
        public new static FocusTargetCastbarConfig DefaultConfig()
        {
            var size = new Vector2(120, 24);
            var pos = new Vector2(0, -1);

            var castNameConfig = new LabelConfig(new Vector2(0, 0), "", DrawAnchor.Center, DrawAnchor.Center);
            var castTimeConfig = new NumericLabelConfig(new Vector2(-5, 0), "", DrawAnchor.Right, DrawAnchor.Right);
            castTimeConfig.Enabled = false;
            castTimeConfig.NumberFormat = 1;

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

        public UnitFrameCastbarConfig(Vector2 position, Vector2 size, LabelConfig castNameConfig, NumericLabelConfig castTimeConfig)
            : base(position, size, castNameConfig, castTimeConfig)
        {

        }
    }

    [DisableParentSettings("HideWhenInactive")]
    public abstract class CastbarConfig : BarConfig
    {
        [Checkbox("Preview")]
        [Order(3)]
        public bool Preview = false;

        [Checkbox("Show Ability Icon")]
        [Order(4)]
        public bool ShowIcon = true;

        [Checkbox("Reverse Fill Background Color")]
        [Order(5)]
        public bool UseReverseFill = false;

        [Checkbox("Show Current Cast Time + Max Cast Time")]
        [Order(6)]
        public bool ShowMaxCastTime = false;

        [NestedConfig("Cast Name", 500)]
        public LabelConfig CastNameLabel;

        [NestedConfig("Cast Time", 505)]
        public NumericLabelConfig CastTimeLabel;

        [ColorEdit4("Color" + "##ReverseFill")]
        [Order(515, collapseWith = nameof(UseReverseFill))]
        public PluginConfigColor ReverseFillColor = new(new Vector4(255f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        public CastbarConfig(Vector2 position, Vector2 size, LabelConfig castNameConfig, NumericLabelConfig castTimeConfig)
            : base(position, size, new PluginConfigColor(new(0f / 255f, 162f / 255f, 252f / 255f, 100f / 100f)), BarDirection.Right)
        {
            CastNameLabel = castNameConfig;
            CastTimeLabel = castTimeConfig;

            Strata = StrataLevel.MID;
        }
    }

    public class CastbarConfigConverter : PluginConfigObjectConverter
    {
        public CastbarConfigConverter()
        {
            SameClassFieldConverter<LabelConfig> name = new SameClassFieldConverter<LabelConfig>(
                "CastNameLabel",
                new LabelConfig(Vector2.Zero, "", DrawAnchor.Center, DrawAnchor.Center)
            );

            NewClassFieldConverter<LabelConfig, NumericLabelConfig> time = new NewClassFieldConverter<LabelConfig, NumericLabelConfig>(
                "CastTimeLabel",
                new NumericLabelConfig(new Vector2(-5, 0), "", DrawAnchor.Right, DrawAnchor.Right),
                (oldValue) =>
                {
                    NumericLabelConfig label = new NumericLabelConfig(oldValue.Position, "", oldValue.FrameAnchor, oldValue.TextAnchor);
                    label.Enabled = oldValue.Enabled;
                    label.FontID = oldValue.FontID;
                    label.NumberFormat = 1;
                    label.Color = oldValue.Color;
                    label.OutlineColor = oldValue.OutlineColor;
                    label.ShowShadow = oldValue.ShowShadow;
                    label.ShadowColor = oldValue.ShadowColor;
                    label.ShadowOffset = oldValue.ShadowOffset;
                    label.UseJobColor = oldValue.UseJobColor;

                    return label;
                });

            FieldConvertersMap.Add("CastNameConfig", name);
            FieldConvertersMap.Add("CastTimeConfig", time);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(CastbarConfig);
        }
    }
}
