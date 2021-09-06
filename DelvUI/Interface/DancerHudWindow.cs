using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.GameStructs;
using DelvUI.Interface.Bars;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using DelvUI.Config;
using DelvUI.Config.Attributes;

namespace DelvUI.Interface
{
    public class DancerHudWindow : HudWindow
    {
        public DancerHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        public override uint JobId => Jobs.DNC;
        private readonly DancerHudConfig _config = (DancerHudConfig) ConfigurationManager.GetInstance().GetConfiguration(new DancerHudConfig());
        private Vector2 Origin => new Vector2(CenterX + _config.Position.X, CenterY + _config.Position.Y);
        private Dictionary<string, uint> EmptyColor => PluginConfiguration.MiscColorMap["empty"];


        protected override void Draw(bool _)
        {
            if (_config.EspritGuageEnabled)
            {
                DrawEspritBar();
            }
            if (_config.FeatherGuageEnabled)
            {
                DrawFeathersBar();
            }
            if (_config.BuffBarEnabled)
            {
                DrawBuffBar();
            }
            if (_config.ProcBarEnabled) // Draw procs before steps since they occupy the same space by default.
            {
                DrawProcBar();
            }
            if (_config.StepBarEnabled)
            {
                DrawStepBar();
            }
            if (_config.StandardBarEnabled)
            {
                DrawStandardBar();
            }
        }

        protected override void DrawPrimaryResourceBar() { }

        private void DrawEspritBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<DNCGauge>();

            var xPos = Origin.X - _config.EspritGaugeOffset.X;
            var yPos = Origin.Y + _config.EspritGaugeOffset.Y;

            var builder = BarBuilder.Create(xPos, yPos, _config.EspritGaugeSize.Y, _config.EspritGaugeSize.X)
                                    .SetChunks(2)
                                    .SetChunkPadding(_config.EspritGaugeChunkPadding)
                                    .SetBackgroundColor(EmptyColor["background"])
                                    .AddInnerBar(gauge.Esprit, 100, _config.EspritGaugeColor.Map, EmptyColor);

            if (_config.EspritTextEnabled)
            {
                builder.SetTextMode(BarTextMode.EachChunk)
                       .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawFeathersBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var flourishingBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1820 or 2021);
            var gauge = PluginInterface.ClientState.JobGauges.Get<DNCGauge>();

            var xPos = Origin.X - _config.FeatherGaugeOffset.X;
            var yPos = Origin.Y + _config.FeatherGaugeOffset.Y;

            var builder = BarBuilder.Create(xPos, yPos, _config.FeatherGaugeSize.Y, _config.FeatherGaugeSize.X)
                                    .SetChunks(4)
                                    .SetBackgroundColor(EmptyColor["background"])
                                    .SetChunkPadding(_config.FeatherGaugeChunkPadding)
                                    .AddInnerBar(gauge.NumFeathers, 4, _config.FeatherGaugeColor.Map);

            if (_config.FlourishingGlowEnabled && flourishingBuff.Any())
            {
                builder.SetGlowColor(_config.FlourishingProcColor.Base);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private unsafe void DrawStepBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<DNCGauge>();
            var gaugePtr = &gauge;
            var openGauge = *(OpenDNCGauge*)gaugePtr;

            if (!openGauge.IsDancing())
            {
                return;
            }

            byte chunkCount = 0;
            var chunkColors = new List<Dictionary<string, uint>>();
            var glowChunks = new List<bool>();
            var danceReady = true;

            for (var i = 0; i < 4; i++)
            {
                var step = (DNCStep)openGauge.stepOrder[i];

                if (step == DNCStep.None)
                {
                    break;
                }

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
                        chunkColors.Add(_config.EmboiteColor.Map);

                        break;

                    case DNCStep.Entrechat:
                        chunkColors.Add(_config.EntrechatColor.Map);

                        break;

                    case DNCStep.Jete:
                        chunkColors.Add(_config.JeteColor.Map);

                        break;

                    case DNCStep.Pirouette:
                        chunkColors.Add(_config.PirouetteColor.Map);

                        break;
                }
            }

            var xPos = Origin.X - _config.StepBarOffset.X;
            var yPos = Origin.Y + _config.StepBarOffset.Y;

            var builder = BarBuilder.Create(xPos, yPos, _config.StepBarSize.Y, _config.StepBarSize.X)
                                    .SetChunks(chunkCount)
                                    .SetChunkPadding(_config.StepBarChunkPadding)
                                    .SetBackgroundColor(EmptyColor["background"])
                                    .AddInnerBar(chunkCount, chunkCount, chunkColors.ToArray());

            if (danceReady && _config.DanceReadyGlowEnabled)
            {
                builder.SetGlowColor(_config.DanceReadyColor.Base);
            }
            else if (_config.StepGlowEnabled)
            {
                builder.SetGlowChunks(glowChunks.ToArray())
                       .SetGlowColor(_config.CurrentStepGlowColor.Base);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawBuffBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var devilmentBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1825);
            var technicalFinishBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1822 or 2050);

            var xPos = Origin.X - _config.BuffBarOffset.X;
            var yPos = Origin.Y + _config.BuffBarOffset.Y;

            var builder = BarBuilder.Create(xPos, yPos, _config.BuffBarSize.Y, _config.BuffBarSize.X).SetBackgroundColor(EmptyColor["background"]);

            if (technicalFinishBuff.Any() && _config.TechnicalBarEnabled)
            {
                builder.AddInnerBar(Math.Abs(technicalFinishBuff.First().Duration), 20, _config.TechnicalFinishBarColor.Map);

                if (_config.TechnicalTextEnabled)
                {
                    var position = _config.DevilmentTextEnabled && _config.DevilmentBarEnabled ? BarTextPosition.CenterLeft : BarTextPosition.CenterMiddle;

                    builder.SetTextMode(BarTextMode.EachChunk)
                           .SetText(position, BarTextType.Current, _config.TechnicalFinishBarColor.Vector, Vector4.UnitW, null);
                }
            }

            if (devilmentBuff.Any() && _config.DevilmentBarEnabled)
            {
                builder.AddInnerBar(Math.Abs(devilmentBuff.First().Duration), 20, _config.DevilmentBarColor.Map);

                if (_config.DevilmentTextEnabled)
                {
                    var position = _config.TechnicalTextEnabled && _config.TechnicalBarEnabled ? BarTextPosition.CenterRight : BarTextPosition.CenterMiddle;

                    builder.SetTextMode(BarTextMode.EachChunk)
                           .SetText(position, BarTextType.Current, _config.DevilmentBarColor.Vector, Vector4.UnitW, null);
                }
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawStandardBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var standardFinishBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1821 or 2024 or 2105 or 2113);

            var xPos = Origin.X - _config.StandardBarOffset.X;
            var yPos = Origin.Y + _config.StandardBarOffset.Y;

            var builder = BarBuilder.Create(xPos, yPos, _config.StandardBarSize.Y, _config.StandardBarSize.X);

            if (standardFinishBuff.Any())
            {
                builder.AddInnerBar(standardFinishBuff.First().Duration, 60, _config.StandardFinishBarColor.Map);

                if (_config.StandardTextEnabled)
                {
                    builder.SetTextMode(BarTextMode.EachChunk)
                           .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.SetBackgroundColor(EmptyColor["background"]).Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawProcBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var flourishingCascadeBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1814);
            var flourishingFountainBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1815);
            var flourishingWindmillBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1816);
            var flourishingShowerBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1817);

            var xPos = Origin.X - _config.ProcBarOffset.X;
            var yPos = Origin.Y + _config.ProcBarOffset.Y;
            var procHeight = _config.ProcBarSize.Y;
            var procWidth = (int) Math.Floor((_config.ProcBarSize.X - 3 * _config.ProcBarChunkPadding) / 4);

            var chunkoffset = procWidth + _config.ProcBarChunkPadding;

            var cascadeBuilder = BarBuilder.Create(xPos, yPos, procHeight, procWidth).SetBackgroundColor(EmptyColor["background"]);
            var fountainBuilder = BarBuilder.Create(xPos + chunkoffset, yPos, procHeight, procWidth).SetBackgroundColor(EmptyColor["background"]);
            var windmillBuilder = BarBuilder.Create(xPos + 2 * chunkoffset, yPos, procHeight, procWidth).SetBackgroundColor(EmptyColor["background"]);
            var showerBuilder = BarBuilder.Create(xPos + 3 * chunkoffset, yPos, procHeight, procWidth).SetBackgroundColor(EmptyColor["background"]);

            var timersEnabled = !(_config.StaticProcBarsEnabled);
            var procEnabled = _config.ProcBarEnabled;
            if (flourishingCascadeBuff.Any() && procEnabled)
            {
                var cascadeStart = timersEnabled ? Math.Abs(flourishingCascadeBuff.First().Duration) : 20;
                cascadeBuilder.AddInnerBar(cascadeStart, 20, _config.FlourishingCascadeColor.Map);
            }

            if (flourishingFountainBuff.Any() && procEnabled)
            {
                var fountainStart = timersEnabled ? Math.Abs(flourishingFountainBuff.First().Duration) : 20;
                fountainBuilder.AddInnerBar(fountainStart, 20, _config.FlourishingFountainColor.Map);
            }

            if (flourishingWindmillBuff.Any() && procEnabled)
            {
                var windmillStart = timersEnabled ? Math.Abs(flourishingWindmillBuff.First().Duration) : 20;
                windmillBuilder.AddInnerBar(windmillStart, 20, _config.FlourishingWindmillColor.Map);
            }

            if (flourishingShowerBuff.Any() && procEnabled)
            {
                var showerStart = timersEnabled ? Math.Abs(flourishingShowerBuff.First().Duration) : 20;
                showerBuilder.AddInnerBar(showerStart, 20, _config.FlourishingShowerColor.Map);
            }

            var drawList = ImGui.GetWindowDrawList();
            cascadeBuilder.Build().Draw(drawList, PluginConfiguration);
            fountainBuilder.Build().Draw(drawList, PluginConfiguration);
            windmillBuilder.Build().Draw(drawList, PluginConfiguration);
            showerBuilder.Build().Draw(drawList, PluginConfiguration);
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Ranged", 0)]
    [SubSection("Dancer", 1)]
    public class DancerHudConfig : PluginConfigObject
    {
        [DragFloat2("Base Offset", min = -4000f, max = 4000f)] 
        public Vector2 Position = new(0, 0);

        // Esprit Guage
        [Checkbox("Show Esprit Guage")] public bool EspritGuageEnabled = true;
        [Checkbox("Show Esprit Guage Text")] public bool EspritTextEnabled = true;
        [DragFloat2("Esprit Gauge Size", max = 2000f)]
        public Vector2 EspritGaugeSize = new Vector2(254, 20);
        [DragFloat2("Esprit Gauge Offset", min = -4000f, max = 4000f)]
        public Vector2 EspritGaugeOffset = new Vector2(127, 395);
        [DragFloat("Esprit Gauge Chunk Padding", min = -4000f, max = 4000f)]
        public float EspritGaugeChunkPadding = 2;

        // Feathers Guage
        [Checkbox("Show Feather Guage")] public bool FeatherGuageEnabled = true;
        [Checkbox("Enable Flourishing Finish Glow")] public bool FlourishingGlowEnabled = true;
        [DragFloat2("Feather Guage Size")]
        public Vector2 FeatherGaugeSize = new Vector2(254, 13);
        [DragFloat2("Feather Gauge Offset", min = -4000f, max = 4000f)]
        public Vector2 FeatherGaugeOffset = new Vector2(127, 380);
        [DragFloat("Feather Gauge Chunk Padding", min = -4000f, max = 4000f)]
        public float FeatherGaugeChunkPadding = 2;

        // Buff Bars
        [Checkbox("Show Buff Bar")] public bool BuffBarEnabled = true;
        [Checkbox("Show Technical Finish Bar")] public bool TechnicalBarEnabled = true;
        [Checkbox("Show Technical Finish Bar Text")] public bool TechnicalTextEnabled = true;
        [Checkbox("Show Devilment Bar")] public bool DevilmentBarEnabled = true;
        [Checkbox("Show Devilment Bar Text")] public bool DevilmentTextEnabled = true;
        [DragFloat2("Buff Bars Size")]
        public Vector2 BuffBarSize = new Vector2(254, 20);
        [DragFloat2("Buff Bars Offset")]
        public Vector2 BuffBarOffset = new Vector2(127, 417);

        // Standard Finish Bar
        [Checkbox("Show Standard Finish Bar")] public bool StandardBarEnabled = true;
        [Checkbox("Show Standard Finish Bar Text")] public bool StandardTextEnabled = true;
        [DragFloat2("Standard Finish Bar Size")]
        public Vector2 StandardBarSize = new Vector2(254, 20);
        [DragFloat2("Standard Finish Bar Offset")]
        public Vector2 StandardBarOffset = new Vector2(127, 439);

        // Step Bars
        [Checkbox("Show Step Bars")] public bool StepBarEnabled = true;
        [Checkbox("Show Step Glow")] public bool StepGlowEnabled = true;
        [Checkbox("Show Dance Ready Glow")] public bool DanceReadyGlowEnabled = true;
        [DragFloat2("Step Bars Size")] 
        public Vector2 StepBarSize = new Vector2(254, 13);
        [DragFloat2("Step Bars Offset")]
        public Vector2 StepBarOffset = new Vector2(127, 365);
        [DragFloat("Step Bar Chunk Padding")]
        public float StepBarChunkPadding = 2;

        // Proc Bars
        [Checkbox("Show Proc Bars")] public bool ProcBarEnabled = true;
        [Checkbox("Use Static Proc Bars")] public bool StaticProcBarsEnabled = true;
        [DragFloat2("Proc Bars Size")]
        public Vector2 ProcBarSize = new Vector2(254, 13);
        [DragFloat2("Proc Bars Offset")]
        public Vector2 ProcBarOffset = new Vector2(127, 365);
        [DragFloat("Proc Bar Chunk Padding")]
        public float ProcBarChunkPadding = 2;

        // ---Colors---
        // Esprit Guage
        [ColorEdit4("Esprit Guage Color")]
        public PluginConfigColor EspritGaugeColor = new PluginConfigColor(new Vector4(72f / 255f, 20f / 255f, 99f / 255f, 100f / 100f));

        // Feathers Guage
        [ColorEdit4("Feather Guage Color")]
        public PluginConfigColor FeatherGaugeColor = new PluginConfigColor(new Vector4(175f / 255f, 229f / 255f, 29f / 255f, 100f / 100f));
        [ColorEdit4("Flourishing Finish Glow Color")]
        public PluginConfigColor FlourishingProcColor = new PluginConfigColor(new Vector4(255f / 255f, 215f / 255f, 0f / 255f, 100f / 100f));

        // Buff Bars
        [ColorEdit4("Technical Finish Bar Color")]
        public PluginConfigColor TechnicalFinishBarColor = new PluginConfigColor(new Vector4(255f / 255f, 9f / 255f, 102f / 255f, 100f / 100f));
        [ColorEdit4("Devilment Bar Color")]
        public PluginConfigColor DevilmentBarColor = new PluginConfigColor(new Vector4(52f / 255f, 78f / 255f, 29f / 255f, 100f / 100f));

        //Standard Finish Bar
        [ColorEdit4("Standard Finish Bar Color")]
        public PluginConfigColor StandardFinishBarColor = new PluginConfigColor(new Vector4(0f / 255f, 193f / 255f, 95f / 255f, 100f / 100f));

        // Step Bars
        [ColorEdit4("Current Step Glow Color")]
        public PluginConfigColor CurrentStepGlowColor = new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));
        [ColorEdit4("Emboite Color")]
        public PluginConfigColor EmboiteColor = new PluginConfigColor(new Vector4(255f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));
        [ColorEdit4("Entrechat Color")]
        public PluginConfigColor EntrechatColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 255f / 255f, 100f / 100f));
        [ColorEdit4("Jete Color")]
        public PluginConfigColor JeteColor = new PluginConfigColor(new Vector4(0f / 255f, 255f / 255f, 0f / 255f, 100f / 100f));
        [ColorEdit4("Pirouette Color")]
        public PluginConfigColor PirouetteColor = new PluginConfigColor(new Vector4(255f / 255f, 215f / 255f, 0f / 255f, 100f / 100f));
        [ColorEdit4("Dance Ready Color")]
        public PluginConfigColor DanceReadyColor = new PluginConfigColor(new Vector4(255f / 255f, 215f / 255f, 0f / 255f, 100f / 100f));

        // Proc Bars
        [ColorEdit4("Flourishing Cascade Color")]
        public PluginConfigColor FlourishingCascadeColor = new PluginConfigColor(new Vector4(0f / 255f, 255f / 255f, 0f / 255f, 100f / 100f));
        [ColorEdit4("Flourishing Fountain Color")]
        public PluginConfigColor FlourishingFountainColor = new PluginConfigColor(new Vector4(255f / 255f, 215f / 255f, 0f / 255f, 100f / 100f));
        [ColorEdit4("Flourishing Windmill Color")]
        public PluginConfigColor FlourishingWindmillColor = new PluginConfigColor(new Vector4(0f / 255f, 215f / 255f, 215f / 255f, 100f / 100f));
        [ColorEdit4("Flourishing Shower Color")]
        public PluginConfigColor FlourishingShowerColor = new PluginConfigColor(new Vector4(255f / 255f, 100f / 255f, 0f / 255f, 100f / 100f));
    }
}
