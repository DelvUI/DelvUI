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
        private int _realMemberAndChocoboCount => PartyListAddon != null ? PartyListAddon->MemberCount + Math.Max(1, (int)PartyListAddon->ChocoboCount) : Plugin.PartyList.Length;

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
            PartyListAddon = (AddonPartyList*)Plugin.GameGui.GetAddonByName("_PartyList", 1).Address;
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
                            if (!_prevDataMap.TryGetValue(key, out InternalMemberData? oldData) ||
                                oldData == null ||
                                newData.Order != oldData.Order)
                            {
                                partyChanged = true;
                                break;
                            }
                        }
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
            catch (Exception e)
            {
                Plugin.Logger.Warning(e.Message);
            }

            _wasCrossWorld = isCrossWorld;
        }

        private Dictionary<string, InternalMemberData> GetMembersDataMap(bool isCrossWorld)
        {
            Dictionary<string, InternalMemberData> dataMap = new Dictionary<string, InternalMemberData>();

            if (_raptureAtkModule == null || _raptureAtkModule->AtkModule.AtkArrayDataHolder.StringArrayCount <= PartyMembersInfoIndex)
            {
                return dataMap;
            }

            // raw info
            int count = isCrossWorld ? _crossRealmInfo->CrossRealmGroups[0].GroupMemberCount : _realMemberCount + PartyListAddon->TrustCount;
            for (int i = 0; i < count; i++)
            {
                InternalMemberData data = new InternalMemberData();
                data.Index = i;

                if (!isCrossWorld)
                {
                    PartyListMemberRawInfo* info = (PartyListMemberRawInfo*)(HudAgent + (PartyListInfoOffset + PartyListMemberRawInfoSize * i));
                    data.ObjectId = info->ObjectId;
                    data.ContentId = info->ContentId;
                    data.Name = info->Name;
                    data.Order = info->Order;
                }
                else
                {
                    CrossRealmMember member = _crossRealmInfo->CrossRealmGroups[0].GroupMembers[i];
                    data.ObjectId = member.EntityId;
                    data.ContentId = (long)member.ContentId;
                    data.Name = member.NameString;
                    data.Order = i;
                }

                if (!dataMap.ContainsKey(data.Name))
                {
                    dataMap.Add(data.Name, data);
                }
            }

            // status string
            var stringArrayData = _raptureAtkModule->AtkModule.AtkArrayDataHolder.StringArrays[PartyMembersInfoIndex];
            for (int i = 0; i < count; i++)
            {
                int index = i * 5;
                if (stringArrayData->AtkArrayData.Size <= index + 3 ||
                    stringArrayData->StringArray[index] == null ||
                    stringArrayData->StringArray[index + 3] == null) { break; }

                IntPtr ptr = new IntPtr(stringArrayData->StringArray[index]);
                string name = MemoryHelper.ReadSeStringNullTerminated(ptr).ToString();

                ptr = new IntPtr(stringArrayData->StringArray[index + 3]);

                string a = MemoryHelper.ReadSeStringNullTerminated(ptr).ToString();


                if (dataMap.TryGetValue(name, out InternalMemberData? data) && data != null)
                {
                    data.Status = MemoryHelper.ReadSeStringNullTerminated(ptr).ToString();
                }
            }

            return dataMap;
        }

        private bool IsCrossWorldParty()
        {
            return _crossRealmInfo->IsCrossRealm && _crossRealmInfo->GroupCount > 0 && _mainGroup.MemberCount == 0;
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
                        member.Update(EnmityForIndex(member.Index), PartyMemberStatus.None, ReadyCheckStatus.None, true, player.ClassJob.RowId);
                    }
                    else
                    {
                        member.Update(EnmityForTrustMemberIndex(member.Index - 1), PartyMemberStatus.None, ReadyCheckStatus.None, false, 0);
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
                        PartyFramesMember playerMember = new PartyFramesMember(player, data.Index, data.Order, EnmityForIndex(i), PartyMemberStatus.None, ReadyCheckStatus.None, true);
                        _groupMembers.Add(playerMember);
                    }
                    else
                    {
                        ICharacter? trustChara = Utils.GetGameObjectByName(keys[i]) as ICharacter;
                        if (trustChara != null)
                        {
                            _groupMembers.Add(new PartyFramesMember(trustChara, data.Index, data.Order, EnmityForTrustMemberIndex(i), PartyMemberStatus.None, ReadyCheckStatus.None, false));
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
            if (!_wasCrossWorld || count != _groupMembers.Count || forced)
            {
                _groupMembers.Clear();
                softUpdate = false;
            }

            // create new members array with cross world data
            for (int i = 0; i < _crossRealmInfo->CrossRealmGroups[0].GroupMemberCount; i++)
            {
                CrossRealmMember member = _crossRealmInfo->CrossRealmGroups[0].GroupMembers[i];
                string memberName = member.NameString;

                if (!dataMap.TryGetValue(memberName, out InternalMemberData? data) || data == null)
                {
                    continue;
                }

                bool isPlayer = member.EntityId == player.EntityId;
                bool isLeader = member.IsPartyLeader;
                PartyMemberStatus status = data.Status != null ? StatusForCrossWorldMember(data.Status) : PartyMemberStatus.None;
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

            if (!_wasRealGroup || _realMemberCount != _groupMembers.Count || forced)
            {
                _groupMembers.Clear();
                softUpdate = false;
            }

            string[] keys = dataMap.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
            {
                if (!dataMap.TryGetValue(keys[i], out InternalMemberData? data) || data == null)
                {
                    continue;
                }

                bool isPlayer = data.ObjectId == player.GameObjectId;
                bool isLeader = IsPartyLeader(data.Order);
                EnmityLevel enmity = EnmityForIndex(data.Index);
                PartyMemberStatus status = data.Status != null ? StatusForMember(data.Status, data.Name) : PartyMemberStatus.None;
                ReadyCheckStatus readyCheckStatus = GetReadyCheckStatus((ulong)data.ContentId);

                if (softUpdate)
                {
                    IPartyFramesMember groupMember = _groupMembers.ElementAt(i);
                    groupMember.Update(enmity, status, readyCheckStatus, isLeader);
                }
                else
                {
                    PartyFramesMember partyMember;
                    var member = GetDalamudPartyMember(data.Name);
                    if (member.HasValue && member.Value.Item1 is DalamudPartyMember dalamudPartyMember)
                    {
                        partyMember = new PartyFramesMember(dalamudPartyMember, i, data.Order, enmity, status, readyCheckStatus, isLeader);
                    }
                    else
                    {
                        partyMember = new PartyFramesMember(data.ObjectId, i, data.Order, enmity, status, readyCheckStatus, isLeader);
                    }
                    _groupMembers.Add(partyMember);
                }
            }

            // player's chocobo (always last)
            if (_config.ShowChocobo)
            {
                IGameObject? companion = Utils.GetBattleChocobo(player);

                if (softUpdate && _groupMembers.FirstOrDefault(o => o.IsChocobo) is PartyFramesMember chocoboMember)
                {
                    if (companion is ICharacter)
                    {
                        chocoboMember.Update(EnmityLevel.Last, PartyMemberStatus.None, ReadyCheckStatus.None, false);
                    }
                    else
                    {
                        _groupMembers.Remove(chocoboMember);
                    }
                }
                else if (companion is ICharacter companionCharacter)
                {
                    _groupMembers.Add(new PartyFramesMember(companionCharacter, _groupMemberCount, 10, EnmityLevel.Last, PartyMemberStatus.None, ReadyCheckStatus.None, false, true));
                }
            }

            if (!softUpdate)
            {
                SortGroupMembers(player);
                MembersChangedEvent?.Invoke(this);
            }
        }


        private void SortGroupMembers(IPlayerCharacter? player = null)
        {
            _sortedGroupMembers.Clear();
            _sortedGroupMembers.AddRange(_groupMembers);

            _sortedGroupMembers.Sort((a, b) =>
            {
                if (a.Order == b.Order)
                {
                    if (a.ObjectId == player?.GameObjectId)
                    {
                        return 1;
                    }
                    else if (b.ObjectId == player?.GameObjectId)
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

        private (DalamudPartyMember?, int)? GetDalamudPartyMember(string name)
        {
            for (int i = 0; i < Plugin.PartyList.Length; i++)
            {
                DalamudPartyMember? member = Plugin.PartyList[i];
                if (member != null && member.Name.ToString() == name)
                {
                    return (member, i);
                }
            }

            return null;
        }

        private void UpdateTrackers()
        {
            _raiseTracker.Update(_groupMembers);
            _invulnTracker.Update(_groupMembers);
            _cleanseTracker.Update(_groupMembers);
        }

        #region utils
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

        private PartyMemberStatus StatusForCrossWorldMember(string statusStr)
        {
            // offline status
            if (statusStr.Contains(_offlineString, StringComparison.InvariantCultureIgnoreCase))
            {
                return PartyMemberStatus.Offline;
            }

            return PartyMemberStatus.None;
        }


        private PartyMemberStatus StatusForMember(string statusStr, string name)
        {
            // offline status
            if (statusStr.Contains(_offlineString, StringComparison.InvariantCultureIgnoreCase))
            {
                return PartyMemberStatus.Offline;
            }

            // viewing cutscene status
            for (int i = 0; i < _mainGroup.MemberCount; i++)
            {
                if (_mainGroup.PartyMembers[i].NameString == name)
                {
                    if ((_mainGroup.PartyMembers[i].Flags & 0x10) != 0)
                    {
                        return PartyMemberStatus.ViewingCutscene;
                    }
                    break;
                }
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

            SortGroupMembers();
            MembersChangedEvent?.Invoke(this);
        }
        #endregion
    }

    internal class InternalMemberData
    {
        internal uint ObjectId = 0;
        internal long ContentId = 0;
        internal string Name = "";
        internal uint JobId = 0;
        internal int Order = 0;
        internal int Index = 0;
        internal string? Status = null;

        public InternalMemberData()
        {
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
