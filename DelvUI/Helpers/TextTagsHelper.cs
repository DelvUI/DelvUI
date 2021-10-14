using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace DelvUI.Helpers
{
    public class TextTag
    {
        // example values are used for tooltips to show how the text tag will behave
        public readonly object[]? ExampleValues;
        public object? ExampleValue => ExampleValues != null && ExampleValues.Length > 0 ? ExampleValues[0] : null;

        public readonly string? ExplicitHelpText;

        // actor, forced values, result
        protected Func<GameObject?, object[]?, string>? Func;

        public TextTag(object[]? exampleValues, Func<GameObject?, object[]?, string>? func, string? helpText = null)
        {
            Func = func;
            ExampleValues = exampleValues;
            ExplicitHelpText = helpText;
        }

        public virtual string Execute(GameObject? actor, object[]? values)
        {
            return Func != null ? Func.Invoke(actor, values) : "";
        }
    }

    public class CharacterTextTag : TextTag
    {
        private Func<Character, object[]?, string> CharacterFunc;

        public CharacterTextTag(object[]? exampleValues, Func<Character, object[]?, string> func, string? helpText = null)
            : base(exampleValues, null, helpText)
        {
            CharacterFunc = func;
        }

        public override string Execute(GameObject? actor, object[]? values)
        {
            if (actor is not Character chara)
            {
                return "";
            }

            return CharacterFunc.Invoke(chara, values);
        }
    }

    public static class TextTagsHelper
    {
        public static Dictionary<string, TextTag> TextTags = new Dictionary<string, TextTag>()
        {
            #region name
            ["[name]"] = new TextTag(
                new object[] { "firstName lastName" },
                (actor, values) => ValidateName(actor, values).CheckForUpperCase()
            ),

            ["[name:first]"] = new TextTag(
                new object[] { "firstName lastName" },
                (actor, values) => ValidateName(actor, values).FirstName().CheckForUpperCase()
            ),

            ["[name:first-initial]"] = new TextTag(
                new object[] { "firstName lastName" },
                (actor, values) =>
                {
                    string name = ValidateName(actor, values).FirstName().CheckForUpperCase();
                    return name.Length > 0 ? name[..1] : "";
                }
            ),

            ["[name:first-npcmedium]"] = new TextTag(
                new object[] { "veryLongFirstName veryLongLastName" },
                (actor, values) =>
                {
                    string name = ValidateName(actor, values);
                    return actor?.ObjectKind == ObjectKind.Player ?
                        name.FirstName().CheckForUpperCase() :
                        name.Truncate(15).CheckForUpperCase();
                }
            ),

            ["[name:first-npclong]"] = new TextTag(
                new object[] { "veryLongFirstName veryLongLastName" },
                (actor, values) =>
                {
                    string name = ValidateName(actor, values);
                    return actor?.ObjectKind == ObjectKind.Player ?
                        name.FirstName().CheckForUpperCase() :
                        name.Truncate(15).CheckForUpperCase();
                }
            ),

            ["[name:first-npcfull]"] = new TextTag(
                new object[] { "veryLongFirstName veryLongLastName" },
                (actor, values) =>
                {
                    string name = ValidateName(actor, values);
                    return actor?.ObjectKind == ObjectKind.Player ?
                        name.FirstName().CheckForUpperCase() :
                        name.CheckForUpperCase();
                }
            ),

            ["[name:last]"] = new TextTag(
                new object[] { "firstName lastName" },
                (actor, values) => ValidateName(actor, values).LastName().CheckForUpperCase()
            ),

            ["[name:last-initial]"] = new TextTag(
                new object[] { "firstName lastName" },
                (actor, values) =>
                {
                    string name = ValidateName(actor, values).LastName().CheckForUpperCase();
                    return name.Length > 0 ? name[..1] : "";
                }
            ),

            ["[name:initials]"] = new TextTag(
                new object[] { "firstName lastName" },
                (actor, values) => ValidateName(actor, values).Initials().CheckForUpperCase()
            ),

            ["[name:abbreviate]"] = new TextTag(
                new object[] { "firstName lastName" },
                (actor, values) => ValidateName(actor, values).Abbreviate().CheckForUpperCase()
            ),

            ["[name:veryshort]"] = new TextTag(
                new object[] { "veryLongFirstName veryLongLastName" },
                (actor, values) => ValidateName(actor, values).Truncate(5).CheckForUpperCase()
            ),

            ["[name:short]"] = new TextTag(
                new object[] { "veryLongFirstName veryLongLastName" },
                (actor, values) => ValidateName(actor, values).Truncate(10).CheckForUpperCase()
            ),

            ["[name:medium]"] = new TextTag(
                new object[] { "veryLongFirstName veryLongLastName" },
                (actor, values) => ValidateName(actor, values).Truncate(15).CheckForUpperCase()
            ),

            ["[name:long]"] = new TextTag(
                new object[] { "veryLongFirstName veryLongLastName" },
                (actor, values) => ValidateName(actor, values).Truncate(20).CheckForUpperCase()
            ),
            #endregion

            #region health
            ["[health:current]"] = new CharacterTextTag(
                new object[] { 69420 },
                (chara, values) =>
                {
                    uint hp = ValidateValue<uint?>(values, 0) ?? chara.CurrentHp;
                    return hp.ToString();
                }
            ),

            ["[health:current-short]"] = new CharacterTextTag(
                new object[] { 69420 },
                (chara, values) =>
                {
                    uint hp = ValidateValue<uint?>(values, 0) ?? chara.CurrentHp;
                    return hp.KiloFormat();
                }
            ),

            ["[health:current-percent]"] = new CharacterTextTag(
                new object[] { 69000, 100000 },
                (chara, values) =>
                {
                    uint hp = ValidateValue<uint?>(values, 0) ?? chara.CurrentHp;
                    uint max = ValidateValue<uint?>(values, 1) ?? chara.MaxHp;
                    return hp == max ? hp.ToString() : $"{Math.Round(100f / max * hp)}";
                },
                "Health value if full, otherwise shows percentage"
            ),

            ["[health:current-percent-short]"] = new CharacterTextTag(
                new object[] { 100000, 100000 },
                (chara, values) =>
                {
                    uint hp = ValidateValue<uint?>(values, 0) ?? chara.CurrentHp;
                    uint max = ValidateValue<uint?>(values, 1) ?? chara.MaxHp;
                    return hp == max ? hp.KiloFormat() : $"{Math.Round(100f / max * hp)}";
                },
                "Health value if full, otherwise shows percentage"
            ),

            ["[health:current-max]"] = new CharacterTextTag(
                new object[] { 69000, 100000 },
                (chara, values) =>
                {
                    uint hp = ValidateValue<uint?>(values, 0) ?? chara.CurrentHp;
                    uint max = ValidateValue<uint?>(values, 1) ?? chara.MaxHp;
                    return $"{hp}  |  {max}";
                },
                "Current Health  |  Max Health)"
            ),

            ["[health:current-max-short]"] = new CharacterTextTag(
                new object[] { 69000, 100000 },
                (chara, values) =>
                {
                    uint hp = ValidateValue<uint?>(values, 0) ?? chara.CurrentHp;
                    uint max = ValidateValue<uint?>(values, 1) ?? chara.MaxHp;
                    return $"{hp.KiloFormat()}  |  {max.KiloFormat()}";
                },
                "Current Health  |  Max Health"
            ),

            ["[health:max]"] = new CharacterTextTag(
                new object[] { 69420 },
                (chara, values) =>
                {
                    uint hp = ValidateValue<uint?>(values, 0) ?? chara.MaxHp;
                    return hp.ToString();
                }
            ),

            ["[health:max-short]"] = new CharacterTextTag(
                new object[] { 69420 },
                (chara, values) =>
                {
                    uint hp = ValidateValue<uint?>(values, 0) ?? chara.MaxHp;
                    return hp.KiloFormat();
                }
            ),

            ["[health:percent]"] = new CharacterTextTag(
                new object[] { 69420, 10000 },
                (chara, values) =>
                {
                    uint hp = ValidateValue<uint?>(values, 0) ?? chara.CurrentHp;
                    uint max = ValidateValue<uint?>(values, 1) ?? chara.MaxHp;
                    return $"{Math.Round(100f / hp * max)}";
                },
                "Current Health  |  Max Health)"
            ),

            ["[health:percent-decimal]"] = new CharacterTextTag(
                new object[] { 69420, 10000 },
                (chara, values) =>
                {
                    uint hp = ValidateValue<uint?>(values, 0) ?? chara.CurrentHp;
                    uint max = ValidateValue<uint?>(values, 1) ?? chara.MaxHp;
                    return FormattableString.Invariant($"{100f / max * hp:##0.#}");
                },
                "Current Health  |  Max Health)"
            ),

            ["[health:deficit]"] = new CharacterTextTag(
                new object[] { 69420, 10000 },
                (chara, values) =>
                {
                    uint hp = ValidateValue<uint?>(values, 0) ?? chara.CurrentHp;
                    uint max = ValidateValue<uint?>(values, 1) ?? chara.MaxHp;
                    return hp == max ? "0" : $"-{max - hp}";
                },
                "Current Health  |  Max Health)"
            ),

            ["[health:deficit-short]"] = new CharacterTextTag(
                new object[] { 69420, 10000 },
                (chara, values) =>
                {
                    uint hp = ValidateValue<uint?>(values, 0) ?? chara.CurrentHp;
                    uint max = ValidateValue<uint?>(values, 1) ?? chara.MaxHp;
                    return hp == max ? "0" : $"-{(max - hp).KiloFormat()}";
                },
                "Current Health  |  Max Health)"
            ),
            #endregion

            #region mana
            ["[mana:current]"] = new CharacterTextTag(
                new object[] { 69420 },
                (chara, values) =>
                {
                    uint mp = ValidateValue<uint?>(values, 0) ?? JobsHelper.CurrentPrimaryResource(chara);
                    return mp.ToString();
                }
            ),

            ["[mana:current-short]"] = new CharacterTextTag(
                new object[] { 69420 },
                (chara, values) =>
                {
                    uint mp = ValidateValue<uint?>(values, 0) ?? JobsHelper.CurrentPrimaryResource(chara);
                    return mp.KiloFormat();
                }
            ),

            ["[mana:current-percent]"] = new CharacterTextTag(
                new object[] { 69000, 100000 },
                (chara, values) =>
                {
                    uint mp = ValidateValue<uint?>(values, 0) ?? JobsHelper.CurrentPrimaryResource(chara);
                    uint max = ValidateValue<uint?>(values, 1) ?? JobsHelper.MaxPrimaryResource(chara);
                    return mp == max ? mp.ToString() : $"{Math.Round(100f / max * mp)}";
                },
                "Mana value if full, otherwise shows percentage"
            ),

            ["[mana:current-percent-short]"] = new CharacterTextTag(
                new object[] { 100000, 100000 },
                (chara, values) =>
                {
                    uint mp = ValidateValue<uint?>(values, 0) ?? JobsHelper.CurrentPrimaryResource(chara);
                    uint max = ValidateValue<uint?>(values, 1) ?? JobsHelper.MaxPrimaryResource(chara);
                    return mp == max ? mp.KiloFormat() : $"{Math.Round(100f / max * mp)}";
                },
                "Mana value if full, otherwise shows percentage"
            ),

            ["[mana:current-max]"] = new CharacterTextTag(
                new object[] { 69000, 100000 },
                (chara, values) =>
                {
                    uint mp = ValidateValue<uint?>(values, 0) ?? JobsHelper.CurrentPrimaryResource(chara);
                    uint max = ValidateValue<uint?>(values, 1) ?? JobsHelper.MaxPrimaryResource(chara);
                    return $"{mp}  |  {max}";
                },
                "Current Mana  |  Max Mana)"
            ),

            ["[mana:current-max-short]"] = new CharacterTextTag(
                new object[] { 69000, 100000 },
                (chara, values) =>
                {
                    uint mp = ValidateValue<uint?>(values, 0) ?? JobsHelper.CurrentPrimaryResource(chara);
                    uint max = ValidateValue<uint?>(values, 1) ?? JobsHelper.MaxPrimaryResource(chara);
                    return $"{mp.KiloFormat()}  |  {max.KiloFormat()}";
                },
                "Current Mana  |  Max Mana"
            ),

            ["[mana:max]"] = new CharacterTextTag(
                new object[] { 69420 },
                (chara, values) =>
                {
                    uint mp = ValidateValue<uint?>(values, 0) ?? JobsHelper.CurrentPrimaryResource(chara);
                    return mp.ToString();
                }
            ),

            ["[mana:max-short]"] = new CharacterTextTag(
                new object[] { 69420 },
                (chara, values) =>
                {
                    uint mp = ValidateValue<uint?>(values, 0) ?? JobsHelper.CurrentPrimaryResource(chara);
                    return mp.KiloFormat();
                }
            ),

            ["[mana:percent]"] = new CharacterTextTag(
                new object[] { 69420, 10000 },
                (chara, values) =>
                {
                    uint mp = ValidateValue<uint?>(values, 0) ?? JobsHelper.CurrentPrimaryResource(chara);
                    uint max = ValidateValue<uint?>(values, 1) ?? JobsHelper.MaxPrimaryResource(chara);
                    return $"{Math.Round(100f / mp * max)}";
                },
                "Current Mana  |  Max Mana"
            ),

            ["[mana:percent-decimal]"] = new CharacterTextTag(
                new object[] { 69420, 10000 },
                (chara, values) =>
                {
                    uint mp = ValidateValue<uint?>(values, 0) ?? JobsHelper.CurrentPrimaryResource(chara);
                    uint max = ValidateValue<uint?>(values, 1) ?? JobsHelper.MaxPrimaryResource(chara);
                    return FormattableString.Invariant($"{100f / max * mp:##0.#}");
                },
                "Current Mana  |  Max Mana"
            ),

            ["[mana:deficit]"] = new CharacterTextTag(
                new object[] { 69420, 10000 },
                (chara, values) =>
                {
                    uint mp = ValidateValue<uint?>(values, 0) ?? JobsHelper.CurrentPrimaryResource(chara);
                    uint max = ValidateValue<uint?>(values, 1) ?? JobsHelper.MaxPrimaryResource(chara);
                    return mp == max ? "0" : $"-{max - mp}";
                },
                "Current Mana  |  Max Mana"
            ),

            ["[mana:deficit-short]"] = new CharacterTextTag(
                new object[] { 69420, 10000 },
                (chara, values) =>
                {
                    uint mp = ValidateValue<uint?>(values, 0) ?? JobsHelper.CurrentPrimaryResource(chara);
                    uint max = ValidateValue<uint?>(values, 1) ?? JobsHelper.MaxPrimaryResource(chara);
                    return mp == max ? "0" : $"-{(max - mp).KiloFormat()}";
                },
                "Current Mana  |  Max Mana"
            ),
            #endregion

            #region misc
            ["[distance]"] = new CharacterTextTag(
                null,
                (chara, values) => (chara.YalmDistanceX + 1).ToString(),
                "Distance to the character"
            ),

            ["[company]"] = new CharacterTextTag(
                null,
                (chara, values) => chara.CompanyTag.ToString(),
                "Company name"
            ),

            ["[level]"] = new CharacterTextTag(
                null,
                (chara, values) => chara.Level.ToString(),
                "Level of the character"
            ),

            ["[job]"] = new CharacterTextTag(
                null,
                (chara, values) => JobsHelper.JobNames.TryGetValue(chara.ClassJob.Id, out var jobName) ? jobName : "",
                "Job of the player (ie \"BLM\")"
            ),
            #endregion

            #region experience
            ["[exp:current]"] = new TextTag(
                new object[] { 100000 },
                (actor, values) =>
                {
                    uint value = ValidateValue<uint?>(values, 0) ?? ExperienceHelper.Instance.CurrentExp;
                    return value.ToString("N0", CultureInfo.InvariantCulture);
                }
            ),

            ["[exp:current-short]"] = new TextTag(
                new object[] { 100000 },
                (actor, values) =>
                {
                    uint value = ValidateValue<uint?>(values, 0) ?? ExperienceHelper.Instance.CurrentExp;
                    return value.KiloFormat();
                }
            ),

            ["[exp:required]"] = new TextTag(
                new object[] { 100000 },
                (actor, values) =>
                {
                    uint value = ValidateValue<uint?>(values, 0) ?? ExperienceHelper.Instance.RequiredExp;
                    return value.ToString("N0", CultureInfo.InvariantCulture);
                }
            ),

            ["[exp:required-short]"] = new TextTag(
                new object[] { 100000 },
                (actor, values) =>
                {
                    uint value = ValidateValue<uint?>(values, 0) ?? ExperienceHelper.Instance.RequiredExp;
                    return value.KiloFormat();
                }
            ),

            ["[exp:rested]"] = new TextTag(
                new object[] { 100000 },
                (actor, values) =>
                {
                    uint value = ValidateValue<uint?>(values, 0) ?? ExperienceHelper.Instance.RestedExp;
                    return value.ToString("N0", CultureInfo.InvariantCulture);
                }
            ),

            ["[exp:rested-short]"] = new TextTag(
                new object[] { 100000 },
                (actor, values) =>
                {
                    uint value = ValidateValue<uint?>(values, 0) ?? ExperienceHelper.Instance.RestedExp;
                    return value.KiloFormat();
                }
            ),
            #endregion
        };

        private static string ReplaceTagWithString(string tag, GameObject? actor, object[]? forcedValues = null)
        {
            if (TextTags.TryGetValue(tag, out TextTag? textTag) && textTag != null)
            {
                return textTag.Execute(actor, forcedValues);
            }

            return "";
        }

        public static string FormattedText(string text, GameObject? actor, string? name = null)
        {
            object[]? forcedValues = name != null ? new object[] { name } : null;
            return FormattedText(text, actor, forcedValues);
        }

        public static string FormattedText(string text, GameObject? actor, object[]? forcedValues = null)
        {
            MatchCollection matches = Regex.Matches(text, @"\[(.*?)\]");
            return matches.Aggregate(text, (current, m) =>
            {
                string formattedText = ReplaceTagWithString(m.Value, actor, forcedValues);
                return current.Replace(m.Value, formattedText);
            });
        }

        #region utils
        private static T? ValidateValue<T>(object[]? values, int index)
        {
            if (values == null || index > values.Length)
            {
                return default;
            }

            object value = values[index];
            if (value is T castedValue)
            {
                return castedValue;
            }

            return default;
        }

        private static string ValidateName(GameObject? actor, object[]? values)
        {
            if (actor != null)
            {
                return actor.Name?.ToString() ?? "";
            }

            if (values != null && values.Length > 0)
            {
                if (values[0] is string str)
                {
                    return str;
                }
                else if (values[0] is SeString seStr)
                {
                    return seStr.ToString();
                }
            }

            return "";
        }
        #endregion
    }
}