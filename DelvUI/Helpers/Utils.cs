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
using Dalamud.Game.ClientState.Objects.SubKinds;
using StructsCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

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

            return GetBuddy(player.ObjectId, BattleNpcSubKind.Chocobo);
        }

        public static GameObject? GetBuddy(uint ownerId, BattleNpcSubKind kind)
        {
            // only the first 200 elements in the array are relevant due to the order in which SE packs data into the array
            // we do a step of 2 because its always an actor followed by its companion
            for (var i = 0; i < 200; i += 2)
            {
                var gameObject = Plugin.ObjectTable[i];

                if (gameObject == null || gameObject.ObjectId == GameObject.InvalidGameObjectId || gameObject is not BattleNpc battleNpc)
                {
                    continue;
                }

                if (battleNpc.BattleNpcKind == kind && battleNpc.OwnerId == ownerId)
                {
                    return gameObject;
                }
            }

            return null;
        }

        public static GameObject? GetGameObjectByName(string name)
        {
            // only the first 200 elements in the array are relevant due to the order in which SE packs data into the array
            // we do a step of 2 because its always an actor followed by its companion
            for (int i = 0; i < 200; i += 2)
            {
                GameObject? gameObject = Plugin.ObjectTable[i];

                if (gameObject == null || gameObject.ObjectId == GameObject.InvalidGameObjectId || gameObject.ObjectId == 0)
                {
                    continue;
                }

                if (gameObject.Name.ToString() == name)
                {
                    return gameObject;
                }
            }

            return null;
        }

        public static unsafe bool IsHostileMemory(BattleNpc npc)
        {
            return npc != null
                && ((npc.BattleNpcKind == BattleNpcSubKind.Enemy || (int)npc.BattleNpcKind == 1)
                && *(byte*)(npc.Address + 0x19C3) != 0);
        }

        public static unsafe float ActorShieldValue(GameObject? actor)
        {
            if (actor == null || actor is not Character)
            {
                return 0f;
            }

            StructsCharacter* chara = (StructsCharacter*)actor.Address;
            return Math.Min((float)chara->ShieldValue, 100f) / 100f;
        }

        public static string DurationToString(double duration)
        {
            if (duration == 0)
            {
                return "";
            }

            TimeSpan t = TimeSpan.FromSeconds(duration);

            return t.Hours switch
            {
                >= 1 => t.Hours + "h",
                _ => t.Minutes switch
                {
                    >= 5 => t.Minutes + "m",
                    >= 1 => $"{t.Minutes}:{t.Seconds:00}",
                    _ => t.Seconds.ToString()
                }
            };
        }

        public static string DurationToFullString(double duration)
        {
            if (duration == 0)
            {
                return "";
            }

            TimeSpan t = TimeSpan.FromSeconds(duration);

            return $"{t.Minutes:00}:{t.Seconds:00}";
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

        public static PluginConfigColor GetColorByScale(float i, ColorByHealthValueConfig config) =>
            GetColorByScale(i, config.LowHealthColorThreshold / 100f, config.FullHealthColorThreshold / 100f, config.LowHealthColor, config.FullHealthColor, config.MaxHealthColor, config.UseMaxHealthColor, config.BlendMode);

        //Method used to interpolate two PluginConfigColors
        //i is scale [0 , 1]
        //min and max are used for color thresholds. for instance return colorLeft if i < min or return ColorRight if i > max
        public static PluginConfigColor GetColorByScale(float i, float min, float max, PluginConfigColor colorLeft, PluginConfigColor colorRight, PluginConfigColor colorMax, bool useMaxColor, BlendMode blendMode)
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
                    float range = max - min;
                    ratio = (i - min) / range;
                }
            }

            //Convert our PluginConfigColor to RGBColor
            RGBColor rgbColorLeft = new RGBColor(colorLeft.Vector.X, colorLeft.Vector.Y, colorLeft.Vector.Z);
            RGBColor rgbColorRight = new RGBColor(colorRight.Vector.X, colorRight.Vector.Y, colorRight.Vector.Z);

            //Interpolate our Alpha now
            float alpha = LinearInterpolation(colorLeft.Vector.W, colorRight.Vector.W, ratio);

            if (ratio >= 1 && useMaxColor)
            {
                return new PluginConfigColor(new Vector4((float)colorMax.Vector.X, (float)colorMax.Vector.Y, (float)colorMax.Vector.Z, colorMax.Vector.W));
            }

            //Allow the users to select different blend modes since interpolating between two colors can result in different blending depending on the color space
            //We convert our RGBColor values into different color spaces. We then interpolate each channel before converting the color back into RGBColor space
            switch (blendMode)
            {
                case BlendMode.LAB:
                    {
                        //convert RGB to LAB
                        LabColor LabLeft = _rgbToLab.Convert(rgbColorLeft);
                        LabColor LabRight = _rgbToLab.Convert(rgbColorRight);

                        RGBColor Lab2RGB = _labToRgb.Convert(
                            new LabColor(
                                LinearInterpolation((float)LabLeft.L, (float)LabRight.L, ratio),
                                LinearInterpolation((float)LabLeft.a, (float)LabRight.a, ratio),
                                LinearInterpolation((float)LabLeft.b, (float)LabRight.b, ratio)
                            )
                        );

                        Lab2RGB.NormalizeIntensity();
                        return new PluginConfigColor(new Vector4((float)Lab2RGB.R, (float)Lab2RGB.G, (float)Lab2RGB.B, alpha));
                    }

                case BlendMode.LChab:
                    {
                        //convert RGB to LChab
                        LChabColor LChabLeft = _rgbToLChab.Convert(rgbColorLeft);
                        LChabColor LChabRight = _rgbToLChab.Convert(rgbColorRight);

                        RGBColor LChab2RGB = _lchabToRgb.Convert(
                            new LChabColor(
                                LinearInterpolation((float)LChabLeft.L, (float)LChabRight.L, ratio),
                                LinearInterpolation((float)LChabLeft.C, (float)LChabRight.C, ratio),
                                LinearInterpolation((float)LChabLeft.h, (float)LChabRight.h, ratio)
                            )
                        );

                        LChab2RGB.NormalizeIntensity();

                        return new PluginConfigColor(new Vector4((float)LChab2RGB.R, (float)LChab2RGB.G, (float)LChab2RGB.B, alpha));
                    }
                case BlendMode.XYZ:
                    {
                        //convert RGB to XYZ
                        XYZColor XYZLeft = _rgbToXyz.Convert(rgbColorLeft);
                        XYZColor XYZRight = _rgbToXyz.Convert(rgbColorRight);

                        RGBColor XYZ2RGB = _xyzToRgb.Convert(
                            new XYZColor(
                                LinearInterpolation((float)XYZLeft.X, (float)XYZRight.X, ratio),
                                LinearInterpolation((float)XYZLeft.Y, (float)XYZRight.Y, ratio),
                                LinearInterpolation((float)XYZLeft.Z, (float)XYZRight.Z, ratio)
                            )
                        );

                        XYZ2RGB.NormalizeIntensity();

                        return new PluginConfigColor(new Vector4((float)XYZ2RGB.R, (float)XYZ2RGB.G, (float)XYZ2RGB.B, alpha));
                    }
                case BlendMode.RGB:
                    {
                        //No conversion needed here because we are already working in RGB space
                        RGBColor newRGB = new RGBColor(
                            LinearInterpolation((float)rgbColorLeft.R, (float)rgbColorRight.R, ratio),
                            LinearInterpolation((float)rgbColorLeft.G, (float)rgbColorRight.G, ratio),
                            LinearInterpolation((float)rgbColorLeft.B, (float)rgbColorRight.B, ratio)
                        );

                        return new PluginConfigColor(new Vector4((float)newRGB.R, (float)newRGB.G, (float)newRGB.B, alpha));
                    }
                case BlendMode.LChuv:
                    {
                        //convert RGB to LChuv
                        LChuvColor LChuvLeft = _rgbToLChuv.Convert(rgbColorLeft);
                        LChuvColor LChuvRight = _rgbToLChuv.Convert(rgbColorRight);

                        RGBColor LChuv2RGB = _lchuvToRgb.Convert(
                            new LChuvColor(
                                LinearInterpolation((float)LChuvLeft.L, (float)LChuvRight.L, ratio),
                                LinearInterpolation((float)LChuvLeft.C, (float)LChuvRight.C, ratio),
                                LinearInterpolation((float)LChuvLeft.h, (float)LChuvRight.h, ratio)
                            )
                        );

                        LChuv2RGB.NormalizeIntensity();

                        return new PluginConfigColor(new Vector4((float)LChuv2RGB.R, (float)LChuv2RGB.G, (float)LChuv2RGB.B, alpha));
                    }

                case BlendMode.Luv:
                    {
                        //convert RGB to Luv
                        LuvColor LuvLeft = _rgbToLuv.Convert(rgbColorLeft);
                        LuvColor LuvRight = _rgbToLuv.Convert(rgbColorRight);

                        RGBColor Luv2RGB = _luvToRgb.Convert(
                            new LuvColor(
                                LinearInterpolation((float)LuvLeft.L, (float)LuvRight.L, ratio),
                                LinearInterpolation((float)LuvLeft.u, (float)LuvRight.u, ratio),
                                LinearInterpolation((float)LuvLeft.v, (float)LuvRight.v, ratio)
                            )
                        );

                        Luv2RGB.NormalizeIntensity();

                        return new PluginConfigColor(new Vector4((float)Luv2RGB.R, (float)Luv2RGB.G, (float)Luv2RGB.B, alpha));

                    }
                case BlendMode.Jzazbz:
                    {
                        //convert RGB to Jzazbz
                        JzazbzColor JzazbzLeft = _rgbToJzazbz.Convert(rgbColorLeft);
                        JzazbzColor JzazbzRight = _rgbToJzazbz.Convert(rgbColorRight);

                        RGBColor Jzazbz2RGB = _jzazbzToRgb.Convert(
                            new JzazbzColor(
                                LinearInterpolation((float)JzazbzLeft.Jz, (float)JzazbzRight.Jz, ratio),
                                LinearInterpolation((float)JzazbzLeft.az, (float)JzazbzRight.az, ratio),
                                LinearInterpolation((float)JzazbzLeft.bz, (float)JzazbzRight.bz, ratio)
                            )
                        );

                        Jzazbz2RGB.NormalizeIntensity();

                        return new PluginConfigColor(new Vector4((float)Jzazbz2RGB.R, (float)Jzazbz2RGB.G, (float)Jzazbz2RGB.B, alpha));
                    }
                case BlendMode.JzCzhz:
                    {
                        //convert RGB to JzCzhz
                        JzCzhzColor JzCzhzLeft = _rgbToJzCzhz.Convert(rgbColorLeft);
                        JzCzhzColor JzCzhzRight = _rgbToJzCzhz.Convert(rgbColorRight);

                        RGBColor JzCzhz2RGB = _jzCzhzToRgb.Convert(
                            new JzCzhzColor(
                                LinearInterpolation((float)JzCzhzLeft.Jz, (float)JzCzhzRight.Jz, ratio),
                                LinearInterpolation((float)JzCzhzLeft.Cz, (float)JzCzhzRight.Cz, ratio),
                                LinearInterpolation((float)JzCzhzLeft.hz, (float)JzCzhzRight.hz, ratio)
                            )
                        );

                        JzCzhz2RGB.NormalizeIntensity();

                        return new PluginConfigColor(new Vector4((float)JzCzhz2RGB.R, (float)JzCzhz2RGB.G, (float)JzCzhz2RGB.B, alpha));
                    }
            }
            return new(Vector4.One);
        }

        public static PluginConfigColor ColorForActor(GameObject? actor)
        {
            if (actor == null || actor is not Character character)
            {
                return GlobalColors.Instance.NPCNeutralColor;
            }

            if (character.ObjectKind == ObjectKind.Player)
            {
                return GlobalColors.Instance.SafeColorForJobId(character.ClassJob.Id);
            }

            return character switch
            {
                BattleNpc { SubKind: 9 } battleNpc when battleNpc.ClassJob.Id > 0 => GlobalColors.Instance.SafeColorForJobId(character.ClassJob.Id), // Trust/Squadron NPCs
                BattleNpc battleNpc when battleNpc.BattleNpcKind == BattleNpcSubKind.Enemy || (battleNpc.StatusFlags & StatusFlags.InCombat) == StatusFlags.InCombat => GlobalColors
                    .Instance.NPCHostileColor, // I still don't think we should be defaulting to "in combat = hostile", but whatever
                BattleNpc battleNpc when battleNpc.BattleNpcKind is BattleNpcSubKind.Chocobo or BattleNpcSubKind.Pet || !IsHostileMemory(battleNpc) => GlobalColors.Instance
                    .NPCFriendlyColor,
                _ => GlobalColors.Instance.NPCNeutralColor
            };
        }

        public static Status? GetTankInvulnerabilityID(BattleChara actor)
        {
            Status? tankInvulnBuff = actor.StatusList.FirstOrDefault(o => o.StatusId is 810 or 811 or 3255 or 1302 or 409 or 1836 or 82);

            return tankInvulnBuff;
        }

        public static bool IsOnCleanseJob()
        {
            PlayerCharacter? player = Plugin.ClientState.LocalPlayer;

            return player != null && JobsHelper.IsJobWithCleanse(player.ClassJob.Id, player.Level);
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
            return anchor switch
            {
                DrawAnchor.Center => position - size / 2f,
                DrawAnchor.Left => position + new Vector2(0, -size.Y / 2f),
                DrawAnchor.Right => position + new Vector2(-size.X, -size.Y / 2f),
                DrawAnchor.Top => position + new Vector2(-size.X / 2f, 0),
                DrawAnchor.TopLeft => position,
                DrawAnchor.TopRight => position + new Vector2(-size.X, 0),
                DrawAnchor.Bottom => position + new Vector2(-size.X / 2f, -size.Y),
                DrawAnchor.BottomLeft => position + new Vector2(0, -size.Y),
                DrawAnchor.BottomRight => position + new Vector2(-size.X, -size.Y),
                _ => position
            };
        }

        public static string UserFriendlyConfigName(string configTypeName) => UserFriendlyString(configTypeName, "Config");

        public static string UserFriendlyString(string str, string? remove)
        {
            string? s = remove != null ? str.Replace(remove, "") : str;

            Regex? regex = new(@"
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
