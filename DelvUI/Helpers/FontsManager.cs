using ImGuiNET;

namespace DelvUI.Helpers
{
    public class FontsManager
    {
        #region Singleton
        private FontsManager() { }

        public static void Initialize() { Instance = new FontsManager(); }

        public static FontsManager Instance { get; private set; }

        #endregion

        public ImFontPtr BigNoodleTooFont = null;
    }
}
