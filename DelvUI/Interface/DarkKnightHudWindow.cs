using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUI.Interface {
    public class DarkKnightHudWindow : HudWindow {
        public override uint JobId => 32;

        private new int XOffset => PluginConfiguration.DRKBaseXOffset;
        private new int YOffset => PluginConfiguration.DRKBaseYOffset;

        private int ManaBarHeight => PluginConfiguration.DRKManaBarHeight;
        private int ManaBarWidth => PluginConfiguration.DRKManaBarWidth;
        private int ManaBarPadding => PluginConfiguration.DRKManaBarPadding;
        private int ManaBarXOffset => PluginConfiguration.DRKManaBarXOffset;
        private int ManaBarYOffset => PluginConfiguration.DRKManaBarYOffset;

        private int BloodGaugeHeight => PluginConfiguration.DRKBloodGaugeHeight;
        private int BloodGaugeWidth => PluginConfiguration.DRKBloodGaugeWidth;
        private int BloodGaugePadding => PluginConfiguration.DRKBloodGaugePadding;
        private int BloodGaugeXOffset => PluginConfiguration.DRKBloodGaugeXOffset;
        private int BloodGaugeYOffset => PluginConfiguration.DRKBloodGaugeYOffset;

        private int DarkArtsBarHeight => PluginConfiguration.DRKDarkArtsBarHeight;
        private int DarkArtsBarWidth => PluginConfiguration.DRKDarkArtsBarWidth;
        private int DarkArtsBarPadding => PluginConfiguration.DRKDarkArtsBarPadding;
        private int DarkArtsBarXOffset => PluginConfiguration.DRKDarkArtsBarXOffset;
        private int DarkArtsBarYOffset => PluginConfiguration.DRKDarkArtsBarYOffset;

        private int InterBarOffset => PluginConfiguration.DRKInterBarOffset;

        private Dictionary<string, uint> ManaColor => PluginConfiguration.JobColorMap[Jobs.DRK * 1000];
        private Dictionary<string, uint> BloodColorLeft => PluginConfiguration.JobColorMap[Jobs.DRK * 1000 + 1];
        private Dictionary<string, uint> BloodColorRight => PluginConfiguration.JobColorMap[Jobs.DRK * 1000 + 2];
        private Dictionary<string, uint> DarkArtsColor => PluginConfiguration.JobColorMap[Jobs.DRK * 1000 + 3];
        private Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.DRK * 1000 + 4];

        private static int BarHeight => 13;
        private static int BarWidth => 254;

        public DarkKnightHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _) {
            DrawHealthBar();
            var nextHeight = DrawManaBar(0);
            nextHeight = DrawBloodGauge(nextHeight);
            // TODO(poly1k):
            // merge dark arts into mana bar
            // create combined delerium/blood weapon buff gauge (copy from paladin)

            DrawDarkArtsBar(nextHeight);
            DrawTargetBar();
            DrawFocusBar();
            DrawCastBar();
        }

        private int DrawManaBar(int initialHeight) {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");

            //var tbn = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1178);
            var actor = PluginInterface.ClientState.LocalPlayer;
            var barWidth = (ManaBarWidth - ManaBarPadding)  / 3.0f;
            var barSize = new Vector2(barWidth, ManaBarHeight);
            var xPos = CenterX - XOffset + ManaBarXOffset;
            var yPos = CenterY + YOffset + ManaBarYOffset + initialHeight;
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
                    cursorPos = new Vector2(cursorPos.X + barWidth + (ManaBarPadding / 2), cursorPos.Y);

                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barSize.X * mana / chunkSize, barSize.Y),
                    ManaColor["gradientLeft"], ManaColor["gradientRight"], ManaColor["gradientRight"], ManaColor["gradientLeft"]
                );
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                DrawManaChunks(index + 1);
            }

            DrawManaChunks();

            return ManaBarHeight + initialHeight + InterBarOffset;
        }

        private int DrawBloodGauge(int initialHeight) {
            var gauge = PluginInterface.ClientState.JobGauges.Get<DRKGauge>();

            var barWidth = (BloodGaugeWidth - BloodGaugePadding) / 2;
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset + initialHeight + BloodGaugeYOffset;
            var cursorPos = new Vector2(xPos, yPos);
            const int chunkSize = 50;
            var barSize = new Vector2(barWidth, BloodGaugeHeight);

            // Chunk 1
            var blood = Math.Min((int)gauge.Blood, chunkSize);
            var scale = (float) blood / chunkSize;

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);

            if (scale >= 1.0f) {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barWidth * scale, BloodGaugeHeight),
                    BloodColorLeft["gradientLeft"], BloodColorLeft["gradientRight"], BloodColorLeft["gradientRight"], BloodColorLeft["gradientLeft"]
                );
            }
            else {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barWidth * scale, BloodGaugeHeight),
                    EmptyColor["gradientLeft"], EmptyColor["gradientRight"], EmptyColor["gradientRight"], EmptyColor["gradientLeft"]
                );
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);


            // Chunk 2
            blood = Math.Max(Math.Min((int)gauge.Blood, chunkSize * 2) - chunkSize, 0);
            scale = (float) blood / chunkSize;
            cursorPos = new Vector2(cursorPos.X + barWidth + BloodGaugePadding, cursorPos.Y);

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);

            if (scale >= 1.0f) {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barWidth * scale, BloodGaugeHeight),
                    BloodColorRight["gradientLeft"], BloodColorRight["gradientRight"], BloodColorRight["gradientRight"], BloodColorRight["gradientLeft"]
                );
            }
            else {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barWidth * scale, BloodGaugeHeight),
                    EmptyColor["gradientLeft"], EmptyColor["gradientRight"], EmptyColor["gradientRight"], EmptyColor["gradientLeft"]
                );
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            return BloodGaugeHeight + initialHeight + InterBarOffset;
        }

        private void DrawDarkArtsBar(int initialHeight) {
            var darkArtsBuff = PluginInterface.ClientState.JobGauges.Get<DRKGauge>().HasDarkArts();

            var barWidth = DarkArtsBarWidth;
            var xPos = CenterX - XOffset + DarkArtsBarXOffset;
            var yPos = CenterY + YOffset + initialHeight + DarkArtsBarYOffset;
            var cursorPos = new Vector2(xPos, yPos);
            var barSize = new Vector2(barWidth, DarkArtsBarHeight);

            var drawList = ImGui.GetWindowDrawList();

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            if (darkArtsBuff)
            {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barSize.X, barSize.Y),
                    DarkArtsColor["gradientLeft"], DarkArtsColor["gradientRight"], DarkArtsColor["gradientRight"], DarkArtsColor["gradientLeft"]
                );
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }
    }
}
