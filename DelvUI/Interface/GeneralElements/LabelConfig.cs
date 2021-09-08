using DelvUI.Config;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Serializable]
    public class EditableLabelConfig : LabelConfig
    {
        public EditableLabelConfig(Vector2 position, string text, LabelTextAnchor anchor, string title = "") : base(position, text, anchor, title)
        {
        }

        public override bool DrawTextInput()
        {
            return ImGui.InputText("", ref Text, 999);
        }
    }

    [Serializable]
    public class LabelConfig : MovablePluginConfigObject
    {
        public string Text;
        public LabelTextAnchor Anchor = LabelTextAnchor.Center;
        public Vector4 Color = Vector4.One;
        public bool ShowOutline = true;
        public Vector4 OutlineColor = Vector4.UnitW;

        [JsonIgnore] public string Title;

        public LabelConfig(Vector2 position, string text, LabelTextAnchor anchor, string title = "")
        {
            Position = position;
            Text = text;
            Anchor = anchor;
            Position = position;
            Title = title;
        }

        public new bool Draw()
        {
            var changed = false;

            ImGui.Text(Title);
            ImGui.BeginGroup();
            {
                changed |= base.Draw();

                changed |= DrawTextInput();

                var options = new string[] { "Center", "Left", "Right", "Top", "Top Left", "Top Right", "Bottom", "Bottom Left", "Bottom Right" };
                var selection = (int)Anchor;
                if (ImGui.Combo("Anchor", ref selection, options, options.Length))
                {
                    Anchor = (LabelTextAnchor)selection;
                    changed = true;
                }
            }

            return changed;
        }

        public virtual bool DrawTextInput()
        {
            return false;
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
