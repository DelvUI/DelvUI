using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Structs;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUIPlugin.Interface {
    public class UnitframeOnlyHudWindow : HudWindow {
        public override uint JobId => 23;

        private int BarHeight => 20;
        private int SmallBarHeight => 10;
        private int BarWidth => 250;
        private new int XOffset => 127;
        private new int YOffset => 440;
        
        public UnitframeOnlyHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _) {
            DrawHealthBar();
            DrawPrimaryResourceBar();
            DrawTargetBar();
            DrawFocusBar();
        }
        
    }
}