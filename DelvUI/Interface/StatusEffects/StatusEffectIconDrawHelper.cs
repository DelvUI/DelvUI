using DelvUI.Helpers;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Numerics;

namespace DelvUI.Interface.StatusEffects
{
    internal static class StatusEffectIconDrawHelper
    {
        public static void DrawStatusEffectIcon(ImDrawListPtr drawList, Vector2 position, StatusEffectData statusEffectData, StatusEffectIconConfig config)
        {
            // icon
            DrawHelper.DrawIcon<Status>(statusEffectData.Data, config.Size, position, config.ShowBorder);

            // border
            if (config.ShowDispellableBorder && statusEffectData.Data.CanDispel)
            {
                drawList.AddRect(position, position + config.Size, config.DispellableBorderColor.Base, 0, ImDrawFlags.None, config.DispellableBorderThickness);
            }
            else if (config.ShowBorder)
            {
                drawList.AddRect(position, position + config.Size, config.BorderColor.Base, 0, ImDrawFlags.None, config.BorderThickness);
            }

            // duration
            if (config.ShowDurationText && !statusEffectData.Data.IsPermanent && !statusEffectData.Data.IsFcBuff)
            {
                var duration = Math.Round(Math.Abs(statusEffectData.StatusEffect.Duration));
                var text = Utils.DurationToString(duration);
                Vector2 textSize = ImGui.CalcTextSize(text);
                DrawHelper.DrawOutlinedText(text, position + new Vector2(config.Size.X / 2f - textSize.X / 2f, config.Size.Y / 2f - textSize.Y / 2f));
            }

            // stacks
            if (config.ShowStacksText && statusEffectData.Data.MaxStacks > 0 && statusEffectData.StatusEffect.StackCount > 0 && !statusEffectData.Data.IsFcBuff)
            {
                var text = $"{statusEffectData.StatusEffect.StackCount}";
                Vector2 textSize = ImGui.CalcTextSize(text);

                DrawHelper.DrawOutlinedText(
                    text,
                    position + new Vector2(config.Size.X * 0.9f - textSize.X / 2f, config.Size.X * 0.2f - textSize.Y / 2f),
                    Vector4.UnitW,
                    Vector4.One
                );
            }
        }
    }
}
