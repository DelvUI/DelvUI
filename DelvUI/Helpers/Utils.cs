using Colourful;
using Dalamud.Game.ClientState.Actors;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Actors.Types.NonPlayer;
using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Interface.GeneralElements;
using System;
using System.Linq;
using System.Numerics;

namespace DelvUI.Helpers
{


    internal static class Utils
    {

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

        public static unsafe float ActorShieldValue(Actor actor)
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

        /*
        public struct RGB
        {
            private float _r;
            private float _g;
            private float _b;

            public RGB(float r, float g, float b)
            {
                this._r = r;
                this._g = g;
                this._b = b;
            }

            public float R
            {
                get { return this._r; }
                set { this._r = value; }
            }

            public float G
            {
                get { return this._g; }
                set { this._g = value; }
            }

            public float B
            {
                get { return this._b; }
                set { this._b = value; }
            }

            public bool Equals(RGB rgb)
            {
                return (this.R == rgb.R) && (this.G == rgb.G) && (this.B == rgb.B);
            }
        }

        public struct HSL
        {
            private int _h;
            private float _s;
            private float _l;

            public HSL(int h, float s, float l)
            {
                this._h = h;
                this._s = s;
                this._l = l;
            }

            public int H
            {
                get { return this._h; }
                set { this._h = value; }
            }

            public float S
            {
                get { return this._s; }
                set { this._s = value; }
            }

            public float L
            {
                get { return this._l; }
                set { this._l = value; }
            }

            public bool Equals(HSL hsl)
            {
                return (this.H == hsl.H) && (this.S == hsl.S) && (this.L == hsl.L);
            }
        }

        public static RGB HSLToRGB(HSL hsl)
        {
            float r = 0;
            float g = 0;
            float b = 0;

            if (hsl.S == 0)
            {
                r = g = b = (float)(hsl.L * 255);
            }
            else
            {
                float v1, v2;
                float hue = (float)hsl.H / 360;

                v2 = (hsl.L < 0.5) ? (hsl.L * (1 + hsl.S)) : ((hsl.L + hsl.S) - (hsl.L * hsl.S));
                v1 = 2 * hsl.L - v2;

                r = (float)(255 * HueToRGB(v1, v2, hue + (1.0f / 3)));
                g = (float)(255 * HueToRGB(v1, v2, hue));
                b = (float)(255 * HueToRGB(v1, v2, hue - (1.0f / 3)));
            }

            return new RGB(r, g, b);
        }

        private static float HueToRGB(float v1, float v2, float vH)
        {
            if (vH < 0)
            {
                vH += 1;
            }

            if (vH > 1)
            {
                vH -= 1;
            }

            if ((6 * vH) < 1)
            {
                return (v1 + (v2 - v1) * 6 * vH);
            }

            if ((2 * vH) < 1)
            {
                return v2;
            }

            if ((3 * vH) < 2)
            {
                return (v1 + (v2 - v1) * ((2.0f / 3) - vH) * 6);
            }

            return v1;
        }

        public static HSL RGBToHSL(RGB rgb)
        {
            HSL hsl = new HSL();

            float r = (rgb.R / 255.0f);
            float g = (rgb.G / 255.0f);
            float b = (rgb.B / 255.0f);

            float min = Math.Min(Math.Min(r, g), b);
            float max = Math.Max(Math.Max(r, g), b);
            float delta = max - min;

            hsl.L = (max + min) / 2;

            if (delta == 0)
            {
                hsl.H = 0;
                hsl.S = 0.0f;
            }
            else
            {
                hsl.S = (hsl.L <= 0.5) ? (delta / (max + min)) : (delta / (2 - max - min));

                float hue;

                if (r == max)
                {
                    hue = ((g - b) / 6) / delta;
                }
                else if (g == max)
                {
                    hue = (1.0f / 3) + ((b - r) / 6) / delta;
                }
                else
                {
                    hue = (2.0f / 3) + ((r - g) / 6) / delta;
                }

                if (hue < 0)
                {
                    hue += 1;
                }

                if (hue > 1)
                {
                    hue -= 1;
                }

                hsl.H = (int)(hue * 360);
            }

            return hsl;
        }
        */


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
                    var range = max - min;
                    ratio = (i - min) / range;
                }
            }

            var _rgbToLab = new ConverterBuilder().FromRGB().ToLab().Build();
            var _labToRgb = new ConverterBuilder().FromLab().ToRGB().Build();

            var rgbFullHealthColor = new RGBColor(fullHealthColor.Vector.X, fullHealthColor.Vector.Y, fullHealthColor.Vector.Z);
            var rgbLowHealthColor = new RGBColor(lowHealthColor.Vector.X, lowHealthColor.Vector.Y, lowHealthColor.Vector.Z);

            var rgbFullHealthLab = _rgbToLab.Convert(rgbFullHealthColor);
            var rgbLowHealthLab = _rgbToLab.Convert(rgbLowHealthColor);

            float resultL = (float)((rgbFullHealthLab.L - rgbLowHealthLab.L) * ratio + rgbLowHealthLab.L);
            float resultA = (float)((rgbFullHealthLab.a - rgbLowHealthLab.a) * ratio + rgbLowHealthLab.a);
            float resultB = (float)((rgbFullHealthLab.b - rgbLowHealthLab.b) * ratio + rgbLowHealthLab.b);

            var newColorLab = new LabColor(resultL, resultA, resultB);
            var newColorLab2RGB = _labToRgb.Convert(newColorLab);
            /*
            RGB rgbFH = new RGB(fullHealthColor.Vector.X,fullHealthColor.Vector.Y, fullHealthColor.Vector.Z);
            HSL hslFH = RGBToHSL(rgbFH);

            RGB rgbLH = new RGB(lowHealthColor.Vector.X, lowHealthColor.Vector.Y, lowHealthColor.Vector.Z);
            HSL hslLH = RGBToHSL(rgbLH);

            float resultHue = (int)((hslFH.H - hslLH.H) * ratio + hslLH.H);
            float resultSat = (hslFH.S - hslLH.S) * ratio + hslLH.S;
            float resultLit  = (hslFH.L - hslLH.L) * ratio + hslLH.L;

            RGB rgb = HSLToRGB(new((int)resultHue, resultSat, resultLit));
            */

            PluginConfigColor newColor = new PluginConfigColor(new Vector4((float)newColorLab2RGB.R, (float)newColorLab2RGB.G, (float)newColorLab2RGB.B, 100f / 100f));
            return newColor;
        }


        public static PluginConfigColor ColorForActor(Chara actor)
        {
            switch (actor.ObjectKind)
            {
                // Still need to figure out the "orange" state; aggroed but not yet attacked.
                case ObjectKind.Player:
                    return GlobalColors.Instance.SafeColorForJobId(actor.ClassJob.Id);

                case ObjectKind.BattleNpc when (actor.StatusFlags & StatusFlags.InCombat) == StatusFlags.InCombat:
                    return GlobalColors.Instance.NPCHostileColor;

                case ObjectKind.BattleNpc:
                    if (!IsHostileMemory((BattleNpc)actor))
                    {
                        return GlobalColors.Instance.NPCFriendlyColor;
                    }
                    break;
            }

            return GlobalColors.Instance.NPCNeutralColor;
        }

        public static bool HasTankInvulnerability(Actor actor)
        {
            var tankInvulnBuff = actor.StatusEffects.Where(o => o.EffectId is 810 or 1302 or 409 or 1836);
            return tankInvulnBuff.Count() > 0;
        }

        public static Actor FindTargetOfTarget(Actor target, Actor player, ActorTable actors)
        {
            if (target == null)
            {
                return null;
            }

            if (target.TargetActorID == 0 && player.TargetActorID == 0)
            {
                return player;
            }

            // only the first 200 elements in the array are relevant due to the order in which SE packs data into the array
            // we do a step of 2 because its always an actor followed by its companion
            for (var i = 0; i < 200; i += 2)
            {
                var actor = actors[i];
                if (actor?.ActorId == target.TargetActorID)
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
