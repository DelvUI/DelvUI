using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using ImGuiNET;
using System;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface
{
    public class RedMageHudWindow : HudWindow
    {
        private RedMageHudConfig _config => (RedMageHudConfig)ConfigurationManager.GetInstance().GetConfiguration(new RedMageHudConfig());

        public RedMageHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        public override uint JobId => Jobs.RDM;

        private float OriginX => CenterX + _config.Position.X;
        private float OriginY => CenterY + YOffset + _config.Position.Y;


        protected override void Draw(bool _)
        {
            DrawBalanceBar();
            DrawWhiteManaBar();
            DrawBlackManaBar();
            DrawAccelerationBar();

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
            var scale = (float)actor.CurrentMp / actor.MaxMp;
            var cursorPos = new Vector2(
                OriginX - _config.ManaBarSize.X / 2 + _config.ManaBarOffset.X,
                OriginY - _config.ManaBarSize.Y + _config.ManaBarOffset.Y
            );

            var color = _config.ManaBarColor;
            if (_config.ShowManaThresholdMarker && actor.CurrentMp < _config.ManaThresholdValue)
            {
                color = _config.ManaBarBelowThresholdColor;
            }

            // bar
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + _config.ManaBarSize, color.Background);

            if (scale > 0)
            {
                drawList.AddRectFilledMultiColor(
                    cursorPos,
                    cursorPos + new Vector2(Math.Max(1, _config.ManaBarSize.X * scale), _config.ManaBarSize.Y),
                    color.LeftGradient,
                    color.RightGradient,
                    color.RightGradient,
                    color.LeftGradient
                );
            }

            drawList.AddRect(cursorPos, cursorPos + _config.ManaBarSize, 0xFF000000);

            // threshold
            if (_config.ShowManaThresholdMarker)
            {
                var position = new Vector2(cursorPos.X + _config.ManaThresholdValue / 10000f * _config.ManaBarSize.X - 3, cursorPos.Y);
                var size = new Vector2(2, _config.ManaBarSize.Y);
                drawList.AddRect(position, position + size, 0xFF000000);
            }

            // text
            if (!_config.ShowManaValue)
            {
                return;
            }

            var mana = PluginInterface.ClientState.LocalPlayer.CurrentMp;
            var text = $"{mana,0}";
            var textSize = ImGui.CalcTextSize(text);
            DrawOutlinedText(text, new Vector2(cursorPos.X + 2, OriginY + _config.Position.Y - _config.ManaBarSize.Y / 2f - textSize.Y / 2f + 2));
        }

        private void DrawBalanceBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<RDMGauge>();
            var whiteGauge = (float)PluginInterface.ClientState.JobGauges.Get<RDMGauge>().WhiteGauge;
            var blackGauge = (float)PluginInterface.ClientState.JobGauges.Get<RDMGauge>().BlackGauge;
            var scale = gauge.WhiteGauge - gauge.BlackGauge;

            var cursorPos = new Vector2(
                OriginX - _config.BalanceBarSize.X / 2f + _config.BalanceBarOffset.X,
                OriginY + _config.BalanceBarOffset.Y
            );

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + _config.BalanceBarSize, 0x88000000);

            PluginConfigColor color = null;

            if (whiteGauge >= 80 && blackGauge >= 80)
            {
                color = _config.BalanceBarColor;
            }
            else if (scale >= 30)
            {
                color = _config.WhiteManaBarColor;
            }
            else if (scale <= -30)
            {
                color = _config.BlackManaBarColor;
            }

            if (color != null)
            {
                drawList.AddRectFilledMultiColor(
                    cursorPos,
                    cursorPos + _config.BalanceBarSize,
                    color.LeftGradient,
                    color.RightGradient,
                    color.RightGradient,
                    color.LeftGradient
                );
            }

            drawList.AddRect(cursorPos, cursorPos + _config.BalanceBarSize, 0xFF000000);
        }

        private void DrawWhiteManaBar()
        {
            var gauge = (int)PluginInterface.ClientState.JobGauges.Get<RDMGauge>().WhiteGauge;
            var scale = gauge / 100f;

            var position = new Vector2(
                OriginX + _config.WhiteManaBarOffset.X,
                OriginY + _config.WhiteManaBarOffset.Y
            );

            DrawManaBar(position, _config.WhiteManaBarSize, _config.WhiteManaBarColor, gauge, scale, _config.WhiteManaBarInverted, _config.ShowWhiteManaValue);
        }

        private void DrawBlackManaBar()
        {
            var gauge = (int)PluginInterface.ClientState.JobGauges.Get<RDMGauge>().BlackGauge;
            var scale = gauge / 100f;

            var position = new Vector2(
                OriginX + _config.BlackManaBarOffset.X,
                OriginY + _config.BlackManaBarOffset.Y
            );

            DrawManaBar(position, _config.WhiteManaBarSize, _config.BlackManaBarColor, gauge, scale, _config.BlackManaBarInverted, _config.ShowBlackManaValue);
        }

        private void DrawManaBar(Vector2 position, Vector2 size, PluginConfigColor color, int value, float scale, bool inverted, bool showText)
        {
            var origin = inverted ? new Vector2(position.X - size.X, position.Y) : position;

            // bar
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(origin, origin + size, color.Background);

            // fill
            if (scale > 0)
            {
                var barStartPos = inverted ? new Vector2(origin.X + size.X * (1 - scale), origin.Y) : origin;

                drawList.AddRectFilledMultiColor(
                    barStartPos,
                    barStartPos + new Vector2(Math.Max(1, size.X * scale), size.Y),
                    color.LeftGradient,
                    color.RightGradient,
                    color.RightGradient,
                    color.LeftGradient
                );
            }

            // border
            drawList.AddRect(origin, origin + size, 0xFF000000);

            // threshold
            var thresholdRatio = inverted ? 0.2f : 0.8f;
            var thresholdPos = new Vector2(origin.X + size.X * thresholdRatio, origin.Y);
            drawList.AddRect(thresholdPos, thresholdPos + new Vector2(2, size.Y), 0xFF000000);

            // text
            if (!showText)
            {
                return;
            }

            var text = $"{value}";
            var textSize = ImGui.CalcTextSize(text);
            var textPos = inverted ? new Vector2(origin.X + size.X - 10 - textSize.X, origin.Y - 2) : new Vector2(origin.X + 10, origin.Y - 2);
            DrawOutlinedText(text, textPos);
        }

        private void DrawAccelerationBar()
        {
            var totalWidth = _config.AccelerationBarSize.X * 3 + _config.HorizontalPadding * 2;
            var cursorPos = new Vector2(
                OriginX - totalWidth / 2 + _config.AccelerationBarOffset.X,
                OriginY + _config.AccelerationBarOffset.Y
            );

            var accelBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 1238);

            var drawList = ImGui.GetWindowDrawList();

            for (var i = 1; i <= 3; i++)
            {
                drawList.AddRectFilled(cursorPos, cursorPos + _config.AccelerationBarSize, _config.AccelerationBarColor.Background);

                if (accelBuff.StackCount >= i)
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos,
                        cursorPos + _config.AccelerationBarSize,
                        _config.AccelerationBarColor.LeftGradient,
                        _config.AccelerationBarColor.RightGradient,
                        _config.AccelerationBarColor.RightGradient,
                        _config.AccelerationBarColor.LeftGradient
                    );
                }

                drawList.AddRect(cursorPos, cursorPos + _config.AccelerationBarSize, 0xFF000000);

                cursorPos.X = cursorPos.X + _config.AccelerationBarSize.X + _config.HorizontalPadding;
            }
        }

        private void DrawDualCastBar()
        {
            var cursorPos = new Vector2(
                OriginX - _config.DualCastSize.X / 2f + _config.DualCastOffset.X,
                OriginY + _config.DualCastOffset.Y
            );

            var dualCastBuff = Math.Abs(PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 1249).Duration);

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + _config.DualCastSize, _config.DualCastColor.Background);

            if (dualCastBuff > 0)
            {
                drawList.AddRectFilledMultiColor(
                    cursorPos,
                    cursorPos + _config.DualCastSize,
                        _config.AccelerationBarColor.LeftGradient,
                        _config.AccelerationBarColor.RightGradient,
                        _config.AccelerationBarColor.RightGradient,
                        _config.AccelerationBarColor.LeftGradient
                );
            }

            drawList.AddRect(cursorPos, cursorPos + _config.DualCastSize, 0xFF000000);
        }

        private void DrawVerstoneProc()
        {
            var duration = Math.Abs(PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 1235).Duration);

            if (duration == 0)
            {
                return;
            }

            var position = new Vector2(
                OriginX - _config.HorizontalPadding - _config.DualCastSize.X,
                OriginY + _config.DualCastOffset.Y + _config.DualCastSize.Y / 2f + _config.ProcsHeight / 2f
            );

            var scale = duration / 30f;
            DrawTimerBar(position, scale, _config.ProcsHeight, _config.VerstoneColor, true);
        }

        private void DrawVerfireProc()
        {
            var duration = Math.Abs(PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 1234).Duration);

            if (duration == 0)
            {
                return;
            }

            var position = new Vector2(
                OriginX + _config.HorizontalPadding + _config.DualCastSize.X,
                OriginY + _config.DualCastOffset.Y + _config.DualCastSize.Y / 2f - _config.ProcsHeight / 2f
            );

            var scale = duration / 30f;
            DrawTimerBar(position, scale, _config.ProcsHeight, _config.VerfireColor, false);
        }

        private void DrawTimerBar(Vector2 position, float scale, float height, PluginConfigColor color, bool inverted)
        {
            var drawList = ImGui.GetWindowDrawList();
            var size = new Vector2((_config.ManaBarSize.X / 2f - _config.DualCastSize.X - _config.HorizontalPadding * 2f) * scale, height);
            size.X = Math.Max(1, size.X);

            var startPoint = inverted ? position - size : position;
            var leftColor = inverted ? color.RightGradient : color.LeftGradient;
            var rightColor = inverted ? color.LeftGradient : color.RightGradient;

            drawList.AddRectFilledMultiColor(
                startPoint,
                startPoint + size,
                leftColor,
                rightColor,
                rightColor,
                leftColor
            );
        }
    }


    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Caster", 0)]
    [SubSection("Red Mage", 1)]
    public class RedMageHudConfig : PluginConfigObject
    {
        [DragFloat2("Base Offset", min = -4000f, max = 4000f)]
        public Vector2 Position = new Vector2(0, -2);

        [DragInt("Horizontal Padding", max = 1000)]
        public int HorizontalPadding = 2;

        [DragFloat2("Mana Bar Size", max = 2000f)]
        public Vector2 ManaBarSize = new Vector2(253, 20);

        [DragFloat2("Mana Bar Offset", min = -2000f, max = 2000f)]
        public Vector2 ManaBarOffset = new Vector2(0, 0);

        [Checkbox("Show Mana Value")] public bool ShowManaValue = true;
        [Checkbox("Show Mana Threshold Marker")] public bool ShowManaThresholdMarker = true;

        [DragInt("Mana Threshold Marker Value", max = 10000)]
        public int ManaThresholdValue = 2600;


        [DragFloat2("Balance Bar Offset", min = -2000f, max = 2000f)]
        public Vector2 BalanceBarOffset = new Vector2(0, -42);

        [DragFloat2("Balance Bar Size", max = 2000f)]
        public Vector2 BalanceBarSize = new Vector2(21, 20);


        [DragFloat2("White Mana Bar Offset", min = -2000f, max = 2000f)]
        public Vector2 WhiteManaBarOffset = new Vector2(-13, -42);

        [DragFloat2("White Mana Bar Size", max = 2000f)]
        public Vector2 WhiteManaBarSize = new Vector2(114, 20);

        [Checkbox("Show White Mana Value")] public bool ShowWhiteManaValue = true;
        [Checkbox("Invert White Mana Bar")] public bool WhiteManaBarInverted = true;


        [DragFloat2("Black Mana Bar Offset", min = -2000f, max = 2000f)]
        public Vector2 BlackManaBarOffset = new Vector2(13, -42);

        [DragFloat2("Black Mana Bar Size", max = 2000f)]
        public Vector2 BlackManaBarSize = new Vector2(114, 20);

        [Checkbox("Show Black Mana Value")] public bool ShowBlackManaValue = true;
        [Checkbox("Invert Black Mana Bar")] public bool BlackManaBarInverted = false;


        [DragFloat2("Acceleration Bar Offset", min = -2000f, max = 2000f)]
        public Vector2 AccelerationBarOffset = new Vector2(0, -56);

        [DragFloat2("Acceleration Size", max = 2000f)]
        public Vector2 AccelerationBarSize = new Vector2(83, 12);


        [Checkbox("Show Dualcast")] public bool ShowDualCast = true;
        [DragFloat2("Dualcast Offset", min = -2000f, max = 2000f)]
        public Vector2 DualCastOffset = new Vector2(0, -74);

        [DragFloat2("Dualcast Size", max = 2000f)]
        public Vector2 DualCastSize = new Vector2(16, 16);


        [Checkbox("Show Verstone Procs")] public bool ShowVerstoneProcs = true;
        [Checkbox("Show Verfire Procs")] public bool ShowVerfireProcs = true;

        [DragInt("Procs Height", max = 1000)]
        public int ProcsHeight = 7;


        [ColorEdit4("Mana Bar Color")]
        public PluginConfigColor ManaBarColor = new PluginConfigColor(new(0f / 255f, 142f / 255f, 254f / 255f, 100f / 100f));

        [ColorEdit4("Mana Bar Below Threshold Color")]
        public PluginConfigColor ManaBarBelowThresholdColor = new PluginConfigColor(new(210f / 255f, 33f / 255f, 33f / 255f, 100f / 100f));

        [ColorEdit4("White Mana Bar Color")]
        public PluginConfigColor WhiteManaBarColor = new PluginConfigColor(new(221f / 255f, 212f / 255f, 212f / 255f, 100f / 100f));

        [ColorEdit4("Black Mana Bar Color")]
        public PluginConfigColor BlackManaBarColor = new PluginConfigColor(new(60f / 255f, 81f / 255f, 197f / 255f, 100f / 100f));

        [ColorEdit4("Balance Bar Color")]
        public PluginConfigColor BalanceBarColor = new PluginConfigColor(new(195f / 255f, 35f / 255f, 35f / 255f, 100f / 100f));

        [ColorEdit4("Acceleration Bar Color")]
        public PluginConfigColor AccelerationBarColor = new PluginConfigColor(new(194f / 255f, 74f / 255f, 74f / 255f, 100f / 100f));

        [ColorEdit4("Dualcast Color")]
        public PluginConfigColor DualCastColor = new PluginConfigColor(new(204f / 255f, 17f / 255f, 255f / 95f, 100f / 100f));

        [ColorEdit4("Verstone Color")]
        public PluginConfigColor VerstoneColor = new PluginConfigColor(new(228f / 255f, 188f / 255f, 145 / 255f, 90f / 100f));

        [ColorEdit4("Verfire Color")]
        public PluginConfigColor VerfireColor = new PluginConfigColor(new(238f / 255f, 119f / 255f, 17 / 255f, 90f / 100f));
    }
}
