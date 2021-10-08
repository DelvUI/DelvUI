using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;

namespace DelvUI.Interface.Bars
{
    public class BarHud
    {
        private string ID { get; set; }

        private Rect BackgroundRect { get; set; } = new Rect();

        private List<Rect> ForegroundRects { get; set; } = new List<Rect>();

        private List<LabelHud> LabelHuds { get; set; } = new List<LabelHud>();

        private bool DrawBorder { get; set; }

        private DrawAnchor Anchor { get; set; }

        private GameObject? Actor { get; set; }

        private PluginConfigColor? GlowColor { get; set; }

        private int GlowSize { get; set; }

        public BarHud(
            string id,
            bool drawBorder = true,
            DrawAnchor anchor = DrawAnchor.TopLeft,
            GameObject? actor = null,
            PluginConfigColor? glowColor = null,
            int? glowSize = 1)
        {
            ID = id;
            DrawBorder = drawBorder;
            Anchor = anchor;
            Actor = actor;
            GlowColor = glowColor;
            GlowSize = glowSize ?? 1;
        }

        public BarHud(BarConfig config, GameObject? actor = null, BarGlowConfig? glowConfig = null)
            : this(config.ID, config.DrawBorder, config.Anchor, actor, glowConfig?.Color, glowConfig?.Size)
        {
            BackgroundRect = new Rect(config.Position, config.Size, config.BackgroundColor);
        }

        public BarHud SetBackground(Rect rect)
        {
            BackgroundRect = rect;
            return this;
        }

        public BarHud AddForegrounds(params Rect[] rects)
        {
            ForegroundRects.AddRange(rects);
            return this;
        }

        public BarHud AddLabels(params LabelConfig[]? labels)
        {
            if (labels != null)
            {
                foreach (LabelConfig config in labels)
                {
                    var labelHud = new LabelHud(ID + "_barLabel" + LabelHuds.Count, config);
                    LabelHuds.Add(labelHud);
                }
            }

            return this;
        }

        public BarHud SetGlow(PluginConfigColor color, int size = 1)
        {
            GlowColor = color;
            GlowSize = size;

            return this;
        }

        public void Draw(Vector2 origin)
        {
            var barPos = Utils.GetAnchoredPosition(origin, BackgroundRect.Size, Anchor);
            var backgroundPos = barPos + BackgroundRect.Position;

            DrawHelper.DrawInWindow(ID, backgroundPos, BackgroundRect.Size, true, false, (drawList) =>
            {
                // Draw background
                drawList.AddRectFilled(backgroundPos, backgroundPos + BackgroundRect.Size, BackgroundRect.Color.Base);

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

                // Draw Glow
                if (GlowColor != null)
                {
                    var glowPosition = new Vector2(backgroundPos.X - 1, backgroundPos.Y - 1);
                    var glowSize = new Vector2(BackgroundRect.Size.X + 2, BackgroundRect.Size.Y + 2);

                    drawList.AddRect(glowPosition, glowPosition + glowSize, GlowColor.Base, 0, ImDrawFlags.None, GlowSize);
                }
            });

            // Draw Labels
            foreach (LabelHud label in LabelHuds)
            {
                label.Draw(backgroundPos, BackgroundRect.Size, Actor);
            }
        }
    }
}
