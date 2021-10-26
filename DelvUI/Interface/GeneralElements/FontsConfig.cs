using Dalamud.Interface;
using Dalamud.Interface.ImGuiFileDialog;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

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

        public string FontsPath = "C:\\";
        [JsonIgnore] public string ValidatedFontsPath => ValidatePath(FontsPath);

        public SortedList<string, FontData> Fonts = new SortedList<string, FontData>();
        public bool SupportChineseCharacters = false;
        public bool SupportKoreanCharacters = false;

        [JsonIgnore] public static readonly List<string> DefaultFontsKeys = new List<string>() { "big-noodle-too_24", "big-noodle-too_20", "big-noodle-too_16" };
        [JsonIgnore] public static string DefaultBigFontKey => DefaultFontsKeys[0];
        [JsonIgnore] public static string DefaultMediumFontKey => DefaultFontsKeys[1];
        [JsonIgnore] public static string DefaultSmallFontKey => DefaultFontsKeys[2];

        [JsonIgnore] private int _inputFont = 0;
        [JsonIgnore] private int _inputSize = 23;

        [JsonIgnore] private string[] _fonts = null!;
        [JsonIgnore] private string[] _sizes = null!;

        [JsonIgnore] private FileDialogManager _fileDialogManager = new FileDialogManager();

        public FontsConfig()
        {
            ReloadFonts();

            // default fonts
            foreach (string key in DefaultFontsKeys)
            {
                if (!Fonts.ContainsKey(key))
                {
                    string[] str = key.Split("_", StringSplitOptions.RemoveEmptyEntries);
                    var defaultFont = new FontData(str[0], int.Parse(str[1]));
                    Fonts.Add(key, defaultFont);
                }

            }

            // sizes
            _sizes = new string[40];
            for (int i = 0; i < _sizes.Length; i++)
            {
                _sizes[i] = (i + 1).ToString();
            }
        }

        private bool IsDefaultFont(string key)
        {
            return DefaultFontsKeys.Contains(key);
        }

        private string ValidatePath(string path)
        {
            if (path.EndsWith("\\") || path.EndsWith("/"))
            {
                return path;
            }

            return path + "\\";
        }

        private string[] FontsFromPath(string path)
        {
            string[] fonts;
            try
            {
                fonts = Directory.GetFiles(path, "*.ttf");
            }
            catch
            {
                fonts = new string[0];
            }

            for (int i = 0; i < fonts.Length; i++)
            {
                fonts[i] = fonts[i]
                    .Replace(path, "")
                    .Replace(".ttf", "")
                    .Replace(".TTF", "");
            }

            return fonts;
        }

        private void ReloadFonts()
        {
            var defaultFontsPath = ValidatePath(FontsManager.Instance.DefaultFontsPath);
            string[] defaultFonts = FontsFromPath(defaultFontsPath);
            string[] userFonts = FontsFromPath(ValidatedFontsPath);

            _fonts = new string[defaultFonts.Length + userFonts.Length];
            defaultFonts.CopyTo(_fonts, 0);
            userFonts.CopyTo(_fonts, defaultFonts.Length);
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

        private void SelectFolder()
        {
            Action<bool, string> callback = (finished, path) =>
            {
                if (finished && path.Length > 0)
                {
                    FontsPath = path;
                    ReloadFonts();
                }
            };

            _fileDialogManager.OpenFolderDialog("Select Fonts Folder", callback);
        }

        [ManualDraw]
        public bool Draw(ref bool changed)
        {
            var flags =
                ImGuiTableFlags.RowBg |
                ImGuiTableFlags.Borders |
                ImGuiTableFlags.BordersOuter |
                ImGuiTableFlags.BordersInner |
                ImGuiTableFlags.ScrollY |
                ImGuiTableFlags.SizingFixedSame;

            var indexToRemove = -1;

            if (ImGui.BeginChild("Fonts", new Vector2(400, 400), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (_fonts.Length == 0)
                {
                    ImGuiHelper.Tab();
                    ImGui.Text("Default font not found in \"%appdata%/Roaming/XIVLauncher/InstalledPlugins/DelvUI/Media/Fonts/big-noodle-too.ttf\"");
                    return false;
                }

                ImGuiHelper.NewLineAndTab();
                if (ImGui.InputText("Path", ref FontsPath, 200, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    changed = true;
                    ReloadFonts();
                }

                ImGui.SameLine();
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button(FontAwesomeIcon.Folder.ToIconString(), new Vector2(0, 0)))
                {
                    SelectFolder();
                }
                ImGui.PopFont();

                ImGuiHelper.Tab();
                ImGui.Combo("Font ##font", ref _inputFont, _fonts, _fonts.Length, 10);

                ImGui.SameLine();
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button("\uf2f9", new Vector2(0, 0)))
                {
                    ReloadFonts();
                }
                ImGui.PopFont();

                ImGuiHelper.Tab();
                ImGui.Combo("Size  ##size", ref _inputSize, _sizes, _sizes.Length, 10);

                ImGui.SameLine();
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button(FontAwesomeIcon.Plus.ToIconString(), new Vector2(0, 0)))
                {
                    changed |= AddNewEntry(_inputFont, _inputSize + 1);
                }
                ImGui.PopFont();

                ImGuiHelper.NewLineAndTab();
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
                        if (!IsDefaultFont(key) && ImGui.TableSetColumnIndex(2))
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
                ImGuiHelper.NewLineAndTab();
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

            _fileDialogManager.Draw();

            return false;
        }
    }
}
