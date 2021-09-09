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
        [Order(40)]
        public bool UseJobColor = false;

        [Checkbox("Show Slide Cast")]
        [CollapseControl(45, 0)]
        public bool ShowSlideCast = true;

        [DragInt("Slide Cast Time (milliseconds)", min = 0, max = 10000)]
        [CollapseWith(0, 0)]
        public int SlideCastTime = 200;

        [ColorEdit4("Slide Cast Color")]
        [CollapseWith(5, 0)]
        public PluginConfigColor SlideCastColor = new PluginConfigColor(new(255f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        public PlayerCastbarConfig(Vector2 position, Vector2 size, LabelConfig castNameConfig, LabelConfig castTimeConfig)
            : base(position, size, castNameConfig, castTimeConfig)
        {

        }

        public new static PlayerCastbarConfig DefaultConfig()
        {
            var size = new Vector2(254, 25);
            var pos = new Vector2(0, HUDConstants.PlayerCastbarY);

            var castNameConfig = new LabelConfig(new Vector2(-size.X / 2f + size.Y + 5, 0), "", LabelTextAnchor.Left);
            var castTimeConfig = new LabelConfig(new Vector2(size.X / 2f - 5, 0), "", LabelTextAnchor.Right);

            return new PlayerCastbarConfig(pos, size, castNameConfig, castTimeConfig);
        }
    }

    [Serializable]
    [Section("Castbars")]
    [SubSection("Target", 0)]
    public class TargetCastbarConfig : CastbarConfig
    {
        [Checkbox("Show Interruptable Color")]
        [CollapseControl(40, 0)]
        public bool ShowInterruptableColor = true;

        [ColorEdit4("Interruptable Color")]
        [CollapseWith(0, 0)]
        public PluginConfigColor InterruptableColor = new PluginConfigColor(new(255f / 255f, 0 / 255f, 255f / 255f, 100f / 100f));

        [Checkbox("Use Damage Type Colors")]
        [CollapseControl(45, 1)]
        public bool UseColorForDamageTypes = true;

        [ColorEdit4("Physical Damage Color")]
        [CollapseWith(0, 1)]
        public PluginConfigColor PhysicalDamageColor = new PluginConfigColor(new(255f / 255f, 0 / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Magical Damage Color")]
        [CollapseWith(5, 1)]
        public PluginConfigColor MagicalDamageColor = new PluginConfigColor(new(0f / 255f, 0 / 255f, 255f / 255f, 100f / 100f));

        [ColorEdit4("Darkness Damage Color")]
        [CollapseWith(10, 1)]
        public PluginConfigColor DarknessDamageColor = new PluginConfigColor(new(255f / 255f, 0 / 255f, 255f / 255f, 100f / 100f));

        public TargetCastbarConfig(Vector2 position, Vector2 size, LabelConfig castNameConfig, LabelConfig castTimeConfig)
            : base(position, size, castNameConfig, castTimeConfig)
        {

        }
        public new static TargetCastbarConfig DefaultConfig()
        {
            var size = new Vector2(254, 25);
            var pos = new Vector2(0, HUDConstants.BaseHUDOffsetY / 2f - size.Y / 2);

            var castNameConfig = new LabelConfig(new Vector2(-size.X / 2f + size.Y + 5, 0), "", LabelTextAnchor.Left);
            var castTimeConfig = new LabelConfig(new Vector2(size.X / 2f - 5, 0), "", LabelTextAnchor.Right);

            return new TargetCastbarConfig(pos, size, castNameConfig, castTimeConfig);
        }
    }

    [Serializable]
    public abstract class CastbarConfig : MovablePluginConfigObject
    {
        [NestedConfig("Cast Name Label", 20)]
        public LabelConfig CastNameConfig;

        [NestedConfig("Cast Time Label", 25)]
        public LabelConfig CastTimeConfig;

        [Checkbox("Show Icon")]
        [Order(30)]
        public bool ShowIcon = true;

        [ColorEdit4("Color")]
        [Order(35)]
        public PluginConfigColor Color = new PluginConfigColor(new(255f / 255f, 158f / 255f, 208f / 255f, 100f / 100f));

        public CastbarConfig(Vector2 position, Vector2 size, LabelConfig castNameConfig, LabelConfig castTimeConfig)
        {
            Position = position;
            Size = size;
            CastNameConfig = castNameConfig;
            CastTimeConfig = castTimeConfig;
        }
    }
}
