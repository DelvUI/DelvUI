using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Memory;
using DelvUI.Config;
using DelvUI.Helpers;
using FFXIVClientStructs.FFXIV.Client.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using StructsFramework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;

namespace DelvUI.Interface.Party
{
    public delegate void PartyMembersChangedEventHandler(PartyManager sender);

    public unsafe class PartyManager : IDisposable
    {
        #region Singleton
        public static PartyManager Instance { get; private set; } = null!;
        private PartyFramesConfig _config = null!;

        private PartyManager()
        {
            _raiseTracker = new PartyFramesRaiseTracker();
            _invulnTracker = new PartyFramesInvulnTracker();
            _cleanseTracker = new PartyFramesCleanseTracker();

            Plugin.Framework.Update += FrameworkOnOnUpdateEvent;
            ConfigurationManager.Instance.ResetEvent += OnConfigReset;

            OnConfigReset(ConfigurationManager.Instance);
            UpdatePreview();
        }

        public static void Initialize()
        {
            Instance = new PartyManager();
        }

        ~PartyManager()
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

            _raiseTracker.Dispose();
            _invulnTracker.Dispose();
            _cleanseTracker.Dispose();

            Plugin.Framework.Update -= FrameworkOnOnUpdateEvent;
            _config.ValueChangeEvent -= OnConfigPropertyChanged;

            Instance = null!;
        }

        private void OnConfigReset(ConfigurationManager sender)
        {
            if (_config != null)
            {
                _config.ValueChangeEvent -= OnConfigPropertyChanged;
            }

            _config = sender.GetConfigObject<PartyFramesConfig>();
            _config.ValueChangeEvent += OnConfigPropertyChanged;
        }

        #endregion Singleton

        public AddonPartyList* PartyListAddon { get; private set; } = null;
        public IntPtr HudAgent { get; private set; } = IntPtr.Zero;

        public RaptureAtkModule* RaptureAtkModule { get; private set; } = null;

        private const int PartyListInfoOffset = 0x0BE0;
        private const int PartyListMemberRawInfoSize = 0x20;
        private const int PartyJobIconIdsOffset = 0x1320;

        private const int PartyCrossWorldNameOffset = 0x0F2A;
        private const int PartyCrossWorldDisplayNameOffset = 0x0EC2;
        private const int PartyCrossWorldEntrySize = 0xD8;

        private const int PartyTrustNameOffset = 0x0C00;
        private const int PartyTrustEntrySize = 0x20;

        private const int PartyMembersInfoIndex = 11;

        private List<PartyListMemberInfo> _partyMembersInfo = null!;
        private bool _dirty = false;

        private List<IPartyFramesMember> _groupMembers = new List<IPartyFramesMember>();
        public IReadOnlyCollection<IPartyFramesMember> GroupMembers => _groupMembers.AsReadOnly();
        public uint MemberCount => (uint)_groupMembers.Count;

        private PartyFramesRaiseTracker _raiseTracker;
        private PartyFramesInvulnTracker _invulnTracker;
        private PartyFramesCleanseTracker _cleanseTracker;

        public event PartyMembersChangedEventHandler? MembersChangedEvent;

        public bool Previewing => _config.Preview;

        public bool IsSoloParty()
        {
            if (!_config.ShowWhenSolo) { return false; }

            return _groupMembers.Count <= 1 ||
                (_groupMembers.Count == 2 && _config.ShowChocobo &&
                _groupMembers[1].Character is BattleNpc npc && npc.BattleNpcKind == BattleNpcSubKind.Chocobo);
        }

        private void FrameworkOnOnUpdateEvent(Framework framework)
        {
            // find party list hud agent
            PartyListAddon = (AddonPartyList*)Plugin.GameGui.GetAddonByName("_PartyList", 1);
            HudAgent = Plugin.GameGui.FindAgentInterface(PartyListAddon);

            UIModule* uiModule = StructsFramework.Instance()->GetUiModule();
            RaptureAtkModule = uiModule != null ? uiModule->GetRaptureAtkModule() : null;

            // no need to update on preview mode
            if (_config.Preview)
            {
                return;
            }

            Update();
        }

        private int _realMemberCount => PartyListAddon != null ? PartyListAddon->MemberCount : Plugin.PartyList.Length;

        private void Update()
        {
            var player = Plugin.ClientState.LocalPlayer;
            if (player is null || player is not PlayerCharacter)
            {
                return;
            }

            try
            {
                // trust
                if (PartyListAddon->TrustCount > 0)
                {
                    UpdateTrustParty(player, PartyListAddon->TrustCount);
                    UpdateTrackers();
                    return;
                }

                // solo
                if (_realMemberCount <= 1)
                {
                    if (_config.ShowWhenSolo)
                    {
                        UpdateSoloParty(player);
                    }
                    else if (_groupMembers.Count > 0)
                    {
                        _groupMembers.Clear();
                        MembersChangedEvent?.Invoke(this);
                    }

                    UpdateTrackers();
                    return;
                }

                // parse raw data and detect changes
                bool partyChanged = ParseRawData();

                // if party is the same, just update actor references
                if (!partyChanged)
                {
                    foreach (var member in _groupMembers)
                    {
                        var index = member.ObjectId == player.ObjectId ? 0 : member.Order - 1;
                        member.Update(EnmityForIndex(index), StatusForIndex(index), IsPartyLeader(index), JobIdForIndex(index));
                    }
                }
                // cross world party
                else if (Plugin.PartyList.Length < _realMemberCount)
                {
                    UpdateCrossWorldParty(player);
                }
                // regular party
                else
                {
                    UpdateRegularParty(player);
                }

                UpdateTrackers();
            }
            catch { }
        }

        private void UpdateTrustParty(PlayerCharacter player, int trustCount)
        {
            bool needsUpdate = _dirty || _groupMembers.Count != trustCount + 1;

            if (needsUpdate)
            {
                _groupMembers.Clear();

                int order = _config.PlayerOrderOverrideEnabled ? _config.PlayerOrder + 1 : 1;
                _groupMembers.Add(new PartyFramesMember(player, order, EnmityForIndex(0), PartyMemberStatus.None, true));

                order = 2;

                for (int i = 0; i < trustCount; i++)
                {
                    long* namePtr = (long*)(HudAgent + (PartyTrustNameOffset + PartyTrustEntrySize * i));
                    string? name = Marshal.PtrToStringUTF8(new IntPtr(*namePtr));
                    if (name == null) { continue; }

                    Character? trustChara = Utils.GetGameObjectByName(name) as Character;
                    if (trustChara != null)
                    {
                        _groupMembers.Add(new PartyFramesMember(trustChara, order, EnmityForTrustMemberIndex(i), PartyMemberStatus.None, true));
                        order++;
                    }
                }

                // sort
                SortGroupMembers(player);
                _dirty = false;

                MembersChangedEvent?.Invoke(this);
            }
            else
            {
                for (int i = 0; i < _groupMembers.Count; i++)
                {
                    if (_groupMembers[i].ObjectId == player.ObjectId)
                    {
                        _groupMembers[i].Update(EnmityForIndex(0), PartyMemberStatus.None, true, player.ClassJob.Id);
                    }
                    else
                    {
                        _groupMembers[i].Update(EnmityForTrustMemberIndex(Math.Max(0, _groupMembers[i].Order - 2)), PartyMemberStatus.None, false, 0);
                    }
                }
            }
        }

        private void UpdateSoloParty(PlayerCharacter player)
        {
            Character? chocobo = null;
            if (_config.ShowChocobo)
            {
                var gameObject = Utils.GetBattleChocobo(player);
                if (gameObject != null && gameObject is Character)
                {
                    chocobo = (Character)gameObject;
                }
            }

            bool needsUpdate =
                _groupMembers.Count == 0 ||
                (_groupMembers.Count != 2 && _config.ShowChocobo && chocobo != null) ||
                (_groupMembers.Count > 1 && !_config.ShowChocobo) ||
                (_groupMembers.Count > 1 && chocobo == null) ||
                (_groupMembers.Count == 2 && _config.ShowChocobo && _groupMembers[1].ObjectId != chocobo?.ObjectId);

            EnmityLevel playerEnmity = PartyListAddon->EnmityLeaderIndex == 0 ? EnmityLevel.Leader : EnmityLevel.Last;

            // for some reason chocobos never get a proper enmity value even though they have aggro
            // if the player enmity is set to first, but the "leader index" is invalid
            // we can pretty much deduce that the chocobo is the one with aggro
            // this might fail on some cases when there are other players not in party hitting the same thing
            // but the edge case is so minor we should be fine
            EnmityLevel chocoboEnmity = PartyListAddon->EnmityLeaderIndex == -1 && PartyListAddon->PartyMember[0].EmnityByte == 1 ? EnmityLevel.Leader : EnmityLevel.Last;

            if (needsUpdate)
            {
                _groupMembers.Clear();

                _groupMembers.Add(new PartyFramesMember(player, 1, playerEnmity, PartyMemberStatus.None, true));

                if (chocobo != null)
                {
                    _groupMembers.Add(new PartyFramesMember(chocobo, 2, chocoboEnmity, PartyMemberStatus.None, false));
                }

                MembersChangedEvent?.Invoke(this);
            }
            else
            {
                for (int i = 0; i < _groupMembers.Count; i++)
                {
                    _groupMembers[i].Update(i == 0 ? playerEnmity : chocoboEnmity, PartyMemberStatus.None, i == 0, i == 0 ? player.ClassJob.Id : 0);
                }
            }
        }

        private bool ParseRawData()
        {
            if (HudAgent == IntPtr.Zero) { return false; }

            // player status
            Dictionary<string, string> PlayerStatusMap = new Dictionary<string, string>();
            if (RaptureAtkModule != null && RaptureAtkModule->AtkModule.AtkArrayDataHolder.StringArrayCount > PartyMembersInfoIndex)
            {
                var stringArrayData = RaptureAtkModule->AtkModule.AtkArrayDataHolder.StringArrays[PartyMembersInfoIndex];
                for (int i = 5; i < 40; i += 5)
                {
                    if (stringArrayData->AtkArrayData.Size <= i + 3 || stringArrayData->StringArray[i] == null || stringArrayData->StringArray[i + 3] == null) { break; }

                    IntPtr ptr = new IntPtr(stringArrayData->StringArray[i]);
                    string name = MemoryHelper.ReadSeStringNullTerminated(ptr).ToString();

                    ptr = new IntPtr(stringArrayData->StringArray[i + 3]);
                    string status = MemoryHelper.ReadSeStringNullTerminated(ptr).ToString();

                    if (!PlayerStatusMap.ContainsKey(name))
                    {
                        PlayerStatusMap.Add(name, status);
                    }
                }
            }

            // party data
            bool partyChanged = _dirty || _partyMembersInfo == null || _groupMembers.Count != _realMemberCount;

            List<PartyListMemberInfo> newInfo = new List<PartyListMemberInfo>(_realMemberCount);
            for (int i = 0; i < _realMemberCount; i++)
            {
                PartyListMemberRawInfo* info = (PartyListMemberRawInfo*)(HudAgent + (PartyListInfoOffset + PartyListMemberRawInfoSize * i));
                string? name = NameForIndex(i);
                string? status = null;

                if (name != null)
                {
                    PlayerStatusMap.TryGetValue(name, out status);
                }

                newInfo.Add(new PartyListMemberInfo(info, name, JobIdForIndex(i), status));
            }

            if (!partyChanged && _partyMembersInfo != null)
            {
                partyChanged = !newInfo.SequenceEqual(_partyMembersInfo);
            }
            _partyMembersInfo = newInfo;

            return partyChanged;
        }

        private void UpdateCrossWorldParty(PlayerCharacter player)
        {
            // create new members array with cross world data
            _groupMembers.Clear();

            for (int i = 0; i < _realMemberCount; i++)
            {
                bool isPlayer = i == 0;

                int order;
                if (isPlayer && _config.PlayerOrderOverrideEnabled)
                {
                    order = _config.PlayerOrder + 1;
                }
                else
                {
                    order = i + 1;
                }

                var enmity = EnmityForIndex(i);
                var status = StatusForIndex(i);
                var isPartyLeader = IsPartyLeader(i);

                var member = isPlayer ?
                    new PartyFramesMember(player, order, enmity, status, isPartyLeader) :
                    new PartyFramesMember(NameForIndex(i), order, JobIdForIndex(i), status, isPartyLeader);

                _groupMembers.Add(member);
            }

            // sort according to default party list
            SortGroupMembers(player);
            _dirty = false;

            // fire event
            MembersChangedEvent?.Invoke(this);
        }

        private void UpdateRegularParty(PlayerCharacter player)
        {
            // create new members array with dalamud's data
            _groupMembers.Clear();

            for (int i = 0; i < Plugin.PartyList.Length; i++)
            {
                var partyMember = Plugin.PartyList[i];
                if (partyMember == null)
                {
                    continue;
                }

                var isPlayer = partyMember.ObjectId == player.ObjectId;

                // player order override
                int order;
                if (isPlayer && _config.PlayerOrderOverrideEnabled)
                {
                    order = _config.PlayerOrder + 1;
                }
                else
                {
                    order = IndexForPartyMember(partyMember) ?? 9;
                }

                int index = isPlayer ? 0 : order - 1;
                EnmityLevel enmity = EnmityForIndex(index);
                PartyMemberStatus status = StatusForIndex(index);
                bool isPartyLeader = i == Plugin.PartyList.PartyLeaderIndex;

                var member = new PartyFramesMember(partyMember, order, enmity, status, isPartyLeader);
                _groupMembers.Add(member);

                // player's chocobo (always last)
                if (_config.ShowChocobo && member.ObjectId == player.ObjectId)
                {
                    var companion = Utils.GetBattleChocobo(player);
                    if (companion is Character companionCharacter)
                    {
                        _groupMembers.Add(new PartyFramesMember(companionCharacter, 10, EnmityLevel.Last, PartyMemberStatus.None, false));
                    }
                }
            }

            // sort according to default party list
            SortGroupMembers(player);
            _dirty = false;

            // fire event
            MembersChangedEvent?.Invoke(this);
        }

        private void UpdateTrackers()
        {
            _raiseTracker.Update(_groupMembers);
            _invulnTracker.Update(_groupMembers);
            _cleanseTracker.Update(_groupMembers);
        }

        #region utils
        private string? NameForIndex(int index)
        {
            if (HudAgent == IntPtr.Zero || index < 0 || index > 7)
            {
                return null;
            }

            IntPtr namePtr = (HudAgent + (PartyCrossWorldNameOffset + PartyCrossWorldEntrySize * index));
            return Marshal.PtrToStringUTF8(namePtr);
        }

        private string? DisplayNameForIndex(int index)
        {
            if (HudAgent == IntPtr.Zero || index < 0 || index > 7)
            {
                return null;
            }

            IntPtr namePtr = (HudAgent + (PartyCrossWorldDisplayNameOffset + PartyCrossWorldEntrySize * index));
            return Marshal.PtrToStringUTF8(namePtr);
        }

        private PartyMemberStatus StatusForIndex(int index)
        {
            if (index < 0 || index > 7)
            {
                return PartyMemberStatus.None;
            }

            // TODO: support for other languages
            // couldn't figure out another way of doing this sadly

            // offline status
            string status = _partyMembersInfo[index].Status;
            if (status.Contains("Offline"))
            {
                return PartyMemberStatus.Offline;
            }

            // viewing cutscene status
            string? displayName = DisplayNameForIndex(index);
            if (displayName != null && displayName.Contains("Viewing Cutscene"))
            {
                return PartyMemberStatus.ViewingCutscene;
            }

            return PartyMemberStatus.None;
        }

        private uint JobIdForIndex(int index)
        {
            if (PartyListAddon == null || index < 0 || index > 7)
            {
                return 0;
            }

            // since we don't get the job info in a nice way when another player is out of reach
            // we infer it from the icon id in the party list
            int* ptr = (int*)(new IntPtr(PartyListAddon) + PartyJobIconIdsOffset + (4 * index));
            int iconId = *ptr;

            return (uint)(Math.Max(0, iconId - 62100));
        }

        private EnmityLevel EnmityForIndex(int index)
        {
            if (PartyListAddon == null || index < 0 || index > 7)
            {
                return EnmityLevel.Last;
            }

            EnmityLevel enmityLevel = (EnmityLevel)PartyListAddon->PartyMember[index].EmnityByte;
            if (enmityLevel == EnmityLevel.Leader && PartyListAddon->EnmityLeaderIndex != index)
            {
                enmityLevel = EnmityLevel.Last;
            }

            return enmityLevel;
        }

        private EnmityLevel EnmityForTrustMemberIndex(int index)
        {
            if (PartyListAddon == null || index < 0 || index > 6)
            {
                return EnmityLevel.Last;
            }

            return (EnmityLevel)PartyListAddon->TrustMember[index].EmnityByte;
        }

        private bool IsPartyLeader(int index)
        {
            if (PartyListAddon == null)
            {
                return false;
            }

            // we use the icon Y coordinate in the party list to know the index (lmao)
            uint partyLeadIndex = (uint)PartyListAddon->LeaderMarkResNode->ChildNode->Y / 40;
            return index == partyLeadIndex;
        }

        private int? IndexForPartyMember(PartyMember member)
        {
            if (_partyMembersInfo == null || _partyMembersInfo.Count == 0)
            {
                return null;
            }

            var name = member.Name.ToString();
            return _partyMembersInfo.FindIndex(o => (member.ObjectId != 0 && o.ObjectId == member.ObjectId) || o.Name == name) + 1;
        }

        private void SortGroupMembers(PlayerCharacter player)
        {
            _groupMembers.Sort((a, b) =>
            {
                if (a.Order == b.Order)
                {
                    if (a.ObjectId == player.ObjectId)
                    {
                        return 1;
                    }
                    else if (b.ObjectId == player.ObjectId)
                    {
                        return -1;
                    }

                    return a.Name.CompareTo(b.Name);
                }

                if (a.Order < b.Order)
                {
                    return -1;
                }

                return 1;
            });
        }
        #endregion

        #region events
        public void OnPlayerOrderChange()
        {
            if (_config.PlayerOrderOverrideEnabled)
            {
                _dirty = true;
            }
        }

        private void OnConfigPropertyChanged(object sender, OnChangeBaseArgs args)
        {
            if (args.PropertyName == "Preview")
            {
                UpdatePreview();
            }
            else if (args.PropertyName == "PlayerOrder" || args.PropertyName == "PlayerOrderOverrideEnabled")
            {
                OnPlayerOrderChange();
            }
            else if (args.PropertyName == "ShowChocobo")
            {
                _dirty = true;
            }
        }

        public void UpdatePreview()
        {
            if (!_config.Preview)
            {
                _groupMembers.Clear();
                MembersChangedEvent?.Invoke(this);
                return;
            }

            // fill list with fake members for UI testing
            _groupMembers.Clear();

            if (_config.Preview)
            {
                int count = FakePartyFramesMember.RNG.Next(4, 9);
                for (int i = 0; i < count; i++)
                {
                    _groupMembers.Add(new FakePartyFramesMember(i));
                }
            }

            MembersChangedEvent?.Invoke(this);
        }

        #endregion

        #region raw party info
        internal unsafe class PartyListMemberInfo : IEquatable<PartyListMemberInfo>
        {
            public readonly string Name;
            public readonly uint ObjectId;
            public readonly byte Type;
            public readonly uint JobId;
            public readonly string Status;

            public PartyListMemberInfo(PartyListMemberRawInfo* info, string? crossWorldName, uint jobId, string? status)
            {
                Name = crossWorldName ?? (Marshal.PtrToStringUTF8(new IntPtr(info->NamePtr)) ?? "");
                ObjectId = info->ObjectId;
                Type = info->Type;
                JobId = jobId;
                Status = status ?? "";
            }

            public bool Equals(PartyListMemberInfo? other)
            {
                return ObjectId == other?.ObjectId && Name == other?.Name;
            }
        }

        [StructLayout(LayoutKind.Explicit, Size = 24)]
        public unsafe struct PartyListMemberRawInfo
        {
            [FieldOffset(0x00)] public byte* NamePtr;
            [FieldOffset(0x08)] public long ContentId;
            [FieldOffset(0x10)] public uint ObjectId;

            // some kind of type
            // 1 = player
            // 2 = party member?
            // 3 = unknown
            // 4 = chocobo
            // 5 = summon?
            [FieldOffset(0x14)] public byte Type;

            public string Name => Marshal.PtrToStringUTF8(new IntPtr(NamePtr)) ?? "";
        }
        #endregion
    }
}
