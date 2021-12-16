using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using System.Collections.Generic;
using System.Numerics;

namespace DelvUI.Interface.PartyCooldowns
{
    public enum PartyCooldownsGrowthDirection
    {
        Down = 0,
        Up
    }

    [Exportable(false)]
    [Section("Party Cooldowns", true)]
    [SubSection("General", 0)]
    public class PartyCooldownsConfig : MovablePluginConfigObject
    {
        public new static PartyCooldownsConfig DefaultConfig() => new PartyCooldownsConfig();

        [Combo("Growth Direction", "Down", "Up", spacing = true)]
        [Order(20)]
        public PartyCooldownsGrowthDirection GrowthDirection = PartyCooldownsGrowthDirection.Down;

        [DragInt2("Padding", min = 1, max = 100)]
        [Order(15)]
        public Vector2 Padding;

        [Checkbox("Show Only in Duties", spacing = true)]
        [Order(20)]
        public bool ShowOnlyInDuties = true;

        [Checkbox("Show When Solo")]
        [Order(21)]
        public bool ShowWhenSolo = false;
    }

    [Disableable(false)]
    [Exportable(false)]
    [DisableParentSettings("Position", "Anchor", "HideWhenInactive", "FillColor", "Background", "FillDirection")]
    [Section("Party Cooldowns", true)]
    [SubSection("Cooldown Bar", 0)]
    public class PartyCooldownsBarConfig : BarConfig
    {
        [ColorEdit4("Action Available Color", spacing = true)]
        [Order(70)]
        public PluginConfigColor AvailableColor = new PluginConfigColor(new Vector4(0f / 255f, 150f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Action Recharging Color", spacing = true)]
        [Order(71)]
        public PluginConfigColor RechargingColor = new PluginConfigColor(new Vector4(150f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        [NestedConfig("Name Label", 100)]
        public EditableLabelConfig NameLabel = new EditableLabelConfig(new Vector2(25, 0), "[name:first-npcmedium]", DrawAnchor.Left, DrawAnchor.Left);

        [NestedConfig("Time Label", 105)]
        public LabelConfig TimeLabel = new LabelConfig(new Vector2(-5, 0), "", DrawAnchor.Right, DrawAnchor.Right);

        public new static PartyCooldownsBarConfig DefaultConfig()
        {
            Vector2 size = new Vector2(100, 20);

            var config = new PartyCooldownsBarConfig(Vector2.Zero, size, new(Vector4.Zero));

            config.NameLabel.FontID = FontsConfig.DefaultMediumFontKey;
            config.TimeLabel.FontID = FontsConfig.DefaultMediumFontKey;

            return config;
        }

        public PartyCooldownsBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor, BarDirection fillDirection = BarDirection.Right)
            : base(position, size, fillColor, fillDirection)
        {
        }
    }

    [Exportable(false)]
    [Disableable(false)]
    [Section("Party Cooldowns", true)]
    [SubSection("Cooldowns Tracked", 0)]
    public class PartyCooldownsDataConfig : PluginConfigObject
    {
        public List<PartyCooldown> Cooldowns = new List<PartyCooldown>();
        private JobRoles _roleFilter = JobRoles.Unknown;

        public new static PartyCooldownsDataConfig DefaultConfig()
        {
            var config = new PartyCooldownsDataConfig();

            ExcelSheet<Action>? sheet = Plugin.DataManager.GetExcelSheet<Action>();
            config.Cooldowns.AddRange(DefaultCooldowns.Values);

            // get cooldowns from data just in case
            // i'd like to get the level requirements as well but thats more complicated
            foreach (PartyCooldown cooldown in config.Cooldowns)
            {
                Action? action = sheet?.GetRow(cooldown.ActionID);
                if (action == null) { continue; }

                cooldown.CooldownDuration = action.Recast100ms / 10;
            }
            return config;
        }

        [ManualDraw]
        public bool Draw(ref bool changed)
        {
            ImGuiHelper.NewLineAndTab();

            // filter
            ImGui.SameLine();
            ImGui.Text("Filter: ");

            DrawFilter("All", JobRoles.Unknown);
            DrawFilter("Tanks", JobRoles.Tank);
            DrawFilter("Healers", JobRoles.Healer);
            DrawFilter("Melee", JobRoles.DPSMelee);
            DrawFilter("Ranged", JobRoles.DPSRanged);
            DrawFilter("Casters", JobRoles.DPSCaster);

            // table
            ImGuiHelper.NewLineAndTab();
            ImGui.SameLine();

            var flags =
                ImGuiTableFlags.RowBg |
                ImGuiTableFlags.Borders |
                ImGuiTableFlags.BordersOuter |
                ImGuiTableFlags.BordersInner |
                ImGuiTableFlags.ScrollY |
                ImGuiTableFlags.SizingFixedSame;

            ExcelSheet<Action>? sheet = Plugin.DataManager.GetExcelSheet<Action>();
            var iconSize = new Vector2(30, 30);

            if (ImGui.BeginTable("##DelvUI_PartyCooldownsTable", 6, flags, new Vector2(800, 500)))
            {
                ImGui.TableSetupColumn("Icon", ImGuiTableColumnFlags.WidthStretch, 5, 0);
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 25, 1);
                ImGui.TableSetupColumn("Cooldown", ImGuiTableColumnFlags.WidthStretch, 10, 2);
                ImGui.TableSetupColumn("Duration", ImGuiTableColumnFlags.WidthStretch, 10, 3);
                ImGui.TableSetupColumn("Priority", ImGuiTableColumnFlags.WidthStretch, 25, 4);
                ImGui.TableSetupColumn("Column", ImGuiTableColumnFlags.WidthStretch, 25, 5);

                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();

                foreach (PartyCooldown cooldown in Cooldowns)
                {
                    // apply filter
                    if (_roleFilter != JobRoles.Unknown)
                    {
                        JobRoles role = cooldown is RolePartyCooldown roleCooldown ? roleCooldown.Role : JobsHelper.RoleForJob(cooldown.JobID);
                        if (role != _roleFilter)
                        {
                            continue;
                        }
                    }

                    Action? action = sheet?.GetRow(cooldown.ActionID);

                    ImGui.PushID(cooldown.ActionID.ToString());
                    ImGui.TableNextRow(ImGuiTableRowFlags.None, iconSize.Y);

                    // icon
                    if (ImGui.TableSetColumnIndex(0) && action != null)
                    {
                        DrawHelper.DrawIcon<Action>(action, ImGui.GetCursorPos(), iconSize, false, true);
                    }

                    // name
                    if (ImGui.TableSetColumnIndex(1))
                    {
                        ImGui.Text(action?.Name ?? "");
                    }

                    // cooldown
                    if (ImGui.TableSetColumnIndex(2))
                    {
                        ImGui.Text($"{cooldown.CooldownDuration}");
                    }

                    // duration
                    if (ImGui.TableSetColumnIndex(3))
                    {
                        ImGui.Text($"{cooldown.EffectDuration}");
                    }

                    // priority
                    if (ImGui.TableSetColumnIndex(4))
                    {
                        ImGui.PushItemWidth(180);
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 4);

                        changed |= ImGui.DragInt($"##{cooldown.ActionID}_priority", ref cooldown.Priority, 1, 0, 100);

                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Priority determines which cooldows show first on the list."); }
                    }

                    // column
                    if (ImGui.TableSetColumnIndex(5))
                    {
                        ImGui.PushItemWidth(180);
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 4);

                        changed |= ImGui.DragInt($"##{cooldown.ActionID}_column", ref cooldown.Column, 0.1f, 1, 5);

                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Allows to separate cooldowns in different columns."); }
                    }

                    ImGui.PopID();
                }

                ImGui.EndTable();
            }

            return false;
        }

        private void DrawFilter(string name, JobRoles role)
        {
            ImGui.SameLine();
            if (ImGui.RadioButton(name, _roleFilter == role))
            {
                _roleFilter = role;
            }
        }

        public static Dictionary<uint, PartyCooldown> DefaultCooldowns = new Dictionary<uint, PartyCooldown>()
        {
            // TANKS
            [7535] = new RolePartyCooldown(7535, JobRoles.Tank, 22, 60, 10, 100, 1), // reprisal
            [3540] = new PartyCooldown(3540, JobIDs.PLD, 56, 90, 30, 90, 1), // divine veil
            [7385] = new PartyCooldown(7385, JobIDs.PLD, 70, 120, 18, 90, 1), // passage of arms
            [7388] = new PartyCooldown(7388, JobIDs.WAR, 68, 90, 15, 90, 1), // shake it off
            [16471] = new PartyCooldown(16471, JobIDs.DRK, 76, 90, 15, 90, 1), // dark missionary
            [16160] = new PartyCooldown(16160, JobIDs.GNB, 64, 90, 15, 90, 1), // heart of light

            // HEALER
            [16552] = new PartyCooldown(16552, JobIDs.AST, 50, 120, 15, 30, 2), // divination
            [3613] = new PartyCooldown(3613, JobIDs.AST, 58, 60, 20, 80, 1), // collective unconscious
            [7436] = new PartyCooldown(7436, JobIDs.SCH, 66, 120, 15, 30, 2), // chain stratagem
            [805] = new PartyCooldown(805, JobIDs.SCH, 40, 120, 20, 50, 3), // fey illumination
            [188] = new PartyCooldown(188, JobIDs.SCH, 50, 30, 15, 80, 1), // sacred soil
            [16536] = new PartyCooldown(16536, JobIDs.WHM, 80, 120, 20, 80, 1), // temperance
            [3569] = new PartyCooldown(3569, JobIDs.WHM, 52, 90, 24, 50, 3), // asylum

            // MELEE
            [7549] = new RolePartyCooldown(7549, JobRoles.DPSMelee, 22, 90, 10, 100, 1), // feint
            [2258] = new PartyCooldown(2258, JobIDs.NIN, 18, 60, 15, 30, 2), // trick attack
            [3557] = new PartyCooldown(3557, JobIDs.DRG, 70, 90, 15, 30, 2), // battle litany
            [7396] = new PartyCooldown(7396, JobIDs.MNK, 58, 60, 20, 90, 1), // brotherhood
            [65] = new PartyCooldown(65, JobIDs.MNK, 42, 90, 15, 50, 3), // mantra

            // RANGED
            [118] = new PartyCooldown(118, JobIDs.BRD, 50, 180, 20, 30, 2), // battle voice
            [7405] = new PartyCooldown(7405, JobIDs.BRD, 62, 120, 15, 70, 1), // troubadour
            [7408] = new PartyCooldown(7408, JobIDs.BRD, 66, 90, 15, 40, 3), // nature's minne
            [16012] = new PartyCooldown(16012, JobIDs.DNC, 56, 120, 15, 70, 1), // shield samba
            [16014] = new PartyCooldown(16014, JobIDs.DNC, 80, 120, 15, 40, 3), // improvisation
            [16889] = new PartyCooldown(16889, JobIDs.MCH, 56, 120, 15, 70, 1), // tactician

            // CASTER
            [7560] = new RolePartyCooldown(7560, JobRoles.DPSCaster, 8, 90, 10, 100, 1), // addle
            [7520] = new PartyCooldown(7520, JobIDs.RDM, 58, 120, 20, 30, 2), // embolden
            [7450] = new PartyCooldown(7450, JobIDs.SMN, 64, 180, 15, 30, 2), // devotion
        };
    }
}

