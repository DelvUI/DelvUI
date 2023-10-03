using Dalamud.Interface.Internal;
using Lumina.Excel;
using static Dalamud.Plugin.Services.ITextureProvider;

namespace DelvUI.Helpers
{
    public class TexturesHelper
    {
        public static IDalamudTextureWrap? GetTexture<T>(uint rowId, uint stackCount = 0, bool hdIcon = true) where T : ExcelRow
        {
            var sheet = Plugin.DataManager.GetExcelSheet<T>();
            return sheet == null ? null : GetTexture<T>(sheet.GetRow(rowId), stackCount, hdIcon);
        }

        public static IDalamudTextureWrap? GetTexture<T>(dynamic? row, uint stackCount = 0, bool hdIcon = true) where T : ExcelRow
        {
            if (row == null)
            {
                return null;
            }

            var iconId = row.Icon;
            return GetTextureFromIconId(iconId, stackCount, hdIcon);
        }

        public static IDalamudTextureWrap? GetTextureFromIconId(uint iconId, uint stackCount = 0, bool hdIcon = true)
        {
            return Plugin.TextureProvider.GetIcon(iconId + stackCount, hdIcon ? IconFlags.HiRes : IconFlags.None);
        }

        public static IDalamudTextureWrap? GetTextureFromPath(string path)
        {
            return Plugin.TextureProvider.GetTextureFromGame(path);
        }
    }
}
