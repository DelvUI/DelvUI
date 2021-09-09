using DelvUI.Config;
using DelvUI.Config.Attributes;
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
    [Section("Misc")]
    [SubSection("MP Ticker", 0)]
    public class MPTickerConfig : MovablePluginConfigObject
    {
        [Checkbox("Hide on Full MP")]
        [Order(20)]
        public bool HideOnFullMP = true;

        [Checkbox("Show Border")]
        [Order(25)]
        public bool ShowBorder = false;

        [ColorEdit4("Color")]
        [Order(30)]
        public PluginConfigColor Color = new PluginConfigColor(new(255f / 255f, 255f / 255f, 255f / 255f, 70f / 100f));

        public MPTickerConfig(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
        }

        public new static MPTickerConfig DefaultConfig()
        {
            var size = new Vector2(254, 4);
            var pos = new Vector2(0, HUDConstants.BaseHUDOffsetY + 27);

            return new MPTickerConfig(pos, size);
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
