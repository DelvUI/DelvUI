using System;

namespace DelvUIPlugin {
    public static class Extensions {
        public static string KiloFormat(this int num)
        {
            if (num >= 100000000)
                return (num / 1000000).ToString("#,0M");

            if (num >= 1000000)
                return (num / 1000000).ToString("0.#") + "M";

            if (num >= 100000)
                return (num / 1000).ToString("#,0K");

            if (num >= 10000)
                return (num / 1000).ToString("0.#") + "K";

            return num.ToString("#,0");
        } 
        
        public static string Abbreviate(this string str) {
            var splits = str.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            
            for (var i = 0; i < splits.Length - 1; i++) {
                splits[i] = splits[i][0].ToString();
            }
    
            return string.Join(". ", splits).ToUpper();
        }
        
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength); 
        }
    }
}