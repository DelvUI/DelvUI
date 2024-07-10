using System;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DelvUI.Config;
using DelvUI.Interface.GeneralElements;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace DelvUI.Interface.Jobs
{
    public class ViperHud : JobHud
    {
        private new ViperConfig Config => (ViperConfig)_config;
        private static readonly List<uint> NoxiousGnashIDs = new() { 3667, 4099 };
        private static readonly List<float> NoxiousGnashDurations = new() { 40f, 40f };

        public ViperHud(ViperConfig config, string? displayName = null) : base(config, displayName) { }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.RattlingCoilGauge.Enabled)
            {
                positions.Add(Config.Position + Config.RattlingCoilGauge.Position);
                sizes.Add(Config.RattlingCoilGauge.Size);
            }
            
            if (Config.Vipersight.Enabled)
            {
                positions.Add(Config.Position + Config.Vipersight.Position);
                sizes.Add(Config.Vipersight.Size);
            }

            if (Config.NoxiousGnash.Enabled)
            {
                positions.Add(Config.Position + Config.NoxiousGnash.Position);
                sizes.Add(Config.NoxiousGnash.Size);
            }

            if (Config.AnguineTribute.Enabled)
            {
                positions.Add(Config.Position + Config.AnguineTribute.Position);
                sizes.Add(Config.AnguineTribute.Size);
            }

            if (Config.SerpentOfferings.Enabled)
            {
                positions.Add(Config.Position + Config.SerpentOfferings.Position);
                sizes.Add(Config.SerpentOfferings.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, IPlayerCharacter player)
        {
            if (Config.RattlingCoilGauge.Enabled)
            {
                DrawRattlingCoilGauge(origin + Config.Position, player);
            }
            
            if (Config.Vipersight.Enabled)
            {
                DrawVipersightBar(origin + Config.Position, player);
            }

            if (Config.NoxiousGnash.Enabled)
            {
                DrawNoxiousGnashBar(origin + Config.Position, player);
            }
            
            if (Config.SerpentOfferings.Enabled)
            {
                DrawSerpentOfferingsBar(origin + Config.Position, player);
            }

            if (Config.AnguineTribute.Enabled)
            {
                DrawAnguineTributeGauge(origin + Config.Position, player);
            }
        }
        
        private unsafe void DrawVipersightBar(Vector2 origin, IPlayerCharacter player)
        {
            var instance = JobGaugeManager.Instance();
            var gauge = (ViperGauge*)instance->CurrentGauge;
            
            var huntersInstinctDuration = Utils.StatusListForBattleChara(player).FirstOrDefault(o => o.StatusId is 3668)?.RemainingTime ?? 0f;
            var swiftScaledDuration = Utils.StatusListForBattleChara(player).FirstOrDefault(o => o.StatusId is 3669)?.RemainingTime ?? 0f;
            
            var isDreadFangsEnder = Utils.StatusListForBattleChara(player).Any(o => o.StatusId is 3646 or 3648 or 3650);
            var isSteelFangsEnder = Utils.StatusListForBattleChara(player).Any(o => o.StatusId is 3645 or 3647 or 3649);
            
            var lastUsedActionId = SpellHelper.Instance.GetLastUsedActionId();

            int comboState;
            
            List<Tuple<PluginConfigColor, float, LabelConfig?>> chunks = new List<Tuple<PluginConfigColor, float, LabelConfig?>>();
            List<bool> glows = new List<bool>();
            
            switch ((ViperCombo) lastUsedActionId)
            {
                case ViperCombo.DreadFangs:
                case ViperCombo.SteelFangs:
                case ViperCombo.SteelMaw:
                case ViperCombo.DreadMaw:
                    comboState = 1;
                    break;
                case ViperCombo.HuntersSting:
                case ViperCombo.SwiftskinsSting:
                case ViperCombo.HuntersBite:
                case ViperCombo.SwiftskinsBite:
                    comboState = 2;
                    break;
                default:
                    comboState = 0;
                    break;
            }
            
            var empty = new Tuple<PluginConfigColor, float, LabelConfig?>(PluginConfigColor.Empty, 1, null);
            
            var start = new Tuple<PluginConfigColor, float, LabelConfig?>(Config.Vipersight.ComboStartColor, 1, null);
            var endFlank = new Tuple<PluginConfigColor, float, LabelConfig?>(Config.Vipersight.ComboEndFlankColor, 1, null);
            var endHind = new Tuple<PluginConfigColor, float, LabelConfig?>(Config.Vipersight.ComboEndHindColor, 1, null);
            var endAOE = new Tuple<PluginConfigColor, float, LabelConfig?>(Config.Vipersight.ComboEndAOEColor, 1, null);

            switch (comboState)
            {
                case 0: // Empty Bars
                {
                    chunks = [empty, empty, empty, empty];
                    glows = [false, false, false, false];
                    break;
                }
                case 1: // Combo started
                {
                    chunks = [empty, start, start, empty];

                    if ((ViperCombo)lastUsedActionId == ViperCombo.DreadMaw || (ViperCombo)lastUsedActionId == ViperCombo.DreadMaw)
                    {   // Glow both, 2nd step AoE always suggests both
                        glows = [false, true, true, false];

                        break;
                    }
                    if (swiftScaledDuration > huntersInstinctDuration)
                    {   // Glow left
                        glows = [false, true, false, false];
                    }
                    else if (huntersInstinctDuration > swiftScaledDuration)
                    {   // Glow right
                        glows = [false, false, true, false];
                    }
                    else
                    {   // Glow both
                        glows = [false, true, true, false];
                    }
                    break;
                }
                case 2: // Combo Finisher, bar fully filled
                {
                    var isFlankEnder = Utils.StatusListForBattleChara(player).Any(o => o.StatusId is 3645 or 3646);
                    var isHindEnder = Utils.StatusListForBattleChara(player).Any(o => o.StatusId is 3647 or 3648);

                    Tuple<PluginConfigColor, float, LabelConfig?> end;
                    
                    if (isFlankEnder)
                    {
                        end = endFlank;
                    }
                    else if (isHindEnder)
                    {
                        end = endHind;
                    }
                    else
                    {
                        end = endAOE;
                    }

                    chunks = [end, start, start, end];
                    if (isSteelFangsEnder)
                    {   // Glow left
                        glows = [true, true, false, false];
                    }
                    else if (isDreadFangsEnder)
                    {   // Glow right
                        glows = [false, false, true, true];
                    }
                    else
                    {   // Glow both
                        glows = [true, true, true, true];
                    }
                    break;
                }
            }

            if (Config.Vipersight.Invert)
            {
                chunks.Reverse();
                glows.Reverse();
            }
                
            if (!Config.Vipersight.HideWhenInactive)
            {
                BarHud[] bars = BarUtilities.GetChunkedBars(Config.Vipersight, chunks.ToArray(), player, Config.Vipersight.GlowConfig, glows.ToArray());
                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.Vipersight.StrataLevel));
                }
            }
        }

        private unsafe void DrawRattlingCoilGauge(Vector2 origin, IPlayerCharacter player)
        {
            var instance = JobGaugeManager.Instance();
            var gauge = (ViperGauge*)instance->CurrentGauge;
            
            
            if (!Config.RattlingCoilGauge.HideWhenInactive || gauge->RattlingCoilStacks > 0)
            {
                var maxStacks = player.Level >= 88 ? 3 : 2;
                BarHud[] bars = BarUtilities.GetChunkedBars(Config.RattlingCoilGauge, maxStacks, gauge->RattlingCoilStacks, maxStacks, 0, player);
                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.RattlingCoilGauge.StrataLevel));
                }
            }
        }
        
        private unsafe void DrawAnguineTributeGauge(Vector2 origin, IPlayerCharacter player)
        {
            var instance = JobGaugeManager.Instance();
            var gauge = (ViperGauge*)instance->CurrentGauge;
            
            if (!Config.AnguineTribute.HideWhenInactive || gauge->AnguineTribute > 0)
            {
                var maxStacks = player.Level >= 96 ? 5 : 4;
                BarHud[] bars = BarUtilities.GetChunkedBars(Config.AnguineTribute, maxStacks, gauge->AnguineTribute, maxStacks, 0, player);
                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.AnguineTribute.StrataLevel));
                }
            }
        }

        private void DrawNoxiousGnashBar(Vector2 origin, IPlayerCharacter player)
        {
            var target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            
            BarHud? bar = BarUtilities.GetDoTBar(Config.NoxiousGnash, player, target, NoxiousGnashIDs, NoxiousGnashDurations);
            if (bar != null)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.NoxiousGnash.StrataLevel));
            }
        }
        
        private unsafe void DrawSerpentOfferingsBar(Vector2 origin, IPlayerCharacter player)
        {
            var instance = JobGaugeManager.Instance();
            var gauge = (ViperGauge*)instance->CurrentGauge;
            
            float reawakenedDuration = Utils.StatusListForBattleChara(player).FirstOrDefault(o => o.StatusId is 3670 or 4094 && o.RemainingTime > 0f)?.RemainingTime ?? 0f;
            bool isReawakened = reawakenedDuration > 0;
            
            var serpentOffering = isReawakened ? reawakenedDuration : gauge->SerpentOffering;
            
            if (!Config.SerpentOfferings.HideWhenInactive)
            {
                Config.SerpentOfferings.Label.SetValue(serpentOffering);

                BarHud bar = BarUtilities.GetProgressBar(Config.SerpentOfferings, serpentOffering, isReawakened ? 30f : 100f, 0f, player);
                AddDrawActions(bar.GetDrawActions(origin, Config.SerpentOfferings.StrataLevel));
            }
        }
    }

    public enum ViperCombo
    {
        SteelFangs = 34606,
        DreadFangs = 34607,
        HuntersSting = 34608,
        SwiftskinsSting = 34609,
        SteelMaw = 34614,
        DreadMaw = 34615,
        HuntersBite = 34616,
        SwiftskinsBite = 34617
    }

    [Section("Job Specific Bars")]
    [SubSection("Melee", 0)]
    [SubSection("Viper", 1)]
    public class ViperConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.VPR;
        public new static ViperConfig DefaultConfig() { return new ViperConfig(); }

        [NestedConfig("Vipersight Bar", 30)]
        public VipersightBarConfig Vipersight = new VipersightBarConfig(
            new(0, -10),
            new(254, 10),
            new(new Vector4(237f / 255f, 141f / 255f, 7f / 255f, 100f / 100f))
        );

        [NestedConfig("Noxious Gnash Bar", 35)]
        public ProgressBarConfig NoxiousGnash = new ProgressBarConfig(
            new(0, -22),
            new(254, 10),
            new(new Vector4(204f / 255f, 40f / 255f, 40f / 255f, 1f))
        );

        [NestedConfig("Rattling Coil Bar", 40)]
        public ChunkedBarConfig RattlingCoilGauge = new ChunkedBarConfig(
            new(0, -34),
            new(254, 10),
            new(new Vector4(204f / 255f, 40f / 255f, 40f / 255f, 1f))
        );
        
        [NestedConfig("Serpent Offerings Bar", 45)]
        public ProgressBarConfig SerpentOfferings = new ProgressBarConfig(
            new(0, -46),
            new(254, 10),
            new(new Vector4(69f / 255f, 115f / 255f, 202f / 255f, 1f))
        );

        [NestedConfig("Anguine Tribute Bar", 50)]
        public ChunkedBarConfig AnguineTribute = new ChunkedBarConfig(
            new(0, -58),
            new(254, 10),
            new(new Vector4(69f / 255f, 115f / 255f, 202f / 255f, 1f))
        );
        
        [Exportable(false)]
        public class VipersightBarConfig : ChunkedBarConfig
        {
            
            [NestedConfig("Show Glow", 39, separator = false, spacing = true)]
            public BarGlowConfig GlowConfig = new BarGlowConfig();
            
            [ColorEdit4("Combo Start", spacing = true)]
            [Order(41)]
            public PluginConfigColor ComboStartColor = new(new Vector4(230f / 255f, 33f / 255f, 33f / 255f, 100f / 100f));

            [ColorEdit4("Flank ender")]
            [Order(42)]
            public PluginConfigColor ComboEndFlankColor = new(new Vector4(46f / 255f, 228f / 255f, 42f / 255f, 1f));
            
            [ColorEdit4("Hind ender")]
            [Order(43)]
            public PluginConfigColor ComboEndHindColor = new(new Vector4(230f / 255f, 33f / 255f, 33f / 255f, 1f));

            [ColorEdit4("Grim/Default Ender")]
            [Order(44)]
            public PluginConfigColor ComboEndAOEColor = new(new Vector4(69f / 255f, 115f / 255f, 202f / 255f, 1f));

            public VipersightBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
                : base(position, size, fillColor)
            {
            }
        }
    }
}