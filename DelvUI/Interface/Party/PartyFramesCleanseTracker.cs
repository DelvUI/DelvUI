using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using DelvUI.Helpers;

namespace DelvUI.Interface.Party
{
    public class PartyFramesCleanseTracker
    {
        private PartyFramesCleanseTrackerConfig _config;
        public PartyFramesCleanseTracker()
        {
            _config = ConfigurationManager.Instance.GetConfigObject<PartyFramesCleanseTrackerConfig>();
            ConfigurationManager.Instance.ResetEvent += OnConfigReset;
        }

        public void OnConfigReset(ConfigurationManager sender)
        {
            _config = sender.GetConfigObject<PartyFramesCleanseTrackerConfig>();
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

                if (member.Character is not BattleChara battleChara)
                {
                    continue;
                }

                // check for disspellable debuff

                foreach (var status in battleChara.StatusList)
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
