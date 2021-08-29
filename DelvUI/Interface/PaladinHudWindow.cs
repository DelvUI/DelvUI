using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Interface.Bars;
using ImGuiNET;

namespace DelvUI.Interface
{
    public class PaladinHudWindow : HudWindow
    {
        public override uint JobId => 19;

        private int ManaBarHeight => PluginConfiguration.PLDManaHeight;
        
        private int ManaBarWidth => PluginConfiguration.PLDManaWidth;
        
        private int ManaBarPadding => PluginConfiguration.PLDManaPadding;
        
        private new int XOffset => PluginConfiguration.PLDBaseXOffset;
        
        private new int YOffset => PluginConfiguration.PLDBaseYOffset;
        
        private int OathGaugeBarHeight => PluginConfiguration.PLDOathGaugeHeight;
        
        private int OathGaugeBarWidth => PluginConfiguration.PLDOathGaugeWidth;
        
        private int OathGaugeBarPadding => PluginConfiguration.PLDOathGaugePadding;
        
        private int OathGaugeXOffset => PluginConfiguration.PLDOathGaugeXOffset;
        
        private int OathGaugeYOffset => PluginConfiguration.PLDOathGaugeYOffset;
        
        private bool OathGaugeText => PluginConfiguration.PLDOathGaugeText;
        
        private int BuffBarHeight => PluginConfiguration.PLDBuffBarHeight;
        
        private int BuffBarWidth => PluginConfiguration.PLDBuffBarWidth;
        
        private int BuffBarXOffset => PluginConfiguration.PLDBuffBarXOffset;
        
        private int BuffBarYOffset => PluginConfiguration.PLDBuffBarYOffset;
        
        private int AtonementBarHeight => PluginConfiguration.PLDAtonementBarHeight;
        
        private int AtonementBarWidth => PluginConfiguration.PLDAtonementBarWidth;
        
        private int AtonementBarPadding => PluginConfiguration.PLDAtonementBarPadding;
        
        private int AtonementBarXOffset => PluginConfiguration.PLDAtonementBarXOffset;
        
        private int AtonementBarYOffset => PluginConfiguration.PLDAtonementBarYOffset;
        
        private int InterBarOffset => PluginConfiguration.PLDInterBarOffset;
        
        private Dictionary<string, uint> ManaColor => PluginConfiguration.JobColorMap[Jobs.PLD * 1000];
        
        private Dictionary<string, uint> OathGaugeColor => PluginConfiguration.JobColorMap[Jobs.PLD * 1000 + 1];
        
        private Dictionary<string, uint> FightOrFlightColor => PluginConfiguration.JobColorMap[Jobs.PLD * 1000 + 2];
        
        private Dictionary<string, uint> RequiescatColor => PluginConfiguration.JobColorMap[Jobs.PLD * 1000 + 3];
        
        private Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.PLD * 1000 + 4];
        
        private Dictionary<string, uint> AtonementColor => PluginConfiguration.JobColorMap[Jobs.PLD * 1000 + 5];
        
        public PaladinHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) {}

        protected override void Draw(bool _)
        {
            var nextHeight = DrawManaBar(0);
            nextHeight = DrawOathGauge(nextHeight);
            nextHeight = DrawBuffBar(nextHeight);
            DrawAtonementBar(nextHeight);
        }

        protected override void DrawPrimaryResourceBar()
        {
        }

        private int DrawManaBar(int initialHeight)
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var actor = PluginInterface.ClientState.LocalPlayer;

            var posX = CenterX - XOffset;
            var posY = CenterY + YOffset;

            var builder = BarBuilder.Create(posX, posY, ManaBarHeight, ManaBarWidth)
                .SetChunks(5)
                .SetChunkPadding(ManaBarPadding)
                .AddInnerBar(actor.CurrentMp, actor.MaxMp, ManaColor, EmptyColor);

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);

            return ManaBarHeight + initialHeight + InterBarOffset;
        }

        private int DrawOathGauge(int initialHeight)
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<PLDGauge>();

            var xPos = CenterX - XOffset + OathGaugeXOffset;
            var yPos = CenterY + YOffset + initialHeight + OathGaugeYOffset;

            var builder = BarBuilder.Create(xPos, yPos, OathGaugeBarHeight, OathGaugeBarWidth)
                .SetChunks(2)
                .SetChunkPadding(OathGaugeBarPadding)
                .AddInnerBar(gauge.GaugeAmount, 100, OathGaugeColor, EmptyColor);

            if (OathGaugeText)
            {
                builder.SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);

            return OathGaugeBarHeight + initialHeight + InterBarOffset;
        }

        private int DrawBuffBar(int initialHeight)
        {
            var fightOrFlightBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 76);
            var requiescatBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1368);
            
            var xPos = CenterX - XOffset + BuffBarXOffset;
            var yPos = CenterY + YOffset + initialHeight + BuffBarYOffset;

            var builder = BarBuilder.Create(xPos, yPos, BuffBarHeight, BuffBarWidth);

            if (fightOrFlightBuff.Any())
            {
                var fightOrFlightDuration = Math.Abs(fightOrFlightBuff.First().Duration);
                builder.AddInnerBar(fightOrFlightDuration, 25, FightOrFlightColor, null)
                    .SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterLeft, BarTextType.Current, PluginConfiguration.PLDFightOrFlightColor, Vector4.UnitW, null);
            }
            if (requiescatBuff.Any())
            {
                var requiescatDuration = Math.Abs(requiescatBuff.First().Duration);
                builder.AddInnerBar(requiescatDuration, 12, RequiescatColor, null)
                    .SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterRight, BarTextType.Current, PluginConfiguration.PLDRequiescatColor, Vector4.UnitW, null);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
            
            return BuffBarHeight + initialHeight + InterBarOffset;
        }

        private int DrawAtonementBar(int initialHeight)
        {
            var atonementBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1902);
            var stackCount = atonementBuff.Any() ? atonementBuff.First().StackCount : 0;
            
            var xPos = CenterX - XOffset + AtonementBarXOffset;
            var yPos = CenterY + YOffset + initialHeight + AtonementBarYOffset;
            
            var builder = BarBuilder.Create(xPos, yPos, AtonementBarHeight, AtonementBarWidth)
                .SetChunks(3)
                .SetChunkPadding(AtonementBarPadding)
                .AddInnerBar(stackCount, 3, AtonementColor, null);

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);

            return AtonementBarHeight + initialHeight + InterBarOffset;
        }
    }
}