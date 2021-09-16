using Dalamud.Game.ClientState.Actors.Types;
using DelvUI.Helpers;
using ImGuiNET;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    public class LabelHud : HudElement
    {
        private LabelConfig Config => (LabelConfig)_config;

        public LabelHud(string id, LabelConfig config) : base(id, config)
        {

        }

        public override void Draw(Vector2 origin)
        {
            if (!Config.Enabled || Config.GetText() == null)
            {
                return;
            }

            DrawLabel(Config.GetText(), origin, Vector2.Zero);
        }

        public void DrawRelativeToParent(Vector2 parentOrigin, Vector2 parentSize)
        {
            if (!Config.Enabled || Config.GetText() == null)
            {
                return;
            }

            DrawLabel(Config.GetText(), parentOrigin, parentSize);
        }

        public void DrawRelativeToParent(Vector2 parentOrigin, Vector2 parentSize, Actor actor)
        {
            if (!Config.Enabled || Config.GetText() == null)
            {
                return;
            }

            var text = TextTags.GenerateFormattedTextFromTags(actor, Config.GetText());
            DrawLabel(text, parentOrigin, parentSize);
        }

        private void DrawLabel(string text, Vector2 parentOrigin, Vector2 parentSize)
        {
            var textSize = ImGui.CalcTextSize(text);
            var offset = OffsetForSize(textSize, parentSize);

            if (Config.ShowOutline)
            {
                DrawHelper.DrawOutlinedText(text, parentOrigin + Config.Position + offset, Config.Color.Vector, Config.OutlineColor.Vector);
            }
            else
            {
                ImGui.SetCursorPos(parentOrigin + Config.Position + offset);
                ImGui.TextColored(Config.Color.Vector, text);
            }
        }

        private Vector2 OffsetForSize(Vector2 textSize, Vector2 parentSize)
        {
            switch (Config.Anchor)
            {
                case LabelTextAnchor.Center: return -textSize / 2f;
                case LabelTextAnchor.Left: return new Vector2(-parentSize.X / 2f, -textSize.Y / 2f);
                case LabelTextAnchor.Right: return new Vector2(parentSize.X / 2f - textSize.X, -textSize.Y / 2f);
                case LabelTextAnchor.Top: return new Vector2(-textSize.X / 2f, -parentSize.Y / 2f);
                case LabelTextAnchor.TopLeft: return -parentSize / 2f;
                case LabelTextAnchor.TopRight: return new Vector2(parentSize.X / 2f - textSize.X, -parentSize.Y / 2f);
                case LabelTextAnchor.Bottom: return new Vector2(-textSize.X / 2f, parentSize.Y / 2f - textSize.Y);
                case LabelTextAnchor.BottomLeft: return new Vector2(-parentSize.X / 2f, parentSize.Y / 2f - textSize.Y);
                case LabelTextAnchor.BottomRight: return new Vector2(parentSize.X / 2f - textSize.X, parentSize.Y / 2f - textSize.Y);
            }

            return Vector2.Zero;
        }
    }
}
