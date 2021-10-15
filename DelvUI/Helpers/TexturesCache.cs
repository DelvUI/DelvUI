using Dalamud.Plugin.Ipc;
using ImGuiScene;
using Lumina.Excel;
using System;
using System.Collections.Generic;

namespace DelvUI.Helpers
{
    public class TexturesCache : IDisposable
    {
        private Dictionary<uint, TextureWrap> _cache = new();

        public TextureWrap? GetTexture<T>(uint rowId, uint stackCount = 0, bool hdIcon = true) where T : ExcelRow
        {
            var sheet = Plugin.DataManager.GetExcelSheet<T>();

            return sheet == null ? null : GetTexture<T>(sheet.GetRow(rowId), stackCount, hdIcon);
        }

        public TextureWrap? GetTexture<T>(dynamic? row, uint stackCount = 0, bool hdIcon = true) where T : ExcelRow
        {
            if (row == null)
            {
                return null;
            }

            var iconId = row.Icon;
            return GetTextureFromIconId(iconId, stackCount, hdIcon);
        }

        public TextureWrap? GetTextureFromIconId(uint iconId, uint stackCount = 0, bool hdIcon = true)
        {
            if (_cache.TryGetValue(iconId + stackCount, out var texture))
            {
                return texture;
            }

            var newTexture = LoadTexture(iconId + stackCount, hdIcon);
            if (newTexture == null)
            {
                return null;
            }

            _cache.Add(iconId + stackCount, newTexture);

            return newTexture;
        }

        private unsafe TextureWrap? LoadTexture(uint id, bool hdIcon)
        {
            var hdString = hdIcon ? "_hr1" : "";
            var path = $"ui/icon/{id / 1000 * 1000:000000}/{id:000000}{hdString}.tex";

            try
            {
                var resolvedPath = _penumbraPathResolver.InvokeFunc(path);

                if (resolvedPath != null && resolvedPath != path)
                {
                    return TextureLoader.LoadTexture(resolvedPath, true);
                }
            }
            catch { }

            return TextureLoader.LoadTexture(path, false);
        }

        private void RemoveTexture<T>(uint rowId) where T : ExcelRow
        {
            var sheet = Plugin.DataManager.GetExcelSheet<T>();

            if (sheet == null)
            {
                return;
            }

            RemoveTexture<T>(sheet.GetRow(rowId));
        }

        public void RemoveTexture<T>(dynamic? row) where T : ExcelRow
        {
            if (row == null || row?.Icon == null)
            {
                return;
            }

            var iconId = row!.Icon;
            RemoveTexture(iconId);
        }

        public void RemoveTexture(uint iconId)
        {
            if (_cache.ContainsKey(iconId))
            {
                _cache.Remove(iconId);
            }
        }

        public void Clear() { _cache.Clear(); }

        #region Singleton
        private ICallGateSubscriber<string, string> _penumbraPathResolver;

        private TexturesCache()
        {
            _penumbraPathResolver = Plugin.PluginInterface.GetIpcSubscriber<string, string>("Penumbra.ResolveDefaultPath");
        }

        public static void Initialize() { Instance = new TexturesCache(); }

        public static TexturesCache Instance { get; private set; } = null!;

        ~TexturesCache()
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

            foreach (var key in _cache.Keys)
            {
                var tex = _cache[key];
                tex?.Dispose();
            }

            _cache.Clear();

            Instance = null!;
        }
        #endregion
    }
}
