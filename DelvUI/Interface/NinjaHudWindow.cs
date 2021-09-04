using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Interface.Bars;
using ImGuiNET;
using DelvUI.Config;

namespace DelvUI.Interface
{
    public class NinjaHudWindow : HudWindow
    {
        public override uint JobId => 30;

        private new int XOffset => PluginConfiguration.NINBaseXOffset;
        private new int YOffset => PluginConfiguration.NINBaseYOffset;

        private bool HutonGaugeEnabled => PluginConfiguration.NINHutonGaugeEnabled;
        private int HutonGaugeHeight => PluginConfiguration.NINHutonGaugeHeight;
        private int HutonGaugeWidth => PluginConfiguration.NINHutonGaugeWidth;
        private int HutonGaugeXOffset => PluginConfiguration.NINHutonGaugeXOffset;
        private int HutonGaugeYOffset => PluginConfiguration.NINHutonGaugeYOffset;

        private bool NinkiGaugeEnabled => PluginConfiguration.NINNinkiGaugeEnabled;
        private bool NinkiGaugeText => PluginConfiguration.NINNinkiGaugeText;
        private bool NinkiChunked => PluginConfiguration.NINNinkiChunked;
        private int NinkiGaugeHeight => PluginConfiguration.NINNinkiGaugeHeight;
        private int NinkiGaugeWidth => PluginConfiguration.NINNinkiGaugeWidth;
        private int NinkiGaugePadding => PluginConfiguration.NINNinkiGaugePadding;
        private int NinkiGaugeXOffset => PluginConfiguration.NINNinkiGaugeXOffset;
        private int NinkiGaugeYOffset => PluginConfiguration.NINNinkiGaugeYOffset;

        private bool TrickBarEnabled => PluginConfiguration.NINTrickBarEnabled;
        private bool TrickBarText => PluginConfiguration.NINTrickBarText;
        private bool SuitonBarText => PluginConfiguration.NINSuitonBarText;
        private int TrickBarHeight => PluginConfiguration.NINTrickBarHeight;
        private int TrickBarWidth => PluginConfiguration.NINTrickBarWidth;
        private int TrickBarXOffset => PluginConfiguration.NINTrickBarXOffset;
        private int TrickBarYOffset => PluginConfiguration.NINTrickBarYOffset;

        private Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.NIN * 1000];
        private Dictionary<string, uint> HutonColor => PluginConfiguration.JobColorMap[Jobs.NIN * 1000 + 1];
        private Dictionary<string, uint> NinkiColor => PluginConfiguration.JobColorMap[Jobs.NIN * 1000 + 2];
        private Dictionary<string, uint> NinkiNotFilledColor => PluginConfiguration.JobColorMap[Jobs.NIN * 1000 + 3];
        private Dictionary<string, uint> TrickColor => PluginConfiguration.JobColorMap[Jobs.NIN * 1000 + 4];
        private Dictionary<string, uint> SuitonColor => PluginConfiguration.JobColorMap[Jobs.NIN * 1000 + 5];

        public NinjaHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _)
        {
            if (HutonGaugeEnabled)
                DrawHutonGauge();
            if (NinkiGaugeEnabled)
                DrawNinkiGauge();
            if (TrickBarEnabled)
                DrawTrickAndSuitonGauge();
        }

        protected override void DrawPrimaryResourceBar()
        {
        }

        private void DrawHutonGauge()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<NINGauge>();
            var hutonDurationLeft = (int)Math.Ceiling((float) (gauge.HutonTimeLeft / (double)1000));

            var xPos = CenterX - XOffset + HutonGaugeXOffset;
            var yPos = CenterY + YOffset + HutonGaugeYOffset;

            var builder = BarBuilder.Create(xPos, yPos, HutonGaugeHeight, HutonGaugeWidth);
            float maximum = 70f;

            Bar bar = builder.AddInnerBar(Math.Abs(hutonDurationLeft), maximum, HutonColor)
                .SetTextMode(BarTextMode.Single)
                .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                .SetBackgroundColor(EmptyColor["background"])
                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawNinkiGauge()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<NINGauge>();

            var xPos = CenterX - XOffset + NinkiGaugeXOffset;
            var yPos = CenterY + YOffset + NinkiGaugeYOffset;

            var builder = BarBuilder.Create(xPos, yPos, NinkiGaugeHeight, NinkiGaugeWidth);
            if(NinkiChunked)
            {
                builder.SetChunks(2)
                .SetChunkPadding(NinkiGaugePadding)
                .AddInnerBar(gauge.Ninki, 100, NinkiColor, NinkiNotFilledColor);
            } else
            {
                builder.AddInnerBar(gauge.Ninki, 100, NinkiColor);
            }
            builder.SetBackgroundColor(EmptyColor["background"]);
            if(NinkiGaugeText)
            {
                builder.SetTextMode(BarTextMode.Single)
                       .SetText(NinkiChunked ? BarTextPosition.CenterLeft : BarTextPosition.CenterMiddle, BarTextType.Current);
            }
            var bar = builder.Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawTrickAndSuitonGauge()
        {
            var xPos = CenterX - XOffset + TrickBarXOffset;
            var yPos = CenterY + YOffset + TrickBarYOffset;

            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;
            var trickDuration = 0f;
            const float trickMaxDuration = 15f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, TrickBarHeight, TrickBarWidth);
            if (target is Dalamud.Game.ClientState.Actors.Types.Chara)
            {
                var trickStatus = target.StatusEffects.FirstOrDefault(o => o.EffectId == 638 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId);
                trickDuration = Math.Max(trickStatus.Duration, 0);
            }

            builder.AddInnerBar(trickDuration, trickMaxDuration, TrickColor);

            if (trickDuration != 0 && TrickBarText)
            {
                builder.SetTextMode(BarTextMode.Single)
                       .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            var suitonBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 507);
            if (suitonBuff.Any())
            {
                var suitonDuration = Math.Abs(suitonBuff.First().Duration);
                builder.AddInnerBar(suitonDuration, 20, SuitonColor);
                if(SuitonBarText)
                {
                    builder.SetTextMode(BarTextMode.Single)
                           .SetText(BarTextPosition.CenterRight, BarTextType.Current, PluginConfiguration.NINSuitonColor, Vector4.UnitW, null);
                }
            }

            Bar bar = builder.Build();
            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }
    }
}