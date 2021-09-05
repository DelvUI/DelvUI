using Dalamud.Data.LuminaExtensions;
using Dalamud.Plugin;
using ImGuiScene;
using Lumina.Data.Files;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace DelvUI.Helpers
{
    public class TexturesCache
    {
        private readonly Dictionary<Type, Dictionary<uint, TextureWrap>> _cache = new()
        {
            [typeof(Status)] = new Dictionary<uint, TextureWrap>(),
            [typeof(Action)] = new Dictionary<uint, TextureWrap>(),
            [typeof(Mount)] = new Dictionary<uint, TextureWrap>(),
            [typeof(Item)] = new Dictionary<uint, TextureWrap>(),
            [typeof(Companion)] = new Dictionary<uint, TextureWrap>()
        };

        public TextureWrap GetTexture<T>(uint rowId, uint stackCount = 0, bool hdIcon = true) where T : ExcelRow
        {
            var sheet = _pluginInterface.Data.GetExcelSheet<T>();

            return sheet == null ? null : GetTexture<T>(sheet.GetRow(rowId), stackCount, hdIcon);
        }

        public TextureWrap GetTexture<T>(dynamic row, uint stackCount = 0, bool hdIcon = true) where T : ExcelRow
        {
            if (row == null)
            {
                return null;
            }

            var iconId = row.Icon;

            return iconId == null ? null : (TextureWrap)GetTextureFromIconId<T>(iconId, stackCount, hdIcon);
        }

        public TextureWrap GetTextureFromIconId<T>(uint iconId, uint stackCount = 0, bool hdIcon = true) where T : ExcelRow
        {
            if (_cache.TryGetValue(typeof(T), out var map))
            {
                if (map.TryGetValue(iconId + stackCount, out var texture))
                {
                    return texture;
                }
            }

            if (map == null)
            {
                return null;
            }

            //TexFile iconFile = pluginInterface.Data.GetIcon((int)iconId + (int)stackCount);
            var iconFile = LoadIcon(iconId + stackCount, hdIcon);

            if (iconFile == null)
            {
                return null;
            }

            var newTexture = _pluginInterface.UiBuilder.LoadImageRaw(iconFile.GetRgbaImageData(), iconFile.Header.Width, iconFile.Header.Height, 4);
            map.Add(iconId + stackCount, newTexture);

            return newTexture;
        }

        private TexFile LoadIcon(uint id, bool hdIcon)
        {
            var hdString = hdIcon ? "_hr1" : "";
            var path = $"ui/icon/{id / 1000 * 1000:000000}/{id:000000}{hdString}.tex";

            return _pluginInterface.Data.GetFile<TexFile>(path);
        }

        private void RemoveTexture<T>(uint rowId) where T : ExcelRow
        {
            var sheet = _pluginInterface.Data.GetExcelSheet<T>();

            if (sheet == null)
            {
                return;
            }

            RemoveTexture<T>(sheet.GetRow(rowId));
        }

        public void RemoveTexture<T>(dynamic row) where T : ExcelRow
        {
            if (row == null)
            {
                return;
            }

            var iconId = row?.Icon;

            if (iconId == null)
            {
                return;
            }

            if (_cache.TryGetValue(typeof(T), out var map))
            {
                if (map.ContainsKey(iconId))
                {
                    map.Remove(iconId);
                }
            }
        }

        public void Clear() { _cache.Clear(); }

        #region Singleton

        private readonly DalamudPluginInterface _pluginInterface;

        private TexturesCache(DalamudPluginInterface pluginInterface) { _pluginInterface = pluginInterface; }

        public static void Initialize(DalamudPluginInterface pluginInterface) { Instance = new TexturesCache(pluginInterface); }

        public static TexturesCache Instance { get; private set; }

        #endregion
    }
}
