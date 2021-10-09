using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Statuses;
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
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.Jobs
{
    public class DancerHud : JobHud
    {
        private new DancerConfig Config => (DancerConfig)_config;

        public DancerHud(DancerConfig config, string? displayName = null) : base(config, displayName)
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

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            if (Config.EspritGuageEnabled)
            {
                DrawEspritBar(origin);
            }

            if (Config.FeatherGuageEnabled)
            {
                DrawFeathersBar(origin, player);
            }

            if (Config.BuffBarEnabled)
            {
                DrawBuffBar(origin, player);
            }

            if (Config.ProcBarEnabled) // Draw procs before steps since they occupy the same space by default.
            {
                DrawProcBar(origin, player);
            }

            if (Config.StepBarEnabled)
            {
                DrawStepBar(origin);
            }

            if (Config.StandardBarEnabled)
            {
                DrawStandardBar(origin, player);
            }
        }

        private void DrawEspritBar(Vector2 origin)
        {
            DNCGauge gauge = Plugin.JobGauges.Get<DNCGauge>();

            if (gauge.Esprit == 0 && Config.OnlyShowEspritWhenActive) { return; }

            var xPos = origin.X + Config.Position.X + Config.EspritGaugePosition.X - Config.EspritGaugeSize.X / 2f;
            var yPos = origin.Y + Config.Position.Y + Config.EspritGaugePosition.Y - Config.EspritGaugeSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.EspritGaugeSize.Y, Config.EspritGaugeSize.X)
                .SetBackgroundColor(EmptyColor.Base);

            if (Config.ChunkEspritGauge)
            {
                builder.SetChunks(2)
                    .SetChunkPadding(Config.EspritGaugeChunkPadding)
                    .AddInnerBar(gauge.Esprit, 100, Config.EspritGaugeColor, PartialFillColor);
            }
            else
            {
                builder.AddInnerBar(gauge.Esprit, 100, Config.EspritGaugeColor);
            }

            if (Config.EspritTextEnabled && gauge.Esprit != 0)
            {
                builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawFeathersBar(Vector2 origin, PlayerCharacter player)
        {
            IEnumerable<Status> flourishingBuff = player.StatusList.Where(o => o.StatusId is 1820 or 2021);
            DNCGauge gauge = Plugin.JobGauges.Get<DNCGauge>();

            if (gauge.Feathers == 0 && !flourishingBuff.Any() && Config.OnlyShowFeatherWhenActive)
            {
                return;
            }

            var xPos = origin.X + Config.Position.X + Config.FeatherGaugePosition.X - Config.FeatherGaugeSize.X / 2f;
            var yPos = origin.Y + Config.Position.Y + Config.FeatherGaugePosition.Y - Config.FeatherGaugeSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.FeatherGaugeSize.Y, Config.FeatherGaugeSize.X)
                                           .SetChunks(4)
                                           .SetBackgroundColor(EmptyColor.Base)
                                           .SetChunkPadding(Config.FeatherGaugeChunkPadding)
                                           .AddInnerBar(gauge.Feathers, 4, Config.FeatherGaugeColor);

            if (Config.FlourishingGlowEnabled && flourishingBuff.Any())
            {
                builder.SetGlowColor(Config.FlourishingProcColor.Base);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private unsafe void DrawStepBar(Vector2 origin)
        {
            var gauge = Plugin.JobGauges.Get<DNCGauge>();

            if (!gauge.IsDancing) { return; }

            byte chunkCount = 0;
            List<PluginConfigColor> chunkColors = new List<PluginConfigColor>();
            List<bool> glowChunks = new List<bool>();
            var danceReady = true;

            for (var i = 0; i < 4; i++)
            {
                DNCStep step = (DNCStep)gauge.Steps[i];

                if (step == DNCStep.None)
                {
                    break;
                }

                chunkCount++;

                if (gauge.CompletedSteps == i)
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
                        //default:
                        //    chunkColors.Add(EmptyColor);
                        //    break;
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
                builder.SetGlowChunks(glowChunks.ToArray())
                        .SetGlowColor(Config.CurrentStepGlowColor.Base);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawBuffBar(Vector2 origin, PlayerCharacter player)
        {
            IEnumerable<Status> devilmentBuff = player.StatusList.Where(o => o.StatusId is 1825 && o.SourceID == player.ObjectId);
            IEnumerable<Status> technicalFinishBuff = player.StatusList.Where(o => o.StatusId is 1822 or 2050 && o.SourceID == player.ObjectId);

            if (!technicalFinishBuff.Any() && !devilmentBuff.Any() && Config.OnlyShowBuffBarWhenActive)
            {
                return;
            }

            var xPos = origin.X + Config.Position.X + Config.BuffBarPosition.X - Config.BuffBarSize.X / 2f;
            var yPos = origin.Y + Config.Position.Y + Config.BuffBarPosition.Y - Config.BuffBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.BuffBarSize.Y, Config.BuffBarSize.X).SetBackgroundColor(EmptyColor.Base);

            if (technicalFinishBuff.Any() && Config.TechnicalBarEnabled)
            {
                builder.AddInnerBar(Math.Abs(technicalFinishBuff.First().RemainingTime), 20, Config.TechnicalFinishBarColor);

                if (Config.TechnicalTextEnabled)
                {
                    BarTextPosition position = Config.DevilmentTextEnabled && Config.DevilmentBarEnabled ? BarTextPosition.CenterLeft : BarTextPosition.CenterMiddle;

                    builder.SetTextMode(BarTextMode.EachChunk).SetText(position, BarTextType.Current, Config.TechnicalFinishBarColor.Base, 0xFF000000, null);
                }
            }

            if (devilmentBuff.Any() && Config.DevilmentBarEnabled)
            {
                builder.AddInnerBar(Math.Abs(devilmentBuff.First().RemainingTime), 20, Config.DevilmentBarColor);

                if (Config.DevilmentTextEnabled)
                {
                    BarTextPosition position = Config.TechnicalTextEnabled && Config.TechnicalBarEnabled ? BarTextPosition.CenterRight : BarTextPosition.CenterMiddle;

                    builder.SetTextMode(BarTextMode.EachChunk).SetText(position, BarTextType.Current, Config.DevilmentBarColor.Base, 0xFF000000, null);
                }
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawStandardBar(Vector2 origin, PlayerCharacter player)
        {
            IEnumerable<Status> standardFinishBuff = player.StatusList.Where(o => o.StatusId is 1821 or 2024 or 2105 or 2113);

            if (!standardFinishBuff.Any() && Config.OnlyShowStandardBarWhenActive) { return; }

            var xPos = origin.X + Config.Position.X + Config.StandardBarPosition.X - Config.StandardBarSize.X / 2f;
            var yPos = origin.Y + Config.Position.Y + Config.StandardBarPosition.Y - Config.StandardBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.StandardBarSize.Y, Config.StandardBarSize.X);

            if (standardFinishBuff.Any())
            {
                builder.AddInnerBar(standardFinishBuff.First().RemainingTime, 60, Config.StandardFinishBarColor);

                if (Config.StandardTextEnabled)
                {
                    builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.SetBackgroundColor(EmptyColor.Base).Build().Draw(drawList);
        }

        private void DrawProcBar(Vector2 origin, PlayerCharacter player)
        {
            IEnumerable<Status> cascadeBuff = player.StatusList.Where(o => o.StatusId is 1814);
            IEnumerable<Status> fountainBuff = player.StatusList.Where(o => o.StatusId is 1815);
            IEnumerable<Status> windmillBuff = player.StatusList.Where(o => o.StatusId is 1816);
            IEnumerable<Status> showerBuff = player.StatusList.Where(o => o.StatusId is 1817);

            float cascadeTimer = cascadeBuff.Any() ? Math.Abs(cascadeBuff.First().RemainingTime) : 0f;
            float fountainTimer = fountainBuff.Any() ? Math.Abs(fountainBuff.First().RemainingTime) : 0f;
            float windmillTimer = windmillBuff.Any() ? Math.Abs(windmillBuff.First().RemainingTime) : 0f;
            float showerTimer = showerBuff.Any() ? Math.Abs(showerBuff.First().RemainingTime) : 0f;

            if (cascadeTimer == 0 && fountainTimer == 0 && windmillTimer == 0 && showerTimer == 0 && Config.OnlyShowProcsWhenActive)
            {
                return;
            }

            var procBarWidth = (Config.ProcBarSize.X - Config.ProcBarChunkPadding * 3) / 4f;
            var procBarSize = new Vector2(procBarWidth, Config.ProcBarSize.Y);

            var cursorPos = new Vector2(
                origin.X + Config.Position.X + Config.ProcBarPosition.X - Config.ProcBarSize.X / 2f,
                origin.Y + Config.Position.Y + Config.ProcBarPosition.Y - Config.ProcBarSize.Y / 2f
            );

            var drawList = ImGui.GetWindowDrawList();

            var order = Config.procsOrder;
            var procTimers = new float[] { cascadeTimer, fountainTimer, windmillTimer, showerTimer };
            var colors = new PluginConfigColor[] { Config.FlourishingCascadeColor, Config.FlourishingFountainColor, Config.FlourishingWindmillColor, Config.FlourishingShowerColor };

            for (int i = 0; i < 4; i++)
            {
                if (Config.StaticProcBarsEnabled)
                {
                    var builder = BarBuilder.Create(cursorPos, procBarSize).SetBackgroundColor(EmptyColor.Background)
                        .AddInnerBar(procTimers[order[i]] > 0 ? 1 : 0, 1, colors[order[i]]);
                    builder.Build().Draw(drawList);
                }
                else
                {
                    var builder = BarBuilder.Create(cursorPos, procBarSize).SetBackgroundColor(EmptyColor.Background)
                        .AddInnerBar(procTimers[order[i]], 20f, colors[order[i]]);
                    builder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                    builder.Build().Draw(drawList);
                }
                cursorPos.X += procBarWidth + Config.ProcBarChunkPadding;
            }
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Ranged", 0)]
    [SubSection("Dancer", 1)]
    public class DancerConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.DNC;
        public new static DancerConfig DefaultConfig() { return new DancerConfig(); }

        #region esprit
        [Checkbox("Esprit", separator = true)]
        [Order(30)]
        public bool EspritGuageEnabled = true;

        [Checkbox("Only Show When Active" + "##Esprit")]
        [Order(35, collapseWith = nameof(EspritGuageEnabled))]
        public bool OnlyShowEspritWhenActive = false;

        [Checkbox("Text" + "##Esprit")]
        [Order(40, collapseWith = nameof(EspritGuageEnabled))]
        public bool EspritTextEnabled = true;

        [Checkbox("Split Bar" + "##Esprit")]
        [Order(41, collapseWith = nameof(EspritGuageEnabled))]
        public bool ChunkEspritGauge = true;

        [DragFloat2("Position" + "##Esprit", min = -4000f, max = 4000f)]
        [Order(45, collapseWith = nameof(EspritGuageEnabled))]
        public Vector2 EspritGaugePosition = new(0, -54);

        [DragFloat2("Size" + "##Esprit", min = 1f, max = 2000f)]
        [Order(50, collapseWith = nameof(EspritGuageEnabled))]
        public Vector2 EspritGaugeSize = new(254, 20);

        [DragFloat("Spacing" + "##Esprit", min = -4000f, max = 4000f)]
        [Order(55, collapseWith = nameof(EspritGuageEnabled))]
        public float EspritGaugeChunkPadding = 2;

        [ColorEdit4("Color" + "##Esprit")]
        [Order(60, collapseWith = nameof(EspritGuageEnabled))]
        public PluginConfigColor EspritGaugeColor = new(new Vector4(72f / 255f, 20f / 255f, 99f / 255f, 100f / 100f));
        #endregion

        #region feathers
        [Checkbox("Feathers", separator = true)]
        [Order(65)]
        public bool FeatherGuageEnabled = true;

        [Checkbox("Only Show When Active" + "##Feather")]
        [Order(70, collapseWith = nameof(FeatherGuageEnabled))]
        public bool OnlyShowFeatherWhenActive = false;

        [Checkbox("Flourishing Finish Glow" + "##Feather")]
        [Order(75, collapseWith = nameof(FeatherGuageEnabled))]
        public bool FlourishingGlowEnabled = true;

        [DragFloat2("Position" + "##Feather", min = -4000f, max = 4000f)]
        [Order(80, collapseWith = nameof(FeatherGuageEnabled))]
        public Vector2 FeatherGaugePosition = new(0, -71);

        [DragFloat2("Size" + "##Feather", min = 1f, max = 2000f)]
        [Order(85, collapseWith = nameof(FeatherGuageEnabled))]
        public Vector2 FeatherGaugeSize = new(254, 10);

        [DragFloat("Spacing" + "##Feather", min = -4000f, max = 4000f)]
        [Order(90, collapseWith = nameof(FeatherGuageEnabled))]
        public float FeatherGaugeChunkPadding = 2;

        [ColorEdit4("Color" + "##Feather")]
        [Order(95, collapseWith = nameof(FeatherGuageEnabled))]
        public PluginConfigColor FeatherGaugeColor = new(new Vector4(175f / 255f, 229f / 255f, 29f / 255f, 100f / 100f));

        [ColorEdit4("Glow" + "##Feather")]
        [Order(100, collapseWith = nameof(FeatherGuageEnabled))]
        public PluginConfigColor FlourishingProcColor = new(new Vector4(255f / 255f, 215f / 255f, 0f / 255f, 100f / 100f));
        #endregion

        #region buff bars
        [Checkbox("Buffs", separator = true)]
        [Order(105)]
        public bool BuffBarEnabled = true;

        [DragFloat2("Position" + "##Buff", min = -4000f, max = 4000f)]
        [Order(110, collapseWith = nameof(BuffBarEnabled))]
        public Vector2 BuffBarPosition = new(0, -32);

        [DragFloat2("Size" + "##Buff", min = 1f, max = 2000f)]
        [Order(115, collapseWith = nameof(BuffBarEnabled))]
        public Vector2 BuffBarSize = new(254, 20);

        [Checkbox("Only Show When Active" + "##Buff")]
        [Order(120, collapseWith = nameof(BuffBarEnabled))]
        public bool OnlyShowBuffBarWhenActive = false;

        [Checkbox("Technical Finish" + "##Buff")]
        [Order(125, collapseWith = nameof(BuffBarEnabled))]
        public bool TechnicalBarEnabled = true;

        [Checkbox("Timer" + "##Buff")]
        [Order(130, collapseWith = nameof(TechnicalBarEnabled))]
        public bool TechnicalTextEnabled = true;

        [Checkbox("Devilment" + "##Buff")]
        [Order(135, collapseWith = nameof(BuffBarEnabled))]
        public bool DevilmentBarEnabled = true;

        [Checkbox("Timer" + "##Buff")]
        [Order(140, collapseWith = nameof(DevilmentBarEnabled))]
        public bool DevilmentTextEnabled = true;

        [ColorEdit4("Color" + "##Buff")]
        [Order(145, collapseWith = nameof(TechnicalBarEnabled))]
        public PluginConfigColor TechnicalFinishBarColor = new(new Vector4(255f / 255f, 9f / 255f, 102f / 255f, 100f / 100f));

        [ColorEdit4("Color" + "##Buff")]
        [Order(150, collapseWith = nameof(DevilmentBarEnabled))]
        public PluginConfigColor DevilmentBarColor = new(new Vector4(52f / 255f, 78f / 255f, 29f / 255f, 100f / 100f));
        #endregion

        #region standard finish
        [Checkbox("Standard Finish", separator = true)]
        [Order(155)]
        public bool StandardBarEnabled = true;

        [Checkbox("Only Show When Active" + "##Standard")]
        [Order(160, collapseWith = nameof(StandardBarEnabled))]
        public bool OnlyShowStandardBarWhenActive = false;

        [Checkbox("Timer" + "##Standard")]
        [Order(165, collapseWith = nameof(StandardBarEnabled))]
        public bool StandardTextEnabled = true;

        [DragFloat2("Position" + "##Standard", min = -4000f, max = 4000f)]
        [Order(170, collapseWith = nameof(StandardBarEnabled))]
        public Vector2 StandardBarPosition = new(0, -10);

        [DragFloat2("Size" + "##Standard", min = 1f, max = 2000f)]
        [Order(175, collapseWith = nameof(StandardBarEnabled))]
        public Vector2 StandardBarSize = new(254, 20);

        [ColorEdit4("Color" + "##Standard")]
        [Order(180, collapseWith = nameof(StandardBarEnabled))]
        public PluginConfigColor StandardFinishBarColor = new(new Vector4(0f / 255f, 193f / 255f, 95f / 255f, 100f / 100f));
        #endregion

        #region steps
        [Checkbox("Steps", separator = true)]
        [Order(185)]
        public bool StepBarEnabled = true;

        [Checkbox("Step Glow" + "##Step")]
        [Order(195, collapseWith = nameof(StepBarEnabled))]
        public bool StepGlowEnabled = true;

        [Checkbox("Dance Ready Glow" + "##Step")]
        [Order(200, collapseWith = nameof(StepBarEnabled))]
        public bool DanceReadyGlowEnabled = true;

        [DragFloat2("Position" + "##Step", min = -4000f, max = 4000f)]
        [Order(205, collapseWith = nameof(StepBarEnabled))]
        public Vector2 StepBarPosition = new(0, -93);

        [DragFloat2("Size" + "##Step", min = 1f, max = 2000f)]
        [Order(210, collapseWith = nameof(StepBarEnabled))]
        public Vector2 StepBarSize = new(254, 10);

        [DragFloat("Spacing" + "##Step", min = -4000f, max = 4000f)]
        [Order(215, collapseWith = nameof(StepBarEnabled))]
        public float StepBarChunkPadding = 2;

        [ColorEdit4("Step Glow" + "##Step")]
        [Order(220, collapseWith = nameof(StepBarEnabled))]
        public PluginConfigColor CurrentStepGlowColor = new(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [ColorEdit4("Dance Ready Glow" + "##Step")]
        [Order(225, collapseWith = nameof(StepBarEnabled))]
        public PluginConfigColor DanceReadyColor = new(new Vector4(255f / 255f, 215f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Emboite" + "##Step")]
        [Order(230, collapseWith = nameof(StepBarEnabled))]
        public PluginConfigColor EmboiteColor = new(new Vector4(255f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Entrechat" + "##Step")]
        [Order(235, collapseWith = nameof(StepBarEnabled))]
        public PluginConfigColor EntrechatColor = new(new Vector4(0f / 255f, 0f / 255f, 255f / 255f, 100f / 100f));

        [ColorEdit4("Jete" + "##Step")]
        [Order(240, collapseWith = nameof(StepBarEnabled))]
        public PluginConfigColor JeteColor = new(new Vector4(0f / 255f, 255f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Pirouette" + "##Step")]
        [Order(245, collapseWith = nameof(StepBarEnabled))]
        public PluginConfigColor PirouetteColor = new(new Vector4(255f / 255f, 215f / 255f, 0f / 255f, 100f / 100f));
        #endregion

        #region procs
        [Checkbox("Procs", separator = true)]
        [Order(250)]
        public bool ProcBarEnabled = true;

        [Checkbox("Only Show When Active" + "##Procs")]
        [Order(255, collapseWith = nameof(ProcBarEnabled))]
        public bool OnlyShowProcsWhenActive = false;

        [Checkbox("Static Bars" + "##Procs")]
        [Order(260, collapseWith = nameof(ProcBarEnabled))]
        public bool StaticProcBarsEnabled = true;

        [DragFloat2("Position" + "##Procs", min = -4000f, max = 4000f)]
        [Order(265, collapseWith = nameof(ProcBarEnabled))]
        public Vector2 ProcBarPosition = new(0, -83);

        [DragFloat2("Size" + "##Procs", min = 1f, max = 2000f)]
        [Order(270, collapseWith = nameof(ProcBarEnabled))]
        public Vector2 ProcBarSize = new(254, 10);

        [DragFloat("Spacing" + "##Procs", min = -4000f, max = 4000f)]
        [Order(275, collapseWith = nameof(ProcBarEnabled))]
        public float ProcBarChunkPadding = 2;

        [ColorEdit4("Flourishing Cascade" + "##Procs")]
        [Order(280, collapseWith = nameof(ProcBarEnabled))]
        public PluginConfigColor FlourishingCascadeColor = new(new Vector4(0f / 255f, 255f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Flourishing Fountain" + "##Procs")]
        [Order(285, collapseWith = nameof(ProcBarEnabled))]
        public PluginConfigColor FlourishingFountainColor = new(new Vector4(255f / 255f, 215f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Flourishing Windmill" + "##Procs")]
        [Order(290, collapseWith = nameof(ProcBarEnabled))]
        public PluginConfigColor FlourishingWindmillColor = new(new Vector4(0f / 255f, 215f / 255f, 215f / 255f, 100f / 100f));

        [ColorEdit4("Flourishing Shower" + "##Procs")]
        [Order(295, collapseWith = nameof(ProcBarEnabled))]
        public PluginConfigColor FlourishingShowerColor = new(new Vector4(255f / 255f, 100f / 255f, 0f / 255f, 100f / 100f));

        [DragDropHorizontal("Order", "Cascade", "Fountain", "Windmill", "Shower" + "##Procs")]
        [Order(300, collapseWith = nameof(ProcBarEnabled))]
        public int[] procsOrder = new int[] { 0, 1, 2, 3 };
        #endregion
    }
}
