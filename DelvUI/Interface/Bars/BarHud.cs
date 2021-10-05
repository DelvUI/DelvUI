using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using System.Numerics;

namespace DelvUI.Interface.Bars
{
    public class BarHud
    {
        public BarConfigBase Config { get; private set; }

        private GameObject? Actor { get; set; } = null;

        public BarHud(BarConfigBase config, GameObject? actor = null)
        {
            Config = config;
            Actor = actor;
        }

        public void Draw(Vector2 origin, float current, float max, float min = 0)
        {
            if (Config.HideWhenInactive && !Config.IsActive(current, max, min))
            {
                return;
            }

            Bar2[] bars = Config.GetBars(current, max, min, Actor);
            var barPos = Utils.GetAnchoredPosition(origin + Config.Position, Config.Size, Config.Anchor);

            DrawHelper.DrawInWindow($"DelvUI_Bar{Config.GetHashCode()}", barPos, Config.Size, false, false, (drawList) =>
            {
                foreach (Bar2 bar in bars)
                {
                    // Draw background
                    DrawHelper.DrawGradientFilledRect(barPos + bar.BackgroundRect.Position, bar.BackgroundRect.Size, bar.BackgroundRect.Color, drawList);

                    // Draw foregrounds
                    foreach (Rect rect in bar.ForegroundRects)
                    {
                        DrawHelper.DrawGradientFilledRect(barPos + rect.Position, rect.Size, rect.Color, drawList);
                    }

                    // Draw Border
                    if (Config.DrawBorder)
                    {
                        drawList.AddRect(barPos + bar.BackgroundRect.Position, barPos + bar.BackgroundRect.Position + bar.BackgroundRect.Size, 0xFF000000);
                    }

                    // Draw Labels
                    foreach (LabelConfig labelConfig in bar.Labels)
                    {
                        var labelHud = new LabelHud($"_label{labelConfig.GetHashCode()}", labelConfig);
                        labelHud.Draw(barPos + bar.BackgroundRect.Position, bar.BackgroundRect.Size, Actor);
                    }
                }
            });
        }
    }
}
