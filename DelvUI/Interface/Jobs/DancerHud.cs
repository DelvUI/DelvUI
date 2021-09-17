using Dalamud.Game.ClientState.Structs;
using Dalamud.Game.ClientState.Structs.JobGauge;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.GameStructs;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.Jobs
{
    public class DancerHud : JobHud
    {
        private new DancerConfig Config => (DancerConfig)_config;

        public DancerHud(string id, DancerConfig config, string displayName = null) : base(id, config, displayName)
        {

        }

        private PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;
        private PluginConfigColor PartialFillColor => GlobalColors.Instance.PartialFillColor;

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.EspritGuageEnabled)
            {
                positions.Add(Config.Position + Config.EspritGaugePosition);
                sizes.Add(Config.EspritGaugeSize);
            }

            if (Config.FeatherGuageEnabled)
            {
                positions.Add(Config.Position + Config.FeatherGaugePosition);
                sizes.Add(Config.FeatherGaugeSize);
            }

            if (Config.BuffBarEnabled)
            {
                positions.Add(Config.Position + Config.BuffBarPosition);
                sizes.Add(Config.BuffBarSize);
            }

            if (Config.StandardBarEnabled)
            {
                positions.Add(Config.Position + Config.StandardBarPosition);
                sizes.Add(Config.StandardBarSize);
            }

            if (Config.ProcBarEnabled)
            {
                positions.Add(Config.Position + Config.ProcBarPosition);
                sizes.Add(Config.ProcBarSize);
            }

            return (positions, sizes);
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (Config.EspritGuageEnabled)
            {
                DrawEspritBar(origin);
            }

            if (Config.FeatherGuageEnabled)
            {
                DrawFeathersBar(origin);
            }

            if (Config.BuffBarEnabled)
            {
                DrawBuffBar(origin);
            }

            if (Config.ProcBarEnabled) // Draw procs before steps since they occupy the same space by default.
            {
                DrawProcBar(origin);
            }

            if (Config.StepBarEnabled)
            {
                DrawStepBar(origin);
            }

            if (Config.StandardBarEnabled)
            {
                DrawStandardBar(origin);
            }
        }

        private void DrawEspritBar(Vector2 origin)
        {
            DNCGauge gauge = Plugin.JobGauges.Get<DNCGauge>();

            var xPos = origin.X + Config.Position.X + Config.EspritGaugePosition.X - Config.EspritGaugeSize.X / 2f;
            var yPos = origin.Y + Config.Position.Y + Config.EspritGaugePosition.Y - Config.EspritGaugeSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.EspritGaugeSize.Y, Config.EspritGaugeSize.X)
                                           .SetChunks(2)
                                           .SetChunkPadding(Config.EspritGaugeChunkPadding)
                                           .SetBackgroundColor(EmptyColor.Base)
                                           .AddInnerBar(gauge.Esprit, 100, Config.EspritGaugeColor, PartialFillColor);

            if (Config.EspritTextEnabled)
            {
                builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawFeathersBar(Vector2 origin)
        {
            IEnumerable<StatusEffect> flourishingBuff = Plugin.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1820 or 2021);
            DNCGauge gauge = Plugin.JobGauges.Get<DNCGauge>();

            var xPos = origin.X + Config.Position.X + Config.FeatherGaugePosition.X - Config.FeatherGaugeSize.X / 2f;
            var yPos = origin.Y + Config.Position.Y + Config.FeatherGaugePosition.Y - Config.FeatherGaugeSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.FeatherGaugeSize.Y, Config.FeatherGaugeSize.X)
                                           .SetChunks(4)
                                           .SetBackgroundColor(EmptyColor.Base)
                                           .SetChunkPadding(Config.FeatherGaugeChunkPadding)
                                           .AddInnerBar(gauge.NumFeathers, 4, Config.FeatherGaugeColor);

            if (Config.FlourishingGlowEnabled && flourishingBuff.Any())
            {
                builder.SetGlowColor(Config.FlourishingProcColor.Base);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private unsafe void DrawStepBar(Vector2 origin)
        {
            DNCGauge gauge = Plugin.JobGauges.Get<DNCGauge>();
            DNCGauge* gaugePtr = &gauge;
            OpenDNCGauge openGauge = *(OpenDNCGauge*)gaugePtr;

            if (!openGauge.IsDancing())
            {
                return;
            }

            byte chunkCount = 0;
            List<PluginConfigColor> chunkColors = new List<PluginConfigColor>();
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
                        chunkColors.Add(Config.EmboiteColor);

                        break;

                    case DNCStep.Entrechat:
                        chunkColors.Add(Config.EntrechatColor);

                        break;

                    case DNCStep.Jete:
                        chunkColors.Add(Config.JeteColor);

                        break;

                    case DNCStep.Pirouette:
                        chunkColors.Add(Config.PirouetteColor);

                        break;
                }
            }

            var xPos = origin.X + Config.Position.X + Config.StepBarPosition.X - Config.StepBarSize.X / 2f;
            var yPos = origin.Y + Config.Position.Y + Config.StepBarPosition.Y - Config.StepBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.StepBarSize.Y, Config.StepBarSize.X)
                                           .SetChunks(chunkCount)
                                           .SetChunkPadding(Config.StepBarChunkPadding)
                                           .SetBackgroundColor(EmptyColor.Base)
                                           .AddInnerBar(chunkCount, chunkCount, chunkColors.ToArray());

            if (danceReady && Config.DanceReadyGlowEnabled)
            {
                builder.SetGlowColor(Config.DanceReadyColor.Base);
            }
            else if (Config.StepGlowEnabled)
            {
                builder.SetGlowChunks(glowChunks.ToArray()).SetGlowColor(Config.CurrentStepGlowColor.Base);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawBuffBar(Vector2 origin)
        {
            IEnumerable<StatusEffect> devilmentBuff = Plugin.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1825);
            IEnumerable<StatusEffect> technicalFinishBuff = Plugin.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1822 or 2050);

            var xPos = origin.X + Config.Position.X + Config.BuffBarPosition.X - Config.BuffBarSize.X / 2f;
            var yPos = origin.Y + Config.Position.Y + Config.BuffBarPosition.Y - Config.BuffBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.BuffBarSize.Y, Config.BuffBarSize.X).SetBackgroundColor(EmptyColor.Base);

            if (technicalFinishBuff.Any() && Config.TechnicalBarEnabled)
            {
                builder.AddInnerBar(Math.Abs(technicalFinishBuff.First().Duration), 20, Config.TechnicalFinishBarColor);

                if (Config.TechnicalTextEnabled)
                {
                    BarTextPosition position = Config.DevilmentTextEnabled && Config.DevilmentBarEnabled ? BarTextPosition.CenterLeft : BarTextPosition.CenterMiddle;

                    builder.SetTextMode(BarTextMode.EachChunk).SetText(position, BarTextType.Current, Config.TechnicalFinishBarColor.Vector, Vector4.UnitW, null);
                }
            }

            if (devilmentBuff.Any() && Config.DevilmentBarEnabled)
            {
                builder.AddInnerBar(Math.Abs(devilmentBuff.First().Duration), 20, Config.DevilmentBarColor);

                if (Config.DevilmentTextEnabled)
                {
                    BarTextPosition position = Config.TechnicalTextEnabled && Config.TechnicalBarEnabled ? BarTextPosition.CenterRight : BarTextPosition.CenterMiddle;

                    builder.SetTextMode(BarTextMode.EachChunk).SetText(position, BarTextType.Current, Config.DevilmentBarColor.Vector, Vector4.UnitW, null);
                }
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawStandardBar(Vector2 origin)
        {
            IEnumerable<StatusEffect> standardFinishBuff = Plugin.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1821 or 2024 or 2105 or 2113);

            var xPos = origin.X + Config.Position.X + Config.StandardBarPosition.X - Config.StandardBarSize.X / 2f;
            var yPos = origin.Y + Config.Position.Y + Config.StandardBarPosition.Y - Config.StandardBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.StandardBarSize.Y, Config.StandardBarSize.X);

            if (standardFinishBuff.Any())
            {
                builder.AddInnerBar(standardFinishBuff.First().Duration, 60, Config.StandardFinishBarColor);

                if (Config.StandardTextEnabled)
                {
                    builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.SetBackgroundColor(EmptyColor.Base).Build().Draw(drawList);
        }

        private void DrawProcBar(Vector2 origin)
        {
            IEnumerable<StatusEffect> flourishingCascadeBuff = Plugin.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1814);
            IEnumerable<StatusEffect> flourishingFountainBuff = Plugin.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1815);
            IEnumerable<StatusEffect> flourishingWindmillBuff = Plugin.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1816);
            IEnumerable<StatusEffect> flourishingShowerBuff = Plugin.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1817);

            var xPos = origin.X + Config.Position.X + Config.ProcBarPosition.X - Config.ProcBarSize.X / 2f;
            var yPos = origin.Y + Config.Position.Y + Config.ProcBarPosition.Y - Config.ProcBarSize.Y / 2f;
            var procHeight = Config.ProcBarSize.Y;
            var procWidth = (int)Math.Floor((Config.ProcBarSize.X - 3 * Config.ProcBarChunkPadding) / 4);

            var chunkoffset = procWidth + Config.ProcBarChunkPadding;

            BarBuilder cascadeBuilder = BarBuilder.Create(xPos, yPos, procHeight, procWidth).SetBackgroundColor(EmptyColor.Base);
            BarBuilder fountainBuilder = BarBuilder.Create(xPos + chunkoffset, yPos, procHeight, procWidth).SetBackgroundColor(EmptyColor.Base);
            BarBuilder windmillBuilder = BarBuilder.Create(xPos + 2 * chunkoffset, yPos, procHeight, procWidth).SetBackgroundColor(EmptyColor.Base);
            BarBuilder showerBuilder = BarBuilder.Create(xPos + 3 * chunkoffset, yPos, procHeight, procWidth).SetBackgroundColor(EmptyColor.Base);

            var timersEnabled = !Config.StaticProcBarsEnabled;
            var procEnabled = Config.ProcBarEnabled;

            if (flourishingCascadeBuff.Any() && procEnabled)
            {
                var cascadeStart = timersEnabled ? Math.Abs(flourishingCascadeBuff.First().Duration) : 20;
                cascadeBuilder.AddInnerBar(cascadeStart, 20, Config.FlourishingCascadeColor);
            }

            if (flourishingFountainBuff.Any() && procEnabled)
            {
                var fountainStart = timersEnabled ? Math.Abs(flourishingFountainBuff.First().Duration) : 20;
                fountainBuilder.AddInnerBar(fountainStart, 20, Config.FlourishingFountainColor);
            }

            if (flourishingWindmillBuff.Any() && procEnabled)
            {
                var windmillStart = timersEnabled ? Math.Abs(flourishingWindmillBuff.First().Duration) : 20;
                windmillBuilder.AddInnerBar(windmillStart, 20, Config.FlourishingWindmillColor);
            }

            if (flourishingShowerBuff.Any() && procEnabled)
            {
                var showerStart = timersEnabled ? Math.Abs(flourishingShowerBuff.First().Duration) : 20;
                showerBuilder.AddInnerBar(showerStart, 20, Config.FlourishingShowerColor);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            cascadeBuilder.Build().Draw(drawList);
            fountainBuilder.Build().Draw(drawList);
            windmillBuilder.Build().Draw(drawList);
            showerBuilder.Build().Draw(drawList);
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Ranged", 0)]
    [SubSection("Dancer", 1)]
    public class DancerConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.DNC;
        public new static DancerConfig DefaultConfig() { return new DancerConfig(); }

        #region espirit
        [Checkbox("Show Esprit Guage")]
        [CollapseControl(30, 0)]
        public bool EspritGuageEnabled = true;

        [Checkbox("Show Esprit Guage Text")]
        [CollapseWith(0, 0)]
        public bool EspritTextEnabled = true;

        [DragFloat2("Esprit Gauge Size", min = 1f, max = 2000f)]
        [CollapseWith(5, 0)]
        public Vector2 EspritGaugeSize = new(254, 20);

        [DragFloat2("Esprit Gauge Position", min = -4000f, max = 4000f)]
        [CollapseWith(10, 0)]
        public Vector2 EspritGaugePosition = new(0, -54);

        [DragFloat("Esprit Gauge Chunk Padding", min = -4000f, max = 4000f)]
        [CollapseWith(15, 0)]
        public float EspritGaugeChunkPadding = 2;

        [ColorEdit4("Esprit Guage Color")]
        [CollapseWith(20, 0)]
        public PluginConfigColor EspritGaugeColor = new(new Vector4(72f / 255f, 20f / 255f, 99f / 255f, 100f / 100f));
        #endregion

        #region feathers
        [Checkbox("Show Feather Guage")]
        [CollapseControl(35, 1)]
        public bool FeatherGuageEnabled = true;

        [Checkbox("Enable Flourishing Finish Glow")]
        [CollapseWith(0, 1)]
        public bool FlourishingGlowEnabled = true;

        [DragFloat2("Feather Guage Size", min = 1f, max = 2000f)]
        [CollapseWith(5, 1)]
        public Vector2 FeatherGaugeSize = new(254, 10);

        [DragFloat2("Feather Gauge Position", min = -4000f, max = 4000f)]
        [CollapseWith(10, 1)]
        public Vector2 FeatherGaugePosition = new(0, -71);

        [DragFloat("Feather Gauge Chunk Padding", min = -4000f, max = 4000f)]
        [CollapseWith(15, 1)]
        public float FeatherGaugeChunkPadding = 2;

        [ColorEdit4("Feather Guage Color")]
        [CollapseWith(20, 1)]
        public PluginConfigColor FeatherGaugeColor = new(new Vector4(175f / 255f, 229f / 255f, 29f / 255f, 100f / 100f));

        [ColorEdit4("Flourishing Finish Glow Color")]
        [CollapseWith(25, 1)]
        public PluginConfigColor FlourishingProcColor = new(new Vector4(255f / 255f, 215f / 255f, 0f / 255f, 100f / 100f));
        #endregion

        #region buff bars
        [Checkbox("Show Buff Bar")]
        [CollapseControl(40, 2)]
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

        [DragFloat2("Buff Bars Size", min = 1f, max = 2000f)]
        [CollapseWith(20, 2)]
        public Vector2 BuffBarSize = new(254, 20);

        [DragFloat2("Buff Bars Position", min = -4000f, max = 4000f)]
        [CollapseWith(25, 2)]
        public Vector2 BuffBarPosition = new(0, -32);

        [ColorEdit4("Technical Finish Bar Color")]
        [CollapseWith(30, 2)]
        public PluginConfigColor TechnicalFinishBarColor = new(new Vector4(255f / 255f, 9f / 255f, 102f / 255f, 100f / 100f));

        [ColorEdit4("Devilment Bar Color")]
        [CollapseWith(35, 2)]
        public PluginConfigColor DevilmentBarColor = new(new Vector4(52f / 255f, 78f / 255f, 29f / 255f, 100f / 100f));
        #endregion

        #region standard finish
        [Checkbox("Show Standard Finish Bar")]
        [CollapseControl(45, 3)]
        public bool StandardBarEnabled = true;

        [Checkbox("Show Standard Finish Bar Text")]
        [CollapseWith(0, 3)]
        public bool StandardTextEnabled = true;

        [DragFloat2("Standard Finish Bar Size", min = 1f, max = 2000f)]
        [CollapseWith(5, 3)]
        public Vector2 StandardBarSize = new(254, 20);

        [DragFloat2("Standard Finish Bar Position", min = -4000f, max = 4000f)]
        [CollapseWith(10, 3)]
        public Vector2 StandardBarPosition = new(0, -10);

        [ColorEdit4("Standard Finish Bar Color")]
        [CollapseWith(15, 3)]
        public PluginConfigColor StandardFinishBarColor = new(new Vector4(0f / 255f, 193f / 255f, 95f / 255f, 100f / 100f));
        #endregion

        #region steps
        [Checkbox("Show Step Bars")]
        [CollapseControl(50, 4)]
        public bool StepBarEnabled = true;

        [Checkbox("Show Step Glow")]
        [CollapseWith(0, 4)]
        public bool StepGlowEnabled = true;

        [Checkbox("Show Dance Ready Glow")]
        [CollapseWith(5, 4)]
        public bool DanceReadyGlowEnabled = true;

        [DragFloat2("Step Bars Size", min = 1f, max = 2000f)]
        [CollapseWith(10, 4)]
        public Vector2 StepBarSize = new(254, 10);

        [DragFloat2("Step Bars Position", min = -4000f, max = 4000f)]
        [CollapseWith(15, 4)]
        public Vector2 StepBarPosition = new(0, -93);

        [DragFloat("Step Bar Chunk Padding", min = -4000f, max = 4000f)]
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
        #endregion

        #region procs
        [Checkbox("Show Proc Bars")]
        [CollapseControl(55, 5)]
        public bool ProcBarEnabled = true;

        [Checkbox("Use Static Proc Bars")]
        [CollapseWith(0, 5)]
        public bool StaticProcBarsEnabled = true;

        [DragFloat2("Proc Bars Size", min = 1f, max = 2000f)]
        [CollapseWith(10, 5)]
        public Vector2 ProcBarSize = new(254, 10);

        [DragFloat2("Proc Bars Position", min = -4000f, max = 4000f)]
        [CollapseWith(15, 5)]
        public Vector2 ProcBarPosition = new(0, -83);

        [DragFloat("Proc Bar Chunk Padding", min = -4000f, max = 4000f)]
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
        #endregion
    }
}
