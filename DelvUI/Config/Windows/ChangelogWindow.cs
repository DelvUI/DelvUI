using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;

namespace DelvUI.Config.Windows
{
    public class ChangelogWindow : Window
    {
        public string Changelog { get; set; }

        public ChangelogWindow(string name, string changelog) : base(name)
        {
            Changelog = changelog;
            Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar;
        }

        public override void PreDraw()
        {
            float height = ImGui.CalcTextSize(Changelog).Y + 100;
            Size = new Vector2(500, Math.Min(height, 500));

            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(10f / 255f, 10f / 255f, 10f / 255f, 0.95f));
        }

        public override void Draw()
        {
            if (!Size.HasValue) { return; }
            Vector2 size = Size.Value;

            ImGui.BeginChild("##delvui_changelog", new Vector2(size.X - 10, size.Y - 80));
            ImGui.PushTextWrapPos(ImGui.GetCursorPosX() + size.X - 24);
            ImGui.TextWrapped(Changelog);
            ImGui.EndChild();

            ImGui.SetCursorPos(new Vector2(10, size.Y - 40));
            if (ImGui.Button("Close", new Vector2(size.X - 20, 30)))
            {
                IsOpen = false;
            }
        }

        public override void PostDraw()
        {
            ImGui.PopStyleColor();
        }
    }
}
