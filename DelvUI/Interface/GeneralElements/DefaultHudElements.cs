﻿using DelvUI.Interface.StatusEffects;
using ImGuiNET;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    public static class DefaultHudElements
    {
        // unit frames
        internal static Vector2 DefaultBigUnitFrameSize = new Vector2(270, 50);
        internal static Vector2 DefaultSmallUnitFrameSize = new Vector2(120, 20);

        public static UnitFrameConfig PlayerUnitFrame()
        {
            var size = DefaultBigUnitFrameSize;
            var pos = new Vector2(-HUDConstants.UnitFramesOffsetX - size.X / 2f, HUDConstants.BaseHUDOffsetY);

            var leftLabelConfig = new EditableLabelConfig(new Vector2(-size.X / 2f + 5, -size.Y / 2f + 2), "[name:abbreviate]", LabelTextAnchor.BottomLeft);
            var rightLabelConfig = new EditableLabelConfig(new Vector2(size.X / 2 + 10, -size.Y / 2f + 2), "[health:current-short] | [health:percent]%", LabelTextAnchor.BottomRight);

            var config = new UnitFrameConfig(pos, size, leftLabelConfig, rightLabelConfig, "Player Unit Frame");
            config.TankStanceIndicatorConfig = new TankStanceIndicatorConfig();

            return config;
        }

        public static UnitFrameConfig TargetUnitFrame()
        {
            var size = DefaultBigUnitFrameSize;
            var pos = new Vector2(HUDConstants.UnitFramesOffsetX + size.X / 2f, HUDConstants.BaseHUDOffsetY);

            var leftLabelConfig = new EditableLabelConfig(new Vector2(-size.X / 2f + 5, -size.Y / 2f + 2), "[health:current-short] | [health:percent]%", LabelTextAnchor.BottomLeft);
            var rightLabelConfig = new EditableLabelConfig(new Vector2(size.X / 2 - 5, -size.Y / 2 + 2), "[name:abbreviate]", LabelTextAnchor.BottomRight);

            return new UnitFrameConfig(pos, size, leftLabelConfig, rightLabelConfig, "Target Unit Frame");
        }

        public static UnitFrameConfig TargetOfTargetUnitFrame()
        {
            var size = DefaultSmallUnitFrameSize;
            var pos = new Vector2(
                HUDConstants.UnitFramesOffsetX + DefaultBigUnitFrameSize.X + 6 + size.X / 2f,
                HUDConstants.BaseHUDOffsetY - 15
            );

            var leftLabelConfig = new EditableLabelConfig(new Vector2(0, -size.Y / 2f + 2), "[name:abbreviate]", LabelTextAnchor.Bottom);
            var rightLabelConfig = new EditableLabelConfig(new Vector2(0, size.Y / 2f), "", LabelTextAnchor.Top);

            return new UnitFrameConfig(pos, size, leftLabelConfig, rightLabelConfig, "Target of Target Unit Frame");
        }

        public static UnitFrameConfig FocusTargetUnitFrame()
        {
            var size = DefaultSmallUnitFrameSize;
            var pos = new Vector2(
                -HUDConstants.UnitFramesOffsetX - DefaultBigUnitFrameSize.X - 6 - size.X / 2f,
                HUDConstants.BaseHUDOffsetY - 15
            );

            var leftLabelConfig = new EditableLabelConfig(new Vector2(0, -size.Y / 2f + 2), "[name:abbreviate]", LabelTextAnchor.Bottom);
            var rightLabelConfig = new EditableLabelConfig(new Vector2(0, size.Y / 2f), "", LabelTextAnchor.Top);

            return new UnitFrameConfig(pos, size, leftLabelConfig, rightLabelConfig, "Focus Target Unit Frame");
        }

        // cast bars
        internal static Vector2 DefaultCastbarSize => new Vector2(254, 25);

        public static PlayerCastbarConfig PlayerCastbar()
        {
            var size = DefaultCastbarSize;
            var pos = new Vector2(0, HUDConstants.BaseHUDOffsetY - size.Y / 2f);

            var castNameConfig = new LabelConfig(new Vector2(5, 0), "", LabelTextAnchor.Left);
            var castTimeConfig = new LabelConfig(new Vector2(size.X / 2f - 5, 0), "", LabelTextAnchor.Right);

            return new PlayerCastbarConfig(pos, size, castNameConfig, castTimeConfig, "Player Castbar");
        }

        public static TargetCastbarConfig TargetCastbar()
        {
            var size = DefaultCastbarSize;
            var pos = new Vector2(0, HUDConstants.BaseHUDOffsetY / 2f - size.Y / 2);

            var castNameConfig = new LabelConfig(new Vector2(5, 0), "", LabelTextAnchor.Left);
            var castTimeConfig = new LabelConfig(new Vector2(size.X / 2f - 5, 0), "", LabelTextAnchor.Right);

            return new TargetCastbarConfig(pos, size, castNameConfig, castTimeConfig, "Target Castbar");
        }

        // status effects list
        internal static Vector2 DefaultStatusEffectsListSize = new Vector2(292, 82);

        public static StatusEffectsListConfig PlayerBuffsList()
        {
            var screenSize = ImGui.GetMainViewport().Size;
            var pos = new Vector2(screenSize.X * 0.38f, -screenSize.Y * 0.45f);

            return new StatusEffectsListConfig(pos, DefaultStatusEffectsListSize, true, false, true, GrowthDirections.Left | GrowthDirections.Down);
        }

        public static StatusEffectsListConfig PlayerDebuffsList()
        {
            var screenSize = ImGui.GetMainViewport().Size;
            var pos = new Vector2(screenSize.X * 0.38f, -screenSize.Y * 0.45f + DefaultStatusEffectsListSize.Y);

            return new StatusEffectsListConfig(pos, DefaultStatusEffectsListSize, false, true, true, GrowthDirections.Left | GrowthDirections.Down);
        }

        public static StatusEffectsListConfig TargetBuffsList()
        {
            var pos = new Vector2(HUDConstants.UnitFramesOffsetX, HUDConstants.BaseHUDOffsetY - 50);

            return new StatusEffectsListConfig(pos, DefaultStatusEffectsListSize, true, false, true, GrowthDirections.Right | GrowthDirections.Up);
        }

        public static StatusEffectsListConfig TargetDebuffsList()
        {
            var pos = new Vector2(HUDConstants.UnitFramesOffsetX, HUDConstants.BaseHUDOffsetY - 50 - DefaultStatusEffectsListSize.Y);

            return new StatusEffectsListConfig(pos, DefaultStatusEffectsListSize, false, true, true, GrowthDirections.Right | GrowthDirections.Up);
        }

        // gcd Indicator
        public static GCDIndicatorConfig GCDIndicator()
        {
            var size = new Vector2(254, 4);
            var pos = new Vector2(0, HUDConstants.BaseHUDOffsetY + DefaultCastbarSize.Y - 4);

            return new GCDIndicatorConfig(pos, size);
        }

        // mp Ticker
        public static MPTickerConfig MPTicker()
        {
            var size = new Vector2(254, 4);
            var pos = new Vector2(0, HUDConstants.BaseHUDOffsetY + DefaultCastbarSize.Y + 2);

            return new MPTickerConfig(pos, size);
        }

        // primary resource bar
        public static PrimaryResourceConfig PrimaryResource()
        {
            var size = new Vector2(254, 20);
            var pos = new Vector2(0, HUDConstants.BaseHUDOffsetY - 37);

            var labelConfig = new LabelConfig(Vector2.Zero, "", LabelTextAnchor.Center, "Value Label");

            return new PrimaryResourceConfig(pos, size, labelConfig);
        }
    }
}
