using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui;
using Dalamud.Interface;
using Dalamud.Plugin;
using DelvUI.Interface.Bars;
using ImGuiNET;
using DelvUI.Config;

namespace DelvUI.Interface
{
    public class PaladinHudWindow : HudWindow
    {
        public override uint JobId => 19;

        private int BaseXOffset => PluginConfiguration.PLDBaseXOffset;

        private int BaseYOffset => PluginConfiguration.PLDBaseYOffset;

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

        private Dictionary<string, uint> OathNotFullColor => PluginConfiguration.JobColorMap[Jobs.PLD * 1000 + 7];

        public PaladinHudWindow(
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
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var actor = ClientState.LocalPlayer;

            var posX = CenterX + BaseXOffset - ManaXOffset;
            var posY = CenterY + BaseYOffset + ManaYOffset;

            var builder = BarBuilder.Create(posX, posY, ManaBarHeight, ManaBarWidth)                
                .SetBackgroundColor(EmptyColor["background"]);
            
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
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawOathGauge()
        {
            var gauge = JobGauges.Get<PLDGauge>();

            var xPos = CenterX + BaseXOffset - OathGaugeXOffset;
            var yPos = CenterY + BaseYOffset + OathGaugeYOffset;

            var builder = BarBuilder.Create(xPos, yPos, OathGaugeBarHeight, OathGaugeBarWidth)
                .SetChunks(2)
                .SetChunkPadding(OathGaugeBarPadding)
                .SetBackgroundColor(EmptyColor["background"])
                .AddInnerBar(gauge.OathGauge, 100, OathGaugeColor, OathNotFullColor);

            if (OathGaugeText) {
                builder.SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawBuffBar()
        {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var fightOrFlightBuff = ClientState.LocalPlayer.StatusList.Where(o => o.StatusId == 76);
            var requiescatBuff = ClientState.LocalPlayer.StatusList.Where(o => o.StatusId == 1368);

            var xPos = CenterX + BaseXOffset - BuffBarXOffset;
            var yPos = CenterY + BaseYOffset + BuffBarYOffset;

            var builder = BarBuilder.Create(xPos, yPos, BuffBarHeight, BuffBarWidth)                
                .SetBackgroundColor(EmptyColor["background"]);

            if (fightOrFlightBuff.Any())
            {
                var fightOrFlightDuration = Math.Abs(fightOrFlightBuff.First().RemainingTime);
                builder.AddInnerBar(fightOrFlightDuration, 25, FightOrFlightColor);
                if (BuffBarText)
                    builder.SetTextMode(BarTextMode.EachChunk)
                        .SetText(BarTextPosition.CenterLeft, BarTextType.Current, PluginConfiguration.PLDFightOrFlightColor, Vector4.UnitW, null);
            }

            if (requiescatBuff.Any())
            {
                var requiescatDuration = Math.Abs(requiescatBuff.First().RemainingTime);
                builder.AddInnerBar(requiescatDuration, 12, RequiescatColor);
                if (BuffBarText)
                    builder.SetTextMode(BarTextMode.EachChunk)
                        .SetText(BarTextPosition.CenterRight, BarTextType.Current, PluginConfiguration.PLDRequiescatColor, Vector4.UnitW, null);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawAtonementBar()
        {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var atonementBuff = ClientState.LocalPlayer.StatusList.Where(o => o.StatusId == 1902);
            var stackCount = atonementBuff.Any() ? atonementBuff.First().StackCount : 0;

            var xPos = CenterX + BaseXOffset - AtonementBarXOffset;
            var yPos = CenterY + BaseYOffset + AtonementBarYOffset;

            var builder = BarBuilder.Create(xPos, yPos, AtonementBarHeight, AtonementBarWidth)
                .SetChunks(3)
                .SetChunkPadding(AtonementBarPadding)
                .SetBackgroundColor(EmptyColor["background"])
                .AddInnerBar(stackCount, 3, AtonementColor, null);

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawDoTBar()
        {
            var actor = TargetManager.SoftTarget ?? TargetManager.Target;

            if (actor is not BattleChara target)
                return;

            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var goringBlade = target.StatusList.FirstOrDefault(o => o.StatusId == 725 && o.SourceID == ClientState.LocalPlayer.ObjectId);
            var duration = Math.Abs(goringBlade?.RemainingTime ?? 0f);
            
            var xPos = CenterX + BaseXOffset - DoTBarXOffset;
            var yPos = CenterY + BaseYOffset + DoTBarYOffset;

            var builder = BarBuilder.Create(xPos, yPos, DoTBarHeight, DoTBarWidth)
                .AddInnerBar(duration, 21, DoTColor)
                .SetBackgroundColor(EmptyColor["background"]);


            if (DoTBarText)
            {
                builder.SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }
    }
}