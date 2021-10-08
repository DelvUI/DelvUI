using DelvUI.Config;
using DelvUI.Config.Attributes;
using System.Numerics;

namespace DelvUI.Interface.Bars
{
    [Exportable(false)]
    public class ChunkedBarConfig : BarConfig
    {
        [DragInt("Padding", min = 0, max = 10000)]
        [Order(45)]
        public int Padding = 2;

        public ChunkedBarConfig(
            Vector2 position,
            Vector2 size,
            PluginConfigColor fillColor,
            int padding = 2) : base(position, size, fillColor)
        {
            Padding = padding;
        }
    }

    [Exportable(false)]
    public class ChunkedProgressBarConfig : ChunkedBarConfig
    {
        [Checkbox("Use Partial Fill Color")]
        [Order(50)]
        public bool UsePartialFillColor = false;

        [ColorEdit4("Partial Fill Color")]
        [Order(55, collapseWith = nameof(UsePartialFillColor))]
        public PluginConfigColor PartialFillColor;

        public ChunkedProgressBarConfig(
            Vector2 position,
            Vector2 size,
            PluginConfigColor fillColor,
            int padding = 2,
            PluginConfigColor? partialFillColor = null) : base(position, size, fillColor, padding)
        {
            PartialFillColor = partialFillColor ?? new PluginConfigColor(new(180f / 255f, 180f / 255f, 180f / 255f, 100f / 100f));
        }
    }
}
