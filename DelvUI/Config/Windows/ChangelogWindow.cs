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

            float height = ImGui.CalcTextSize(Changelog).Y + 100;
            Size = new Vector2(500, Math.Min(height, 500));
            SizeCondition = ImGuiCond.FirstUseEver;
        }

        public override void PreDraw()
        {
            if (ConfigurationManager.Instance.OverrideDalamudStyle)
            {
                ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(10f / 255f, 10f / 255f, 10f / 255f, 0.95f));
            }
        }

        public override void Draw()
        {
            Vector2 size = ImGui.GetWindowSize();
            ImGui.PushTextWrapPos(ImGui.GetCursorPosX() + size.X - 24);
            ImGui.TextWrapped(Changelog);
        }

        public override void PostDraw()
        {
            if (ConfigurationManager.Instance.OverrideDalamudStyle)
            {
                ImGui.PopStyleColor();
            }
        }
    }
}
