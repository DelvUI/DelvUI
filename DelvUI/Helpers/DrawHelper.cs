using System.Numerics;
using ImGuiNET;

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
    }
}