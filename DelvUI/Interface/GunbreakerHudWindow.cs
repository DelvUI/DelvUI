using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Interface.Bars;
using DelvUI.Helpers;
using ImGuiNET;
using System.Collections.Generic;
using System.Linq;

namespace DelvUI.Interface
{
    public class GunbreakerHudWindow : HudWindow
    {
        public GunbreakerHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        public override uint JobId => 37;

        private new int XOffset => PluginConfiguration.GNBBaseXOffset;
        private new int YOffset => PluginConfiguration.GNBBaseYOffset;

        private bool PowderGaugeEnabled => PluginConfiguration.GNBPowderGaugeEnabled;
        private int PowderGaugeHeight => PluginConfiguration.GNBPowderGaugeHeight;
        private int PowderGaugeWidth => PluginConfiguration.GNBPowderGaugeWidth;
        private int PowderGaugeXOffset => PluginConfiguration.GNBPowderGaugeXOffset;
        private int PowderGaugeYOffset => PluginConfiguration.GNBPowderGaugeYOffset;
        private int PowderGaugePadding => PluginConfiguration.GNBPowderGaugePadding;
        private Dictionary<string, uint> GunPowderColor => PluginConfiguration.JobColorMap[Jobs.GNB * 1000];
        private Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.GNB * 1000 + 2];
        private bool NoMercyBarEnabled => PluginConfiguration.GNBNoMercyBarEnabled;
        private int NoMercyBarHeight => PluginConfiguration.GNBNoMercyBarHeight;
        private int NoMercyBarWidth => PluginConfiguration.GNBNoMercyBarWidth;
        private int NoMercyBarXOffset => PluginConfiguration.GNBNoMercyBarXOffset;
        private int NoMercyBarYOffset => PluginConfiguration.GNBNoMercyBarYOffset;
        private Dictionary<string, uint> NoMercyColor => PluginConfiguration.JobColorMap[Jobs.GNB * 1000 + 1];

        protected override void Draw(bool _)
        {
            if (PowderGaugeEnabled)
            {
                DrawPowderGauge();
            }

            if (NoMercyBarEnabled)
            {
                DrawNoMercyBar();
            }
        }

        protected override void DrawPrimaryResourceBar() { }

        private void DrawPowderGauge()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<GNBGauge>();

            var xPos = CenterX - XOffset + PowderGaugeXOffset;
            var yPos = CenterY + YOffset + PowderGaugeYOffset;

            var builder = BarBuilder.Create(xPos, yPos, PowderGaugeHeight, PowderGaugeWidth);

            builder.SetChunks(2)
                   .SetChunkPadding(PowderGaugePadding)
                   .AddInnerBar(gauge.NumAmmo, 2, GunPowderColor, null)
                   .SetBackgroundColor(EmptyColor["background"]);

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawNoMercyBar()
        {
            var xPos = CenterX - XOffset + NoMercyBarXOffset;
            var yPos = CenterY + YOffset + NoMercyBarYOffset;

            var noMercyBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1831);

            var builder = BarBuilder.Create(xPos, yPos, NoMercyBarHeight, NoMercyBarWidth).SetBackgroundColor(EmptyColor["background"]);

            if (noMercyBuff.Any())
            {
                var duration = noMercyBuff.First().Duration;

                builder.AddInnerBar(duration, 20, NoMercyColor, null)
                       .SetTextMode(BarTextMode.EachChunk)
                       .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }
    }
}
