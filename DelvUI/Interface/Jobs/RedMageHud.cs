using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
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
    public class RedMageHud : JobHud
    {
        private new RedMageConfig Config => (RedMageConfig)_config;
        private PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;

        public RedMageHud(string id, RedMageConfig config, string? displayName = null) : base(id, config, displayName)
        {
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.ShowBalanceBar)
            {
                positions.Add(Config.Position + Config.BalanceBarPosition);
                sizes.Add(Config.BalanceBarSize);
            }

            if (Config.ShowWhiteManaBar)
            {
                positions.Add(Config.Position + Config.WhiteManaBarPosition);
                sizes.Add(Config.WhiteManaBarSize);
            }

            if (Config.ShowBlackManaBar)
            {
                positions.Add(Config.Position + Config.BlackManaBarPosition);
                sizes.Add(Config.BlackManaBarSize);
            }

            if (Config.ShowAcceleration)
            {
                positions.Add(Config.Position + Config.AccelerationBarPosition);
                sizes.Add(Config.AccelerationBarSize);
            }

            if (Config.ShowDualCast)
            {
                positions.Add(Config.Position + Config.DualCastPosition);
                sizes.Add(Config.DualCastSize);
            }

            if (Config.ShowVerstoneProcs)
            {
                positions.Add(Config.Position + Config.VerstoneBarPosition);
                sizes.Add(Config.VerstoneBarSize);
            }

            if (Config.ShowVerfireProcs)
            {
                positions.Add(Config.Position + Config.VerfireBarPosition);
                sizes.Add(Config.VerfireBarSize);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            if (Config.ShowBalanceBar)
            {
                DrawBalanceBar(origin);
            }

            if (Config.ShowWhiteManaBar)
            {
                DrawWhiteManaBar(origin);
            }

            if (Config.ShowBlackManaBar)
            {
                DrawBlackManaBar(origin);
            }

            if (Config.ShowAcceleration)
            {
                DrawAccelerationBar(origin, player);
            }

            if (Config.ShowDualCast)
            {
                DrawDualCastBar(origin, player);
            }

            if (Config.ShowVerstoneProcs)
            {
                DrawVerstoneProc(origin, player);
            }

            if (Config.ShowVerfireProcs)
            {
                DrawVerfireProc(origin, player);
            }
        }

        private void DrawBalanceBar(Vector2 origin)
        {
            var gauge = Plugin.JobGauges.Get<RDMGauge>();
            var whiteGauge = (float)Plugin.JobGauges.Get<RDMGauge>().WhiteMana;
            var blackGauge = (float)Plugin.JobGauges.Get<RDMGauge>().BlackMana;
            var scale = gauge.WhiteMana - gauge.BlackMana;

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

            if (Config.OnlyShowBalanceWhenActive && value is 0) { return; }

            var drawList = ImGui.GetWindowDrawList();
            var builder = BarBuilder.Create(position, Config.BalanceBarSize)
                .AddInnerBar(value, 1, color)
                .SetBackgroundColor(EmptyColor.Base);

            builder.Build().Draw(drawList);
        }

        private void DrawWhiteManaBar(Vector2 origin)
        {
            var gauge = (int)Plugin.JobGauges.Get<RDMGauge>().WhiteMana;
            if (Config.OnlyShowWhiteManaWhenActive && gauge is 0) { return; }
            var thresholdRatio = Config.WhiteManaBarInverted ? 0.2f : 0.8f;

            var position = new Vector2(
                origin.X + Config.Position.X + Config.WhiteManaBarPosition.X - Config.BlackManaBarSize.X / 2f,
                origin.Y + Config.Position.Y + Config.WhiteManaBarPosition.Y - Config.WhiteManaBarSize.Y / 2f
            );

            DrawCustomBar(position, Config.WhiteManaBarSize, Config.WhiteManaBarColor, gauge, 100, thresholdRatio, Config.WhiteManaBarInverted, gauge > 0 ? Config.ShowWhiteManaValue : false);
        }

        private void DrawBlackManaBar(Vector2 origin)
        {
            var gauge = (int)Plugin.JobGauges.Get<RDMGauge>().BlackMana;
            if (Config.OnlyShowBlackManaWhenActive && gauge is 0) { return; }
            var thresholdRatio = Config.BlackManaBarInverted ? 0.2f : 0.8f;

            var position = new Vector2(
                origin.X + Config.Position.X + Config.BlackManaBarPosition.X - Config.BlackManaBarSize.X / 2f,
                origin.Y + Config.Position.Y + Config.BlackManaBarPosition.Y - Config.BlackManaBarSize.Y / 2f
            );

            DrawCustomBar(position, Config.BlackManaBarSize, Config.BlackManaBarColor, gauge, 100, thresholdRatio, Config.BlackManaBarInverted, gauge > 0 ? Config.ShowBlackManaValue : false);
        }

        private void DrawAccelerationBar(Vector2 origin, PlayerCharacter player)
        {
            var accelBuff = player.StatusList.Where(o => o.StatusId == 1238);
            int stackCount = accelBuff.Any() ? accelBuff.First().StackCount : 0;
            if (Config.OnlyShowAccelerationWhenActive && stackCount is 0) { return; }

            var position = new Vector2(
                origin.X + Config.Position.X + Config.AccelerationBarPosition.X - Config.AccelerationBarSize.X / 2f,
                origin.Y + Config.Position.Y + Config.AccelerationBarPosition.Y - Config.AccelerationBarSize.Y / 2f
            );

            var bar = BarBuilder.Create(position, Config.AccelerationBarSize)
                                .SetChunks(3)
                                .SetChunkPadding(Config.AccelerationBarPadding)
                                .AddInnerBar(stackCount, 3, Config.AccelerationBarColor, EmptyColor)
                                .SetBackgroundColor(EmptyColor.Base)
                                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        private void DrawDualCastBar(Vector2 origin, PlayerCharacter player)
        {
            var dualCastBuff = player.StatusList.Where(o => o.StatusId is 1249);
            var dualCastDuration = dualCastBuff.Any() ? Math.Abs(dualCastBuff.First().RemainingTime) : 0;
            if (Config.OnlyShowDualcastWhenActive && dualCastDuration is 0) { return; }

            var position = new Vector2(
                origin.X + Config.Position.X + Config.DualCastPosition.X - Config.DualCastSize.X / 2f,
                origin.Y + Config.Position.Y + Config.DualCastPosition.Y - Config.DualCastSize.Y / 2f
            );

            var drawList = ImGui.GetWindowDrawList();
            var builder = BarBuilder.Create(position, Config.DualCastSize)
                .AddInnerBar(dualCastDuration, 15f, Config.DualCastColor)
                .SetBackgroundColor(EmptyColor.Base)
                .SetVertical(Config.SetVerticalDualCastBar)
                .SetFlipDrainDirection(Config.SetInvertedDualCastBar);

            builder.Build().Draw(drawList);
        }

        private void DrawVerstoneProc(Vector2 origin, PlayerCharacter player)
        {
            var verstone = player.StatusList.Where(o => o.StatusId is 1235);
            int duration = verstone.Any() ? (int)Math.Abs(verstone.First().RemainingTime) : 0;

            if (Config.OnlyShowVerstoneWhenActive && duration is 0) { return; }

            var position = new Vector2(
                origin.X + Config.Position.X + Config.VerstoneBarPosition.X - Config.VerstoneBarSize.X / 2f,
                origin.Y + Config.Position.Y + Config.VerstoneBarPosition.Y - Config.VerstoneBarSize.Y / 2f
            );

            DrawCustomBar(position, Config.VerstoneBarSize, Config.VerstoneColor, duration, 30, 0, Config.InvertVerstoneBar, Config.ShowVerstoneText);
        }

        private void DrawVerfireProc(Vector2 origin, PlayerCharacter player)
        {
            var verfire = player.StatusList.Where(o => o.StatusId is 1234);
            int duration = verfire.Any() ? (int)Math.Abs(verfire.First().RemainingTime) : 0;

            if (Config.OnlyShowVerfireWhenActive && duration is 0) { return; }

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
                .AddInnerBar(value, max, color)
                .SetFlipDrainDirection(inverted)
                .SetBackgroundColor(EmptyColor.Background);

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

    [Section("Job Specific Bars")]
    [SubSection("Caster", 0)]
    [SubSection("Red Mage", 1)]
    public class RedMageConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.RDM;
        public new static RedMageConfig DefaultConfig()
        {
            var config = new RedMageConfig();
            config.UseDefaultPrimaryResourceBar = true;
            return config;
        }

        #region balance bar
        [Checkbox("Balance", separator = true)]
        [Order(30)]
        public bool ShowBalanceBar = true;

        [Checkbox("Only Show When Active" + "##Balance")]
        [Order(31, collapseWith = nameof(ShowBalanceBar))]
        public bool OnlyShowBalanceWhenActive = false;

        [DragFloat2("Position" + "##Balance", min = -2000f, max = 2000f)]
        [Order(35, collapseWith = nameof(ShowBalanceBar))]
        public Vector2 BalanceBarPosition = new Vector2(0, -10);

        [DragFloat2("Size" + "##Balance", max = 2000f)]
        [Order(40, collapseWith = nameof(ShowBalanceBar))]
        public Vector2 BalanceBarSize = new Vector2(22, 20);

        [ColorEdit4("Color" + "##Balance")]
        [Order(45, collapseWith = nameof(ShowBalanceBar))]
        public PluginConfigColor BalanceBarColor = new PluginConfigColor(new(195f / 255f, 35f / 255f, 35f / 255f, 100f / 100f));
        #endregion

        #region white mana bar
        [Checkbox("White Mana", separator = true)]
        [Order(50)]
        public bool ShowWhiteManaBar = true;

        [Checkbox("Only Show When Active" + "##WhiteMana")]
        [Order(51, collapseWith = nameof(ShowWhiteManaBar))]
        public bool OnlyShowWhiteManaWhenActive = false;

        [Checkbox("Text" + "##WhiteMana")]
        [Order(55, collapseWith = nameof(ShowWhiteManaBar))]
        public bool ShowWhiteManaValue = true;

        [Checkbox("Inverted" + "##WhiteMana")]
        [Order(60, collapseWith = nameof(ShowWhiteManaBar))]
        public bool WhiteManaBarInverted = true;

        [DragFloat2("Position" + "##WhiteMana", min = -2000f, max = 2000f)]
        [Order(65, collapseWith = nameof(ShowWhiteManaBar))]
        public Vector2 WhiteManaBarPosition = new Vector2(-70, -10);

        [DragFloat2("Size" + "##WhiteMana", max = 2000f)]
        [Order(70, collapseWith = nameof(ShowWhiteManaBar))]
        public Vector2 WhiteManaBarSize = new Vector2(114, 20);

        [ColorEdit4("Color" + "##WhiteMana")]
        [Order(75, collapseWith = nameof(ShowWhiteManaBar))]
        public PluginConfigColor WhiteManaBarColor = new PluginConfigColor(new(221f / 255f, 212f / 255f, 212f / 255f, 100f / 100f));
        #endregion

        #region black mana bar
        [Checkbox("Black Mana", separator = true)]
        [Order(80)]
        public bool ShowBlackManaBar = true;

        [Checkbox("Only Show When Active" + "##BlackMana")]
        [Order(81, collapseWith = nameof(ShowBlackManaBar))]
        public bool OnlyShowBlackManaWhenActive = false;

        [Checkbox("Text" + "##BlackMana")]
        [Order(85, collapseWith = nameof(ShowBlackManaBar))]
        public bool ShowBlackManaValue = true;

        [Checkbox("Inverted" + "##BlackMana")]
        [Order(90, collapseWith = nameof(ShowBlackManaBar))]
        public bool BlackManaBarInverted = false;

        [DragFloat2("Position" + "##BlackMana", min = -2000f, max = 2000f)]
        [Order(95, collapseWith = nameof(ShowBlackManaBar))]
        public Vector2 BlackManaBarPosition = new Vector2(70, -10);

        [DragFloat2("Size" + "##BlackMana", max = 2000f)]
        [Order(100, collapseWith = nameof(ShowBlackManaBar))]
        public Vector2 BlackManaBarSize = new Vector2(114, 20);

        [ColorEdit4("Color" + "##BlackMana")]
        [Order(105, collapseWith = nameof(ShowBlackManaBar))]
        public PluginConfigColor BlackManaBarColor = new PluginConfigColor(new(60f / 255f, 81f / 255f, 197f / 255f, 100f / 100f));
        #endregion

        #region acceleration
        [Checkbox("Acceleration", separator = true)]
        [Order(110)]
        public bool ShowAcceleration = true;

        [Checkbox("Only Show When Active" + "##Acceleration")]
        [Order(111, collapseWith = nameof(ShowAcceleration))]
        public bool OnlyShowAccelerationWhenActive = false;

        [DragFloat2("Position" + "##Acceleration", min = -2000f, max = 2000f)]
        [Order(115, collapseWith = nameof(ShowAcceleration))]
        public Vector2 AccelerationBarPosition = new Vector2(0, -28);

        [DragFloat2("Size" + "##Acceleration", max = 2000f)]
        [Order(120, collapseWith = nameof(ShowAcceleration))]
        public Vector2 AccelerationBarSize = new Vector2(254, 12);

        [DragInt("Spacing" + "##Acceleration", max = 1000)]
        [Order(125, collapseWith = nameof(ShowAcceleration))]
        public int AccelerationBarPadding = 2;

        [ColorEdit4("Color" + "##Acceleration")]
        [Order(130, collapseWith = nameof(ShowAcceleration))]
        public PluginConfigColor AccelerationBarColor = new PluginConfigColor(new(194f / 255f, 74f / 255f, 74f / 255f, 100f / 100f));
        #endregion

        #region dualcast
        [Checkbox("Dualcast", separator = true)]
        [Order(140)]
        public bool ShowDualCast = true;

        [Checkbox("Only Show When Active" + "##DualCast")]
        [Order(141, collapseWith = nameof(ShowDualCast))]
        public bool OnlyShowDualcastWhenActive = false;

        [Checkbox("Inverted" + "##DualCast")]
        [Order(142, collapseWith = nameof(ShowDualCast))]
        public bool SetInvertedDualCastBar = false;

        [Checkbox("Vertical" + "##DualCast")]
        [Order(143, collapseWith = nameof(ShowDualCast))]
        public bool SetVerticalDualCastBar = false;

        [DragFloat2("Position" + "##DualCast", min = -2000f, max = 2000f)]
        [Order(145, collapseWith = nameof(ShowDualCast))]
        public Vector2 DualCastPosition = new Vector2(0, -44);

        [DragFloat2("Size" + "##DualCast", max = 2000f)]
        [Order(150, collapseWith = nameof(ShowDualCast))]
        public Vector2 DualCastSize = new Vector2(18, 16);

        [ColorEdit4("Color" + "##DualCast")]
        [Order(155, collapseWith = nameof(ShowDualCast))]
        public PluginConfigColor DualCastColor = new PluginConfigColor(new(204f / 255f, 17f / 255f, 255f / 95f, 100f / 100f));
        #endregion

        #region verstone
        [Checkbox("Verstone", separator = true)]
        [Order(160)]
        public bool ShowVerstoneProcs = true;

        [Checkbox("Only Show When Active" + "##Verstone")]
        [Order(161, collapseWith = nameof(ShowVerstoneProcs))]
        public bool OnlyShowVerstoneWhenActive = false;

        [Checkbox("Timer" + "##Verstone")]
        [Order(165, collapseWith = nameof(ShowVerstoneProcs))]
        public bool ShowVerstoneText = true;

        [Checkbox("Inverted" + "##Verstone")]
        [Order(170, collapseWith = nameof(ShowVerstoneProcs))]
        public bool InvertVerstoneBar = true;

        [DragFloat2("Position" + "##Verstone", min = -2000, max = 2000f)]
        [Order(175, collapseWith = nameof(ShowVerstoneProcs))]
        public Vector2 VerstoneBarPosition = new Vector2(-69, -44);

        [DragFloat2("Size" + "##Verstone", max = 2000f)]
        [Order(180, collapseWith = nameof(ShowVerstoneProcs))]
        public Vector2 VerstoneBarSize = new Vector2(116, 16);

        [ColorEdit4("Color" + "##Verstone")]
        [Order(185, collapseWith = nameof(ShowVerstoneProcs))]
        public PluginConfigColor VerstoneColor = new PluginConfigColor(new(228f / 255f, 188f / 255f, 145 / 255f, 90f / 100f));
        #endregion

        #region verfire
        [Checkbox("Verfire", separator = true)]
        [Order(190)]
        public bool ShowVerfireProcs = true;

        [Checkbox("Only Show When Active" + "##Verfire")]
        [Order(191, collapseWith = nameof(ShowVerfireProcs))]
        public bool OnlyShowVerfireWhenActive = false;

        [Checkbox("Timer" + "##Verfire")]
        [Order(195, collapseWith = nameof(ShowVerfireProcs))]
        public bool ShowVerfireText = true;

        [Checkbox("Inverted" + "##Verfire")]
        [Order(200, collapseWith = nameof(ShowVerfireProcs))]
        public bool InvertVerfireBar = false;

        [DragFloat2("Position" + "##Verfire", min = -2000, max = 2000f)]
        [Order(205, collapseWith = nameof(ShowVerfireProcs))]
        public Vector2 VerfireBarPosition = new Vector2(69, -44);

        [DragFloat2("Size" + "##Verfire", max = 2000f)]
        [Order(210, collapseWith = nameof(ShowVerfireProcs))]
        public Vector2 VerfireBarSize = new Vector2(116, 16);

        [ColorEdit4("Color" + "##Verfire")]
        [Order(215, collapseWith = nameof(ShowVerfireProcs))]
        public PluginConfigColor VerfireColor = new PluginConfigColor(new(238f / 255f, 119f / 255f, 17 / 255f, 90f / 100f));
        #endregion
    }
}
