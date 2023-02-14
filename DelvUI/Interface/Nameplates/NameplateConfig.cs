using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Interface.Bars;
using DelvUI.Interface.StatusEffects;
using System;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    public enum NameplatesOcclusionMode
    {
        None = 0,
        Simple = 1,
        Full
    };

    [DisableParentSettings("Strata", "Position")]
    [Section("Nameplates")]
    [SubSection("General", 0)]
    public class NameplatesGeneralConfig : MovablePluginConfigObject
    {
        public new static NameplatesGeneralConfig DefaultConfig() => new NameplatesGeneralConfig();

        [Combo("Occlusion Mode", new string[] { "Disabled", "Simple", "Full" }, help = "This controls wheter you'll see nameplates through walls and objects.\n\nDisabled: Nameplates will always be seen for units in range.\nSimple: Uses simple calculations to check if a nameplate is being covered by walls or objects. Use this for better performance.\nFull: Uses more complex calculations to check if a nameplate is being covered by walls or objects. Use this for better results.")]
        [Order(10)]
        public NameplatesOcclusionMode OcclusionMode = NameplatesOcclusionMode.Full;

        [Checkbox("Try to keep nameplates on screen", help = "Disclaimer: DelvUI relies heavily on the the game's default nameplates so this setting won't be a huge improvement.\nThis setting tries to prevent nameplates from being cutoff in the border of the screen, but it won't keep showing nameplates that the game wouldn't.")]
        [Order(20)]
        public bool ClampToScreen = true;

        [Checkbox("Always show nameplate for target")]
        [Order(21)]
        public bool AlwaysShowTargetNameplate = true;
    }

    [DisableParentSettings("HideWhenInactive")]
    [Section("Nameplates")]
    [SubSection("Player", 0)]
    public class PlayerNameplateConfig : NameplateWithPlayerBarConfig
    {
        public PlayerNameplateConfig(
            Vector2 position,
            EditableLabelConfig nameLabel,
            EditableNonFormattableLabelConfig titleLabelConfig,
            NameplatePlayerBarConfig barConfig)
            : base(position, nameLabel, titleLabelConfig, barConfig)
        {
        }

        public new static PlayerNameplateConfig DefaultConfig()
        {
            return NameplatesHelper.GetNameplateWithBarConfig<PlayerNameplateConfig, NameplatePlayerBarConfig>(
                0xFFD0E5E0,
                0xFF30444A,
                HUDConstants.DefaultPlayerNameplateBarSize
            );
        }
    }

    [DisableParentSettings("HideWhenInactive", "TitleLabelConfig", "SwapLabelsWhenNeeded")]
    [Section("Nameplates")]
    [SubSection("Enemies", 0)]
    public class EnemyNameplateConfig : NameplateWithEnemyBarConfig
    {
        public EnemyNameplateConfig(
            Vector2 position,
            EditableLabelConfig nameLabel,
            EditableNonFormattableLabelConfig titleLabelConfig,
            NameplateEnemyBarConfig barConfig)
            : base(position, nameLabel, titleLabelConfig, barConfig)
        {
        }

        public new static EnemyNameplateConfig DefaultConfig()
        {
            EnemyNameplateConfig config = NameplatesHelper.GetNameplateWithBarConfig<EnemyNameplateConfig, NameplateEnemyBarConfig>(
                0xFF993535,
                0xFF000000,
                HUDConstants.DefaultEnemyNameplateBarSize
            );

            config.SwapLabelsWhenNeeded = false;

            config.NameLabelConfig.Position = new Vector2(-8, 0);
            config.NameLabelConfig.Text = "Lv[level] [name]";
            config.NameLabelConfig.FrameAnchor = DrawAnchor.TopRight;
            config.NameLabelConfig.TextAnchor = DrawAnchor.Right;
            config.NameLabelConfig.Color = PluginConfigColor.FromHex(0xFFFFFFFF);

            config.BarConfig.LeftLabelConfig.Enabled = true;
            config.BarConfig.OnlyShowWhenNotFull = false;

            // debuffs
            LabelConfig durationConfig = new LabelConfig(new Vector2(0, -4), "", DrawAnchor.Bottom, DrawAnchor.Center);
            durationConfig.FontID = FontsConfig.DefaultMediumFontKey;

            LabelConfig stacksConfig = new LabelConfig(new Vector2(-3, 4), "", DrawAnchor.TopRight, DrawAnchor.Center);
            durationConfig.FontID = FontsConfig.DefaultMediumFontKey;
            stacksConfig.Color = new(Vector4.UnitW);
            stacksConfig.OutlineColor = new(Vector4.One);

            StatusEffectIconConfig iconConfig = new StatusEffectIconConfig(durationConfig, stacksConfig);
            iconConfig.Size = new Vector2(30, 30);
            iconConfig.DispellableBorderConfig.Enabled = false;

            Vector2 pos = new Vector2(2, -20);
            Vector2 size = new Vector2(230, 70);

            EnemyNameplateStatusEffectsListConfig debuffs = new EnemyNameplateStatusEffectsListConfig(
                DrawAnchor.TopLeft,
                pos,
                size,
                false,
                true,
                false,
                GrowthDirections.Right | GrowthDirections.Up,
                iconConfig
            );
            debuffs.Limit = 7;
            debuffs.ShowPermanentEffects = true;
            debuffs.IconConfig.DispellableBorderConfig.Enabled = false;
            debuffs.IconPadding = new Vector2(1, 6);
            debuffs.ShowOnlyMine = true;
            debuffs.ShowTooltips = false;
            debuffs.DisableInteraction = true;
            config.DebuffsConfig = debuffs;

            // castbar
            Vector2 castbarSize = new Vector2(config.BarConfig.Size.X, 10);
            
            LabelConfig castNameConfig = new LabelConfig(new Vector2(0, -1), "", DrawAnchor.Center, DrawAnchor.Center);
            castNameConfig.FontID = FontsConfig.DefaultSmallFontKey;

            NumericLabelConfig castTimeConfig = new NumericLabelConfig(new Vector2(-5, 0), "", DrawAnchor.Right, DrawAnchor.Right);
            castTimeConfig.Enabled = false;
            castTimeConfig.FontID = FontsConfig.DefaultSmallFontKey;
            castTimeConfig.NumberFormat = 1;

            NameplateCastbarConfig castbarConfig = new NameplateCastbarConfig(Vector2.Zero, castbarSize, castNameConfig, castTimeConfig);
            castbarConfig.HealthBarAnchor = DrawAnchor.BottomLeft;
            castbarConfig.Anchor = DrawAnchor.TopLeft;
            castbarConfig.ShowIcon = false;
            config.CastbarConfig = castbarConfig;          

            return config;
        }
    }

    [DisableParentSettings("HideWhenInactive")]
    [Section("Nameplates")]
    [SubSection("Party Members", 0)]
    public class PartyMembersNameplateConfig : NameplateWithPlayerBarConfig
    {
        public PartyMembersNameplateConfig(
            Vector2 position,
            EditableLabelConfig nameLabel,
            EditableNonFormattableLabelConfig titleLabelConfig,
            NameplatePlayerBarConfig barConfig)
            : base(position, nameLabel, titleLabelConfig, barConfig)
        {
        }

        public new static PartyMembersNameplateConfig DefaultConfig()
        {
            PartyMembersNameplateConfig config = NameplatesHelper.GetNameplateWithBarConfig<PartyMembersNameplateConfig, NameplatePlayerBarConfig>(
                0xFFD0E5E0,
                0xFF000000,
                HUDConstants.DefaultPlayerNameplateBarSize
            );

            config.BarConfig.UseRoleColor = true;
            config.NameLabelConfig.UseRoleColor = true;
            config.TitleLabelConfig.UseRoleColor = true;
            return config;
        }
    }

    [DisableParentSettings("HideWhenInactive")]
    [Section("Nameplates")]
    [SubSection("Alliance Members", 0)]
    public class AllianceMembersNameplateConfig : NameplateWithPlayerBarConfig
    {
        public AllianceMembersNameplateConfig(
            Vector2 position,
            EditableLabelConfig nameLabel,
            EditableNonFormattableLabelConfig titleLabelConfig,
            NameplatePlayerBarConfig barConfig)
            : base(position, nameLabel, titleLabelConfig, barConfig)
        {
        }

        public new static AllianceMembersNameplateConfig DefaultConfig()
        {
            return NameplatesHelper.GetNameplateWithBarConfig<AllianceMembersNameplateConfig, NameplatePlayerBarConfig>(
                0xFF99BE46,
                0xFF3D4C1C,
                HUDConstants.DefaultPlayerNameplateBarSize
            );
        }
    }

    [DisableParentSettings("HideWhenInactive")]
    [Section("Nameplates")]
    [SubSection("Friends", 0)]
    public class FriendPlayerNameplateConfig : NameplateWithPlayerBarConfig
    {
        public FriendPlayerNameplateConfig(
            Vector2 position,
            EditableLabelConfig nameLabel,
            EditableNonFormattableLabelConfig titleLabelConfig,
            NameplatePlayerBarConfig barConfig)
            : base(position, nameLabel, titleLabelConfig, barConfig)
        {
        }

        public new static FriendPlayerNameplateConfig DefaultConfig()
        {
            return NameplatesHelper.GetNameplateWithBarConfig<FriendPlayerNameplateConfig, NameplatePlayerBarConfig>(
                0xFFEB6211,
                0xFF4A2008,
                HUDConstants.DefaultPlayerNameplateBarSize
            );
        }
    }

    [DisableParentSettings("HideWhenInactive")]
    [Section("Nameplates")]
    [SubSection("Other Players", 0)]
    public class OtherPlayerNameplateConfig : NameplateWithPlayerBarConfig
    {
        public OtherPlayerNameplateConfig(
            Vector2 position,
            EditableLabelConfig nameLabel,
            EditableNonFormattableLabelConfig titleLabelConfig,
            NameplatePlayerBarConfig barConfig)
            : base(position, nameLabel, titleLabelConfig, barConfig)
        {
        }

        public new static OtherPlayerNameplateConfig DefaultConfig()
        {
            return NameplatesHelper.GetNameplateWithBarConfig<OtherPlayerNameplateConfig, NameplatePlayerBarConfig>(
                0xFF91BBD8,
                0xFF33434E,
                HUDConstants.DefaultPlayerNameplateBarSize
            );
        }
    }

    [DisableParentSettings("HideWhenInactive")]
    [Section("Nameplates")]
    [SubSection("Pets", 0)]
    public class PetNameplateConfig : NameplateWithNPCBarConfig
    {
        public PetNameplateConfig(
            Vector2 position,
            EditableLabelConfig nameLabel,
            EditableNonFormattableLabelConfig titleLabelConfig,
            NameplateBarConfig barConfig)
            : base(position, nameLabel, titleLabelConfig, barConfig)
        {
        }

        public new static PetNameplateConfig DefaultConfig()
        {
            PetNameplateConfig config = NameplatesHelper.GetNameplateWithBarConfig<PetNameplateConfig, NameplateBarConfig>(
                0xFFD1E5C8,
                0xFF2A2F28,
                HUDConstants.DefaultPlayerNameplateBarSize
            );
            config.OnlyShowWhenTargeted = true;
            config.SwapLabelsWhenNeeded = false;
            config.NameLabelConfig.Text = "Lv[level] [name]";
            config.NameLabelConfig.FontID = FontsConfig.DefaultSmallFontKey;
            config.TitleLabelConfig.FontID = FontsConfig.DefaultSmallFontKey;

            return config;
        }
    }

    [DisableParentSettings("HideWhenInactive")]
    [Section("Nameplates")]
    [SubSection("NPCs", 0)]
    public class NPCNameplateConfig : NameplateWithNPCBarConfig
    {
        public NPCNameplateConfig(
            Vector2 position,
            EditableLabelConfig nameLabel,
            EditableNonFormattableLabelConfig titleLabelConfig,
            NameplateBarConfig barConfig)
            : base(position, nameLabel, titleLabelConfig, barConfig)
        {
        }

        public new static NPCNameplateConfig DefaultConfig()
        {
            NPCNameplateConfig config = NameplatesHelper.GetNameplateWithBarConfig<NPCNameplateConfig, NameplateBarConfig>(
                0xFFD1E5C8, 
                0xFF3A4b1E,
                HUDConstants.DefaultPlayerNameplateBarSize
            );
            config.NameLabelConfig.Position = new Vector2(0, -20);
            config.TitleLabelConfig.Position = Vector2.Zero;

            return config;
        }
    }

    [DisableParentSettings("HideWhenInactive", "SwapLabelsWhenNeeded")]
    [Section("Nameplates")]
    [SubSection("Minions", 0)]
    public class MinionNPCNameplateConfig : NameplateConfig
    {
        public MinionNPCNameplateConfig(
            Vector2 position,
            EditableLabelConfig nameLabel,
            EditableNonFormattableLabelConfig titleLabelConfig)
            : base(position, nameLabel, titleLabelConfig)
        {
        }

        public new static MinionNPCNameplateConfig DefaultConfig()
        {
            MinionNPCNameplateConfig config = NameplatesHelper.GetNameplateConfig<MinionNPCNameplateConfig>(0xFFFFFFFF, 0xFF000000);
            config.OnlyShowWhenTargeted = true;
            config.SwapLabelsWhenNeeded = false;
            config.NameLabelConfig.Position = new Vector2(0, -17);
            config.NameLabelConfig.FontID = FontsConfig.DefaultSmallFontKey;
            config.TitleLabelConfig.Position = new Vector2(0, 0);
            config.TitleLabelConfig.FontID = FontsConfig.DefaultSmallFontKey;

            return config;
        }
    }

    [DisableParentSettings("HideWhenInactive", "SwapLabelsWhenNeeded")]
    [Section("Nameplates")]
    [SubSection("Objects", 0)]
    public class ObjectsNameplateConfig : NameplateConfig
    {
        public ObjectsNameplateConfig(
            Vector2 position,
            EditableLabelConfig nameLabel,
            EditableNonFormattableLabelConfig titleLabelConfig)
            : base(position, nameLabel, titleLabelConfig)
        {
        }

        public new static ObjectsNameplateConfig DefaultConfig()
        {
            ObjectsNameplateConfig config = NameplatesHelper.GetNameplateConfig<ObjectsNameplateConfig>(0xFFFFFFFF, 0xFF000000);
            config.SwapLabelsWhenNeeded = false;

            return config;
        }
    }

    public class NameplateConfig : MovablePluginConfigObject
    {
        [Checkbox("Only show when targeted")]
        [Order(1)]
        public bool OnlyShowWhenTargeted = false;

        [Checkbox("Swap Name and Title labels when needed", spacing = true, help = "This will swap the contents of these labels depending on if the title goes before or after the name of a player.")]
        [Order(20)]
        public bool SwapLabelsWhenNeeded = true;

        [NestedConfig("Name Label", 21)]
        public EditableLabelConfig NameLabelConfig = null!;

        [NestedConfig("Title Label", 22)]
        public EditableNonFormattableLabelConfig TitleLabelConfig = null!;

        [NestedConfig("Change Alpha Based on Range", 145)]
        public NameplateRangeConfig RangeConfig = new();

        [NestedConfig("Visibility", 200)]
        public VisibilityConfig VisibilityConfig = new VisibilityConfig();

        public NameplateConfig(Vector2 position, EditableLabelConfig nameLabelConfig, EditableNonFormattableLabelConfig titleLabelConfig)
            : base()
        {
            Position = position;
            NameLabelConfig = nameLabelConfig;
            TitleLabelConfig = titleLabelConfig;
        }

        public NameplateConfig() : base() { } // don't remove
    }

    public interface NameplateWithBarConfig
    {
        public NameplateBarConfig GetBarConfig();
    }

    public class NameplateWithNPCBarConfig : NameplateConfig, NameplateWithBarConfig
    {
        [NestedConfig("Health Bar", 40)]
        public NameplateBarConfig BarConfig = null!;

        public NameplateBarConfig GetBarConfig() => BarConfig;

        public NameplateWithNPCBarConfig(
            Vector2 position,
            EditableLabelConfig nameLabel,
            EditableNonFormattableLabelConfig titleLabelConfig,
            NameplateBarConfig barConfig)
            : base(position, nameLabel, titleLabelConfig)
        {
            BarConfig = barConfig;
        }

        public NameplateWithNPCBarConfig() : base() { } // don't remove
    }

    public class NameplateWithPlayerBarConfig : NameplateConfig, NameplateWithBarConfig
    {
        [NestedConfig("Health Bar", 40)]
        public NameplatePlayerBarConfig BarConfig = null!;

        [NestedConfig("Role/Job Icon", 50)]
        public NameplateRoleJobIconConfig RoleIconConfig = new NameplateRoleJobIconConfig(
            new Vector2(-5, 0),
            new Vector2(30, 30),
            DrawAnchor.Right,
            DrawAnchor.Left
        )
        { Strata = StrataLevel.LOWEST };

        [NestedConfig("Player State Icon", 55)]
        public NameplateIconConfig StateIconConfig = new NameplateIconConfig(
            new Vector2(5, 0),
            new Vector2(30, 30),
            DrawAnchor.Left,
            DrawAnchor.Right
        )
        { Strata = StrataLevel.LOWEST };

        public NameplateBarConfig GetBarConfig() => BarConfig;

        public NameplateWithPlayerBarConfig(
            Vector2 position,
            EditableLabelConfig nameLabel,
            EditableNonFormattableLabelConfig titleLabelConfig,
            NameplatePlayerBarConfig barConfig)
            : base(position, nameLabel, titleLabelConfig)
        {
            BarConfig = barConfig;
        }

        public NameplateWithPlayerBarConfig() : base() { } // don't remove
    }

    public class NameplateWithEnemyBarConfig : NameplateConfig, NameplateWithBarConfig
    {
        [NestedConfig("Health Bar", 40)]
        public NameplateEnemyBarConfig BarConfig = null!;

        [NestedConfig("Icon", 45)]
        public NameplateIconConfig IconConfig = new NameplateIconConfig(
            new Vector2(0, 0),
            new Vector2(40, 40),
            DrawAnchor.Right,
            DrawAnchor.Left
        )
        { PrioritizeHealthBarAnchor = true, Strata = StrataLevel.LOWEST };

        [NestedConfig("Debuffs", 50)]
        public EnemyNameplateStatusEffectsListConfig DebuffsConfig = null!;

        [NestedConfig("Castbar", 55)]
        public NameplateCastbarConfig CastbarConfig = null!;

        public NameplateBarConfig GetBarConfig() => BarConfig;

        public NameplateWithEnemyBarConfig(
            Vector2 position,
            EditableLabelConfig nameLabel,
            EditableNonFormattableLabelConfig titleLabelConfig,
            NameplateEnemyBarConfig barConfig)
            : base(position, nameLabel, titleLabelConfig)
        {
            BarConfig = barConfig;
        }

        public NameplateWithEnemyBarConfig() : base() { } // don't remove
    }

    [DisableParentSettings("HideWhenInactive")]
    public class NameplateBarConfig : BarConfig
    {
        [Checkbox("Only Show when not at full Health")]
        [Order(1)]
        public bool OnlyShowWhenNotFull = true;
        
        [Checkbox("Hide Health when fully depleted", help = "This will hide the healthbar when the characters HP has been brought to zero")]
        [Order(2)]
        public bool HideHealthAtZero = true;

        [ColorEdit4("Targeted Border Color")]
        [Order(38, collapseWith = nameof(DrawBorder))]
        public PluginConfigColor TargetedBorderColor = PluginConfigColor.FromHex(0xFFFFFFFF);

        [DragInt("Targeted Border Thickness", min = 1, max = 10)]
        [Order(39, collapseWith = nameof(DrawBorder))]
        public int TargetedBorderThickness = 2;

        [NestedConfig("Color Based On Health Value", 50, collapsingHeader = false)]
        public ColorByHealthValueConfig ColorByHealth = new ColorByHealthValueConfig();

        [Checkbox("Hide Health if Possible", spacing = true, help = "This will hide any label that has a health tag if the character doesn't have health (ie minions, friendly npcs, etc)")]
        [Order(121)]
        public bool HideHealthIfPossible = true;

        [NestedConfig("Left Text", 125)]
        public EditableLabelConfig LeftLabelConfig = null!;

        [NestedConfig("Right Text", 130)]
        public EditableLabelConfig RightLabelConfig = null!;

        [NestedConfig("Optional Text", 131)]
        public EditableLabelConfig OptionalLabelConfig = null!;

        [NestedConfig("Shields", 140)]
        public ShieldConfig ShieldConfig = new ShieldConfig();

        [NestedConfig("Custom Mouseover Area", 150)]
        public MouseoverAreaConfig MouseoverAreaConfig = new MouseoverAreaConfig();

        public NameplateBarConfig(Vector2 position, Vector2 size, EditableLabelConfig leftLabelConfig, EditableLabelConfig rightLabelConfig, EditableLabelConfig optionalLabelConfig)
            : base(position, size, new PluginConfigColor(new(40f / 255f, 40f / 255f, 40f / 255f, 100f / 100f)))
        {
            Position = position;
            Size = size;
            LeftLabelConfig = leftLabelConfig;
            RightLabelConfig = rightLabelConfig;
            OptionalLabelConfig = optionalLabelConfig;
            BackgroundColor = new PluginConfigColor(new(0f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));
            ColorByHealth.Enabled = false;
            MouseoverAreaConfig.Enabled = false;
        }

        public bool IsVisible(uint hp, uint maxHp)
        {
            return Enabled && (!OnlyShowWhenNotFull || hp < maxHp) && !(HideHealthAtZero && hp <= 0);
        }

        public NameplateBarConfig() : base(Vector2.Zero, Vector2.Zero, new(Vector4.Zero)) { } // don't remove
    }

    public class NameplatePlayerBarConfig : NameplateBarConfig
    {
        [Checkbox("Use Job Color", spacing = true)]
        [Order(45)]
        public bool UseJobColor = false;

        [Checkbox("Use Role Color")]
        [Order(46)]
        public bool UseRoleColor = false;

        [Checkbox("Job Color As Background Color", spacing = true)]
        [Order(50)]
        public bool UseJobColorAsBackgroundColor = false;

        [Checkbox("Role Color As Background Color")]
        [Order(51)]
        public bool UseRoleColorAsBackgroundColor = false;

        public NameplatePlayerBarConfig(Vector2 position, Vector2 size, EditableLabelConfig leftLabelConfig, EditableLabelConfig rightLabelConfig, EditableLabelConfig optionalLabelConfig)
            : base(position, size, leftLabelConfig, rightLabelConfig, optionalLabelConfig)
        {
        }
    }

    public class NameplateEnemyBarConfig : NameplateBarConfig
    {
        [Checkbox("Use State Colors", spacing = true)]
        [Order(45)]
        public bool UseStateColor = true;

        [ColorEdit4("Out of Combat")]
        [Order(46, collapseWith = nameof(UseStateColor))]
        public PluginConfigColor OutOfCombatColor = PluginConfigColor.FromHex(0xFFDA9D2E);

        [ColorEdit4("Out of Combat (Hostile)")]
        [Order(46, collapseWith = nameof(UseStateColor))]
        public PluginConfigColor OutOfCombatHostileColor = PluginConfigColor.FromHex(0xFF994B35);

        [ColorEdit4("In Combat")]
        [Order(46, collapseWith = nameof(UseStateColor))]
        public PluginConfigColor InCombatColor = PluginConfigColor.FromHex(0xFF993535);

        [NestedConfig("Order Label", 132)]
        public DefaultFontLabelConfig OrderLabelConfig = new DefaultFontLabelConfig(new Vector2(5, 0), "", DrawAnchor.Right, DrawAnchor.Left)
        {
            Strata = StrataLevel.LOWEST
        };

        public NameplateEnemyBarConfig(Vector2 position, Vector2 size, EditableLabelConfig leftLabelConfig, EditableLabelConfig rightLabelConfig, EditableLabelConfig optionalLabelConfig)
            : base(position, size, leftLabelConfig, rightLabelConfig, optionalLabelConfig)
        {

        }
    }

    [Exportable(false)]
    public class NameplateRangeConfig : PluginConfigObject
    {
        [DragInt("Fade start range (yalms)", min = 1, max = 500)]
        [Order(5)]
        public int StartRange = 50;

        [DragInt("Fade end range (yalms)", min = 1, max = 500)]
        [Order(10)]
        public int EndRange = 64;

        public float AlphaForDistance(float distance, float maxAlpha = 1f)
        {
            float diff = distance - StartRange;
            if (!Enabled || diff <= 0)
            {
                return maxAlpha;
            }

            float a = diff / (EndRange - StartRange);
            return Math.Max(0, Math.Min(maxAlpha, 1 - a));
        }
    }

    public class EnemyNameplateStatusEffectsListConfig : StatusEffectsListConfig
    {
        [Anchor("Health Bar Anchor")]
        [Order(4)]
        public DrawAnchor HealthBarAnchor = DrawAnchor.BottomLeft;

        public EnemyNameplateStatusEffectsListConfig(DrawAnchor anchor, Vector2 position, Vector2 size, bool showBuffs, bool showDebuffs, bool showPermanentEffects,
            GrowthDirections growthDirections, StatusEffectIconConfig iconConfig)
            : base(position, size, showBuffs, showDebuffs, showPermanentEffects, growthDirections, iconConfig)
        {
            HealthBarAnchor = anchor;
        }
    }

    [DisableParentSettings("AnchorToUnitFrame", "UnitFrameAnchor", "HideWhenInactive", "FillDirection")]
    public class NameplateCastbarConfig : TargetCastbarConfig
    {
        [Anchor("Health Bar Anchor")]
        [Order(16)]
        public DrawAnchor HealthBarAnchor = DrawAnchor.BottomLeft;

        public NameplateCastbarConfig(Vector2 position, Vector2 size, LabelConfig castNameConfig, NumericLabelConfig castTimeConfig)
            : base(position, size, castNameConfig, castTimeConfig)
        {

        }
    }

    internal static class NameplatesHelper
    {
        internal static T GetNameplateConfig<T>(uint bgColor, uint borderColor) where T : NameplateConfig
        {
            EditableLabelConfig nameLabelConfig = new EditableLabelConfig(new Vector2(0, 0), "[name]", DrawAnchor.Top, DrawAnchor.Bottom)
            {
                Color = PluginConfigColor.FromHex(bgColor),
                OutlineColor = PluginConfigColor.FromHex(borderColor),
                FontID = FontsConfig.DefaultMediumFontKey
            };

            EditableNonFormattableLabelConfig titleLabelConfig = new EditableNonFormattableLabelConfig(new Vector2(0, -25), "<[title]>", DrawAnchor.Top, DrawAnchor.Bottom)
            {
                Color = PluginConfigColor.FromHex(bgColor),
                OutlineColor = PluginConfigColor.FromHex(borderColor),
                FontID = FontsConfig.DefaultMediumFontKey
            };

            return (T)Activator.CreateInstance(typeof(T), Vector2.Zero, nameLabelConfig, titleLabelConfig)!;
        }

        internal static T GetNameplateWithBarConfig<T, B>(uint bgColor, uint borderColor, Vector2 barSize)
            where T : NameplateConfig
            where B : NameplateBarConfig
        {
            EditableLabelConfig leftLabelConfig = new EditableLabelConfig(new Vector2(5, 0), "[health:current-short]", DrawAnchor.Left, DrawAnchor.Left)
            {
                Enabled = false,
                FontID = FontsConfig.DefaultMediumFontKey,
                Strata = StrataLevel.LOWEST
            };
            EditableLabelConfig rightLabelConfig = new EditableLabelConfig(new Vector2(-5, 0), "", DrawAnchor.Right, DrawAnchor.Right)
            {
                Enabled = false,
                FontID = FontsConfig.DefaultMediumFontKey,
                Strata = StrataLevel.LOWEST
            };
            EditableLabelConfig optionalLabelConfig = new EditableLabelConfig(new Vector2(0, 0), "", DrawAnchor.Center, DrawAnchor.Center)
            {
                Enabled = false,
                FontID = FontsConfig.DefaultSmallFontKey,
                Strata = StrataLevel.LOWEST
            };

            var barConfig = Activator.CreateInstance(typeof(B), new Vector2(0, -5), barSize, leftLabelConfig, rightLabelConfig, optionalLabelConfig)!;
            if (barConfig is BarConfig bar)
            {
                bar.FillColor = PluginConfigColor.FromHex(bgColor);
                bar.BackgroundColor = PluginConfigColor.FromHex(0xAA000000);
            }

            EditableLabelConfig nameLabelConfig = new EditableLabelConfig(new Vector2(0, -20), "[name]", DrawAnchor.Top, DrawAnchor.Bottom)
            {
                Color = PluginConfigColor.FromHex(bgColor),
                OutlineColor = PluginConfigColor.FromHex(borderColor),
                FontID = FontsConfig.DefaultMediumFontKey,
                Strata = StrataLevel.LOWEST
            };
            EditableNonFormattableLabelConfig titleLabelConfig = new EditableNonFormattableLabelConfig(new Vector2(0, 0), "<[title]>", DrawAnchor.Top, DrawAnchor.Bottom)
            {
                Color = PluginConfigColor.FromHex(bgColor),
                OutlineColor = PluginConfigColor.FromHex(borderColor),
                FontID = FontsConfig.DefaultMediumFontKey,
                Strata = StrataLevel.LOWEST
            };

            return (T)Activator.CreateInstance(typeof(T), Vector2.Zero, nameLabelConfig, titleLabelConfig, barConfig)!;
        }
    }
}
