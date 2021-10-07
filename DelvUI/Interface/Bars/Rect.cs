using DelvUI.Config;
using System.Numerics;

namespace DelvUI.Interface.Bars
{
    public class Rect
    {
        public Vector2 Position { get; set; }

        public Vector2 Size { get; set; }

        public PluginConfigColor Color { get; set; }

        public Rect(Vector2 pos, Vector2 size, PluginConfigColor color)
        {
            Position = pos;
            Size = size;
            Color = color;
        }

        public Rect() : this(new(0, 0), new(0, 0), new PluginConfigColor(new(0, 0, 0, 0))) { }
    }
}
