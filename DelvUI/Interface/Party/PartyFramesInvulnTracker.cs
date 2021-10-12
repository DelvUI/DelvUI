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
                        if (InvulnIds.Contains(status.StatusId))
                        {
                            // apply invuln data based on buff
                            member.InvulnTime = status.RemainingTime;
                            break;
                        }
                        else
                        {
                            // making sure the invuln buff doesnt exists anymore, was having issues with the way raisetracker was handling it
                            member.InvulnTime = null;
                        }
                    }
                    
                }
            }
        }

        #region invuln ids
        //these need to be mapped instead
        private static List<uint> InvulnIds = new List<uint>()
        {
            810,  // LIVING DEAD
            811,  // WALKING DEAD
            1302, // HALLOWED GROUND
            409,  // HOLMGANG
            1836, // SUPERBOLIDE

        };        
        private static List<uint> InvulnIcons = new List<uint>()
        {
            013115, // LIVING DEAD
            013116, // WALKING DEAD
            010255, // HOLMGANG
            013606, // SUPERBOLIDE
            012504, // HALLOWED GROUND

        };
        
        #endregion
    }
}
