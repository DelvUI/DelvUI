﻿using System;
using Dalamud.Game.ClientState.Actors.Types;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using ImGuiNET;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Dalamud.Plugin;
using DelvUI.Config;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DelvUI.Interface.GeneralElements
{
    public class GCDIndicatorHud : DraggableHudElement, IHudElementWithActor
    {
        private GCDIndicatorConfig Config => (GCDIndicatorConfig)_config;
        public Actor Actor { get; set; } = null;

        public GCDIndicatorHud(string ID, GCDIndicatorConfig config, string displayName) : base(ID, config, displayName) { }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            return (new List<Vector2>() { Config.Position }, new List<Vector2>() { Config.Size });
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

            var startPos = origin + Config.Position - Config.Size / 2f;
            
            if (Config.AnchorToMouse)
            {
                startPos = ImGui.GetMousePos();

                if (Config.OffsetMousePosition)
                {
                    startPos += Config.Position;
                }
            }
            
            if (Config.CircularMode)
            {
                DrawCircularIndicator(startPos, Config.CircleRadius, elapsed, total);
            }
            else
            {
                DrawNormalBar(startPos, elapsed, total);
            }
        }

        private void DrawCircularIndicator(Vector2 position, float radius, float current, float total)
        {
            current = Math.Min(current, total);
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            // controls how smooth the arc looks
            const int segments = 100;
            const float queueTime = 0.5f;
            // TODO add this as a parameter
            const float startAngle = 0f;

            // always draw until the queue threshold
            float progressAngle = Math.Min(current, total - (Config.ShowGCDQueueIndicator ? queueTime : 0f)) / total * 2f * (float)Math.PI;
            // drawing an arc with thickness to make it look like an annular sector
            drawList.PathArcTo(position, radius, startAngle, progressAngle, segments);
            drawList.PathStroke(Config.Color.Base, ImDrawFlags.None, Config.CircleThickness);
            // draw the queue indicator
            if(Config.ShowGCDQueueIndicator && current > total - queueTime)
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
        }

        private void DrawNormalBar(Vector2 position, float current, float total)
        {
            var size = !Config.VerticalMode ? Config.Size : new Vector2(Config.Size.Y, -Config.Size.X);

            var percentNonQueue = 1F - (500f / 1000f) / total;

            var drawList = ImGui.GetWindowDrawList();
            var builder = BarBuilder.Create(position, size)
                                    .SetChunks(new float[2] { percentNonQueue, 1F - percentNonQueue })
                                    .AddInnerBar(current, total, Config.Color.Map)
                                    .SetDrawBorder(Config.ShowBorder)
                                    .SetVertical(Config.VerticalMode);

            var queueStartOffset = Config.VerticalMode ? new Vector2(0, percentNonQueue * size.Y) : new Vector2(percentNonQueue * size.X, 0);
            var queueEndOffset = Config.VerticalMode ? new Vector2(size.X, percentNonQueue * size.Y + 1f) : new Vector2(percentNonQueue * size.X + 1f, size.Y);
            if (Config.ShowGCDQueueIndicator)
            {
                builder.SetChunksColors(new Dictionary<string, uint>[2] { Config.Color.Map, Config.QueueColor.Map });
                drawList.AddRect(position + queueStartOffset, position + queueEndOffset, Config.QueueColor.Base);
            }
            
            builder.Build().Draw(drawList);
        }
        
    }
}
