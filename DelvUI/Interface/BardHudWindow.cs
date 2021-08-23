using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui;
using Dalamud.Interface;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Interface.Bars;
using ImGuiNET;

namespace DelvUI.Interface {
    public class BardHudWindow : HudWindow {
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
        
        public BardHudWindow(
            ClientState clientState,
            DalamudPluginInterface pluginInterface,
            DataManager dataManager,
            Framework framework,
            GameGui gameGui,
            JobGauges jobGauges,
            ObjectTable objectTable, 
            PluginConfiguration pluginConfiguration,
            SigScanner sigScanner,
            TargetManager targetManager,
            UiBuilder uiBuilder
        ) : base(
            clientState,
            pluginInterface,
            dataManager,
            framework,
            gameGui,
            jobGauges,
            objectTable,
            pluginConfiguration,
            sigScanner,
            targetManager,
            uiBuilder
        ) { }

        protected override void Draw(bool _)
        {
            DrawActiveDots();
            HandleCurrentSong();
            DrawSoulVoiceBar();
        }

        protected override void DrawPrimaryResourceBar()
        {
        }

        private void DrawActiveDots()
        {
            if (!BRDShowCB && !BRDShowSB) {
                return;
            }
            
            var actor = TargetManager.SoftTarget ?? TargetManager.Target;

            if (actor is not BattleChara target) {
                return;
            }
            
            var xPos = CenterX - XOffset + BRDCBXOffset;
            var yPos = CenterY + YOffset + BRDCBYOffset;

            var barDrawList = new List<Bar>();

            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            if (BRDShowCB) {
                var cb = target.StatusList.FirstOrDefault(o =>
                    o.StatusId == 1200 && o.SourceID == ClientState.LocalPlayer.ObjectId ||
                    o.StatusId == 124 && o.SourceID == ClientState.LocalPlayer.ObjectId);
                var duration = Math.Abs(cb?.RemainingTime ?? 0f);

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

            if (BRDShowSB)
            {
                var sb = target.StatusList.FirstOrDefault(o =>
                    o.StatusId == 1201 && o.SourceID == ClientState.LocalPlayer.ObjectId ||
                    o.StatusId == 129 && o.SourceID == ClientState.LocalPlayer.ObjectId);
                var duration = Math.Abs(sb?.RemainingTime ?? 0f);

                var color = duration <= 5 ? ExpireColor : SBColor;

                var builder = BarBuilder.Create(xPos, yPos, BRDSBHeight, BRDSBWidth);

                var sbBar = builder.AddInnerBar(duration, 30f, color)
                    .SetFlipDrainDirection(BRDSBInverted)
                    .SetBackgroundColor(EmptyColor["background"])
                    .Build();
                barDrawList.Add(sbBar);
            }

            if (barDrawList.Count > 0) {
                var drawList = ImGui.GetWindowDrawList();
                foreach (var bar in barDrawList) {
                    bar.Draw(drawList, PluginConfiguration);
                }
            }
        }

        private void HandleCurrentSong() {
            var gauge = JobGauges.Get<BRDGauge>();
            var songStacks = gauge.Repertoire;
            var song = gauge.Song;
            var songTimer = gauge.SongTimer;

            switch (song) {
                case Song.WANDERER:
                    if (BRDShowWMStacks) {
                        DrawStacks(songStacks, 3, WMStackColor);
                    }
                    DrawSongTimer(songTimer, WMColor);
                    break;
                case Song.MAGE:
                    if (BRDShowMBProc) {
                        DrawBloodletterReady(MBStackColor);
                    }

                    DrawSongTimer(songTimer, MBColor);
                    break;
                case Song.ARMY:
                    if (BRDShowAPStacks) {
                        DrawStacks(songStacks, 4, APStackColor);
                    }

                    DrawSongTimer(songTimer, APColor);
                    break;
                case Song.NONE:
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

        private void DrawSongTimer(short songTimer, Dictionary<string, uint> songColor)
        {
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

        private void DrawSoulVoiceBar()
        {
            if (!BRDShowSoulGauge) return;
            var soulVoice = JobGauges.Get<BRDGauge>().SoulVoice;

            var xPos = CenterX - XOffset + BRDSoulGaugeXOffset;
            var yPos = CenterY + YOffset + BRDSoulGaugeYOffset;

            var builder = BarBuilder.Create(xPos, yPos, BRDSoulGaugeHeight, BRDSoulGaugeWidth);

            var bar = builder.AddInnerBar(soulVoice, 100f, SVColor)
                .SetBackgroundColor(EmptyColor["background"])
                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawStacks(int amount, int max, Dictionary<string, uint> stackColor)
        {
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