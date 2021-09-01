using System;
using System.Collections.Generic;
using Dalamud.Data.LuminaExtensions;
using Dalamud.Plugin;
using ImGuiScene;
using Lumina.Data.Files;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Action = Lumina.Excel.GeneratedSheets.Action;
using Status = Lumina.Excel.GeneratedSheets.Status;

namespace DelvUI.Helpers
{
    public class TexturesCache
    {
        #region Singleton
        private static TexturesCache instance = null;
        private DalamudPluginInterface pluginInterface;

        private TexturesCache(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public static void Initialize(DalamudPluginInterface pluginInterface)
        {
            instance = new TexturesCache(pluginInterface);
        }

        public static TexturesCache Instance
        {
            get
            {
                return instance;
            }
        }
        #endregion

        private Dictionary<Type, Dictionary<uint, TextureWrap>> Cache = new Dictionary<Type, Dictionary<uint, TextureWrap>>()
        {
            [typeof(Status)] = new Dictionary<uint, TextureWrap>(),
            [typeof(Action)] = new Dictionary<uint, TextureWrap>(),
            [typeof(Mount)] = new Dictionary<uint, TextureWrap>(),
            [typeof(Item)] = new Dictionary<uint, TextureWrap>(),
            [typeof(Companion)] = new Dictionary<uint, TextureWrap>()
        };

        public TextureWrap GetTexture<T>(uint rowId, uint stackCount = 0) where T : ExcelRow
        {
            var sheet = pluginInterface.Data.GetExcelSheet<T>();
            if (sheet == null) return null;

            return GetTexture<T>(sheet.GetRow(rowId), stackCount);
        }

        public TextureWrap GetTexture<T>(dynamic row, uint stackCount = 0) where T : ExcelRow
        {
            if (row == null) return null;

            var iconId = row?.Icon;
            if (iconId == null) return null;

            return GetTextureFromIconId<T>(iconId, stackCount);
        }

        public TextureWrap GetTextureFromIconId<T>(uint iconId, uint stackCount = 0) where T : ExcelRow
        {
            if (Cache.TryGetValue(typeof(T), out var map))
            {
                if (map.TryGetValue(iconId + stackCount, out TextureWrap texture))
                {
                    return texture;
                }
            }

            if (map == null) return null;

            TexFile iconFile = pluginInterface.Data.GetIcon((int)iconId + (int)stackCount);
            if (iconFile == null) return null;

            var newTexture = pluginInterface.UiBuilder.LoadImageRaw(iconFile.GetRgbaImageData(), iconFile.Header.Width, iconFile.Header.Height, 4);
            map.Add(iconId + stackCount, newTexture);

            return newTexture;
        }

        private void RemoveTexture<T>(uint rowId) where T : ExcelRow
        {
            var sheet = pluginInterface.Data.GetExcelSheet<T>();
            if (sheet == null) return;

            RemoveTexture<T>(sheet.GetRow(rowId));
        }

        public void RemoveTexture<T>(dynamic row) where T : ExcelRow
        {
            if (row == null) return;

            var iconId = row?.Icon;
            if (iconId == null) return;

            if (Cache.TryGetValue(typeof(T), out var map))
            {
                if (map.ContainsKey(iconId))
                {
                    map.Remove(iconId);
                }
            }
        }

        public void Clear()
        {
            Cache.Clear();
        }
    }
}
