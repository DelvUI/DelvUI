﻿using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Interface.GeneralElements;
using DelvUI.Interface.StatusEffects;
using Newtonsoft.Json;
using System;
using System.Numerics;

namespace DelvUI.Interface.Party
{
    [Section("Party Frames")]
    [SubSection("General", 0)]
    public class PartyFramesConfig : MovablePluginConfigObject
    {
        public new static PartyFramesConfig DefaultConfig() { return new PartyFramesConfig(); }

        [Checkbox("Lock")]
        [Order(3)]

        public bool Lock = true;
        [Checkbox("Preview", isMonitored = true)]
        [Order(4)]
        public bool Preview = false;

        [DragInt2("Size", isMonitored = true)]
        [Order(30)]
        public Vector2 Size = new Vector2(650, 150);

        [Anchor("Bars Anchor", isMonitored = true)]
        [Order(40)]
        public DrawAnchor BarsAnchor = DrawAnchor.TopLeft;

        [Checkbox("Show When Solo", separator = true)]
        [Order(50)]
        public bool ShowWhenSolo = false;

        [Checkbox("Show Chocobo")]
        [Order(55)]
        public bool ShowChocobo = true;

        [Checkbox("Fill Rows First", isMonitored = true, separator = true)]
        [Order(60)]
        public bool FillRowsFirst = true;

        [Combo("Sorting Mode",
            "Tank => DPS => Healer",
            "Tank => Healer => DPS",
            "DPS => Tank => Healer",
            "DPS => Healer => Tank",
            "Healer => Tank => DPS",
            "Healer => DPS => Tank",
            isMonitored = true
        )]
        [Order(65)]
        public PartySortingMode SortingMode = PartySortingMode.Tank_Healer_DPS;
    }

    [Disableable(false)]
    [Section("Party Frames")]
    [SubSection("Health Bar", 0)]
    public class PartyFramesHealthBarsConfig : PluginConfigObject
    {
        public new static PartyFramesHealthBarsConfig DefaultConfig() { return new PartyFramesHealthBarsConfig(); }

        [DragInt2("Size", isMonitored = true)]
        [Order(30)]
        public Vector2 Size = new Vector2(180, 80);

        [DragInt2("Padding", isMonitored = true)]
        [Order(35)]
        public Vector2 Padding = new Vector2(1, 1);

        [NestedConfig("Name Label", 40)]
        public EditableLabelConfig NameLabelConfig = new EditableLabelConfig(Vector2.Zero, "[name:first-initial]. [name:last-initial].", DrawAnchor.Center, DrawAnchor.Center);

        [NestedConfig("Colors", 45)]
        public PartyFramesColorsConfig ColorsConfig = new PartyFramesColorsConfig();

        [NestedConfig("Shield", 50)]
        public ShieldConfig ShieldConfig = new ShieldConfig();

        [NestedConfig("Change Alpha Based on Range", 55)]
        public PartyFramesRangeConfig RangeConfig = new PartyFramesRangeConfig();
    }

    [Disableable(false)]
    [Portable(false)]
    public class PartyFramesColorsConfig : PluginConfigObject
    {
        [Checkbox("Use Role Colors", isMonitored = true)]
        [Order(5)]
        public bool UseRoleColors = false;

        [ColorEdit4("Tank Role Color")]
        [Order(10, collapseWith = nameof(UseRoleColors))]
        public PluginConfigColor TankRoleColor = new PluginConfigColor(new Vector4(21f / 255f, 28f / 255f, 100f / 255f, 100f / 100f));

        [ColorEdit4("DPS Role Color")]
        [Order(15, collapseWith = nameof(UseRoleColors))]
        public PluginConfigColor DPSRoleColor = new PluginConfigColor(new Vector4(153f / 255f, 23f / 255f, 23f / 255f, 100f / 100f));

        [ColorEdit4("Healer Role Color")]
        [Order(20, collapseWith = nameof(UseRoleColors))]
        public PluginConfigColor HealerRoleColor = new PluginConfigColor(new Vector4(46f / 255f, 125f / 255f, 50f / 255f, 100f / 100f));

        [ColorEdit4("Generic Role Color")]
        [Order(25, collapseWith = nameof(UseRoleColors))]
        public PluginConfigColor GenericRoleColor = new PluginConfigColor(new Vector4(0f / 255f, 145f / 255f, 6f / 255f, 100f / 100f));

        [Checkbox("Highlight When Hovering With Cursor")]
        [Order(30)]
        public bool ShowHighlight = true;

        [ColorEdit4("Highlight Color")]
        [Order(35, collapseWith = nameof(ShowHighlight))]
        public PluginConfigColor HighlightColor = new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 5f / 100f));

        [ColorEdit4("Background Color")]
        [Order(40, collapseWith = nameof(ShowHighlight))]
        public PluginConfigColor BackgroundColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 70f / 100f));

        [ColorEdit4("Out of Reach Color")]
        [Order(45, collapseWith = nameof(ShowHighlight))]
        public PluginConfigColor UnreachableColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 30f / 100f));
    }

    [Portable(false)]
    public class PartyFramesRangeConfig : PluginConfigObject
    {
        [DragInt("Range (yalms)", min = 1, max = 500)]
        [Order(5)]
        public int Range = 30;

        [DragFloat("Alpha", min = 1, max = 100)]
        [Order(10)]
        public float Alpha = 25;

        [Checkbox("Use Additional Range Check")]
        [Order(15)]
        public bool UseAdditionalRangeCheck = false;

        [DragInt("Additional Range (yalms)", min = 1, max = 500)]
        [Order(20, collapseWith = nameof(UseAdditionalRangeCheck))]
        public int AdditionalRange = 15;

        [DragFloat("Additional Alpha", min = 1, max = 100)]
        [Order(25, collapseWith = nameof(UseAdditionalRangeCheck))]
        public float AdditionalAlpha = 60;

        public float AlphaForDistance(int distance)
        {
            if (!Enabled)
            {
                return 100f;
            }

            if (!UseAdditionalRangeCheck)
            {
                return distance > Range ? Alpha : 100f;
            }

            if (Range > AdditionalRange)
            {
                return distance > Range ? Alpha : (distance > AdditionalRange ? AdditionalAlpha : 100f);
            }

            return distance > AdditionalRange ? AdditionalAlpha : (distance > Range ? Alpha : 100f);
        }
    }

    [Section("Party Frames")]
    [SubSection("Mana Bar", 0)]
    public class PartyFramesManaBarConfig : MovablePluginConfigObject
    {
        public new static PartyFramesManaBarConfig DefaultConfig()
        {
            var config = new PartyFramesManaBarConfig();
            config.ValueLabelConfig.Enabled = false;
            return config;
        }

        [DragInt2("Size", min = 1, max = 1000)]
        [Order(20)]
        public Vector2 Size = new(180, 6);

        [Anchor("Health Bar Anchor")]
        [Order(25)]
        public DrawAnchor HealthBarAnchor = DrawAnchor.BottomLeft;

        [Anchor("Anchor")]
        [Order(30)]
        public DrawAnchor Anchor = DrawAnchor.BottomLeft;

        [Checkbox("Show Only For Healers")]
        [Order(35)]
        public bool ShowOnlyForHealers = true;

        [ColorEdit4("Color")]
        [Order(40)]
        public PluginConfigColor Color = new PluginConfigColor(new(0 / 255f, 162f / 255f, 252f / 255f, 100f / 100f));

        [ColorEdit4("Background Color")]
        [Order(45)]
        public PluginConfigColor BackgroundColor = new PluginConfigColor(new(0 / 255f, 20f / 255f, 100f / 255f, 100f / 100f));

        [NestedConfig("Label", 50)]
        public EditableLabelConfig ValueLabelConfig = new EditableLabelConfig(Vector2.Zero, "[mana:current-short]", DrawAnchor.Center, DrawAnchor.Center);
    }

    [Section("Party Frames")]
    [SubSection("Role-Job Icon", 0)]
    public class PartyFramesRoleIconConfig : MovablePluginConfigObject
    {
        public new static PartyFramesRoleIconConfig DefaultConfig() { return new PartyFramesRoleIconConfig(); }

        [DragInt2("Size", min = 1, max = 1000)]
        [Order(20)]
        public Vector2 Size = new(20, 20);

        [Anchor("Health Bar Anchor")]
        [Order(25)]
        public DrawAnchor HealthBarAnchor = DrawAnchor.TopLeft;

        [Anchor("Anchor")]
        [Order(30)]
        public DrawAnchor Anchor = DrawAnchor.TopLeft;

        [Combo("Style", "Style 1", "Style 2")]
        [Order(35)]
        public int Style = 0;

        [Checkbox("Use Role Icons")]
        [Order(40)]
        public bool UseRoleIcons = false;

        [Checkbox("Use Specific DPS Role Icons")]
        [Order(45, collapseWith = nameof(UseRoleIcons))]
        public bool UseSpecificDPSRoleIcons = false;
    }

    [Section("Party Frames")]
    [SubSection("Buffs", 0)]
    public class PartyFramesBuffsConfig : PartyFramesStatusEffectsListConfig
    {
        public new static PartyFramesBuffsConfig DefaultConfig()
        {
            var durationConfig = new LabelConfig(new Vector2(0, -4), "", DrawAnchor.Bottom, DrawAnchor.Center);
            var stacksConfig = new LabelConfig(new Vector2(-3, 4), "", DrawAnchor.TopRight, DrawAnchor.Center);
            stacksConfig.Color = new(Vector4.UnitW);
            stacksConfig.OutlineColor = new(Vector4.One);

            var iconConfig = new StatusEffectIconConfig(durationConfig, stacksConfig);
            iconConfig.DispellableBorderConfig.Enabled = false;
            iconConfig.Size = new Vector2(24, 24);

            var pos = new Vector2(-2, 2);
            var size = new Vector2(iconConfig.Size.X * 4 + 6, iconConfig.Size.Y);

            var config = new PartyFramesBuffsConfig(DrawAnchor.TopRight, pos, size, true, false, false, GrowthDirections.Left | GrowthDirections.Down, iconConfig);
            config.Limit = 4;

            return config;
        }

        public PartyFramesBuffsConfig(DrawAnchor anchor, Vector2 position, Vector2 size, bool showBuffs, bool showDebuffs, bool showPermanentEffects,
            GrowthDirections growthDirections, StatusEffectIconConfig iconConfig)
            : base(anchor, position, size, showBuffs, showDebuffs, showPermanentEffects, growthDirections, iconConfig)
        {
        }
    }

    [Section("Party Frames")]
    [SubSection("Debuffs", 0)]
    public class PartyFramesDebuffsConfig : PartyFramesStatusEffectsListConfig
    {
        public new static PartyFramesDebuffsConfig DefaultConfig()
        {
            var durationConfig = new LabelConfig(new Vector2(0, -4), "", DrawAnchor.Bottom, DrawAnchor.Center);
            var stacksConfig = new LabelConfig(new Vector2(-3, 4), "", DrawAnchor.TopRight, DrawAnchor.Center);
            stacksConfig.Color = new(Vector4.UnitW);
            stacksConfig.OutlineColor = new(Vector4.One);

            var iconConfig = new StatusEffectIconConfig(durationConfig, stacksConfig);
            iconConfig.Size = new Vector2(24, 24);

            var pos = new Vector2(-2, -2);
            var size = new Vector2(iconConfig.Size.X * 4 + 6, iconConfig.Size.Y);

            var config = new PartyFramesDebuffsConfig(DrawAnchor.BottomRight, pos, size, false, true, false, GrowthDirections.Left | GrowthDirections.Up, iconConfig);
            config.Limit = 4;

            return config;
        }

        public PartyFramesDebuffsConfig(DrawAnchor anchor, Vector2 position, Vector2 size, bool showBuffs, bool showDebuffs, bool showPermanentEffects,
            GrowthDirections growthDirections, StatusEffectIconConfig iconConfig)
            : base(anchor, position, size, showBuffs, showDebuffs, showPermanentEffects, growthDirections, iconConfig)
        {
        }
    }

    public class PartyFramesStatusEffectsListConfig : StatusEffectsListConfig
    {
        [Anchor("Health Bar Anchor")]
        [Order(4)]
        public DrawAnchor HealthBarAnchor = DrawAnchor.BottomLeft;

        public PartyFramesStatusEffectsListConfig(DrawAnchor anchor, Vector2 position, Vector2 size, bool showBuffs, bool showDebuffs, bool showPermanentEffects,
            GrowthDirections growthDirections, StatusEffectIconConfig iconConfig)
            : base(position, size, showBuffs, showDebuffs, showPermanentEffects, growthDirections, iconConfig)
        {
            HealthBarAnchor = anchor;
        }
    }

    [Section("Party Frames")]
    [SubSection("Castbars", 0)]
    public class PartyFramesCastbarConfig : CastbarConfig
    {
        public new static PartyFramesCastbarConfig DefaultConfig()
        {
            var size = new Vector2(180, 10);
            var pos = Vector2.Zero;

            var castNameConfig = new LabelConfig(new Vector2(5, 0), "", DrawAnchor.Left, DrawAnchor.Left);
            var castTimeConfig = new LabelConfig(new Vector2(-5, 0), "", DrawAnchor.Right, DrawAnchor.Right);
            castTimeConfig.Enabled = false;

            var config = new PartyFramesCastbarConfig(pos, size, castNameConfig, castTimeConfig);
            config.HealthBarAnchor = DrawAnchor.BottomLeft;
            config.Anchor = DrawAnchor.TopLeft;
            config.ShowIcon = false;
            config.Enabled = false;

            return config;
        }

        [Anchor("Health Bar Anchor")]
        [Order(14)]
        public DrawAnchor HealthBarAnchor = DrawAnchor.BottomLeft;

        public PartyFramesCastbarConfig(Vector2 position, Vector2 size, LabelConfig castNameConfig, LabelConfig castTimeConfig)
            : base(position, size, castNameConfig, castTimeConfig)
        {

        }
    }
}
