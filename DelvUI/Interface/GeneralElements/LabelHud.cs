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

        private void DrawLabel(string text, Vector2 parentPos, Vector2 parentSize)
        {
            var textSize = ImGui.CalcTextSize(text);
            var textPos = Utils.GetAnchoredPosition(Utils.GetAnchoredPosition(parentPos + Config.Position, -parentSize, Config.FrameAnchor), textSize, Config.TextAnchor);
            var drawList = ImGui.GetWindowDrawList();

            if (Config.ShowOutline)
            {
                DrawHelper.DrawOutlinedText(text, textPos, Config.Color.Base, Config.OutlineColor.Base, drawList);
            }
            else
            {
                drawList.AddText(textPos, Config.Color.Base, text);
            }
        }
    }
}
