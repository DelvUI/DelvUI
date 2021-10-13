using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using System;
using System.Collections.Generic;
using System.Linq;

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

                if (member.Character is not BattleChara battleChara)
                {
                    continue;
                }

                // check invuln buff
                if (member.HP > 0)
                {
                    foreach (var status in battleChara.StatusList)
                    {
                        if (InvulnMap.Keys.Contains(status.StatusId))
                        {
                            // apply invuln data based on buff
                            member.InvulnTime = status.RemainingTime;
                            member.InvulnIcon = InvulnMap[status.StatusId];
                            break;
                        }

                        // making sure the invuln buff doesnt exists anymore, was having issues with the way raisetracker was handling it
                        member.InvulnTime = null;
                    }
                    
                }
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
