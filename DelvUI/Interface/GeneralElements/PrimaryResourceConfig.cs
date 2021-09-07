using DelvUI.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DelvUI.Interface.GeneralElements
{
    [Serializable]
    public class PrimaryResourceConfig : MovablePluginConfigObject
    {
        public bool ShowValue = true;
        public LabelConfig ValueLabelConfig;

        public bool ShowThresholdMarker = false;
        public int ThresholdMarkerValue = 7000;

        public PluginConfigColor Color = new PluginConfigColor(new(0 / 255f, 205f / 255f, 230f / 255f, 100f / 100f));

        public PrimaryResourceConfig(Vector2 position, Vector2 size, LabelConfig valueLabelConfig) : base(position, size)
        {
            ValueLabelConfig = valueLabelConfig;
        }
    }
}
