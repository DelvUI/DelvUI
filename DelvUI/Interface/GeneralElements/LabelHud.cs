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

        public void Draw(Vector2 origin, Vector2? parentSize = null, GameObject? actor = null, string? actorName = null)
        {
            if (!Config.Enabled || Config.GetText() == null)
            {
                return;
            }

            var text = actor == null && actorName == null ?
                Config.GetText() :
                TextTags.GenerateFormattedTextFromTags(actor, Config.GetText(), actorName);

            DrawLabel(text, origin, parentSize ?? Vector2.Zero, actor);
        }

        private void DrawLabel(string text, Vector2 parentPos, Vector2 parentSize, GameObject? actor = null)
        {
            var fontPushed = FontsManager.Instance.PushFont(Config.FontID);

            var size = ImGui.CalcTextSize(text);
            var pos = Utils.GetAnchoredPosition(Utils.GetAnchoredPosition(parentPos + Config.Position, -parentSize, Config.FrameAnchor), size, Config.TextAnchor);

            DrawHelper.DrawInWindow(ID, pos, size, false, true, (drawList) =>
            {
                var color = Color(actor);

                if (Config.ShowOutline)
                {
                    DrawHelper.DrawOutlinedText(text, pos, color.Base, Config.OutlineColor.Base, drawList);
                }
                else
                {
                    drawList.AddText(pos, color.Base, text);
                }
            });

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