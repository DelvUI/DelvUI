using DelvUI.Config;
using ImGuiNET;
using System;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Serializable]
    public class GCDIndicatorConfig : MovablePluginConfigObject
    {
        public bool AlwaysShow = false;
        public bool ShowBorder = false;
        public bool VerticalMode = false;

        public PluginConfigColor Color = new PluginConfigColor(new(255f / 255f, 255f / 255f, 255f / 255f, 70f / 100f));

        public GCDIndicatorConfig(Vector2 position, Vector2 size) : base(position, size)
        {

        }

        public new bool Draw()
        {
            var changed = false;

            ImGui.Text("GCD Indicator");
            ImGui.BeginGroup();
            {
                changed |= base.Draw();

                changed |= ImGui.Checkbox("Always Show", ref AlwaysShow);
                changed |= ImGui.Checkbox("Show Border", ref ShowBorder);
                changed |= ImGui.Checkbox("Vertical Mode", ref VerticalMode);
                changed |= ColorEdit4("Color", ref Color);
            }

            return changed;
        }
    }
}
