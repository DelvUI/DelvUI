using System;
using System.Numerics;
using ImGuiNET;
using DelvUI.Helpers;
using Lumina.Excel.GeneratedSheets;


namespace DelvUI.Interface.StatusEffects
{
    static class StatusEffectIconDrawHelper
    {
        public static void DrawStatusEffectIcon(ImDrawListPtr drawList, Vector2 position, StatusEffectData statusEffectData, StatusEffectIconConfig config)
        {
            // icon
            DrawHelper.DrawIcon<Status>(statusEffectData.data, config.Size, position, config.ShowBorder);

            // border
            if (config.ShowDispellableBorder && statusEffectData.data.CanDispel)
            {
                drawList.AddRect(position, position + config.Size, config.DispellableBorderColor.Base, 0, ImDrawFlags.None, config.DispellableBorderThickness);
            }
            else if (config.ShowBorder)
            {
                drawList.AddRect(position, position + config.Size, config.BorderColor.Base, 0, ImDrawFlags.None, config.BorderThickness);
            }

            // duration
            if (config.ShowDurationText && !statusEffectData.data.IsPermanent && !statusEffectData.data.IsFcBuff)
            {
                var duration = Math.Round(Math.Abs(statusEffectData.status.RemainingTime));
                var text = Utils.DurationToString(duration);
                var textSize = ImGui.CalcTextSize(text);
                DrawHelper.DrawOutlinedText(text, position + new Vector2(config.Size.X / 2f - textSize.X / 2f, config.Size.Y / 2f - textSize.Y / 2f));
            }

            // stacks
            if (config.ShowStacksText && statusEffectData.data.MaxStacks > 0 && statusEffectData.status.StackCount > 0 && !statusEffectData.data.IsFcBuff)
            {
                var text = $"{statusEffectData.status.StackCount}";
                var textSize = ImGui.CalcTextSize(text);
                DrawHelper.DrawOutlinedText(text, position + new Vector2(config.Size.X * 0.9f - textSize.X / 2f, config.Size.X * 0.2f - textSize.Y / 2f), Vector4.UnitW, Vector4.One);
            }
        }
    }
}
