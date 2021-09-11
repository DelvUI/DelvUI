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

    public enum PrimaryResourceTypes
    {
        MP = 0,
        CP = 1,
        GP = 2,
        None = 3
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
            [JobIDs.GLD] = JobRoles.Tank,
            [JobIDs.MRD] = JobRoles.Tank,
            [JobIDs.PLD] = JobRoles.Tank,
            [JobIDs.WAR] = JobRoles.Tank,
            [JobIDs.DRK] = JobRoles.Tank,
            [JobIDs.GNB] = JobRoles.Tank,

            // melee dps
            [JobIDs.PGL] = JobRoles.DPS,
            [JobIDs.LNC] = JobRoles.DPS,
            [JobIDs.ROG] = JobRoles.DPS,
            [JobIDs.MNK] = JobRoles.DPS,
            [JobIDs.DRG] = JobRoles.DPS,
            [JobIDs.NIN] = JobRoles.DPS,
            [JobIDs.SAM] = JobRoles.DPS,

            // ranged phys dps
            [JobIDs.ARC] = JobRoles.DPS,
            [JobIDs.BRD] = JobRoles.DPS,
            [JobIDs.MCH] = JobRoles.DPS,
            [JobIDs.DNC] = JobRoles.DPS,

            // ranged magic dps
            [JobIDs.THM] = JobRoles.DPS,
            [JobIDs.ACN] = JobRoles.DPS,
            [JobIDs.BLM] = JobRoles.DPS,
            [JobIDs.SMN] = JobRoles.DPS,
            [JobIDs.RDM] = JobRoles.DPS,
            [JobIDs.BLU] = JobRoles.DPS,

            // healers
            [JobIDs.CNJ] = JobRoles.Healer,
            [JobIDs.WHM] = JobRoles.Healer,
            [JobIDs.SCH] = JobRoles.Healer,
            [JobIDs.AST] = JobRoles.Healer,

            // crafters
            [JobIDs.CRP] = JobRoles.Crafter,
            [JobIDs.BSM] = JobRoles.Crafter,
            [JobIDs.ARM] = JobRoles.Crafter,
            [JobIDs.GSM] = JobRoles.Crafter,
            [JobIDs.LTW] = JobRoles.Crafter,
            [JobIDs.WVR] = JobRoles.Crafter,
            [JobIDs.ALC] = JobRoles.Crafter,
            [JobIDs.CUL] = JobRoles.Crafter,

            // gatherers
            [JobIDs.MIN] = JobRoles.Gatherer,
            [JobIDs.BOT] = JobRoles.Gatherer,
            [JobIDs.FSH] = JobRoles.Gatherer,
        };

        public static Dictionary<uint, string> JobNames = new Dictionary<uint, string>()
        {
            // tanks
            [JobIDs.GLD] = "GLD",
            [JobIDs.MRD] = "MRD",
            [JobIDs.PLD] = "PLD",
            [JobIDs.WAR] = "WAR",
            [JobIDs.DRK] = "DRK",
            [JobIDs.GNB] = "GNB",

            // melee dps
            [JobIDs.PGL] = "PGL",
            [JobIDs.LNC] = "LNC",
            [JobIDs.ROG] = "ROG",
            [JobIDs.MNK] = "MNK",
            [JobIDs.DRG] = "DRG",
            [JobIDs.NIN] = "NIN",
            [JobIDs.SAM] = "SAM",

            // ranged phys dps
            [JobIDs.ARC] = "ARC",
            [JobIDs.BRD] = "BRD",
            [JobIDs.MCH] = "MCH",
            [JobIDs.DNC] = "DNC",

            // ranged magic dps
            [JobIDs.THM] = "THM",
            [JobIDs.ACN] = "ACN",
            [JobIDs.BLM] = "BLM",
            [JobIDs.SMN] = "SMN",
            [JobIDs.RDM] = "RDM",
            [JobIDs.BLU] = "BLU",

            // healers
            [JobIDs.CNJ] = "CNJ",
            [JobIDs.WHM] = "WHM",
            [JobIDs.SCH] = "SCH",
            [JobIDs.AST] = "AST",

            // crafters
            [JobIDs.CRP] = "CRP",
            [JobIDs.BSM] = "BSM",
            [JobIDs.ARM] = "ARM",
            [JobIDs.GSM] = "GSM",
            [JobIDs.LTW] = "LTW",
            [JobIDs.WVR] = "WVR",
            [JobIDs.ALC] = "ALC",
            [JobIDs.CUL] = "CUL",

            // gatherers
            [JobIDs.MIN] = "MIN",
            [JobIDs.BOT] = "BOT",
            [JobIDs.FSH] = "FSH",
        };
    }

    public static class JobIDs
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
