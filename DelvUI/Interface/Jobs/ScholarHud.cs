using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.Jobs
{
    public class ScholarHud : JobHud
    {
        private new ScholarConfig Config => (ScholarConfig)_config;

        public ScholarHud(ScholarConfig config, string? displayName = null) : base(config, displayName)
        {

        }

        private PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.AetherflowBar.Enabled)
            {
                positions.Add(Config.Position + Config.AetherflowBar.Position);
                sizes.Add(Config.AetherflowBar.Size);
            }

            if (Config.FairyGaugeBar.Enabled)
            {
                positions.Add(Config.Position + Config.FairyGaugeBar.Position);
                sizes.Add(Config.FairyGaugeBar.Size);
            }

            if (Config.BioBar.Enabled)
            {
                positions.Add(Config.Position + Config.BioBar.Position);
                sizes.Add(Config.BioBar.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            Vector2 pos = origin + Config.Position;

            if (Config.BioBar.Enabled)
            {
                DrawBioBar(pos, player);
            }

            if (Config.FairyGaugeBar.Enabled)
            {
                DrawFairyGaugeBar(pos);
            }

            if (Config.AetherflowBar.Enabled)
            {
                DrawAetherBar(pos, player);
            }
        }

        private static List<uint> BioDoTIDs = new List<uint> { 179, 189, 1895 };
        private static List<float> BioDoTDurations = new List<float> { 30, 30, 30 };

        private void DrawBioBar(Vector2 origin, PlayerCharacter player)
        {
            var target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;

            BarUtilities.GetDoTBar(Config.BioBar, player, target, BioDoTIDs, BioDoTDurations)?.
                Draw(origin);
        }

        private void DrawFairyGaugeBar(Vector2 origin)
        {
            byte fairyGauge = Plugin.JobGauges.Get<SCHGauge>().FairyGauge;
            short seraphDuration = Math.Abs(Plugin.JobGauges.Get<SCHGauge>().SeraphTimer);

            if (Config.FairyGaugeBar.HideWhenInactive && fairyGauge == 0 && (seraphDuration == 0 || !Config.FairyGaugeBar.ShowSeraph))
            {
                return;
            }

            if (Config.FairyGaugeBar.ShowSeraph && seraphDuration > 0)
            {
                Config.FairyGaugeBar.Label.SetText($"{seraphDuration / 1000}");
                BarUtilities.GetProgressBar(Config.FairyGaugeBar, seraphDuration, 22000, fillColor: Config.FairyGaugeBar.SeraphColor).
                    Draw(origin);
            }
            else
            {
                Config.FairyGaugeBar.Label.SetText($"{fairyGauge}");
                BarUtilities.GetProgressBar(Config.FairyGaugeBar, fairyGauge, 100).
                    Draw(origin);
            }
        }

        private void DrawAetherBar(Vector2 origin, PlayerCharacter player)
        {
            byte stackCount = player.StatusList.FirstOrDefault(o => o.StatusId is 304)?.StackCount ?? 0;

            if (Config.AetherflowBar.HideWhenInactive && stackCount == 0)
            {
                return;
            };

            BarUtilities.GetChunkedProgressBars(Config.AetherflowBar, 3, stackCount, 3)
                .Draw(origin);
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Healer", 0)]
    [SubSection("Scholar", 1)]
    public class ScholarConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.SCH;
        public new static ScholarConfig DefaultConfig()
        {
            var config = new ScholarConfig();
            config.UseDefaultPrimaryResourceBar = true;
            return config;
        }

        [NestedConfig("Fairy Gauge", 30)]
        public ProgressBarConfig BioBar = new ProgressBarConfig(
            new(0, -10),
            new(254, 20),
            new(new Vector4(50f / 255f, 93f / 255f, 37f / 255f, 1f))
        );

        [NestedConfig("Fairy Gauge", 35)]
        public ScholarFairyGaugeBarConfig FairyGaugeBar = new ScholarFairyGaugeBarConfig(
            new(0, -32),
            new(254, 20),
            new(new Vector4(69f / 255f, 199 / 255f, 164f / 255f, 100f / 100f))
        );

        [NestedConfig("Aetherflow Bar", 40)]
        public ChunkedBarConfig AetherflowBar = new ChunkedBarConfig(
            new(0, -54),
            new(254, 20),
            new(new Vector4(0f / 255f, 255f / 255f, 0f / 255f, 100f / 100f))
        );
    }

    [Exportable(false)]
    public class ScholarFairyGaugeBarConfig : ProgressBarConfig
    {
        [Checkbox("Show Seraph", spacing = true)]
        [Order(50)]
        public bool ShowSeraph = true;

        [ColorEdit4("Color" + "##SeraphColor")]
        [Order(55)]
        public PluginConfigColor SeraphColor = new(new Vector4(232f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        public ScholarFairyGaugeBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
             : base(position, size, fillColor)
        {
        }
    }
}
