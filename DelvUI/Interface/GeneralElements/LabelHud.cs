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

            var size = ImGui.CalcTextSize(Config.GetText());
            var offset = OffsetForSize(size);
            var drawList = ImGui.GetWindowDrawList();

            if (Config.ShowOutline)
            {
                DrawHelper.DrawOutlinedText(Config.GetText(), origin + Config.Position + offset, Config.Color.Base, Config.OutlineColor.Base, drawList);
            }
            else
            {
                drawList.AddText(origin + Config.Position + offset, Config.Color.Base, Config.GetText());
            }
        }

        public void DrawWithActor(Vector2 origin, Actor actor)
        {
            if (!Config.Enabled || Config.GetText() == null)
            {
                return;
            }

            var text = TextTags.GenerateFormattedTextFromTags(actor, Config.GetText());
            var size = ImGui.CalcTextSize(text);
            var offset = OffsetForSize(size);
            var drawList = ImGui.GetWindowDrawList();

            if (Config.ShowOutline)
            {
                DrawHelper.DrawOutlinedText(text, origin + Config.Position + offset, Config.Color.Base, Config.OutlineColor.Base, drawList);
            }
            else
            {
                drawList.AddText(origin + Config.Position + offset, Config.Color.Base, Config.GetText());
            }
        }

        private Vector2 OffsetForSize(Vector2 size)
        {
            switch (Config.Anchor)
            {
                case LabelTextAnchor.Center: return -size / 2f;
                case LabelTextAnchor.Left: return new Vector2(0, -size.Y / 2f);
                case LabelTextAnchor.Right: return new Vector2(-size.X, -size.Y / 2f);
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
}
