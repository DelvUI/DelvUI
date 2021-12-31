using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using System;
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

        private PluginConfigColor? BorderColor { get; set; }

        private int BorderThickness { get; set; }

        private DrawAnchor Anchor { get; set; }

        private GameObject? Actor { get; set; }

        private PluginConfigColor? GlowColor { get; set; }

        private int GlowSize { get; set; }

        private float? Current;
        private float? Max;

        public BarHud(
            string id,
            bool drawBorder = true,
            PluginConfigColor? borderColor = null,
            int borderThickness = 1,
            DrawAnchor anchor = DrawAnchor.TopLeft,
            GameObject? actor = null,
            PluginConfigColor? glowColor = null,
            int? glowSize = 1,
            float? current = null,
            float? max = null)
        {
            ID = id;
            DrawBorder = drawBorder;
            BorderColor = borderColor;
            BorderThickness = borderThickness;
            Anchor = anchor;
            Actor = actor;
            GlowColor = glowColor;
            GlowSize = glowSize ?? 1;
            Current = current;
            Max = max;
        }

        public BarHud(BarConfig config, GameObject? actor = null, BarGlowConfig? glowConfig = null, float? current = null, float? max = null)
            : this(config.ID, config.DrawBorder, config.BorderColor, config.BorderThickness, config.Anchor, actor, glowConfig?.Color, glowConfig?.Size, current, max)
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
                    var labelHud = new LabelHud(config);
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

            DrawRects(barPos, backgroundPos);

            // labels
            foreach (LabelHud label in LabelHuds)
            {
                label.Draw(backgroundPos, BackgroundRect.Size, Actor, null, (uint?)Current, (uint?)Max);
            }
        }

        public List<(StrataLevel, Action)> GetDrawActions(Vector2 origin, StrataLevel strataLevel)
        {
            List<(StrataLevel, Action)> drawActions = new List<(StrataLevel, Action)>();

            var barPos = Utils.GetAnchoredPosition(origin, BackgroundRect.Size, Anchor);
            var backgroundPos = barPos + BackgroundRect.Position;

            drawActions.Add((strataLevel, () =>
            {
                DrawRects(barPos, backgroundPos);
            }
            ));

            // labels
            foreach (LabelHud label in LabelHuds)
            {
                drawActions.Add((label.GetConfig().StrataLevel, () =>
                {
                    label.Draw(backgroundPos, BackgroundRect.Size, Actor, null, (uint?)Current, (uint?)Max);
                }
                ));
            }

            return drawActions;
        }

        private void DrawRects(Vector2 barPos, Vector2 backgroundPos)
        {
            DrawHelper.DrawInWindow(ID, backgroundPos, BackgroundRect.Size, false, false, (drawList) =>
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
                    drawList.AddRect(backgroundPos, backgroundPos + BackgroundRect.Size, BorderColor?.Base ?? 0xFF000000, 0, ImDrawFlags.None, BorderThickness);
                }

                // Draw Glow
                if (GlowColor != null)
                {
                    var glowPosition = new Vector2(backgroundPos.X - 1, backgroundPos.Y - 1);
                    var glowSize = new Vector2(BackgroundRect.Size.X + 2, BackgroundRect.Size.Y + 2);

                    drawList.AddRect(glowPosition, glowPosition + glowSize, GlowColor.Base, 0, ImDrawFlags.None, GlowSize);
                }
            });
        }
    }
}
