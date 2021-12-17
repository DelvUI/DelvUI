﻿using DelvUI.Config;
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
        public new static PartyCooldownsConfig DefaultConfig()
        {
            var config = new PartyCooldownsConfig();
            config.Position = new Vector2(-ImGui.GetMainViewport().Size.X / 2 + 100, -ImGui.GetMainViewport().Size.Y / 2 + 100);

            return config;
        }

        [Checkbox("Preview", isMonitored = true)]
        [Order(4)]
        public bool Preview = false;

        [Combo("Growth Direction", "Down", "Up", spacing = true)]
        [Order(20)]
        public PartyCooldownsGrowthDirection GrowthDirection = PartyCooldownsGrowthDirection.Down;

        [DragInt2("Padding", min = -10, max = 100)]
        [Order(15)]
        public Vector2 Padding = new Vector2(0, -1);

        [Checkbox("Show Only in Duties", spacing = true, isMonitored = true)]
        [Order(20)]
        public bool ShowOnlyInDuties = true;

        [Checkbox("Show When Solo", isMonitored = true)]
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
        [ColorEdit4("Available Color", spacing = true)]
        [Order(70)]
        public PluginConfigColor AvailableColor = new PluginConfigColor(new Vector4(0f / 255f, 150f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Available Background Color")]
        [Order(71)]
        public PluginConfigColor AvailableBackgroundColor = new PluginConfigColor(new Vector4(0f / 255f, 150f / 255f, 0f / 255f, 25f / 100f));

        [ColorEdit4("Recharging Color", spacing = true)]
        [Order(72)]
        public PluginConfigColor RechargingColor = new PluginConfigColor(new Vector4(150f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Recharging Background Color")]
        [Order(73)]
        public PluginConfigColor RechargingBackgroundColor = new PluginConfigColor(new Vector4(150f / 255f, 0f / 255f, 0f / 255f, 25f / 100f));

        [NestedConfig("Name Label", 100)]
        public EditableLabelConfig NameLabel = new EditableLabelConfig(new Vector2(5, 0), "[name:initials]", DrawAnchor.Left, DrawAnchor.Left);

        [NestedConfig("Time Label", 105)]
        public NumericLabelConfig TimeLabel = new NumericLabelConfig(new Vector2(-5, 0), "", DrawAnchor.Right, DrawAnchor.Right);

        public new static PartyCooldownsBarConfig DefaultConfig()
        {
            Vector2 size = new Vector2(150, 24);

            var config = new PartyCooldownsBarConfig(Vector2.Zero, size, new(Vector4.Zero));

            config.NameLabel.FontID = FontsConfig.DefaultMediumFontKey;
            config.TimeLabel.FontID = FontsConfig.DefaultMediumFontKey;
            config.TimeLabel.NumberFormat = 1;

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
        public List<PartyCooldownData> Cooldowns = new List<PartyCooldownData>();
        private JobRoles _roleFilter = JobRoles.Unknown;

        public const int ColumnCount = 5;

        public delegate void CooldownsDataChangedEventHandler(PartyCooldownsDataConfig sender);
        public event CooldownsDataChangedEventHandler? CooldownsDataChangedEvent;

        public delegate void CooldownsDataEnabledChangedEventHandler(PartyCooldownsDataConfig sender);
        public event CooldownsDataEnabledChangedEventHandler? CooldownsDataEnabledChangedEvent;

        public new static PartyCooldownsDataConfig DefaultConfig()
        {
            var config = new PartyCooldownsDataConfig();

            ExcelSheet<Action>? sheet = Plugin.DataManager.GetExcelSheet<Action>();
            config.Cooldowns.AddRange(DefaultCooldowns.Values);

            // get cooldowns from data just in case
            // i'd like to get the level requirements as well but thats more complicated
            foreach (PartyCooldownData cooldown in config.Cooldowns)
            {
                Action? action = sheet?.GetRow(cooldown.ActionId);
                if (action == null) { continue; }

                if (action.Recast100ms > 0)
                {
                    cooldown.CooldownDuration = action.Recast100ms / 10;
                }

                cooldown.IconId = action.Icon;
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

            if (ImGui.BeginTable("##DelvUI_PartyCooldownsTable", 7, flags, new Vector2(800, 500)))
            {
                ImGui.TableSetupColumn("Enabled", ImGuiTableColumnFlags.WidthStretch, 6, 0);
                ImGui.TableSetupColumn("Icon", ImGuiTableColumnFlags.WidthStretch, 5, 1);
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 22, 2);
                ImGui.TableSetupColumn("Cooldown", ImGuiTableColumnFlags.WidthStretch, 10, 3);
                ImGui.TableSetupColumn("Duration", ImGuiTableColumnFlags.WidthStretch, 10, 4);
                ImGui.TableSetupColumn("Priority", ImGuiTableColumnFlags.WidthStretch, 22, 5);
                ImGui.TableSetupColumn("Column", ImGuiTableColumnFlags.WidthStretch, 22, 6);

                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();

                foreach (PartyCooldownData cooldown in Cooldowns)
                {
                    // apply filter
                    if (_roleFilter != JobRoles.Unknown)
                    {
                        JobRoles role = cooldown.Role != JobRoles.Unknown ? cooldown.Role : JobsHelper.RoleForJob(cooldown.JobId);
                        if (role != _roleFilter)
                        {
                            continue;
                        }
                    }

                    Action? action = sheet?.GetRow(cooldown.ActionId);

                    ImGui.PushID(cooldown.ActionId.ToString());
                    ImGui.TableNextRow(ImGuiTableRowFlags.None, iconSize.Y);

                    // enabled
                    if (ImGui.TableSetColumnIndex(0))
                    {
                        ImGui.SetCursorPos(new Vector2(ImGui.GetCursorPosX() + 10, ImGui.GetCursorPosY() + 4));
                        if (ImGui.Checkbox("", ref cooldown.Enabled))
                        {
                            CooldownsDataEnabledChangedEvent?.Invoke(this);
                        }
                    }

                    // icon
                    if (ImGui.TableSetColumnIndex(1) && action != null)
                    {
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 3);
                        DrawHelper.DrawIcon<Action>(action, ImGui.GetCursorPos(), iconSize, false, true);

                        //TooltipsHelper.Instance.ShowTooltipOnCursor(
                        //    action.Description.ToDalamudString().ToString(),
                        //    statusEffectData.Data.Name,
                        //    statusEffectData.Status.StatusID,
                        //    GetStatusActorName(statusEffectData.Status)
                        //);
                    }

                    // name
                    if (ImGui.TableSetColumnIndex(2))
                    {
                        ImGui.Text(action?.Name ?? "");
                    }

                    // cooldown
                    if (ImGui.TableSetColumnIndex(3))
                    {
                        ImGui.Text($"{cooldown.CooldownDuration}");
                    }

                    // duration
                    if (ImGui.TableSetColumnIndex(4))
                    {
                        ImGui.Text($"{cooldown.EffectDuration}");
                    }

                    // priority
                    if (ImGui.TableSetColumnIndex(5))
                    {
                        ImGui.PushItemWidth(160);
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 4);

                        changed |= ImGui.DragInt($"##{cooldown.ActionId}_priority", ref cooldown.Priority, 1, 0, 100);

                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Priority determines which cooldows show first on the list."); }
                    }

                    // column
                    if (ImGui.TableSetColumnIndex(6))
                    {
                        ImGui.PushItemWidth(160);
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 4);

                        changed |= ImGui.DragInt($"##{cooldown.ActionId}_column", ref cooldown.Column, 0.1f, 1, ColumnCount);

                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Allows to separate cooldowns in different columns."); }
                    }

                    ImGui.PopID();
                }

                ImGui.EndTable();
            }

            if (changed)
            {
                CooldownsDataChangedEvent?.Invoke(this);
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

        public static Dictionary<uint, PartyCooldownData> DefaultCooldowns = new Dictionary<uint, PartyCooldownData>()
        {
            // TANKS
            [7535] = NewData(7535, JobRoles.Tank, 22, 60, 10, 100, 1), // reprisal
            [3540] = NewData(3540, JobIDs.PLD, 56, 90, 30, 90, 2), // divine veil
            [7385] = NewData(7385, JobIDs.PLD, 70, 120, 18, 90, 2), // passage of arms
            [7388] = NewData(7388, JobIDs.WAR, 68, 90, 15, 90, 2), // shake it off
            [16471] = NewData(16471, JobIDs.DRK, 76, 90, 15, 90, 2), // dark missionary
            [16160] = NewData(16160, JobIDs.GNB, 64, 90, 15, 90, 2), // heart of light

            // HEALER
            [16552] = NewData(16552, JobIDs.AST, 50, 120, 15, 30, 3), // divination
            [3613] = NewData(3613, JobIDs.AST, 58, 60, 18, 80, 2), // collective unconscious
            [7436] = NewData(7436, JobIDs.SCH, 66, 120, 15, 30, 3), // chain stratagem
            [805] = NewData(805, JobIDs.SCH, 40, 120, 20, 50, 2), // fey illumination
            [188] = NewData(188, JobIDs.SCH, 50, 30, 15, 80, 2), // sacred soil
            [25868] = NewData(25868, JobIDs.SCH, 90, 120, 20, 80, 2), // expedient
            [16536] = NewData(16536, JobIDs.WHM, 80, 120, 20, 80, 2), // temperance
            [3569] = NewData(3569, JobIDs.WHM, 52, 90, 24, 50, 2), // asylum
            [24298] = NewData(24298, JobIDs.SGE, 50, 30, 15, 80, 2), // kerachole
            [24310] = NewData(24310, JobIDs.SGE, 76, 120, 20, 80, 2), // holos
            [24311] = NewData(24311, JobIDs.SGE, 80, 120, 15, 80, 2), // panhaima

            // MELEE
            [7549] = NewData(7549, JobRoles.DPSMelee, 22, 90, 10, 100, 1), // feint
            [2258] = NewData(2258, JobIDs.NIN, 18, 60, 15, 30, 3), // trick attack
            [3557] = NewData(3557, JobIDs.DRG, 52, 120, 15, 30, 3), // battle litany
            [7396] = NewData(7396, JobIDs.MNK, 70, 120, 15, 90, 3), // brotherhood
            [65] = NewData(65, JobIDs.MNK, 42, 90, 15, 50, 2), // mantra
            [24405] = NewData(24405, JobIDs.RPR, 72, 120, 20, 30, 3), // arcane circle

            // RANGED
            [118] = NewData(118, JobIDs.BRD, 50, 120, 15, 30, 3), // battle voice
            [7405] = NewData(7405, JobIDs.BRD, 88, 90, 15, 70, 2), // troubadour
            [7408] = NewData(7408, JobIDs.BRD, 66, 90, 15, 40, 2), // nature's minne
            [16012] = NewData(16012, JobIDs.DNC, 88, 90, 15, 70, 2), // shield samba
            [16889] = NewData(16889, JobIDs.MCH, 56, 90, 15, 70, 2), // tactician

            // CASTER
            [7560] = NewData(7560, JobRoles.DPSCaster, 8, 90, 10, 100, 1), // addle
            [7520] = NewData(7520, JobIDs.RDM, 58, 120, 20, 30, 3), // embolden
            [25801] = NewData(25801, JobIDs.SMN, 66, 120, 30, 30, 3), // searing light
        };

        #region helpers
        private static PartyCooldownData NewData(uint actionId, uint jobId, uint level, int cooldown, int effectDuration, int priority, int column)
        {
            PartyCooldownData data = new PartyCooldownData();
            data.ActionId = actionId;
            data.JobId = jobId;
            data.Role = JobRoles.Unknown;
            data.RequiredLevel = level;
            data.CooldownDuration = cooldown;
            data.EffectDuration = effectDuration;
            data.Priority = priority;
            data.Column = column;

            return data;
        }

        private static PartyCooldownData NewData(uint actionId, JobRoles role, uint level, int cooldown, int effectDuration, int priority, int column)
        {
            PartyCooldownData data = new PartyCooldownData();
            data.ActionId = actionId;
            data.JobId = 0;
            data.Role = role;
            data.RequiredLevel = level;
            data.CooldownDuration = cooldown;
            data.EffectDuration = effectDuration;
            data.Priority = priority;
            data.Column = column;

            return data;
        }
        #endregion
    }
}

