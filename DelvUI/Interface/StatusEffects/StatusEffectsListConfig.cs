using System;
using System.Numerics;
using ImGuiNET;
using System.Collections.Generic;
using DelvUI.Config;

namespace DelvUI.Interface.StatusEffects
{
    [Serializable]
    public class StatusEffectsListConfig: PluginConfigObject
    {
        public bool Enabled = true;
        public Vector2 Position;
        public Vector2 MaxSize = new Vector2(340, 100);
        public Vector2 IconPadding = new Vector2(2, 2);
        public bool ShowArea = false;
        public StatusEffectIconConfig IconConfig = new StatusEffectIconConfig();
        public bool ShowBuffs;
        public bool ShowDebuffs;
        public bool ShowPermanentEffects;
        public bool FillRowsFirst = true;
        public int Limit = -1;
        public GrowthDirections GrowthDirections;

        public StatusEffectsListConfig(Vector2 position, bool showBuffs, bool showDebuffs, bool showPermanentEffects, GrowthDirections growthDirections)
        {
            Position = position;
            ShowBuffs = showBuffs;
            ShowDebuffs = showDebuffs;
            ShowPermanentEffects = showPermanentEffects;
            GrowthDirections = growthDirections;
        }

        public bool Draw()
        {
            var changed = false;

            changed |= ImGui.Checkbox("Enabled", ref Enabled);

            ImGui.Text("Layout");
            ImGui.BeginGroup();
            {
                changed |= ImGui.DragFloat2("Base Offset", ref Position, 1f, -4000, 4000);
                changed |= ImGui.DragFloat2("Area Size", ref MaxSize, 1f, 1, 2000);
                changed |= ImGui.DragFloat2("Padding", ref IconPadding, 1f, -200, 200);

                List<GrowthDirections> directions = new List<GrowthDirections>()
                {
                    GrowthDirections.RIGHT | GrowthDirections.DOWN,
                    GrowthDirections.RIGHT | GrowthDirections.UP,
                    GrowthDirections.LEFT | GrowthDirections.DOWN,
                    GrowthDirections.LEFT | GrowthDirections.UP
                };
                int selection = Math.Max(0, directions.IndexOf((GrowthDirections)GrowthDirections));
                string[] directionsStrings = new string[]
                {
                    "Right and Down",
                    "Right and Up",
                    "Left and Down",
                    "Left and Up"
                };
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
    public class StatusEffectIconConfig: PluginConfigObject
    {
        public Vector2 Size = new Vector2(40, 40);
        public bool ShowDurationText = true;
        public bool ShowStacksText = true;

        public bool ShowBorder = true;
        public int BorderThickness = 1;
        public PluginConfigColor BorderColor = new PluginConfigColor(new Vector4(0f / 255f, 0 / 255f, 0 / 255f, 100f / 100f));

        public bool ShowDispellableBorder = true;
        public int DispellableBorderThickness = 2;
        public PluginConfigColor DispellableBorderColor = new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        public bool Draw()
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
