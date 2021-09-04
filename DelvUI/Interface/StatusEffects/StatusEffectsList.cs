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

    public class StatusEffectsList
    {
        private readonly DalamudPluginInterface _pluginInterface;
        public readonly StatusEffectsListConfig Config;
        private uint _colCount;
        private GrowthDirections _lastGrowthDirections;
        private GrowthDirections _lastValidGrowthDirections = GrowthDirections.Left | GrowthDirections.Down;

        private uint _rowCount;

        public Actor Actor = null;
        public Vector2 Center = Vector2.Zero;

        public StatusEffectsList(DalamudPluginInterface pluginInterface, StatusEffectsListConfig config)
        {
            _pluginInterface = pluginInterface;
            Config = config;
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

        private uint CalculateLayout(List<StatusEffectData> list)
        {
            var effectCount = (uint)list.Count;
            var count = Config.Limit >= 0 ? Math.Min((uint)Config.Limit, effectCount) : effectCount;

            if (Actor == null || count <= 0)
            {
                return 0;
            }

            LayoutHelper.CalculateLayout(
                Config.MaxSize,
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

            var sheet = _pluginInterface.Data.GetExcelSheet<Status>();

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

        public void Draw()
        {
            Draw(new List<uint>());
        }

        public void Draw(List<uint> filterBuffs)
        {
            if (!Config.Enabled || Actor == null)
            {
                return;
            }

            // calculate layout
            var list = StatusEffectsData(filterBuffs);
            var count = CalculateLayout(list);

            // validate growth directions
            var directions = Config.GrowthDirections;

            if (directions != _lastGrowthDirections)
            {
                _lastGrowthDirections = directions;
                directions = ValidateGrowthDirections(directions);
                _lastValidGrowthDirections = directions;
            }

            // draw area
            var drawList = ImGui.GetWindowDrawList();
            var origin = Center + Config.Position;

            if (Config.ShowArea)
            {
                var area = Config.MaxSize;

                if ((directions & GrowthDirections.Left) != 0)
                {
                    area.X = -area.X;
                }

                if ((directions & GrowthDirections.Up) != 0)
                {
                    area.Y = -area.Y;
                }

                drawList.AddRectFilled(origin, origin + area, 0x88000000);
            }

            var row = 0;
            var col = 0;

            for (var i = 0; i < count; i++)
            {
                var statusEffectData = list[i];
                int directionX;
                int directionY;
                float offsetX;
                float offsetY;
                if ((directions & GrowthDirections.Out) != 0)
                {
                    directionX = 1;
                    directionY = 1;
                    offsetX = (directions & GrowthDirections.Right) != 0 ? -1 * (Config.IconConfig.Size.X + Config.IconPadding.X) * list.Count / 2 : 0;
                    offsetY = (directions & GrowthDirections.Down) != 0 ? -1 * (Config.IconConfig.Size.Y + Config.IconPadding.Y) * list.Count / 2 : 0;
                }
                else
                {
                    directionX = (directions & GrowthDirections.Right) != 0 ? 1 : -1;
                    directionY = (directions & GrowthDirections.Down) != 0 ? 1 : -1;
                    offsetX = directionX == 1 ? 0 : -Config.IconConfig.Size.X;
                    offsetY = directionY == 1 ? 0 : -Config.IconConfig.Size.Y;
                }

                var pos = new Vector2(
                    origin.X + offsetX + Config.IconConfig.Size.X * col * directionX + Config.IconPadding.X * col * directionX,
                    origin.Y + offsetY + Config.IconConfig.Size.Y * row * directionY + Config.IconPadding.Y * row * directionY
                );

                // draw
                StatusEffectIconDrawHelper.DrawStatusEffectIcon(drawList, pos, statusEffectData, Config.IconConfig);

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
    }
}
