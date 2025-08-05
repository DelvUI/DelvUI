using Dalamud.Interface.Windowing;
using DelvUI.Interface.GeneralElements;
using Dalamud.Bindings.ImGui;
using System.Numerics;

namespace DelvUI.Config.Windows
{
    public class GridWindow : Window
    {
        public GridWindow(string name) : base(name)
        {
            Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse;
            Size = new Vector2(300, 200);
        }

        public override void OnClose()
        {
            ConfigurationManager.Instance.LockHUD = true;
        }

        public override void PreDraw()
        {
            if (ConfigurationManager.Instance.OverrideDalamudStyle)
            {
                ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(10f / 255f, 10f / 255f, 10f / 255f, 0.95f));
            }

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
