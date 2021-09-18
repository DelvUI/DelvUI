using DelvUI.Config;
using DelvUI.Helpers;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.Party
{
    public class PartyFramesHud : DraggableHudElement
    {
        private PartyFramesConfig Config => (PartyFramesConfig)_config;
        private PartyFramesHealthBarsConfig HealthBarsConfig;

        private const ImGuiWindowFlags _lockedBarFlags = ImGuiWindowFlags.NoBackground |
                                                        ImGuiWindowFlags.NoMove |
                                                        ImGuiWindowFlags.NoResize |
                                                        ImGuiWindowFlags.NoNav;

        private Vector2 _contentMargin = new Vector2(5, 5);
        private const string _mainWindowName = "Party List";
        private static int MaxMemberCount = 8;

        // layout
        private Vector2 _origin;
        private Vector2 _size;
        private LayoutInfo _layoutInfo;
        private uint _memberCount = 0;
        private bool _layoutDirty = true;

        private List<PartyFramesHealthBar> bars;


        public PartyFramesHud(string id, PartyFramesConfig config, PartyFramesHealthBarsConfig healthBarsConfig, string displayName) : base(id, config, displayName)
        {
            HealthBarsConfig = healthBarsConfig;
            config.onValueChanged += OnLayoutPropertyChanged;
            healthBarsConfig.onValueChanged += OnLayoutPropertyChanged;
            healthBarsConfig.ColorsConfig.onValueChanged += OnLayoutPropertyChanged;

            bars = new List<PartyFramesHealthBar>(MaxMemberCount);
            for (int i = 0; i < bars.Capacity; i++)
            {
                bars.Add(new PartyFramesHealthBar(healthBarsConfig));
            }

            PartyManager.Instance.MembersChangedEvent += OnMembersChanged;
            UpdateBars(Vector2.Zero);
        }

        ~PartyFramesHud()
        {
            bars.Clear();

            _config.onValueChanged -= OnLayoutPropertyChanged;
            HealthBarsConfig.onValueChanged -= OnLayoutPropertyChanged;
            HealthBarsConfig.ColorsConfig.onValueChanged -= OnLayoutPropertyChanged;
            PartyManager.Instance.MembersChangedEvent -= OnMembersChanged;
        }

        private void OnLayoutPropertyChanged(object sender, OnChangeBaseArgs args)
        {
            if (args.PropertyName == "Size" || args.PropertyName == "FillRowsFirst" || args.PropertyName == "BarsAnchor")
            {
                _layoutDirty = true;
            }
        }

        private void OnMembersChanged(object sender, EventArgs args)
        {
            UpdateBars(_origin);
        }

        public void UpdateBars(Vector2 origin)
        {
            var memberCount = PartyManager.Instance.MemberCount;
            uint row = 0;
            uint col = 0;
            var spaceSize = Config.Size - _contentMargin * 2;

            for (int i = 0; i < bars.Count; i++)
            {
                PartyFramesHealthBar bar = bars[i];
                if (i >= memberCount)
                {
                    bar.Visible = false;
                    continue;
                }

                // update bar
                IPartyFramesMember member = PartyManager.Instance.GroupMembers.ElementAt(i);
                bar.Member = member;
                bar.Visible = true;

                // anchor and position
                CalculateBarPosition(origin, spaceSize, out var x, out var y);
                bar.Position = new Vector2(
                    x + HealthBarsConfig.Size.X * col + HealthBarsConfig.Padding.X * col,
                    y + HealthBarsConfig.Size.Y * row + HealthBarsConfig.Padding.Y * row
                );

                // layout
                if (Config.FillRowsFirst)
                {
                    col = col + 1;
                    if (col >= _layoutInfo.TotalColCount)
                    {
                        col = 0;
                        row = row + 1;
                    }
                }
                else
                {
                    row = row + 1;
                    if (row >= _layoutInfo.TotalRowCount)
                    {
                        row = 0;
                        col = col + 1;
                    }
                }
            }
        }

        private void CalculateBarPosition(Vector2 position, Vector2 spaceSize, out float x, out float y)
        {
            x = position.X;
            y = position.Y;

            if (Config.BarsAnchor == HudElementAnchor.Top ||
                Config.BarsAnchor == HudElementAnchor.Center ||
                Config.BarsAnchor == HudElementAnchor.Bottom)
            {
                x += (spaceSize.X - _layoutInfo.ContentSize.X) / 2f;
            }
            else if (Config.BarsAnchor == HudElementAnchor.TopRight ||
                Config.BarsAnchor == HudElementAnchor.Right ||
                Config.BarsAnchor == HudElementAnchor.BottomRight)
            {
                x += spaceSize.X - _layoutInfo.ContentSize.X;
            }

            if (Config.BarsAnchor == HudElementAnchor.Left ||
                Config.BarsAnchor == HudElementAnchor.Center ||
                Config.BarsAnchor == HudElementAnchor.Right)
            {
                y += (spaceSize.Y - _layoutInfo.ContentSize.Y) / 2f;
            }
            else if (Config.BarsAnchor == HudElementAnchor.BottomLeft ||
                Config.BarsAnchor == HudElementAnchor.Bottom ||
                Config.BarsAnchor == HudElementAnchor.BottomRight)
            {
                y += spaceSize.Y - _layoutInfo.ContentSize.Y;
            }
        }

        private void UpdateBarsPosition(Vector2 delta)
        {
            foreach (var bar in bars)
            {
                bar.Position = bar.Position + delta;
            }
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            return (new List<Vector2>() { Config.Position + Config.Size / 2f }, new List<Vector2>() { Config.Size });
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (!_config.Enabled)
            {
                return;
            }

            var count = PartyManager.Instance.MemberCount;
            if (count < 1)
            {
                return;
            }

            // size and position
            ImGui.SetNextWindowPos(Config.Position - _contentMargin, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(Config.Size + _contentMargin * 2, ImGuiCond.FirstUseEver);

            var windowFlags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoTitleBar;
            if (Config.Lock)
            {
                windowFlags |= _lockedBarFlags;
            }

            ImGui.Begin(_mainWindowName, windowFlags);
            var windowPos = ImGui.GetWindowPos();
            var windowSize = ImGui.GetWindowSize();
            Config.Position = windowPos;
            Config.Size = windowSize;

            // recalculate layout on settings or size change
            var contentStartPos = windowPos + _contentMargin;
            var maxSize = windowSize - _contentMargin * 2;

            if (_layoutDirty || _size != maxSize || _memberCount != count)
            {
                _layoutInfo = LayoutHelper.CalculateLayout(
                    maxSize,
                    HealthBarsConfig.Size,
                    PartyManager.Instance.MemberCount,
                    HealthBarsConfig.Padding,
                    Config.FillRowsFirst
                );

                UpdateBars(contentStartPos);
            }
            else if (_origin != contentStartPos)
            {
                UpdateBarsPosition(contentStartPos - _origin);
            }

            _layoutDirty = false;
            _origin = contentStartPos;
            _memberCount = count;
            _size = maxSize;

            var target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.CurrentTarget;
            var targetIndex = -1;

            // draw
            var drawList = ImGui.GetWindowDrawList();
            for (int i = 0; i < count; i++)
            {
                bars[i].Draw(origin, drawList);

                if (target != null && bars[i].Member.ActorID == target.ActorId)
                {
                    targetIndex = i;
                }
            }

            // target border
            if (targetIndex >= 0)
            {
                var borderPos = bars[targetIndex].Position - Vector2.One;
                var borderSize = HealthBarsConfig.Size + Vector2.One * 2;
                drawList.AddRect(borderPos, borderPos + borderSize, 0xFFFFFFFF);
            }

            ImGui.End();
        }
    }
}
