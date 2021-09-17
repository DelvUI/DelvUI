using System;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Serializable]
    [Section("Misc")]
    [SubSection("Hide Options", 0)]
    public class HideHudConfig : PluginConfigObject
    {
        [Checkbox("Hide DelvUI outside of combat")]
        [Order(5)]
        public bool HideOutsideOfCombat = false;

        [Checkbox("Hide Default Job Gauges", isMonitored = true)]
        [Order(10)]
        public bool HideDefaultJobGauges = false;

        [Checkbox("Hide Default Castbar", isMonitored = true)]
        [Order(15)]
        public bool HideDefaultCastbar = false;

        [Checkbox("Enable Combat Hotbars", isMonitored = true)]
        [CollapseControl(20, 0)]
        public bool EnableCombatActionBars = false;

        [DynamicList("Hotbars Shown Only In Combat", "Hotbar 1", "Hotbar 2", "Hotbar 3", "Hotbar 4", "Hotbar 5", "Hotbar 6", "Hotbar 7", "Hotbar 8", "Hotbar 9", "Hotbar 10", isMonitored = true)]
        [CollapseWith(0, 0)]
        public List<string> CombatActionBars = new();

        public new static HideHudConfig DefaultConfig() { return new HideHudConfig(); }
    }
}
