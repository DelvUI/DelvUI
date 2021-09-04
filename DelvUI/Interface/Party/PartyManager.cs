using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.Internal;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using PartyMember = FFXIVClientStructs.FFXIV.Client.Game.Group.PartyMember;

namespace DelvUI.Interface.Party {
    public unsafe class PartyManager {
        #region Singleton
        private static PartyManager _instance = null;
        private DalamudPluginInterface _pluginInterface;
        private PartyHudConfig _config;

        private PartyManager(DalamudPluginInterface pluginInterface, PartyHudConfig config) {
            _pluginInterface = pluginInterface;
            _pluginInterface.Framework.OnUpdateEvent += FrameworkOnOnUpdateEvent;

            _config = config;
            _config.PropertyChanged += OnConfigPropertyChanged;
            _config.SortConfig.PropertyChanged += OnSortConfigPropertyChanged;
        }

        ~PartyManager() {
            _pluginInterface.Framework.OnUpdateEvent -= FrameworkOnOnUpdateEvent;
            _config.PropertyChanged -= OnConfigPropertyChanged;
        }

        public static void Initialize(DalamudPluginInterface pluginInterface, PartyHudConfig config) {
            _instance = new PartyManager(pluginInterface, config);
        }

        public static PartyManager Instance => _instance;
        #endregion Singleton

        private List<IGroupMember> _groupMembers = new List<IGroupMember>();
        public IReadOnlyCollection<IGroupMember> GroupMembers => _groupMembers.AsReadOnly();
        public uint MemberCount => (uint)_groupMembers.Count;

        public event EventHandler<EventArgs> MembersChangedEvent;

        public bool isInParty {
            get {
                if (_config.Preview) {
                    return true;
                }

                var manager = GroupManager.Instance();
                return manager->MemberCount > 0;
            }
        }

        private void FrameworkOnOnUpdateEvent(Framework framework) {
            if (_config.Preview) {
                return;
            }

            var player = _pluginInterface.ClientState.LocalPlayer;
            if (player is null || player is not PlayerCharacter) {
                return;
            }

            var manager = GroupManager.Instance();
            if (_groupMembers.Count == manager->MemberCount) {
                return;
            }

            try {
                _groupMembers.Clear();

                for (int i = 0; i < manager->MemberCount; i++) {
                    PartyMember* partyMember = (PartyMember*)(new IntPtr(manager->PartyMembers) + 0x230 * i);
                    _groupMembers.Add(new GroupMember(partyMember, _pluginInterface));
                }

                PartySortingHelper.SortPartyMembers(ref _groupMembers, _config.SortConfig.Mode);
            }
            catch {
                _groupMembers.Clear();
            }

            if (MembersChangedEvent != null) {
                MembersChangedEvent(this, null);
            }
        }

        private void OnConfigPropertyChanged(object sender, PropertyChangedEventArgs args) {
            if (args.PropertyName == "Preview") {
                UpdatePreview();
            }
        }

        private void UpdatePreview() {
            if (!_config.Preview) {
                return;
            }

            // fill list with fake members for UI testing
            _groupMembers.Clear();

            if (_config.Preview) {
                for (int i = 0; i < 8; i++) {
                    _groupMembers.Add(new FakeGroupMember());
                }

                PartySortingHelper.SortPartyMembers(ref _groupMembers, _config.SortConfig.Mode);
            }

            if (MembersChangedEvent != null) {
                MembersChangedEvent(this, null);
            }
        }

        private void OnSortConfigPropertyChanged(object sender, PropertyChangedEventArgs args) {
            if (args.PropertyName == "Mode") {
                UpdateSortingMode();
            }
        }

        private void UpdateSortingMode() {
            PartySortingHelper.SortPartyMembers(ref _groupMembers, _config.SortConfig.Mode);

            if (MembersChangedEvent != null) {
                MembersChangedEvent(this, null);
            }
        }
    }
}
