using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using System.Numerics;

namespace DelvUI.Interface.Party
{
    public class PartyHudWindow
    {
        private PluginConfiguration pluginConfiguration;
        private const ImGuiWindowFlags LockedBarFlags = ImGuiWindowFlags.NoBackground |
                                                        ImGuiWindowFlags.NoMove |
                                                        ImGuiWindowFlags.NoResize |
                                                        ImGuiWindowFlags.NoNav |
                                                        ImGuiWindowFlags.NoInputs;
        private const string MainWindowName = "Party List";
        
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

        private List<PartyHealthBar> bars;


        public PartyHudWindow(PluginConfiguration pluginConfiguration)
        {
            this.pluginConfiguration = pluginConfiguration;
            pluginConfiguration.ConfigChangedEvent += OnConfigChanged;

            lastSize = pluginConfiguration.PartyListSize;
            lastBarSize = new Vector2(pluginConfiguration.PartyListHealthBarWidth, pluginConfiguration.PartyListHealthBarHeight);
            lastHorizontalPadding = pluginConfiguration.PartyListHorizontalPadding;
            lastVerticalPadding = pluginConfiguration.PartyListVerticalPadding;
            lastFillRowsFirst = pluginConfiguration.PartyListFillRowsFirst;
            lastUseRoleColors = pluginConfiguration.PartyListUseRoleColors;

            bars = new List<PartyHealthBar>(8);
            for (int i = 0; i < bars.Capacity; i++)
            {
                bars.Add(new PartyHealthBar(pluginConfiguration));
            }

            PartyManager.Instance.MembersChangedEvent += OnMembersChanged;
        }

        ~PartyHudWindow()
        {
            pluginConfiguration.ConfigChangedEvent -= OnConfigChanged;
            pluginConfiguration = null;
            bars.Clear();

            PartyManager.Instance.MembersChangedEvent -= OnMembersChanged;
        }

        private void OnMembersChanged(object sender, EventArgs args)
        {
            var barSize = new Vector2(pluginConfiguration.PartyListHealthBarWidth, pluginConfiguration.PartyListHealthBarHeight);
            UpdateBars(lastOrigin, barSize, lastRowCount, lastColCount, lastHorizontalPadding, lastVerticalPadding, lastFillRowsFirst);
        }

        public void UpdateBars(Vector2 origin, Vector2 barSize, uint rowCount, uint colCount, int horizontalPadding, int verticalPadding, bool fillRowsFirst)
        {
            var memberCount = PartyManager.Instance.MemberCount;
            int row = 0;
            int col = 0;

            for (int i = 0; i < bars.Count; i++)
            {
                PartyHealthBar bar = bars[i];
                if (i >= memberCount)
                {
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
                if (fillRowsFirst)
                {
                    col = col + 1;
                    if (col >= colCount)
                    {
                        col = 0;
                        row = row + 1;
                    }
                }
                else
                {
                    row = row + 1;
                    if (row >= rowCount)
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

        public void Draw()
        {
            if (!pluginConfiguration.ShowPartyList) return;

            // size and position
            ImGui.SetNextWindowPos(pluginConfiguration.PartyListPosition, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(pluginConfiguration.PartyListSize, ImGuiCond.FirstUseEver);

            var windowFlags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoTitleBar;
            if (pluginConfiguration.PartyListLocked)
            {
                windowFlags |= LockedBarFlags;
            }

            ImGui.Begin(MainWindowName, windowFlags);
            var windowPos = ImGui.GetWindowPos();
            var windowSize = ImGui.GetWindowSize();
            UpdateConfig(windowPos, windowSize);

            var count = PartyManager.Instance.MemberCount;
            if (count < 1) return;
            
            // recalculate layout on settings or size change
            var margin = ImGui.GetWindowContentRegionMin().X;
            var origin = windowPos + new Vector2(margin, 0);
            var maxSize = windowSize - new Vector2(margin + 5, 0);
            var barSize = new Vector2(pluginConfiguration.PartyListHealthBarWidth, pluginConfiguration.PartyListHealthBarHeight);
            var horizontalPadding = pluginConfiguration.PartyListHorizontalPadding;
            var verticalPadding = pluginConfiguration.PartyListVerticalPadding;
            var fillRowsFirst = pluginConfiguration.PartyListFillRowsFirst;
            var rowCount = lastRowCount;
            var colCount = lastColCount;

            if (lastSize != windowSize || lastBarSize != barSize ||
                lastHorizontalPadding != horizontalPadding || lastVerticalPadding != verticalPadding ||
                lastFillRowsFirst != fillRowsFirst)
            {
                CalculateLayoutSize(
                    maxSize,
                    barSize,
                    PartyManager.Instance.MemberCount,
                    horizontalPadding,
                    verticalPadding,
                    fillRowsFirst,
                    out rowCount,
                    out colCount
                );
            }

            if (rowCount != lastRowCount || colCount != lastColCount)
            {
                UpdateBars(lastOrigin, barSize, rowCount, colCount, horizontalPadding, verticalPadding, fillRowsFirst);
            }
            else if (lastOrigin != origin)
            {
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

            // draw
            var drawList = ImGui.GetWindowDrawList();
            for (int i = 0; i < bars.Count; i++)
            {
                bars[i].Draw(drawList, lastOrigin);
            }

            ImGui.End();
        }

        private void CalculateLayoutSize(Vector2 maxSize, Vector2 barSize, uint count, int horizontalPadding, int verticalPadding, bool fillRowsFirst, out uint rowCount, out uint colCount)
        {
            rowCount = 1;
            colCount = 1;

            if (maxSize.X < barSize.X)
            {
                colCount = count;
                return;
            } 
            else if (maxSize.Y < barSize.Y)
            {
                rowCount = count;
                return;
            }

            if (fillRowsFirst)
            {
                colCount = (uint)(maxSize.X / barSize.X);
                if (barSize.X * colCount + horizontalPadding * (colCount - 1) > maxSize.X)
                {
                    colCount = Math.Max(1, colCount - 1);
                }

                rowCount = (uint)Math.Ceiling((double)count / colCount);
            }
            else
            {
                rowCount = (uint)(maxSize.Y / barSize.Y);
                if (barSize.Y * rowCount + verticalPadding * (rowCount - 1) > maxSize.Y)
                {
                    rowCount = Math.Max(1, rowCount - 1);
                }

                colCount = (uint)Math.Ceiling((double)count / rowCount);
            }
        }

        private void UpdateConfig(Vector2 position, Vector2 size)
        {
            if (position == pluginConfiguration.PartyListPosition && size == pluginConfiguration.PartyListSize)
            {
                return;
            }

            pluginConfiguration.PartyListPosition = position;
            pluginConfiguration.PartyListSize = size;
        }

        private void OnConfigChanged(object sender, EventArgs args)
        {
            if (lastUseRoleColors == pluginConfiguration.PartyListUseRoleColors) return;

            foreach (PartyHealthBar bar in bars) 
            {
                bar.UpdateColor();
            }
        }
    }
}
