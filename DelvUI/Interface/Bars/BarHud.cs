using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using System;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.Bars
{
    public class BarHud
    {
        public BarConfig Config { get; private set; }

        private GameObject? Actor { get; set; } = null;

        private LabelHud[] Labels { get; set; }

        public BarHud(BarConfig config, GameObject? actor, params LabelConfig[] labelConfigs)
        {
            Config = config;
            Labels = labelConfigs.Select(config => new LabelHud($"_label{config.GetHashCode()}", config)).ToArray();
            Actor = actor;
        }

        public BarHud(BarConfig config, params LabelConfig[] labelConfigs) : this(config, null, labelConfigs) { }

        public void Draw(Vector2 origin, float current, float max, float mid = 0, int chunks = 1, int chunkPadding = 2)
        {
            if (current <= 0f && Config.HideWhenInactive)
            {
                return;
            }

            var barPos = Utils.GetAnchoredPosition(origin + Config.Position, Config.Size, Config.Anchor);
            float progressPercent = current / max;
            float midPercent = mid / max;
            float percentPerChunk = 1f / chunks;

            DrawHelper.DrawInWindow($"DelvUI_Bar{Config.GetHashCode()}", barPos, Config.Size, false, false, (drawList) =>
            {
                for (int i = 0; i < chunks; i++)
                {
                    float chunkProgress = Math.Clamp((progressPercent - percentPerChunk * i) / percentPerChunk, 0f, 1f);
                    float midProgress = Math.Clamp((midPercent - percentPerChunk * i) / percentPerChunk, 0f, 1f);
                    int barIndex = (Config.FillDirection == BarDirection.Left || Config.FillDirection == BarDirection.Up) ? chunks - i - 1 : i;

                    Vector2 chunkPos, chunkSize, chunkFillSize, midFillSize;
                    if (Config.FillDirection == BarDirection.Right || Config.FillDirection == BarDirection.Left)
                    {
                        chunkSize = new Vector2((Config.Size.X - chunkPadding * (chunks - 1)) / chunks, Config.Size.Y);
                        chunkPos = barPos + new Vector2((chunkSize.X + chunkPadding) * barIndex, 0);
                        chunkFillSize = new Vector2(chunkSize.X * chunkProgress, chunkSize.Y);
                        midFillSize = new Vector2(chunkSize.X * midProgress - chunkFillSize.X, chunkSize.Y);
                    }
                    else
                    {
                        chunkSize = new Vector2(Config.Size.X, (Config.Size.Y - chunkPadding * (chunks - 1)) / chunks);
                        chunkPos = barPos + new Vector2(0, (chunkSize.Y + chunkPadding) * barIndex);
                        chunkFillSize = new Vector2(chunkSize.X, chunkSize.Y * chunkProgress);
                        midFillSize = new Vector2(chunkSize.X, chunkSize.Y * midProgress - chunkFillSize.Y);
                    }

                    Vector2 chunkFillPos = chunkPos + Config.FillDirection switch
                    {
                        BarDirection.Left => new Vector2(chunkSize.X - chunkFillSize.X, 0),
                        BarDirection.Up => new Vector2(0, chunkSize.Y - chunkFillSize.Y),
                        _ => Vector2.Zero
                    };

                    // Draw bar background
                    drawList.AddRectFilled(chunkPos, chunkPos + chunkSize, Config.BackgroundColor.Base);

                    if (mid > current)
                    {
                        Vector2 midFillPos = chunkFillPos + Config.FillDirection switch
                        {
                            BarDirection.Right => new Vector2(chunkFillSize.X, 0),
                            BarDirection.Down => new Vector2(0, chunkFillSize.Y),
                            BarDirection.Left => new Vector2(-midFillSize.X, 0),
                            BarDirection.Up => new Vector2(0, -midFillSize.Y),
                            _ => Vector2.Zero
                        };

                        PluginConfigColor midBarColor = Config.GetBarColor(mid, Actor);
                        drawList.AddRectFilled(midFillPos, midFillPos + midFillSize, midBarColor.Base);
                    }

                    // Draw inner bar
                    PluginConfigColor barColor = Config.GetBarColor(current, Actor);
                    drawList.AddRectFilled(chunkFillPos, chunkFillPos + chunkFillSize, barColor.Base);

                    // Draw border
                    if (Config.DrawBorder)
                    {
                        drawList.AddRect(chunkPos, chunkPos + chunkSize, 0xFF000000);
                    }

                    if (progressPercent < percentPerChunk * (i + 1) && progressPercent >= percentPerChunk * i)
                    {
                        foreach (LabelHud labelHud in Labels)
                        {
                            labelHud.Draw(chunkPos, chunkSize, Actor);
                        }
                    }
                }
            });
        }
    }
}
