using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Structs;
using DelvUI.Helpers;
using Actor = Dalamud.Game.ClientState.Actors.Types.Actor;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUI.Interface.StatusEffects
{
    public struct StatusEffectData
    {
        public StatusEffect statusEffect;
        public Status data;

        public StatusEffectData(StatusEffect statusEffect, Status data)
        {
            this.statusEffect = statusEffect;
            this.data = data;
        }
    }

    public enum GrowthDirections: short
    {
        UP = 1,
        DOWN = 2,
        LEFT = 4,
        RIGHT = 8
    }

    public class StatusEffectsList
    {
        private DalamudPluginInterface _pluginInterface;
        private GrowthDirections _lastGrowthDirections;
        private GrowthDirections _lastValidGrowthDirections = GrowthDirections.LEFT | GrowthDirections.DOWN;

        private uint rowCount = 0;
        private uint colCount = 0;

        public Actor Actor = null;
        public StatusEffectsListConfig Config;
        public Vector2 Center = Vector2.Zero;

        public StatusEffectsList(DalamudPluginInterface pluginInterface, StatusEffectsListConfig config)
        {
            _pluginInterface = pluginInterface;
            Config = config;
        }

        private GrowthDirections ValidateGrowthDirections(GrowthDirections directions)
        {
            if (directions == 0) return GrowthDirections.RIGHT | GrowthDirections.DOWN;

            // validate RIGHT & LEFT
            if ((directions & GrowthDirections.RIGHT) == 0 && (directions & GrowthDirections.LEFT) == 0)
            {
                return directions | GrowthDirections.RIGHT;
            } 
            if ((directions & GrowthDirections.RIGHT) != 0 && (directions & GrowthDirections.LEFT) != 0)
            {
                return directions & ~GrowthDirections.LEFT;
            }

            // validate DOWN & UP
            if ((directions & GrowthDirections.DOWN) == 0 && (directions & GrowthDirections.UP) == 0)
            {
                return directions | GrowthDirections.DOWN;
            }
            if ((directions & GrowthDirections.DOWN) != 0 && (directions & GrowthDirections.UP) != 0)
            {
                return directions & ~GrowthDirections.UP;
            }

            return directions;
        }

        private uint CalculateLayout(List<StatusEffectData> list)
        {
            var effectCount = (uint)list.Count;
            uint count = Config.Limit >= 0 ? Math.Min((uint)Config.Limit, effectCount) : effectCount;

            if (Actor == null || count <= 0)
            {
                return 0;
            }

            LayoutHelper.CalculateLayout(
                Config.MaxSize, 
                Config.IconConfig.Size, 
                (uint)count,
                (int)Config.IconPadding.X, 
                (int)Config.IconPadding.Y,
                Config.FillRowsFirst,
                out rowCount, 
                out colCount
            );

            return count;
        }

        private List<StatusEffectData> StatusEffectsData()
        {
            var list = new List<StatusEffectData>();
            if (Actor == null) return list;

            var effectCount = Actor.StatusEffects.Length;
            if (effectCount == 0) return list;

            var sheet = _pluginInterface.Data.GetExcelSheet<Status>();
            if (sheet == null) return list;

            for (int i = 0; i < effectCount; i++)
            {
                var status = Actor.StatusEffects[i];
                if (status.EffectId <= 0) continue;

                var row = sheet.GetRow((uint)status.EffectId);
                if (row == null) continue;

                if (!Config.ShowBuffs && row.Category == 1) continue;
                if (!Config.ShowDebuffs && row.Category != 1) continue;
                if (!Config.ShowPermanentEffects && row.IsPermanent) continue;

                list.Add(new StatusEffectData(status, row));
            }

            return list;
        }

        public void Draw()
        {
            if (!Config.Enabled || Actor == null) return;

            // calculate layout
            var list = StatusEffectsData();
            var count = CalculateLayout(list);

            // validate growth directions
            GrowthDirections directions = (GrowthDirections)Config.GrowthDirections;
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
                if ((directions & GrowthDirections.LEFT) != 0) area.X = -area.X;
                if ((directions & GrowthDirections.UP) != 0) area.Y = -area.Y;

                drawList.AddRectFilled(origin, origin + area, 0x88000000);
            }

            int row = 0;
            int col = 0;

            for (int i = 0; i < count; i++)
            {
                var statusEffectData = list[i];

                // calculate icon position
                int directionX = (directions & GrowthDirections.RIGHT) != 0 ? 1 : -1;
                int directionY = (directions & GrowthDirections.DOWN) != 0 ? 1 : -1;
                float offsetX = directionX == 1 ? 0 : -Config.IconConfig.Size.X;
                float offsetY = directionY == 1 ? 0 : -Config.IconConfig.Size.Y;

                var pos = new Vector2(
                    origin.X + offsetX + Config.IconConfig.Size.X * col * directionX + Config.IconPadding.X * col * directionX,
                    origin.Y + offsetY + Config.IconConfig.Size.Y * row * directionY + Config.IconPadding.Y * row * directionY
                );

                // draw
                StatusEffectIconDrawHelper.DrawStatusEffectIcon(drawList, pos, statusEffectData, Config.IconConfig);
                
                // rows / columns
                if (Config.FillRowsFirst)
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
    }

    [Serializable]
    public class StatusEffectsListConfig
    {
        public bool Enabled = true;
        //public int PositionX;
        //public int PositionY;
        //public int MaxSizeWidth = 340;
        //public int MaxSizeHeight = 100;
        //public int IconPaddingHorizontal = 2;
        //public int IconPaddingVertical = 2;
        public Vector2 Position;
        public Vector2 MaxSize = new Vector2(340, 100);
        public Vector2 IconPadding = new Vector2(2, 2);
        public bool ShowArea = false;
        public StatusEffectIconConfig IconConfig = new StatusEffectIconConfig();
        public bool ShowBuffs;
        public bool ShowDebuffs;
        public bool ShowPermanentEffects;
        public bool FillRowsFirst = true;
        public int Limit = -1;
        public short GrowthDirections;

        public StatusEffectsListConfig(Vector2 position, bool showBuffs, bool showDebuffs, bool showPermanentEffects, GrowthDirections growthDirections)
        {
            Position = position;
            ShowBuffs = showBuffs;
            ShowDebuffs = showDebuffs;
            ShowPermanentEffects = showPermanentEffects;
            GrowthDirections = (short)growthDirections;
        }
    }

    [Serializable]
    public class StatusEffectIconConfig
    {
        public Vector2 Size = new Vector2(40, 40);
        public bool ShowDurationText = true;
        public bool ShowStacksText = true;

        public bool ShowBorder = true;
        public int BorderThickness = 1;
        public Vector4 BorderColor = new Vector4(0f / 255f, 0 / 255f, 0 / 255f, 100f / 100f);

        public bool ShowDispellableBorder = true;
        public int DispellableBorderThickness = 2;
        public Vector4 DispellableBorderColor = new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f);
    }
}
