using DelvUI.Config;
using System;
using System.Numerics;

namespace DelvUI.Interface.StatusEffects
{
    [Serializable]
    public class StatusEffectsListConfig : MovablePluginConfigObject
    {
        public bool FillRowsFirst = true;
        public GrowthDirections GrowthDirections;
        public StatusEffectIconConfig IconConfig = new StatusEffectIconConfig();
        public Vector2 IconPadding = new(2, 2);
        public int Limit = -1;
        public bool ShowArea;
        public bool ShowBuffs;
        public bool ShowDebuffs;
        public bool ShowPermanentEffects;

        public StatusEffectsListConfig(
            Vector2 position,
            Vector2 size,
            bool showBuffs,
            bool showDebuffs,
            bool showPermanentEffects,
            GrowthDirections growthDirections,
            StatusEffectIconConfig iconConfig = null)
        {
            Position = position;
            Size = size;
            ShowBuffs = showBuffs;
            ShowDebuffs = showDebuffs;
            ShowPermanentEffects = showPermanentEffects;
            GrowthDirections = growthDirections;

            if (iconConfig != null)
            {
                IconConfig = iconConfig;
            }
        }
    }

    [Serializable]
    public class StatusEffectIconConfig : PluginConfigObject
    {
        public PluginConfigColor BorderColor = new(new Vector4(0f / 255f, 0 / 255f, 0 / 255f, 100f / 100f));
        public int BorderThickness = 1;
        public PluginConfigColor DispellableBorderColor = new(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));
        public int DispellableBorderThickness = 2;

        public bool ShowBorder = true;

        public bool ShowDispellableBorder = true;
        public bool ShowDurationText = true;
        public bool ShowStacksText = true;
        public Vector2 Size = new(40, 40);

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
