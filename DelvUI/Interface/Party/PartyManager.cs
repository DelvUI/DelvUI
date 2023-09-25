using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Memory;
using DelvUI.Config;
using DelvUI.Helpers;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DalamudPartyMember = Dalamud.Game.ClientState.Party.PartyMember;
using StructsFramework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;
using StructsPartyMember = FFXIVClientStructs.FFXIV.Client.Game.Group.PartyMember;

namespace DelvUI.Interface.Party
{
    public delegate void PartyMembersChangedEventHandler(PartyManager sender);

    public unsafe class PartyManager : IDisposable
    {
        #region Singleton
        public static PartyManager Instance { get; private set; } = null!;
        private PartyFramesConfig _config = null!;
        private PartyFramesIconsConfig _iconsConfig = null!;

        private PartyManager()
        {
            _readyCheckHelper = new PartyReadyCheckHelper();
            _raiseTracker = new PartyFramesRaiseTracker();
            _invulnTracker = new PartyFramesInvulnTracker();
            _cleanseTracker = new PartyFramesCleanseTracker();

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

            _readyCheckHelper.Dispose();
            _raiseTracker.Dispose();
            _invulnTracker.Dispose();
            _cleanseTracker.Dispose();

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

            _iconsConfig = ConfigurationManager.Instance.GetConfigObject<PartyFramesIconsConfig>();
        }

        #endregion Singleton

        public AddonPartyList* PartyListAddon { get; private set; } = null;
        public IntPtr HudAgent { get; private set; } = IntPtr.Zero;

        public RaptureAtkModule* RaptureAtkModule { get; private set; } = null;

        private const int PartyListInfoOffset = 0x0CD0;
        private const int PartyListMemberRawInfoSize = 0x20;
        private const int PartyJobIconIdsOffset = 0x1298;

        private const int PartyCrossWorldNameOffset = 0x14CA;
        private const int PartyCrossWorldDisplayNameOffset = 0x1462;
        private const int PartyCrossWorldEntrySize = 0xD8;

        private const int PartyTrustNameOffset = 0x0CF0;
        private const int PartyTrustEntrySize = 0x20;

        private const int PartyMembersInfoIndex = 11;
        private const int FirstOfMyRoleOrder = 8;

        private List<PartyListMemberInfo> _partyMembersInfo = null!;
        private bool _dirty = false;
        private uint _previousJob = 0;

        private List<IPartyFramesMember> _groupMembers = new List<IPartyFramesMember>();
        public IReadOnlyCollection<IPartyFramesMember> GroupMembers => _groupMembers.AsReadOnly();
        public uint MemberCount => (uint)_groupMembers.Count;

        private string? _partyTitle = null;
        public string PartyTitle => _partyTitle ?? "";

        private uint _groupMemberCount => GroupManager.Instance()->MemberCount;
        private int _realMemberCount => PartyListAddon != null ? PartyListAddon->MemberCount : Plugin.PartyList.Length;

        private PartyReadyCheckHelper _readyCheckHelper;
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

        public void Update()
        {
            // find party list hud agent
            PartyListAddon = (AddonPartyList*)Plugin.GameGui.GetAddonByName("_PartyList", 1);
            HudAgent = Plugin.GameGui.FindAgentInterface(PartyListAddon);

            if (PartyListAddon == null || HudAgent == IntPtr.Zero)
            {
                if (_groupMembers.Count > 0)
                {
                    _groupMembers.Clear();
                    _dirty = false;

                    MembersChangedEvent?.Invoke(this);
                }

                return;
            }

            UIModule* uiModule = StructsFramework.Instance()->GetUiModule();
            RaptureAtkModule = uiModule != null ? uiModule->GetRaptureAtkModule() : null;

            // no need to update on preview mode
            if (_config.Preview)
            {
                return;
            }

            InternalUpdate();
        }

        private void InternalUpdate()
        {
            PlayerCharacter? player = Plugin.ClientState.LocalPlayer;
            if (player is null || player is not PlayerCharacter)
            {
                return;
            }

            // detect job change
            if (player.ClassJob.Id != _previousJob)
            {
                _previousJob = player.ClassJob.Id;

                if (_config.PlayerOrderOverrideEnabled && _config.PlayerOrder == FirstOfMyRoleOrder)
                {
                    _dirty = true;
                }
            }

            // ready check update
            if (_iconsConfig.ReadyCheckStatus.Enabled)
            {
                _readyCheckHelper.Update(_iconsConfig.ReadyCheckStatus.Duration);
            }

            try
            {
                // title
                _partyTitle = GetPartyListTitle();

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
                    bool jobsChanged = false;
                    PartyFramesMember? playerMember = null;

                    foreach (IPartyFramesMember member in _groupMembers)
                    {
                        int index = member.ObjectId == player.ObjectId ? 0 : member.Order - 1;
                        ReadyCheckStatus readyCheckStatus = ReadyCheckStatusForMember(member, index, player.ObjectId);

                        uint jobId = JobIdForIndex(index);
                        jobsChanged = jobsChanged || jobId != member.JobId;

                        if (index == 0)
                        {
                            playerMember = member as PartyFramesMember;
                        }

                        member.Update(EnmityForIndex(index), StatusForIndex(index), readyCheckStatus, IsPartyLeader(index), jobId);
                    }

                    if (jobsChanged & playerMember != null)
                    {
                        Sort(player, playerMember);
                    }
                }
                // cross world party
                else if (IsCrossWorldParty())
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

        private bool IsCrossWorldParty()
        {
            return _groupMemberCount < _realMemberCount;
        }

        private ReadyCheckStatus ReadyCheckStatusForMember(IPartyFramesMember member, int index, uint playerId)
        {
            if (!_iconsConfig.ReadyCheckStatus.Enabled) { return ReadyCheckStatus.None; }

            bool isCrossWorld = IsCrossWorldParty();

            // regular party
            if (!isCrossWorld)
            {
                //  local player
                if (member.ObjectId == playerId)
                {
                    return _readyCheckHelper.GetStatusForIndex(0, isCrossWorld);
                }

                // find index 
                bool foundPlayer = false;
                for (int i = 0; i < Plugin.PartyList.Length; i++)
                {
                    DalamudPartyMember? dalamudMember = GetPartyMemberForIndex(i);
                    if (dalamudMember?.ObjectId == playerId)
                    {
                        foundPlayer = true;
                        continue;
                    }

                    if (dalamudMember?.ObjectId == member.ObjectId)
                    {
                        return _readyCheckHelper.GetStatusForIndex(foundPlayer ? i : i + 1, isCrossWorld);
                    }
                }
            }
            else
            {
                return _readyCheckHelper.GetStatusForIndex(index, isCrossWorld);
            }

            return ReadyCheckStatus.None;
        }

        private void UpdateTrustParty(PlayerCharacter player, int trustCount)
        {
            bool needsUpdate = _dirty || _groupMembers.Count != trustCount + 1;

            List<string> names = new List<string>();
            for (int i = 0; i < trustCount; i++)
            {
                long* namePtr = (long*)(HudAgent + (PartyTrustNameOffset + PartyTrustEntrySize * i));
                string? name = Marshal.PtrToStringUTF8(new IntPtr(*namePtr));
                names.Add(name ?? i.ToString());

                if (_groupMembers.Count > i + 1 && name != _groupMembers[i + 1].Name)
                {
                    needsUpdate = true;
                }
            }

            if (needsUpdate)
            {
                _groupMembers.Clear();

                PartyFramesMember playerMember = new PartyFramesMember(player, 1, EnmityForIndex(0), PartyMemberStatus.None, ReadyCheckStatus.None, true);
                _groupMembers.Add(playerMember);

                int order = 2;

                for (int i = 0; i < trustCount; i++)
                {
                    Character? trustChara = Utils.GetGameObjectByName(names[i]) as Character;
                    if (trustChara != null)
                    {
                        _groupMembers.Add(new PartyFramesMember(trustChara, order, EnmityForTrustMemberIndex(i), PartyMemberStatus.None, ReadyCheckStatus.None, false));
                        order++;
                    }
                }

                Sort(player, playerMember);
            }
            else
            {
                for (int i = 0; i < _groupMembers.Count; i++)
                {
                    if (_groupMembers[i].ObjectId == player.ObjectId)
                    {
                        _groupMembers[i].Update(EnmityForIndex(0), PartyMemberStatus.None, ReadyCheckStatus.None, true, player.ClassJob.Id);
                    }
                    else
                    {
                        _groupMembers[i].Update(EnmityForTrustMemberIndex(Math.Max(0, _groupMembers[i].Order - 2)), PartyMemberStatus.None, ReadyCheckStatus.None, false, 0);
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

                _groupMembers.Add(new PartyFramesMember(player, 1, playerEnmity, PartyMemberStatus.None, ReadyCheckStatus.None, true));

                if (chocobo != null)
                {
                    _groupMembers.Add(new PartyFramesMember(chocobo, 2, chocoboEnmity, PartyMemberStatus.None, ReadyCheckStatus.None, false));
                }

                MembersChangedEvent?.Invoke(this);
            }
            else
            {
                for (int i = 0; i < _groupMembers.Count; i++)
                {
                    _groupMembers[i].Update(i == 0 ? playerEnmity : chocoboEnmity, PartyMemberStatus.None, ReadyCheckStatus.None, i == 0, i == 0 ? player.ClassJob.Id : 0);
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

                int order = i + 1;
                EnmityLevel enmity = EnmityForIndex(i);
                PartyMemberStatus status = StatusForIndex(i);
                bool isPartyLeader = IsPartyLeader(i);

                PartyFramesMember member = isPlayer ?
                    new PartyFramesMember(player, order, enmity, status, ReadyCheckStatus.None, isPartyLeader) :
                    new PartyFramesMember(NameForIndex(i), order, JobIdForIndex(i), status, ReadyCheckStatus.None, isPartyLeader);
                _groupMembers.Add(member);
            }

            Sort(player, null);
        }

        private void UpdateRegularParty(PlayerCharacter player)
        {
            // create new members array with dalamud's data
            _groupMembers.Clear();

            PartyFramesMember? playerMember = null;

            for (int i = 0; i < _groupMemberCount; i++)
            {
                DalamudPartyMember? partyMember = GetPartyMemberForIndex(i);
                if (partyMember == null) { continue; }

                bool isPlayer = partyMember.ObjectId == player.ObjectId;
                int order = isPlayer ? 1 : (IndexForPartyMember(partyMember) ?? 9);
                int index = isPlayer ? 0 : order - 1;
                EnmityLevel enmity = EnmityForIndex(index);
                PartyMemberStatus status = StatusForIndex(index);
                bool isPartyLeader = i == Plugin.PartyList.PartyLeaderIndex;

                PartyFramesMember member = new PartyFramesMember(partyMember, order, enmity, status, ReadyCheckStatus.None, isPartyLeader);

                if (isPlayer)
                {
                    playerMember = member;
                }

                _groupMembers.Add(member);

                // player's chocobo (always last)
                if (_config.ShowChocobo && member.ObjectId == player.ObjectId)
                {
                    var companion = Utils.GetBattleChocobo(player);
                    if (companion is Character companionCharacter)
                    {
                        _groupMembers.Add(new PartyFramesMember(companionCharacter, 10, EnmityLevel.Last, PartyMemberStatus.None, ReadyCheckStatus.None, false));
                    }
                }
            }

            Sort(player, playerMember);
        }

        private DalamudPartyMember? GetPartyMemberForIndex(int index)
        {
            if (_groupMemberCount <= 0)
            {
                return null;
            }

            StructsPartyMember* memberStruct = GroupManager.Instance()->GetPartyMemberByIndex(index);
            return Plugin.PartyList.CreatePartyMemberReference(new IntPtr(memberStruct));
        }

        private void Sort(PlayerCharacter player, PartyFramesMember? playerMember)
        {
            // calculate player overriden position
            if (playerMember != null && _config.PlayerOrderOverrideEnabled)
            {
                if (_config.PlayerOrder == FirstOfMyRoleOrder)
                {
                    int? roleFirstOrder = PartyOrderHelper.GetRoleFirstOrder(_groupMembers);
                    if (roleFirstOrder.HasValue)
                    {
                        playerMember.Order = roleFirstOrder.Value + 1;
                    }
                }
                else
                {
                    playerMember.Order = _config.PlayerOrder + 1;
                }
            }

            // sort
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

        private int? IndexForPartyMember(Dalamud.Game.ClientState.Party.PartyMember member)
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

        private static unsafe string? GetPartyListTitle()
        {
            AgentModule* agentModule = AgentModule.Instance();
            if (agentModule == null) { return ""; }

            AgentHUD* agentHUD = agentModule->GetAgentHUD();
            if (agentHUD == null) { return ""; }

            return Plugin.DataManager.GetExcelSheet<Addon>()?.GetRow(agentHUD->PartyTitleAddonId)?.Text.ToString();
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
            _iconsConfig.Sign.Preview = _config.Preview;

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
