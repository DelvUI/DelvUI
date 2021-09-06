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
    public class StatusEffectsListHud: HudElement, IHudElementWithActor
    {
        private StatusEffectsListConfig Config => (StatusEffectsListConfig)_config;

        private uint _rowCount;
        private uint _colCount;
        private GrowthDirections _lastGrowthDirections;

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

            var sheet = Plugin.InterfaceInstance.Data.GetExcelSheet<Status>();
            if (sheet == null)
            {
                return list;
            }

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

                if (!Config.ShowBuffs && row.Category == 1)
                {
                    continue;
                }

                if (!Config.ShowDebuffs && row.Category != 1)
                {
                    continue;
                }

                if (!Config.ShowPermanentEffects && row.IsPermanent)
                {
                    continue;
                }

                list.Add(new StatusEffectData(status, row));
            }

            if (filterBuffs.Count == 0)
            {
                return list;
            }

            // Always adhere to the priority of buffs set by filterBuffs.
            var toReturn = new List<StatusEffectData>();
            foreach (var buffId in filterBuffs)
            {
                var idx = list.FindIndex(s => (uint)s.StatusEffect.EffectId == buffId);
                if (idx >= 0)
                {
                    toReturn.Add(list[idx]);
                }
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

            // calculate layout
            var list = StatusEffectsData(filterStatusEffects);
            var count = CalculateLayout(list);

            // validate growth directions
            var growthDirections = Config.GrowthDirections;
            if (growthDirections != _lastGrowthDirections)
            {
                growthDirections = ValidateGrowthDirections(growthDirections);
                _lastGrowthDirections = growthDirections;
            }

            // draw area
            var drawList = ImGui.GetWindowDrawList();
            var position = origin + Config.Position;

            if (Config.ShowArea)
            {
                var area = Config.Size;

                if ((growthDirections & GrowthDirections.Left) != 0)
                {
                    area.X = -area.X;
                }

                if ((growthDirections & GrowthDirections.Up) != 0)
                {
                    area.Y = -area.Y;
                }

                drawList.AddRectFilled(position, position + area, 0x88000000);
            }

            var row = 0;
            var col = 0;

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
