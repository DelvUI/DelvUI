using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;
using DelvUI.Helpers;

namespace DelvUI.Interface {
    public class DarkKnightHudWindow : HudWindow {
        public override uint JobId => 32;

        private new int XOffset => PluginConfiguration.DRKBaseXOffset;
        private new int YOffset => PluginConfiguration.DRKBaseYOffset;

        private bool ManaBarEnabled => PluginConfiguration.DRKManaBarEnabled;
        private bool ManaBarOverflowEnabled => PluginConfiguration.DRKManaBarOverflowEnabled;
        private int ManaBarHeight => PluginConfiguration.DRKManaBarHeight;
        private int ManaBarWidth => PluginConfiguration.DRKManaBarWidth;
        private int ManaBarPadding => PluginConfiguration.DRKManaBarPadding;
        private int ManaBarXOffset => PluginConfiguration.DRKManaBarXOffset;
        private int ManaBarYOffset => PluginConfiguration.DRKManaBarYOffset;

        private bool BloodGaugeEnabled => PluginConfiguration.DRKBloodGaugeEnabled;
        private bool BloodGaugeSplit => PluginConfiguration.DRKBloodGaugeSplit;
        private bool BloodGaugeThreshold => PluginConfiguration.DRKBloodGaugeThreshold;
        private int BloodGaugeHeight => PluginConfiguration.DRKBloodGaugeHeight;
        private int BloodGaugeWidth => PluginConfiguration.DRKBloodGaugeWidth;
        private int BloodGaugePadding => PluginConfiguration.DRKBloodGaugePadding;
        private int BloodGaugeXOffset => PluginConfiguration.DRKBloodGaugeXOffset;
        private int BloodGaugeYOffset => PluginConfiguration.DRKBloodGaugeYOffset;

        private bool BuffBarEnabled => PluginConfiguration.DRKBuffBarEnabled;
        private int BuffBarHeight => PluginConfiguration.DRKBuffBarHeight;
        private int BuffBarWidth => PluginConfiguration.DRKBuffBarWidth;
        private int BuffBarPadding => PluginConfiguration.DRKBuffBarPadding;
        private int BuffBarXOffset => PluginConfiguration.DRKBuffBarXOffset;
        private int BuffBarYOffset => PluginConfiguration.DRKBuffBarYOffset;

        private bool LivingShadowBarEnabled => PluginConfiguration.DRKLivingShadowBarEnabled;
        private int LivingShadowBarHeight => PluginConfiguration.DRKLivingShadowBarHeight;
        private int LivingShadowBarWidth => PluginConfiguration.DRKLivingShadowBarWidth;
        private int LivingShadowBarPadding => PluginConfiguration.DRKLivingShadowBarPadding;
        private int LivingShadowBarXOffset => PluginConfiguration.DRKLivingShadowBarXOffset;
        private int LivingShadowBarYOffset => PluginConfiguration.DRKLivingShadowBarYOffset;

        private Dictionary<string, uint> ManaColor => PluginConfiguration.JobColorMap[Jobs.DRK * 1000];
        private Dictionary<string, uint> BloodColorLeft => PluginConfiguration.JobColorMap[Jobs.DRK * 1000 + 1];
        private Dictionary<string, uint> BloodColorRight => PluginConfiguration.JobColorMap[Jobs.DRK * 1000 + 2];
        private Dictionary<string, uint> DarkArtsColor => PluginConfiguration.JobColorMap[Jobs.DRK * 1000 + 3];
        private Dictionary<string, uint> BloodWeaponColor => PluginConfiguration.JobColorMap[Jobs.DRK * 1000 + 4];
        private Dictionary<string, uint> DeliriumColor => PluginConfiguration.JobColorMap[Jobs.DRK * 1000 + 5];
        private Dictionary<string, uint> LivingShadowColor => PluginConfiguration.JobColorMap[Jobs.DRK * 1000 + 6];
        private Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.DRK * 1000 + 7];

        private static int BarHeight => 13;
        private static int BarWidth => 254;

        public DarkKnightHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _) {
            if (ManaBarEnabled)
                DrawManaBar();
            if (BloodGaugeEnabled)
                DrawBloodGauge();
            if (BuffBarEnabled)
                DrawBuffBar();
            if (LivingShadowBarEnabled)
                DrawLivingShadowBar();
        }
        protected override void DrawPrimaryResourceBar()
        {
        }

        private void DrawManaBar() {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");

            //var tbn = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1178);
            var darkArtsBuff = PluginInterface.ClientState.JobGauges.Get<DRKGauge>().HasDarkArts();

            var actor = PluginInterface.ClientState.LocalPlayer;
            var barWidth = (ManaBarWidth - ManaBarPadding * 2)  / 3.0f;
            var barSize = new Vector2(barWidth, ManaBarHeight);
            var xPos = CenterX - XOffset + ManaBarXOffset;
            var yPos = CenterY + YOffset + ManaBarYOffset;
            var cursorPos = new Vector2(xPos, yPos);
            const int chunkSize = 3000;

            var drawList = ImGui.GetWindowDrawList();

            void DrawManaChunks(int index = 1) {
                if (index > 3)
                    return;

                var mana = Math.Min(actor.CurrentMp, chunkSize * index);
                if (index == 2)
                    mana = Math.Max(mana - chunkSize, 0);
                else if (index == 3)
                    mana = Math.Max(mana - chunkSize * 2, 0);

                if (index > 1)
                    cursorPos = new Vector2(cursorPos.X + barWidth + ManaBarPadding, cursorPos.Y);

                if (darkArtsBuff) {
                    var glowPosition = new Vector2(cursorPos.X - 1, cursorPos.Y - 1);
                    var glowSize = new Vector2(barSize.X + 2, barSize.Y + 2);
                    var glowColor = ImGui.ColorConvertFloat4ToU32((PluginConfiguration.DRKDarkArtsColor.AdjustColor(+0.2f)));

                    drawList.AddRect(glowPosition, glowPosition + glowSize, glowColor);
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, DarkArtsColor["background"]);

                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barSize.X * mana / chunkSize, barSize.Y),
                        DarkArtsColor["gradientLeft"], DarkArtsColor["gradientRight"], DarkArtsColor["gradientRight"], DarkArtsColor["gradientLeft"]
                    );
                }
                else {
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);

                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barSize.X * mana / chunkSize, barSize.Y),
                        ManaColor["gradientLeft"], ManaColor["gradientRight"], ManaColor["gradientRight"], ManaColor["gradientLeft"]
                    );
                }

                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                DrawManaChunks(index + 1);
            }

            DrawManaChunks();

            if (ManaBarOverflowEnabled && actor.CurrentMp > 9000) {
                var over9000 = 9000 - actor.CurrentMp;
                cursorPos = new Vector2(cursorPos.X + barWidth - 1, cursorPos.Y);
                var inverseOffset = cursorPos + new Vector2((barSize.X / 10) * over9000 / ManaBarWidth, barSize.Y);

                drawList.AddRectFilledMultiColor(
                    cursorPos, inverseOffset,
                    DarkArtsColor["gradientLeft"], DarkArtsColor["gradientRight"], DarkArtsColor["gradientRight"], DarkArtsColor["gradientLeft"]
                );

                drawList.AddRect(cursorPos, inverseOffset, 0xFF000000);
            }
        }

        private void DrawBloodGauge() {
            var gauge = PluginInterface.ClientState.JobGauges.Get<DRKGauge>();

            var padding = BloodGaugeSplit ? BloodGaugePadding : 0;
            var barWidth = (BloodGaugeWidth - padding) / 2;
            var xPos = CenterX - XOffset + BloodGaugeXOffset;
            var yPos = CenterY + YOffset + BloodGaugeYOffset;

            var cursorPos = new Vector2(xPos, yPos);
            var thresholdCursorPos = new Vector2(cursorPos.X + barWidth, cursorPos.Y);

            const int chunkSize = 50;

            var barSize = new Vector2(BloodGaugeWidth, BloodGaugeHeight);
            var barSplitSize = new Vector2(barWidth, BloodGaugeHeight);

            var drawList = ImGui.GetWindowDrawList();

            if (! BloodGaugeSplit)
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, EmptyColor["background"]);

            void DrawBloodChunks(int index = 1) {
                if (index > 2)
                    return;

                var blood = Math.Min((int)gauge.Blood, chunkSize * index);
                var scale = (float) blood / chunkSize;

                var gradientLeft = index == 1 ? BloodColorLeft["gradientLeft"] : BloodColorRight["gradientLeft"];
                var gradientRight = index == 1 ? BloodColorLeft["gradientRight"] : BloodColorRight["gradientRight"];

                if (index == 2) {
                    blood = Math.Max(blood - chunkSize, 0);
                    scale = (float) blood / chunkSize;
                    cursorPos = new Vector2(cursorPos.X + barWidth + padding, cursorPos.Y);
                }

                if (BloodGaugeSplit)
                    drawList.AddRectFilled(cursorPos, cursorPos + barSplitSize, EmptyColor["background"]);

                if (scale >= 1.0f) {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth * scale, BloodGaugeHeight),
                        gradientLeft, gradientRight, gradientRight, gradientLeft
                    );
                }
                else {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth * scale, BloodGaugeHeight),
                        EmptyColor["gradientLeft"], EmptyColor["gradientRight"], EmptyColor["gradientRight"], EmptyColor["gradientLeft"]
                    );
                }

                if (BloodGaugeSplit)
                    drawList.AddRect(cursorPos, cursorPos + barSplitSize, 0xFF000000);

                DrawBloodChunks(index + 1);
            }

            DrawBloodChunks();

            if (! BloodGaugeSplit) {
                var cursor = new Vector2(xPos, yPos);
                drawList.AddRect(cursor, cursor + barSize, 0xFF000000);

                if (BloodGaugeThreshold)
                    drawList.AddLine(thresholdCursorPos, new Vector2(thresholdCursorPos.X, thresholdCursorPos.Y + BloodGaugeHeight), 0x88000000);
            }
        }

        private void DrawBuffBar()
        {
            var bloodWeaponBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 742);
            var deliriumBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1972);

            var buffBarBarWidth = BuffBarWidth;
            var xPos = CenterX - XOffset + BuffBarXOffset;
            var yPos = CenterY + YOffset + BuffBarYOffset;
            var cursorPos = new Vector2(xPos, yPos);
            var buffBarBarHeight = BuffBarHeight;
            var barSize = new Vector2(buffBarBarWidth, buffBarBarHeight);

            var drawList = ImGui.GetWindowDrawList();

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, EmptyColor["background"]);
            if (bloodWeaponBuff.Any() && deliriumBuff.Any())
            {
                var innerBarHeight = buffBarBarHeight / 2;
                barSize = new Vector2(buffBarBarWidth, innerBarHeight);

                var bloodWeaponDuration = Math.Abs(bloodWeaponBuff.First().Duration);
                var deliriumDuration = Math.Abs(deliriumBuff.First().Duration);

                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barSize.X / 10f * bloodWeaponDuration, barSize.Y),
                    BloodWeaponColor["gradientLeft"], BloodWeaponColor["gradientRight"], BloodWeaponColor["gradientRight"], BloodWeaponColor["gradientLeft"]
                );
                drawList.AddRectFilledMultiColor(
                    cursorPos + new Vector2(0.0f, innerBarHeight), cursorPos + new Vector2(barSize.X / 10f * deliriumDuration, barSize.Y * 2f),
                    DeliriumColor["gradientLeft"], DeliriumColor["gradientRight"], DeliriumColor["gradientRight"], DeliriumColor["gradientLeft"]
                );

                var bloodWeaponDurationText = bloodWeaponDuration == 0 ? "" : Math.Ceiling(bloodWeaponDuration).ToString(CultureInfo.InvariantCulture);
                DrawOutlinedText(bloodWeaponDurationText, new Vector2(cursorPos.X + 5f, cursorPos.Y - 2f), PluginConfiguration.DRKBloodWeaponColor, new Vector4(0f, 0f, 0f, 1f));

                var deliriumDurationText = deliriumDuration == 0 ? "" : Math.Ceiling(deliriumDuration).ToString(CultureInfo.InvariantCulture);
                DrawOutlinedText(deliriumDurationText, new Vector2(cursorPos.X + 27f, cursorPos.Y - 2f), PluginConfiguration.DRKDeliriumColor, new Vector4(0f, 0f, 0f, 1f));

                barSize = new Vector2(buffBarBarWidth, buffBarBarHeight);
            }
            else if (bloodWeaponBuff.Any())
            {
                var bloodWeaponDuration = Math.Abs(bloodWeaponBuff.First().Duration);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barSize.X / 10f * bloodWeaponDuration, barSize.Y),
                    BloodWeaponColor["gradientLeft"], BloodWeaponColor["gradientRight"], BloodWeaponColor["gradientRight"], BloodWeaponColor["gradientLeft"]
                );

                var bloodWeaponDurationText = bloodWeaponDuration == 0 ? "" : Math.Ceiling(bloodWeaponDuration).ToString();
                DrawOutlinedText(bloodWeaponDurationText, new Vector2(cursorPos.X + 5f, cursorPos.Y - 2f), PluginConfiguration.DRKBloodWeaponColor, new Vector4(0f, 0f, 0f, 1f));
            }
            else if (deliriumBuff.Any())
            {
                var deliriumDuration = Math.Abs(deliriumBuff.First().Duration);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barSize.X / 10f * deliriumDuration, barSize.Y),
                    DeliriumColor["gradientLeft"], DeliriumColor["gradientRight"], DeliriumColor["gradientRight"], DeliriumColor["gradientLeft"]
                );

                var deliriumDurationText = deliriumDuration == 0 ? "" : Math.Ceiling(deliriumDuration).ToString(CultureInfo.InvariantCulture);
                DrawOutlinedText(deliriumDurationText, new Vector2(cursorPos.X + 5f, cursorPos.Y - 2f), PluginConfiguration.DRKDeliriumColor, new Vector4(0f, 0f, 0f, 1f));
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }

        private void DrawLivingShadowBar() {
            var actor = PluginInterface.ClientState.LocalPlayer;
            var shadowTimeRemaining = PluginInterface.ClientState.JobGauges.Get<DRKGauge>().ShadowTimeRemaining / 100; // ms
            var livingShadow = actor.Level >= 80 && shadowTimeRemaining > 0 && shadowTimeRemaining <= 24;

            var barWidth = LivingShadowBarWidth;
            var xPos = CenterX - XOffset + LivingShadowBarXOffset;
            var yPos = CenterY + YOffset + LivingShadowBarYOffset;
            var cursorPos = new Vector2(xPos, yPos);
            var barSize = new Vector2(barWidth, LivingShadowBarHeight);

            var drawList = ImGui.GetWindowDrawList();

            float duration = 0;
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, EmptyColor["background"]);
            if (livingShadow)
            {
                duration = Math.Abs(shadowTimeRemaining);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barSize.X / 24 * duration, barSize.Y),
                    LivingShadowColor["gradientLeft"], LivingShadowColor["gradientRight"], LivingShadowColor["gradientRight"], LivingShadowColor["gradientLeft"]
                );
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            var durationText = duration != 0 ? Math.Round(duration).ToString(CultureInfo.InvariantCulture) : "";
            var textSize = ImGui.CalcTextSize(durationText);
            DrawOutlinedText(durationText, new Vector2(cursorPos.X + LivingShadowBarWidth / 2f - textSize.X / 2f, cursorPos.Y-2));
        }
    }
}
