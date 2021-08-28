using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Dalamud.Game.ClientState.Actors.Types;
using static DelvUI.Extensions;

namespace DelvUI.Models
{
    public class ActivePlayerActor
    {
        private int ActorId;
        private int TargetActorId;
        private int CurrentHp;
        private int MaxHp;
        private int CurrentMp;
        private int MaxMp;
        private int CurrentGp;
        private int MaxGp;
        private int CurrentCp;
        private int MaxCp;
        private string Name;
        private string CompanyTag;

        private List<string> UsedTextTags = new List<string>();
        private Dictionary<string, string> TextTags = new Dictionary<string, string>();

        public ActivePlayerActor(PlayerCharacter actor, IEnumerable<string> tags)
        {
            ActorId = actor.ActorId;
            TargetActorId = actor.TargetActorID;
            CurrentHp = actor.CurrentHp;
            MaxHp = actor.MaxHp;
            CurrentMp = actor.CurrentMp;
            MaxMp = actor.MaxMp;
            CurrentGp = actor.CurrentMp;
            MaxGp = actor.MaxGp;
            CurrentCp = actor.CurrentCp;
            MaxCp = actor.MaxCp;
            Name = actor.Name;
            CompanyTag = actor.CompanyTag;
            ExtractTags(tags);
            UpdateInUseTextFields();
        }

        private void ExtractTags(IEnumerable<string> tags)
        {
            foreach (var tag in tags)
            {
                var matches = Regex.Matches(tag, @"\[(.*?)\]");
                foreach (Match m in matches)
                {
                    UsedTextTags.Add(m.Value);
                }
                
            }
        }
        
        public void UpdateInUseTextFields(){
            foreach (var tag in UsedTextTags)
            {
                TextTags[tag] = tag switch
                {
                    // Health
                    "[health:current]" => CurrentHp.ToString(),
                    "[health:current-short]" => CurrentHp.KiloFormat(),
                    "[health:current-percent]" => CurrentHp == MaxHp
                        ? CurrentHp.ToString()
                        : $"{Math.Round(100f / MaxHp * CurrentHp)}%",
                    "[health:current-percent-short]" => CurrentHp == MaxHp
                        ? CurrentHp.KiloFormat()
                        : $"{Math.Round(100f / MaxHp * CurrentHp)}%",
                    "[health:current-max]" => $"{CurrentHp.ToString()} | {MaxHp}",
                    "[health:current-max-short]" => $"{CurrentHp.KiloFormat()} | {MaxHp.KiloFormat()}",
                    "[health:current-max-percent]" => CurrentHp == MaxHp
                        ? $"{Math.Round(100f / MaxHp * CurrentHp)}% - 100%" : $"{CurrentHp} - {MaxHp}",
                    "[health:current-max-percent-short]" => CurrentHp == MaxHp
                        ? $"{Math.Round(100f / MaxHp * CurrentHp)}% - 100%" : $"{CurrentHp.KiloFormat()} | {MaxHp.KiloFormat()}",
                    "[health:max]" => MaxHp.ToString(),
                    "[health:max-short]" => MaxHp.KiloFormat(),
                    "[health:percent]" => $"{Math.Round(100f / MaxHp * CurrentHp)}%",
                    "[health:deficit]" => $"-{MaxHp - CurrentHp}",
                    "[health:deficit-short]" => $"-{(MaxHp - CurrentHp).KiloFormat()}",
                    
                    // Mana
                    "[mana:current]" => CurrentMp.ToString(),
                    "[mana:current-short]" => CurrentMp.KiloFormat(),
                    "[mana:current-percent]" => CurrentMp == MaxMp
                        ? CurrentMp.ToString()
                        : $"{Math.Round(100f / MaxMp * CurrentMp)}%",
                    "[mana:current-percent-short]" => CurrentMp == MaxMp
                        ? CurrentMp.KiloFormat()
                        : $"{Math.Round(100f / MaxMp * CurrentMp)}%",
                    "[mana:current-max]" => $"{CurrentMp.ToString()} | {MaxMp}",
                    "[mana:current-max-short]" => $"{CurrentMp.KiloFormat()} - {MaxMp.KiloFormat()}",
                    "[mana:current-max-percent]" => CurrentMp == MaxMp
                        ? $"{Math.Round(100f / MaxMp * CurrentMp)}% | 100%" : $"{CurrentMp} - {MaxMp}",
                    "[mana:current-max-percent-short]" => CurrentMp == MaxMp
                        ? $"{Math.Round(100f / MaxMp * CurrentMp)}% | 100%" : $"{CurrentMp.KiloFormat()} - {MaxMp.KiloFormat()}",
                    "[mana:max]" => MaxMp.ToString(),
                    "[mana:max-short]" => MaxMp.KiloFormat(),
                    "[mana:percent]" => $"{Math.Round(100f / MaxMp * CurrentMp)}%",
                    "[mana:deficit]" => $"-{MaxMp - CurrentMp}",
                    "[mana:deficit-short]" => $"-{(MaxMp - CurrentMp).KiloFormat()}",
                    
                    // Name
                    "[name]" => Name,
                    "[name:abbreviate]" => Name.Abbreviate(),
                    "[name:veryshort]" => Name.Truncate(5),
                    "[name:short]" => Name.Truncate(10),
                    "[name:medium]" => Name.Truncate(15),
                    "[name:long]" => Name.Truncate(20),
                    "" => "",
                    
                    // Misc
                    "[company]" => CompanyTag,
                    _ => ""
                };
            }
        }

        public string GenerateTextFromTags(string text)
        {
            return TextTags.Aggregate(text, (current, tag) => current.Replace(tag.Key, tag.Value));
        }
    }
}