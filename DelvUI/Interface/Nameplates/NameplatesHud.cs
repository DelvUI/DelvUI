using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
using DelvUI.Config;
using DelvUI.Interface.GeneralElements;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using System.Numerics;

namespace DelvUI.Interface.Nameplates
{
    internal class NameplatesHud : HudElement
    {
        private NameplatesGeneralConfig Config => (NameplatesGeneralConfig)_config;

        private NameplateWithPlayerBar _playerHud;
        private NameplateWithEnemyBar _enemyHud;
        private NameplateWithPlayerBar _partyMemberHud;
        private NameplateWithPlayerBar _allianceMemberHud;
        private NameplateWithPlayerBar _friendsHud;
        private NameplateWithPlayerBar _otherPlayersHud;
        private NameplateWithBar _petHud;
        private Nameplate _nonCombatNPCHud;
        private Nameplate _minionNPCHud;
        private Nameplate _objectHud;

        public NameplatesHud(NameplatesGeneralConfig config) : base(config)
        {
            ConfigurationManager manager = ConfigurationManager.Instance;
            _playerHud = new NameplateWithPlayerBar(manager.GetConfigObject<PlayerNameplateConfig>());
            _enemyHud = new NameplateWithEnemyBar(manager.GetConfigObject<EnemyNameplateConfig>()); 
            _partyMemberHud = new NameplateWithPlayerBar(manager.GetConfigObject<PartyMembersNameplateConfig>());
            _allianceMemberHud = new NameplateWithPlayerBar(manager.GetConfigObject<AllianceMembersNameplateConfig>());
            _friendsHud = new NameplateWithPlayerBar(manager.GetConfigObject<FriendPlayerNameplateConfig>());
            _otherPlayersHud = new NameplateWithPlayerBar(manager.GetConfigObject<OtherPlayerNameplateConfig>());
            _petHud = new NameplateWithBar(manager.GetConfigObject<PetNameplateConfig>());
            _nonCombatNPCHud = new Nameplate(manager.GetConfigObject<NonCombatNPCNameplateConfig>());
            _minionNPCHud = new Nameplate(manager.GetConfigObject<MinionNPCNameplateConfig>());
            _objectHud = new Nameplate(manager.GetConfigObject<ObjectsNameplateConfig>());
        }

        protected override void CreateDrawActions(Vector2 origin)
        {
            if (NameplatesManager.Instance == null) { return; }

            foreach (NameplateData data in NameplatesManager.Instance.Data)
            {
                Nameplate? nameplate = GetNameplate(data);
                if (nameplate == null) { continue; }

                if (IsPointObstructed(data)) { continue; }

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

                case ObjectKind.BattleNpc:
                    if (data.GameObject is BattleNpc battleNpc)
                    {
                        if ((BattleNpcSubKind)battleNpc.SubKind == BattleNpcSubKind.Pet ||
                            (BattleNpcSubKind)battleNpc.SubKind == BattleNpcSubKind.Chocobo)
                        {
                            return _petHud;
                        }
                        else if ((BattleNpcSubKind)battleNpc.SubKind == BattleNpcSubKind.Enemy)
                        {
                            return _enemyHud;
                        }
                    }
                    break;

                case ObjectKind.EventNpc: return _nonCombatNPCHud;
                case ObjectKind.Companion: return _minionNPCHud;
                default: return _objectHud;
            }

            return null;
        }

        private unsafe bool IsPointObstructed(NameplateData data)
        {
            if (data.GameObject == null) { return true; }
            if (Config.OcclusionMode == NameplatesOcclusionMode.None) { return false; }

            Camera camera = Control.Instance()->CameraManager.Camera->CameraBase.SceneCamera;
            Vector3 cameraPos = camera.Object.Position;

            BGCollisionModule* collisionModule = Framework.Instance()->BGCollisionModule;
            int* flags = stackalloc int[] { 0x2000 };
            bool obstructed = false;

            // simple mode
            if (Config.OcclusionMode == NameplatesOcclusionMode.Simple)
            {
                Vector3 direction = Vector3.Normalize(data.WorldPosition - cameraPos);
                RaycastHit hit;
                obstructed = collisionModule->RaycastEx(&hit, cameraPos, direction, data.Distance, 1, flags);
            }
            // full mode
            else
            {
                int obstructionCount = 0;
                RaycastHit hit;

                Vector2[] points = new Vector2[]
                {
                    data.ScreenPosition + new Vector2(-30, 0), // left
                    data.ScreenPosition + new Vector2(30, 0), // right
                };

                foreach (Vector2 point in points) {
                    Ray ray = camera.ScreenPointToRay(point);
                    collisionModule->RaycastEx(&hit, ray.Origin, ray.Direction, data.Distance, 1, flags);

                    if (hit.Distance <= data.Distance * 0.95f)
                    {
                        obstructionCount++;
                    }
                }

                obstructed = obstructionCount == points.Length;
            }

            return obstructed;
        }
    }
}
