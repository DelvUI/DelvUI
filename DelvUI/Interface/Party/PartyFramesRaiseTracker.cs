using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DelvUI.Interface.Party
{
    public class PartyFramesRaiseTracker : IDisposable
    {
        private PartyFramesRaiseTrackerConfig _config = null!;

        public PartyFramesRaiseTracker()
        {
            ConfigurationManager.Instance.ResetEvent += OnConfigReset;
            OnConfigReset(ConfigurationManager.Instance);
        }

        ~PartyFramesRaiseTracker()
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
            _config = sender.GetConfigObject<PartyFramesTrackersConfig>().Raise;
        }

        public void Update(List<IPartyFramesMember> partyMembers)
        {
            if (!_config.Enabled)
            {
                return;
            }

            Dictionary<uint, IPartyFramesMember> deadAndNotRaised = new Dictionary<uint, IPartyFramesMember>();
            float? limitBreakTime = null;
            Dictionary<uint, float> raiseTimeMap = new Dictionary<uint, float>();

            foreach (var member in partyMembers)
            {
                if (member.Character == null || member.ObjectId == 0)
                {
                    member.RaiseTime = null;
                    continue;
                }

                if (member.HP > 0)
                {
                    member.RaiseTime = null;
                }

                if (member.Character is not BattleChara battleChara)
                {
                    continue;
                }

                // check raise casts
                if (battleChara.IsCasting)
                {
                    var remaining = Math.Max(0, battleChara.TotalCastTime - battleChara.CurrentCastTime);

                    // check limit break
                    if (IsRaiseLimitBreakAction(battleChara.CastActionId) &&
                        (limitBreakTime.HasValue && limitBreakTime.Value > remaining))
                    {
                        limitBreakTime = remaining;
                    }
                    // check regular raise
                    else if (IsRaiseAction(battleChara.CastActionId))
                    {
                        if (raiseTimeMap.TryGetValue(battleChara.CastTargetObjectId, out float raiseTime))
                        {
                            if (raiseTime > remaining)
                            {
                                raiseTimeMap[battleChara.CastTargetObjectId] = remaining;
                            }
                        }
                        else
                        {
                            raiseTimeMap.Add(battleChara.CastTargetObjectId, remaining);
                        }
                    }
                }

                // check raise buff
                if (member.HP <= 0)
                {
                    bool hasBuff = false;

                    foreach (var status in battleChara.StatusList)
                    {
                        if (status == null || (status.StatusId != 148 && status.StatusId != 1140))
                        {
                            continue;
                        }

                        // apply raise data based on buff
                        member.RaiseTime = status.RemainingTime;
                        hasBuff = true;
                        break;
                    }

                    if (!hasBuff)
                    {
                        deadAndNotRaised.Add(member.ObjectId, member);
                    }
                }
            }

            // apply raise data based on casts
            foreach (var memberId in deadAndNotRaised.Keys)
            {
                var member = deadAndNotRaised[memberId];

                if (raiseTimeMap.TryGetValue(memberId, out float raiseTime))
                {
                    if (limitBreakTime.HasValue && limitBreakTime.Value < raiseTime)
                    {
                        member.RaiseTime = limitBreakTime;
                    }
                    else
                    {
                        member.RaiseTime = raiseTime;
                    }
                }
                else
                {
                    member.RaiseTime = limitBreakTime; // its fine if this is null here
                }
            }
        }

        #region raise ids
        private static bool IsRaiseLimitBreakAction(uint actionId)
        {
            return LimitBreakIds.Contains(actionId);
        }

        private static bool IsRaiseAction(uint actionId)
        {
            return RaiseIds.Contains(actionId);
        }


        private static List<uint> RaiseIds = new List<uint>()
        {
            173, // ACN, SMN, SCH
            125, // CNH, WHM
            3603, // AST
            18317, // BLU
            22345, // Lost Sacrifice, Bozja
            20730, // Lost Arise, Bozja
            12996, // Raise L, Eureka
            24287 // SGE
        };

        private static List<uint> LimitBreakIds = new List<uint>()
        {
            208, // WHM
            4247, // SCH
            4248, // AST
            24859 // SGE
        };
        #endregion
    }
}
