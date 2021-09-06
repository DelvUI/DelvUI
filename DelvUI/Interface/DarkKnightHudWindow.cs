using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Config;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using DelvUI.Config.Attributes;

namespace DelvUI.Interface
{
    public class DarkKnightHudWindow : HudWindow
    {

        public override uint JobId => Jobs.DRK;

        private readonly DarkKnightHudConfig _config = (DarkKnightHudConfig)ConfigurationManager.GetInstance().GetConfiguration(new DarkKnightHudConfig());
        private Vector2 Origin => new Vector2(CenterX + _config.Position.X, CenterY + _config.Position.Y);

        private PluginConfigColor EmptyColor;

        private PluginConfigColor PartialFillColor;

        public DarkKnightHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration)
        {
            EmptyColor = new PluginConfigColor(PluginConfiguration.EmptyColor);
            PartialFillColor = new PluginConfigColor(PluginConfiguration.PartialFillColor);
        }

        protected override void Draw(bool _)
        {
            if (_config.ShowManaBar)
            {
                DrawManaBar();
            }

            if (_config.ShowBloodGauge)
            {
                DrawBloodGauge();
            }

            if (_config.ShowBuffBar)
            {
                DrawBuffBar();
            }

            if (_config.ShowLivingShadowBar)
            {
                DrawLivingShadowBar();
            }
        }

        protected override void DrawPrimaryResourceBar() { }

        private void DrawManaBar()
        {

            PluginLog.Log(EmptyColor.ToString());

            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");

            var darkArtsBuff = PluginInterface.ClientState.JobGauges.Get<DRKGauge>().HasDarkArts();

            var actor = PluginInterface.ClientState.LocalPlayer;
            var barWidth = (_config.ManaBarSize.X - _config.ManaBarSpacing * 2) / 3.0f;
            var barSize = new Vector2(barWidth, _config.ManaBarSize.Y);
            var xPos = CenterX - XOffset + _config.ManaBarPosition.X;
            var yPos = CenterY + YOffset + _config.ManaBarPosition.Y;
            var cursorPos = new Vector2(xPos, yPos);
            const int chunkSize = 3000;

            var drawList = ImGui.GetWindowDrawList();

            void DrawManaChunks(int index = 1)
            {
                if (index > 3)
                {
                    return;
                }

                var mana = Math.Min(actor.CurrentMp, chunkSize * index);

                if (index == 2)
                {
                    mana = Math.Max(mana - chunkSize, 0);
                }
                else if (index == 3)
                {
                    mana = Math.Max(mana - chunkSize * 2, 0);
                }

                if (index > 1)
                {
                    cursorPos = new Vector2(cursorPos.X + barWidth + _config.ManaBarSpacing, cursorPos.Y);
                }

                if (darkArtsBuff)
                {
                    var glowPosition = new Vector2(cursorPos.X - 1, cursorPos.Y - 1);
                    var glowSize = new Vector2(barSize.X + 2, barSize.Y + 2);
                    var glowColor = ImGui.ColorConvertFloat4ToU32(_config.DarkArtsColor.Vector.AdjustColor(+0.2f));

                    drawList.AddRect(glowPosition, glowPosition + glowSize, glowColor);
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, _config.DarkArtsColor.Background);

                    drawList.AddRectFilledMultiColor(
                        cursorPos,
                        cursorPos + new Vector2(barSize.X * mana / chunkSize, barSize.Y),
                        _config.DarkArtsColor.LeftGradient,
                        _config.DarkArtsColor.RightGradient,
                        _config.DarkArtsColor.RightGradient,
                        _config.DarkArtsColor.LeftGradient
                    );
                }
                else
                {
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);

                    drawList.AddRectFilledMultiColor(
                        cursorPos,
                        cursorPos + new Vector2(barSize.X * mana / chunkSize, barSize.Y),
                        _config.ManaColor.LeftGradient,
                        _config.ManaColor.RightGradient,
                        _config.ManaColor.RightGradient,
                        _config.ManaColor.LeftGradient
                    );
                }

                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                DrawManaChunks(index + 1);
            }

            DrawManaChunks();

            if (_config.ShowManaBarOverflow && actor.CurrentMp > 9000)
            {
                var over9000 = 9000 - actor.CurrentMp;
                cursorPos = new Vector2(cursorPos.X + barWidth - 1, cursorPos.Y);
                var inverseOffset = cursorPos + new Vector2(barSize.X / 10 * over9000 / _config.ManaBarSize.X, barSize.Y);

                drawList.AddRectFilledMultiColor(
                    cursorPos,
                    inverseOffset,
                    _config.DarkArtsColor.LeftGradient,
                    _config.DarkArtsColor.RightGradient,
                    _config.DarkArtsColor.RightGradient,
                    _config.DarkArtsColor.LeftGradient
                );

                drawList.AddRect(cursorPos, inverseOffset, 0xFF000000);
            }
        }

        private void DrawBloodGauge()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<DRKGauge>();

            var padding = _config.BloodGaugeSplit ? _config.BloodGaugePadding : 0;
            var barWidth = (_config.BloodGaugeSize.X - padding) / 2;
            var xPos = CenterX - XOffset + _config.BloodGaugePosition.X;
            var yPos = CenterY + YOffset + _config.BloodGaugePosition.Y;

            var cursorPos = new Vector2(xPos, yPos);
            var thresholdCursorPos = new Vector2(cursorPos.X + barWidth, cursorPos.Y);

            const int chunkSize = 50;

            var barSize = _config.BloodGaugeSize;
            var barSplitSize = new Vector2(barWidth, _config.BloodGaugeSize.Y);

            var drawList = ImGui.GetWindowDrawList();

            if (!_config.BloodGaugeSplit)
            {
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, EmptyColor.Background);
            }

            void DrawBloodChunks(int index = 1)
            {
                if (index > 2)
                {
                    return;
                }

                var blood = Math.Min(gauge.Blood, chunkSize * index);
                var scale = (float)blood / chunkSize;

                var gradientLeft = index == 1 ? _config.BloodColorLeft.LeftGradient : _config.BloodColorRight.LeftGradient;
                var gradientRight = index == 1 ? _config.BloodColorLeft.RightGradient : _config.BloodColorRight.RightGradient;

                if (index == 2)
                {
                    blood = Math.Max(blood - chunkSize, 0);
                    scale = (float)blood / chunkSize;
                    cursorPos = new Vector2(cursorPos.X + barWidth + padding, cursorPos.Y);
                }

                if (_config.BloodGaugeSplit)
                {
                    drawList.AddRectFilled(cursorPos, cursorPos + barSplitSize, EmptyColor.Background);
                }

                if (scale >= 1.0f)
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos,
                        cursorPos + new Vector2(barWidth * scale, _config.BloodGaugeSize.Y),
                        gradientLeft,
                        gradientRight,
                        gradientRight,
                        gradientLeft
                    );
                }
                else
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos,
                        cursorPos + new Vector2(barWidth * scale, _config.BloodGaugeSize.Y),
                        PartialFillColor.LeftGradient,
                        PartialFillColor.RightGradient,
                        PartialFillColor.RightGradient,
                        PartialFillColor.LeftGradient
                    );
                }

                if (_config.BloodGaugeSplit)
                {
                    drawList.AddRect(cursorPos, cursorPos + barSplitSize, 0xFF000000);
                }

                DrawBloodChunks(index + 1);
            }

            DrawBloodChunks();

            if (!_config.BloodGaugeSplit)
            {
                var cursor = new Vector2(xPos, yPos);
                drawList.AddRect(cursor, cursor + barSize, 0xFF000000);

                if (_config.DrawBloodGaugeThreshold)
                {
                    drawList.AddLine(thresholdCursorPos, new Vector2(thresholdCursorPos.X, thresholdCursorPos.Y + _config.BloodGaugeSize.Y), 0x88000000);
                }
            }
        }

        private void DrawBuffBar()
        {
            var bloodWeaponBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 742);
            var deliriumBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1972);

            var buffBarBarWidth = _config.BuffBarSize.X;
            var xPos = CenterX - XOffset + _config.BuffBarPosition.X;
            var yPos = CenterY + YOffset + _config.BuffBarPosition.Y;
            var cursorPos = new Vector2(xPos, yPos);
            var buffBarBarHeight = _config.BuffBarSize.Y;
            var barSize = new Vector2(buffBarBarWidth, buffBarBarHeight);

            var drawList = ImGui.GetWindowDrawList();

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, EmptyColor.Background);

            if (bloodWeaponBuff.Any() && deliriumBuff.Any())
            {
                var innerBarHeight = buffBarBarHeight / 2;
                barSize = new Vector2(buffBarBarWidth, innerBarHeight);

                var bloodWeaponDuration = Math.Abs(bloodWeaponBuff.First().Duration);
                var deliriumDuration = Math.Abs(deliriumBuff.First().Duration);

                drawList.AddRectFilledMultiColor(
                    cursorPos,
                    cursorPos + new Vector2(barSize.X / 10f * bloodWeaponDuration, barSize.Y),
                    _config.BloodWeaponColor.LeftGradient,
                    _config.BloodWeaponColor.RightGradient,
                    _config.BloodWeaponColor.RightGradient,
                    _config.BloodWeaponColor.LeftGradient
                );

                drawList.AddRectFilledMultiColor(
                    cursorPos + new Vector2(0.0f, innerBarHeight),
                    cursorPos + new Vector2(barSize.X / 10f * deliriumDuration, barSize.Y * 2f),
                    _config.DeliriumColor.LeftGradient,
                    _config.DeliriumColor.RightGradient,
                    _config.DeliriumColor.RightGradient,
                    _config.DeliriumColor.LeftGradient
                );

                var bloodWeaponDurationText = bloodWeaponDuration == 0 ? "" : Math.Ceiling(bloodWeaponDuration).ToString(CultureInfo.InvariantCulture);
                DrawOutlinedText(bloodWeaponDurationText, new Vector2(cursorPos.X + 5f, cursorPos.Y - 2f), _config.BloodWeaponColor.Vector, new Vector4(0f, 0f, 0f, 1f));

                var deliriumDurationText = deliriumDuration == 0 ? "" : Math.Ceiling(deliriumDuration).ToString(CultureInfo.InvariantCulture);
                DrawOutlinedText(deliriumDurationText, new Vector2(cursorPos.X + 27f, cursorPos.Y - 2f), _config.DeliriumColor.Vector, new Vector4(0f, 0f, 0f, 1f));

                barSize = new Vector2(buffBarBarWidth, buffBarBarHeight);
            }
            else if (bloodWeaponBuff.Any())
            {
                var bloodWeaponDuration = Math.Abs(bloodWeaponBuff.First().Duration);

                drawList.AddRectFilledMultiColor(
                    cursorPos,
                    cursorPos + new Vector2(barSize.X / 10f * bloodWeaponDuration, barSize.Y),
                    _config.BloodWeaponColor.LeftGradient,
                    _config.BloodWeaponColor.RightGradient,
                    _config.BloodWeaponColor.RightGradient,
                    _config.BloodWeaponColor.LeftGradient
                );

                var bloodWeaponDurationText = bloodWeaponDuration == 0 ? "" : Math.Ceiling(bloodWeaponDuration).ToString();
                DrawOutlinedText(bloodWeaponDurationText, new Vector2(cursorPos.X + 5f, cursorPos.Y - 2f), _config.BloodWeaponColor.Vector, new Vector4(0f, 0f, 0f, 1f));
            }
            else if (deliriumBuff.Any())
            {
                var deliriumDuration = Math.Abs(deliriumBuff.First().Duration);

                drawList.AddRectFilledMultiColor(
                    cursorPos,
                    cursorPos + new Vector2(barSize.X / 10f * deliriumDuration, barSize.Y),
                    _config.DeliriumColor.LeftGradient,
                    _config.DeliriumColor.RightGradient,
                    _config.DeliriumColor.RightGradient,
                    _config.DeliriumColor.LeftGradient
                );

                var deliriumDurationText = deliriumDuration == 0 ? "" : Math.Ceiling(deliriumDuration).ToString(CultureInfo.InvariantCulture);
                DrawOutlinedText(deliriumDurationText, new Vector2(cursorPos.X + 5f, cursorPos.Y - 2f), _config.DeliriumColor.Vector, new Vector4(0f, 0f, 0f, 1f));
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }

        private void DrawLivingShadowBar()
        {
            var actor = PluginInterface.ClientState.LocalPlayer;
            var shadowTimeRemaining = PluginInterface.ClientState.JobGauges.Get<DRKGauge>().ShadowTimeRemaining / 100; // ms
            var livingShadow = actor.Level >= 80 && shadowTimeRemaining is > 0 and <= 24;

            var barWidth = _config.LivingShadowSize.X;
            var xPos = CenterX - XOffset + _config.LivingShadowPosition.X;
            var yPos = CenterY + YOffset + _config.LivingShadowPosition.X;
            var cursorPos = new Vector2(xPos, yPos);
            var barSize = new Vector2(barWidth, _config.LivingShadowSize.Y);

            var drawList = ImGui.GetWindowDrawList();

            float duration = 0;
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, EmptyColor.Background);

            if (livingShadow)
            {
                duration = Math.Abs(shadowTimeRemaining);

                drawList.AddRectFilledMultiColor(
                    cursorPos,
                    cursorPos + new Vector2(barSize.X / 24 * duration, barSize.Y),
                    _config.LivingShadowColor.LeftGradient,
                    _config.LivingShadowColor.RightGradient,
                    _config.LivingShadowColor.RightGradient,
                    _config.LivingShadowColor.LeftGradient
                );
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            var durationText = duration != 0 ? Math.Round(duration).ToString(CultureInfo.InvariantCulture) : "";
            var textSize = ImGui.CalcTextSize(durationText);
            DrawOutlinedText(durationText, new Vector2(cursorPos.X + _config.LivingShadowSize.X / 2f - textSize.X / 2f, cursorPos.Y - 2));
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Tank", 0)]
    [SubSection("Dark Knight", 1)]
    public class DarkKnightHudConfig : PluginConfigObject
    {
        [DragFloat2("Base offset", min = -4000f, max = 4000f)]
        [Order(0)]
        public Vector2 Position = new Vector2(0, 415);

        /* Mana Bar */
        [Checkbox("Show Mana Bar")]
        [CollapseControl(5, 0)]
        public bool ShowManaBar = true;

        [Checkbox("Show Mana Bar Overflow")]
        [CollapseWith(0, 0)]
        public bool ShowManaBarOverflow = false;

        [DragFloat2("Mana Bar Position", min = -4000f, max = 4000f)]
        [CollapseWith(5, 0)]
        public Vector2 ManaBarPosition = new Vector2(0, 0);

        [DragFloat2("Mana Bar Size", min = 0, max = 4000f)]
        [CollapseWith(10, 0)]
        public Vector2 ManaBarSize = new Vector2(254, 10);

        [DragInt("Mana Bar Padding", min = 0)]
        [CollapseWith(15, 0)]
        public int ManaBarSpacing = 1;

        [ColorEdit4("Mana Color")]
        [CollapseWith(20, 0)]
        public PluginConfigColor ManaColor = new(new Vector4(0f / 255f, 142f / 255f, 254f / 255f, 100f / 100f));

        [ColorEdit4("Dark Arts Buff Color")]
        [CollapseWith(25, 0)]
        public PluginConfigColor DarkArtsColor = new(new Vector4(210f / 255f, 33f / 255f, 33f / 255f, 100f / 100f));

        /* Blood Gauge */
        [Checkbox("Show Blood Gauge")]
        [CollapseControl(10, 1)]
        public bool ShowBloodGauge = true;

        [Checkbox("Split Blood Gauge")]
        [CollapseWith(0, 1)]
        public bool BloodGaugeSplit = false;

        [Checkbox("Draw Blood Gauge Threshold")]
        [CollapseWith(5, 1)]
        public bool DrawBloodGaugeThreshold = false;

        [DragFloat2("Blood Gauge Position", min = -4000f, max = 4000f)]
        [CollapseWith(10, 1)]
        public Vector2 BloodGaugePosition = new Vector2(0, 12);

        [DragFloat2("Blood Gauge Size", min = 0, max = 4000f)]
        [CollapseWith(15, 1)]
        public Vector2 BloodGaugeSize = new Vector2(254, 10);

        [DragInt("Blood Gauge Padding", min = 0)]
        [CollapseWith(20, 1)]
        public int BloodGaugePadding = 2;

        [ColorEdit4("Blood Color Left")]
        [CollapseWith(25, 1)]
        public PluginConfigColor BloodColorLeft = new(new Vector4(196f / 255f, 20f / 255f, 122f / 255f, 100f / 100f));

        [ColorEdit4("Blood Color Right")]
        [CollapseWith(30, 1)]
        public PluginConfigColor BloodColorRight = new(new Vector4(216f / 255f, 0f / 255f, 73f / 255f, 100f / 100f));

        /* Buff Bar */
        [Checkbox("Show Buff Bar")]
        [CollapseControl(15, 2)]
        public bool ShowBuffBar = true;

        [DragFloat2("Buff Bar Position", min = -4000f, max = 4000f)]
        [CollapseWith(0, 2)]
        public Vector2 BuffBarPosition = new Vector2(0, 24);

        [DragFloat2("Buff Bar Size", min = 0, max = 4000f)]
        [CollapseWith(5, 2)]
        public Vector2 BuffBarSize = new Vector2(254, 20);

        [DragInt("Buff Bar Padding", min = 0)]
        [CollapseWith(10, 2)]
        public int BuffBarPadding = 2;

        [ColorEdit4("Blood Weapon Color")]
        [CollapseWith(15, 2)]
        public PluginConfigColor BloodWeaponColor = new(new Vector4(160f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Delirium Color")]
        [CollapseWith(20, 2)]
        public PluginConfigColor DeliriumColor = new(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        /* Living Shadow */
        [Checkbox("Show Living Shadow Bar")]
        [CollapseControl(20, 3)]
        public bool ShowLivingShadowBar = true;

        [DragFloat2("Living Shadow Position", min = -4000f, max = 4000f)]
        [CollapseWith(0, 3)]
        public Vector2 LivingShadowPosition = new Vector2(0, 46);

        [DragFloat2("Living Shadow Size", min = 0, max = 4000f)]
        [CollapseWith(5, 3)]
        public Vector2 LivingShadowSize = new Vector2(254, 20);

        [DragInt("Living Shadow Padding", min = 0)]
        [CollapseWith(10, 3)]
        public int LivingShadowPadding = 2;

        [ColorEdit4("Living Shadow Color")]
        [CollapseWith(15, 3)]
        public PluginConfigColor LivingShadowColor = new(new Vector4(225f / 255f, 105f / 255f, 205f / 255f, 100f / 100f));
    }
}
