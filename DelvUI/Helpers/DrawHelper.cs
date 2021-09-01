using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Actors;
using ImGuiNET;
using Actor = Dalamud.Game.ClientState.Actors.Types.Actor;


namespace DelvUI.Helpers
{
    public class DrawHelper
    {
        public static void DrawOutlinedText(string text, Vector2 pos) {
            DrawOutlinedText(text, pos, Vector4.One, new Vector4(0f, 0f, 0f, 1f));
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