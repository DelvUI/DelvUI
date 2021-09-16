
using Dalamud.Game.ClientState.Actors.Types;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using static DelvUI.Extensions;

namespace DelvUI.Helpers
{
    public static class TextTags
    {
        private static string ReplaceTagWithString(string tag, dynamic actor)
        {
            return tag switch
            {
                // Health
                "[health:current]" when IsPropertyExist(actor, "CurrentHp") => actor.CurrentHp.ToString(),
                "[health:current-short]" when IsPropertyExist(actor, "CurrentHp") => ((int)actor.CurrentHp).KiloFormat(),
                "[health:current-percent]" when IsPropertyExist(actor, "CurrentHp") && IsPropertyExist(actor, "MaxHp") => actor.CurrentHp == actor.MaxHp
                    ? actor.CurrentHp.ToString()
                    : $"{Math.Round(100f / actor.MaxHp * actor.CurrentHp)}",
                "[health:current-percent-short]" when IsPropertyExist(actor, "CurrentHp") && IsPropertyExist(actor, "MaxHp") => actor.CurrentHp == actor.MaxHp
                    ? ((int)actor.CurrentHp).KiloFormat()
                    : $"{Math.Round(100f / actor.MaxHp * actor.CurrentHp)}",
                "[health:current-max]" when IsPropertyExist(actor, "CurrentHp") && IsPropertyExist(actor, "MaxHp") => $"{actor.CurrentHp.ToString()} | {actor.MaxHp}",
                "[health:current-max-short]" when IsPropertyExist(actor, "CurrentHp") && IsPropertyExist(actor, "MaxHp") =>
                    $"{((int)actor.CurrentHp).KiloFormat()} | {((int)actor.MaxHp).KiloFormat()}",
                "[health:current-max-percent]" when IsPropertyExist(actor, "CurrentHp") && IsPropertyExist(actor, "MaxHp") => actor.CurrentHp == actor.MaxHp
                    ? $"{Math.Round(100f / actor.MaxHp * actor.CurrentHp)} - 100"
                    : $"{actor.CurrentHp} - {actor.MaxHp}",
                "[health:current-max-percent-short]" when IsPropertyExist(actor, "CurrentHp") && IsPropertyExist(actor, "MaxHp") => actor.CurrentHp == actor.MaxHp
                    ? $"{Math.Round(100f / actor.MaxHp * actor.CurrentHp)} - 100"
                    : $"{((int)actor.CurrentHp).KiloFormat()} | {((int)actor.MaxHp).KiloFormat()}",
                "[health:max]" when IsPropertyExist(actor, "MaxHp") => actor.MaxHp.ToString(),
                "[health:max-short]" when IsPropertyExist(actor, "MaxHp") => ((int)actor.MaxHp).KiloFormat(),
                "[health:percent]" when IsPropertyExist(actor, "CurrentHp") && IsPropertyExist(actor, "MaxHp") => $"{Math.Round(100f / actor.MaxHp * actor.CurrentHp)}",
                "[health:percent-decimal]" when IsPropertyExist(actor, "CurrentHp") && IsPropertyExist(actor, "MaxHp") => $"{100f / actor.MaxHp * actor.CurrentHp:##0.#}",
                "[health:deficit]" when IsPropertyExist(actor, "CurrentHp") && IsPropertyExist(actor, "MaxHp") => $"-{actor.MaxHp - actor.CurrentHp}",
                "[health:deficit-short]" when IsPropertyExist(actor, "CurrentHp") && IsPropertyExist(actor, "MaxHp") =>
                    $"-{((int)actor.MaxHp - (int)actor.CurrentHp).KiloFormat()}",

                // Mana
                "[mana:current]" when IsPropertyExist(actor, "CurrentMp") => actor.CurrentMp.ToString(),
                "[mana:current-short]" when IsPropertyExist(actor, "CurrentMp") => ((int)actor.CurrentMp).KiloFormat(),
                "[mana:current-percent]" when IsPropertyExist(actor, "CurrentMp") && IsPropertyExist(actor, "MaxMp") => actor.CurrentMp == actor.MaxMp
                    ? actor.CurrentMp.ToString()
                    : $"{Math.Round(100f / actor.MaxMp * actor.CurrentMp)}",
                "[mana:current-percent-short]" when IsPropertyExist(actor, "CurrentMp") && IsPropertyExist(actor, "MaxMp") => actor.CurrentMp == actor.MaxMp
                    ? ((int)actor.CurrentMp).KiloFormat()
                    : $"{Math.Round(100f / actor.MaxMp * actor.CurrentMp)}",
                "[mana:current-max]" when IsPropertyExist(actor, "CurrentMp") && IsPropertyExist(actor, "MaxMp") => $"{actor.CurrentMp.ToString()} | {actor.MaxMp}",
                "[mana:current-max-short]" when IsPropertyExist(actor, "CurrentMp") && IsPropertyExist(actor, "MaxMp") =>
                    $"{((int)actor.CurrentMp).KiloFormat()} | {((int)actor.MaxMp).KiloFormat()}",
                "[mana:current-max-percent]" when IsPropertyExist(actor, "CurrentMp") && IsPropertyExist(actor, "MaxMp") => actor.CurrentMp == actor.MaxMp
                    ? $"{Math.Round(100f / actor.MaxMp * actor.CurrentMp)} | 100"
                    : $"{actor.CurrentMp} - {actor.MaxMp}",
                "[mana:current-max-percent-short]" when IsPropertyExist(actor, "CurrentMp") && IsPropertyExist(actor, "MaxMp") => actor.CurrentMp == actor.MaxMp
                    ? $"{Math.Round(100f / actor.MaxMp * actor.CurrentMp)} | 100"
                    : $"{((int)actor.CurrentMp).KiloFormat()} - {((int)actor.MaxMp).KiloFormat()}",
                "[mana:max]" when IsPropertyExist(actor, "MaxMp") => actor.MaxMp.ToString(),
                "[mana:max-short]" when IsPropertyExist(actor, "MaxMp") => ((int)actor.MaxMp).KiloFormat(),
                "[mana:percent]" when IsPropertyExist(actor, "CurrentMp") && IsPropertyExist(actor, "MaxMp") =>
                    $"{Math.Round(100f / actor.MaxMp * actor.CurrentMp)}",
                "[mana:percent-decimal]" when IsPropertyExist(actor, "CurrentMp") && IsPropertyExist(actor, "MaxMp") =>
                    $"{100f / actor.MaxMp * actor.CurrentMp:##0.#}",
                "[mana:deficit]" when IsPropertyExist(actor, "CurrentMp") && IsPropertyExist(actor, "MaxMp") =>
                    $"-{(int)actor.MaxMp - (int)actor.CurrentMp}",
                "[mana:deficit-short]" when IsPropertyExist(actor, "CurrentMp") && IsPropertyExist(actor, "MaxMp")
                    => $"-{((int)actor.MaxMp - (int)actor.CurrentMp).KiloFormat()}",

                // Name
                "[name]" when IsPropertyExist(actor, "Name") => actor.Name.ToString(),
                "[name:first]" when IsPropertyExist(actor, "Name") => ((string)actor.Name).FirstName(),
                "[name:first-initial]" when IsPropertyExist(actor, "Name") => ((string)actor.Name).FirstName().Length == 0 ? "" : ((string)actor.Name).FirstName().Substring(0, 1),
                "[name:last]" when IsPropertyExist(actor, "Name") => ((string)actor.Name).LastName(),
                "[name:last-initial]" when IsPropertyExist(actor, "Name") => ((string)actor.Name).LastName().Length == 0 ? "" : ((string)actor.Name).LastName().Substring(0, 1),
                "[name:abbreviate]" when IsPropertyExist(actor, "Name") => ((string)actor.Name).Abbreviate(),
                "[name:veryshort]" when Extensions.IsPropertyExist(actor, "Name") => ((string)actor.Name).Truncate(5),
                "[name:short]" when IsPropertyExist(actor, "Name") => ((string)actor.Name).Truncate(10),
                "[name:medium]" when IsPropertyExist(actor, "Name") => ((string)actor.Name).Truncate(15),
                "[name:long]" when IsPropertyExist(actor, "Name") => ((string)actor.Name).Truncate(20),
                "" => "",

                // Misc
                "[company]" when IsPropertyExist(actor, "CompanyTag") => actor.CompanyTag,
                "[level]" when IsPropertyExist(actor, "Level") => actor.Level.ToString(),
                "[job]" when actor is Chara => JobsHelper.JobNames.TryGetValue(((Chara)actor).ClassJob.Id, out var jobName) ? jobName : "",
                _ => ""
            };
        }

        public static string GenerateFormattedTextFromTags(dynamic actor, string text)
        {
            text = text.Replace("%", "%%"); // Fixes rendering for % in ImGui
            var matches = Regex.Matches(text, @"\[(.*?)\]");

            return matches.Cast<Match>().Aggregate(text, (current, m) => current.Replace(m.Value, ReplaceTagWithString(m.Value, actor)));
        }
    }
}
