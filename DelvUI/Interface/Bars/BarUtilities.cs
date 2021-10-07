using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using System;
using System.Numerics;

namespace DelvUI.Interface.Bars
{
    public class BarUtilities
    {
        public static BarHud GetProgressBar(ProgressBarConfig config, float current, float max, float min = 0f, GameObject? actor = null)
        {
            var bar = new BarHud(config, actor);
            PluginConfigColor fillColor = config.IsThresholdActive(current) ? config.ThresholdColor : config.FillColor;
            Rect foreground = GetFillRect(config.Position, config.Size, config.FillDirection, fillColor, current, max, min);
            bar.Foreground(foreground);

            if (config.ThresholdMarker)
            {
                var thresholdPercent = Math.Clamp(config.ThresholdValue / (max - min), 0f, 1f);
                var offset = GetFillDirectionOffset(new Vector2(config.Size.X * thresholdPercent, config.Size.Y * thresholdPercent), config.FillDirection);
                var markerSize = config.FillDirection.IsHorizontal() ? new Vector2(config.ThresholdMarkerSize, config.Size.Y) : new Vector2(config.Size.X, config.ThresholdMarkerSize);
                var markerPos = config.FillDirection.IsInverted() ? config.Position + GetFillDirectionOffset(config.Size, config.FillDirection) - offset : config.Position + offset;
                var anchoredPos = Utils.GetAnchoredPosition(markerPos, markerSize, config.FillDirection.IsHorizontal() ? DrawAnchor.Top : DrawAnchor.Left);
                Rect marker = new Rect(anchoredPos, markerSize, config.ThresholdMarkerColor);
                bar.Foreground(marker);
            }

            return bar.Labels(config.Label);
        }

        // Tuple is <foregroundColor, percent fill, labels>
        public static BarHud[] GetChunkedBars(
            ChunkedBarConfig config,
            GameObject? actor,
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

                bars[i] = new BarHud(config.DrawBorder, actor: actor).Background(background).Foreground(foreground);

                var label = chunks[i].Item3;
                if (label is not null)
                {
                    bars[i].Labels(label);
                }
            }

            return bars;
        }

        public static BarHud[] GetChunkedProgressBars(
            ChunkedBarConfig config,
            int chunks,
            float current,
            float max,
            float min = 0f,
            GameObject? actor = null,
            LabelConfig? label = null)
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

            return GetChunkedBars(config, actor, barChunks);
        }

        public static BarHud GetBar(BarConfig Config, float current, float max, float min = 0f, GameObject? actor = null, params LabelConfig[] labels)
        {
            Rect foreground = GetFillRect(Config.Position, Config.Size, Config.FillDirection, Config.FillColor, current, max, min);
            return new BarHud(Config, actor).Foreground(foreground).Labels(labels);
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
