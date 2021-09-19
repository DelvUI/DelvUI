using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.Internal;
using DelvUI.Config;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

            // testing
            if (manager->MemberCount == 0 && !_config.Preview)
            {
                if (_groupMembers.Count == 1)
                {
                    return;
                }

                _groupMembers.Clear();
                _groupMembers.Add(new PartyFramesMember(player));

                if (MembersChangedEvent != null)
                {
                    MembersChangedEvent(this, null);
                }

                return;
            }
            // testing

            if (_groupMembers.Count == manager->MemberCount)
            {
                return;
            }

            try
            {
                _groupMembers.Clear();

                for (int i = 0; i < manager->MemberCount; i++)
                {
                    PartyMember* partyMember = (PartyMember*)(new IntPtr(manager->PartyMembers) + 0x230 * i);
                    _groupMembers.Add(new PartyFramesMember(partyMember));
                }

                PartySortingHelper.SortPartyMembers(ref _groupMembers, _config.SortingMode);
            }
            catch
            {
                _groupMembers.Clear();
            }

            if (MembersChangedEvent != null)
            {
                MembersChangedEvent(this, null);
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
