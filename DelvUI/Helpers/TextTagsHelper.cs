using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.GeneratedSheets;
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

        public static Dictionary<string, Func<GameObject?, string?, int, string>> TextTags = new Dictionary<string, Func<GameObject?, string?, int, string>>()
        {
            #region generic names
            ["[name]"] = (actor, name, length) => ValidateName(actor, name, length).CheckForUpperCase(),

            ["[name:first]"] = (actor, name, length) => ValidateName(actor, name, length).FirstName().CheckForUpperCase(),

            ["[name:last]"] = (actor, name, length) => ValidateName(actor, name, length).LastName().CheckForUpperCase(),

            ["[name:initials]"] = (actor, name, length) => ValidateName(actor, name, length).Initials().CheckForUpperCase(),
            #endregion

            #region player names
            ["[player_name]"] = (actor, name, length) => ValidatePlayerName(actor, name, length).CheckForUpperCase(),

            ["[player_name:first]"] = (actor, name, length) => ValidatePlayerName(actor, name, length).FirstName().CheckForUpperCase(),

            ["[player_name:last]"] = (actor, name, length) => ValidatePlayerName(actor, name, length).LastName().CheckForUpperCase(),

            ["[player_name:initials]"] = (actor, name, length) => ValidatePlayerName(actor, name, length).Initials().CheckForUpperCase(),
            #endregion

            #region npc names
            ["[npc_name]"] = (actor, name, length) => ValidateNPCName(actor, name, length).CheckForUpperCase(),

            ["[npc_name:first]"] = (actor, name, length) => ValidateNPCName(actor, name, length).FirstName().CheckForUpperCase(),

            ["[npc_name:last]"] = (actor, name, length) => ValidateNPCName(actor, name, length).LastName().CheckForUpperCase(),

            ["[npc_name:initials]"] = (actor, name, length) => ValidateNPCName(actor, name, length).Initials().CheckForUpperCase(),
            #endregion
        };

        public static Dictionary<string, Func<GameObject?, string?, string>> ExpTags = new Dictionary<string, Func<GameObject?, string?, string>>()
        {
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
            int length = 0;
            if (tag.Contains("."))
            {
                int index = tag.IndexOf(".");
                string lengthString = tag.Substring(index + 1);
                lengthString = lengthString.Substring(0, lengthString.Length - 1);

                try
                {
                    length = int.Parse(lengthString);
                }
                catch { }

                tag = tag.Substring(0, tag.Length - lengthString.Length - 2) + "]";
            }

            if (TextTags.TryGetValue(tag, out Func<GameObject?, string?, int, string>? func) && func != null)
            {
                return func(actor, name, length);
            }

            if (ExpTags.TryGetValue(tag, out Func<GameObject?, string?, string>? expFunc) && expFunc != null)
            {
                return expFunc(actor, name);
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

        private static string ValidateName(GameObject? actor, string? name, int length = 0)
        {
            string str = actor != null ? actor.Name.ToString() : (name ?? "");

            if (length > 0)
            {
                str = str.Substring(0, Math.Min(str.Length, length));
            }

            return str;
        }

        private static string ValidatePlayerName(GameObject? actor, string? name, int length = 0)
        {
            if (actor?.ObjectKind != ObjectKind.Player)
            {
                return "";
            }

            return ValidateName(actor, name, length);
        }

        private static string ValidateNPCName(GameObject? actor, string? name, int length = 0)
        {
            if (actor?.ObjectKind == ObjectKind.Player)
            {
                return "";
            }

            return ValidateName(actor, name, length);
        }

        private static string ConsistentDigitPercentage(float currentVal, float maxVal){
            var rawPercentage = 100f * currentVal / Math.Max(1f, maxVal);
            return rawPercentage >= 100 || rawPercentage <= 0 ? rawPercentage.ToString("N0") : rawPercentage.ToString("N1");
        }
    }
}