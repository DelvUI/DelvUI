using DelvUI.Helpers;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using DelvUI.Config;


namespace DelvUI.Interface.Party {
    public class PartyHudWindow {
        private PluginConfiguration _pluginConfiguration;
        private PartyHudConfig _config;

        private const ImGuiWindowFlags _lockedBarFlags = ImGuiWindowFlags.NoBackground |
                                                        ImGuiWindowFlags.NoMove |
                                                        ImGuiWindowFlags.NoResize |
                                                        ImGuiWindowFlags.NoNav |
                                                        ImGuiWindowFlags.NoInputs;
        private const string _mainWindowName = "Party List";
        private static int MaxMemberCount = 8;

        // layout
        private Vector2 _origin;
        private Vector2 _size;
        private uint _rowCount = 0;
        private uint _colCount = 0;
        private uint _memberCount = 0;
        private bool _layoutDirty = true;

        private List<PartyHealthBar> bars;


        public PartyHudWindow(PluginConfiguration pluginConfiguration, PartyHudConfig config) {
            _pluginConfiguration = pluginConfiguration;
        
            _config = config;
            _config.PropertyChanged += OnLayoutPropertyChanged;
            _config.HealthBarsConfig.PropertyChanged += OnLayoutPropertyChanged;
            _config.SortConfig.PropertyChanged += OnSortingPropertyChanged;

            bars = new List<PartyHealthBar>(PartyHudWindow.MaxMemberCount);
            for (int i = 0; i < bars.Capacity; i++) {
                bars.Add(new PartyHealthBar(pluginConfiguration, config));
            }

            PartyManager.Instance.MembersChangedEvent += OnMembersChanged;
        }

        ~PartyHudWindow() {
            bars.Clear();

            _config.PropertyChanged -= OnLayoutPropertyChanged;
            _config.HealthBarsConfig.PropertyChanged -= OnLayoutPropertyChanged;
            _config.SortConfig.PropertyChanged -= OnSortingPropertyChanged;
            PartyManager.Instance.MembersChangedEvent -= OnMembersChanged;
        }

        private void OnLayoutPropertyChanged(object sender, PropertyChangedEventArgs args) {
            _layoutDirty = true;
        }

        private void OnSortingPropertyChanged(object sender, PropertyChangedEventArgs args) {

            if (args.PropertyName != "UseRoleColors") {
                return;
            }

            foreach (var bar in bars) {
                bar.UpdateColor();
            }
        }

        private void OnMembersChanged(object sender, EventArgs args) {
            UpdateBars(_origin);
        }

        public void UpdateBars(Vector2 origin) { 
            var memberCount = PartyManager.Instance.MemberCount;
            int row = 0;
            int col = 0;

            for (int i = 0; i < bars.Count; i++) {
                PartyHealthBar bar = bars[i];
                if (i >= memberCount) {
                    bar.Visible = false;
                    continue;
                }

                // update bar
                IGroupMember member = PartyManager.Instance.GroupMembers.ElementAt(i);
                bar.Member = member;
                bar.Position = new Vector2(
                    origin.X + _config.HealthBarsConfig.Size.X * col + _config.HealthBarsConfig.Padding.X * col,
                    origin.Y + _config.HealthBarsConfig.Size.Y * row + _config.HealthBarsConfig.Padding.Y * row
                );
                bar.Size = _config.HealthBarsConfig.Size;
                bar.Visible = true;

                // layout
                if (_config.FillRowsFirst) {
                    col = col + 1;
                    if (col >= _colCount) {
                        col = 0;
                        row = row + 1;
                    }
                }
                else {
                    row = row + 1;
                    if (row >= _rowCount) {
                        row = 0;
                        col = col + 1;
                    }
                }
            }
        }

        private void UpdateBarsPosition(Vector2 delta) {
            foreach (var bar in bars) {
                bar.Position = bar.Position + delta;
            }
        }

        public void Draw() {
            if (!_config.Enabled) {
                return;
            }

            // size and position
            ImGui.SetNextWindowPos(_config.Position, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(_config.Size, ImGuiCond.FirstUseEver);

            var windowFlags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoTitleBar;
            if (_config.Lock) {
                windowFlags |= _lockedBarFlags;
            }

            ImGui.Begin(_mainWindowName, windowFlags);
            var windowPos = ImGui.GetWindowPos();
            var windowSize = ImGui.GetWindowSize();
            UpdateConfig(windowPos, windowSize);

            var count = PartyManager.Instance.MemberCount;
            if (count < 1) {
                return;
            }

            // recalculate layout on settings or size change
            var margin = ImGui.GetWindowContentRegionMin().X;
            var origin = windowPos + new Vector2(margin, 0);
            var maxSize = windowSize - new Vector2(margin + 5, 0);

            if (_layoutDirty) {
                LayoutHelper.CalculateLayout(
                    maxSize,
                    _config.HealthBarsConfig.Size,
                    PartyManager.Instance.MemberCount,
                    _config.HealthBarsConfig.Padding,
                    _config.FillRowsFirst,
                    out _rowCount,
                    out _colCount
                );

                UpdateBars(origin);
            }
            else if (_origin != origin) {
                UpdateBarsPosition(origin - _origin);
            }

            _layoutDirty = false;
            _origin = origin;
            _memberCount = count;

            // draw
            var drawList = ImGui.GetWindowDrawList();
            for (int i = 0; i < bars.Count; i++) {
                bars[i].Draw(drawList, origin);
            }

            ImGui.End();
        }
        private void UpdateConfig(Vector2 position, Vector2 size) {
            if (position == _config.Position && size == _config.Size) {
                return;
            }

            _config.Position = position;
            _config.Size = size;
        }
    }
}
