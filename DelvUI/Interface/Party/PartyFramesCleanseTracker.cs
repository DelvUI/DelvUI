using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using DelvUI.Helpers;

namespace DelvUI.Interface.Party
{
    public class PartyFramesCleanseTracker : IDisposable
    {
        private PartyFramesCleanseTrackerConfig _config = null!;

        public PartyFramesCleanseTracker()
        {
            ConfigurationManager.Instance.ResetEvent += OnConfigReset;
            OnConfigReset(ConfigurationManager.Instance);
        }

        ~PartyFramesCleanseTracker()
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
            _config = ConfigurationManager.Instance.GetConfigObject<PartyFramesTrackersConfig>().Cleanse;
        }

        public void Update(List<IPartyFramesMember> partyMembers)
        {
            if (!_config.Enabled)
            {
                return;
            }

            foreach (var member in partyMembers)
            {
                member.HasDispellableDebuff = false;

                if (member.Character is not IBattleChara battleChara)
                {
                    continue;
                }

                // check for disspellable debuff

                var statusList = Utils.StatusListForBattleChara(battleChara); 
                foreach (var status in statusList)
                {
                    if (status == null || !status.GameData.CanDispel)
                    {
                        continue;
                    }

                    // apply raise data based on buff
                    member.HasDispellableDebuff = true;
                    break;
                }
            }
        }
    }
}
