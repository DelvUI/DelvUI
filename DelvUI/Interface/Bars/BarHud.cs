using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.Bars
{
    public class BarHud
    {
        public Rect BackgroundRect { get; private set; } = new Rect();

        public  List<Rect> ForegroundRects { get;  set; } = new List<Rect>();

        public List<LabelHud> LabelHuds { get; private set; } = new List<LabelHud>();

        public bool DrawBorder { get; private set; }

        public DrawAnchor Anchor { get; private set; }

        public GameObject? Actor { get; private set; }

        public BarHud(bool drawBorder = true, DrawAnchor anchor = DrawAnchor.TopLeft, GameObject? actor = null)
        {
            DrawBorder = drawBorder;
            Anchor = anchor;
            Actor = actor;
        }

        public BarHud(BarConfig config, GameObject? actor = null) : this(config.DrawBorder, config.Anchor, actor) {}

        public BarHud Background(Rect rect)
        {
            BackgroundRect = rect;
            return this;
        }

        public BarHud Foreground(params Rect[] rects)
        {
            ForegroundRects.AddRange(rects);
            return this;
        }

        public BarHud Labels(params LabelConfig[] labels)
        {
            LabelHuds = labels.Select(c => new LabelHud("_barLabel", c)).ToList();
            return this;
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
                foreach (LabelHud label in LabelHuds)
                {
                    label.Draw(backgroundPos, BackgroundRect.Size, Actor);
                }
            });
        }
    }
}
