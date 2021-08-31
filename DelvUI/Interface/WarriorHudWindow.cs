using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Interface.Bars;
using ImGuiNET;

namespace DelvUI.Interface
{
    public class WarriorHudWindow : HudWindow
    {
        public override uint JobId => 21;

        private bool StormsEyeEnabled => PluginConfiguration.WARStormsEyeEnabled;
        private bool StormsEyeText => PluginConfiguration.WARStormsEyeText;
        private int StormsEyeHeight => PluginConfiguration.WARStormsEyeHeight;
        private int StormsEyeWidth => PluginConfiguration.WARStormsEyeWidth;

        private int StormsEyeXOffset => PluginConfiguration.WARStormsEyeXOffset;
        private int StormsEyeYOffset => PluginConfiguration.WARStormsEyeYOffset;

        private bool BeastGaugeEnabled => PluginConfiguration.WARBeastGaugeEnabled;
        private bool BeastGaugeText => PluginConfiguration.WARBeastGaugeText;
        private int BeastGaugeHeight => PluginConfiguration.WARBeastGaugeHeight;
        private int BeastGaugeWidth => PluginConfiguration.WARBeastGaugeWidth;
        private int BeastGaugePadding => PluginConfiguration.WARBeastGaugePadding;
        private int BeastGaugeXOffset => PluginConfiguration.WARBeastGaugeXOffset;
        private int BeastGaugeYOffset => PluginConfiguration.WARBeastGaugeYOffset;

        private Dictionary<string, uint> InnerReleaseColor => PluginConfiguration.JobColorMap[Jobs.WAR * 1000];
        private Dictionary<string, uint> StormsEyeColor => PluginConfiguration.JobColorMap[Jobs.WAR * 1000 + 1];
        private Dictionary<string, uint> FellCleaveColor => PluginConfiguration.JobColorMap[Jobs.WAR * 1000 + 2];
        private Dictionary<string, uint> NascentChaosColor => PluginConfiguration.JobColorMap[Jobs.WAR * 1000 + 3];
        private Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.WAR * 1000 + 4];

        public WarriorHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _) {
            if (StormsEyeEnabled)
                DrawStormsEyeBar();
            if (BeastGaugeEnabled)
                DrawBeastGauge();
        }

        protected override void DrawPrimaryResourceBar() {
        }

        private void DrawStormsEyeBar() {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var innerReleaseBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1177);
            var stormsEyeBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 90);

            var xPos = CenterX - StormsEyeXOffset;
            var yPos = CenterY + StormsEyeYOffset;

            var builder = BarBuilder.Create(xPos, yPos, StormsEyeHeight, StormsEyeWidth);

            var duration = 0f;
            var maximum = 10f;
            var color = EmptyColor;
            if (innerReleaseBuff.Any())
            {
                duration = Math.Abs(innerReleaseBuff.First().Duration);
                color = InnerReleaseColor;
            }
            else if (stormsEyeBuff.Any())
            {
                duration = Math.Abs(stormsEyeBuff.First().Duration);
                maximum = 60f;
                color = StormsEyeColor;
            }

            builder.AddInnerBar(duration, maximum, color);

            if (StormsEyeText)
            {
                builder.SetTextMode(BarTextMode.EachChunk)
                    .SetText(new BarText(BarTextPosition.CenterMiddle, BarTextType.Current));
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawBeastGauge() {
            var gauge = PluginInterface.ClientState.JobGauges.Get<WARGauge>();
            var nascentChaosBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1897);

            var xPos = CenterX - BeastGaugeXOffset;
            var yPos = CenterY + BeastGaugeYOffset;

            var builder = BarBuilder.Create(xPos, yPos, BeastGaugeHeight, BeastGaugeWidth)
                .SetChunks(2)
                .AddInnerBar(gauge.BeastGaugeAmount, 100, FellCleaveColor, EmptyColor)
                .SetChunkPadding(BeastGaugePadding);
            if (nascentChaosBuff.Any())
                builder.SetChunksColors(NascentChaosColor);
            if (BeastGaugeText)
            {
                builder.SetTextMode(BarTextMode.EachChunk)
                    .SetText(new BarText(BarTextPosition.CenterMiddle, BarTextType.Current));
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }
    }
}