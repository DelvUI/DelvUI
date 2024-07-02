using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Dalamud.Interface;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Logging;
using DelvUI.Config;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using ImGuiScene;

namespace DelvUI.Helpers
{
    public struct BarTextureData
    {
        public string Name;
        public string Path;
        public bool IsCustom;

        public BarTextureData(string name, string path, bool isCustom)
        {
            Name = name;
            Path = path;
            IsCustom = isCustom;
        }
    }

    public class BarTexturesManager : IDisposable
    {
        #region Singleton
        private BarTexturesManager(string basePath)
        {
            DefaultBarTexturesPath = Path.GetDirectoryName(basePath) + "\\Media\\Images\\textures\\";
        }

        public static void Initialize(string basePath)
        {
            Instance = new BarTexturesManager(basePath);
        }

        public static BarTexturesManager Instance { get; private set; } = null!;
        private BarTexturesConfig? _config;

        public void LoadConfig()
        {
            if (_config != null)
            {
                return;
            }

            _config = ConfigurationManager.Instance.GetConfigObject<BarTexturesConfig>();
            ConfigurationManager.Instance.ResetEvent += OnConfigReset;

            ReloadTextures();
        }

        private void OnConfigReset(ConfigurationManager sender)
        {
            _config = sender.GetConfigObject<BarTexturesConfig>();
        }

        ~BarTexturesManager()
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

        public readonly string DefaultBarTexturesPath;
        public static readonly string DefaultBarTextureName = "Default";

        public bool DefaultFontBuilt { get; private set; }
        public ImFontPtr DefaultFont { get; private set; } = null;

        private List<BarTextureData> _textures = new List<BarTextureData>();
        public IReadOnlyCollection<BarTextureData> BarTextures => _textures.AsReadOnly();

        private List<string> _textureNames = new List<string>();
        public IReadOnlyCollection<string> BarTextureNames => _textureNames.AsReadOnly();

        private Dictionary<string, ISharedImmediateTexture> _cache = new();

        public IDalamudTextureWrap? GetBarTexture(string? name)
        {
            if (name == null || name == DefaultBarTextureName) { return null; }

            // get cached texture
            if (_cache.TryGetValue(name, out ISharedImmediateTexture? cachedTexture) && cachedTexture != null)
            {
                return cachedTexture.GetWrapOrDefault();
            }

            // lazy load
            BarTextureData? data = _textures.FirstOrDefault(o => o.Name == name);
            if (!data.HasValue) { return null; }

            if (File.Exists(data.Value.Path))
            {
                try
                {
                    ISharedImmediateTexture? texture = Plugin.TextureProvider.GetFromFile(data.Value.Path);
                    if (texture != null)
                    {
                        _cache.Add(name, texture);
                    }

                    return texture?.GetWrapOrDefault();
                }
                catch
                (Exception ex)
                {
                    Plugin.Logger.Warning($"Image failed to load. {data.Value.Path}: " + ex.Message);
                }
            }

            return null;
        }

        public void ReloadTextures()
        {
            _textures.Clear();

            // embedded textures 
            _textures.AddRange(TexturesFromPath(DefaultBarTexturesPath, true));

            // custom textures
            if (_config != null)
            {
                _textures.AddRange(TexturesFromPath(_config.ValidatedBarTexturesPath, true));
            }

            // sort by name
            _textures = _textures.OrderBy(o => o.Name).ToList();

            // default always first
            _textures.Insert(0, new BarTextureData(DefaultBarTextureName, "", false));

            _textureNames = _textures.Select(o => o.Name).ToList();
        }

        private List<BarTextureData> TexturesFromPath(string path, bool isCustom)
        {
            string[] textures;
            try
            {
                string[] allowedExtensions = new string[] { ".png", ".tga" };
                textures = Directory
                    .GetFiles(path)
                    .Where(file => allowedExtensions.Any(file.ToLower().EndsWith))
                    .ToArray();
            }
            catch
            {
                textures = new string[0];
            }

            List<BarTextureData> result = new List<BarTextureData>(textures.Length);

            for (int i = 0; i < textures.Length; i++)
            {
                string name = SanitizedTextureName(textures[i].Replace(path, ""));
                result.Add(new BarTextureData(name, textures[i], isCustom));
            }

            return result;
        }

        private string SanitizedTextureName(string name)
        {
            return name
                .Replace(".png", "")
                .Replace(".PNG", "")
                .Replace(".tga", "")
                .Replace(".TGA", "");
        }
    }
}
