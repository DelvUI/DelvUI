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

        public MonkHud(MonkConfig config, string? displayName = null) : base(config, displayName)
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
            var opoOpoForm = player.StatusList.Where(o => o.StatusId is 107);
            var raptorForm = player.StatusList.Where(o => o.StatusId is 108);
            var coeurlForm = player.StatusList.Where(o => o.StatusId is 109);
            var formlessFist = player.StatusList.Where(o => o.StatusId is 2513);

            var opoOpoFormDuration = 0f;
            if (opoOpoForm.Any())
            {
                opoOpoFormDuration = Math.Abs(opoOpoForm.First().RemainingTime);
            }
            var raptorFormDuration = 0f;
            if (raptorForm.Any())
            {
                raptorFormDuration = Math.Abs(raptorForm.First().RemainingTime);
            }
            var coeurlFormDuration = 0f;
            if (coeurlForm.Any())
            {
                coeurlFormDuration = Math.Abs(coeurlForm.First().RemainingTime);
            }
            var formlessFistDuration = 0f;
            if (formlessFist.Any())
            {
                formlessFistDuration = Math.Abs(formlessFist.First().RemainingTime);
            }

            if (Config.OnlyShowFormsWhenActive && opoOpoFormDuration == 0 && raptorFormDuration == 0 && coeurlFormDuration == 0 && formlessFistDuration == 0)
            {
                return;
            }

            var position = origin + Config.Position + Config.FormsBarPosition - Config.FormsBarSize / 2f;

            var formsBuilder = BarBuilder.Create(position, Config.FormsBarSize)
                .SetBackgroundColor(EmptyColor.Background);
            var maximum = 15f;

            if (opoOpoFormDuration > 0)
            {
                formsBuilder.AddInnerBar(Math.Abs(opoOpoFormDuration), maximum, Config.FormsBarFillColor)
                                 .SetTextMode(BarTextMode.Single)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, "Opo-Opo Form");
            }

            else if (raptorFormDuration > 0)
            {
                formsBuilder.AddInnerBar(Math.Abs(raptorFormDuration), maximum, Config.FormsBarFillColor)
                                 .SetTextMode(BarTextMode.Single)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, "Raptor Form");
            }

            else if (coeurlFormDuration > 0)
            {
                var bar = formsBuilder.AddInnerBar(Math.Abs(coeurlFormDuration), maximum, Config.FormsBarFillColor)
                                 .SetTextMode(BarTextMode.Single)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, "Coeurl Form");
            }

            else if (formlessFistDuration > 0)
            {
                var bar = formsBuilder.AddInnerBar(Math.Abs(formlessFistDuration), maximum, Config.FormsBarFillColor)
                                 .SetTextMode(BarTextMode.Single)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, "Formless Fist");
            }

            var drawList = ImGui.GetWindowDrawList();
            formsBuilder.Build().Draw(drawList);
        }

        private void DrawTrueNorthBar(Vector2 origin, PlayerCharacter player)
        {
            var trueNorth = player.StatusList.Where(o => o.StatusId is 1250);
            var trueNorthDuration = 0f;
            if (trueNorth.Any())
            {
                trueNorthDuration = Math.Abs(trueNorth.First().RemainingTime);
            }

            if (trueNorthDuration == 0 && Config.OnlyShowTrueNorthWhenActive)
            {
                return;
            }

            var position = origin + Config.Position + Config.TrueNorthBarPosition - Config.TrueNorthBarSize / 2f;
            var trueNorthBuilder = BarBuilder.Create(position, Config.TrueNorthBarSize)
                .SetBackgroundColor(EmptyColor.Background);
            var maximum = 10f;

            if (trueNorthDuration > 0)
            {
                trueNorthBuilder.AddInnerBar(Math.Abs(trueNorthDuration), maximum, Config.TrueNorthBarFillColor);

                if (Config.ShowTrueNorthText)
                {
                    trueNorthBuilder.SetTextMode(BarTextMode.Single)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
            }

            var drawList = ImGui.GetWindowDrawList();
            trueNorthBuilder.Build().Draw(drawList);
        }

        private void DrawPerfectBalanceBar(Vector2 origin, PlayerCharacter player)
        {
            var perfectBalance = player.StatusList.Where(o => o.StatusId is 110);
            var perfectBalanceDuration = 0f;
            var perfectBalanceStacks = 0;
            if (perfectBalance.Any())
            {
                perfectBalanceStacks = perfectBalance.First().StackCount;
                perfectBalanceDuration = perfectBalance.First().RemainingTime;
            }

            if (perfectBalanceDuration == 0 && Config.OnlyShowPerfectBalanceWhenActive)
            {
                return;
            }

            var position = origin + Config.Position + Config.PerfectBalanceBarPosition - Config.PerfectBalanceBarSize / 2f;
            var perfectBalanceBuilder = BarBuilder.Create(position, Config.PerfectBalanceBarSize)
                .SetBackgroundColor(EmptyColor.Background);
            var maximum = 15f;

            if (perfectBalanceDuration > 0)
            {
                perfectBalanceBuilder.AddInnerBar(perfectBalanceDuration, maximum, Config.PerfectBalanceBarFillColor)
                                 .SetVertical(Config.PerfectBalanceVertical)
                                 .SetFlipDrainDirection(Config.PerfectBalanceInverted)
                                 .SetTextMode(BarTextMode.Single)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, perfectBalanceStacks.ToString());
            }

            var drawList = ImGui.GetWindowDrawList();
            perfectBalanceBuilder.Build().Draw(drawList);
        }

        private void DrawRiddleOfEarthBar(Vector2 origin, PlayerCharacter player)
        {
            var riddleOfEarth = player.StatusList.Where(o => o.StatusId is 1179);
            var riddleOfEarthDuration = 0f;
            if (riddleOfEarth.Any())
            {
                riddleOfEarthDuration = Math.Abs(riddleOfEarth.First().RemainingTime);
            }

            if (riddleOfEarthDuration == 0 && Config.OnlyShowRiddleOfEarthWhenActive)
            {
                return;
            }

            var position = origin + Config.Position + Config.RiddleofEarthBarPosition - Config.RiddleofEarthBarSize / 2f;
            var riddleOfEarthBuilder = BarBuilder.Create(position, Config.RiddleofEarthBarSize)
                .SetBackgroundColor(EmptyColor.Background);
            var maximum = 3f;

            if (riddleOfEarthDuration > 0)
            {
                riddleOfEarthBuilder.AddInnerBar(Math.Abs(riddleOfEarthDuration), maximum, Config.RiddleofEarthBarFillColor)
                                 .SetFlipDrainDirection(Config.RiddleofEarthInverted);

                if (Config.ShowRiddleofEarthText)
                {
                    riddleOfEarthBuilder.SetTextMode(BarTextMode.Single)
                        .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
            }

            var drawList = ImGui.GetWindowDrawList();
            riddleOfEarthBuilder.Build().Draw(drawList);
        }

        private void DrawChakraGauge(Vector2 origin)
        {
            var gauge = Plugin.JobGauges.Get<MNKGauge>();

            if (gauge.Chakra == 0 && Config.OnlyShowChakraWhenActive)
            {
                return;
            }

            var position = origin + Config.Position + Config.ChakraBarPosition - Config.ChakraBarSize / 2f;
            var chakraBuilder = BarBuilder.Create(position, Config.ChakraBarSize).SetBackgroundColor(EmptyColor.Background)
                                .SetChunks(5)
                                .SetChunkPadding(2);

            if (gauge.Chakra > 0)
            {
                chakraBuilder.AddInnerBar(gauge.Chakra, 5, Config.ChakraBarFillColor, EmptyColor);
            }           

            var drawList = ImGui.GetWindowDrawList();
            chakraBuilder.Build().Draw(drawList);
        }

        private void DrawTwinSnakesBar(Vector2 origin, PlayerCharacter player)
        {
            var twinSnakes = player.StatusList.Where(o => o.StatusId is 101);
            var twinSnakesDuration = 0f;
            if (twinSnakes.Any())
            {
                twinSnakesDuration = Math.Abs(twinSnakes.First().RemainingTime);
            }

            if (twinSnakesDuration == 0 && Config.OnlyShowTwinSnakesWhenActive)
            {
                return;
            }

            var position = origin + Config.Position + Config.TwinSnakesBarPosition - Config.TwinSnakesBarSize / 2f;

            var twinSnakesBuilder = BarBuilder.Create(position, Config.TwinSnakesBarSize)
                .SetBackgroundColor(EmptyColor.Background);
            var maximum = 15f;

            if(twinSnakesDuration > 0)
            {
                twinSnakesBuilder.AddInnerBar(Math.Abs(twinSnakesDuration), maximum, Config.TwinSnakesBarFillColor)
                             .SetFlipDrainDirection(Config.TwinSnakesBarInverted);

                if (Config.ShowTwinSnakesText)
                {
                    twinSnakesBuilder.SetTextMode(BarTextMode.Single)
                        .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
            }

            var drawList = ImGui.GetWindowDrawList();
            twinSnakesBuilder.Build().Draw(drawList);
        }

        private void DrawLeadenFistBar(Vector2 origin, PlayerCharacter player)
        {
            var leadenFist = player.StatusList.Where(o => o.StatusId is 1861);
            var leadenFistDuration = 0f;
            if (leadenFist.Any())
            {
                leadenFistDuration = Math.Abs(leadenFist.First().RemainingTime);
            }

            if (leadenFistDuration == 0 && Config.OnlyShowLeadenFistWhenActive)
            {
                return;
            }

            var position = origin + Config.Position + Config.LeadenFistBarPosition - Config.LeadenFistBarSize / 2f;
            var leadenFistBuilder = BarBuilder.Create(position, Config.LeadenFistBarSize)
                .SetBackgroundColor(EmptyColor.Background);
            var maximum = 30f;

            if (leadenFistDuration > 0)
            {
                leadenFistBuilder.AddInnerBar(Math.Abs(leadenFistDuration), maximum, Config.LeadenFistBarFillColor)
                                 .SetVertical(Config.LeadenFistVertical);

                if (Config.ShowLeadenFistText)
                {
                    leadenFistBuilder.SetTextMode(BarTextMode.Single)
                        .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
            }

            var drawList = ImGui.GetWindowDrawList();
            leadenFistBuilder.Build().Draw(drawList);
        }

        private void DrawDemolishBar(Vector2 origin, PlayerCharacter player)
        {
            var actor = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target ?? player;

            var demolishDuration = 0f;            

            if (actor is BattleChara target)
            {
                var demolish = target.StatusList.Where(o => o.StatusId is 246 && o.SourceID == player.ObjectId);

                if (demolish.Any())
                {
                    demolishDuration = Math.Abs(demolish.First().RemainingTime);
                }                
            }

            if (demolishDuration == 0 && Config.OnlyShowDemolishWhenActive)
            {
                return;
            }

            var position = origin + Config.Position + Config.DemolishBarPosition - Config.DemolishBarSize / 2f;
            var demolishBuilder = BarBuilder.Create(position, Config.DemolishBarSize)
                .SetBackgroundColor(EmptyColor.Background);
            var maximum = 18f;

            if (demolishDuration > 0)
            {
                demolishBuilder.AddInnerBar(Math.Abs(demolishDuration), maximum, Config.DemolishBarFillColor);

                if (Config.ShowDemolishText)
                {
                    demolishBuilder.SetTextMode(BarTextMode.Single)
                        .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
            }

            var drawList = ImGui.GetWindowDrawList();
            demolishBuilder.Build().Draw(drawList);
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

        [Checkbox("Only Show When Active" + "##Demolish")]
        [Order(35, collapseWith = nameof(ShowDemolishBar))]
        public bool OnlyShowDemolishWhenActive = false;

        [DragFloat2("Position" + "##Demolish", min = -4000f, max = 4000f)]
        [Order(40, collapseWith = nameof(ShowDemolishBar))]
        public Vector2 DemolishBarPosition = new(71, -10);

        [DragFloat2("Size" + "##Demolish", min = 0, max = 4000f)]
        [Order(45, collapseWith = nameof(ShowDemolishBar))]
        public Vector2 DemolishBarSize = new(111, 20);

        [ColorEdit4("Color" + "##Demolish")]
        [Order(50, collapseWith = nameof(ShowDemolishBar))]
        public PluginConfigColor DemolishBarFillColor = new(new Vector4(246f / 255f, 169f / 255f, 255f / 255f, 100f / 100f));

        [Checkbox("Text" + "##Demolish")]
        [Order(55, collapseWith = nameof(ShowDemolishBar))]
        public bool ShowDemolishText = true;
        #endregion

        #region Chakra Bar
        [Checkbox("Chakra", separator = true)]
        [Order(60)]
        public bool ShowChakraBar = true;

        [Checkbox("Only Show When Active" + "##Chakra")]
        [Order(65, collapseWith = nameof(ShowChakraBar))]
        public bool OnlyShowChakraWhenActive = false;

        [DragFloat2("Position" + "##Chakra", min = -4000f, max = 4000f)]
        [Order(70, collapseWith = nameof(ShowChakraBar))]
        public Vector2 ChakraBarPosition = new(0, -32);

        [DragFloat2("Size" + "##Chakra", min = 0, max = 4000f)]
        [Order(75, collapseWith = nameof(ShowChakraBar))]
        public Vector2 ChakraBarSize = new(254, 20);

        [ColorEdit4("Color" + "##Chakra")]
        [Order(80, collapseWith = nameof(ShowChakraBar))]
        public PluginConfigColor ChakraBarFillColor = new(new Vector4(204f / 255f, 115f / 255f, 0f, 100f / 100f));
        #endregion

        #region Leaden Fist Bar
        [Checkbox("Leaden Fist", separator = true)]
        [Order(85)]
        public bool ShowLeadenFistBar = true;

        [Checkbox("Only Show When Active" + "##LeadenFist")]
        [Order(90, collapseWith = nameof(ShowLeadenFistBar))]
        public bool OnlyShowLeadenFistWhenActive = false;

        [DragFloat2("Position" + "##LeadenFist", min = -4000f, max = 4000f)]
        [Order(95, collapseWith = nameof(ShowLeadenFistBar))]
        public Vector2 LeadenFistBarPosition = new(0, -10);

        [DragFloat2("Size" + "##LeadenFist", min = 0, max = 4000f)]
        [Order(100, collapseWith = nameof(ShowLeadenFistBar))]
        public Vector2 LeadenFistBarSize = new(28, 20);

        [ColorEdit4("Color" + "##LeadenFist")]
        [Order(105, collapseWith = nameof(ShowLeadenFistBar))]
        public PluginConfigColor LeadenFistBarFillColor = new(new Vector4(255f / 255f, 0f, 0f, 100f / 100f));

        [Checkbox("Text" + "##LeadenFist")]
        [Order(110, collapseWith = nameof(ShowLeadenFistBar))]
        public bool ShowLeadenFistText = false;

        [Checkbox("Vertical" + "##LeadenFist")]
        [Order(115, collapseWith = nameof(ShowLeadenFistBar))]
        public bool LeadenFistVertical = true;
        #endregion

        #region Twin Snakes Bar
        [Checkbox("Twin Snakes", separator = true)]
        [Order(120)]
        public bool ShowTwinSnakesBar = true;

        [Checkbox("Only Show When Active" + "##TwinSnakes")]
        [Order(125, collapseWith = nameof(ShowTwinSnakesBar))]
        public bool OnlyShowTwinSnakesWhenActive = false;

        [DragFloat2("Position" + "##TwinSnakes", min = -4000f, max = 4000f)]
        [Order(130, collapseWith = nameof(ShowTwinSnakesBar))]
        public Vector2 TwinSnakesBarPosition = new(-71, -10);

        [DragFloat2("Size" + "##TwinSnakes", min = 0, max = 4000f)]
        [Order(135, collapseWith = nameof(ShowTwinSnakesBar))]
        public Vector2 TwinSnakesBarSize = new(111, 20);

        [ColorEdit4("Color" + "##TwinSnakes")]
        [Order(140, collapseWith = nameof(ShowTwinSnakesBar))]
        public PluginConfigColor TwinSnakesBarFillColor = new(new Vector4(227f / 255f, 255f / 255f, 64f / 255f, 100f / 100f));

        [Checkbox("Text" + "##TwinSnakes")]
        [Order(145, collapseWith = nameof(ShowTwinSnakesBar))]
        public bool ShowTwinSnakesText = true;

        [Checkbox("Inverted" + "##TwinSnakes")]
        [Order(150, collapseWith = nameof(ShowTwinSnakesBar))]
        public bool TwinSnakesBarInverted = true;
        #endregion

        #region Riddle of Earth
        [Checkbox("Riddle of Earth", separator = true)]
        [Order(155)]
        public bool ShowRiddleofEarthBar = true;

        [Checkbox("Only Show When Active" + "##RiddleofEarth")]
        [Order(160, collapseWith = nameof(ShowRiddleofEarthBar))]
        public bool OnlyShowRiddleOfEarthWhenActive = false;

        [DragFloat2("Position" + "##RiddleofEarth", min = -4000f, max = 4000f)]
        [Order(165, collapseWith = nameof(ShowRiddleofEarthBar))]
        public Vector2 RiddleofEarthBarPosition = new(-69, -54);

        [DragFloat2("Size" + "##RiddleofEarth", min = 0, max = 4000f)]
        [Order(170, collapseWith = nameof(ShowRiddleofEarthBar))]
        public Vector2 RiddleofEarthBarSize = new(115, 20);

        [ColorEdit4("Color" + "##RiddleofEarth")]
        [Order(175, collapseWith = nameof(ShowRiddleofEarthBar))]
        public PluginConfigColor RiddleofEarthBarFillColor = new(new Vector4(157f / 255f, 59f / 255f, 255f / 255f, 100f / 100f));

        [Checkbox("Text" + "##RiddleofEarth")]
        [Order(180, collapseWith = nameof(ShowRiddleofEarthBar))]
        public bool ShowRiddleofEarthText = true;

        [Checkbox("Inverted" + "##RiddleofEarth")]
        [Order(185, collapseWith = nameof(ShowRiddleofEarthBar))]
        public bool RiddleofEarthInverted = true;
        #endregion

        #region Perfect Balance
        [Checkbox("Perfect Balance", separator = true)]
        [Order(190)]
        public bool ShowPerfectBalanceBar = true;

        [Checkbox("Only Show When Active" + "##PerfectBalance")]
        [Order(195, collapseWith = nameof(ShowPerfectBalanceBar))]
        public bool OnlyShowPerfectBalanceWhenActive = false;

        [DragFloat2("Position" + "##PerfectBalance", min = -4000f, max = 4000f)]
        [Order(200, collapseWith = nameof(ShowPerfectBalanceBar))]
        public Vector2 PerfectBalanceBarPosition = new(0, -54);

        [DragFloat2("Size" + "##PerfectBalance", min = 0, max = 4000f)]
        [Order(205, collapseWith = nameof(ShowPerfectBalanceBar))]
        public Vector2 PerfectBalanceBarSize = new(20, 20);

        [ColorEdit4("Color" + "##PerfectBalance")]
        [Order(210, collapseWith = nameof(ShowPerfectBalanceBar))]
        public PluginConfigColor PerfectBalanceBarFillColor = new(new Vector4(150f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [Checkbox("Inverted" + "##PerfectBalance")]
        [Order(215, collapseWith = nameof(ShowPerfectBalanceBar))]
        public bool PerfectBalanceInverted = true;

        [Checkbox("Vertical" + "##PerfectBalance")]
        [Order(220, collapseWith = nameof(ShowPerfectBalanceBar))]
        public bool PerfectBalanceVertical = true;
        #endregion

        #region True North
        [Checkbox("True North", separator = true)]
        [Order(225)]
        public bool ShowTrueNorthBar = true;

        [Checkbox("Only Show When Active" + "##TrueNorth")]
        [Order(230, collapseWith = nameof(ShowTrueNorthBar))]
        public bool OnlyShowTrueNorthWhenActive = false;

        [DragFloat2("Position" + "##TrueNorth", min = -4000f, max = 4000f)]
        [Order(235, collapseWith = nameof(ShowTrueNorthBar))]
        public Vector2 TrueNorthBarPosition = new(69, -54);

        [DragFloat2("Size" + "##TrueNorth", min = 0, max = 4000f)]
        [Order(240, collapseWith = nameof(ShowTrueNorthBar))]
        public Vector2 TrueNorthBarSize = new(115, 20);

        [ColorEdit4("Color" + "##TrueNorth")]
        [Order(245, collapseWith = nameof(ShowTrueNorthBar))]
        public PluginConfigColor TrueNorthBarFillColor = new(new Vector4(255f / 255f, 225f / 255f, 189f / 255f, 100f / 100f));

        [Checkbox("Text" + "##TrueNorth")]
        [Order(250, collapseWith = nameof(ShowTrueNorthBar))]
        public bool ShowTrueNorthText = true;
        #endregion

        #region Forms
        [Checkbox("Forms" + "##Forms", separator = true)]
        [Order(265)]
        public bool ShowFormsBar = false;

        [Checkbox("Only Show When Active" + "##Forms")]
        [Order(270, collapseWith = nameof(ShowFormsBar))]
        public bool OnlyShowFormsWhenActive = false;

        [DragFloat2("Position" + "##Forms", min = -4000f, max = 4000f)]
        [Order(275, collapseWith = nameof(ShowFormsBar))]
        public Vector2 FormsBarPosition = new(0, -76);

        [DragFloat2("Size" + "##Forms", min = 0, max = 4000f)]
        [Order(280, collapseWith = nameof(ShowFormsBar))]
        public Vector2 FormsBarSize = new(254, 20);

        [ColorEdit4("Color" + "##Forms")]
        [Order(285, collapseWith = nameof(ShowFormsBar))]
        public PluginConfigColor FormsBarFillColor = new(new Vector4(36f / 255f, 131f / 255f, 255f / 255f, 100f / 100f));
        #endregion
    }
}
