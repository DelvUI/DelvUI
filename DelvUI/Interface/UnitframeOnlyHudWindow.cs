using Dalamud.Plugin;
using System.Diagnostics;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Gui;
using Dalamud.Interface;
using DelvUI.Config;

namespace DelvUI.Interface {
    public class UnitFrameOnlyHudWindow : HudWindow
    {
        public override uint JobId { get; }

        public UnitFrameOnlyHudWindow(
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
        ) {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");

            //  To prevent SwapJobs() from being spammed in Plugin.cs Draw()
            JobId = ClientState.LocalPlayer.ClassJob.Id;
        }

        protected override void Draw(bool _) {
        }
    }
}