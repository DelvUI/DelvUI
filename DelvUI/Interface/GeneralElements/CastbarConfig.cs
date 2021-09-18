using DelvUI.Config;
using DelvUI.Config.Attributes;
using System;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Serializable]
    [Section("Castbars")]
    [SubSection("Player", 0)]
    public class PlayerCastbarConfig : CastbarConfig
    {
        [Checkbox("Use Job Color")]
        [Order(35)]
        public bool UseJobColor = false;

        [Checkbox("Slide Cast", separator = true)]
        [CollapseControl(40, 0)]
        public bool ShowSlideCast = true;

        [DragInt("Time (milliseconds)", min = 0, max = 10000)]
        [CollapseWith(0, 0)]
        public int SlideCastTime = 200;

        [ColorEdit4("Color ##SlidecastColor")]
        [CollapseWith(5, 0)]
        public PluginConfigColor SlideCastColor = new PluginConfigColor(new(190f / 255f, 28f / 255f, 57f / 255f, 100f / 100f));

        public PlayerCastbarConfig(Vector2 position, Vector2 size, LabelConfig castNameConfig, LabelConfig castTimeConfig)
            : base(position, size, castNameConfig, castTimeConfig)
        {

        }

        public new static PlayerCastbarConfig DefaultConfig()
        {
            var size = new Vector2(254, 24);
            var pos = new Vector2(0, HUDConstants.PlayerCastbarY);

            var castNameConfig = new LabelConfig(new Vector2(5, 0), "", HudElementAnchor.Left, HudElementAnchor.Left);
            var castTimeConfig = new LabelConfig(new Vector2(-5, 0), "", HudElementAnchor.Right, HudElementAnchor.Right);

            return new PlayerCastbarConfig(pos, size, castNameConfig, castTimeConfig);
        }
    }

    [Serializable]
    [Section("Castbars")]
    [SubSection("Target", 0)]
    public class TargetCastbarConfig : CastbarConfig
    {
        [Checkbox("User Interruptable Color")]
        [CollapseControl(35, 0)]
        public bool ShowInterruptableColor = true;

        [ColorEdit4("Interruptable")]
        [CollapseWith(0, 0)]
        public PluginConfigColor InterruptableColor = new PluginConfigColor(new(255f / 255f, 87f / 255f, 113f / 255f, 100f / 100f));

        [Checkbox("Use Damage Type Colors")]
        [CollapseControl(40, 1)]
        public bool UseColorForDamageTypes = true;

        [ColorEdit4("Physical Damage")]
        [CollapseWith(0, 1)]
        public PluginConfigColor PhysicalDamageColor = new PluginConfigColor(new(190f / 255f, 28f / 255f, 57f / 255f, 100f / 100f));

        [ColorEdit4("Magical Damage")]
        [CollapseWith(5, 1)]
        public PluginConfigColor MagicalDamageColor = new PluginConfigColor(new(0f / 255f, 72f / 255f, 179f / 255f, 100f / 100f));

        [ColorEdit4("Darkness Damage")]
        [CollapseWith(10, 1)]
        public PluginConfigColor DarknessDamageColor = new PluginConfigColor(new(188f / 255f, 19f / 255f, 254f / 255f, 100f / 100f));

        public TargetCastbarConfig(Vector2 position, Vector2 size, LabelConfig castNameConfig, LabelConfig castTimeConfig)
            : base(position, size, castNameConfig, castTimeConfig)
        {

        }
        public new static TargetCastbarConfig DefaultConfig()
        {
            var size = new Vector2(254, 24);
            var pos = new Vector2(0, HUDConstants.BaseHUDOffsetY / 2f - size.Y / 2);

            var castNameConfig = new LabelConfig(new Vector2(5, 0), "", HudElementAnchor.Left, HudElementAnchor.Left);
            var castTimeConfig = new LabelConfig(new Vector2(-5, 0), "", HudElementAnchor.Right, HudElementAnchor.Right);

            return new TargetCastbarConfig(pos, size, castNameConfig, castTimeConfig);
        }
    }

    [Serializable]
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
            var pos = new Vector2(
                HUDConstants.UnitFramesOffsetX + HUDConstants.DefaultBigUnitFrameSize.X + 6 + size.X / 2f,
                HUDConstants.BaseHUDOffsetY + 5
            );

            var castNameConfig = new LabelConfig(new Vector2(5, 0), "", HudElementAnchor.Left, HudElementAnchor.Left);
            var castTimeConfig = new LabelConfig(new Vector2(-5, 0), "", HudElementAnchor.Right, HudElementAnchor.Right);

            return new TargetOfTargetCastbarConfig(pos, size, castNameConfig, castTimeConfig);
        }
    }

    [Serializable]
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
            var pos = new Vector2(
                -HUDConstants.UnitFramesOffsetX - HUDConstants.DefaultBigUnitFrameSize.X - 6 - size.X / 2f,
                HUDConstants.BaseHUDOffsetY + 5
            );

            var castNameConfig = new LabelConfig(new Vector2(5, 0), "", HudElementAnchor.Left, HudElementAnchor.Left);
            var castTimeConfig = new LabelConfig(new Vector2(-5, 0), "", HudElementAnchor.Right, HudElementAnchor.Right);

            return new FocusTargetCastbarConfig(pos, size, castNameConfig, castTimeConfig);
        }
    }

    [Serializable]
    public abstract class CastbarConfig : MovablePluginConfigObject
    {
        [DragInt2("Size", min = 1, max = 4000)]
        [Order(15)]
        public Vector2 Size;
        
        [Checkbox("Preview")]
        [Order(20)]
        public bool Preview = false;
        
        [Checkbox("Icon", separator = true)]
        [Order(25)]
        public bool ShowIcon = true;

        [ColorEdit4("Color ##Castbar")]
        [Order(30)]
        public PluginConfigColor Color = new PluginConfigColor(new(0f / 255f, 162f / 255f, 252f / 255f, 100f / 100f));
        
        //CHARA TYPE SPECIFIC CONFIGS SPAWN HERE
        
        [NestedConfig("Cast Name", 45)]
        public LabelConfig CastNameConfig;

        [NestedConfig("Cast Time", 50)]
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
