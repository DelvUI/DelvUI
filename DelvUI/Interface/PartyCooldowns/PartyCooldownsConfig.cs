using Dalamud.Logging;
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
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace DelvUI.Interface.PartyCooldowns
{
    public enum PartyCooldownsGrowthDirection
    {
        Down = 0,
        Up,
        Right,
        Left
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

        [Combo("Sections Growth Direction", "Down", "Up", "Right", "Left", spacing = true)]
        [Order(20)]
        public PartyCooldownsGrowthDirection GrowthDirection = PartyCooldownsGrowthDirection.Down;

        [DragInt2("Padding", min = -1000, max = 1000)]
        [Order(15)]
        public Vector2 Padding = new Vector2(0, -1);

        [Checkbox("Tooltips", spacing = true)]
        [Order(16)]
        public bool ShowTooltips = true;

        [Checkbox("Show Only in Duties", spacing = true, isMonitored = true)]
        [Order(20)]
        public bool ShowOnlyInDuties = true;

        [Checkbox("Show When Solo", isMonitored = true)]
        [Order(21)]
        public bool ShowWhenSolo = false;

        [NestedConfig("Visibility", 200)]
        public VisibilityConfig VisibilityConfig = new VisibilityConfig();
    }

    [Disableable(false)]
    [Exportable(false)]
    [DisableParentSettings("Position", "Anchor", "HideWhenInactive", "FillColor", "Background", "FillDirection")]
    [Section("Party Cooldowns", true)]
    [SubSection("Cooldown Bar", 0)]
    public class PartyCooldownsBarConfig : BarConfig
    {
        [Checkbox("Show Bar", spacing = true)]
        [Order(70)]
        public bool ShowBar = true;

        [ColorEdit4("Available Color")]
        [Order(71, collapseWith = nameof(ShowBar))]
        public PluginConfigColor AvailableColor = new PluginConfigColor(new Vector4(0f / 255f, 150f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Available Background Color")]
        [Order(72, collapseWith = nameof(ShowBar))]
        public PluginConfigColor AvailableBackgroundColor = new PluginConfigColor(new Vector4(0f / 255f, 150f / 255f, 0f / 255f, 25f / 100f));

        [ColorEdit4("Recharging Color")]
        [Order(73, collapseWith = nameof(ShowBar))]
        public PluginConfigColor RechargingColor = new PluginConfigColor(new Vector4(150f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Recharging Background Color")]
        [Order(74, collapseWith = nameof(ShowBar))]
        public PluginConfigColor RechargingBackgroundColor = new PluginConfigColor(new Vector4(150f / 255f, 0f / 255f, 0f / 255f, 25f / 100f));

        [Checkbox("Show Icon", spacing = true)]
        [Order(80)]
        public bool ShowIcon = true;

        [Checkbox("Show Icon Cooldown Animation")]
        [Order(81, collapseWith = nameof(ShowIcon))]
        public bool ShowIconCooldownAnimation = false;

        [Checkbox("Change Icon Border When Active")]
        [Order(82, collapseWith = nameof(ShowIcon))]
        public bool ChangeIconBorderWhenActive = false;

        [ColorEdit4("Icon Active Border Color")]
        [Order(83, collapseWith = nameof(ChangeIconBorderWhenActive))]
        public PluginConfigColor IconActiveBorderColor = new PluginConfigColor(new Vector4(255f / 255f, 200f / 255f, 35f / 255f, 100f / 100f));

        [DragInt("Icon Active Border Thickness", min = 1, max = 10)]
        [Order(84, collapseWith = nameof(ChangeIconBorderWhenActive))]
        public int IconActiveBorderThickness = 3;

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
        private uint _tankFilter = 0;
        private uint _healerFilter = 0;
        private uint _meleeFilter = 0;
        private uint _rangedFilter = 0;
        private uint _casterFilter = 0;

        public const int ColumnCount = 5;

        public delegate void CooldownsDataChangedEventHandler(PartyCooldownsDataConfig sender);
        public event CooldownsDataChangedEventHandler? CooldownsDataChangedEvent;

        public delegate void CooldownsDataEnabledChangedEventHandler(PartyCooldownsDataConfig sender);
        public event CooldownsDataEnabledChangedEventHandler? CooldownsDataEnabledChangedEvent;

        public new static PartyCooldownsDataConfig DefaultConfig() => new PartyCooldownsDataConfig();

        private string[] _enabledOptions = new string[] {
            "Enabled",
            "Party Cooldowns Only",
            "Party Frames Only",
            "Disabled"
        };


        public void UpdateDataIfNeeded()
        {
            bool needsSave = false;

            foreach (uint key in DefaultCooldowns.Keys)
            {
                PartyCooldownData? data = Cooldowns.FirstOrDefault(data => data.ActionId == key);
                PartyCooldownData defaultData = DefaultCooldowns[key];

                if (data == null)
                {
                    Cooldowns.Add(defaultData);
                    needsSave = true;
                }
                else if (data != null && !data.Equals(defaultData))
                {
                    data.RequiredLevel = defaultData.RequiredLevel;
                    data.JobId = defaultData.JobId;
                    data.JobIds = defaultData.JobIds;
                    data.Role = defaultData.Role;
                    data.Roles = defaultData.Roles;
                    data.CooldownDuration = defaultData.CooldownDuration;
                    data.EffectDuration = defaultData.EffectDuration;

                    needsSave = true;
                }
            }

            ExcelSheet<Action>? sheet = Plugin.DataManager.GetExcelSheet<Action>();
            ExcelSheet<ActionTransient>? descriptionsSheet = Plugin.DataManager.GetExcelSheet<ActionTransient>();

            foreach (PartyCooldownData cooldown in Cooldowns)
            {
                Action? action = sheet?.GetRow(cooldown.ActionId);
                if (action == null) { continue; }

                // get real cooldown from data
                // keep hardcoded value for technical finish
                if (action.Recast100ms > 0 && cooldown.ActionId != 16004)
                {
                    cooldown.CooldownDuration = action.Recast100ms / 10;
                }

                // not happy about this but didn't want to over-complicate things
                // special case for troubadour, shield samba and tactician
                if (cooldown.ActionId == 7405 || cooldown.ActionId == 16012 || cooldown.ActionId == 16889)
                {
                    cooldown.OverriddenCooldownText = "90-120";
                }

                cooldown.IconId = action.Icon;
                cooldown.Name = action.Name;
            }

            if (needsSave)
            {
                ConfigurationManager.Instance.SaveConfigurations(true);
            }
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

            DrawJobFilters();

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
                ImGui.TableSetupColumn("Enabled", ImGuiTableColumnFlags.WidthStretch, 27, 0);
                ImGui.TableSetupColumn("Icon", ImGuiTableColumnFlags.WidthStretch, 5, 1);
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 24, 2);
                ImGui.TableSetupColumn("Cooldown", ImGuiTableColumnFlags.WidthStretch, 12, 3);
                ImGui.TableSetupColumn("Duration", ImGuiTableColumnFlags.WidthStretch, 12, 4);
                ImGui.TableSetupColumn("Priority", ImGuiTableColumnFlags.WidthStretch, 13, 5);
                ImGui.TableSetupColumn("Section", ImGuiTableColumnFlags.WidthStretch, 13, 6);

                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();

                foreach (PartyCooldownData cooldown in Cooldowns)
                {
                    // apply filter
                    if (_roleFilter != JobRoles.Unknown)
                    {
                        if (!cooldown.HasRole(_roleFilter)) { continue; }

                        if (_roleFilter == JobRoles.Tank && _tankFilter != 0 && !cooldown.IsUsableBy(_tankFilter)) { continue; }
                        if (_roleFilter == JobRoles.Healer && _healerFilter != 0 && !cooldown.IsUsableBy(_healerFilter)) { continue; }
                        if (_roleFilter == JobRoles.DPSMelee && _meleeFilter != 0 && !cooldown.IsUsableBy(_meleeFilter)) { continue; }
                        if (_roleFilter == JobRoles.DPSRanged && _rangedFilter != 0 && !cooldown.IsUsableBy(_rangedFilter)) { continue; }
                        if (_roleFilter == JobRoles.DPSCaster && _casterFilter != 0 && !cooldown.IsUsableBy(_casterFilter)) { continue; }
                    }

                    Action? action = sheet?.GetRow(cooldown.ActionId);

                    ImGui.PushID(cooldown.ActionId.ToString());
                    ImGui.TableNextRow(ImGuiTableRowFlags.None, iconSize.Y);

                    // enabled
                    if (ImGui.TableSetColumnIndex(0))
                    {
                        ImGui.PushItemWidth(178);
                        ImGui.SetCursorPos(new Vector2(ImGui.GetCursorPosX() + 2, ImGui.GetCursorPosY() + 2));
                        int enabled = (int)cooldown.EnabledV2;
                        if (ImGui.Combo($"##{cooldown.ActionId}_enabled", ref enabled, _enabledOptions, _enabledOptions.Length))
                        {
                            changed = true;
                            cooldown.EnabledV2 = (PartyCooldownEnabled)enabled;
                            CooldownsDataEnabledChangedEvent?.Invoke(this);
                        }
                    }

                    // icon
                    if (ImGui.TableSetColumnIndex(1) && action != null)
                    {
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 3);
                        DrawHelper.DrawIcon<Action>(action, ImGui.GetCursorPos(), iconSize, false, false);
                    }

                    // name
                    if (ImGui.TableSetColumnIndex(2))
                    {
                        ImGui.Text(cooldown.Name);
                    }

                    // cooldown
                    if (ImGui.TableSetColumnIndex(3))
                    {
                        string cooldownText = cooldown.OverriddenCooldownText != null ? cooldown.OverriddenCooldownText : $"{cooldown.CooldownDuration}";
                        ImGui.Text(cooldownText);
                    }

                    // duration
                    if (ImGui.TableSetColumnIndex(4))
                    {
                        ImGui.Text($"{cooldown.EffectDuration}");
                    }

                    // priority
                    if (ImGui.TableSetColumnIndex(5))
                    {
                        ImGui.PushItemWidth(90);
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 2);

                        if (ImGui.DragInt($"##{cooldown.ActionId}_priority", ref cooldown.Priority, 1, 0, 100, "%i", ImGuiSliderFlags.NoInput))
                        {
                            changed = true;
                            CooldownsDataChangedEvent?.Invoke(this);
                        }

                        ImGuiHelper.SetTooltip("Priority determines which cooldows show first on the list.");
                    }

                    // column
                    if (ImGui.TableSetColumnIndex(6))
                    {
                        ImGui.PushItemWidth(90);
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 2);

                        if (ImGui.DragInt($"##{cooldown.ActionId}_column", ref cooldown.Column, 0.1f, 1, ColumnCount, "%i", ImGuiSliderFlags.NoInput))
                        {
                            changed = true;
                            CooldownsDataChangedEvent?.Invoke(this);
                        }

                        ImGuiHelper.SetTooltip("Allows to separate cooldowns in different columns.");
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

        private void DrawJobFilter(string name, uint job, ref uint filter)
        {
            ImGui.SameLine();
            if (ImGui.RadioButton(name, filter == job))
            {
                filter = job;
            }
        }

        private void DrawJobFilters()
        {
            if (_roleFilter ==  JobRoles.Unknown) { return; }

            ImGui.Text("\t\t\t\t   ");

            if (_roleFilter == JobRoles.Tank)
            {
                DrawJobFilter("All##tank", 0, ref _tankFilter);
                DrawJobFilter(JobsHelper.JobNames[JobIDs.PLD], JobIDs.PLD, ref _tankFilter);
                DrawJobFilter(JobsHelper.JobNames[JobIDs.WAR], JobIDs.WAR, ref _tankFilter);
                DrawJobFilter(JobsHelper.JobNames[JobIDs.DRK], JobIDs.DRK, ref _tankFilter);
                DrawJobFilter(JobsHelper.JobNames[JobIDs.GNB], JobIDs.GNB, ref _tankFilter);
            }
            else if (_roleFilter == JobRoles.Healer)
            {
                DrawJobFilter("All##healer", 0, ref _healerFilter);
                DrawJobFilter(JobsHelper.JobNames[JobIDs.WHM], JobIDs.WHM, ref _healerFilter);
                DrawJobFilter(JobsHelper.JobNames[JobIDs.SCH], JobIDs.SCH, ref _healerFilter);
                DrawJobFilter(JobsHelper.JobNames[JobIDs.AST], JobIDs.AST, ref _healerFilter);
                DrawJobFilter(JobsHelper.JobNames[JobIDs.SGE], JobIDs.SGE, ref _healerFilter);
            }
            else if (_roleFilter == JobRoles.DPSMelee)
            {
                DrawJobFilter("All##melee", 0, ref _meleeFilter);
                DrawJobFilter(JobsHelper.JobNames[JobIDs.MNK], JobIDs.MNK, ref _meleeFilter);
                DrawJobFilter(JobsHelper.JobNames[JobIDs.DRG], JobIDs.DRG, ref _meleeFilter);
                DrawJobFilter(JobsHelper.JobNames[JobIDs.NIN], JobIDs.NIN, ref _meleeFilter);
                DrawJobFilter(JobsHelper.JobNames[JobIDs.SAM], JobIDs.SAM, ref _meleeFilter);
                DrawJobFilter(JobsHelper.JobNames[JobIDs.RPR], JobIDs.RPR, ref _meleeFilter);
            }
            else if (_roleFilter == JobRoles.DPSRanged)
            {
                DrawJobFilter("All##ranged", 0, ref _rangedFilter);
                DrawJobFilter(JobsHelper.JobNames[JobIDs.BRD], JobIDs.BRD, ref _rangedFilter);
                DrawJobFilter(JobsHelper.JobNames[JobIDs.MCH], JobIDs.MCH, ref _rangedFilter);
                DrawJobFilter(JobsHelper.JobNames[JobIDs.DNC], JobIDs.DNC, ref _rangedFilter);
            }
            else if (_roleFilter == JobRoles.DPSCaster)
            {
                DrawJobFilter("All##caster", 0, ref _casterFilter);
                DrawJobFilter(JobsHelper.JobNames[JobIDs.BLM], JobIDs.BLM, ref _casterFilter);
                DrawJobFilter(JobsHelper.JobNames[JobIDs.SMN], JobIDs.SMN, ref _casterFilter);
                DrawJobFilter(JobsHelper.JobNames[JobIDs.RDM], JobIDs.RDM, ref _casterFilter);
            }
        }

        public static Dictionary<uint, PartyCooldownData> DefaultCooldowns = new Dictionary<uint, PartyCooldownData>()
        {
            // PARTY-WIDE EFFECTS

            // Default columns:
            // 1. Role mitigations (Reprisal, Addle, Feint)
            // 2. Job specific mitigations
            // 3. Damage buffs / burst window cooldowns
            // 4. Personal mitigations / immunities / externals
            // 5. Misc (Provoke, Shirk, Switcast, Rescue, etc)

            // TANKS -------------------------------------------------------------------------------------------------
            [7535] = NewData(7535, JobRoles.Tank, 22, 60, 10, 100, 1, PartyCooldownEnabled.PartyFrames), // reprisal
            [7531] = NewData(7531, JobRoles.Tank, 8, 90, 20, 50, 4, PartyCooldownEnabled.PartyFrames), // rampart
            [7533] = NewData(7533, JobRoles.Tank, 15, 30, 1, 50, 5, PartyCooldownEnabled.PartyFrames), // provoke
            [7537] = NewData(7537, JobRoles.Tank, 48, 120, 1, 50, 5, PartyCooldownEnabled.PartyFrames), // shirk

            // PLD
            [3540] = NewData(3540, JobIDs.PLD, 56, 90, 30, 90, 2, PartyCooldownEnabled.PartyFrames), // divine veil
            [7385] = NewData(7385, JobIDs.PLD, 70, 120, 18, 90, 2, PartyCooldownEnabled.PartyFrames), // passage of arms
            [20] = NewData(20, JobIDs.PLD, 2, 60, 20, 10, 3, PartyCooldownEnabled.PartyFrames), // fight or flight
            [17] = NewData(17, JobIDs.PLD, 38, 120, 15, 90, 4, PartyCooldownEnabled.PartyFrames), // sentinel
            [22] = NewData(22, JobIDs.PLD, 52, 90, 10, 90, 4, PartyCooldownEnabled.PartyFrames), // bulwark
            [27] = NewData(27, JobIDs.PLD, 45, 120, 12, 90, 4, PartyCooldownEnabled.PartyFrames), // cover
            [30] = NewData(30, JobIDs.PLD, 50, 420, 10, 100, 4, PartyCooldownEnabled.PartyFrames), // hallowed ground

            // WAR
            [7388] = NewData(7388, JobIDs.WAR, 68, 90, 30, 90, 2, PartyCooldownEnabled.PartyFrames), // shake it off
            [52] = NewData(52, JobIDs.WAR, 50, 60, 30, 10, 3, PartyCooldownEnabled.PartyFrames), // infuriate
            [7389] = NewData(7389, JobIDs.WAR, 70, 60, 30, 10, 3, PartyCooldownEnabled.PartyFrames), // inner release
            [40] = NewData(40, JobIDs.WAR, 30, 90, 10, 90, 4, PartyCooldownEnabled.PartyFrames), // thrill of battle
            [44] = NewData(44, JobIDs.WAR, 38, 120, 15, 90, 4, PartyCooldownEnabled.PartyFrames), // vengeance
            [3552] = NewData(3552, JobIDs.WAR, 58, 60, 15, 90, 4, PartyCooldownEnabled.PartyFrames), // equilibrium
            [25751] = NewData(25751, JobIDs.WAR, 82, 25, 8, 90, 4, PartyCooldownEnabled.PartyFrames), // bloodwhetting
            [43] = NewData(43, JobIDs.WAR, 42, 240, 10, 100, 4, PartyCooldownEnabled.PartyFrames), // holmgang

            // DRK
            [16471] = NewData(16471, JobIDs.DRK, 76, 90, 15, 90, 2, PartyCooldownEnabled.PartyFrames), // dark missionary
            [3625] = NewData(3625, JobIDs.DRK, 35, 60, 15, 10, 3, PartyCooldownEnabled.PartyFrames), // blood weapon
            [7390] = NewData(7390, JobIDs.DRK, 68, 60, 15, 10, 3, PartyCooldownEnabled.PartyFrames), // delirium
            [16472] = NewData(16472, JobIDs.DRK, 80, 120, 20, 10, 3, PartyCooldownEnabled.PartyFrames), // living shadow
            [3636] = NewData(3636, JobIDs.DRK, 38, 120, 15, 90, 4, PartyCooldownEnabled.PartyFrames), // shadow wall
            [3634] = NewData(3634, JobIDs.DRK, 45, 60, 10, 90, 4, PartyCooldownEnabled.PartyFrames), // dark mind
            [25754] = NewData(25754, JobIDs.DRK, 82, 60, 10, 90, 4, PartyCooldownEnabled.PartyFrames), // oblation
            [3638] = NewData(3638, JobIDs.DRK, 50, 300, 10, 100, 4, PartyCooldownEnabled.PartyFrames), // living dead

            // GNB
            [16160] = NewData(16160, JobIDs.GNB, 64, 90, 15, 90, 2, PartyCooldownEnabled.PartyFrames), // heart of light
            [16164] = NewData(16164, JobIDs.GNB, 76, 120, 1, 10, 3, PartyCooldownEnabled.PartyFrames), // bloodfest
            [16140] = NewData(16140, JobIDs.GNB, 6, 90, 20, 90, 4, PartyCooldownEnabled.PartyFrames), // camouflage
            [16148] = NewData(16148, JobIDs.GNB, 38, 120, 15, 90, 4, PartyCooldownEnabled.PartyFrames), // nebula
            [16161] = NewData(16161, JobIDs.GNB, 68, 25, 7, 90, 4, PartyCooldownEnabled.PartyFrames), // heart of stone
            [16152] = NewData(16152, JobIDs.GNB, 50, 360, 10, 100, 4, PartyCooldownEnabled.PartyFrames), // superbolide

            // HEALER -------------------------------------------------------------------------------------------------
            [7571] = NewData(7571, JobRoles.Healer, 48, 120, 0, 80, 5, PartyCooldownEnabled.Disabled), // rescue

            // AST
            [16552] = NewData(16552, JobIDs.AST, 50, 120, 15, 30, 3, PartyCooldownEnabled.PartyCooldowns), // divination
            [3613] = NewData(3613, JobIDs.AST, 58, 60, 18, 80, 2, PartyCooldownEnabled.PartyFrames), // collective unconscious
            [16553] = NewData(16553, JobIDs.AST, 60, 60, 15, 80, 2, PartyCooldownEnabled.PartyFrames), // celestial opposition
            [7439] = NewData(7439, JobIDs.AST, 62, 60, 20, 80, 2, PartyCooldownEnabled.PartyFrames), // earthly star (stellar detonation = 8324)
            [16559] = NewData(16559, JobIDs.AST, 80, 120, 20, 80, 2, PartyCooldownEnabled.PartyFrames), // neutral sect
            [25873] = NewData(25873, JobIDs.AST, 86, 60, 8, 80, 2, PartyCooldownEnabled.PartyFrames), // exaltation
            [25874] = NewData(25874, JobIDs.AST, 90, 180, 15, 80, 2, PartyCooldownEnabled.PartyFrames), // macrocosmos (microcosmos = 25875)
            [3606] = NewData(3606, JobIDs.AST, 6, 90, 15, 10, 4, PartyCooldownEnabled.PartyFrames), // lightspeed

            // SCH
            [805] = NewData(805, JobIDs.SCH, 40, 120, 20, 50, 2, PartyCooldownEnabled.PartyFrames), // fey illumination
            [188] = NewData(188, JobIDs.SCH, 50, 30, 15, 80, 2, PartyCooldownEnabled.PartyFrames), // sacred soil
            [3585] = NewData(3585, JobIDs.SCH, 56, 90, 1, 80, 2, PartyCooldownEnabled.PartyFrames), // deployment tactics
            [25867] = NewData(25867, JobIDs.SCH, 86, 60, 10, 80, 2, PartyCooldownEnabled.PartyFrames), // protraction
            [25868] = NewData(25868, JobIDs.SCH, 90, 120, 20, 80, 2, PartyCooldownEnabled.PartyFrames), // expedient
            [7436] = NewData(7436, JobIDs.SCH, 66, 120, 15, 30, 3, PartyCooldownEnabled.PartyCooldowns), // chain stratagem
            [7434] = NewData(7434, JobIDs.SCH, 62, 45, 1, 50, 4, PartyCooldownEnabled.PartyFrames), // excogitation

            // WHM
            [16536] = NewData(16536, JobIDs.WHM, 80, 120, 20, 80, 2, PartyCooldownEnabled.PartyFrames), // temperance
            [3569] = NewData(3569, JobIDs.WHM, 52, 90, 24, 50, 2, PartyCooldownEnabled.PartyFrames), // asylum
            [25862] = NewData(25862, JobIDs.WHM, 90, 180, 15, 80, 2, PartyCooldownEnabled.PartyFrames), // liturgy of the bell
            [136] = NewData(136, JobIDs.WHM, 30, 120, 15, 10, 4, PartyCooldownEnabled.PartyFrames), // presence of mind
            [140] = NewData(140, JobIDs.WHM, 50, 180, 1, 10, 4, PartyCooldownEnabled.PartyFrames), // benediction
            [3570] = NewData(3570, JobIDs.WHM, 60, 60, 1, 10, 4, PartyCooldownEnabled.PartyFrames), // tetragrammaton
            [25861] = NewData(25861, JobIDs.WHM, 86, 60, 8, 10, 4, PartyCooldownEnabled.PartyFrames), // aquaveil

            // SGE
            [24298] = NewData(24298, JobIDs.SGE, 50, 30, 15, 80, 2, PartyCooldownEnabled.PartyFrames), // kerachole
            [24302] = NewData(24302, JobIDs.SGE, 60, 60, 15, 80, 2, PartyCooldownEnabled.PartyFrames), // physis ii
            [24310] = NewData(24310, JobIDs.SGE, 76, 120, 20, 80, 2, PartyCooldownEnabled.PartyFrames), // holos
            [24311] = NewData(24311, JobIDs.SGE, 80, 120, 15, 80, 2, PartyCooldownEnabled.PartyFrames), // panhaima
            [24318] = NewData(24318, JobIDs.SGE, 90, 120, 1, 80, 2, PartyCooldownEnabled.PartyFrames), // pneuma
            [24303] = NewData(24303, JobIDs.SGE, 62, 45, 15, 10, 4, PartyCooldownEnabled.PartyFrames), // taurochole
            [24305] = NewData(24305, JobIDs.SGE, 70, 120, 15, 10, 4, PartyCooldownEnabled.PartyFrames), // haima
            [24317] = NewData(24317, JobIDs.SGE, 86, 60, 10, 10, 4, PartyCooldownEnabled.PartyFrames), // krasis

            // MELEE -------------------------------------------------------------------------------------------------
            [7549] = NewData(7549, JobRoles.DPSMelee, 22, 90, 10, 100, 1, PartyCooldownEnabled.PartyFrames), // feint
            [7542] = NewData(7542, JobRoles.DPSMelee, 12, 90, 20, 10, 4, PartyCooldownEnabled.PartyFrames), // bloodbath

            // SAM
            // lol?

            // NIN
            [2248] = NewData(2248, JobIDs.NIN, 15, 120, 20, 30, 3, PartyCooldownEnabled.PartyCooldowns), // mug
            [2258] = NewData(2258, JobIDs.NIN, 18, 60, 15, 10, 3, PartyCooldownEnabled.PartyFrames), // trick attack
            [2241] = NewData(2241, JobIDs.NIN, 2, 120, 20, 20, 4, PartyCooldownEnabled.PartyFrames), // shade shift

            // DRG
            [3557] = NewData(3557, JobIDs.DRG, 52, 120, 15, 30, 3, PartyCooldownEnabled.PartyCooldowns), // battle litany
            [7398] = NewData(7398, JobIDs.DRG, 66, 120, 20, 30, 3, PartyCooldownEnabled.PartyCooldowns), // dragon sight
            [85] = NewData(85, JobIDs.DRG, 30, 60, 20, 10, 3, PartyCooldownEnabled.PartyFrames), // lance charge

            // MNK
            [65] = NewData(65, JobIDs.MNK, 42, 90, 15, 50, 2, PartyCooldownEnabled.PartyFrames), // mantra
            [7396] = NewData(7396, JobIDs.MNK, 70, 120, 15, 90, 3, PartyCooldownEnabled.PartyCooldowns), // brotherhood
            [7395] = NewData(7395, JobIDs.MNK, 68, 60, 20, 10, 3, PartyCooldownEnabled.PartyFrames), // riddle of fire
            [7394] = NewData(7394, JobIDs.MNK, 64, 120, 15, 20, 4, PartyCooldownEnabled.PartyFrames), // riddle of earth

            // RPR
            [24405] = NewData(24405, JobIDs.RPR, 72, 120, 20, 30, 3, PartyCooldownEnabled.PartyCooldowns), // arcane circle
            [24404] = NewData(24404, JobIDs.RPR, 40, 30, 5, 10, 4, PartyCooldownEnabled.PartyFrames), // arcane crest

            // RANGED -------------------------------------------------------------------------------------------------
            // BRD
            [118] = NewData(118, JobIDs.BRD, 50, 120, 15, 30, 3, PartyCooldownEnabled.PartyCooldowns), // battle voice
            [7405] = NewData(7405, JobIDs.BRD, 62, 90, 15, 70, 2, PartyCooldownEnabled.PartyFrames, "90-120"), // troubadour
            [7408] = NewData(7408, JobIDs.BRD, 66, 120, 15, 40, 2, PartyCooldownEnabled.PartyFrames), // nature's minne
            [25785] = NewData(25785, JobIDs.BRD, 90, 110, 15, 30, 3, PartyCooldownEnabled.PartyCooldowns), // radiant finale

            // DNC
            [16012] = NewData(16012, JobIDs.DNC, 56, 90, 15, 70, 2, PartyCooldownEnabled.PartyFrames, "90-120"), // shield samba
            [16004] = NewData(16004, JobIDs.DNC, 70, 120, 20, 30, 3, PartyCooldownEnabled.PartyCooldowns), // technical step / finish

            // MCH
            [16889] = NewData(16889, JobIDs.MCH, 56, 90, 15, 70, 2, PartyCooldownEnabled.PartyFrames, "90-120"), // tactician
            [2887] = NewData(2887, JobIDs.MCH, 62, 120, 10, 70, 2, PartyCooldownEnabled.PartyFrames), // dismantle

            // CASTER -------------------------------------------------------------------------------------------------
            [7560] = NewData(7560, JobRoles.DPSCaster, 8, 90, 10, 100, 1, PartyCooldownEnabled.PartyFrames), // addle

            // RDM
            [25857] = NewData(25857, JobIDs.RDM, 86, 120, 10, 70, 2, PartyCooldownEnabled.PartyCooldownsAndPartyFrames), // magick barrier
            [7520] = NewData(7520, JobIDs.RDM, 58, 120, 20, 30, 3, PartyCooldownEnabled.PartyCooldowns), // embolden

            // SMN
            [25801] = NewData(25801, JobIDs.SMN, 66, 120, 30, 30, 3, PartyCooldownEnabled.PartyCooldowns), // searing light
            [25799] = NewData(25799, JobIDs.SMN, 2, 60, 30, 10, 3, PartyCooldownEnabled.PartyFrames), // radiant aegis

            // BLM
            [3573] = NewData(3573, JobIDs.BLM, 52, 120, 30, 90, 3, PartyCooldownEnabled.PartyFrames), // ley lines
            [157] = NewData(157, JobIDs.BLM, 38, 120, 20, 10, 4, PartyCooldownEnabled.PartyFrames), // manaward

            // MULTI-ROLE  -------------------------------------------------------------------------------------------------
            [7541] = NewData(7541, new List<JobRoles>() { JobRoles.DPSMelee, JobRoles.DPSRanged }, 8, 120, 0, 80, 4, PartyCooldownEnabled.PartyFrames), // second wind
            [7561] = NewData(7561, new List<JobRoles>() { JobRoles.Healer, JobRoles.DPSCaster }, 18, 60, 1, 80, 5, PartyCooldownEnabled.PartyFrames), // swiftcast
            [7562] = NewData(7562, new List<JobRoles>() { JobRoles.Healer, JobRoles.DPSCaster }, 14, 60, 21, 80, 5, PartyCooldownEnabled.Disabled), // lucid dreaming
        };

        #region helpers
        private static PartyCooldownData NewData(uint actionId, uint jobId, uint level, int cooldown, int effectDuration, int priority, int column, PartyCooldownEnabled enabled, string? overriddenCooldownText = null)
        {
            PartyCooldownData data = NewData(actionId, level, cooldown, effectDuration, priority, column, enabled, overriddenCooldownText);
            data.JobId = jobId;
            data.Role = JobRoles.Unknown;

            return data;
        }

        private static PartyCooldownData NewData(uint actionId, JobRoles role, uint level, int cooldown, int effectDuration, int priority, int column, PartyCooldownEnabled enabled, string? overriddenCooldownText = null)
        {
            PartyCooldownData data = NewData(actionId, level, cooldown, effectDuration, priority, column, enabled, overriddenCooldownText);
            data.JobId = 0;
            data.Role = role;

            return data;
        }

        private static PartyCooldownData NewData(uint actionId, List<JobRoles> roles, uint level, int cooldown, int effectDuration, int priority, int column, PartyCooldownEnabled enabled, string? overriddenCooldownText = null)
        {
            PartyCooldownData data = NewData(actionId, level, cooldown, effectDuration, priority, column, enabled, overriddenCooldownText);
            data.JobId = 0;
            data.Role = JobRoles.Unknown;
            data.Roles = roles;

            return data;
        }

        private static PartyCooldownData NewData(uint actionId, uint level, int cooldown, int effectDuration, int priority, int column, PartyCooldownEnabled enabled, string? overriddenCooldownText = null)
        {
            PartyCooldownData data = new PartyCooldownData();
            data.ActionId = actionId;
            data.RequiredLevel = level;
            data.CooldownDuration = cooldown;
            data.EffectDuration = effectDuration;
            data.Priority = priority;
            data.Column = column;
            data.EnabledV2 = enabled;
            data.OverriddenCooldownText = overriddenCooldownText;

            return data;
        }
        #endregion
    }
}

