using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Interface.GeneralElements;
using DelvUI.Interface.StatusEffects;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Numerics;

namespace DelvUI.Interface.Party
{
    [Section("Party Frames")]
    [SubSection("General", 0)]
    public class PartyFramesConfig : MovablePluginConfigObject
    {
        public new static PartyFramesConfig DefaultConfig()
        {
            var config = new PartyFramesConfig();
            config.Position = new Vector2(-ImGui.GetMainViewport().Size.X / 3 - config.Size.X / 2, -config.Size.Y / 2);

            return config;
        }

        [Checkbox("Lock")]
        [Order(3)]

        public bool Lock = true;
        [Checkbox("Preview", isMonitored = true)]
        [Order(4)]
        public bool Preview = false;

        public Vector2 Size = new Vector2(380, 340);

        [Anchor("Bars Anchor", isMonitored = true, spacing = true)]
        [Order(15)]
        public DrawAnchor BarsAnchor = DrawAnchor.TopLeft;

        [Checkbox("Fill Rows First", isMonitored = true)]
        [Order(20)]
        public bool FillRowsFirst = true;

        [Checkbox("Player Order Override Enabled (Tip: Ctrl+Alt+Shift Click on a bar to set your desired spot in the frames)", spacing = true)]
        [Order(25)]
        public bool PlayerOrderOverrideEnabled = false;

        [Combo("Player Position", "1", "2", "3", "4", "5", "6", "7", "8", isMonitored = true)]
        [Order(25, collapseWith = nameof(PlayerOrderOverrideEnabled))]
        public int PlayerOrder = 1;

        [Checkbox("Show When Solo", spacing = true)]
        [Order(50)]
        public bool ShowWhenSolo = false;

        [Checkbox("Show Chocobo")]
        [Order(55)]
        public bool ShowChocobo = true;
    }

    [Exportable(false)]
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

        [NestedConfig("Health Label", 45)]
        public EditableLabelConfig HealthLabelConfig = new EditableLabelConfig(Vector2.Zero, "[health:current-short]", DrawAnchor.Right, DrawAnchor.Right);

        [NestedConfig("Order Position Label", 50)]
        public LabelConfig OrderLabelConfig = new LabelConfig(new Vector2(2, 4), "[name:first-initial]. [name:last-initial].", DrawAnchor.TopLeft, DrawAnchor.TopLeft);

        [NestedConfig("Colors", 55)]
        public PartyFramesColorsConfig ColorsConfig = new PartyFramesColorsConfig();

        [NestedConfig("Shield", 60)]
        public ShieldConfig ShieldConfig = new ShieldConfig();

        [NestedConfig("Change Alpha Based on Range", 65)]
        public PartyFramesRangeConfig RangeConfig = new PartyFramesRangeConfig();

        [NestedConfig("Use Smooth Transitions", 70)]
        public SmoothHealthConfig SmoothHealthConfig = new SmoothHealthConfig();
    }

    [Disableable(false)]
    [Exportable(false)]
    public class PartyFramesColorsConfig : PluginConfigObject
    {
        [ColorEdit4("Border Color")]
        [Order(5)]
        public PluginConfigColor BorderColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Target Border Color")]
        [Order(10)]
        public PluginConfigColor TargetBordercolor = new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [ColorEdit4("Background Color")]
        [Order(15)]
        public PluginConfigColor BackgroundColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 70f / 100f));

        [Checkbox("Use Role Colors", isMonitored = true, spacing = true)]
        [Order(20)]
        public bool UseRoleColors = false;

        [ColorEdit4("Tank Role Color")]
        [Order(25, collapseWith = nameof(UseRoleColors))]
        public PluginConfigColor TankRoleColor = new PluginConfigColor(new Vector4(21f / 255f, 28f / 255f, 100f / 255f, 100f / 100f));

        [ColorEdit4("DPS Role Color")]
        [Order(30, collapseWith = nameof(UseRoleColors))]
        public PluginConfigColor DPSRoleColor = new PluginConfigColor(new Vector4(153f / 255f, 23f / 255f, 23f / 255f, 100f / 100f));

        [ColorEdit4("Healer Role Color")]
        [Order(35, collapseWith = nameof(UseRoleColors))]
        public PluginConfigColor HealerRoleColor = new PluginConfigColor(new Vector4(46f / 255f, 125f / 255f, 50f / 255f, 100f / 100f));

        [ColorEdit4("Generic Role Color")]
        [Order(40, collapseWith = nameof(UseRoleColors))]
        public PluginConfigColor GenericRoleColor = new PluginConfigColor(new Vector4(0f / 255f, 145f / 255f, 6f / 255f, 100f / 100f));

        [Checkbox("Color Based On Health Value", isMonitored = true)]
        [Order(45)]
        public bool UseColorBasedOnHealthValue = false;

        [ColorEdit4("Full Health Color")]
        [Order(50, collapseWith = nameof(UseColorBasedOnHealthValue))]
        public PluginConfigColor FullHealthColor = new PluginConfigColor(new Vector4(0f / 255f, 255f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Low Health Color")]
        [Order(55, collapseWith = nameof(UseColorBasedOnHealthValue))]
        public PluginConfigColor LowHealthColor = new PluginConfigColor(new Vector4(255f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        [DragFloat("Full Health Color Above Health %", min = 50f, max = 100f, velocity = 1f)]
        [Order(60, collapseWith = nameof(UseColorBasedOnHealthValue))]
        public float FullHealthColorThreshold = 75f;

        [DragFloat("Low Health Color Below Health %", min = 0f, max = 50f, velocity = 1f)]
        [Order(65, collapseWith = nameof(UseColorBasedOnHealthValue))]
        public float LowHealthColorThreshold = 25f;

        [Combo("Blend Mode", "LAB", "LChab", "XYZ", "RGB", "LChuv", "Luv", "Jzazbz", "JzCzhz")]
        [Order(66, collapseWith = nameof(UseColorBasedOnHealthValue))]
        public BlendMode blendMode = BlendMode.LAB;

        [Checkbox("Highlight When Hovering With Cursor", spacing = true)]
        [Order(70)]
        public bool ShowHighlight = true;

        [ColorEdit4("Highlight Color")]
        [Order(75, collapseWith = nameof(ShowHighlight))]
        public PluginConfigColor HighlightColor = new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 5f / 100f));

        [Checkbox("Show Enmity Border Colors", spacing = true)]
        [Order(80)]
        public bool ShowEnmityBorderColors = true;

        [ColorEdit4("Enmity Leader Color")]
        [Order(85, collapseWith = nameof(ShowEnmityBorderColors))]
        public PluginConfigColor EnmityLeaderBordercolor = new PluginConfigColor(new Vector4(255f / 255f, 40f / 255f, 40f / 255f, 100f / 100f));

        [Checkbox("Show Second Enmity")]
        [Order(90, collapseWith = nameof(ShowEnmityBorderColors))]
        public bool ShowSecondEnmity = true;

        [Checkbox("Hide Second Enmity in Light Parties")]
        [Order(95, collapseWith = nameof(ShowSecondEnmity))]
        public bool HideSecondEnmityInLightParties = true;

        [ColorEdit4("Enmity Second Color")]
        [Order(100, collapseWith = nameof(ShowSecondEnmity))]
        public PluginConfigColor EnmitySecondBordercolor = new PluginConfigColor(new Vector4(255f / 255f, 175f / 255f, 40f / 255f, 100f / 100f));
    }

    [Exportable(false)]
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

        public float AlphaForDistance(int distance, float alpha = 100f)
        {
            if (!Enabled)
            {
                return 100f;
            }

            if (!UseAdditionalRangeCheck)
            {
                return distance > Range ? Alpha : alpha;
            }

            if (Range > AdditionalRange)
            {
                return distance > Range ? Alpha : (distance > AdditionalRange ? AdditionalAlpha : alpha);
            }

            return distance > AdditionalRange ? AdditionalAlpha : (distance > Range ? Alpha : alpha);
        }
    }

    [Exportable(false)]
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

    [Exportable(false)]
    [Section("Party Frames")]
    [SubSection("Role-Job Icon", 0)]
    public class PartyFramesRoleIconConfig : MovablePluginConfigObject
    {
        public new static PartyFramesRoleIconConfig DefaultConfig()
        {
            var config = new PartyFramesRoleIconConfig();
            config.Position = new Vector2(20, 0);

            return config;
        }

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

    [Exportable(false)]
    [Section("Party Frames")]
    [SubSection("Party Leader Icon", 0)]
    public class PartyFramesLeaderIconConfig : MovablePluginConfigObject
    {
        public new static PartyFramesLeaderIconConfig DefaultConfig()
        {
            var config = new PartyFramesLeaderIconConfig();
            config.Position = new Vector2(-12, -12);

            return config;
        }

        [DragInt2("Size", min = 1, max = 1000)]
        [Order(20)]
        public Vector2 Size = new(24, 24);

        [Anchor("Health Bar Anchor")]
        [Order(25)]
        public DrawAnchor HealthBarAnchor = DrawAnchor.TopLeft;

        [Anchor("Anchor")]
        [Order(30)]
        public DrawAnchor Anchor = DrawAnchor.TopLeft;
    }

    [Exportable(false)]
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

    [Exportable(false)]
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

    [Exportable(false)]
    [Section("Party Frames")]
    [SubSection("Castbars", 0)]
    public class PartyFramesCastbarConfig : CastbarConfig
    {
        public new static PartyFramesCastbarConfig DefaultConfig()
        {
            var size = new Vector2(182, 10);
            var pos = new Vector2(-1, 0);

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

    [Exportable(false)]
    [Section("Party Frames")]
    [SubSection("Raise Tracker", 0)]
    public class PartyFramesRaiseTrackerConfig : MovablePluginConfigObject
    {
        public new static PartyFramesRaiseTrackerConfig DefaultConfig() { return new PartyFramesRaiseTrackerConfig(); }

        [DragInt2("Icon Size", min = 1, max = 1000)]
        [Order(10)]
        public Vector2 IconSize = new(50, 50);

        [Anchor("Health Bar Anchor")]
        [Order(15)]
        public DrawAnchor HealthBarAnchor = DrawAnchor.Center;

        [Anchor("Anchor")]
        [Order(20)]
        public DrawAnchor Anchor = DrawAnchor.Center;

        [Checkbox("Hide Name When Raised", spacing = true)]
        [Order(25)]
        public bool HideNameWhenRaised = true;

        [Checkbox("Keep Icon After Cast Finishes")]
        [Order(30)]
        public bool KeepIconAfterCastFinishes = true;

        [Checkbox("Change Background Color When Raised", spacing = true)]
        [Order(35)]
        public bool ChangeBackgroundColorWhenRaised = true;

        [ColorEdit4("Raise Backround Color")]
        [Order(40, collapseWith = nameof(ChangeBackgroundColorWhenRaised))]
        public PluginConfigColor BackgroundColor = new(new Vector4(211f / 255f, 235f / 255f, 215f / 245f, 50f / 100f));

        [Checkbox("Change Border Color When Raised", spacing = true)]
        [Order(45)]
        public bool ChangeBorderColorWhenRaised = true;

        [ColorEdit4("Raise Border Color")]
        [Order(50, collapseWith = nameof(ChangeBorderColorWhenRaised))]
        public PluginConfigColor BorderColor = new(new Vector4(47f / 255f, 169f / 255f, 215f / 255f, 100f / 100f));

        [NestedConfig("Label", 55)]
        public LabelConfig LabelConfig = new LabelConfig(Vector2.Zero, "", DrawAnchor.Center, DrawAnchor.Center);
    }
}
