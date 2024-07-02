﻿/*
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
using DelvUI.Config;
using DelvUI.Interface.GeneralElements;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.Interop.Generated;
using ImGuiNET;
using Lumina.Excel;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace DelvUI.Helpers
{
    public unsafe class InputsHelper : IDisposable
    {
        public delegate void OnSetUIMouseoverActor(long arg1, long arg2);
        private delegate bool UseActionDelegate(IntPtr manager, ActionType actionType, uint actionId, uint targetId, uint a4, uint a5, uint a6, IntPtr a7);

        #region Singleton
        private InputsHelper()
        {
            _sheet = Plugin.DataManager.GetExcelSheet<Action>();

            try
            {
                /*
                 Part of setUIMouseOverActorId disassembly signature
                .text:00007FF64830FD70                   sub_7FF64830FD70 proc near
                .text:00007FF64830FD70 48 89 91 90 02 00+mov     [rcx+290h], rdx
                .text:00007FF64830FD70 00
                */
                _setUIMouseOverActor = Plugin.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B 5C 24 40 4C 8B 74 24 58 83 FD 02");
                //_uiMouseOverActorHook = Plugin.GameInteropProvider.HookFromSignature<OnSetUIMouseoverActor>(
                //    "E8 ?? ?? ?? ?? 48 8B 5C 24 40 4C 8B 74 24 58 83 FD 02",
                //    HandleUIMouseOverActorId
                //);
            }
            catch
            {
                Plugin.Logger.Error("InputsHelper OnSetUIMouseoverActor Hook failed!!!");
            }

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
            OnConfigReset(ConfigurationManager.Instance);
        }

        public static void Initialize() { Instance = new InputsHelper(); }

        public static InputsHelper Instance { get; private set; } = null!;

        ~InputsHelper()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

            _requestActionHook?.Disable();
            _requestActionHook?.Dispose();
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            ConfigurationManager.Instance.ResetEvent -= OnConfigReset;

            //_uiMouseOverActorHook?.Disable();
            //_uiMouseOverActorHook?.Dispose();

            _requestActionHook?.Disable();
            _requestActionHook?.Dispose();

            // give imgui the control of inputs again
            if (_wndHandle != IntPtr.Zero && _imguiWndProcPtr != IntPtr.Zero)
            {
                SetWindowLongPtr(_wndHandle, GWL_WNDPROC, _imguiWndProcPtr);
            }

            Instance = null!;
        }
        #endregion

        private HUDOptionsConfig _config = null!;

        private IntPtr _setUIMouseOverActor;
        //private Hook<OnSetUIMouseoverActor>? _uiMouseOverActorHook;

        private Hook<UseActionDelegate>? _requestActionHook;

        private ExcelSheet<Action>? _sheet;

        public bool HandlingMouseInputs { get; private set; } = false;
        private IGameObject? _target = null;
        private bool _ignoringMouseover = false;

        public void SetTarget(IGameObject? target, bool ignoreMouseover = false)
        {
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
            // set mouseover target in-game
            if (_config.MouseoverEnabled && !_config.MouseoverAutomaticMode && !_ignoringMouseover)
            {
                long pronounModuleAddress = (long)Framework.Instance()->GetUIModule()->GetPronounModule();

                OnSetUIMouseoverActor func = Marshal.GetDelegateForFunctionPointer<OnSetUIMouseoverActor>(_setUIMouseOverActor);
                func.Invoke(pronounModuleAddress, address);
            }
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

        private bool HandleRequestAction(IntPtr manager, ActionType actionType, uint actionId, uint targetId, uint a4, uint a5,
                                          uint a6, IntPtr a7)
        {
            if (_requestActionHook == null) { return false; }

            if (_config.MouseoverEnabled && _config.MouseoverAutomaticMode && IsActionValid(actionId, _target) && !_ignoringMouseover)
            {
                return _requestActionHook.Original(manager, actionType, actionId, targetId, a4, a5, a6, a7);
            }

            return _requestActionHook.Original(manager, actionType, actionId, targetId, a4, a5, a6, a7);
        }

        private bool IsActionValid(ulong actionID, IGameObject? target)
        {
            if (target == null || actionID == 0 || _sheet == null)
            {
                return false;
            }

            var action = _sheet.GetRow((uint)actionID);
            if (action == null)
            {
                return false;
            }

            // handle actions that automatically switch to other actions
            // ie GNB Continuation or SMN Egi Assaults
            // these actions dont have an attack type or animation so in these cases
            // we assume its a hostile spell
            // if this doesn't work on all cases we can switch to a hardcoded list
            // of special cases later
            if (action.AttackType.Row == 0 && action.AnimationStart.Row == 0 &&
                (action.CanTargetDead && !action.CanTargetFriendly && !action.CanTargetHostile && !action.CanTargetParty && action.CanTargetSelf))
            {
                // special case for AST cards and SMN rekindle
                if (actionID == 17055 || actionID == 7443 || actionID == 25822)
                {
                    return target is IPlayerCharacter || target is IBattleNpc battleNpc && battleNpc.BattleNpcKind == BattleNpcSubKind.Chocobo;
                }

                return target is IBattleNpc npcTarget && npcTarget.BattleNpcKind == BattleNpcSubKind.Enemy;
            }

            // friendly player (TODO: pvp? lol)
            if (target is IPlayerCharacter)
            {
                return action.CanTargetFriendly || action.CanTargetParty || action.CanTargetSelf;
            }

            // friendly npc
            if (target is IBattleNpc npc)
            {
                if (npc.BattleNpcKind != BattleNpcSubKind.Enemy)
                {
                    return action.CanTargetFriendly || action.CanTargetParty || action.CanTargetSelf;
                }
            }

            return action.CanTargetHostile;
        }

        #region mouseover inputs proxy
        private bool? _leftButtonClicked = null;
        public bool LeftButtonClicked => _leftButtonClicked.HasValue ? _leftButtonClicked.Value : ImGui.GetIO().MouseClicked[0];

        private bool? _rightButtonClicked = null;
        public bool RightButtonClicked => _rightButtonClicked.HasValue ? _rightButtonClicked.Value : ImGui.GetIO().MouseClicked[1];

        private bool _leftButtonWasDown = false;
        private bool _rightButtonWasDown = false;


        public void ClearClicks()
        {
            WndProcDetour(_wndHandle, WM_LBUTTONUP, 0, 0);
            WndProcDetour(_wndHandle, WM_RBUTTONUP, 0, 0);
        }

        // wnd proc detour
        // if we're "eating" inputs, we only process left and right clicks
        // any other message is passed along to the ImGui scene
        private IntPtr WndProcDetour(IntPtr hWnd, uint msg, ulong wParam, long lParam)
        {
            // eat left and right clicks?
            if (HandlingMouseInputs)
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

        public void Update()
        {
            if (_wndProcPtr == IntPtr.Zero)
            {
                HookWndProc();
            }

            _leftButtonClicked = null;
            _rightButtonClicked = null;
        }

        private void HookWndProc()
        {
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

            Plugin.Logger.Debug("Hooking WndProc for window: " + hWnd.ToString("X"));
            Plugin.Logger.Debug("Old WndProc: " + _imguiWndProcPtr.ToString("X"));
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
