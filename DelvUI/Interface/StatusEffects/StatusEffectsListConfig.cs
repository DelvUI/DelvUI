using DelvUI.Config;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DelvUI.Interface.StatusEffects
{
    public static class DefaultStatusEffectsLists
    {
        internal static Vector2 DefaultSize = new Vector2(292, 82);

        public static StatusEffectsListConfig PlayerBuffsList()
        {
            var screenSize = ImGui.GetMainViewport().Size;
            var pos = new Vector2(screenSize.X * 0.38f, -screenSize.Y * 0.45f);

            return new StatusEffectsListConfig(pos, DefaultSize, true, false, true, GrowthDirections.Left | GrowthDirections.Down);
        }

        public static StatusEffectsListConfig PlayerDebuffsList()
        {
            var screenSize = ImGui.GetMainViewport().Size;
            var pos = new Vector2(screenSize.X * 0.38f, -screenSize.Y * 0.45f + DefaultSize.Y);

            return new StatusEffectsListConfig(pos, DefaultSize, false, true, true, GrowthDirections.Left | GrowthDirections.Down);
        }

        public static StatusEffectsListConfig TargetBuffsList()
        {
            var pos = new Vector2(HUDConstants.UnitFramesOffsetX, HUDConstants.BaseHUDOffsetY - 50);

            return new StatusEffectsListConfig(pos, DefaultSize, true, false, true, GrowthDirections.Right | GrowthDirections.Up);
        }

        public static StatusEffectsListConfig TargetDebuffsList()
        {
            var pos = new Vector2(HUDConstants.UnitFramesOffsetX, HUDConstants.BaseHUDOffsetY - 50 - DefaultSize.Y);

            return new StatusEffectsListConfig(pos, DefaultSize, false, true, true, GrowthDirections.Right | GrowthDirections.Up);
        }
    }

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
            StatusEffectIconConfig iconConfig = null) : base(position, size)
        {
            ShowBuffs = showBuffs;
            ShowDebuffs = showDebuffs;
            ShowPermanentEffects = showPermanentEffects;
            GrowthDirections = growthDirections;

            if (iconConfig != null)
            {
                IconConfig = iconConfig;
            }
        }

        public new bool Draw()
        {
            var changed = false;

            ImGui.Text("Layout");
            ImGui.BeginGroup();
            {
                changed |= base.Draw();
                changed |= ImGui.DragFloat2("Padding", ref IconPadding, 1f, -200, 200);

                var directions = new List<GrowthDirections>
                {
                    GrowthDirections.Right | GrowthDirections.Down,
                    GrowthDirections.Right | GrowthDirections.Up,
                    GrowthDirections.Left | GrowthDirections.Down,
                    GrowthDirections.Left | GrowthDirections.Up,
                    GrowthDirections.Out | GrowthDirections.Right,
                    GrowthDirections.Out | GrowthDirections.Down,
                };

                var selection = Math.Max(0, directions.IndexOf(GrowthDirections));
                string[] directionsStrings = { "Right and Down", "Right and Up", "Left and Down", "Left and Up", "Out from Middle and Right", "Out from Middle and Down" };

                if (ImGui.Combo("Icons Growth Direction", ref selection, directionsStrings, directionsStrings.Length))
                {
                    GrowthDirections = directions[selection];
                    changed = true;
                }

                changed |= ImGui.Checkbox("Show Area", ref ShowArea);
                changed |= ImGui.Checkbox("Fill Rows First", ref FillRowsFirst);
                changed |= ImGui.Checkbox("Show Permanent Effects", ref ShowPermanentEffects);
                changed |= ImGui.DragInt("Limit (-1 means no limit)", ref Limit, .1f, -1, 100);
            }

            ImGui.EndGroup();

            changed = IconConfig.Draw();

            return changed;
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

        public new bool Draw()
        {
            var changed = false;

            ImGui.Text("Icons");
            ImGui.BeginGroup();
            {
                changed |= ImGui.DragFloat2("Size", ref Size, 1f, 1, 200);

                changed |= ImGui.Checkbox("Show Duration", ref ShowDurationText);
                changed |= ImGui.Checkbox("Show Stacks", ref ShowStacksText);

                changed |= ImGui.Checkbox("Show Border", ref ShowBorder);
                changed |= ImGui.DragInt("Border Thickness", ref BorderThickness, .1f, 1, 5);
                changed |= ColorEdit4("Border Color", ref BorderColor);

                changed |= ImGui.Checkbox("Show Dispellable Border", ref ShowDispellableBorder);
                changed |= ImGui.DragInt("Dispellable Border Thickness", ref DispellableBorderThickness, .1f, 1, 5);
                changed |= ColorEdit4("Dispellable order Color", ref DispellableBorderColor);
            }

            ImGui.EndGroup();

            return changed;
        }
    }
}
