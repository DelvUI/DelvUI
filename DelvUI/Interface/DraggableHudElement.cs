using Dalamud.Logging;
using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Helpers;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DelvUI.Interface
{
    public delegate void DraggableHudElementSelectHandler(DraggableHudElement element);

    public class DraggableHudElement : HudElement
    {
        public DraggableHudElement(MovablePluginConfigObject config, string? displayName = null) : base(config)
        {
            _displayName = displayName ?? ID;
        }

        public event DraggableHudElementSelectHandler? SelectEvent;
        public bool Selected = false;

        private string _displayName;
        protected bool _windowPositionSet = false;
        private Vector2 _lastWindowPos = Vector2.Zero;
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

        public bool CanTakeInputForDrag = false;
        public bool NeedsInputForDrag { get; private set; } = false;

        public virtual Vector2 ParentPos() { return Vector2.Zero; } // override

        protected sealed override void CreateDrawActions(Vector2 origin)
        {
            if (_draggingEnabled)
            {
                AddDrawAction(_config.StrataLevel, () =>
                {
                    DrawDraggableArea(origin);
                });
                return;
            }

            DrawChildren(origin);
        }

        public virtual void DrawChildren(Vector2 origin) { }

        private bool CalculateNeedsInput(Vector2 pos, Vector2 size, bool selected)
        {
            Vector2 mousePos = ImGui.GetMousePos();

            if (ImGui.IsMouseHoveringRect(pos, pos + size))
            {
                return true;
            }

            if (!selected)
            {
                return false;
            }

            var arrowsPos = DraggablesHelper.GetArrowPositions(pos, size);

            foreach (Vector2 arrowPos in arrowsPos)
            {
                if (ImGui.IsMouseHoveringRect(arrowPos, arrowPos + DraggablesHelper.ArrowSize))
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual void DrawDraggableArea(Vector2 origin)
        {
            var windowFlags = ImGuiWindowFlags.NoScrollbar
            | ImGuiWindowFlags.NoTitleBar
            | ImGuiWindowFlags.NoResize
            | ImGuiWindowFlags.NoBackground
            | ImGuiWindowFlags.NoDecoration
            | ImGuiWindowFlags.NoSavedSettings;

            // always update size
            var size = MaxPos - MinPos + _contentMargin * 2;
            ImGui.SetNextWindowSize(size, ImGuiCond.Always);

            // needs input?
            NeedsInputForDrag = CanTakeInputForDrag && CalculateNeedsInput(_lastWindowPos, size, Selected);

            if (!NeedsInputForDrag)
            {
                windowFlags |= ImGuiWindowFlags.NoMove;
            }

            // set initial position
            if (!_windowPositionSet)
            {
                ImGui.SetNextWindowPos(origin + MinPos - _contentMargin);
                _windowPositionSet = true;

                _positionOffset = _config.Position - MinPos + _contentMargin;
            }

            // update config object position
            ImGui.Begin(ID + "_dragArea", windowFlags);
            var windowPos = ImGui.GetWindowPos();
            _lastWindowPos = windowPos;
            _config.Position = windowPos + _positionOffset - origin;

            // check selection
            var tooltipText = "x: " + _config.Position.X.ToString() + "    y: " + _config.Position.Y.ToString();

            if (NeedsInputForDrag && ImGui.IsMouseHoveringRect(windowPos, windowPos + size))
            {
                bool cliked = ImGui.IsMouseClicked(ImGuiMouseButton.Left) || ImGui.IsMouseDown(ImGuiMouseButton.Left);
                if (cliked && !Selected)
                {
                    SelectEvent?.Invoke(this);
                }

                // tooltip
                TooltipsHelper.Instance.ShowTooltipOnCursor(tooltipText);
            }

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

            ImGui.End();

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

            // element name
            var textSize = ImGui.CalcTextSize(_displayName);
            var textColor = Selected ? 0xFFFFFFFF : 0xEEFFFFFF;
            var textOutlineColor = Selected ? 0xFF000000 : 0xEE000000;
            DrawHelper.DrawOutlinedText(_displayName, contentPos + contentSize / 2f - textSize / 2f, textColor, textOutlineColor, drawList);
        }

        #region draggable area
        protected Vector2? _minPos = null;
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

                var anchorConfig = _config as AnchorablePluginConfigObject;
                for (int i = 0; i < positions.Count; i++)
                {
                    var pos = GetAnchoredPosition(positions[i], sizes[i], anchorConfig?.Anchor ?? DrawAnchor.Center);
                    minX = Math.Min(minX, pos.X);
                    minY = Math.Min(minY, pos.Y);
                }

                _minPos = new Vector2(minX, minY);
                return (Vector2)_minPos;
            }
        }

        protected Vector2? _maxPos = null;
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

                var anchorConfig = _config as AnchorablePluginConfigObject;
                for (int i = 0; i < positions.Count; i++)
                {
                    var pos = GetAnchoredPosition(positions[i], sizes[i], anchorConfig?.Anchor ?? DrawAnchor.Center) + sizes[i];
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

        protected virtual Vector2 GetAnchoredPosition(Vector2 position, Vector2 size, DrawAnchor anchor)
        {
            return Utils.GetAnchoredPosition(ParentPos() + position, size, anchor);
        }

        protected virtual (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            return (new List<Vector2>(), new List<Vector2>());
        }
        #endregion
    }

    public abstract class ParentAnchoredDraggableHudElement : DraggableHudElement
    {
        public ParentAnchoredDraggableHudElement(MovablePluginConfigObject config, string? displayName = null)
            : base(config, displayName)
        {
        }

        protected virtual bool AnchorToParent { get; }
        protected virtual DrawAnchor ParentAnchor { get; }
        public AnchorablePluginConfigObject? ParentConfig { get; set; }

        private Vector2? _lastParentPosition = null;

        private bool IsAnchored => AnchorToParent && ParentConfig != null;

        public override Vector2 ParentPos()
        {
            if (!IsAnchored)
            {
                return Vector2.Zero;
            }

            Vector2 parentAnchoredPos = Utils.GetAnchoredPosition(ParentConfig!.Position, ParentConfig!.Size, ParentConfig!.Anchor);
            return Utils.GetAnchoredPosition(parentAnchoredPos, -ParentConfig!.Size, ParentAnchor);
        }

        protected override void DrawDraggableArea(Vector2 origin)
        {
            // if the parent moved, update own draggable area
            if (IsAnchored && (_lastParentPosition == null || _lastParentPosition != ParentConfig!.Position))
            {
                _windowPositionSet = false;
                _minPos = null;
                _maxPos = null;
                _lastParentPosition = ParentConfig!.Position;
            }

            base.DrawDraggableArea(origin);
        }
    }
}
