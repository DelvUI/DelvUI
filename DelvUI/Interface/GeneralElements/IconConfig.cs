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
