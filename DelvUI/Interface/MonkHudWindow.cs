using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Interface.Bars;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Structs;
using Actor = Dalamud.Game.ClientState.Actors.Types.Actor;

namespace DelvUI.Interface
{
    public class MonkHudWindow : HudWindow
    {
        public MonkHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        public override uint JobId => 20;

        protected override void Draw(bool _)
        {
            if (FormsEnabled)
            {
                DrawFormsBar();
            }

            if (RiddleOfEarthEnabled)
            {
                DrawRiddleOfEarthBar();
            }

            if (PerfectBalanceEnabled)
            {
                DrawPerfectBalanceBar();
            }

            if (TrueNorthEnabled)
            {
                DrawTrueNorthBar();
            }

            if (ChakraEnabled)
            {
                DrawChakraGauge();
            }

            if (LeadenFistEnabled)
            {
                DrawLeadenFistBar();
            }

            if (TwinSnakesEnabled)
            {
                DrawTwinSnakesBar();
            }

            if (DemolishEnabled)
            {
                DrawDemolishBar();
            }
        }

        protected override void DrawPrimaryResourceBar() { }

        private void DrawFormsBar()
        {
            PlayerCharacter target = PluginInterface.ClientState.LocalPlayer;
            StatusEffect opoOpoForm = target.StatusEffects.FirstOrDefault(o => o.EffectId == 107);
            StatusEffect raptorForm = target.StatusEffects.FirstOrDefault(o => o.EffectId == 108);
            StatusEffect coeurlForm = target.StatusEffects.FirstOrDefault(o => o.EffectId == 109);
            StatusEffect formlessFist = target.StatusEffects.FirstOrDefault(o => o.EffectId == 2513);

            var opoOpoFormDuration = opoOpoForm.Duration;
            var raptorFormDuration = raptorForm.Duration;
            var coeurlFormDuration = coeurlForm.Duration;
            var formlessFistDuration = formlessFist.Duration;

            var xPos = CenterX - XOffset + FormsXOffset + 33;
            var yPos = CenterY + YOffset - FormsYOffset - 87;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, FormsHeight, FormsWidth);
            var maximum = 15f;

            if (opoOpoFormDuration > 0)
            {
                Bar bar = builder.AddInnerBar(Math.Abs(opoOpoFormDuration), maximum, FormsColor)
                                 .SetTextMode(BarTextMode.EachChunk)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, "Opo-Opo Form")
                                 .SetBackgroundColor(EmptyColor["background"])
                                 .Build();

                ImDrawListPtr drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }

            if (raptorFormDuration > 0)
            {
                Bar bar = builder.AddInnerBar(Math.Abs(raptorFormDuration), maximum, FormsColor)
                                 .SetTextMode(BarTextMode.EachChunk)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, "Raptor Form")
                                 .SetBackgroundColor(EmptyColor["background"])
                                 .Build();

                ImDrawListPtr drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }

            if (coeurlFormDuration > 0)
            {
                Bar bar = builder.AddInnerBar(Math.Abs(coeurlFormDuration), maximum, FormsColor)
                                 .SetTextMode(BarTextMode.EachChunk)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, "Coeurl Form")
                                 .SetBackgroundColor(EmptyColor["background"])
                                 .Build();

                ImDrawListPtr drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }

            if (formlessFist.Duration > 0)
            {
                Bar bar = builder.AddInnerBar(Math.Abs(formlessFist.Duration), maximum, FormsColor)
                                 .SetTextMode(BarTextMode.EachChunk)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, "Formless Fist")
                                 .SetBackgroundColor(EmptyColor["background"])
                                 .Build();

                ImDrawListPtr drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
            else
            {
                Bar bar = builder.AddInnerBar(0, maximum, FormsColor)
                                 .SetBackgroundColor(EmptyColor["background"])
                                 .Build();

                ImDrawListPtr drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
        }

        private void DrawTrueNorthBar()
        {
            PlayerCharacter target = PluginInterface.ClientState.LocalPlayer;
            StatusEffect trueNorth = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1250);
            var trueNorthDuration = trueNorth.Duration;

            var xPos = CenterX - XOffset + TrueNorthXOffset + 172;
            var yPos = CenterY + YOffset - TrueNorthYOffset - 65;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, TrueNorthHeight, TrueNorthWidth);
            var maximum = 10f;

            if (trueNorthDuration > 0)
            {
                Bar bar = builder.AddInnerBar(Math.Abs(trueNorthDuration), maximum, TrueNorthColor)
                                 .SetTextMode(BarTextMode.EachChunk)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                                 .SetBackgroundColor(EmptyColor["background"])
                                 .Build();

                ImDrawListPtr drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
            else
            {
                Bar bar = builder.AddInnerBar(Math.Abs(trueNorthDuration), maximum, TrueNorthColor)
                                 .SetBackgroundColor(EmptyColor["background"])
                                 .Build();

                ImDrawListPtr drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
        }

        private void DrawPerfectBalanceBar()
        {
            PlayerCharacter target = PluginInterface.ClientState.LocalPlayer;
            StatusEffect perfectBalance = target.StatusEffects.FirstOrDefault(o => o.EffectId == 110);
            var perfectBalanceDuration = perfectBalance.StackCount;

            var xPos = CenterX - XOffset + PerfectBalanceXOffset + 150;
            var yPos = CenterY + YOffset - PerfectBalanceYOffset - 65;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, PerfectBalanceHeight, PerfectBalanceWidth);
            var maximum = 6f;

            if (perfectBalanceDuration > 0)
            {
                Bar bar = builder.AddInnerBar(Math.Abs(perfectBalanceDuration), maximum, PerfectBalanceColor)
                                 .SetVertical(true)
                                 .SetFlipDrainDirection(PerfectBalanceBarFlipped)
                                 .SetTextMode(BarTextMode.EachChunk)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                                 .SetBackgroundColor(EmptyColor["background"])
                                 .Build();

                ImDrawListPtr drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
            else
            {
                Bar bar = builder.AddInnerBar(Math.Abs(perfectBalanceDuration), maximum, PerfectBalanceColor)
                                 .SetBackgroundColor(EmptyColor["background"])
                                 .Build();

                ImDrawListPtr drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
        }

        private void DrawRiddleOfEarthBar()
        {
            PlayerCharacter target = PluginInterface.ClientState.LocalPlayer;
            StatusEffect riddleOfEarth = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1179);
            var riddleOfEarthDuration = riddleOfEarth.StackCount;

            var xPos = CenterX - XOffset + RiddleOfEarthXOffset + 33;
            var yPos = CenterY + YOffset - RiddleOfEarthYOffset - 65;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, RiddleOfEarthHeight, RiddleOfEarthWidth);
            var maximum = 3f;

            if (riddleOfEarthDuration > 0)
            {
                Bar bar = builder.AddInnerBar(Math.Abs(riddleOfEarthDuration), maximum, RiddleOfEarthColor)
                                 .SetTextMode(BarTextMode.EachChunk)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                                 .SetBackgroundColor(EmptyColor["background"])
                                 .SetFlipDrainDirection(RiddleOfEarthBarFlipped)
                                 .Build();

                ImDrawListPtr drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
            else
            {
                Bar bar = builder.AddInnerBar(Math.Abs(riddleOfEarthDuration), maximum, RiddleOfEarthColor)
                                 .SetBackgroundColor(EmptyColor["background"])
                                 .SetFlipDrainDirection(RiddleOfEarthBarFlipped == false)
                                 .Build();

                ImDrawListPtr drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
        }

        private void DrawChakraGauge()
        {
            MNKGauge gauge = PluginInterface.ClientState.JobGauges.Get<MNKGauge>();

            var xPos = CenterX - XOffset + ChakraXOffset + 33;
            var yPos = CenterY + YOffset - ChakraYOffset - 43;

            Bar bar = BarBuilder.Create(xPos, yPos, ChakraHeight, ChakraWidth)
                                .SetChunks(5)
                                .SetChunkPadding(2)
                                .AddInnerBar(gauge.NumChakra, 5, ChakraColor, EmptyColor)
                                .SetBackgroundColor(EmptyColor["background"])
                                .Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawTwinSnakesBar()
        {
            PlayerCharacter target = PluginInterface.ClientState.LocalPlayer;
            StatusEffect twinSnakes = target.StatusEffects.FirstOrDefault(o => o.EffectId == 101);
            var twinSnakesDuration = twinSnakes.Duration;

            var xPos = CenterX - XOffset + TwinSnakesXOffset + 33;
            var yPos = CenterY + YOffset - TwinSnakesYOffset - 21;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, TwinSnakesHeight, TwinSnakesWidth);
            var maximum = 15f;

            Bar bar = builder.AddInnerBar(Math.Abs(twinSnakesDuration), maximum, TwinSnakesColor)
                             .SetTextMode(BarTextMode.EachChunk)
                             .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                             .SetBackgroundColor(EmptyColor["background"])
                             .SetFlipDrainDirection(TwinSnakesBarFlipped)
                             .Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawLeadenFistBar()
        {
            PlayerCharacter target = PluginInterface.ClientState.LocalPlayer;
            StatusEffect leadenFist = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1861);
            var leadenFistDuration = leadenFist.Duration;

            var xPos = CenterX - XOffset + LeadenFistXOffset + 146;
            var yPos = CenterY + YOffset - LeadenFistYOffset - 21;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, LeadenFistHeight, LeadenFistWidth);
            var maximum = 30f;

            if (leadenFistDuration > 0)
            {
                Bar bar = builder.AddInnerBar(Math.Abs(leadenFistDuration), maximum, LeadenFistColor)
                                 .SetVertical(true)
                                 .SetTextMode(BarTextMode.EachChunk)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                                 .SetBackgroundColor(EmptyColor["background"])
                                 .Build();

                ImDrawListPtr drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
            else
            {
                Bar bar = builder.AddInnerBar(Math.Abs(leadenFistDuration), maximum, LeadenFistColor)
                                 .SetBackgroundColor(EmptyColor["background"])
                                 .Build();

                ImDrawListPtr drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
        }

        private void DrawDemolishBar()
        {
            Actor target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget ?? PluginInterface.ClientState.LocalPlayer;
            StatusEffect demolish = target.StatusEffects.FirstOrDefault(o => o.EffectId == 246 || o.EffectId == 1309);
            var demolishDuration = demolish.Duration;

            var xPos = CenterX - XOffset + DemolishXOffset + 176;
            var yPos = CenterY + YOffset - DemolishYOffset - 21;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, DemolishHeight, DemolishWidth);
            var maximum = 18f;

            Bar bar = builder.AddInnerBar(Math.Abs(demolishDuration), maximum, DemolishColor)
                             .SetTextMode(BarTextMode.EachChunk)
                             .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                             .SetBackgroundColor(EmptyColor["background"])
                             .Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

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
    }
}
