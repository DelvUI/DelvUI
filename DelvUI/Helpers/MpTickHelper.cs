using System;
using System.Linq;
using Dalamud.Game.Internal;
using Dalamud.Plugin;
using ImGuiNET;
using System.Diagnostics;


namespace DelvUI.Helpers
{
    class MpTickHelper
    {
        protected readonly DalamudPluginInterface pluginInterface;

        public const double serverTickRate = 3;
        protected const float pollingRate = 1 / 30f;
        protected double lastUpdate = 0;
        protected double lastTickTime = 0;
        private int lastMpValue = -1;

        public double lastTick { get { return lastTickTime; } }

        public MpTickHelper(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
            this.pluginInterface.Framework.OnUpdateEvent += FrameworkOnOnUpdateEvent;
        }

        private void FrameworkOnOnUpdateEvent(Framework framework)
        {
            if (pluginInterface.ClientState.LocalPlayer == null) return;

            var now = ImGui.GetTime();
            if (now - lastUpdate < pollingRate)
            {
                return;
            }
            lastUpdate = now;

            var mp = pluginInterface.ClientState.LocalPlayer.CurrentMp;

            // account for lucid dreaming screwing up mp calculations
            var lucidDreamingActive = pluginInterface.ClientState.LocalPlayer.StatusEffects.Any(e => e.EffectId == 1204);
            if (!lucidDreamingActive && lastMpValue < mp)
            {
                lastTickTime = now;
            }
            else if (lastTickTime + serverTickRate <= now)
            {
                lastTickTime += serverTickRate;
            }

            lastMpValue = mp;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            pluginInterface.Framework.OnUpdateEvent -= FrameworkOnOnUpdateEvent;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MpTickHelper()
        {
            Dispose(true);
        }
    }
}
