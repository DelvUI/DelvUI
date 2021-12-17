using DelvUI.Helpers;
using DelvUI.Interface.Party;
using ImGuiNET;

namespace DelvUI.Interface.PartyCooldowns
{
    public class PartyCooldown
    {
        public readonly PartyCooldownData Data;
        public readonly uint SourceId;
        public readonly PartyFramesMember? Member;

        public double LastTimeUsed = 0;

        public PartyCooldown(PartyCooldownData data, uint sourceID, PartyFramesMember? member)
        {
            Data = data;
            SourceId = sourceID;
            Member = member;
        }

        public float EffectTimeRemaining()
        {
            double timeSinceUse = ImGui.GetTime() - LastTimeUsed;
            if (timeSinceUse > Data.EffectDuration)
            {
                return 0;
            }

            return Data.EffectDuration - (float)timeSinceUse;
        }

        public float CooldownTimeRemaining()
        {
            double timeSinceUse = ImGui.GetTime() - LastTimeUsed;
            if (timeSinceUse > Data.CooldownDuration)
            {
                return 0;
            }

            return Data.CooldownDuration - (float)timeSinceUse;
        }
    }

    public class PartyCooldownData
    {
        public uint ActionId = 0;
        public uint JobId = 0;
        public JobRoles Role = JobRoles.Unknown;
        public uint RequiredLevel = 0;

        public bool Enabled = true;

        public int CooldownDuration = 0;
        public int EffectDuration = 0;

        public int Priority = 0;
        public int Column = 1;

        public uint IconId = 0;

        public virtual bool IsUsableBy(uint jobId)
        {
            return Role != JobRoles.Unknown ? JobsHelper.RoleForJob(jobId) == Role : JobId == jobId;
        }
    }
}
