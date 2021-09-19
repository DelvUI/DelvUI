using DelvUI.Config;
using DelvUI.Config.Attributes;
using Newtonsoft.Json;
using System;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Portable(false)]
    public class EditableLabelConfig : LabelConfig
    {
        [InputText("Text")]
        [Order(20)]
        public string Text;

        public EditableLabelConfig(Vector2 position, string text, LabelTextAnchor frameAnchor, LabelTextAnchor textAnchor) : base(position, text, frameAnchor, textAnchor)
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
        [JsonIgnore] protected string _text;

        [Combo("Frame Anchor", "Center", "Left", "Right", "Top", "TopLeft", "TopRight", "Bottom", "BottomLeft", "BottomRight")]
        [Order(25)]
        public LabelTextAnchor FrameAnchor = LabelTextAnchor.Center;

        [Combo("Text Anchor", "Center", "Left", "Right", "Top", "TopLeft", "TopRight", "Bottom", "BottomLeft", "BottomRight")]
        [Order(30)]
        public LabelTextAnchor TextAnchor = LabelTextAnchor.TopLeft;

        [ColorEdit4("Color ##Text")]
        [Order(35)]
        public PluginConfigColor Color = new PluginConfigColor(Vector4.One);

        [Checkbox("Outline")]
        [CollapseControl(40, 0)]
        public bool ShowOutline = true;

        [Checkbox("Use Job Color")]
        [Order(45)]
        public bool UseJobColor = false;

        [ColorEdit4("Color ##Outline")]
        [CollapseWith(0, 0)]
        public PluginConfigColor OutlineColor = new PluginConfigColor(Vector4.UnitW);

        public LabelConfig(Vector2 position, string text, LabelTextAnchor frameAnchor, LabelTextAnchor textAnchor)
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

    public enum LabelTextAnchor
    {
        Center = 0,
        Left = 1,
        Right = 2,
        Top = 3,
        TopLeft = 4,
        TopRight = 5,
        Bottom = 6,
        BottomLeft = 7,
        BottomRight = 8
    }
}