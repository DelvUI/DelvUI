using Dalamud.Interface.Windowing;
using DelvUI.Helpers;
using Dalamud.Bindings.ImGui;
using System;
using System.Numerics;

namespace DelvUI.Config.Windows
{
    public class ChangelogWindow : Window
    {
        public string Changelog { get; set; }
        private bool _needsToSetSize = true;

        public bool AutoClose = false;
        private double _openTime = -1;

        private bool _popColors = false;

        public ChangelogWindow(string name, string changelog) : base(name)
        {
            Changelog = changelog;
        }

        public override void PreDraw()
        {
            if (ConfigurationManager.Instance.OverrideDalamudStyle)
            {
                ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(10f / 255f, 10f / 255f, 10f / 255f, 0.95f));
                _popColors = true;
            }

            if (_needsToSetSize)
            {
                float height = ImGui.CalcTextSize(Changelog).Y + 100;
                ImGui.SetNextWindowSize(new Vector2(500, Math.Min(height, 500)), ImGuiCond.FirstUseEver);
                _needsToSetSize = false;
            }
        }

        public override void Draw()
        {
            Vector2 size = ImGui.GetWindowSize();
            ImGui.PushTextWrapPos(ImGui.GetCursorPosX() + size.X - 24);
            ImGui.TextWrapped(Changelog);

            if (AutoClose &&
                _openTime > 0 && 
                ImGui.GetTime() - _openTime > 10)
            {
                IsOpen = false;
            }
        }

        public override void PostDraw()
        {
            if (_popColors)
            {
                ImGui.PopStyleColor();
                _popColors = false;
            }
        }

        public override void OnOpen()
        {
            _openTime = ImGui.GetTime();
        }

        public override void OnClose()
        {
            if (AutoClose && InputsHelper.Instance != null)
            {
                AutoClose = false;
                Plugin.LoadTime = ImGui.GetTime() - InputsHelper.InitializationDelay;
            }
        }
    }
}
