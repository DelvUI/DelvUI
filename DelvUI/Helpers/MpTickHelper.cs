/*
Copyright(c) 2021 talimity (https://github.com/talimity/mptimer)
Modifications Copyright(c) 2021 DelvUI
08/29/2021 - Mostly using original's code with minimal adaptations
for DelvUI.

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using ImGuiNET;
using System;
using System.Linq;
using Dalamud.Game;
using Dalamud.Plugin.Services;

namespace DelvUI.Helpers
{
    internal class MPTickHelper : IDisposable
    {
        public const double ServerTickRate = 3;
        protected const float PollingRate = 1 / 30f;
        private int _lastMpValue = -1;
        protected double LastTickTime;
        protected double LastUpdate;

        public MPTickHelper()
        {
            Plugin.Framework.Update += FrameworkOnOnUpdateEvent;
        }

        public double LastTick => LastTickTime;

        private void FrameworkOnOnUpdateEvent(IFramework framework)
        {
            var player = Plugin.ClientState.LocalPlayer;
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
            var lucidDreamingActive = Utils.StatusListForBattleChara(player).Any(e => e.StatusId == 1204);

            if (!lucidDreamingActive && _lastMpValue < mp)
            {
                LastTickTime = now;
            }
            else if (LastTickTime + ServerTickRate <= now)
            {
                LastTickTime += ServerTickRate;
            }

            _lastMpValue = (int)mp;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            Plugin.Framework.Update -= FrameworkOnOnUpdateEvent;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MPTickHelper()
        {
            Dispose(false);
        }
    }
}
