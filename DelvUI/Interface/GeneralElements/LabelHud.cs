using DelvUI.Helpers;
using ImGuiNET;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;

namespace DelvUI.Interface.GeneralElements
{
    public class LabelHud : HudElement
    {
        private new LabelConfig Config => (LabelConfig)base.Config;

        public LabelHud(string id, LabelConfig config) : base(id, config)
        {

        }

        public override void Draw(Vector2 origin)
        {
            Draw(origin);
        }

        public void Draw(Vector2 origin, Vector2? parentSize = null, GameObject? actor = null, string? actorName = null)
        {
            if (!Config.Enabled || Config.GetText() == null)
            {
                return;
            }

            string? text = actor == null && actorName == null ?
                Config.GetText() :
                TextTags.GenerateFormattedTextFromTags(actor, Config.GetText(), actorName);

            Vector2 size = parentSize ?? Vector2.Zero;

            DrawLabel(text, origin, size, actor);
        }

        private void DrawLabel(string text, Vector2 parentPos, Vector2 parentSize, GameObject? actor = null)
        {
            bool fontPushed = FontsManager.Instance.PushFont(Config.FontID);

            Vector2 textSize = ImGui.CalcTextSize(text);
            Vector2 textPos = Utils.GetAnchoredPosition(Utils.GetAnchoredPosition(parentPos + Config.Position, -parentSize, Config.FrameAnchor), textSize, Config.TextAnchor);
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            PluginConfigColor? color = Color(actor);

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
