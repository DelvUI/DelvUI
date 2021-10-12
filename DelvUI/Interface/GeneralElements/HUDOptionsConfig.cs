using DelvUI.Config;
using DelvUI.Config.Attributes;
using System.Collections.Generic;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Disableable(false)]
    [Section("Misc")]
    [SubSection("HUD Options", 0)]
    public class HUDOptionsConfig : PluginConfigObject
    {
        [Checkbox("Global HUD Position")]
        [Order(5)]
        public bool UseGlobalHudShift = false;

        [DragInt2("Position", min = -4000, max = 4000)]
        [Order(80, collapseWith = nameof(UseGlobalHudShift))]
        public Vector2 HudOffset = new(0, 0);

        [Checkbox("Hide DelvUI outside of combat", separator = true)]
        [Order(5)]
        public bool HideOutsideOfCombat = false;

        [Checkbox("Hide DelvUI in Gold Saucer")]
        [Order(10)]
        public bool HideInGoldSaucer = false;

        [Checkbox("Hide only JobPack HUD outside of combat")]
        [Order(15)]
        public bool HideOnlyJobPackHudOutsideOfCombat = false;

        [Checkbox("Hide Default Job Gauges", isMonitored = true, spacing = true)]
        [Order(20)]
        public bool HideDefaultJobGauges = false;

        [Checkbox("Hide Default Castbar", isMonitored = true)]
        [Order(30)]
        public bool HideDefaultCastbar = false;        
        
        [Checkbox("Hide Default Pulltimer", isMonitored = true)]
        [Order(35)]
        public bool HideDefaultPulltimer = false;

        [Checkbox("Enable Combat Hotbars", isMonitored = true, spacing = true)]
        [Order(35)]
        public bool EnableCombatActionBars = false;

        [DynamicList("Hotbars Shown Only In Combat", "Hotbar 1", "Hotbar 2", "Hotbar 3", "Hotbar 4", "Hotbar 5", "Hotbar 6", "Hotbar 7", "Hotbar 8", "Hotbar 9", "Hotbar 10", isMonitored = true)]
        [Order(40, collapseWith = nameof(EnableCombatActionBars))]
        public List<string> CombatActionBars = new List<string>();

        public Vector2 CastBarOriginalPosition;
        public Vector2 PulltimerOriginalPosition;
        public Dictionary<string, Vector2> JobGaugeOriginalPosition = new Dictionary<string, Vector2>();

        public new static HUDOptionsConfig DefaultConfig() { return new HUDOptionsConfig(); }
    }
}
