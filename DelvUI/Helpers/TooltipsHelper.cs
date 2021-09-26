using DelvUI.Config;
using DelvUI.Config.Attributes;
using ImGuiNET;
using System;
using System.Numerics;
using System.Text.RegularExpressions;

namespace DelvUI.Helpers
{
    public class TooltipsHelper
    {
        #region Singleton
        private TooltipsHelper()
        {
            _config = ConfigurationManager.GetInstance().GetConfigObject<TooltipsConfig>();
        }

        public static void Initialize() { Instance = new TooltipsHelper(); }

        public static TooltipsHelper Instance { get; private set; }

        #endregion

        private static float MaxWidth = 300;
        private static float Margin = 5;

        private TooltipsConfig _config;

        private string _currentTooltipText = null;
        private Vector2 _textSize;
        private string _currentTooltipTitle = null;
        private Vector2 _titleSize;
        private string _previousRawText = null;

        private Vector2 _position;
        private Vector2 _size;

        public void ShowTooltipOnCursor(string text, string title = null)
        {
            ShowTooltip(text, ImGui.GetMousePos(), title);
        }

        public void ShowTooltip(string text, Vector2 position, string title = null)
        {
            if (text == null)
            {
                return;
            }

            // remove styling tags from text
            if (_previousRawText != text)
            {
                _currentTooltipText = SanitizeText(text);
                _previousRawText = text;
            }

            // calcualte title size
            _titleSize = Vector2.Zero;
            if (title != null)
            {
                _currentTooltipTitle = title;
                _titleSize = ImGui.CalcTextSize(title, MaxWidth);
                _titleSize.Y += Margin;
            }

            // calculate text size
            _textSize = ImGui.CalcTextSize(_currentTooltipText, MaxWidth);
            _size = new Vector2(Math.Max(_titleSize.X, _textSize.X) + Margin * 2, _titleSize.Y + _textSize.Y + Margin * 2);

            // position tooltip using the given coordinates as bottom center
            position.X = position.X - _size.X / 2f;
            position.Y = position.Y - _size.Y;

            // correct tooltips off screen
            _position = ConstrainPosition(position, _size);
        }

        public void RemoveTooltip()
        {
            _currentTooltipText = null;
            _currentTooltipTitle = null;
            _previousRawText = null;
        }

        public void Draw()
        {
            if (_currentTooltipText == null)
            {
                return;
            }

            // bg
            ImGuiWindowFlags windowFlags =
                  ImGuiWindowFlags.NoTitleBar
                | ImGuiWindowFlags.NoMove
                | ImGuiWindowFlags.NoDecoration
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoInputs;

            // imgui clips the left and right borders inside windows for some reason
            // we make the window bigger so the actual drawable size is the expected one
            var windowMargin = new Vector2(4, 0);
            var windowPos = _position - windowMargin;

            ImGui.SetNextWindowPos(windowPos, ImGuiCond.Always);
            ImGui.SetNextWindowSize(_size + windowMargin * 2);
            ImGui.SetNextWindowFocus();

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
            ImGui.Begin("delvui_tooltip", windowFlags);
            var drawList = ImGui.GetWindowDrawList();

            drawList.AddRectFilled(_position, _position + _size, _config.BackgroundColor.Base);

            if (_currentTooltipTitle != null)
            {
                // title
                var cursorPos = new Vector2(windowMargin.X + _size.X / 2f - _titleSize.X / 2f, Margin);
                ImGui.SetCursorPos(cursorPos);
                ImGui.PushTextWrapPos(cursorPos.X + _titleSize.X);
                ImGui.TextColored(_config.TitleColor.Vector, _currentTooltipTitle);
                ImGui.PopTextWrapPos();

                // text
                cursorPos = new Vector2(windowMargin.X + _size.X / 2f - _textSize.X / 2f, Margin + _titleSize.Y);
                ImGui.SetCursorPos(cursorPos);
                ImGui.PushTextWrapPos(cursorPos.X + _textSize.X);
                ImGui.TextColored(_config.TextColor.Vector, _currentTooltipText);
                ImGui.PopTextWrapPos();
            }
            else
            {
                // text
                var cursorPos = windowMargin + new Vector2(Margin, Margin);
                var textWidth = _size.X - Margin * 2;

                ImGui.SetCursorPos(cursorPos);
                ImGui.PushTextWrapPos(cursorPos.X + textWidth);
                ImGui.TextColored(_config.TextColor.Vector, _currentTooltipText);
                ImGui.PopTextWrapPos();
            }

            ImGui.End();
            ImGui.PopStyleVar();

            RemoveTooltip();
        }

        private string SanitizeText(string text)
        {
            // some data comes with unicode characters i couldn't figure out how to get rid of
            // so im doing a pretty aggressive replace to keep only "nice" characters
            var result = Regex.Replace(text, @"[^a-zA-Z0-9 -\:\.\,\?\!\(\)%]", "");

            // after that there's still some leftovers characters that need to be removed
            Regex regex = new Regex("HI(.*?)IH");
            foreach (Match match in regex.Matches(result))
            {
                if (match.Groups.Count > 1)
                {
                    result = result.Replace(match.Value, match.Groups[1].Value);
                }
            }

            result = result.Replace("%", "%%");

            return result;
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

        [ColorEdit4("Title Color")]
        [Order(10)]
        public PluginConfigColor TitleColor = new PluginConfigColor(new(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [ColorEdit4("Text Color")]
        [Order(15)]
        public PluginConfigColor TextColor = new PluginConfigColor(new(255f / 255f, 255f / 255f, 255f / 255f, 80f / 100f));

        [ColorEdit4("Background Color")]
        [Order(20)]
        public PluginConfigColor BackgroundColor = new PluginConfigColor(new(0f / 255f, 0f / 255f, 0f / 255f, 60f / 100f));
    }
}
