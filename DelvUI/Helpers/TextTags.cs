using System;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;
using Dalamud.Game.ClientState.Actors.Types;
using static DelvUI.Extensions;
using Actor = Dalamud.Game.ClientState.Structs.Actor;

namespace DelvUI.Helpers
{
    public static class TextTags
    {
        private static string ReplaceTagWithString(string tag, dynamic actor)
        {
            return tag switch
                {
                    // Health
                    "[health:current]" => actor.CurrentHp.ToString(),
                    "[health:current-short]" => ((int) actor.CurrentHp).KiloFormat(),
                    "[health:current-percent]" => actor.CurrentHp == actor.MaxHp
                        ? actor.CurrentHp.ToString()
                        : $"{Math.Round(100f / actor.MaxHp * actor.CurrentHp)}%",
                    "[health:current-percent-short]" => actor.CurrentHp == actor.MaxHp
                        ? ((int) actor.CurrentHp).KiloFormat()
                        : $"{Math.Round(100f / actor.MaxHp * actor.CurrentHp)}%",
                    "[health:current-max]" => $"{actor.CurrentHp.ToString()} | {actor.MaxHp}",
                    "[health:current-max-short]" => $"{actor.CurrentHp.KiloFormat()} | {((int) actor.MaxHp).KiloFormat()}",
                    "[health:current-max-percent]" => actor.CurrentHp == actor.MaxHp
                        ? $"{Math.Round(100f / actor.MaxHp * actor.CurrentHp)}% - 100%" : $"{actor.CurrentHp} - {actor.MaxHp}",
                    "[health:current-max-percent-short]" => actor.CurrentHp == actor.MaxHp
                        ? $"{Math.Round(100f / actor.MaxHp * actor.CurrentHp)}% - 100%"
                        : $"{((int) actor.CurrentHp).KiloFormat()} | {((int) actor.MaxHp).KiloFormat()}",
                    "[health:max]" => actor.MaxHp.ToString(),
                    "[health:max-short]" => ((int) actor.MaxHp).KiloFormat(),
                    "[health:percent]" => $"{Math.Round(100f / actor.MaxHp * actor.CurrentHp)}%",
                    "[health:deficit]" => $"-{actor.MaxHp - actor.CurrentHp}",
                    "[health:deficit-short]" => $"-{((int) actor.MaxHp - (int) actor.CurrentHp).KiloFormat()}",
                    
                    // Mana
                    "[mana:current]" => actor.CurrentMp.ToString(),
                    "[mana:current-short]" => ((int) actor.CurrentMp).KiloFormat(),
                    "[mana:current-percent]" => actor.CurrentMp == actor.MaxMp
                        ? actor.CurrentMp.ToString()
                        : $"{Math.Round(100f / actor.MaxMp * actor.CurrentMp)}%",
                    "[mana:current-percent-short]" => actor.CurrentMp == actor.MaxMp
                        ? ((int) actor.CurrentMp).KiloFormat()
                        : $"{Math.Round(100f / actor.MaxMp * actor.CurrentMp)}%",
                    "[mana:current-max]" => $"{actor.CurrentMp.ToString()} | {actor.MaxMp}",
                    "[mana:current-max-short]" => $"{((int) actor.CurrentMp).KiloFormat()} | {((int) actor.MaxMp).KiloFormat()}",
                    "[mana:current-max-percent]" => actor.CurrentMp == actor.MaxMp
                        ? $"{Math.Round(100f / actor.MaxMp * actor.CurrentMp)}% | 100%" : $"{actor.CurrentMp} - {actor.MaxMp}",
                    "[mana:current-max-percent-short]" => actor.CurrentMp == actor.MaxMp
                        ? $"{Math.Round(100f / actor.MaxMp * actor.CurrentMp)}% | 100%"
                        : $"{((int) actor.CurrentMp).KiloFormat()} - {((int) actor.MaxMp).KiloFormat()}",
                    "[mana:max]" => actor.MaxMp.ToString(),
                    "[mana:max-short]" => ((int)actor.MaxMp).KiloFormat(),
                    "[mana:percent]" => $"{Math.Round(100f / actor.MaxMp * actor.CurrentMp)}%",
                    "[mana:deficit]" => $"-{(int) actor.MaxMp - (int) actor.CurrentMp}",
                    "[mana:deficit-short]" => $"-{((int) actor.MaxMp - (int) actor.CurrentMp).KiloFormat()}",
                    
                    // Name
                    "[name]" => actor.Name.ToString(),
                    "[name:abbreviate]" => ((string) actor.Name).Abbreviate(),
                    "[name:veryshort]" => ((string) actor.Name).Truncate(5),
                    "[name:short]" => ((string) actor.Name).Truncate(10),
                    "[name:medium]" => ((string) actor.Name).Truncate(15),
                    "[name:long]" => ((string) actor.Name).Truncate(20),
                    "" => "",
                    
                    // Misc
                    "[company]" when IsPropertyExist(actor, "CompanyTag") => actor.CompanyTag,
                    "[level]"  when IsPropertyExist(actor, "Level") => actor.Level.ToString(),
                    _ => ""
                };
        }
        
        public static string GenerateFormattedTextFromTags(dynamic actor, string text){
            var matches = Regex.Matches(text, @"\[(.*?)\]");
            return matches.Cast<Match>().Aggregate(text, (current, m) => current.Replace(m.Value, ReplaceTagWithString(m.Value, actor)));
        }
    }
}