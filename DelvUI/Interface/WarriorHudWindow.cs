using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Gui;
using Dalamud.Interface;
using Dalamud.Plugin;
using DelvUI.Interface.Bars;
using ImGuiNET;
using DelvUI.Config;

namespace DelvUI.Interface
{
    public class WarriorHudWindow : HudWindow
    {
        public override uint JobId => 21;

        private int BaseXOffset => PluginConfiguration.WARBaseXOffset;
        private int BaseYOffset => PluginConfiguration.WARBaseYOffset;

        private bool StormsEyeEnabled => PluginConfiguration.WARStormsEyeEnabled;
        private bool StormsEyeText => PluginConfiguration.WARStormsEyeText;
        private float StormsEyeTextScale => PluginConfiguration.WARStormsEyeTextScale;
        private int StormsEyeHeight => PluginConfiguration.WARStormsEyeHeight;
        private int StormsEyeWidth => PluginConfiguration.WARStormsEyeWidth;

        private int StormsEyeXOffset => PluginConfiguration.WARStormsEyeXOffset;
        private int StormsEyeYOffset => PluginConfiguration.WARStormsEyeYOffset;

        private bool BeastGaugeEnabled => PluginConfiguration.WARBeastGaugeEnabled;
        private bool BeastGaugeText => PluginConfiguration.WARBeastGaugeText;
        private float BeastGaugeTextScale => PluginConfiguration.WARBeastGaugeTextScale;
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

        public WarriorHudWindow(
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
            if (StormsEyeEnabled)
                DrawStormsEyeBar();
            if (BeastGaugeEnabled)
                DrawBeastGauge();
        }

        protected override void DrawPrimaryResourceBar() {
        }

        private void DrawStormsEyeBar() {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var innerReleaseBuff = ClientState.LocalPlayer.StatusList.Where(o => o.StatusId == 1177);
            var stormsEyeBuff = ClientState.LocalPlayer.StatusList.Where(o => o.StatusId == 90);

            var xPos = CenterX + BaseXOffset - StormsEyeXOffset;
            var yPos = CenterY + BaseYOffset + StormsEyeYOffset;

            var builder = BarBuilder.Create(xPos, yPos, StormsEyeHeight, StormsEyeWidth).SetBackgroundColor(EmptyColor["background"]);

            var duration = 0f;
            var maximum = 10f;
            var color = EmptyColor;

            if (innerReleaseBuff.Any())
            {
                duration = Math.Abs(innerReleaseBuff.First().RemainingTime);
                color = InnerReleaseColor;
            }
            else if (stormsEyeBuff.Any())
            {
                duration = Math.Abs(stormsEyeBuff.First().RemainingTime);
                maximum = 60f;
                color = StormsEyeColor;
            }

            builder.AddInnerBar(duration, maximum, color);

            if (StormsEyeText)
            {
                builder.SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Current, StormsEyeTextScale);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawBeastGauge() {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var nascentChaosBuff = ClientState.LocalPlayer.StatusList.Where(o => o.StatusId == 1897);
            var gauge = JobGauges.Get<WARGauge>();
            var xPos = CenterX + BaseXOffset - BeastGaugeXOffset;
            var yPos = CenterY + BaseYOffset + BeastGaugeYOffset;

            var builder = BarBuilder.Create(xPos, yPos, BeastGaugeHeight, BeastGaugeWidth)
                .SetChunks(2)
                .AddInnerBar(gauge.BeastGauge, 100, FellCleaveColor).SetBackgroundColor(EmptyColor["background"])
                .SetChunkPadding(BeastGaugePadding);

            if (nascentChaosBuff.Any())
                builder.SetChunksColors(NascentChaosColor);
            if (BeastGaugeText)
            {
                builder.SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Current, BeastGaugeTextScale);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }
    }
}