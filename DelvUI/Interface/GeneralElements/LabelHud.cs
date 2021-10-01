using DelvUI.Helpers;
using ImGuiNET;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;

namespace DelvUI.Interface.GeneralElements
{
    public class LabelHud : HudElement
    {
        private LabelConfig Config => (LabelConfig)_config;

        private string? _text = null;
        private Vector2 _pos;
        private Vector2 _size;

        public LabelHud(string id, LabelConfig config) : base(id, config)
        {

        }

        public override void Draw(Vector2 origin)
        {
            Draw(origin);
        }

        public (Vector2, Vector2) Precalculate(Vector2 origin, Vector2? parentSize = null, GameObject? actor = null, string? actorName = null)
        {
            _text = null;
            _pos = Vector2.Zero;
            _size = Vector2.Zero;

            if (!Config.Enabled || Config.GetText() == null)
            {
                return (_pos, _size);
            }

            var fontPushed = FontsManager.Instance.PushFont(Config.FontID);

            _text = actor == null && actorName == null ? Config.GetText() : TextTags.GenerateFormattedTextFromTags(actor, Config.GetText(), actorName);
            _size = ImGui.CalcTextSize(_text);
            _pos = Utils.GetAnchoredPosition(Utils.GetAnchoredPosition(origin + Config.Position, -parentSize ?? Vector2.Zero, Config.FrameAnchor), _size, Config.TextAnchor);

            if (fontPushed)
            {
                ImGui.PopFont();
            }

            return (_pos, _size);
        }

        public void Draw(Vector2 origin, Vector2? parentSize = null, GameObject? actor = null, string? actorName = null)
        {
            if (!Config.Enabled || Config.GetText() == null)
            {
                return;
            }

            var size = parentSize ?? Vector2.Zero;
            if (_text != null)
            {
                DrawLabel(_text, _pos, _size, origin, size, actor);
            }

            var text = actor == null && actorName == null ?
                Config.GetText() :
                TextTags.GenerateFormattedTextFromTags(actor, Config.GetText(), actorName);

            DrawLabel(text, origin, size, actor);
        }

        private void DrawLabel(string text, Vector2 parentPos, Vector2 parentSize, GameObject? actor = null)
        {
            var textSize = ImGui.CalcTextSize(text);
            var textPos = Utils.GetAnchoredPosition(Utils.GetAnchoredPosition(parentPos + Config.Position, -parentSize, Config.FrameAnchor), textSize, Config.TextAnchor);

            DrawLabel(text, textPos, textSize, parentPos, parentSize, actor);
        }

        private void DrawLabel(string text, Vector2 textPos, Vector2 textSize, Vector2 parentPos, Vector2 parentSize, GameObject? actor = null)
        {
            var fontPushed = FontsManager.Instance.PushFont(Config.FontID);
            var drawList = ImGui.GetWindowDrawList();
            var color = Color(actor);

            if (Config.ShowOutline)
            {
                DrawHelper.DrawOutlinedText(text, textPos, color.Base, Config.OutlineColor.Base, drawList);
            }
            else
            {
                drawList.AddText(textPos, color.Base, text);
            }

            if (fontPushed)
            {
                ImGui.PopFont();
            }
        }

        public virtual PluginConfigColor Color(GameObject? actor = null)
        {
            if (!Config.UseJobColor || actor == null)
            {
                return Config.Color;
            }

            if (Config.UseJobColor && actor is not Character)
            {
                return GlobalColors.Instance.NPCFriendlyColor;
            }

            return Utils.ColorForActor(actor);
        }
    }
}