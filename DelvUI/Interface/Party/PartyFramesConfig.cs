using Dalamud.Logging;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using DelvUI.Interface.StatusEffects;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace DelvUI.Interface.Party
{
    [Exportable(false)]
    [Section("Party Frames", true)]
    [SubSection("General", 0)]
    public class PartyFramesConfig : MovablePluginConfigObject
    {
        public new static PartyFramesConfig DefaultConfig()
        {
            var config = new PartyFramesConfig();
            config.Position = new Vector2(-ImGui.GetMainViewport().Size.X / 3 - 180, -120);

            return config;
        }

        [Checkbox("Preview", isMonitored = true)]
        [Order(4)]
        public bool Preview = false;

        [DragInt("Rows", spacing = true, isMonitored = true, min = 1, max = 8, velocity = 0.2f)]
        [Order(10)]
        public int Rows = 4;

        [DragInt("Columns", isMonitored = true, min = 1, max = 8, velocity = 0.2f)]
        [Order(11)]
        public int Columns = 2;

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

        [Checkbox("Show Chocobo", isMonitored = true)]
        [Order(55)]
        public bool ShowChocobo = true;
    }

    [Exportable(false)]
    [Disableable(false)]
    [DisableParentSettings("Position", "Anchor", "BackgroundColor", "FillColor", "HideWhenInactive", "DrawBorder", "BorderColor", "BorderThickness")]
    [Section("Party Frames", true)]
    [SubSection("Health Bar", 0)]
    public class PartyFramesHealthBarsConfig : BarConfig
    {
        public new static PartyFramesHealthBarsConfig DefaultConfig()
        {
            var config = new PartyFramesHealthBarsConfig(Vector2.Zero, new(180, 80), new PluginConfigColor(Vector4.Zero));
            config.MouseoverAreaConfig.Enabled = false;

            return config;
        }

        [DragInt2("Padding", isMonitored = true, min = 0)]
        [Order(31)]
        public Vector2 Padding = new Vector2(0, 0);

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

        [NestedConfig("Custom Mouseover Area", 75)]
        public MouseoverAreaConfig MouseoverAreaConfig = new MouseoverAreaConfig();

        public PartyFramesHealthBarsConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor, BarDirection fillDirection = BarDirection.Right)
            : base(position, size, fillColor, fillDirection)
        {
        }

        protected override PluginConfigObject? InternalLoad(FileInfo fileInfo, string currentVersion, string? previousVersion)
        {
            if (previousVersion == null) { return null; }

            // change introduced in 0.6.2.0
            Version previous = new Version(previousVersion);
            if (previous.Major > 0 || previous.Minor > 6 || previous.Minor == 6 && previous.Build >= 2) { return null; }

            PartyFramesHealthBarsConfig? config = LoadFromJson<PartyFramesHealthBarsConfig>(fileInfo.FullName);
            if (config == null) { return null; }

            config.FillDirection = BarDirection.Right;

            return config;
        }

        public override void ImportFromOldVersion(Dictionary<Type, PluginConfigObject> oldConfigObjects, string currentVersion, string? previousVersion)
        {
            if (previousVersion == null) { return; }

            // change introduced in 0.6.2.0
            Version previous = new Version(previousVersion);
            if (previous.Major > 0 || previous.Minor > 6 || previous.Minor == 6 && previous.Build >= 2) { return; }

            FillDirection = BarDirection.Right;
        }
    }

    [Disableable(false)]
    [Exportable(false)]
    public class PartyFramesColorsConfig : PluginConfigObject
    {
        [Checkbox("Show Border")]
        [Order(4)]
        public bool ShowBorder = true;

        [ColorEdit4("Border Color")]
        [Order(5, collapseWith = nameof(ShowBorder))]
        public PluginConfigColor BorderColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Target Border Color")]
        [Order(6, collapseWith = nameof(ShowBorder))]
        public PluginConfigColor TargetBordercolor = new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [DragInt("Inactive Border Thickness", min = 1, max = 10, help = "This is the border thickness that will be used when the border is in the default state (aka not targetted, not showing enmity, etc).")]
        [Order(6, collapseWith = nameof(ShowBorder))]
        public int InactiveBorderThickness = 1;

        [DragInt("Active Border Thickness", min = 1, max = 10, help = "This is the border thickness that will be used when the border active (aka targetted, showing enmity, etc).")]
        [Order(7, collapseWith = nameof(ShowBorder))]
        public int ActiveBorderThickness = 1;

        [ColorEdit4("Background Color", spacing = true)]
        [Order(15)]
        public PluginConfigColor BackgroundColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 70f / 100f));

        [ColorEdit4("Out of Reach Background Color", help = "This background color will be used when the player's data couldn't be retreived (i.e. player is disconnected)")]
        [Order(15)]
        public PluginConfigColor OutOfReachBackgroundColor = new PluginConfigColor(new Vector4(50f / 255f, 50f / 255f, 50f / 255f, 70f / 100f));

        [Checkbox("Use Death Indicator Background Color", isMonitored = true, spacing = true)]
        [Order(18)]
        public bool UseDeathIndicatorBackgroundColor = false;

        [ColorEdit4("Death Indicator Background Color")]
        [Order(19, collapseWith = nameof(UseDeathIndicatorBackgroundColor))]
        public PluginConfigColor DeathIndicatorBackgroundColor = new PluginConfigColor(new Vector4(204f / 255f, 3f / 255f, 3f / 255f, 80f / 100f));

        [Checkbox("Use Role Colors", isMonitored = true, spacing = true)]
        [Order(20)]
        public bool UseRoleColors = false;

        [NestedConfig("Color Based On Health Value", 30, collapsingHeader = false)]
        public ColorByHealthValueConfig ColorByHealth = new ColorByHealthValueConfig();

        [Checkbox("Highlight When Hovering With Cursor", spacing = true)]
        [Order(40)]
        public bool ShowHighlight = true;

        [ColorEdit4("Highlight Color")]
        [Order(45, collapseWith = nameof(ShowHighlight))]
        public PluginConfigColor HighlightColor = new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 5f / 100f));


        [Checkbox("Missing Health Color", spacing = true)]
        [Order(46)]
        public bool UseMissingHealthBar = false;

        [Checkbox("Job Color As Missing Health Color")]
        [Order(47, collapseWith = nameof(UseMissingHealthBar))]
        public bool UseJobColorAsMissingHealthColor = false;

        [Checkbox("Role Color As Missing Health Color")]
        [Order(48, collapseWith = nameof(UseMissingHealthBar))]
        public bool UseRoleColorAsMissingHealthColor = false;

        [ColorEdit4("Color" + "##MissingHealth")]
        [Order(49, collapseWith = nameof(UseMissingHealthBar))]
        public PluginConfigColor HealthMissingColor = new(new Vector4(255f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        [Checkbox("Job Color As Background Color")]
        [Order(50)]
        public bool UseJobColorAsBackgroundColor = false;

        [Checkbox("Role Color As Background Color")]
        [Order(51)]
        public bool UseRoleColorAsBackgroundColor = false;

        [Checkbox("Show Enmity Border Colors", spacing = true)]
        [Order(54)]
        public bool ShowEnmityBorderColors = true;

        [ColorEdit4("Enmity Leader Color")]
        [Order(55, collapseWith = nameof(ShowEnmityBorderColors))]
        public PluginConfigColor EnmityLeaderBordercolor = new PluginConfigColor(new Vector4(255f / 255f, 40f / 255f, 40f / 255f, 100f / 100f));

        [Checkbox("Show Second Enmity")]
        [Order(60, collapseWith = nameof(ShowEnmityBorderColors))]
        public bool ShowSecondEnmity = true;

        [Checkbox("Hide Second Enmity in Light Parties")]
        [Order(65, collapseWith = nameof(ShowSecondEnmity))]
        public bool HideSecondEnmityInLightParties = true;

        [ColorEdit4("Enmity Second Color")]
        [Order(70, collapseWith = nameof(ShowSecondEnmity))]
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

    public class PartyFramesManaBarConfigConverter : PluginConfigObjectConverter
    {
        public PartyFramesManaBarConfigConverter()
        {
            NewTypeFieldConverter<bool, PartyFramesManaBarDisplayMode> converter;
            converter = new NewTypeFieldConverter<bool, PartyFramesManaBarDisplayMode>(
                "PartyFramesManaBarDisplayMode",
                PartyFramesManaBarDisplayMode.HealersOnly,
                (oldValue) =>
                {
                    return oldValue ? PartyFramesManaBarDisplayMode.HealersOnly : PartyFramesManaBarDisplayMode.Always;
                });

            FieldConvertersMap.Add("ShowOnlyForHealers", converter);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PartyFramesManaBarConfig);
        }
    }

    public enum PartyFramesManaBarDisplayMode
    {
        HealersAndRaiseJobs,
        HealersOnly,
        Always,
    }

    [DisableParentSettings("HideWhenInactive", "Label")]
    [Exportable(false)]
    [Section("Party Frames", true)]
    [SubSection("Mana Bar", 0)]
    public class PartyFramesManaBarConfig : PrimaryResourceConfig
    {
        public new static PartyFramesManaBarConfig DefaultConfig()
        {
            var config = new PartyFramesManaBarConfig(Vector2.Zero, new(180, 6));
            config.HealthBarAnchor = DrawAnchor.Bottom;
            config.Anchor = DrawAnchor.Bottom;
            config.ValueLabel.Enabled = false;
            return config;
        }

        [Anchor("Health Bar Anchor")]
        [Order(14)]
        public DrawAnchor HealthBarAnchor = DrawAnchor.BottomLeft;

        [RadioSelector("Show For All Jobs With Raise", "Show Only For Healers", "Show For All Jobs")]
        [Order(42)]
        public PartyFramesManaBarDisplayMode ManaBarDisplayMode = PartyFramesManaBarDisplayMode.HealersOnly;

        public PartyFramesManaBarConfig(Vector2 position, Vector2 size)
            : base(position, size)
        {
        }
    }

    [Exportable(false)]
    [Section("Party Frames", true)]
    [SubSection("Castbar", 0)]
    public class PartyFramesCastbarConfig : CastbarConfig
    {
        public new static PartyFramesCastbarConfig DefaultConfig()
        {
            var size = new Vector2(182, 10);
            var pos = new Vector2(-1, 0);

            var castNameConfig = new LabelConfig(new Vector2(5, 0), "", DrawAnchor.Left, DrawAnchor.Left);
            var castTimeConfig = new NumericLabelConfig(new Vector2(-5, 0), "", DrawAnchor.Right, DrawAnchor.Right);
            castTimeConfig.Enabled = false;
            castTimeConfig.NumberFormat = 1;

            var config = new PartyFramesCastbarConfig(pos, size, castNameConfig, castTimeConfig);
            config.HealthBarAnchor = DrawAnchor.BottomLeft;
            config.Anchor = DrawAnchor.TopLeft;
            config.ShowIcon = false;
            config.Enabled = false;

            return config;
        }

        [Anchor("Health Bar Anchor")]
        [Order(16)]
        public DrawAnchor HealthBarAnchor = DrawAnchor.BottomLeft;

        public PartyFramesCastbarConfig(Vector2 position, Vector2 size, LabelConfig castNameConfig, NumericLabelConfig castTimeConfig)
            : base(position, size, castNameConfig, castTimeConfig)
        {

        }
    }

    [Disableable(false)]
    [Exportable(false)]
    [Section("Party Frames", true)]
    [SubSection("Icons", 0)]
    public class PartyFramesIconsConfig : PluginConfigObject
    {
        public new static PartyFramesIconsConfig DefaultConfig() { return new PartyFramesIconsConfig(); }

        [NestedConfig("Role / Job", 10, separator = false)]
        public PartyFramesRoleIconConfig Role = new PartyFramesRoleIconConfig(
            new Vector2(20, 0),
            new Vector2(20, 20),
            DrawAnchor.TopLeft,
            DrawAnchor.TopLeft
        );

        [NestedConfig("Leader", 15)]
        public PartyFramesLeaderIconConfig Leader = new PartyFramesLeaderIconConfig(
            new Vector2(-12, -12),
            new Vector2(24, 24),
            DrawAnchor.TopLeft,
            DrawAnchor.TopLeft
        );

        [NestedConfig("Player Status", 15)]
        public PartyFramesPlayerStatusConfig PlayerStatus = new PartyFramesPlayerStatusConfig();

        protected override PluginConfigObject? InternalLoad(FileInfo fileInfo, string currentVersion, string? previousVersion)
        {
            if (previousVersion == null) { return null; }

            // change introduced in 0.4.0.0
            Version previous = new Version(previousVersion);
            if (previous.Major > 0 || previous.Minor > 3) { return null; }

            string? path = fileInfo.DirectoryName;
            if (path == null) { return null; }

            PartyFramesIconsConfig config = new PartyFramesIconsConfig();

            // role / job icon
            try
            {
                string nestedConfigPath = Path.Combine(path, "Role-Job Icon.json");
                config.Role = LoadFromJson<PartyFramesRoleIconConfig>(nestedConfigPath) ?? config.Role;
            }
            catch (Exception e)
            {
                PluginLog.Error("Error while merging role-job icon configs: " + e.Message);
            }

            // party leader
            try
            {
                string nestedConfigPath = Path.Combine(path, "Party Leader Icon.json");
                config.Leader = LoadFromJson<PartyFramesLeaderIconConfig>(nestedConfigPath) ?? config.Leader;
            }
            catch (Exception e)
            {
                PluginLog.Error("Error while merging invuln tracker configs: " + e.Message);
            }

            return config;
        }

        public override void ImportFromOldVersion(Dictionary<Type, PluginConfigObject> oldConfigObjects, string currentVersion, string? previousVersion)
        {
            if (oldConfigObjects.TryGetValue(typeof(PartyFramesRoleIconConfig), out PluginConfigObject? roleObj)
                && roleObj is PartyFramesRoleIconConfig role)
            {
                Role = role;
            }

            if (oldConfigObjects.TryGetValue(typeof(PartyFramesLeaderIconConfig), out PluginConfigObject? leaderObj)
                && leaderObj is PartyFramesLeaderIconConfig leader)
            {
                Leader = leader;
            }
        }
    }

    [Exportable(false)]
    public class PartyFramesRoleIconConfig : RoleJobIconConfig
    {
        public PartyFramesRoleIconConfig() : base() { }

        public PartyFramesRoleIconConfig(Vector2 position, Vector2 size, DrawAnchor anchor, DrawAnchor frameAnchor)
            : base(position, size, anchor, frameAnchor)
        {
        }
    }

    [Exportable(false)]
    public class PartyFramesLeaderIconConfig : IconConfig
    {
        public PartyFramesLeaderIconConfig() : base() { }

        public PartyFramesLeaderIconConfig(Vector2 position, Vector2 size, DrawAnchor anchor, DrawAnchor frameAnchor)
            : base(position, size, anchor, frameAnchor)
        {
        }
    }

    [Exportable(false)]
    public class PartyFramesPlayerStatusConfig : PluginConfigObject
    {
        public new static PartyFramesPlayerStatusConfig DefaultConfig()
        {
            var config = new PartyFramesPlayerStatusConfig();
            config.Label.Enabled = false;

            return config;
        }

        [Checkbox("Hide Name When Showing Status")]
        [Order(5)]
        public bool HideName = false;

        [NestedConfig("Icon", 10)]
        public IconConfig Icon = new IconConfig(
            new Vector2(0, 5),
            new Vector2(16, 16),
            DrawAnchor.Top,
            DrawAnchor.Top
        );

        [NestedConfig("Label", 15)]
        public LabelConfig Label = new LabelConfig(Vector2.Zero, "", DrawAnchor.Center, DrawAnchor.Center);
    }

    [Exportable(false)]
    [Section("Party Frames", true)]
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
    [Section("Party Frames", true)]
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

    [Disableable(false)]
    [Exportable(false)]
    [Section("Party Frames", true)]
    [SubSection("Trackers", 0)]
    public class PartyFramesTrackersConfig : PluginConfigObject
    {
        public new static PartyFramesTrackersConfig DefaultConfig() { return new PartyFramesTrackersConfig(); }

        [NestedConfig("Raise Tracker", 10, separator = false)]
        public PartyFramesRaiseTrackerConfig Raise = new PartyFramesRaiseTrackerConfig();

        [NestedConfig("Invulnerabilities Tracker", 15)]
        public PartyFramesInvulnTrackerConfig Invuln = new PartyFramesInvulnTrackerConfig();

        [NestedConfig("Cleanse Tracker", 15)]
        public PartyFramesCleanseTrackerConfig Cleanse = new PartyFramesCleanseTrackerConfig();

        protected override PluginConfigObject? InternalLoad(FileInfo fileInfo, string currentVersion, string? previousVersion)
        {
            if (previousVersion == null) { return null; }

            // change introduced in 0.4.0.0
            Version previous = new Version(previousVersion);
            if (previous.Major > 0 || previous.Minor > 3) { return null; }

            string? path = fileInfo.DirectoryName;
            if (path == null) { return null; }

            PartyFramesTrackersConfig config = new PartyFramesTrackersConfig();

            // raise tracker
            try
            {
                string nestedConfigPath = Path.Combine(path, "Raise Tracker.json");
                config.Raise = LoadFromJson<PartyFramesRaiseTrackerConfig>(nestedConfigPath) ?? config.Raise;
            }
            catch (Exception e)
            {
                PluginLog.Error("Error while merging raise tracker configs: " + e.Message);
            }

            // invuln tracker
            try
            {
                string nestedConfigPath = Path.Combine(path, "Invuln Tracker.json");
                config.Invuln = LoadFromJson<PartyFramesInvulnTrackerConfig>(nestedConfigPath) ?? config.Invuln;
            }
            catch (Exception e)
            {
                PluginLog.Error("Error while merging invuln tracker configs: " + e.Message);
            }

            // cleanse tracker
            try
            {
                string nestedConfigPath = Path.Combine(path, "Cleanse Tracker.json");
                config.Cleanse = LoadFromJson<PartyFramesCleanseTrackerConfig>(nestedConfigPath) ?? config.Cleanse;
            }
            catch (Exception e)
            {
                PluginLog.Error("Error while merging cleanse tracker configs: " + e.Message);
            }

            return config;
        }

        public override void ImportFromOldVersion(Dictionary<Type, PluginConfigObject> oldConfigObjects, string currentVersion, string? previousVersion)
        {
            if (oldConfigObjects.TryGetValue(typeof(PartyFramesRaiseTrackerConfig), out PluginConfigObject? raiseObj)
                && raiseObj is PartyFramesRaiseTrackerConfig raise)
            {
                Raise = raise;
            }

            if (oldConfigObjects.TryGetValue(typeof(PartyFramesInvulnTrackerConfig), out PluginConfigObject? invulvObj)
                && invulvObj is PartyFramesInvulnTrackerConfig invuln)
            {
                Invuln = invuln;
            }

            if (oldConfigObjects.TryGetValue(typeof(PartyFramesCleanseTrackerConfig), out PluginConfigObject? cleanseObj)
                && cleanseObj is PartyFramesCleanseTrackerConfig cleanse)
            {
                Cleanse = cleanse;
            }
        }
    }

    [Exportable(false)]
    public class PartyFramesRaiseTrackerConfig : PluginConfigObject
    {
        public new static PartyFramesRaiseTrackerConfig DefaultConfig() { return new PartyFramesRaiseTrackerConfig(); }

        [Checkbox("Hide Name When Raised")]
        [Order(10)]
        public bool HideNameWhenRaised = true;

        [Checkbox("Keep Icon After Cast Finishes")]
        [Order(15)]
        public bool KeepIconAfterCastFinishes = true;

        [Checkbox("Change Background Color When Raised", spacing = true)]
        [Order(20)]
        public bool ChangeBackgroundColorWhenRaised = true;

        [ColorEdit4("Raise Background Color")]
        [Order(25, collapseWith = nameof(ChangeBackgroundColorWhenRaised))]
        public PluginConfigColor BackgroundColor = new(new Vector4(211f / 255f, 235f / 255f, 215f / 245f, 50f / 100f));

        [Checkbox("Change Border Color When Raised", spacing = true)]
        [Order(30)]
        public bool ChangeBorderColorWhenRaised = true;

        [ColorEdit4("Raise Border Color")]
        [Order(35, collapseWith = nameof(ChangeBorderColorWhenRaised))]
        public PluginConfigColor BorderColor = new(new Vector4(47f / 255f, 169f / 255f, 215f / 255f, 100f / 100f));

        [NestedConfig("Icon", 50)]
        public IconWithLabelConfig Icon = new IconWithLabelConfig(
            new Vector2(0, 0),
            new Vector2(50, 50),
            DrawAnchor.Center,
            DrawAnchor.Center
        );
    }

    [Exportable(false)]
    public class PartyFramesInvulnTrackerConfig : PluginConfigObject
    {
        public new static PartyFramesInvulnTrackerConfig DefaultConfig() { return new PartyFramesInvulnTrackerConfig(); }

        [Checkbox("Hide Name When Invuln is Up")]
        [Order(10)]
        public bool HideNameWhenInvuln = true;

        [Checkbox("Change Background Color When Invuln is Up", spacing = true)]
        [Order(15)]
        public bool ChangeBackgroundColorWhenInvuln = true;

        [ColorEdit4("Invuln Background Color")]
        [Order(20, collapseWith = nameof(ChangeBackgroundColorWhenInvuln))]
        public PluginConfigColor BackgroundColor = new(new Vector4(211f / 255f, 235f / 255f, 215f / 245f, 50f / 100f));

        [Checkbox("Walking Dead Custom Color")]
        [Order(25, collapseWith = nameof(ChangeBackgroundColorWhenInvuln))]
        public bool UseCustomWalkingDeadColor = true;

        [ColorEdit4("Walking Dead Background Color")]
        [Order(30, collapseWith = nameof(UseCustomWalkingDeadColor))]
        public PluginConfigColor WalkingDeadBackgroundColor = new(new Vector4(158f / 255f, 158f / 255f, 158f / 255f, 50f / 100f));

        [NestedConfig("Icon", 50)]
        public IconWithLabelConfig Icon = new IconWithLabelConfig(
            new Vector2(0, 0),
            new Vector2(50, 50),
            DrawAnchor.Center,
            DrawAnchor.Center
        );
    }

    public class PartyFramesTrackerConfigConverter : PluginConfigObjectConverter
    {
        public PartyFramesTrackerConfigConverter()
        {
            SameTypeFieldConverter<Vector2> pos = new SameTypeFieldConverter<Vector2>("Icon.Position", Vector2.Zero);
            FieldConvertersMap.Add("Position", pos);

            SameTypeFieldConverter<Vector2> size = new SameTypeFieldConverter<Vector2>("Icon.Size", new Vector2(50, 50));
            FieldConvertersMap.Add("IconSize", size);

            SameTypeFieldConverter<DrawAnchor> anchor = new SameTypeFieldConverter<DrawAnchor>("Icon.Anchor", DrawAnchor.Center);
            FieldConvertersMap.Add("Anchor", anchor);

            SameTypeFieldConverter<DrawAnchor> frameAnchor = new SameTypeFieldConverter<DrawAnchor>("Icon.FrameAnchor", DrawAnchor.Center);
            FieldConvertersMap.Add("HealthBarAnchor", frameAnchor);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PartyFramesRaiseTrackerConfig) ||
                   objectType == typeof(PartyFramesInvulnTrackerConfig);
        }
    }

    [DisableParentSettings("Position", "Strata")]
    [Exportable(false)]
    public class PartyFramesCleanseTrackerConfig : MovablePluginConfigObject
    {
        public new static PartyFramesCleanseTrackerConfig DefaultConfig() { return new PartyFramesCleanseTrackerConfig(); }

        [Checkbox("Show only on jobs with cleanses", spacing = true)]
        [Order(10)]
        public bool CleanseJobsOnly = true;

        [Checkbox("Change Health Bar Color ", spacing = true)]
        [Order(15)]
        public bool ChangeHealthBarCleanseColor = true;

        [ColorEdit4("Health Bar Color")]
        [Order(20, collapseWith = nameof(ChangeHealthBarCleanseColor))]
        public PluginConfigColor HealthBarColor = new(new Vector4(255f / 255f, 0f / 255f, 104f / 255f, 100f / 100f));

        [Checkbox("Change Border Color", spacing = true)]
        [Order(25)]
        public bool ChangeBorderCleanseColor = true;

        [ColorEdit4("Border Color")]
        [Order(30, collapseWith = nameof(ChangeBorderCleanseColor))]
        public PluginConfigColor BorderColor = new(new Vector4(255f / 255f, 0f / 255f, 104f / 255f, 100f / 100f));
    }
}
