using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using Dalamud.Bindings.ImGui;
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
        private Hook<AgentReadyCheck.Delegates.InitiateReadyCheck>? _onReadyCheckStartHook;
        private Hook<AgentReadyCheck.Delegates.EndReadyCheck>? _onReadyCheckEndHook;

        private delegate void ActorControlDelegate(uint entityId, uint type, uint buffID, uint direct, uint actionId, uint sourceId, uint arg7, uint arg8, uint arg9, uint arg10, ulong targetId, byte arg12);
        private Hook<ActorControlDelegate>? _actorControlHook;

        private bool _readyCheckOngoing = false;
        private double _lastReadyCheckEndTime = -1;


        public unsafe PartyReadyCheckHelper()
        {
            try
            {
                _onReadyCheckStartHook = Plugin.GameInteropProvider.HookFromAddress<AgentReadyCheck.Delegates.InitiateReadyCheck>(
                    AgentReadyCheck.MemberFunctionPointers.InitiateReadyCheck, 
                    OnReadyCheckStart
                );
                _onReadyCheckStartHook?.Enable();

                _onReadyCheckEndHook = Plugin.GameInteropProvider.HookFromAddress<AgentReadyCheck.Delegates.EndReadyCheck>(
                    AgentReadyCheck.MemberFunctionPointers.EndReadyCheck,
                    OnReadycheckEnd
                );
                _onReadyCheckEndHook?.Enable();

                _actorControlHook = Plugin.GameInteropProvider.HookFromSignature<ActorControlDelegate>(
                    "E8 ?? ?? ?? ?? 0F B7 0B 83 E9 64", 
                    OnActorControl
                );
                _actorControlHook?.Enable();
            }
            catch (Exception e)
            {
                Plugin.Logger.Error("Error initiating ready check sigs!!!\n" + e.Message);
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

        private unsafe void OnReadyCheckStart(AgentReadyCheck *ptr)
        {
            _onReadyCheckStartHook?.Original(ptr);
            _readyCheckOngoing = true;
            _lastReadyCheckEndTime = -1;
        }

        private unsafe void OnReadycheckEnd(AgentReadyCheck *ptr)
        {
            _onReadyCheckEndHook?.Original(ptr);
            _lastReadyCheckEndTime = ImGui.GetTime();
        }

        private void OnActorControl(uint entityId, uint type, uint buffID, uint direct, uint actionId, uint sourceId, uint arg7, uint arg8, uint arg9, uint arg10, ulong targetId, byte arg12)
        {
            _actorControlHook?.Original(entityId, type, buffID, direct, actionId, sourceId, arg7, arg8, arg9, arg10, targetId, arg12);

            // I'm not exactly sure what id == 503 means, but its always triggered when the fight starts
            // which is all I care about
            if (type == 503)
            {
                _readyCheckOngoing = false;
            }
        }

        public void Update(double maxDuration)
        {
            if (_readyCheckOngoing &&
                _lastReadyCheckEndTime != -1 &&
                ImGui.GetTime() - _lastReadyCheckEndTime >= maxDuration)
            {
                _readyCheckOngoing = false;
            }
        }

        public unsafe ReadyCheckStatus GetStatusForContentId(ulong contentId)
        {
            if (!_readyCheckOngoing)
            {
                return ReadyCheckStatus.None;
            }

            try
            {
                for (int i = 0; i < 8; i++)
                {
                    ReadyCheckEntry entry = AgentReadyCheck.Instance()->ReadyCheckEntries[i];
                    if (entry.ContentId == contentId)
                    {
                        return ParseStatus(entry);
                    }
                }
            }
            catch { }

            return ReadyCheckStatus.None;
        }

        private ReadyCheckStatus ParseStatus(ReadyCheckEntry entry)
        {
            if (entry.Status == FFXIVClientStructs.FFXIV.Client.UI.Agent.ReadyCheckStatus.Ready)
            {
                return ReadyCheckStatus.Ready;
            }
            else if (entry.Status == FFXIVClientStructs.FFXIV.Client.UI.Agent.ReadyCheckStatus.NotReady ||
                     entry.Status == FFXIVClientStructs.FFXIV.Client.UI.Agent.ReadyCheckStatus.MemberNotPresent)
            {
                return ReadyCheckStatus.NotReady;
            }

            return ReadyCheckStatus.None;
        }
    }
}

