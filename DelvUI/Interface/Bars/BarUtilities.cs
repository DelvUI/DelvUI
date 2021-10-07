﻿using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.Bars
{
    public class BarUtilities
    {
        public static BarHud GetProgressBar(string id, ProgressBarConfig config, float current, float max, float min = 0f, GameObject? actor = null, PluginConfigColor? fillColor = null)
        {
            return GetProgressBar(id, config, config.ThresholdConfig, new LabelConfig[] { config.Label }, current, max, min, actor, fillColor);
        }

        public static BarHud GetProgressBar(
            string id,
            BarConfig config,
            ThresholdConfig? thresholdConfig,
            LabelConfig[] labelConfigs,
            float current,
            float max,
            float min = 0f,
            GameObject? actor = null,
            PluginConfigColor? fillColor = null,
            BarGlowConfig? glowConfig = null
        )
        {
            BarHud bar = new BarHud(id, config, actor, glowConfig);

            PluginConfigColor color = fillColor ?? config.FillColor;
            if (thresholdConfig != null)
            {
                color = thresholdConfig.ChangeColor && thresholdConfig.IsActive(current) ? thresholdConfig.Color : color;
            }

            Rect foreground = GetFillRect(config.Position, config.Size, config.FillDirection, color, current, max, min);
            bar.AddForegrounds(foreground);
            bar.AddLabels(labelConfigs);

            AddThresholdMarker(bar, config, thresholdConfig, max, min);

            return bar;
        }

        public static BarHud? GetProcBar(
            string id,
            ProgressBarConfig config,
            PlayerCharacter player,
            uint statusId,
            float maxDuration)
        {
            float duration = Math.Abs(player.StatusList.FirstOrDefault(o => o.StatusId == statusId)?.RemainingTime ?? 0);

            if (duration == 0 && config.HideWhenInactive)
            {
                return null;
            }

            config.Label.SetText($"{(int)duration,0}");
            return GetProgressBar(id, config, duration, maxDuration, 0);
        }

        public static BarHud? GetDoTBar(
            string id,
            ProgressBarConfig config,
            PlayerCharacter player,
            GameObject? target,
            uint statusId,
            float maxDuration)
        {
            return GetDoTBar(id, config, player, target, new List<uint> { statusId }, new List<float> { maxDuration });
        }

        public static BarHud? GetDoTBar(
            string id,
            ProgressBarConfig config,
            PlayerCharacter player,
            GameObject? target,
            List<uint> statusIDs,
            List<float> maxDurations)
        {
            if (statusIDs.Count == 0 || maxDurations.Count == 0) { return null; }

            Status? status = null;

            if (target != null && target is BattleChara targetChara)
            {
                status = targetChara.StatusList.FirstOrDefault(o => o.SourceID == player.ObjectId && statusIDs.Contains(o.StatusId));
            }

            if (status == null && config.HideWhenInactive)
            {
                return null;
            }

            int index = status != null ? statusIDs.IndexOf(status.StatusId) : 0;
            float duration = Math.Abs(status?.RemainingTime ?? 0);
            float maxDuration = maxDurations[index];

            config.Label.SetText($"{(int)duration,0}");
            return GetProgressBar(id, config, duration, maxDuration, 0);
        }

        private static void AddThresholdMarker(BarHud bar, BarConfig config, ThresholdConfig? thresholdConfig, float max, float min)
        {
            if (thresholdConfig == null || !thresholdConfig.Enabled || !thresholdConfig.ShowMarker)
            {
                return;
            }

            float thresholdPercent = Math.Clamp(thresholdConfig.Value / (max - min), 0f, 1f);
            Vector2 offset = GetFillDirectionOffset(
                new Vector2(config.Size.X * thresholdPercent, config.Size.Y * thresholdPercent),
                config.FillDirection
            );

            Vector2 markerSize = config.FillDirection.IsHorizontal() ?
                new Vector2(thresholdConfig.MarkerSize, config.Size.Y) :
                new Vector2(config.Size.X, thresholdConfig.MarkerSize);

            Vector2 markerPos = config.FillDirection.IsInverted() ?
                config.Position + GetFillDirectionOffset(config.Size, config.FillDirection) - offset :
                config.Position + offset;

            Vector2 anchoredPos = Utils.GetAnchoredPosition(markerPos, markerSize, config.FillDirection.IsHorizontal() ? DrawAnchor.Top : DrawAnchor.Left);
            Rect marker = new Rect(anchoredPos, markerSize, thresholdConfig.MarkerColor);
            bar.AddForegrounds(marker);
        }

        // Tuple is <foregroundColor, percent fill, labels>
        public static BarHud[] GetChunkedBars(
            string id,
            ChunkedBarConfig config,
            GameObject? actor,
            params Tuple<PluginConfigColor, float, LabelConfig?>[] chunks)
        {
            return GetChunkedBars(id, config, actor, null, chunks);
        }

        public static BarHud[] GetChunkedBars(
            string id,
            ChunkedBarConfig config,
            GameObject? actor,
            BarGlowConfig? glowConfig = null,
            params Tuple<PluginConfigColor, float, LabelConfig?>[] chunks)
        {
            BarHud[] bars = new BarHud[chunks.Length];
            var pos = Utils.GetAnchoredPosition(config.Position, config.Size, config.Anchor);

            for (int i = 0; i < chunks.Length; i++)
            {
                Vector2 chunkPos, chunkSize;
                if (config.FillDirection.IsHorizontal())
                {
                    chunkSize = new Vector2((config.Size.X - config.Padding * (chunks.Length - 1)) / chunks.Length, config.Size.Y);
                    chunkPos = pos + new Vector2((chunkSize.X + config.Padding) * i, 0);
                }
                else
                {
                    chunkSize = new Vector2(config.Size.X, (config.Size.Y - config.Padding * (chunks.Length - 1)) / chunks.Length);
                    chunkPos = pos + new Vector2(0, (chunkSize.Y + config.Padding) * i);
                }

                Rect background = new Rect(chunkPos, chunkSize, config.BackgroundColor);
                Rect foreground = GetFillRect(chunkPos, chunkSize, config.FillDirection, chunks[i].Item1, chunks[i].Item2, 1f, 0f);
                BarGlowConfig? glow = chunks[i].Item2 >= 1 ? glowConfig : null;

                bars[i] = new BarHud(id + i, config.DrawBorder, actor: actor, glowColor: glow?.Color, glowSize: glow?.Size)
                    .SetBackground(background)
                    .AddForegrounds(foreground);

                var label = chunks[i].Item3;
                if (label is not null)
                {
                    bars[i].AddLabels(label);
                }
            }

            return bars;
        }

        public static BarHud[] GetChunkedProgressBars(
            string id,
            ChunkedBarConfig config,
            int chunks,
            float current,
            float max,
            float min = 0f,
            GameObject? actor = null,
            LabelConfig? label = null,
            BarGlowConfig? glowConfig = null)
        {
            float chunkRange = (max - min) / chunks;

            var barChunks = new Tuple<PluginConfigColor, float, LabelConfig?>[chunks];
            for (int i = 0; i < chunks; i++)
            {
                int barIndex = config.FillDirection.IsInverted() ? chunks - i - 1 : i;
                float chunkMin = min + chunkRange * i;
                float chunkMax = min + chunkRange * (i + 1);
                float chunkPercent = Math.Clamp((current - chunkMin) / (chunkMax - chunkMin), 0f, 1f);
                PluginConfigColor chunkColor = config.UsePartialFillColor && config.PartialFillColor is not null && current < chunkMax ? config.PartialFillColor : config.FillColor;
                barChunks[barIndex] = new Tuple<PluginConfigColor, float, LabelConfig?>(chunkColor, chunkPercent, chunkPercent < 1f ? label : null);
            }

            return GetChunkedBars(id, config, actor, glowConfig, barChunks);
        }

        public static BarHud GetBar(
            string id,
            BarConfig Config,
            float current,
            float max,
            float min = 0f,
            GameObject? actor = null,
            PluginConfigColor? fillColor = null,
            BarGlowConfig? glowConfig = null,
            params LabelConfig[] labels)
        {
            Rect foreground = GetFillRect(Config.Position, Config.Size, Config.FillDirection, fillColor ?? Config.FillColor, current, max, min);
            return new BarHud(id, Config, actor, glowConfig).AddForegrounds(foreground).AddLabels(labels);
        }

        /// <summary>
        /// Gets the horizonal or vertical offset depending on the fill direction.
        /// </summary>
        public static Vector2 GetFillDirectionOffset(Vector2 size, BarDirection fillDirection)
        {
            return fillDirection.IsHorizontal() ? new(size.X, 0) : new(0, size.Y);
        }

        public static Rect GetFillRect(Vector2 pos, Vector2 size, BarDirection fillDirection, PluginConfigColor color, float current, float max, float min = 0f)
        {
            float fillPercent = Math.Clamp((current - min) / (max - min), 0f, 1f);

            Vector2 fillPos = Vector2.Zero;
            Vector2 fillSize = fillDirection.IsHorizontal() ? new(size.X * fillPercent, size.Y) : new(size.X, size.Y * fillPercent);
            if (fillDirection == BarDirection.Left)
            {
                fillPos = Utils.GetAnchoredPosition(new(size.X, 0), fillSize, DrawAnchor.TopRight);
            }
            else if (fillDirection == BarDirection.Up)
            {
                fillPos = Utils.GetAnchoredPosition(new(0, size.Y), fillSize, DrawAnchor.BottomLeft);
            }

            return new Rect(pos + fillPos, fillSize, color);
        }
    }
}