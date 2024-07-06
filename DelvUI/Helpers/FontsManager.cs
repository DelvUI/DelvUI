using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using DelvUI.Config;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DelvUI.Helpers
{
    public class FontScope : IDisposable
    {
        private readonly IFontHandle? _handle;

        public FontScope(IFontHandle? handle)
        {
            _handle = handle;
            _handle?.Push();
        }

        public void Dispose()
        {
            _handle?.Pop();
            GC.SuppressFinalize(this);
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
        public IFontHandle? DefaultFont { get; private set; } = null!;

        private List<IFontHandle> _fonts = new List<IFontHandle>();
        public IReadOnlyCollection<IFontHandle> Fonts => _fonts.AsReadOnly();

        public FontScope PushDefaultFont()
        {
            if (DefaultFontBuilt && DefaultFont != null)
            {
                return new FontScope(DefaultFont);
            }

            return new FontScope(null);
        }

        public FontScope PushFont(string? fontId)
        {
            if (fontId == null || _config == null || !_config.Fonts.ContainsKey(fontId))
            {
                return new FontScope(null);
            }

            var index = _config.Fonts.IndexOfKey(fontId);
            if (index < 0 || index >= _fonts.Count)
            {
                return new FontScope(null);
            }

            return new FontScope(_fonts[index]);
        }

        public void ClearFonts()
        {
            foreach (IFontHandle font in _fonts)
            {
                font.Dispose();
            }

            _fonts.Clear();
        }

        public unsafe void BuildFonts()
        {
            ClearFonts();
            DefaultFontBuilt = false;

            FontsConfig config = ConfigurationManager.Instance.GetConfigObject<FontsConfig>();
            ImGuiIOPtr io = ImGui.GetIO();
            ushort[]? ranges = GetCharacterRanges(config, io);

            foreach (KeyValuePair<string, FontData> fontData in config.Fonts)
            {
                bool isGameFont = config.GameFontMap.ContainsValue(fontData.Value.Name);
                string path = DefaultFontsPath + fontData.Value.Name + ".ttf";

                if (!File.Exists(path))
                {
                    path = config.ValidatedFontsPath + fontData.Value.Name + ".ttf";

                    if (!File.Exists(path) && !isGameFont)
                    {
                        continue;
                    }
                }

                try
                {
                    IFontHandle font;

                    if (isGameFont)
                    {
                        GameFontFamily fontFamily = (GameFontFamily)Enum.Parse(
                            typeof(GameFontFamily),
                            config.GameFontMap.FirstOrDefault(x => x.Value == fontData.Value.Name).Key
                        );
                        GameFontStyle style = new GameFontStyle(fontFamily, fontData.Value.Size);

                        font = Plugin.UiBuilder.FontAtlas.NewGameFontHandle(style);
                    }
                    else
                    {
                        font = Plugin.UiBuilder.FontAtlas.NewDelegateFontHandle
                        (
                            e => e.OnPreBuild
                            (
                                tk => tk.AddFontFromFile
                                (
                                    path,
                                    new SafeFontConfig
                                    {
                                        SizePx = fontData.Value.Size,
                                        GlyphRanges = ranges
                                    }
                                )
                            )
                        );
                    }

                    _fonts.Add(font);

                    // save default font
                    if (fontData.Key == FontsConfig.DefaultBigFontKey)
                    {
                        DefaultFont = font;
                        DefaultFontBuilt = true;
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Logger.Error($"Error loading font from path {path}:\n{ex.Message}");
                }

            }
        }

        private unsafe ushort[]? GetCharacterRanges(FontsConfig config, ImGuiIOPtr io)
        {
            if (!config.SupportChineseCharacters && !config.SupportKoreanCharacters && !config.SupportCyrillicCharacters)
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

            if (config.SupportCyrillicCharacters)
            {
                builder.AddRanges(io.Fonts.GetGlyphRangesCyrillic());
            }

            return builder.BuildRangesToArray();
        }
    }
}
