using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Helpers;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DelvUI.Interface
{
    public class DraggableHudElement : HudElement
    {
        public DraggableHudElement(string id, MovablePluginConfigObject config, string displayName = null) : base(id, config)
        {
            _displayName = displayName ?? id;
        }

        public event EventHandler SelectEvent;
        public bool Selected = false;

        private string _displayName;
        private bool _windowPositionSet = false;
        private Vector2 _positionOffset;
        private Vector2 _contentMargin = new Vector2(4, 0);

        private bool _draggingEnabled = false;
        public bool DraggingEnabled
        {
            get => _draggingEnabled;
            set
            {
                _draggingEnabled = value;

                if (_draggingEnabled)
                {
                    _windowPositionSet = false;
                    _minPos = null;
                    _maxPos = null;
                }
            }
        }

        public sealed override void Draw(Vector2 origin)
        {

            DrawChildren(origin);

            if (!_draggingEnabled)
            {
                return;
            }

            var windowFlags = ImGuiWindowFlags.NoScrollbar
            | ImGuiWindowFlags.NoTitleBar
            | ImGuiWindowFlags.NoResize
            | ImGuiWindowFlags.NoBackground
            | ImGuiWindowFlags.NoDecoration;

            // always update size
            var size = MaxPos - MinPos + _contentMargin * 2;
            ImGui.SetNextWindowSize(size, ImGuiCond.Always);

            var anchorConfig = _config as AnchorablePluginConfigObject;
            var winPos = Utils.GetAnchoredPosition(origin + MinPos, size, anchorConfig?.Anchor ?? DrawAnchor.Center);
            var winPosOffset = winPos - (origin + MinPos);

            // set initial position
            if (!_windowPositionSet)
            {
                ImGui.SetNextWindowPos(winPos);
                _windowPositionSet = true;

                _positionOffset = _config.Position - MinPos - winPosOffset;
            }

            // update config object position
            ImGui.Begin("dragArea " + ID, windowFlags);
            var windowPos = ImGui.GetWindowPos();
            _config.Position = windowPos + _positionOffset - origin;

            // check selection
            var tooltipText = "x: " + _config.Position.X.ToString() + "    y: " + _config.Position.Y.ToString();

            // draw window
            var drawList = ImGui.GetWindowDrawList();
            var contentPos = windowPos + _contentMargin;
            var contentSize = size - _contentMargin * 2;

            // draw draggable indicators
            drawList.AddRectFilled(contentPos, contentPos + contentSize, 0x88444444, 3);

            var lineColor = Selected ? 0xEEFFFFFF : 0x66FFFFFF;
            drawList.AddRect(contentPos, contentPos + contentSize, lineColor, 3, ImDrawFlags.None, 2);
            drawList.AddLine(contentPos + new Vector2(contentSize.X / 2f, 0), contentPos + new Vector2(contentSize.X / 2, contentSize.Y), lineColor);
            drawList.AddLine(contentPos + new Vector2(0, contentSize.Y / 2f), contentPos + new Vector2(contentSize.X, contentSize.Y / 2), lineColor);

            // element name
            var textSize = ImGui.CalcTextSize(_displayName);
            var textColor = Selected ? 0xFFFFFFFF : 0xEEFFFFFF;
            var textOutlineColor = Selected ? 0xFF000000 : 0xEE000000;
            DrawHelper.DrawOutlinedText(_displayName, contentPos + contentSize / 2f - textSize / 2f, textColor, textOutlineColor, drawList);

            bool mouseOver = ImGui.IsMouseHoveringRect(windowPos, windowPos + size);
            if (mouseOver)
            {
                bool clicked = ImGui.IsMouseClicked(ImGuiMouseButton.Left) || ImGui.IsMouseDown(ImGuiMouseButton.Left);
                if (clicked && !Selected && SelectEvent != null)
                {
                    SelectEvent(this, null);
                }

                // tooltip
                TooltipsHelper.Instance.ShowTooltipOnCursor(tooltipText);
            }

            // arrows
            if (Selected)
            {
                if (DraggablesHelper.DrawArrows(windowPos, size, tooltipText, out var movement))
                {
                    _minPos = null;
                    _maxPos = null;
                    _config.Position += movement;
                    _windowPositionSet = false;
                }
            }

            if (mouseOver || Selected)
            {
                var anchorableConfig = _config as AnchorablePluginConfigObject;
                var anchorIndicatorSize = new Vector2(10, 10);
                var anchor = anchorableConfig?.Anchor ?? DrawAnchor.Center;
                var anchorIndicatorPos = Utils.GetAnchoredPosition(Utils.GetAnchoredPosition(contentPos, -contentSize, anchor), anchorIndicatorSize, DrawAnchor.Center) + new Vector2(0, 1);
                drawList.AddRectFilled(anchorIndicatorPos, anchorIndicatorPos + anchorIndicatorSize, 0xAAFFFFFF);
            }

            ImGui.End();
        }

        public virtual void DrawChildren(Vector2 origin) { }

        #region draggable area
        private Vector2? _minPos = null;
        public Vector2 MinPos
        {
            get
            {
                if (_minPos != null)
                {
                    return (Vector2)_minPos;
                }

                var (positions, sizes) = ChildrenPositionsAndSizes();
                if (positions.Count == 0 || sizes.Count == 0)
                {
                    return Vector2.Zero;
                }

                float minX = float.MaxValue;
                float minY = float.MaxValue;

                for (int i = 0; i < positions.Count; i++)
                {
                    var pos = positions[i];
                    minX = Math.Min(minX, pos.X);
                    minY = Math.Min(minY, pos.Y);
                }

                _minPos = new Vector2(minX, minY);
                return (Vector2)_minPos;
            }
        }

        private Vector2? _maxPos = null;
        public Vector2 MaxPos
        {
            get
            {
                if (_maxPos != null)
                {
                    return (Vector2)_maxPos;
                }

                var (positions, sizes) = ChildrenPositionsAndSizes();
                if (positions.Count == 0 || sizes.Count == 0)
                {
                    return Vector2.Zero;
                }

                float maxX = float.MinValue;
                float maxY = float.MinValue;

                for (int i = 0; i < positions.Count; i++)
                {
                    var pos = positions[i] + sizes[i];
                    maxX = Math.Max(maxX, pos.X);
                    maxY = Math.Max(maxY, pos.Y);
                }

                _maxPos = new Vector2(maxX, maxY);
                return (Vector2)_maxPos;
            }
        }

        public void FlagDraggableAreaDirty()
        {
            _minPos = null;
            _maxPos = null;
        }

        protected virtual (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            return (new List<Vector2>(), new List<Vector2>());
        }
        #endregion
    }
}
