﻿using ImGuiNET;
using Lumina.Excel;
using System;
using System.Numerics;
using DelvUI.Config;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiScene;

namespace DelvUI.Helpers
{
    public enum GradientDirection
    {
        None,
        Right,
        Left,
        Up,
        Down,
        CenteredHorizonal,
    }

    public static class DrawHelper
    {
        private static uint[] ColorArray(PluginConfigColor color, GradientDirection gradientDirection)
        {
            switch (gradientDirection)
            {
                case GradientDirection.None: return new uint[] { color.Base, color.Base, color.Base, color.Base };
                case GradientDirection.Right: return new uint[] { color.TopGradient, color.BottomGradient, color.BottomGradient, color.TopGradient };
                case GradientDirection.Left: return new uint[] { color.BottomGradient, color.TopGradient, color.TopGradient, color.BottomGradient };
                case GradientDirection.Up: return new uint[] { color.BottomGradient, color.BottomGradient, color.TopGradient, color.TopGradient };
            }

            return new uint[] { color.TopGradient, color.TopGradient, color.BottomGradient, color.BottomGradient };
        }

        public static void DrawGradientFilledRect(Vector2 position, Vector2 size, PluginConfigColor color, ImDrawListPtr drawList)
        {
            var gradientDirection = ConfigurationManager.GetInstance().GradientDirection;
            DrawGradientFilledRect(position, size, color, drawList, gradientDirection);
        }

        public static void DrawGradientFilledRect(Vector2 position, Vector2 size, PluginConfigColor color, ImDrawListPtr drawList, GradientDirection gradientDirection = GradientDirection.Down)
        {
            var colorArray = ColorArray(color, gradientDirection);

            if (gradientDirection == GradientDirection.CenteredHorizonal)
            {
                var halfSize = new Vector2(size.X, size.Y / 2f);
                drawList.AddRectFilledMultiColor(
                    position, position + halfSize,
                    colorArray[0], colorArray[1], colorArray[2], colorArray[3]
                );

                var pos = position + new Vector2(0, halfSize.Y);
                drawList.AddRectFilledMultiColor(
                    pos, pos + halfSize,
                    colorArray[3], colorArray[2], colorArray[1], colorArray[0]
                );
            }
            else
            {
                drawList.AddRectFilledMultiColor(
                    position, position + size,
                    colorArray[0], colorArray[1], colorArray[2], colorArray[3]
                );
            }
        }

        public static void DrawOutlinedText(string text, Vector2 pos, float fontScale)
        {
            DrawOutlinedText(text, pos, Vector4.One, Vector4.UnitW, fontScale);
        }

        public static void DrawOutlinedText(string text, Vector2 pos, Vector4 color, Vector4 outlineColor, float fontScale)
        {
            DrawOutlinedText(text, pos, color, outlineColor, fontScale, FontsManager.Instance.DefaultFont);
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

        public static TextureWrap? GetIconAndTexCoordinates<T>(dynamic row, Vector2 size, out Vector2 uv0, out Vector2 uv1) where T : ExcelRow
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

        public static void DrawOvershield(float shield, Vector2 cursorPos, Vector2 barSize, float height, bool useRatioForHeight, PluginConfigColor color, ImDrawListPtr drawList)
        {
            if (shield == 0)
            {
                return;
            }

            var h = !useRatioForHeight ? barSize.Y / 100 * height : height;

            DrawGradientFilledRect(cursorPos, new Vector2(Math.Max(1, barSize.X * shield), h), color, drawList);
        }

        public static void DrawShield(float shield, float hp, Vector2 cursorPos, Vector2 barSize, float height, bool useRatioForHeight, PluginConfigColor color, ImDrawListPtr drawList)
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
            DrawGradientFilledRect(shieldStartPos, new Vector2(Math.Max(1, barSize.X * s), barSize.Y), color, drawList);


            // overshield
            shield = shield - s;
            if (shield <= 0)
            {
                return;
            }

            DrawGradientFilledRect(cursorPos, new Vector2(Math.Max(1, barSize.X * shield), h), color, drawList);
        }

        public static unsafe void ClipAround(AtkUnitBase* addon, string windowName, ImDrawListPtr drawList, Action<ImDrawListPtr, string> drawAction)
        {
            if (addon->IsVisible)
            {
                ClipAround(
                    new Vector2(addon->X + 5, addon->Y + 5),
                    new Vector2(
                        addon->X + addon->WindowNode->AtkResNode.Width - 5,
                        addon->Y + addon->WindowNode->AtkResNode.Height - 5
                    ),
                    windowName, drawList, drawAction
                );
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
