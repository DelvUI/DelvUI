using DelvUI.Helpers;
using Newtonsoft.Json;

namespace DelvUI.Interface.Jobs
{
    public class BaseJobsConfig : JobConfig
    {
        public BaseJobsConfig()
        {
            UseDefaulyPrimaryResourceBar = true;
        }
    }

    public class BlueMageConfig : BaseJobsConfig
    {
        [JsonIgnore] public new uint JobId = JobIDs.BLU;
    }

    public class GladiatorConfig : BaseJobsConfig
    {
        [JsonIgnore] public new uint JobId = JobIDs.GLD;
    }

    public class MarauderConfig : BaseJobsConfig
    {
        [JsonIgnore] public new uint JobId = JobIDs.MRD;
    }

    public class PugilistConfig : BaseJobsConfig
    {
        [JsonIgnore] public new uint JobId = JobIDs.PGL;
    }

    public class LancerConfig : BaseJobsConfig
    {
        [JsonIgnore] public new uint JobId = JobIDs.LNC;
    }

    public class RogueConfig : BaseJobsConfig
    {
        [JsonIgnore] public new uint JobId = JobIDs.ROG;
    }

    public class ArcherConfig : BaseJobsConfig
    {
        [JsonIgnore] public new uint JobId = JobIDs.ARC;
    }

    public class ThaumaturgeConfig : BaseJobsConfig
    {
        [JsonIgnore] public new uint JobId = JobIDs.THM;
    }

    public class ArcanistConfig : BaseJobsConfig
    {
        [JsonIgnore] public new uint JobId = JobIDs.ACN;
    }

    public class ConjurerConfig : BaseJobsConfig
    {
        [JsonIgnore] public new uint JobId = JobIDs.CNJ;
    }
}
