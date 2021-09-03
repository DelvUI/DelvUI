using System;
using System.Numerics;
using ImGuiNET;

namespace DelvUI.Interface.Party
{
    public class PartyHudConfig
    {
        public Vector2 Position = new Vector2(200, 200);
        public Vector2 Size = new Vector2(650, 150);

        public bool Enabled = true;
        public bool Preview = false;
        public bool Lock = true;
        public bool FillRowsFirst = true;

        public PartyHudSortConfig SortConfig = new PartyHudSortConfig();
        public PartyHudHealthBarsConfig HealthBarsConfig = new PartyHudHealthBarsConfig();

        public bool Draw()
        {
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

    public class PartyHudSortConfig
    {
        public PartySortingMode Mode = PartySortingMode.Tank_Healer_DPS;
        public bool UseRoleColors = false;
        public Vector4 TankRoleColor = new Vector4(21f / 255f, 28f / 255f, 100f / 255f, 100f / 100f);
        public Vector4 DPSRoleColor = new Vector4(153f / 255f, 23f / 255f, 23f / 255f, 100f / 100f);
        public Vector4 HealerRoleColor = new Vector4(46f / 255f, 125f / 255f, 50f / 255f, 100f / 100f);
        public Vector4 GenericRoleColor = new Vector4(0f / 255f, 145f / 255f, 6f / 255f, 100f / 100f);

        public bool Draw()
        {
            var changed = false;

            ImGui.Text("Sorting");
            ImGui.BeginGroup();
            {
                int selection = (int)Mode;
                var names = PartySortingHelper.SortingModesNames;
                if (ImGui.Combo("Sorting priority", ref selection, PartySortingHelper.SortingModesNames, names.Length))
                {
                    Mode = (PartySortingMode)selection;
                    changed = true;
                }

                changed |= ImGui.Checkbox("Use Role Colors", ref UseRoleColors);
                changed |= ImGui.ColorEdit4("Tank Color", ref TankRoleColor);
                changed |= ImGui.ColorEdit4("DPS Color", ref DPSRoleColor);
                changed |= ImGui.ColorEdit4("Healer Color", ref HealerRoleColor);
                changed |= ImGui.ColorEdit4("Generic Color", ref GenericRoleColor);
            }
            ImGui.EndGroup();

            return changed;
        }
    }

    public class PartyHudHealthBarsConfig
    {
        public string TextFormat = "[name:initials]";
        public Vector2 Size = new Vector2(150, 50);
        public Vector2 Padding = new Vector2(1, 1);
        public Vector4 BackgroundColor = new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 70f / 100f);

        public PartyHudShieldsConfig ShieldsConfig = new PartyHudShieldsConfig();

        public bool Draw()
        {
            var changed = false;

            ImGui.Text("Bars");
            ImGui.BeginGroup();
            {
                changed |= ImGui.InputTextWithHint("Text Fromat", "Example: [name:initials]", ref TextFormat, 64);

                int width = (int)Size.X;
                if (ImGui.DragInt("Width", ref width, 1, 1, 1000))
                {
                    Size.X = width;
                    changed = true;
                }
                int height = (int)Size.Y;
                if (ImGui.DragInt("Height", ref height, 1, 1, 1000))
                {
                    Size.Y = height;
                    changed = true;
                }

                int paddingX = (int)Padding.X;
                if (ImGui.DragInt("Horizontal Padding", ref paddingX, 1, -200, 200))
                {
                    Padding.X = paddingX;
                    changed = true;
                }
                int paddingY = (int)Padding.Y;
                if (ImGui.DragInt("Vertical Padding", ref paddingY, 1, -200, 200))
                {
                    Padding.Y = paddingY;
                    changed = true;
                }

                changed |= ImGui.ColorEdit4("Background Color", ref BackgroundColor);
            }

            changed |= ShieldsConfig.Draw();

            return changed;
        }
    }

    public class PartyHudShieldsConfig
    {
        public bool Enabled = true;
        public int Height = 10;
        public bool HeightInPixels = false;
        public bool FillHealthFirst = true;
        public Vector4 Color = new Vector4(198f / 255f, 210f / 255f, 255f / 255f, 70f / 100f);

        public bool Draw()
        {
            var changed = false;

            ImGui.Text("Shields");
            ImGui.BeginGroup();
            {
                changed |= ImGui.Checkbox("Enabled", ref Enabled);
                changed |= ImGui.DragInt("Shield Size", ref Height, .1f, 1, 1000);
                changed |= ImGui.Checkbox("Size in pixels", ref HeightInPixels);
                changed |= ImGui.Checkbox("Fill Health First", ref FillHealthFirst);
                changed |= ImGui.ColorEdit4("Shield Color", ref Color);
            }

            return changed;
        }
    }

}
