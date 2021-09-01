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

namespace DelvUI.Interface
{
    public class PaladinHudWindow : HudWindow
    {
        public override uint JobId => 19;

        private bool ManaEnabled => PluginConfiguration.PLDManaEnabled;

        private bool ManaChunked => PluginConfiguration.PLDManaChunked;

        private bool ManaBarText => PluginConfiguration.PLDManaBarText;

        private int ManaBarHeight => PluginConfiguration.PLDManaHeight;
        
        private int ManaBarWidth => PluginConfiguration.PLDManaWidth;
        
        private int ManaBarPadding => PluginConfiguration.PLDManaPadding;
        
        private int ManaXOffset => PluginConfiguration.PLDManaXOffset;
        
        private int ManaYOffset => PluginConfiguration.PLDManaYOffset;
        
        private bool OathGaugeEnabled => PluginConfiguration.PLDOathGaugeEnabled;
        
        private int OathGaugeBarHeight => PluginConfiguration.PLDOathGaugeHeight;
        
        private int OathGaugeBarWidth => PluginConfiguration.PLDOathGaugeWidth;
        
        private int OathGaugeBarPadding => PluginConfiguration.PLDOathGaugePadding;
        
        private int OathGaugeXOffset => PluginConfiguration.PLDOathGaugeXOffset;
        
        private int OathGaugeYOffset => PluginConfiguration.PLDOathGaugeYOffset;
        
        private bool OathGaugeText => PluginConfiguration.PLDOathGaugeText;
        
        private bool BuffBarEnabled => PluginConfiguration.PLDBuffBarEnabled;
        
        private bool BuffBarText => PluginConfiguration.PLDBuffBarText;
        
        private int BuffBarHeight => PluginConfiguration.PLDBuffBarHeight;
        
        private int BuffBarWidth => PluginConfiguration.PLDBuffBarWidth;
        
        private int BuffBarXOffset => PluginConfiguration.PLDBuffBarXOffset;
        
        private int BuffBarYOffset => PluginConfiguration.PLDBuffBarYOffset;
        
        private bool AtonementEnabled => PluginConfiguration.PLDAtonementBarEnabled;
        
        private int AtonementBarHeight => PluginConfiguration.PLDAtonementBarHeight;
        
        private int AtonementBarWidth => PluginConfiguration.PLDAtonementBarWidth;
        
        private int AtonementBarPadding => PluginConfiguration.PLDAtonementBarPadding;
        
        private int AtonementBarXOffset => PluginConfiguration.PLDAtonementBarXOffset;
        
        private int AtonementBarYOffset => PluginConfiguration.PLDAtonementBarYOffset;
        
        private bool DoTBarEnabled => PluginConfiguration.PLDDoTBarEnabled;
        
        private int DoTBarHeight => PluginConfiguration.PLDDoTBarHeight;
        
        private int DoTBarWidth => PluginConfiguration.PLDDoTBarWidth;
        
        private int DoTBarXOffset => PluginConfiguration.PLDDoTBarXOffset;
        
        private int DoTBarYOffset => PluginConfiguration.PLDDoTBarYOffset;
        
        private bool DoTBarText => PluginConfiguration.PLDDoTBarText;
        
        private Dictionary<string, uint> ManaColor => PluginConfiguration.JobColorMap[Jobs.PLD * 1000];
        
        private Dictionary<string, uint> OathGaugeColor => PluginConfiguration.JobColorMap[Jobs.PLD * 1000 + 1];
        
        private Dictionary<string, uint> FightOrFlightColor => PluginConfiguration.JobColorMap[Jobs.PLD * 1000 + 2];
        
        private Dictionary<string, uint> RequiescatColor => PluginConfiguration.JobColorMap[Jobs.PLD * 1000 + 3];
        
        private Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.PLD * 1000 + 4];
        
        private Dictionary<string, uint> AtonementColor => PluginConfiguration.JobColorMap[Jobs.PLD * 1000 + 5];

        private Dictionary<string, uint> DoTColor => PluginConfiguration.JobColorMap[Jobs.PLD * 1000 + 6];

        public PaladinHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) {}

        protected override void Draw(bool _)
        {
            if (ManaEnabled)
                DrawManaBar();
            if (OathGaugeEnabled)
                DrawOathGauge();
            if (BuffBarEnabled)
                DrawBuffBar();
            if (AtonementEnabled)
                DrawAtonementBar();
            if (DoTBarEnabled)
                DrawDoTBar();
        }

        protected override void DrawPrimaryResourceBar()
        {
        }

        private void DrawManaBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var actor = PluginInterface.ClientState.LocalPlayer;

            var posX = CenterX - ManaXOffset;
            var posY = CenterY + ManaYOffset;

            var builder = BarBuilder.Create(posX, posY, ManaBarHeight, ManaBarWidth);

            if(ManaChunked)
            {
                builder.SetChunks(5)
                       .SetChunkPadding(ManaBarPadding)
                       .AddInnerBar(actor.CurrentMp, actor.MaxMp, ManaColor, EmptyColor);
            } else
            {
                builder.AddInnerBar(actor.CurrentMp, actor.MaxMp, ManaColor);
            }

            if (ManaBarText)
            {
                var formattedManaText = Helpers.TextTags.GenerateFormattedTextFromTags(actor, "[mana:current-short]");
                builder.SetTextMode(BarTextMode.Single)
                       .SetText(BarTextPosition.CenterLeft, BarTextType.Custom, formattedManaText);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawOathGauge()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<PLDGauge>();

            var xPos = CenterX - OathGaugeXOffset;
            var yPos = CenterY + OathGaugeYOffset;

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
        }

        private void DrawBuffBar()
        {
            var fightOrFlightBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 76);
            var requiescatBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1368);

            var xPos = CenterX - BuffBarXOffset;
            var yPos = CenterY + BuffBarYOffset;

            var builder = BarBuilder.Create(xPos, yPos, BuffBarHeight, BuffBarWidth);

            if (fightOrFlightBuff.Any())
            {
                var fightOrFlightDuration = Math.Abs(fightOrFlightBuff.First().Duration);
                builder.AddInnerBar(fightOrFlightDuration, 25, FightOrFlightColor);
                if (BuffBarText)
                    builder.SetTextMode(BarTextMode.EachChunk)
                        .SetText(BarTextPosition.CenterLeft, BarTextType.Current, PluginConfiguration.PLDFightOrFlightColor, Vector4.UnitW, null);
            }

            if (requiescatBuff.Any())
            {
                var requiescatDuration = Math.Abs(requiescatBuff.First().Duration);
                builder.AddInnerBar(requiescatDuration, 12, RequiescatColor);
                if (BuffBarText)
                    builder.SetTextMode(BarTextMode.EachChunk)
                        .SetText(BarTextPosition.CenterRight, BarTextType.Current, PluginConfiguration.PLDRequiescatColor, Vector4.UnitW, null);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawAtonementBar()
        {
            var atonementBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1902);
            var stackCount = atonementBuff.Any() ? atonementBuff.First().StackCount : 0;

            var xPos = CenterX - AtonementBarXOffset;
            var yPos = CenterY + AtonementBarYOffset;

            var builder = BarBuilder.Create(xPos, yPos, AtonementBarHeight, AtonementBarWidth)
                .SetChunks(3)
                .SetChunkPadding(AtonementBarPadding)
                .AddInnerBar(stackCount, 3, AtonementColor, null);

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawDoTBar()
        {
            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;

            if (target is not Chara)
                return;

            var goringBlade = target.StatusEffects.FirstOrDefault(o =>
                o.EffectId == 725 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId);
            var duration = Math.Abs(goringBlade.Duration);
            
            var xPos = CenterX - DoTBarXOffset;
            var yPos = CenterY + DoTBarYOffset;

            var builder = BarBuilder.Create(xPos, yPos, DoTBarHeight, DoTBarWidth)
                .AddInnerBar(duration, 21, DoTColor);

            if (DoTBarText)
            {
                builder.SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }
    }
}