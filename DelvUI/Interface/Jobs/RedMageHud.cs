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
    public class RedMageHud : JobHud
    {
        private new RedMageConfig Config => (RedMageConfig)_config;
        private Dictionary<string, uint> EmptyColor => GlobalColors.Instance.EmptyColor.Map;

        public RedMageHud(string id, RedMageConfig config, string displayName = null) : base(id, config, displayName)
        {

        }

        public override void DrawChildren(Vector2 origin)
        {
            DrawManaBar(origin);
            DrawBalanceBar(origin);
            DrawWhiteManaBar(origin);
            DrawBlackManaBar(origin);

            if (Config.ShowAcceleration)
            {
                DrawAccelerationBar(origin);
            }

            if (Config.ShowDualCast)
            {
                DrawDualCastBar(origin);
            }

            if (Config.ShowVerstoneProcs)
            {
                DrawVerstoneProc(origin);
            }

            if (Config.ShowVerfireProcs)
            {
                DrawVerfireProc(origin);
            }
        }

        protected void DrawManaBar(Vector2 origin)
        {
            var actor = Plugin.ClientState.LocalPlayer;

            var position = new Vector2(
                origin.X + Config.Position.X + Config.ManaBarPosition.X - Config.ManaBarSize.X / 2f,
                origin.Y + Config.Position.Y + Config.ManaBarPosition.Y - Config.ManaBarSize.Y / 2f
            );

            var drawList = ImGui.GetWindowDrawList();
            var builder = BarBuilder.Create(position, Config.ManaBarSize)
                .AddInnerBar(actor.CurrentMp, actor.MaxMp, Config.ManaBarColor.Map)
                .SetBackgroundColor(EmptyColor["background"]);

            // text
            if (Config.ShowManaValue)
            {
                builder.SetTextMode(BarTextMode.Single);
                builder.SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            builder.Build().Draw(drawList);

            // threshold marker
            if (Config.ShowManaThresholdMarker)
            {
                var pos = new Vector2(
                    position.X + Config.ManaThresholdValue / 10000f * Config.ManaBarSize.X,
                    position.Y + Config.ManaBarSize.Y
                );
                var size = new Vector2(3, Config.ManaBarSize.Y);

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

        private void DrawBalanceBar(Vector2 origin)
        {
            var gauge = Plugin.JobGauges.Get<RDMGauge>();
            var whiteGauge = (float)Plugin.JobGauges.Get<RDMGauge>().WhiteGauge;
            var blackGauge = (float)Plugin.JobGauges.Get<RDMGauge>().BlackGauge;
            var scale = gauge.WhiteGauge - gauge.BlackGauge;

            var position = new Vector2(
                origin.X + Config.Position.X + Config.BalanceBarPosition.X - Config.BalanceBarSize.X / 2f,
                origin.Y + Config.Position.Y + Config.BalanceBarPosition.Y - Config.BalanceBarSize.Y / 2f
            );

            PluginConfigColor color = Config.BalanceBarColor;
            var value = 0;

            if (whiteGauge >= 80 && blackGauge >= 80)
            {
                value = 1;
            }
            else if (scale >= 30)
            {
                color = Config.WhiteManaBarColor;
                value = 1;
            }
            else if (scale <= -30)
            {
                color = Config.BlackManaBarColor;
                value = 1;
            }

            var drawList = ImGui.GetWindowDrawList();
            var builder = BarBuilder.Create(position, Config.BalanceBarSize)
                .AddInnerBar(value, 1, color.Map)
                .SetBackgroundColor(EmptyColor["background"]);

            builder.Build().Draw(drawList);
        }

        private void DrawWhiteManaBar(Vector2 origin)
        {
            var gauge = (int)Plugin.JobGauges.Get<RDMGauge>().WhiteGauge;
            var thresholdRatio = Config.WhiteManaBarInverted ? 0.2f : 0.8f;

            var position = new Vector2(
                origin.X + Config.Position.X + Config.WhiteManaBarPosition.X - Config.BlackManaBarSize.X / 2f,
                origin.Y + Config.Position.Y + Config.WhiteManaBarPosition.Y - Config.WhiteManaBarSize.Y / 2f
            );

            DrawCustomBar(position, Config.WhiteManaBarSize, Config.WhiteManaBarColor, gauge, 100, thresholdRatio, Config.WhiteManaBarInverted, Config.ShowWhiteManaValue);
        }

        private void DrawBlackManaBar(Vector2 origin)
        {
            var gauge = (int)Plugin.JobGauges.Get<RDMGauge>().BlackGauge;
            var thresholdRatio = Config.BlackManaBarInverted ? 0.2f : 0.8f;

            var position = new Vector2(
                origin.X + Config.Position.X + Config.BlackManaBarPosition.X - Config.BlackManaBarSize.X / 2f,
                origin.Y + Config.Position.Y + Config.BlackManaBarPosition.Y - Config.BlackManaBarSize.Y / 2f
            );

            DrawCustomBar(position, Config.BlackManaBarSize, Config.BlackManaBarColor, gauge, 100, thresholdRatio, Config.BlackManaBarInverted, Config.ShowBlackManaValue);
        }

        private void DrawAccelerationBar(Vector2 origin)
        {
            var accelBuff = Plugin.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 1238);

            var position = new Vector2(
                origin.X + Config.Position.X + Config.AccelerationBarPosition.X - Config.AccelerationBarSize.X / 2f,
                origin.Y + Config.Position.Y + Config.AccelerationBarPosition.Y - Config.AccelerationBarSize.Y / 2f
            );

            var bar = BarBuilder.Create(position, Config.AccelerationBarSize)
                                .SetChunks(3)
                                .SetChunkPadding(Config.AccelerationBarPadding)
                                .AddInnerBar(accelBuff.StackCount, 3, Config.AccelerationBarColor.Map, EmptyColor)
                                .SetBackgroundColor(EmptyColor["background"])
                                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        private void DrawDualCastBar(Vector2 origin)
        {
            var dualCastBuff = Math.Abs(Plugin.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 1249).Duration);
            var value = dualCastBuff > 0 ? 1 : 0;

            var position = new Vector2(
                origin.X + Config.Position.X + Config.DualCastPosition.X - Config.DualCastSize.X / 2f,
                origin.Y + Config.Position.Y + Config.DualCastPosition.Y - Config.DualCastSize.Y / 2f
            );

            var drawList = ImGui.GetWindowDrawList();
            var builder = BarBuilder.Create(position, Config.DualCastSize)
                .AddInnerBar(value, 1, Config.DualCastColor.Map)
                .SetBackgroundColor(EmptyColor["background"]);

            builder.Build().Draw(drawList);
        }

        private void DrawVerstoneProc(Vector2 origin)
        {
            var duration = (int)Math.Abs(Plugin.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 1235).Duration);

            var position = new Vector2(
                origin.X + Config.Position.X + Config.VerstoneBarPosition.X - Config.VerstoneBarSize.X / 2f,
                origin.Y + Config.Position.Y + Config.VerstoneBarPosition.Y - Config.VerstoneBarSize.Y / 2f
            );

            DrawCustomBar(position, Config.VerstoneBarSize, Config.VerstoneColor, duration, 30, 0, Config.InvertVerstoneBar, Config.ShowVerstoneText);
        }

        private void DrawVerfireProc(Vector2 origin)
        {
            var duration = (int)Math.Abs(Plugin.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 1234).Duration);

            var position = new Vector2(
                origin.X + Config.Position.X + Config.VerfireBarPosition.X - Config.VerfireBarSize.X / 2f,
                origin.Y + Config.Position.Y + Config.VerfireBarPosition.Y - Config.VerfireBarSize.Y / 2f
            );

            DrawCustomBar(position, Config.VerfireBarSize, Config.VerfireColor, duration, 30, 0, Config.InvertVerfireBar, Config.ShowVerfireText);
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
            builder.Build().Draw(drawList);

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
    public class RedMageConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.RDM;
        public new static RedMageConfig DefaultConfig() { return new RedMageConfig(); }

        #region mana bar
        [DragFloat2("Mana Bar Size", max = 2000f)]
        [Order(30)]
        public Vector2 ManaBarSize = new Vector2(254, 20);

        [DragFloat2("Mana Bar Position", min = -2000f, max = 2000f)]
        [Order(35)]
        public Vector2 ManaBarPosition = new Vector2(0, -10);

        [Checkbox("Show Mana Value")]
        [Order(40)]
        public bool ShowManaValue = true;

        [CollapseControl(45, 0)]
        [Checkbox("Show Mana Threshold Marker")] public bool ShowManaThresholdMarker = true;

        [DragInt("Mana Threshold Marker Value", max = 10000)]
        [CollapseWith(0, 0)]
        public int ManaThresholdValue = 2600;

        [ColorEdit4("Mana Bar Color")]
        [Order(45)]
        public PluginConfigColor ManaBarColor = new PluginConfigColor(new(0f / 255f, 142f / 255f, 254f / 255f, 100f / 100f));
        #endregion

        #region balance bar
        [DragFloat2("Balance Bar Position", min = -2000f, max = 2000f)]
        [Order(50)]
        public Vector2 BalanceBarPosition = new Vector2(0, -32);

        [DragFloat2("Balance Bar Size", max = 2000f)]
        [Order(55)]
        public Vector2 BalanceBarSize = new Vector2(22, 20);

        [ColorEdit4("Balance Bar Color")]
        [Order(60)]
        public PluginConfigColor BalanceBarColor = new PluginConfigColor(new(195f / 255f, 35f / 255f, 35f / 255f, 100f / 100f));
        #endregion

        #region white mana bar
        [DragFloat2("White Mana Bar Position", min = -2000f, max = 2000f)]
        [Order(65)]
        public Vector2 WhiteManaBarPosition = new Vector2(-70, -32);

        [DragFloat2("White Mana Bar Size", max = 2000f)]
        [Order(70)]
        public Vector2 WhiteManaBarSize = new Vector2(114, 20);

        [Checkbox("Show White Mana Value")]
        [Order(75)]
        public bool ShowWhiteManaValue = true;

        [Checkbox("Invert White Mana Bar")]
        [Order(80)]
        public bool WhiteManaBarInverted = true;

        [ColorEdit4("White Mana Bar Color")]
        [Order(85)]
        public PluginConfigColor WhiteManaBarColor = new PluginConfigColor(new(221f / 255f, 212f / 255f, 212f / 255f, 100f / 100f));
        #endregion

        #region black mana bar
        [DragFloat2("Black Mana Bar Position", min = -2000f, max = 2000f)]
        [Order(90)]
        public Vector2 BlackManaBarPosition = new Vector2(70, -32);

        [DragFloat2("Black Mana Bar Size", max = 2000f)]
        [Order(95)]
        public Vector2 BlackManaBarSize = new Vector2(114, 20);

        [Checkbox("Show Black Mana Value")]
        [Order(100)]
        public bool ShowBlackManaValue = true;

        [Checkbox("Invert Black Mana Bar")]
        [Order(105)]
        public bool BlackManaBarInverted = false;

        [ColorEdit4("Black Mana Bar Color")]
        [Order(110)]
        public PluginConfigColor BlackManaBarColor = new PluginConfigColor(new(60f / 255f, 81f / 255f, 197f / 255f, 100f / 100f));
        #endregion

        #region acceleration
        [Checkbox("Show Acceleration Bar")]
        [CollapseControl(115, 1)]
        public bool ShowAcceleration = true;

        [DragFloat2("Acceleration Bar Position", min = -2000f, max = 2000f)]
        [CollapseWith(0, 1)]
        public Vector2 AccelerationBarPosition = new Vector2(0, -50);

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
        [CollapseControl(120, 2)]
        public bool ShowDualCast = true;

        [DragFloat2("Dualcast Position", min = -2000f, max = 2000f)]
        [CollapseWith(0, 2)]
        public Vector2 DualCastPosition = new Vector2(0, -66);

        [DragFloat2("Dualcast Size", max = 2000f)]
        [CollapseWith(5, 2)]
        public Vector2 DualCastSize = new Vector2(18, 16);

        [ColorEdit4("Dualcast Color")]
        [CollapseWith(10, 2)]
        public PluginConfigColor DualCastColor = new PluginConfigColor(new(204f / 255f, 17f / 255f, 255f / 95f, 100f / 100f));
        #endregion

        #region verstone
        [Checkbox("Show Verstone Procs")]
        [CollapseControl(125, 3)]
        public bool ShowVerstoneProcs = true;

        [Checkbox("Show Verstone Text")]
        [CollapseWith(0, 3)]
        public bool ShowVerstoneText = true;

        [Checkbox("Invert Verstone Bar")]
        [CollapseWith(5, 3)]
        public bool InvertVerstoneBar = true;

        [DragFloat2("Verstone Bar Position", min = -2000, max = 2000f)]
        [CollapseWith(10, 3)]
        public Vector2 VerstoneBarPosition = new Vector2(-69, -66);

        [DragFloat2("Verstone Bar Size", max = 2000f)]
        [CollapseWith(15, 3)]
        public Vector2 VerstoneBarSize = new Vector2(116, 16);

        [ColorEdit4("Verstone Color")]
        [CollapseWith(20, 3)]
        public PluginConfigColor VerstoneColor = new PluginConfigColor(new(228f / 255f, 188f / 255f, 145 / 255f, 90f / 100f));
        #endregion

        #region verfire
        [Checkbox("Show Verfire Procs")]
        [CollapseControl(130, 4)]
        public bool ShowVerfireProcs = true;

        [Checkbox("Show Verfire Text")]
        [CollapseWith(0, 4)]
        public bool ShowVerfireText = true;

        [Checkbox("Invert Verfire Bar")]
        [CollapseWith(5, 4)]
        public bool InvertVerfireBar = false;

        [DragFloat2("Verfire Bar Position", min = -2000, max = 2000f)]
        [CollapseWith(10, 4)]
        public Vector2 VerfireBarPosition = new Vector2(69, -66);

        [DragFloat2("Verfire Bar Size", max = 2000f)]
        [CollapseWith(15, 4)]
        public Vector2 VerfireBarSize = new Vector2(116, 16);

        [ColorEdit4("Verfire Color")]
        [CollapseWith(20, 4)]
        public PluginConfigColor VerfireColor = new PluginConfigColor(new(238f / 255f, 119f / 255f, 17 / 255f, 90f / 100f));
        #endregion
    }
}
