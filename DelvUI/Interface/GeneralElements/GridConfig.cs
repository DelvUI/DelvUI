using DelvUI.Config;
using DelvUI.Config.Attributes;

namespace DelvUI.Interface.GeneralElements
{
    [Portable(false)]
    [Section("Misc")]
    [SubSection("Grid", 0)]
    public class GridConfig : PluginConfigObject
    {
        public new static GridConfig DefaultConfig()
        {
            var config = new GridConfig();
            config.Enabled = false;

            return config;
        }

        [DragFloat("Background Alpha", min = 0, max = 1, velocity = .05f, spacing = true)]
        [Order(10)]
        public float BackgroundAlpha = 0.3f;

        [Checkbox("Show Center Lines")]
        [Order(15)]
        public bool ShowCenterLines = true;
        [Checkbox("Show Anchor Points")]
        [Order(20)]

        public bool ShowAnchorPoints = true;
        [Checkbox("Grid Divisions", spacing = true)]
        [CollapseControl(25, 0)]
        public bool ShowGrid = true;

        [DragInt("Divisions Distance", min = 50, max = 500)]
        [CollapseWith(0, 0)]
        public int GridDivisionsDistance = 50;

        [DragInt("Subdivision Count", min = 1, max = 10)]
        [CollapseWith(5, 0)]
        public int GridSubdivisionCount = 4;
    }
}
