﻿using DelvUI.Config;
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

        [Checkbox("Automatically disable HUD elements preview", help = "If enabled, all HUD elements preview modes are disabled when DelvUI's setting window is closed.")]
        [Order(11)]
        public bool AutomaticPreviewDisabling = true;

        [Checkbox("Use DelvUI style", help = "If enabled, DelvUI will use its own style for the setting window instead of the general Dalamud style.")]
        [Order(12)]
        public bool OverrideDalamudStyle = true;

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

        //[Checkbox("Support Special Mouse Clicks", isMonitored = true, spacing = true, help =
        //    "When enabled DelvUI will attempt to support special mouse binds (mousewheel, M4, M5, etc) when the cursor\n" +
        //    "is hovering on top of DelvUI's unit frames.\n\n" +
        //    "If you don't have actions bound to these mouse buttons, it is adviced that you leave this feature disabled.\n\n" +
        //    "This feature can cause some issues such as click inputs not working in DelvUI, or through out the game.\n" +
        //    "If you run into these kinds of issues, you can try reloading DelvUI, restarting the game, or disabling this feature.")]
        //[Order(17)]
        public bool InputsProxyEnabled = false;

        [Checkbox("Hide Default Job Gauges", isMonitored = true, separator = true)]
        [Order(40)]
        public bool HideDefaultJobGauges = false;

        [Checkbox("Hide Default Castbar", isMonitored = true)]
        [Order(45)]
        public bool HideDefaultCastbar = false;

        [Checkbox("Hide Default Pulltimer", isMonitored = true)]
        [Order(50)]
        public bool HideDefaultPulltimer = false;

        [Checkbox("Use Regional Number Format", help = "When enabled, DelvUI will use your system's regional format settings when showing numbers.\nWhen disabled, DelvUI will use English number formatting instead.", separator = true)]
        [Order(60)]
        public bool UseRegionalNumberFormats = true;

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
