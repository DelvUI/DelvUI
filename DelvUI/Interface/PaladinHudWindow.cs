using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using ImGuiNET;

namespace DelvUI.Interface {
    public class PaladinHudWindow : HudWindow {
        public PaladinHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

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
        private Dictionary<string, uint> OathNotFullColor => PluginConfiguration.MiscColorMap["partial"];

        protected override void Draw(bool _) {
            if (ManaEnabled) {
                DrawManaBar();
            }

            if (OathGaugeEnabled) {
                DrawOathGauge();
            }

            if (BuffBarEnabled) {
                DrawBuffBar();
            }

            if (AtonementEnabled) {
                DrawAtonementBar();
            }

            if (DoTBarEnabled) {
                DrawDoTBar();
            }
        }

        protected override void DrawPrimaryResourceBar() { }

        private void DrawManaBar() {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var actor = PluginInterface.ClientState.LocalPlayer;

            var posX = CenterX + BaseXOffset - ManaXOffset;
            var posY = CenterY + BaseYOffset + ManaYOffset;

            var builder = BarBuilder.Create(posX, posY, ManaBarHeight, ManaBarWidth)
                                    .SetBackgroundColor(EmptyColor["background"]);

            if (ManaChunked) {
                builder.SetChunks(5)
                       .SetChunkPadding(ManaBarPadding)
                       .AddInnerBar(actor.CurrentMp, actor.MaxMp, ManaColor, EmptyColor);
            }
            else {
                builder.AddInnerBar(actor.CurrentMp, actor.MaxMp, ManaColor);
            }

            if (ManaBarText) {
                var formattedManaText = TextTags.GenerateFormattedTextFromTags(actor, "[mana:current-short]");

                builder.SetTextMode(BarTextMode.Single)
                       .SetText(BarTextPosition.CenterLeft, BarTextType.Custom, formattedManaText);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawOathGauge() {
            var gauge = PluginInterface.ClientState.JobGauges.Get<PLDGauge>();

            var xPos = CenterX + BaseXOffset - OathGaugeXOffset;
            var yPos = CenterY + BaseYOffset + OathGaugeYOffset;

            var builder = BarBuilder.Create(xPos, yPos, OathGaugeBarHeight, OathGaugeBarWidth)
                                    .SetChunks(2)
                                    .SetChunkPadding(OathGaugeBarPadding)
                                    .SetBackgroundColor(EmptyColor["background"])
                                    .AddInnerBar(gauge.GaugeAmount, 100, OathGaugeColor, OathNotFullColor);

            if (OathGaugeText) {
                builder.SetTextMode(BarTextMode.EachChunk)
                       .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawBuffBar() {
            var fightOrFlightBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 76);
            var requiescatBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1368);

            var xPos = CenterX + BaseXOffset - BuffBarXOffset;
            var yPos = CenterY + BaseYOffset + BuffBarYOffset;

            var builder = BarBuilder.Create(xPos, yPos, BuffBarHeight, BuffBarWidth)
                                    .SetBackgroundColor(EmptyColor["background"]);

            if (fightOrFlightBuff.Any()) {
                var fightOrFlightDuration = Math.Abs(fightOrFlightBuff.First().Duration);
                builder.AddInnerBar(fightOrFlightDuration, 25, FightOrFlightColor);

                if (BuffBarText) {
                    builder.SetTextMode(BarTextMode.EachChunk)
                           .SetText(BarTextPosition.CenterLeft, BarTextType.Current, PluginConfiguration.PLDFightOrFlightColor, Vector4.UnitW, null);
                }
            }

            if (requiescatBuff.Any()) {
                var requiescatDuration = Math.Abs(requiescatBuff.First().Duration);
                builder.AddInnerBar(requiescatDuration, 12, RequiescatColor);

                if (BuffBarText) {
                    builder.SetTextMode(BarTextMode.EachChunk)
                           .SetText(BarTextPosition.CenterRight, BarTextType.Current, PluginConfiguration.PLDRequiescatColor, Vector4.UnitW, null);
                }
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawAtonementBar() {
            var atonementBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1902);
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

        private void DrawDoTBar() {
            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;

            if (target is not Chara) {
                return;
            }

            var goringBlade = target.StatusEffects.FirstOrDefault(
                o =>
                    o.EffectId == 725 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
            );

            var duration = Math.Abs(goringBlade.Duration);

            var xPos = CenterX + BaseXOffset - DoTBarXOffset;
            var yPos = CenterY + BaseYOffset + DoTBarYOffset;

            var builder = BarBuilder.Create(xPos, yPos, DoTBarHeight, DoTBarWidth)
                                    .AddInnerBar(duration, 21, DoTColor)
                                    .SetBackgroundColor(EmptyColor["background"]);

            if (DoTBarText) {
                builder.SetTextMode(BarTextMode.EachChunk)
                       .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }
    }
}
