using Dalamud.Interface;
using Dalamud.Plugin;
using DelvUI.Config;
using ImGuiNET;
using System.Numerics;

namespace DelvUI.Interface
{
    public abstract class HudWindow
    {
        protected readonly PluginConfiguration PluginConfiguration;
        protected readonly DalamudPluginInterface PluginInterface;
        public bool IsVisible = true;
        private readonly Vector2 Center = new(CenterX, CenterY);

        protected HudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration)
        {
            PluginInterface = pluginInterface;
            PluginConfiguration = pluginConfiguration;
        }

        public abstract uint JobId { get; }

        protected static float CenterX => ImGui.GetMainViewport().Size.X / 2f;
        protected static float CenterY => ImGui.GetMainViewport().Size.Y / 2f;
        protected static int XOffset => 160;
        protected static int YOffset => 460;

        protected Vector2 BarSize { get; private set; }

        protected virtual void DrawPrimaryResourceBar()
        {
        }

        public void Draw()
        {
            return;
            if (!ShouldBeVisible() || PluginInterface.ClientState.LocalPlayer == null)
            {
                return;
            }

            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.SetNextWindowSize(ImGui.GetMainViewport().Size);

            bool begin = ImGui.Begin(
                "DelvUI",
                ImGuiWindowFlags.NoTitleBar
              | ImGuiWindowFlags.NoScrollbar
              | ImGuiWindowFlags.AlwaysAutoResize
              | ImGuiWindowFlags.NoBackground
              | ImGuiWindowFlags.NoInputs
              | ImGuiWindowFlags.NoBringToFrontOnFocus
            );

            if (!begin)
            {
                return;
            }

            DrawGenericElements();

            Draw(true);

            ImGui.End();
        }

        protected void DrawGenericElements()
        {

        }

        protected abstract void Draw(bool _);

        protected virtual unsafe bool ShouldBeVisible()
        {
            return false;
        }

    }
}
