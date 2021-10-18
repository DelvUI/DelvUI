using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using DelvUI.Config.Tree;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using System;
using System.Numerics;

namespace DelvUI.Config.Windows
{
    public class MainConfigWindow : Window
    {
        public BaseNode? node { get; set; }
        public Action? CloseAction;

        private float _alpha = 1f;
        private Vector2 _lastWindowPos = Vector2.Zero;

        public MainConfigWindow(string name) : base(name)
        {
            Flags = ImGuiWindowFlags.NoTitleBar |
                    ImGuiWindowFlags.NoScrollbar |
                    ImGuiWindowFlags.NoResize |
                    ImGuiWindowFlags.NoScrollWithMouse;

            Size = new Vector2(1050, 750);
        }

        public override void OnClose()
        {
            CloseAction?.Invoke();
        }

        private bool CheckWindowFocus()
        {
            Vector2 mousePos = ImGui.GetMousePos();
            Vector2 endPos = _lastWindowPos + Size!.Value;

            return mousePos.X >= _lastWindowPos.X && mousePos.X <= endPos.X &&
                   mousePos.Y >= _lastWindowPos.Y && mousePos.Y <= endPos.Y;
        }

        public override void PreDraw()
        {
            _alpha = 1;

            HUDOptionsConfig? config = ConfigurationManager.Instance.GetConfigObject<HUDOptionsConfig>();
            if (config?.DimConfigWindow == true)
            {
                _alpha = CheckWindowFocus() ? 1 : 0.5f;
            }

            ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0f / 255f, 0f / 255f, 0f / 255f, _alpha));
            ImGui.PushStyleColor(ImGuiCol.BorderShadow, new Vector4(0f / 255f, 0f / 255f, 0f / 255f, _alpha));
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(20f / 255f, 21f / 255f, 20f / 255f, _alpha));

            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 1);
        }

        public override void Draw()
        {
            _lastWindowPos = ImGui.GetWindowPos();

            ImGui.PopStyleColor(3);
            ImGui.PopStyleVar(2);

            node?.Draw(_alpha);
        }
    }
}
