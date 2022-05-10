using DelvUI.Config;
using DelvUI.Config.Attributes;
using System;
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

        [Checkbox("Hide DelvUI outside of combat", isMonitored = true, separator = true, help = "Show in Duty and Show on Weapon Drawn-options available once enabled.")]
        [Order(17)]
        public bool HideOutsideOfCombat = false;

        [Checkbox("Hide Player Frame even when not at full HP outside of combat.")]
        [Order(18, collapseWith = nameof(HideOutsideOfCombat))]
        public bool AlwaysHidePlayerFrameWhenDelvUIHidden = false;               

        [Checkbox("Show in duty" + "##HideOutsideCombat")]
        [Order(21, collapseWith = nameof(HideOutsideOfCombat))]
        public bool ShowDelvUIFramesInDuty = false;

        [Checkbox("Show on Weapon Drawn" + "##HideOutsideCombat")]
        [Order(22, collapseWith = nameof(HideOutsideOfCombat))]
        public bool ShowDelvUIFramesOnWeaponDrawn = false;

        [Checkbox("Show when Crafting" + "##HideOutsideCombat")]
        [Order(23, collapseWith = nameof(HideOutsideOfCombat))]
        public bool ShowDelvUIFramesWhenCrafting = false;

        [Checkbox("Hide DelvUI in Gold Saucer")]
        [Order(25)]
        public bool HideInGoldSaucer = false;

        [Checkbox("Hide Player Frame at full HP")]
        [Order(26)]
        public bool HidePlayerFrameAtFullHP = false;

        [Checkbox("Hide only JobPack HUD outside of combat")]
        [Order(30)]
        public bool HideOnlyJobPackHudOutsideOfCombat = false;

        [Checkbox("Show in duty" + "##HideOnlyJobPack")]
        [Order(31, collapseWith = nameof(HideOnlyJobPackHudOutsideOfCombat))]
        public bool ShowJobPackInDuty = false;

        [Checkbox("Show on Weapon Drawn" + "##HideOnlyJobPack")]
        [Order(322, collapseWith = nameof(HideOnlyJobPackHudOutsideOfCombat))]
        public bool ShowJobPackOnWeaponDrawn = false;

        [Checkbox("Automatically disable HUD elements preview", help = "Enabling this will make it so all HUD elements preview modes are disabled when DelvUI's setting window is closed.")]
        [Order(35)]
        public bool AutomaticPreviewDisabling = true;

        [Checkbox("Hide Default Job Gauges", isMonitored = true, spacing = true)]
        [Order(40)]
        public bool HideDefaultJobGauges = false;

        [Checkbox("Hide Default Castbar", isMonitored = true)]
        [Order(45)]
        public bool HideDefaultCastbar = false;

        [Checkbox("Hide Default Pulltimer", isMonitored = true)]
        [Order(50)]
        public bool HideDefaultPulltimer = false;

        [Checkbox("Enable Combat Hotbars", isMonitored = true, separator = true, help =
            "Show in Duty, Show on Weapon Drawn and Use with Cross Hotbar-options available once enabled.")]
        [Order(200)]
        public bool EnableCombatActionBars = false;

        [Checkbox("Show in duty" + "##CombatActionBars")]
        [Order(201, collapseWith = nameof(EnableCombatActionBars))]
        public bool ShowCombatActionBarsInDuty = false;

        [Checkbox("Show on Weapon Drawn" + "##CombatActionBars")]
        [Order(202, collapseWith = nameof(EnableCombatActionBars))]
        public bool ShowCombatActionBarsOnWeaponDrawn = false;

        [Checkbox("Use with Cross Hotbar", isMonitored = true, help = "Show in Duty and Show on Weapon Drawn will apply to Cross Hotbar instead when enabled.")]
        [Order(203, collapseWith = nameof(EnableCombatActionBars))]
        public bool CombatActionBarsWithCrossHotbar = false;

        [DynamicList("Hotbars Shown Only In Combat", "Hotbar 1", "Hotbar 2", "Hotbar 3", "Hotbar 4", "Hotbar 5", "Hotbar 6", "Hotbar 7", "Hotbar 8", "Hotbar 9", "Hotbar 10", isMonitored = true)]
        [Order(204, collapseWith = nameof(EnableCombatActionBars))]
        public List<string> CombatActionBars = new();

        // saves original positions for all 4 layouts
        public Vector2[] CastBarOriginalPositions = new Vector2[] { Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero };
        public Vector2[] PulltimerOriginalPositions = new Vector2[] { Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero };
        public Dictionary<string, Vector2>[] JobGaugeOriginalPositions = new Dictionary<string, Vector2>[] { new(), new(), new(), new() };

        public new static HUDOptionsConfig DefaultConfig() => new();
    }

    public class HUDOptionsConfigConverter : PluginConfigObjectConverter
    {
        public HUDOptionsConfigConverter()
        {
            Func<Vector2, Vector2[]> func = (value) =>
            {
                Vector2[] array = new Vector2[4];
                for (int i = 0; i < 4; i++)
                {
                    array[i] = value;
                }

                return array;
            };

            TypeToClassFieldConverter<Vector2, Vector2[]> castBar = new TypeToClassFieldConverter<Vector2, Vector2[]>(
                "CastBarOriginalPositions",
                new Vector2[] { Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero },
                func
            );

            TypeToClassFieldConverter<Vector2, Vector2[]> pullTimer = new TypeToClassFieldConverter<Vector2, Vector2[]>(
                "PulltimerOriginalPositions",
                new Vector2[] { Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero },
                func
            );

            NewClassFieldConverter<Dictionary<string, Vector2>, Dictionary<string, Vector2>[]> jobGauge =
                new NewClassFieldConverter<Dictionary<string, Vector2>, Dictionary<string, Vector2>[]>(
                    "JobGaugeOriginalPositions",
                    new Dictionary<string, Vector2>[] { new(), new(), new(), new() },
                    (oldValue) =>
                    {
                        Dictionary<string, Vector2>[] array = new Dictionary<string, Vector2>[4];
                        for (int i = 0; i < 4; i++)
                        {
                            array[i] = oldValue;
                        }

                        return array;
                    });

            FieldConvertersMap.Add("CastBarOriginalPosition", castBar);
            FieldConvertersMap.Add("PulltimerOriginalPosition", pullTimer);
            FieldConvertersMap.Add("JobGaugeOriginalPosition", jobGauge);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(HUDOptionsConfig);
        }
    }
}
