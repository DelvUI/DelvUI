using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Interface.Bars;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Section("Misc")]
    [SubSection("Pull Timer", 0)]
    public class PullTimerConfig : ProgressBarConfig
    {
        [Checkbox("Use Job Color")]
        [Order(45)]
        public bool UseJobColor = false;

        [Checkbox("Show Decimals")]
        [Order(50)]
        public bool ShowDecimals = true;

        public PullTimerConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor) : base(position, size, fillColor)
        {
        }

        public new static PullTimerConfig DefaultConfig()
        {
            var config = new PullTimerConfig(
                new Vector2(0, HUDConstants.BaseHUDOffsetY - 35),
                new Vector2(254, 20),
                new PluginConfigColor(new Vector4(233f / 255f, 4f / 255f, 4f / 255f, 100f / 100f)));

            config.HideWhenInactive = true;

            return config;
        }
    }
}
