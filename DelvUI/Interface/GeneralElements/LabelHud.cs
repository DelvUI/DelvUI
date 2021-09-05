using Dalamud.Game.ClientState.Actors.Types;
using DelvUI.Config;
using DelvUI.Helpers;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    public class LabelHud : HudElement
    {
        private LabelConfig Config => (LabelConfig)_config;

        public LabelHud(LabelConfig config) : base(config)
        {

        }

        public override void Draw(Vector2 origin)
        {
            if (!Config.Enabled || Config.Text == null)
            {
                return;
            }

            var size = ImGui.CalcTextSize(Config.Text);
            var offset = OffsetForSize(size);
            DrawHelper.DrawOutlinedText(Config.Text, origin + Config.Position, Config.Color, Config.OutlineColor);
        }

        public void DrawWithActor(Vector2 origin, Actor actor)
        {
            if (!Config.Enabled || Config.Text == null)
            {
                return;
            }

            var text = TextTags.GenerateFormattedTextFromTags(actor, Config.Text);
            var size = ImGui.CalcTextSize(text);
            var offset = OffsetForSize(size);

            var drawList = ImGui.GetWindowDrawList();
            DrawHelper.DrawOutlinedText(text, origin + Config.Position + offset, Config.Color, Config.OutlineColor);
        }

        private Vector2 OffsetForSize(Vector2 size)
        {
            switch (Config.Anchor)
            {
                case LabelTextAnchor.Center: return -size / 2f;
                case LabelTextAnchor.Left: return new Vector2(0, -size.Y / 2f);
                case LabelTextAnchor.Right: return new Vector2(size.X, -size.Y / 2f);
                case LabelTextAnchor.Top: return new Vector2(-size.X / 2f, 0);
                case LabelTextAnchor.TopLeft: return Vector2.Zero;
                case LabelTextAnchor.TopRight: return new Vector2(size.X, 0);
                case LabelTextAnchor.Bottom: return new Vector2(-size.X / 2f, -size.Y);
                case LabelTextAnchor.BottomLeft: return new Vector2(0, -size.Y);
                case LabelTextAnchor.BottomRight: return -size;
            }

            return Vector2.Zero;
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

    [Serializable]
    public class LabelConfig : MovablePluginConfigObject
    {
        public string Text;
        public LabelTextAnchor Anchor = LabelTextAnchor.Center;
        public Vector4 Color = Vector4.One;
        public bool ShowOutline = true;
        public Vector4 OutlineColor = Vector4.UnitW;

        [JsonIgnore] public string Title;

        public LabelConfig(Vector2 position, string text, LabelTextAnchor anchor, string title = "") : base(position)
        {
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
                changed |= ImGui.InputText("", ref Text, 999);

                string[] options = new string[] { "Center", "Left", "Right", "Top", "Top Left", "Top Right", "Bottom", "Bottom Left", "Bottom Right" };
                var selection = (int)Anchor;
                if (ImGui.Combo("Anchor", ref selection, options, options.Length))
                {
                    Anchor = (LabelTextAnchor)selection;
                    changed = true;
                }
            }

            return changed;
        }
    }
}
