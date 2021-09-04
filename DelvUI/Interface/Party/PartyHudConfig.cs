using DelvUI.Config;
using ImGuiNET;
using System;
using System.Numerics;

namespace DelvUI.Interface.Party {
    [Serializable]
    public class PartyHudConfig : PluginConfigObject {
        public Vector2 Position = new Vector2(200, 200);
        public Vector2 Size = new Vector2(650, 150);

        public bool Enabled = true;
        public bool Preview = false;
        public bool Lock = true;
        public bool FillRowsFirst = true;

        public PartyHudSortConfig SortConfig = new PartyHudSortConfig();
        public PartyHudHealthBarsConfig HealthBarsConfig = new PartyHudHealthBarsConfig();

        public bool Draw() {
            var changed = false;

            ImGui.Text("General settings");
            ImGui.BeginGroup();
            {
                changed |= ImGui.Checkbox("Enabled", ref Enabled);
                changed |= ImGui.Checkbox("Preview", ref Preview);
                changed |= ImGui.Checkbox("Lock", ref Lock);
                changed |= ImGui.Checkbox("Fill Rows First", ref FillRowsFirst);
            }
            ImGui.EndGroup();

            changed |= SortConfig.Draw();

            return changed;
        }
    }

    [Serializable]
    public class PartyHudSortConfig : PluginConfigObject {
        public PartySortingMode Mode = PartySortingMode.Tank_Healer_DPS;
        public bool UseRoleColors = false;
        public PluginConfigColor TankRoleColor = new PluginConfigColor(new Vector4(21f / 255f, 28f / 255f, 100f / 255f, 100f / 100f));
        public PluginConfigColor DPSRoleColor = new PluginConfigColor(new Vector4(153f / 255f, 23f / 255f, 23f / 255f, 100f / 100f));
        public PluginConfigColor HealerRoleColor = new PluginConfigColor(new Vector4(46f / 255f, 125f / 255f, 50f / 255f, 100f / 100f));
        public PluginConfigColor GenericRoleColor = new PluginConfigColor(new Vector4(0f / 255f, 145f / 255f, 6f / 255f, 100f / 100f));

        public bool Draw() {
            var changed = false;

            ImGui.Text("Sorting");
            ImGui.BeginGroup();
            {
                var selection = (int)Mode;
                var names = PartySortingHelper.SortingModesNames;
                if (ImGui.Combo("Sorting priority", ref selection, PartySortingHelper.SortingModesNames, names.Length)) {
                    Mode = (PartySortingMode)selection;
                    changed = true;
                }

                changed |= ImGui.Checkbox("Use Role Colors", ref UseRoleColors);
                changed |= ColorEdit4("Tank Color", ref TankRoleColor);
                changed |= ColorEdit4("DPS Color", ref DPSRoleColor);
                changed |= ColorEdit4("Healer Color", ref HealerRoleColor);
                changed |= ColorEdit4("Generic Color", ref GenericRoleColor);
            }
            ImGui.EndGroup();

            return changed;
        }
    }

    [Serializable]
    public class PartyHudHealthBarsConfig : PluginConfigObject {
        public string TextFormat = "[name:initials]";
        public Vector2 Size = new Vector2(150, 50);
        public Vector2 Padding = new Vector2(1, 1);
        public PluginConfigColor BackgroundColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 70f / 100f));
        public PluginConfigColor UnreachableColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 30f / 100f));

        public PartyHudShieldsConfig ShieldsConfig = new PartyHudShieldsConfig();

        public bool Draw() {
            var changed = false;

            ImGui.Text("Bars");
            ImGui.BeginGroup();
            {
                changed |= ImGui.InputTextWithHint("Text Fromat", "Example: [name:initials]", ref TextFormat, 64);
                changed |= ImGui.DragFloat2("Size", ref Size, 1, 1, 1000);
                changed |= ImGui.DragFloat2("Padding", ref Padding, 1, -200, 200);
                changed |= ColorEdit4("Background Color", ref BackgroundColor);
                changed |= ColorEdit4("Member Unreachable Color", ref UnreachableColor);
            }

            changed |= ShieldsConfig.Draw();

            return changed;
        }
    }

    [Serializable]
    public class PartyHudShieldsConfig : PluginConfigObject {
        public bool Enabled = true;
        public int Height = 10;
        public bool HeightInPixels = false;
        public bool FillHealthFirst = true;
        public PluginConfigColor Color = new PluginConfigColor(new Vector4(198f / 255f, 210f / 255f, 255f / 255f, 70f / 100f));

        public bool Draw() {
            var changed = false;

            ImGui.Text("Shields");
            ImGui.BeginGroup();
            {
                changed |= ImGui.Checkbox("Enabled", ref Enabled);
                changed |= ImGui.DragInt("Shield Size", ref Height, .1f, 1, 1000);
                changed |= ImGui.Checkbox("Size in pixels", ref HeightInPixels);
                changed |= ImGui.Checkbox("Fill Health First", ref FillHealthFirst);
                changed |= ColorEdit4("Shield Color", ref Color);
            }

            return changed;
        }
    }

}
