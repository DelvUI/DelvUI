using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using System;
using System.Numerics;

namespace DelvUI.Interface.Bars
{
    public class Bar2
    {
        public BarConfig Config { get; private set; }

        private LabelHud _barLabel { get; set; }

        public Bar2(BarConfig config)
        {
            Config = config;
            _barLabel = new LabelHud("_barLabel", Config.BarLabelConfig);
        }

        public void SetBarText(string text)
        {
            Config.BarLabelConfig.SetText(text);
        }

        public void Draw(Vector2 origin, float current, float max, float threshold = 0f)
        {
            if (current <= 0 && Config.HideWhenInactive)
            {
                return;
            }

            var barPos = Utils.GetAnchoredPosition(origin + Config.Position, Config.Size, Config.Anchor);
            float progressPercent = current / max;
            int chunks = Config.Chunk ? Config.ChunkNum : 1;
            float percentPerChunk = 1f / chunks;

            var thresholdPos = new Vector2(barPos.X + threshold / max * Config.Size.X, barPos.Y);
            var thresholdMarkerSize = new Vector2(2, Config.Size.Y);

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            for (int i = 0; i < chunks; i++)
            {
                float chunkProgress = Math.Clamp((progressPercent - percentPerChunk * i) / percentPerChunk, 0f, 1f);
                int barIndex = (Config.FillDirection == BarDirection.Left || Config.FillDirection == BarDirection.Up) ? chunks - i - 1 : i;

                Vector2 chunkPos, chunkSize, chunkFillSize;
                if (Config.FillDirection == BarDirection.Right || Config.FillDirection == BarDirection.Left)
                {
                    chunkSize = new Vector2((Config.Size.X - Config.ChunkPadding * (chunks - 1)) / chunks, Config.Size.Y);
                    chunkPos = barPos + new Vector2((chunkSize.X + Config.ChunkPadding) * barIndex, 0);
                    chunkFillSize = new Vector2(chunkSize.X * chunkProgress, chunkSize.Y);
                }
                else
                {
                    chunkSize = new Vector2(Config.Size.X, (Config.Size.Y - Config.ChunkPadding * (chunks - 1)) / chunks);
                    chunkPos = barPos + new Vector2(0, (chunkSize.Y + Config.ChunkPadding) * barIndex);
                    chunkFillSize = new Vector2(chunkSize.X, chunkSize.Y * chunkProgress);
                }

                Vector2 chunkFillOffset = Config.FillDirection switch
                {
                    BarDirection.Left => new Vector2(chunkSize.X - chunkFillSize.X, 0),
                    BarDirection.Up => new Vector2(0, chunkSize.Y - chunkFillSize.Y),
                    _ => Vector2.Zero
                };

                var chunkFillPos = chunkPos + chunkFillOffset;

                // Draw bar background
                drawList.AddRectFilled(chunkPos, chunkPos + chunkSize, Config.BackgroundColor.Base);

                // Draw inner bar
                var barColor = Config.UseThresholdColor && current < threshold ? Config.ThresholdColor.Base : Config.FillColor.Base;
                drawList.AddRectFilled(chunkFillPos, chunkFillPos + chunkFillSize, barColor);

                // Draw border
                if (Config.DrawBorder)
                {
                    drawList.AddRect(chunkPos, chunkPos + chunkSize, 0xFF000000);
                }
            }

            // Draw threshold marker
            if (current > threshold && Config.DrawThresholdMarker)
            {
                drawList.AddRectFilled(thresholdPos, thresholdPos + thresholdMarkerSize, Config.ThresholdMarkerColor.Base);
            }

            // Draw label
            _barLabel.Draw(barPos, Config.Size);
        }
    }
}
