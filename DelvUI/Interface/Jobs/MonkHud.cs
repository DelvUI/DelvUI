using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using DelvUI.Config;
using DelvUI.Config.Attributes;
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
    public class MonkHud : JobHud
    {
        private new MonkConfig Config => (MonkConfig)_config;

        public MonkHud(string id, MonkConfig config, string? displayName = null) : base(id, config, displayName)
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

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            if (Config.ShowFormsBar)
            {
                DrawFormsBar(origin, player);
            }

            if (Config.ShowRiddleofEarthBar)
            {
                DrawRiddleOfEarthBar(origin, player);
            }

            if (Config.ShowPerfectBalanceBar)
            {
                DrawPerfectBalanceBar(origin, player);
            }

            if (Config.ShowTrueNorthBar)
            {
                DrawTrueNorthBar(origin, player);
            }

            if (Config.ShowChakraBar)
            {
                DrawChakraGauge(origin);
            }

            if (Config.ShowLeadenFistBar)
            {
                DrawLeadenFistBar(origin, player);
            }

            if (Config.ShowTwinSnakesBar)
            {
                DrawTwinSnakesBar(origin, player);
            }

            if (Config.ShowDemolishBar)
            {
                DrawDemolishBar(origin, player);
            }
        }

        private void DrawFormsBar(Vector2 origin, PlayerCharacter player)
        {
            var opoOpoForm = player.StatusList.FirstOrDefault(o => o.StatusId == 107);
            var raptorForm = player.StatusList.FirstOrDefault(o => o.StatusId == 108);
            var coeurlForm = player.StatusList.FirstOrDefault(o => o.StatusId == 109);
            var formlessFist = player.StatusList.FirstOrDefault(o => o.StatusId == 2513);

            var opoOpoFormDuration = opoOpoForm?.RemainingTime ?? 0f;
            var raptorFormDuration = raptorForm?.RemainingTime ?? 0f;
            var coeurlFormDuration = coeurlForm?.RemainingTime ?? 0f;
            var formlessFistDuration = formlessFist?.RemainingTime ?? 0f;

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

            if (formlessFistDuration > 0)
            {
                var bar = builder.AddInnerBar(Math.Abs(formlessFistDuration), maximum, Config.FormsBarFillColor)
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

        private void DrawTrueNorthBar(Vector2 origin, PlayerCharacter player)
        {
            var trueNorth = player.StatusList.FirstOrDefault(o => o.StatusId == 1250);
            var trueNorthDuration = trueNorth?.RemainingTime ?? 0f;

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

        private void DrawPerfectBalanceBar(Vector2 origin, PlayerCharacter player)
        {
            var perfectBalance = player.StatusList.FirstOrDefault(o => o.StatusId == 110);
            var perfectBalanceDuration = perfectBalance?.StackCount ?? 0;

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

        private void DrawRiddleOfEarthBar(Vector2 origin, PlayerCharacter player)
        {
            var riddleOfEarth = player.StatusList.FirstOrDefault(o => o.StatusId == 1179);
            var riddleOfEarthDuration = riddleOfEarth?.StackCount ?? 0;

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
                                .AddInnerBar(gauge.Chakra, 5, Config.ChakraBarFillColor, EmptyColor)
                                .SetBackgroundColor(EmptyColor.Background)
                                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        private void DrawTwinSnakesBar(Vector2 origin, PlayerCharacter player)
        {
            var twinSnakes = player.StatusList.FirstOrDefault(o => o.StatusId == 101);
            var twinSnakesDuration = twinSnakes?.RemainingTime ?? 0f;

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

        private void DrawLeadenFistBar(Vector2 origin, PlayerCharacter player)
        {
            var leadenFist = player.StatusList.FirstOrDefault(o => o.StatusId == 1861);
            var leadenFistDuration = leadenFist?.RemainingTime ?? 0f;

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

        private void DrawDemolishBar(Vector2 origin, PlayerCharacter player)
        {
            var actor = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target ?? player;

            var demolishDuration = 0f;

            if (actor is BattleChara target)
            {
                Status? demolish = target.StatusList.FirstOrDefault(o => o.StatusId is 246 && o.SourceID == player.ObjectId);
                demolishDuration = demolish?.RemainingTime ?? 0f;
            }

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

    [Section("Job Specific Bars")]
    [SubSection("Melee", 0)]
    [SubSection("Monk", 1)]
    public class MonkConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.MNK;
        public new static MonkConfig DefaultConfig() { return new MonkConfig(); }

        #region Demolish Bar
        [Checkbox("Demolish", separator = true)]
        [Order(30)]
        public bool ShowDemolishBar = true;

        [DragFloat2("Position" + "##Demolish", min = -4000f, max = 4000f)]
        [Order(35, collapseWith = nameof(ShowDemolishBar))]
        public Vector2 DemolishBarPosition = new(71, -10);

        [DragFloat2("Size" + "##Demolish", min = 0, max = 4000f)]
        [Order(40, collapseWith = nameof(ShowDemolishBar))]
        public Vector2 DemolishBarSize = new(111, 20);

        [ColorEdit4("Color" + "##Demolish")]
        [Order(45, collapseWith = nameof(ShowDemolishBar))]
        public PluginConfigColor DemolishBarFillColor = new(new Vector4(246f / 255f, 169f / 255f, 255f / 255f, 100f / 100f));
        #endregion

        #region Chakra Bar
        [Checkbox("Chakra", separator = true)]
        [Order(50)]
        public bool ShowChakraBar = true;

        [DragFloat2("Position" + "##Chakbra", min = -4000f, max = 4000f)]
        [Order(55, collapseWith = nameof(ShowChakraBar))]
        public Vector2 ChakraBarPosition = new(0, -32);

        [DragFloat2("Size" + "##Chakbra", min = 0, max = 4000f)]
        [Order(60, collapseWith = nameof(ShowChakraBar))]
        public Vector2 ChakraBarSize = new(254, 20);

        [ColorEdit4("Color" + "##Chakbra")]
        [Order(65, collapseWith = nameof(ShowChakraBar))]
        public PluginConfigColor ChakraBarFillColor = new(new Vector4(204f / 255f, 115f / 255f, 0f, 100f / 100f));
        #endregion

        #region Leaden Fist Bar
        [Checkbox("Leaden Fist", separator = true)]
        [Order(70)]
        public bool ShowLeadenFistBar = true;

        [DragFloat2("Position" + "##LeadenFist", min = -4000f, max = 4000f)]
        [Order(75, collapseWith = nameof(ShowLeadenFistBar))]
        public Vector2 LeadenFistBarPosition = new(0, -10);

        [DragFloat2("Size" + "##LeadenFist", min = 0, max = 4000f)]
        [Order(80, collapseWith = nameof(ShowLeadenFistBar))]
        public Vector2 LeadenFistBarSize = new(28, 20);

        [ColorEdit4("Color" + "##LeadenFist")]
        [Order(85, collapseWith = nameof(ShowLeadenFistBar))]
        public PluginConfigColor LeadenFistBarFillColor = new(new Vector4(255f / 255f, 0f, 0f, 100f / 100f));
        #endregion

        #region Twin Snakes Bar
        [Checkbox("Twin Snakes", separator = true)]
        [Order(90)]
        public bool ShowTwinSnakesBar = true;

        [DragFloat2("Position" + "##TwinSnakes", min = -4000f, max = 4000f)]
        [Order(95, collapseWith = nameof(ShowTwinSnakesBar))]
        public Vector2 TwinSnakesBarPosition = new(-71, -10);

        [DragFloat2("Size" + "##TwinSnakes", min = 0, max = 4000f)]
        [Order(100, collapseWith = nameof(ShowTwinSnakesBar))]
        public Vector2 TwinSnakesBarSize = new(111, 20);

        [ColorEdit4("Color" + "##TwinSnakes")]
        [Order(105, collapseWith = nameof(ShowTwinSnakesBar))]
        public PluginConfigColor TwinSnakesBarFillColor = new(new Vector4(227f / 255f, 255f / 255f, 64f / 255f, 100f / 100f));

        [Checkbox("Inverted" + "##TwinSnakes")]
        [Order(110, collapseWith = nameof(ShowTwinSnakesBar))]
        public bool TwinSnakesBarInverted = true;
        #endregion

        #region Riddle of Earth
        [Checkbox("Riddle of Earth", separator = true)]
        [Order(115)]
        public bool ShowRiddleofEarthBar = true;

        [DragFloat2("Position" + "##RiddleofEarth", min = -4000f, max = 4000f)]
        [Order(120, collapseWith = nameof(ShowRiddleofEarthBar))]
        public Vector2 RiddleofEarthBarPosition = new(-69, -54);

        [DragFloat2("Size" + "##RiddleofEarth", min = 0, max = 4000f)]
        [Order(125, collapseWith = nameof(ShowRiddleofEarthBar))]
        public Vector2 RiddleofEarthBarSize = new(115, 20);

        [ColorEdit4("Color" + "##RiddleofEarth")]
        [Order(130, collapseWith = nameof(ShowRiddleofEarthBar))]
        public PluginConfigColor RiddleofEarthBarFillColor = new(new Vector4(157f / 255f, 59f / 255f, 255f / 255f, 100f / 100f));

        [Checkbox("Inverted" + "##RiddleofEarth")]
        [Order(135, collapseWith = nameof(ShowRiddleofEarthBar))]
        public bool RiddleofEarthInverted = true;
        #endregion

        #region Perfect Balance
        [Checkbox("Perfect Balance", separator = true)]
        [Order(140)]
        public bool ShowPerfectBalanceBar = true;

        [DragFloat2("Position" + "##PerfectBalance", min = -4000f, max = 4000f)]
        [Order(145, collapseWith = nameof(ShowPerfectBalanceBar))]
        public Vector2 PerfectBalanceBarPosition = new(0, -54);

        [DragFloat2("Size" + "##PerfectBalance", min = 0, max = 4000f)]
        [Order(150, collapseWith = nameof(ShowPerfectBalanceBar))]
        public Vector2 PerfectBalanceBarSize = new(20, 20);

        [ColorEdit4("Color" + "##PerfectBalance")]
        [Order(155, collapseWith = nameof(ShowPerfectBalanceBar))]
        public PluginConfigColor PerfectBalanceBarFillColor = new(new Vector4(150f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [Checkbox("Inverted" + "##PerfectBalance")]
        [Order(160, collapseWith = nameof(ShowPerfectBalanceBar))]
        public bool PerfectBalanceInverted = true;
        #endregion

        #region True North
        [Checkbox("True North", separator = true)]
        [Order(165)]
        public bool ShowTrueNorthBar = true;

        [DragFloat2("Position" + "##TrueNorth", min = -4000f, max = 4000f)]
        [Order(170, collapseWith = nameof(ShowTrueNorthBar))]
        public Vector2 TrueNorthBarPosition = new(69, -54);

        [DragFloat2("Size" + "##TrueNorth", min = 0, max = 4000f)]
        [Order(175, collapseWith = nameof(ShowTrueNorthBar))]
        public Vector2 TrueNorthBarSize = new(115, 20);

        [ColorEdit4("Color" + "##TrueNorth")]
        [Order(180, collapseWith = nameof(ShowTrueNorthBar))]
        public PluginConfigColor TrueNorthBarFillColor = new(new Vector4(255f / 255f, 225f / 255f, 189f / 255f, 100f / 100f));
        #endregion

        #region Forms
        [Checkbox("Forms" + "##Forms", separator = true)]
        [Order(185)]
        public bool ShowFormsBar = false;

        [DragFloat2("Position" + "##Forms", min = -4000f, max = 4000f)]
        [Order(190, collapseWith = nameof(ShowFormsBar))]
        public Vector2 FormsBarPosition = new(0, -76);

        [DragFloat2("Size" + "##Forms", min = 0, max = 4000f)]
        [Order(195, collapseWith = nameof(ShowFormsBar))]
        public Vector2 FormsBarSize = new(254, 20);

        [ColorEdit4("Color" + "##Forms")]
        [Order(200, collapseWith = nameof(ShowFormsBar))]
        public PluginConfigColor FormsBarFillColor = new(new Vector4(36f / 255f, 131f / 255f, 255f / 255f, 100f / 100f));
        #endregion
    }
}
