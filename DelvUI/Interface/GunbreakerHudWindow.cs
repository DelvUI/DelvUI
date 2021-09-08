using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Interface.Bars;
using ImGuiNET;

namespace DelvUI.Interface
{
    public class GunbreakerHudWindow : HudWindow
    {
        public GunbreakerHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        private GunbreakerHudConfig _config => (GunbreakerHudConfig)ConfigurationManager.GetInstance().GetConfiguration(new GunbreakerHudConfig());

        public override uint JobId => Jobs.GNB;

        private Vector2 Origin => new(CenterX + _config.Position.X, CenterY + _config.Position.Y);

        private Dictionary<string, uint> EmptyColor => PluginConfiguration.MiscColorMap["empty"];

        private Vector2 CalculatePosition(Vector2 position, Vector2 size)
        {
            return Origin + position - size / 2f;
        }

        protected override void Draw(bool _)
        {
            if (_config.ShowPowderGauge)
            {
                DrawPowderGauge();
            }

            if (_config.ShowNoMercyBar)
            {
                DrawNoMercyBar();
            }
        }

        protected override void DrawPrimaryResourceBar() { }

        private void DrawPowderGauge()
        {

            var position = CalculatePosition(_config.PowderGaugeBarPosition, _config.PowderGaugeBarSize);
            var builder = BarBuilder.Create(position, _config.PowderGaugeBarSize);

            var gauge = PluginInterface.ClientState.JobGauges.Get<GNBGauge>();

            builder.SetChunks(2)
                   .SetChunkPadding(_config.PowderGaugeSpacing)
                   .AddInnerBar(gauge.NumAmmo, 2, _config.PowderGaugeFillColor.Map, null)
                   .SetBackgroundColor(EmptyColor["background"]);

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawNoMercyBar()
        {

            var position = CalculatePosition(_config.NoMercyBarPosition, _config.NoMercyBarSize);
            var noMercyBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1831);

            var builder = BarBuilder.Create(position, _config.NoMercyBarSize)
                .SetBackgroundColor(EmptyColor["background"]);

            if (noMercyBuff.Any())
            {
                var duration = noMercyBuff.First().Duration;

                builder.AddInnerBar(duration, 20, _config.NoMercyFillColor.Map, null)
                       .SetTextMode(BarTextMode.EachChunk)
                       .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Tank", 0)]
    [SubSection("Gunbreaker", 1)]
    public class GunbreakerHudConfig : PluginConfigObject
    {
        [DragFloat2("Base Position" + "##Gunbreaker", min = -4000f, max = 4000f)]
        [Order(0)]
        public Vector2 Position = new Vector2(0, 0);

        #region Powder Gauge
        [Checkbox("Show Powder Gauge")]
        [CollapseControl(5, 0)]
        public bool ShowPowderGauge = true;

        [DragFloat2("Position" + "##PowderGauge", min = -4000f, max = 4000f)]
        [CollapseWith(0, 0)]
        public Vector2 PowderGaugeBarPosition = new(0, 428);

        [DragFloat2("Size" + "##PowderGauge", min = 1f, max = 4000f)]
        [CollapseWith(5, 0)]
        public Vector2 PowderGaugeBarSize = new(254, 20);

        [DragFloat("Spacing" + "##PowderGauge", min = 0)]
        [CollapseWith(10, 0)]
        public float PowderGaugeSpacing = 2.0f;

        [ColorEdit4("Color" + "##PowderGauge")]
        [CollapseWith(15, 0)]
        public PluginConfigColor PowderGaugeFillColor = new(new Vector4(46f / 255f, 179f / 255f, 255f / 255f, 1f));
        #endregion

        #region No Mercy
        [Checkbox("Show No Mercy Bar")]
        [CollapseControl(10, 1)]
        public bool ShowNoMercyBar = true;

        [DragFloat2("Position" + "##NoMercy", min = -4000f, max = 4000f)]
        [CollapseWith(0, 1)]
        public Vector2 NoMercyBarPosition = new(0, 449);

        [DragFloat2("Size" + "##NoMercy", min = 1f, max = 4000f)]
        [CollapseWith(5, 1)]
        public Vector2 NoMercyBarSize = new(254, 20);

        [ColorEdit4("Color" + "##NoMercy")]
        [CollapseWith(10, 1)]
        public PluginConfigColor NoMercyFillColor = new(new Vector4(252f / 255f, 204f / 255f, 255f / 255f, 1f));
        #endregion
    }
}
