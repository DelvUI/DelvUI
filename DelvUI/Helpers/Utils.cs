using DelvUI.Interface.GeneralElements;
using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Enums;
using Colourful;

namespace DelvUI.Helpers
{
    internal static class Utils
    {
        public static GameObject? GetBattleChocobo(GameObject? player)
        {
            if (player == null)
            {
                return null;
            }

            // only the first 200 elements in the array are relevant due to the order in which SE packs data into the array
            // we do a step of 2 because its always an actor followed by its companion
            for (int i = 0; i < 200; i += 2)
            {
                GameObject? gameObject = Plugin.ObjectTable[i];

                if (gameObject == null || gameObject is not BattleNpc battleNpc)
                {
                    continue;
                }

                if (battleNpc.BattleNpcKind == BattleNpcSubKind.Chocobo && battleNpc.OwnerId == player.ObjectId)
                {
                    return gameObject;
                }
            }

            return null;
        }

        public static unsafe bool IsHostileMemory(BattleNpc npc)
        {
            if (npc == null)
            {
                return false;
            }

            return (npc.BattleNpcKind == BattleNpcSubKind.Enemy || (int)npc.BattleNpcKind == 1)
                && *(byte*)(npc.Address + 0x1980) != 0
                && *(byte*)(npc.Address + 0x193C) != 1;
        }

        public static unsafe float ActorShieldValue(GameObject? actor)
        {
            if (actor == null)
            {
                return 0f;
            }

            return Math.Min(*(int*)(actor.Address + 0x1997), 100) / 100f;
        }

        public static string DurationToString(double duration)
        {
            if (duration == 0)
            {
                return "";
            }

            var t = TimeSpan.FromSeconds(duration);

            if (t.Hours > 1)
            {
                return t.Hours + "h";
            }

            if (t.Minutes >= 5)
            {
                return t.Minutes + "m";
            }

            if (t.Minutes >= 1)
            {
                return $"{t.Minutes}:{t.Seconds:00}";
            }

            return t.Seconds.ToString();
        }

        public static PluginConfigColor ColorByHealthValue(float i, float min, float max, PluginConfigColor fullHealthColor, PluginConfigColor lowHealthColor)
        {
            float ratio = i;
            if (min > 0 || max < 1)
            {
                if (i < min)
                {
                    ratio = 0;
                }
                else if (i > max)
                {
                    ratio = 1;
                }
                else
                {
                    float range = max - min;
                    ratio = (i - min) / range;
                }
            }

            IColorConverter<RGBColor, LabColor>? _rgbToLab = new ConverterBuilder().FromRGB().ToLab().Build();
            IColorConverter<LabColor, RGBColor>? _labToRgb = new ConverterBuilder().FromLab().ToRGB().Build();

            var rgbFullHealthColor = new RGBColor(fullHealthColor.Vector.X, fullHealthColor.Vector.Y, fullHealthColor.Vector.Z);
            var rgbLowHealthColor = new RGBColor(lowHealthColor.Vector.X, lowHealthColor.Vector.Y, lowHealthColor.Vector.Z);

            LabColor rgbFullHealthLab = _rgbToLab.Convert(rgbFullHealthColor);
            LabColor rgbLowHealthLab = _rgbToLab.Convert(rgbLowHealthColor);

            float resultL = (float)((rgbFullHealthLab.L - rgbLowHealthLab.L) * ratio + rgbLowHealthLab.L);
            float resultA = (float)((rgbFullHealthLab.a - rgbLowHealthLab.a) * ratio + rgbLowHealthLab.a);
            float resultB = (float)((rgbFullHealthLab.b - rgbLowHealthLab.b) * ratio + rgbLowHealthLab.b);

            var newColorLab = new LabColor(resultL, resultA, resultB);
            RGBColor newColorLab2RGB = _labToRgb.Convert(newColorLab);

            float alpha = (fullHealthColor.Vector.W - lowHealthColor.Vector.W) * ratio + lowHealthColor.Vector.W;

            newColorLab2RGB.Clamp();

            PluginConfigColor newColor = new PluginConfigColor(new Vector4((float)newColorLab2RGB.R, (float)newColorLab2RGB.G, (float)newColorLab2RGB.B, alpha));
            return newColor;
        }

        public static PluginConfigColor ColorForActor(GameObject? actor)
        {
            if (actor == null || actor is not Character character)
            {
                return GlobalColors.Instance.NPCNeutralColor;
            }

            switch (character.ObjectKind)
            {
                // Still need to figure out the "orange" state; aggroed but not yet attacked.
                case ObjectKind.Player:
                    return GlobalColors.Instance.SafeColorForJobId(character.ClassJob.Id);

                case ObjectKind.BattleNpc when (character.StatusFlags & StatusFlags.InCombat) == StatusFlags.InCombat:
                    return GlobalColors.Instance.NPCHostileColor;

                case ObjectKind.BattleNpc:
                    if (!IsHostileMemory((BattleNpc)character))
                    {
                        return GlobalColors.Instance.NPCFriendlyColor;
                    }
                    break;
            }

            return GlobalColors.Instance.NPCNeutralColor;
        }

        public static bool HasTankInvulnerability(BattleChara actor)
        {
            System.Collections.Generic.IEnumerable<Dalamud.Game.ClientState.Statuses.Status>? tankInvulnBuff = actor.StatusList.Where(o => o.StatusId is 810 or 1302 or 409 or 1836);
            return tankInvulnBuff.Any();
        }

        public static GameObject? FindTargetOfTarget(GameObject? target, GameObject? player, ObjectTable actors)
        {
            if (target == null)
            {
                return null;
            }

            if (target.TargetObjectId == 0 && player != null && player.TargetObjectId == 0)
            {
                return player;
            }

            // only the first 200 elements in the array are relevant due to the order in which SE packs data into the array
            // we do a step of 2 because its always an actor followed by its companion
            for (int i = 0; i < 200; i += 2)
            {
                GameObject? actor = actors[i];
                if (actor?.ObjectId == target.TargetObjectId)
                {
                    return actor;
                }
            }

            return null;
        }

        public static Vector2 GetAnchoredPosition(Vector2 position, Vector2 size, DrawAnchor anchor)
        {
            switch (anchor)
            {
                case DrawAnchor.Center: return position - size / 2f;
                case DrawAnchor.Left: return position + new Vector2(0, -size.Y / 2f);
                case DrawAnchor.Right: return position + new Vector2(-size.X, -size.Y / 2f);
                case DrawAnchor.Top: return position + new Vector2(-size.X / 2f, 0);
                case DrawAnchor.TopLeft: return position;
                case DrawAnchor.TopRight: return position + new Vector2(-size.X, 0);
                case DrawAnchor.Bottom: return position + new Vector2(-size.X / 2f, -size.Y);
                case DrawAnchor.BottomLeft: return position + new Vector2(0, -size.Y);
                case DrawAnchor.BottomRight: return position + new Vector2(-size.X, -size.Y);
            }

            return position;
        }
    }
}
