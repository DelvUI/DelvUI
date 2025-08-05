/*
Copyright(c) 2021 Arrenton (https://github.com/Arrenton/KingdomHeartsPlugin/)
Modifications Copyright(c) 2021 DelvUI
09/21/2021 - Extracted code to hook the game's limit break functions.

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

using System;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Hooking;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace DelvUI.Helpers
{
    public unsafe class LimitBreakHelper
    {
        #region Singleton

        private LimitBreakHelper()
        {
            LimitBreakBarWidth = new int[5];
        }

        public static void Initialize() { Instance = new LimitBreakHelper(); }

        public static LimitBreakHelper Instance { get; private set; } = null!;

        ~LimitBreakHelper() { Dispose(false); }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            Instance = null!;
        }

        #endregion

        public bool LimitBreakActive { get; set; }
        public int LimitBreakLevel { get; set; }
        public int LimitBreakMaxLevel { get; set; }
        public int[] LimitBreakBarWidth;
        public int MaxLimitBarWidth { get; set; }

        public void Update()
        {
            //Get Limit Break Bar
            AtkUnitBase* LBWidget = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("_LimitBreak", 1).Address;
            //Get Compressed Aether Bar
            AtkUnitBase* CAWidget = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("HWDAetherGauge", 1).Address;

            var foundCaGauge = false;

            LimitBreakActive = false;
            LimitBreakMaxLevel = 1;
            MaxLimitBarWidth = 128;

            // Diadem Compatibility
            if (CAWidget != null && CAWidget->UldManager.NodeListCount == 10)
            {
                if ((CAWidget->UldManager.SearchNodeById(3)->Alpha_2 > 0 && CAWidget->UldManager.SearchNodeById(3)->IsVisible()))
                {
                    bool usedAuger = CAWidget->UldManager.SearchNodeById(10)->IsVisible();
                    for (uint i = 0; i < 5; i++)
                    {
                        AtkResNode* node = CAWidget->UldManager.SearchNodeById(5 + i + (usedAuger ? 1u : 0))->GetComponent()->UldManager.SearchNodeById(3);

                        if (LimitBreakBarWidth != null)
                        {
                            LimitBreakBarWidth[i] = node->IsVisible() ? node->Width - 14 : 0;
                        }
                    }

                    MaxLimitBarWidth = 80;
                    LimitBreakMaxLevel = 5;
                    foundCaGauge = true;
                }
            }

            if (!foundCaGauge)
            {
                // Get LB Width
                if (LBWidget == null || LBWidget->UldManager.NodeListCount != 6)
                {
                    return;
                }

                AtkResNode* node = LBWidget->UldManager.SearchNodeById(3);
                if (node == null)
                {
                    return;
                }

                if (node->Alpha_2 == 0 || !node->IsVisible())
                {
                    return;
                }

                for (uint i = 0; i < 3; i++)
                {
                    if (LimitBreakBarWidth != null)
                    {
                        LimitBreakBarWidth[i] = LBWidget->UldManager.SearchNodeById(6 - i)->GetComponent()->UldManager.SearchNodeById(3)->Width - 18;
                    }

                    if (LBWidget->UldManager.SearchNodeById(6 - i)->IsVisible() && i > 0)
                    {
                        LimitBreakMaxLevel++;
                    }
                }
            }

            // Set Limit Break Level
            LimitBreakLevel = 0;
            LimitBreakActive = true;

            if (LimitBreakBarWidth == null)
            {
                return;
            }

            foreach (int barWidth in LimitBreakBarWidth)
            {
                if (barWidth == MaxLimitBarWidth)
                {
                    LimitBreakLevel++;
                }
            }
        }
    }
}