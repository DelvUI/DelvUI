using DelvUI.Config;
using DelvUI.Helpers;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private const string _mainWindowName = "Party List";
        private static int MaxMemberCount = 8;

        // layout
        private Vector2 _origin;
        private Vector2 _size;
        private uint _rowCount = 0;
        private uint _colCount = 0;
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
            if (args.PropertyName == "Size" || args.PropertyName == "FillRowsFirst")
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
            int row = 0;
            int col = 0;

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
                bar.Position = new Vector2(
                    origin.X + HealthBarsConfig.Size.X * col + HealthBarsConfig.Padding.X * col,
                    origin.Y + HealthBarsConfig.Size.Y * row + HealthBarsConfig.Padding.Y * row
                );
                bar.Visible = true;

                // layout
                if (Config.FillRowsFirst)
                {
                    col = col + 1;
                    if (col >= _colCount)
                    {
                        col = 0;
                        row = row + 1;
                    }
                }
                else
                {
                    row = row + 1;
                    if (row >= _rowCount)
                    {
                        row = 0;
                        col = col + 1;
                    }
                }
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
            var margin = new Vector2(5, 5);
            ImGui.SetNextWindowPos(Config.Position - margin, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(Config.Size + margin * 2, ImGuiCond.FirstUseEver);

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
            var contentStartPos = windowPos + margin;
            var maxSize = windowSize - margin * 2;

            if (_layoutDirty || _size != maxSize || _memberCount != count)
            {
                LayoutHelper.CalculateLayout(
                    maxSize,
                    HealthBarsConfig.Size,
                    PartyManager.Instance.MemberCount,
                    HealthBarsConfig.Padding,
                    Config.FillRowsFirst,
                    out _rowCount,
                    out _colCount
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

            // draw
            var drawList = ImGui.GetWindowDrawList();
            for (int i = 0; i < bars.Count; i++)
            {
                bars[i].Draw(origin, target, drawList);
            }

            ImGui.End();
        }
    }
}
