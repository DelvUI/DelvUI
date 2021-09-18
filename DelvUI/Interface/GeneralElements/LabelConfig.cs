using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using Newtonsoft.Json;
using System;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Serializable]
    [Portable(false)]
    public class EditableLabelConfig : LabelConfig
    {
        [InputText("Text")]
        [Order(20)]
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

    [Serializable]
    [Portable(false)]
    public class LabelConfig : MovablePluginConfigObject
    {
        [JsonIgnore] protected string _text;

        [Anchor("Frame Anchor")]
        [Order(25)]
        public DrawAnchor FrameAnchor = DrawAnchor.Center;

        [Anchor("Text Anchor")]
        [Order(30)]
        public DrawAnchor TextAnchor = DrawAnchor.TopLeft;

        [ColorEdit4("Color ##Text")]
        [Order(35)]
        public PluginConfigColor Color = new PluginConfigColor(Vector4.One);

        [Checkbox("Outline")]
        [CollapseControl(40, 0)]
        public bool ShowOutline = true;

        [ColorEdit4("Color ##Outline")]
        [CollapseWith(0, 0)]
        public PluginConfigColor OutlineColor = new PluginConfigColor(Vector4.UnitW);

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
