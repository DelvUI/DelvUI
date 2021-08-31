using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Interface.Bars;
using ImGuiNET;

namespace DelvUI.Interface
{
    public class NinjaHudWindow : HudWindow
    {
        public override uint JobId => 30;

        private new int XOffset => PluginConfiguration.NINBaseXOffset;
        private new int YOffset => PluginConfiguration.NINBaseYOffset;

        private int HutonGaugeHeight => PluginConfiguration.NINHutonGaugeHeight;
        private int HutonGaugeWidth => PluginConfiguration.NINHutonGaugeWidth;
        private int NinkiGaugeHeight => PluginConfiguration.NINNinkiGaugeHeight;
        private int NinkiGaugeWidth => PluginConfiguration.NINNinkiGaugeWidth;
        private int NinkiGaugePadding => PluginConfiguration.NINNinkiGaugePadding;
        private int NinkiGaugeXOffset => PluginConfiguration.NINNinkiGaugeXOffset;
        private int NinkiGaugeYOffset => PluginConfiguration.NINNinkiGaugeYOffset;
        private int TrickBarHeight => PluginConfiguration.NINTrickBarHeight;
        private int TrickBarWidth => PluginConfiguration.NINTrickBarWidth;
        private int TrickBarXOffset => PluginConfiguration.NINTrickBarXOffset;
        private int TrickBarYOffset => PluginConfiguration.NINTrickBarYOffset;

        private Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.NIN * 1000];
        private Dictionary<string, uint> HutonColor => PluginConfiguration.JobColorMap[Jobs.NIN * 1000 + 1];
        private Dictionary<string, uint> NinkiColor => PluginConfiguration.JobColorMap[Jobs.NIN * 1000 + 2];
        private Dictionary<string, uint> TrickColor => PluginConfiguration.JobColorMap[Jobs.NIN * 1000 + 3];
        private Dictionary<string, uint> SuitonColor => PluginConfiguration.JobColorMap[Jobs.NIN * 1000 + 4];

        private int InterBarOffset => PluginConfiguration.NINInterBarOffset;

        public NinjaHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _)
        {
            var nextHeight = DrawHutonGauge(0);
            nextHeight = DrawNinkiGauge(nextHeight);
            DrawTrickAndSuitonBar(nextHeight);
        }

        protected override void DrawPrimaryResourceBar()
        {
        }

        private int DrawHutonGauge(int initialHeight)
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<NINGauge>();
            var hutonDurationLeft = (int)Math.Ceiling((float) (gauge.HutonTimeLeft / (double)1000));

            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset + initialHeight;

            var builder = BarBuilder.Create(xPos, yPos, HutonGaugeHeight, HutonGaugeWidth);
            float maximum = 70f;

            Bar bar = builder.AddInnerBar(Math.Abs(hutonDurationLeft), maximum, HutonColor)
                .SetTextMode(BarTextMode.EachChunk)
                .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);

            return HutonGaugeHeight + initialHeight + InterBarOffset;
        }

        private int DrawNinkiGauge(int initialHeight)
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<NINGauge>();

            var xPos = CenterX - XOffset + NinkiGaugeXOffset;
            var yPos = CenterY + YOffset + initialHeight + NinkiGaugeYOffset;

            var bar = BarBuilder.Create(xPos, yPos, NinkiGaugeHeight, NinkiGaugeWidth)
                .SetChunks(2)
                .SetChunkPadding(NinkiGaugePadding)
                .AddInnerBar(gauge.Ninki, 100, NinkiColor, EmptyColor)
                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);

            return NinkiGaugeHeight + initialHeight + InterBarOffset;
        }

        private int DrawTrickAndSuitonBar(int initialHeight)
        {
            var xPos = CenterX - XOffset + TrickBarXOffset;
            var yPos = CenterY + YOffset + initialHeight + TrickBarYOffset;

            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;
            var bar = BarBuilder.Create(xPos, yPos, TrickBarHeight, TrickBarWidth);
            var targeted = target is Dalamud.Game.ClientState.Actors.Types.Chara;
            if(targeted)
            {
                // TODO figure out which one of these is actually the trick attack vuln up debuff!
                var trickStatus = target.StatusEffects.FirstOrDefault(o => o.EffectId == 2014 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId ||
                                                                           o.EffectId == 64 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId ||
                                                                           o.EffectId == 444 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId ||
                                                                           o.EffectId == 638 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId ||
                                                                           o.EffectId == 1054 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId ||
                                                                           o.EffectId == 1208 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId ||
                                                                           o.EffectId == 1402 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId ||
                                                                           o.EffectId == 1845 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId);
                var trickDuration = trickStatus.Duration;
                var trickIsUp = trickStatus.Duration > 0;

                // TODO without the conditional, get a flash of -15 for some reason
                bar.AddInnerBar((trickIsUp ? trickDuration : 0), 15, TrickColor);

                // only show trick attack timer text if it's is up
                if (trickIsUp)
                    bar.SetTextMode(BarTextMode.EachChunk)
                       .SetText(BarTextPosition.CenterLeft, BarTextType.Current, PluginConfiguration.NINTrickColor, Vector4.UnitW, null);
            }
            else
            {
                // nothing is targeted, draw an empty bar
                bar.AddInnerBar(0, 15, EmptyColor);
            }

            var suitonBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 507);
            if (suitonBuff.Any())
            {
                var suitonDuration = Math.Abs(suitonBuff.First().Duration);
                bar.AddInnerBar(suitonDuration, 20, SuitonColor)
                   .SetTextMode(BarTextMode.EachChunk)
                   .SetText(BarTextPosition.CenterRight, BarTextType.Current, PluginConfiguration.NINSuitonColor, Vector4.UnitW, null);
            }

            // draw the Suiton bar
            var drawList = ImGui.GetWindowDrawList();
            bar.Build()
               .Draw(drawList);

            return TrickBarHeight + initialHeight + InterBarOffset;
        }
    }
}