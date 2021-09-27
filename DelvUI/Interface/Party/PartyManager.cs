using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Helpers;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.UI;
using System;
using System.Collections.Generic;
using PartyMember = FFXIVClientStructs.FFXIV.Client.Game.Group.PartyMember;

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

        private List<IPartyFramesMember> _groupMembers = new List<IPartyFramesMember>();
        public IReadOnlyCollection<IPartyFramesMember> GroupMembers => _groupMembers.AsReadOnly();
        public uint MemberCount => (uint)_groupMembers.Count;

        public event PartyMembersChangedEventHandler? MembersChangedEvent;

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

                    if (_config.ShowChocobo)
                    {
                        var companion = Utils.GetBattleChocobo(member.Character);
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

                    MembersChangedEvent?.Invoke(this);
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

            if (_config.ShowChocobo)
            {
                var companion = Utils.GetBattleChocobo(player);
                if (companion is Character companionCharacter)
                {
                    newMembers.Add(new PartyFramesMember(companionCharacter));
                }
            }

            if (newMembers.Count != _groupMembers.Count)
            {
                _groupMembers = newMembers;
                MembersChangedEvent?.Invoke(this);
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

            MembersChangedEvent?.Invoke(this);
        }

        private void UpdateSortingMode()
        {
            PartySortingHelper.SortPartyMembers(ref _groupMembers, _config.SortingMode);

            MembersChangedEvent?.Invoke(this);
        }
    }
}
