using DelvUI.Config;
using DelvUI.Config.Attributes;
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

        public EditableLabelConfig(Vector2 position, string text, LabelTextAnchor anchor) : base(position, text, anchor)
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

        [Combo("Anchor", "Center", "Left", "Right", "Top", "TopLeft", "TopRight", "Bottom", "BottomLeft", "BottomRight")]
        [Order(25)]
        public LabelTextAnchor Anchor = LabelTextAnchor.Center;

        [ColorEdit4("Color")]
        [Order(30)]
        public PluginConfigColor Color = new PluginConfigColor(Vector4.One);

        [Checkbox("Show Outline")]
        [CollapseControl(35, 0)]
        public bool ShowOutline = true;

        [ColorEdit4("Outline Color")]
        [CollapseWith(0, 0)]
        public PluginConfigColor OutlineColor = new PluginConfigColor(Vector4.UnitW);

        public LabelConfig(Vector2 position, string text, LabelTextAnchor anchor)
        {
            Position = position;
            _text = text;
            Anchor = anchor;
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
