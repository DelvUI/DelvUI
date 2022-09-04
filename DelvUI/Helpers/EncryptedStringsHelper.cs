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
                return strings[(int)ClientLanguage.English];

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
            [30478] = new string[] { "クロウ・アンド・テイル", "Claw to Tail", "Kralle und Schwanz", "Griffes et queue" },
            [30482] = new string[] { "テイル・アンド・クロウ", "Tail to Claw", "Schwanz und Kralle", "Queue et griffes" },
            [30491] = new string[] { "ダブルラッシュ", "Double Rush", "Doppelsturm", "Double ruée" },
            [30492] = new string[] { "ダブルラッシュ", "Double Rush", "Doppelsturm", "Double ruée" },

            // p6s
            [30828] = new string[] { "", "Exchange of Agonies" },
            [30838] = new string[] { "", "Cachexia" },
            [30839] = new string[] { "", "Aetheronecrosis" },
            [30840] = new string[] { "", "Dual Predation" },
            [30841] = new string[] { "", "Dual Predation" },
            [30842] = new string[] { "", "Glossal Predation" },
            [30843] = new string[] { "", "Chelic Predation" },
            [30844] = new string[] { "", "Ptera Ixou" },
            [30845] = new string[] { "", "Ptera Ixou" },
            [30846] = new string[] { "", "Ptera Ixou" },
            [30858] = new string[] { "", "Chelic Synergy" },

            // p7s
            [30746] = new string[] { "", "Inviolate Bonds" },
            [30750] = new string[] { "", "Inviolate Purgation" },
            [31221] = new string[] { "", "Multicast" },
            [31311] = new string[] { "", "Famine's Harvest" },
            [31312] = new string[] { "", "Death's Harvest" },
            [31313] = new string[] { "", "War's Harvest" },

            // p8s
            [28938] = new string[] { "", "High Concept" },
            [30996] = new string[] { "", "Conceptual Octaflare" },
            [30997] = new string[] { "", "Conceptual Tetraflare" },
            [30998] = new string[] { "", "Conceptual Tetraflare" },
            [30999] = new string[] { "", "Conceptual Diflare" },
            [31000] = new string[] { "", "Emergent Octaflare" },
            [31001] = new string[] { "", "Emergent Tetraflare" },
            [31003] = new string[] { "", "Emergent Diflare" },
            [31005] = new string[] { "", "Octaflare" },
            [31006] = new string[] { "", "Tetraflare" },
            [31007] = new string[] { "", "Nest of Flamevipers" },
            [31008] = new string[] { "", "Nest of Flamevipers" },
            [31009] = new string[] { "", "Manifold Flames" },
            [31010] = new string[] { "", "Manifold Flames" },
            [31021] = new string[] { "", "Eye of the Gorgon" },
            [31022] = new string[] { "", "Crown of the Gorgon" },
            [31023] = new string[] { "", "Blood of the Gorgon" },
            [31024] = new string[] { "", "Breath of the Gorgon" },
            [31030] = new string[] { "", "Stomp Dead" },
            [31031] = new string[] { "", "Stomp Dead" },
            [31032] = new string[] { "", "Blazing Footfalls" },
            [31033] = new string[] { "", "Trailblaze" },
            [31038] = new string[] { "", "Trailblaze" },
            [31040] = new string[] { "", "Rain of Fire" },
            [31043] = new string[] { "", "Rain of Fire" },
            [31044] = new string[] { "", "Genesis of Flame" },
            [31050] = new string[] { "", "Genesis of Flame" },
            [31148] = new string[] { "", "High Concept" },
            [31149] = new string[] { "", "Conceptual Shift" },
            [31150] = new string[] { "", "Conceptual Shift" },
            [31151] = new string[] { "", "Conceptual Shift" },
            [31152] = new string[] { "", "Conception" },
            [31153] = new string[] { "", "Failure of Imagination" },
            [31154] = new string[] { "", "Splicer" },
            [31155] = new string[] { "", "Splicer" },
            [31156] = new string[] { "", "Splicer" },
            [31157] = new string[] { "", "Everburn" },
            [31158] = new string[] { "", "Arcane Control" },
            [31159] = new string[] { "", "Arcane Channel" },
            [31160] = new string[] { "", "Arcane Wave" },
            [31162] = new string[] { "", "Ego Death" },
            [31163] = new string[] { "", "Natural Alignment" },
            [31164] = new string[] { "", "Twist Nature" },
            [31165] = new string[] { "", "Forcible Trifire" },
            [31166] = new string[] { "", "Forcible Difreeze" },
            [31167] = new string[] { "", "Forcible Fire III" },
            [31168] = new string[] { "", "Forcible Fire II" },
            [31169] = new string[] { "", "Forcible Fire IV" },
            [31170] = new string[] { "", "Inverse Magicks" },
            [31171] = new string[] { "", "Fates Unending" },
            [31172] = new string[] { "", "Unsightly Chaos" },
            [31173] = new string[] { "", "Unsightly Chaos" },
            [31176] = new string[] { "", "Illusory Soul" },
            [31177] = new string[] { "", "Soul Strand" },
            [31178] = new string[] { "", "Soul Strand" },
            [31179] = new string[] { "", "Soul Strand" },
            [31180] = new string[] { "", "End of the End" },
            [31181] = new string[] { "", "Dirge of Second Death" },
            [31182] = new string[] { "", "Limitless Kindling" },
            [31184] = new string[] { "", "Somatic Dirge of Second Death" },
            [31185] = new string[] { "", "Somatic Tyrant's Fire III" },
            [31186] = new string[] { "", "Somatic Dirge of Second Death" },
            [31187] = new string[] { "", "Somatic Tyrant's Fire III" },
            [31188] = new string[] { "", "Dead Outside" },
            [31193] = new string[] { "", "Dominion" },
            [31194] = new string[] { "", "Orogenic Annihilation" },
            [31195] = new string[] { "", "Orogenic Deformation" },
            [31196] = new string[] { "", "Orogenic Shift" },
            [31197] = new string[] { "", "Tyrant's Unholy Darkness" },
            [31198] = new string[] { "", "Tyrant's Unholy Darkness" },
            [31199] = new string[] { "", "Aioniopyr" },
            [31204] = new string[] { "", "Ego Death" },
            [31205] = new string[] { "", "Somatic End of the End" },
            [31210] = new string[] { "", "Ektothermos" },
            [31266] = new string[] { "", "Aionagonia" },
            [31366] = new string[] { "", "Ego Death" },
            [31367] = new string[] { "", "Ego Death" },
        };

        private static Dictionary<uint, string[]> StatusNameMap = new Dictionary<uint, string[]>()
        {
            // p6s
            [3315] = new string[] { "", "Chelomorph" },
            [3319] = new string[] { "", "Glossal Resistance Down" },
            [3320] = new string[] { "", "Chelic Resistance Down" },
            [3321] = new string[] { "", "Aetheronecrosis" },

            // p7s
            [3308] = new string[] { "", "Inviolate Winds" },
            [3309] = new string[] { "", "Holy Bonds" },
            [3310] = new string[] { "", "Purgatory Winds" },
            [3311] = new string[] { "", "Holy Purgation" },
            [3391] = new string[] { "", "Purgatory Winds" },
            [3392] = new string[] { "", "Purgatory Winds" },
            [3393] = new string[] { "", "Purgatory Winds" },
            [3394] = new string[] { "", "Holy Purgation" },
            [3395] = new string[] { "", "Holy Purgation" },
            [3396] = new string[] { "", "Holy Purgation" },
            [3397] = new string[] { "", "Inviolate Winds" },
            [3398] = new string[] { "", "Holy Bonds" },

            // p8s
            [3325] = new string[] { "", "Conceptual Mastery" },
            [3326] = new string[] { "", "Blood of the Gorgon" },
            [3327] = new string[] { "", "Breath of the Gorgon" },
            [3330] = new string[] { "", "Imperfection: Alpha" },
            [3331] = new string[] { "", "Imperfection: Beta" },
            [3332] = new string[] { "", "Imperfection: Gamma" },
            [3333] = new string[] { "", "Perfection: Alpha" },
            [3334] = new string[] { "", "Perfection: Beta" },
            [3335] = new string[] { "", "Perfection: Gamma" },
            [3336] = new string[] { "", "Inconceivable" },
            [3337] = new string[] { "", "Winged Conception" },
            [3338] = new string[] { "", "Aquatic Conception" },
            [3339] = new string[] { "", "Shocking Conception" },
            [3340] = new string[] { "", "Fiery Conception" },
            [3341] = new string[] { "", "Toxic Conception" },
            [3342] = new string[] { "", "Growing Conception" },
            [3343] = new string[] { "", "Immortal Spark" },
            [3344] = new string[] { "", "Immortal Conception" },
            [3345] = new string[] { "", "Solosplice" },
            [3346] = new string[] { "", "Multisplice" },
            [3347] = new string[] { "", "Supersplice" },
            [3349] = new string[] { "", "Inverse Magicks" },
            [3350] = new string[] { "", "Soul Stranded" },
            [3351] = new string[] { "", "Eye of the Gorgon" },
            [3352] = new string[] { "", "Crown of the Gorgon" },
            [3406] = new string[] { "", "Everburn" },
            [3412] = new string[] { "", "Natural Alignment" },
        };

        private static Dictionary<uint, string[]> StatusDescriptionMap = new Dictionary<uint, string[]>()
        {
            // p6s
            [3315] = new string[] { "", "Host to a chelic parasite, which will take control of body once this effect expires." },
            [3319] = new string[] { "", "Resistance to attacks by glossal parasites is reduced." },
            [3320] = new string[] { "", "Resistance to attacks by chelic parasites is reduced." },
            [3321] = new string[] { "", "Infected with aetherially activated cells, which will burst explosively when this effect expires." },

            // p7s
            [3308] = new string[] { "", "Ensnared by wind magicks that will be unleashed in the surrounding area when this effect expires." },
            [3309] = new string[] { "", "Ensnared by light magicks that will be unleashed in the surrounding area when this effect expires." },
            [3310] = new string[] { "", "Ensnared by punishing wind magicks that will be unleashed in the surrounding area when this effect expires, and leave a timed sigil in their wake." },
            [3311] = new string[] { "", "Ensnared by punishing wind magicks that will be unleashed in the surrounding area when this effect expires, and leave a timed sigil in their wake." },
            [3391] = new string[] { "", "Ensnared by punishing wind magicks that will be unleashed in the surrounding area when this effect expires, and leave a timed sigil in their wake." },
            [3392] = new string[] { "", "Ensnared by punishing wind magicks that will be unleashed in the surrounding area when this effect expires, and leave a timed sigil in their wake." },
            [3393] = new string[] { "", "Ensnared by punishing wind magicks that will be unleashed in the surrounding area when this effect expires, and leave a timed sigil in their wake." },
            [3394] = new string[] { "", "Ensnared by punishing wind magicks that will be unleashed in the surrounding area when this effect expires, and leave a timed sigil in their wake." },
            [3395] = new string[] { "", "Ensnared by punishing wind magicks that will be unleashed in the surrounding area when this effect expires, and leave a timed sigil in their wake." },
            [3396] = new string[] { "", "Ensnared by punishing wind magicks that will be unleashed in the surrounding area when this effect expires, and leave a timed sigil in their wake." },
            [3397] = new string[] { "", "Ensnared by wind magicks that will be unleashed in the surrounding area when this effect expires." },
            [3398] = new string[] { "", "Ensnared by light magicks that will be unleashed in the surrounding area when this effect expires." },

            // p8s
            [3325] = new string[] { "", "Primed with powerful magicks." },
            [3326] = new string[] { "", "Cursed to unleash poison in the surrounding area when this effect expires." },
            [3327] = new string[] { "", "Cursed to unleash poison in the surrounding area when this effect expires." },
            [3330] = new string[] { "", "Imbued with a concept incompatible with this form, which will cause an explosive reaction and influence self and nearby concepts when this effect expires." },
            [3331] = new string[] { "", "Imbued with a concept incompatible with this form, which will cause an explosive reaction and influence self and nearby concepts when this effect expires." },
            [3332] = new string[] { "", "Imbued with a concept incompatible with this form, which will cause an explosive reaction and influence self and nearby concepts when this effect expires." },
            [3333] = new string[] { "", "The perfect vessel for a perfect concept.Drawing near to other perfect concepts will result in mutual influence." },
            [3334] = new string[] { "", "The perfect vessel for a perfect concept.Drawing near to other perfect concepts will result in mutual influence." },
            [3335] = new string[] { "", "The perfect vessel for a perfect concept.Drawing near to other perfect concepts will result in mutual influence." },
            [3336] = new string[] { "", "Unable to draw upon perfect concepts until this effect expires." },
            [3337] = new string[] { "", "Realizing an airborne concept.Resistance to certain wind magicks is increased until this effect expires." },
            [3338] = new string[] { "", "Realizing an aquatic concept.Resistance to certain water magicks is increased until this effect expires." },
            [3339] = new string[] { "", "Realizing a levin - wielding concept. Resistance to certain lightning magicks is increased until this effect expires." },
            [3340] = new string[] { "", "Realizing a burning concept, causing damage over time until this effect expires." },
            [3341] = new string[] { "", "Realizing a poisonous concept, causing damage over time until this effect expires." },
            [3342] = new string[] { "", "Realizing a plantlike concept, causing damage over time until this effect expires." },
            [3343] = new string[] { "", "Conceiving of the Phoenix in part. Together, four such sparks will give birth to a legendary bird." },
            [3344] = new string[] { "", "Realizing a Phoenix concept." },
            [3345] = new string[] { "", "Self - concept is being warped beyond recognition, resulting in an adverse reaction determined by nearby influences when this effect expires." },
            [3346] = new string[] { "", "Self - concept is being warped beyond recognition, resulting in an adverse reaction determined by nearby influences when this effect expires." },
            [3347] = new string[] { "", "Self - concept is being warped beyond recognition, resulting in an adverse reaction determined by nearby influences when this effect expires." },
            [3349] = new string[] { "", "The order of forcible magicks to be cast is inverted." },
            [3350] = new string[] { "", "Physical and spiritual forms have been separated." },
            [3351] = new string[] { "", "Cursed to unleash a petrifying attack in the direction of gaze when this effect expires." },
            [3352] = new string[] { "", "Cursed to unleash a petrifying light upon those nearby when this effect expires." },
            [3406] = new string[] { "", "Calling upon the power of a Phoenix concept. Damage dealt is increased." },
            [3412] = new string[] { "", "Graven with a sigil and sustaining damage over time. Taking damage from certain actions caused by Twist Nature will result in a destructive forcible failure." }
        };
    }
}
