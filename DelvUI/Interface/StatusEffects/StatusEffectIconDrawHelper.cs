using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Numerics;

namespace DelvUI.Interface.StatusEffects
{
    internal static class StatusEffectIconDrawHelper
    {
        public static void DrawStatusEffectIcon(
            ImDrawListPtr drawList,
            Vector2 position,
            StatusEffectData statusEffectData,
            StatusEffectIconConfig config,
            LabelHud durationLabel,
            LabelHud stacksLabel)
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
            if (config.DurationLabelConfig.Enabled && !statusEffectData.Data.IsPermanent && !statusEffectData.Data.IsFcBuff)
            {
                var duration = Math.Round(Math.Abs(statusEffectData.StatusEffect.Duration));
                config.DurationLabelConfig.SetText(Utils.DurationToString(duration));

                durationLabel.Draw(position, config.Size);
            }

            // stacks
            if (config.StacksLabelConfig.Enabled && statusEffectData.Data.MaxStacks > 0 && statusEffectData.StatusEffect.StackCount > 0 && !statusEffectData.Data.IsFcBuff)
            {
                var text = $"{statusEffectData.StatusEffect.StackCount}";
                config.StacksLabelConfig.SetText(text);

                stacksLabel.Draw(position, config.Size);
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
