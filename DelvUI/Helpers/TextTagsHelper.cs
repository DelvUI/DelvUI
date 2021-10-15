using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace DelvUI.Helpers
{
    public static class TextTagsHelper
    {
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

        public static Dictionary<string, Func<Character, string>> CharaTextTags = new Dictionary<string, Func<Character, string>>()
        {
            #region health
            ["[health:current]"] = (chara) => chara.CurrentHp.ToString(),

            ["[health:current-short]"] = (chara) => chara.CurrentHp.KiloFormat(),

            ["[health:current-percent]"] = (chara) =>
                chara.CurrentHp == chara.MaxHp ?
                    chara.CurrentHp.ToString() :
                    (100f * chara.CurrentHp / Math.Max(1, chara.MaxHp)).ToString("N0"),

            ["[health:current-percent-short]"] = (chara) =>
                chara.CurrentHp == chara.MaxHp ?
                    chara.CurrentHp.KiloFormat() :
                    (100f * chara.CurrentHp / Math.Max(1, chara.MaxHp)).ToString("N0"),

            ["[health:current-max]"] = (chara) => $"{chara.CurrentHp}  |  {chara.MaxHp}",

            ["[health:current-max-short]"] = (chara) => $"{chara.CurrentHp.KiloFormat()}  |  {chara.MaxHp.KiloFormat()}",

            ["[health:chara.max]"] = (chara) => chara.CurrentHp.ToString(),

            ["[health:chara.max-short]"] = (chara) => chara.CurrentHp.KiloFormat(),

            ["[health:percent]"] = (chara) => (100f * chara.CurrentHp / Math.Max(1, chara.MaxHp)).ToString("N0"),

            ["[health:percent-decimal]"] = (chara) => FormattableString.Invariant($"{100f * chara.CurrentHp / Math.Max(1f, chara.MaxHp):##0.#}"),

            ["[health:deficit]"] = (chara) => chara.CurrentHp == chara.MaxHp ? "0" : $"-{chara.MaxHp - chara.CurrentHp}",

            ["[health:deficit-short]"] = (chara) => chara.CurrentHp == chara.MaxHp ? "0" : $"-{(chara.MaxHp - chara.CurrentHp).KiloFormat()}",
            #endregion

            #region mana
            ["[mana:current]"] = (chara) => JobsHelper.CurrentPrimaryResource(chara).ToString(),

            ["[mana:current-short]"] = (chara) => JobsHelper.CurrentPrimaryResource(chara).KiloFormat(),

            ["[mana:current-percent]"] = (chara) =>
            {
                uint mp = JobsHelper.CurrentPrimaryResource(chara);
                uint max = JobsHelper.MaxPrimaryResource(chara);
                return mp == max ? mp.ToString() : (100f * mp / Math.Max(1, max)).ToString("N0");
            },

            ["[mana:current-percent-short]"] = (chara) =>
            {
                uint mp = JobsHelper.CurrentPrimaryResource(chara);
                uint max = JobsHelper.MaxPrimaryResource(chara);
                return mp == max ? mp.KiloFormat() : (100f * mp / Math.Max(1, max)).ToString("N0");
            },

            ["[mana:current-max]"] = (chara) => $"{JobsHelper.CurrentPrimaryResource(chara)}  |  {JobsHelper.MaxPrimaryResource(chara)}",

            ["[mana:current-max-short]"] = (chara) => $"{JobsHelper.CurrentPrimaryResource(chara).KiloFormat()}  |  {JobsHelper.MaxPrimaryResource(chara).KiloFormat()}",

            ["[mana:max]"] = (chara) => JobsHelper.CurrentPrimaryResource(chara).ToString(),

            ["[mana:max-short]"] = (chara) => JobsHelper.CurrentPrimaryResource(chara).KiloFormat(),

            ["[mana:percent]"] = (chara) => (100f * JobsHelper.CurrentPrimaryResource(chara) / Math.Max(1, JobsHelper.MaxPrimaryResource(chara))).ToString("N0"),

            ["[mana:percent-decimal]"] = (chara) => FormattableString.Invariant($"{100f * JobsHelper.CurrentPrimaryResource(chara) / Math.Max(1, JobsHelper.MaxPrimaryResource(chara)):##0.#}"),

            ["[mana:deficit]"] = (chara) =>
            {
                uint mp = JobsHelper.CurrentPrimaryResource(chara);
                uint max = JobsHelper.MaxPrimaryResource(chara);
                return mp == max ? "0" : $"-{mp - max}";
            },

            ["[mana:deficit-short]"] = (chara) =>
            {
                uint mp = JobsHelper.CurrentPrimaryResource(chara);
                uint max = JobsHelper.MaxPrimaryResource(chara);
                return mp == max ? "0" : $"-{(mp - max).KiloFormat()}";
            },
            #endregion

            #region misc
            ["[distance]"] = (chara) => (chara.YalmDistanceX + 1).ToString(),

            ["[company]"] = (chara) => chara.CompanyTag.ToString(),

            ["[level]"] = (chara) => chara.Level.ToString(),

            ["[job]"] = (chara) => JobsHelper.JobNames.TryGetValue(chara.ClassJob.Id, out var jobName) ? jobName : "",
            #endregion
        };

        private static string ReplaceTagWithString(string tag, GameObject? actor, string? name = null)
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

            return "";
        }

        public static string FormattedText(string text, GameObject? actor, string? name = null)
        {
            MatchCollection matches = Regex.Matches(text, @"\[(.*?)\]");
            return matches.Aggregate(text, (current, m) =>
            {
                string formattedText = ReplaceTagWithString(m.Value, actor, name);
                return current.Replace(m.Value, formattedText);
            });
        }

        private static string ValidateName(GameObject? actor, string? name)
        {
            return actor != null ? actor.Name.ToString() : (name ?? "");
        }
    }
}