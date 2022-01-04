using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Helpers;
using ImGuiNET;

namespace DelvUI.Interface.GeneralElements
{
    public class LabelHud : HudElement
    {
        private LabelConfig Config => (LabelConfig)_config;

        public LabelHud(LabelConfig config) : base(config)
        {
        }

        protected override void CreateDrawActions(Vector2 origin)
        {
            // unused
        }

        public override void Draw(Vector2 origin)
        {
            Draw(origin);
        }

        public void Draw(Vector2 origin, Vector2? parentSize = null,
            GameObject? actor = null, string? actorName = null, uint? actorCurrentHp = null, uint? actorMaxHp = null)
        {
            if (!Config.Enabled || Config.GetText() == null)
            {
                return;
            }

            string? text = actor == null && actorName == null && actorCurrentHp == null && actorMaxHp == null ?
                Config.GetText() :
                TextTagsHelper.FormattedText(Config.GetText(), actor, actorName, actorCurrentHp, actorMaxHp);

            DrawLabel(text, origin, parentSize ?? Vector2.Zero, actor);
        }

        private void DrawLabel(string text, Vector2 parentPos, Vector2 parentSize, GameObject? actor = null)
        {
            using (FontsManager.Instance.PushFont(Config.FontID))
            {
                Vector2 size = ImGui.CalcTextSize(text);
                Vector2 pos = Utils.GetAnchoredPosition(Utils.GetAnchoredPosition(parentPos + Config.Position, -parentSize, Config.FrameAnchor), size, Config.TextAnchor);

                DrawHelper.DrawInWindow(ID, pos, size, false, true, (drawList) =>
                {
                    PluginConfigColor? color = Color(actor);

                    if (Config.ShowShadow)
                    {
                        DrawHelper.DrawShadowText(text, pos, color.Base, Config.ShadowColor.Base, drawList, Config.ShadowOffset);
                    }

                    if (Config.ShowOutline)
                    {
                        DrawHelper.DrawOutlinedText(text, pos, color.Base, Config.OutlineColor.Base, drawList);
                    }

                    if (!Config.ShowOutline && !Config.ShowShadow)
                    {
                        drawList.AddText(pos, color.Base, text);
                    }
                });
            }
        }

        public virtual PluginConfigColor Color(GameObject? actor = null)
        {
            switch (Config.UseJobColor)
            {
                case true when (actor is Character || actor is BattleNpc battleNpc && battleNpc.ClassJob.Id > 0):
                    return Utils.ColorForActor(actor);
                case true when actor is not Character:
                    return GlobalColors.Instance.NPCFriendlyColor;
            }

            switch (Config.UseRoleColor)
            {
                case true when (actor is Character || actor is BattleNpc battleNpc && battleNpc.ClassJob.Id > 0):
                    {
                        Character? character = actor as Character;
                        return character != null && character.ClassJob.Id > 0 ?
                            GlobalColors.Instance.SafeRoleColorForJobId(character.ClassJob.Id) :
                            Utils.ColorForActor(character);
                    }
                case true when actor is not Character:
                    return GlobalColors.Instance.NPCFriendlyColor;
                default:
                    return Config.Color;
            }
        }
    }
}