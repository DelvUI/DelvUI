using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Numerics;

namespace DelvUI.Interface.StatusEffects
{
    [Section("Buffs and Debuffs")]
    [SubSection("Player Buffs", 0)]
    public class PlayerBuffsListConfig : StatusEffectsListConfig
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
    public class PlayerDebuffsListConfig : StatusEffectsListConfig
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
    public class TargetBuffsListConfig : StatusEffectsListConfig
    {
        public new static TargetBuffsListConfig DefaultConfig()
        {
            var pos = new Vector2(HUDConstants.UnitFramesOffsetX, HUDConstants.BaseHUDOffsetY - 50);
            var iconConfig = new StatusEffectIconConfig();
            iconConfig.DispellableBorderConfig.Enabled = false;

            return new TargetBuffsListConfig(pos, HUDConstants.DefaultStatusEffectsListSize, true, false, true, GrowthDirections.Right | GrowthDirections.Up, iconConfig);
        }

        public TargetBuffsListConfig(Vector2 position, Vector2 size, bool showBuffs, bool showDebuffs, bool showPermanentEffects,
            GrowthDirections growthDirections, StatusEffectIconConfig iconConfig)
            : base(position, size, showBuffs, showDebuffs, showPermanentEffects, growthDirections, iconConfig)
        {
        }
    }

    [Section("Buffs and Debuffs")]
    [SubSection("Target Debuffs", 0)]
    public class TargetDebuffsListConfig : StatusEffectsListConfig
    {
        public new static TargetDebuffsListConfig DefaultConfig()
        {
            var pos = new Vector2(HUDConstants.UnitFramesOffsetX, HUDConstants.BaseHUDOffsetY - HUDConstants.DefaultStatusEffectsListSize.Y - 50);
            var iconConfig = new StatusEffectIconConfig();
            iconConfig.DispellableBorderConfig.Enabled = false;

            return new TargetDebuffsListConfig(pos, HUDConstants.DefaultStatusEffectsListSize, false, true, true, GrowthDirections.Right | GrowthDirections.Up, iconConfig);
        }

        public TargetDebuffsListConfig(Vector2 position, Vector2 size, bool showBuffs, bool showDebuffs, bool showPermanentEffects,
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
        [Order(16)]
        public Vector2 IconPadding = new(2, 2);

        [Checkbox("Preview")]
        [Order(20)]
        public bool ShowArea;

        [Checkbox("Fill Rows First", separator = true)]
        [Order(25)]
        public bool FillRowsFirst = true;

        [Combo("Icons Growth Direction",
            "Right and Down",
            "Right and Up",
            "Left and Down",
            "Left and Up"
        )]
        //"Centered (horizontal)",    not working as expected
        //"Centered (vertical)"       not working as expected
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

        [Checkbox("Tooltips")]
        [Order(55)]
        public bool ShowTooltips = true;

        [NestedConfig("Icons", 60)]
        public StatusEffectIconConfig IconConfig;

        [NestedConfig("Black List", 65)]
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
            GrowthDirections.Out | GrowthDirections.Right,
            GrowthDirections.Out | GrowthDirections.Down
        };
    }

    [Portable(false)]
    [Disableable(false)]
    public class StatusEffectIconConfig : PluginConfigObject
    {
        [DragInt2("Icon Size", min = 1, max = 1000)]
        [Order(0)]
        public Vector2 Size = new(40, 40);

        [NestedConfig("Duration", 5)]
        public LabelConfig DurationLabelConfig;

        [NestedConfig("Stacks", 10)]
        public LabelConfig StacksLabelConfig;

        [NestedConfig("Border", 15)]
        public StatusEffectIconBorderConfig BorderConfig = new StatusEffectIconBorderConfig();

        [NestedConfig("Dispellable Effects Border", 20)]
        public StatusEffectIconBorderConfig DispellableBorderConfig = new StatusEffectIconBorderConfig(new(new(141f / 255f, 206f / 255f, 229f / 255f, 100f / 100f)), 2);

        [NestedConfig("My Effects Border", 25)]
        public StatusEffectIconBorderConfig OwnedBorderConfig = new StatusEffectIconBorderConfig(new(new(35f / 255f, 179f / 255f, 69f / 255f, 100f / 100f)), 2);

        public StatusEffectIconConfig(LabelConfig durationLabelConfig = null, LabelConfig stacksLabelConfig = null)
        {
            DurationLabelConfig = durationLabelConfig ?? StatusEffectsListsDefaults.DefaultDurationLabelConfig();
            StacksLabelConfig = stacksLabelConfig ?? StatusEffectsListsDefaults.DefaultStacksLabelConfig();
        }
    }

    [Portable(false)]
    public class StatusEffectIconBorderConfig : PluginConfigObject
    {
        [ColorEdit4("Color")]
        [Order(0)]
        public PluginConfigColor Color = new(Vector4.UnitW);

        [DragInt("Thickness", min = 1, max = 100)]
        [Order(5)]
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
            return new LabelConfig(Vector2.Zero, "", LabelTextAnchor.Center, LabelTextAnchor.Center);
        }

        internal static LabelConfig DefaultStacksLabelConfig()
        {
            var config = new LabelConfig(new Vector2(16, -11), "", LabelTextAnchor.Center, LabelTextAnchor.Center);
            config.Color = new(Vector4.UnitW);
            config.OutlineColor = new(Vector4.One);

            return config;
        }
    }

    [Portable(false)]
    public class StatusEffectsBlacklistConfig : PluginConfigObject
    {
        public bool UseAsWhitelist = false;
        public SortedList<uint, string> List = new SortedList<uint, string>();

        public bool StatusEffectIDAllowed(uint id)
        {
            var inList = List.ContainsKey((uint)id);
            if ((inList && !UseAsWhitelist) || (!inList && UseAsWhitelist))
            {
                return false;
            }

            return true;
        }

        public bool AddNewEntry(Status status)
        {
            if (status != null && !List.ContainsKey(status.RowId))
            {
                List.Add(status.RowId, status.Name);
                _input = "";

                return true;
            }

            return false;
        }

        private bool AddNewEntry(string input, ExcelSheet<Status> sheet)
        {
            if (input.Length > 0)
            {
                Status status = null;

                // try id
                if (uint.TryParse(input, out uint uintValue))
                {
                    if (uintValue > 0)
                    {
                        status = sheet.GetRow(uintValue);
                    }
                }

                // try name
                if (status == null)
                {
                    var enumerator = sheet.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        Status item = enumerator.Current;
                        if (item.Name.ToString().ToLower() == input.ToLower())
                        {
                            status = item;
                            break;
                        }
                    }
                }

                return AddNewEntry(status);
            }

            return false;
        }

        [JsonIgnore]
        private string _input = "";

        [ManualDraw]
        public bool Draw()
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
                ImGuiTableFlags.SizingFixedFit;

            var changed = false;
            var sheet = Plugin.DataManager.GetExcelSheet<Status>();
            var iconSize = new Vector2(30, 30);
            var indexToRemove = -1;

            if (ImGui.BeginChild("blacklist", new Vector2(0, 360), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                changed |= ImGui.Checkbox("Use as White List", ref UseAsWhitelist);

                ImGui.Text("");
                ImGui.Text("Tip: You can [Ctrl + Alt + Shift] + Left Click on a status effect to automatically add it to the list.");
                ImGui.Text("");

                ImGui.Text("Type an ID or Name");

                if (ImGui.InputText("", ref _input, 64, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    changed |= AddNewEntry(_input, sheet);
                    ImGui.SetKeyboardFocusHere(-1);
                }

                ImGui.SameLine();

                if (ImGui.Button("Add", new Vector2(60, 23)))
                {
                    changed |= AddNewEntry(_input, sheet);
                    ImGui.SetKeyboardFocusHere(-2);
                }

                ImGui.NewLine();

                if (ImGui.BeginTable("table", 4, flags, new Vector2(620, 200)))
                {
                    ImGui.TableSetupColumn("Remove", ImGuiTableColumnFlags.WidthFixed, 0, 0);
                    ImGui.TableSetupColumn("Icon", ImGuiTableColumnFlags.WidthFixed, 0, 1);
                    ImGui.TableSetupColumn("ID", ImGuiTableColumnFlags.WidthFixed, 0, 2);
                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed, 0, 3);
                    ImGui.TableSetupScrollFreeze(0, 1);
                    ImGui.TableHeadersRow();

                    for (int i = 0; i < List.Count; i++)
                    {
                        var id = List.Keys[i];
                        var name = List.Values[i];

                        ImGui.PushID(i.ToString());
                        ImGui.TableNextRow(ImGuiTableRowFlags.None, iconSize.Y);

                        // remove
                        if (ImGui.TableSetColumnIndex(0))
                        {
                            var cursorPos = ImGui.GetCursorPos();
                            cursorPos.X += 8;
                            ImGui.SetCursorPos(cursorPos);

                            if (ImGui.Button("X", iconSize))
                            {
                                changed = true;
                                indexToRemove = i;
                            }
                        }

                        // icon
                        if (ImGui.TableSetColumnIndex(1))
                        {
                            var row = sheet.GetRow(id);
                            if (row != null)
                            {
                                DrawHelper.DrawIcon<Status>(row, ImGui.GetCursorPos(), iconSize, false);
                            }
                        }

                        // id
                        if (ImGui.TableSetColumnIndex(2))
                        {
                            ImGui.Text(id.ToString());
                        }

                        // name
                        if (ImGui.TableSetColumnIndex(3))
                        {
                            ImGui.Text(name);
                        }

                        ImGui.PopID();
                    }

                    ImGui.EndTable();
                }
            }

            if (indexToRemove >= 0)
            {
                List.RemoveAt(indexToRemove);
            }

            ImGui.EndChild();

            return changed;
        }
    }
}

// SAVING THESE FOR LATER
/*
 
        protected uint[] _raidWideBuffs =
        {
            // See https://external-preview.redd.it/bKacLk4PKav7vdP1ilT66gAtB1t7BTJjxsMrImRHr1k.png?auto=webp&s=cbe6880c34b45e2db20c247c8ab9eef543538e96
            // Left Eye
            1184, 1454,
            // Battle Litany
            786, 1414,
            // Brotherhood
            1185, 2174,
            // Battle Voice
            141,
            // Devilment
            1825,
            // Technical Finish
            1822, 2050,
            // Standard Finish
            1821, 2024, 2105, 2113,
            // Embolden
            1239, 1297, 2282,
            // Devotion
            1213,
            // ------ AST Card Buffs -------
            // The Balance
            829, 1338, 1882,
            // The Bole
            830, 1339, 1883,
            // The Arrow
            831, 1884,
            // The Spear
            832, 1885,
            // The Ewer
            833, 1340, 1886,
            // The Spire
            834, 1341, 1887,
            // Lord of Crowns
            1451, 1876,
            // Lady of Crowns
            1452, 1877,
            // Divination
            1878, 2034,
            // Chain Stratagem
            1221, 1406
        };

*/