using DelvUI.Config;
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
            DrawHelper.DrawIcon<Status>(statusEffectData.Data, position, config.Size, false, drawList);

            // border
            var borderConfig = GetBorderConfig(config, statusEffectData);
            if (borderConfig != null)
            {
                drawList.AddRect(position, position + config.Size, borderConfig.Color.Base, 0, ImDrawFlags.None, borderConfig.Thickness);
            }

            // duration
            if (config.ShowDurationText && !statusEffectData.Data.IsPermanent && !statusEffectData.Data.IsFcBuff)
            {
                var duration = Math.Round(Math.Abs(statusEffectData.StatusEffect.Duration));
                var text = Utils.DurationToString(duration);
                var textSize = ImGui.CalcTextSize(text);

                DrawHelper.DrawOutlinedText(
                    text,
                    position + new Vector2(config.Size.X / 2f - textSize.X / 2f, config.Size.Y / 2f - textSize.Y / 2f),
                    drawList,
                    2
                );
            }

            // stacks
            if (config.ShowStacksText && statusEffectData.Data.MaxStacks > 0 && statusEffectData.StatusEffect.StackCount > 0 && !statusEffectData.Data.IsFcBuff)
            {
                var text = $"{statusEffectData.StatusEffect.StackCount}";
                var textSize = ImGui.CalcTextSize(text);

                DrawHelper.DrawOutlinedText(
                    text,
                    position + new Vector2(config.Size.X * 0.9f - textSize.X / 2f, config.Size.X * 0.2f - textSize.Y / 2f),
                    0xFF000000,
                    0xFFFFFFFF,
                    drawList,
                    2
                );
            }
        }

        public static StatusEffectIconBorderConfig GetBorderConfig(StatusEffectIconConfig config, StatusEffectData statusEffectData)
        {
            StatusEffectIconBorderConfig borderConfig = null;

            if (config.OwnedBorderConfig.Enabled && statusEffectData.StatusEffect.OwnerId == Plugin.ClientState.LocalPlayer?.ActorId)
            {
                borderConfig = config.OwnedBorderConfig;
            }
            else if (config.DispellableBorderConfig.Enabled && statusEffectData.Data.CanDispel)
            {
                borderConfig = config.DispellableBorderConfig;
            }
            else if (config.BorderConfig.Enabled)
            {
                borderConfig = config.BorderConfig;
            }

            return borderConfig;
        }
    }
}
