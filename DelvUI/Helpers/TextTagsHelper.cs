using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Collections.Generic;
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

        public static Dictionary<string, Func<GameObject?, string?, int, bool?, string>> TextTags = new Dictionary<string, Func<GameObject?, string?, int, bool?, string>>()
        {
            #region generic names
            ["[name]"] = (actor, name, length, isPlayerName) =>
                ValidateName(actor, name).
                Truncated(length).
                CheckForUpperCase(),

            ["[name:first]"] = (actor, name, length, isPlayerName) =>
                ValidateName(actor, name).
                FirstName().
                Truncated(length).
                CheckForUpperCase(),

            ["[name:last]"] = (actor, name, length, isPlayerName) =>
                ValidateName(actor, name).
                LastName().
                Truncated(length).
                CheckForUpperCase(),

            ["[name:initials]"] = (actor, name, length, isPlayerName) =>
                ValidateName(actor, name).
                Initials().
                Truncated(length).
                CheckForUpperCase(),
            #endregion

            #region player names
            ["[player_name]"] = (actor, name, length, isPlayerName) =>
                ValidatePlayerName(actor, name, isPlayerName).
                Truncated(length).
                CheckForUpperCase(),

            ["[player_name:first]"] = (actor, name, length, isPlayerName) =>
                ValidatePlayerName(actor, name, isPlayerName).
                FirstName().
                Truncated(length).
                CheckForUpperCase(),

            ["[player_name:last]"] = (actor, name, length, isPlayerName) =>
                ValidatePlayerName(actor, name, isPlayerName).
                LastName().
                Truncated(length).
                CheckForUpperCase(),

            ["[player_name:initials]"] = (actor, name, length, isPlayerName) =>
                ValidatePlayerName(actor, name, isPlayerName).
                Initials().
                Truncated(length).
                CheckForUpperCase(),
            #endregion

            #region npc names
            ["[npc_name]"] = (actor, name, length, isPlayerName) =>
                ValidateNPCName(actor, name, isPlayerName).
                CheckForUpperCase(),

            ["[npc_name:first]"] = (actor, name, length, isPlayerName) =>
                ValidateNPCName(actor, name, isPlayerName).
                FirstName().
                CheckForUpperCase(),

            ["[npc_name:last]"] = (actor, name, length, isPlayerName) =>
                ValidateNPCName(actor, name, isPlayerName).
                LastName().
                CheckForUpperCase(),

            ["[npc_name:initials]"] = (actor, name, length, isPlayerName) =>
                ValidateNPCName(actor, name, isPlayerName).
                Initials().
                CheckForUpperCase(),
            #endregion
        };

        public static Dictionary<string, Func<GameObject?, string?, string>> ExpTags = new Dictionary<string, Func<GameObject?, string?, string>>()
        {
            #region experience
            ["[exp:current]"] = (actor, name) => ExperienceHelper.Instance.CurrentExp.ToString(),

            ["[exp:current-formatted]"] = (actor, name) => ExperienceHelper.Instance.CurrentExp.ToString("N0", ConfigurationManager.Instance.ActiveCultreInfo),

            ["[exp:current-short]"] = (actor, name) => ExperienceHelper.Instance.CurrentExp.KiloFormat(),

            ["[exp:required]"] = (actor, name) => ExperienceHelper.Instance.RequiredExp.ToString(),

            ["[exp:required-formatted]"] = (actor, name) => ExperienceHelper.Instance.RequiredExp.ToString("N0", ConfigurationManager.Instance.ActiveCultreInfo),

            ["[exp:required-short]"] = (actor, name) => ExperienceHelper.Instance.RequiredExp.KiloFormat(),

            ["[exp:required-to-level]"] = (actor, name) => (ExperienceHelper.Instance.RequiredExp - ExperienceHelper.Instance.CurrentExp).ToString(),

            ["[exp:required-to-level-formatted]"] = (actor, name) => (ExperienceHelper.Instance.RequiredExp - ExperienceHelper.Instance.CurrentExp).ToString("N0", ConfigurationManager.Instance.ActiveCultreInfo),

            ["[exp:required-to-level-short]"] = (actor, name) => (ExperienceHelper.Instance.RequiredExp - ExperienceHelper.Instance.CurrentExp).KiloFormat(),

            ["[exp:rested]"] = (actor, name) => ExperienceHelper.Instance.RestedExp.ToString(),

            ["[exp:rested-formatted]"] = (actor, name) => ExperienceHelper.Instance.RestedExp.ToString("N0", ConfigurationManager.Instance.ActiveCultreInfo),

            ["[exp:rested-short]"] = (actor, name) => ExperienceHelper.Instance.RestedExp.KiloFormat(),

            ["[exp:percent]"] = (actor, name) => ExperienceHelper.Instance.PercentExp.ToString("N0"),

            ["[exp:percent-decimal]"] = (actor, name) => ExperienceHelper.Instance.PercentExp.ToString("N1", ConfigurationManager.Instance.ActiveCultreInfo),
            #endregion
        };

        public static Dictionary<string, Func<uint, uint, string>> HealthTextTags = new Dictionary<string, Func<uint, uint, string>>()
        {
            #region health
            ["[health:current]"] = (currentHp, maxHp) => currentHp.ToString(),

            ["[health:current-formatted]"] = (currentHp, maxHp) => currentHp.ToString("N0", ConfigurationManager.Instance.ActiveCultreInfo),

            ["[health:current-short]"] = (currentHp, maxHp) => currentHp.KiloFormat(),

            ["[health:current-percent]"] = (currentHp, maxHp) => currentHp == maxHp ? currentHp.ToString() : (100f * currentHp / Math.Max(1, maxHp)).ToString("N0"),

            ["[health:current-percent-short]"] = (currentHp, maxHp) => currentHp == maxHp ? currentHp.KiloFormat() : (100f * currentHp / Math.Max(1, maxHp)).ToString("N0"),

            ["[health:max]"] = (currentHp, maxHp) => maxHp.ToString(),

            ["[health:max-formatted]"] = (currentHp, maxHp) => maxHp.ToString("N0", ConfigurationManager.Instance.ActiveCultreInfo),

            ["[health:max-short]"] = (currentHp, maxHp) => maxHp.KiloFormat(),

            ["[health:percent]"] = (currentHp, maxHp) => (100f * currentHp / Math.Max(1, maxHp)).ToString("N0"),

            ["[health:percent-decimal]"] = (currentHp, maxHp) => (100f * currentHp / Math.Max(1f, maxHp)).ToString("N1", ConfigurationManager.Instance.ActiveCultreInfo),

            ["[health:percent-decimal-uniform]"] = (currentHp, maxHp) => ConsistentDigitPercentage(currentHp, maxHp),

            ["[health:deficit]"] = (currentHp, maxHp) => currentHp == maxHp ? "0" : $"-{maxHp - currentHp}",

            ["[health:deficit-formatted]"] = (currentHp, maxHp) => currentHp == maxHp ? "0" : "-" + (maxHp - currentHp).ToString("N0", ConfigurationManager.Instance.ActiveCultreInfo),

            ["[health:deficit-short]"] = (currentHp, maxHp) => currentHp == maxHp ? "0" : $"-{(maxHp - currentHp).KiloFormat()}",
            #endregion
        };

        public static Dictionary<string, Func<uint, uint, string>> ManaTextTags = new Dictionary<string, Func<uint, uint, string>>()
        {
            #region mana
            ["[mana:current]"] = (currentMp, maxMp) => currentMp.ToString(),

            ["[mana:current-formatted]"] = (currentMp, maxMp) => currentMp.ToString("N0", ConfigurationManager.Instance.ActiveCultreInfo),

            ["[mana:current-short]"] = (currentMp, maxMp) => currentMp.KiloFormat(),

            ["[mana:current-percent]"] = (currentMp, maxMp) => currentMp == maxMp ? currentMp.ToString() : (100f * currentMp / Math.Max(1, maxMp)).ToString("N0"),

            ["[mana:current-percent-short]"] = (currentMp, maxMp) => currentMp == maxMp ? currentMp.KiloFormat() : (100f * currentMp / Math.Max(1, maxMp)).ToString("N0"),

            ["[mana:max]"] = (currentMp, maxMp) => maxMp.ToString(),

            ["[mana:max-formatted]"] = (currentMp, maxMp) => maxMp.ToString("N0", ConfigurationManager.Instance.ActiveCultreInfo),

            ["[mana:max-short]"] = (currentMp, maxMp) => maxMp.KiloFormat(),

            ["[mana:percent]"] = (currentMp, maxMp) => (100f * currentMp / Math.Max(1, maxMp)).ToString("N0", ConfigurationManager.Instance.ActiveCultreInfo),

            ["[mana:percent-decimal]"] = (currentMp, maxMp) => (100f * currentMp / Math.Max(1, maxMp)).ToString("N1", ConfigurationManager.Instance.ActiveCultreInfo),

            ["[mana:percent-decimal-uniform]"] = (currentMp, maxMp) => ConsistentDigitPercentage(currentMp, maxMp),

            ["[mana:deficit]"] = (currentMp, maxMp) => currentMp == maxMp ? "0" : $"-{maxMp - currentMp}",

            ["[mana:deficit-formatted]"] = (currentMp, maxMp) => currentMp == maxMp ? "0" : "-" + (maxMp - currentMp).ToString("N0", ConfigurationManager.Instance.ActiveCultreInfo),

            ["[mana:deficit-short]"] = (currentMp, maxMp) => currentMp == maxMp ? "0" : $"-{(maxMp - currentMp).KiloFormat()}",
            #endregion
        };

        public static Dictionary<string, Func<Character, string>> CharaTextTags = new Dictionary<string, Func<Character, string>>()
        {
            #region misc
            ["[distance]"] = (chara) => (chara.YalmDistanceX + 1).ToString(),

            ["[company]"] = (chara) => chara.CompanyTag.ToString(),

            ["[level]"] = (chara) => chara.Level > 0 ? chara.Level.ToString() : "-",

            ["[job]"] = (chara) => JobsHelper.JobNames.TryGetValue(chara.ClassJob.Id, out var jobName) ? jobName : "",

            ["[job-full]"] = (chara) => JobsHelper.JobFullNames.TryGetValue(chara.ClassJob.Id, out var jobName) ? jobName : "",

            ["[time-till-max-gp]"] = JobsHelper.TimeTillMaxGP,

            ["[chocobo-time]"] = (chara) =>
            {
                unsafe
                {
                    if (chara is BattleNpc npc && npc.BattleNpcKind == BattleNpcSubKind.Chocobo)
                    {
                        float seconds = UIState.Instance()->Buddy.CompanionInfo.TimeLeft;
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

        public static Dictionary<string, Func<string, int, string>> TitleTextTags = new Dictionary<string, Func<string, int, string>>()
        {
            #region title
            ["[title]"] = (title, length) => title.Truncated(length).CheckForUpperCase(),

            ["[title:first]"] = (title, length) => title.FirstName().Truncated(length).CheckForUpperCase(),

            ["[title:last]"] = (title, length) => title.LastName().Truncated(length).CheckForUpperCase(),

            ["[title:initials]"] = (title, length) => title.Initials().Truncated(length).CheckForUpperCase(),
            #endregion
        };

        private static List<Dictionary<string, Func<uint, uint, string>>> NumericValuesTagMaps = new List<Dictionary<string, Func<uint, uint, string>>>()
        {
            HealthTextTags,
            ManaTextTags
        };

        private static string ReplaceTagWithString(
            string tag,
            GameObject? actor,
            string? name = null,
            uint? current = null,
            uint? max = null,
            bool? isPlayerName = null,
            string? title = null)
        {
            int length = 0;
            ParseLength(ref tag, ref length);

            if (TextTags.TryGetValue(tag, out Func<GameObject?, string?, int, bool?, string>? func) && func != null)
            {
                return func(actor, name, length, isPlayerName);
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

            if (title != null &&
                TitleTextTags.TryGetValue(tag, out Func<string, int, string>? titlefunc) && titlefunc != null)
            {
                return titlefunc(title, length);
            }

            return "";
        }

        public static string FormattedText(
            string text,
            GameObject? actor,
            string? name = null,
            uint? current = null,
            uint? max = null,
            bool? isPlayerName = null,
            string? title = null)
        {
            bool isPlayer = (isPlayerName.HasValue && isPlayerName.Value == true) ||
                            (actor != null && actor.ObjectKind == ObjectKind.Player);

            try
            {
                // grouping
                List<string> groups = ParseGroups(text);
                string result = "";

                foreach (string group in groups)
                {
                    // tags
                    string groupText = ParseGroup(group, isPlayer);

                    MatchCollection matches = Regex.Matches(groupText, @"\[(.*?)\]");
                    string formattedGroupText = matches.Aggregate(groupText, (c, m) =>
                    {
                        string formattedText = ReplaceTagWithString(m.Value, actor, name, current, max, isPlayerName, title);
                        return c.Replace(m.Value, formattedText);
                    });

                    result += formattedGroupText;
                }

                return result;
            }
            catch (Exception e)
            {
                Plugin.Logger.Error(e.Message);
                return text;
            }
        }

        private static List<string> ParseGroups(string text)
        {
            MatchCollection matches = Regex.Matches(text, @"\{(.*?)\}");
            if (matches.Count == 0)
            {
                return new List<string>() { text };
            }

            List<string> result = new List<string>();
            int index = 0;

            foreach (Match match in matches)
            {
                if (index < match.Index)
                {
                    result.Add(text.Substring(0, match.Index - index));
                }

                result.Add(text.Substring(match.Index, match.Length));
                index = match.Index + match.Length;
            }

            if (index < text.Length)
            {
                result.Add(text.Substring(index));
            }

            return result;
        }

        private static string ParseGroup(string text, bool isPlayer)
        {
            if (!text.Contains("="))
            {
                return text;
            }

            if (isPlayer)
            {
                if (text.StartsWith("{player="))
                {
                    text = text.Substring(8);
                }
                else
                {
                    return "";
                }
            }
            else
            {
                if (text.StartsWith("{npc="))
                {
                    text = text.Substring(5);
                }
                else
                {
                    return "";
                }
            }

            int groupEndIndex = text.IndexOf("}");
            if (groupEndIndex > 0)
            {
                text = text.Remove(groupEndIndex, 1);
            }

            return text;
        }

        private static void ParseLength(ref string tag, ref int length)
        {
            int index = tag.IndexOf(".");
            if (index != -1)
            {
                string lengthString = tag.Substring(index + 1);
                lengthString = lengthString.Substring(0, lengthString.Length - 1);

                try
                {
                    length = int.Parse(lengthString);
                }
                catch { }

                tag = tag.Substring(0, tag.Length - lengthString.Length - 2) + "]";
            }
        }

        private static string ValidateName(GameObject? actor, string? name)
        {
            string? n = actor?.Name.ToString() ?? name;

            // Detour for PetRenamer
            try
            {
                string? customPetName = PetRenamerHelper.Instance.GetPetName(actor);
                n = customPetName ?? n;
            }
            catch { }

            return (n == null || n == "") ? "" : n;
        }

        private static string ValidatePlayerName(GameObject? actor, string? name, bool? isPlayerName = null)
        {
            if (isPlayerName.HasValue && isPlayerName.Value == false)
            {
                return "";
            }
            else if (!isPlayerName.HasValue && actor?.ObjectKind != ObjectKind.Player)
            {
                return "";
            }

            return ValidateName(actor, name);
        }

        private static string ValidateNPCName(GameObject? actor, string? name, bool? isPlayerName = null)
        {
            if (isPlayerName.HasValue && isPlayerName.Value == true)
            {
                return "";
            }
            else if (!isPlayerName.HasValue && actor?.ObjectKind == ObjectKind.Player)
            {
                return "";
            }

            return ValidateName(actor, name);
        }

        private static string ConsistentDigitPercentage(float currentVal, float maxVal)
        {
            var rawPercentage = 100f * currentVal / Math.Max(1f, maxVal);
            return rawPercentage >= 100 || rawPercentage <= 0 ? rawPercentage.ToString("N0") : rawPercentage.ToString("N1");
        }
    }
}