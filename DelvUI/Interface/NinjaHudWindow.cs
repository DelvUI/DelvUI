using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui;
using Dalamud.Interface;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Interface.Bars;
using ImGuiNET;

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

        public NinjaHudWindow(
            ClientState clientState,
            DalamudPluginInterface pluginInterface,
            DataManager dataManager,
            Framework framework,
            GameGui gameGui,
            JobGauges jobGauges,
            ObjectTable objectTable, 
            PluginConfiguration pluginConfiguration,
            SigScanner sigScanner,
            TargetManager targetManager,
            UiBuilder uiBuilder
        ) : base(
            clientState,
            pluginInterface,
            dataManager,
            framework,
            gameGui,
            jobGauges,
            objectTable,
            pluginConfiguration,
            sigScanner,
            targetManager,
            uiBuilder
        ) { }

        protected override void Draw(bool _) {
            if (HutonGaugeEnabled) {
                DrawHutonGauge();
            }

            if (NinkiGaugeEnabled) {
                DrawNinkiGauge();
            }

            if (TrickBarEnabled) {
                DrawTrickAndSuitonGauge();
            }
        }

        protected override void DrawPrimaryResourceBar() {
        }

        private void DrawHutonGauge()
        {
            var gauge = JobGauges.Get<NINGauge>();
            var hutonDurationLeft = (int)Math.Ceiling((float) (gauge.HutonTimer / (double)1000));

            var xPos = CenterX - XOffset + HutonGaugeXOffset;
            var yPos = CenterY + YOffset + HutonGaugeYOffset;

            var builder = BarBuilder.Create(xPos, yPos, HutonGaugeHeight, HutonGaugeWidth);
            const float maximum = 70f;

            var bar = builder.AddInnerBar(Math.Abs(hutonDurationLeft), maximum, HutonColor)
                .SetTextMode(BarTextMode.Single)
                .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                .SetBackgroundColor(EmptyColor["background"])
                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawNinkiGauge()
        {
            var gauge = JobGauges.Get<NINGauge>();

            var xPos = CenterX - XOffset + NinkiGaugeXOffset;
            var yPos = CenterY + YOffset + NinkiGaugeYOffset;

            var builder = BarBuilder.Create(xPos, yPos, NinkiGaugeHeight, NinkiGaugeWidth);
            if(NinkiChunked) {
                builder.SetChunks(2)
                .SetChunkPadding(NinkiGaugePadding)
                .AddInnerBar(gauge.Ninki, 100, NinkiColor, NinkiNotFilledColor);
            } 
            else {
                builder.AddInnerBar(gauge.Ninki, 100, NinkiColor);
            }
            builder.SetBackgroundColor(EmptyColor["background"]);
            if(NinkiGaugeText) {
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

            var actor = TargetManager.SoftTarget ?? TargetManager.Target;
            var trickDuration = 0f;
            const float trickMaxDuration = 15f;

            var builder = BarBuilder.Create(xPos, yPos, TrickBarHeight, TrickBarWidth);
            if (actor is BattleChara target) {
                Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
                var trickStatus = target.StatusList.FirstOrDefault(o => o.StatusId == 638 && o.SourceID == ClientState.LocalPlayer.ObjectId);
                trickDuration = Math.Max(trickStatus?.RemainingTime ?? 0f, 0);
            }

            builder.AddInnerBar(trickDuration, trickMaxDuration, TrickColor);

            if (trickDuration != 0 && TrickBarText) {
                builder.SetTextMode(BarTextMode.Single)
                       .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var suitonBuff = ClientState.LocalPlayer.StatusList.Where(o => o.StatusId == 507);
            if (suitonBuff.Any()) {
                var suitonDuration = Math.Abs(suitonBuff.First().RemainingTime);
                builder.AddInnerBar(suitonDuration, 20, SuitonColor);
                
                if (SuitonBarText) {
                    builder.SetTextMode(BarTextMode.Single)
                           .SetText(BarTextPosition.CenterRight, BarTextType.Current, PluginConfiguration.NINSuitonColor, Vector4.UnitW, null);
                }
            }

            var bar = builder.Build();
            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }
    }
}