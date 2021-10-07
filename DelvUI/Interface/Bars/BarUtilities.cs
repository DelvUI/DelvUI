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
            Rect background = new Rect(config.Position, config.Size, config.BackgroundColor);
            PluginConfigColor fillColor = config.IsThresholdActive(current) ? config.ThresholdColor : config.FillColor;
            Rect foreground = GetFillRect(config.Position, config.Size, config.FillDirection, fillColor, current, max, min);

            bar.Background(background);
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

        public static BarHud[] GetChunkedProgressBars(
            Vector2 position,
            Vector2 size,
            int chunks,
            int padding,
            float current,
            float max,
            float min,
            bool drawBorder,
            DrawAnchor anchor,
            BarDirection fillDirection,
            LabelConfig? activeChunkLabel,
            PluginConfigColor backgroundColor,
            PluginConfigColor fillColor,
            PluginConfigColor? partialFillColor = null,
            GameObject? actor = null)
        {
            BarHud[] bars = new BarHud[chunks];
            float chunkRange = (max - min) / chunks;
            var pos = Utils.GetAnchoredPosition(position, size, anchor);

            for (int i = 0; i < chunks; i++)
            {
                int barIndex = (fillDirection.IsInverted()) ? chunks - i - 1 : i;

                Vector2 chunkPos, chunkSize;
                if (fillDirection.IsHorizontal())
                {
                    chunkSize = new Vector2((size.X - padding * (chunks - 1)) / chunks, size.Y);
                    chunkPos = pos + new Vector2((chunkSize.X + padding) * barIndex, 0);
                }
                else
                {
                    chunkSize = new Vector2(size.X, (size.Y - padding * (chunks - 1)) / chunks);
                    chunkPos = pos + new Vector2(0, (chunkSize.Y + padding) * barIndex);
                }

                Rect background = new Rect(chunkPos, chunkSize, backgroundColor);

                float chunkMin = min + chunkRange * i;
                float chunkMax = min + chunkRange * (i + 1);
                PluginConfigColor chunkColor = partialFillColor is not null && current < chunkMax ? partialFillColor : fillColor;
                Rect foreground = GetFillRect(chunkPos, chunkSize, fillDirection, chunkColor, current, chunkMax, chunkMin);

                bars[i] = new BarHud(drawBorder, DrawAnchor.TopLeft, actor).Background(background).Foreground(foreground);

                if (activeChunkLabel is not null && current >= chunkMin && current < chunkMax)
                {
                    bars[i].Labels(activeChunkLabel);
                }
            }

            return bars;
        }

        public static BarHud[] GetChunkedProgressBars(
            BarConfig config,
            PluginConfigColor partialFillColor,
            int chunks,
            int padding,
            float current,
            float max,
            float min = 0f,
            LabelConfig? activeChunkLabel = null,
            GameObject? actor = null)
        {
            return GetChunkedProgressBars(
                config.Position,
                config.Size,
                chunks,
                padding,
                current,
                max,
                min,
                config.DrawBorder,
                config.Anchor,
                config.FillDirection,
                activeChunkLabel,
                config.BackgroundColor,
                config.FillColor,
                partialFillColor,
                actor);
        }

        /// <summary>
        /// Gets the horizonal or vertical offset depending on the fill direction.
        /// </summary>
        public static Vector2 GetFillDirectionOffset(Vector2 size, BarDirection fillDirection)
        {
            return fillDirection.IsHorizontal() ? new(size.X, 0) : new(0, size.Y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        /// <param name="fillDirection"></param>
        /// <param name="color"></param>
        /// <param name="current"></param>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public static Rect GetFillRect(Vector2 pos, Vector2 size, BarDirection fillDirection, PluginConfigColor color, float current, float max, float min = 0f)
        {
            float fillPercent = Math.Clamp((current - min) / (max - min), 0f, 1f);

            Vector2 fillPos = Vector2.Zero;
            Vector2 fillSize = fillDirection.IsHorizontal() ? new(size.X * fillPercent, size.Y) : new(size.X * fillPercent, size.Y);
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
