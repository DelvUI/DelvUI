/*
Copyright(c) 2021 jkcclemens HUD Manager
Modifications Copyright(c) 2021 DelvUI
09/27/2021 - Extracted code to move in-game UI elements.

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

using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using DelvUI.Interface.Jobs;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DelvUI.Interface
{
    public class HudHelper : IDisposable
    {
        private HUDOptionsConfig Config => ConfigurationManager.Instance.GetConfigObject<HUDOptionsConfig>();

        private bool _previousCombatState = true;
        private bool _isInitial = true;
        private readonly uint[] _goldSaucerIDs = { 144, 388, 389, 390, 391, 579, 792, 899, 941 };

        public HudHelper()
        {
            Config.ValueChangeEvent += ConfigValueChanged;
        }

        ~HudHelper()
        {
            Dispose(false);
        }

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

            Config.ValueChangeEvent -= ConfigValueChanged;

            UpdateCombatActionBars(true);
            UpdateDefaultCastBar(true);
            UpdateDefaultPulltimer(true);
            UpdateJobGauges(true);
        }

        public void Update()
        {
            UpdateCombatActionBars(_isInitial);
            UpdateJobGauges();
            UpdateDefaultCastBar();
            UpdateDefaultPulltimer();

            _isInitial = false;
        }

        public bool IsElementHidden(HudElement element)
        {
            if (!ConfigurationManager.Instance.LockHUD) { return false; }
            if (element.GetType() == typeof(PlayerCastbarHud)) { return false; }
            if (!element.GetConfig().Enabled) { return true; }

            // hide in gold saucer
            if (Config.HideInGoldSaucer && _goldSaucerIDs.Count(id => id == Plugin.ClientState.TerritoryType) > 0) { return true; }

            bool isHidden = Config.ShowDelvUIFramesInDuty
                ? Config.HideOutsideOfCombat && !IsInCombat() && !IsInDuty()
                : Config.HideOutsideOfCombat && !IsInCombat();

            PlayerCharacter? player = Plugin.ClientState.LocalPlayer;

            if (player is not null)
            {
                isHidden = isHidden && Config.ShowDelvUIFramesOnWeaponDrawn
                    ? Config.HideOutsideOfCombat && !IsInCombat() && !HasWeaponDrawn()
                    : Config.ShowDelvUIFramesInDuty
                        ? Config.HideOutsideOfCombat && !IsInCombat() && !IsInDuty()
                        : Config.HideOutsideOfCombat && !IsInCombat();

                // hide only jobpack hud outside of combat
                if (!isHidden && element is JobHud)
                {
                    isHidden = Config.ShowJobPackInDuty
                        ? Config.HideOnlyJobPackHudOutsideOfCombat && !IsInCombat() && !IsInDuty()
                        : Config.HideOnlyJobPackHudOutsideOfCombat && !IsInCombat();

                    isHidden = isHidden && Config.ShowJobPackOnWeaponDrawn
                        ? Config.HideOnlyJobPackHudOutsideOfCombat && !IsInCombat() && !HasWeaponDrawn()
                        : Config.ShowJobPackInDuty
                            ? Config.HideOnlyJobPackHudOutsideOfCombat && !IsInCombat() && !IsInDuty()
                            : Config.HideOnlyJobPackHudOutsideOfCombat && !IsInCombat();
                }

                // hide player frame when at full health
                if (Config.HidePlayerFrameAtFullHP && element.GetConfig().GetType() == typeof(PlayerUnitFrameConfig) && !Config.HideOutsideOfCombat)
                {
                    isHidden = !isHidden && player.CurrentHp == player.MaxHp;
                }
                else if (element.GetConfig().GetType() == typeof(PlayerUnitFrameConfig))
                {
                    isHidden = Config.AlwaysHidePlayerFrameWhenDelvUIHidden ? isHidden : isHidden && player.CurrentHp == player.MaxHp;
                }
            }

            return isHidden;
        }

        private void ConfigValueChanged(object sender, OnChangeBaseArgs e)
        {
            switch (e.PropertyName)
            {
                case "HideDefaultCastbar":
                    UpdateDefaultCastBar();
                    break;
                case "HideDefaultPulltimer":
                    UpdateDefaultPulltimer();
                    break;
                case "HideDefaultJobGauges":
                case "DisableJobGaugeSounds":
                    UpdateJobGauges();
                    break;
                case "EnableCombatActionBars":
                    Config.CombatActionBars.ForEach(name => ToggleActionbar(name, Config.EnableCombatActionBars));
                    break;
                case "CombatActionBarsWithCrossHotbar":
                    ToggleCrossActionbar(Config.CombatActionBarsWithCrossHotbar);
                    break;
                case "CombatActionBars" when e is OnChangeEventArgs<string> listEvent:
                    switch (listEvent.ChangeType)
                    {
                        case ChangeType.ListAdd:
                            ToggleActionbar(listEvent.Value, !IsInCombat());
                            break;
                        case ChangeType.ListRemove:
                            ToggleActionbar(listEvent.Value, false);
                            break;
                    }

                    break;
            }
        }

        private void UpdateCombatActionBars(bool forceUpdate = false)
        {
            if (!Config.EnableCombatActionBars) { return; }

            PlayerCharacter? player = Plugin.ClientState.LocalPlayer;

            bool currentCombatState = Config.ShowCombatActionBarsInDuty
                ? IsInDuty() || IsInCombat()
                : IsInCombat();

            if (player is not null)
            {
                currentCombatState = !currentCombatState && Config.ShowCombatActionBarsOnWeaponDrawn
                    ? HasWeaponDrawn() || IsInCombat()
                    : Config.ShowCombatActionBarsInDuty
                        ? IsInDuty() || IsInCombat()
                        : IsInCombat();

                if (_previousCombatState != currentCombatState && Config.CombatActionBars.Count > 0 || forceUpdate)
                {
                    Config.CombatActionBars.ForEach(name => ToggleActionbar(name, !currentCombatState));
                    _previousCombatState = currentCombatState;
                }
                else if (_previousCombatState != currentCombatState && Config.CombatActionBarsWithCrossHotbar)
                {
                    ToggleCrossActionbar(!currentCombatState);
                    _previousCombatState = currentCombatState;
                }
            }
        }

        private void ToggleActionbar(string targetName, bool isHidden)
        {
            string[] splits = targetName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string hotbarNumber = splits.Last();
            string toggleText = isHidden ? "off" : "on";

            ChatHelper.SendChatMessage("/hotbar display " + hotbarNumber + " " + toggleText);
        }

        private void ToggleCrossActionbar(bool isHidden)
        {
            string toggleText = isHidden ? "off" : "on";

            ChatHelper.SendChatMessage("/crosshotbardisplay" + " " + toggleText);
        }

        private unsafe void SetAddonVisible(AtkUnitBase* addon, bool visible)
        {
            if (addon == null) { return; }

            if (visible && addon->UldManager.NodeListCount == 0)
            {
                addon->UldManager.UpdateDrawNodeList();

            }
            else if (!visible && addon->UldManager.NodeListCount != 0)
            {
                addon->UldManager.NodeListCount = 0;
            }
        }

        private unsafe void UpdateDefaultCastBar(bool forceVisible = false)
        {
            AtkUnitBase* addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("_CastBar", 1);
            if (addon == null) { return; }

            SetAddonVisible(addon, forceVisible || !Config.HideDefaultCastbar);
        }

        private unsafe void UpdateDefaultPulltimer(bool forceVisible = false)
        {
            AtkUnitBase* addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("ScreenInfo_CountDown", 1);
            if (addon == null) { return; }

            SetAddonVisible(addon, forceVisible || !Config.HideDefaultPulltimer);
        }

        private unsafe void UpdateJobGauges(bool forceVisible = false)
        {
            var (addons, names) = FindAddonsStartingWith("JobHud");

            for (int i = 0; i < addons.Count; i++)
            {
                AtkUnitBase* addon = (AtkUnitBase*)addons[i];
                if (addon == null) { continue; }

                SetAddonVisible(addon, forceVisible || !Config.HideDefaultJobGauges);
            }
        }

        public static unsafe (List<IntPtr>, List<string>) FindAddonsStartingWith(string startingWith)
        {
            List<IntPtr> addons = new();
            List<string> names = new();

            AtkStage* stage = AtkStage.GetSingleton();
            if (stage == null)
            {
                return (addons, names);
            }

            AtkUnitList* loadedUnitsList = &stage->RaptureAtkUnitManager->AtkUnitManager.AllLoadedUnitsList;
            if (loadedUnitsList == null) { return (addons, names); }

            AtkUnitBase** addonList = &loadedUnitsList->AtkUnitEntries;
            if (addonList == null) { return (addons, names); }

            for (int i = 0; i < loadedUnitsList->Count; i++)
            {
                AtkUnitBase* addon = addonList[i];
                if (addon == null) { continue; }

                string? name = Marshal.PtrToStringAnsi(new IntPtr(addon->Name));
                if (name == null || !name.StartsWith(startingWith)) { continue; }

                addons.Add((IntPtr)addon);
                names.Add(name);
            }

            return (addons, names);
        }
        #region Helpers

        private bool IsInCombat() => Plugin.Condition[ConditionFlag.InCombat];

        private bool IsInDuty() => Plugin.Condition[ConditionFlag.BoundByDuty];

        private bool HasWeaponDrawn() => (Plugin.ClientState.LocalPlayer != null && Plugin.ClientState.LocalPlayer.StatusFlags.HasFlag(StatusFlags.WeaponOut));

        #endregion
    }

    internal class StrataLevelComparer<TKey> : IComparer<TKey> where TKey : PluginConfigObject
    {
        public int Compare(TKey? a, TKey? b)
        {
            MovablePluginConfigObject? configA = a is MovablePluginConfigObject ? a as MovablePluginConfigObject : null;
            MovablePluginConfigObject? configB = b is MovablePluginConfigObject ? b as MovablePluginConfigObject : null;

            if (configA == null && configB == null) { return 0; }
            if (configA == null && configB != null) { return -1; }
            if (configA != null && configB == null) { return 1; }

            if (configA!.StrataLevel == configB!.StrataLevel)
            {
                return configA.ID.CompareTo(configB.ID);
            }

            if (configA.StrataLevel < configB.StrataLevel)
            {
                return -1;
            }

            return 1;
        }
    }
}
