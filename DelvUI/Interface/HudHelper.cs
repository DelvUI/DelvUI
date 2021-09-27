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
using Dalamud.Game.ClientState.Objects.SubKinds;
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
        private delegate void SetPositionDelegate(IntPtr addon, short x, short y);
        private delegate IntPtr GetBaseUIObjectDelegate();
        private delegate byte UpdateAddonPositionDelegate(IntPtr manager, IntPtr addon, byte clicked);

        private HideHudConfig Config => ConfigurationManager.Instance.GetConfigObject<HideHudConfig>();

        private bool _previousCombatState = true;
        private bool _isInitial = true;
        private uint[] GoldSaucerIDs = new uint[] { 144, 388, 389, 390, 391, 579, 792, 899, 941 };

        private GetBaseUIObjectDelegate? _getBaseUIObject;
        private SetPositionDelegate? _setPosition;
        private UpdateAddonPositionDelegate? _updateAddonPosition;

        public HudHelper()
        {
            Config.ValueChangeEvent += ConfigValueChanged;

            var getBaseUiObjectPtr = Plugin.SigScanner.ScanText("E8 ?? ?? ?? ?? 41 B8 01 00 00 00 48 8D 15 ?? ?? ?? ?? 48 8B 48 20 E8 ?? ?? ?? ?? 48 8B CF");
            _getBaseUIObject = Marshal.GetDelegateForFunctionPointer<GetBaseUIObjectDelegate>(getBaseUiObjectPtr);

            var setPositionPtr = Plugin.SigScanner.ScanText("4C 8B 89 ?? ?? ?? ?? 41 0F BF C0");
            _setPosition = Marshal.GetDelegateForFunctionPointer<SetPositionDelegate>(setPositionPtr);

            var updateAddonPositionPtr = Plugin.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B 8B ?? ?? ?? ?? 33 D2 48 8B 01 FF 90 ?? ?? ?? ??");
            _updateAddonPosition = Marshal.GetDelegateForFunctionPointer<UpdateAddonPositionDelegate>(updateAddonPositionPtr);
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

            UpdateCombatActionBars(true, true);
            UpdateDefaultCastBar(true);
            UpdateJobGauges(true);
        }

        public void Update()
        {
            UpdateCombatActionBars(_isInitial);
            UpdateJobGauges();

            if (_isInitial)
            {
                UpdateDefaultCastBar();
            }

            _isInitial = false;
        }

        public bool IsElementHidden(HudElement element)
        {
            if (!ConfigurationManager.Instance.LockHUD)
            {
                return ConfigurationManager.Instance.LockHUD;
            }

            if (element.GetType() == typeof(PlayerCastbarHud))
            {
                return false;
            }

            // hide in gold saucer
            if (Config.HideInGoldSaucer && GoldSaucerIDs.Where(id => id == Plugin.ClientState.TerritoryType).Count() > 0)
            {
                return true;
            }

            bool isHidden = Config.HideOutsideOfCombat && !IsInCombat();
            if (!isHidden && element is JobHud)
            {
                return Config.HideOnlyJobPackHudOutsideOfCombat && !IsInCombat();
            }

            if (element.GetConfig().GetType() == typeof(PlayerUnitFrameConfig))
            {
                PlayerCharacter? player = Plugin.ClientState.LocalPlayer;
                if (player is not null)
                {
                    isHidden = isHidden && player.CurrentHp == player.MaxHp;
                }
            }

            return isHidden;
        }

        private void ConfigValueChanged(object sender, OnChangeBaseArgs e)
        {
            if (e.PropertyName == "HideDefaultCastbar")
            {
                UpdateDefaultCastBar();
            }
            else if (e.PropertyName == "HideDefaultJobGauges" || e.PropertyName == "DisableJobGaugeSounds")
            {
                UpdateJobGauges();
            }
            else if (e.PropertyName == "EnableCombatActionBars")
            {
                Config.CombatActionBars.ForEach(name => ToggleActionbar(name, Config.EnableCombatActionBars));
            }
            else if (e.PropertyName == "CombatActionBars" && e is OnChangeEventArgs<string> listEvent)
            {
                switch (listEvent.ChangeType)
                {
                    case ChangeType.ListAdd:
                        ToggleActionbar(listEvent.Value, !IsInCombat());
                        break;
                    case ChangeType.ListRemove:
                        ToggleActionbar(listEvent.Value, false);
                        break;
                }
            }
        }

        private void UpdateCombatActionBars(bool forceUpdate = false, bool forceVisible = false)
        {
            if (!Config.EnableCombatActionBars)
            {
                return;
            }

            var currentCombatState = IsInCombat();
            if (_previousCombatState != currentCombatState && Config.CombatActionBars.Count > 0 || forceUpdate)
            {
                Config.CombatActionBars.ForEach(name => ToggleActionbar(name, !currentCombatState));
                _previousCombatState = currentCombatState;
            }
        }

        private unsafe void ToggleActionbar(string targetName, bool isHidden)
        {
            string[] splits = targetName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var hotbarNumber = splits.Last();
            var toggleText = isHidden ? "off" : "on";

            ChatHelper.SendChatMessage("/hotbar display " + hotbarNumber + " " + toggleText);
        }

        private void SetAddonVisible(IntPtr addon, bool visible, Vector2 originalPosition)
        {
            if (_getBaseUIObject == null || _setPosition == null || _updateAddonPosition == null)
            {
                return;
            }

            var baseUi = _getBaseUIObject();
            var manager = Marshal.ReadIntPtr(baseUi + 0x20);

            short x = visible ? (short)originalPosition.X : (short)32000;
            short y = visible ? (short)originalPosition.Y : (short)32000;

            _updateAddonPosition(manager, addon, 1);
            _setPosition(addon, x, y);
            _updateAddonPosition(manager, addon, 0);
        }

        private unsafe bool UpdateAddonOriginalPosition(AtkUnitBase* addon, ref Vector2 originalPosition)
        {
            const float outOfScreen = 30000f;
            var addonPosition = new Vector2(addon->X, addon->Y);

            if (addonPosition != Vector2.Zero && addonPosition.X < outOfScreen && addonPosition.Y < outOfScreen)
            {
                originalPosition = addonPosition;
            }

            // in case something goes wrong, restore position to 0 so the element is not permanently lost off screen
            if (originalPosition.X > outOfScreen || originalPosition.Y > outOfScreen)
            {
                originalPosition = Vector2.Zero;
            }

            return (addonPosition.X < outOfScreen || addonPosition.Y < outOfScreen);
        }

        private unsafe void UpdateDefaultCastBar(bool forceVisible = false)
        {
            var addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("_CastBar", 1);

            var previousPos = Config.CastBarOriginalPosition;
            var isVisible = UpdateAddonOriginalPosition(addon, ref Config.CastBarOriginalPosition);

            if (previousPos != Config.CastBarOriginalPosition)
            {
                ConfigurationManager.Instance.SaveConfigurations();
            }

            if (isVisible != Config.HideDefaultCastbar)
            {
                return;
            }

            SetAddonVisible((IntPtr)addon, forceVisible || !Config.HideDefaultCastbar, Config.CastBarOriginalPosition);
        }

        private unsafe void UpdateJobGauges(bool forceVisible = false)
        {
            var (addons, names) = FindAddonsStartingWith("JobHud");

            for (int i = 0; i < addons.Count; i++)
            {
                var addon = (AtkUnitBase*)addons[i];
                var name = names[i];

                Vector2 pos = Vector2.Zero;
                bool existed = Config.JobGaugeOriginalPosition.TryGetValue(name, out pos);

                Vector2 previousPos = pos;
                UpdateAddonOriginalPosition(addon, ref pos);

                if (previousPos != pos || !existed)
                {
                    Config.JobGaugeOriginalPosition[name] = pos;
                    ConfigurationManager.Instance.SaveConfigurations();
                }

                SetAddonVisible((IntPtr)addon, forceVisible || !Config.HideDefaultJobGauges, Config.JobGaugeOriginalPosition[name]);
            }
        }

        public static unsafe (List<IntPtr>, List<string>) FindAddonsStartingWith(string startingWith)
        {
            var addons = new List<IntPtr>();
            var names = new List<string>();

            var stage = AtkStage.GetSingleton();
            if (stage == null)
            {
                return (addons, names);
            }

            var loadedUnitsList = &stage->RaptureAtkUnitManager->AtkUnitManager.AllLoadedUnitsList;
            if (loadedUnitsList == null)
            {
                return (addons, names);
            }

            var addonList = &loadedUnitsList->AtkUnitEntries;
            if (addonList == null)
            {
                return (addons, names);
            }

            for (var i = 0; i < loadedUnitsList->Count; i++)
            {
                AtkUnitBase* addon = addonList[i];
                if (addon == null)
                {
                    continue;
                }

                var name = Marshal.PtrToStringAnsi(new IntPtr(addon->Name));
                if (name == null || !name.StartsWith(startingWith))
                {
                    continue;
                }

                addons.Add((IntPtr)addon);
                names.Add(name);
            }

            return (addons, names);
        }
        #region Helpers

        private bool IsInCombat()
        {
            return Plugin.Condition[ConditionFlag.InCombat];
        }

        private bool IsInDuty()
        {
            return Plugin.Condition[ConditionFlag.BoundByDuty];
        }
        #endregion
    }
}
