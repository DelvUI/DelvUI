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
        private float _lastTotalCastTime = 0;

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
                _lastTotalCastTime = 0;
                return;
            }

            if (_lastTotalCastTime == 0 && ((BattleChara)Actor).IsCasting)
            {
                _lastTotalCastTime = ((BattleChara)Actor).TotalCastTime;
            }

            var scale = elapsed / total;
            if (scale <= 0)
            {
                _lastTotalCastTime = 0;
                return;
            }

            bool instantGCDsOnly = Config.InstantGCDsOnly && _lastTotalCastTime != 0;
            bool thresholdGCDs = Config.LimitGCDThreshold && _lastTotalCastTime > Config.GCDThreshold;

            if (instantGCDsOnly || thresholdGCDs)
            {
                if (Config.AlwaysShow)
                {
                    elapsed = 0;
                    total = 0;
                }
                else
                {
                    return;
                }
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

                AddDrawAction(_config.StrataLevel, () =>
                {
                    DrawCircularIndicator(pos, Config.CircleRadius, elapsed, total);
                });
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
                float startAngle = 0f;
                float endAngle = 2f * (float)Math.PI;
                float offset = (float)(-Math.PI / 2f + (Config.CircleStartAngle * (Math.PI / 180f)));

                if (Config.RotateCCW)
                {
                    startAngle *= -1;
                    endAngle *= -1;
                }

                if (Config.AlwaysShow && current == total)
                {
                    drawList.PathArcTo(position, radius, startAngle + offset, endAngle + offset, segments);
                    drawList.PathStroke(Config.FillColor.Base, ImDrawFlags.None, Config.CircleThickness);
                }
                else
                {
                    // always draw until the queue threshold
                    float progressAngle = Math.Min(current, total - (Config.ShowGCDQueueIndicator ? queueTime : 0f)) / total * endAngle;

                    // drawing an arc with thickness to make it look like an annular sector
                    drawList.PathArcTo(position, radius, startAngle + offset, progressAngle + offset, segments);
                    drawList.PathStroke(Config.FillColor.Base, ImDrawFlags.None, Config.CircleThickness);

                    // draw the queue indicator
                    if (Config.ShowGCDQueueIndicator && current > total - queueTime)
                    {
                        float oldAngle = progressAngle - 0.0003f * total * endAngle;
                        progressAngle = current / total * endAngle;
                        drawList.PathArcTo(position, radius, oldAngle + offset, progressAngle + offset, segments);
                        drawList.PathStroke(Config.QueueColor.Base, ImDrawFlags.None, Config.CircleThickness);
                    }

                    // anything that remains is background
                    drawList.PathArcTo(position, radius, progressAngle + offset, endAngle + offset, segments);
                    drawList.PathStroke(Config.BackgroundColor.Base, ImDrawFlags.None, Config.CircleThickness);
                }

                if (Config.ShowBorder)
                {
                    drawList.PathArcTo(position, radius - Config.CircleThickness / 2f, 0, endAngle, segments);
                    drawList.PathStroke(0xFF000000, ImDrawFlags.None, 1);

                    drawList.PathArcTo(position, radius + Config.CircleThickness / 2f, 0, endAngle, segments);
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

            AddDrawActions(bar.GetDrawActions(origin, _config.StrataLevel));
        }
    }
}
