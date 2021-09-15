using DelvUI.Interface;
using ImGuiNET;
using System;
using System.Numerics;

namespace DelvUI.Helpers
{
    public static class DraggablesHelper
    {
        public static void DrawGrid(DraggablesConfig config)
        {
            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.SetNextWindowSize(ImGui.GetMainViewport().Size);

            ImGui.SetNextWindowBgAlpha(0.3f);

            ImGui.Begin("DelvUI_draggables",
                ImGuiWindowFlags.NoTitleBar
              | ImGuiWindowFlags.NoScrollbar
              | ImGuiWindowFlags.AlwaysAutoResize
              //| ImGuiWindowFlags.NoBackground
              | ImGuiWindowFlags.NoInputs
              | ImGuiWindowFlags.NoBringToFrontOnFocus
              | ImGuiWindowFlags.NoDecoration
            );

            var drawList = ImGui.GetWindowDrawList();
            var screenSize = ImGui.GetMainViewport().Size;

            if (config.ShowGrid)
            {
                int count = (int)(Math.Max(screenSize.X, screenSize.Y) / config.GridSubdivisionDistance) / 2 + 1;
                var center = screenSize / 2f;

                for (int i = 1; i < count; i++)
                {
                    var step = i * config.GridSubdivisionDistance;

                    drawList.AddLine(new Vector2(center.X + step, 0), new Vector2(center.X + step, screenSize.Y), 0x33FFFFFF);
                    drawList.AddLine(new Vector2(center.X - step, 0), new Vector2(center.X - step, screenSize.Y), 0x33FFFFFF);

                    drawList.AddLine(new Vector2(0, center.Y + step), new Vector2(screenSize.X, center.Y + step), 0x33FFFFFF);
                    drawList.AddLine(new Vector2(0, center.Y - step), new Vector2(screenSize.X, center.Y - step), 0x33FFFFFF);
                }
            }

            if (config.ShowGuideLines)
            {
                drawList.AddLine(new Vector2(screenSize.X / 2f, 0), new Vector2(screenSize.X / 2f, screenSize.Y), 0xAA000000, 3);
                drawList.AddLine(new Vector2(0, screenSize.Y / 2f), new Vector2(screenSize.X, screenSize.Y / 2f), 0xAA000000, 3);
            }

            ImGui.End();
        }

        public static bool DrawArrows(Vector2 position, Vector2 size, string tooltipText, out Vector2 offset)
        {
            offset = Vector2.Zero;

            var windowFlags = ImGuiWindowFlags.NoScrollbar
                | ImGuiWindowFlags.NoTitleBar
                | ImGuiWindowFlags.NoResize
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoDecoration;

            var arrowSize = new Vector2(40, 40);
            var margin = new Vector2(4, 0);
            var windowSize = arrowSize + margin * 2;

            // left, right, up, down
            var positions = new Vector2[]
            {
                new Vector2(position.X - arrowSize.X + 10, position.Y + size.Y / 2f - arrowSize.Y / 2f - 2),
                new Vector2(position.X + size.X - 8, position.Y + size.Y / 2f - arrowSize.Y / 2f - 2),
                new Vector2(position.X + size.X / 2f - arrowSize.X / 2f + 2, position.Y - arrowSize.Y + 1),
                new Vector2(position.X + size.X / 2f - arrowSize.X / 2f + 2, position.Y + size.Y - 7)
            };
            var offsets = new Vector2[]
            {
                new Vector2(-1, 0),
                new Vector2(1, 0),
                new Vector2(0, -1),
                new Vector2(0, 1)
            };

            for (int i = 0; i < 4; i++)
            {
                var pos = positions[i] - margin;

                ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);
                ImGui.SetNextWindowPos(pos);

                ImGui.Begin("arrow " + i.ToString(), windowFlags);

                // fake button
                ImGui.ArrowButton("arrow button " + i.ToString(), (ImGuiDir)i);

                if (ImGui.IsMouseHoveringRect(pos, pos + windowSize))
                {
                    // track click manually to not deal with window focus stuff
                    if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                    {
                        offset = offsets[i];
                    }

                    // tooltip
                    TooltipsHelper.Instance.ShowTooltipOnCursor(tooltipText);
                }

                ImGui.End();
            }

            return offset != Vector2.Zero;
        }
    }
}
