/*
Copyright(c) 2021 attickdoor (https://github.com/attickdoor/MOActionPlugin)
Modifications Copyright(c) 2021 DelvUI
09/21/2021 - Used original's code hooks and action validations while using
DelvUI's own logic to select a target.

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

using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using DelvUI.Config;
using DelvUI.Interface.GeneralElements;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Dalamud.Bindings.ImGui;
using Lumina.Excel;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static FFXIVClientStructs.FFXIV.Client.Game.ActionManager;
using Action = Lumina.Excel.Sheets.Action;
using BattleNpcSubKind = Dalamud.Game.ClientState.Objects.Enums.BattleNpcSubKind;

namespace DelvUI.Helpers
{
    public unsafe class InputsHelper : IDisposable
    {
        private delegate bool UseActionDelegate(ActionManager* manager, ActionType actionType, uint actionId, ulong targetId, uint extraParam, UseActionMode mode, uint comboRouteId, bool* outOptAreaTargeted);

        #region Singleton
        private InputsHelper()
        {
            _sheet = Plugin.DataManager.GetExcelSheet<Action>();

            //try
            //{
            //    /*
            //     Part of setUIMouseOverActorId disassembly signature
            //    .text:00007FF64830FD70                   sub_7FF64830FD70 proc near
            //    .text:00007FF64830FD70 48 89 91 90 02 00+mov     [rcx+290h], rdx
            //    .text:00007FF64830FD70 00
            //    */

            //    _uiMouseOverActorHook = Plugin.GameInteropProvider.HookFromSignature<OnSetUIMouseoverActor>(
            //        "E8 ?? ?? ?? ?? 48 8B 7C 24 ?? 4C 8B 74 24 ?? 83 FD 02",
            //        HandleUIMouseOverActorId
            //    );
            //}
            //catch
            //{
            //    Plugin.Logger.Error("InputsHelper OnSetUIMouseoverActor Hook failed!!!");
            //}

            try
            {
                _requestActionHook = Plugin.GameInteropProvider.HookFromSignature<UseActionDelegate>(
                    ActionManager.Addresses.UseAction.String,
                    HandleRequestAction
                );
                _requestActionHook?.Enable();
            }
            catch
            {
                Plugin.Logger.Error("InputsHelper UseActionDelegate Hook failed!!!");
            }

            // mouseover setting
            ConfigurationManager.Instance.ResetEvent += OnConfigReset;
            Plugin.Framework.Update += OnFrameworkUpdate;

            OnConfigReset(ConfigurationManager.Instance);
        }

        public static void Initialize() { Instance = new InputsHelper(); }

        public static InputsHelper Instance { get; private set; } = null!;

        public static int InitializationDelay = 5;

        ~InputsHelper()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Plugin.Logger.Info("\tDisposing InputsHelper...");
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            ConfigurationManager.Instance.ResetEvent -= OnConfigReset;
            Plugin.Framework.Update -= OnFrameworkUpdate;

            Plugin.Logger.Info("\t\tDisposing _requestActionHook: " + _requestActionHook?.Address.ToString("X") ?? "null");
            _requestActionHook?.Disable();
            _requestActionHook?.Dispose();

            // give imgui the control of inputs again
            RestoreWndProc();

            Instance = null!;
        }
        #endregion

        private HUDOptionsConfig _config = null!;

        //private Hook<OnSetUIMouseoverActor>? _uiMouseOverActorHook;

        private Hook<UseActionDelegate>? _requestActionHook;

        private ExcelSheet<Action>? _sheet;

        public bool HandlingMouseInputs { get; private set; } = false;
        private IGameObject? _target = null;
        private bool _ignoringMouseover = false;

        public bool IsProxyEnabled => _config.InputsProxyEnabled;

        public void ToggleProxy(bool enabled)
        {
            _config.InputsProxyEnabled = enabled;
            ConfigurationManager.Instance.SaveConfigurations();
        }

        public void SetTarget(IGameObject? target, bool ignoreMouseover = false)
        {
            if (!IsProxyEnabled &&
                ClipRectsHelper.Instance?.IsPointClipped(ImGui.GetMousePos()) == false)
            {
                ImGui.SetNextFrameWantCaptureMouse(true);
            }

            _target = target;
            HandlingMouseInputs = true;
            _ignoringMouseover = ignoreMouseover;

            if (!_ignoringMouseover)
            {
                long address = _target != null && _target.GameObjectId != 0 ? (long)_target.Address : 0;
                SetGameMouseoverTarget(address);
            }
        }

        public void ClearTarget()
        {
            _target = null;
            HandlingMouseInputs = false;

            SetGameMouseoverTarget(0);
        }

        public void StartHandlingInputs()
        {
            HandlingMouseInputs = true;
        }

        public void StopHandlingInputs()
        {
            HandlingMouseInputs = false;
            _ignoringMouseover = false;
        }

        private unsafe void SetGameMouseoverTarget(long address)
        {
            if (!_config.MouseoverEnabled || _config.MouseoverAutomaticMode || _ignoringMouseover)
            {
                return;
            }

            UIModule* uiModule = Framework.Instance()->GetUIModule();
            if (uiModule == null) { return; }

            PronounModule* pronounModule = uiModule->GetPronounModule();
            if (pronounModule == null) { return; }

            pronounModule->UiMouseOverTarget = (GameObject*)address;
        }

        private void OnConfigReset(ConfigurationManager sender)
        {
            _config = sender.GetConfigObject<HUDOptionsConfig>();
        }

        //private void HandleUIMouseOverActorId(long arg1, long arg2)
        //{
        //Plugin.Logger.Log("MO: {0} - {1}", arg1.ToString("X"), arg2.ToString("X"));
        //_uiMouseOverActorHook?.Original(arg1, arg2);
        //}

        private bool HandleRequestAction(
            ActionManager* manager,
            ActionType actionType,
            uint actionId,
            ulong targetId,
            uint extraParam,
            UseActionMode mode,
            uint comboRouteId,
            bool* outOptAreaTargeted
        )
        {
            if (_requestActionHook == null) { return false; }

            if (_config.MouseoverEnabled &&
                _config.MouseoverAutomaticMode &&
                _target != null &&
                IsActionValid(actionId, _target) &&
                !_ignoringMouseover)
            {
                return _requestActionHook.Original(manager, actionType, actionId, _target.GameObjectId, extraParam, mode, comboRouteId, outOptAreaTargeted);
            }

            return _requestActionHook.Original(manager, actionType, actionId, targetId, extraParam, mode, comboRouteId, outOptAreaTargeted);
        }

        private bool IsActionValid(ulong actionID, IGameObject? target)
        {
            if (target == null || actionID == 0 || _sheet == null)
            {
                return false;
            }

            bool found = _sheet.TryGetRow((uint)actionID, out Action action);
            if (!found)
            {
                return false;
            }

            // handle actions that automatically switch to other actions
            // ie GNB Continuation or SMN Egi Assaults
            // these actions dont have an attack type or animation so in these cases
            // we assume its a hostile spell
            // if this doesn't work on all cases we can switch to a hardcoded list
            // of special cases later
            if (action.AttackType.RowId == 0 && action.AnimationStart.RowId == 0 &&
                (!action.CanTargetAlly && !action.CanTargetHostile && !action.CanTargetParty && action.CanTargetSelf))
            {
                // special case for AST cards and SMN rekindle
                if (actionID is 37019 or 37020 or 37021 or 25822)
                {
                    return target is IPlayerCharacter or IBattleNpc { BattleNpcKind: BattleNpcSubKind.Chocobo };
                }

                return target is IBattleNpc npcTarget && npcTarget.BattleNpcKind == BattleNpcSubKind.Enemy;
            }

            // friendly player (TODO: pvp? lol)
            if (target is IPlayerCharacter)
            {
                return action.CanTargetAlly || action.CanTargetParty || action.CanTargetSelf;
            }

            // friendly npc
            if (target is IBattleNpc npc)
            {
                if (npc.BattleNpcKind != BattleNpcSubKind.Enemy)
                {
                    return action.CanTargetAlly || action.CanTargetParty || action.CanTargetSelf;
                }
            }

            return action.CanTargetHostile;
        }

        #region mouseover inputs proxy
        private bool? _leftButtonClicked = null;
        public bool LeftButtonClicked => _leftButtonClicked.HasValue ?
            _leftButtonClicked.Value :
            (IsProxyEnabled ? false : ImGui.IsMouseClicked(ImGuiMouseButton.Left));

        private bool? _rightButtonClicked = null;
        public bool RightButtonClicked => _rightButtonClicked.HasValue ?
            _rightButtonClicked.Value :
            (IsProxyEnabled ? false : ImGui.IsMouseClicked(ImGuiMouseButton.Right));

        private bool _leftButtonWasDown = false;
        private bool _rightButtonWasDown = false;


        public void ClearClicks()
        {
            if (IsProxyEnabled)
            {
                WndProcDetour(_wndHandle, WM_LBUTTONUP, 0, 0);
                WndProcDetour(_wndHandle, WM_RBUTTONUP, 0, 0);
            }
        }

        // wnd proc detour
        // if we're "eating" inputs, we only process left and right clicks
        // any other message is passed along to the ImGui scene
        private IntPtr WndProcDetour(IntPtr hWnd, uint msg, ulong wParam, long lParam)
        {
            // eat left and right clicks?
            if (HandlingMouseInputs && IsProxyEnabled)
            {
                switch (msg)
                {
                    // mouse clicks
                    case WM_LBUTTONDOWN:
                    case WM_RBUTTONDOWN:
                    case WM_LBUTTONUP:
                    case WM_RBUTTONUP:

                        // if there's not a game window covering the cursor location
                        // we eat the message and handle the inputs manually
                        if (ClipRectsHelper.Instance?.IsPointClipped(ImGui.GetMousePos()) == false)
                        {
                            _leftButtonClicked = _leftButtonWasDown && msg == WM_LBUTTONUP;
                            _rightButtonClicked = _rightButtonWasDown && msg == WM_RBUTTONUP;

                            bool shouldTakeInput = true;
                            if (msg == WM_LBUTTONUP && !_leftButtonWasDown ||
                                msg == WM_RBUTTONUP && !_rightButtonWasDown)
                            {
                                shouldTakeInput = false;
                            }

                            _leftButtonWasDown = msg == WM_LBUTTONDOWN;
                            _rightButtonWasDown = msg == WM_RBUTTONDOWN;

                            if (shouldTakeInput)
                            {
                                return (IntPtr)0;
                            }
                        }
                        // otherwise we let imgui handle the inputs
                        else
                        {
                            _leftButtonClicked = null;
                            _rightButtonClicked = null;
                        }
                        break;
                }
            }

            // call imgui's wnd proc
            return (IntPtr)CallWindowProc(_imguiWndProcPtr, hWnd, msg, wParam, lParam);
        }

        public void OnFrameworkUpdate(IFramework framework)
        {
            if (IsProxyEnabled)
            {
                if (_wndProcPtr == IntPtr.Zero) {
                    HookWndProc();
                }
            }
            else if (_wndProcPtr != IntPtr.Zero)
            {
                RestoreWndProc();
            }
        }

        public void OnFrameEnd()
        {
            _leftButtonClicked = null;
            _rightButtonClicked = null;
        }

        private void HookWndProc()
        {
            if (Plugin.LoadTime <= 0 ||
                ImGui.GetTime() - Plugin.LoadTime < InitializationDelay)
            {
                return;
            }

            ulong processId = (ulong)Process.GetCurrentProcess().Id;

            IntPtr hWnd = IntPtr.Zero;
            do
            {
                hWnd = FindWindowExW(IntPtr.Zero, hWnd, "FFXIVGAME", null);
                if (hWnd == IntPtr.Zero) { return; }

                ulong wndProcessId = 0;
                GetWindowThreadProcessId(hWnd, ref wndProcessId);

                if (wndProcessId == processId)
                {
                    break;
                }

            } while (hWnd != IntPtr.Zero);

            if (hWnd == IntPtr.Zero) { return; }

            _wndHandle = hWnd;
            _wndProcDelegate = WndProcDetour;
            _wndProcPtr = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate);
            _imguiWndProcPtr = SetWindowLongPtr(hWnd, GWL_WNDPROC, _wndProcPtr);

            Plugin.Logger.Info("Initializing DelvUI Inputs v" + Plugin.Version);
            Plugin.Logger.Info("\tHooking WndProc for window: " + hWnd.ToString("X"));
            Plugin.Logger.Info("\tOld WndProc: " + _imguiWndProcPtr.ToString("X"));
        }

        private void RestoreWndProc()
        {
            if (_wndHandle != IntPtr.Zero && _imguiWndProcPtr != IntPtr.Zero)
            {
                Plugin.Logger.Info("\t\tRestoring WndProc");
                Plugin.Logger.Info("\t\t\tOld _wndHandle = " + _wndHandle.ToString("X"));
                Plugin.Logger.Info("\t\t\tOld _imguiWndProcPtr = " + _imguiWndProcPtr.ToString("X"));

                SetWindowLongPtr(_wndHandle, GWL_WNDPROC, _imguiWndProcPtr);
                Plugin.Logger.Info("\t\t\tDone!");

                _wndHandle = IntPtr.Zero;
                _imguiWndProcPtr = IntPtr.Zero;
            }
        }

        private IntPtr _wndHandle = IntPtr.Zero;
        private WndProcDelegate _wndProcDelegate = null!;
        private IntPtr _wndProcPtr = IntPtr.Zero;
        private IntPtr _imguiWndProcPtr = IntPtr.Zero;

        public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, ulong wParam, long lParam);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "CallWindowProcW")]
        public static extern long CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, ulong wParam, long lParam);

        [DllImport("user32.dll", EntryPoint = "FindWindowExW", SetLastError = true)]
        public static extern IntPtr FindWindowExW(IntPtr hWndParent, IntPtr hWndChildAfter, [MarshalAs(UnmanagedType.LPWStr)] string? lpszClass, [MarshalAs(UnmanagedType.LPWStr)] string? lpszWindow);

        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId", SetLastError = true)]
        public static extern ulong GetWindowThreadProcessId(IntPtr hWnd, ref ulong id);

        private const uint WM_LBUTTONDOWN = 513;
        private const uint WM_LBUTTONUP = 514;
        private const uint WM_RBUTTONDOWN = 516;
        private const uint WM_RBUTTONUP = 517;

        private const int GWL_WNDPROC = -4;
        #endregion
    }
}
