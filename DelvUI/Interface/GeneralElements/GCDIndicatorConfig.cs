using DelvUI.Config;
using DelvUI.Config.Attributes;
using ImGuiNET;
using System;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Serializable]
    [Section("Misc")]
    [SubSection("GCD Indicator", 0)]
    public class GCDIndicatorConfig : MovablePluginConfigObject
    {
        [Checkbox("Always Show")]
        [Order(0)]
        public bool AlwaysShow = false;

        [Checkbox("Show Border")]
        [Order(5)]
        public bool ShowBorder = false;

        [Checkbox("Vertical Mode")]
        [Order(10)]
        public bool VerticalMode = false;

        [ColorEdit4("Color")]
        [Order(15)]
        public PluginConfigColor Color = new PluginConfigColor(new(255f / 255f, 255f / 255f, 255f / 255f, 70f / 100f));

        public GCDIndicatorConfig(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
        }

        public new static GCDIndicatorConfig DefaultConfig()
        {
            var size = new Vector2(254, 4);
            var pos = new Vector2(0, HUDConstants.BaseHUDOffsetY + 21);

            return new GCDIndicatorConfig(pos, size);
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
