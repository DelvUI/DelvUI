using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Numerics;
using System.Text;
using FFXIVClientStructs.FFXIV.Client.System.String;
using Dalamud.Game.Text.SeStringHandling;

namespace DelvUI {
    public static class Extensions {
        public static string Abbreviate(this SeString str) {
            var splits = str.TextValue.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            
            for (var i = 0; i < splits.Length - 1; i++) {
                splits[i] = splits[i][0].ToString();
            }
    
            return string.Join(". ", splits).ToUpper();
        }

        public static Vector4 AdjustColor(this Vector4 vec, float correctionFactor) {
            var red = vec.X;
            var green = vec.Y;
            var blue = vec.Z;

            if (correctionFactor < 0) {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else {
                red = (1 - red) * correctionFactor + red;
                green = (1 - green) * correctionFactor + green;
                blue = (1 - blue) * correctionFactor + blue;
            }

            return new Vector4(red, green, blue, vec.W);
        }
        
        public static bool IsPropertyExist(dynamic settings, string name)
        {
            if (settings is ExpandoObject)
                return ((IDictionary<string, object>)settings).ContainsKey(name);

            return settings.GetType().GetProperty(name) != null;
        }
        
        public static unsafe string GetString(this Utf8String utf8String) {
            var s = utf8String.BufUsed > int.MaxValue ? int.MaxValue : (int) utf8String.BufUsed;
            try {
                return s <= 1 ? string.Empty : Encoding.UTF8.GetString(utf8String.StringPtr, s - 1);
            } catch (Exception ex) {
                return $"<<{ex.Message}>>";
            }
        }
        
        public static string KiloFormat(this uint num)
        {
            if (num >= 100000000)
                return (num / 1000000).ToString("#,0M", CultureInfo.InvariantCulture);

            if (num >= 1000000)
                return (num / 1000000).ToString("0.#", CultureInfo.InvariantCulture) + "M";

            if (num >= 100000)
                return (num / 1000).ToString("#,0K", CultureInfo.InvariantCulture);

            if (num >= 10000)
                return (num / 1000).ToString("0.#", CultureInfo.InvariantCulture) + "K";

            return num.ToString("#,0", CultureInfo.InvariantCulture);
        }
        
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength); 
        }
    }
}
