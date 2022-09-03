using Dalamud;
using System.Collections.Generic;

namespace DelvUI.Helpers
{
    public static class EncryptedStringsHelper
    {
        private static string? GetString(Dictionary<uint, string[]> map, uint key)
        {
            if (map.TryGetValue(key, out string[]? strings) && strings != null)
            {
                int language = (int)Plugin.ClientState.ClientLanguage;
                if (language < 0 || language >= strings.Length) 
                {
                    language = (int)ClientLanguage.English;
                }

                return strings[language];
            }

            return null;
        }

        public static string? GetActionString(uint? actionId)
        {
            if (!actionId.HasValue) { return null; }

            return GetString(ActionsMap, actionId.Value);
        }

        public static string? GetStatusNameString(uint statusId)
        {
            return GetString(StatusNameMap, statusId);
        }

        public static string? GetStatusDescriptionString(uint statusId)
        {
            return GetString(StatusDescriptionMap, statusId);
        }

        private static Dictionary<uint, string[]> ActionsMap = new Dictionary<uint, string[]>()
        {
            // p5s
            [30478] = new string[] { "Claw to Tail", "Claw to Tail" },
            [30482] = new string[] { "Tail to Claw", "Tail to Claw" },
            [30491] = new string[] { "Double Rush", "Double Rush" },
            [30492] = new string[] { "Double Rush", "Double Rush" },
        };

        private static Dictionary<uint, string[]> StatusNameMap = new Dictionary<uint, string[]>()
        {
        };

        private static Dictionary<uint, string[]> StatusDescriptionMap = new Dictionary<uint, string[]>()
        {
        };
    }
}
