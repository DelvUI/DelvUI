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
        private static PartyManager instance = null;
        private DalamudPluginInterface pluginInterface;
        private PluginConfiguration pluginConfiguration;

        private PartyManager(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration)
        {
            this.pluginInterface = pluginInterface;
            this.pluginInterface.Framework.OnUpdateEvent += FrameworkOnOnUpdateEvent;
            
            this.pluginConfiguration = pluginConfiguration;
            this.pluginConfiguration.ConfigChangedEvent += OnConfigChanged;

            UpdateTesting(pluginConfiguration.PartyListTestingEnabled);
        }

        ~PartyManager()
        {
            pluginInterface.Framework.OnUpdateEvent -= FrameworkOnOnUpdateEvent;
            pluginConfiguration.ConfigChangedEvent -= OnConfigChanged;
        }

        public static void Initialize(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration)
        {
            instance = new PartyManager(pluginInterface, pluginConfiguration);
        }

        public static PartyManager Instance
        {
            get
            {
                return instance;
            }
        }
        #endregion Singleton

        private List<IGroupMember> _groupMembers = new List<IGroupMember>();
        public IReadOnlyCollection<IGroupMember> GroupMembers => _groupMembers.AsReadOnly();
        public uint MemberCount => (uint)_groupMembers.Count;
        private int lastCount = 0;

        private bool Testing;

        public bool isInParty
        {
            get
            {
                if (Testing) return true;

                var manager = GroupManager.Instance();
                return manager->MemberCount > 0;
            }
        }

        private void FrameworkOnOnUpdateEvent(Framework framework)
        {
            var player = pluginInterface.ClientState.LocalPlayer;
            if (player is null || player is not PlayerCharacter) return;

            var manager = GroupManager.Instance();
            if (lastCount == manager->MemberCount) return;

            try
            {
                _groupMembers.Clear();
                for (int i = 0; i < manager->MemberCount; i++)
                {
                    PartyMember* partyMember = (PartyMember*)(new IntPtr(manager->PartyMembers) + 0x230 * i);
                    _groupMembers.Add(new GroupMember(partyMember, pluginInterface));
                }
            }
            catch 
            {
                _groupMembers.Clear();
            }
        }

        private void OnConfigChanged(object sender, EventArgs args)
        {
            UpdateTesting(pluginConfiguration.PartyListTestingEnabled);
        }

        private void UpdateTesting(bool testing)
        {
            if (Testing == testing) return;
            Testing = testing;

            _groupMembers.Clear();

            if (Testing)
            {
                for (int i = 0; i < 8; i++)
                {
                    _groupMembers.Add(new FakeGroupMember());
                }
            }
        }
    }
}
