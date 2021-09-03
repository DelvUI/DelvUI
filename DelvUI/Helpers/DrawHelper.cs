using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Actors;
using ImGuiNET;
using Actor = Dalamud.Game.ClientState.Actors.Types.Actor;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;


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
        
        public static void DrawOutlinedText(string text, Vector2 pos) {
            DrawOutlinedText(text, pos, Vector4.One, Vector4.UnitW);
        }
        
        public static void DrawOutlinedText(string text, Vector2 pos, Vector4 color, Vector4 outlineColor) {
            ImGui.SetCursorPos(new Vector2(pos.X - 1, pos.Y + 1));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X, pos.Y+1));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X+1, pos.Y+1));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X-1, pos.Y));
            ImGui.TextColored(outlineColor, text);

            ImGui.SetCursorPos(new Vector2(pos.X+1, pos.Y));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X-1, pos.Y-1));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X, pos.Y-1));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X+1, pos.Y-1));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X, pos.Y));
            ImGui.TextColored(color, text);
        }

        public static void DrawIcon<T>(dynamic row, Vector2 size, Vector2 position, bool drawBorder) where T : ExcelRow
        {
            // Status = 24x32, show from 2,7 until 22,26
            var texture = TexturesCache.Instance.GetTexture<T>(row);
            if (texture == null) return;
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

        public static void DrawShield(float shield, Vector2 cursorPos, Vector2 barSize, float height, bool useRatioForHeight, Dictionary<string, uint> color)
        {
            if (shield == 0) return;

            var h = useRatioForHeight ? barSize.Y / 100 * height : height;

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(Math.Max(1, barSize.X * shield), h),
                color["gradientLeft"], color["gradientRight"], color["gradientRight"], color["gradientLeft"]
            );
        }

        public static void DrawShield(float shield, float hp, Vector2 cursorPos, Vector2 barSize, float height, bool useRatioForHeight, Dictionary<string, uint> color)
        {
            if (shield == 0) return;
            if (hp == 1)
            {
                DrawShield(shield, cursorPos, barSize, height, useRatioForHeight, color);
                return;
            }

            var h = useRatioForHeight ? barSize.Y / 100 * Math.Min(100, height) : height;
            var drawList = ImGui.GetWindowDrawList();

            // hp portion
            var missingHPRatio = 1 - hp;
            var s = Math.Min(shield, missingHPRatio);
            var shieldStartPos = cursorPos + new Vector2(Math.Max(1, barSize.X * hp), 0);
            drawList.AddRectFilledMultiColor(
                shieldStartPos, shieldStartPos + new Vector2(Math.Max(1, barSize.X * s), barSize.Y),
                color["gradientLeft"], color["gradientRight"], color["gradientRight"], color["gradientLeft"]
            );

            // overshield
            shield = shield - s;
            if (shield <= 0) return;

            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(Math.Max(1, barSize.X * shield), h),
                color["gradientLeft"], color["gradientRight"], color["gradientRight"], color["gradientLeft"]
            );
        }
    }
}