﻿using ImGuiScene;
using Lumina.Data.Files;
using Lumina.Excel;
using System;
using System.Collections.Generic;
using Dalamud.Utility;

namespace DelvUI.Helpers
{
    public class TexturesCache : IDisposable
    {
        private readonly Dictionary<uint, TextureWrap> _cache = new();

        public TextureWrap? GetTexture<T>(uint rowId, uint stackCount = 0, bool hdIcon = true) where T : ExcelRow
        {
            ExcelSheet<T>? sheet = Plugin.DataManager.GetExcelSheet<T>();

            return sheet == null ? null : GetTexture<T>(sheet.GetRow(rowId), stackCount, hdIcon);
        }

        public TextureWrap? GetTexture<T>(dynamic? row, uint stackCount = 0, bool hdIcon = true) where T : ExcelRow
        {
            if (row == null)
            {
                return null;
            }

            dynamic iconId = row.Icon;
            return GetTextureFromIconId(iconId);
        }

        public TextureWrap? GetTextureFromIconId(uint iconId, uint stackCount = 0, bool hdIcon = true)
        {
            if (_cache.TryGetValue(iconId + stackCount, out TextureWrap? texture))
            {
                return texture;
            }

            TexFile? iconFile = LoadIcon(iconId + stackCount, hdIcon);

            if (iconFile == null)
            {
                return null;
            }

            Dalamud.Interface.UiBuilder? builder = Plugin.UiBuilder;
            TextureWrap? newTexture = builder.LoadImageRaw(iconFile.GetRgbaImageData(), iconFile.Header.Width, iconFile.Header.Height, 4);
            _cache.Add(iconId + stackCount, newTexture);

            return newTexture;
        }

        private TexFile? LoadIcon(uint id, bool hdIcon)
        {
            string? hdString = hdIcon ? "_hr1" : "";
            string? path = $"ui/icon/{id / 1000 * 1000:000000}/{id:000000}{hdString}.tex";

            return Plugin.DataManager.GetFile<TexFile>(path);
        }

        private void RemoveTexture<T>(uint rowId) where T : ExcelRow
        {
            ExcelSheet<T>? sheet = Plugin.DataManager.GetExcelSheet<T>();

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

            dynamic iconId = row!.Icon;
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
        private TexturesCache() { }

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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            foreach (uint key in _cache.Keys)
            {
                TextureWrap? tex = _cache[key];
                tex?.Dispose();
            }

            _cache.Clear();

            Instance = null!;
        }
        #endregion
    }
}
