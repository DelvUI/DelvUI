using Dalamud.Game.ClientState.Objects.SubKinds;
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
        public static BarHud GetProgressBar(ProgressBarConfig config, float current, float max, float min = 0f, GameObject? actor = null, PluginConfigColor? fillColor = null, BarGlowConfig? barGlowConfig = null)
        {
            return GetProgressBar(config, config.ThresholdConfig, new LabelConfig[] { config.Label }, current, max, min, actor, fillColor, barGlowConfig);
        }

        public static BarHud GetProgressBar(
            BarConfig config,
            ThresholdConfig? thresholdConfig,
            LabelConfig[]? labelConfigs,
            float current,
            float max,
            float min = 0f,
            GameObject? actor = null,
            PluginConfigColor? fillColor = null,
            BarGlowConfig? glowConfig = null,
            PluginConfigColor? backgroundColor = null
        )
        {
            BarHud bar = new(config, actor, glowConfig, current, max);

            PluginConfigColor color = fillColor ?? config.FillColor;
            if (thresholdConfig != null)
            {
                color = thresholdConfig.ChangeColor && thresholdConfig.IsActive(current) ? thresholdConfig.Color : color;
            }

            Rect foreground = GetFillRect(config.Position, config.Size, config.FillDirection, color, current, max, min);
            bar.AddForegrounds(foreground);
            bar.AddLabels(labelConfigs);

            if (backgroundColor != null)
            {
                Rect bg = new Rect(config.Position, config.Size, backgroundColor);
                bar.SetBackground(bg);
            }

            AddThresholdMarker(bar, config, thresholdConfig, max, min);

            return bar;
        }

        public static BarHud? GetProcBar(
            ProgressBarConfig config,
            PlayerCharacter player,
            uint statusId,
            float maxDuration,
            bool trackDuration = true)
        {
            float duration = Math.Abs(player.StatusList.FirstOrDefault(o => o.StatusId == statusId)?.RemainingTime ?? 0);

            if (duration == 0 && config.HideWhenInactive)
            {
                return null;
            }

            if (trackDuration)
            {
                config.Label.SetValue(duration);
                return GetProgressBar(config, duration, maxDuration, 0, player);
            }

            config.Label.SetText("");
            return GetBar(config, duration <= 0 ? 0 : 1, 1, 0);
        }

        public static BarHud? GetDoTBar(
            ProgressBarConfig config,
            PlayerCharacter player,
            GameObject? target,
            uint statusId,
            float maxDuration)
        {
            return GetDoTBar(config, player, target, new List<uint> { statusId }, new List<float> { maxDuration });
        }

        public static BarHud? GetDoTBar(
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

            config.Label.SetValue(duration);
            return GetProgressBar(config, duration, maxDuration, 0, player);
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
            Rect marker = new(anchoredPos, markerSize, thresholdConfig.MarkerColor);
            bar.AddForegrounds(marker);
        }

        // Tuple is <foregroundColor, percent fill, labels>
        public static BarHud[] GetChunkedBars(
            ChunkedBarConfig config,
            Tuple<PluginConfigColor, float, LabelConfig?>[] chunks,
            GameObject? actor,
            BarGlowConfig glowConfig)
        {
            List<bool> chunksToGlowList = new();
            for (int i = 0; i < chunks.Length; i++)
            {
                chunksToGlowList.Add(chunks[i].Item2 >= 1f);
            }

            return GetChunkedBars(config, chunks, actor, glowConfig, chunksToGlowList.ToArray());
        }

        public static BarHud[] GetChunkedBars(
            ChunkedBarConfig config,
            Tuple<PluginConfigColor, float, LabelConfig?>[] chunks,
            GameObject? actor,
            BarGlowConfig? glowConfig = null,
            bool[]? chunksToGlow = null)
        {
            BarHud[] bars = new BarHud[chunks.Length];
            Vector2 pos = Utils.GetAnchoredPosition(config.Position, config.Size, config.Anchor);

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

                Rect background = new(chunkPos, chunkSize, config.BackgroundColor);
                Rect foreground = GetFillRect(chunkPos, chunkSize, config.FillDirection, chunks[i].Item1, chunks[i].Item2, 1f, 0f);
                BarGlowConfig? glow = chunksToGlow?[i] == true ? glowConfig : null;

                bars[i] = new BarHud(config.ID + i, config.DrawBorder, config.BorderColor, config.BorderThickness, actor: actor, glowColor: glow?.Color, glowSize: glow?.Size)
                          .SetBackground(background)
                          .AddForegrounds(foreground);

                LabelConfig? label = chunks[i].Item3;
                if (label is not null)
                {
                    bars[i].AddLabels(label);
                }
            }

            return bars;
        }

        public static BarHud[] GetChunkedBars(
            ChunkedBarConfig config,
            int chunks,
            float current,
            float max,
            float min = 0f,
            GameObject? actor = null,
            LabelConfig?[]? labels = null,
            PluginConfigColor? fillColor = null,
            PluginConfigColor? partialFillColor = null,
            BarGlowConfig? glowConfig = null,
            bool[]? chunksToGlow = null)
        {
            float chunkRange = (max - min) / chunks;

            var barChunks = new Tuple<PluginConfigColor, float, LabelConfig?>[chunks];
            for (int i = 0; i < chunks; i++)
            {
                int barIndex = config.FillDirection.IsInverted() ? chunks - i - 1 : i;
                float chunkMin = min + chunkRange * i;
                float chunkMax = min + chunkRange * (i + 1);
                float chunkPercent = Math.Clamp((current - chunkMin) / (chunkMax - chunkMin), 0f, 1f);

                PluginConfigColor chunkColor = partialFillColor != null && current < chunkMax ? partialFillColor : fillColor ?? config.FillColor;
                barChunks[barIndex] = new Tuple<PluginConfigColor, float, LabelConfig?>(chunkColor, chunkPercent, labels?[i]);
            }

            if (glowConfig != null && chunksToGlow == null)
            {
                return GetChunkedBars(config, barChunks, actor, glowConfig);
            }

            return GetChunkedBars(config, barChunks, actor, glowConfig, chunksToGlow);
        }

        public static BarHud[] GetChunkedProgressBars(
            ChunkedProgressBarConfig config,
            int chunks,
            float current,
            float max,
            float min = 0f,
            GameObject? actor = null,
            BarGlowConfig? glowConfig = null,
            PluginConfigColor? fillColor = null,
            int thresholdChunk = 1,
            bool[]? chunksToGlow = null)
        {
            var color = fillColor ?? config.FillColor;

            if (config.UseChunks)
            {
                NumericLabelConfig?[] labels = new NumericLabelConfig?[chunks];
                for (int i = 0; i < chunks; i++)
                {
                    float chunkRange = (max - min) / chunks;
                    float chunkMin = min + chunkRange * i;
                    float chunkMax = min + chunkRange * (i + 1);
                    float chunkPercent = Math.Clamp((current - chunkMin) / (chunkMax - chunkMin), 0f, 1f);

                    NumericLabelConfig? label = config.Label;
                    switch (config.LabelMode)
                    {
                        case LabelMode.AllChunks:
                            label = config.Label.Clone();
                            label.SetValue(Math.Clamp(current - chunkMin, 0, chunkRange));
                            break;
                        case LabelMode.ActiveChunk:
                            label = chunkPercent < 1f && chunkPercent > 0f ? config.Label.Clone() : null;
                            break;
                    };

                    labels[i] = label;
                }

                var partialColor = config.UsePartialFillColor ? config.PartialFillColor : null;
                return GetChunkedBars(config, chunks, current, max, min, actor, labels, color, partialColor, glowConfig, chunksToGlow);
            }

            var threshold = GetThresholdConfigForChunk(config, thresholdChunk, chunks, min, max);
            BarHud bar = GetProgressBar(config, threshold, new LabelConfig[] { config.Label }, current, max, min, actor, color, glowConfig);
            return new BarHud[] { bar };
        }

        public static Rect[] GetShieldForeground(
            ShieldConfig shieldConfig,
            Vector2 pos,
            Vector2 size,
            Vector2 healthFillSize,
            BarDirection fillDirection,
            float shieldPercent,
            float currentHp,
            float maxHp)
        {
            float shieldValue = shieldPercent * maxHp;
            float overshield = shieldConfig.FillHealthFirst ? Math.Max(shieldValue + currentHp - maxHp, 0f) : shieldValue;
            float shieldSize = shieldConfig.Height;

            if (!shieldConfig.HeightInPixels)
            {
                shieldSize = (fillDirection.IsHorizontal() ? size.Y : size.X) * shieldConfig.Height / 100f;
            }

            var overshieldSize = fillDirection.IsHorizontal()
                ? new Vector2(size.X, Math.Min(shieldSize, size.Y))
                : new Vector2(Math.Min(shieldSize, size.X), size.Y);

            Rect overshieldFill = GetFillRect(pos, overshieldSize, fillDirection, shieldConfig.Color, overshield, maxHp);

            if (shieldConfig.FillHealthFirst && currentHp < maxHp)
            {
                var shieldPos = fillDirection.IsInverted() ? pos : pos + BarUtilities.GetFillDirectionOffset(healthFillSize, fillDirection);
                var shieldFillSize = size - GetFillDirectionOffset(healthFillSize, fillDirection);
                var healthFillShieldSize = fillDirection.IsHorizontal()
                    ? new Vector2(shieldFillSize.X, Math.Min(shieldSize, size.Y))
                    : new Vector2(Math.Min(shieldSize, size.X), shieldFillSize.Y);

                Rect shieldFill = GetFillRect(shieldPos, healthFillShieldSize, fillDirection, shieldConfig.Color, shieldValue - overshield, maxHp - currentHp, 0f);
                return new[] { overshieldFill, shieldFill };
            }

            return new[] { overshieldFill };
        }

        public static BarHud GetBar(
            BarConfig Config,
            float current,
            float max,
            float min = 0f,
            GameObject? actor = null,
            PluginConfigColor? fillColor = null,
            BarGlowConfig? glowConfig = null,
            LabelConfig[]? labels = null)
        {
            Rect foreground = GetFillRect(Config.Position, Config.Size, Config.FillDirection, fillColor ?? Config.FillColor, current, max, min);

            BarHud bar = new BarHud(Config, actor, glowConfig);
            bar.AddForegrounds(foreground);
            bar.AddLabels(labels);

            return bar;
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
            float fillPercent = max == 0 ? 1f : Math.Clamp((current - min) / (max - min), 0f, 1f);

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

        public static ThresholdConfig GetThresholdConfigForChunk(ChunkedProgressBarConfig config, int chunk, int chunks, float min, float max) =>
            new ThresholdConfig
            {
                ThresholdType = ThresholdType.Below,
                Color = config.PartialFillColor,
                Enabled = config.UsePartialFillColor,
                Value = (max - min) / chunks * chunk,
                ChangeColor = true,
                ShowMarker = false
            };
    }
}
