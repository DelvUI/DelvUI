using Dalamud.Interface;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.StatusEffects
{
    [Section("Buffs and Debuffs")]
    [SubSection("Player Buffs", 0)]
    public class PlayerBuffsListConfig : UnitFrameStatusEffectsListConfig
    {
        public new static PlayerBuffsListConfig DefaultConfig()
        {
            var screenSize = ImGui.GetMainViewport().Size;
            var pos = new Vector2(screenSize.X * 0.38f, -screenSize.Y * 0.45f);
            var iconConfig = new StatusEffectIconConfig();
            iconConfig.DispellableBorderConfig.Enabled = false;

            return new PlayerBuffsListConfig(pos, HUDConstants.DefaultStatusEffectsListSize, true, false, true, GrowthDirections.Left | GrowthDirections.Down, iconConfig);
        }

        public PlayerBuffsListConfig(Vector2 position, Vector2 size, bool showBuffs, bool showDebuffs, bool showPermanentEffects,
            GrowthDirections growthDirections, StatusEffectIconConfig iconConfig)
            : base(position, size, showBuffs, showDebuffs, showPermanentEffects, growthDirections, iconConfig)
        {
        }
    }

    [Section("Buffs and Debuffs")]
    [SubSection("Player Debuffs", 0)]
    public class PlayerDebuffsListConfig : UnitFrameStatusEffectsListConfig
    {
        public new static PlayerDebuffsListConfig DefaultConfig()
        {
            var screenSize = ImGui.GetMainViewport().Size;
            var pos = new Vector2(screenSize.X * 0.38f, -screenSize.Y * 0.45f + HUDConstants.DefaultStatusEffectsListSize.Y);
            var iconConfig = new StatusEffectIconConfig();

            return new PlayerDebuffsListConfig(pos, HUDConstants.DefaultStatusEffectsListSize, false, true, true, GrowthDirections.Left | GrowthDirections.Down, iconConfig);
        }

        public PlayerDebuffsListConfig(Vector2 position, Vector2 size, bool showBuffs, bool showDebuffs, bool showPermanentEffects,
            GrowthDirections growthDirections, StatusEffectIconConfig iconConfig)
            : base(position, size, showBuffs, showDebuffs, showPermanentEffects, growthDirections, iconConfig)
        {
        }
    }

    [Section("Buffs and Debuffs")]
    [SubSection("Target Buffs", 0)]
    public class TargetBuffsListConfig : UnitFrameStatusEffectsListConfig
    {
        public new static TargetBuffsListConfig DefaultConfig()
        {
            var pos = new Vector2(0, -1);
            var iconConfig = new StatusEffectIconConfig();
            iconConfig.DispellableBorderConfig.Enabled = false;

            var config = new TargetBuffsListConfig(pos, HUDConstants.DefaultStatusEffectsListSize, true, false, true, GrowthDirections.Right | GrowthDirections.Up, iconConfig);
            config.AnchorToUnitFrame = true;
            config.UnitFrameAnchor = DrawAnchor.TopLeft;

            return config;
        }

        public TargetBuffsListConfig(Vector2 position, Vector2 size, bool showBuffs, bool showDebuffs, bool showPermanentEffects,
            GrowthDirections growthDirections, StatusEffectIconConfig iconConfig)
            : base(position, size, showBuffs, showDebuffs, showPermanentEffects, growthDirections, iconConfig)
        {
        }
    }

    [Section("Buffs and Debuffs")]
    [SubSection("Target Debuffs", 0)]
    public class TargetDebuffsListConfig : UnitFrameStatusEffectsListConfig
    {
        public new static TargetDebuffsListConfig DefaultConfig()
        {
            var pos = new Vector2(0, -85);
            var iconConfig = new StatusEffectIconConfig();
            iconConfig.DispellableBorderConfig.Enabled = false;

            var config = new TargetDebuffsListConfig(pos, HUDConstants.DefaultStatusEffectsListSize, false, true, true, GrowthDirections.Right | GrowthDirections.Up, iconConfig);
            config.AnchorToUnitFrame = true;
            config.UnitFrameAnchor = DrawAnchor.TopLeft;

            return config;
        }

        public TargetDebuffsListConfig(Vector2 position, Vector2 size, bool showBuffs, bool showDebuffs, bool showPermanentEffects,
            GrowthDirections growthDirections, StatusEffectIconConfig iconConfig)
            : base(position, size, showBuffs, showDebuffs, showPermanentEffects, growthDirections, iconConfig)
        {
        }
    }

    [Section("Buffs and Debuffs")]
    [SubSection("Focus Target Buffs", 0)]
    public class FocusTargetBuffsListConfig : UnitFrameStatusEffectsListConfig
    {
        public new static FocusTargetBuffsListConfig DefaultConfig()
        {
            var pos = new Vector2(0, -1);
            var iconConfig = new StatusEffectIconConfig();
            iconConfig.DispellableBorderConfig.Enabled = false;

            var config = new FocusTargetBuffsListConfig(pos, HUDConstants.DefaultStatusEffectsListSize, true, false, true, GrowthDirections.Right | GrowthDirections.Up, iconConfig);
            config.AnchorToUnitFrame = true;
            config.UnitFrameAnchor = DrawAnchor.TopLeft;

            return config;
        }

        public FocusTargetBuffsListConfig(Vector2 position, Vector2 size, bool showBuffs, bool showDebuffs, bool showPermanentEffects,
            GrowthDirections growthDirections, StatusEffectIconConfig iconConfig)
            : base(position, size, showBuffs, showDebuffs, showPermanentEffects, growthDirections, iconConfig)
        {
        }
    }

    [Section("Buffs and Debuffs")]
    [SubSection("Focus Target Debuffs", 0)]
    public class FocusTargetDebuffsListConfig : UnitFrameStatusEffectsListConfig
    {
        public new static FocusTargetDebuffsListConfig DefaultConfig()
        {
            var pos = new Vector2(0, -85);
            var iconConfig = new StatusEffectIconConfig();
            iconConfig.DispellableBorderConfig.Enabled = false;

            var config = new FocusTargetDebuffsListConfig(pos, HUDConstants.DefaultStatusEffectsListSize, false, true, true, GrowthDirections.Right | GrowthDirections.Up, iconConfig);
            config.AnchorToUnitFrame = true;
            config.UnitFrameAnchor = DrawAnchor.TopLeft;

            return config;
        }

        public FocusTargetDebuffsListConfig(Vector2 position, Vector2 size, bool showBuffs, bool showDebuffs, bool showPermanentEffects,
            GrowthDirections growthDirections, StatusEffectIconConfig iconConfig)
            : base(position, size, showBuffs, showDebuffs, showPermanentEffects, growthDirections, iconConfig)
        {
        }
    }

    public abstract class UnitFrameStatusEffectsListConfig : StatusEffectsListConfig
    {
        [Checkbox("Anchor to Unit Frame")]
        [Order(16)]
        public bool AnchorToUnitFrame = false;

        [Anchor("Unit Frame Anchor")]
        [Order(17, collapseWith = nameof(AnchorToUnitFrame))]
        public DrawAnchor UnitFrameAnchor = DrawAnchor.TopLeft;

        public UnitFrameStatusEffectsListConfig(Vector2 position, Vector2 size, bool showBuffs, bool showDebuffs, bool showPermanentEffects,
            GrowthDirections growthDirections, StatusEffectIconConfig iconConfig)
            : base(position, size, showBuffs, showDebuffs, showPermanentEffects, growthDirections, iconConfig)
        {
        }
    }

    public class StatusEffectsListConfig : MovablePluginConfigObject
    {
        public bool ShowBuffs;
        public bool ShowDebuffs;

        [DragInt2("Size", min = 1, max = 4000)]
        [Order(15)]
        public Vector2 Size;

        [DragInt2("Icon Padding", min = 0, max = 100)]
        [Order(19)]
        public Vector2 IconPadding = new(2, 2);

        [Checkbox("Preview", isMonitored = true)]
        [Order(20)]
        public bool Preview;

        [Checkbox("Fill Rows First", spacing = true)]
        [Order(25)]
        public bool FillRowsFirst = true;

        [Combo("Icons Growth Direction",
            "Right and Down",
            "Right and Up",
            "Left and Down",
            "Left and Up",
            "Centered and Up",
            "Centered and Down"
        )]
        [Order(30)]
        public int Directions;

        [DragInt("Limit (-1 for no limit)", min = -1, max = 1000)]
        [Order(35)]
        public int Limit = -1;

        [Checkbox("Permanent Effects", spacing = true)]
        [Order(40)]
        public bool ShowPermanentEffects;

        [Checkbox("Only My Effects")]
        [Order(45)]
        public bool ShowOnlyMine = false;

        [Checkbox("My Effects First")]
        [Order(50)]
        public bool ShowMineFirst = false;

        [Checkbox("Pet As Own Effect")]
        [Order(55)]
        public bool IncludePetAsOwn = false;

        [Checkbox("Tooltips")]
        [Order(60)]
        public bool ShowTooltips = true;

        [Checkbox("Disable Interaction", help = "Enabling this will disable right clicking buffs off, or the shortcut to blacklist/whitelist a status effect.")]
        [Order(61)]
        public bool DisableInteraction = false;

        [NestedConfig("Icons", 65)]
        public StatusEffectIconConfig IconConfig;

        [NestedConfig("Filter Status Effects", 70, separator = true, spacing = false, collapsingHeader = false)]
        public StatusEffectsBlacklistConfig BlacklistConfig = new StatusEffectsBlacklistConfig();


        public StatusEffectsListConfig(Vector2 position, Vector2 size, bool showBuffs, bool showDebuffs, bool showPermanentEffects,
                                       GrowthDirections growthDirections, StatusEffectIconConfig iconConfig)
        {
            Position = position;
            Size = size;
            ShowBuffs = showBuffs;
            ShowDebuffs = showDebuffs;
            ShowPermanentEffects = showPermanentEffects;

            SetGrowthDirections(growthDirections);

            IconConfig = iconConfig;

            Strata = StrataLevel.HIGH;
        }

        private void SetGrowthDirections(GrowthDirections growthDirections)
        {
            var index = DirectionOptionsValues.FindIndex(d => d == growthDirections);
            if (index > 0)
            {
                Directions = index;
            }
        }

        public GrowthDirections GetGrowthDirections()
        {
            if (Directions > 0 && Directions < DirectionOptionsValues.Count)
            {
                return DirectionOptionsValues[Directions];
            }

            return DirectionOptionsValues[0];
        }

        [JsonIgnore]
        internal List<GrowthDirections> DirectionOptionsValues = new List<GrowthDirections>()
        {
            GrowthDirections.Right | GrowthDirections.Down,
            GrowthDirections.Right | GrowthDirections.Up,
            GrowthDirections.Left | GrowthDirections.Down,
            GrowthDirections.Left | GrowthDirections.Up,
            GrowthDirections.Centered | GrowthDirections.Up,
            GrowthDirections.Centered | GrowthDirections.Down
        };
    }

    [Exportable(false)]
    [Disableable(false)]
    public class StatusEffectIconConfig : PluginConfigObject
    {
        [DragInt2("Icon Size", min = 1, max = 1000)]
        [Order(5)]
        public Vector2 Size = new(40, 40);

        [Checkbox("Crop Icon", spacing = true)]
        [Order(20)]
        public bool CropIcon = true;

        [NestedConfig("Border", 25, collapseWith = nameof(CropIcon), collapsingHeader = false)]
        public StatusEffectIconBorderConfig BorderConfig = new();

        [NestedConfig("Dispellable Effects Border", 30, collapseWith = nameof(CropIcon), collapsingHeader = false)]
        public StatusEffectIconBorderConfig DispellableBorderConfig = new(new PluginConfigColor(new Vector4(141f / 255f, 206f / 255f, 229f / 255f, 100f / 100f)), 2);

        [NestedConfig("My Effects Border", 35, collapseWith = nameof(CropIcon), collapsingHeader = false)]
        public StatusEffectIconBorderConfig OwnedBorderConfig = new(new PluginConfigColor(new Vector4(35f / 255f, 179f / 255f, 69f / 255f, 100f / 100f)), 1);

        [NestedConfig("Duration", 50)]
        public LabelConfig DurationLabelConfig;

        [NestedConfig("Stacks", 60)]
        public LabelConfig StacksLabelConfig;

        public StatusEffectIconConfig(LabelConfig? durationLabelConfig = null, LabelConfig? stacksLabelConfig = null)
        {
            DurationLabelConfig = durationLabelConfig ?? StatusEffectsListsDefaults.DefaultDurationLabelConfig();
            StacksLabelConfig = stacksLabelConfig ?? StatusEffectsListsDefaults.DefaultStacksLabelConfig();
        }
    }

    [Exportable(false)]
    public class StatusEffectIconBorderConfig : PluginConfigObject
    {
        [ColorEdit4("Color")]
        [Order(5)]
        public PluginConfigColor Color = new(Vector4.UnitW);

        [DragInt("Thickness", min = 1, max = 100)]
        [Order(10)]
        public int Thickness = 1;

        public StatusEffectIconBorderConfig()
        {
        }

        public StatusEffectIconBorderConfig(PluginConfigColor color, int thickness)
        {
            Color = color;
            Thickness = thickness;
        }
    }

    internal class StatusEffectsListsDefaults
    {
        internal static LabelConfig DefaultDurationLabelConfig()
        {
            return new LabelConfig(Vector2.Zero, "", DrawAnchor.Center, DrawAnchor.Center);
        }

        internal static LabelConfig DefaultStacksLabelConfig()
        {
            var config = new LabelConfig(new Vector2(16, -11), "", DrawAnchor.Center, DrawAnchor.Center);
            config.Color = new(Vector4.UnitW);
            config.OutlineColor = new(Vector4.One);
            config.ShadowColor = new(Vector4.One);

            return config;
        }
    }

    public enum FilterType
    {
        Blacklist,
        Whitelist
    }

    [Exportable(false)]
    public class StatusEffectsBlacklistConfig : PluginConfigObject
    {
        [RadioSelector(typeof(FilterType))]
        [Order(5)]
        public FilterType FilterType;

        public SortedList<string, uint> List = new SortedList<string, uint>();

        [JsonIgnore] private string? _errorMessage = null;
        [JsonIgnore] private string? _importString = null;
        [JsonIgnore] private bool _clearingList = false;

        private string KeyName(Status status)
        {
            return status.Name + "[" + status.RowId.ToString() + "]";
        }

        public bool StatusAllowed(Status status)
        {
            var inList = List.Any(pair => pair.Key.EndsWith($"[{status.RowId}]"));
            if ((inList && FilterType == FilterType.Blacklist) || (!inList && FilterType == FilterType.Whitelist))
            {
                return false;
            }

            return true;
        }

        public bool AddNewEntry(Status? status)
        {
            if (status != null && !List.ContainsKey(KeyName(status)))
            {
                List.Add(KeyName(status), status.RowId);
                _input = "";

                return true;
            }

            return false;
        }

        private bool AddNewEntry(string input, ExcelSheet<Status>? sheet)
        {
            if (input.Length > 0 && sheet != null)
            {
                List<Status> statusToAdd = new List<Status>();

                // try id
                if (uint.TryParse(input, out uint uintValue))
                {
                    if (uintValue > 0)
                    {
                        Status? status = sheet.GetRow(uintValue);
                        if (status != null)
                        {
                            statusToAdd.Add(status);
                        }
                    }
                }

                // try name
                if (statusToAdd.Count == 0)
                {
                    var enumerator = sheet.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        Status item = enumerator.Current;
                        if (item.Name.ToString().ToLower() == input.ToLower())
                        {
                            statusToAdd.Add(item);
                        }
                    }
                }

                bool added = false;
                foreach (Status status in statusToAdd)
                {
                    added |= AddNewEntry(status);
                }
                return added;
            }

            return false;
        }

        private string ExportList()
        {
            string exportString = "";

            for (int i = 0; i < List.Keys.Count; i++)
            {
                exportString += List.Keys[i] + "|";
                exportString += List.Values[i] + "|";
            }

            return exportString;
        }

        private string? ImportList(string importString)
        {
            SortedList<string, uint> tmpList = new SortedList<string, uint>();

            try
            {
                string[] strings = importString.Trim().Split("|", StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < strings.Length; i += 2)
                {
                    if (i + 1 >= strings.Length)
                    {
                        break;
                    }

                    string key = strings[i];
                    uint value = uint.Parse(strings[i + 1]);

                    tmpList.Add(key, value);
                }
            }
            catch
            {
                return "Error importing list!";
            }

            List = tmpList;
            return null;
        }

        [JsonIgnore]
        private string _input = "";

        [ManualDraw]
        public bool Draw(ref bool changed)
        {
            if (!Enabled)
            {
                return false;
            }

            var flags =
                ImGuiTableFlags.RowBg |
                ImGuiTableFlags.Borders |
                ImGuiTableFlags.BordersOuter |
                ImGuiTableFlags.BordersInner |
                ImGuiTableFlags.ScrollY |
                ImGuiTableFlags.SizingFixedSame;

            var sheet = Plugin.DataManager.GetExcelSheet<Status>();
            var iconSize = new Vector2(30, 30);
            var indexToRemove = -1;

            if (ImGui.BeginChild("Filter Effects", new Vector2(0, 360), false, ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.Text("\u2002 \u2002");
                ImGui.SameLine();
                ImGui.Text("Type an ID or Name");

                ImGui.Text("\u2002 \u2002");
                ImGui.SameLine();
                ImGui.PushItemWidth(300);
                if (ImGui.InputText("", ref _input, 64, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    changed |= AddNewEntry(_input, sheet);
                    ImGui.SetKeyboardFocusHere(-1);
                }

                // add
                ImGui.SameLine();
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button(FontAwesomeIcon.Plus.ToIconString(), new Vector2(0, 0)))
                {
                    changed |= AddNewEntry(_input, sheet);
                    ImGui.SetKeyboardFocusHere(-2);
                }

                // export
                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 154);
                if (ImGui.Button(FontAwesomeIcon.Upload.ToIconString(), new Vector2(0, 0)))
                {
                    ImGui.SetClipboardText(ExportList());
                    ImGui.OpenPopup("export_succes_popup");
                }
                ImGui.PopFont();
                if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Export List to Clipboard"); }

                // export success popup
                if (ImGui.BeginPopup("export_succes_popup"))
                {
                    ImGui.Text("List exported to clipboard!");
                    ImGui.EndPopup();
                }

                // import
                ImGui.SameLine();
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button(FontAwesomeIcon.Download.ToIconString(), new Vector2(0, 0)))
                {
                    _importString = ImGui.GetClipboardText();
                }
                ImGui.PopFont();
                if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Import List from Clipboard"); }

                // clear
                ImGui.SameLine();
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button(FontAwesomeIcon.Trash.ToIconString(), new Vector2(0, 0)))
                {
                    _clearingList = true;
                }
                ImGui.PopFont();
                if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Clear List"); }

                ImGui.Text("\u2002 \u2002");
                ImGui.SameLine();

                if (ImGui.BeginTable("table", 4, flags, new Vector2(583, List.Count > 0 ? 200 : 40)))
                {
                    ImGui.TableSetupColumn("Icon", ImGuiTableColumnFlags.WidthFixed, 0, 0);
                    ImGui.TableSetupColumn("ID", ImGuiTableColumnFlags.WidthFixed, 0, 1);
                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 0, 2);
                    ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 0, 3);

                    ImGui.TableSetupScrollFreeze(0, 1);
                    ImGui.TableHeadersRow();

                    for (int i = 0; i < List.Count; i++)
                    {
                        var id = List.Values[i];
                        var name = List.Keys[i];
                        var row = sheet?.GetRow(id);

                        if (_input != "" && !name.ToUpper().Contains(_input.ToUpper()))
                        {
                            continue;
                        }

                        ImGui.PushID(i.ToString());
                        ImGui.TableNextRow(ImGuiTableRowFlags.None, iconSize.Y);

                        // icon
                        if (ImGui.TableSetColumnIndex(0))
                        {
                            if (row != null)
                            {
                                DrawHelper.DrawIcon<Status>(row, ImGui.GetCursorPos(), iconSize, false, true);
                            }
                        }

                        // id
                        if (ImGui.TableSetColumnIndex(1))
                        {
                            ImGui.Text(id.ToString());
                        }

                        // name
                        if (ImGui.TableSetColumnIndex(2))
                        {
                            var displayName = row != null ? row.Name : name;
                            ImGui.Text(displayName);
                        }

                        // remove
                        if (ImGui.TableSetColumnIndex(3))
                        {
                            ImGui.PushFont(UiBuilder.IconFont);
                            ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
                            ImGui.PushStyleColor(ImGuiCol.ButtonActive, Vector4.Zero);
                            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Vector4.Zero);
                            if (ImGui.Button(FontAwesomeIcon.Trash.ToIconString(), iconSize))
                            {
                                changed = true;
                                indexToRemove = i;
                            }
                            ImGui.PopFont();
                            ImGui.PopStyleColor(3);
                        }
                        ImGui.PopID();
                    }

                    ImGui.EndTable();
                }
                ImGui.Text("\u2002 \u2002");
                ImGui.SameLine();
                ImGui.Text("Tip: You can [Ctrl + Alt + Shift] + Left Click on a status effect to automatically add it to the list.");

            }

            if (indexToRemove >= 0)
            {
                List.RemoveAt(indexToRemove);
            }

            ImGui.EndChild();

            // error message
            if (_errorMessage != null)
            {
                if (ImGuiHelper.DrawErrorModal(_errorMessage))
                {
                    _errorMessage = null;
                }
            }

            // import confirmation
            if (_importString != null)
            {
                string[] message = new string[] {
                    "All the elements in the list will be replaced.",
                    "Are you sure you want to import?"
                };
                var (didConfirm, didClose) = ImGuiHelper.DrawConfirmationModal("Import?", message);

                if (didConfirm)
                {
                    _errorMessage = ImportList(_importString);
                    changed = true;
                }

                if (didConfirm || didClose)
                {
                    _importString = null;
                }
            }

            // clear confirmation
            if (_clearingList)
            {
                string message = "Are you sure you want to clear the list?";

                var (didConfirm, didClose) = ImGuiHelper.DrawConfirmationModal("Clear List?", message);

                if (didConfirm)
                {
                    List.Clear();
                    changed = true;
                }

                if (didConfirm || didClose)
                {
                    _clearingList = false;
                }
            }

            return false;
        }
    }


    public class StatusEffectsBlacklistConfigConverter : PluginConfigObjectConverter
    {
        public StatusEffectsBlacklistConfigConverter()
        {
            NewTypeFieldConverter<bool, FilterType> converter;
            converter = new NewTypeFieldConverter<bool, FilterType>("FilterType", FilterType.Blacklist, (oldValue) =>
            {
                return oldValue ? FilterType.Whitelist : FilterType.Blacklist;
            });

            FieldConvertersMap.Add("UseAsWhitelist", converter);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(StatusEffectsBlacklistConfig);
        }
    }

    [Section("Buffs and Debuffs")]
    [SubSection("Custom Effects", 0)]
    public class CustomEffectsListConfig : StatusEffectsListConfig
    {
        public new static CustomEffectsListConfig DefaultConfig()
        {
            var iconConfig = new StatusEffectIconConfig();
            iconConfig.DispellableBorderConfig.Enabled = false;
            iconConfig.Size = new Vector2(30, 30);

            var pos = new Vector2(0, HUDConstants.BaseHUDOffsetY);
            var size = new Vector2(250, iconConfig.Size.Y * 3 + 10);

            var config = new CustomEffectsListConfig(pos, size, true, true, false, GrowthDirections.Centered | GrowthDirections.Up, iconConfig);
            config.Enabled = false;
            config.Directions = 5;

            // pre-populated white list
            config.BlacklistConfig.FilterType = FilterType.Whitelist;

            ExcelSheet<Status>? sheet = Plugin.DataManager.GetExcelSheet<Status>();
            if (sheet != null)
            {
                // Left Eye
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1184));
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1454));

                // Battle Litany
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(786));
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1414));

                // Brotherhood
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1185));
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(2174));

                // Battle Voice
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(141));

                // Devilment
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1825));

                // Technical Finish
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1822));
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(2050));

                // Standard Finish
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1821));
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(2024));
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(2105));
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(2113));

                // Embolden
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1239));
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1297));
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(2282));

                // Devotion
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1213));

                // ------ AST Card Buffs -------
                // The Balance
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(829));
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1338));
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1882));

                // The Bole
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(830));
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1339));
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1883));

                // The Arrow
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(831));
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1884));

                // The Spear
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(832));
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1885));

                // The Ewer
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(833));
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1340));
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1886));

                // The Spire
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(834));
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1341));
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1887));

                // Lord of Crowns
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1451));
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1876));

                // Lady of Crowns
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1452));
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1877));

                // Divination
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1878));
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(2034));

                // Chain Stratagem
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1221));
                config.BlacklistConfig.AddNewEntry(sheet.GetRow(1406));
            }

            return config;
        }

        public CustomEffectsListConfig(Vector2 position, Vector2 size, bool showBuffs, bool showDebuffs, bool showPermanentEffects,
            GrowthDirections growthDirections, StatusEffectIconConfig iconConfig)
            : base(position, size, showBuffs, showDebuffs, showPermanentEffects, growthDirections, iconConfig)
        {
        }
    }
}