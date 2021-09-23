using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.Internal;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Helpers;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using PartyMember = FFXIVClientStructs.FFXIV.Client.Game.Group.PartyMember;

namespace DelvUI.Interface.Party
{
    public unsafe class PartyManager
    {
        #region Singleton
        private static PartyManager _instance = null;
        private PartyFramesConfig _config;

        private PartyManager(PartyFramesConfig config)
        {
            Plugin.Framework.OnUpdateEvent += FrameworkOnOnUpdateEvent;

            _config = config;
            _config.onValueChanged += OnConfigPropertyChanged;

            UpdatePreview();
        }

        ~PartyManager()
        {
            Plugin.Framework.OnUpdateEvent -= FrameworkOnOnUpdateEvent;
            _config.onValueChanged -= OnConfigPropertyChanged;
        }

        public static void Initialize()
        {
            var config = ConfigurationManager.GetInstance().GetConfigObject<PartyFramesConfig>();
            _instance = new PartyManager(config);
        }

        public static void Destroy()
        {
            _instance = null;
        }

        public static PartyManager Instance => _instance;
        #endregion Singleton

        private List<IPartyFramesMember> _groupMembers = new List<IPartyFramesMember>();
        public IReadOnlyCollection<IPartyFramesMember> GroupMembers => _groupMembers.AsReadOnly();
        public uint MemberCount => (uint)_groupMembers.Count;

        public event EventHandler<EventArgs> MembersChangedEvent;

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

            var manager = GroupManager.Instance();

            // solo
            if (_config.ShowWhenSolo && manager->MemberCount == 0 && !_config.Preview)
            {
                UpdateSoloParty(player);
                return;
            }

            // party
            try
            {
                bool partyChanged = _groupMembers.Count != manager->MemberCount;
                List<IPartyFramesMember> newMembers = new List<IPartyFramesMember>();

                for (int i = 0; i < manager->MemberCount; i++)
                {
                    PartyMember* partyMember = (PartyMember*)(new IntPtr(manager->PartyMembers) + 0x230 * i);

                    if (i < _groupMembers.Count && partyMember->ObjectID != _groupMembers[i].ActorID)
                    {
                        partyChanged = true;
                    }

                    var member = new PartyFramesMember(partyMember);
                    newMembers.Add(member);

                    if (_config.ShowCompanions)
                    {
                        var companion = Utils.GetBattleCompanion(member.GetActor());
                        if (companion != null)
                        {
                            _groupMembers.Add(new PartyFramesMember(companion));
                        }
                    }
                }

                PartySortingHelper.SortPartyMembers(ref newMembers, _config.SortingMode);

                if (partyChanged)
                {
                    _groupMembers = newMembers;

                    if (MembersChangedEvent != null)
                    {
                        MembersChangedEvent(this, null);
                    }
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
                if (companion != null)
                {
                    newMembers.Add(new PartyFramesMember(companion));
                }
            }

            if (newMembers.Count != _groupMembers.Count)
            {
                _groupMembers = newMembers;
                if (MembersChangedEvent != null)
                {
                    MembersChangedEvent(this, null);
                }
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

            if (MembersChangedEvent != null)
            {
                MembersChangedEvent(this, null);
            }
        }

        private void UpdateSortingMode()
        {
            PartySortingHelper.SortPartyMembers(ref _groupMembers, _config.SortingMode);

            if (MembersChangedEvent != null)
            {
                MembersChangedEvent(this, null);
            }
        }
    }
}
