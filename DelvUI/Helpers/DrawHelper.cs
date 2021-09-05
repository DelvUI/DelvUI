using DelvUI.Config;
using ImGuiNET;
using Lumina.Excel;
using System.Numerics;

namespace DelvUI.Helpers
{
    public static class DrawHelper
    {
        public static void DrawOutlinedText(string text, Vector2 pos, float fontScale, PluginConfiguration pluginConfiguration)
        {
            DrawOutlinedText(text, pos, Vector4.One, Vector4.UnitW, fontScale, pluginConfiguration);
        }

        public static void DrawOutlinedText(string text, Vector2 pos, Vector4 color, Vector4 outlineColor, float fontScale, PluginConfiguration pluginConfiguration)
        {
            DrawOutlinedText(text, pos, color, outlineColor, fontScale, pluginConfiguration.BigNoodleTooFont);
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

        public static void DrawIcon<T>(dynamic row, Vector2 size, Vector2 position, bool drawBorder) where T : ExcelRow
        {
            // Status = 24x32, show from 2,7 until 22,26
            var texture = TexturesCache.Instance.GetTexture<T>(row);

            if (texture == null)
            {
                return;
            }

            var uv0 = new Vector2(4f / texture.Width, 14f / texture.Height);
            var uv1 = new Vector2(1f - 4f / texture.Width, 1f - 12f / texture.Height);

            ImGui.SetCursorPos(position);
            ImGui.Image(texture.ImGuiHandle, size, uv0, uv1);

            if (drawBorder)
            {
                var drawList = ImGui.GetWindowDrawList();
                drawList.AddRect(position, position + size, 0xFF000000);
            }
        }
    }
}
