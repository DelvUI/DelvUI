﻿using System;
using System.Globalization;
using System.Numerics;
using System.Text;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.System.String;

namespace DelvUI
{
    public static class Extensions
    {
        public static string Abbreviate(this SeString str)
        {
            string[] splits = str.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < splits.Length - 1; i++)
            {
                splits[i] = splits[i][0].ToString();
            }

            return string.Join(". ", splits).ToUpper();
        }

        public static string FirstName(this SeString str)
        {
            string[] splits = str.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return splits.Length > 0 ? splits[0] : "";
        }

        public static string LastName(this SeString str)
        {
            string[] splits = str.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return splits.Length > 1 ? splits[^1] : "";
        }

        public static Vector4 AdjustColor(this Vector4 vec, float correctionFactor)
        {
            float red = vec.X;
            float green = vec.Y;
            float blue = vec.Z;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (1 - red) * correctionFactor + red;
                green = (1 - green) * correctionFactor + green;
                blue = (1 - blue) * correctionFactor + blue;
            }

            return new Vector4(red, green, blue, vec.W);
        }

        public static unsafe string GetString(this Utf8String utf8String)
        {
            int s = utf8String.BufUsed > int.MaxValue ? int.MaxValue : (int)utf8String.BufUsed;

            try
            {
                return s <= 1 ? string.Empty : Encoding.UTF8.GetString(utf8String.StringPtr, s - 1);
            }
            catch (Exception ex)
            {
                return $"<<{ex.Message}>>";
            }
        }

        public static string KiloFormat(this uint num)
        {
            return num switch
            {
                >= 100000000 => (num / 1000000.0).ToString("#,0M", CultureInfo.InvariantCulture),
                >= 1000000 => (num / 1000000.0).ToString("0.0", CultureInfo.InvariantCulture) + "M",
                >= 100000 => (num / 1000.0).ToString("#,0K", CultureInfo.InvariantCulture),
                >= 10000 => (num / 1000.0).ToString("0.0", CultureInfo.InvariantCulture) + "K",
                _ => num.ToString("#,0", CultureInfo.InvariantCulture)
            };
        }

        public static string Truncate(this SeString value, int maxLength)
        {
            string str = value.ToString();
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            return str.Length <= maxLength ? str : str[..maxLength];
        }
    }
}
