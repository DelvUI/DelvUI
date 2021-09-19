using Dalamud.Game.ClientState.Actors.Types;
using DelvUI.Config;
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

            DrawLabel(text, origin, size, actor);
        }

        private void DrawLabel(string text, Vector2 parentOrigin, Vector2 parentSize, Actor actor = null)
        {
            var textSize = ImGui.CalcTextSize(text);
            var offset = OffsetForFrameAnchor(parentSize) + OffsetForTextAnchor(textSize);
            var drawList = ImGui.GetWindowDrawList();
            var color = Color(actor);

            if (Config.ShowOutline)
            {
                DrawHelper.DrawOutlinedText(text, parentOrigin + Config.Position + offset, color.Base, Config.OutlineColor.Base, drawList);
            }
            else
            {
                drawList.AddText(parentOrigin + Config.Position + offset, color.Base, text);
            }
        }

        public virtual PluginConfigColor Color(Actor actor = null)
        {
            if (!Config.UseJobColor)
            {
                return Config.Color;
            }
            else if (Config.UseJobColor && actor is not Chara)
            {
                return GlobalColors.Instance.NPCFriendlyColor;
            }

            return Utils.ColorForActor((Chara)actor);
        }

        private Vector2 OffsetForTextAnchor(Vector2 textSize)
        {
            switch (Config.TextAnchor)
            {
                case HudElementAnchor.Center: return -textSize / 2f;
                case HudElementAnchor.Left: return new Vector2(0, -textSize.Y / 2f);
                case HudElementAnchor.Right: return new Vector2(-textSize.X, -textSize.Y / 2f);
                case HudElementAnchor.Top: return new Vector2(-textSize.X / 2f, 0);
                case HudElementAnchor.TopLeft: return Vector2.Zero;
                case HudElementAnchor.TopRight: return new Vector2(-textSize.X, 0);
                case HudElementAnchor.Bottom: return new Vector2(-textSize.X / 2f, -textSize.Y);
                case HudElementAnchor.BottomLeft: return new Vector2(0, -textSize.Y);
                case HudElementAnchor.BottomRight: return new Vector2(-textSize.X, -textSize.Y);
            }

            return Vector2.Zero;
        }

        private Vector2 OffsetForFrameAnchor(Vector2 parentSize)
        {
            switch (Config.FrameAnchor)
            {
                case HudElementAnchor.Center: return Vector2.Zero;
                case HudElementAnchor.Left: return new Vector2(-parentSize.X / 2f, 0);
                case HudElementAnchor.Right: return new Vector2(parentSize.X / 2f, 0);
                case HudElementAnchor.Top: return new Vector2(0, -parentSize.Y / 2f);
                case HudElementAnchor.TopLeft: return -parentSize / 2f;
                case HudElementAnchor.TopRight: return new Vector2(parentSize.X / 2f, -parentSize.Y / 2f);
                case HudElementAnchor.Bottom: return new Vector2(0, parentSize.Y / 2f);
                case HudElementAnchor.BottomLeft: return new Vector2(-parentSize.X / 2f, parentSize.Y / 2f);
                case HudElementAnchor.BottomRight: return parentSize / 2f;
            }

            return Vector2.Zero;
        }
    }
}