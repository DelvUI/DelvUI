using Dalamud.Game.ClientState.Structs.JobGauge;
using DelvUI.Config;
using DelvUI.Config.Attributes;
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
    public class MonkHud : JobHud
    {
        private new MonkConfig Config => (MonkConfig)_config;

        public MonkHud(string id, MonkConfig config, string displayName = null) : base(id, config, displayName)
        {

        }

        private PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;


        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.ShowDemolishBar)
            {
                positions.Add(Config.Position + Config.DemolishBarPosition);
                sizes.Add(Config.DemolishBarSize);
            }

            if (Config.ShowChakraBar)
            {
                positions.Add(Config.Position + Config.ChakraBarPosition);
                sizes.Add(Config.ChakraBarSize);
            }

            if (Config.ShowLeadenFistBar)
            {
                positions.Add(Config.Position + Config.LeadenFistBarPosition);
                sizes.Add(Config.LeadenFistBarSize);
            }

            if (Config.ShowTwinSnakesBar)
            {
                positions.Add(Config.Position + Config.TwinSnakesBarPosition);
                sizes.Add(Config.TwinSnakesBarSize);
            }

            if (Config.ShowRiddleofEarthBar)
            {
                positions.Add(Config.Position + Config.RiddleofEarthBarPosition);
                sizes.Add(Config.RiddleofEarthBarSize);
            }

            if (Config.ShowPerfectBalanceBar)
            {
                positions.Add(Config.Position + Config.PerfectBalanceBarPosition);
                sizes.Add(Config.PerfectBalanceBarSize);
            }

            if (Config.ShowTrueNorthBar)
            {
                positions.Add(Config.Position + Config.TrueNorthBarPosition);
                sizes.Add(Config.TrueNorthBarSize);
            }

            return (positions, sizes);
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (Config.ShowFormsBar)
            {
                DrawFormsBar(origin);
            }

            if (Config.ShowRiddleofEarthBar)
            {
                DrawRiddleOfEarthBar(origin);
            }

            if (Config.ShowPerfectBalanceBar)
            {
                DrawPerfectBalanceBar(origin);
            }

            if (Config.ShowTrueNorthBar)
            {
                DrawTrueNorthBar(origin);
            }

            if (Config.ShowChakraBar)
            {
                DrawChakraGauge(origin);
            }

            if (Config.ShowLeadenFistBar)
            {
                DrawLeadenFistBar(origin);
            }

            if (Config.ShowTwinSnakesBar)
            {
                DrawTwinSnakesBar(origin);
            }

            if (Config.ShowDemolishBar)
            {
                DrawDemolishBar(origin);
            }
        }

        private void DrawFormsBar(Vector2 origin)
        {
            var target = Plugin.ClientState.LocalPlayer;
            var opoOpoForm = target.StatusEffects.FirstOrDefault(o => o.EffectId == 107);
            var raptorForm = target.StatusEffects.FirstOrDefault(o => o.EffectId == 108);
            var coeurlForm = target.StatusEffects.FirstOrDefault(o => o.EffectId == 109);
            var formlessFist = target.StatusEffects.FirstOrDefault(o => o.EffectId == 2513);

            var opoOpoFormDuration = opoOpoForm.Duration;
            var raptorFormDuration = raptorForm.Duration;
            var coeurlFormDuration = coeurlForm.Duration;
            var formlessFistDuration = formlessFist.Duration;

            var position = origin + Config.Position + Config.FormsBarPosition - Config.FormsBarSize / 2f;

            var builder = BarBuilder.Create(position, Config.FormsBarSize);
            var maximum = 15f;

            if (opoOpoFormDuration > 0)
            {
                var bar = builder.AddInnerBar(Math.Abs(opoOpoFormDuration), maximum, Config.FormsBarFillColor)
                                 .SetTextMode(BarTextMode.EachChunk)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, "Opo-Opo Form")
                                 .SetBackgroundColor(EmptyColor.Background)
                                 .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList);
            }

            if (raptorFormDuration > 0)
            {
                var bar = builder.AddInnerBar(Math.Abs(raptorFormDuration), maximum, Config.FormsBarFillColor)
                                 .SetTextMode(BarTextMode.EachChunk)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, "Raptor Form")
                                 .SetBackgroundColor(EmptyColor.Background)
                                 .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList);
            }

            if (coeurlFormDuration > 0)
            {
                var bar = builder.AddInnerBar(Math.Abs(coeurlFormDuration), maximum, Config.FormsBarFillColor)
                                 .SetTextMode(BarTextMode.EachChunk)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, "Coeurl Form")
                                 .SetBackgroundColor(EmptyColor.Background)
                                 .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList);
            }

            if (formlessFist.Duration > 0)
            {
                var bar = builder.AddInnerBar(Math.Abs(formlessFist.Duration), maximum, Config.FormsBarFillColor)
                                 .SetTextMode(BarTextMode.EachChunk)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, "Formless Fist")
                                 .SetBackgroundColor(EmptyColor.Background)
                                 .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList);
            }
            else
            {
                var bar = builder.AddInnerBar(0, maximum, Config.FormsBarFillColor)
                                 .SetBackgroundColor(EmptyColor.Background)
                                 .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList);
            }
        }

        private void DrawTrueNorthBar(Vector2 origin)
        {
            var target = Plugin.ClientState.LocalPlayer;
            var trueNorth = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1250);
            var trueNorthDuration = trueNorth.Duration;

            var position = origin + Config.Position + Config.TrueNorthBarPosition - Config.TrueNorthBarSize / 2f;
            var builder = BarBuilder.Create(position, Config.TrueNorthBarSize);
            var maximum = 10f;

            if (trueNorthDuration > 0)
            {
                var bar = builder.AddInnerBar(Math.Abs(trueNorthDuration), maximum, Config.TrueNorthBarFillColor)
                                 .SetTextMode(BarTextMode.EachChunk)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                                 .SetBackgroundColor(EmptyColor.Background)
                                 .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList);
            }
            else
            {
                var bar = builder.AddInnerBar(Math.Abs(trueNorthDuration), maximum, Config.TrueNorthBarFillColor)
                                 .SetBackgroundColor(EmptyColor.Background)
                                 .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList);
            }
        }

        private void DrawPerfectBalanceBar(Vector2 origin)
        {
            var target = Plugin.ClientState.LocalPlayer;
            var perfectBalance = target.StatusEffects.FirstOrDefault(o => o.EffectId == 110);
            var perfectBalanceDuration = perfectBalance.StackCount;

            var position = origin + Config.Position + Config.PerfectBalanceBarPosition - Config.PerfectBalanceBarSize / 2f;
            var builder = BarBuilder.Create(position, Config.PerfectBalanceBarSize);
            var maximum = 6f;

            if (perfectBalanceDuration > 0)
            {
                var bar = builder.AddInnerBar(Math.Abs(perfectBalanceDuration), maximum, Config.PerfectBalanceBarFillColor)
                                 .SetVertical(true)
                                 .SetFlipDrainDirection(Config.PerfectBalanceInverted)
                                 .SetTextMode(BarTextMode.EachChunk)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                                 .SetBackgroundColor(EmptyColor.Background)
                                 .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList);
            }
            else
            {
                var bar = builder.AddInnerBar(Math.Abs(perfectBalanceDuration), maximum, Config.PerfectBalanceBarFillColor)
                                 .SetBackgroundColor(EmptyColor.Background)
                                 .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList);
            }
        }

        private void DrawRiddleOfEarthBar(Vector2 origin)
        {
            var target = Plugin.ClientState.LocalPlayer;
            var riddleOfEarth = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1179);
            var riddleOfEarthDuration = riddleOfEarth.StackCount;

            var position = origin + Config.Position + Config.RiddleofEarthBarPosition - Config.RiddleofEarthBarSize / 2f;
            var builder = BarBuilder.Create(position, Config.RiddleofEarthBarSize);
            var maximum = 3f;

            if (riddleOfEarthDuration > 0)
            {
                var bar = builder.AddInnerBar(Math.Abs(riddleOfEarthDuration), maximum, Config.RiddleofEarthBarFillColor)
                                 .SetTextMode(BarTextMode.EachChunk)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                                 .SetBackgroundColor(EmptyColor.Background)
                                 .SetFlipDrainDirection(Config.RiddleofEarthInverted)
                                 .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList);
            }
            else
            {
                var bar = builder.AddInnerBar(Math.Abs(riddleOfEarthDuration), maximum, Config.RiddleofEarthBarFillColor)
                                 .SetBackgroundColor(EmptyColor.Background)
                                 .SetFlipDrainDirection(Config.RiddleofEarthInverted == false)
                                 .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList);
            }
        }

        private void DrawChakraGauge(Vector2 origin)
        {
            var gauge = Plugin.JobGauges.Get<MNKGauge>();

            var position = origin + Config.Position + Config.ChakraBarPosition - Config.ChakraBarSize / 2f;
            var bar = BarBuilder.Create(position, Config.ChakraBarSize)
                                .SetChunks(5)
                                .SetChunkPadding(2)
                                .AddInnerBar(gauge.NumChakra, 5, Config.ChakraBarFillColor, EmptyColor)
                                .SetBackgroundColor(EmptyColor.Background)
                                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        private void DrawTwinSnakesBar(Vector2 origin)
        {
            var target = Plugin.ClientState.LocalPlayer;
            var twinSnakes = target.StatusEffects.FirstOrDefault(o => o.EffectId == 101);
            var twinSnakesDuration = twinSnakes.Duration;

            var position = origin + Config.Position + Config.TwinSnakesBarPosition - Config.TwinSnakesBarSize / 2f;

            var builder = BarBuilder.Create(position, Config.TwinSnakesBarSize);
            var maximum = 15f;

            var bar = builder.AddInnerBar(Math.Abs(twinSnakesDuration), maximum, Config.TwinSnakesBarFillColor)
                             .SetTextMode(BarTextMode.EachChunk)
                             .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                             .SetBackgroundColor(EmptyColor.Background)
                             .SetFlipDrainDirection(Config.TwinSnakesBarInverted)
                             .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        private void DrawLeadenFistBar(Vector2 origin)
        {
            var target = Plugin.ClientState.LocalPlayer;
            var leadenFist = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1861);
            var leadenFistDuration = leadenFist.Duration;

            var position = origin + Config.Position + Config.LeadenFistBarPosition - Config.LeadenFistBarSize / 2f;
            var builder = BarBuilder.Create(position, Config.LeadenFistBarSize);
            var maximum = 30f;

            if (leadenFistDuration > 0)
            {
                var bar = builder.AddInnerBar(Math.Abs(leadenFistDuration), maximum, Config.LeadenFistBarFillColor)
                                 .SetVertical(true)
                                 .SetTextMode(BarTextMode.EachChunk)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                                 .SetBackgroundColor(EmptyColor.Background)
                                 .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList);
            }
            else
            {
                var bar = builder.AddInnerBar(Math.Abs(leadenFistDuration), maximum, Config.LeadenFistBarFillColor)
                                 .SetBackgroundColor(EmptyColor.Background)
                                 .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList);
            }
        }

        private void DrawDemolishBar(Vector2 origin)
        {
            var target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.CurrentTarget ?? Plugin.ClientState.LocalPlayer;
            var demolish = target.StatusEffects.FirstOrDefault(o => o.EffectId == 246 || o.EffectId == 1309);
            var demolishDuration = demolish.Duration;

            var position = origin + Config.Position + Config.DemolishBarPosition - Config.DemolishBarSize / 2f;
            var builder = BarBuilder.Create(position, Config.DemolishBarSize);
            var maximum = 18f;

            var bar = builder.AddInnerBar(Math.Abs(demolishDuration), maximum, Config.DemolishBarFillColor)
                             .SetTextMode(BarTextMode.EachChunk)
                             .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                             .SetBackgroundColor(EmptyColor.Background)
                             .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Melee", 0)]
    [SubSection("Monk", 1)]
    public class MonkConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.MNK;
        public new static MonkConfig DefaultConfig() { return new MonkConfig(); }

        #region Demolish Bar
        [Checkbox("Show Demolish Bar")]
        [CollapseControl(30, 0)]
        public bool ShowDemolishBar = true;

        [DragFloat2("Position" + "##Demolish", min = -4000f, max = 4000f)]
        [CollapseWith(0, 0)]
        public Vector2 DemolishBarPosition = new(71, -10);

        [DragFloat2("Size" + "##Demolish", min = 0, max = 4000f)]
        [CollapseWith(5, 0)]
        public Vector2 DemolishBarSize = new(111, 20);

        [ColorEdit4("Color" + "##Demolish")]
        [CollapseWith(10, 0)]
        public PluginConfigColor DemolishBarFillColor = new(new Vector4(246f / 255f, 169f / 255f, 255f / 255f, 100f / 100f));
        #endregion

        #region Chakra Bar
        [Checkbox("Show Chakra Bar")]
        [CollapseControl(35, 1)]
        public bool ShowChakraBar = true;

        [DragFloat2("Position" + "##Chakbra", min = -4000f, max = 4000f)]
        [CollapseWith(0, 1)]
        public Vector2 ChakraBarPosition = new(0, -32);

        [DragFloat2("Size" + "##Chakbra", min = 0, max = 4000f)]
        [CollapseWith(5, 1)]
        public Vector2 ChakraBarSize = new(254, 20);

        [ColorEdit4("Color" + "##Chakbra")]
        [CollapseWith(10, 1)]
        public PluginConfigColor ChakraBarFillColor = new(new Vector4(204f / 255f, 115f / 255f, 0f, 100f / 100f));
        #endregion

        #region Leaden Fist Bar
        [Checkbox("Show Leaden Fist Bar")]
        [CollapseControl(40, 2)]
        public bool ShowLeadenFistBar = true;

        [DragFloat2("Position" + "##LeadenFist", min = -4000f, max = 4000f)]
        [CollapseWith(0, 2)]
        public Vector2 LeadenFistBarPosition = new(0, -10);

        [DragFloat2("Size" + "##LeadenFist", min = 0, max = 4000f)]
        [CollapseWith(5, 2)]
        public Vector2 LeadenFistBarSize = new(28, 20);

        [ColorEdit4("Color" + "##LeadenFist")]
        [CollapseWith(10, 2)]
        public PluginConfigColor LeadenFistBarFillColor = new(new Vector4(255f / 255f, 0f, 0f, 100f / 100f));
        #endregion

        #region Twin Snakes Bar
        [Checkbox("Show Twin Snakes Bar")]
        [CollapseControl(45, 3)]
        public bool ShowTwinSnakesBar = true;

        [DragFloat2("Position" + "##TwinSnakes", min = -4000f, max = 4000f)]
        [CollapseWith(0, 3)]
        public Vector2 TwinSnakesBarPosition = new(-71, -10);

        [DragFloat2("Size" + "##TwinSnakes", min = 0, max = 4000f)]
        [CollapseWith(5, 3)]
        public Vector2 TwinSnakesBarSize = new(111, 20);

        [ColorEdit4("Color" + "##TwinSnakes")]
        [CollapseWith(10, 3)]
        public PluginConfigColor TwinSnakesBarFillColor = new(new Vector4(227f / 255f, 255f / 255f, 64f / 255f, 100f / 100f));

        [Checkbox("Inverted" + "##TwinSnakes")]
        [CollapseWith(15, 3)]
        public bool TwinSnakesBarInverted = true;
        #endregion

        #region Riddle of Earth
        [Checkbox("Show Riddle of Earth Bar")]
        [CollapseControl(50, 4)]
        public bool ShowRiddleofEarthBar = true;

        [DragFloat2("Position" + "##RiddleofEarth", min = -4000f, max = 4000f)]
        [CollapseWith(0, 4)]
        public Vector2 RiddleofEarthBarPosition = new(-69, -54);

        [DragFloat2("Size" + "##RiddleofEarth", min = 0, max = 4000f)]
        [CollapseWith(5, 4)]
        public Vector2 RiddleofEarthBarSize = new(115, 20);

        [ColorEdit4("Color" + "##RiddleofEarth")]
        [CollapseWith(10, 4)]
        public PluginConfigColor RiddleofEarthBarFillColor = new(new Vector4(157f / 255f, 59f / 255f, 255f / 255f, 100f / 100f));

        [Checkbox("Inverted" + "##RiddleofEarth")]
        [CollapseWith(15, 4)]
        public bool RiddleofEarthInverted = true;
        #endregion

        #region Perfect Balance
        [Checkbox("Show Perfect Balance Bar")]
        [CollapseControl(55, 5)]
        public bool ShowPerfectBalanceBar = true;

        [DragFloat2("Position" + "##PerfectBalance", min = -4000f, max = 4000f)]
        [CollapseWith(0, 5)]
        public Vector2 PerfectBalanceBarPosition = new(0, -54);

        [DragFloat2("Size" + "##PerfectBalance", min = 0, max = 4000f)]
        [CollapseWith(5, 5)]
        public Vector2 PerfectBalanceBarSize = new(20, 20);

        [ColorEdit4("Color" + "##PerfectBalance")]
        [CollapseWith(10, 5)]
        public PluginConfigColor PerfectBalanceBarFillColor = new(new Vector4(150f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [Checkbox("Inverted" + "##PerfectBalance")]
        [CollapseWith(15, 5)]
        public bool PerfectBalanceInverted = true;
        #endregion

        #region True North
        [Checkbox("Show True North Bar")]
        [CollapseControl(60, 6)]
        public bool ShowTrueNorthBar = true;

        [DragFloat2("Position" + "##TrueNorth", min = -4000f, max = 4000f)]
        [CollapseWith(0, 6)]
        public Vector2 TrueNorthBarPosition = new(69, -54);

        [DragFloat2("Size" + "##TrueNorth", min = 0, max = 4000f)]
        [CollapseWith(5, 6)]
        public Vector2 TrueNorthBarSize = new(115, 20);

        [ColorEdit4("Color" + "##TrueNorth")]
        [CollapseWith(10, 6)]
        public PluginConfigColor TrueNorthBarFillColor = new(new Vector4(255f / 255f, 225f / 255f, 189f / 255f, 100f / 100f));
        #endregion

        #region Forms
        [Checkbox("Show Forms Bar" + "##Forms")]
        [CollapseControl(65, 7)]
        public bool ShowFormsBar = false;

        [DragFloat2("Position" + "##Forms", min = -4000f, max = 4000f)]
        [CollapseWith(0, 7)]
        public Vector2 FormsBarPosition = new(0, -76);

        [DragFloat2("Size" + "##Forms", min = 0, max = 4000f)]
        [CollapseWith(5, 7)]
        public Vector2 FormsBarSize = new(254, 20);

        [ColorEdit4("Color" + "##Forms")]
        [CollapseWith(10, 7)]
        public PluginConfigColor FormsBarFillColor = new(new Vector4(36f / 255f, 131f / 255f, 255f / 255f, 100f / 100f));
        #endregion
    }
}
