using Dalamud.Game.ClientState.Objects.Types;
using System.Collections.Generic;

namespace DelvUI.Helpers
{
    public enum JobRoles
    {
        Tank = 0,
        Healer = 1,
        DPSMelee = 2,
        DPSRanged = 3,
        DPSCaster = 4,
        Crafter = 5,
        Gatherer = 6,
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

        public static bool IsJobWithCleanse(uint jobId, int level)
        {
            var isOnCleanseJob = _cleanseJobs.Contains(jobId);

            if (jobId == JobIDs.BRD && level < 35)
            {
                isOnCleanseJob = false;
            }

            return isOnCleanseJob;
        }

        private static readonly List<uint> _cleanseJobs = new List<uint>()
        {
            JobIDs.CNJ,
            JobIDs.WHM,
            JobIDs.SCH,
            JobIDs.AST,
            JobIDs.SGE,
            JobIDs.BRD,
            JobIDs.BLU
        };

        public static bool IsJobHealer(uint jobId)
        {
            return IsJobARole(jobId, JobRoles.Healer);
        }

        public static bool IsJobDPS(uint jobId)
        {
            if (JobRolesMap.TryGetValue(jobId, out var r))
            {
                return r == JobRoles.DPSMelee || r == JobRoles.DPSRanged || r == JobRoles.DPSCaster;
            }

            return false;
        }

        public static bool IsJobDPSMelee(uint jobId)
        {
            return IsJobARole(jobId, JobRoles.DPSMelee);
        }

        public static bool IsJobDPSRanged(uint jobId)
        {
            return IsJobARole(jobId, JobRoles.DPSRanged);
        }

        public static bool IsJobDPSCaster(uint jobId)
        {
            return IsJobARole(jobId, JobRoles.DPSCaster);
        }

        public static bool IsJobCrafter(uint jobId)
        {
            return IsJobARole(jobId, JobRoles.Crafter);
        }

        public static bool IsJobGatherer(uint jobId)
        {
            return IsJobARole(jobId, JobRoles.Gatherer);
        }

        public static bool IsJobWithRaise(uint jobId, uint level)
        {
            var isOnRaiseJob = _raiseJobs.Contains(jobId);

            if ((jobId == JobIDs.RDM && level < 64) || level < 12)
            {
                isOnRaiseJob = false;
            }

            return isOnRaiseJob;
        }

        private static readonly List<uint> _raiseJobs = new List<uint>()
        {
            JobIDs.CNJ,
            JobIDs.WHM,
            JobIDs.SCH,
            JobIDs.AST,
            JobIDs.RDM,
            JobIDs.SMN,
            JobIDs.SGE
        };

        public static uint CurrentPrimaryResource(Character? character)
        {
            if (character == null)
            {
                return 0;
            }

            uint jobId = character.ClassJob.Id;

            if (IsJobGatherer(jobId))
            {
                return character.CurrentGp;
            }

            if (IsJobCrafter(jobId))
            {
                return character.CurrentCp;
            }

            return character.CurrentMp;
        }

        public static uint MaxPrimaryResource(Character? character)
        {
            if (character == null)
            {
                return 0;
            }

            uint jobId = character.ClassJob.Id;

            if (IsJobGatherer(jobId))
            {
                return character.MaxGp;
            }

            if (IsJobCrafter(jobId))
            {
                return character.MaxCp;
            }

            return character.MaxMp;
        }

        public static uint GPResourceRate(Character? character)
        {
            if (character == null)
            {
                return 0;
            }

            // Preferably I'd want to check the active traits because these traits are locked behind job quests, but no idea how to check traits.

            // Level 83 Trait 239 (MIN), 240 (BTN), 241 (FSH)
            if (character.Level >= 83)
            {
                return 8;
            }

            // Level 80 Trait 236 (MIN), 237 (BTN), 238 (FSH)
            if (character.Level >= 80)
            {
                return 7;
            }

            // Level 70 Trait 192 (MIN), 193 (BTN), 194 (FSH)
            if (character.Level >= 70)
            {
                return 6;
            }

            return 5;
        }

        public static string TimeTillMaxGP(Character? character)
        {
            if (character == null)
            {
                return "";
            }

            uint jobId = character.ClassJob.Id;

            if (!IsJobGatherer(jobId))
            {
                return "";
            }

            uint gpRate = GPResourceRate(character);

            if (character.CurrentGp == character.MaxGp)
            {
                return "";
            }

            // Since I'm not using a stopwatch or anything like MPTickHelper here the time will only update every 3 seconds, would be nice if the time ticks down every second.
            float gpPerSecond = gpRate / 3f;
            float secondsTillMax = (character.MaxGp - character.CurrentGp) / gpPerSecond;

            return $"({Utils.DurationToFullString(secondsTillMax)})";
        }

        public static uint IconIDForJob(uint jobId)
        {
            return jobId + 62000;
        }

        public static uint RoleIconIDForJob(uint jobId, bool specificDPSIcons = false)
        {
            var role = RoleForJob(jobId);

            switch (role)
            {
                case JobRoles.Tank: return 62581;
                case JobRoles.Healer: return 62582;

                case JobRoles.DPSMelee:
                case JobRoles.DPSRanged:
                case JobRoles.DPSCaster:
                    if (specificDPSIcons && SpecificDPSIcons.TryGetValue(jobId, out var iconId))
                    {
                        return iconId;
                    }
                    else
                    {
                        return 62583;
                    }

                case JobRoles.Gatherer:
                case JobRoles.Crafter:
                    return IconIDForJob(jobId);
            }

            return 0;
        }

        public static uint RoleIconIDForBattleCompanion => 62041;

        public static Dictionary<uint, JobRoles> JobRolesMap = new Dictionary<uint, JobRoles>()
        {
            // tanks
            [JobIDs.GLA] = JobRoles.Tank,
            [JobIDs.MRD] = JobRoles.Tank,
            [JobIDs.PLD] = JobRoles.Tank,
            [JobIDs.WAR] = JobRoles.Tank,
            [JobIDs.DRK] = JobRoles.Tank,
            [JobIDs.GNB] = JobRoles.Tank,

            // healers
            [JobIDs.CNJ] = JobRoles.Healer,
            [JobIDs.WHM] = JobRoles.Healer,
            [JobIDs.SCH] = JobRoles.Healer,
            [JobIDs.AST] = JobRoles.Healer,
            [JobIDs.SGE] = JobRoles.Healer,

            // melee dps
            [JobIDs.PGL] = JobRoles.DPSMelee,
            [JobIDs.LNC] = JobRoles.DPSMelee,
            [JobIDs.ROG] = JobRoles.DPSMelee,
            [JobIDs.MNK] = JobRoles.DPSMelee,
            [JobIDs.DRG] = JobRoles.DPSMelee,
            [JobIDs.NIN] = JobRoles.DPSMelee,
            [JobIDs.SAM] = JobRoles.DPSMelee,
            [JobIDs.RPR] = JobRoles.DPSMelee,

            // ranged phys dps
            [JobIDs.ARC] = JobRoles.DPSRanged,
            [JobIDs.BRD] = JobRoles.DPSRanged,
            [JobIDs.MCH] = JobRoles.DPSRanged,
            [JobIDs.DNC] = JobRoles.DPSRanged,

            // ranged magic dps
            [JobIDs.THM] = JobRoles.DPSCaster,
            [JobIDs.ACN] = JobRoles.DPSCaster,
            [JobIDs.BLM] = JobRoles.DPSCaster,
            [JobIDs.SMN] = JobRoles.DPSCaster,
            [JobIDs.RDM] = JobRoles.DPSCaster,
            [JobIDs.BLU] = JobRoles.DPSCaster,

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

        public static Dictionary<JobRoles, List<uint>> JobsByRole = new Dictionary<JobRoles, List<uint>>()
        {
            // tanks
            [JobRoles.Tank] = new List<uint>() {
                JobIDs.GLA,
                JobIDs.MRD,
                JobIDs.PLD,
                JobIDs.WAR,
                JobIDs.DRK,
                JobIDs.GNB,
            },

            // healers
            [JobRoles.Healer] = new List<uint>()
            {
                JobIDs.CNJ,
                JobIDs.WHM,
                JobIDs.SCH,
                JobIDs.AST,
                JobIDs.SGE
            },

            // melee dps
            [JobRoles.DPSMelee] = new List<uint>() {
                JobIDs.PGL,
                JobIDs.LNC,
                JobIDs.ROG,
                JobIDs.MNK,
                JobIDs.DRG,
                JobIDs.NIN,
                JobIDs.SAM,
                JobIDs.RPR
            },

            // ranged phys dps
            [JobRoles.DPSRanged] = new List<uint>()
            {
                JobIDs.ARC,
                JobIDs.BRD,
                JobIDs.MCH,
                JobIDs.DNC,
            },

            // ranged magic dps
            [JobRoles.DPSCaster] = new List<uint>()
            {
                JobIDs.THM,
                JobIDs.ACN,
                JobIDs.BLM,
                JobIDs.SMN,
                JobIDs.RDM,
                JobIDs.BLU,
            },

            // crafters
            [JobRoles.Crafter] = new List<uint>()
            {
                JobIDs.CRP,
                JobIDs.BSM,
                JobIDs.ARM,
                JobIDs.GSM,
                JobIDs.LTW,
                JobIDs.WVR,
                JobIDs.ALC,
                JobIDs.CUL,
            },

            // gatherers
            [JobRoles.Gatherer] = new List<uint>()
            {
                JobIDs.MIN,
                JobIDs.BOT,
                JobIDs.FSH,
            },

            // unknown
            [JobRoles.Unknown] = new List<uint>()
        };

        public static Dictionary<uint, string> JobNames = new Dictionary<uint, string>()
        {
            // tanks
            [JobIDs.GLA] = "GLA",
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
            [JobIDs.RPR] = "RPR",

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
            [JobIDs.SGE] = "SGE",
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

        public static Dictionary<JobRoles, string> RoleNames = new Dictionary<JobRoles, string>()
        {
            [JobRoles.Tank] = "Tank",
            [JobRoles.Healer] = "Healer",
            [JobRoles.DPSMelee] = "Melee",
            [JobRoles.DPSRanged] = "Ranged",
            [JobRoles.DPSCaster] = "Caster",
            [JobRoles.Crafter] = "Crafter",
            [JobRoles.Gatherer] = "Gatherer",
            [JobRoles.Unknown] = "Unknown"
        };

        public static Dictionary<uint, uint> SpecificDPSIcons = new Dictionary<uint, uint>()
        {
            // melee dps
            [JobIDs.PGL] = 62584,
            [JobIDs.LNC] = 62584,
            [JobIDs.ROG] = 62584,
            [JobIDs.MNK] = 62584,
            [JobIDs.DRG] = 62584,
            [JobIDs.NIN] = 62584,
            [JobIDs.SAM] = 62584,
            [JobIDs.RPR] = 62584,

            // ranged phys dps
            [JobIDs.ARC] = 62586,
            [JobIDs.BRD] = 62586,
            [JobIDs.MCH] = 62586,
            [JobIDs.DNC] = 62586,

            // ranged magic dps
            [JobIDs.THM] = 62587,
            [JobIDs.ACN] = 62587,
            [JobIDs.BLM] = 62587,
            [JobIDs.SMN] = 62587,
            [JobIDs.RDM] = 62587,
            [JobIDs.BLU] = 62587
        };

        public static Dictionary<JobRoles, PrimaryResourceTypes> PrimaryResourceTypesByRole = new Dictionary<JobRoles, PrimaryResourceTypes>()
        {
            [JobRoles.Tank] = PrimaryResourceTypes.MP,
            [JobRoles.Healer] = PrimaryResourceTypes.MP,
            [JobRoles.DPSMelee] = PrimaryResourceTypes.MP,
            [JobRoles.DPSRanged] = PrimaryResourceTypes.MP,
            [JobRoles.DPSCaster] = PrimaryResourceTypes.MP,
            [JobRoles.Crafter] = PrimaryResourceTypes.CP,
            [JobRoles.Gatherer] = PrimaryResourceTypes.GP,
            [JobRoles.Unknown] = PrimaryResourceTypes.MP
        };
    }

    public static class JobIDs
    {
        public const uint GLA = 1;
        public const uint MRD = 3;
        public const uint PLD = 19;
        public const uint WAR = 21;
        public const uint DRK = 32;
        public const uint GNB = 37;

        public const uint CNJ = 6;
        public const uint WHM = 24;
        public const uint SCH = 28;
        public const uint AST = 33;
        public const uint SGE = 40;

        public const uint PGL = 2;
        public const uint LNC = 4;
        public const uint ROG = 29;
        public const uint MNK = 20;
        public const uint DRG = 22;
        public const uint NIN = 30;
        public const uint SAM = 34;
        public const uint RPR = 39;

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
