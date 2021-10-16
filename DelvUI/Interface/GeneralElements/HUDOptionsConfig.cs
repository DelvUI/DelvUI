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
        [Order(6, collapseWith = nameof(UseGlobalHudShift))]
        public Vector2 HudOffset = new(0, 0);

        [Checkbox("Dim DelvUI's settings window when not focused")]
        [Order(10)]
        public bool DimConfigWindow = false;

        [Checkbox("Mouseover", separator = true)]
        [Order(15)]
        public bool MouseoverEnabled = true;

        [Checkbox("Automatic Mode", help =
            "When enabled: All your actions will automatically assume mouseover when your cursor is on top of a unit frame.\n" +
            "Mouseover macros or other mouseover plugins are not necessary and WON'T WORK in this mode!\n\n" +
            "When disabled: DelvUI unit frames will behave like the game's ones.\n" +
            "You'll need to use mouseover macros or other mouseover related plugins in this mode.")]
        [Order(16, collapseWith = nameof(MouseoverEnabled))]
        public bool MouseoverAutomaticMode = true;

        [Checkbox("Hide DelvUI outside of combat", separator = true, help = "Show in Duty-option available once enabed.")]
        [Order(20)]
        public bool HideOutsideOfCombat = false;

        [Checkbox("Show in duty")]
        [Order(21, collapseWith = nameof(HideOutsideOfCombat))]
        public bool ShowInDuty = false;

        [Checkbox("Hide DelvUI in Gold Saucer")]
        [Order(25)]
        public bool HideInGoldSaucer = false;

        [Checkbox("Hide only JobPack HUD outside of combat")]
        [Order(30)]
        public bool HideOnlyJobPackHudOutsideOfCombat = false;

        [Checkbox("Hide Default Job Gauges", isMonitored = true, spacing = true)]
        [Order(35)]
        public bool HideDefaultJobGauges = false;

        [Checkbox("Hide Default Castbar", isMonitored = true)]
        [Order(40)]
        public bool HideDefaultCastbar = false;

        [Checkbox("Hide Default Pulltimer", isMonitored = true)]
        [Order(45)]
        public bool HideDefaultPulltimer = false;

        [Checkbox("Enable Combat Hotbars", isMonitored = true, separator = true)]
        [Order(200)]
        public bool EnableCombatActionBars = false;

        [DynamicList("Hotbars Shown Only In Combat", "Hotbar 1", "Hotbar 2", "Hotbar 3", "Hotbar 4", "Hotbar 5", "Hotbar 6", "Hotbar 7", "Hotbar 8", "Hotbar 9", "Hotbar 10", isMonitored = true)]
        [Order(201, collapseWith = nameof(EnableCombatActionBars))]
        public List<string> CombatActionBars = new List<string>();

        public Vector2 CastBarOriginalPosition;
        public Vector2 PulltimerOriginalPosition;
        public Dictionary<string, Vector2> JobGaugeOriginalPosition = new Dictionary<string, Vector2>();

        public new static HUDOptionsConfig DefaultConfig() { return new HUDOptionsConfig(); }
    }
}
