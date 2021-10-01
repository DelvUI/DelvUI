using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using System;
using System.Collections.Generic;

namespace DelvUI.Interface.GeneralElements
{
    public class GlobalColors : IDisposable
    {
        #region Singleton
        private readonly MiscColorConfig _miscColorConfig;

        private readonly Dictionary<uint, PluginConfigColor> _colorMap;

        private GlobalColors()
        {
            _miscColorConfig = ConfigurationManager.Instance.GetConfigObject<MiscColorConfig>();

            TanksColorConfig? tanksColorConfig = ConfigurationManager.Instance.GetConfigObject<TanksColorConfig>();
            HealersColorConfig? healersColorConfig = ConfigurationManager.Instance.GetConfigObject<HealersColorConfig>();
            MeleeColorConfig? meleeColorConfig = ConfigurationManager.Instance.GetConfigObject<MeleeColorConfig>();
            RangedColorConfig? rangedColorConfig = ConfigurationManager.Instance.GetConfigObject<RangedColorConfig>();
            CastersColorConfig? castersColorConfig = ConfigurationManager.Instance.GetConfigObject<CastersColorConfig>();

            _colorMap = new Dictionary<uint, PluginConfigColor>()
            {
                // tanks
                [JobIDs.GLD] = tanksColorConfig.GLDColor,
                [JobIDs.MRD] = tanksColorConfig.MRDColor,
                [JobIDs.PLD] = tanksColorConfig.PLDColor,
                [JobIDs.WAR] = tanksColorConfig.WARColor,
                [JobIDs.DRK] = tanksColorConfig.DRKColor,
                [JobIDs.GNB] = tanksColorConfig.GNBColor,

                // healers
                [JobIDs.CNJ] = healersColorConfig.CNJColor,
                [JobIDs.WHM] = healersColorConfig.WHMColor,
                [JobIDs.SCH] = healersColorConfig.SCHColor,
                [JobIDs.AST] = healersColorConfig.ASTColor,

                // melee
                [JobIDs.PGL] = meleeColorConfig.PGLColor,
                [JobIDs.LNC] = meleeColorConfig.LNCColor,
                [JobIDs.ROG] = meleeColorConfig.ROGColor,
                [JobIDs.MNK] = meleeColorConfig.MNKColor,
                [JobIDs.DRG] = meleeColorConfig.DRGColor,
                [JobIDs.NIN] = meleeColorConfig.NINColor,
                [JobIDs.SAM] = meleeColorConfig.SAMColor,

                // ranged 
                [JobIDs.ARC] = rangedColorConfig.ARCColor,
                [JobIDs.BRD] = rangedColorConfig.BRDColor,
                [JobIDs.MCH] = rangedColorConfig.MCHColor,
                [JobIDs.DNC] = rangedColorConfig.DNCColor,

                // casters
                [JobIDs.THM] = castersColorConfig.THMColor,
                [JobIDs.ACN] = castersColorConfig.ACNColor,
                [JobIDs.BLM] = castersColorConfig.BLMColor,
                [JobIDs.SMN] = castersColorConfig.SMNColor,
                [JobIDs.RDM] = castersColorConfig.RDMColor,
                [JobIDs.BLU] = castersColorConfig.BLUColor,

                // crafters
                [JobIDs.CRP] = _miscColorConfig.HANDColor,
                [JobIDs.BSM] = _miscColorConfig.HANDColor,
                [JobIDs.ARM] = _miscColorConfig.HANDColor,
                [JobIDs.GSM] = _miscColorConfig.HANDColor,
                [JobIDs.LTW] = _miscColorConfig.HANDColor,
                [JobIDs.WVR] = _miscColorConfig.HANDColor,
                [JobIDs.ALC] = _miscColorConfig.HANDColor,
                [JobIDs.CUL] = _miscColorConfig.HANDColor,

                // gatherers
                [JobIDs.MIN] = _miscColorConfig.LANDColor,
                [JobIDs.BOT] = _miscColorConfig.LANDColor,
                [JobIDs.FSH] = _miscColorConfig.LANDColor
            };
        }

        public static void Initialize()
        {
            Instance = new GlobalColors();
        }

        public static GlobalColors Instance { get; private set; } = null!;

        ~GlobalColors()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            Instance = null!;
        }
        #endregion

        public PluginConfigColor? ColorForJobId(uint jobId)
        {
            if (_colorMap.TryGetValue(jobId, out PluginConfigColor? color))
            {
                return color;
            }

            return null;
        }

        public PluginConfigColor SafeColorForJobId(uint jobId)
        {
            return ColorForJobId(jobId) ?? _miscColorConfig.NPCNeutralColor;
        }

        public PluginConfigColor EmptyUnitFrameColor => _miscColorConfig.EmptyUnitFrameColor;
        public PluginConfigColor EmptyColor => _miscColorConfig.EmptyColor;
        public PluginConfigColor PartialFillColor => _miscColorConfig.PartialFillColor;
        public PluginConfigColor NPCFriendlyColor => _miscColorConfig.NPCFriendlyColor;
        public PluginConfigColor NPCHostileColor => _miscColorConfig.NPCHostileColor;
        public PluginConfigColor NPCNeutralColor => _miscColorConfig.NPCNeutralColor;
    }

    [Disableable(false)]
    [Section("Colors")]
    [SubSection("Tanks", 0)]
    public class TanksColorConfig : PluginConfigObject
    {
        public static new TanksColorConfig DefaultConfig() { return new TanksColorConfig(); }

        [ColorEdit4("Paladin", spacing = true)]
        [Order(5)]
        public PluginConfigColor PLDColor = new(new(168f / 255f, 210f / 255f, 230f / 255f, 100f / 100f));

        [ColorEdit4("Dark Knight")]
        [Order(10)]
        public PluginConfigColor DRKColor = new(new(209f / 255f, 38f / 255f, 204f / 255f, 100f / 100f));

        [ColorEdit4("Warrior")]
        [Order(15)]
        public PluginConfigColor WARColor = new(new(207f / 255f, 38f / 255f, 33f / 255f, 100f / 100f));

        [ColorEdit4("Gunbreaker")]
        [Order(20)]
        public PluginConfigColor GNBColor = new(new(121f / 255f, 109f / 255f, 48f / 255f, 100f / 100f));

        [ColorEdit4("Gladiator", spacing = true)]
        [Order(25)]
        public PluginConfigColor GLDColor = new(new(168f / 255f, 210f / 255f, 230f / 255f, 100f / 100f));

        [ColorEdit4("Marauder")]
        [Order(30)]
        public PluginConfigColor MRDColor = new(new(207f / 255f, 38f / 255f, 33f / 255f, 100f / 100f));
    }

    [Disableable(false)]
    [Section("Colors")]
    [SubSection("Healers", 0)]
    public class HealersColorConfig : PluginConfigObject
    {
        public static new HealersColorConfig DefaultConfig() { return new HealersColorConfig(); }

        [ColorEdit4("Scholar", spacing = true)]
        [Order(5)]
        public PluginConfigColor SCHColor = new(new(134f / 255f, 87f / 255f, 255f / 255f, 100f / 100f));

        [ColorEdit4("White Mage")]
        [Order(10)]
        public PluginConfigColor WHMColor = new(new(255f / 255f, 240f / 255f, 220f / 255f, 100f / 100f));

        [ColorEdit4("Astrologian")]
        [Order(15)]
        public PluginConfigColor ASTColor = new(new(255f / 255f, 231f / 255f, 74f / 255f, 100f / 100f));

        [ColorEdit4("Conjurer", spacing = true)]
        [Order(20)]
        public PluginConfigColor CNJColor = new(new(255f / 255f, 240f / 255f, 220f / 255f, 100f / 100f));
    }

    [Disableable(false)]
    [Section("Colors")]
    [SubSection("Melee", 0)]
    public class MeleeColorConfig : PluginConfigObject
    {
        public static new MeleeColorConfig DefaultConfig() { return new MeleeColorConfig(); }

        [ColorEdit4("Monk", spacing = true)]
        [Order(5)]
        public PluginConfigColor MNKColor = new(new(214f / 255f, 156f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Ninja")]
        [Order(10)]
        public PluginConfigColor NINColor = new(new(175f / 255f, 25f / 255f, 100f / 255f, 100f / 100f));

        [ColorEdit4("Dragoon")]
        [Order(15)]
        public PluginConfigColor DRGColor = new(new(65f / 255f, 100f / 255f, 205f / 255f, 100f / 100f));

        [ColorEdit4("Samurai")]
        [Order(20)]
        public PluginConfigColor SAMColor = new(new(228f / 255f, 109f / 255f, 4f / 255f, 100f / 100f));

        [ColorEdit4("Pugilist", spacing = true)]
        [Order(25)]
        public PluginConfigColor PGLColor = new(new(214f / 255f, 156f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Rogue")]
        [Order(30)]
        public PluginConfigColor ROGColor = new(new(175f / 255f, 25f / 255f, 100f / 255f, 100f / 100f));

        [ColorEdit4("Lancer")]
        [Order(35)]
        public PluginConfigColor LNCColor = new(new(65f / 255f, 100f / 255f, 205f / 255f, 100f / 100f));
    }

    [Disableable(false)]
    [Section("Colors")]
    [SubSection("Ranged", 0)]
    public class RangedColorConfig : PluginConfigObject
    {
        public static new RangedColorConfig DefaultConfig() { return new RangedColorConfig(); }

        [ColorEdit4("Bard", spacing = true)]
        [Order(5)]
        public PluginConfigColor BRDColor = new(new(145f / 255f, 186f / 255f, 94f / 255f, 100f / 100f));

        [ColorEdit4("Machinist")]
        [Order(10)]
        public PluginConfigColor MCHColor = new(new(110f / 255f, 225f / 255f, 214f / 255f, 100f / 100f));

        [ColorEdit4("Dancer")]
        [Order(15)]
        public PluginConfigColor DNCColor = new(new(226f / 255f, 176f / 255f, 175f / 255f, 100f / 100f));

        [ColorEdit4("Archer", separator = true)]
        [Order(20)]
        public PluginConfigColor ARCColor = new(new(145f / 255f, 186f / 255f, 94f / 255f, 100f / 100f));
    }

    [Disableable(false)]
    [Section("Colors")]
    [SubSection("Caster", 0)]
    public class CastersColorConfig : PluginConfigObject
    {
        public static new CastersColorConfig DefaultConfig() { return new CastersColorConfig(); }

        [ColorEdit4("Black Mage", spacing = true)]
        [Order(5)]
        public PluginConfigColor BLMColor = new(new(165f / 255f, 121f / 255f, 214f / 255f, 100f / 100f));

        [ColorEdit4("Summoner")]
        [Order(10)]
        public PluginConfigColor SMNColor = new(new(45f / 255f, 155f / 255f, 120f / 255f, 100f / 100f));

        [ColorEdit4("Red Mage")]
        [Order(15)]
        public PluginConfigColor RDMColor = new(new(232f / 255f, 123f / 255f, 123f / 255f, 100f / 100f));

        [ColorEdit4("Blue Mage", spacing = true)]
        [Order(20)]
        public PluginConfigColor BLUColor = new(new(0f / 255f, 185f / 255f, 247f / 255f, 100f / 100f));

        [ColorEdit4("Thaumaturge")]
        [Order(25)]
        public PluginConfigColor THMColor = new(new(165f / 255f, 121f / 255f, 214f / 255f, 100f / 100f));

        [ColorEdit4("Arcanist")]
        [Order(30)]
        public PluginConfigColor ACNColor = new(new(45f / 255f, 155f / 255f, 120f / 255f, 100f / 100f));
    }

    [Disableable(false)]
    [Section("Colors")]
    [SubSection("Misc", 0)]
    public class MiscColorConfig : PluginConfigObject
    {
        public static new MiscColorConfig DefaultConfig() { return new MiscColorConfig(); }

        [Combo("Gradient Type For Bars", "Flat Color", "Right", "Left", "Up", "Down", "Centered Horizontal", spacing = true)]
        [Order(4)]
        public GradientDirection GradientDirection = GradientDirection.Down;

        [ColorEdit4("Empty Unit Frame", separator = true)]
        [Order(5)]
        public PluginConfigColor EmptyUnitFrameColor = new(new(0f / 255f, 0f / 255f, 0f / 255f, 95f / 100f));

        [ColorEdit4("Empty Bar")]
        [Order(10)]
        public PluginConfigColor EmptyColor = new(new(0f / 255f, 0f / 255f, 0f / 255f, 50f / 100f));

        [ColorEdit4("Partially Filled Bar")]
        [Order(15)]
        public PluginConfigColor PartialFillColor = new(new(180f / 255f, 180f / 255f, 180f / 255f, 100f / 100f));

        [ColorEdit4("NPC Friendly", separator = true)]
        [Order(20)]
        public PluginConfigColor NPCFriendlyColor = new(new(99f / 255f, 172f / 255f, 14f / 255f, 100f / 100f));

        [ColorEdit4("NPC Hostile")]
        [Order(25)]
        public PluginConfigColor NPCHostileColor = new(new(233f / 255f, 4f / 255f, 4f / 255f, 100f / 100f));

        [ColorEdit4("NPC Neutral")]
        [Order(30)]
        public PluginConfigColor NPCNeutralColor = new(new(218f / 255f, 157f / 255f, 46f / 255f, 100f / 100f));

        [ColorEdit4("Disciples of the Land", spacing = true)]
        [Order(35)]
        public PluginConfigColor LANDColor = new(new(99f / 255f, 172f / 255f, 14f / 255f, 100f / 100f));

        [ColorEdit4("Disciples of the Hand")]
        [Order(40)]
        public PluginConfigColor HANDColor = new(new(99f / 255f, 172f / 255f, 14f / 255f, 100f / 100f));
    }
}
