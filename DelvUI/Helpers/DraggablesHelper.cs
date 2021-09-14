using DelvUI.Interface;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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

    }
}
