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
            // p5s
            [30478] = new string[] { "クロウ・アンド・テイル", "Claw to Tail", "Kralle und Schwanz", "Griffes et queue" },
            [30482] = new string[] { "テイル・アンド・クロウ", "Tail to Claw", "Schwanz und Kralle", "Queue et griffes" },
            [30491] = new string[] { "ダブルラッシュ", "Double Rush", "Doppelsturm", "Double ruée" },
            [30492] = new string[] { "ダブルラッシュ", "Double Rush", "Doppelsturm", "Double ruée" },

            // p6s
            [30828] = new string[] { "チェンジバースト", "Exchange of Agonies", "Wechselschub", "Panaché ténébreux" },
            [30838] = new string[] { "カヘキシー", "Cachexia", "Cachexia", "Cachexie" },
            [30839] = new string[] { "魔活細胞", "Aetheronecrosis", "Explozelle", "Cellules magiques actives" },
            [30840] = new string[] { "甲軟双撃", "Dual Predation", "Doppelte Prädation", "Double attaque parasitaire" },
            [30841] = new string[] { "甲軟双撃", "Dual Predation", "Doppelte Prädation", "Double attaque parasitaire" },
            [30842] = new string[] { "軟体水撃", "Glossal Predation", "Glossale Prädation", "Attaque de mollusque" },
            [30843] = new string[] { "甲殻爪撃", "Chelic Predation", "Chelische Prädation", "Attaque de crustacé" },
            [30844] = new string[] { "プテラ・イクソス", "Ptera Ixou", "Ptera Ixou", "Ptera Ixou" },
            [30845] = new string[] { "プテラ・イクソス", "Ptera Ixou", "Ptera Ixou", "Ptera Ixou" },
            [30846] = new string[] { "プテラ・イクソス", "Ptera Ixou", "Ptera Ixou", "Ptera Ixou" },
            [30858] = new string[] { "キール・シュネルギア", "Chelic Synergy", "Chelische Synergie", "Synergie chélique" },

            // p7s
            [30746] = new string[] { "魔印創成", "Inviolate Bonds", "Siegelschaffung", "Tracé de sigil" },
            [30750] = new string[] { "魔印創成・獄", "Inviolate Purgation", "Siegelschaffung der Hölle", "Tracé de sigils multiples" },
            [31221] = new string[] { "マルチキャスト", "Multicast", "Multizauber", "Multisort" },
            [31311] = new string[] { "生命の繁茂【猛】", "Famine's Harvest", "Wilde Wucherung des Lebens", "Bourgeonnement de vie féroce" },
            [31312] = new string[] { "生命の繁茂【凶】", "Death's Harvest", "Unheilvolle Wucherung des Lebens", "Bourgeonnement de vie morbide" },
            [31313] = new string[] { "生命の繁茂【乱】", "War's Harvest", "Chaotische Wucherung des Lebens", "Bourgeonnement de vie chaotique" },

            // p8s
            [28938] = new string[] { "概念支配", "High Concept", "Konzeptkontrolle", "Manipulation conceptuelle" },
            [30996] = new string[] { "オクタフレア・コンシーヴ", "Conceptual Octaflare", "Konzeptionelle Oktaflare", "Octobrasier conceptuel" },
            [30997] = new string[] { "テトラフレア・コンシーヴ", "Conceptual Tetraflare", "Konzeptionelle Tetraflare", "Tetrabrasier conceptuel" },
            [30998] = new string[] { "テトラフレア・コンシーヴ", "Conceptual Tetraflare", "Konzeptionelle Tetraflare", "Tetrabrasier conceptuel" },
            [30999] = new string[] { "ディフレア・コンシーヴ", "Conceptual Diflare", "Konzeptionelle Diflare", "Dibrasier conceptuel" },
            [31000] = new string[] { "エマージ・オクタフレア", "Emergent Octaflare", "Steigende Oktaflare", "Octobrasier émergent" },
            [31001] = new string[] { "エマージ・テトラフレア", "Emergent Tetraflare", "Steigende Tetraflare", "Tetrabrasier émergent" },
            [31003] = new string[] { "エマージ・ディフレア", "Emergent Diflare", "Steigende Diflare", "Dibrasier émergent" },
            [31005] = new string[] { "オクタフレア", "Octaflare", "Oktaflare", "Octobrasier" },
            [31006] = new string[] { "テトラフレア", "Tetraflare", "Tetraflare", "Tetrabrasier" },
            [31007] = new string[] { "スプレッドヴァイパー", "Nest of Flamevipers", "Ausbreitende Viper", "Vipère élancée" },
            [31008] = new string[] { "スプレッドヴァイパー", "Nest of Flamevipers", "Ausbreitende Viper", "Vipère élancée" },
            [31009] = new string[] { "多重操炎", "Manifold Flames", "Mannigfaltige Flammen", "Flammes orientées multiples" },
            [31010] = new string[] { "多重操炎", "Manifold Flames", "Mannigfaltige Flammen", "Flammes orientées multiples" },
            [31021] = new string[] { "ゴルゴンの石眼", "Eye of the Gorgon", "Gorgons Steinauge", "Œil pétrifiant de gorgone" },
            [31022] = new string[] { "ゴルゴンの石光", "Crown of the Gorgon", "Gorgons Steinlicht", "Lueur pétrifiante de gorgone" },
            [31023] = new string[] { "ゴルゴンの蛇毒", "Blood of the Gorgon", "Gorgons Schlangengift", "Venin reptilien de gorgone" },
            [31024] = new string[] { "ゴルゴンの邪毒", "Breath of the Gorgon", "Gorgons Übelgift", "Poison insidieux de gorgone" },
            [31030] = new string[] { "フェイタルストンプ", "Stomp Dead", "Fataler Stampfer", "Piétinement mortel" },
            [31031] = new string[] { "フェイタルストンプ", "Stomp Dead", "Fataler Stampfer", "Piétinement mortel" },
            [31032] = new string[] { "ブレイジングフィート", "Blazing Footfalls", "Fackelnde Füße", "Pas ardents" },
            [31033] = new string[] { "トレイルブレイズ", "Trailblaze", "Flammender Pfad", "Traînée ardente" },
            [31038] = new string[] { "トレイルブレイズ", "Trailblaze", "Flammender Pfad", "Traînée ardente" },
            [31040] = new string[] { "炎雨降天", "Rain of Fire", "Feuerregen", "Pluie de feu" },
            [31043] = new string[] { "炎雨降天", "Rain of Fire", "Feuerregen", "Pluie de feu" },
            [31044] = new string[] { "創世の真炎", "Genesis of Flame", "Flammende Genesis", "Flammes de la création" },
            [31050] = new string[] { "創世の真炎", "Genesis of Flame", "Flammende Genesis", "Flammes de la création" },
            [31148] = new string[] { "概念支配", "High Concept", "Konzeptkontrolle", "Manipulation conceptuelle" },
            [31149] = new string[] { "概念変異", "Conceptual Shift", "Konzeptänderung", "Bascule conceptuelle" },
            [31150] = new string[] { "概念変異", "Conceptual Shift", "Konzeptänderung", "Bascule conceptuelle" },
            [31151] = new string[] { "概念変異", "Conceptual Shift", "Konzeptänderung", "Bascule conceptuelle" },
            [31152] = new string[] { "概念生成", "Conception", "Konzeptumsetzung", "Conceptualisation" },
            [31153] = new string[] { "混沌概念", "Failure of Imagination", "Chaotisches Konzept", "Désordre conceptuel" },
            [31154] = new string[] { "概念反発", "Splicer", "Konzeptreflektion", "Réaction conceptuelle" },
            [31155] = new string[] { "概念反発", "Splicer", "Konzeptreflektion", "Réaction conceptuelle" },
            [31156] = new string[] { "概念反発", "Splicer", "Konzeptreflektion", "Réaction conceptuelle" },
            [31157] = new string[] { "不死鳥創造", "Everburn", "Phoinix-Erschaffung", "Oiseau immortel" },
            [31158] = new string[] { "魔法陣起動", "Arcane Control", "Beleben des Kreises", "Activation arcanique" },
            [31159] = new string[] { "魔陣波動", "Arcane Channel", "Zirkelimpuls", "Vague arcanique" },
            [31160] = new string[] { "魔陣大波動", "Arcane Wave", "Großer Zirkelimpuls", "Grande vague arcanique" },
            [31162] = new string[] { "自己概念崩壊", "Ego Death", "Egotod", "Destruction de l'ego" },
            [31163] = new string[] { "術式記述", "Natural Alignment", "Rituelle Anpassung", "Description rituelle" },
            [31164] = new string[] { "強制詠唱", "Twist Nature", "Zwangsbeschwörung", "Incantation forcée" },
            [31165] = new string[] { "フォースド・トリファイア", "Forcible Trifire", "Erzwungenes Trifeuer", "Tri Feu forcé" },
            [31166] = new string[] { "フォースド・ディフリーズ", "Forcible Difreeze", "Erzwungenes Di-Einfrieren", "Di Gel forcé" },
            [31167] = new string[] { "フォースド・ファイガ", "Forcible Fire III", "Erzwungenes Feuga", "Méga Feu forcé" },
            [31168] = new string[] { "フォースド・ファイラ", "Forcible Fire II", "Erzwungenes Feura", "Extra Feu forcé" },
            [31169] = new string[] { "フォースド・ファイジャ", "Forcible Fire IV", "Erzwungenes Feuka", "Giga Feu forcé" },
            [31170] = new string[] { "マジックインヴァージョン", "Inverse Magicks", "Magische Umkehr", "Inversion magique" },
            [31171] = new string[] { "万象魔操", "Fates Unending", "Kosmisches Schicksal", "Magie universelle" },
            [31172] = new string[] { "禁忌の混沌", "Unsightly Chaos", "Verbotenes Chaos", "Chaos défendu" },
            [31173] = new string[] { "禁忌の混沌", "Unsightly Chaos", "Verbotene Ordnung", "Chaos défendu" },
            [31176] = new string[] { "幻影創造：葬送", "Illusory Soul", "Schattenschöpfung: Trauerzug", "Création d'ombres funéraire" },
            [31177] = new string[] { "身魂剥離", "Soul Strand", "Seelenband", "Découpe d'âme" },
            [31178] = new string[] { "身魂剥離", "Soul Strand", "Seelenband", "Découpe d'âme" },
            [31179] = new string[] { "身魂剥離", "Soul Strand", "Seelenband", "Découpe d'âme" },
            [31180] = new string[] { "ハルメギドフレイム", "End of the End", "Ende des Endes", "Consumation de Megiddo" },
            [31181] = new string[] { "ゲヘナダージ", "Dirge of Second Death", "Gehenna-Gesang", "Chant de Géhenne" },
            [31182] = new string[] { "万象降炎", "Limitless Kindling", "Kosmisches Feuer", "Feu universel" },
            [31184] = new string[] { "マージド・ゲヘナダージ", "Somatic Dirge of Second Death", "Somatischer Gehenna-Gesang", "Chant de Géhenne somatique" },
            [31185] = new string[] { "マージド・タイラントファイガ", "Somatic Tyrant's Fire III", "Somatisches Feuga des Tyrannen", "Méga Feu de tyran somatique" },
            [31186] = new string[] { "マージド・ゲヘナダージ", "Somatic Dirge of Second Death", "Somatischer Gehenna-Gesang", "Chant de Géhenne somatique" },
            [31187] = new string[] { "マージド・タイラントファイガ", "Somatic Tyrant's Fire III", "Somatisches Feuga des Tyrannen", "Méga Feu de tyran somatique" },
            [31188] = new string[] { "葬送波動", "Dead Outside", "Tödliche Welle", "Vague funéraire" },
            [31193] = new string[] { "支配者の一撃", "Dominion", "Schlag des Herrschers", "Poing du maître" },
            [31194] = new string[] { "地盤崩壊", "Orogenic Annihilation", "Bodensturz", "Érosion des sols" },
            [31195] = new string[] { "地盤大隆起", "Orogenic Deformation", "Gewaltige Bodenhebung", "Grande surrection" },
            [31196] = new string[] { "地盤隆起", "Orogenic Shift", "Bodenhebung", "Surrection" },
            [31197] = new string[] { "タイラント・ダークホーリー", "Tyrant's Unholy Darkness", "Unheiliges Dunkel des Tyrannen", "Miracle ténébreux de tyran" },
            [31198] = new string[] { "タイラント・ダークホーリー", "Tyrant's Unholy Darkness", "Unheiliges Dunkel des Tyrannen", "Miracle ténébreux de tyran" },
            [31199] = new string[] { "アイオンピュール", "Aioniopyr", "Aioniopyr", "Aion pur" },
            [31204] = new string[] { "自己概念崩壊", "Ego Death", "Egotod", "Destruction de l'ego" },
            [31205] = new string[] { "マージド・ハルメギドフレイム", "Somatic End of the End", "Somatisches Ende des Endes", "Consumation de Megiddo somatique" },
            [31210] = new string[] { "爆炎波動", "Ektothermos", "Ektothermos", "Vague d'énergie explosive" },
            [31266] = new string[] { "アイオンアゴニア", "Aionagonia", "Eiserne Agonie", "Aion agonia" },
            [31366] = new string[] { "自己概念崩壊", "Ego Death", "Egotod", "Destruction de l'ego" },
            [31367] = new string[] { "自己概念崩壊", "Ego Death", "Egotod", "Destruction de l'ego" },
        };

        private static Dictionary<uint, string[]> StatusNameMap = new Dictionary<uint, string[]>()
        {
            // p6s
            [3315] = new string[] { "甲殻寄生", "Chelomorph", "Chelomorph", "Crustacé parasite" },
            [3319] = new string[] { "軟体攻撃耐性低下", "Glossal Resistance Down", "Glossaler Widerstand -", "Résistance aux attaques de mollusques réduite" },
            [3320] = new string[] { "甲殻攻撃耐性低下", "Chelic Resistance Down", "Chelischer Widerstand -", "Résistance aux attaques de crustacés réduite" },
            [3321] = new string[] { "魔活細胞", "Aetheronecrosis", "Explozelle", "Cellules magiques actives" },

            // p7s
            [3308] = new string[] { "風の魔印", "Inviolate Winds", "Zeichen des Windes", "Sigil du vent" },
            [3309] = new string[] { "聖の魔印", "Holy Bonds", "Zeichen der Heiligkeit", "Sigil sacré" },
            [3310] = new string[] { "風の二重印", "Purgatory Winds", "Doppeltes Zeichen des Windes", "Double sigil du vent" },
            [3311] = new string[] { "聖の二重印", "Holy Purgation", "Doppeltes Zeichen der Heiligkeit", "Double sigil sacré" },
            [3391] = new string[] { "風の二重印", "Purgatory Winds", "Doppeltes Zeichen des Windes", "Double sigil du vent" },
            [3392] = new string[] { "風の二重印", "Purgatory Winds", "Doppeltes Zeichen des Windes", "Double sigil du vent" },
            [3393] = new string[] { "風の二重印", "Purgatory Winds", "Doppeltes Zeichen des Windes", "Double sigil du vent" },
            [3394] = new string[] { "聖の二重印", "Holy Purgation", "Doppeltes Zeichen der Heiligkeit", "Double sigil sacré" },
            [3395] = new string[] { "聖の二重印", "Holy Purgation", "Doppeltes Zeichen der Heiligkeit", "Double sigil sacré" },
            [3396] = new string[] { "聖の二重印", "Holy Purgation", "Doppeltes Zeichen der Heiligkeit", "Double sigil sacré" },
            [3397] = new string[] { "風の魔印", "Inviolate Winds", "Doppeltes Zeichen des Windes", "Sigil du vent" },
            [3398] = new string[] { "聖の魔印", "Holy Bonds", "Doppeltes Zeichen der Heiligkeit", "Sigil sacré" },

            // p8s
            [3325] = new string[] { "マジックコンシーヴ", "Conceptual Mastery", "Magische Einnistung", "Conception magique" },
            [3326] = new string[] { "ゴルゴンの呪詛：蛇毒", "Blood of the Gorgon", "Gorgons Fluch: Schlangengift", "Malédiction de gorgone: venin reptilien" },
            [3327] = new string[] { "ゴルゴンの呪詛：邪毒", "Breath of the Gorgon", "Gorgons Fluch: Übelgift", "Malédiction de gorgone: poison insidieux" },
            [3330] = new string[] { "未完概念：α", "Imperfection: Alpha", "Unvollständiges Konzept α", "Concept immature: α" },
            [3331] = new string[] { "未完概念：β", "Imperfection: Beta", "Unvollständiges Konzept β", "Concept immature: β" },
            [3332] = new string[] { "未完概念：γ", "Imperfection: Gamma", "Unvollständiges Konzept γ", "Concept immature: γ" },
            [3333] = new string[] { "完成概念：α", "Perfection: Alpha", "Vollständiges Konzept α", "Concept mature: α" },
            [3334] = new string[] { "完成概念：β", "Perfection: Beta", "Vollständiges Konzept β", "Concept mature: β" },
            [3335] = new string[] { "完成概念：γ", "Perfection: Gamma", "Vollständiges Konzept γ", "Concept mature: γ" },
            [3336] = new string[] { "概念拒絶", "Inconceivable", "Konzeptablehnung", "Rejet conceptuel" },
            [3337] = new string[] { "概念生成：飛行生物", "Winged Conception", "Konzeptumsetzung: Flugwesen", "Conceptualisation: créature volante" },
            [3338] = new string[] { "概念生成：水棲生物", "Aquatic Conception", "Konzeptumsetzung: Wasserorganismus", "Conceptualisation: créature marine" },
            [3339] = new string[] { "概念生成：雷獣", "Shocking Conception", "Konzeptumsetzung: Gewitterwesen", "Conceptualisation: Raijû" },
            [3340] = new string[] { "概念生成：火精", "Fiery Conception", "Konzeptumsetzung: Flammengeist", "Conceptualisation: esprit des flammes" },
            [3341] = new string[] { "概念生成：有毒生物", "Toxic Conception", "Konzeptumsetzung: Gifttier", "Conceptualisation: créature venimeuse" },
            [3342] = new string[] { "概念生成：草木生物", "Growing Conception", "Konzeptumsetzung: Pflanzenwesen", "Conceptualisation: créature végétale" },
            [3343] = new string[] { "概念の断片：不死鳥", "Immortal Spark", "Konzeptfragment: Phoinix", "Concept fragmentaire: oiseau immortel" },
            [3344] = new string[] { "概念生成：不死鳥", "Immortal Conception", "Konzeptumsetzung: Phoinix", "Conceptualisation: oiseau immortel" },
            [3345] = new string[] { "単相式反発概念", "Solosplice", "Einfache Konzeptreflektion", "Concept adverse simple" },
            [3346] = new string[] { "複相式反発概念", "Multisplice", "Multiple Konzeptreflektion", "Concepts adverses multiples" },
            [3347] = new string[] { "重相式反発概念", "Supersplice", "Komplexe Konzeptreflektion", "Concept adverse puissant" },
            [3349] = new string[] { "マジックインヴァージョン", "Inverse Magicks", "Magische Umkehr", "Inversion magique" },
            [3350] = new string[] { "幽体離脱", "Soul Stranded", "Außerkörperlichkeit", "Dissociation corps-âme" },
            [3351] = new string[] { "ゴルゴンの呪詛：石眼", "Eye of the Gorgon", "Gorgons Fluch: Steinauge", "Malédiction de gorgone: œil pétrifiant" },
            [3352] = new string[] { "ゴルゴンの呪詛：石光", "Crown of the Gorgon", "Gorgons Fluch: Steinlicht", "Malédiction de gorgone: lueur pétrifiante" },
            [3406] = new string[] { "不死鳥の力", "Everburn", "Macht des Phoinix", "Puissance de l'oiseau immortel" },
            [3412] = new string[] { "術式記述[被]", "Natural Alignment", "Rituelle Anpassung (empfangen)", "Description rituelle (ciblage)" },
        };

        private static Dictionary<uint, string[]> StatusDescriptionMap = new Dictionary<uint, string[]>()
        {
            // p6s
            [3315] = new string[] { "甲殻生物に寄生された状態。効果時間が終了すると、制御不能状態に陥る。", "Host to a chelic parasite, which will take control of body once this effect expires.", "Parasitenbefall. Nach Ablauf verliert der Wirtskörper die Kontrolle.", "Le corps est parasité par un crustacé. Devient Incontrôlable lorsque la durée de l'effet s'est écoulée." },
            [3319] = new string[] { "軟体生物から放たれる攻撃への耐性が低下した状態。", "Resistance to attacks by glossal parasites is reduced.", "Parasitenbefall. Nach Ablauf verliert der Wirtskörper die Kontrolle.", "La résistance aux attaques de mollusques est réduite." },
            [3320] = new string[] { "甲殻生物から放たれる攻撃への耐性が低下した状態。", "Resistance to attacks by chelic parasites is reduced.", "Parasitenbefall. Nach Ablauf verliert der Wirtskörper die Kontrolle.", "La résistance aux attaques de crustacés est réduite." },
            [3321] = new string[] { "活性化した細胞を付けられた状態。効果時間が終了すると爆発を起こす。", "Infected with aetherially activated cells, which will burst explosively when this effect expires.", "Zellulärer Befall. Löst nach Ablauf eine Explosion aus.", "Le corps est envahi par des cellules magiques actives. Celles-ci produisent une explosion lorsque la durée de l'effet s'est écoulée." },

            // p7s
            [3308] = new string[] { "魔印が刻まれた状態。効果時間が終了すると、周囲に魔法が発動する。", "Ensnared by wind magicks that will be unleashed in the surrounding area when this effect expires.", "Löst nach Ablauf einen flächendeckenden Zauber aus.", "Le corps est marqué par un sigil. Un sort se déclenche autour de lui lorsque la durée de l'effet s'est écoulée." },
            [3309] = new string[] { "魔印が刻まれた状態。効果時間が終了すると、周囲に魔法が発動する。", "Ensnared by light magicks that will be unleashed in the surrounding area when this effect expires.", "Löst nach Ablauf einen flächendeckenden Zauber aus.", "Le corps est marqué par un sigil. Un sort se déclenche autour de lui lorsque la durée de l'effet s'est écoulée." },
            [3310] = new string[] { "二重に魔印が刻まれた状態。効果時間が終了すると周囲に魔法が発動し、さらにその場に時限式の魔法陣が設置される。", "Ensnared by punishing wind magicks that will be unleashed in the surrounding area when this effect expires, and leave a timed sigil in their wake.", "Löst nach Ablauf einen flächendeckenden Zauber und einen temporären Magiezirkel aus.", "Le corps est marqué par un double sigil. Un sort se déclenche autour de lui et une sphère arcanique à retardement apparaît sur le terrain lorsque la durée de l'effet s'est écoulée." },
            [3311] = new string[] { "二重に魔印が刻まれた状態。効果時間が終了すると周囲に魔法が発動し、さらにその場に時限式の魔法陣が設置される。", "Ensnared by punishing wind magicks that will be unleashed in the surrounding area when this effect expires, and leave a timed sigil in their wake.", "Löst nach Ablauf einen flächendeckenden Zauber und einen temporären Magiezirkel aus.", "Le corps est marqué par un double sigil. Un sort se déclenche autour de lui et une sphère arcanique à retardement apparaît sur le terrain lorsque la durée de l'effet s'est écoulée." },
            [3391] = new string[] { "二重に魔印が刻まれた状態。効果時間が終了すると周囲に魔法が発動し、さらにその場に時限式の魔法陣が設置される。", "Ensnared by punishing wind magicks that will be unleashed in the surrounding area when this effect expires, and leave a timed sigil in their wake.", "Löst nach Ablauf einen flächendeckenden Zauber und einen temporären Magiezirkel aus.", "Le corps est marqué par un double sigil. Un sort se déclenche autour de lui et une sphère arcanique à retardement apparaît sur le terrain lorsque la durée de l'effet s'est écoulée." },
            [3392] = new string[] { "二重に魔印が刻まれた状態。効果時間が終了すると周囲に魔法が発動し、さらにその場に時限式の魔法陣が設置される。", "Ensnared by punishing wind magicks that will be unleashed in the surrounding area when this effect expires, and leave a timed sigil in their wake.", "Löst nach Ablauf einen flächendeckenden Zauber und einen temporären Magiezirkel aus.", "Le corps est marqué par un double sigil. Un sort se déclenche autour de lui et une sphère arcanique à retardement apparaît sur le terrain lorsque la durée de l'effet s'est écoulée." },
            [3393] = new string[] { "二重に魔印が刻まれた状態。効果時間が終了すると周囲に魔法が発動し、さらにその場に時限式の魔法陣が設置される。", "Ensnared by punishing wind magicks that will be unleashed in the surrounding area when this effect expires, and leave a timed sigil in their wake.", "Löst nach Ablauf einen flächendeckenden Zauber und einen temporären Magiezirkel aus.", "Le corps est marqué par un double sigil. Un sort se déclenche autour de lui et une sphère arcanique à retardement apparaît sur le terrain lorsque la durée de l'effet s'est écoulée." },
            [3394] = new string[] { "二重に魔印が刻まれた状態。効果時間が終了すると周囲に魔法が発動し、さらにその場に時限式の魔法陣が設置される。", "Ensnared by punishing wind magicks that will be unleashed in the surrounding area when this effect expires, and leave a timed sigil in their wake.", "Löst nach Ablauf einen flächendeckenden Zauber und einen temporären Magiezirkel aus.", "Le corps est marqué par un double sigil. Un sort se déclenche autour de lui et une sphère arcanique à retardement apparaît sur le terrain lorsque la durée de l'effet s'est écoulée." },
            [3395] = new string[] { "二重に魔印が刻まれた状態。効果時間が終了すると周囲に魔法が発動し、さらにその場に時限式の魔法陣が設置される。", "Ensnared by punishing wind magicks that will be unleashed in the surrounding area when this effect expires, and leave a timed sigil in their wake.", "Löst nach Ablauf einen flächendeckenden Zauber und einen temporären Magiezirkel aus.", "Le corps est marqué par un double sigil. Un sort se déclenche autour de lui et une sphère arcanique à retardement apparaît sur le terrain lorsque la durée de l'effet s'est écoulée." },
            [3396] = new string[] { "二重に魔印が刻まれた状態。効果時間が終了すると周囲に魔法が発動し、さらにその場に時限式の魔法陣が設置される。", "Ensnared by punishing wind magicks that will be unleashed in the surrounding area when this effect expires, and leave a timed sigil in their wake.", "Löst nach Ablauf einen flächendeckenden Zauber und einen temporären Magiezirkel aus.", "Le corps est marqué par un double sigil. Un sort se déclenche autour de lui et une sphère arcanique à retardement apparaît sur le terrain lorsque la durée de l'effet s'est écoulée." },
            [3397] = new string[] { "魔印が刻まれた状態。効果時間が終了すると、周囲に魔法が発動する。", "Ensnared by wind magicks that will be unleashed in the surrounding area when this effect expires.", "Löst nach Ablauf einen flächendeckenden Zauber und einen temporären Magiezirkel aus.", "Le corps est marqué par un sigil. Un sort se déclenche autour de lui lorsque la durée de l'effet s'est écoulée." },
            [3398] = new string[] { "魔印が刻まれた状態。効果時間が終了すると、周囲に魔法が発動する。", "Ensnared by light magicks that will be unleashed in the surrounding area when this effect expires.", "Löst nach Ablauf einen flächendeckenden Zauber und einen temporären Magiezirkel aus.", "Le corps est marqué par un sigil. Un sort se déclenche autour de lui lorsque la durée de l'effet s'est écoulée." },

            // p8s
            [3325] = new string[] { "発動する魔法を自身に宿した状態。", "Primed with powerful magicks.", "Magische Energie nistet sich in den Körper ein.", "Le corps héberge un sort qui peut se déclencher n'importe quand." },
            [3326] = new string[] { "ゴルゴンの呪いを受けた状態。効果時間が終了すると、周囲に毒の攻撃を放つ。", "Cursed to unleash poison in the surrounding area when this effect expires.", "Sprüht nach Ablauf die Umgebung mit Gift ein.", "Déclenche une attaque empoisonnée dans les alentours lorsque la durée de l'effet s'est écoulée." },
            [3327] = new string[] { "ゴルゴンの呪いを受けた状態。効果時間が終了すると、周囲に毒の攻撃を放つ。", "Cursed to unleash poison in the surrounding area when this effect expires.", "Sprüht nach Ablauf die Umgebung mit Gift ein.", "Déclenche une attaque empoisonnée dans les alentours lorsque la durée de l'effet s'est écoulée." },
            [3330] = new string[] { "自身とは異なる生体のイデアを付与された状態。効果終了時に爆発反応を起こし、自身と周囲のイデアに影響を及ぼす。", "Imbued with a concept incompatible with this form, which will cause an explosive reaction and influence self and nearby concepts when this effect expires.", "Löst nach Ablauf eine Explosion aus, die das eigene Konzept und Konzepte in der Nähe beeinflusst.", "L'être est contaminé par le concept d'une créature. Une explosion se produit à la fin de l'effet, impactant la victime et les concepts alentour." },
            [3331] = new string[] { "自身とは異なる生体のイデアを付与された状態。効果終了時に爆発反応を起こし、自身と周囲のイデアに影響を及ぼす。", "Imbued with a concept incompatible with this form, which will cause an explosive reaction and influence self and nearby concepts when this effect expires.", "Löst nach Ablauf eine Explosion aus, die das eigene Konzept und Konzepte in der Nähe beeinflusst.", "L'être est contaminé par le concept d'une créature. Une explosion se produit à la fin de l'effet, impactant la victime et les concepts alentour." },
            [3332] = new string[] { "自身とは異なる生体のイデアを付与された状態。効果終了時に爆発反応を起こし、自身と周囲のイデアに影響を及ぼす。", "Imbued with a concept incompatible with this form, which will cause an explosive reaction and influence self and nearby concepts when this effect expires.", "Löst nach Ablauf eine Explosion aus, die das eigene Konzept und Konzepte in der Nähe beeinflusst.", "L'être est contaminé par le concept d'une créature. Une explosion se produit à la fin de l'effet, impactant la victime et les concepts alentour." },
            [3333] = new string[] { "自身とは異なる生体のイデアが身体に宿った状態。他者の完成概念と近づくことで互いに影響を及ぼす。", "The perfect vessel for a perfect concept.Drawing near to other perfect concepts will result in mutual influence.", "Gegenseitige Beeinflussung, wenn sich vollständige Konzepte von anderen in der Nähe befinden.", "Le corps héberge le concept mature d'une créature. En cas de rapprochement avec un autre concept mature, une influence mutuelle se produit." },
            [3334] = new string[] { "自身とは異なる生体のイデアが身体に宿った状態。他者の完成概念と近づくことで互いに影響を及ぼす。", "The perfect vessel for a perfect concept.Drawing near to other perfect concepts will result in mutual influence.", "Gegenseitige Beeinflussung, wenn sich vollständige Konzepte von anderen in der Nähe befinden.", "Le corps héberge le concept mature d'une créature. En cas de rapprochement avec un autre concept mature, une influence mutuelle se produit." },
            [3335] = new string[] { "自身とは異なる生体のイデアが身体に宿った状態。他者の完成概念と近づくことで互いに影響を及ぼす。", "The perfect vessel for a perfect concept.Drawing near to other perfect concepts will result in mutual influence.", "Gegenseitige Beeinflussung, wenn sich vollständige Konzepte von anderen in der Nähe befinden.", "Le corps héberge le concept mature d'une créature. En cas de rapprochement avec un autre concept mature, une influence mutuelle se produit." },
            [3336] = new string[] { "効果時間終了まで他者の完成概念の影響を受けない状態。", "Unable to draw upon perfect concepts until this effect expires.", "Wird während der Wirkungsdauer nicht von vollständigen Konzepten beeinflusst.", "L'influence des concepts matures hébergés chez les autres n'est plus subie." },
            [3337] = new string[] { "飛行生物の概念を生成した状態。効果時間終了まで特定の風属性魔法に耐性を得る。", "Realizing an airborne concept. Resistance to certain wind magicks is increased until this effect expires.", "Verleiht Resistenz gegen bestimmte Windzauber.", "Un concept de créature volante est créé, octroyant une résistance à certains sorts de vent." },
            [3338] = new string[] { "水棲生物の概念を生成した状態。効果時間終了まで特定の水属性魔法に耐性を得る。", "Realizing an aquatic concept. Resistance to certain water magicks is increased until this effect expires.", "Verleiht Resistenz gegen bestimmte Wasserzauber.", "Un concept de créature marine est créé, octroyant une résistance à certains sorts d'eau." },
            [3339] = new string[] { "雷獣の概念を生成した状態。効果時間終了まで特定の雷属性魔法に耐性を得る。", "Realizing a levin - wielding concept. Resistance to certain lightning magicks is increased until this effect expires.", "Verleiht Resistenz gegen bestimmte Blitzzauber.", "Un concept de Raijû est créé, octroyant une résistance à certains sorts de foudre." },
            [3340] = new string[] { "火精の概念を生成した状態。効果時間終了までＨＰが徐々に失われる。", "Realizing a burning concept, causing damage over time until this effect expires.", "Erleidet schrittweise Schaden.", "Un concept d'esprit des flammes est créé, faisant subir des dégâts périodiques." },
            [3341] = new string[] { "有毒生物の概念を生成した状態。効果時間終了までＨＰが徐々に失われる。", "Realizing a poisonous concept, causing damage over time until this effect expires.", "Erleidet schrittweise Schaden.", "Un concept de créature venimeuse est créé, faisant subir des dégâts périodiques." },
            [3342] = new string[] { "草木生物の概念を生成した状態。効果時間終了までＨＰが徐々に失われる。", "Realizing a plantlike concept, causing damage over time until this effect expires.", "Erleidet schrittweise Schaden.", "Un concept de créature végétale est créé, faisant subir des dégâts périodiques." },
            [3343] = new string[] { "不死鳥の概念の断片を身に宿した状態。4つ集めることで不死鳥の概念が生成される。", "Conceiving of the Phoenix in part. Together, four such sparks will give birth to a legendary bird.", "Löst bei 4 Stapeln Konzeptumsetzung: Phoinix aus.", "Le corps héberge un fragment du concept d'oiseau immortel. Si quatre fragments sont réunis, un concept complet est créé." },
            [3344] = new string[] { "不死鳥の概念を生成した状態。", "Realizing a Phoenix concept.", "Wurde von der Kraft des Phoinix wiederbelebt.", "Le concept d'oiseau immortel a été créé." },
            [3345] = new string[] { "自身のイデアが変異しかけている状態。効果終了時、周囲の影響を受けて反発反応を引き起こす。", "Self - concept is being warped beyond recognition, resulting in an adverse reaction determined by nearby influences when this effect expires.", "Löst nach Ablauf eine flächendeckende Reaktion aus.", "La conception du soi est altérée. À la fin de l'effet, l'influence des alentours provoque une réaction spontanée." },
            [3346] = new string[] { "自身のイデアが変異しかけている状態。効果終了時、周囲の影響を受けて反発反応を引き起こす。", "Self - concept is being warped beyond recognition, resulting in an adverse reaction determined by nearby influences when this effect expires.", "Löst nach Ablauf eine flächendeckende Reaktion aus.", "La conception du soi est altérée. À la fin de l'effet, l'influence des alentours provoque une réaction spontanée." },
            [3347] = new string[] { "自身のイデアが変異しかけている状態。効果終了時、周囲の影響を受けて反発反応を引き起こす。", "Self - concept is being warped beyond recognition, resulting in an adverse reaction determined by nearby influences when this effect expires.", "Löst nach Ablauf eine flächendeckende Reaktion aus.", "La conception du soi est altérée. À la fin de l'effet, l'influence des alentours provoque une réaction spontanée." },
            [3349] = new string[] { "自身が強制詠唱させられる魔法の発動順が逆転した状態。", "The order of forcible magicks to be cast is inverted.", "Die Reihenfolge der zwanghaft gewirkten Zauber wird umgekehrt.", "L'odre de lancement des sorts dont l'incantation est forcée est inversé." },
            [3350] = new string[] { "身体と魂が分離してしまった状態。", "Physical and spiritual forms have been separated.", "Geist und Körper sind vorübergehend voneinander gelöst.", "L'âme et le corps sont séparés l'un de l'autre." },
            [3351] = new string[] { "ゴルゴンの呪いを受けた状態。効果時間が終了すると、前方に石化の攻撃を放つ。", "Cursed to unleash a petrifying attack in the direction of gaze when this effect expires.", "Entfesselt nach Ablauf einen versteinernden Angriff nach vorne.", "Déclenche une attaque pétrifiante en ligne droite vers l'avant lorsque la durée de l'effet s'est écoulée." },
            [3352] = new string[] { "ゴルゴンの呪いを受けた状態。効果時間が終了すると、周囲に石化の光を放つ。", "Cursed to unleash a petrifying light upon those nearby when this effect expires.", "Entfesselt nach Ablauf ein versteinerndes Licht auf die Umgebung", "Émet une lueur pétrifiante alentour lorsque la durée de l'effet s'est écoulée." },
            [3406] = new string[] { "不死鳥の概念を生成し、その力を身に宿した状態。与ダメージが上昇する。", "Calling upon the power of a Phoenix concept. Damage dealt is increased.", "Ausgeteilter Schaden ist erhöht.", "Un concept d'oiseau immortel est créé, augmentant les dégâts infligés." },
            [3412] = new string[] { "ヘファイストスの術式が刻まれ、ＨＰが徐々に失われる状態。「強制詠唱」によって放たれた特定アクションを受けた場合、崩壊術式が発動する。", "Graven with a sigil and sustaining damage over time. Taking damage from certain actions caused by Twist Nature will result in a destructive forcible failure.", "Hephaistos' Kräfte verursachen schrittweise Schaden. Löst Rituelle Zerstörung aus, wenn von einem Kommando betroffen, das durch Zwangsbeschwörung gewirkt wurde.", "Des dégâts périodiques sont subis. Rituel destructeur se déclenche si la victime subit un certain sort à incantation forcée." }
        };
    }
}
