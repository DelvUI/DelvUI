using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using DelvUI.Config;
using DelvUI.Helpers;
using System;
using System.Collections.Generic;

namespace DelvUI.Interface.Party
{
    public class InvulnStatus
    {
        public readonly uint InvulnIcon;
        public readonly float InvulnTime;
        public readonly uint InvulnId;

        public InvulnStatus(uint invulnIcon, float invulnTime, uint invulnId)
        {
            InvulnIcon = invulnIcon;
            InvulnTime = invulnTime;
            InvulnId = invulnId;
        }
    }
    public class PartyFramesInvulnTracker : IDisposable
    {
        private PartyFramesInvulnTrackerConfig _config = null!;

        public PartyFramesInvulnTracker()
        {
            ConfigurationManager.Instance.ResetEvent += OnConfigReset;
            OnConfigReset(ConfigurationManager.Instance);
        }

        ~PartyFramesInvulnTracker()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            ConfigurationManager.Instance.ResetEvent -= OnConfigReset;
        }

        public void OnConfigReset(ConfigurationManager sender)
        {
            _config = ConfigurationManager.Instance.GetConfigObject<PartyFramesTrackersConfig>().Invuln;
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
                    member.InvulnStatus = null;
                    continue;
                }

                if (member.Character is not BattleChara battleChara || member.HP <= 0)
                {
                    member.InvulnStatus = null;
                    continue;
                }

                // check invuln buff
                Status? tankInvuln = Utils.GetTankInvulnerabilityID(battleChara);
                if (tankInvuln == null)
                {
                    member.InvulnStatus = null;
                    continue;
                }

                // apply invuln data based on buff

                member.InvulnStatus = new InvulnStatus(InvulnMap[tankInvuln.StatusId], tankInvuln.RemainingTime, tankInvuln.StatusId);
            }
        }

        #region invuln ids
        //these need to be mapped instead
        private static Dictionary<uint, uint> InvulnMap = new Dictionary<uint, uint>()
        {
            { 810, 003077 },    // LIVING DEAD
            { 3255, 003077},    // UNDEAD REBIRTH
            { 811, 003077 },    // WALKING DEAD
            { 1302, 002502 },   // HALLOWED GROUND
            { 82, 002502 },     // HALLOWED GROUND
            { 409, 000266 },    // HOLMGANG
            { 1836, 003416 }    // SUPERBOLIDE
        };

        #endregion
    }
}
