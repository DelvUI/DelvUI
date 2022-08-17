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
                int language = (int)Plugin.ClientState.ClientLanguage;
                if (language < 0 || language >= strings.Length) 
                {
                    language = (int)ClientLanguage.English;
                }

                return strings[language];
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
            [9259] = new string[] { "グランドクロス・アルファ", "Grand Cross Alpha", "Supernova Alpha", "Croix suprême alpha" },
            [9260] = new string[] { "グランドクロス・デルタ", "Grand Cross Delta", "Supernova Delta", "Croix suprême delta" },
            [9261] = new string[] { "グランドクロス・オメガ", "Grand Cross Omega", "Supernova Omega", "Croix suprême oméga" },
            [27174] = new string[] { "近思の魔撃", "Nearsight", "Blick nach Innen", "Frappe introspective" },
            [27175] = new string[] { "遠見の魔撃", "Farsight", "Blick in die Ferne", "Frappe visionnaire" },
            [28049] = new string[] { "フレイム・オブ・アスカロン", "Flames of Ascalon", "Flamme von Askalon", "Feu d'Ascalon" },
            [28050] = new string[] { "アイス・オブ・アスカロン", "Ice of Ascalon", "Eis von Askalon", "Glace d'Ascalon" },
            [28051] = new string[] { "騎竜剣アク・モーン", "Akh Morn's Edge", "Akh Morns Klinge", "Lame d'Akh Morn" },
            [28052] = new string[] { "騎竜剣アク・モーン", "Akh Morn's Edge", "Akh Morns Klinge", "Lame d'Akh Morn" },
            [28053] = new string[] { "騎竜剣アク・モーン", "Akh Morn's Edge", "Akh Morns Klinge", "Lame d'Akh Morn" },
            [28054] = new string[] { "騎竜剣アク・モーン", "Akh Morn's Edge", "Akh Morns Klinge", "Lame d'Akh Morn" },
            [28055] = new string[] { "騎竜剣アク・モーン", "Akh Morn's Edge", "Akh Morns Klinge", "Lame d'Akh Morn" },
            [28056] = new string[] { "騎竜剣アク・モーン", "Akh Morn's Edge", "Akh Morns Klinge", "Lame d'Akh Morn" },
            [28057] = new string[] { "騎竜剣ギガフレア", "Gigaflare's Edge", "Gigaflare-Klinge", "Lame de GigaBrasier" },
            [28058] = new string[] { "騎竜剣ギガフレア", "Gigaflare's Edge", "Gigaflare-Klinge", "Lame de GigaBrasier" },
            [28059] = new string[] { "騎竜剣エクサフレア", "Exaflare's Edge", "Exaflare-Klinge", "Lame d'ExaBrasier" },
            [28060] = new string[] { "騎竜剣エクサフレア", "Exaflare's Edge", "Exaflare-Klinge", "Lame d'ExaBrasier" },
            [28061] = new string[] { "騎竜剣エクサフレア", "Exaflare's Edge", "Exaflare-Klinge", "Lame d'ExaBrasier" },
            [28114] = new string[] { "騎竜剣ギガフレア", "Gigaflare's Edge", "Gigaflare-Klinge", "Lame de GigaBrasier" },
            [28115] = new string[] { "騎竜剣ギガフレア", "Gigaflare's Edge", "Gigaflare-Klinge", "Lame de GigaBrasier" },
            [28206] = new string[] { "騎竜剣モーン・アファー", "Morn Afah's Edge", "Morn Afahs Klinge", "Lame de Morn Afah" },
            [28207] = new string[] { "騎竜剣モーン・アファー", "Morn Afah's Edge", "Morn Afahs Klinge", "Lame de Morn Afah" },
            [28208] = new string[] { "騎竜剣モーン・アファー", "Morn Afah's Edge", "Morn Afahs Klinge", "Lame de Morn Afah" },
            [28209] = new string[] { "騎竜剣モーン・アファー", "Morn Afah's Edge", "Morn Afahs Klinge", "Lame de Morn Afah" },
            [28280] = new string[] { "半神の双撃", "Demigod Double", "Hemitheischer Hieb", "Gémellité du demi-dieu" },
            [29452] = new string[] { "騎竜剣アク・モーン", "Akh Morn's Edge", "Akh Morns Klinge", "Lame d'Akh Morn" },
            [29453] = new string[] { "騎竜剣アク・モーン", "Akh Morn's Edge", "Akh Morns Klinge", "Lame d'Akh Morn" },
            [29454] = new string[] { "騎竜剣アク・モーン", "Akh Morn's Edge", "Akh Morns Klinge", "Lame d'Akh Morn" },
            [29455] = new string[] { "騎竜剣モーン・アファー", "Morn Afah's Edge", "Morn Afahs Klinge", "Lame de Morn Afah" },
            [29456] = new string[] { "騎竜剣モーン・アファー", "Morn Afah's Edge", "Morn Afahs Klinge", "Lame de Morn Afah" },
            [29457] = new string[] { "騎竜剣モーン・アファー", "Morn Afah's Edge", "Morn Afahs Klinge", "Lame de Morn Afah" },
            [29752] = new string[] { "アルティメットエンド・オルタナ", "Alternative End", "Ein neues Ende", "Fin alternativ" }
        };

        private static Dictionary<uint, string[]> StatusNameMap = new Dictionary<uint, string[]>()
        {
            [1379] = new string[] { "アルマゲスト", "Almagest", "Almagest", "Almageste" },
            [2748] = new string[] { "盟友の想い", "Soul of Friendship", "Essenz der Freundschaft", "Amitié éternelle" },
            [2749] = new string[] { "巫女の想い", "Soul of Devotion", "Essenz der Tugend", "Dévotion éternelle" },
            [2758] = new string[] { "復讐の炎", "Spreading Flames", "Flammende Rache", "Vengeance consumante" },
            [2759] = new string[] { "道連れの炎", "Entangled Flames", "Verwobene Flammen", "Flammes enchevêtrées" },
            [2777] = new string[] { "爪牙不変", "Bound and Determined", "Nidhoggs Stigmata", "Stigmates de Nidhogg" },
            [2800] = new string[] { "腐呪のクラミュス", "Casting Chlamys", "Faulende Chlamys", "Chlamyde maudite" },
            [2801] = new string[] { "属性耐性低下", "Elemental Resistance Down", "Resistenz -", "Résistance élémentaire réduite" },
            [2802] = new string[] { "三役の腐呪", "Role Call", "Dreifäulenoper", "Rôle tragique" },
            [2803] = new string[] { "三役の抗呪", "Miscast", "Zwischenakt", "Résistance tragique" },
            [2804] = new string[] { "エーテルソーン", "Thornpricked", "Ätherdorn", "Épines d'éther" },
            [2895] = new string[] { "不殺の誓い", "Solemn Vow", "Schwur des Friedens", "Serment de paix" },
            [2896] = new string[] { "滅殺の誓い", "Mortal Vow", "Schwur der Vergeltung", "Vœu d'anéantissement" },
            [2897] = new string[] { "滅殺の償い", "Mortal Atonement", "Versiegte Vergeltung", "Vœu d'anéantissement rompu" },
            [2925] = new string[] { "攻手の魔呪", "Acting DPS", "Fluch des Schmerzes", "Malédiction des attaquants" },
            [2926] = new string[] { "癒手の魔呪", "Acting Healer", "Fluch des Lebens", "Malédiction des guérisseurs" },
            [2927] = new string[] { "守手の魔呪", "Acting Tank", "Fluch des Schutzes", "Malédiction des protecteurs" },
            [2977] = new string[] { "被回復低下", "HP Recovery Down", "Heilung -", "Soins diminués" },
            [2978] = new string[] { "被回復低下", "HP Recovery Down", "Heilung -", "Soins diminués" }
        };

        private static Dictionary<uint, string[]> StatusDescriptionMap = new Dictionary<uint, string[]>()
        {
            [1379] = new string[] { "天体魔法を受けた状態。ＨＰが徐々に失われる。", "Celestial magicks are causing damage over time.", "Erleidet schrittweise Schaden durch außerweltliche Magie.", "Sous l'effet d'une magie astrale. Des dégâts périodiques sont subis." },
            [2748] = new string[] { "盟友に託された想いによって、邪竜の右眼への攻撃が可能な状態。", "A beloved friend is making it possible to attack Nidhogg's right eye.", "Eines Freundes inniger Wunsch ermöglicht den Angriff auf Nidhoggs rechtes Auge.", "Un ami cher permet d'attaquer l'œil droit de Nidhogg." },
            [2749] = new string[] { "氷の巫女に託された想いによって、邪竜の左眼への攻撃が可能な状態。", "A faithful ally is making it possible to attack Nidhogg's left eye.", "Eisherz' inniger Wunsch ermöglicht den Angriff auf Nidhoggs linkes Auge.", "Cœur-de-glace permet d'attaquer l'œil gauche de Nidhogg." },
            [2758] = new string[] { "邪竜ニーズヘッグの復讐を望む念に縛られた状態。", "Powerless against Nidhogg's desire for vengeance.", "Gefesselt von der Macht Nidhoggs unbändigen Durstes nach Vergeltung.", "La cible est assujettie par la volonté de vengeance de Nidhogg." },
            [2759] = new string[] { "邪竜ニーズヘッグの道連れを望む念に縛られた状態。", "Powerless against Nidhogg's desire that another share his suffering.", "Wehrlos gegen Nidhoggs Wunsch, sein Leid auch andere spüren zu lassen.", "La cible est assujettie par la volonté d'enchevêtrement de Nidhogg." },
            [2777] = new string[] { "邪竜の爪と邪竜の牙の性質が変化しなくなった状態。", "Unable to transition between Clawbound and Fangbound states.", "Kein Zustandswechsel zwischen Nidhoggs Fang und Klaue möglich.", "Les caractéristiques de la griffe et du croc de Nidhogg sont immuables." },
            [2800] = new string[] { "エーテルクラミュスに三役の呪毒を蓄えた状態。", "Chlamys is replete with the cursed aether of one of three roles.", "Der Fluch der Dreifäulenoper erfüllt die Fasern der Chlamys.", "L'éther de la chlamyde est maudit." },
            [2801] = new string[] { "何らかの属性に対する耐性が低下した状態。", "Resistance to all elements is reduced.", "Diverse Resistenzen sind verringert.", "Résistance élémentaire rêduite La résistance aux élémentsest rêduite." },
            [2802] = new string[] { "三役の腐呪がかけられた状態。他のプレイヤーと接触した場合に呪いを移す。効果終了時に、特定のロールに対して大ダメージを与える。", "Cast as the receptacle for cursed aether. Effect may be transferred by comming into contact with another player. When this effect expires, players of a certain role will take massive damage.", "Ein Protagonist in der Dreifäulenoper. Der Fluch wird bei Kontakt auf anderen Personen übertragen. Bei Ende des Effekts erleiden bestimmte Rollen verheerenden Schaden.", "Une malédiction a été infligée et peut se transmettre via contact direct avec un autre joueur. Inflige des dégâts considérables à certains rôles lorsque l'effet prend fin." },
            [2803] = new string[] { "三役の腐呪にかからなくなった状態。", "No longer subject to the effects of Role Call.", "Gegen den Fluch der Dreifäulenoper immunisiert.", "La malédiction e été levée." },
            [2804] = new string[] { "エーテルソーンが打ち込まれた状態。効果終了時に内部のエーテルが解放され、攻撃が実行される。", "Flesh has been pierced by aetherial barbs. When this effect expires, the thorns' aether will disperse, resulting in attack damage.", "Ein Ätherdorn bohrt sich in das Fleisch. Bei Ende des Effekts wird der Äther im Inneren freigesetzt.", "Des épines d'éther ont transpercé la chair. Lorsque l'effet prend funm leur éther est libéré et inflige des dégâts." },
            [2895] = new string[] { "聖竜フレースヴェルグが、愛するシヴァを喰らった際に立てた不殺の誓い。", "Recognized under the oath Hraesvelgr swore to his beloved Shiva─that he would never kill her kin.", "Hält sein Versprechen an Shiva, kein fremdes Blut mehr zu vergießen.", "Hraesvelgr a promis à Shiva de ne plus tuer d'humains." },
            [2896] = new string[] { "邪竜ニーズヘッグが愛する詩竜を失った際に立てた滅殺の誓い、その対象となった状態。自身からのＨＰ回復効果が低下し、かつＨＰが徐々に失われる。また効果終了時に、周囲に苦痛を与える。", "Condemned by Nidhogg's vow to avenge his brood-sister. Healing potency is decreased. Taking damage over time, and will inflict anguish on those nearby in turn when this effect expires.", "Unter Einfluss von Nidhoggs Schwur, sich an Ratatoskrs Mördern zu rächen. Erhaltene Heileffekte sind verringert und es wird schrittweise Schaden erlitten. Bei Ende des Effekts wird allen Umstehenden schlimmer Schmerz zugefügt.", "Cible de la haine de Nidhogg ayant pour cause la perte de Ratatosk. Les dégâts infligés et la puissance des effets curatifs prodigués sont réduits, et des dégâts périodiques sont subis. Lorsque l'effet prend fin, une douleur atroce est infligée aux alentours." },
            [2897] = new string[] { "滅殺の誓いの対象から外れた状態。", "No longer condemned by Nidhogg's Mortal Vow.", "Von Nidhoggs Schwur der Vergeltung befreit.", "La cible est libérée du vœu d'anéantissement de Nidhogg." },
            [2925] = new string[] { "攻手の魔呪がかけられた状態。効果終了時に、DPS以外のロールに対して大ダメージを与える。なお、特定の攻撃を受けると、大ダメージを受けることなく効果が解除される。", "When this effect expires, non-DPS will sustain heavy damage. However, being hit by certain attacks will remove this effect without the resulting damage.", "Mit dem Fluch des Schmerzes belegt. Bei Ende des Effekts erleiden Darsteller, die nicht der Rolle eines Angreifers spielen, enormen Schaden. Werden bestimmte Attacken erlitten, endet der Effekt jedoch ohne jeglichen Schaden.", "Les DPS sont maudits. Lorsque l'effet prend fin, les deux autres rôles subissent des dégâts considérables. Recevoir certains attaques permet d'annuler l'effet sans subir lesdits dégâts." },
            [2926] = new string[] { "癒手の魔呪がかけられた状態。効果終了時に、ヒーラー以外のロールに対して大ダメージを与える。 なお、特定の攻撃を受けると、大ダメージを受けることなく効果が解除される。", "When this effect expires, non-Healer will sustain heavy damage. However, being hit by certain attacks will remove this effect without the resulting damage.", "Mit dem Fluch des Lebens belegt. Bei Ende des Effekts erleiden Darsteller, die nicht der Rolle eines Heilers spielen, enormen Schaden. Werden bestimmte Attacken erlitten, endet der Effekt jedoch ohne jeglichen Schaden.", "Les soigneurs sont maudits. Lorsque l'effet prend fin, les deux autres rôles subissent des dégâts considérables. Recevoir certains attaques permet d'annuler l'effet sans subir lesdits dégâts." },
            [2927] = new string[] { "守手の魔呪がかけられた状態。効果終了時に、タンク以外のロールに対して大ダメージを与える。なお、特定の攻撃を受けると、大ダメージを受けることなく効果が解除される。", "When this effect expires, non-Tanks will sustain heavy damage. However, being hit by certain attacks will remove this effect without the resulting damage.", "Mit dem Fluch des Schutzes belegt. Bei Ende des Effekts erleiden Darsteller, die nicht der Rolle eines Verteidigers spielen, enormen Schaden. Werden bestimmte Attacken erlitten, endet der Effekt jedoch ohne jeglichen Schaden.", "Les tanks sont maudits. Lorsque l'effet prend fin, les deux autres rôles subissent des dégâts considérables. Recevoir certains attaques permet d'annuler l'effet sans subir lesdits dégâts." },
            [2977] = new string[] { "自身に対するＨＰ回復効果が20％低下した状態。", "HP recovery is reduced by 20%.", "Erhaltene Heileffekte sind um 20 % verringert.", "L'effet des sorts de restauration des PV est réduit de 20%." },
            [2978] = new string[] { "自身に対するＨＰ回復効果が100％低下した状態。", "HP recovery is reduced by 100%.", "Erhaltene Heileffekte sind um 100 % verringert.", "L'effet des sorts de restauration des PV est réduit de 100%." }
        };
    }
}
