using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using System.Numerics;

namespace DelvUI.Interface.Bars
{
    [Portable(false)]
    public abstract class BarConfigBase : AnchorablePluginConfigObject
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

        public BarConfigBase(Vector2 position, Vector2 size, PluginConfigColor fillColor)
        {
            Position = position;
            Size = size;
            FillColor = fillColor;
        }

        public abstract bool IsActive(float current, float max, float min);

        public abstract BarHud[] GetBars(float current, float max, float min = 0f, GameObject? actor = null);
    }

    public enum BarDirection
    {
        Left,
        Right,
        Up,
        Down
    }
}
