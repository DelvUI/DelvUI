using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Interface.Bars;
using ImGuiNET;
using System;
using System.Numerics;
using System.Linq;
using System.Diagnostics;

namespace DelvUI.Interface
{
    public class MonkHudWindow : HudWindow
    {
        public override uint JobId => Jobs.MNK;

        private MonkHudConfig _config => (MonkHudConfig)ConfigurationManager.GetInstance().GetConfiguration(new MonkHudConfig());

        private Vector2 Origin => new(CenterX + _config.Position.X, CenterY + _config.Position.Y);

        private readonly PluginConfigColor EmptyColor;

        public MonkHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration)
        {
            EmptyColor = new(PluginConfiguration.EmptyColor);
        }

        private Vector2 CalculatePosition(Vector2 position, Vector2 size) => Origin + position - size / 2f;

        protected override void Draw(bool _)
        {
            if (_config.ShowFormsBar)
            {
                DrawFormsBar();
            }

            if (_config.ShowRiddleofEarthBar)
            {
                DrawRiddleOfEarthBar();
            }

            if (_config.ShowPerfectBalanceBar)
            {
                DrawPerfectBalanceBar();
            }

            if (_config.ShowTrueNorthBar)
            {
                DrawTrueNorthBar();
            }

            if (_config.ShowChakraBar)
            {
                DrawChakraGauge();
            }

            if (_config.ShowLeadenFistBar)
            {
                DrawLeadenFistBar();
            }

            if (_config.ShowTwinSnakesBar)
            {
                DrawTwinSnakesBar();
            }

            if (_config.ShowDemolishBar)
            {
                DrawDemolishBar();
            }
        }

        protected override void DrawPrimaryResourceBar() { }

        private void DrawFormsBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var target = PluginInterface.ClientState.LocalPlayer;
            var opoOpoForm = target.StatusEffects.FirstOrDefault(o => o.EffectId == 107);
            var raptorForm = target.StatusEffects.FirstOrDefault(o => o.EffectId == 108);
            var coeurlForm = target.StatusEffects.FirstOrDefault(o => o.EffectId == 109);
            var formlessFist = target.StatusEffects.FirstOrDefault(o => o.EffectId == 2513);

            var opoOpoFormDuration = opoOpoForm.Duration;
            var raptorFormDuration = raptorForm.Duration;
            var coeurlFormDuration = coeurlForm.Duration;
            var formlessFistDuration = formlessFist.Duration;

            var position = CalculatePosition(_config.FormsBarPosition, _config.FormsBarSize);

            var builder = BarBuilder.Create(position, _config.FormsBarSize);
            var maximum = 15f;

            if (opoOpoFormDuration > 0)
            {
                var bar = builder.AddInnerBar(Math.Abs(opoOpoFormDuration), maximum, _config.FormsBarFillColor.Map)
                                 .SetTextMode(BarTextMode.EachChunk)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, "Opo-Opo Form")
                                 .SetBackgroundColor(EmptyColor.Background)
                                 .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }

            if (raptorFormDuration > 0)
            {
                var bar = builder.AddInnerBar(Math.Abs(raptorFormDuration), maximum, _config.FormsBarFillColor.Map)
                                 .SetTextMode(BarTextMode.EachChunk)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, "Raptor Form")
                                 .SetBackgroundColor(EmptyColor.Background)
                                 .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }

            if (coeurlFormDuration > 0)
            {
                var bar = builder.AddInnerBar(Math.Abs(coeurlFormDuration), maximum, _config.FormsBarFillColor.Map)
                                 .SetTextMode(BarTextMode.EachChunk)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, "Coeurl Form")
                                 .SetBackgroundColor(EmptyColor.Background)
                                 .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }

            if (formlessFist.Duration > 0)
            {
                var bar = builder.AddInnerBar(Math.Abs(formlessFist.Duration), maximum, _config.FormsBarFillColor.Map)
                                 .SetTextMode(BarTextMode.EachChunk)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, "Formless Fist")
                                 .SetBackgroundColor(EmptyColor.Background)
                                 .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
            else
            {
                var bar = builder.AddInnerBar(0, maximum, _config.FormsBarFillColor.Map)
                                 .SetBackgroundColor(EmptyColor.Background)
                                 .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
        }

        private void DrawTrueNorthBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var target = PluginInterface.ClientState.LocalPlayer;
            var trueNorth = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1250);
            var trueNorthDuration = trueNorth.Duration;

            Vector2 position = CalculatePosition(_config.TrueNorthBarPosition, _config.TrueNorthBarSize);
            var builder = BarBuilder.Create(position, _config.TrueNorthBarSize);
            var maximum = 10f;

            if (trueNorthDuration > 0)
            {
                var bar = builder.AddInnerBar(Math.Abs(trueNorthDuration), maximum, _config.TrueNorthBarFillColor.Map)
                                 .SetTextMode(BarTextMode.EachChunk)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                                 .SetBackgroundColor(EmptyColor.Background)
                                 .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
            else
            {
                var bar = builder.AddInnerBar(Math.Abs(trueNorthDuration), maximum, _config.TrueNorthBarFillColor.Map)
                                 .SetBackgroundColor(EmptyColor.Background)
                                 .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
        }

        private void DrawPerfectBalanceBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var target = PluginInterface.ClientState.LocalPlayer;
            var perfectBalance = target.StatusEffects.FirstOrDefault(o => o.EffectId == 110);
            var perfectBalanceDuration = perfectBalance.StackCount;

            Vector2 position = CalculatePosition(_config.PerfectBalanceBarPosition, _config.PerfectBalanceBarSize);
            var builder = BarBuilder.Create(position, _config.PerfectBalanceBarSize);
            var maximum = 6f;

            if (perfectBalanceDuration > 0)
            {
                var bar = builder.AddInnerBar(Math.Abs(perfectBalanceDuration), maximum, _config.PerfectBalanceBarFillColor.Map)
                                 .SetVertical(true)
                                 .SetFlipDrainDirection(_config.PerfectBalanceInverted)
                                 .SetTextMode(BarTextMode.EachChunk)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                                 .SetBackgroundColor(EmptyColor.Background)
                                 .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
            else
            {
                var bar = builder.AddInnerBar(Math.Abs(perfectBalanceDuration), maximum, _config.PerfectBalanceBarFillColor.Map)
                                 .SetBackgroundColor(EmptyColor.Background)
                                 .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
        }

        private void DrawRiddleOfEarthBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var target = PluginInterface.ClientState.LocalPlayer;
            var riddleOfEarth = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1179);
            var riddleOfEarthDuration = riddleOfEarth.StackCount;

            Vector2 position = CalculatePosition(_config.RiddleofEarthBarPosition, _config.RiddleofEarthBarSize);
            var builder = BarBuilder.Create(position, _config.RiddleofEarthBarSize);
            var maximum = 3f;

            if (riddleOfEarthDuration > 0)
            {
                var bar = builder.AddInnerBar(Math.Abs(riddleOfEarthDuration), maximum, _config.RiddleofEarthBarFillColor.Map)
                                 .SetTextMode(BarTextMode.EachChunk)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                                 .SetBackgroundColor(EmptyColor.Background)
                                 .SetFlipDrainDirection(_config.RiddleofEarthInverted)
                                 .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
            else
            {
                var bar = builder.AddInnerBar(Math.Abs(riddleOfEarthDuration), maximum, _config.RiddleofEarthBarFillColor.Map)
                                 .SetBackgroundColor(EmptyColor.Background)
                                 .SetFlipDrainDirection(_config.RiddleofEarthInverted == false)
                                 .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
        }

        private void DrawChakraGauge()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<MNKGauge>();

            Vector2 position = CalculatePosition(_config.ChakraBarPosition, _config.ChakraBarSize);
            var bar = BarBuilder.Create(position, _config.ChakraBarSize)
                                .SetChunks(5)
                                .SetChunkPadding(2)
                                .AddInnerBar(gauge.NumChakra, 5, _config.ChakraBarFillColor.Map, EmptyColor.Map)
                                .SetBackgroundColor(EmptyColor.Background)
                                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawTwinSnakesBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var target = PluginInterface.ClientState.LocalPlayer;
            var twinSnakes = target.StatusEffects.FirstOrDefault(o => o.EffectId == 101);
            var twinSnakesDuration = twinSnakes.Duration;

            Vector2 position = CalculatePosition(_config.TwinSnakesBarPosition, _config.TwinSnakesBarSize);
            var builder = BarBuilder.Create(position, _config.TwinSnakesBarSize);
            var maximum = 15f;

            var bar = builder.AddInnerBar(Math.Abs(twinSnakesDuration), maximum, _config.TwinSnakesBarFillColor.Map)
                             .SetTextMode(BarTextMode.EachChunk)
                             .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                             .SetBackgroundColor(EmptyColor.Background)
                             .SetFlipDrainDirection(_config.TwinSnakesBarInverted)
                             .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawLeadenFistBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var target = PluginInterface.ClientState.LocalPlayer;
            var leadenFist = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1861);
            var leadenFistDuration = leadenFist.Duration;

            Vector2 position = CalculatePosition(_config.LeadenFistBarPosition, _config.LeadenFistBarSize);
            var builder = BarBuilder.Create(position, _config.LeadenFistBarSize);
            var maximum = 30f;

            if (leadenFistDuration > 0)
            {
                var bar = builder.AddInnerBar(Math.Abs(leadenFistDuration), maximum, _config.LeadenFistBarFillColor.Map)
                                 .SetVertical(true)
                                 .SetTextMode(BarTextMode.EachChunk)
                                 .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                                 .SetBackgroundColor(EmptyColor.Background)
                                 .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
            else
            {
                var bar = builder.AddInnerBar(Math.Abs(leadenFistDuration), maximum, _config.LeadenFistBarFillColor.Map)
                                 .SetBackgroundColor(EmptyColor.Background)
                                 .Build();

                var drawList = ImGui.GetWindowDrawList();
                bar.Draw(drawList, PluginConfiguration);
            }
        }

        private void DrawDemolishBar()
        {
            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget ?? PluginInterface.ClientState.LocalPlayer;
            var demolish = target.StatusEffects.FirstOrDefault(o => o.EffectId == 246 || o.EffectId == 1309);
            var demolishDuration = demolish.Duration;

            Vector2 position = CalculatePosition(_config.DemolishBarPosition, _config.DemolishBarSize);
            var builder = BarBuilder.Create(position, _config.DemolishBarSize);
            var maximum = 18f;

            var bar = builder.AddInnerBar(Math.Abs(demolishDuration), maximum, _config.DemolishBarFillColor.Map)
                             .SetTextMode(BarTextMode.EachChunk)
                             .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                             .SetBackgroundColor(EmptyColor.Background)
                             .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Melee", 0)]
    [SubSection("Monk", 1)]
    public class MonkHudConfig : PluginConfigObject
    {
        [DragFloat2("Base Position" + "##Monk", min = -4000f, max = 4000f)]
        [Order(0)]
        public Vector2 Position = new(0, 0);
        // public Vector2 Position = new(160, 460);

        #region Demolish Bar
        [Checkbox("Show Demolish Bar")]
        [CollapseControl(5, 0)]
        public bool ShowDemolishBar = true;

        [DragFloat2("Position" + "##Demolish", min = -4000f, max = 4000f)]
        [CollapseWith(0, 0)]
        public Vector2 DemolishBarPosition = new(71, 449);

        [DragFloat2("Size" + "##Demolish", min = 0, max = 4000f)]
        [CollapseWith(5, 0)]
        public Vector2 DemolishBarSize = new(111, 20);

        [ColorEdit4("Color" + "##Demolish")]
        [CollapseWith(10, 0)]
        public PluginConfigColor DemolishBarFillColor = new(new Vector4(246f / 255f, 169f / 255f, 255f / 255f, 100f / 100f));
        #endregion

        #region Chakra Bar
        [Checkbox("Show Chakra Bar")]
        [CollapseControl(10, 1)]
        public bool ShowChakraBar = true;

        [DragFloat2("Position" + "##Chakbra", min = -4000f, max = 4000f)]
        [CollapseWith(0, 1)]
        public Vector2 ChakraBarPosition = new(0, 427);

        [DragFloat2("Size" + "##Chakbra", min = 0, max = 4000f)]
        [CollapseWith(5, 1)]
        public Vector2 ChakraBarSize = new(254, 20);

        [ColorEdit4("Color" + "##Chakbra")]
        [CollapseWith(10, 1)]
        public PluginConfigColor ChakraBarFillColor = new(new Vector4(204f / 255f, 115f / 255f, 0f, 100f / 100f));
        #endregion

        #region Leaden Fist Bar
        [Checkbox("Show Leaden Fist Bar")]
        [CollapseControl(15, 2)]
        public bool ShowLeadenFistBar = true;

        [DragFloat2("Position" + "##LeadenFist", min = -4000f, max = 4000f)]
        [CollapseWith(0, 2)]
        public Vector2 LeadenFistBarPosition = new(0, 449);

        [DragFloat2("Size" + "##LeadenFist", min = 0, max = 4000f)]
        [CollapseWith(5, 2)]
        public Vector2 LeadenFistBarSize = new(28, 20);

        [ColorEdit4("Color" + "##LeadenFist")]
        [CollapseWith(10, 2)]
        public PluginConfigColor LeadenFistBarFillColor = new(new Vector4(255f / 255f, 0f, 0f, 100f / 100f));
        #endregion

        #region Twin Snakes Bar
        [Checkbox("Show Twin Snakes Bar")]
        [CollapseControl(20, 3)]
        public bool ShowTwinSnakesBar = true;

        [DragFloat2("Position" + "##TwinSnakes", min = -4000f, max = 4000f)]
        [CollapseWith(0, 3)]
        public Vector2 TwinSnakesBarPosition = new(-71, 449);

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
        [CollapseControl(25, 4)]
        public bool ShowRiddleofEarthBar = true;

        [DragFloat2("Position" + "##RiddleofEarth", min = -4000f, max = 4000f)]
        [CollapseWith(0, 4)]
        public Vector2 RiddleofEarthBarPosition = new(-69, 405);

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
        [CollapseControl(30, 5)]
        public bool ShowPerfectBalanceBar = true;

        [DragFloat2("Position" + "##PerfectBalance", min = -4000f, max = 4000f)]
        [CollapseWith(0, 5)]
        public Vector2 PerfectBalanceBarPosition = new(0, 405);

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
        [CollapseControl(35, 6)]
        public bool ShowTrueNorthBar = true;

        [DragFloat2("Position" + "##TrueNorth", min = -4000f, max = 4000f)]
        [CollapseWith(0, 6)]
        public Vector2 TrueNorthBarPosition = new(69, 405);

        [DragFloat2("Size" + "##TrueNorth", min = 0, max = 4000f)]
        [CollapseWith(5, 6)]
        public Vector2 TrueNorthBarSize = new(115, 20);

        [ColorEdit4("Color" + "##TrueNorth")]
        [CollapseWith(10, 6)]
        public PluginConfigColor TrueNorthBarFillColor = new(new Vector4(255f / 255f, 225f / 255f, 189f / 255f, 100f / 100f));
        #endregion

        #region Forms
        [Checkbox("Show Forms Bar" + "##Forms")]
        [CollapseControl(40, 7)]
        public bool ShowFormsBar = false;

        [DragFloat2("Position" + "##Forms", min = -4000f, max = 4000f)]
        [CollapseWith(0, 7)]
        public Vector2 FormsBarPosition = new(0, 383);

        [DragFloat2("Size" + "##Forms", min = 0, max = 4000f)]
        [CollapseWith(5, 7)]
        public Vector2 FormsBarSize = new(254, 20);

        [ColorEdit4("Color" + "##Forms")]
        [CollapseWith(10, 7)]
        public PluginConfigColor FormsBarFillColor = new(new Vector4(36f / 255f, 131f / 255f, 255f / 255f, 100f / 100f));
        #endregion
    }
}
