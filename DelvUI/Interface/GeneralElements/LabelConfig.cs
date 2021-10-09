using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using Newtonsoft.Json;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Exportable(false)]
    public class EditableLabelConfig : LabelConfig
    {
        [InputText("Text")]
        [Order(10)]
        public string Text;

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

    [Exportable(false)]
    public class LabelConfig : MovablePluginConfigObject
    {
        [JsonIgnore] protected string _text;

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
        public PluginConfigColor Color = new PluginConfigColor(Vector4.One);

        [Checkbox("Outline")]
        [Order(35)]
        public bool ShowOutline = true;

        [ColorEdit4("Color ##Outline")]
        [Order(40, collapseWith = nameof(ShowOutline))]
        public PluginConfigColor OutlineColor = new PluginConfigColor(Vector4.UnitW);

        [Checkbox("Shadow")]
        [Order(45)]
        public bool ShowShadow = false;

        [ColorEdit4("Color ##Shadow")]
        [Order(50, collapseWith = nameof(ShowShadow))]
        public PluginConfigColor ShadowColor = new PluginConfigColor(Vector4.UnitW);
        
        [DragInt("Offset ##Shadow")]
        [Order(55, collapseWith = nameof(ShowShadow))]
        public int ShadowOffset = 1;
        
        [Checkbox("Use Job Color")]
        [Order(60)]
        public bool UseJobColor = false;

        public LabelConfig(Vector2 position, string text, DrawAnchor frameAnchor, DrawAnchor textAnchor)
        {
            Position = position;
            _text = text;
            FrameAnchor = frameAnchor;
            TextAnchor = textAnchor;
            Position = position;
        }

        public virtual string GetText()
        {
            return _text;
        }

        public virtual void SetText(string text)
        {
            _text = text;
        }
    }
}
