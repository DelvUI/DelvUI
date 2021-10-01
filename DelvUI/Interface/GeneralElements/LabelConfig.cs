using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using Newtonsoft.Json;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Portable(false)]
    public class EditableLabelConfig : LabelConfig
    {
        [InputText("Text")]
        [Order(10)]
        public new string Text;

        public EditableLabelConfig(Vector2 position, string text, DrawAnchor frameAnchor, DrawAnchor textAnchor) : base(position, text, frameAnchor, textAnchor)
        {
            Text = text;
        }

        public override string GetText()
        {
            return Text;
        }

        public override void SetText(string text)
        {
            Text = text;
        }
    }

    [Portable(false)]
    public class LabelConfig : MovablePluginConfigObject
    {
        [JsonIgnore] protected string Text;

        [Font]
        [Order(15)]
        public string? FontID = null;

        [Anchor("Frame Anchor")]
        [Order(20)]
        public DrawAnchor FrameAnchor = DrawAnchor.Center;

        [Anchor("Text Anchor")]
        [Order(25)]
        public DrawAnchor TextAnchor = DrawAnchor.TopLeft;

        [ColorEdit4("Color ##Text")]
        [Order(30)]
        public PluginConfigColor Color = new(Vector4.One);

        [Checkbox("Outline")]
        [Order(35)]
        public bool ShowOutline = true;

        [ColorEdit4("Color ##Outline")]
        [Order(40, collapseWith = nameof(ShowOutline))]
        public PluginConfigColor OutlineColor = new(Vector4.UnitW);

        [Checkbox("Use Job Color")]
        [Order(45)]
        public bool UseJobColor = false;

        public LabelConfig(Vector2 position, string text, DrawAnchor frameAnchor, DrawAnchor textAnchor)
        {
            Position = position;
            Text = text;
            FrameAnchor = frameAnchor;
            TextAnchor = textAnchor;
            Position = position;
        }

        public virtual string GetText()
        {
            return Text;
        }

        public virtual void SetText(string text)
        {
            Text = text;
        }
    }
}
