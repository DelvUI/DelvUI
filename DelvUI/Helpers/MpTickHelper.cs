using Dalamud.Game.Internal;
using Dalamud.Plugin;
using ImGuiNET;
using System;
using System.Linq;

namespace DelvUI.Helpers
{
    internal class MpTickHelper
    {
        public const double ServerTickRate = 3;
        protected const float PollingRate = 1 / 30f;
        protected readonly DalamudPluginInterface PluginInterface;
        private int _lastMpValue = -1;
        protected double LastTickTime;
        protected double LastUpdate;

        public MpTickHelper(DalamudPluginInterface pluginInterface)
        {
            PluginInterface = pluginInterface;
            PluginInterface.Framework.OnUpdateEvent += FrameworkOnOnUpdateEvent;
        }

        public double LastTick => LastTickTime;

        private void FrameworkOnOnUpdateEvent(Framework framework)
        {
            var player = PluginInterface.ClientState.LocalPlayer;

            if (player is null)
            {
                return;
            }

            var now = ImGui.GetTime();

            if (now - LastUpdate < PollingRate)
            {
                return;
            }

            LastUpdate = now;

            var mp = player.CurrentMp;

            // account for lucid dreaming screwing up mp calculations
            var lucidDreamingActive = player.StatusEffects.Any(e => e.EffectId == 1204);

            if (!lucidDreamingActive && _lastMpValue < mp)
            {
                LastTickTime = now;
            }
            else if (LastTickTime + ServerTickRate <= now)
            {
                LastTickTime += ServerTickRate;
            }

            _lastMpValue = mp;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            PluginInterface.Framework.OnUpdateEvent -= FrameworkOnOnUpdateEvent;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MpTickHelper() { Dispose(true); }
    }
}
