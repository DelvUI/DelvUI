using System.Collections.Generic;

namespace DelvUI.Helpers
{
    public enum JobRoles
    {
        Tank = 0,
        DPS = 1,
        Healer = 2,
        Crafter = 3,
        Gatherer = 4,
        Unknown
    }

    public static class JobsHelper
    {
        public static JobRoles RoleForJob(uint jobId)
        {
            if (JobRolesMap.TryGetValue(jobId, out var role))
            {
                return role;
            }

            return JobRoles.Unknown;
        }

        public static bool IsJobARole(uint jobId, JobRoles role)
        {
            if (JobRolesMap.TryGetValue(jobId, out var r))
            {
                return r == role;
            }

            return false;
        }

        public static bool IsJobTank(uint jobId)
        {
            return IsJobARole(jobId, JobRoles.Tank);
        }

        public static bool IsJobHealer(uint jobId)
        {
            return IsJobARole(jobId, JobRoles.Healer);
        }

        public static bool IsJobDPS(uint jobId)
        {
            return IsJobARole(jobId, JobRoles.DPS);
        }

        public static bool IsJobCrafter(uint jobId)
        {
            return IsJobARole(jobId, JobRoles.Crafter);
        }

        public static bool IsJobGatherer(uint jobId)
        {
            return IsJobARole(jobId, JobRoles.Gatherer);
        }

        public static Dictionary<uint, JobRoles> JobRolesMap = new Dictionary<uint, JobRoles>()
        {
            // tanks
            [Jobs.GLD] = JobRoles.Tank,
            [Jobs.MRD] = JobRoles.Tank,
            [Jobs.PLD] = JobRoles.Tank,
            [Jobs.WAR] = JobRoles.Tank,
            [Jobs.DRK] = JobRoles.Tank,
            [Jobs.GNB] = JobRoles.Tank,

            // melee dps
            [Jobs.PGL] = JobRoles.DPS,
            [Jobs.LNC] = JobRoles.DPS,
            [Jobs.ROG] = JobRoles.DPS,
            [Jobs.MNK] = JobRoles.DPS,
            [Jobs.DRG] = JobRoles.DPS,
            [Jobs.NIN] = JobRoles.DPS,
            [Jobs.SAM] = JobRoles.DPS,

            // ranged phys dps
            [Jobs.ARC] = JobRoles.DPS,
            [Jobs.BRD] = JobRoles.DPS,
            [Jobs.MCH] = JobRoles.DPS,
            [Jobs.DNC] = JobRoles.DPS,

            // ranged magic dps
            [Jobs.THM] = JobRoles.DPS,
            [Jobs.ACN] = JobRoles.DPS,
            [Jobs.BLM] = JobRoles.DPS,
            [Jobs.SMN] = JobRoles.DPS,
            [Jobs.RDM] = JobRoles.DPS,
            [Jobs.BLU] = JobRoles.DPS,

            // healers
            [Jobs.CNJ] = JobRoles.Healer,
            [Jobs.WHM] = JobRoles.Healer,
            [Jobs.SCH] = JobRoles.Healer,
            [Jobs.AST] = JobRoles.Healer,

            // crafters
            [Jobs.CRP] = JobRoles.Crafter,
            [Jobs.BSM] = JobRoles.Crafter,
            [Jobs.ARM] = JobRoles.Crafter,
            [Jobs.GSM] = JobRoles.Crafter,
            [Jobs.LTW] = JobRoles.Crafter,
            [Jobs.WVR] = JobRoles.Crafter,
            [Jobs.ALC] = JobRoles.Crafter,
            [Jobs.CUL] = JobRoles.Crafter,

            // gatherers
            [Jobs.MIN] = JobRoles.Gatherer,
            [Jobs.BOT] = JobRoles.Gatherer,
            [Jobs.FSH] = JobRoles.Gatherer,
        };
    }

    public static class Jobs
    {
        public const uint GLD = 1;
        public const uint MRD = 3;
        public const uint PLD = 19;
        public const uint WAR = 21;
        public const uint DRK = 32;
        public const uint GNB = 37;

        public const uint CNJ = 6;
        public const uint WHM = 24;
        public const uint SCH = 28;
        public const uint AST = 33;

        public const uint PGL = 2;
        public const uint LNC = 4;
        public const uint ROG = 29;
        public const uint MNK = 20;
        public const uint DRG = 22;
        public const uint NIN = 30;
        public const uint SAM = 34;

        public const uint ARC = 5;
        public const uint BRD = 23;
        public const uint MCH = 31;
        public const uint DNC = 38;

        public const uint THM = 7;
        public const uint ACN = 26;
        public const uint BLM = 25;
        public const uint SMN = 27;
        public const uint RDM = 35;
        public const uint BLU = 36;

        public const uint CRP = 8;
        public const uint BSM = 9;
        public const uint ARM = 10;
        public const uint GSM = 11;
        public const uint LTW = 12;
        public const uint WVR = 13;
        public const uint ALC = 14;
        public const uint CUL = 15;

        public const uint MIN = 16;
        public const uint BOT = 17;
        public const uint FSH = 18;
    }
}
