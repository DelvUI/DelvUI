using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Dalamud.Plugin;
using Dalamud.Game.Internal;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using PartyMember = FFXIVClientStructs.FFXIV.Client.Game.Group.PartyMember;
using Dalamud.Game.ClientState.Actors.Types;


namespace DelvUI.Interface.Party
{
    public unsafe class PartyManager
    {
        #region Singleton
        private static PartyManager _instance = null;
        private DalamudPluginInterface _pluginInterface;
        private PluginConfiguration _pluginConfiguration;
        private PartyHudConfig _config;

        private PartyManager(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration, PartyHudConfig config)
        {
            _pluginInterface = pluginInterface;
            _pluginInterface.Framework.OnUpdateEvent += FrameworkOnOnUpdateEvent;
            
            _pluginConfiguration = pluginConfiguration;
            _pluginConfiguration.ConfigChangedEvent += OnConfigChanged;

            _config = config;

            _lastSortingMode = config.SortConfig.Mode;
            UpdatePreview(true);
        }

        ~PartyManager()
        {
            _pluginInterface.Framework.OnUpdateEvent -= FrameworkOnOnUpdateEvent;
            _pluginConfiguration.ConfigChangedEvent -= OnConfigChanged;
        }

        public static void Initialize(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration, PartyHudConfig config)
        {
            _instance = new PartyManager(pluginInterface, pluginConfiguration, config);
        }

        public static PartyManager Instance => _instance;
        #endregion Singleton

        private List<IGroupMember> _groupMembers = new List<IGroupMember>();
        public IReadOnlyCollection<IGroupMember> GroupMembers => _groupMembers.AsReadOnly();
        public uint MemberCount => (uint)_groupMembers.Count;

        public event EventHandler<EventArgs> MembersChangedEvent;

        private bool _lastPreview;
        private PartySortingMode _lastSortingMode;
        
        public bool isInParty
        {
            get
            {
                if (_config.Preview) return true;

                var manager = GroupManager.Instance();
                return manager->MemberCount > 0;
            }
        }

        private void FrameworkOnOnUpdateEvent(Framework framework)
        {
            if (_config.Preview) return;

            var player = _pluginInterface.ClientState.LocalPlayer;
            if (player is null || player is not PlayerCharacter) return;

            var manager = GroupManager.Instance();
            if (_groupMembers.Count == manager->MemberCount) return;

            try
            {
                _groupMembers.Clear();

                for (int i = 0; i < manager->MemberCount; i++)
                {
                    PartyMember* partyMember = (PartyMember*)(new IntPtr(manager->PartyMembers) + 0x230 * i);
                    _groupMembers.Add(new GroupMember(partyMember, _pluginInterface));
                }

                PartySortingHelper.SortPartyMembers(ref _groupMembers, _config.SortConfig.Mode);
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

        private void OnConfigChanged(object sender, EventArgs args)
        {
            UpdatePreview(false);
            UpdateSortingMode();
        }

        private void UpdatePreview(bool forced)
        {
            if (!forced && _lastPreview == _config.Preview) return;
            _lastPreview = _config.Preview;

            _groupMembers.Clear();

            if (_config.Preview)
            {
                for (int i = 0; i < 8; i++)
                {
                    _groupMembers.Add(new FakeGroupMember());
                }

                PartySortingHelper.SortPartyMembers(ref _groupMembers, _config.SortConfig.Mode);
            }

            if (MembersChangedEvent != null)
            {
                MembersChangedEvent(this, null);
            }
        }
        private void UpdateSortingMode()
        {
            if (_lastSortingMode == _config.SortConfig.Mode) return;
            _lastSortingMode = _config.SortConfig.Mode;

            PartySortingHelper.SortPartyMembers(ref _groupMembers, _config.SortConfig.Mode);

            if (MembersChangedEvent != null)
            {
                MembersChangedEvent(this, null);
            }
        }
    }
}
