using DelvUI.Config;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DelvUI.Interface.GeneralElements
{
    [Serializable]
    public class MPTickerConfig : MovablePluginConfigObject
    {
        public bool HideOnFullMP = true;
        public bool ShowBorder = false;

        public PluginConfigColor Color = new PluginConfigColor(new(255f / 255f, 255f / 255f, 255f / 255f, 70f / 100f));

        public MPTickerConfig(Vector2 position, Vector2 size) : base(position, size)
        {

        }

        public new bool Draw()
        {
            var changed = false;

            ImGui.Text("MP Ticker");
            ImGui.BeginGroup();
            {
                changed |= base.Draw();

                changed |= ImGui.Checkbox("Hide on Full Mana", ref HideOnFullMP);
                changed |= ImGui.Checkbox("Show Border", ref ShowBorder);
                changed |= ColorEdit4("Color", ref Color);
            }

            return changed;
        }
    }
}
