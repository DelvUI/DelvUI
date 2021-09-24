using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Interface.GeneralElements;
using System.Numerics;

namespace DelvUI.Interface.Bars
{
    [Portable(false)]
    public class BarConfig : AnchorablePluginConfigObject
    {
        [ColorEdit4("Background Color")]
        [Order(20)]
        public PluginConfigColor BackgroundColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 50f / 100f));

        [ColorEdit4("Fill Color")]
        [Order(25)]
        public PluginConfigColor FillColor = new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [Combo("Fill Direction", new string[] { "Left", "Right", "Up", "Down" })]
        [Order(30)]
        public BarDirection FillDirection = BarDirection.Right;

        [Checkbox("Draw Border")]
        [Order(35)]
        public bool DrawBorder = true;

        [Checkbox("Chunk Bar" + "##Chunk")]
        [CollapseControl(45, 1)]
        public bool Chunk = false;

        [DragInt("Chunk Number" + "##Chunk", min = 2, max = 10)]
        [CollapseWith(0, 1)]
        public int ChunkNum = 2;

        [DragInt("Chunk Padding" + "##Chunk", min = 0, max = 4000)]
        [CollapseWith(5, 1)]
        public int ChunkPadding = 2;

        [NestedConfig("Bar Text", 50)]
        public LabelConfig BarLabelConfig;

        public BarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
        {
            Position = position;
            Size = size;
            BarLabelConfig = new LabelConfig(position, "", DrawAnchor.Center, DrawAnchor.Center);
            FillColor = fillColor;
        }
    }

    public enum BarDirection
    {
        Left,
        Right,
        Up,
        Down
    }
}
