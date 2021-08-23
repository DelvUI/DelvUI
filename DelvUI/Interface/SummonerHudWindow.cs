using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
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
    public class SummonerHudWindow : HudWindow
    {
        public override uint JobId => 27;
        private new int XOffset => PluginConfiguration.SmnBaseXOffset;
        private new int YOffset => PluginConfiguration.SmnBaseYOffset;
        private int MiasmaBarWidth => PluginConfiguration.SmnMiasmaBarWidth;
        private int MiasmaBarHeight => PluginConfiguration.SmnMiasmaBarHeight;
        private int MiasmaBarXOffset => PluginConfiguration.SmnMiasmaBarXOffset;
        private int MiasmaBarYOffset => PluginConfiguration.SmnMiasmaBarYOffset;
        private bool MiasmaBarInverted => PluginConfiguration.SmnMiasmaBarFlipped;
        private int BioBarWidth => PluginConfiguration.SmnBioBarWidth;
        private int BioBarHeight => PluginConfiguration.SmnBioBarHeight;
        private int BioBarXOffset => PluginConfiguration.SmnBioBarXOffset;
        private int BioBarYOffset => PluginConfiguration.SmnBioBarYOffset;
        private bool ShowMiasmaBar => PluginConfiguration.SmnMiasmaBarEnabled;
        private bool ShowBioBar => PluginConfiguration.SmnBioBarEnabled;
        private bool BioBarInverted => PluginConfiguration.SmnBioBarFlipped;
        private bool ShowRuinBar => PluginConfiguration.SmnRuinBarEnabled;
        private int RuinBarXOffset => PluginConfiguration.SmnRuinBarXOffset;
        private int RuinBarYOffset => PluginConfiguration.SmnRuinBarYOffset;
        private int RuinBarHeight => PluginConfiguration.SmnRuinBarHeight;
        private int RuinBarWidth => PluginConfiguration.SmnRuinBarWidth;
        private int RuinBarPadding => PluginConfiguration.SmnRuinBarPadding;
        private bool ShowAetherBar => PluginConfiguration.SmnAetherBarEnabled;
        private int AetherBarXOffset => PluginConfiguration.SmnAetherBarXOffset;
        private int AetherBarYOffset => PluginConfiguration.SmnAetherBarYOffset;
        private int AetherBarHeight => PluginConfiguration.SmnAetherBarHeight;
        private int AetherBarWidth => PluginConfiguration.SmnAetherBarWidth;
        private int AetherBarPadding => PluginConfiguration.SmnAetherBarPadding;
        private Dictionary<string, uint> SmnAetherColor => PluginConfiguration.JobColorMap[Jobs.SMN * 1000];
        private Dictionary<string, uint> SmnRuinColor => PluginConfiguration.JobColorMap[Jobs.SMN * 1000 + 1];
        private Dictionary<string, uint> SmnEmptyColor => PluginConfiguration.JobColorMap[Jobs.SMN * 1000 + 2];
        private Dictionary<string, uint> SmnMiasmaColor => PluginConfiguration.JobColorMap[Jobs.SMN * 1000 + 3];
        private Dictionary<string, uint> SmnBioColor => PluginConfiguration.JobColorMap[Jobs.SMN * 1000 + 4];
        private Dictionary<string, uint> SmnExpiryColor => PluginConfiguration.JobColorMap[Jobs.SMN * 1000 + 5];

        public SummonerHudWindow(
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
            DrawRuinBar();
            DrawAetherBar();
        }

        protected override void DrawPrimaryResourceBar() {
        }

        private void DrawActiveDots()
        {
            var actor = TargetManager.SoftTarget ?? TargetManager.Target;
            if (!ShowBioBar && !ShowMiasmaBar) return;
            if (actor is not BattleChara target) return;
            var xPos = CenterX - XOffset + MiasmaBarXOffset;
            var yPos = CenterY + YOffset + MiasmaBarYOffset;
            var barDrawList = new List<Bar>();

            if (ShowMiasmaBar)
            {
                Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
                var miasma = target.StatusList.FirstOrDefault(o => o.StatusId == 1215 && o.SourceID == ClientState.LocalPlayer.ObjectId ||
                                                                      o.StatusId == 180 && o.SourceID == ClientState.LocalPlayer.ObjectId);
                var miasmaDuration = Math.Abs(miasma?.RemainingTime ?? 0f);
                var miasmaColor = miasmaDuration > 5 ? SmnMiasmaColor : SmnExpiryColor;
                var builder = BarBuilder.Create(xPos, yPos, MiasmaBarHeight, MiasmaBarWidth );
                var miasmaBar = builder.AddInnerBar(miasmaDuration, 30f, miasmaColor)
                    .SetFlipDrainDirection(MiasmaBarInverted)
                    .Build();
                barDrawList.Add(miasmaBar);
            }

            if (ShowBioBar)
            {
                Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
                var bio = target.StatusList.FirstOrDefault(o => o.StatusId == 1214 && o.SourceID == ClientState.LocalPlayer.ObjectId ||
                                                                   o.StatusId == 179 && o.SourceID == ClientState.LocalPlayer.ObjectId ||
                                                                   o.StatusId == 189 && o.SourceID == ClientState.LocalPlayer.ObjectId);
                var bioDuration = Math.Abs(bio?.RemainingTime ?? 0f);
                var bioColor = bioDuration > 5 ? SmnBioColor : SmnExpiryColor;
                xPos = CenterX - XOffset + BioBarXOffset;
                yPos = CenterY + YOffset + BioBarYOffset;
                var builder = BarBuilder.Create(xPos, yPos, BioBarHeight, BioBarWidth);
                var bioBar = builder.AddInnerBar(bioDuration, 30f, bioColor)
                    .SetFlipDrainDirection(BioBarInverted)
                    .Build();
                barDrawList.Add(bioBar);
            }
            
            if (barDrawList.Count > 0)
            {
                var drawList = ImGui.GetWindowDrawList();
                foreach (var bar in barDrawList)
                {
                    bar.Draw(drawList, PluginConfiguration);
                }
            }
        }
        private void DrawRuinBar()
        {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var ruinBuff = ClientState.LocalPlayer.StatusList.FirstOrDefault(o => o.StatusId == 1212);
            var xPos = CenterX - XOffset + RuinBarXOffset;
            var yPos = CenterY + YOffset + RuinBarYOffset;
            if (!ShowRuinBar) return;
            var bar = BarBuilder.Create(xPos, yPos, RuinBarHeight, RuinBarWidth)
                .SetChunks(4)
                .SetChunkPadding(RuinBarPadding)
                .AddInnerBar(ruinBuff?.StackCount ?? 0, 4, SmnRuinColor).SetBackgroundColor(SmnEmptyColor["background"])
                .Build();
            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }
        private void DrawAetherBar()
        {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var aetherFlowBuff = ClientState.LocalPlayer.StatusList.FirstOrDefault(o => o.StatusId == 304);
            var xPos = CenterX - XOffset + AetherBarXOffset;
            var yPos = CenterY + YOffset + AetherBarYOffset;
            if (!ShowAetherBar) return;
            var bar = BarBuilder.Create(xPos, yPos, AetherBarHeight, AetherBarWidth)
                .SetChunks(2)
                .SetChunkPadding(AetherBarPadding)
                .AddInnerBar(aetherFlowBuff?.StackCount ?? 0, 2, SmnAetherColor).SetBackgroundColor(SmnEmptyColor["background"])
                .Build();
            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }
    }
}