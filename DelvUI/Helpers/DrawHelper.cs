using DelvUI.Config;
using ImGuiNET;
using ImGuiScene;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Numerics;

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
            var gradientDirection = ConfigurationManager.Instance.GradientDirection;
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

        public static void DrawShadowText(string text, Vector2 pos, uint color, uint shadowColor, ImDrawListPtr drawList, int offset = 1)
        {
            // TODO: Add parameter to allow to choose a direction

            // Shadow
            drawList.AddText(new Vector2(pos.X + offset, pos.Y + offset), shadowColor, text);

            // Text
            drawList.AddText(new Vector2(pos.X, pos.Y), color, text);
        }

        public static void DrawIcon<T>(dynamic row, Vector2 position, Vector2 size, bool drawBorder, bool cropIcon, int stackCount = 1) where T : ExcelRow
        {
            TextureWrap texture = TexturesCache.Instance.GetTexture<T>(row, (uint)Math.Max(0, stackCount - 1));
            if (texture == null)
            {
                return;
            }

            (Vector2 uv0, Vector2 uv1) = GetTexCoordinates(texture, size, cropIcon);

            ImGui.SetCursorPos(position);
            ImGui.Image(texture.ImGuiHandle, size, uv0, uv1);

            if (drawBorder)
            {
                var drawList = ImGui.GetWindowDrawList();
                drawList.AddRect(position, position + size, 0xFF000000);
            }
        }

        public static void DrawIcon<T>(ImDrawListPtr drawList, dynamic row, Vector2 position, Vector2 size, bool drawBorder, bool cropIcon, int stackCount = 1) where T : ExcelRow
        {
            TextureWrap texture = TexturesCache.Instance.GetTexture<T>(row, (uint)Math.Max(0, stackCount - 1));
            if (texture == null)
            {
                return;
            }

            (Vector2 uv0, Vector2 uv1) = GetTexCoordinates(texture, size, cropIcon);

            drawList.AddImage(texture.ImGuiHandle, position, position + size, uv0, uv1);

            if (drawBorder)
            {
                drawList.AddRect(position, position + size, 0xFF000000);
            }
        }

        public static void DrawIcon(uint iconId, Vector2 position, Vector2 size, bool drawBorder, ImDrawListPtr drawList)
        {
            TextureWrap? texture = TexturesCache.Instance.GetTextureFromIconId(iconId);
            if (texture == null)
            {
                return;
            }

            drawList.AddImage(texture.ImGuiHandle, position, position + size, Vector2.Zero, Vector2.One);

            if (drawBorder)
            {
                drawList.AddRect(position, position + size, 0xFF000000);
            }
        }

        public static (Vector2, Vector2) GetTexCoordinates(TextureWrap texture, Vector2 size, bool cropIcon = true)
        {
            if (texture == null)
            {
                return (Vector2.Zero, Vector2.Zero);
            }

            // Status = 24x32, show from 2,7 until 22,26
            //show from 0,0 until 24,32 for uncropped status icon

            float uv0x = cropIcon ? 4f : 1f;
            float uv0y = cropIcon ? 14f : 1f;

            float uv1x = cropIcon ? 4f : 1f;
            float uv1y = cropIcon ? 12f : 1f;

            var uv0 = new Vector2(uv0x / texture.Width, uv0y / texture.Height);
            var uv1 = new Vector2(1f - uv1x / texture.Width, 1f - uv1y / texture.Height);

            return (uv0, uv1);
        }

        public static void DrawOvershield(float shield, Vector2 cursorPos, Vector2 barSize, float height, bool useRatioForHeight, PluginConfigColor color, ImDrawListPtr drawList)
        {
            if (shield == 0)
            {
                return;
            }

            var h = useRatioForHeight ? barSize.Y / 100 * height : height;

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
            var h = useRatioForHeight ? barSize.Y / 100 * Math.Min(100, height) : height;
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

        public static void DrawInWindow(string name, Vector2 pos, Vector2 size, bool needsInput, bool needsFocus, Action<ImDrawListPtr> drawAction)
        {
            ImGuiWindowFlags windowFlags =
                ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoBackground |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoResize;

            DrawInWindow(name, pos, size, needsInput, needsFocus, false, windowFlags, drawAction);
        }

        public static void DrawInWindow(
            string name,
            Vector2 pos,
            Vector2 size,
            bool needsInput,
            bool needsFocus,
            bool needsWindow,
            ImGuiWindowFlags windowFlags,
            Action<ImDrawListPtr> drawAction)
        {
            if (!needsInput)
            {
                windowFlags |= ImGuiWindowFlags.NoInputs;
            }

            if (!needsFocus)
            {
                windowFlags |= ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoBringToFrontOnFocus;
            }

            ClipRect? clipRect = ClipRectsHelper.Instance.GetClipRectForArea(pos, size);

            // no clipping needed
            if (!clipRect.HasValue)
            {
                ImDrawListPtr drawList = ImGui.GetWindowDrawList();

                if (!needsInput && !needsWindow)
                {
                    drawAction(drawList);
                    return;
                }

                ImGui.SetNextWindowPos(pos);
                ImGui.SetNextWindowSize(size);

                var begin = ImGui.Begin(name, windowFlags);
                if (!begin)
                {
                    ImGui.End();
                    return;
                }

                drawAction(drawList);

                ImGui.End();
            }

            // clip around game's window
            else
            {
                var flags = windowFlags;
                if (needsInput && clipRect.Value.Contains(ImGui.GetMousePos()))
                {
                    flags |= ImGuiWindowFlags.NoInputs;
                }

                var invertedClipRects = ClipRectsHelper.GetInvertedClipRects(clipRect.Value);
                for (int i = 0; i < invertedClipRects.Length; i++)
                {
                    ImGui.SetNextWindowPos(pos);
                    ImGui.SetNextWindowSize(size);

                    var begin = ImGui.Begin(name + "_" + i, flags);
                    if (!begin)
                    {
                        ImGui.End();
                        continue;
                    }

                    ImGui.PushClipRect(invertedClipRects[i].Min, invertedClipRects[i].Max, false);
                    drawAction(ImGui.GetWindowDrawList());
                    ImGui.PopClipRect();

                    ImGui.End();
                }
            }
        }

        public static bool DrawChangelogWindow(string changelog)
        {
            float height = ImGui.CalcTextSize(changelog).Y + 100;

            bool didClose = false;
            Vector2 size = new Vector2(500, Math.Min(height, 500));

            ImGui.SetNextWindowSize(size, ImGuiCond.Appearing);
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(10f / 255f, 10f / 255f, 10f / 255f, 0.95f));

            string title = "DelvUI Changelog v" + Plugin.Version + " ##DelvUI";
            if (!ImGui.Begin(title, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar))
            {
                ImGui.End();
                return didClose;
            }

            ImGui.BeginChild("##delvui_changelog", new Vector2(size.X - 10, size.Y - 80));
            ImGui.PushTextWrapPos(ImGui.GetCursorPosX() + size.X - 24);
            ImGui.TextWrapped(changelog);
            ImGui.EndChild();

            ImGui.SetCursorPos(new Vector2(10, size.Y - 40));
            if (ImGui.Button("Close", new Vector2(size.X - 20, 30)))
            {
                didClose = true;
            }

            ImGui.End();
            ImGui.PopStyleColor();

            return didClose;
        }
    }
}
