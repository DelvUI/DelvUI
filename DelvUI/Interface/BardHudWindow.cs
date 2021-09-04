using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Interface.Bars;
using ImGuiNET;

namespace DelvUI.Interface {
    public class BardHudWindow : HudWindow {
        public BardHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        public override uint JobId => 23;
        private new int XOffset => PluginConfiguration.BRDBaseXOffset;
        private new int YOffset => PluginConfiguration.BRDBaseYOffset;
        private int BRDSongGaugeWidth => PluginConfiguration.BRDSongGaugeWidth;
        private int BRDSongGaugeHeight => PluginConfiguration.BRDSongGaugeHeight;
        private int BRDSongGaugeXOffset => PluginConfiguration.BRDSongGaugeXOffset;
        private int BRDSongGaugeYOffset => PluginConfiguration.BRDSongGaugeYOffset;
        private int BRDSoulGaugeWidth => PluginConfiguration.BRDSoulGaugeWidth;
        private int BRDSoulGaugeHeight => PluginConfiguration.BRDSoulGaugeHeight;
        private int BRDSoulGaugeXOffset => PluginConfiguration.BRDSoulGaugeXOffset;
        private int BRDSoulGaugeYOffset => PluginConfiguration.BRDSoulGaugeYOffset;
        private int BRDStackWidth => PluginConfiguration.BRDStackWidth;
        private int BRDStackHeight => PluginConfiguration.BRDStackHeight;
        private int BRDStackXOffset => PluginConfiguration.BRDStackXOffset;
        private int BRDStackYOffset => PluginConfiguration.BRDStackYOffset;
        private int BRDCBWidth => PluginConfiguration.BRDCBWidth;
        private int BRDCBHeight => PluginConfiguration.BRDCBHeight;
        private int BRDCBXOffset => PluginConfiguration.BRDCBXOffset;
        private int BRDCBYOffset => PluginConfiguration.BRDCBYOffset;
        private int BRDSBWidth => PluginConfiguration.BRDSBWidth;
        private int BRDSBHeight => PluginConfiguration.BRDSBHeight;
        private int BRDSBXOffset => PluginConfiguration.BRDSBXOffset;
        private int BRDSBYOffset => PluginConfiguration.BRDSBYOffset;
        private int BRDStackPadding => PluginConfiguration.BRDStackPadding;
        private bool BRDShowSongGauge => PluginConfiguration.BRDShowSongGauge;
        private bool BRDShowSoulGauge => PluginConfiguration.BRDShowSoulGauge;
        private bool BRDShowWMStacks => PluginConfiguration.BRDShowWMStacks;
        private bool BRDShowMBProc => PluginConfiguration.BRDShowMBProc;
        private bool BRDShowAPStacks => PluginConfiguration.BRDShowAPStacks;
        private bool BRDShowCB => PluginConfiguration.BRDShowCB;
        private bool BRDShowSB => PluginConfiguration.BRDShowSB;
        private bool BRDCBInverted => PluginConfiguration.BRDCBInverted;
        private bool BRDSBInverted => PluginConfiguration.BRDSBInverted;
        private Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.BRD * 1000];
        private Dictionary<string, uint> ExpireColor => PluginConfiguration.JobColorMap[Jobs.BRD * 1000 + 1];
        private Dictionary<string, uint> WMColor => PluginConfiguration.JobColorMap[Jobs.BRD * 1000 + 2];
        private Dictionary<string, uint> MBColor => PluginConfiguration.JobColorMap[Jobs.BRD * 1000 + 3];
        private Dictionary<string, uint> APColor => PluginConfiguration.JobColorMap[Jobs.BRD * 1000 + 4];
        private Dictionary<string, uint> WMStackColor => PluginConfiguration.JobColorMap[Jobs.BRD * 1000 + 5];
        private Dictionary<string, uint> MBStackColor => PluginConfiguration.JobColorMap[Jobs.BRD * 1000 + 6];
        private Dictionary<string, uint> APStackColor => PluginConfiguration.JobColorMap[Jobs.BRD * 1000 + 7];
        private Dictionary<string, uint> SBColor => PluginConfiguration.JobColorMap[Jobs.BRD * 1000 + 8];
        private Dictionary<string, uint> CBColor => PluginConfiguration.JobColorMap[Jobs.BRD * 1000 + 9];
        private Dictionary<string, uint> SVColor => PluginConfiguration.JobColorMap[Jobs.BRD * 1000 + 10];

        protected override void Draw(bool _) {
            DrawActiveDots();
            HandleCurrentSong();
            DrawSoulVoiceBar();
        }

        protected override void DrawPrimaryResourceBar() { }

        private void DrawActiveDots() {
            if (!BRDShowCB && !BRDShowSB) {
                return;
            }

            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;

            if (target is not Chara) {
                return;
            }

            var xPos = CenterX - XOffset + BRDCBXOffset;
            var yPos = CenterY + YOffset + BRDCBYOffset;

            var barDrawList = new List<Bar>();

            if (BRDShowCB) {
                var cb = target.StatusEffects.FirstOrDefault(
                    o =>
                        o.EffectId == 1200 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                     || o.EffectId == 124 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                );

                var duration = Math.Abs(cb.Duration);

                var color = duration <= 5 ? ExpireColor : CBColor;

                var builder = BarBuilder.Create(xPos, yPos, BRDCBHeight, BRDCBWidth);

                var cbBar = builder.AddInnerBar(duration, 30f, color)
                                   .SetFlipDrainDirection(BRDCBInverted)
                                   .SetBackgroundColor(EmptyColor["background"])
                                   .Build();

                barDrawList.Add(cbBar);
            }

            xPos = CenterX - XOffset + BRDSBXOffset;
            yPos = CenterY + YOffset + BRDSBYOffset;

            if (BRDShowSB) {
                var sb = target.StatusEffects.FirstOrDefault(
                    o =>
                        o.EffectId == 1201 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                     || o.EffectId == 129 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                );

                var duration = Math.Abs(sb.Duration);

                var color = duration <= 5 ? ExpireColor : SBColor;

                var builder = BarBuilder.Create(xPos, yPos, BRDSBHeight, BRDSBWidth);

                var sbBar = builder.AddInnerBar(duration, 30f, color)
                                   .SetFlipDrainDirection(BRDSBInverted)
                                   .SetBackgroundColor(EmptyColor["background"])
                                   .Build();

                barDrawList.Add(sbBar);
            }

            if (barDrawList.Count <= 0) {
                return;
            }

            var drawList = ImGui.GetWindowDrawList();

            foreach (var bar in barDrawList) {
                bar.Draw(drawList, PluginConfiguration);
            }
        }

        private void HandleCurrentSong() {
            var gauge = PluginInterface.ClientState.JobGauges.Get<BRDGauge>();
            var songStacks = gauge.NumSongStacks;
            var song = gauge.ActiveSong;
            var songTimer = gauge.SongTimer;

            switch (song) {
                case CurrentSong.WANDERER:
                    if (BRDShowWMStacks) {
                        DrawStacks(songStacks, 3, WMStackColor);
                    }

                    DrawSongTimer(songTimer, WMColor);

                    break;

                case CurrentSong.MAGE:
                    if (BRDShowMBProc) {
                        DrawBloodletterReady(MBStackColor);
                    }

                    DrawSongTimer(songTimer, MBColor);

                    break;

                case CurrentSong.ARMY:
                    if (BRDShowAPStacks) {
                        DrawStacks(songStacks, 4, APStackColor);
                    }

                    DrawSongTimer(songTimer, APColor);

                    break;

                case CurrentSong.NONE:
                    DrawSongTimer(0, EmptyColor);

                    break;

                default:
                    DrawSongTimer(0, EmptyColor);

                    break;
            }
        }

        private void DrawBloodletterReady(Dictionary<string, uint> color) {
            // I want to draw Bloodletter procs here (just color entire bar red to indicate cooldown is ready).
            // But can't find a way yet to accomplish this.
        }

        private void DrawSongTimer(short songTimer, Dictionary<string, uint> songColor) {
            if (!BRDShowSongGauge) {
                return;
            }

            var xPos = CenterX - XOffset + BRDSongGaugeXOffset;
            var yPos = CenterY + YOffset + BRDSongGaugeYOffset;

            var builder = BarBuilder.Create(xPos, yPos, BRDSongGaugeHeight, BRDSongGaugeWidth);

            var duration = Math.Abs(songTimer);

            var bar = builder.AddInnerBar(duration / 1000f, 30f, songColor)
                             .SetTextMode(BarTextMode.EachChunk)
                             .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                             .SetBackgroundColor(EmptyColor["background"])
                             .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawSoulVoiceBar() {
            if (!BRDShowSoulGauge) {
                return;
            }

            var soulVoice = PluginInterface.ClientState.JobGauges.Get<BRDGauge>().SoulVoiceValue;

            var xPos = CenterX - XOffset + BRDSoulGaugeXOffset;
            var yPos = CenterY + YOffset + BRDSoulGaugeYOffset;

            var builder = BarBuilder.Create(xPos, yPos, BRDSoulGaugeHeight, BRDSoulGaugeWidth);

            var bar = builder.AddInnerBar(soulVoice, 100f, SVColor)
                             .SetBackgroundColor(EmptyColor["background"])
                             .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawStacks(int amount, int max, Dictionary<string, uint> stackColor) {
            var xPos = CenterX - XOffset + BRDStackXOffset;
            var yPos = CenterY + YOffset + BRDStackYOffset;

            var bar = BarBuilder.Create(xPos, yPos, BRDStackHeight, BRDStackWidth)
                                .SetChunks(max)
                                .SetChunkPadding(BRDStackPadding)
                                .AddInnerBar(amount, max, stackColor)
                                .SetBackgroundColor(EmptyColor["background"])
                                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }
    }
}
