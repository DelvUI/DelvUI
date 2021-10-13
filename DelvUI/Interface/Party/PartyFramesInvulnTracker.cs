using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Statuses;
using DelvUI.Helpers;

namespace DelvUI.Interface.Party
{
    public class PartyFramesInvulnTracker
    {
        private PartyFramesInvulnTrackerConfig _config;
        public PartyFramesInvulnTracker()
        {
            _config = ConfigurationManager.Instance.GetConfigObject<PartyFramesInvulnTrackerConfig>();
            ConfigurationManager.Instance.ResetEvent += OnConfigReset;
        }

        public void OnConfigReset(ConfigurationManager sender)
        {
            _config = sender.GetConfigObject<PartyFramesInvulnTrackerConfig>();
        }

        public void Update(List<IPartyFramesMember> partyMembers)
        {
            if (!_config.Enabled)
            {
                return;
            }
            

            foreach (var member in partyMembers)
            {
                if (member.Character == null || member.ObjectId == 0)
                {
                    member.InvulnTime = null;
                    continue;
                }

                if (member.Character is not BattleChara battleChara || member.HP <= 0)
                {
                    continue;
                }

                // check invuln buff
                Status tankInvuln = Utils.HasTankInvulnerability(battleChara);
                if (tankInvuln == null)
                {
                    member.InvulnTime = null;
                    break;
                }
                
                // apply invuln data based on buff
                member.InvulnTime = tankInvuln.RemainingTime;
                member.InvulnIcon = InvulnMap[tankInvuln.StatusId];
                member.InvulnId = tankInvuln.StatusId;

            }
        }

        #region invuln ids
        //these need to be mapped instead
        private static Dictionary<uint, uint> InvulnMap = new Dictionary<uint, uint>()
        {
            { 810, 003077 },    // LIVING DEAD
            { 811, 003077 },    // WALKING DEAD
            { 1302, 002502 },   // HALLOWED GROUND
            { 409, 000266 },    // HOLMGANG
            { 1836, 003416 }    // SUPERBOLIDE
        };
        
        #endregion
    }
}
