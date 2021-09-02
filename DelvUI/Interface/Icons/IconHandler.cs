using System.Numerics;
using Dalamud.Plugin;
using DelvUI.Helpers;
using ImGuiNET;
using Lumina.Excel;

namespace DelvUI.Interface.Icons
{
    public class IconHandler
    {
        #region Singleton

        private DalamudPluginInterface _pluginInterface;

        private IconHandler(DalamudPluginInterface pluginInterface)
        {
            this._pluginInterface = pluginInterface;
        }

        public static void Initialize(DalamudPluginInterface pluginInterface)
        {
            Instance = new IconHandler(pluginInterface);
        }

        public static IconHandler Instance { get; private set; } = null;

        #endregion

        public static void DrawIcon<T>(dynamic row, Vector2 size, Vector2 position, bool drawBorder) where T : ExcelRow
        {
            // Status = 24x32, show from 2,7 until 22,26
            var texture = TexturesCache.Instance.GetTexture<T>(row, true);
            if (texture == null) return;
            var uv0 = new Vector2(4f / texture.Width, 14f / texture.Height);
            var uv1 = new Vector2(1f - 4f / texture.Width, 1f - 12f / texture.Height);
            

            ImGui.SetCursorPos(position);
            ImGui.Image(texture.ImGuiHandle, size, uv0, uv1);
            if (drawBorder)
            {
                var drawList = ImGui.GetWindowDrawList();
                drawList.AddRect(position, position + size, 0xFF000000);
            }
        }
    }
}