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

        private bool _wasBarEnabled = true;
        private bool _wasCircularModeEnabled = false;

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
            Vector2 pos = Config.AnchorToMouse ? ImGui.GetMousePos() + Config.Position : origin + Config.Position;
            Vector2 size = Config.Bar.Size;

            if (Config.CircularMode)
            {
                size = new Vector2(Config.CircleRadius * 2, Config.CircleRadius * 2);
                pos += size / 2f;
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
            CheckToggles();

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

            Config.Bar.Position = Config.Position;
            Config.Bar.Anchor = Config.Anchor;
            Config.Bar.BackgroundColor = Config.BackgroundColor;
            Config.Bar.FillColor = Config.FillColor;
            Config.Bar.DrawBorder = Config.ShowBorder;

            if (Config.Bar.Enabled)
            {
                DrawNormalBar(origin, elapsed, total);
            }
            else
            {
                var (pos, size) = GetPositionAndSize(origin);
                pos = Utils.GetAnchoredPosition(pos, size, Config.Anchor);
                DrawCircularIndicator(pos, Config.CircleRadius, elapsed, total);
            }

        }

        private void CheckToggles()
        {
            bool barEnabledChanged = _wasBarEnabled != Config.Bar.Enabled;
            if (barEnabledChanged)
            {
                Config.CircularMode = !Config.Bar.Enabled;
            }
            else
            {
                bool circularModeChanged = _wasCircularModeEnabled != Config.CircularMode;
                if (circularModeChanged)
                {
                    Config.Bar.Enabled = !Config.CircularMode;
                }
            }

            _wasBarEnabled = Config.Bar.Enabled;
            _wasCircularModeEnabled = Config.CircularMode;
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
                    drawList.PathStroke(Config.FillColor.Base, ImDrawFlags.None, Config.CircleThickness);
                }
                else
                {
                    // always draw until the queue threshold
                    float progressAngle = Math.Min(current, total - (Config.ShowGCDQueueIndicator ? queueTime : 0f)) / total * 2f * (float)Math.PI;

                    // drawing an arc with thickness to make it look like an annular sector
                    drawList.PathArcTo(position, radius, startAngle, progressAngle, segments);
                    drawList.PathStroke(Config.FillColor.Base, ImDrawFlags.None, Config.CircleThickness);

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
                    drawList.PathStroke(Config.BackgroundColor.Base, ImDrawFlags.None, Config.CircleThickness);
                }

                if (Config.ShowBorder)
                {
                    drawList.PathArcTo(position, radius - Config.CircleThickness / 2f, 0f, 2f * (float)Math.PI, segments);
                    drawList.PathStroke(0xFF000000, ImDrawFlags.None, 1);

                    drawList.PathArcTo(position, radius + Config.CircleThickness / 2f, 0f, 2f * (float)Math.PI, segments);
                    drawList.PathStroke(0xFF000000, ImDrawFlags.None, 1);
                }
            });
        }

        private void DrawNormalBar(Vector2 origin, float current, float total)
        {
            GCDBarConfig config = Config.Bar;

            Rect mainRect = BarUtilities.GetFillRect(config.Position, config.Size, config.FillDirection, config.FillColor, current, total, 0);
            BarHud bar = new BarHud(config, null, null);
            bar.AddForegrounds(mainRect);

            float currentPercent = current / total;
            float percentNonQueue = total != 0 ? 1F - (500f / 1000f) / total : 0;

            PluginLog.Log(current + " - " + percentNonQueue + " - " + total);

            if (percentNonQueue > 0 && currentPercent >= percentNonQueue && Config.ShowGCDQueueIndicator)
            {
                float scale = 1 - percentNonQueue;
                Vector2 size = config.FillDirection.IsHorizontal() ?
                    new Vector2(config.Size.X * scale, config.Size.Y) :
                    new Vector2(config.Size.X, config.Size.Y * scale);

                Vector2 pos = config.Position;
                if (config.FillDirection == BarDirection.Right)
                {
                    pos.X += config.Size.X * percentNonQueue;
                }
                else if (config.FillDirection == BarDirection.Down)
                {
                    pos.Y += config.Size.Y * percentNonQueue;
                }

                Rect foreground = BarUtilities.GetFillRect(pos, size, config.FillDirection, Config.QueueColor, currentPercent - percentNonQueue, scale, 0);
                bar.AddForegrounds(foreground);
            }

            bar.Draw(origin);
        }
    }
}
