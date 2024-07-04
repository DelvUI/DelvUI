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
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace DelvUI.Interface.Jobs
{
    public class ViperHud : JobHud
    {
        private new ViperConfig Config => (ViperConfig)_config;

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
                var maxStacks = player.Level >= 96 ? 3 : 2;
                BarHud[] bars = BarUtilities.GetChunkedBars(Config.AnguineTribute, maxStacks, gauge->AnguineTribute, maxStacks, 0, player);
                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.AnguineTribute.StrataLevel));
                }
            }
        }

        private void DrawNoxiousGnashBar(Vector2 origin, IPlayerCharacter player)
        {
            float noxiousGnashDuration = Utils.StatusListForBattleChara(player).FirstOrDefault(o => o.StatusId is 3667 && o.RemainingTime > 0f)?.RemainingTime ?? 0f;
            if (!Config.NoxiousGnash.HideWhenInactive || noxiousGnashDuration > 0)
            {
                Config.NoxiousGnash.Label.SetValue(noxiousGnashDuration);

                BarHud bar = BarUtilities.GetProgressBar(Config.NoxiousGnash, noxiousGnashDuration, 20f, 0f, player);
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
            new(254, 20),
            new(new Vector4(204f / 255f, 40f / 255f, 40f / 255f, 1f))
        );

        [NestedConfig("Noxious Gnash Bar", 35)]
        public ProgressBarConfig NoxiousGnash = new ProgressBarConfig(
            new(0, -32),
            new(254, 20),
            new(new Vector4(204f / 255f, 40f / 255f, 40f / 255f, 1f))
        );

        [NestedConfig("Rattling Coil Bar", 40)]
        public ChunkedBarConfig RattlingCoilGauge = new ChunkedBarConfig(
            new(0, -44),
            new(254, 10),
            new(new Vector4(204f / 255f, 40f / 255f, 40f / 255f, 1f))
        );
        
        [NestedConfig("Serpent Offerings Bar", 45)]
        public ProgressBarConfig SerpentOfferings = new ProgressBarConfig(
            new(0, -66),
            new(254, 20),
            new(new Vector4(69f / 255f, 115f / 255f, 202f / 255f, 1f))
        );

        [NestedConfig("Anguine Tribute Bar", 50)]
        public ChunkedBarConfig AnguineTribute = new ChunkedBarConfig(
            new(0, -78),
            new(254, 10),
            new(new Vector4(69f / 255f, 115f / 255f, 202f / 255f, 1f))
        );
        
        [Exportable(false)]
        public class VipersightBarConfig : ChunkedBarConfig
        {
            [ColorEdit4("Fill Color", spacing = true)]
            [Order(41)]
            public PluginConfigColor VSColor = new(new Vector4(237f / 255f, 141f / 255f, 7f / 255f, 100f / 100f));
            
            [ColorEdit4("Steel Fangs", spacing = true)]
            [Order(41)]
            public PluginConfigColor SFColor = new(new Vector4(204f / 255f, 40f / 255f, 40f / 255f, 1f));

            [ColorEdit4("Dread Fangs")]
            [Order(42)]
            public PluginConfigColor DFColor = new(new Vector4(69f / 255f, 115f / 255f, 202f / 255f, 1f));
            
            [NestedConfig("Show Glow", 39, separator = false, spacing = true)]
            public BarGlowConfig GlowConfig = new BarGlowConfig();

            public VipersightBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
                : base(position, size, fillColor)
            {
            }
        }
    }
}