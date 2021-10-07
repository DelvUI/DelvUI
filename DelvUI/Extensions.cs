using System;
using System.Globalization;
using System.Numerics;
using System.Text;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling;
using DelvUI.Interface.Bars;
using FFXIVClientStructs.FFXIV.Client.System.String;
using static System.Globalization.CultureInfo;

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

        public static string Initials(this SeString str)
        {
            var initials = "";
            var firstName = FirstName(str);
            var lastName = LastName(str);

            if (firstName.Length > 0)
            {
                initials = firstName[0] + ".";
            }

            if (lastName.Length > 0)
            {
                initials += " " + lastName[0] + ".";
            }

            return initials;
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

        public static Vector4 AdjustColorAlpha(this Vector4 vec, float correctionFactor)
        {
            return new Vector4(vec.X, vec.Y, vec.Z, Math.Min(1, Math.Max(0, vec.W + 1 * correctionFactor)));
        }

        public static Vector4 WithNewAlpha(this Vector4 vec, float alpha)
        {
            return new Vector4(vec.X, vec.Y, vec.Z, alpha);
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

        public static bool IsHorizontal(this BarDirection direction)
        {
            return direction == BarDirection.Right || direction == BarDirection.Left;
        }

        public static bool IsInverted(this BarDirection direction)
        {
            return direction == BarDirection.Left || direction == BarDirection.Up;
        }

        public static void Draw(this BarHud[] bars, Vector2 origin)
        {
            foreach (BarHud bar in bars)
            {
                bar.Draw(origin);
            }
        }
        
        public static string CheckForUpperCase(this string str)
        {            
            var culture = CurrentCulture.TextInfo;
            if (!string.IsNullOrEmpty(str) && char.IsLetter(str[0]) && !char.IsUpper(str[0]))
            {
                str = culture.ToTitleCase(str);
            }

            return str;
        }
    }
}
