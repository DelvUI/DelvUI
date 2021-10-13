using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Interface.Bars;
using ImGuiNET;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Section("Misc")]
    [SubSection("Limit Break", 0)]
    public class LimitBreakConfig : ChunkedProgressBarConfig
    {
        public LimitBreakConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor) : base(position, size, fillColor)
        {
        }

        public new static LimitBreakConfig DefaultConfig()
        {
            var config = new LimitBreakConfig(
                new Vector2(0, -ImGui.GetMainViewport().Size.Y * 0.4f),
                new Vector2(500, 10),
                new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 0f / 255f, 100f / 100f)));

            config.HideWhenInactive = true;
            config.UsePartialFillColor = true;
            config.PartialFillColor = new PluginConfigColor(new Vector4(0f / 255f, 181f / 255f, 255f / 255f, 100f / 100f));

            return config;
        }
    }
}
