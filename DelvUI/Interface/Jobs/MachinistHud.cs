using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.Jobs
{
    public class MachinistHud : JobHud
    {
        private readonly float[] _robotDuration = { 12.450f, 13.950f, 15.450f, 16.950f, 18.450f, 19.950f };
        private new MachinistConfig Config => (MachinistConfig)_config;

        public MachinistHud(string id, MachinistConfig config, string? displayName = null) : base(id, config, displayName)
        {

        }

        private PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;
        private PluginConfigColor PartialFillColor => GlobalColors.Instance.PartialFillColor;

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.ShowOverheat)
            {
                positions.Add(Config.Position + Config.OverheatPosition);
                sizes.Add(Config.OverheatSize);
            }

            if (Config.ShowHeatGauge)
            {
                positions.Add(Config.Position + Config.HeatGaugePosition);
                sizes.Add(Config.HeatGaugeSize);
            }

            if (Config.ShowBatteryGauge)
            {
                positions.Add(Config.Position + Config.BatteryGaugePosition);
                sizes.Add(Config.BatteryGaugeSize);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            if (Config.ShowOverheat)
            {
                DrawOverheatBar(origin);
            }

            if (Config.ShowHeatGauge)
            {
                DrawHeatGauge(origin);
            }

            if (Config.ShowBatteryGauge)
            {
                DrawBatteryGauge(origin);
            }

            if (Config.ShowWildfire)
            {
                DrawWildfireBar(origin, player);
            }
        }

        private void DrawHeatGauge(Vector2 origin)
        {
            var gauge = Plugin.JobGauges.Get<MCHGauge>();

            if (gauge.Heat == 0 && Config.OnlyShowHeatGaugeWhenActive)
            {
                return;
            }

            var position = origin + Config.Position + Config.HeatGaugePosition - Config.HeatGaugeSize / 2f;

            var builder = BarBuilder.Create(position, Config.HeatGaugeSize)
                                    .SetChunks(2)
                                    .SetChunkPadding(Config.HeatGaugePadding)
                                    .AddInnerBar(gauge.Heat, 100, Config.HeatGaugeFillColor, PartialFillColor);

            if (Config.ShowHeatGaugeText && gauge.Heat != 0)
            {
                builder.SetTextMode(BarTextMode.EachChunk)
                       .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                       .SetBackgroundColor(EmptyColor.Background);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawBatteryGauge(Vector2 origin)
        {
            var gauge = Plugin.JobGauges.Get<MCHGauge>();

            var position = origin + Config.Position + Config.BatteryGaugePosition - Config.BatteryGaugeSize / 2f;

            var builder = BarBuilder.Create(position, Config.BatteryGaugeSize)
                                    .SetChunks(new[] { .5f, .1f, .1f, .1f, .1f, .1f })
                                    .SetChunkPadding(Config.BatteryGaugePadding)
                                    .SetBackgroundColor(EmptyColor.Background);

            if (Config.ShowBatteryGaugeBattery)
            {
                builder.AddInnerBar(gauge.Battery, 100, Config.BatteryFillColor, PartialFillColor);

                if (Config.ShowBatteryGaugeBatteryText && gauge.Battery != 0)
                {
                    builder.SetTextMode(BarTextMode.Single)
                           .SetText(BarTextPosition.CenterLeft, BarTextType.Current, Config.BatteryFillColor.Vector, Vector4.UnitW, null);
                }
            }

            if (gauge.IsRobotActive && Config.ShowBatteryGaugeRobotDuration)
            {
                builder.AddInnerBar(gauge.SummonTimeRemaining / 1000f, _robotDuration[gauge.LastSummonBatteryPower / 10 - 5], Config.RobotFillColor, null);

                if (Config.ShowBatteryGaugeRobotDurationText && gauge.SummonTimeRemaining != 0)
                {
                    builder.SetTextMode(BarTextMode.Single)
                           .SetText(BarTextPosition.CenterRight, BarTextType.Current, Config.RobotFillColor.Vector, Vector4.UnitW, null);
                }
            }

            if (gauge.Battery == 0 && gauge.SummonTimeRemaining == 0 && Config.OnlyShowBatteryGaugeWhenActive)
            {
                return;
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            var bar = builder.Build();
            bar.Draw(drawList);
        }

        private void DrawOverheatBar(Vector2 origin)
        {
            var gauge = Plugin.JobGauges.Get<MCHGauge>();

            if (!gauge.IsOverheated && Config.OnlyShowOverheatWhenActive)
            {
                return;
            }

            var position = origin + Config.Position + Config.OverheatPosition - Config.OverheatSize / 2f;

            var builder = BarBuilder.Create(position, Config.OverheatSize)
                .SetBackgroundColor(EmptyColor.Background);

            if (gauge.IsOverheated)
            {
                builder.AddInnerBar(gauge.OverheatTimeRemaining / 1000f, 8, Config.OverheatFillColor, null);

                if (Config.ShowOverheatText)
                {
                    builder.SetTextMode(BarTextMode.EachChunk)
                           .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawWildfireBar(Vector2 origin, PlayerCharacter player)
        {
            var wildfireBuff = player.StatusList.Where(o => o.StatusId == 1946);
            float duration = 0f;

            var position = origin + Config.Position + Config.WildfirePosition - Config.WildfireSize / 2f;

            var builder = BarBuilder.Create(position, Config.WildfireSize).SetBackgroundColor(EmptyColor.Background);

            if (wildfireBuff.Any())
            {
                duration = wildfireBuff.First().RemainingTime;                

                builder.AddInnerBar(duration, 10, Config.WildfireFillColor, null);

                if (Config.ShowWildfireText)
                {
                    builder.SetTextMode(BarTextMode.EachChunk)
                           .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
            }

            if (duration == 0 && Config.OnlyShowWildfireWhenActive)
            {
                return;
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Ranged", 0)]
    [SubSection("Machinist", 1)]
    public class MachinistConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.MCH;
        public new static MachinistConfig DefaultConfig() { return new MachinistConfig(); }

        #region Overheat
        [Checkbox("Overheat", separator = true)]
        [Order(30)]
        public bool ShowOverheat = true;

        [Checkbox("Only Show When Active" + "##Overheat")]
        [Order(31, collapseWith = nameof(ShowOverheat))]
        public bool OnlyShowOverheatWhenActive = false;

        [Checkbox("Text" + "##Overheat")]
        [Order(35, collapseWith = nameof(ShowOverheat))]
        public bool ShowOverheatText = true;

        [DragFloat2("Position" + "##Overheat", min = -4000f, max = 4000f)]
        [Order(40, collapseWith = nameof(ShowOverheat))]
        public Vector2 OverheatPosition = new(0, -54);

        [DragFloat2("Size" + "##Overheat", min = 0, max = 4000f)]
        [Order(45, collapseWith = nameof(ShowOverheat))]
        public Vector2 OverheatSize = new(254, 20);

        [ColorEdit4("Color" + "##Overheat")]
        [Order(50, collapseWith = nameof(ShowOverheat))]
        public PluginConfigColor OverheatFillColor = new(new Vector4(255f / 255f, 239f / 255f, 14f / 255f, 100f / 100f));
        #endregion

        #region Heat Gauge
        [Checkbox("Show Heat Gauge", separator = true)]
        [Order(55)]
        public bool ShowHeatGauge = true;

        [Checkbox("Only Show When Active" + "##HeatGauge")]
        [Order(56, collapseWith = nameof(ShowHeatGauge))]
        public bool OnlyShowHeatGaugeWhenActive = false;

        [Checkbox("Text" + "##HeatGauge")]
        [Order(60, collapseWith = nameof(ShowHeatGauge))]
        public bool ShowHeatGaugeText = true;

        [DragFloat2("Position" + "##HeatGauge", min = -4000f, max = 4000f)]
        [Order(65, collapseWith = nameof(ShowHeatGauge))]
        public Vector2 HeatGaugePosition = new(0, -32);

        [DragFloat2("Size" + "##HeatGauge", min = 0, max = 4000f)]
        [Order(70, collapseWith = nameof(ShowHeatGauge))]
        public Vector2 HeatGaugeSize = new(254, 20);

        [DragInt("Spacing" + "##HeatGauge", min = 0)]
        [Order(75, collapseWith = nameof(ShowHeatGauge))]
        public int HeatGaugePadding = 2;

        [ColorEdit4("Color" + "##HeatGauge")]
        [Order(80, collapseWith = nameof(ShowHeatGauge))]
        public PluginConfigColor HeatGaugeFillColor = new(new Vector4(201f / 255f, 13f / 255f, 13f / 255f, 100f / 100f));
        #endregion

        #region Battery Gauge
        [Checkbox("Battery", separator = true)]
        [Order(85)]
        public bool ShowBatteryGauge = true;

        [Checkbox("Only Show When Active" + "##BatteryGauge")]
        [Order(86, collapseWith = nameof(ShowBatteryGauge))]
        public bool OnlyShowBatteryGaugeWhenActive = false;

        [Checkbox("Battery" + "##BatteryGauge")]
        [Order(90, collapseWith = nameof(ShowBatteryGauge))]
        public bool ShowBatteryGaugeBattery = true;

        [Checkbox("Text" + "##BatteryGauge")]
        [Order(95, collapseWith = nameof(ShowBatteryGaugeBattery))]
        public bool ShowBatteryGaugeBatteryText = false;

        [Checkbox("Robot" + "##BatteryGauge")]
        [Order(100, collapseWith = nameof(ShowBatteryGauge))]
        public bool ShowBatteryGaugeRobotDuration = true;

        [Checkbox("Timer" + "##Robot")]
        [Order(105, collapseWith = nameof(ShowBatteryGaugeRobotDuration))]
        public bool ShowBatteryGaugeRobotDurationText = true;

        [DragFloat2("Position" + "##BatteryGauge", min = -4000f, max = 4000f)]
        [Order(110, collapseWith = nameof(ShowBatteryGauge))]
        public Vector2 BatteryGaugePosition = new(0, -10);

        [DragFloat2("Size" + "##BatteryGauge", min = 0, max = 4000f)]
        [Order(115, collapseWith = nameof(ShowBatteryGauge))]
        public Vector2 BatteryGaugeSize = new(254, 20);

        [DragInt("Spacing" + "##BatteryGauge", min = 0)]
        [Order(120, collapseWith = nameof(ShowBatteryGauge))]
        public int BatteryGaugePadding = 2;

        [ColorEdit4("Battery" + "##BatteryGauge")]
        [Order(125, collapseWith = nameof(ShowBatteryGauge))]
        public PluginConfigColor BatteryFillColor = new(new Vector4(106f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [ColorEdit4("Robot" + "##BatteryGauge")]
        [Order(130, collapseWith = nameof(ShowBatteryGauge))]
        public PluginConfigColor RobotFillColor = new(new Vector4(153f / 255f, 0f / 255f, 255f / 255f, 100f / 100f));
        #endregion

        #region Wildfire
        [Checkbox("Wildfire", separator = true)]
        [Order(135)]
        public bool ShowWildfire = false;

        [Checkbox("Only Show When Active" + "##Wildfire")]
        [Order(136, collapseWith = nameof(ShowWildfire))]
        public bool OnlyShowWildfireWhenActive = false;

        [Checkbox("Text" + "##Wildfire")]
        [Order(140, collapseWith = nameof(ShowWildfire))]
        public bool ShowWildfireText = true;

        [DragFloat2("Position" + "##Wildfire", min = -4000f, max = 4000f)]
        [Order(145, collapseWith = nameof(ShowWildfire))]
        public Vector2 WildfirePosition = new(0, -76);

        [DragFloat2("Size" + "##Wildfire", min = 0, max = 4000f)]
        [Order(150, collapseWith = nameof(ShowWildfire))]
        public Vector2 WildfireSize = new(254, 20);

        [ColorEdit4("Color" + "##Wildfire")]
        [Order(155, collapseWith = nameof(ShowWildfire))]
        public PluginConfigColor WildfireFillColor = new(new Vector4(255f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));
        #endregion
    }
}
