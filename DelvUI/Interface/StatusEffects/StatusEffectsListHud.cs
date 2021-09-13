using Dalamud.Game.ClientState.Actors;
using Dalamud.Game.ClientState.Structs;
using Dalamud.Plugin;
using DelvUI.Helpers;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Numerics;
using Actor = Dalamud.Game.ClientState.Actors.Types.Actor;

namespace DelvUI.Interface.StatusEffects
{
    public class StatusEffectsListHud : HudElement, IHudElementWithActor
    {
        private StatusEffectsListConfig Config => (StatusEffectsListConfig)_config;

        private uint _rowCount;
        private uint _colCount;
        private GrowthDirections _lastGrowthDirections;
        private bool _showingTooltip = false;

        public Actor Actor { get; set; } = null;

        public StatusEffectsListHud(string ID, StatusEffectsListConfig config) : base(ID, config)
        {
        }

        private uint CalculateLayout(List<StatusEffectData> list)
        {
            var effectCount = (uint)list.Count;
            var count = Config.Limit >= 0 ? Math.Min((uint)Config.Limit, effectCount) : effectCount;

            if (Actor == null || count <= 0)
            {
                return 0;
            }

            LayoutHelper.CalculateLayout(
                Config.Size,
                Config.IconConfig.Size,
                count,
                (int)Config.IconPadding.X,
                (int)Config.IconPadding.Y,
                Config.FillRowsFirst,
                out _rowCount,
                out _colCount
            );

            return count;
        }

        private List<StatusEffectData> StatusEffectsData(List<uint> filterBuffs)
        {
            var list = new List<StatusEffectData>();
            if (Actor == null)
            {
                return list;
            }

            var effectCount = Actor.StatusEffects.Length;
            if (effectCount == 0)
            {
                return list;
            }

            var sheet = Plugin.DataManager.GetExcelSheet<Status>();
            if (sheet == null)
            {
                return list;
            }

            var player = Plugin.ClientState.LocalPlayer;

            for (var i = 0; i < effectCount; i++)
            {
                var status = Actor.StatusEffects[i];

                if (status.EffectId <= 0)
                {
                    continue;
                }

                var row = sheet.GetRow((uint)status.EffectId);
                if (row == null)
                {
                    continue;
                }

                // buffs
                if (!Config.ShowBuffs && row.Category == 1)
                {
                    continue;
                }

                // debuffs
                if (!Config.ShowDebuffs && row.Category != 1)
                {
                    continue;
                }

                // permanent
                if (!Config.ShowPermanentEffects && row.IsPermanent)
                {
                    continue;
                }

                // only mine
                if (Config.ShowOnlyMine && player?.ActorId != status.OwnerId)
                {
                    continue;
                }

                list.Add(new StatusEffectData(status, row));
            }

            // filters
            var toReturn = list;
            if (filterBuffs.Count > 0)
            {
                toReturn = new List<StatusEffectData>();

                foreach (var buffId in filterBuffs)
                {
                    var idx = list.FindIndex(s => (uint)s.StatusEffect.EffectId == buffId);
                    if (idx >= 0)
                    {
                        toReturn.Add(list[idx]);
                    }
                }
            }

            // show mine first
            if (Config.ShowMineFirst && player != null)
            {
                toReturn.Sort((a, b) =>
                {
                    bool isAFromPlayer = a.StatusEffect.OwnerId == player.ActorId;
                    bool isBFromPlayer = b.StatusEffect.OwnerId == player.ActorId;

                    if (isAFromPlayer && !isBFromPlayer)
                    {
                        return -1;
                    }
                    else if (!isAFromPlayer && isBFromPlayer)
                    {
                        return 1;
                    }

                    return 0;
                });
            }

            return toReturn;
        }

        public override void Draw(Vector2 origin)
        {
            Draw(origin, new List<uint>());
        }

        public void Draw(Vector2 origin, List<uint> filterStatusEffects)
        {
            if (!Config.Enabled || Actor == null)
            {
                return;
            }

            if (Actor.ObjectKind != ObjectKind.Player && Actor.ObjectKind != ObjectKind.BattleNpc)
            {
                return;
            }

            // calculate layout
            var list = StatusEffectsData(filterStatusEffects);
            var count = CalculateLayout(list);

            // validate growth directions
            var growthDirections = Config.GetGrowthDirections();
            if (growthDirections != _lastGrowthDirections)
            {
                growthDirections = ValidateGrowthDirections(growthDirections);
                _lastGrowthDirections = growthDirections;
            }

            // window
            ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoTitleBar;
            //windowFlags |= ImGuiWindowFlags.NoMove;
            windowFlags |= ImGuiWindowFlags.NoDecoration;
            windowFlags |= ImGuiWindowFlags.NoBackground;

            // imgui clips the left and right borders inside windows for some reason
            // we make the window bigger so the actual drawable size is the expected one
            var position = origin + Config.Position;
            var margin = new Vector2(4, 0);
            var windowPos = CalculateStartPosition(position, Config.Size, growthDirections) - margin;
            ImGui.SetNextWindowPos(windowPos, ImGuiCond.Always);
            ImGui.SetNextWindowSize(Config.Size + margin * 2);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
            ImGui.Begin("statusEffecdtList " + ID, windowFlags);
            var drawList = ImGui.GetWindowDrawList();

            // draw area
            if (Config.ShowArea)
            {
                var areaStartPos = windowPos + margin;
                drawList.AddRectFilled(areaStartPos, areaStartPos + Config.Size, 0x88000000);
            }

            var row = 0;
            var col = 0;
            var showingTooltip = false;

            for (var i = 0; i < count; i++)
            {
                var statusEffectData = list[i];

                CalculateAxisDirections(growthDirections, list.Count, out var direction, out var offset);
                var iconPos = new Vector2(
                    position.X + offset.X + Config.IconConfig.Size.X * col * direction.X + Config.IconPadding.X * col * direction.X,
                    position.Y + offset.Y + Config.IconConfig.Size.Y * row * direction.Y + Config.IconPadding.Y * row * direction.Y
                );

                // draw
                StatusEffectIconDrawHelper.DrawStatusEffectIcon(drawList, iconPos, statusEffectData, Config.IconConfig);

                // tooltip
                if (ImGui.IsMouseHoveringRect(iconPos, iconPos + Config.IconConfig.Size))
                {
                    TooltipsHelper.Instance.ShowTooltipOnCursor(statusEffectData.Data.Description, statusEffectData.Data.Name);
                    showingTooltip = true;

                    // remove buff on right click
                    bool isFromPlayer = statusEffectData.StatusEffect.OwnerId == Plugin.ClientState.LocalPlayer?.ActorId;

                    if (statusEffectData.Data.Category == 1 && isFromPlayer && ImGui.GetIO().MouseClicked[1])
                    {
                        ChatHelper.SendChatMessage("/statusoff \"" + statusEffectData.Data.Name + "\"");
                    }
                }

                // rows / columns
                if (Config.FillRowsFirst)
                {
                    col += 1;
                    if (col >= _colCount)
                    {
                        col = 0;
                        row += 1;
                    }
                }
                else
                {
                    row += 1;
                    if (row >= _rowCount)
                    {
                        row = 0;
                        col += 1;
                    }
                }
            }

            ImGui.End();
            ImGui.PopStyleVar();

            if (_showingTooltip && !showingTooltip)
            {
                TooltipsHelper.Instance.RemoveTooltip();
            }
            _showingTooltip = showingTooltip;
        }

        private GrowthDirections ValidateGrowthDirections(GrowthDirections directions)
        {
            if (directions == 0)
            {
                return GrowthDirections.Right | GrowthDirections.Down;
            }

            // validate Right & Left
            if ((directions & GrowthDirections.Right) == 0 && (directions & GrowthDirections.Left) == 0)
            {
                return directions | GrowthDirections.Right;
            }

            if ((directions & GrowthDirections.Right) != 0 && (directions & GrowthDirections.Left) != 0)
            {
                return directions & ~GrowthDirections.Left;
            }

            // validate Down & Up
            if ((directions & GrowthDirections.Down) == 0 && (directions & GrowthDirections.Up) == 0)
            {
                return directions | GrowthDirections.Down;
            }

            if ((directions & GrowthDirections.Down) != 0 && (directions & GrowthDirections.Up) != 0)
            {
                return directions & ~GrowthDirections.Up;
            }

            // validate Out
            if ((directions & GrowthDirections.Out) != 0 && (directions & GrowthDirections.Left) != 0)
            {
                return GrowthDirections.Out | GrowthDirections.Right;
            }
            if ((directions & GrowthDirections.Out) != 0 && (directions & GrowthDirections.Up) != 0)
            {
                return GrowthDirections.Out | GrowthDirections.Down;
            }

            return directions;
        }

        private void CalculateAxisDirections(GrowthDirections growthDirections, int elementCount, out Vector2 direction, out Vector2 offset)
        {
            if ((growthDirections & GrowthDirections.Out) != 0)
            {
                direction.X = 1;
                direction.Y = 1;
                offset.X = (growthDirections & GrowthDirections.Right) != 0 ? -1 * (Config.IconConfig.Size.X + Config.IconPadding.X) * elementCount / 2 : 0;
                offset.Y = (growthDirections & GrowthDirections.Down) != 0 ? -1 * (Config.IconConfig.Size.Y + Config.IconPadding.Y) * elementCount / 2 : 0;
            }
            else
            {
                direction.X = (growthDirections & GrowthDirections.Right) != 0 ? 1 : -1;
                direction.Y = (growthDirections & GrowthDirections.Down) != 0 ? 1 : -1;
                offset.X = direction.X == 1 ? 0 : -Config.IconConfig.Size.X;
                offset.Y = direction.Y == 1 ? 0 : -Config.IconConfig.Size.Y;
            }
        }

        private Vector2 CalculateStartPosition(Vector2 position, Vector2 size, GrowthDirections growthDirections)
        {
            var area = size;
            if ((growthDirections & GrowthDirections.Left) != 0)
            {
                area.X = -area.X;
            }

            if ((growthDirections & GrowthDirections.Up) != 0)
            {
                area.Y = -area.Y;
            }

            var endPos = position + area;
            var startPos = position;

            if (endPos.X < position.X)
            {
                startPos.X = endPos.X;
            }

            if (endPos.Y < position.Y)
            {
                startPos.Y = endPos.Y;
            }

            return startPos;
        }
    }

    public struct StatusEffectData
    {
        public StatusEffect StatusEffect;
        public readonly Status Data;

        public StatusEffectData(StatusEffect statusEffect, Status data)
        {
            StatusEffect = statusEffect;
            Data = data;
        }
    }

    [Flags]
    public enum GrowthDirections : short
    {
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8,
        Out = 16,
    }
}
