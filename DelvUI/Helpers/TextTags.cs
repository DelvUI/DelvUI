﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;

namespace DelvUI.Helpers
{
    public static class TextTags
    {
        private static string ReplaceTagWithString(string tag, GameObject actor)
        {
            switch (tag)
            {
                case "[name]":
                    return actor.Name.ToString();

                case "[name:first]":
                    return actor.Name.FirstName();

                case "[name:first-initial]":
                    return actor.Name.FirstName().Length == 0 ? "" : actor.Name.FirstName()[..1];

                case "[name:first-npcmedium]":
                    return actor.ObjectKind == ObjectKind.Player ? actor.Name.FirstName() : actor.Name.Truncate(15);
                
                case "[name:first-npclong]":
                    return actor.ObjectKind == ObjectKind.Player ? actor.Name.FirstName() : actor.Name.Truncate(20);
                
                case "[name:first-npcfull]":
                    return actor.ObjectKind == ObjectKind.Player ? actor.Name.FirstName() : actor.Name.ToString();
                
                case "[name:last]":
                    return actor.Name.LastName();

                case "[name:last-initial]":
                    return actor.Name.LastName().Length == 0 ? "" : actor.Name.LastName()[..1];

                case "[name:abbreviate]":
                    return actor.Name.Abbreviate();

                case "[name:veryshort]":
                    return actor.Name.Truncate(5);

                case "[name:short]":
                    return actor.Name.Truncate(10);

                case "[name:medium]":
                    return actor.Name.Truncate(15);

                case "[name:long]":
                    return actor.Name.Truncate(20);
            }

            if (actor is Character character) {
                switch (tag)
                {
                    case "[health:current]":
                        return character.CurrentHp.ToString();

                    case "[health:current-short]":
                        return character.CurrentHp.KiloFormat();

                    case "[health:current-percent]":
                        return character.CurrentHp == character.MaxHp ? character.CurrentHp.ToString() : $"{Math.Round(100f / character.MaxHp * character.CurrentHp)}";

                    case "[health:current-percent-short]":
                        return character.CurrentHp == character.MaxHp ? character.CurrentHp.KiloFormat() : $"{Math.Round(100f / character.MaxHp * character.CurrentHp)}";

                    case "[health:current-max]":
                        return $"{character.CurrentHp.ToString()}  |  {character.MaxHp}";

                    case "[health:current-max-short]":
                        return $"{character.CurrentHp.KiloFormat()}  |  {character.MaxHp.KiloFormat()}";

                    case "[health:current-max-percent]":
                        return character.CurrentHp == character.MaxHp ? $"{Math.Round(100f / character.MaxHp * character.CurrentHp)} - 100" : $"{character.CurrentHp} - {character.MaxHp}";

                    case "[health:current-max-percent-short]":
                        return character.CurrentHp == character.MaxHp
                            ? $"{Math.Round(100f / character.MaxHp * character.CurrentHp)} - 100"
                            : $"{character.CurrentHp.KiloFormat()}  |  {character.MaxHp.KiloFormat()}";

                    case "[health:max]":
                        return character.MaxHp.ToString();

                    case "[health:max-short]":
                        return character.MaxHp.KiloFormat();

                    case "[health:percent]":
                        return $"{Math.Round(100f / character.MaxHp * character.CurrentHp)}";

                    case "[health:percent-decimal]":
                        return $"{100f / character.MaxHp * character.CurrentHp:##0.#}";

                    case "[health:deficit]":
                        return $"-{character.MaxHp - character.CurrentHp}";

                    case "[health:deficit-short]":
                        return $"-{(character.MaxHp - character.CurrentHp).KiloFormat()}";

                    case "[mana:current]":
                        return character.CurrentMp.ToString();

                    case "[mana:current-short]":
                        return character.CurrentMp.KiloFormat();

                    case "[mana:current-percent]":
                        return character.CurrentMp == character.MaxMp ? character.CurrentMp.ToString() : $"{Math.Round(100f / character.MaxMp * character.CurrentMp)}";

                    case "[mana:current-percent-short]":
                        return character.CurrentMp == character.MaxMp ? character.CurrentMp.KiloFormat() : $"{Math.Round(100f / character.MaxMp * character.CurrentMp)}";

                    case "[mana:current-max]":
                        return $"{character.CurrentMp.ToString()}  |  {character.MaxMp}";

                    case "[mana:current-max-short]":
                        return $"{character.CurrentMp.KiloFormat()} | {character.MaxMp.KiloFormat()}";

                    case "[mana:current-max-percent]":
                        return character.CurrentMp == character.MaxMp ? $"{Math.Round(100f / character.MaxMp * character.CurrentMp)}  |  100" : $"{character.CurrentMp} - {character.MaxMp}";

                    case "[mana:current-max-percent-short]":
                        return character.CurrentMp == character.MaxMp
                            ? $"{Math.Round(100f / character.MaxMp * character.CurrentMp)}  |  100"
                            : $"{character.CurrentMp.KiloFormat()} - {character.MaxMp.KiloFormat()}";

                    case "[mana:max]":
                        return character.MaxMp.ToString();

                    case "[mana:max-short]":
                        return character.MaxMp.KiloFormat();

                    case "[mana:percent]":
                        return $"{Math.Round(100f / character.MaxMp * character.CurrentMp)}";

                    case "[mana:percent-decimal]":
                        return $"{100f / character.MaxMp * character.CurrentMp:##0.#}";

                    case "[mana:deficit]":
                        return $"-character.MaxMp-character.CurrentMp";

                    case "[mana:deficit-short]":
                        return $"-{(character.MaxMp - character.CurrentMp).KiloFormat()}";

                    case "[company]":
                        return character.CompanyTag.ToString();

                    case "[level]":
                        return character.Level.ToString();

                    case "[job]":
                        return JobsHelper.JobNames.TryGetValue(character.ClassJob.Id, out var jobName) ? jobName : "";
                }
            }

            return "";
        }

        public static string GenerateFormattedTextFromTags(GameObject actor, string text)
        {
            MatchCollection matches = Regex.Matches(text, @"\[(.*?)\]");
            return matches.Aggregate(text, (current, m) => current.Replace(m.Value, ReplaceTagWithString(m.Value, actor)));
        }
    }
}
