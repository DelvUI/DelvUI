using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Interface.Bars;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface
{
    public class RedMageHudWindow : HudWindow
    {
        private RedMageHudConfig _config => (RedMageHudConfig)ConfigurationManager.GetInstance().GetConfiguration(new RedMageHudConfig());

        public RedMageHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        public override uint JobId => Jobs.RDM;

        private Dictionary<string, uint> EmptyColor => PluginConfiguration.MiscColorMap["empty"];


        protected override void Draw(bool _)
        {
            DrawBalanceBar();
            DrawWhiteManaBar();
            DrawBlackManaBar();

            if (_config.ShowAcceleration)
            {
                DrawAccelerationBar();
            }

            if (_config.ShowDualCast)
            {
                DrawDualCastBar();
            }

            if (_config.ShowVerstoneProcs)
            {
                DrawVerstoneProc();
            }

            if (_config.ShowVerfireProcs)
            {
                DrawVerfireProc();
            }
        }

        protected override void DrawPrimaryResourceBar()
        {
            var actor = PluginInterface.ClientState.LocalPlayer;

            var position = new Vector2(
                CenterX + _config.Position.X + _config.ManaBarPosition.X - _config.ManaBarSize.X / 2f,
                CenterY + _config.Position.Y + _config.ManaBarPosition.Y - _config.ManaBarSize.Y / 2f
            );

            var drawList = ImGui.GetWindowDrawList();
            var builder = BarBuilder.Create(position, _config.ManaBarSize)
                .AddInnerBar(actor.CurrentMp, actor.MaxMp, _config.ManaBarColor.Map)
                .SetBackgroundColor(EmptyColor["background"]);

            // text
            if (_config.ShowManaValue)
            {
                builder.SetTextMode(BarTextMode.Single);
                builder.SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            builder.Build().Draw(drawList, PluginConfiguration);

            // threshold marker
            if (_config.ShowManaThresholdMarker)
            {
                var pos = new Vector2(
                    position.X + _config.ManaThresholdValue / 10000f * _config.ManaBarSize.X,
                    position.Y + _config.ManaBarSize.Y
                );
                var size = new Vector2(3, _config.ManaBarSize.Y);

                drawList.AddRectFilledMultiColor(
                    pos,
                    pos - size,
                    0xFF000000,
                    0x00000000,
                    0x00000000,
                    0xFF000000
                );
            }
        }

        private void DrawBalanceBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<RDMGauge>();
            var whiteGauge = (float)PluginInterface.ClientState.JobGauges.Get<RDMGauge>().WhiteGauge;
            var blackGauge = (float)PluginInterface.ClientState.JobGauges.Get<RDMGauge>().BlackGauge;
            var scale = gauge.WhiteGauge - gauge.BlackGauge;

            var position = new Vector2(
                CenterX + _config.Position.X + _config.BalanceBarPosition.X - _config.BalanceBarSize.X / 2f,
                CenterY + _config.Position.Y + _config.BalanceBarPosition.Y - _config.BalanceBarSize.Y / 2f
            );

            PluginConfigColor color = _config.BalanceBarColor;
            var value = 0;

            if (whiteGauge >= 80 && blackGauge >= 80)
            {
                value = 1;
            }
            else if (scale >= 30)
            {
                color = _config.WhiteManaBarColor;
                value = 1;
            }
            else if (scale <= -30)
            {
                color = _config.BlackManaBarColor;
                value = 1;
            }

            var drawList = ImGui.GetWindowDrawList();
            var builder = BarBuilder.Create(position, _config.BalanceBarSize)
                .AddInnerBar(value, 1, color.Map)
                .SetBackgroundColor(EmptyColor["background"]);

            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawWhiteManaBar()
        {
            var gauge = (int)PluginInterface.ClientState.JobGauges.Get<RDMGauge>().WhiteGauge;
            var thresholdRatio = _config.WhiteManaBarInverted ? 0.2f : 0.8f;

            var position = new Vector2(
                CenterX + _config.Position.X + _config.WhiteManaBarPosition.X - _config.BlackManaBarSize.X / 2f,
                CenterY + _config.Position.Y + _config.WhiteManaBarPosition.Y - _config.WhiteManaBarSize.Y / 2f
            );

            DrawCustomBar(position, _config.WhiteManaBarSize, _config.WhiteManaBarColor, gauge, 100, thresholdRatio, _config.WhiteManaBarInverted, _config.ShowWhiteManaValue);
        }

        private void DrawBlackManaBar()
        {
            var gauge = (int)PluginInterface.ClientState.JobGauges.Get<RDMGauge>().BlackGauge;
            var thresholdRatio = _config.BlackManaBarInverted ? 0.2f : 0.8f;

            var position = new Vector2(
                CenterX + _config.Position.X + _config.BlackManaBarPosition.X - _config.BlackManaBarSize.X / 2f,
                CenterY + _config.Position.Y + _config.BlackManaBarPosition.Y - _config.BlackManaBarSize.Y / 2f
            );

            DrawCustomBar(position, _config.BlackManaBarSize, _config.BlackManaBarColor, gauge, 100, thresholdRatio, _config.BlackManaBarInverted, _config.ShowBlackManaValue);
        }

        private void DrawAccelerationBar()
        {
            var accelBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 1238);

            var position = new Vector2(
                CenterX + _config.Position.X + _config.AccelerationBarPosition.X - _config.AccelerationBarSize.X / 2f,
                CenterY + _config.Position.Y + _config.AccelerationBarPosition.Y - _config.AccelerationBarSize.Y / 2f
            );

            var bar = BarBuilder.Create(position, _config.AccelerationBarSize)
                                .SetChunks(3)
                                .SetChunkPadding(_config.AccelerationBarPadding)
                                .AddInnerBar(accelBuff.StackCount, 3, _config.AccelerationBarColor.Map, EmptyColor)
                                .SetBackgroundColor(EmptyColor["background"])
                                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawDualCastBar()
        {
            var dualCastBuff = Math.Abs(PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 1249).Duration);
            var value = dualCastBuff > 0 ? 1 : 0;

            var position = new Vector2(
                CenterX + _config.Position.X + _config.DualCastPosition.X - _config.DualCastSize.X / 2f,
                CenterY + _config.Position.Y + _config.DualCastPosition.Y - _config.DualCastSize.Y / 2f
            );

            var drawList = ImGui.GetWindowDrawList();
            var builder = BarBuilder.Create(position, _config.DualCastSize)
                .AddInnerBar(value, 1, _config.DualCastColor.Map)
                .SetBackgroundColor(EmptyColor["background"]);

            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawVerstoneProc()
        {
            var duration = (int)Math.Abs(PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 1235).Duration);

            var position = new Vector2(
                CenterX + _config.Position.X + _config.VerstoneBarPosition.X - _config.VerstoneBarSize.X / 2f,
                CenterY + _config.Position.Y + _config.VerstoneBarPosition.Y - _config.VerstoneBarSize.Y / 2f
            );

            DrawCustomBar(position, _config.VerstoneBarSize, _config.VerstoneColor, duration, 30, 0, _config.InvertVerstoneBar, _config.ShowVerstoneText);
        }

        private void DrawVerfireProc()
        {
            var duration = (int)Math.Abs(PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 1234).Duration);

            var position = new Vector2(
                CenterX + _config.Position.X + _config.VerfireBarPosition.X - _config.VerfireBarSize.X / 2f,
                CenterY + _config.Position.Y + _config.VerfireBarPosition.Y - _config.VerfireBarSize.Y / 2f
            );

            DrawCustomBar(position, _config.VerfireBarSize, _config.VerfireColor, duration, 30, 0, _config.InvertVerfireBar, _config.ShowVerfireText);
        }

        private void DrawCustomBar(
            Vector2 position,
            Vector2 size,
            PluginConfigColor color,
            int value,
            int max,
            float thresholdRatio,
            bool inverted,
            bool showText)
        {
            var builder = BarBuilder.Create(position, size)
                .AddInnerBar(value, max, color.Map)
                .SetFlipDrainDirection(inverted);

            if (showText)
            {
                var textPos = inverted ? BarTextPosition.CenterRight : BarTextPosition.CenterLeft;
                builder.SetTextMode(BarTextMode.Single);
                builder.SetText(textPos, BarTextType.Current);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);

            // threshold marker
            if (thresholdRatio <= 0)
            {
                return;
            }

            var pos = new Vector2(
                position.X + size.X * thresholdRatio,
                position.Y + size.Y
            );

            drawList.AddRectFilledMultiColor(
                pos,
                pos - new Vector2(3, size.Y),
                0xFF000000,
                0x00000000,
                0x00000000,
                0xFF000000
            );
        }
    }


    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Caster", 0)]
    [SubSection("Red Mage", 1)]
    public class RedMageHudConfig : PluginConfigObject
    {
        [DragFloat2("Base Position", min = -4000f, max = 4000f)]
        [Order(0)]
        public Vector2 Position = new Vector2(0, 0);

        #region mana bar
        [DragFloat2("Mana Bar Size", max = 2000f)]
        [Order(5)]
        public Vector2 ManaBarSize = new Vector2(254, 20);

        [DragFloat2("Mana Bar Position", min = -2000f, max = 2000f)]
        [Order(10)]
        public Vector2 ManaBarPosition = new Vector2(0, 448);

        [Checkbox("Show Mana Value")]
        [Order(15)]
        public bool ShowManaValue = true;

        [CollapseControl(20, 0)]
        [Checkbox("Show Mana Threshold Marker")] public bool ShowManaThresholdMarker = true;

        [DragInt("Mana Threshold Marker Value", max = 10000)]
        [CollapseWith(0, 0)]
        public int ManaThresholdValue = 2600;

        [ColorEdit4("Mana Bar Color")]
        [Order(20)]
        public PluginConfigColor ManaBarColor = new PluginConfigColor(new(0f / 255f, 142f / 255f, 254f / 255f, 100f / 100f));
        #endregion

        #region balance bar
        [DragFloat2("Balance Bar Position", min = -2000f, max = 2000f)]
        [Order(25)]
        public Vector2 BalanceBarPosition = new Vector2(0, 426);

        [DragFloat2("Balance Bar Size", max = 2000f)]
        [Order(30)]
        public Vector2 BalanceBarSize = new Vector2(22, 20);

        [ColorEdit4("Balance Bar Color")]
        [Order(35)]
        public PluginConfigColor BalanceBarColor = new PluginConfigColor(new(195f / 255f, 35f / 255f, 35f / 255f, 100f / 100f));
        #endregion

        #region white mana bar
        [DragFloat2("White Mana Bar Position", min = -2000f, max = 2000f)]
        [Order(40)]
        public Vector2 WhiteManaBarPosition = new Vector2(-70, 426);

        [DragFloat2("White Mana Bar Size", max = 2000f)]
        [Order(45)]
        public Vector2 WhiteManaBarSize = new Vector2(114, 20);

        [Checkbox("Show White Mana Value")]
        [Order(50)]
        public bool ShowWhiteManaValue = true;

        [Checkbox("Invert White Mana Bar")]
        [Order(55)]
        public bool WhiteManaBarInverted = true;

        [ColorEdit4("White Mana Bar Color")]
        [Order(60)]
        public PluginConfigColor WhiteManaBarColor = new PluginConfigColor(new(221f / 255f, 212f / 255f, 212f / 255f, 100f / 100f));
        #endregion

        #region black mana bar
        [DragFloat2("Black Mana Bar Position", min = -2000f, max = 2000f)]
        [Order(65)]
        public Vector2 BlackManaBarPosition = new Vector2(70, 426);

        [DragFloat2("Black Mana Bar Size", max = 2000f)]
        [Order(70)]
        public Vector2 BlackManaBarSize = new Vector2(114, 20);

        [Checkbox("Show Black Mana Value")]
        [Order(75)]
        public bool ShowBlackManaValue = true;

        [Checkbox("Invert Black Mana Bar")]
        [Order(80)]
        public bool BlackManaBarInverted = false;

        [ColorEdit4("Black Mana Bar Color")]
        [Order(85)]
        public PluginConfigColor BlackManaBarColor = new PluginConfigColor(new(60f / 255f, 81f / 255f, 197f / 255f, 100f / 100f));
        #endregion

        #region acceleration
        [Checkbox("Show Acceleration Bar")]
        [CollapseControl(90, 1)]
        public bool ShowAcceleration = true;

        [DragFloat2("Acceleration Bar Position", min = -2000f, max = 2000f)]
        [CollapseWith(0, 1)]
        public Vector2 AccelerationBarPosition = new Vector2(0, 408);

        [DragFloat2("Acceleration Size", max = 2000f)]
        [CollapseWith(5, 1)]
        public Vector2 AccelerationBarSize = new Vector2(254, 12);

        [DragInt("Acceleration Padding", max = 1000)]
        [CollapseWith(10, 1)]
        public int AccelerationBarPadding = 2;

        [ColorEdit4("Acceleration Bar Color")]
        [CollapseWith(15, 1)]
        public PluginConfigColor AccelerationBarColor = new PluginConfigColor(new(194f / 255f, 74f / 255f, 74f / 255f, 100f / 100f));
        #endregion

        #region dualcast
        [Checkbox("Show Dualcast")]
        [CollapseControl(95, 2)]
        public bool ShowDualCast = true;

        [DragFloat2("Dualcast Position", min = -2000f, max = 2000f)]
        [CollapseWith(0, 2)]
        public Vector2 DualCastPosition = new Vector2(0, 392);

        [DragFloat2("Dualcast Size", max = 2000f)]
        [CollapseWith(5, 2)]
        public Vector2 DualCastSize = new Vector2(18, 16);

        [ColorEdit4("Dualcast Color")]
        [CollapseWith(10, 2)]
        public PluginConfigColor DualCastColor = new PluginConfigColor(new(204f / 255f, 17f / 255f, 255f / 95f, 100f / 100f));
        #endregion

        #region verstone
        [Checkbox("Show Verstone Procs")]
        [CollapseControl(100, 3)]
        public bool ShowVerstoneProcs = true;

        [Checkbox("Show Verstone Text")]
        [CollapseWith(0, 3)]
        public bool ShowVerstoneText = true;

        [Checkbox("Invert Verstone Bar")]
        [CollapseWith(5, 3)]
        public bool InvertVerstoneBar = true;

        [DragFloat2("Verstone Bar Position", min = -2000, max = 2000f)]
        [CollapseWith(10, 3)]
        public Vector2 VerstoneBarPosition = new Vector2(-69, 392);

        [DragFloat2("Verstone Bar Size", max = 2000f)]
        [CollapseWith(15, 3)]
        public Vector2 VerstoneBarSize = new Vector2(116, 16);

        [ColorEdit4("Verstone Color")]
        [CollapseWith(20, 3)]
        public PluginConfigColor VerstoneColor = new PluginConfigColor(new(228f / 255f, 188f / 255f, 145 / 255f, 90f / 100f));
        #endregion

        #region verfire
        [Checkbox("Show Verfire Procs")]
        [CollapseControl(105, 4)]
        public bool ShowVerfireProcs = true;

        [Checkbox("Show Verfire Text")]
        [CollapseWith(0, 4)]
        public bool ShowVerfireText = true;

        [Checkbox("Invert Verfire Bar")]
        [CollapseWith(5, 4)]
        public bool InvertVerfireBar = false;

        [DragFloat2("Verfire Bar Position", min = -2000, max = 2000f)]
        [CollapseWith(10, 4)]
        public Vector2 VerfireBarPosition = new Vector2(69, 392);

        [DragFloat2("Verfire Bar Size", max = 2000f)]
        [CollapseWith(15, 4)]
        public Vector2 VerfireBarSize = new Vector2(116, 16);

        [ColorEdit4("Verfire Color")]
        [CollapseWith(20, 4)]
        public PluginConfigColor VerfireColor = new PluginConfigColor(new(238f / 255f, 119f / 255f, 17 / 255f, 90f / 100f));
        #endregion
    }
}
