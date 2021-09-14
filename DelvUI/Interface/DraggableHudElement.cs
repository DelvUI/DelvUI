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

        private string _displayName;

        public bool Selected = false;

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

            // update config object position
            ImGui.Begin("dragArea " + ID, windowFlags);
            var windowPos = ImGui.GetWindowPos();
            _config.Position = windowPos + size / 2f - origin;

            // draw window
            var drawList = ImGui.GetWindowDrawList();
            var contentPos = windowPos + contentMargin;
            var contentSize = size - contentMargin * 2;

            // draw hud elements
            DrawChildren(origin);

            // draw draggable indicators
            drawList.AddRectFilled(contentPos, contentPos + contentSize, 0x88444444, 3);
            drawList.AddRect(contentPos, contentPos + contentSize, 0xAAFFFFFF, 3, ImDrawFlags.None, 2);
            drawList.AddLine(contentPos + new Vector2(contentSize.X / 2f, 0), contentPos + new Vector2(contentSize.X / 2, contentSize.Y), 0xAAFFFFFF);
            drawList.AddLine(contentPos + new Vector2(0, contentSize.Y / 2f), contentPos + new Vector2(contentSize.X, contentSize.Y / 2), 0xAAFFFFFF);

            // element name
            var textSize = ImGui.CalcTextSize(_displayName);
            DrawHelper.DrawOutlinedText(_displayName, contentPos + contentSize / 2f - textSize / 2f, drawList);

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
