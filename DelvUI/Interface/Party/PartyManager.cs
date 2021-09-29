﻿using Dalamud.Game;
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

        private AddonPartyList* _partyListAddon = null;
        private IntPtr _hudAgent = IntPtr.Zero;

        private PartyManager(PartyFramesConfig config)
        {
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

        private const int PartyListInfoOffset = 0x0B50;
        private const int PartyListMemberRawInfoSize = 0x18;
        private List<PartyListMemberInfo> _partyMembersInfo = null!;
        private bool _playerOrderChanged = false;

        private List<IPartyFramesMember> _groupMembers = new List<IPartyFramesMember>();
        public IReadOnlyCollection<IPartyFramesMember> GroupMembers => _groupMembers.AsReadOnly();
        public uint MemberCount => (uint)_groupMembers.Count;

        public event PartyMembersChangedEventHandler? MembersChangedEvent;


        private void FrameworkOnOnUpdateEvent(Framework framework)
        {
            // find party list hud agent
            if (_hudAgent == IntPtr.Zero || _partyListAddon == null)
            {
                _partyListAddon = (AddonPartyList*)Plugin.GameGui.GetAddonByName("_PartyList", 1);
                _hudAgent = Plugin.GameGui.FindAgentInterface(_partyListAddon);

                PluginLog.Log($"_PartyList Hud Angent found at: 0x{_hudAgent.ToInt64():X16}");
            }

            // no need to update on preview mode
            if (_config.Preview)
            {
                return;
            }

            var player = Plugin.ClientState.LocalPlayer;
            if (player is null || player is not PlayerCharacter)
            {
                return;
            }

            try
            {
                // solo
                var memberCount = Plugin.PartyList.Length;
                if (_config.ShowWhenSolo && memberCount == 0)
                {
                    UpdateSoloParty(player);
                    return;
                }

                // party
                bool partyChanged = _playerOrderChanged || _partyMembersInfo == null || _groupMembers.Count != memberCount;

                if (_hudAgent != IntPtr.Zero)
                {
                    List<PartyListMemberInfo> newInfo = new List<PartyListMemberInfo>(8);

                    for (int i = 0; i < 8; i++)
                    {
                        PartyListMemberRawInfo* info = (PartyListMemberRawInfo*)(_hudAgent + (PartyListInfoOffset + PartyListMemberRawInfoSize * i));
                        newInfo.Add(new PartyListMemberInfo(info));
                    }

                    if (!partyChanged && _partyMembersInfo != null)
                    {
                        partyChanged = !newInfo.SequenceEqual(_partyMembersInfo);
                    }
                    _partyMembersInfo = newInfo;
                }

                // if party is the same, just update actor references
                if (!partyChanged)
                {
                    foreach (var member in _groupMembers)
                    {
                        var index = member.ObjectId == player.ObjectId ? 0 : member.Order - 1;
                        member.Update(EnmityForIndex(index), IsLeader(member));
                    }

                    return;
                }

                PluginLog.Log("Setup party");

                // create new members array with dalamud's data
                _groupMembers.Clear();

                for (int i = 0; i < memberCount; i++)
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
            catch (Exception e)
            {
                PluginLog.LogError("ERROR getting party data: " + e.Message);
            }
        }

        private EnmityLevel EnmityForIndex(int index)
        {
            if (_partyListAddon == null || index < 0 || index > 7)
            {
                return EnmityLevel.Last;
            }

            EnmityLevel enmityLevel = (EnmityLevel)_partyListAddon->PartyMember[index].EmnityByte;
            if (enmityLevel == EnmityLevel.Leader && _partyListAddon->EnmityLeaderIndex != index)
            {
                enmityLevel = EnmityLevel.Last;
            }

            return enmityLevel;
        }

        private bool IsLeader(IPartyFramesMember member)
        {
            var partyList = Plugin.PartyList;

            for (int i = 0; i < partyList.Length; i++)
            {
                var m = partyList[i];
                if (m == null)
                {
                    continue;
                }

                if (m.ObjectId == member.ObjectId || m.Name.ToString() == member.Name)
                {
                    return i == partyList.PartyLeaderIndex;
                }
            }

            return false;
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

        public void OnPlayerOrderChange()
        {
            _playerOrderChanged = true;
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

            EnmityLevel playerEnmity = _partyListAddon->EnmityLeaderIndex == 0 ? EnmityLevel.Leader : EnmityLevel.Last;

            // for some reason chocobos never get a proper enmity value even though they have aggro
            // if the player enmity is set to first, but the "leader index" is invalid
            // we can pretty much deduce that the chocobo is the one with aggro
            // this might fail on some cases when there are other players not in party hitting the same thing
            // but the edge case is so minor we should be fine
            EnmityLevel chocoboEnmity = _partyListAddon->EnmityLeaderIndex == -1 && _partyListAddon->PartyMember[0].EmnityByte == 1 ? EnmityLevel.Leader : EnmityLevel.Last;

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
                    _groupMembers[i].Update(i == 0 ? playerEnmity : chocoboEnmity, i == 0);
                }
            }
        }

        private void OnConfigPropertyChanged(object sender, OnChangeBaseArgs args)
        {
            if (args.PropertyName == "Preview")
            {
                UpdatePreview();
            }
            else if (args.PropertyName == "PlayerOrder")
            {
                OnPlayerOrderChange();
            }
        }

        private void UpdatePreview()
        {
            if (!_config.Preview)
            {
                _groupMembers.Clear();
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

        #region raw party info
        internal unsafe class PartyListMemberInfo : IEquatable<PartyListMemberInfo>
        {
            public readonly string Name;
            public readonly uint ObjectId;
            public readonly byte Type;

            public PartyListMemberInfo(PartyListMemberRawInfo* info)
            {
                Name = Marshal.PtrToStringAnsi(new IntPtr(info->NamePtr)) ?? "";
                ObjectId = info->ObjectId;
                Type = info->Type;
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