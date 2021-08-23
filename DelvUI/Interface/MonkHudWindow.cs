using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dalamud.Interface;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui;
using Dalamud.Plugin;
using ImGuiNET;
using DelvUI.Interface.Bars;
using DelvUI.Config;

namespace DelvUI.Interface
{
    public class MonkHudWindow : HudWindow
    {
        public override uint JobId => 20;

        #region MNK Integration's

        private new int XOffset => PluginConfiguration.MNKBaseXOffset;
        private new int YOffset => PluginConfiguration.MNKBaseYOffset;
        private bool TwinSnakesBarFlipped => PluginConfiguration.TwinSnakesBarFlipped;
        private bool RiddleOfEarthBarFlipped => PluginConfiguration.TwinSnakesBarFlipped;
        private bool PerfectBalanceBarFlipped => PluginConfiguration.PerfectBalanceBarFlipped;
        private bool DemolishEnabled => PluginConfiguration.DemolishEnabled;
        private bool ChakraEnabled => PluginConfiguration.ChakraEnabled;
        private bool LeadenFistEnabled => PluginConfiguration.LeadenFistEnabled;
        private bool TwinSnakesEnabled => PluginConfiguration.TwinSnakesEnabled;
        private bool RiddleOfEarthEnabled => PluginConfiguration.RiddleOfEarthEnabled;
        private bool PerfectBalanceEnabled => PluginConfiguration.PerfectBalanceEnabled;
        private bool TrueNorthEnabled => PluginConfiguration.TrueNorthEnabled;
        private bool FormsEnabled => PluginConfiguration.FormsEnabled;
        private int DemolishHeight => PluginConfiguration.MNKDemolishHeight;
        private int DemolishWidth => PluginConfiguration.MNKDemolishWidth;
        private int DemolishXOffset => PluginConfiguration.MNKDemolishXOffset;
        private int DemolishYOffset => PluginConfiguration.MNKDemolishYOffset;
        private int ChakraHeight => PluginConfiguration.MNKChakraHeight;
        private int ChakraWidth => PluginConfiguration.MNKChakraWidth;
        private int ChakraXOffset => PluginConfiguration.MNKChakraXOffset;
        private int ChakraYOffset => PluginConfiguration.MNKChakraYOffset;
        private int LeadenFistHeight => PluginConfiguration.MNKLeadenFistHeight;
        private int LeadenFistWidth => PluginConfiguration.MNKLeadenFistWidth;
        private int LeadenFistXOffset => PluginConfiguration.MNKLeadenFistXOffset;
        private int LeadenFistYOffset => PluginConfiguration.MNKLeadenFistYOffset;
        private int TwinSnakesHeight => PluginConfiguration.MNKTwinSnakesHeight;
        private int TwinSnakesWidth => PluginConfiguration.MNKTwinSnakesWidth;
        private int TwinSnakesXOffset => PluginConfiguration.MNKTwinSnakesXOffset;
        private int TwinSnakesYOffset => PluginConfiguration.MNKTwinSnakesYOffset;
        private int RiddleOfEarthHeight => PluginConfiguration.MNKRiddleOfEarthHeight;
        private int RiddleOfEarthWidth => PluginConfiguration.MNKRiddleOfEarthWidth;
        private int RiddleOfEarthXOffset => PluginConfiguration.MNKRiddleOfEarthXOffset;
        private int RiddleOfEarthYOffset => PluginConfiguration.MNKRiddleOfEarthYOffset;
        private int PerfectBalanceHeight => PluginConfiguration.MNKPerfectBalanceHeight;
        private int PerfectBalanceWidth => PluginConfiguration.MNKPerfectBalanceWidth;
        private int PerfectBalanceXOffset => PluginConfiguration.MNKPerfectBalanceXOffset;
        private int PerfectBalanceYOffset => PluginConfiguration.MNKPerfectBalanceYOffset;
        private int TrueNorthHeight => PluginConfiguration.MNKTrueNorthHeight;
        private int TrueNorthWidth => PluginConfiguration.MNKTrueNorthWidth;
        private int TrueNorthXOffset => PluginConfiguration.MNKTrueNorthXOffset;
        private int TrueNorthYOffset => PluginConfiguration.MNKTrueNorthYOffset;
        private int FormsHeight => PluginConfiguration.MNKFormsHeight;
        private int FormsWidth => PluginConfiguration.MNKFormsWidth;
        private int FormsXOffset => PluginConfiguration.MNKFormsXOffset;
        private int FormsYOffset => PluginConfiguration.MNKFormsYOffset;

        private Dictionary<string, uint> DemolishColor => PluginConfiguration.JobColorMap[Jobs.MNK * 1000];
        private Dictionary<string, uint> ChakraColor => PluginConfiguration.JobColorMap[Jobs.MNK * 1000 + 1];
        private Dictionary<string, uint> LeadenFistColor => PluginConfiguration.JobColorMap[Jobs.MNK * 1000 + 2];
        private Dictionary<string, uint> TwinSnakesColor => PluginConfiguration.JobColorMap[Jobs.MNK * 1000 + 3];
        private Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.MNK * 1000 + 4];
        private Dictionary<string, uint> RiddleOfEarthColor => PluginConfiguration.JobColorMap[Jobs.MNK * 1000 + 5];
        private Dictionary<string, uint> PerfectBalanceColor => PluginConfiguration.JobColorMap[Jobs.MNK * 1000 + 6];
        private Dictionary<string, uint> TrueNorthColor => PluginConfiguration.JobColorMap[Jobs.MNK * 1000 + 7];
        private Dictionary<string, uint> FormsColor => PluginConfiguration.JobColorMap[Jobs.MNK * 1000 + 8];

        #endregion

        public MonkHudWindow(
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
            if (FormsEnabled) {
                DrawFormsBar();
            }

            if (RiddleOfEarthEnabled) {
                DrawRiddleOfEarthBar();
            }

            if (PerfectBalanceEnabled) {
                DrawPerfectBalanceBar();
            }

            if (TrueNorthEnabled) {
                DrawTrueNorthBar();
            }

            if (ChakraEnabled) {
                DrawChakraGauge();
            }

            if (LeadenFistEnabled) {
                DrawLeadenFistBar();
            }

            if (TwinSnakesEnabled) {
                DrawTwinSnakesBar();
            }

            if (DemolishEnabled) {
                DrawDemolishBar();
            }
        }

        protected override void DrawPrimaryResourceBar() {
        }

        private void DrawFormsBar()
        {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var target = ClientState.LocalPlayer;
            var opoOpoForm = target.StatusList.FirstOrDefault(o => o.StatusId == 107);
            var raptorForm = target.StatusList.FirstOrDefault(o => o.StatusId == 108);
            var coeurlForm = target.StatusList.FirstOrDefault(o => o.StatusId == 109);
            var formlessFist = target.StatusList.FirstOrDefault(o => o.StatusId == 2513);

            var opoOpoFormDuration = opoOpoForm?.RemainingTime ?? 0f;
            var raptorFormDuration = raptorForm?.RemainingTime ?? 0f;
            var coeurlFormDuration = coeurlForm?.RemainingTime ?? 0f;

            var xPos = CenterX - XOffset + FormsXOffset + 33;
            var yPos = CenterY + YOffset - FormsYOffset - 87;

            var builder = BarBuilder.Create(xPos, yPos, FormsHeight, FormsWidth);
            const float maximum = 15f;

            if (opoOpoFormDuration > 0) {
                var bar = builder.AddInnerBar(Math.Abs(opoOpoFormDuration), maximum, FormsColor)
                    .SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, "Opo-Opo Form")
                    .SetBackgroundColor(EmptyColor["background"])
                    .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
            
            if (raptorFormDuration > 0) {
                var bar = builder.AddInnerBar(Math.Abs(raptorFormDuration), maximum, FormsColor)
                    .SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, "Raptor Form")
                    .SetBackgroundColor(EmptyColor["background"])
                    .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
            
            if (coeurlFormDuration > 0) {
                var bar = builder.AddInnerBar(Math.Abs(coeurlFormDuration), maximum, FormsColor)
                    .SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, "Coeurl Form")
                    .SetBackgroundColor(EmptyColor["background"])
                    .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
            
            if (formlessFist?.RemainingTime > 0) {
                var bar = builder.AddInnerBar(Math.Abs(formlessFist.RemainingTime), maximum, FormsColor)
                    .SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, "Formless Fist")
                    .SetBackgroundColor(EmptyColor["background"])
                    .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
            else {
                var bar = builder.AddInnerBar(0, maximum, FormsColor)
                    .SetBackgroundColor(EmptyColor["background"])
                    .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
        }

        private void DrawTrueNorthBar()
        {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var target = ClientState.LocalPlayer;
            var trueNorth = target.StatusList.FirstOrDefault(o => o.StatusId == 1250);
            var trueNorthDuration = trueNorth?.RemainingTime ?? 0f;

            var xPos = CenterX - XOffset + TrueNorthXOffset + 172;
            var yPos = CenterY + YOffset - TrueNorthYOffset - 65;

            var builder = BarBuilder.Create(xPos, yPos, TrueNorthHeight, TrueNorthWidth);
            const float maximum = 10f;

            if (trueNorthDuration > 0) {
                var bar = builder.AddInnerBar(Math.Abs(trueNorthDuration), maximum, TrueNorthColor)
                    .SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                    .SetBackgroundColor(EmptyColor["background"])
                    .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
            else {
                var bar = builder.AddInnerBar(Math.Abs(trueNorthDuration), maximum, TrueNorthColor)
                    .SetBackgroundColor(EmptyColor["background"])
                    .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
        }

        private void DrawPerfectBalanceBar()
        {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var target = ClientState.LocalPlayer;
            var perfectBalance = target.StatusList.FirstOrDefault(o => o.StatusId == 110);
            var perfectBalanceDuration = perfectBalance?.StackCount ?? 0;

            var xPos = CenterX - XOffset + PerfectBalanceXOffset + 150;
            var yPos = CenterY + YOffset - PerfectBalanceYOffset - 65;

            var builder = BarBuilder.Create(xPos, yPos, PerfectBalanceHeight, PerfectBalanceWidth);
            const float maximum = 6f;

            if (perfectBalanceDuration > 0) {
                var bar = builder.AddInnerBar(Math.Abs(perfectBalanceDuration), maximum, PerfectBalanceColor)
                    .SetVertical(true)
                    .SetFlipDrainDirection(PerfectBalanceBarFlipped)
                    .SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                    .SetBackgroundColor(EmptyColor["background"])
                    .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
            else {
                var bar = builder.AddInnerBar(Math.Abs(perfectBalanceDuration), maximum, PerfectBalanceColor)
                    .SetBackgroundColor(EmptyColor["background"])
                    .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
        }

        private void DrawRiddleOfEarthBar() {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var target = ClientState.LocalPlayer;
            var riddleOfEarth = target.StatusList.FirstOrDefault(o => o.StatusId == 1179);
            var riddleOfEarthDuration = riddleOfEarth?.StackCount ?? 0;

            var xPos = CenterX - XOffset + RiddleOfEarthXOffset + 33;
            var yPos = CenterY + YOffset - RiddleOfEarthYOffset - 65;

            var builder = BarBuilder.Create(xPos, yPos, RiddleOfEarthHeight, RiddleOfEarthWidth);
            const float maximum = 3f;

            if (riddleOfEarthDuration > 0) {
                var bar = builder.AddInnerBar(Math.Abs(riddleOfEarthDuration), maximum, RiddleOfEarthColor)
                    .SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                    .SetBackgroundColor(EmptyColor["background"])
                    .SetFlipDrainDirection(RiddleOfEarthBarFlipped)
                    .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
            else {
                var bar = builder.AddInnerBar(Math.Abs(riddleOfEarthDuration), maximum, RiddleOfEarthColor)
                    .SetBackgroundColor(EmptyColor["background"])
                    .SetFlipDrainDirection(RiddleOfEarthBarFlipped == false)
                    .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
        }

        private void DrawChakraGauge() {
            var gauge = JobGauges.Get<MNKGauge>();

            var xPos = CenterX - XOffset + ChakraXOffset + 33;
            var yPos = CenterY + YOffset - ChakraYOffset - 43;

            var bar = BarBuilder.Create(xPos, yPos, ChakraHeight, ChakraWidth)
                .SetChunks(5)
                .SetChunkPadding(2)
                .AddInnerBar(gauge.Chakra, 5, ChakraColor, EmptyColor)
                .SetBackgroundColor(EmptyColor["background"])
                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawTwinSnakesBar() {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var target = ClientState.LocalPlayer;
            var twinSnakes = target.StatusList.FirstOrDefault(o => o.StatusId == 101);
            var twinSnakesDuration = twinSnakes?.RemainingTime ?? 0f;

            var xPos = CenterX - XOffset + TwinSnakesXOffset + 33;
            var yPos = CenterY + YOffset - TwinSnakesYOffset - 21;

            var builder = BarBuilder.Create(xPos, yPos, TwinSnakesHeight, TwinSnakesWidth);
            const float maximum = 15f;

            var bar = builder.AddInnerBar(Math.Abs(twinSnakesDuration), maximum, TwinSnakesColor)
                .SetTextMode(BarTextMode.EachChunk)
                .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                .SetBackgroundColor(EmptyColor["background"])
                .SetFlipDrainDirection(TwinSnakesBarFlipped)
                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawLeadenFistBar() {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var target = ClientState.LocalPlayer;
            var leadenFist = target.StatusList.FirstOrDefault(o => o.StatusId == 1861);
            var leadenFistDuration = leadenFist?.RemainingTime ?? 0f;

            var xPos = CenterX - XOffset + LeadenFistXOffset + 146;
            var yPos = CenterY + YOffset - LeadenFistYOffset - 21;

            var builder = BarBuilder.Create(xPos, yPos, LeadenFistHeight, LeadenFistWidth);
            const float maximum = 30f;

            if (leadenFistDuration > 0) {
                var bar = builder.AddInnerBar(Math.Abs(leadenFistDuration), maximum, LeadenFistColor)
                    .SetVertical(true)
                    .SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                    .SetBackgroundColor(EmptyColor["background"])
                    .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
            else {
                var bar = builder.AddInnerBar(Math.Abs(leadenFistDuration), maximum, LeadenFistColor)
                    .SetBackgroundColor(EmptyColor["background"])
                    .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
        }

        private void DrawDemolishBar()
        {
            var actor = TargetManager.SoftTarget ?? TargetManager.Target ?? ClientState.LocalPlayer;

            var demolishDuration = 0f;
            if (actor is BattleChara target) {
                var demolish = target.StatusList.FirstOrDefault(o => o.StatusId is 246 or 1309);
                demolishDuration = demolish?.RemainingTime ?? 0f;
            }

            var xPos = CenterX - XOffset + DemolishXOffset + 176;
            var yPos = CenterY + YOffset - DemolishYOffset - 21;

            var builder = BarBuilder.Create(xPos, yPos, DemolishHeight, DemolishWidth);
            const float maximum = 18f;

            var bar = builder.AddInnerBar(Math.Abs(demolishDuration), maximum, DemolishColor)
                .SetTextMode(BarTextMode.EachChunk)
                .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                .SetBackgroundColor(EmptyColor["background"])
                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }
    }
}