﻿using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Helpers;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using System;
using System.Collections.Generic;
using PartyMember = FFXIVClientStructs.FFXIV.Client.Game.Group.PartyMember;

namespace DelvUI.Interface.Party
{
    public unsafe class PartyManager : IDisposable
    {
        #region Singleton
        private static PartyManager _instance = null;
        private PartyFramesConfig _config;

        private PartyManager(PartyFramesConfig config)
        {
            Plugin.Framework.Update += FrameworkOnOnUpdateEvent;

            _config = config;
            _config.onValueChanged += OnConfigPropertyChanged;

            UpdatePreview();
        }

        public static void Initialize()
        {
            var config = ConfigurationManager.GetInstance().GetConfigObject<PartyFramesConfig>();
            _instance = new PartyManager(config);
        }

        public void Dispose()
        {
            Plugin.Framework.Update -= FrameworkOnOnUpdateEvent;
            _config.onValueChanged -= OnConfigPropertyChanged;
        }

        public static PartyManager Instance => _instance;
        #endregion Singleton

        private List<IPartyFramesMember> _groupMembers = new List<IPartyFramesMember>();
        public IReadOnlyCollection<IPartyFramesMember> GroupMembers => _groupMembers.AsReadOnly();
        public uint MemberCount => (uint)_groupMembers.Count;

        public event EventHandler<EventArgs>? MembersChangedEvent;

        public bool isInParty
        {
            get
            {
                if (_config.Preview)
                {
                    return true;
                }

                var manager = GroupManager.Instance();
                return manager->MemberCount > 0;
            }
        }

        private void FrameworkOnOnUpdateEvent(Framework framework)
        {
            if (_config.Preview)
            {
                return;
            }

            var player = Plugin.ClientState.LocalPlayer;
            if (player is null || player is not PlayerCharacter)
            {
                return;
            }

            // solo
            var memberCount = Plugin.PartyList.Length;
            if (_config.ShowWhenSolo && memberCount == 0)
            {
                UpdateSoloParty(player);
                return;
            }

            // party
            try
            {
                bool partyChanged = _groupMembers.Count != memberCount;
                List<IPartyFramesMember> newMembers = new List<IPartyFramesMember>();

                for (int i = 0; i < memberCount; i++)
                {
                    var partyMember = Plugin.PartyList[i];
                    if (partyMember == null)
                    {
                        continue;
                    }

                    if (i < _groupMembers.Count && partyMember.ObjectId != _groupMembers[i].ObjectId)
                    {
                        partyChanged = true;
                    }

                    var member = new PartyFramesMember(partyMember);
                    newMembers.Add(member);

                    if (_config.ShowCompanions)
                    {
                        var companion = Utils.GetBattleCompanion(member.Character);
                        if (companion is Character companionCharacter)
                        {
                            newMembers.Add(new PartyFramesMember(companionCharacter));
                        }
                    }
                }

                PartySortingHelper.SortPartyMembers(ref newMembers, _config.SortingMode);

                if (partyChanged)
                {
                    _groupMembers = newMembers;

                    MembersChangedEvent?.Invoke(this, new EventArgs());
                }
            }
            catch
            {

            }
        }

        private void UpdateSoloParty(PlayerCharacter player)
        {
            List<IPartyFramesMember> newMembers = new List<IPartyFramesMember>();

            newMembers.Add(new PartyFramesMember(player));

            if (_config.ShowCompanions)
            {
                var companion = Utils.GetBattleCompanion(player);
                if (companion is Character companionCharacter)
                {
                    newMembers.Add(new PartyFramesMember(companionCharacter));
                }
            }

            if (newMembers.Count != _groupMembers.Count)
            {
                _groupMembers = newMembers;
                MembersChangedEvent?.Invoke(this, new EventArgs());
            }
        }

        private void OnConfigPropertyChanged(object sender, OnChangeBaseArgs args)
        {
            if (args.PropertyName == "Preview")
            {
                UpdatePreview();
            }
            else if (args.PropertyName == "SortingMode")
            {
                UpdateSortingMode();
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
                    _groupMembers.Add(new FakePartyFramesMember());
                }

                PartySortingHelper.SortPartyMembers(ref _groupMembers, _config.SortingMode);
            }

            MembersChangedEvent?.Invoke(this, new EventArgs());
        }

        private void UpdateSortingMode()
        {
            PartySortingHelper.SortPartyMembers(ref _groupMembers, _config.SortingMode);

            MembersChangedEvent?.Invoke(this, new EventArgs());
        }
    }
}
