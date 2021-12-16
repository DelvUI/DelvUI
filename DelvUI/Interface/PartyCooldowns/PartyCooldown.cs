using DelvUI.Helpers;

namespace DelvUI.Interface.PartyCooldowns
{
    public class PartyCooldown
    {
        public readonly uint ActionID;
        public readonly uint JobID;
        public readonly uint RequiredLevel;

        public int CooldownDuration;
        public int EffectDuration;

        public int Priority;
        public int Column;

        public PartyCooldown(uint actionID, uint jobID, uint requiredLevel, int cooldownDuration, int effectDuration, int priority, int column)
        {
            ActionID = actionID;
            JobID = jobID;
            RequiredLevel = requiredLevel;
            CooldownDuration = cooldownDuration;
            EffectDuration = effectDuration;
            Priority = priority;
            Column = column;
        }

        public virtual bool IsUsableBy(uint jobId)
        {
            return JobID == jobId;
        }
    }

    public class RolePartyCooldown : PartyCooldown
    {
        public readonly JobRoles Role;

        public RolePartyCooldown(uint actionID, JobRoles role, uint requiredLevel, int cooldownDuration, int effectDuration, int priority, int column)
            : base(actionID, 0, requiredLevel, cooldownDuration, effectDuration, priority, column)
        {
            Role = role;
        }


        public override bool IsUsableBy(uint jobId)
        {
            return JobsHelper.RoleForJob(jobId) == Role;
        }
    }
}
