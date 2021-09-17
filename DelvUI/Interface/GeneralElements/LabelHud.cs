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
            Draw(origin, null, null);
        }

        public void Draw(Vector2 origin, Vector2? parentSize = null, Actor actor = null)
        {
            if (!Config.Enabled || Config.GetText() == null)
            {
                return;
            }

            var text = actor != null ? TextTags.GenerateFormattedTextFromTags(actor, Config.GetText()) : Config.GetText();
            var size = parentSize ?? Vector2.Zero;

            DrawLabel(text, origin, size);
        }

        private void DrawLabel(string text, Vector2 parentOrigin, Vector2 parentSize)
        {
            var textSize = ImGui.CalcTextSize(text);
            var offset = OffsetForFrameAnchor(parentSize) + OffsetForTextAnchor(textSize);
            var drawList = ImGui.GetWindowDrawList();

            if (Config.ShowOutline)
            {
                DrawHelper.DrawOutlinedText(text, parentOrigin + Config.Position + offset, Config.Color.Base, Config.OutlineColor.Base, drawList);
            }
            else
            {
                drawList.AddText(parentOrigin + Config.Position + offset, Config.Color.Base, text);
            }
        }

        private Vector2 OffsetForTextAnchor(Vector2 textSize)
        {
            switch (Config.TextAnchor)
            {
                case LabelTextAnchor.Center: return -textSize / 2f;
                case LabelTextAnchor.Left: return new Vector2(0, -textSize.Y / 2f);
                case LabelTextAnchor.Right: return new Vector2(-textSize.X, -textSize.Y / 2f);
                case LabelTextAnchor.Top: return new Vector2(-textSize.X / 2f, 0);
                case LabelTextAnchor.TopLeft: return Vector2.Zero;
                case LabelTextAnchor.TopRight: return new Vector2(-textSize.X, 0);
                case LabelTextAnchor.Bottom: return new Vector2(-textSize.X / 2f, -textSize.Y);
                case LabelTextAnchor.BottomLeft: return new Vector2(0, -textSize.Y);
                case LabelTextAnchor.BottomRight: return new Vector2(-textSize.X, -textSize.Y);
            }

            return Vector2.Zero;
        }

        private Vector2 OffsetForFrameAnchor(Vector2 parentSize)
        {
            switch (Config.FrameAnchor)
            {
                case LabelTextAnchor.Center: return Vector2.Zero;
                case LabelTextAnchor.Left: return new Vector2(-parentSize.X / 2f, 0);
                case LabelTextAnchor.Right: return new Vector2(parentSize.X / 2f, 0);
                case LabelTextAnchor.Top: return new Vector2(0, -parentSize.Y / 2f);
                case LabelTextAnchor.TopLeft: return -parentSize / 2f;
                case LabelTextAnchor.TopRight: return new Vector2(parentSize.X / 2f, -parentSize.Y / 2f);
                case LabelTextAnchor.Bottom: return new Vector2(0, parentSize.Y / 2f);
                case LabelTextAnchor.BottomLeft: return new Vector2(-parentSize.X / 2f, parentSize.Y / 2f);
                case LabelTextAnchor.BottomRight: return parentSize / 2f;
            }

            return Vector2.Zero;
        }
    }
}
