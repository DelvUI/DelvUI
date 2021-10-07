using DelvUI.Config;
using DelvUI.Config.Attributes;
using System.Numerics;

namespace DelvUI.Interface.Bars
{
    [Exportable(false)]
    public class BarConfig : AnchorablePluginConfigObject
    {
        [ColorEdit4("Background Color")]
        [Order(20)]
        public PluginConfigColor BackgroundColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 50f / 100f));

        [ColorEdit4("Fill Color")]
        [Order(25)]
        public PluginConfigColor FillColor;

        [Combo("Fill Direction", new string[] { "Left", "Right", "Up", "Down" })]
        [Order(30)]
        public BarDirection FillDirection = BarDirection.Right;

        [Checkbox("Show Border")]
        [Order(35)]
        public bool DrawBorder = true;

        [Checkbox("Hide When Inactive")]
        [Order(40)]
        public bool HideWhenInactive = false;

        public BarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
        {
            Position = position;
            Size = size;
            FillColor = fillColor;
        }
    }

    [Exportable(false)]
    public class BarGlowConfig : PluginConfigObject
    {
        [ColorEdit4("Color")]
        [Order(5)]
        public PluginConfigColor Color = new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 50f / 100f));

        [DragInt("Size", min = 1, max = 100)]
        [Order(25)]
        public int Size = 1;
    }

    public enum BarDirection
    {
        Left,
        Right,
        Up,
        Down
    }
}
