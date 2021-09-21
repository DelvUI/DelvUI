using System;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Disableable(false)]
    [Section("Misc")]
    [SubSection("Hide Options", 0)]
    public class HideHudConfig : PluginConfigObject
    {
        [Checkbox("Hide DelvUI outside of combat", spacing = true)]
        [Order(5)]
        public bool HideOutsideOfCombat = false;

        [Checkbox("Hide DelvUI in Gold Saucer")]
        [Order(10)]
        public bool HideInGoldSaucer = true;

        [Checkbox("Hide only JobPack HUD outside of combat")]
        [Order(15)]
        public bool HideOnlyJobPackHudOutsideOfCombat = false;

        [Checkbox("Hide Default Job Gauges", isMonitored = true, spacing = true)]
        [CollapseControl(20, 0)]
        public bool HideDefaultJobGauges = false;

        [Checkbox("Disable Job Gauge Sounds", isMonitored = true)]
        [CollapseWith(0, 0)]
        public bool DisableJobGaugeSounds = false;

        [Checkbox("Hide Default Castbar", isMonitored = true)]
        [Order(25)]
        public bool HideDefaultCastbar = false;

        [Checkbox("Enable Combat Hotbars", isMonitored = true, spacing = true)]
        [CollapseControl(30, 1)]
        public bool EnableCombatActionBars = false;

        [DynamicList("Hotbars Shown Only In Combat", "Hotbar 1", "Hotbar 2", "Hotbar 3", "Hotbar 4", "Hotbar 5", "Hotbar 6", "Hotbar 7", "Hotbar 8", "Hotbar 9", "Hotbar 10", isMonitored = true)]
        [CollapseWith(0, 1)]
        public List<string> CombatActionBars = new();

        public new static HideHudConfig DefaultConfig() { return new HideHudConfig(); }
    }
}
