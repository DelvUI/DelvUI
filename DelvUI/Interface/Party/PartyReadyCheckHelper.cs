using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
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

        private delegate void ActorControlDelegate(uint entityId, uint id, uint unk1, uint type, uint unk2, uint unk3, uint unk4, uint unk5, UInt64 targetId, byte unk6);
        private Hook<ActorControlDelegate>? _actorControlHook;

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

                IntPtr actorControlPtr = Plugin.SigScanner.ScanText("E8 ?? ?? ?? ?? 0F B7 0B 83 E9 64");
                _actorControlHook = Hook<ActorControlDelegate>.FromAddress(actorControlPtr, OnActorControl);
                _actorControlHook?.Enable();
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

            _actorControlHook?.Disable();
            _actorControlHook?.Dispose();
        }

        private void OnReadyCheckStart(IntPtr ptr)
        {
            _onReadyCheckStartHook?.Original(ptr);
            _readyCheckData = ptr;
            _readyCheckOngoing = true;

            PluginLog.Log(_readyCheckData.ToString("X"));
        }

        private void OnReadycheckEnd(IntPtr ptr)
        {
            _onReadyCheckEndHook?.Original(ptr);
            _readyCheckData = ptr;
            _readyCheckOngoing = false;
            _lastReadyCheckEndTime = ImGui.GetTime();
        }

        private void OnActorControl(uint entityId, uint id, uint unk1, uint type, uint unk2, uint unk3, uint unk4, uint unk5, UInt64 targetId, byte unk6)
        {
            _actorControlHook?.Original(entityId, id, unk1, type, unk2, unk3, unk4, unk5, targetId, unk6);

            // I'm not exactly sure what id == 503 means, but its always triggered when the fight starts
            // which is all I care about
            if (id == 503)
            {
                _readyCheckData = IntPtr.Zero;
            }
        }

        public void Update(double maxDuration)
        {
            if (_readyCheckData != IntPtr.Zero && !_readyCheckOngoing && ImGui.GetTime() - _lastReadyCheckEndTime >= maxDuration)
            {
                _readyCheckData = IntPtr.Zero;
            }
        }

        public unsafe ReadyCheckStatus GetStatusForIndex(int index, bool isCrossWorld)
        {
            if (_readyCheckData == IntPtr.Zero || index < 0 || index > 7)
            {
                return ReadyCheckStatus.None;
            }

            int rawStatus = -1;
            if (!isCrossWorld)
            {
                int* ptr = (int*)(_readyCheckData + 0xB8 + (0x10 * index));
                rawStatus = *ptr;
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    long* ptr = (long*)(_readyCheckData + 0xB0 + (0x10 * i));
                    long id = *ptr;

                    CrossRealmMember* member = InfoProxyCrossRealm.GetMemberByContentId((ulong)id);
                    if (member == null) { continue; }

                    if (member->MemberIndex == index)
                    {
                        int* p = (int*)(_readyCheckData + 0xB8 + (0x10 * i));
                        rawStatus = *p;
                        break;
                    }
                }
            }

            return ParseStatus(rawStatus);
        }

        private ReadyCheckStatus ParseStatus(int rawValue)
        {
            if (rawValue == 2)
            {
                return ReadyCheckStatus.Ready;
            }
            else if (rawValue > 2 && rawValue < 5)
            {
                return ReadyCheckStatus.NotReady;
            }

            return ReadyCheckStatus.None;
        }
    }
}

