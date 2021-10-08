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
using System.Text.RegularExpressions;

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
            for (var i = 0; i < 200; i += 2)
            {
                var gameObject = Plugin.ObjectTable[i];

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

        public static PluginConfigColor ColorByHealthValue(float i, float min, float max, PluginConfigColor fullHealthColor, PluginConfigColor lowHealthColor, BlendMode blendMode)
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
                    var range = max - min;
                    ratio = (i - min) / range;
                }
            }

            //build our converters
            var _rgbToLab = new ConverterBuilder().FromRGB().ToLab().Build();
            var _labToRgb = new ConverterBuilder().FromLab().ToRGB().Build();

            var _rgbToXyz = new ConverterBuilder().FromRGB(RGBWorkingSpaces.sRGB).ToXYZ(Illuminants.D65).Build();
            var _xyzToRgb = new ConverterBuilder().FromXYZ(Illuminants.D65).ToRGB(RGBWorkingSpaces.sRGB).Build();

            var _rgbToLChuv = new ConverterBuilder().FromRGB().ToLChuv().Build();
            var _lchuvToRgb = new ConverterBuilder().FromLChuv().ToRGB().Build();

            var rgbFullHealthColor = new RGBColor(fullHealthColor.Vector.X, fullHealthColor.Vector.Y, fullHealthColor.Vector.Z);
            var rgbLowHealthColor = new RGBColor(lowHealthColor.Vector.X, lowHealthColor.Vector.Y, lowHealthColor.Vector.Z);

            //convert RGB to LAB
            var rgbFullHealthLab = _rgbToLab.Convert(rgbFullHealthColor);
            var rgbLowHealthLab = _rgbToLab.Convert(rgbLowHealthColor);

            //convert RGB to XYZ
            var rgbFullHealthXyz = _rgbToXyz.Convert(rgbFullHealthColor);
            var rgbLowHealthXyz = _rgbToXyz.Convert(rgbLowHealthColor);

            //convert RGB to LChuv
            var rgbFullHealthLChuv = _rgbToLChuv.Convert(rgbFullHealthColor);
            var rgbLowHealthLChuv = _rgbToLChuv.Convert(rgbLowHealthColor);

            //XYZ interpolation results
            float resultX = (float)((rgbFullHealthXyz.X - rgbLowHealthXyz.X) * ratio + rgbLowHealthXyz.X);
            float resultY = (float)((rgbFullHealthXyz.Y - rgbLowHealthXyz.Y) * ratio + rgbLowHealthXyz.Y);
            float resultZ = (float)((rgbFullHealthXyz.Z - rgbLowHealthXyz.Z) * ratio + rgbLowHealthXyz.Z);

            //LAB interpolation results
            float resultL = (float)((rgbFullHealthLab.L - rgbLowHealthLab.L) * ratio + rgbLowHealthLab.L);
            float resultA = (float)((rgbFullHealthLab.a - rgbLowHealthLab.a) * ratio + rgbLowHealthLab.a);
            float resultB = (float)((rgbFullHealthLab.b - rgbLowHealthLab.b) * ratio + rgbLowHealthLab.b);

            //RGB interpolation results
            float resultR = (float)((fullHealthColor.Vector.X - lowHealthColor.Vector.X) * ratio + lowHealthColor.Vector.X);
            float resultG = (float)((fullHealthColor.Vector.Y - lowHealthColor.Vector.Y) * ratio + lowHealthColor.Vector.Y);
            float resultb = (float)((fullHealthColor.Vector.Z - lowHealthColor.Vector.Z) * ratio + lowHealthColor.Vector.Z);

            //LChuv interpolation results
            float resultl = (float)((rgbFullHealthLChuv.L - rgbLowHealthLChuv.L) * ratio + rgbLowHealthLChuv.L);
            float resultc = (float)((rgbFullHealthLChuv.C - rgbLowHealthLChuv.C) * ratio + rgbLowHealthLChuv.C);
            float resulth = (float)((rgbFullHealthLChuv.h - rgbLowHealthLChuv.h) * ratio + rgbLowHealthLChuv.h);

            var newColorLab = new LabColor(resultL, resultA, resultB);
            var newColorXYZ = new XYZColor(resultX, resultY, resultZ);
            var newColorRGB = new RGBColor(resultR, resultG, resultb);
            var newColorLChuv = new LChuvColor(resultl, resultc, resulth);

            var newColorLab2RGB = _labToRgb.Convert(newColorLab);
            var newColorXYZ2RGB = _xyzToRgb.Convert(newColorXYZ);
            var newColorLChuv2RGB = _lchuvToRgb.Convert(newColorLChuv);

            float alpha = (fullHealthColor.Vector.W - lowHealthColor.Vector.W) * ratio + lowHealthColor.Vector.W;

            newColorLab2RGB.Clamp();
            newColorXYZ2RGB.Clamp();
            newColorLChuv2RGB.Clamp();

            switch (blendMode)
            {
                case BlendMode.CIELAB: return new PluginConfigColor(new Vector4((float)newColorLab2RGB.R, (float)newColorLab2RGB.G, (float)newColorLab2RGB.B, alpha));
                case BlendMode.XYZ: return new PluginConfigColor(new Vector4((float)newColorXYZ2RGB.R, (float)newColorXYZ2RGB.G, (float)newColorXYZ2RGB.B, alpha));
                case BlendMode.RGB: return new PluginConfigColor(new Vector4((float)newColorRGB.R, (float)newColorRGB.G, (float)newColorRGB.B, alpha));
                case BlendMode.LChuv: return new PluginConfigColor(new Vector4((float)newColorLChuv2RGB.R, (float)newColorLChuv2RGB.G, (float)newColorLChuv2RGB.B, alpha));
            }

            return new PluginConfigColor(new Vector4((float)newColorLab2RGB.R, (float)newColorLab2RGB.G, (float)newColorLab2RGB.B, alpha));
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
            var tankInvulnBuff = actor.StatusList.Where(o => o.StatusId is 810 or 1302 or 409 or 1836);
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
            for (var i = 0; i < 200; i += 2)
            {
                var actor = actors[i];
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

        public static string UserFriendlyConfigName(string configTypeName)
        {
            return UserFriendlyString(configTypeName, "Config");
        }

        public static string UserFriendlyString(string str, string? remove)
        {
            var s = remove != null ? str.Replace(remove, "") : str;

            var regex = new Regex(@"
                    (?<=[A-Z])(?=[A-Z][a-z]) |
                    (?<=[^A-Z])(?=[A-Z]) |
                    (?<=[A-Za-z])(?=[^A-Za-z])",
                RegexOptions.IgnorePatternWhitespace);

            return regex.Replace(s, " ");
        }
    }
}
