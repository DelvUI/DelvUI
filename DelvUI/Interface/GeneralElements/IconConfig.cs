using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Interface.Party;
using Newtonsoft.Json.Linq;
using System;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    public class IconConfig : AnchorablePluginConfigObject
    {
        [Anchor("Frame Anchor")]
        [Order(15)]
        public DrawAnchor FrameAnchor = DrawAnchor.Center;

        public IconConfig() { } // don't remove (used by json converter)

        public IconConfig(Vector2 position, Vector2 size, DrawAnchor anchor, DrawAnchor frameAnchor)
        {
            Position = position;
            Size = size;
            Anchor = anchor;
            FrameAnchor = frameAnchor;
        }
    }

    public class IconWithDurationConfig : IconConfig
    {
        [NestedConfig("Label", 20, spacing = true, separator = false, nest = true)]
        public LabelConfig Label = new LabelConfig(Vector2.Zero, "", DrawAnchor.Center, DrawAnchor.Center);

        public IconWithDurationConfig(Vector2 position, Vector2 size, DrawAnchor anchor, DrawAnchor frameAnchor)
            : base(position, size, anchor, frameAnchor)
        {
        }
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
