using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Interface.Bars;
using ImGuiNET;
using DelvUI.Helpers;

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
        private int InterBarOffset => PluginConfiguration.BRDInterBarOffset;
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
        public BardHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _)
        {
            var nextHeight = DrawActiveDots(-2);
            nextHeight = HandleCurrentSong(nextHeight);
            DrawSoulVoiceBar(nextHeight);
        }

        protected override void DrawPrimaryResourceBar()
        {
        }

        private int DrawActiveDots(int initialHeight)
        {
            var nextHeight = Math.Abs(BRDCBYOffset - BRDSBYOffset) + BRDCBHeight;
            if (!BRDShowCB && !BRDShowSB) return nextHeight;
            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;

            if (target is not Chara) {
                return nextHeight;
            }
            var xPos = CenterX - XOffset + BRDCBXOffset;
            var yPos = CenterY + YOffset + BRDCBYOffset + initialHeight;

            var barDrawList = new List<Bar>();

            if (BRDShowCB)
            {
                var cb = target.StatusEffects.FirstOrDefault(o =>
                    o.EffectId == 1200 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId ||
                    o.EffectId == 124 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId);
                var duration = Math.Abs(cb.Duration);

                var color = duration <= 5 ? ExpireColor : CBColor;
            
                var builder = BarBuilder.Create(xPos, yPos, BRDCBHeight, BRDCBWidth);
                
                var cbBar = builder.AddInnerBar(duration, 30f, color)
                    .SetFlipDrainDirection(BRDCBInverted)
                    .Build();
                barDrawList.Add(cbBar);
            }
            
            xPos = CenterX - XOffset + BRDSBXOffset;
            yPos = CenterY + YOffset + BRDSBYOffset + initialHeight;
            
            if (BRDShowSB)
            {
                var sb = target.StatusEffects.FirstOrDefault(o =>
                    o.EffectId == 1201 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId ||
                    o.EffectId == 129 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId);
                var duration = Math.Abs(sb.Duration);
                
                var color = duration <= 5 ? ExpireColor : SBColor;
            
                var builder = BarBuilder.Create(xPos, yPos, BRDSBHeight, BRDSBWidth);
                
                var sbBar = builder.AddInnerBar(duration, 30f, color)
                    .SetFlipDrainDirection(BRDSBInverted)
                    .Build();
                barDrawList.Add(sbBar);

            }
            
            if (barDrawList.Count > 0)
            {
                var drawList = ImGui.GetWindowDrawList();
                foreach (var bar in barDrawList)
                {
                    bar.Draw(drawList);
                }
            }
            return nextHeight;
        }

        private int HandleCurrentSong(int initialHeight)
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<BRDGauge>();
            var songStacks = gauge.NumSongStacks;
            var song = gauge.ActiveSong;
            var songTimer = gauge.SongTimer;
            
            var nextHeight = BRDStackHeight + initialHeight + InterBarOffset;
            
            switch (song)
            {
                case CurrentSong.WANDERER:
                    if(BRDShowWMStacks) nextHeight = DrawStacks(initialHeight, songStacks, 3, WMStackColor);
                    return DrawSongTimer(nextHeight, songTimer, WMColor);
                case CurrentSong.MAGE:
                    if(BRDShowMBProc) nextHeight = DrawBloodletterReady(initialHeight, MBStackColor);
                    return DrawSongTimer(nextHeight, songTimer, MBColor);
                case CurrentSong.ARMY:
                    if(BRDShowAPStacks) nextHeight = DrawStacks(initialHeight, songStacks, 4, APStackColor);
                    return DrawSongTimer(nextHeight, songTimer, APColor);
                case CurrentSong.NONE:
                    return DrawSongTimer(nextHeight, 0, EmptyColor);
                default:
                    return DrawSongTimer(nextHeight, 0, EmptyColor);
            }
        }

        private int DrawBloodletterReady(int initialHeight, Dictionary<string, uint> color)
        {
            // I want to draw Bloodletter procs here (just color entire bar red to indicate cooldown is ready).
            // But can't find a way yet to accomplish this.
            return initialHeight + BRDStackHeight + InterBarOffset;
        }

        private int DrawSongTimer(int initialHeight, short songTimer, Dictionary<string, uint> songColor)
        {
            var nextHeight = BRDSongGaugeHeight + initialHeight + InterBarOffset;
            if (!BRDShowSongGauge) return nextHeight;
            var xPos = CenterX - XOffset + BRDSongGaugeXOffset;
            var yPos = CenterY + YOffset + BRDSongGaugeYOffset + initialHeight;
            
            var builder = BarBuilder.Create(xPos, yPos, BRDSongGaugeHeight, BRDSongGaugeWidth);

            var duration = Math.Abs(songTimer);
            
            var bar = builder.AddInnerBar(duration / 1000f, 30f, songColor)
                .SetTextMode(BarTextMode.EachChunk)
                .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                .Build();
            
            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
            
            return nextHeight;
        }

        private int DrawSoulVoiceBar(int initialHeight)
        {
            var nextHeight = BRDSoulGaugeHeight + initialHeight + InterBarOffset;
            if (!BRDShowSoulGauge) return nextHeight;
            var soulVoice = PluginInterface.ClientState.JobGauges.Get<BRDGauge>().SoulVoiceValue;

            var xPos = CenterX - XOffset + BRDSoulGaugeXOffset;
            var yPos = CenterY + YOffset + BRDSoulGaugeYOffset + initialHeight;
            
            var builder = BarBuilder.Create(xPos, yPos, BRDSoulGaugeHeight, BRDSoulGaugeWidth);
            
            var bar = builder.AddInnerBar(soulVoice, 100f, SVColor)
                .Build();
            
            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
            
            return nextHeight;
        }

        private int DrawStacks(int initialHeight, int amount, int max, Dictionary<string, uint> stackColor)
        {
            var xPos = CenterX - XOffset + BRDStackXOffset;
            var yPos = CenterY + YOffset + initialHeight + BRDStackYOffset;
            var bar = BarBuilder.Create(xPos, yPos, BRDStackHeight, BRDStackWidth)
                .SetChunks(max)
                .SetChunkPadding(BRDStackPadding)
                .AddInnerBar(amount, max, stackColor)
                .Build();
            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);

            return BRDStackHeight + initialHeight + InterBarOffset;
        }
    }
}