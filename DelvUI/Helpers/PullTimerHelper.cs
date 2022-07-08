/*
Copyright(c) 2021 xorus (https://github.com/xorus/EngageTimer)
Modifications Copyright(c) 2021 DelvUI
09/21/2021 - Extracted code to hook the game's pulltimer functions.

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

namespace DelvUI.Helpers
{
    public unsafe class PullTimerHelper
    {
        #region Singleton
        private PullTimerHelper()
        {
            PullTimerState = new PullTimerState();

            /*
             Part of Countdown disassembly Signature
            .text:00007FF647F88F20                   CountdownPointer proc near
            .text:00007FF647F88F20
            .text:00007FF647F88F20                   var_28= xmmword ptr -28h
            .text:00007FF647F88F20                   var_18= xmmword ptr -18h
            .text:00007FF647F88F20                   arg_0= qword ptr  8
            .text:00007FF647F88F20
            .text:00007FF647F88F20 48 89 5C 24 08    mov     [rsp+arg_0], rbx
            .text:00007FF647F88F25 57                push    rdi
            .text:00007FF647F88F26 48 83 EC 40       sub     rsp, 40h
            .text:00007FF647F88F2A 8B 41 28          mov     eax, [rcx+28h]
            .text:00007FF647F88F2D 48 8B D9          mov     rbx, rcx
            .text:00007FF647F88F30 89 41 2C          mov     [rcx+2Ch], eax
            .text:00007FF647F88F33 48 8B 05 1E 3C AE+mov     rax, cs:g_Framework_2
            */

            IntPtr countdownPtr = Plugin.SigScanner.ScanText("48 89 5C 24 ?? 57 48 83 EC 40 8B 41");
            try
            {
                //_countdownTimerHook = Hook<CountdownTimer>.FromAddress(countdownPtr, CountdownTimerFunc);
                _countdownTimerHook = new Hook<CountdownTimer>(countdownPtr, CountdownTimerFunc);
                _countdownTimerHook?.Enable();
            }
            catch (Exception e)
            {
                PluginLog.Error("Could not hook to timer\n" + e);
            }
        }
        public static void Initialize() { Instance = new PullTimerHelper(); }
        public static PullTimerHelper Instance { get; private set; } = null!;

        ~PullTimerHelper()
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

            _countdownTimerHook?.Disable();
            _countdownTimerHook?.Dispose();

            Instance = null!;
        }
        #endregion

        private DateTime _combatTimeEnd;
        private DateTime _combatTimeStart;

        private ulong _countDown;
        public bool CountDownRunning;

        private int _countDownStallTicks;

        private readonly Hook<CountdownTimer>? _countdownTimerHook;
        public float LastCountDownValue;
        private bool _shouldRestartCombatTimer = true;
        private bool _lastMaxValueSet = false;

        public readonly PullTimerState PullTimerState;

        public void Update()
        {
            if (PullTimerState.Mocked)
            {
                return;
            }

            UpdateCountDown();
            UpdateEncounterTimer();
            PullTimerState.InInstance = Plugin.Condition[ConditionFlag.BoundByDuty];
        }

        private IntPtr CountdownTimerFunc(ulong value)
        {
            _countDown = value;
            return _countdownTimerHook!.Original(value);
        }

        private void UpdateEncounterTimer()
        {
            if (Plugin.Condition[ConditionFlag.InCombat])
            {
                PullTimerState.InCombat = true;
                if (_shouldRestartCombatTimer)
                {
                    _shouldRestartCombatTimer = false;
                    _combatTimeStart = DateTime.Now;
                }

                _combatTimeEnd = DateTime.Now;
            }
            else
            {
                PullTimerState.InCombat = false;
                _shouldRestartCombatTimer = true;
            }

            PullTimerState.CombatStart = _combatTimeStart;
            PullTimerState.CombatDuration = _combatTimeEnd - _combatTimeStart;
            PullTimerState.CombatEnd = _combatTimeEnd;
        }

        private void UpdateCountDown()
        {
            PullTimerState.CountingDown = false;

            if (_countDown == 0)
            {
                return;
            }

            var countDownPointerValue = Marshal.PtrToStructure<float>((IntPtr)_countDown + 0x2c);

            // is last value close enough (workaround for floating point approx)
            if (Math.Abs(countDownPointerValue - LastCountDownValue) < 0.001f)
            {
                _countDownStallTicks++;
            }
            else
            {
                _countDownStallTicks = 0;
                CountDownRunning = true;
            }

            if (_countDownStallTicks > 50)
            {
                CountDownRunning = false;
            }

            if (countDownPointerValue > 0 && CountDownRunning)
            {
                PullTimerState.CountDownValue = Marshal.PtrToStructure<float>((IntPtr)_countDown + 0x2c);
                PullTimerState.CountingDown = true;
            }

            if (!_lastMaxValueSet && CountDownRunning)
            {
                PullTimerState.CountDownMax = Marshal.PtrToStructure<float>((IntPtr)_countDown + 0x2c);
                _lastMaxValueSet = true;
            }

            if (_lastMaxValueSet && !CountDownRunning)
            {
                _lastMaxValueSet = false;
            }

            LastCountDownValue = countDownPointerValue;
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        private delegate IntPtr CountdownTimer(ulong param1);

    }

    public class PullTimerState
    {
        private bool _inCombat;
        private bool _countingDown;
        public TimeSpan CombatDuration { get; set; }
        public DateTime CombatEnd { get; set; }
        public DateTime CombatStart { get; set; }

        public bool Mocked { get; set; }

        public bool InCombat
        {
            get => _inCombat;
            set
            {
                if (_inCombat == value)
                {
                    return;
                }

                _inCombat = value;
                InCombatChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool CountingDown
        {
            get => _countingDown;
            set
            {
                if (_countingDown == value)
                {
                    return;
                }

                _countingDown = value;
                CountingDownChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool InInstance { get; set; }

        public float CountDownValue { get; set; } = 0f;
        public float CountDownMax { get; set; } = 0f;
        public event EventHandler? InCombatChanged;
        public event EventHandler? CountingDownChanged;
        //
    }
}
