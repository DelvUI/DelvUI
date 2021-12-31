using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Interface.Party;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    public class IconConfig : AnchorablePluginConfigObject
    {
        [Anchor("Frame Anchor")]
        [Order(15)]
        public DrawAnchor FrameAnchor = DrawAnchor.Center;

        // don't remove (used by json converter)
        public IconConfig()
        {
            Strata = StrataLevel.MID_HIGH;
        }

        public IconConfig(Vector2 position, Vector2 size, DrawAnchor anchor, DrawAnchor frameAnchor)
        {
            Position = position;
            Size = size;
            Anchor = anchor;
            FrameAnchor = frameAnchor;

            Strata = StrataLevel.MID_HIGH;
        }
    }

    public class IconWithLabelConfig : IconConfig
    {
        [NestedConfig("Label", 20)]
        public LabelConfig Label = new LabelConfig(Vector2.Zero, "", DrawAnchor.Center, DrawAnchor.Center);

        public IconWithLabelConfig(Vector2 position, Vector2 size, DrawAnchor anchor, DrawAnchor frameAnchor)
            : base(position, size, anchor, frameAnchor)
        {
        }
    }

    public class RoleJobIconConfig : IconConfig
    {
        public RoleJobIconConfig() : base() { }

        public RoleJobIconConfig(Vector2 position, Vector2 size, DrawAnchor anchor, DrawAnchor frameAnchor)
            : base(position, size, anchor, frameAnchor)
        {
        }

        [Combo("Style", "Style 1", "Style 2", spacing = true)]
        [Order(25)]
        public int Style = 0;

        [Checkbox("Use Role Icons", spacing = true)]
        [Order(30)]
        public bool UseRoleIcons = false;

        [Checkbox("Use Specific DPS Role Icons")]
        [Order(35, collapseWith = nameof(UseRoleIcons))]
        public bool UseSpecificDPSRoleIcons = false;
    }

    public class PartyFramesIconsConverter : PluginConfigObjectConverter
    {
        public PartyFramesIconsConverter()
        {
            SameTypeFieldConverter<DrawAnchor> converter = new SameTypeFieldConverter<DrawAnchor>("FrameAnchor", DrawAnchor.Center);
            FieldConvertersMap.Add("HealthBarAnchor", converter);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PartyFramesRoleIconConfig) ||
                   objectType == typeof(PartyFramesLeaderIconConfig);
        }
    }
}
