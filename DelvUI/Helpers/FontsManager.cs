using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;

namespace DelvUI.Helpers
{
    public class FontsManager
    {
        #region Singleton
        private FontsManager(string basePath)
        {
            FontsPath = Path.GetDirectoryName(basePath) + "\\Media\\Fonts\\";
        }

        public static void Initialize(string basePath)
        {
            Instance = new FontsManager(basePath);
        }

        public static FontsManager Instance { get; private set; }

        #endregion

        public readonly string FontsPath;

        public bool DefaultFontBuilt { get; private set; }
        public ImFontPtr DefaultFont { get; private set; } = null;

        private List<ImFontPtr> _fonts = new List<ImFontPtr>();
        public IReadOnlyCollection<ImFontPtr> Fonts => _fonts.AsReadOnly();

        public bool PushDefaultFont()
        {
            if (DefaultFontBuilt)
            {
                ImGui.PushFont(DefaultFont);
                return true;
            }

            return false;
        }

        public bool PushFont(int index)
        {
            if (index < 0 || index >= _fonts.Count)
            {
                return false;
            }

            ImGui.PushFont(_fonts[index]);
            return true;
        }

        public unsafe void BuildFonts()
        {
            _fonts.Clear();
            DefaultFontBuilt = false;

            var config = ConfigurationManager.GetInstance().GetConfigObject<FontsConfig>();
            ImGuiIOPtr io = ImGui.GetIO();
            var ranges = GetCharacterRanges(config, io);

            foreach (var fontData in config.Fonts)
            {
                var path = FontsPath + fontData.Value.Name + ".ttf";
                if (!File.Exists(path))
                {
                    continue;
                }

                try
                {
                    ImFontPtr font = ranges == null ? io.Fonts.AddFontFromFileTTF(path, fontData.Value.Size)
                        : io.Fonts.AddFontFromFileTTF(path, fontData.Value.Size, null, ranges.Value.Data);
                    _fonts.Add(font);

                    if (fontData.Key == config.DefaultFontKey)
                    {
                        DefaultFont = font;
                        DefaultFontBuilt = true;
                    }
                }
                catch (Exception ex)
                {
                    PluginLog.Log($"Font failed to load: {path}");
                    PluginLog.Log(ex.ToString());
                }
            }
        }

        private unsafe ImVector? GetCharacterRanges(FontsConfig config, ImGuiIOPtr io)
        {
            if (!config.SupportChineseCharacters && !config.SupportKoreanCharacters)
            {
                return null;
            }

            var builder = new ImFontGlyphRangesBuilderPtr(ImGuiNative.ImFontGlyphRangesBuilder_ImFontGlyphRangesBuilder());

            if (config.SupportChineseCharacters)
            {
                // GetGlyphRangesChineseFull() includes Default + Hiragana, Katakana, Half-Width, Selection of 1946 Ideographs
                // https://skia.googlesource.com/external/github.com/ocornut/imgui/+/v1.53/extra_fonts/README.txt
                builder.AddRanges(io.Fonts.GetGlyphRangesChineseFull());
            }

            if (config.SupportKoreanCharacters)
            {
                builder.AddRanges(io.Fonts.GetGlyphRangesKorean());
            }

            builder.BuildRanges(out ImVector ranges);

            return ranges;
        }
    }
}
