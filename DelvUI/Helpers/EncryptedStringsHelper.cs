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
                // only english for now
                return strings[0];

                //int language = (int)Plugin.ClientState.ClientLanguage;
                //if (language < 0 || language >= strings.Length) 
                //{
                //    language = (int)ClientLanguage.English;
                //}

                //return strings[language];
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
            [30478] = new string[] { "Claw to Tail" },
            [30482] = new string[] { "Tail to Claw" },
            [30491] = new string[] { "Double Rush" },
            [30492] = new string[] { "Double Rush" },

            // p6s
            [30828] = new string[] { "Exchange of Agonies" },
            [30838] = new string[] { "Cachexia" },
            [30839] = new string[] { "Aetheronecrosis" },
            [30840] = new string[] { "Dual Predation" },
            [30841] = new string[] { "Dual Predation" },
            [30842] = new string[] { "Glossal Predation" },
            [30843] = new string[] { "Chelic Predation" },
            [30844] = new string[] { "Ptera Ixou" },
            [30845] = new string[] { "Ptera Ixou" },
            [30846] = new string[] { "Ptera Ixou" },
            [30858] = new string[] { "Chelic Synergy" },

            // p7s
            [30746] = new string[] { "Inviolate Bonds" },
            [30750] = new string[] { "Inviolate Purgation" },
            [31221] = new string[] { "Multicast" },
            [31311] = new string[] { "Famine's Harvest" },
            [31312] = new string[] { "Death's Harvest" },
            [31313] = new string[] { "War's Harvest" }

        };

        private static Dictionary<uint, string[]> StatusNameMap = new Dictionary<uint, string[]>()
        {
            // p6s
            [3315] = new string[] { "Chelomorph" },
            [3319] = new string[] { "Glossal Resistance Down" },
            [3320] = new string[] { "Chelic Resistance Down" },
            [3321] = new string[] { "Aetheronecrosis" },

            // p7s
            [3308] = new string[] { "Inviolate Winds" },
            [3309] = new string[] { "Holy Bonds" },
            [3310] = new string[] { "Purgatory Winds" },
            [3311] = new string[] { "Holy Purgation" },
            [3391] = new string[] { "Purgatory Winds" },
            [3392] = new string[] { "Purgatory Winds" },
            [3393] = new string[] { "Purgatory Winds" },
            [3394] = new string[] { "Holy Purgation" },
            [3395] = new string[] { "Holy Purgation" },
            [3396] = new string[] { "Holy Purgation" },
            [3397] = new string[] { "Inviolate Winds" },
            [3398] = new string[] { "Holy Bonds" }
        };

        private static Dictionary<uint, string[]> StatusDescriptionMap = new Dictionary<uint, string[]>()
        {
            // p6s
            [3315] = new string[] { "Host to a chelic parasite, which will take control of body once this effect expires." },
            [3319] = new string[] { "Resistance to attacks by glossal parasites is reduced." },
            [3320] = new string[] { "Resistance to attacks by chelic parasites is reduced." },
            [3321] = new string[] { "Infected with aetherially activated cells, which will burst explosively when this effect expires." },

            // p7s
            [3308] = new string[] { "Ensnared by wind magicks that will be unleashed in the surrounding area when this effect expires." },
            [3309] = new string[] { "Ensnared by light magicks that will be unleashed in the surrounding area when this effect expires." },
            [3310] = new string[] { "Ensnared by punishing wind magicks that will be unleashed in the surrounding area when this effect expires, and leave a timed sigil in their wake." },
            [3311] = new string[] { "Ensnared by punishing wind magicks that will be unleashed in the surrounding area when this effect expires, and leave a timed sigil in their wake." },
            [3391] = new string[] { "Ensnared by punishing wind magicks that will be unleashed in the surrounding area when this effect expires, and leave a timed sigil in their wake." },
            [3392] = new string[] { "Ensnared by punishing wind magicks that will be unleashed in the surrounding area when this effect expires, and leave a timed sigil in their wake." },
            [3393] = new string[] { "Ensnared by punishing wind magicks that will be unleashed in the surrounding area when this effect expires, and leave a timed sigil in their wake." },
            [3394] = new string[] { "Ensnared by punishing wind magicks that will be unleashed in the surrounding area when this effect expires, and leave a timed sigil in their wake." },
            [3395] = new string[] { "Ensnared by punishing wind magicks that will be unleashed in the surrounding area when this effect expires, and leave a timed sigil in their wake." },
            [3396] = new string[] { "Ensnared by punishing wind magicks that will be unleashed in the surrounding area when this effect expires, and leave a timed sigil in their wake." },
            [3397] = new string[] { "Ensnared by wind magicks that will be unleashed in the surrounding area when this effect expires." },
            [3398] = new string[] { "Ensnared by light magicks that will be unleashed in the surrounding area when this effect expires." }
        };
    }
}
