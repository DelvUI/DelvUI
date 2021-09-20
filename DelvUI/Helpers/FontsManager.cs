using ImGuiNET;
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

        public ImFontPtr BigNoodleTooFont = null;
    }
}
