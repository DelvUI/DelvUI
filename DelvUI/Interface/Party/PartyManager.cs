using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Memory;
using DelvUI.Config;
using DelvUI.Helpers;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static FFXIVClientStructs.FFXIV.Client.Game.Group.GroupManager;
using DalamudPartyMember = Dalamud.Game.ClientState.Party.IPartyMember;
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

            // find offline string for active language
            if (Plugin.DataManager.GetExcelSheet<Addon>().TryGetRow(9836, out Addon row))
            {
                _offlineString = row.Text.ToString();
            }
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

        private RaptureAtkModule* _raptureAtkModule = null;

        private const int PartyListInfoOffset = 0x0D40;
        private const int PartyListMemberRawInfoSize = 0x28;

        private const int PartyMembersInfoIndex = 11;

        private List<IPartyFramesMember> _groupMembers = new List<IPartyFramesMember>();
        public IReadOnlyCollection<IPartyFramesMember> GroupMembers => _groupMembers.AsReadOnly();

        private List<IPartyFramesMember> _sortedGroupMembers = new List<IPartyFramesMember>();
        public IReadOnlyCollection<IPartyFramesMember> SortedGroupMembers => _sortedGroupMembers.AsReadOnly();

        public uint MemberCount => (uint)_groupMembers.Count;

        private string? _partyTitle = null;
        public string PartyTitle => _partyTitle ?? "";

        private int _groupMemberCount => GroupManager.Instance()->MainGroup.MemberCount;
        private int _realMemberCount => PartyListAddon != null ? PartyListAddon->MemberCount : Plugin.PartyList.Length;
        private int _prevMemberCount = 0;

        private Dictionary<string, InternalMemberData> _prevDataMap = new();

        private bool _wasRealGroup = false;
        private bool _wasCrossWorld = false;

        private InfoProxyCrossRealm* _crossRealmInfo => InfoProxyCrossRealm.Instance();
        private Group _mainGroup => GroupManager.Instance()->MainGroup;

        private string _offlineString = "offline";

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
                _groupMembers[1].Character is IBattleNpc npc && npc.BattleNpcKind == BattleNpcSubKind.Chocobo);
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
                    MembersChangedEvent?.Invoke(this);
                }

                return;
            }

            _raptureAtkModule = RaptureAtkModule.Instance();

            // no need to update on preview mode
            if (_config.Preview)
            {
                return;
            }

            InternalUpdate();
        }

        private void InternalUpdate()
        {
            IPlayerCharacter? player = Plugin.ClientState.LocalPlayer;
            if (player is null || player is not IPlayerCharacter)
            {
                return;
            }

            bool isCrossWorld = IsCrossWorldParty();

            // ready check update
            if (_iconsConfig.ReadyCheckStatus.Enabled)
            {
                _readyCheckHelper.Update(_iconsConfig.ReadyCheckStatus.Duration);
            }

            try
            {
                // title
                _partyTitle = GetPartyListTitle();

                // solo
                if (_realMemberCount <= 1 && PartyListAddon->TrustCount == 0)
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

                    _wasRealGroup = false;
                }
                else
                {
                    // player maps
                    Dictionary<string, InternalMemberData> dataMap = GetMembersDataMap(isCrossWorld);
                    bool partyChanged = _prevDataMap.Count != dataMap.Count;

                    if (!partyChanged)
                    {
                        foreach (string key in dataMap.Keys)
                        {
                            InternalMemberData newData = dataMap[key];
                            if (!_prevDataMap.TryGetValue(key, out InternalMemberData oldData) ||
                                newData.Order != oldData.Order)
                            {
                                partyChanged = true;
                                break;
                            }
                        }
                    }

                    if (partyChanged)
                    {
                        Plugin.Logger.Debug(partyChanged.ToString());
                    }
                    _prevDataMap = dataMap;

                    // trust
                    if (PartyListAddon->TrustCount > 0)
                    {
                        UpdateTrustParty(player, dataMap, partyChanged);
                    }
                    // cross world party
                    else if (isCrossWorld)
                    {
                        UpdateCrossWorldParty(player, dataMap, partyChanged);
                    }
                    // regular party
                    else
                    {
                        UpdateRegularParty(player, dataMap, partyChanged);
                    }

                    _wasRealGroup = true;
                }

                UpdateTrackers();
            }
            catch { }

            _prevMemberCount = _groupMemberCount;
            _wasCrossWorld = isCrossWorld;
        }

        private Dictionary<string, InternalMemberData> GetMembersDataMap(bool isCrossWorld)
        {
            Dictionary<string, InternalMemberData> dataMap = new Dictionary<string, InternalMemberData>();

            if (_raptureAtkModule == null || _raptureAtkModule->AtkModule.AtkArrayDataHolder.StringArrayCount <= PartyMembersInfoIndex)
            {
                return dataMap;
            }

            Plugin.Logger.Debug(HudAgent.ToString("X"));
            int count = isCrossWorld ? _crossRealmInfo->CrossRealmGroups[0].GroupMemberCount : _realMemberCount + PartyListAddon->TrustCount;

            var stringArrayData = _raptureAtkModule->AtkModule.AtkArrayDataHolder.StringArrays[PartyMembersInfoIndex];
            for (int i = 0; i < count; i++)
            {
                int order = i;
                string name = "asd";

                if (!isCrossWorld)
                {
                    PartyListMemberRawInfo* info = (PartyListMemberRawInfo*)(HudAgent + (PartyListInfoOffset + PartyListMemberRawInfoSize * i));
                    name = info->Name;
                    order = info->Order;
                }
                else
                {
                    name = _crossRealmInfo->CrossRealmGroups[0].GroupMembers[i].NameString;
                }

                Plugin.Logger.Debug(name);

                int index = i * 5;
                if (stringArrayData->AtkArrayData.Size <= index + 3 ||
                    stringArrayData->StringArray[index] == null ||
                    stringArrayData->StringArray[index + 3] == null) { break; }

                IntPtr ptr = new IntPtr(stringArrayData->StringArray[index + 3]);
                string status = MemoryHelper.ReadSeStringNullTerminated(ptr).ToString();

                if (!dataMap.ContainsKey(name))
                {
                    dataMap.Add(name, new InternalMemberData(order, status));
                }
            }

            return dataMap;
        }

        private bool IsCrossWorldParty()
        {
            return _crossRealmInfo->IsCrossRealm > 0 && _crossRealmInfo->GroupCount > 0 && _mainGroup.MemberCount == 0;
        }

        private ReadyCheckStatus GetReadyCheckStatus(ulong contentId)
        {
            return _readyCheckHelper.GetStatusForContentId(contentId);
        }

        private void UpdateTrustParty(IPlayerCharacter player, Dictionary<string, InternalMemberData> dataMap, bool forced)
        {
            bool softUpdate = true;

            if (_groupMembers.Count != dataMap.Count || forced)
            {
                _groupMembers.Clear();
                softUpdate = false;
            }

            if (softUpdate)
            {
                foreach (IPartyFramesMember member in _groupMembers)
                {
                    if (member.ObjectId == player.GameObjectId)
                    {
                        member.Update(EnmityForIndex(member.Order), PartyMemberStatus.None, ReadyCheckStatus.None, true, player.ClassJob.RowId);
                    }
                    else
                    {
                        member.Update(EnmityForTrustMemberIndex(member.Order), PartyMemberStatus.None, ReadyCheckStatus.None, false, 0);
                    }
                }
            }
            else
            {
                string[] keys = dataMap.Keys.ToArray();
                for (int i = 0; i < keys.Length; i++)
                {
                    InternalMemberData data = dataMap[keys[i]];
                    if (keys[i] == player.Name.ToString())
                    {
                        PartyFramesMember playerMember = new PartyFramesMember(player, i, data.Order, EnmityForIndex(data.Order), PartyMemberStatus.None, ReadyCheckStatus.None, true);
                        _groupMembers.Add(playerMember);
                    }
                    else
                    {
                        ICharacter? trustChara = Utils.GetGameObjectByName(keys[i]) as ICharacter;
                        if (trustChara != null)
                        {
                            _groupMembers.Add(new PartyFramesMember(trustChara, i, data.Order, EnmityForTrustMemberIndex(data.Order), PartyMemberStatus.None, ReadyCheckStatus.None, false));
                        }
                    }
                }
            }

            if (!softUpdate)
            {
                SortGroupMembers(player);
                MembersChangedEvent?.Invoke(this);
            }
        }

        private void UpdateSoloParty(IPlayerCharacter player)
        {
            ICharacter? chocobo = null;
            if (_config.ShowChocobo)
            {
                var gameObject = Utils.GetBattleChocobo(player);
                if (gameObject != null && gameObject is ICharacter)
                {
                    chocobo = (ICharacter)gameObject;
                }
            }

            bool needsUpdate =
                _groupMembers.Count == 0 ||
                (_groupMembers.Count != 2 && _config.ShowChocobo && chocobo != null) ||
                (_groupMembers.Count > 1 && !_config.ShowChocobo) ||
                (_groupMembers.Count > 1 && chocobo == null) ||
                (_groupMembers.Count == 2 && _config.ShowChocobo && _groupMembers[1].ObjectId != chocobo?.EntityId);

            EnmityLevel playerEnmity = PartyListAddon->EnmityLeaderIndex == 0 ? EnmityLevel.Leader : EnmityLevel.Last;

            // for some reason chocobos never get a proper enmity value even though they have aggro
            // if the player enmity is set to first, but the "leader index" is invalid
            // we can pretty much deduce that the chocobo is the one with aggro
            // this might fail on some cases when there are other players not in party hitting the same thing
            // but the edge case is so minor we should be fine
            EnmityLevel chocoboEnmity = PartyListAddon->EnmityLeaderIndex == -1 && PartyListAddon->PartyMembers[0].EmnityByte == 1 ? EnmityLevel.Leader : EnmityLevel.Last;

            if (needsUpdate)
            {
                _groupMembers.Clear();

                _groupMembers.Add(new PartyFramesMember(player, 0, 0, playerEnmity, PartyMemberStatus.None, ReadyCheckStatus.None, true));

                if (chocobo != null)
                {
                    _groupMembers.Add(new PartyFramesMember(chocobo, 1, 1, chocoboEnmity, PartyMemberStatus.None, ReadyCheckStatus.None, false));
                }

                SortGroupMembers(player);
                MembersChangedEvent?.Invoke(this);
            }
            else
            {
                for (int i = 0; i < _groupMembers.Count; i++)
                {
                    _groupMembers[i].Update(i == 0 ? playerEnmity : chocoboEnmity, PartyMemberStatus.None, ReadyCheckStatus.None, i == 0, i == 0 ? player.ClassJob.RowId : 0);
                }
            }
        }

        private void UpdateCrossWorldParty(IPlayerCharacter player, Dictionary<string, InternalMemberData> dataMap, bool forced)
        {
            bool softUpdate = true;

            int count = _crossRealmInfo->CrossRealmGroups[0].GroupMemberCount;
            if (!_wasCrossWorld || count != _prevMemberCount || forced)
            {
                _groupMembers.Clear();
                softUpdate = false;
            }

            // create new members array with cross world data
            for (int i = 0; i < _crossRealmInfo->CrossRealmGroups[0].GroupMemberCount; i++)
            {
                CrossRealmMember member = _crossRealmInfo->CrossRealmGroups[0].GroupMembers[i];
                string memberName = member.NameString;

                if (!dataMap.TryGetValue(memberName, out InternalMemberData data))
                {
                    continue;
                }

                bool isPlayer = member.EntityId == player.EntityId;
                bool isLeader = member.IsPartyLeader > 0;
                PartyMemberStatus status = data.Status != null ? StatusForMember(data.Status, i) : PartyMemberStatus.None;
                ReadyCheckStatus readyCheckStatus = GetReadyCheckStatus(member.ContentId);

                if (softUpdate)
                {
                    IPartyFramesMember groupMember = _groupMembers.ElementAt(i);
                    groupMember.Update(EnmityLevel.Last, status, readyCheckStatus, isLeader, member.ClassJobId);
                }
                else
                {
                    PartyFramesMember partyMember = isPlayer ?
                        new PartyFramesMember(player, i, data.Order, EnmityLevel.Last, status, readyCheckStatus, isLeader) :
                        new PartyFramesMember(memberName, i, data.Order, member.ClassJobId, status, readyCheckStatus, isLeader);
                    _groupMembers.Add(partyMember);
                }
            }

            if (!softUpdate)
            {
                SortGroupMembers(player);
                MembersChangedEvent?.Invoke(this);
            }
        }

        private void UpdateRegularParty(IPlayerCharacter player, Dictionary<string, InternalMemberData> dataMap, bool forced)
        {
            bool softUpdate = true;

            if (!_wasRealGroup || _groupMemberCount != _prevMemberCount || forced)
            {
                _groupMembers.Clear();
                softUpdate = false;
            }

            for (int i = 0; i < _groupMemberCount; i++)
            {
                DalamudPartyMember? member = GetPartyMemberForIndex(i);
                if (member == null) { continue; }

                if (!dataMap.TryGetValue(member.Name.ToString(), out InternalMemberData data))
                {
                    continue;
                }

                bool isPlayer = member.ObjectId == player.GameObjectId;
                bool isLeader = _mainGroup.PartyLeaderIndex == i;
                EnmityLevel enmity = EnmityForIndex(i);
                PartyMemberStatus status = data.Status != null ? StatusForMember(data.Status, i) : PartyMemberStatus.None;
                ReadyCheckStatus readyCheckStatus = GetReadyCheckStatus((ulong)member.ContentId);

                if (softUpdate)
                {
                    IPartyFramesMember groupMember = _groupMembers.ElementAt(i);
                    groupMember.Update(enmity, status, readyCheckStatus, isLeader, member.ClassJob.RowId);
                }
                else
                {
                    PartyFramesMember partyMember = new PartyFramesMember(member, i, data.Order, enmity, status, readyCheckStatus, isLeader);
                    _groupMembers.Add(partyMember);
                }
            }

            // player's chocobo (always last)
            if (!softUpdate && _config.ShowChocobo)
            {
                var companion = Utils.GetBattleChocobo(player);
                if (companion is ICharacter companionCharacter)
                {
                    _groupMembers.Add(new PartyFramesMember(companionCharacter, _groupMemberCount, 10, EnmityLevel.Last, PartyMemberStatus.None, ReadyCheckStatus.None, false));
                }
            }

            if (!softUpdate)
            {
                SortGroupMembers(player);
                MembersChangedEvent?.Invoke(this);
            }
        }


        private void SortGroupMembers(IPlayerCharacter player)
        {
            _sortedGroupMembers.Clear();
            _sortedGroupMembers.AddRange(_groupMembers);

            _sortedGroupMembers.Sort((a, b) =>
            {
                if (a.Order == b.Order)
                {
                    if (a.ObjectId == player.GameObjectId)
                    {
                        return 1;
                    }
                    else if (b.ObjectId == player.GameObjectId)
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

        private DalamudPartyMember? GetPartyMemberForIndex(int index)
        {
            if (_groupMemberCount <= 0)
            {
                return null;
            }

            StructsPartyMember* memberStruct = GroupManager.Instance()->GetGroup()->GetPartyMemberByIndex(index);
            return Plugin.PartyList.CreatePartyMemberReference(new IntPtr(memberStruct));
        }

        private void UpdateTrackers()
        {
            _raiseTracker.Update(_groupMembers);
            _invulnTracker.Update(_groupMembers);
            _cleanseTracker.Update(_groupMembers);
        }

        #region utils

        private PartyMemberStatus StatusForMember(string name, int index)
        {
            // TODO: support for other languages
            // couldn't figure out another way of doing this sadly

            // offline status
            if (name.Contains(_offlineString, StringComparison.InvariantCultureIgnoreCase))
            {
                return PartyMemberStatus.Offline;
            }

            // viewing cutscene status
            if (index >= 0 && index < _mainGroup.MemberCount &&
                (_mainGroup.PartyMembers[index].Flags & 0x10) != 0)
            {
                return PartyMemberStatus.ViewingCutscene;
            }

            return PartyMemberStatus.None;
        }

        private EnmityLevel EnmityForIndex(int index)
        {
            if (PartyListAddon == null || index < 0 || index > 7)
            {
                return EnmityLevel.Last;
            }

            EnmityLevel enmityLevel = (EnmityLevel)PartyListAddon->PartyMembers[index].EmnityByte;
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

            return (EnmityLevel)PartyListAddon->TrustMembers[index].EmnityByte;
        }

        private static unsafe string? GetPartyListTitle()
        {
            AgentModule* agentModule = AgentModule.Instance();
            if (agentModule == null) { return ""; }

            AgentHUD* agentHUD = agentModule->GetAgentHUD();
            if (agentHUD == null) { return ""; }

            Lumina.Excel.ExcelSheet<Addon> sheet = Plugin.DataManager.GetExcelSheet<Addon>();
            if (sheet.TryGetRow(agentHUD->PartyTitleAddonId, out Addon row))
            {
                return row.Text.ToString();
            }

            return null;
        }
        #endregion

        #region events
        private void OnConfigPropertyChanged(object sender, OnChangeBaseArgs args)
        {
            if (args.PropertyName == "Preview")
            {
                UpdatePreview();
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
    }

    internal struct InternalMemberData
    {
        internal int Order;
        internal string? Status;

        public InternalMemberData(int order, string? status)
        {
            Order = order;
            Status = status;
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 28)]
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

        [FieldOffset(0x18)] public byte Order;

        public string Name => Marshal.PtrToStringUTF8(new IntPtr(NamePtr)) ?? "";
    }
}
