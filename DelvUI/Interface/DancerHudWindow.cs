using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Structs;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.GameStructs;
using DelvUI.Interface.Bars;
using ImGuiNET;

namespace DelvUI.Interface
{
    public class DancerHudWindow : HudWindow
    {
        public DancerHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        public override uint JobId => Jobs.DNC;
        private DancerHudConfig _config => (DancerHudConfig)ConfigurationManager.GetInstance().GetConfiguration(new DancerHudConfig());
        private Vector2 Origin => new(CenterX + _config.Position.X, CenterY + _config.Position.Y);
        private Dictionary<string, uint> EmptyColor => PluginConfiguration.MiscColorMap["empty"];
        private Dictionary<string, uint> PartialFillColor => PluginConfiguration.MiscColorMap["partial"];

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
            DNCGauge gauge = PluginInterface.ClientState.JobGauges.Get<DNCGauge>();

            var xPos = Origin.X + _config.EspritGaugePosition.X - _config.EspritGaugeSize.X / 2f;
            var yPos = Origin.Y + _config.EspritGaugePosition.Y - _config.EspritGaugeSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, _config.EspritGaugeSize.Y, _config.EspritGaugeSize.X)
                                           .SetChunks(2)
                                           .SetChunkPadding(_config.EspritGaugeChunkPadding)
                                           .SetBackgroundColor(EmptyColor["background"])
                                           .AddInnerBar(gauge.Esprit, 100, _config.EspritGaugeColor.Map, PartialFillColor);

            if (_config.EspritTextEnabled)
            {
                builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawFeathersBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            IEnumerable<StatusEffect> flourishingBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1820 or 2021);
            DNCGauge gauge = PluginInterface.ClientState.JobGauges.Get<DNCGauge>();

            var xPos = Origin.X + _config.FeatherGaugePosition.X - _config.FeatherGaugeSize.X / 2f;
            var yPos = Origin.Y + _config.FeatherGaugePosition.Y - _config.FeatherGaugeSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, _config.FeatherGaugeSize.Y, _config.FeatherGaugeSize.X)
                                           .SetChunks(4)
                                           .SetBackgroundColor(EmptyColor["background"])
                                           .SetChunkPadding(_config.FeatherGaugeChunkPadding)
                                           .AddInnerBar(gauge.NumFeathers, 4, _config.FeatherGaugeColor.Map);

            if (_config.FlourishingGlowEnabled && flourishingBuff.Any())
            {
                builder.SetGlowColor(_config.FlourishingProcColor.Base);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private unsafe void DrawStepBar()
        {
            DNCGauge gauge = PluginInterface.ClientState.JobGauges.Get<DNCGauge>();
            DNCGauge* gaugePtr = &gauge;
            OpenDNCGauge openGauge = *(OpenDNCGauge*)gaugePtr;

            if (!openGauge.IsDancing())
            {
                return;
            }

            byte chunkCount = 0;
            List<Dictionary<string, uint>> chunkColors = new List<Dictionary<string, uint>>();
            List<bool> glowChunks = new List<bool>();
            var danceReady = true;

            for (var i = 0; i < 4; i++)
            {
                DNCStep step = (DNCStep)openGauge.stepOrder[i];

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

            var xPos = Origin.X + _config.StepBarPosition.X - _config.StepBarSize.X / 2f;
            var yPos = Origin.Y + _config.StepBarPosition.Y - _config.StepBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, _config.StepBarSize.Y, _config.StepBarSize.X)
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
                builder.SetGlowChunks(glowChunks.ToArray()).SetGlowColor(_config.CurrentStepGlowColor.Base);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawBuffBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            IEnumerable<StatusEffect> devilmentBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1825);
            IEnumerable<StatusEffect> technicalFinishBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1822 or 2050);

            var xPos = Origin.X + _config.BuffBarPosition.X - _config.BuffBarSize.X / 2f;
            var yPos = Origin.Y + _config.BuffBarPosition.Y - _config.BuffBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, _config.BuffBarSize.Y, _config.BuffBarSize.X).SetBackgroundColor(EmptyColor["background"]);

            if (technicalFinishBuff.Any() && _config.TechnicalBarEnabled)
            {
                builder.AddInnerBar(Math.Abs(technicalFinishBuff.First().Duration), 20, _config.TechnicalFinishBarColor.Map);

                if (_config.TechnicalTextEnabled)
                {
                    BarTextPosition position = _config.DevilmentTextEnabled && _config.DevilmentBarEnabled ? BarTextPosition.CenterLeft : BarTextPosition.CenterMiddle;

                    builder.SetTextMode(BarTextMode.EachChunk).SetText(position, BarTextType.Current, _config.TechnicalFinishBarColor.Vector, Vector4.UnitW, null);
                }
            }

            if (devilmentBuff.Any() && _config.DevilmentBarEnabled)
            {
                builder.AddInnerBar(Math.Abs(devilmentBuff.First().Duration), 20, _config.DevilmentBarColor.Map);

                if (_config.DevilmentTextEnabled)
                {
                    BarTextPosition position = _config.TechnicalTextEnabled && _config.TechnicalBarEnabled ? BarTextPosition.CenterRight : BarTextPosition.CenterMiddle;

                    builder.SetTextMode(BarTextMode.EachChunk).SetText(position, BarTextType.Current, _config.DevilmentBarColor.Vector, Vector4.UnitW, null);
                }
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawStandardBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            IEnumerable<StatusEffect> standardFinishBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1821 or 2024 or 2105 or 2113);

            var xPos = Origin.X + _config.StandardBarPosition.X - _config.StandardBarSize.X / 2f;
            var yPos = Origin.Y + _config.StandardBarPosition.Y - _config.StandardBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, _config.StandardBarSize.Y, _config.StandardBarSize.X);

            if (standardFinishBuff.Any())
            {
                builder.AddInnerBar(standardFinishBuff.First().Duration, 60, _config.StandardFinishBarColor.Map);

                if (_config.StandardTextEnabled)
                {
                    builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.SetBackgroundColor(EmptyColor["background"]).Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawProcBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            IEnumerable<StatusEffect> flourishingCascadeBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1814);
            IEnumerable<StatusEffect> flourishingFountainBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1815);
            IEnumerable<StatusEffect> flourishingWindmillBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1816);
            IEnumerable<StatusEffect> flourishingShowerBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1817);

            var xPos = Origin.X + _config.ProcBarPosition.X - _config.ProcBarSize.X / 2f;
            var yPos = Origin.Y + _config.ProcBarPosition.Y - _config.ProcBarSize.Y / 2f;
            var procHeight = _config.ProcBarSize.Y;
            var procWidth = (int)Math.Floor((_config.ProcBarSize.X - 3 * _config.ProcBarChunkPadding) / 4);

            var chunkoffset = procWidth + _config.ProcBarChunkPadding;

            BarBuilder cascadeBuilder = BarBuilder.Create(xPos, yPos, procHeight, procWidth).SetBackgroundColor(EmptyColor["background"]);
            BarBuilder fountainBuilder = BarBuilder.Create(xPos + chunkoffset, yPos, procHeight, procWidth).SetBackgroundColor(EmptyColor["background"]);
            BarBuilder windmillBuilder = BarBuilder.Create(xPos + 2 * chunkoffset, yPos, procHeight, procWidth).SetBackgroundColor(EmptyColor["background"]);
            BarBuilder showerBuilder = BarBuilder.Create(xPos + 3 * chunkoffset, yPos, procHeight, procWidth).SetBackgroundColor(EmptyColor["background"]);

            var timersEnabled = !_config.StaticProcBarsEnabled;
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

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
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
        [DragFloat2("Base Position", min = -4000f, max = 4000f)]
        [Order(0)]
        public Vector2 Position = new(0, 0);

        // Esprit Guage
        [Checkbox("Show Esprit Guage")]
        [CollapseControl(5, 0)]
        public bool EspritGuageEnabled = true;

        [Checkbox("Show Esprit Guage Text")]
        [CollapseWith(0, 0)]
        public bool EspritTextEnabled = true;

        [DragFloat2("Esprit Gauge Size", max = 2000f)]
        [CollapseWith(5, 0)]
        public Vector2 EspritGaugeSize = new(254, 20);

        [DragFloat2("Esprit Gauge Position", min = -4000f, max = 4000f)]
        [CollapseWith(10, 0)]
        public Vector2 EspritGaugePosition = new(0, 405);

        [DragFloat("Esprit Gauge Chunk Padding", min = -4000f, max = 4000f)]
        [CollapseWith(15, 0)]
        public float EspritGaugeChunkPadding = 2;

        [ColorEdit4("Esprit Guage Color")]
        [CollapseWith(20, 0)]
        public PluginConfigColor EspritGaugeColor = new(new Vector4(72f / 255f, 20f / 255f, 99f / 255f, 100f / 100f));

        // Feathers Guage
        [Checkbox("Show Feather Guage")]
        [CollapseControl(10, 1)]
        public bool FeatherGuageEnabled = true;

        [Checkbox("Enable Flourishing Finish Glow")]
        [CollapseWith(0, 1)]
        public bool FlourishingGlowEnabled = true;

        [DragFloat2("Feather Guage Size")]
        [CollapseWith(5, 1)]
        public Vector2 FeatherGaugeSize = new(254, 10);

        [DragFloat2("Feather Gauge Position", min = -4000f, max = 4000f)]
        [CollapseWith(10, 1)]
        public Vector2 FeatherGaugePosition = new(0, 388);

        [DragFloat("Feather Gauge Chunk Padding", min = -4000f, max = 4000f)]
        [CollapseWith(15, 1)]
        public float FeatherGaugeChunkPadding = 2;

        [ColorEdit4("Feather Guage Color")]
        [CollapseWith(20, 1)]
        public PluginConfigColor FeatherGaugeColor = new(new Vector4(175f / 255f, 229f / 255f, 29f / 255f, 100f / 100f));

        [ColorEdit4("Flourishing Finish Glow Color")]
        [CollapseWith(25, 1)]
        public PluginConfigColor FlourishingProcColor = new(new Vector4(255f / 255f, 215f / 255f, 0f / 255f, 100f / 100f));

        // Buff Bars
        [Checkbox("Show Buff Bar")]
        [CollapseControl(15, 2)]
        public bool BuffBarEnabled = true;

        [Checkbox("Show Technical Finish Bar")]
        [CollapseWith(0, 2)]
        public bool TechnicalBarEnabled = true;

        [Checkbox("Show Technical Finish Bar Text")]
        [CollapseWith(5, 2)]
        public bool TechnicalTextEnabled = true;

        [Checkbox("Show Devilment Bar")]
        [CollapseWith(10, 2)]
        public bool DevilmentBarEnabled = true;

        [Checkbox("Show Devilment Bar Text")]
        [CollapseWith(15, 2)]
        public bool DevilmentTextEnabled = true;

        [DragFloat2("Buff Bars Size")]
        [CollapseWith(20, 2)]
        public Vector2 BuffBarSize = new(254, 20);

        [DragFloat2("Buff Bars Position")]
        [CollapseWith(25, 2)]
        public Vector2 BuffBarPosition = new(0, 427);

        [ColorEdit4("Technical Finish Bar Color")]
        [CollapseWith(30, 2)]
        public PluginConfigColor TechnicalFinishBarColor = new(new Vector4(255f / 255f, 9f / 255f, 102f / 255f, 100f / 100f));

        [ColorEdit4("Devilment Bar Color")]
        [CollapseWith(35, 2)]
        public PluginConfigColor DevilmentBarColor = new(new Vector4(52f / 255f, 78f / 255f, 29f / 255f, 100f / 100f));

        // Standard Finish Bar
        [Checkbox("Show Standard Finish Bar")]
        [CollapseControl(20, 3)]
        public bool StandardBarEnabled = true;

        [Checkbox("Show Standard Finish Bar Text")]
        [CollapseWith(0, 3)]
        public bool StandardTextEnabled = true;

        [DragFloat2("Standard Finish Bar Size")]
        [CollapseWith(5, 3)]
        public Vector2 StandardBarSize = new(254, 20);

        [DragFloat2("Standard Finish Bar Position")]
        [CollapseWith(10, 3)]
        public Vector2 StandardBarPosition = new(0, 449);

        [ColorEdit4("Standard Finish Bar Color")]
        [CollapseWith(15, 3)]
        public PluginConfigColor StandardFinishBarColor = new(new Vector4(0f / 255f, 193f / 255f, 95f / 255f, 100f / 100f));

        // Step Bars
        [Checkbox("Show Step Bars")]
        [CollapseControl(25, 4)]
        public bool StepBarEnabled = true;

        [Checkbox("Show Step Glow")]
        [CollapseWith(0, 4)]
        public bool StepGlowEnabled = true;

        [Checkbox("Show Dance Ready Glow")]
        [CollapseWith(5, 4)]
        public bool DanceReadyGlowEnabled = true;

        [DragFloat2("Step Bars Size")]
        [CollapseWith(10, 4)]
        public Vector2 StepBarSize = new(254, 10);

        [DragFloat2("Step Bars Position")]
        [CollapseWith(15, 4)]
        public Vector2 StepBarPosition = new(0, 376);

        [DragFloat("Step Bar Chunk Padding")]
        [CollapseWith(20, 4)]
        public float StepBarChunkPadding = 2;

        [ColorEdit4("Current Step Glow Color")]
        [CollapseWith(25, 4)]
        public PluginConfigColor CurrentStepGlowColor = new(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [ColorEdit4("Emboite Color")]
        [CollapseWith(30, 4)]
        public PluginConfigColor EmboiteColor = new(new Vector4(255f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Entrechat Color")]
        [CollapseWith(35, 4)]
        public PluginConfigColor EntrechatColor = new(new Vector4(0f / 255f, 0f / 255f, 255f / 255f, 100f / 100f));

        [ColorEdit4("Jete Color")]
        [CollapseWith(40, 4)]
        public PluginConfigColor JeteColor = new(new Vector4(0f / 255f, 255f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Pirouette Color")]
        [CollapseWith(45, 4)]
        public PluginConfigColor PirouetteColor = new(new Vector4(255f / 255f, 215f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Dance Ready Color")]
        [CollapseWith(50, 4)]
        public PluginConfigColor DanceReadyColor = new(new Vector4(255f / 255f, 215f / 255f, 0f / 255f, 100f / 100f));

        // Proc Bars
        [Checkbox("Show Proc Bars")]
        [CollapseControl(30, 5)]
        public bool ProcBarEnabled = true;

        [Checkbox("Use Static Proc Bars")]
        [CollapseWith(0, 5)]
        public bool StaticProcBarsEnabled = true;

        [DragFloat2("Proc Bars Size")]
        [CollapseWith(10, 5)]
        public Vector2 ProcBarSize = new(254, 10);

        [DragFloat2("Proc Bars Position")]
        [CollapseWith(15, 5)]
        public Vector2 ProcBarPosition = new(0, 376);

        [DragFloat("Proc Bar Chunk Padding")]
        [CollapseWith(20, 5)]
        public float ProcBarChunkPadding = 2;

        [ColorEdit4("Flourishing Cascade Color")]
        [CollapseWith(25, 5)]
        public PluginConfigColor FlourishingCascadeColor = new(new Vector4(0f / 255f, 255f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Flourishing Fountain Color")]
        [CollapseWith(30, 5)]
        public PluginConfigColor FlourishingFountainColor = new(new Vector4(255f / 255f, 215f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Flourishing Windmill Color")]
        [CollapseWith(35, 5)]
        public PluginConfigColor FlourishingWindmillColor = new(new Vector4(0f / 255f, 215f / 255f, 215f / 255f, 100f / 100f));

        [ColorEdit4("Flourishing Shower Color")]
        [CollapseWith(40, 5)]
        public PluginConfigColor FlourishingShowerColor = new(new Vector4(255f / 255f, 100f / 255f, 0f / 255f, 100f / 100f));
    }
}
