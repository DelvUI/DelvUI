using Dalamud.Interface.Windowing;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using System;
using System.Numerics;

namespace DelvUI.Config.Windows
{
    public class GridWindow : Window
    {
        public GridWindow(string name) : base(name)
        {
            Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse;
            Size = new Vector2(340, 300);
        }

        public override void OnClose()
        {
            ConfigurationManager.Instance.LockHUD = true;
        }

        public override void PreDraw()
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(10f / 255f, 10f / 255f, 10f / 255f, 0.95f));
            ImGui.SetNextWindowFocus();
        }

        public override void Draw()
        {
            var configManager = ConfigurationManager.Instance;
            var node = configManager.GetConfigPageNode<GridConfig>();
            if (node == null)
            {
                return;
            }

            ImGui.PushItemWidth(150);
            bool changed = false;
            node.Draw(ref changed);

            ImGui.SetCursorPos(new Vector2(8, 260));
            if (ImGui.Button("Lock HUD", new Vector2(ImGui.GetWindowContentRegionWidth(), 30)))
            {
                configManager.LockHUD = true;
            }
        }

        public override void PostDraw()
        {
            ImGui.PopStyleColor();
        }
    }
}
