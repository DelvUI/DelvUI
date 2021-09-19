using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Interface.GeneralElements;
using DelvUI.Interface.StatusEffects;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Numerics;

namespace DelvUI.Interface.Party
{
    [Serializable]
    [Section("Party Frames")]
    [SubSection("General", 0)]
    public class PartyFramesConfig : MovablePluginConfigObject
    {
        public new static PartyFramesConfig DefaultConfig() { return new PartyFramesConfig(); }

        [DragInt2("Size", isMonitored = true)]
        [Order(30)]
        public Vector2 Size = new Vector2(650, 150);

        [Checkbox("Lock")]
        [Order(35)]
        public bool Lock = true;

        [Checkbox("Preview", isMonitored = true)]
        [Order(40)]
        public bool Preview = false;

        [Checkbox("Fill Rows First", isMonitored = true)]
        [Order(45)]
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
        [Order(50)]
        public PartySortingMode SortingMode = PartySortingMode.Tank_Healer_DPS;

        [Anchor("Bars Anchor", isMonitored = true)]
        [Order(55)]
        public HudElementAnchor BarsAnchor = HudElementAnchor.TopLeft;
    }


    [Serializable]
    [Section("Party Frames")]
    [SubSection("Bars", 0)]
    public class PartyFramesBarsConfig : PluginConfigObject
    {
        public new static PartyFramesBarsConfig DefaultConfig() { return new PartyFramesBarsConfig(); }

        [DragInt2("Size", isMonitored = true)]
        [Order(30)]
        public Vector2 Size = new Vector2(180, 80);

        [DragInt2("Padding", isMonitored = true)]
        [Order(35)]
        public Vector2 Padding = new Vector2(1, 1);

        [NestedConfig("Name Label", 40)]
        public EditableLabelConfig NameLabelConfig = new EditableLabelConfig(Vector2.Zero, "[name:first-initial]. [name:last-initial].", HudElementAnchor.Center, HudElementAnchor.Center);

        [NestedConfig("Mana Bar", 45)]
        public PartyFramesManaBarConfig ManaBarConfig = new PartyFramesManaBarConfig();

        [NestedConfig("Colors", 50)]
        public PartyFramesColorsConfig ColorsConfig = new PartyFramesColorsConfig();

        [NestedConfig("Job/Role Icons", 55)]
        public PartyFramesRoleIconConfig RoleIconConfig = new PartyFramesRoleIconConfig();

        [NestedConfig("Shield", 60)]
        public ShieldConfig ShieldConfig = new ShieldConfig();
    }

    [Serializable]
    [Portable(false)]
    public class PartyFramesManaBarConfig : MovablePluginConfigObject
    {
        [DragInt("Height", min = 1, max = 1000)]
        [Order(30)]
        public int Height = 6;

        [Checkbox("Show Only For Healers")]
        [Order(35)]
        public bool ShowOnlyForHealers = true;

        [ColorEdit4("Color")]
        [Order(40)]
        public PluginConfigColor Color = new PluginConfigColor(new(0 / 255f, 162f / 255f, 252f / 255f, 100f / 100f));

        [ColorEdit4("Background Color")]
        [Order(45)]
        public PluginConfigColor BackgroundColor = new PluginConfigColor(new(0 / 255f, 20f / 255f, 100f / 255f, 100f / 100f));
    }

    [Serializable]
    [Portable(false)]
    public class PartyFramesColorsConfig : PluginConfigObject
    {
        [Checkbox("Use Role Colors", isMonitored = true)]
        [CollapseControl(0, 0)]
        public bool UseRoleColors = false;

        [ColorEdit4("Tank Role Color")]
        [CollapseWith(0, 0)]
        public PluginConfigColor TankRoleColor = new PluginConfigColor(new Vector4(21f / 255f, 28f / 255f, 100f / 255f, 100f / 100f));

        [ColorEdit4("DPS Role Color")]
        [CollapseWith(5, 0)]
        public PluginConfigColor DPSRoleColor = new PluginConfigColor(new Vector4(153f / 255f, 23f / 255f, 23f / 255f, 100f / 100f));

        [ColorEdit4("Healer Role Color")]
        [CollapseWith(10, 0)]
        public PluginConfigColor HealerRoleColor = new PluginConfigColor(new Vector4(46f / 255f, 125f / 255f, 50f / 255f, 100f / 100f));

        [ColorEdit4("Generic Role Color")]
        [CollapseWith(15, 0)]
        public PluginConfigColor GenericRoleColor = new PluginConfigColor(new Vector4(0f / 255f, 145f / 255f, 6f / 255f, 100f / 100f));

        [Checkbox("Highlight When Hovering With Cursor")]
        [CollapseControl(5, 1)]
        public bool ShowHighlight = true;

        [ColorEdit4("Highlight Color")]
        [CollapseWith(0, 1)]
        public PluginConfigColor HighlightColor = new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 5f / 100f));

        [ColorEdit4("Background Color")]
        [Order(10)]
        public PluginConfigColor BackgroundColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 70f / 100f));

        [ColorEdit4("Out of Reach Color")]
        [Order(15)]
        public PluginConfigColor UnreachableColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 30f / 100f));
    }

    [Serializable]
    [Portable(false)]
    public class PartyFramesRoleIconConfig : MovablePluginConfigObject
    {
        [DragInt2("Size", min = 1, max = 1000)]
        [Order(30)]
        public Vector2 Size = new(20, 20);

        [Combo("Style", "Style 1", "Style 2")]
        [Order(35)]
        public int Style = 0;

        [Checkbox("Use Role Icons")]
        [CollapseControl(40, 0)]
        public bool UseRoleIcons = false;

        [Checkbox("Use Specific DPS Role Icons")]
        [CollapseWith(0, 0)]
        public bool UseSpecificDPSRoleIcons = false;
    }

    [Serializable]
    [Section("Party Frames")]
    [SubSection("Buffs", 0)]
    public class PartyFramesBuffsConfig : PartyFramesStatusEffectsListConfig
    {
        public new static PartyFramesBuffsConfig DefaultConfig()
        {
            var durationConfig = new LabelConfig(new Vector2(0, -4), "", HudElementAnchor.Bottom, HudElementAnchor.Center);
            var stacksConfig = new LabelConfig(new Vector2(-3, 4), "", HudElementAnchor.TopRight, HudElementAnchor.Center);
            stacksConfig.Color = new(Vector4.UnitW);
            stacksConfig.OutlineColor = new(Vector4.One);

            var iconConfig = new StatusEffectIconConfig(durationConfig, stacksConfig);
            iconConfig.DispellableBorderConfig.Enabled = false;
            iconConfig.Size = new Vector2(24, 24);

            var pos = new Vector2(-2, 2);
            var size = new Vector2(iconConfig.Size.X * 4 + 6, iconConfig.Size.Y);

            var config = new PartyFramesBuffsConfig(HudElementAnchor.TopRight, pos, size, true, false, false, GrowthDirections.Left | GrowthDirections.Down, iconConfig);
            config.Limit = 4;

            return config;
        }

        public PartyFramesBuffsConfig(HudElementAnchor anchor, Vector2 position, Vector2 size, bool showBuffs, bool showDebuffs, bool showPermanentEffects,
            GrowthDirections growthDirections, StatusEffectIconConfig iconConfig)
            : base(anchor, position, size, showBuffs, showDebuffs, showPermanentEffects, growthDirections, iconConfig)
        {
        }
    }

    [Serializable]
    [Section("Party Frames")]
    [SubSection("Debuffs", 0)]
    public class PartyFramesDebuffsConfig : PartyFramesStatusEffectsListConfig
    {
        public new static PartyFramesDebuffsConfig DefaultConfig()
        {
            var durationConfig = new LabelConfig(new Vector2(0, -4), "", HudElementAnchor.Bottom, HudElementAnchor.Center);
            var stacksConfig = new LabelConfig(new Vector2(-3, 4), "", HudElementAnchor.TopRight, HudElementAnchor.Center);
            stacksConfig.Color = new(Vector4.UnitW);
            stacksConfig.OutlineColor = new(Vector4.One);

            var iconConfig = new StatusEffectIconConfig(durationConfig, stacksConfig);
            iconConfig.Size = new Vector2(24, 24);

            var pos = new Vector2(-2, -2);
            var size = new Vector2(iconConfig.Size.X * 4 + 6, iconConfig.Size.Y);

            var config = new PartyFramesDebuffsConfig(HudElementAnchor.BottomRight, pos, size, false, true, false, GrowthDirections.Left | GrowthDirections.Up, iconConfig);
            config.Limit = 4;

            return config;
        }

        public PartyFramesDebuffsConfig(HudElementAnchor anchor, Vector2 position, Vector2 size, bool showBuffs, bool showDebuffs, bool showPermanentEffects,
            GrowthDirections growthDirections, StatusEffectIconConfig iconConfig)
            : base(anchor, position, size, showBuffs, showDebuffs, showPermanentEffects, growthDirections, iconConfig)
        {
        }
    }

    [Serializable]

    public class PartyFramesStatusEffectsListConfig : StatusEffectsListConfig
    {
        [Combo("Anchor", "Top Left", "Top Right", "Bottom Left", "Bottom Right")]
        [Order(4)]
        public int AnchorIndex;

        [JsonIgnore]
        public HudElementAnchor Anchor
        {
            get
            {
                switch (AnchorIndex)
                {
                    case 0: return HudElementAnchor.TopLeft;
                    case 1: return HudElementAnchor.TopRight;
                    case 2: return HudElementAnchor.BottomLeft;
                }

                return HudElementAnchor.BottomRight;
            }

            set
            {
                switch (value)
                {
                    case HudElementAnchor.TopLeft: AnchorIndex = 0; return;
                    case HudElementAnchor.TopRight: AnchorIndex = 1; return;
                    case HudElementAnchor.BottomLeft: AnchorIndex = 2; return;
                }

                AnchorIndex = 3;
            }
        }

        public PartyFramesStatusEffectsListConfig(HudElementAnchor anchor, Vector2 position, Vector2 size, bool showBuffs, bool showDebuffs, bool showPermanentEffects,
            GrowthDirections growthDirections, StatusEffectIconConfig iconConfig)
            : base(position, size, showBuffs, showDebuffs, showPermanentEffects, growthDirections, iconConfig)
        {
            Anchor = anchor;
        }
    }
}
