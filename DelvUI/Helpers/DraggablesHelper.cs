﻿using DelvUI.Config;
using DelvUI.Interface;
using ImGuiNET;
using System;
using System.Numerics;

namespace DelvUI.Helpers
{
    public static class DraggablesHelper
    {
        public static void DrawGrid(GridConfig config)
        {
            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.SetNextWindowSize(ImGui.GetMainViewport().Size);

            ImGui.SetNextWindowBgAlpha(config.BackgroundAlpha);

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
                int count = (int)(Math.Max(screenSize.X, screenSize.Y) / config.GridDivisionsDistance) / 2 + 1;
                var center = screenSize / 2f;

                for (int i = 0; i < count; i++)
                {
                    var step = i * config.GridDivisionsDistance;

                    drawList.AddLine(new Vector2(center.X + step, 0), new Vector2(center.X + step, screenSize.Y), 0x88888888);
                    drawList.AddLine(new Vector2(center.X - step, 0), new Vector2(center.X - step, screenSize.Y), 0x88888888);

                    drawList.AddLine(new Vector2(0, center.Y + step), new Vector2(screenSize.X, center.Y + step), 0x88888888);
                    drawList.AddLine(new Vector2(0, center.Y - step), new Vector2(screenSize.X, center.Y - step), 0x88888888);

                    if (config.GridSubdivisionCount > 1)
                    {
                        for (int j = 1; j < config.GridSubdivisionCount; j++)
                        {
                            var subStep = j * (config.GridDivisionsDistance / config.GridSubdivisionCount);

                            drawList.AddLine(new Vector2(center.X + step + subStep, 0), new Vector2(center.X + step + subStep, screenSize.Y), 0x44888888);
                            drawList.AddLine(new Vector2(center.X - step - subStep, 0), new Vector2(center.X - step - subStep, screenSize.Y), 0x44888888);

                            drawList.AddLine(new Vector2(0, center.Y + step + subStep), new Vector2(screenSize.X, center.Y + step + subStep), 0x44888888);
                            drawList.AddLine(new Vector2(0, center.Y - step - subStep), new Vector2(screenSize.X, center.Y - step - subStep), 0x44888888);
                        }
                    }
                }
            }

            if (config.ShowCenterLines)
            {
                drawList.AddLine(new Vector2(screenSize.X / 2f, 0), new Vector2(screenSize.X / 2f, screenSize.Y), 0xAAFFFFFF, 1);
                drawList.AddLine(new Vector2(0, screenSize.Y / 2f), new Vector2(screenSize.X, screenSize.Y / 2f), 0xAAFFFFFF, 1);
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

        public static void DrawGridWindow(GridConfig config)
        {
            var configManager = ConfigurationManager.GetInstance();
            var node = configManager.GetConfigPageNode<GridConfig>();
            if (node == null)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(345, 253), ImGuiCond.Appearing);
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(10f / 255f, 10f / 255f, 10f / 255f, 0.95f));

            if (!ImGui.Begin("Grid", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse))
            {
                return;
            }

            var changed = false;
            node.Draw(ref changed);

            ImGui.NewLine();

            if (ImGui.Button("Lock HUD", new Vector2(329, 30)))
            {
                changed = true;
                config.Enabled = false;
                configManager.LockHUD = true;
            }

            if (changed)
            {
                configManager.SaveConfigurations();
            }

            ImGui.End();
        }
    }
}
