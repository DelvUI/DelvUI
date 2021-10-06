using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using System;
using System.Numerics;
using System.Linq;

namespace DelvUI.Interface.Bars
{
    public class BarHud
    {
        public Rect BackgroundRect { get; private set; }

        public Rect[] ForegroundRects { get; private set; }

        public LabelHud[] Labels { get; private set; }

        public bool DrawBorder { get; private set; }

        public DrawAnchor Anchor { get; private set; }

        public GameObject? Actor { get; private set; }

        public BarHud(Rect background, Rect[] foregrounds, bool drawBorder, DrawAnchor anchor, LabelConfig[]? labels = null, GameObject? actor = null)
        {
            BackgroundRect = background;
            ForegroundRects = foregrounds;
            Labels = labels is not null ? labels.Select(c => new LabelHud($"_label{c.GetHashCode()}", c)).ToArray() : Array.Empty<LabelHud>();
            DrawBorder = drawBorder;
            Anchor = anchor;
            Actor = actor;
        }

        public void Draw(Vector2 origin)
        {
            var barPos = Utils.GetAnchoredPosition(origin, BackgroundRect.Size, Anchor);
            var backgroundPos = barPos + BackgroundRect.Position;

            DrawHelper.DrawInWindow($"DelvUI_Bar", backgroundPos, BackgroundRect.Size, false, false, (drawList) =>
            {
                // Draw background
                DrawHelper.DrawGradientFilledRect(backgroundPos, BackgroundRect.Size, BackgroundRect.Color, drawList);

                // Draw foregrounds
                foreach (Rect rect in ForegroundRects)
                {
                    DrawHelper.DrawGradientFilledRect(barPos + rect.Position, rect.Size, rect.Color, drawList);
                }

                // Draw Border
                if (DrawBorder)
                {
                    drawList.AddRect(backgroundPos, backgroundPos + BackgroundRect.Size, 0xFF000000);
                }

                // Draw Labels
                foreach (LabelHud label in Labels)
                {
                    label.Draw(backgroundPos, BackgroundRect.Size, Actor);
                }
            });
        }

        public static BarHud[] GetChunkedBars(
            int chunks,
            int chunkPadding,
            Vector2 size,
            float current,
            float max,
            float min,
            bool drawBorder,
            DrawAnchor anchor,
            BarDirection fillDirection,
            LabelConfig? activeChunkLabel,
            PluginConfigColor backgroundColor,
            PluginConfigColor fillColor,
            PluginConfigColor? chunkFullColor = null)
        {
            BarHud[] bars = new BarHud[chunks];
            float chunkRange = (max - min) / chunks;

            for (int i = 0; i < chunks; i++)
            {
                int barIndex = (fillDirection == BarDirection.Left || fillDirection == BarDirection.Up) ? chunks - i - 1 : i;

                Vector2 chunkPos, chunkSize;
                if (fillDirection == BarDirection.Right || fillDirection == BarDirection.Left)
                {
                    chunkSize = new Vector2((size.X - chunkPadding * (chunks - 1)) / chunks, size.Y);
                    chunkPos = new Vector2((chunkSize.X + chunkPadding) * barIndex, 0);
                }
                else
                {
                    chunkSize = new Vector2(size.X, (size.Y - chunkPadding * (chunks - 1)) / chunks);
                    chunkPos = new Vector2(0, (chunkSize.Y + chunkPadding) * barIndex);
                }

                Rect background = new Rect(chunkPos, chunkSize, backgroundColor);

                float chunkMin = min + chunkRange * i;
                float chunkMax = min + chunkRange * (i + 1);
                PluginConfigColor chunkColor = chunkFullColor is not null && current >= chunkMax ? chunkFullColor : fillColor;
                Rect foreground = Rect.GetFillRect(chunkPos, chunkSize, fillDirection, chunkColor, current, chunkMax, chunkMin);

                LabelConfig[]? labels = activeChunkLabel is not null && current >= chunkMin && current < chunkMax ? new[] { activeChunkLabel } : null;
                bars[i] = new BarHud(background, new[] { foreground }, drawBorder, anchor, labels);
            }

            return bars;
        }
    }

    public class Rect
    {
        public Vector2 Position { get; set; }

        public Vector2 Size { get; set; }

        public PluginConfigColor Color { get; set; }

        public Rect(Vector2 pos, Vector2 size, PluginConfigColor color)
        {
            Position = pos;
            Size = size;
            Color = color;
        }

        public static Vector2 GetFillDirectionOffset(Vector2 size, BarDirection fillDirection)
        {
            return fillDirection switch
            {
                BarDirection.Left => new(size.X, 0),
                BarDirection.Right => new(size.X, 0),
                BarDirection.Up => new(0, size.Y),
                BarDirection.Down => new(0, size.Y),
                _ => Vector2.Zero
            };
        }

        public static Rect GetFillRect(Vector2 pos, Vector2 size, BarDirection fillDirection, PluginConfigColor color, float current, float max, float min = 0f)
        {
            Vector2 fillPos = Vector2.Zero;
            Vector2 fillSize = Vector2.Zero;
            float fillPercent = Math.Clamp((current - min) / (max - min), 0f, 1f);

            if (fillDirection == BarDirection.Right)
            {
                fillSize = new Vector2(size.X * fillPercent, size.Y);
            }
            else if (fillDirection == BarDirection.Down)
            {
                fillSize = new Vector2(size.X, size.Y * fillPercent);
            }
            else if (fillDirection == BarDirection.Left)
            {
                fillSize = new Vector2(size.X * fillPercent, size.Y);
                fillPos = Utils.GetAnchoredPosition(new(size.X, 0), fillSize, DrawAnchor.TopRight);
            }
            else if (fillDirection == BarDirection.Up)
            {
                fillSize = new Vector2(size.X, size.Y * fillPercent);
                fillPos = Utils.GetAnchoredPosition(new(0, size.Y), fillSize, DrawAnchor.BottomLeft);
            }

            return new Rect(pos + fillPos, fillSize, color);
        }
    }
}
