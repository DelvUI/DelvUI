using System;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;
using DelvUI.Config;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;

namespace DelvUI.Interface.GeneralElements
{
    public class GCDIndicatorHud : DraggableHudElement, IHudElementWithActor
    {
        private GCDIndicatorConfig Config => (GCDIndicatorConfig)_config;
        public GameObject? Actor { get; set; } = null;

        public GCDIndicatorHud(GCDIndicatorConfig config, string displayName) : base(config, displayName) { }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            var (pos, size) = GetPositionAndSize(Vector2.Zero);

            if (Config.CircularMode)
            {
                pos -= size / 2f;
            }

            return (new List<Vector2>() { pos }, new List<Vector2>() { size });
        }

        private (Vector2, Vector2) GetPositionAndSize(Vector2 origin)
        {
            Vector2 pos = Config.AnchorToMouse ? ImGui.GetMousePos() : origin + Config.Position;
            Vector2 size = Config.Size;

            if (Config.CircularMode)
            {
                size = new Vector2(Config.CircleRadius * 2, Config.CircleRadius * 2);
                pos += size / 2f;
            }
            else
            {
                if (Config.VerticalMode)
                {
                    size = new Vector2(Config.Size.Y, Config.Size.X);
                }
            }

            return (pos, size);
        }

        protected override void DrawDraggableArea(Vector2 origin)
        {
            if (Config.AnchorToMouse)
            {
                return;
            }

            base.DrawDraggableArea(origin);
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (!Config.Enabled || Actor == null || Actor is not PlayerCharacter)
            {
                return;
            }

            GCDHelper.GetGCDInfo((PlayerCharacter)Actor, out var elapsed, out var total);

            if (!Config.AlwaysShow && total == 0)
            {
                return;
            }

            var scale = elapsed / total;
            if (scale <= 0)
            {
                return;
            }

            var (pos, size) = GetPositionAndSize(origin);
            pos = Utils.GetAnchoredPosition(pos, size, Config.Anchor);

            if (Config.CircularMode)
            {
                DrawCircularIndicator(pos, Config.CircleRadius, elapsed, total);
            }
            else
            {
                DrawNormalBar(pos, size, elapsed, total);
            }
        }

        private void DrawCircularIndicator(Vector2 position, float radius, float current, float total)
        {
            total = Config.AlwaysShow && total == 0 ? 1 : total;
            current = Config.AlwaysShow && current == 0 ? total : current;

            var size = new Vector2(radius * 2);
            DrawHelper.DrawInWindow(ID, position - size / 2, size, false, false, (drawList) =>
            {
                current = Math.Min(current, total);

                // controls how smooth the arc looks
                const int segments = 100;
                const float queueTime = 0.5f;
                const float startAngle = 0f;

                if (Config.AlwaysShow && current == total)
                {
                    drawList.PathArcTo(position, radius, startAngle, 2f * (float)Math.PI, segments);
                    drawList.PathStroke(Config.Color.Base, ImDrawFlags.None, Config.CircleThickness);
                    return;
                }

                // always draw until the queue threshold
                float progressAngle = Math.Min(current, total - (Config.ShowGCDQueueIndicator ? queueTime : 0f)) / total * 2f * (float)Math.PI;

                // drawing an arc with thickness to make it look like an annular sector
                drawList.PathArcTo(position, radius, startAngle, progressAngle, segments);
                drawList.PathStroke(Config.Color.Base, ImDrawFlags.None, Config.CircleThickness);

                // draw the queue indicator
                if (Config.ShowGCDQueueIndicator && current > total - queueTime)
                {
                    float oldAngle = progressAngle - 0.0003f * total * 2f * (float)Math.PI;
                    progressAngle = current / total * 2f * (float)Math.PI;
                    drawList.PathArcTo(position, radius, oldAngle, progressAngle, segments);
                    drawList.PathStroke(Config.QueueColor.Base, ImDrawFlags.None, Config.CircleThickness);
                }

                // anything that remains is background
                drawList.PathArcTo(position, radius, progressAngle, 2f * (float)Math.PI, segments);
                drawList.PathStroke(Config.Color.Background, ImDrawFlags.None, Config.CircleThickness);

                if (Config.ShowBorder)
                {
                    drawList.PathArcTo(position, radius - Config.CircleThickness / 2f, 0f, 2f * (float)Math.PI, segments);
                    drawList.PathStroke(0xFF000000, ImDrawFlags.None, 1);

                    drawList.PathArcTo(position, radius + Config.CircleThickness / 2f, 0f, 2f * (float)Math.PI, segments);
                    drawList.PathStroke(0xFF000000, ImDrawFlags.None, 1);
                }
            });
        }

        private void DrawNormalBar(Vector2 position, Vector2 size, float current, float total)
        {
            DrawHelper.DrawInWindow(ID, position, size, false, false, (drawList) =>
            {
                var percentNonQueue = total != 0 ? 1F - (500f / 1000f) / total : 0;

                var builder = BarBuilder.Create(position, size);

                if (percentNonQueue > 0 && Config.ShowGCDQueueIndicator)
                {
                    builder.SetChunks(new float[2] { percentNonQueue, 1f - percentNonQueue });
                }

                total = Config.AlwaysShow && total == 0 ? 1 : total;
                current = Config.AlwaysShow && current == 0 ? total : current;

                builder.AddInnerBar(current, total, Config.Color)
                    .SetDrawBorder(Config.ShowBorder)
                    .SetVertical(Config.VerticalMode);

                if (percentNonQueue > 0 && Config.ShowGCDQueueIndicator)
                {
                    Vector2 queueStartOffset = Config.VerticalMode ? new(0, percentNonQueue * size.Y) : new(percentNonQueue * size.X, 0);
                    Vector2 queueEndOffset = Config.VerticalMode ? new(size.X, percentNonQueue * size.Y) : new(percentNonQueue * size.X, size.Y);

                    builder.SetChunksColors(new PluginConfigColor[2] { Config.Color, Config.QueueColor });
                    drawList.AddRect(position + queueStartOffset, position + queueEndOffset, Config.QueueColor.Base);
                }

                builder.Build().Draw(drawList);
            });
        }
    }
}
