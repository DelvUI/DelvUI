using Dalamud.Game.Internal.Gui.Addon;
using ImGuiNET;
using ImGuiScene;
using Lumina.Excel;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DelvUI.Helpers
{
    public static class DrawHelper
    {
        public static void DrawOutlinedText(string text, Vector2 pos, float fontScale)
        {
            DrawOutlinedText(text, pos, Vector4.One, Vector4.UnitW, fontScale);
        }

        public static void DrawOutlinedText(string text, Vector2 pos, Vector4 color, Vector4 outlineColor, float fontScale)
        {
            DrawOutlinedText(text, pos, color, outlineColor, fontScale, FontsManager.Instance.BigNoodleTooFont);
        }

        public static void DrawOutlinedText(string text, Vector2 pos, Vector4 color, Vector4 outlineColor, float fontScale, ImFontPtr fontPtr)
        {
            var originalScale = fontPtr.Scale;
            fontPtr.Scale = fontScale;
            ImGui.PushFont(fontPtr);
            DrawOutlinedText(text, pos, color, outlineColor);
            ImGui.PopFont();
            fontPtr.Scale = originalScale;
        }

        public static void DrawOutlinedText(string text, Vector2 pos) { DrawOutlinedText(text, pos, Vector4.One, Vector4.UnitW); }

        public static void DrawOutlinedText(string text, Vector2 pos, Vector4 color, Vector4 outlineColor)
        {
            ImGui.SetCursorPos(new Vector2(pos.X - 1, pos.Y + 1));
            ImGui.TextColored(outlineColor, text);

            ImGui.SetCursorPos(new Vector2(pos.X, pos.Y + 1));
            ImGui.TextColored(outlineColor, text);

            ImGui.SetCursorPos(new Vector2(pos.X + 1, pos.Y + 1));
            ImGui.TextColored(outlineColor, text);

            ImGui.SetCursorPos(new Vector2(pos.X - 1, pos.Y));
            ImGui.TextColored(outlineColor, text);

            ImGui.SetCursorPos(new Vector2(pos.X + 1, pos.Y));
            ImGui.TextColored(outlineColor, text);

            ImGui.SetCursorPos(new Vector2(pos.X - 1, pos.Y - 1));
            ImGui.TextColored(outlineColor, text);

            ImGui.SetCursorPos(new Vector2(pos.X, pos.Y - 1));
            ImGui.TextColored(outlineColor, text);

            ImGui.SetCursorPos(new Vector2(pos.X + 1, pos.Y - 1));
            ImGui.TextColored(outlineColor, text);

            ImGui.SetCursorPos(new Vector2(pos.X, pos.Y));
            ImGui.TextColored(color, text);
        }

        public static void DrawOutlinedText(string text, Vector2 pos, ImDrawListPtr drawList, int thickness = 1)
        {
            DrawOutlinedText(text, pos, 0xFFFFFFFF, 0xFF000000, drawList, thickness);
        }

        public static void DrawOutlinedText(string text, Vector2 pos, uint color, uint outlineColor, ImDrawListPtr drawList, int thickness = 1)
        {
            // outline
            for (int i = 1; i < thickness + 1; i++)
            {
                drawList.AddText(new Vector2(pos.X - i, pos.Y + i), outlineColor, text);
                drawList.AddText(new Vector2(pos.X, pos.Y + i), outlineColor, text);
                drawList.AddText(new Vector2(pos.X + i, pos.Y + i), outlineColor, text);
                drawList.AddText(new Vector2(pos.X - i, pos.Y), outlineColor, text);
                drawList.AddText(new Vector2(pos.X + i, pos.Y), outlineColor, text);
                drawList.AddText(new Vector2(pos.X - i, pos.Y - i), outlineColor, text);
                drawList.AddText(new Vector2(pos.X, pos.Y - i), outlineColor, text);
                drawList.AddText(new Vector2(pos.X + i, pos.Y - i), outlineColor, text);
            }

            // text
            drawList.AddText(new Vector2(pos.X, pos.Y), color, text);
        }

        public static void DrawIcon<T>(dynamic row, Vector2 position, Vector2 size, bool drawBorder) where T : ExcelRow
        {
            var texture = GetIconAndTexCoordinates<T>(row, size, out Vector2 uv0, out Vector2 uv1);
            if (texture == null)
            {
                return;
            }

            ImGui.SetCursorPos(position);
            ImGui.Image(texture.ImGuiHandle, size, uv0, uv1);

            if (drawBorder)
            {
                var drawList = ImGui.GetWindowDrawList();
                drawList.AddRect(position, position + size, 0xFF000000);
            }
        }

        public static void DrawIcon<T>(dynamic row, Vector2 position, Vector2 size, bool drawBorder, ImDrawListPtr drawList) where T : ExcelRow
        {
            var texture = GetIconAndTexCoordinates<T>(row, size, out Vector2 uv0, out Vector2 uv1);
            if (texture == null)
            {
                return;
            }

            drawList.AddImage(texture.ImGuiHandle, position, position + size, uv0, uv1);

            if (drawBorder)
            {
                drawList.AddRect(position, position + size, 0xFF000000);
            }
        }

        public static TextureWrap GetIconAndTexCoordinates<T>(dynamic row, Vector2 size, out Vector2 uv0, out Vector2 uv1) where T : ExcelRow
        {
            uv0 = Vector2.Zero;
            uv1 = Vector2.Zero;

            // Status = 24x32, show from 2,7 until 22,26
            var texture = TexturesCache.Instance.GetTexture<T>(row);
            if (texture == null)
            {
                return null;
            }

            uv0 = new Vector2(4f / texture.Width, 14f / texture.Height);
            uv1 = new Vector2(1f - 4f / texture.Width, 1f - 12f / texture.Height);
            return texture;
        }

        public static void DrawOvershield(float shield, Vector2 cursorPos, Vector2 barSize, float height, bool useRatioForHeight, Dictionary<string, uint> color, ImDrawListPtr drawList)
        {
            if (shield == 0)
            {
                return;
            }

            var h = !useRatioForHeight ? barSize.Y / 100 * height : height;

            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(Math.Max(1, barSize.X * shield), h),
                color["gradientTop"], color["gradientTop"], color["gradientBottom"], color["gradientBottom"]
            );
        }

        public static void DrawShield(float shield, float hp, Vector2 cursorPos, Vector2 barSize, float height, bool useRatioForHeight, Dictionary<string, uint> color, ImDrawListPtr drawList)
        {
            if (shield == 0)
            {
                return;
            }

            // on full hp just draw overshield
            if (hp == 1)
            {
                DrawOvershield(shield, cursorPos, barSize, height, useRatioForHeight, color, drawList);
                return;
            }


            // hp portion
            var h = !useRatioForHeight ? barSize.Y / 100 * Math.Min(100, height) : height;
            var missingHPRatio = 1 - hp;
            var s = Math.Min(shield, missingHPRatio);
            var shieldStartPos = cursorPos + new Vector2(Math.Max(1, barSize.X * hp), 0);
            drawList.AddRectFilledMultiColor(
                shieldStartPos, shieldStartPos + new Vector2(Math.Max(1, barSize.X * s), barSize.Y),
                color["gradientTop"], color["gradientTop"], color["gradientBottom"], color["gradientBottom"]
            );

            // overshield
            shield = shield - s;
            if (shield <= 0)
            {
                return;
            }

            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(Math.Max(1, barSize.X * shield), h),
                color["gradientTop"], color["gradientTop"], color["gradientBottom"], color["gradientBottom"]
            );
        }

        public static void ClipAround(Addon addon, string windowName, ImDrawListPtr drawList, Action<ImDrawListPtr, string> drawAction)
        {
            if (addon is { Visible: true })
            {
                ClipAround(new Vector2(addon.X + 5, addon.Y + 5), new Vector2(addon.X + addon.Width - 5, addon.Y + addon.Height - 5), windowName, drawList, drawAction);
            }
            else
            {
                drawAction(drawList, windowName);
            }
        }

        public static void ClipAround(Vector2 min, Vector2 max, string windowName, ImDrawListPtr drawList, Action<ImDrawListPtr, string> drawAction)
        {
            var maxX = ImGui.GetMainViewport().Size.X;
            var maxY = ImGui.GetMainViewport().Size.Y;
            var aboveMin = new Vector2(0, 0);
            var aboveMax = new Vector2(maxX, min.Y);
            var leftMin = new Vector2(0, min.Y);
            var leftMax = new Vector2(min.X, maxY);

            var rightMin = new Vector2(max.X, min.Y);
            var rightMax = new Vector2(maxX, max.Y);
            var belowMin = new Vector2(min.X, max.Y);
            var belowMax = new Vector2(maxX, maxY);

            for (var i = 0; i < 4; i++)
            {
                Vector2 clipMin;
                Vector2 clipMax;

                switch (i)
                {
                    default:
                        clipMin = aboveMin;
                        clipMax = aboveMax;

                        break;

                    case 1:
                        clipMin = leftMin;
                        clipMax = leftMax;

                        break;

                    case 2:
                        clipMin = rightMin;
                        clipMax = rightMax;

                        break;

                    case 3:
                        clipMin = belowMin;
                        clipMax = belowMax;

                        break;
                }

                ImGui.PushClipRect(clipMin, clipMax, false);
                drawAction(drawList, windowName + "_" + i);
                ImGui.PopClipRect();
            }
        }
    }
}
