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
using Dalamud.Logging;
using DelvUI.Config;
using DelvUI.Interface.GeneralElements;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
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
        public delegate ulong OnRequestAction(long arg1, uint arg2, uint arg3, long arg4, int arg5, int arg6, int arg7, byte* arg8);

        #region Singleton
        private InputsHelper()
        {
            _sheet = Plugin.DataManager.GetExcelSheet<Action>();

            /*
             Part of setUIMouseOverActorId disassembly signature
            .text:00007FF64830FD70                   sub_7FF64830FD70 proc near
            .text:00007FF64830FD70 48 89 91 90 02 00+mov     [rcx+290h], rdx
            .text:00007FF64830FD70 00
            */
            _setUIMouseOverActor = Plugin.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B 6C 24 ?? 48 8B 5C 24 ?? 4C 8B 7C 24 ?? 41 83 FC 02");
            _uiMouseOverActorHook = new Hook<OnSetUIMouseoverActor>(_setUIMouseOverActor, new OnSetUIMouseoverActor(HandleUIMouseOverActorId));

            /*
             Part of requestAction disassembly signature
            .text:00007FF6484F05A0                   Client__Game__ActionManager_UseAction proc near
            .text:00007FF6484F05A0
            .text:00007FF6484F05A0                   var_68= qword ptr -68h
            .text:00007FF6484F05A0                   var_60= dword ptr -60h
            .text:00007FF6484F05A0                   var_58= dword ptr -58h
            .text:00007FF6484F05A0                   var_50= dword ptr -50h
            .text:00007FF6484F05A0                   var_48= dword ptr -48h
            .text:00007FF6484F05A0                   var_40= dword ptr -40h
            .text:00007FF6484F05A0                   var_38= qword ptr -38h
            .text:00007FF6484F05A0                   arg_0= qword ptr  8
            .text:00007FF6484F05A0                   arg_8= qword ptr  10h
            .text:00007FF6484F05A0                   arg_10= qword ptr  18h
            .text:00007FF6484F05A0                   arg_20= dword ptr  28h
            .text:00007FF6484F05A0                   arg_28= dword ptr  30h
            .text:00007FF6484F05A0                   arg_30= dword ptr  38h
            .text:00007FF6484F05A0
            .text:00007FF6484F05A0 40 53             push    rbx
            .text:00007FF6484F05A2 55                push    rbp
            .text:00007FF6484F05A3 57                push    rdi
            .text:00007FF6484F05A4 41 54             push    r12
            .text:00007FF6484F05A6 41 57             push    r15
            .text:00007FF6484F05A8 48 83 EC 60       sub     rsp, 60h
            .text:00007FF6484F05AC 83 BC 24 B8 00 00+cmp     [rsp+88h+arg_28], 1
            .text:00007FF6484F05AC 00 01
            .text:00007FF6484F05B4 49 8B E9          mov     rbp, r9.text:00007FF6484F05B7 45 8B E0          mov     r12d, r8d
            .text:00007FF6484F05BA 44 8B FA          mov     r15d, edx
            .text:00007FF6484F05BD 48 8B F9          mov     rdi, rcx
            .text:00007FF6484F05C0 41 8B D8          mov     ebx, r8d
            .text:00007FF6484F05C3 74 14             jz      short loc_7FF6484F05D9
            */
            _requestAction = Plugin.SigScanner.ScanText("E8 ?? ?? ?? ?? EB 64 B1 01");
            _requestActionHook = new Hook<OnRequestAction>(_requestAction, new OnRequestAction(HandleRequestAction));
            _requestActionHook.Enable();

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
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            ConfigurationManager.Instance.ResetEvent -= OnConfigReset;

            _uiMouseOverActorHook?.Disable();
            _uiMouseOverActorHook?.Dispose();

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
        private Hook<OnSetUIMouseoverActor>? _uiMouseOverActorHook;

        private IntPtr _requestAction;
        private Hook<OnRequestAction> _requestActionHook;

        private ExcelSheet<Action>? _sheet;

        public bool HandlingMouseInputs { get; private set; } = false;
        private GameObject? _target = null;

        public void SetTarget(GameObject? target)
        {
            _target = target;
            HandlingMouseInputs = true;

            long address = _target != null && _target.ObjectId != 0 ? (long)_target.Address : 0;
            SetGameMouseoverTarget(address);
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
        }

        private unsafe void SetGameMouseoverTarget(long address)
        {
            // set mouseover target in-game
            if (_config.MouseoverEnabled && !_config.MouseoverAutomaticMode)
            {
                long pronounModuleAddress = (long)Framework.Instance()->GetUiModule()->GetPronounModule();

                OnSetUIMouseoverActor func = Marshal.GetDelegateForFunctionPointer<OnSetUIMouseoverActor>(_setUIMouseOverActor);
                func.Invoke(pronounModuleAddress, address);
            }
        }

        private void OnConfigReset(ConfigurationManager sender)
        {
            _config = sender.GetConfigObject<HUDOptionsConfig>();
        }

        private void HandleUIMouseOverActorId(long arg1, long arg2)
        {
            //PluginLog.Log("MO: {0} - {1}", arg1.ToString("X"), arg2.ToString("X"));
            _uiMouseOverActorHook?.Original(arg1, arg2);
        }

        private ulong HandleRequestAction(long arg1, uint arg2, uint arg3, long arg4, int arg5, int arg6, int arg7, byte* arg8)
        {
            if (_config.MouseoverEnabled && _config.MouseoverAutomaticMode && IsActionValid(arg3, _target))
            {
                return _requestActionHook.Original(arg1, arg2, arg3, _target!.ObjectId, arg5, arg6, arg7, arg8);
            }

            return _requestActionHook.Original(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        private bool IsActionValid(ulong actionID, GameObject? target)
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
                    return target is PlayerCharacter || target is BattleNpc battleNpc && battleNpc.BattleNpcKind == BattleNpcSubKind.Chocobo;
                }

                return target is BattleNpc npcTarget && npcTarget.BattleNpcKind == BattleNpcSubKind.Enemy;
            }

            // friendly player (TODO: pvp? lol)
            if (target is PlayerCharacter)
            {
                return action.CanTargetFriendly || action.CanTargetParty || action.CanTargetSelf;
            }

            // friendly npc
            if (target is BattleNpc npc)
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

            PluginLog.Log("Hooking WndProc for window: " + hWnd.ToString("X"));
            PluginLog.Log("Old WndProc: " + _imguiWndProcPtr.ToString("X"));
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
