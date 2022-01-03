using System;
using System.Numerics;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using ImGuiNET;

namespace DelvUI.Helpers
{
    public class TooltipsHelper : IDisposable
    {
        #region Singleton
        private TooltipsHelper()
        {
        }

        public static void Initialize() { Instance = new TooltipsHelper(); }

        public static TooltipsHelper Instance { get; private set; } = null!;

        ~TooltipsHelper()
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

        private static float MaxWidth = 300;
        private static float Margin = 5;

        private TooltipsConfig _config => ConfigurationManager.Instance.GetConfigObject<TooltipsConfig>();

        private string? _currentTooltipText = null;
        private Vector2 _textSize;
        private string? _currentTooltipTitle = null;
        private Vector2 _titleSize;
        private string? _previousRawText = null;

        private Vector2 _position;
        private Vector2 _size;

        private bool _dataIsValid = false;

        public void ShowTooltipOnCursor(string text, string? title = null, uint id = 0, string name = "")
        {
            ShowTooltip(text, ImGui.GetMousePos(), title, id, name);
        }

        public void ShowTooltip(string text, Vector2 position, string? title = null, uint id = 0, string name = "")
        {
            if (text == null)
            {
                return;
            }

            // remove styling tags from text
            if (_previousRawText != text)
            {
                _currentTooltipText = text;
                _previousRawText = text;
            }

            // calcualte title size
            _titleSize = Vector2.Zero;
            if (title != null)
            {
                _currentTooltipTitle = title;

                if (_config.ShowSourceName)
                {
                    _currentTooltipTitle += $" ({name})";
                }

                if (_config.ShowStatusIDs)
                {
                    _currentTooltipTitle += " (ID: " + id + ")";
                }

                using (FontsManager.Instance.PushFont(_config.TitleFontID))
                {
                    _titleSize = ImGui.CalcTextSize(_currentTooltipTitle, MaxWidth);
                    _titleSize.Y += Margin;
                }
            }

            // calculate text size
            using (FontsManager.Instance.PushFont(_config.TextFontID))
                _textSize = ImGui.CalcTextSize(_currentTooltipText, MaxWidth);

            _size = new Vector2(Math.Max(_titleSize.X, _textSize.X) + Margin * 2, _titleSize.Y + _textSize.Y + Margin * 2);

            // position tooltip using the given coordinates as bottom center
            position.X = position.X - _size.X / 2f;
            position.Y = position.Y - _size.Y;

            // correct tooltips off screen
            _position = ConstrainPosition(position, _size);

            _dataIsValid = true;
        }

        public void RemoveTooltip()
        {
            _dataIsValid = false;
        }

        public void Draw()
        {
            if (!_dataIsValid || ConfigurationManager.Instance.ShowingModalWindow)
            {
                return;
            }

            // bg
            ImGuiWindowFlags windowFlags =
                  ImGuiWindowFlags.NoTitleBar
                | ImGuiWindowFlags.NoMove
                | ImGuiWindowFlags.NoDecoration
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoInputs
                | ImGuiWindowFlags.NoSavedSettings;

            // imgui clips the left and right borders inside windows for some reason
            // we make the window bigger so the actual drawable size is the expected one
            var windowMargin = new Vector2(4, 0);
            var windowPos = _position - windowMargin;

            ImGui.SetNextWindowPos(windowPos, ImGuiCond.Always);
            ImGui.SetNextWindowSize(_size + windowMargin * 2);
            ImGui.SetNextWindowFocus();

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
            ImGui.Begin("DelvUI_tooltip", windowFlags);
            var drawList = ImGui.GetWindowDrawList();

            drawList.AddRectFilled(_position, _position + _size, _config.BackgroundColor.Base);

            if (_config.BorderConfig.Enabled)
            {
                drawList.AddRect(_position, _position + _size, _config.BorderConfig.Color.Base, 0, ImDrawFlags.None, _config.BorderConfig.Thickness);
            }

            if (_currentTooltipTitle != null)
            {
                // title
                Vector2 cursorPos;
                using (FontsManager.Instance.PushFont(_config.TitleFontID))
                {
                    cursorPos = new Vector2(windowMargin.X + _size.X / 2f - _titleSize.X / 2f, Margin);
                    ImGui.SetCursorPos(cursorPos);
                    ImGui.PushTextWrapPos(cursorPos.X + _titleSize.X);
                    ImGui.TextColored(_config.TitleColor.Vector, _currentTooltipTitle);
                    ImGui.PopTextWrapPos();
                }

                // text
                using (FontsManager.Instance.PushFont(_config.TextFontID))
                {
                    cursorPos = new Vector2(windowMargin.X + _size.X / 2f - _textSize.X / 2f, Margin + _titleSize.Y);
                    ImGui.SetCursorPos(cursorPos);
                    ImGui.PushTextWrapPos(cursorPos.X + _textSize.X);
                    ImGui.TextColored(_config.TextColor.Vector, _currentTooltipText);
                    ImGui.PopTextWrapPos();
                }
            }
            else
            {
                // text
                using (FontsManager.Instance.PushFont(_config.TextFontID))
                {
                    var cursorPos = windowMargin + new Vector2(Margin, Margin);
                    var textWidth = _size.X - Margin * 2;

                    ImGui.SetCursorPos(cursorPos);
                    ImGui.PushTextWrapPos(cursorPos.X + textWidth);
                    ImGui.TextColored(_config.TextColor.Vector, _currentTooltipText);
                    ImGui.PopTextWrapPos();
                }
            }

            ImGui.End();
            ImGui.PopStyleVar();

            RemoveTooltip();
        }

        private Vector2 ConstrainPosition(Vector2 position, Vector2 size)
        {
            var screenSize = ImGui.GetWindowViewport().Size;

            if (position.X < 0)
            {
                position.X = Margin;
            }
            else if (position.X + size.X > screenSize.X)
            {
                position.X = screenSize.X - size.X - Margin;
            }

            if (position.Y < 0)
            {
                position.Y = Margin;
            }

            return position;
        }
    }

    [Section("Misc")]
    [SubSection("Tooltips", 0)]
    public class TooltipsConfig : PluginConfigObject
    {
        public new static TooltipsConfig DefaultConfig() { return new TooltipsConfig(); }

        [Checkbox("Show Status Effects IDs")]
        [Order(5)]
        public bool ShowStatusIDs = false;

        [Checkbox("Show Source Name")]
        [Order(10)]
        public bool ShowSourceName = false;

        [ColorEdit4("Background Color")]
        [Order(15)]
        public PluginConfigColor BackgroundColor = new PluginConfigColor(new(19f / 255f, 19f / 255f, 19f / 255f, 190f / 250f));

        [Font("Title Font and Size", spacing = true)]
        [Order(20)]
        public string? TitleFontID = null;

        [ColorEdit4("Title Color")]
        [Order(25)]
        public PluginConfigColor TitleColor = new PluginConfigColor(new(255f / 255f, 210f / 255f, 31f / 255f, 100f / 100f));

        [Font("Text Font and Size", spacing = true)]
        [Order(30)]
        public string? TextFontID = null;

        [ColorEdit4("Text Color")]
        [Order(35)]
        public PluginConfigColor TextColor = new PluginConfigColor(new(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [NestedConfig("Border", 40, separator = false, spacing = true, collapsingHeader = false)]
        public TooltipBorderConfig BorderConfig = new();
    }

    [Exportable(false)]
    public class TooltipBorderConfig : PluginConfigObject
    {
        [ColorEdit4("Color")]
        [Order(5)]
        public PluginConfigColor Color = new(new Vector4(10f / 255f, 10f / 255f, 10f / 255f, 160f / 255f));

        [DragInt("Thickness", min = 1, max = 100)]
        [Order(10)]
        public int Thickness = 4;

        public TooltipBorderConfig()
        {
        }

        public TooltipBorderConfig(PluginConfigColor color, int thickness)
        {
            Color = color;
            Thickness = thickness;
        }
    }
}
