using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Interface.Bars;
using ImGuiNET;

namespace DelvUI.Interface
{
    public class WarriorHudWindow : HudWindow
    {
        public override uint JobId => 21;

        private int StormsEyeHeight => PluginConfiguration.WARStormsEyeHeight;

        private int StormsEyeWidth => PluginConfiguration.WARStormsEyeWidth;

        private new int XOffset => PluginConfiguration.WARBaseXOffset;

        private new int YOffset => PluginConfiguration.WARBaseYOffset;

        private int BeastGaugeHeight => PluginConfiguration.WARBeastGaugeHeight;

        private int BeastGaugeWidth => PluginConfiguration.WARBeastGaugeWidth;

        private int BeastGaugePadding => PluginConfiguration.WARBeastGaugePadding;

        private int BeastGaugeXOffset => PluginConfiguration.WARBeastGaugeXOffset;

        private int BeastGaugeYOffset => PluginConfiguration.WARBeastGaugeYOffset;

        private int InterBarOffset => PluginConfiguration.WARInterBarOffset;

        private Dictionary<string, uint> InnerReleaseColor => PluginConfiguration.JobColorMap[Jobs.WAR * 1000];

        private Dictionary<string, uint> StormsEyeColor => PluginConfiguration.JobColorMap[Jobs.WAR * 1000 + 1];

        private Dictionary<string, uint> FellCleaveColor => PluginConfiguration.JobColorMap[Jobs.WAR * 1000 + 2];

        private Dictionary<string, uint> NascentChaosColor => PluginConfiguration.JobColorMap[Jobs.WAR * 1000 + 3];

        private Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.WAR * 1000 + 4];

        public WarriorHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _) {
            DrawHealthBar();
            var nextHeight = DrawStormsEyeBar(0);
            DrawBeastGauge(nextHeight);
            DrawTargetBar();
            DrawFocusBar();
            DrawCastBar();
        }

        private int DrawStormsEyeBar(int initialHeight)
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var innerReleaseBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1177);
            var stormsEyeBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 90);

            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset + initialHeight;

            var builder = BarBuilder.Create(xPos, yPos, StormsEyeHeight, StormsEyeWidth);

            float duration = 0f;
            float maximum = 10f;
            Dictionary<string, uint> color = EmptyColor;
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

            Bar bar = builder.AddInnerBar(duration, maximum, color)
                .SetTextMode(BarTextMode.EachChunk)
                .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                .Build();
            
            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);

            return StormsEyeHeight + initialHeight + InterBarOffset;
        }

        private int DrawBeastGauge(int initialHeight) {
            var gauge = PluginInterface.ClientState.JobGauges.Get<WARGauge>();
            var nascentChaosBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1897);
            
            var xPos = CenterX - XOffset + BeastGaugeXOffset;
            var yPos = CenterY + YOffset + initialHeight + BeastGaugeYOffset;

            var builder = BarBuilder.Create(xPos, yPos, BeastGaugeHeight, BeastGaugeWidth)
                .SetChunks(2)
                .AddInnerBar(gauge.BeastGaugeAmount, 100, FellCleaveColor, EmptyColor)
                .SetChunkPadding(BeastGaugePadding);
            if (nascentChaosBuff.Any())
                builder.SetChunksColors(NascentChaosColor);
            var bar = builder.Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);

            return BeastGaugeHeight + initialHeight + InterBarOffset;
        }
    }
}