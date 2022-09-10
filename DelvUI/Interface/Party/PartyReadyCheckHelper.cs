using Dalamud.Hooking;
using Dalamud.Logging;
using ImGuiNET;
using System;

namespace DelvUI.Interface.Party
{
    public enum ReadyCheckStatus
    {
        Ready = 0,
        NotReady = 1,
        None = 2
    }

    public class PartyReadyCheckHelper : IDisposable
    {
        private delegate void ReadyCheckDelegate(IntPtr ptr);
        private Hook<ReadyCheckDelegate>? _onReadyCheckStartHook;
        private Hook<ReadyCheckDelegate>? _onReadyCheckEndHook;

        private IntPtr _readyCheckData = IntPtr.Zero;
        private bool _readyCheckOngoing = false;
        private double _lastReadyCheckEndTime = -1;

        public PartyReadyCheckHelper()
        {
            try
            {
                IntPtr startPtr = Plugin.SigScanner.ScanText("40 ?? 48 83 ?? ?? 48 8B ?? E8 ?? ?? ?? ?? 48 ?? ?? ?? 33 C0 ?? 89");
                _onReadyCheckStartHook = Hook<ReadyCheckDelegate>.FromAddress(startPtr, OnReadyCheckStart);
                _onReadyCheckStartHook?.Enable();

                IntPtr endPtr = Plugin.SigScanner.ScanText("40 ?? 53 48 ?? ?? ?? ?? 48 81 ?? ?? ?? ?? ?? 48 8B ?? ?? ?? ?? ?? 48 33 ?? ?? 89 ?? ?? ?? 83 ?? ?? ?? 48 8B ?? 75 ?? 48");
                _onReadyCheckEndHook = Hook<ReadyCheckDelegate>.FromAddress(endPtr, OnReadycheckEnd);
                _onReadyCheckEndHook?.Enable();
            }
            catch (Exception e)
            {
                PluginLog.Error("Error initiating ready check sigs!!!\n" + e.Message);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            _onReadyCheckStartHook?.Disable();
            _onReadyCheckStartHook?.Dispose();

            _onReadyCheckEndHook?.Disable();
            _onReadyCheckEndHook?.Dispose();
        }

        private void OnReadyCheckStart(IntPtr ptr)
        {
            _onReadyCheckStartHook?.Original(ptr);
            _readyCheckData = ptr;
            _readyCheckOngoing = true;
        }

        private void OnReadycheckEnd(IntPtr ptr)
        {
            _onReadyCheckEndHook?.Original(ptr);
            _readyCheckData = ptr;
            _readyCheckOngoing = false;
            _lastReadyCheckEndTime = ImGui.GetTime();
        }

        public void Update(double maxDuration)
        {
            if (_readyCheckData != IntPtr.Zero && !_readyCheckOngoing && ImGui.GetTime() - _lastReadyCheckEndTime >= maxDuration)
            {
                _readyCheckData = IntPtr.Zero;
            }
        }

        public unsafe ReadyCheckStatus GetStatusForIndex(int index)
        {
            if (_readyCheckData == IntPtr.Zero || index < 0 || index > 7)
            {
                return ReadyCheckStatus.None;
            }

            int* ptr = (int*)(_readyCheckData + 0xB8 + (0x10 * index));
            int rawStatus = *ptr;

            if (rawStatus == 2)
            {
                return ReadyCheckStatus.Ready;
            }
            else if (rawStatus > 2 && rawStatus < 5)
            {
                return ReadyCheckStatus.NotReady;
            }

            return ReadyCheckStatus.None;
        }
    }
}

