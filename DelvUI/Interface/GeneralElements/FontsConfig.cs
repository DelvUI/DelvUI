using Dalamud.Interface;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using ImGuiNET;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;

namespace DelvUI.Interface.GeneralElements
{
    public struct FontData
    {
        public string Name;
        public int Size;

        public FontData(string name, int size)
        {
            Name = name;
            Size = size;
        }
    }

    [Disableable(false)]
    [Section("Misc")]
    [SubSection("Fonts", 0)]
    public class FontsConfig : PluginConfigObject
    {
        public new static FontsConfig DefaultConfig() { return new FontsConfig(); }

        public SortedList<string, FontData> Fonts = new SortedList<string, FontData>();
        public bool SupportChineseCharacters = false;
        public bool SupportKoreanCharacters = false;

        [JsonIgnore] public readonly string DefaultFontKey = "big-noodle-too_24";
        [JsonIgnore] private int _inputFont = 0;
        [JsonIgnore] private int _inputSize = 23;

        [JsonIgnore] private string[] _fonts;
        [JsonIgnore] private string[] _sizes;

        public FontsConfig()
        {
            ReloadFonts();

            // default font
            if (!Fonts.ContainsKey(DefaultFontKey))
            {
                var defaultFont = new FontData("big-noodle-too", 24);
                Fonts.Add(DefaultFontKey, defaultFont);
            }

            // sizes
            _sizes = new string[40];
            for (int i = 0; i < _sizes.Length; i++)
            {
                _sizes[i] = (i + 1).ToString();
            }
        }

        private void ReloadFonts()
        {
            var fontsPath = FontsManager.Instance.FontsPath;
            var fonts = Directory.GetFiles(fontsPath, "*.ttf");

            _fonts = new string[fonts.Length];
            for (int i = 0; i < fonts.Length; i++)
            {
                _fonts[i] = fonts[i].Replace(fontsPath, "").Replace(".ttf", "");
            }
        }

        private bool AddNewEntry(int font, int size)
        {
            if (font < 0 || font > _fonts.Length)
            {
                return false;
            }

            if (size <= 0 || size > _sizes.Length)
            {
                return false;
            }

            var fontName = _fonts[font];
            var key = fontName + "_" + size.ToString();

            if (Fonts.ContainsKey(key))
            {
                return false;
            }

            var fontData = new FontData(fontName, size);
            Fonts.Add(key, fontData);

            Plugin.UiBuilder.RebuildFonts();

            return true;
        }

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
                ImGuiTableFlags.SizingFixedSame;

            var changed = false;
            var indexToRemove = -1;

            if (ImGui.BeginChild("Fonts", new Vector2(400, 400), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (_fonts.Length == 0)
                {
                    ImGui.Text("\u2002");
                    ImGui.SameLine();
                    ImGui.Text("Please add a font file in the Media/Fonts directory of DelvUI");
                    return false;
                }

                ImGui.NewLine();
                ImGui.Text("\u2002");
                ImGui.SameLine();
                ImGui.Text("Select a font and size to add:");

                ImGui.Text("\u2002");
                ImGui.SameLine();
                ImGui.Combo("Font ##font", ref _inputFont, _fonts, _fonts.Length, 10);

                ImGui.SameLine();
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button("\uf2f9", new Vector2(0, 0)))
                {
                    ReloadFonts();
                }
                ImGui.PopFont();

                ImGui.Text("\u2002");
                ImGui.SameLine();
                ImGui.Combo("Size  ##size", ref _inputSize, _sizes, _sizes.Length, 10);

                ImGui.SameLine();
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button(FontAwesomeIcon.Plus.ToIconString(), new Vector2(0, 0)))
                {
                    changed |= AddNewEntry(_inputFont, _inputSize + 1);
                }
                ImGui.PopFont();

                ImGui.NewLine();
                ImGui.Text("\u2002");
                ImGui.SameLine();
                if (ImGui.BeginTable("table", 3, flags, new Vector2(326, 150)))
                {
                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 0, 0);
                    ImGui.TableSetupColumn("Size", ImGuiTableColumnFlags.WidthFixed, 0, 1);
                    ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 0, 2);

                    ImGui.TableSetupScrollFreeze(0, 1);
                    ImGui.TableHeadersRow();

                    for (int i = 0; i < Fonts.Count; i++)
                    {
                        var key = Fonts.Keys[i];
                        var fontData = Fonts.Values[i];

                        ImGui.PushID(i.ToString());
                        ImGui.TableNextRow(ImGuiTableRowFlags.None);

                        // icon
                        if (ImGui.TableSetColumnIndex(0))
                        {
                            ImGui.Text(fontData.Name);
                        }

                        // id
                        if (ImGui.TableSetColumnIndex(1))
                        {
                            ImGui.Text(fontData.Size.ToString());
                        }

                        // remove
                        if (key != DefaultFontKey && ImGui.TableSetColumnIndex(2))
                        {
                            ImGui.PushFont(UiBuilder.IconFont);
                            ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
                            ImGui.PushStyleColor(ImGuiCol.ButtonActive, Vector4.Zero);
                            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Vector4.Zero);

                            if (ImGui.Button(FontAwesomeIcon.Trash.ToIconString()))
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

                ImGui.NewLine();
                ImGui.NewLine();
                ImGui.Text("\u2002");
                ImGui.SameLine();
                if (ImGui.Checkbox("Support Chinese", ref SupportChineseCharacters))
                {
                    changed = true;
                    Plugin.UiBuilder.RebuildFonts();
                }

                ImGui.SameLine();
                if (ImGui.Checkbox("Support Korean", ref SupportKoreanCharacters))
                {
                    changed = true;
                    Plugin.UiBuilder.RebuildFonts();
                }
            }

            if (indexToRemove >= 0)
            {
                Fonts.RemoveAt(indexToRemove);
                Plugin.UiBuilder.RebuildFonts();
            }

            ImGui.EndChild();

            return changed;
        }
    }
}
