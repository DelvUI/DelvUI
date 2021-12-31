using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.Jobs
{
    public class GunbreakerHud : JobHud
    {
        private new GunbreakerConfig Config => (GunbreakerConfig)_config;

        public GunbreakerHud(GunbreakerConfig config, string? displayName = null) : base(config, displayName) { }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.PowderGauge.Enabled)
            {
                positions.Add(Config.Position + Config.PowderGauge.Position);
                sizes.Add(Config.PowderGauge.Size);
            }

            if (Config.NoMercy.Enabled)
            {
                positions.Add(Config.Position + Config.NoMercy.Position);
                sizes.Add(Config.NoMercy.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            if (Config.PowderGauge.Enabled)
            {
                DrawPowderGauge(origin + Config.Position, player);
            }

            if (Config.NoMercy.Enabled)
            {
                DrawNoMercyBar(origin + Config.Position, player);
            }
        }

        private void DrawPowderGauge(Vector2 origin, PlayerCharacter player)
        {
            var gauge = Plugin.JobGauges.Get<GNBGauge>();
            if (!Config.PowderGauge.HideWhenInactive || gauge.Ammo > 0)
            {
                var maxCartridges = player.Level >= 88 ? 3 : 2;
                BarHud[] bars = BarUtilities.GetChunkedBars(Config.PowderGauge, maxCartridges, gauge.Ammo, maxCartridges, 0, player);
                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.PowderGauge.StrataLevel));
                }
            }
        }

        private void DrawNoMercyBar(Vector2 origin, PlayerCharacter player)
        {
            float noMercyDuration = player.StatusList.FirstOrDefault(o => o.StatusId == 1831 && o.RemainingTime > 0f)?.RemainingTime ?? 0f;
            if (!Config.NoMercy.HideWhenInactive || noMercyDuration > 0)
            {
                Config.NoMercy.Label.SetValue(noMercyDuration);

                BarHud bar = BarUtilities.GetProgressBar(Config.NoMercy, noMercyDuration, 20f, 0f, player);
                AddDrawActions(bar.GetDrawActions(origin, Config.NoMercy.StrataLevel));
            }
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Tank", 0)]
    [SubSection("Gunbreaker", 1)]
    public class GunbreakerConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.GNB;
        public new static GunbreakerConfig DefaultConfig() { return new GunbreakerConfig(); }

        [NestedConfig("Powder Gauge", 30)]
        public ChunkedBarConfig PowderGauge = new ChunkedBarConfig(
            new(0, -32),
            new(254, 20),
            new(new Vector4(0f / 255f, 162f / 255f, 252f / 255f, 1f))
        );

        [NestedConfig("No Mercy", 35)]
        public ProgressBarConfig NoMercy = new ProgressBarConfig(
            new(0, -10),
            new(254, 20),
            new(new Vector4(252f / 255f, 204f / 255f, 255f / 255f, 1f))
        );
    }
}