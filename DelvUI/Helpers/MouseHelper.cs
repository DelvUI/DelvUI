using Dalamud.Game;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DelvUI.Helpers
{
    public class MouseHelper : IDisposable
    {
        #region Singleton
        private MouseHelper()
        {
        }

        public static void Initialize() { Instance = new MouseHelper(); }

        public static MouseHelper Instance { get; private set; } = null!;

        ~MouseHelper()
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

            Instance = null!;
        }
        #endregion

        public MouseButtonState LeftButton { get; private set; } = MouseButtonState.Released;
        public MouseButtonState RightButton { get; private set; } = MouseButtonState.Released;

        public void Update()
        {
            LeftButton = UpdateButton(Control.MouseButtons == MouseButtons.Left, LeftButton);
            RightButton = UpdateButton(Control.MouseButtons == MouseButtons.Right, RightButton);
        }

        public MouseButtonState UpdateButton(bool pressed, MouseButtonState currentState)
        {
            // release
            if (!pressed)
            {
                return MouseButtonState.Released;
            }

            // click
            if (currentState == MouseButtonState.Released)
            {
                return MouseButtonState.Clicked;
            }

            // held
            return MouseButtonState.Held;
        }
    }

    public enum MouseButtonState
    {
        Clicked,
        Held,
        Released
    }
}
