using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Interface.GeneralElements;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using System.Numerics;

namespace DelvUI.Interface.Nameplates
{
    internal class NameplatesHud : HudElement
    {
        private NameplatesGeneralConfig Config => (NameplatesGeneralConfig)_config;

        private NameplateWithPlayerBar _playerHud;
        private NameplateWithPlayerBar _partyMemberHud;
        private NameplateWithPlayerBar _allianceMemberHud;
        private NameplateWithPlayerBar _friendsHud;
        private NameplateWithPlayerBar _otherPlayersHud;
        private Nameplate _nonCombatNPCHud;
        private Nameplate _minionNPCHud;
        private Nameplate _objectHud;

        public NameplatesHud(NameplatesGeneralConfig config) : base(config)
        {
            ConfigurationManager manager = ConfigurationManager.Instance;
            _playerHud =         new NameplateWithPlayerBar(manager.GetConfigObject<PlayerNameplateConfig>());
            _partyMemberHud =    new NameplateWithPlayerBar(manager.GetConfigObject<PartyMembersNameplateConfig>());
            _allianceMemberHud = new NameplateWithPlayerBar(manager.GetConfigObject<AllianceMembersNameplateConfig>());
            _friendsHud =        new NameplateWithPlayerBar(manager.GetConfigObject<FriendPlayerNameplateConfig>());
            _otherPlayersHud =   new NameplateWithPlayerBar(manager.GetConfigObject<OtherPlayerNameplateConfig>());
            _nonCombatNPCHud =   new Nameplate(manager.GetConfigObject<NonCombatNPCNameplateConfig>());
            _minionNPCHud =      new Nameplate(manager.GetConfigObject<MinionNPCNameplateConfig>());
            _objectHud =         new Nameplate(manager.GetConfigObject<ObjectsNameplateConfig>());
        }

        protected override void CreateDrawActions(Vector2 origin)
        {
            if (NameplatesManager.Instance == null) { return; }

            foreach (NameplateData data in NameplatesManager.Instance.Data)
            {
                Nameplate? nameplate = GetNameplate(data);
                if (nameplate == null) { continue; }

                if (nameplate is NameplateWithBar nameplateWithBar)
                {
                    AddDrawActions(nameplateWithBar.GetBarDrawActions(data));
                }

                AddDrawActions(nameplate.GetElementsDrawActions(data));
            }
        }

        private unsafe Nameplate? GetNameplate(NameplateData data)
        {
            switch (data.Kind)
            {
                case ObjectKind.Player:
                    if (data.GameObject == Plugin.ClientState.LocalPlayer)
                    {
                        return _playerHud;
                    }

                    if (data.GameObject is Character character)
                    {
                         
                        if ((character.StatusFlags & (StatusFlags)0x20) != 0) // StatusFlags.PartyMember is wrong
                        {
                            return _partyMemberHud;
                        }
                        else if ((character.StatusFlags & (StatusFlags)0x40) != 0) // StatusFlags.AllianceMember is wrong
                        {
                            return _allianceMemberHud;
                        }
                        else if ((character.StatusFlags & (StatusFlags)0x80) != 0) // StatusFlags.Friend is wrong
                        {
                            return _friendsHud;
                        }
                    }

                    return _otherPlayersHud;

                case ObjectKind.EventNpc: return _nonCombatNPCHud;
                case ObjectKind.Companion: return _minionNPCHud;
                default: return _objectHud;
            }
        }
    }
}
