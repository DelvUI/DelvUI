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

        public LabelHud(string id, LabelConfig config) : base(id, config)
        {

        }

        public override void Draw(Vector2 origin)
        {
            Draw(origin);
        }

        public void Draw(Vector2 origin, Vector2? parentSize = null, GameObject? actor = null)
        {
            if (!Config.Enabled || Config.GetText() == null)
            {
                return;
            }

            var text = actor != null ? TextTags.GenerateFormattedTextFromTags(actor, Config.GetText()) : Config.GetText();
            var size = parentSize ?? Vector2.Zero;

            DrawLabel(text, origin, size, actor);
        }

        private void DrawLabel(string text, Vector2 parentPos, Vector2 parentSize, GameObject? actor = null)
        {
            var fontPushed = FontsManager.Instance.PushFont(Config.FontID);

            var textSize = ImGui.CalcTextSize(text);
            var textPos = Utils.GetAnchoredPosition(Utils.GetAnchoredPosition(parentPos + Config.Position, -parentSize, Config.FrameAnchor), textSize, Config.TextAnchor);
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
            if (!Config.UseJobColor)
            {
                return Config.Color;
            }

            if (Config.UseJobColor && actor is not null)
            {
                return GlobalColors.Instance.NPCFriendlyColor;
            }

            return Utils.ColorForActor(actor);
        }
    }
}