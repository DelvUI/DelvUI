using Colourful;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using Dalamud.Logging;
using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Interface.GeneralElements;
using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
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

        //Build our converter objects and store them in a field. This will be used to convert our PluginConfigColors into different color spaces to be used for interpolation
        private static readonly IColorConverter<RGBColor, LabColor> _rgbToLab = new ConverterBuilder().FromRGB().ToLab().Build();
        private static readonly IColorConverter<LabColor, RGBColor> _labToRgb = new ConverterBuilder().FromLab().ToRGB().Build();

        private static readonly IColorConverter<RGBColor, LChabColor> _rgbToLChab = new ConverterBuilder().FromRGB().ToLChab().Build();
        private static readonly IColorConverter<LChabColor, RGBColor> _lchabToRgb = new ConverterBuilder().FromLChab().ToRGB().Build();

        private static readonly IColorConverter<RGBColor, XYZColor> _rgbToXyz = new ConverterBuilder().FromRGB(RGBWorkingSpaces.sRGB).ToXYZ(Illuminants.D65).Build();
        private static readonly IColorConverter<XYZColor, RGBColor> _xyzToRgb = new ConverterBuilder().FromXYZ(Illuminants.D65).ToRGB(RGBWorkingSpaces.sRGB).Build();

        private static readonly IColorConverter<RGBColor, LChuvColor> _rgbToLChuv = new ConverterBuilder().FromRGB().ToLChuv().Build();
        private static readonly IColorConverter<LChuvColor, RGBColor> _lchuvToRgb = new ConverterBuilder().FromLChuv().ToRGB().Build();

        private static readonly IColorConverter<RGBColor, LuvColor> _rgbToLuv = new ConverterBuilder().FromRGB().ToLuv().Build();
        private static readonly IColorConverter<LuvColor, RGBColor> _luvToRgb = new ConverterBuilder().FromLuv().ToRGB().Build();

        private static readonly IColorConverter<RGBColor, JzazbzColor> _rgbToJzazbz = new ConverterBuilder().FromRGB().ToJzazbz().Build();
        private static readonly IColorConverter<JzazbzColor, RGBColor> _jzazbzToRgb = new ConverterBuilder().FromJzazbz().ToRGB().Build();

        private static readonly IColorConverter<RGBColor, JzCzhzColor> _rgbToJzCzhz = new ConverterBuilder().FromRGB().ToJzCzhz().Build();
        private static readonly IColorConverter<JzCzhzColor, RGBColor> _jzCzhzToRgb = new ConverterBuilder().FromJzCzhz().ToRGB().Build();

        //Simple LinearInterpolation method. T = [0 , 1]
        private static float LinearInterpolation(float left, float right, float t)
            => left + ((right - left) * t);

        //Method used to interpolate two PluginConfigColors
        //i is scale [0 , 1]
        //min and max are used for color thresholds. for instance return colorLeft if i < min or return ColorRight if i > max
        public static PluginConfigColor GetColorByScale(float i, float min, float max, PluginConfigColor colorLeft, PluginConfigColor colorRight, BlendMode blendMode)
        {
            //Set our thresholds where the ratio is the range of values we will use for interpolation. 
            //Values outside this range will either return colorLeft or colorRight
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

            //Convert our PluginConfigColor to RGBColor
            var rgbColorLeft = new RGBColor(colorLeft.Vector.X, colorLeft.Vector.Y, colorLeft.Vector.Z);
            var rgbColorRight = new RGBColor(colorRight.Vector.X, colorRight.Vector.Y, colorRight.Vector.Z);

            //Interpolate our Alpha now
            var alpha = LinearInterpolation(colorLeft.Vector.W, colorRight.Vector.W, ratio);

            //Allow the users to select different blend modes since interpolating between two colors can result in different blending depending on the color space
            //We convert our RGBColor values into different color spaces. We then interpolate each channel before converting the color back into RGBColor space
            switch (blendMode)
            {
                case BlendMode.LAB:
                    {
                        //convert RGB to LAB
                        var LabLeft = _rgbToLab.Convert(rgbColorLeft);
                        var LabRight = _rgbToLab.Convert(rgbColorRight);
                        
                        var Lab2RGB =_labToRgb.Convert(new LabColor(LinearInterpolation((float)LabLeft.L, (float)LabRight.L, ratio), LinearInterpolation((float)LabLeft.a, (float)LabRight.a, ratio), LinearInterpolation((float)LabLeft.b, (float)LabRight.b, ratio)));

                        Lab2RGB.NormalizeIntensity();

                        return new PluginConfigColor(new Vector4((float)Lab2RGB.R, (float)Lab2RGB.G, (float)Lab2RGB.B, alpha));
                    }

                case BlendMode.LChab:
                    {
                        //convert RGB to LChab
                        var LChabLeft = _rgbToLChab.Convert(rgbColorLeft);
                        var LChabRight = _rgbToLChab.Convert(rgbColorRight);

                        var LChab2RGB = _lchabToRgb.Convert(new LChabColor(LinearInterpolation((float)LChabLeft.L, (float)LChabRight.L, ratio), LinearInterpolation((float)LChabLeft.C, (float)LChabRight.C, ratio), LinearInterpolation((float)LChabLeft.h, (float)LChabRight.h, ratio)));

                        LChab2RGB.NormalizeIntensity();

                        return new PluginConfigColor(new Vector4((float)LChab2RGB.R, (float)LChab2RGB.G, (float)LChab2RGB.B, alpha));
                    }
                case BlendMode.XYZ:
                    {
                        //convert RGB to XYZ
                        var XYZLeft = _rgbToXyz.Convert(rgbColorLeft);
                        var XYZRight = _rgbToXyz.Convert(rgbColorRight);
                        
                        var XYZ2RGB = _xyzToRgb.Convert(new XYZColor(LinearInterpolation((float)XYZLeft.X, (float)XYZRight.X, ratio), LinearInterpolation((float)XYZLeft.Y, (float)XYZRight.Y, ratio), LinearInterpolation((float)XYZLeft.Z, (float)XYZRight.Z, ratio)));

                        XYZ2RGB.NormalizeIntensity();

                        return new PluginConfigColor(new Vector4((float)XYZ2RGB.R, (float)XYZ2RGB.G, (float)XYZ2RGB.B, alpha));
                    }
                case BlendMode.RGB:
                    {
                        //No conversion needed here because we are already working in RGB space
                        var newRGB = new RGBColor(LinearInterpolation((float)rgbColorLeft.R, (float)rgbColorRight.R, ratio), LinearInterpolation((float)rgbColorLeft.G, (float)rgbColorRight.G, ratio), LinearInterpolation((float)rgbColorLeft.B, (float)rgbColorRight.B, ratio));
                        
                        return new PluginConfigColor(new Vector4((float)newRGB.R, (float)newRGB.G, (float)newRGB.B, alpha));
                    }
                case BlendMode.LChuv:
                    {
                        //convert RGB to LChuv
                        var LChuvLeft = _rgbToLChuv.Convert(rgbColorLeft);
                        var LChuvRight = _rgbToLChuv.Convert(rgbColorRight);

                        var LChuv2RGB = _lchuvToRgb.Convert(new LChuvColor(LinearInterpolation((float)LChuvLeft.L, (float)LChuvRight.L, ratio), LinearInterpolation((float)LChuvLeft.C, (float)LChuvRight.C, ratio), LinearInterpolation((float)LChuvLeft.h, (float)LChuvRight.h, ratio)));

                        LChuv2RGB.NormalizeIntensity();

                        return new PluginConfigColor(new Vector4((float)LChuv2RGB.R, (float)LChuv2RGB.G, (float)LChuv2RGB.B, alpha));
                    }

                case BlendMode.Luv:
                    {
                        //convert RGB to Luv
                        var LuvLeft = _rgbToLuv.Convert(rgbColorLeft);
                        var LuvRight = _rgbToLuv.Convert(rgbColorRight);

                        var Luv2RGB = _luvToRgb.Convert(new LuvColor(LinearInterpolation((float)LuvLeft.L, (float)LuvRight.L, ratio), LinearInterpolation((float)LuvLeft.u, (float)LuvRight.u, ratio), LinearInterpolation((float)LuvLeft.v, (float)LuvRight.v, ratio)));

                        Luv2RGB.NormalizeIntensity();

                        return new PluginConfigColor(new Vector4((float)Luv2RGB.R, (float)Luv2RGB.G, (float)Luv2RGB.B, alpha));

                    }
                case BlendMode.Jzazbz:
                    {
                        //convert RGB to Jzazbz
                        var JzazbzLeft = _rgbToJzazbz.Convert(rgbColorLeft);
                        var JzazbzRight = _rgbToJzazbz.Convert(rgbColorRight);

                        var Jzazbz2RGB = _jzazbzToRgb.Convert(new JzazbzColor(LinearInterpolation((float)JzazbzLeft.Jz, (float)JzazbzRight.Jz, ratio), LinearInterpolation((float)JzazbzLeft.az, (float)JzazbzRight.az, ratio), LinearInterpolation((float)JzazbzLeft.bz, (float)JzazbzRight.bz, ratio)));

                        Jzazbz2RGB.NormalizeIntensity();

                        return new PluginConfigColor(new Vector4((float)Jzazbz2RGB.R, (float)Jzazbz2RGB.G, (float)Jzazbz2RGB.B, alpha));
                    }
                case BlendMode.JzCzhz:
                    {
                        //convert RGB to JzCzhz
                        var JzCzhzLeft = _rgbToJzCzhz.Convert(rgbColorLeft);
                        var JzCzhzRight = _rgbToJzCzhz.Convert(rgbColorRight);

                        var JzCzhz2RGB = _jzCzhzToRgb.Convert(new JzCzhzColor(LinearInterpolation((float)JzCzhzLeft.Jz, (float)JzCzhzRight.Jz, ratio), LinearInterpolation((float)JzCzhzLeft.Cz, (float)JzCzhzRight.Cz, ratio), LinearInterpolation((float)JzCzhzLeft.hz, (float)JzCzhzRight.hz, ratio)));

                        JzCzhz2RGB.NormalizeIntensity();

                        return new PluginConfigColor(new Vector4((float)JzCzhz2RGB.R, (float)JzCzhz2RGB.G, (float)JzCzhz2RGB.B, alpha));
                    }

                default: throw new ArgumentOutOfRangeException();
            }
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

                case ObjectKind.BattleNpc when (actor is BattleNpc battleNpc):
                    if (!IsHostileMemory(battleNpc))
                    {
                        return GlobalColors.Instance.NPCFriendlyColor;
                    }
                    break;
            }

            return GlobalColors.Instance.NPCNeutralColor;
        }

        public static Status HasTankInvulnerability(BattleChara actor)
        {
            var tankInvulnBuff = actor.StatusList.FirstOrDefault(o => o.StatusId is 810 or 811 or 1302 or 409 or 1836 or 82);
            return tankInvulnBuff!;
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

        public static void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                try
                {
                    // hack because of this: https://github.com/dotnet/corefx/issues/10361
                    if (RuntimeInformation.IsOSPlatform(osPlatform: OSPlatform.Windows))
                    {
                        url = url.Replace("&", "^&");
                        Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        Process.Start("xdg-open", url);
                    }
                }
                catch (Exception e)
                {
                    PluginLog.Error("Error trying to open url: " + e.Message);
                }
            }
        }
    }
}
