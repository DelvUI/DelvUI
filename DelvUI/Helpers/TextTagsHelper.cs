using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace DelvUI.Helpers
{
    public static class TextTagsHelper
    {
        public static void Initialize()
        {
            foreach (string key in HealthTextTags.Keys)
            {
                CharaTextTags.Add(key, (chara) => HealthTextTags[key](chara.CurrentHp, chara.MaxHp));
            }

            foreach (string key in ManaTextTags.Keys)
            {
                CharaTextTags.Add(key, (chara) => ManaTextTags[key](JobsHelper.CurrentPrimaryResource(chara), JobsHelper.MaxPrimaryResource(chara)));
            }
        }

        public static Dictionary<string, Func<GameObject?, string?, string>> TextTags = new Dictionary<string, Func<GameObject?, string?, string>>()
        {
            #region name
            ["[name]"] = (actor, name) => ValidateName(actor, name).CheckForUpperCase(),

            ["[name:first]"] = (actor, name) => ValidateName(actor, name).FirstName().CheckForUpperCase(),

            ["[name:first-initial]"] = (actor, name) =>
            {
                name = ValidateName(actor, name).FirstName().CheckForUpperCase();
                return name.Length > 0 ? name[..1] : "";
            },

            ["[name:first-npcmedium]"] = (actor, name) =>
            {
                name = ValidateName(actor, name);
                return actor?.ObjectKind == ObjectKind.Player ?
                    name.FirstName().CheckForUpperCase() :
                    name.Truncate(15).CheckForUpperCase();
            },

            ["[name:first-npclong]"] = (actor, name) =>
            {
                name = ValidateName(actor, name);
                return actor?.ObjectKind == ObjectKind.Player ?
                    name.FirstName().CheckForUpperCase() :
                    name.Truncate(15).CheckForUpperCase();
            },

            ["[name:first-npcfull]"] = (actor, name) =>
            {
                name = ValidateName(actor, name);
                return actor?.ObjectKind == ObjectKind.Player ?
                    name.FirstName().CheckForUpperCase() :
                    name.CheckForUpperCase();
            },

            ["[name:last]"] = (actor, name) => ValidateName(actor, name).LastName().CheckForUpperCase(),

            ["[name:last-initial]"] = (actor, name) =>
            {
                name = ValidateName(actor, name).LastName().CheckForUpperCase();
                return name.Length > 0 ? name[..1] : "";
            },

            ["[name:initials]"] = (actor, name) => ValidateName(actor, name).Initials().CheckForUpperCase(),

            ["[name:abbreviate]"] = (actor, name) => ValidateName(actor, name).Abbreviate().CheckForUpperCase(),

            ["[name:veryshort]"] = (actor, name) => ValidateName(actor, name).Truncate(5).CheckForUpperCase(),

            ["[name:short]"] = (actor, name) => ValidateName(actor, name).Truncate(10).CheckForUpperCase(),

            ["[name:medium]"] = (actor, name) => ValidateName(actor, name).Truncate(15).CheckForUpperCase(),

            ["[name:long]"] = (actor, name) => ValidateName(actor, name).Truncate(20).CheckForUpperCase(),
            #endregion

            #region experience
            ["[exp:current]"] = (actor, name) => ExperienceHelper.Instance.CurrentExp.ToString("N0", CultureInfo.InvariantCulture),

            ["[exp:current-short]"] = (actor, name) => ExperienceHelper.Instance.CurrentExp.KiloFormat(),

            ["[exp:required]"] = (actor, name) => ExperienceHelper.Instance.RequiredExp.ToString("N0", CultureInfo.InvariantCulture),

            ["[exp:required-short]"] = (actor, name) => ExperienceHelper.Instance.RequiredExp.KiloFormat(),

            ["[exp:rested]"] = (actor, name) => ExperienceHelper.Instance.RestedExp.ToString("N0", CultureInfo.InvariantCulture),

            ["[exp:rested-short]"] = (actor, name) => ExperienceHelper.Instance.RestedExp.KiloFormat(),

            ["[exp:percent]"] = (actor, name) => ExperienceHelper.Instance.PercentExp.ToString("N1", CultureInfo.InvariantCulture),
            #endregion
        };

        public static Dictionary<string, Func<uint, uint, string>> HealthTextTags = new Dictionary<string, Func<uint, uint, string>>()
        {
            #region health
            ["[health:current]"] = (currentHp, maxHp) => currentHp.ToString(),

            ["[health:current-short]"] = (currentHp, maxHp) => currentHp.KiloFormat(),

            ["[health:current-percent]"] = (currentHp, maxHp) => currentHp == maxHp ? currentHp.ToString() : (100f * currentHp / Math.Max(1, maxHp)).ToString("N0"),

            ["[health:current-percent-short]"] = (currentHp, maxHp) => currentHp == maxHp ? currentHp.KiloFormat() : (100f * currentHp / Math.Max(1, maxHp)).ToString("N0"),

            ["[health:current-max]"] = (currentHp, maxHp) => $"{currentHp}  |  {maxHp}",

            ["[health:current-max-short]"] = (currentHp, maxHp) => $"{currentHp.KiloFormat()}  |  {maxHp.KiloFormat()}",

            ["[health:max]"] = (currentHp, maxHp) => maxHp.ToString(),

            ["[health:max-short]"] = (currentHp, maxHp) => maxHp.KiloFormat(),

            ["[health:percent]"] = (currentHp, maxHp) => (100f * currentHp / Math.Max(1, maxHp)).ToString("N0"),

            ["[health:percent-decimal]"] = (currentHp, maxHp) => FormattableString.Invariant($"{100f * currentHp / Math.Max(1f, maxHp):##0.#}"),

            ["[health:percent-decimal-uniform]"] = (currentHp, maxHp) => ConsistentDigitPercentage(currentHp, maxHp),

            ["[health:deficit]"] = (currentHp, maxHp) => currentHp == maxHp ? "0" : $"-{maxHp - currentHp}",

            ["[health:deficit-short]"] = (currentHp, maxHp) => currentHp == maxHp ? "0" : $"-{(maxHp - currentHp).KiloFormat()}",
            #endregion
        };

        public static Dictionary<string, Func<uint, uint, string>> ManaTextTags = new Dictionary<string, Func<uint, uint, string>>()
        {
            #region mana
            ["[mana:current]"] = (currentMp, maxMp) => currentMp.ToString(),

            ["[mana:current-short]"] = (currentMp, maxMp) => currentMp.KiloFormat(),

            ["[mana:current-percent]"] = (currentMp, maxMp) => currentMp == maxMp ? currentMp.ToString() : (100f * currentMp / Math.Max(1, maxMp)).ToString("N0"),

            ["[mana:current-percent-short]"] = (currentMp, maxMp) => currentMp == maxMp ? currentMp.KiloFormat() : (100f * currentMp / Math.Max(1, maxMp)).ToString("N0"),

            ["[mana:current-max]"] = (currentMp, maxMp) => $"{currentMp}  |  {maxMp}",

            ["[mana:current-max-short]"] = (currentMp, maxMp) => $"{currentMp.KiloFormat()}  |  {maxMp.KiloFormat()}",

            ["[mana:max]"] = (currentMp, maxMp) => maxMp.ToString(),

            ["[mana:max-short]"] = (currentMp, maxMp) => maxMp.KiloFormat(),

            ["[mana:percent]"] = (currentMp, maxMp) => (100f * currentMp / Math.Max(1, maxMp)).ToString("N0"),

            ["[mana:percent-decimal]"] = (currentMp, maxMp) => FormattableString.Invariant($"{100f * currentMp / Math.Max(1, maxMp):##0.#}"),

            ["[mana:percent-decimal-uniform]"] = (currentMp, maxMp) => ConsistentDigitPercentage(currentMp, maxMp),

            ["[mana:deficit]"] = (currentMp, maxMp) => currentMp == maxMp ? "0" : $"-{currentMp - maxMp}",

            ["[mana:deficit-short]"] = (currentMp, maxMp) => currentMp == maxMp ? "0" : $"-{(currentMp - maxMp).KiloFormat()}",
            #endregion
        };

        public static Dictionary<string, Func<Character, string>> CharaTextTags = new Dictionary<string, Func<Character, string>>()
        {
            #region misc
            ["[distance]"] = (chara) => (chara.YalmDistanceX + 1).ToString(),

            ["[company]"] = (chara) => chara.CompanyTag.ToString(),

            ["[level]"] = (chara) => chara.Level > 0 ? chara.Level.ToString() : "-",

            ["[job]"] = (chara) => JobsHelper.JobNames.TryGetValue(chara.ClassJob.Id, out var jobName) ? jobName : "",

            ["[time-till-max-gp]"] = JobsHelper.TimeTillMaxGP,

            ["[chocobo-time]"] = (chara) =>
            {
                unsafe
                {
                    if (chara is BattleNpc npc && npc.BattleNpcKind == BattleNpcSubKind.Chocobo)
                    {
                        float seconds = UIState.Instance()->Buddy.TimeLeft;
                        if (seconds <= 0)
                        {
                            return "";
                        }

                        TimeSpan time = TimeSpan.FromSeconds(seconds);
                        return time.ToString(@"mm\:ss");
                    }
                }
                return "";
            }
            #endregion
        };

        private static List<Dictionary<string, Func<uint, uint, string>>> NumericValuesTagMaps = new List<Dictionary<string, Func<uint, uint, string>>>()
        {
            HealthTextTags,
            ManaTextTags
        };

        private static string ReplaceTagWithString(string tag, GameObject? actor, string? name = null, uint? current = null, uint? max = null)
        {
            if (TextTags.TryGetValue(tag, out Func<GameObject?, string?, string>? func) && func != null)
            {
                return func(actor, name);
            }

            if (actor is Character chara &&
                CharaTextTags.TryGetValue(tag, out Func<Character, string>? charaFunc) && charaFunc != null)
            {
                return charaFunc(chara);
            }
            else if (current.HasValue && max.HasValue)
            {
                foreach (var map in NumericValuesTagMaps)
                {
                    if (map.TryGetValue(tag, out Func<uint, uint, string>? numericFunc) && numericFunc != null)
                    {
                        return numericFunc(current.Value, max.Value);
                    }
                }
            }

            return "";
        }

        public static string FormattedText(string text, GameObject? actor, string? name = null, uint? current = null, uint? max = null)
        {
            MatchCollection matches = Regex.Matches(text, @"\[(.*?)\]");
            return matches.Aggregate(text, (c, m) =>
            {
                string formattedText = ReplaceTagWithString(m.Value, actor, name, current, max);
                return c.Replace(m.Value, formattedText);
            });
        }

        private static string ValidateName(GameObject? actor, string? name)
        {
            return actor != null ? actor.Name.ToString() : (name ?? "");
        }

        private static string ConsistentDigitPercentage(float currentVal, float maxVal){
            var rawPercentage = 100f * currentVal / Math.Max(1f, maxVal);
            return rawPercentage >= 100 || rawPercentage <= 0 ? rawPercentage.ToString("N0") : rawPercentage.ToString("N1");
        }
    }
}