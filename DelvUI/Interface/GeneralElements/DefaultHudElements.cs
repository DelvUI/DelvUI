using DelvUI.Interface.StatusEffects;
using ImGuiNET;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    public static class DefaultHudElements
    {

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

        // primary resource bar
        public static PrimaryResourceConfig PrimaryResource()
        {
            var size = new Vector2(254, 20);
            var pos = new Vector2(0, HUDConstants.BaseHUDOffsetY - 37);

            var labelConfig = new LabelConfig(Vector2.Zero, "", LabelTextAnchor.Center);

            return new PrimaryResourceConfig(pos, size, labelConfig);
        }
    }
}
