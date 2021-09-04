using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.GameStructs;
using DelvUI.Interface.Bars;
using ImGuiNET;

namespace DelvUI.Interface {
    public class DancerHudWindow : HudWindow {
        public DancerHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        public override uint JobId => 38;

        private int BaseXOffset => PluginConfiguration.DNCBaseXOffset;
        private int BaseYOffset => PluginConfiguration.DNCBaseYOffset;

        private bool EspritEnabled => PluginConfiguration.DNCEspritEnabled;
        private bool EspritText => PluginConfiguration.DNCEspritText;
        private int EspritHeight => PluginConfiguration.DNCEspritHeight;
        private int EspritWidth => PluginConfiguration.DNCEspritWidth;
        private int EspritXOffset => PluginConfiguration.DNCEspritXOffset;
        private int EspritYOffset => PluginConfiguration.DNCEspritYOffset;
        private int EspritPadding => PluginConfiguration.DNCEspritPadding;
        private bool FeatherEnabled => PluginConfiguration.DNCFeatherEnabled;
        private bool FlourishingGlowEnabled => PluginConfiguration.DNCFlourishingProcGlowEnabled;
        private int FeatherHeight => PluginConfiguration.DNCFeatherHeight;
        private int FeatherWidth => PluginConfiguration.DNCFeatherWidth;
        private int FeatherXOffset => PluginConfiguration.DNCFeatherXOffset;
        private int FeatherYOffset => PluginConfiguration.DNCFeatherYOffset;
        private int FeatherPadding => PluginConfiguration.DNCFeatherPadding;
        private bool BuffEnabled => PluginConfiguration.DNCBuffEnabled;
        private bool TechnicalBarEnabled => PluginConfiguration.DNCTechnicalBarEnabled;
        private bool TechnicalTextEnabled => PluginConfiguration.DNCTechnicalTextEnabled;
        private bool DevilmentBarEnabled => PluginConfiguration.DNCDevilmentBarEnabled;
        private bool DevilmentTextEnabled => PluginConfiguration.DNCDevilmentTextEnabled;
        private int BuffHeight => PluginConfiguration.DNCBuffHeight;
        private int BuffWidth => PluginConfiguration.DNCBuffWidth;
        private int BuffXOffset => PluginConfiguration.DNCBuffXOffset;
        private int BuffYOffset => PluginConfiguration.DNCBuffYOffset;
        private bool StandardBarEnabled => PluginConfiguration.DNCStandardEnabled;
        private bool StandardTextEnabled => PluginConfiguration.DNCStandardText;
        private int StandardHeight => PluginConfiguration.DNCStandardHeight;
        private int StandardWidth => PluginConfiguration.DNCStandardWidth;
        private int StandardXOffset => PluginConfiguration.DNCStandardXOffset;
        private int StandardYOffset => PluginConfiguration.DNCStandardYOffset;
        private bool StepEnabled => PluginConfiguration.DNCStepEnabled;
        private bool StepGlow => PluginConfiguration.DNCStepGlowEnabled;
        private bool DanceReadyGlow => PluginConfiguration.DNCDanceReadyGlowEnabled;
        private int StepHeight => PluginConfiguration.DNCStepHeight;
        private int StepWidth => PluginConfiguration.DNCStepWidth;
        private int StepXOffset => PluginConfiguration.DNCStepXOffset;
        private int StepYOffset => PluginConfiguration.DNCStepYOffset;
        private int StepPadding => PluginConfiguration.DNCStepPadding;
        private bool ProcEnabled => PluginConfiguration.DNCProcEnabled;
        private bool ProcTimersEnabled => PluginConfiguration.DNCProcTimersEnabled;
        private int ProcHeight => PluginConfiguration.DNCProcHeight;
        private int ProcWidth => PluginConfiguration.DNCProcWidth;
        private int ProcPadding => PluginConfiguration.DNCProcPadding;
        private int ProcXOffset => PluginConfiguration.DNCProcXOffset;
        private int ProcYOffset => PluginConfiguration.DNCProcYOffset;

        private Dictionary<string, uint> EspritColor => PluginConfiguration.JobColorMap[Jobs.DNC * 1000];
        private Dictionary<string, uint> FeatherColor => PluginConfiguration.JobColorMap[Jobs.DNC * 1000 + 1];
        private Dictionary<string, uint> FlourishingProcColor => PluginConfiguration.JobColorMap[Jobs.DNC * 1000 + 2];
        private Dictionary<string, uint> StandardFinishColor => PluginConfiguration.JobColorMap[Jobs.DNC * 1000 + 3];
        private Dictionary<string, uint> TechnicalFinishColor => PluginConfiguration.JobColorMap[Jobs.DNC * 1000 + 4];
        private Dictionary<string, uint> CurrentStepColor => PluginConfiguration.JobColorMap[Jobs.DNC * 1000 + 5];
        private Dictionary<string, uint> StepEmboiteColor => PluginConfiguration.JobColorMap[Jobs.DNC * 1000 + 6];
        private Dictionary<string, uint> StepEntrechatColor => PluginConfiguration.JobColorMap[Jobs.DNC * 1000 + 7];
        private Dictionary<string, uint> StepJeteColor => PluginConfiguration.JobColorMap[Jobs.DNC * 1000 + 8];
        private Dictionary<string, uint> StepPirouetteColor => PluginConfiguration.JobColorMap[Jobs.DNC * 1000 + 9];
        private Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.DNC * 1000 + 10];
        private Dictionary<string, uint> DanceReadyColor => PluginConfiguration.JobColorMap[Jobs.DNC * 1000 + 11];
        private Dictionary<string, uint> DevilmentColor => PluginConfiguration.JobColorMap[Jobs.DNC * 1000 + 12];
        private Dictionary<string, uint> FlourishingCascadeColor => PluginConfiguration.JobColorMap[Jobs.DNC * 1000 + 13];
        private Dictionary<string, uint> FlourishingFountainColor => PluginConfiguration.JobColorMap[Jobs.DNC * 1000 + 14];
        private Dictionary<string, uint> FlourishingWindmillColor => PluginConfiguration.JobColorMap[Jobs.DNC * 1000 + 15];
        private Dictionary<string, uint> FlourishingShowerColor => PluginConfiguration.JobColorMap[Jobs.DNC * 1000 + 16];

        protected override void Draw(bool _) {
            if (EspritEnabled) {
                DrawEspritBar();
            }

            if (ProcEnabled) {
                DrawProcBar();
            }

            if (FeatherEnabled) {
                DrawFeathersBar();
            }

            if (BuffEnabled) {
                DrawBuffBar();
            }

            if (StepEnabled) {
                DrawStepBar();
            }

            if (StandardBarEnabled) {
                DrawStandardBar();
            }
        }

        protected override void DrawPrimaryResourceBar() { }

        private void DrawEspritBar() {
            var gauge = PluginInterface.ClientState.JobGauges.Get<DNCGauge>();

            var xPos = CenterX + BaseXOffset - EspritXOffset;
            var yPos = CenterY + BaseYOffset + EspritYOffset;

            var builder = BarBuilder.Create(xPos, yPos, EspritHeight, EspritWidth)
                                    .SetChunks(2)
                                    .SetChunkPadding(EspritPadding)
                                    .SetBackgroundColor(EmptyColor["background"])
                                    .AddInnerBar(gauge.Esprit, 100, EspritColor, EmptyColor);

            if (EspritText) {
                builder.SetTextMode(BarTextMode.EachChunk)
                       .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawFeathersBar() {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var flourishingBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1820 or 2021);
            var gauge = PluginInterface.ClientState.JobGauges.Get<DNCGauge>();

            var xPos = CenterX + BaseXOffset - FeatherXOffset;
            var yPos = CenterY + BaseYOffset + FeatherYOffset;

            var builder = BarBuilder.Create(xPos, yPos, FeatherHeight, FeatherWidth)
                                    .SetChunks(4)
                                    .SetBackgroundColor(EmptyColor["background"])
                                    .SetChunkPadding(FeatherPadding)
                                    .AddInnerBar(gauge.NumFeathers, 4, FeatherColor);

            if (FlourishingGlowEnabled && flourishingBuff.Any()) {
                builder.SetGlowColor(FlourishingProcColor["base"]);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private unsafe void DrawStepBar() {
            var gauge = PluginInterface.ClientState.JobGauges.Get<DNCGauge>();
            var gaugePtr = &gauge;
            var openGauge = *(OpenDNCGauge*)gaugePtr;

            if (!openGauge.IsDancing()) {
                return;
            }

            byte chunkCount = 0;
            var chunkColors = new List<Dictionary<string, uint>>();
            var glowChunks = new List<bool>();
            var danceReady = true;

            for (var i = 0; i < 4; i++) {
                var step = (DNCStep)openGauge.stepOrder[i];

                if (step == DNCStep.None) {
                    break;
                }

                chunkCount++;

                if (openGauge.NumCompleteSteps == i) {
                    glowChunks.Add(true);
                    danceReady = false;
                }
                else {
                    glowChunks.Add(false);
                }

                switch (step) {
                    case DNCStep.Emboite:
                        chunkColors.Add(StepEmboiteColor);

                        break;

                    case DNCStep.Entrechat:
                        chunkColors.Add(StepEntrechatColor);

                        break;

                    case DNCStep.Jete:
                        chunkColors.Add(StepJeteColor);

                        break;

                    case DNCStep.Pirouette:
                        chunkColors.Add(StepPirouetteColor);

                        break;
                }
            }

            var xPos = CenterX + BaseXOffset - StepXOffset;
            var yPos = CenterY + BaseYOffset + StepYOffset;

            var builder = BarBuilder.Create(xPos, yPos, StepHeight, StepWidth)
                                    .SetChunks(chunkCount)
                                    .SetChunkPadding(StepPadding)
                                    .SetBackgroundColor(EmptyColor["background"])
                                    .AddInnerBar(chunkCount, chunkCount, chunkColors.ToArray());

            if (danceReady && DanceReadyGlow) {
                builder.SetGlowColor(DanceReadyColor["base"]);
            }
            else if (StepGlow) {
                builder.SetGlowChunks(glowChunks.ToArray())
                       .SetGlowColor(CurrentStepColor["base"]);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawBuffBar() {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var devilmentBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1825);
            var technicalFinishBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1822 or 2050);

            var xPos = CenterX + BaseXOffset - BuffXOffset;
            var yPos = CenterY + BaseYOffset + BuffYOffset;

            var builder = BarBuilder.Create(xPos, yPos, BuffHeight, BuffWidth).SetBackgroundColor(EmptyColor["background"]);

            if (technicalFinishBuff.Any() && TechnicalBarEnabled) {
                builder.AddInnerBar(Math.Abs(technicalFinishBuff.First().Duration), 20, TechnicalFinishColor);

                if (TechnicalTextEnabled) {
                    var position = DevilmentTextEnabled && DevilmentBarEnabled ? BarTextPosition.CenterLeft : BarTextPosition.CenterMiddle;

                    builder.SetTextMode(BarTextMode.EachChunk)
                           .SetText(position, BarTextType.Current, PluginConfiguration.DNCTechnicalFinishColor, Vector4.UnitW, null);
                }
            }

            if (devilmentBuff.Any() && DevilmentBarEnabled) {
                builder.AddInnerBar(Math.Abs(devilmentBuff.First().Duration), 20, DevilmentColor);

                if (DevilmentTextEnabled) {
                    var position = TechnicalTextEnabled && TechnicalBarEnabled ? BarTextPosition.CenterRight : BarTextPosition.CenterMiddle;

                    builder.SetTextMode(BarTextMode.EachChunk)
                           .SetText(position, BarTextType.Current, PluginConfiguration.DNCDevilmentColor, Vector4.UnitW, null);
                }
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawStandardBar() {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var standardFinishBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1821 or 2024 or 2105 or 2113);

            var xPos = CenterX + BaseXOffset - StandardXOffset;
            var yPos = CenterY + BaseYOffset + StandardYOffset;

            var builder = BarBuilder.Create(xPos, yPos, StandardHeight, StandardWidth);

            if (standardFinishBuff.Any()) {
                builder.AddInnerBar(standardFinishBuff.First().Duration, 60, StandardFinishColor);

                if (StandardTextEnabled) {
                    builder.SetTextMode(BarTextMode.EachChunk)
                           .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.SetBackgroundColor(EmptyColor["background"]).Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawProcBar() {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var flourishingCascadeBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1814);
            var flourishingFountainBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1815);
            var flourishingWindmillBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1816);
            var flourishingShowerBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1817);

            var xPos = CenterX + BaseXOffset - ProcXOffset;
            var yPos = CenterY + BaseYOffset + ProcYOffset;

            var cascadeBuilder = BarBuilder.Create(xPos, yPos, ProcHeight, ProcWidth).SetBackgroundColor(EmptyColor["background"]);
            var fountainBuilder = BarBuilder.Create(xPos + ProcWidth + ProcPadding, yPos, ProcHeight, ProcWidth).SetBackgroundColor(EmptyColor["background"]);
            var windmillBuilder = BarBuilder.Create(xPos + 2 * ProcWidth + 2 * ProcPadding, yPos, ProcHeight, ProcWidth).SetBackgroundColor(EmptyColor["background"]);
            var showerBuilder = BarBuilder.Create(xPos + 3 * ProcWidth + 3 * ProcPadding, yPos, ProcHeight, ProcWidth).SetBackgroundColor(EmptyColor["background"]);

            if (flourishingCascadeBuff.Any() && ProcEnabled) {
                var cascadeStart = ProcTimersEnabled ? Math.Abs(flourishingCascadeBuff.First().Duration) : 20;
                cascadeBuilder.AddInnerBar(cascadeStart, 20, FlourishingCascadeColor);
            }

            if (flourishingFountainBuff.Any() && ProcEnabled) {
                var fountainStart = ProcTimersEnabled ? Math.Abs(flourishingFountainBuff.First().Duration) : 20;
                fountainBuilder.AddInnerBar(fountainStart, 20, FlourishingFountainColor);
            }

            if (flourishingWindmillBuff.Any() && ProcEnabled) {
                var windmillStart = ProcTimersEnabled ? Math.Abs(flourishingWindmillBuff.First().Duration) : 20;
                windmillBuilder.AddInnerBar(windmillStart, 20, FlourishingWindmillColor);
            }

            if (flourishingShowerBuff.Any() && ProcEnabled) {
                var showerStart = ProcTimersEnabled ? Math.Abs(flourishingShowerBuff.First().Duration) : 20;
                showerBuilder.AddInnerBar(showerStart, 20, FlourishingShowerColor);
            }

            var drawList = ImGui.GetWindowDrawList();
            cascadeBuilder.Build().Draw(drawList, PluginConfiguration);
            fountainBuilder.Build().Draw(drawList, PluginConfiguration);
            windmillBuilder.Build().Draw(drawList, PluginConfiguration);
            showerBuilder.Build().Draw(drawList, PluginConfiguration);
        }
    }
}
