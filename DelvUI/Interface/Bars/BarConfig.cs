using DelvUI.Config;
using DelvUI.Config.Attributes;
using System.Numerics;

namespace DelvUI.Interface.Bars
{
    [Exportable(false)]
    public class BarConfig : AnchorablePluginConfigObject
    {
        [ColorEdit4("Background Color")]
        [Order(16)]
        public PluginConfigColor BackgroundColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 50f / 100f));

        [ColorEdit4("Fill Color")]
        [Order(25)]
        public PluginConfigColor FillColor;

        [Combo("Fill Direction", new string[] { "Left", "Right", "Up", "Down" })]
        [Order(30)]
        public BarDirection FillDirection;

        [Checkbox("Show Border", spacing = true)]
        [Order(35)]
        public bool DrawBorder = true;

        [ColorEdit4("Border Color")]
        [Order(36, collapseWith = nameof(DrawBorder))]
        public PluginConfigColor BorderColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        [DragInt("Border Thickness", min = 1, max = 10)]
        [Order(37, collapseWith = nameof(DrawBorder))]
        public int BorderThickness = 1;

        [Checkbox("Hide When Inactive", spacing = true)]
        [Order(40)]
        public bool HideWhenInactive = false;

        public BarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor, BarDirection fillDirection = BarDirection.Right)
        {
            Position = position;
            Size = size;
            FillColor = fillColor;
            FillDirection = fillDirection;
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
