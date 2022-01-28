using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Exportable(false)]
    public class EditableLabelConfig : LabelConfig
    {
        [InputText("Text")]
        [Order(10)]
        public string Text;

        public EditableLabelConfig(Vector2 position, string text, DrawAnchor frameAnchor, DrawAnchor textAnchor)
            : base(position, text, frameAnchor, textAnchor)
        {
            Text = text;
        }

        public override string GetText() => Text;

        public override void SetText(string text)
        {
            Text = text;
        }
    }

    [Exportable(false)]
    public class NumericLabelConfig : LabelConfig
    {
        [Combo("Number Format", "No Decimals (i.e. \"12\")", "One Decimal (i.e. \"12.3\")", "Two Decimals (i.e. \"12.34\")")]
        [Order(10)]
        public int NumberFormat;
        
        [Combo("Rounding Mode", "Truncate", "Floor", "Ceil", "Round")]
        [Order(15)]
        public int NumberFunction;

        [Checkbox("Hide Text When Zero")]
        [Order(65)]
        public bool HideIfZero = false;

        public NumericLabelConfig(Vector2 position, string text, DrawAnchor frameAnchor, DrawAnchor textAnchor)
            : base(position, text, frameAnchor, textAnchor)
        {
        }

        public void SetValue(float value)
        {
            if (value == 0)
            {
                _text = HideIfZero ? string.Empty : "0";
                return;
            }

            int aux = (int)Math.Pow(10, NumberFormat);
            double textValue = value * aux;

            textValue = NumberFunction switch
            {
                0 => Math.Truncate(textValue),
                1 => Math.Floor(textValue),
                2 => Math.Ceiling(textValue),
                3 => Math.Round(textValue),
                var _ => Math.Truncate(textValue)
            };

            double v = textValue / aux;
            _text = v.ToString($"F{NumberFormat}", CultureInfo.InvariantCulture);
        }

        public NumericLabelConfig Clone() =>
            new NumericLabelConfig(Position, _text, FrameAnchor, TextAnchor)
            {
                Color = Color,
                OutlineColor = OutlineColor,
                ShadowColor = ShadowColor,
                ShadowOffset = ShadowOffset,
                ShowOutline = ShowOutline,
                ShowShadow = ShowShadow,
                FontID = FontID,
                UseJobColor = UseJobColor,
                Enabled = Enabled,
                HideIfZero = HideIfZero
            };
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
        public int ShadowOffset = 2;

        [Checkbox("Use Job Color")]
        [Order(60)]
        public bool UseJobColor = false;

        [Checkbox("Use Role Color")]
        [Order(65)]
        public bool UseRoleColor = false;

        public LabelConfig(Vector2 position, string text, DrawAnchor frameAnchor, DrawAnchor textAnchor)
        {
            Position = position;
            _text = text;
            FrameAnchor = frameAnchor;
            TextAnchor = textAnchor;
            Position = position;

            Strata = StrataLevel.HIGHEST;
        }

        public virtual string GetText() => _text;

        public virtual void SetText(string text)
        {
            _text = text;
        }
    }
}
