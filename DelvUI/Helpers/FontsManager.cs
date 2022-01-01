using System;
using System.Collections.Generic;
using System.IO;
using Dalamud.Logging;
using DelvUI.Config;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;

namespace DelvUI.Helpers
{
    public class FontScope : IDisposable
    {
        private bool _fontPushed;

        public FontScope(bool fontPushed)
        {
            _fontPushed = fontPushed;
        }

        public void Dispose()
        {
            if (_fontPushed)
            {
                ImGui.PopFont();
            }
        }
    }

    public class FontsManager : IDisposable
    {
        #region Singleton
        private FontsManager(string basePath)
        {
            DefaultFontsPath = Path.GetDirectoryName(basePath) + "\\Media\\Fonts\\";
        }

        public static void Initialize(string basePath)
        {
            Instance = new FontsManager(basePath);
        }

        public static FontsManager Instance { get; private set; } = null!;
        private FontsConfig? _config;

        public void LoadConfig()
        {
            if (_config != null)
            {
                return;
            }

            _config = ConfigurationManager.Instance.GetConfigObject<FontsConfig>();
            ConfigurationManager.Instance.ResetEvent += OnConfigReset;
        }

        private void OnConfigReset(ConfigurationManager sender)
        {
            _config = sender.GetConfigObject<FontsConfig>();
        }

        ~FontsManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            ConfigurationManager.Instance.ResetEvent -= OnConfigReset;
            Instance = null!;
        }
        #endregion

        public readonly string DefaultFontsPath;

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

        public FontScope PushFont(string? fontId)
        {
            if (fontId == null || _config == null || !_config.Fonts.ContainsKey(fontId))
            {
                return new FontScope(false);
            }

            var index = _config.Fonts.IndexOfKey(fontId);
            if (index < 0 || index >= _fonts.Count)
            {
                return new FontScope(false);
            }

            ImGui.PushFont(_fonts[index]);
            return new FontScope(true);
        }

        public unsafe void BuildFonts()
        {
            _fonts.Clear();
            DefaultFontBuilt = false;

            var config = ConfigurationManager.Instance.GetConfigObject<FontsConfig>();
            ImGuiIOPtr io = ImGui.GetIO();
            var ranges = GetCharacterRanges(config, io);

            foreach (var fontData in config.Fonts)
            {
                var path = DefaultFontsPath + fontData.Value.Name + ".ttf";
                if (!File.Exists(path))
                {
                    path = config.ValidatedFontsPath + fontData.Value.Name + ".ttf";
                    if (!File.Exists(path))
                    {
                        continue;
                    }
                }

                try
                {
                    ImFontPtr font = ranges == null ? io.Fonts.AddFontFromFileTTF(path, fontData.Value.Size)
                        : io.Fonts.AddFontFromFileTTF(path, fontData.Value.Size, null, ranges.Value.Data);
                    _fonts.Add(font);

                    if (fontData.Key == FontsConfig.DefaultBigFontKey)
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
