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

        private void DrawLabel(string text, Vector2 parentPos, Vector2 parentSize, Actor actor = null)
        {
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
    }
}