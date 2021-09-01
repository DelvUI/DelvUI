using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelvUI.Helpers
{
    public static class JobsHelper
    {
        public static List<uint> tankJobIds = new List<uint>() { 
            Jobs.GLD, Jobs.MRD, Jobs.PLD, Jobs.WAR, Jobs.DRK, Jobs.GNB 
        };

        public static List<uint> healerJobIds = new List<uint>() {
            Jobs.CNJ, Jobs.WHM, Jobs.SCH, Jobs.AST 
        };

        public static List<uint> dpsJobIds = new List<uint>() {
            Jobs.PGL, Jobs.LNC, Jobs.ROG, Jobs.MNK, Jobs.DRG, Jobs.NIN, Jobs.SAM,
            Jobs.ARC, Jobs.BRD, Jobs.MCH, Jobs.DNC,
            Jobs.THM, Jobs.ACN, Jobs.BLM, Jobs.SMN, Jobs.RDM, Jobs.BLU,
        };

        public static List<uint> gathererJobIds = new List<uint>() { 
            Jobs.MIN, Jobs.BOT, Jobs.FSH
        };

        public static List<uint> crafterJobIds = new List<uint>() { 
            Jobs.CRP, Jobs.BSM, Jobs.ARM, Jobs.GSM, 
            Jobs.LTW, Jobs.WVR, Jobs.ALC, Jobs.CUL
        };

        public static bool isJobTank(uint jobId)
        {
            return tankJobIds.Contains(jobId);
        }
        
        public static bool isJobHealer(uint jobId)
        {
            return healerJobIds.Contains(jobId);
        }

        public static bool isJobDPS(uint jobId)
        {
            return dpsJobIds.Contains(jobId);
        }

        public static bool isJobCrafter(uint jobId)
        {
            return crafterJobIds.Contains(jobId);
        }

        public static bool isJobGatherer(uint jobId)
        {
            return gathererJobIds.Contains(jobId);
        }
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
