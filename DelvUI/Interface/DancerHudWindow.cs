using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.GameStructs;
using DelvUI.Interface.Bars;
using ImGuiNET;
using DelvUI.Helpers;

namespace DelvUI.Interface {
    public class DancerHudWindow : HudWindow {
        public override uint JobId => 38;

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
        private bool TechnicalBarEnabled  => PluginConfiguration.DNCTechnicalBarEnabled;
        private bool TechnicalTextEnabled  => PluginConfiguration.DNCTechnicalTextEnabled;
        private bool DevilmentBarEnabled  => PluginConfiguration.DNCDevilmentBarEnabled;
        private bool DevilmentTextEnabled  => PluginConfiguration.DNCDevilmentTextEnabled;
        private int BuffHeight => PluginConfiguration.DNCBuffHeight;
        private int BuffWidth => PluginConfiguration.DNCBuffWidth;
        private int BuffXOffset => PluginConfiguration.DNCBuffXOffset;
        private int BuffYOffset => PluginConfiguration.DNCBuffYOffset;
        private bool StandardBarEnabled  => PluginConfiguration.DNCStandardEnabled;
        private bool StandardTextEnabled  => PluginConfiguration.DNCStandardText;
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

        public DancerHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _) {
            if (EspritEnabled)
                DrawEspritBar();
            if (FeatherEnabled)
                DrawFeathersBar();
            if (BuffEnabled)
                DrawBuffBar();
            if (StepEnabled)
                DrawStepBar();
            if (StandardBarEnabled)
                DrawStandardBar();
        }

        protected override void DrawPrimaryResourceBar() 
        {
        }

        private void DrawEspritBar() {
            var gauge = PluginInterface.ClientState.JobGauges.Get<DNCGauge>();
            
            var xPos = CenterX - EspritXOffset;
            var yPos = CenterY + EspritYOffset;

            var builder = BarBuilder.Create(xPos, yPos, EspritHeight, EspritWidth)
                .SetChunks(2)
                .SetChunkPadding(EspritPadding)
                .AddInnerBar(gauge.Esprit, 100, EspritColor, EmptyColor);

            if (EspritText)
            {
                builder.SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }
        
        private void DrawFeathersBar() {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var flourishingBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1820 or 2021);
            var gauge = PluginInterface.ClientState.JobGauges.Get<DNCGauge>();
            
            var xPos = CenterX - FeatherXOffset;
            var yPos = CenterY + FeatherYOffset;
            
            var builder = BarBuilder.Create(xPos, yPos, FeatherHeight, FeatherWidth)
                .SetChunks(4)
                .SetChunkPadding(FeatherPadding)
                .AddInnerBar(gauge.NumFeathers, 4, FeatherColor);

            if (FlourishingGlowEnabled && flourishingBuff.Any())
            {
                builder.SetGlowColor(FlourishingProcColor["base"]);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private unsafe void DrawStepBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<DNCGauge>();
            DNCGauge* gaugePtr = &gauge;
            OpenDNCGauge openGauge = *(OpenDNCGauge*) gaugePtr;
            
            if (!openGauge.IsDancing())
                return;

            byte chunkCount = 0;
            List<Dictionary<string, uint>> chunkColors = new List<Dictionary<string, uint>>();
            List<bool> glowChunks = new List<bool>();
            var danceReady = true;

            for (var i = 0; i < 4; i++)
            {
                DNCStep step = (DNCStep) openGauge.stepOrder[i];
                if (step == DNCStep.None)
                    break;

                chunkCount++;
                if (openGauge.NumCompleteSteps == i)
                {
                    glowChunks.Add(true);
                    danceReady = false;
                }
                else
                {
                    glowChunks.Add(false);
                }

                switch (step)
                {
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

            var xPos = CenterX - StepXOffset;
            var yPos = CenterY + StepYOffset;

            var builder = BarBuilder.Create(xPos, yPos, StepHeight, StepWidth)
                .SetChunks(chunkCount)
                .SetChunkPadding(StepPadding)
                .AddInnerBar(chunkCount, chunkCount, chunkColors.ToArray());

            if (danceReady && DanceReadyGlow)
            {
                builder.SetGlowColor(DanceReadyColor["base"]);
            }
            else if (StepGlow)
            {
                builder.SetGlowChunks(glowChunks.ToArray())
                    .SetGlowColor(CurrentStepColor["base"]);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawBuffBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var devilmentBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1825);
            var technicalFinishBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1822 or 2050);

            var xPos = CenterX - BuffXOffset;
            var yPos = CenterY + BuffYOffset;

            var builder = BarBuilder.Create(xPos, yPos, BuffHeight, BuffWidth);

            if (technicalFinishBuff.Any() && TechnicalBarEnabled)
            {
                builder.AddInnerBar(Math.Abs(technicalFinishBuff.First().Duration), 20, TechnicalFinishColor);
                if (TechnicalTextEnabled)
                {
                    BarTextPosition position = DevilmentTextEnabled && DevilmentBarEnabled ? BarTextPosition.CenterLeft : BarTextPosition.CenterMiddle;
                    builder.SetTextMode(BarTextMode.EachChunk)
                        .SetText(position, BarTextType.Current, PluginConfiguration.DNCTechnicalFinishColor, Vector4.UnitW, null);
                }
            }
            if (devilmentBuff.Any() && DevilmentBarEnabled)
            {
                builder.AddInnerBar(Math.Abs(devilmentBuff.First().Duration), 20, DevilmentColor);
                if (DevilmentTextEnabled)
                {
                    BarTextPosition position = TechnicalTextEnabled && TechnicalBarEnabled ? BarTextPosition.CenterRight : BarTextPosition.CenterMiddle;
                    builder.SetTextMode(BarTextMode.EachChunk)
                        .SetText(position, BarTextType.Current, PluginConfiguration.DNCDevilmentColor, Vector4.UnitW, null);
                }
            }
            
            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawStandardBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var standardFinishBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1821 or 2024 or 2105 or 2113);
            
            var xPos = CenterX - StandardXOffset;
            var yPos = CenterY + StandardYOffset;

            var builder = BarBuilder.Create(xPos, yPos, StandardHeight, StandardWidth);

            if (standardFinishBuff.Any())
            {
                builder.AddInnerBar(standardFinishBuff.First().Duration, 60, StandardFinishColor);
                if (StandardTextEnabled)
                {
                    builder.SetTextMode(BarTextMode.EachChunk)
                        .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }
    }
}

