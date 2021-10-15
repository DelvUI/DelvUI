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
using DelvUI.Config;
using DelvUI.Interface.GeneralElements;
using Lumina.Excel;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace DelvUI.Helpers
{
    public delegate void OnSetUIMouseoverActor(long arg1, long arg2);
    public delegate ulong OnRequestAction(long arg1, uint arg2, ulong arg3, long arg4, uint arg5, uint arg6, int arg7);

    public unsafe class MouseOverHelper : IDisposable
    {
        #region Singleton
        private MouseOverHelper()
        {
            _sheet = Plugin.DataManager.GetExcelSheet<Action>();

            /*
             Part of setUIMouseOverActorId disassembly signature
            .text:00007FF64830FD70                   sub_7FF64830FD70 proc near
            .text:00007FF64830FD70 48 89 91 90 02 00+mov     [rcx+290h], rdx
            .text:00007FF64830FD70 00
            */
            _setUIMouseOverActor = Plugin.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B 6C 24 ?? 48 8B 5C 24 ?? 4C 8B 7C 24 ?? 41 83 FC 02");
            //_uiMouseOverActorIdHook = new Hook<OnSetUIMouseoverActor>(_setUIMouseOverActorId, new OnSetUIMouseoverActor(HandleUIMouseOverActorId));
            //_uiMouseOverActorIdHook.Enable();

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
            _requestAction = Plugin.SigScanner.ScanText("40 53 55 57 41 54 41 57 48 83 EC 60 83 BC 24 ?? ?? ?? ?? ?? 49 8B E9 45 8B E0 44 8B FA 48 8B F9 41 8B D8 74 14 80 79 68 00 74 0E 32 C0 48 83 C4 60 41 5F 41 5C 5F 5D 5B C3");
            _requsetActionHook = new Hook<OnRequestAction>(_requestAction, new OnRequestAction(HandleRequestAction));
            _requsetActionHook.Enable();

            // WndProc detour
            IntPtr windowHandle = Process.GetCurrentProcess().MainWindowHandle;
            _wndProcDelegate = WndProcDetour;
            _wndProcPtr = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate);
            _imguiWndProcPtr = SetWindowLongPtr(windowHandle, GWL_WNDPROC, _wndProcPtr);

            // mouseover setting
            ConfigurationManager.Instance.ResetEvent += OnConfigReset;
            OnConfigReset(ConfigurationManager.Instance);
        }

        public static void Initialize() { Instance = new MouseOverHelper(); }

        public static MouseOverHelper Instance { get; private set; } = null!;

        ~MouseOverHelper()
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

            _uiMouseOverActorHook?.Dispose();
            _requsetActionHook?.Dispose();
            Instance = null!;
        }
        #endregion

        private HUDOptionsConfig _config = null!;

        private const int UnknownOffset = 0xAA750;

        private IntPtr _setUIMouseOverActor;
        private Hook<OnSetUIMouseoverActor>? _uiMouseOverActorHook;

        private IntPtr _requestAction;
        private Hook<OnRequestAction> _requsetActionHook;

        private WndProcDelegate _wndProcDelegate;
        private IntPtr _wndProcPtr;
        private IntPtr _imguiWndProcPtr;

        private ExcelSheet<Action>? _sheet;

        private GameObject? _target = null;
        public GameObject? Target
        {
            get => _target;
            set
            {
                _target = value;

                // set mouseover target in-game
                if (_config.MouseoverEnabled && !_config.MouseoverAutomaticMode)
                {
                    IntPtr uiModule = Plugin.GameGui.GetUIModule();
                    long unknownAddress = (long)uiModule + UnknownOffset;
                    long targetAddress = _target != null && _target.ObjectId != 0 ? (long)_target.Address : 0;

                    OnSetUIMouseoverActor func = Marshal.GetDelegateForFunctionPointer<OnSetUIMouseoverActor>(_setUIMouseOverActor);
                    func.Invoke(unknownAddress, targetAddress);
                }
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

        private ulong HandleRequestAction(long arg1, uint arg2, ulong arg3, long arg4, uint arg5, uint arg6, int arg7)
        {
            //PluginLog.Log("ACTION: {0} - {1} - {2} - {3} - {4} - {5} - {6}}", arg1, arg2, arg3, arg4, arg5, arg6, arg7);

            if (_config.MouseoverEnabled && _config.MouseoverAutomaticMode && IsActionValid(arg3, Target))
            {
                return _requsetActionHook.Original(arg1, arg2, arg3, Target!.ObjectId, arg5, arg6, arg7);
            }

            return _requsetActionHook.Original(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
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
        public bool HandlingInputs => Target != null;

        public bool LeftButtonClicked = false;
        public bool RightButtonClicked = false;

        // wnd proc detour
        // if we're "eating" inputs, we only process left and right clicks
        // any other message is passed along to the ImGui scene
        private IntPtr WndProcDetour(IntPtr hWnd, uint msg, ulong wParam, long lParam)
        {
            // eat left and right clicks?
            if (HandlingInputs)
            {
                switch (msg)
                {
                    case WM_LBUTTONDOWN:
                    case WM_RBUTTONDOWN:
                        return (IntPtr)0;

                    case WM_LBUTTONUP:
                        LeftButtonClicked = true;
                        return (IntPtr)0;

                    case WM_RBUTTONUP:
                        RightButtonClicked = true;
                        return (IntPtr)0;
                }
            }

            // call imgui's wnd proc
            return (IntPtr)CallWindowProc(_imguiWndProcPtr, hWnd, msg, wParam, lParam);
        }

        public void Update()
        {
            LeftButtonClicked = false;
            RightButtonClicked = false;
        }

        public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, ulong wParam, long lParam);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "CallWindowProcW")]
        public static extern long CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, ulong wParam, long lParam);

        private const uint WM_LBUTTONDOWN = 513;
        private const uint WM_LBUTTONUP = 514;
        private const uint WM_RBUTTONDOWN = 516;
        private const uint WM_RBUTTONUP = 517;

        private const int GWL_WNDPROC = -4;
        #endregion
    }
}
