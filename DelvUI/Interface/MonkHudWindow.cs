using System;
using System.Linq;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;
using DelvUI.Interface.Bars;

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
        private bool EnabRiddleOfEarthEnabled => PluginConfiguration.RiddleOfEarthEnabled;
        private bool PerfectBalanceEnabled => PluginConfiguration.PerfectBalanceEnabled;
        private bool TrueNorthEnabled => PluginConfiguration.TrueNorthEnabled;
        private bool FormsEnabled => PluginConfiguration.FormsEnabled;
        protected int DemolishHeight => PluginConfiguration.MNKDemolishHeight;
        protected int DemolishWidth => PluginConfiguration.MNKDemolishWidth;
        protected int DemolishXOffset => PluginConfiguration.MNKDemolishXOffset;
        protected int DemolishYOffset => PluginConfiguration.MNKDemolishYOffset;
        protected int ChakraHeight => PluginConfiguration.MNKChakraHeight;
        protected int ChakraWidth => PluginConfiguration.MNKChakraWidth;
        protected int ChakraXOffset => PluginConfiguration.MNKChakraXOffset;
        protected int ChakraYOffset => PluginConfiguration.MNKChakraYOffset;
        protected int LeadenFistHeight => PluginConfiguration.MNKLeadenFistHeight;
        protected int LeadenFistWidth => PluginConfiguration.MNKLeadenFistWidth;
        protected int LeadenFistXOffset => PluginConfiguration.MNKLeadenFistXOffset;
        protected int LeadenFistYOffset => PluginConfiguration.MNKLeadenFistYOffset;
        protected int TwinSnakesHeight => PluginConfiguration.MNKTwinSnakesHeight;
        protected int TwinSnakesWidth => PluginConfiguration.MNKTwinSnakesWidth;
        protected int TwinSnakesXOffset => PluginConfiguration.MNKTwinSnakesXOffset;
        protected int TwinSnakesYOffset => PluginConfiguration.MNKTwinSnakesYOffset;
        protected int RiddleOfEarthHeight => PluginConfiguration.MNKRiddleOfEarthHeight;
        protected int RiddleOfEarthWidth => PluginConfiguration.MNKRiddleOfEarthWidth;
        protected int RiddleOfEarthXOffset => PluginConfiguration.MNKRiddleOfEarthXOffset;
        protected int RiddleOfEarthYOffset => PluginConfiguration.MNKRiddleOfEarthYOffset;
        protected int PerfectBalanceHeight => PluginConfiguration.MNKPerfectBalanceHeight;
        protected int PerfectBalanceWidth => PluginConfiguration.MNKPerfectBalanceWidth;
        protected int PerfectBalanceXOffset => PluginConfiguration.MNKPerfectBalanceXOffset;
        protected int PerfectBalanceYOffset => PluginConfiguration.MNKPerfectBalanceYOffset;
        protected int TrueNorthHeight => PluginConfiguration.MNKTrueNorthHeight;
        protected int TrueNorthWidth => PluginConfiguration.MNKTrueNorthWidth;
        protected int TrueNorthXOffset => PluginConfiguration.MNKTrueNorthXOffset;
        protected int TrueNorthYOffset => PluginConfiguration.MNKTrueNorthYOffset;
        protected int FormsHeight => PluginConfiguration.MNKFormsHeight;
        protected int FormsWidth => PluginConfiguration.MNKFormsWidth;
        protected int FormsXOffset => PluginConfiguration.MNKFormsXOffset;
        protected int FormsYOffset => PluginConfiguration.MNKFormsYOffset;

        protected Dictionary<string, uint> DemolishColor => PluginConfiguration.JobColorMap[Jobs.MNK * 1000];
        protected Dictionary<string, uint> ChakraColor => PluginConfiguration.JobColorMap[Jobs.MNK * 1000 + 1];
        protected Dictionary<string, uint> LeadenFistColor => PluginConfiguration.JobColorMap[Jobs.MNK * 1000 + 2];
        protected Dictionary<string, uint> TwinSnakesColor => PluginConfiguration.JobColorMap[Jobs.MNK * 1000 + 3];
        protected Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.MNK * 1000 + 4];
        protected Dictionary<string, uint> RiddleOfEarthColor => PluginConfiguration.JobColorMap[Jobs.MNK * 1000 + 5];
        protected Dictionary<string, uint> PerfectBalanceColor => PluginConfiguration.JobColorMap[Jobs.MNK * 1000 + 6];
        protected Dictionary<string, uint> TrueNorthColor => PluginConfiguration.JobColorMap[Jobs.MNK * 1000 + 7];
        protected Dictionary<string, uint> FormsColor => PluginConfiguration.JobColorMap[Jobs.MNK * 1000 + 8];

        #endregion

        public MonkHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _)
        {
            if (FormsEnabled)
                DrawFormsBar();
            if (EnabRiddleOfEarthEnabled)
                DrawRiddleOfEarthBar();
            if (PerfectBalanceEnabled)
                DrawPerfectBalanceBar();
            if (TrueNorthEnabled)
                DrawTrueNorthBar();
            if (ChakraEnabled)
                DrawChakraGauge();
            if (LeadenFistEnabled)
                DrawLeadenFistBar();
            if (TwinSnakesEnabled)
                DrawTwinSnakesBar();
            if (DemolishEnabled)
                DrawDemolishBar();
        }

        protected override void DrawPrimaryResourceBar()
        {
        }

        private void DrawFormsBar()
        {
            var target = PluginInterface.ClientState.LocalPlayer;
            var opoOpoForm = target.StatusEffects.FirstOrDefault(o => o.EffectId == 107);
            var raptorForm = target.StatusEffects.FirstOrDefault(o => o.EffectId == 108);
            var coeurlForm = target.StatusEffects.FirstOrDefault(o => o.EffectId == 109);
            var formlessFist = target.StatusEffects.FirstOrDefault(o => o.EffectId == 2513);

            var opoOpoFormDuration = opoOpoForm.Duration;
            var raptorFormDuration = raptorForm.Duration;
            var coeurlFormDuration = coeurlForm.Duration;
            var formlessFistDuration = formlessFist.Duration;

            var xPos = CenterX - XOffset + FormsXOffset + 33;
            var yPos = CenterY + YOffset - FormsYOffset - 87;

            var builder = BarBuilder.Create(xPos, yPos, FormsHeight, FormsWidth);
            float maximum = 15f;

            if (opoOpoFormDuration > 0)
            {
                Bar bar = builder.AddInnerBar(Math.Abs(opoOpoFormDuration), maximum, FormsColor)
                    .SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, "Opo-Opo Form")
                    .SetBackgroundColor(EmptyColor["background"])
                    .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
            if (raptorFormDuration > 0)
            {
                Bar bar = builder.AddInnerBar(Math.Abs(raptorFormDuration), maximum, FormsColor)
                    .SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, "Raptor Form")
                    .SetBackgroundColor(EmptyColor["background"])
                    .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
            if (coeurlFormDuration > 0)
            {
                Bar bar = builder.AddInnerBar(Math.Abs(coeurlFormDuration), maximum, FormsColor)
                    .SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, "Coeurl Form")
                    .SetBackgroundColor(EmptyColor["background"])
                    .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
            if (formlessFist.Duration > 0)
            {
                Bar bar = builder.AddInnerBar(Math.Abs(formlessFist.Duration), maximum, FormsColor)
                    .SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, "Formless Fist")
                    .SetBackgroundColor(EmptyColor["background"])
                    .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
            else
            {
                Bar bar = builder.AddInnerBar(0, maximum, FormsColor)
                    .SetBackgroundColor(EmptyColor["background"])
                    .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
        }

        private void DrawTrueNorthBar()
        {
            var target = PluginInterface.ClientState.LocalPlayer;
            var trueNorth = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1250);
            var trueNorthDuration = trueNorth.Duration;

            var xPos = CenterX - XOffset + TrueNorthXOffset + 172;
            var yPos = CenterY + YOffset - TrueNorthYOffset - 65;

            var builder = BarBuilder.Create(xPos, yPos, TrueNorthHeight, TrueNorthWidth);
            float maximum = 10f;

            if (trueNorthDuration > 0)
            {
                Bar bar = builder.AddInnerBar(Math.Abs(trueNorthDuration), maximum, TrueNorthColor)
                    .SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                    .SetBackgroundColor(EmptyColor["background"])
                    .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
            else
            {
                Bar bar = builder.AddInnerBar(Math.Abs(trueNorthDuration), maximum, TrueNorthColor)
                    .SetBackgroundColor(EmptyColor["background"])
                    .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
        }

        private void DrawPerfectBalanceBar()
        {
            var target = PluginInterface.ClientState.LocalPlayer;
            var perfectBalance = target.StatusEffects.FirstOrDefault(o => o.EffectId == 110);
            var perfectBalanceDuration = perfectBalance.StackCount;

            var xPos = CenterX - XOffset + PerfectBalanceXOffset + 150;
            var yPos = CenterY + YOffset - PerfectBalanceYOffset - 65;

            var builder = BarBuilder.Create(xPos, yPos, PerfectBalanceHeight, PerfectBalanceWidth);
            float maximum = 6f;

            if (perfectBalanceDuration > 0)
            {
                Bar bar = builder.AddInnerBar(Math.Abs(perfectBalanceDuration), maximum, PerfectBalanceColor)
                    .SetVertical(true)
                    .SetFlipDrainDirection(PerfectBalanceBarFlipped)
                    .SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                    .SetBackgroundColor(EmptyColor["background"])
                    .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
            else
            {
                Bar bar = builder.AddInnerBar(Math.Abs(perfectBalanceDuration), maximum, PerfectBalanceColor)
                    .SetBackgroundColor(EmptyColor["background"])
                    .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
        }

        private void DrawRiddleOfEarthBar()
        {
            var target = PluginInterface.ClientState.LocalPlayer;
            var RiddleOfearth = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1179);
            var RiddleOfearthDuration = RiddleOfearth.StackCount;

            var xPos = CenterX - XOffset + RiddleOfEarthXOffset + 33;
            var yPos = CenterY + YOffset - RiddleOfEarthYOffset - 65;

            var builder = BarBuilder.Create(xPos, yPos, RiddleOfEarthHeight, RiddleOfEarthWidth);
            float maximum = 3f;

            if (RiddleOfearthDuration > 0)
            {
                Bar bar = builder.AddInnerBar(Math.Abs(RiddleOfearthDuration), maximum, RiddleOfEarthColor)
                    .SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                    .SetBackgroundColor(EmptyColor["background"])
                    .SetFlipDrainDirection(RiddleOfEarthBarFlipped)
                    .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
            else
            {
                Bar bar = builder.AddInnerBar(Math.Abs(RiddleOfearthDuration), maximum, RiddleOfEarthColor)
                    .SetBackgroundColor(EmptyColor["background"])
                    .SetFlipDrainDirection(RiddleOfEarthBarFlipped == false)
                    .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
        }

        private void DrawChakraGauge()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<MNKGauge>();

            var xPos = CenterX - XOffset + ChakraXOffset + 33;
            var yPos = CenterY + YOffset - ChakraYOffset - 43;

            var bar = BarBuilder.Create(xPos, yPos, ChakraHeight, ChakraWidth)
                .SetChunks(5)
                .SetChunkPadding(2)
                .AddInnerBar(gauge.NumChakra, 5, ChakraColor, EmptyColor)
                .SetBackgroundColor(EmptyColor["background"])
                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawTwinSnakesBar()
        {
            var target = PluginInterface.ClientState.LocalPlayer;
            var twinSnakes = target.StatusEffects.FirstOrDefault(o => o.EffectId == 101);
            var twinSnakesDuration = twinSnakes.Duration;

            var xPos = CenterX - XOffset + TwinSnakesXOffset + 33;
            var yPos = CenterY + YOffset - TwinSnakesYOffset - 21;

            var builder = BarBuilder.Create(xPos, yPos, TwinSnakesHeight, TwinSnakesWidth);
            float maximum = 15f;

            Bar bar = builder.AddInnerBar(Math.Abs(twinSnakesDuration), maximum, TwinSnakesColor)
                .SetTextMode(BarTextMode.EachChunk)
                .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                .SetBackgroundColor(EmptyColor["background"])
                .SetFlipDrainDirection(TwinSnakesBarFlipped)
                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawLeadenFistBar()
        {
            var target = PluginInterface.ClientState.LocalPlayer;
            var leadenFist = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1861);
            var leadenFistDuration = leadenFist.Duration;

            var xPos = CenterX - XOffset + LeadenFistXOffset + 146;
            var yPos = CenterY + YOffset - LeadenFistYOffset - 21;

            var builder = BarBuilder.Create(xPos, yPos, LeadenFistHeight, LeadenFistWidth);
            float maximum = 30f;

            if (leadenFistDuration > 0)
            {
                Bar bar = builder.AddInnerBar(Math.Abs(leadenFistDuration), maximum, LeadenFistColor)
                    .SetVertical(true)
                    .SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                    .SetBackgroundColor(EmptyColor["background"])
                    .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
            else
            {
                Bar bar = builder.AddInnerBar(Math.Abs(leadenFistDuration), maximum, LeadenFistColor)
                    .SetBackgroundColor(EmptyColor["background"])
                    .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
        }

        private void DrawDemolishBar()
        {
            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget ?? PluginInterface.ClientState.LocalPlayer;
            var Demolish = target.StatusEffects.FirstOrDefault(o => o.EffectId == 246 || o.EffectId == 1309);
            var DemolishDuration = Demolish.Duration;

            var xPos = CenterX - XOffset + DemolishXOffset + 176;
            var yPos = CenterY + YOffset - DemolishYOffset - 21;

            var builder = BarBuilder.Create(xPos, yPos, DemolishHeight, DemolishWidth);
            float maximum = 18f;

            Bar bar = builder.AddInnerBar(Math.Abs(DemolishDuration), maximum, DemolishColor)
                .SetTextMode(BarTextMode.EachChunk)
                .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                .SetBackgroundColor(EmptyColor["background"])
                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }
    }
}