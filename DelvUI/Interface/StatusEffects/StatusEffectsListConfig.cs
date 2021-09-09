using DelvUI.Config;
using DelvUI.Config.Attributes;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DelvUI.Interface.StatusEffects
{
    [Serializable]
    [Section("Buffs / Debuffs")]
    [SubSection("Player Buffs", 0)]
    public class PlayerBuffsListConfig : StatusEffectsListConfig
    {
        public new static PlayerBuffsListConfig DefaultConfig()
        {
            var screenSize = ImGui.GetMainViewport().Size;
            var pos = new Vector2(screenSize.X * 0.38f, -screenSize.Y * 0.45f);
            var iconConfig = new StatusEffectIconConfig();

            return new PlayerBuffsListConfig(pos, HUDConstants.DefaultStatusEffectsListSize, true, false, true, GrowthDirections.Left | GrowthDirections.Down, iconConfig);
        }

        public PlayerBuffsListConfig(Vector2 position, Vector2 size, bool showBuffs, bool showDebuffs, bool showPermanentEffects,
            GrowthDirections growthDirections, StatusEffectIconConfig iconConfig)
            : base(position, size, showBuffs, showDebuffs, showPermanentEffects, growthDirections, iconConfig)
        {
        }
    }

    [Serializable]
    [Section("Buffs / Debuffs")]
    [SubSection("Player Debuffs", 0)]
    public class PlayerDebuffsListConfig : StatusEffectsListConfig
    {
        public new static PlayerDebuffsListConfig DefaultConfig()
        {
            var screenSize = ImGui.GetMainViewport().Size;
            var pos = new Vector2(screenSize.X * 0.38f, -screenSize.Y * 0.45f + HUDConstants.DefaultStatusEffectsListSize.Y);
            var iconConfig = new StatusEffectIconConfig();

            return new PlayerDebuffsListConfig(pos, HUDConstants.DefaultStatusEffectsListSize, false, true, true, GrowthDirections.Left | GrowthDirections.Down, iconConfig);
        }

        public PlayerDebuffsListConfig(Vector2 position, Vector2 size, bool showBuffs, bool showDebuffs, bool showPermanentEffects,
            GrowthDirections growthDirections, StatusEffectIconConfig iconConfig)
            : base(position, size, showBuffs, showDebuffs, showPermanentEffects, growthDirections, iconConfig)
        {
        }
    }

    [Serializable]
    [Section("Buffs / Debuffs")]
    [SubSection("Target Buffs", 0)]
    public class TargetBuffsListConfig : StatusEffectsListConfig
    {
        public new static TargetBuffsListConfig DefaultConfig()
        {
            var pos = new Vector2(HUDConstants.UnitFramesOffsetX, HUDConstants.BaseHUDOffsetY - 50);
            var iconConfig = new StatusEffectIconConfig();

            return new TargetBuffsListConfig(pos, HUDConstants.DefaultStatusEffectsListSize, true, false, true, GrowthDirections.Right | GrowthDirections.Up, iconConfig);
        }

        public TargetBuffsListConfig(Vector2 position, Vector2 size, bool showBuffs, bool showDebuffs, bool showPermanentEffects,
            GrowthDirections growthDirections, StatusEffectIconConfig iconConfig)
            : base(position, size, showBuffs, showDebuffs, showPermanentEffects, growthDirections, iconConfig)
        {
        }
    }

    [Serializable]
    [Section("Buffs / Debuffs")]
    [SubSection("Target Debuffs", 0)]
    public class TargetDebuffsListConfig : StatusEffectsListConfig
    {
        public new static TargetDebuffsListConfig DefaultConfig()
        {
            var pos = new Vector2(HUDConstants.UnitFramesOffsetX, HUDConstants.BaseHUDOffsetY - HUDConstants.DefaultStatusEffectsListSize.Y - 50);
            var iconConfig = new StatusEffectIconConfig();

            return new TargetDebuffsListConfig(pos, HUDConstants.DefaultStatusEffectsListSize, false, true, true, GrowthDirections.Right | GrowthDirections.Up, iconConfig);
        }

        public TargetDebuffsListConfig(Vector2 position, Vector2 size, bool showBuffs, bool showDebuffs, bool showPermanentEffects,
            GrowthDirections growthDirections, StatusEffectIconConfig iconConfig)
            : base(position, size, showBuffs, showDebuffs, showPermanentEffects, growthDirections, iconConfig)
        {
        }
    }

    [Serializable]
    public class StatusEffectsListConfig : MovablePluginConfigObject
    {
        [Checkbox("Fill Rows First")]
        [Order(20)]
        public bool FillRowsFirst = true;

        [Combo("Icons Growth Direction",
            "Right and Down",
            "Right and Up",
            "Left and Down",
            "Left and Up",
            "Centered (horizontal)",
            "Centered (vertical)"
        )]
        [Order(25)]
        public int Directions;

        [NestedConfig("Icons", 30)]
        public StatusEffectIconConfig IconConfig;

        [DragInt2("Icon Padding", min = 0, max = 100)]
        [Order(35)]
        public Vector2 IconPadding = new(2, 2);

        [DragInt("Limit (-1 for not limit)", min = -1, max = 1000)]
        [Order(40)]
        public int Limit = -1;

        [Checkbox("Show Area")]
        [Order(45)]
        public bool ShowArea;

        [Checkbox("Show Buffs")]
        [Order(50)]
        public bool ShowBuffs;

        [Checkbox("Show Debuffs")]
        [Order(55)]
        public bool ShowDebuffs;

        [Checkbox("Show Permanent Effects")]
        [Order(60)]
        public bool ShowPermanentEffects;

        public StatusEffectsListConfig(Vector2 position, Vector2 size, bool showBuffs, bool showDebuffs, bool showPermanentEffects,
            GrowthDirections growthDirections, StatusEffectIconConfig iconConfig)
        {
            Position = position;
            Size = size;
            ShowBuffs = showBuffs;
            ShowDebuffs = showDebuffs;
            ShowPermanentEffects = showPermanentEffects;

            SetGrowthDirections(growthDirections);

            IconConfig = iconConfig;
        }

        private void SetGrowthDirections(GrowthDirections growthDirections)
        {
            var index = DirectionOptionsValues.FindIndex(d => d == growthDirections);
            if (index > 0)
            {
                Directions = index;
            }
        }

        public GrowthDirections GetGrowthDirections()
        {
            if (Directions > 0 && Directions < DirectionOptionsValues.Count)
            {
                return DirectionOptionsValues[Directions];
            }

            return DirectionOptionsValues[0];
        }

        [JsonIgnore]
        internal List<GrowthDirections> DirectionOptionsValues = new List<GrowthDirections>()
        {
            GrowthDirections.Right | GrowthDirections.Down,
            GrowthDirections.Right | GrowthDirections.Up,
            GrowthDirections.Left | GrowthDirections.Down,
            GrowthDirections.Left | GrowthDirections.Up,
            GrowthDirections.Out | GrowthDirections.Right,
            GrowthDirections.Out | GrowthDirections.Down
        };
    }

    [Serializable]
    [Portable(false)]
    public class StatusEffectIconConfig : PluginConfigObject
    {
        [DragInt2("Size", min = 1, max = 1000)]
        [Order(0)]
        public Vector2 Size = new(40, 40);

        [Checkbox("Show Duration")]
        [Order(5)]
        public bool ShowDurationText = true;

        [Checkbox("Show Stacks")]
        [Order(10)]
        public bool ShowStacksText = true;

        [Checkbox("Show Border")]
        [CollapseControl(15, 0)]
        public bool ShowBorder = true;

        [ColorEdit4("Border Color")]
        [CollapseWith(0, 0)]
        public PluginConfigColor BorderColor = new(new Vector4(0f / 255f, 0 / 255f, 0 / 255f, 100f / 100f));

        [DragInt("Thickness", min = 1, max = 100)]
        [CollapseWith(5, 0)]
        public int BorderThickness = 1;

        [Checkbox("Show Dispellable Border")]
        [CollapseControl(20, 1)]
        public bool ShowDispellableBorder = true;

        [ColorEdit4("Color ##DispellableColor")]
        [CollapseWith(0, 1)]
        public PluginConfigColor DispellableBorderColor = new(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [DragInt("Thickness ##DispellableThickness", min = 1, max = 100)]
        [CollapseWith(5, 1)]
        public int DispellableBorderThickness = 2;

        public StatusEffectIconConfig()
        {

        }

        public StatusEffectIconConfig(Vector2 size, bool showDurationText, bool showStacksText, bool showBorder, bool showDispellableBorder)
        {
            Size = size;
            ShowDurationText = showDurationText;
            ShowStacksText = showStacksText;
            ShowBorder = showBorder;
            ShowDispellableBorder = showDispellableBorder;
        }
    }
}
