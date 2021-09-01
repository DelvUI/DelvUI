using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using System.Numerics;
using DelvUI.Interface.Party;
using DelvUI.Helpers;
using Lumina.Excel.GeneratedSheets;

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
        private int HorizonalPadding => pluginConfiguration.PartyListHorizontalPadding;
        private int VerticalPadding => pluginConfiguration.PartyListVerticalPadding;
        private bool FillRowsFirst => pluginConfiguration.PartyListFillRowsFirst;


        public PartyHudWindow(PluginConfiguration pluginConfiguration)
        {
            this.pluginConfiguration = pluginConfiguration;
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

            // draw window
            var margin = ImGui.GetWindowContentRegionMin().X;
            var origin = windowPos + new Vector2(margin, 0);
            var maxSize = windowSize - new Vector2(margin + 5, 0);
            var barSize = new Vector2(pluginConfiguration.PartyListHealthBarWidth, pluginConfiguration.PartyListHealthBarHeight);
            var drawList = ImGui.GetWindowDrawList();

            CalculateLayout(
                maxSize,
                barSize,
                PartyManager.Instance.MemberCount,
                HorizonalPadding,
                VerticalPadding,
                FillRowsFirst,
                out uint rowCount,
                out uint colCount
            );

            int row = 0;
            int col = 0;
            for (int i = 0; i < count; i++)
            {
                IGroupMember member = PartyManager.Instance.GroupMembers.ElementAt(i);

                // color
                Dictionary<string, uint> colors = null;
                if (pluginConfiguration.PartyListUseRoleColors)
                {
                    if (JobsHelper.isJobTank(member.JobId))
                    {
                        colors = pluginConfiguration.PartyListColorMap["tank"];
                    }
                    else if (JobsHelper.isJobHealer(member.JobId))
                    {
                        colors = pluginConfiguration.PartyListColorMap["healer"];
                    }
                    else if (JobsHelper.isJobDPS(member.JobId))
                    {
                        colors = pluginConfiguration.PartyListColorMap["dps"];
                    }
                    else
                    {
                        colors = pluginConfiguration.PartyListColorMap["generic_role"];
                    }
                }
                else
                {
                    pluginConfiguration.JobColorMap.TryGetValue(member.JobId, out colors);
                    colors ??= pluginConfiguration.NPCColorMap["friendly"];
                }

                // bg
                var isClose = member.MaxHP > 0;
                var cursorPos = new Vector2(
                    origin.X + barSize.X * col + HorizonalPadding * col,
                    origin.Y + barSize.Y * row + VerticalPadding * row
                );

                drawList.AddRectFilled(cursorPos, cursorPos + barSize, isClose ? (uint)0x66000000 : (uint)0x22000000);

                // hp
                if (isClose)
                {
                    var scale = member.MaxHP > 0 ? (float)member.HP / (float)member.MaxHP : 1;
                    var fillPos = cursorPos;
                    var fillSize = new Vector2(Math.Max(1, barSize.X * scale), barSize.Y / 2f);
                    drawList.AddRectFilledMultiColor(
                        fillPos, fillPos + fillSize,
                        colors["gradientLeft"], colors["gradientLeft"], colors["gradientRight"], colors["gradientRight"]
                    );

                    fillPos.Y = fillPos.Y + barSize.Y / 2f;
                    drawList.AddRectFilledMultiColor(
                        fillPos, fillPos + fillSize,
                        colors["gradientRight"], colors["gradientRight"], colors["gradientLeft"], colors["gradientLeft"]
                    );
                }

                // shield
                if (pluginConfiguration.PartyListShieldEnabled)
                {
                    if (pluginConfiguration.PartyListShieldFillHealthFirst && member.MaxHP > 0)
                    {
                        DrawHelper.DrawShield(member.Shield, (float)member.HP / member.MaxHP, cursorPos, barSize, 
                            pluginConfiguration.PartyListShieldHeight, !pluginConfiguration.PartyListShieldHeightPixels, 
                            pluginConfiguration.PartyListColorMap["shield"]);
                    }
                    else
                    {
                        DrawHelper.DrawShield(member.Shield, cursorPos, barSize,
                            pluginConfiguration.PartyListShieldHeight, !pluginConfiguration.PartyListShieldHeightPixels,
                            pluginConfiguration.PartyListColorMap["shield"]);
                    }                    
                }

                // buffs / debuffs
                var statusEffects = member.StatusEffects;
                var pos = cursorPos + barSize - origin + new Vector2(6, 1);

                for (int s = 0; s < statusEffects.Length; s++)
                {
                    var id = (uint)statusEffects[s].EffectId;
                    if (id == 0) continue;

                    var texture = TexturesCache.Instance.GetTexture<Status>(id, (uint)Math.Max(0, statusEffects[s].StackCount - 1));
                    if (texture == null) continue;

                    var size = new Vector2(texture.Width, texture.Height);
                    ImGui.SetCursorPos(pos - size);
                    ImGui.Image(texture.ImGuiHandle, size);

                    pos.X = pos.X - size.X - 1;
                }

                // name
                var actor = member.GetActor();
                var name = member.Name.Abbreviate() ?? "";
                
                if (actor != null)
                {
                    name = TextTags.GenerateFormattedTextFromTags(actor, pluginConfiguration.PartyListHealthBarText);
                }

                var textSize = ImGui.CalcTextSize(name);
                var textPos = new Vector2(cursorPos.X + barSize.X / 2f - textSize.X / 2f, cursorPos.Y + barSize.Y / 2f - textSize.Y / 2f);
                drawList.AddText(textPos, actor == null ? 0x44FFFFFF : 0xFFFFFFFF, name);

                // border
                //drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                // layout
                if (FillRowsFirst)
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

            ImGui.End();
        }

        private void CalculateLayout(Vector2 maxSize, Vector2 barSize, uint count, int horizontalPadding, int verticalPadding, bool fillRowsFirst, out uint rowCount, out uint colCount)
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
    }
}
