using DelvUI.Config;
using DelvUI.Helpers;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;


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

        // layout
        private Vector2 lastSize;
        private uint lastRowCount = 0;
        private uint lastColCount = 0;
        private Vector2 lastOrigin;
        private Vector2 lastBarSize;
        private int lastHorizontalPadding;
        private int lastVerticalPadding;
        private bool lastFillRowsFirst;
        private bool lastUseRoleColors;
        private uint lastMemberCount = 0;

        private List<PartyHealthBar> bars;


        public PartyHudWindow(PluginConfiguration pluginConfiguration, PartyHudConfig config) {
            _pluginConfiguration = pluginConfiguration;
            pluginConfiguration.ConfigChangedEvent += OnConfigChanged;

            _config = config;

            lastSize = _config.Size;
            lastBarSize = _config.HealthBarsConfig.Size;
            lastHorizontalPadding = (int)_config.HealthBarsConfig.Padding.X;
            lastVerticalPadding = (int)_config.HealthBarsConfig.Padding.Y;
            lastFillRowsFirst = _config.FillRowsFirst;
            lastUseRoleColors = _config.SortConfig.UseRoleColors;

            bars = new List<PartyHealthBar>(8);
            for (int i = 0; i < bars.Capacity; i++) {
                bars.Add(new PartyHealthBar(pluginConfiguration, config));
            }

            PartyManager.Instance.MembersChangedEvent += OnMembersChanged;
        }

        ~PartyHudWindow() {
            _pluginConfiguration.ConfigChangedEvent -= OnConfigChanged;
            _pluginConfiguration = null;
            bars.Clear();

            PartyManager.Instance.MembersChangedEvent -= OnMembersChanged;
        }

        private void OnMembersChanged(object sender, EventArgs args) {
            UpdateBars(lastOrigin, _config.HealthBarsConfig.Size, lastRowCount, lastColCount, lastHorizontalPadding, lastVerticalPadding, lastFillRowsFirst);
        }

        public void UpdateBars(Vector2 origin, Vector2 barSize, uint rowCount, uint colCount, int horizontalPadding, int verticalPadding, bool fillRowsFirst) {
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
                    origin.X + barSize.X * col + horizontalPadding * col,
                    origin.Y + barSize.Y * row + verticalPadding * row
                );
                bar.Size = barSize;
                bar.Visible = true;

                // layout
                if (fillRowsFirst) {
                    col = col + 1;
                    if (col >= colCount) {
                        col = 0;
                        row = row + 1;
                    }
                }
                else {
                    row = row + 1;
                    if (row >= rowCount) {
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
            var barSize = _config.HealthBarsConfig.Size;
            var horizontalPadding = (int)_config.HealthBarsConfig.Padding.X;
            var verticalPadding = (int)_config.HealthBarsConfig.Padding.Y;
            var fillRowsFirst = _config.FillRowsFirst;
            var rowCount = lastRowCount;
            var colCount = lastColCount;

            if (lastSize != windowSize || lastBarSize != barSize || lastMemberCount < count ||
                lastHorizontalPadding != horizontalPadding || lastVerticalPadding != verticalPadding ||
                lastFillRowsFirst != fillRowsFirst) {
                LayoutHelper.CalculateLayout(
                    maxSize,
                    barSize,
                    PartyManager.Instance.MemberCount,
                    horizontalPadding,
                    verticalPadding,
                    fillRowsFirst,
                    out rowCount,
                    out colCount
                );

                UpdateBars(lastOrigin, barSize, rowCount, colCount, horizontalPadding, verticalPadding, fillRowsFirst);
            }
            else if (rowCount != lastRowCount || colCount != lastColCount) {
                UpdateBars(lastOrigin, barSize, rowCount, colCount, horizontalPadding, verticalPadding, fillRowsFirst);
            }
            else if (lastOrigin != origin) {
                UpdateBarsPosition(origin - lastOrigin);
            }

            // save values
            lastSize = windowSize;
            lastOrigin = origin;
            lastBarSize = barSize;
            lastHorizontalPadding = horizontalPadding;
            lastVerticalPadding = verticalPadding;
            lastFillRowsFirst = fillRowsFirst;
            lastRowCount = rowCount;
            lastColCount = colCount;
            lastMemberCount = count;

            // draw
            var drawList = ImGui.GetWindowDrawList();
            for (int i = 0; i < bars.Count; i++) {
                bars[i].Draw(drawList, lastOrigin);
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

        private void OnConfigChanged(object sender, EventArgs args) {
            if (lastUseRoleColors == _config.SortConfig.UseRoleColors) {
                return;
            }
            lastUseRoleColors = _config.SortConfig.UseRoleColors;

            foreach (PartyHealthBar bar in bars) {
                bar.UpdateColor();
            }
        }
    }
}
