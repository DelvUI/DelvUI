﻿using DelvUI.Config;
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

        [DragFloat("Background Alpha", min = 0, max = 1, velocity = .05f)]
        [Order(10)]
        public float BackgroundAlpha = 0.3f;

        [Checkbox("Show Center Lines")]
        [Order(15)]
        public bool ShowCenterLines = true;

        [Checkbox("Show Grid")]
        [Order(20)]
        public bool ShowGrid = true;

        [DragInt("Divisions Distance", min = 50, max = 500)]
        [Order(25)]
        public int GridDivisionsDistance = 50;

        [DragInt("Subdivision Count", min = 1, max = 10)]
        [Order(30)]
        public int GridSubdivisionCount = 4;

        [Checkbox("Show Anchor Points")]
        [Order(35)]
        public bool ShowAnchorPoints = true;
    }
}
