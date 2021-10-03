using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Logging;
using DelvUI.Config;
using DelvUI.Helpers;
using FFXIVClientStructs.FFXIV.Client.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace DelvUI.Interface.Party
{
    public delegate void PartyMembersChangedEventHandler(PartyManager sender);

    public unsafe class PartyManager : IDisposable
    {
        #region Singleton
        public static PartyManager Instance { get; private set; } = null!;
        private PartyFramesConfig _config;

        private PartyManager(PartyFramesConfig config)
        {
            _raiseTracker = new PartyFramesRaiseTracker();

            Plugin.Framework.Update += FrameworkOnOnUpdateEvent;

            _config = config;
            _config.ValueChangeEvent += OnConfigPropertyChanged;

            UpdatePreview();
        }

        public static void Initialize()
        {
            var config = ConfigurationManager.Instance.GetConfigObject<PartyFramesConfig>();
            Instance = new PartyManager(config);
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

            Plugin.Framework.Update -= FrameworkOnOnUpdateEvent;
            _config.ValueChangeEvent -= OnConfigPropertyChanged;

            Instance = null!;
        }


        #endregion Singleton

        public AddonPartyList* PartyListAddon { get; private set; } = null;
        public IntPtr HudAgent { get; private set; } = IntPtr.Zero;

        private const int PartyListInfoOffset = 0x0B50;
        private const int PartyListMemberRawInfoSize = 0x18;
        private const int PartyJobIconIdsOffset = 0x0F20;

        private const int PartyCrossWorldNameOffset = 0x0E52;
        private const int PartyCrossWorldEntrySize = 0xD8;

        private List<PartyListMemberInfo> _partyMembersInfo = null!;
        private bool _playerOrderChanged = false;

        private List<IPartyFramesMember> _groupMembers = new List<IPartyFramesMember>();
        public IReadOnlyCollection<IPartyFramesMember> GroupMembers => _groupMembers.AsReadOnly();
        public uint MemberCount => (uint)_groupMembers.Count;

        private PartyFramesRaiseTracker _raiseTracker;

        public event PartyMembersChangedEventHandler? MembersChangedEvent;


        private void FrameworkOnOnUpdateEvent(Framework framework)
        {
            // find party list hud agent
            if (HudAgent == IntPtr.Zero || PartyListAddon == null)
            {
                PartyListAddon = (AddonPartyList*)Plugin.GameGui.GetAddonByName("_PartyList", 1);
                HudAgent = Plugin.GameGui.FindAgentInterface(PartyListAddon);
            }

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

                    _raiseTracker.Update(_groupMembers);
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
                        member.Update(EnmityForIndex(index), IsLeader(index), JobIdForIndex(index));
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

                _raiseTracker.Update(_groupMembers);
            }
            catch (Exception e)
            {
                PluginLog.LogError("ERROR getting party data: " + e.Message);
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

            bool needsUpdate = _groupMembers.Count == 0 ||
                (_groupMembers.Count == 1 && _config.ShowChocobo) ||
                (_groupMembers.Count > 1 && !_config.ShowChocobo) ||
                (_groupMembers.Count > 1 && chocobo == null);

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

                _groupMembers.Add(new PartyFramesMember(player, 1, playerEnmity, true));

                if (chocobo != null)
                {
                    _groupMembers.Add(new PartyFramesMember(chocobo, 2, chocoboEnmity, false));
                }

                MembersChangedEvent?.Invoke(this);
            }
            else
            {
                for (int i = 0; i < _groupMembers.Count; i++)
                {
                    _groupMembers[i].Update(i == 0 ? playerEnmity : chocoboEnmity, i == 0, i == 0 ? player.ClassJob.Id : 0);
                }
            }
        }

        private bool ParseRawData()
        {
            bool partyChanged = _playerOrderChanged || _partyMembersInfo == null || _groupMembers.Count != _realMemberCount;

            if (HudAgent != IntPtr.Zero)
            {
                List<PartyListMemberInfo> newInfo = new List<PartyListMemberInfo>(_realMemberCount);

                for (int i = 0; i < _realMemberCount; i++)
                {
                    PartyListMemberRawInfo* info = (PartyListMemberRawInfo*)(HudAgent + (PartyListInfoOffset + PartyListMemberRawInfoSize * i));
                    newInfo.Add(new PartyListMemberInfo(info, NameForIndex(i), JobIdForIndex(i)));
                }

                if (!partyChanged && _partyMembersInfo != null)
                {
                    partyChanged = !newInfo.SequenceEqual(_partyMembersInfo);
                }
                _partyMembersInfo = newInfo;
            }

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

                var enmity = EnmityForIndex(isPlayer ? 0 : order - 1);
                var isPartyLeader = IsLeader(i);

                var member = isPlayer ?
                    new PartyFramesMember(player, order, enmity, isPartyLeader) :
                    new PartyFramesMember(NameForIndex(i), order, JobIdForIndex(i), isPartyLeader);

                _groupMembers.Add(member);
            }

            // sort according to default party list
            SortGroupMembers(player);
            _playerOrderChanged = false;

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

                var enmity = EnmityForIndex(isPlayer ? 0 : order - 1);
                var isPartyLeader = i == Plugin.PartyList.PartyLeaderIndex;

                var member = new PartyFramesMember(partyMember, order, enmity, isPartyLeader);
                _groupMembers.Add(member);

                // player's chocobo (always last)
                if (_config.ShowChocobo && member.ObjectId == player.ObjectId)
                {
                    var companion = Utils.GetBattleChocobo(player);
                    if (companion is Character companionCharacter)
                    {
                        _groupMembers.Add(new PartyFramesMember(companionCharacter, 10, EnmityLevel.Last, false));
                    }
                }
            }

            // sort according to default party list
            SortGroupMembers(player);
            _playerOrderChanged = false;

            // fire event
            MembersChangedEvent?.Invoke(this);
        }

        #region utils
        private string? NameForIndex(int index)
        {
            if (HudAgent == IntPtr.Zero || index < 0 || index > 7)
            {
                return null;
            }

            IntPtr namePtr = (HudAgent + (PartyCrossWorldNameOffset + PartyCrossWorldEntrySize * index));
            return Marshal.PtrToStringAnsi(namePtr);
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

        private bool IsLeader(int index)
        {
            var partyLeadIndex = Plugin.PartyList.PartyLeaderIndex;
            if (partyLeadIndex >= 0 && partyLeadIndex < 8)
            {
                return index == partyLeadIndex;
            }

            if (PartyListAddon == null)
            {
                return false;
            }

            // we use the icon Y coordinate in the party list to know the index (lmao)
            partyLeadIndex = (uint)PartyListAddon->LeaderMarkResNode->ChildNode->Y / 40;
            return index == partyLeadIndex;
        }

        private int? IndexForPartyMember(PartyMember member)
        {
            if (_partyMembersInfo == null || _partyMembersInfo.Count == 0)
            {
                return null;
            }

            var name = member.Name.ToString();
            return _partyMembersInfo.FindIndex(o => o.ObjectId == member.ObjectId || o.Name == name) + 1;
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
                _playerOrderChanged = true;
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
        }

        private void UpdatePreview()
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
                for (int i = 0; i < 8; i++)
                {
                    EnmityLevel enmityLevel = i <= 1 ? (EnmityLevel)i + 1 : EnmityLevel.Last;
                    bool isPartyLeader = i == 0;

                    _groupMembers.Add(new FakePartyFramesMember(i, enmityLevel, isPartyLeader));
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

            public PartyListMemberInfo(PartyListMemberRawInfo* info, string? crossWorldName, uint jobId)
            {
                Name = crossWorldName ?? (Marshal.PtrToStringAnsi(new IntPtr(info->NamePtr)) ?? "");
                ObjectId = info->ObjectId;
                Type = info->Type;
                JobId = jobId;
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

            public string Name => Marshal.PtrToStringAnsi(new IntPtr(NamePtr)) ?? "";
        }
        #endregion
    }
}
