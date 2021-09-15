using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Helpers;
using ImGuiNET;
using Newtonsoft.Json;
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
            if (!_draggingEnabled)
            {
                DrawChildren(origin);
                return;
            }

            var windowFlags = ImGuiWindowFlags.NoScrollbar
                | ImGuiWindowFlags.NoTitleBar
                | ImGuiWindowFlags.NoResize
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoDecoration;

            // validate size
            var contentMargin = new Vector2(4, 0);
            var size = MaxPos - MinPos;
            size.X = Math.Max(100, size.X + contentMargin.X * 2);
            size.Y = Math.Max(20, size.Y + contentMargin.Y * 2);

            // always update size
            ImGui.SetNextWindowSize(size, ImGuiCond.Always);

            // set initial position
            if (!_windowPositionSet)
            {
                ImGui.SetNextWindowPos(origin + _config.Position - size / 2f);
                _windowPositionSet = true;
            }
            var tooltipText = "x: " + _config.Position.X.ToString() + "    y: " + _config.Position.Y.ToString();

            // update config object position
            ImGui.Begin("dragArea " + ID, windowFlags);
            var windowPos = ImGui.GetWindowPos();

            // round numbers when saving the position
            _config.Position = new Vector2(
                (int)(windowPos.X + size.X / 2f - origin.X),
                (int)(windowPos.Y + size.Y / 2f - origin.Y)
            );

            // check selection
            if (ImGui.IsMouseHoveringRect(windowPos, windowPos + size))
            {
                bool cliked = ImGui.IsMouseClicked(ImGuiMouseButton.Left) || ImGui.IsMouseDown(ImGuiMouseButton.Left);
                if (cliked && !Selected && SelectEvent != null)
                {
                    SelectEvent(this, null);
                }

                // tooltip

                TooltipsHelper.Instance.ShowTooltipOnCursor(tooltipText);
            }

            // draw window
            var drawList = ImGui.GetWindowDrawList();
            var contentPos = windowPos + contentMargin;
            var contentSize = size - contentMargin * 2;

            // draw hud elements
            DrawChildren(origin);

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

            ImGui.End();

            // arrows
            if (Selected)
            {
                if (DraggablesHelper.DrawArrows(windowPos, size, tooltipText, out var offset))
                {
                    _minPos += offset;
                    _maxPos += offset;
                    _config.Position += offset;
                    _windowPositionSet = false;
                }
            }
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
                    var pos = positions[i] - sizes[i] / 2f;
                    minX = Math.Min(minX, pos.X);
                    minY = Math.Min(minY, pos.Y);
                }

                return new Vector2(minX, minY);
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
                    var pos = positions[i] + sizes[i] / 2f;
                    maxX = Math.Max(maxX, pos.X);
                    maxY = Math.Max(maxY, pos.Y);
                }

                return new Vector2(maxX, maxY);
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
