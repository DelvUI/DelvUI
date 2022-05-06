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
        private delegate void SetPositionDelegate(IntPtr addon, short x, short y);
        private delegate IntPtr GetBaseUIObjectDelegate();
        private delegate byte UpdateAddonPositionDelegate(IntPtr manager, IntPtr addon, byte clicked);
        private delegate IntPtr GetFilePointerDelegate(byte index);

        private HUDOptionsConfig Config => ConfigurationManager.Instance.GetConfigObject<HUDOptionsConfig>();

        private bool _previousCombatState = true;
        private bool _isInitial = true;
        private readonly uint[] _goldSaucerIDs = { 144, 388, 389, 390, 391, 579, 792, 899, 941 };

        private GetBaseUIObjectDelegate? _getBaseUIObject;
        private SetPositionDelegate? _setPosition;
        private UpdateAddonPositionDelegate? _updateAddonPosition;
        private GetFilePointerDelegate? _getFilePointer = null;

        public HudHelper()
        {
            #region Signatures
            Config.ValueChangeEvent += ConfigValueChanged;

            /*
             Part of getBaseUiObject disassembly signature
            .text:00007FF6481C2F60                   Component__GUI__AtkStage_GetSingleton1 proc near
            .text:00007FF6481C2F60 48 8B 05 99 04 8D+mov     rax, cs:g_AtkStage
            .text:00007FF6481C2F60 01
            .text:00007FF6481C2F67 C3                retn
            .text:00007FF6481C2F67                   Component__GUI__AtkStage_GetSingleton1 endp
            .text:00007FF6481C2F67
            */
            IntPtr getBaseUiObjectPtr = Plugin.SigScanner.ScanText("E8 ?? ?? ?? ?? 0F BF D5");
            _getBaseUIObject = Marshal.GetDelegateForFunctionPointer<GetBaseUIObjectDelegate>(getBaseUiObjectPtr);

            /*
             Part of setPosition disassembly signature
            .text:00007FF6481BFF20                   Component__GUI__AtkUnitBase_SetPosition proc near
            .text:00007FF6481BFF20 4C 8B 89 C8 00 00+mov     r9, [rcx+0C8h]
            .text:00007FF6481BFF20 00
            .text:00007FF6481BFF27 41 0F BF C0       movsx   eax, r8w
            .text:00007FF6481BFF2B 66 89 91 BC 01 00+mov     [rcx+1BCh], dx
            .text:00007FF6481BFF2B 00
            .text:00007FF6481BFF32 66 44 89 81 BE 01+mov     [rcx+1BEh], r8w
            .text:00007FF6481BFF32 00 00
            .text:00007FF6481BFF3A 66 0F 6E C8       movd    xmm1, eax
            .text:00007FF6481BFF3E 0F BF C2          movsx   eax, dx
            .text:00007FF6481BFF41 0F 5B C9          cvtdq2ps xmm1, xmm1
            .text:00007FF6481BFF44 66 0F 6E D0       movd    xmm2, eax
            .text:00007FF6481BFF48 0F 5B D2          cvtdq2ps xmm2, xmm2
            .text:00007FF6481BFF4B 4D 85 C9          test    r9, r9
            .text:00007FF6481BFF4E 74 3B             jz      short locret_7FF6481BFF8B
            */
            IntPtr setPositionPtr = Plugin.SigScanner.ScanText("4C 8B 89 ?? ?? ?? ?? 41 0F BF C0");
            _setPosition = Marshal.GetDelegateForFunctionPointer<SetPositionDelegate>(setPositionPtr);

            /*
            Part of updateAddonPosition disassembly signature
            .text:00007FF6481CF020                   sub_7FF6481CF020 proc near
            .text:00007FF6481CF020
            .text:00007FF6481CF020                   arg_0= qword ptr  8
            .text:00007FF6481CF020
            .text:00007FF6481CF020 48 89 5C 24 08    mov     [rsp+arg_0], rbx
            .text:00007FF6481CF025 57                push    rdi
            .text:00007FF6481CF026 48 83 EC 20       sub     rsp, 20h
            .text:00007FF6481CF02A 48 8B DA          mov     rbx, rdx
            .text:00007FF6481CF02D 48 8B F9          mov     rdi, rcx
            .text:00007FF6481CF030 48 85 D2          test    rdx, rdx
            .text:00007FF6481CF033 0F 84 CA 00 00 00 jz      loc_7FF6481CF103
            */
            IntPtr updateAddonPositionPtr = Plugin.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B 8B ?? ?? ?? ?? 33 D2 48 8B 01 FF 90 ?? ?? ?? ??");
            _updateAddonPosition = Marshal.GetDelegateForFunctionPointer<UpdateAddonPositionDelegate>(updateAddonPositionPtr);

            var getFilePointerPtr = Plugin.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 85 C0 74 14 83 7B 44 00");
            if (getFilePointerPtr != IntPtr.Zero)
            {
                _getFilePointer = Marshal.GetDelegateForFunctionPointer<GetFilePointerDelegate>(getFilePointerPtr);
            }
            #endregion
        }

        internal static byte GetStatus(GameObject actor)
        {
            // 40 57 48 83 EC 70 48 8B F9 E8 ?? ?? ?? ?? 81 BF ?? ?? ?? ?? ?? ?? ?? ??
            const int offset = 0x19A0;
            return Marshal.ReadByte(actor.Address + offset);
        }

        internal int GetActiveHUDLayoutIndex()
        {
            if (_getFilePointer == null) { return 0; }

            IntPtr dataPtr = _getFilePointer.Invoke(0) + 0x50;
            IntPtr slotPtr = Marshal.ReadIntPtr(dataPtr) + 0x59e8;
            int index = Marshal.ReadInt32(slotPtr);

            return Math.Clamp(index, 0, 3);
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
                    : isHidden;

                isHidden = isHidden && Config.ShowDelvUIFramesWhenCrafting
                    ? Config.HideOutsideOfCombat && !IsInCombat() && !IsCrafting()
                    : isHidden;

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

        private void SetAddonVisible(IntPtr addon, bool visible, Vector2 originalPosition)
        {
            if (_getBaseUIObject == null || _setPosition == null || _updateAddonPosition == null) { return; }

            IntPtr baseUi = _getBaseUIObject();
            IntPtr manager = Marshal.ReadIntPtr(baseUi + 0x20);

            short x = visible ? (short)originalPosition.X : (short)32000;
            short y = visible ? (short)originalPosition.Y : (short)32000;

            _updateAddonPosition(manager, addon, 1);
            _setPosition(addon, x, y);
            _updateAddonPosition(manager, addon, 0);
        }

        private unsafe bool UpdateAddonOriginalPosition(AtkUnitBase* addon, ref Vector2 originalPosition)
        {
            if (addon == null) { return false; }

            const float outOfScreen = 30000f;
            Vector2 addonPosition = new(addon->X, addon->Y);

            if (addonPosition != Vector2.Zero && addonPosition.X < outOfScreen && addonPosition.Y < outOfScreen)
            {
                originalPosition = addonPosition;
            }

            // in case something goes wrong, restore position to 0 so the element is not permanently lost off screen
            if (originalPosition.X > outOfScreen || originalPosition.Y > outOfScreen)
            {
                originalPosition = Vector2.Zero;
            }

            return addonPosition.X < outOfScreen || addonPosition.Y < outOfScreen;
        }

        private unsafe void UpdateDefaultCastBar(bool forceVisible = false)
        {
            AtkUnitBase* addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("_CastBar", 1);
            if (addon == null) { return; }

            int index = GetActiveHUDLayoutIndex();
            Vector2 previousPos = Config.CastBarOriginalPositions[index];
            bool isVisible = UpdateAddonOriginalPosition(addon, ref Config.CastBarOriginalPositions[index]);

            if (previousPos != Config.CastBarOriginalPositions[index])
            {
                ConfigurationManager.Instance.SaveConfigurations(true);
            }

            if (isVisible != Config.HideDefaultCastbar && !forceVisible) { return; }

            SetAddonVisible((IntPtr)addon, forceVisible || !Config.HideDefaultCastbar, Config.CastBarOriginalPositions[index]);
        }

        private unsafe void UpdateDefaultPulltimer(bool forceVisible = false)
        {
            AtkUnitBase* addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("ScreenInfo_CountDown", 1);
            if (addon == null) { return; }

            int index = GetActiveHUDLayoutIndex();
            Vector2 previousPos = Config.PulltimerOriginalPositions[index];
            bool isVisible = UpdateAddonOriginalPosition(addon, ref Config.PulltimerOriginalPositions[index]);

            if (previousPos != Config.PulltimerOriginalPositions[index])
            {
                ConfigurationManager.Instance.SaveConfigurations(true);
            }

            if (isVisible != Config.HideDefaultPulltimer && !forceVisible) { return; }

            SetAddonVisible((IntPtr)addon, forceVisible || !Config.HideDefaultPulltimer, Config.PulltimerOriginalPositions[index]);
        }

        private unsafe void UpdateJobGauges(bool forceVisible = false)
        {
            var (addons, names) = FindAddonsStartingWith("JobHud");

            int index = GetActiveHUDLayoutIndex();
            Dictionary<string, Vector2> dict = Config.JobGaugeOriginalPositions[index];

            for (int i = 0; i < addons.Count; i++)
            {
                AtkUnitBase* addon = (AtkUnitBase*)addons[i];
                string name = names[i];

                bool existed = dict.TryGetValue(name, out Vector2 pos);

                Vector2 previousPos = pos;
                UpdateAddonOriginalPosition(addon, ref pos);

                if (previousPos != pos || !existed)
                {
                    dict[name] = pos;
                    ConfigurationManager.Instance.SaveConfigurations(true);
                }

                SetAddonVisible((IntPtr)addon, forceVisible || !Config.HideDefaultJobGauges, dict[name]);
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

        private bool IsCrafting() => Plugin.Condition[ConditionFlag.Crafting];

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
