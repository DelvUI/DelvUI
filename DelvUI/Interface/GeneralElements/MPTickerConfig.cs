using DelvUI.Config;
using DelvUI.Config.Attributes;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Section("Misc")]
    [SubSection("MP Ticker", 0)]
    public class MPTickerConfig : AnchorablePluginConfigObject
    {
        [Checkbox("Hide on Full MP", spacing = true)]
        [Order(20)]
        public bool HideOnFullMP = true;

        [Checkbox("Show Border")]
        [Order(25)]
        public bool ShowBorder = true;

        [ColorEdit4("Color")]
        [Order(30)]
        public PluginConfigColor Color = new PluginConfigColor(new(240f / 255f, 92f / 255f, 232f / 255f, 100f / 100f));

        public MPTickerConfig(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
        }

        public new static MPTickerConfig DefaultConfig()
        {
            var size = new Vector2(254, 4);
            var pos = new Vector2(0, HUDConstants.BaseHUDOffsetY + 27);

            var config = new MPTickerConfig(pos, size);
            config.Enabled = false;

            return config;
        }
    }
}
