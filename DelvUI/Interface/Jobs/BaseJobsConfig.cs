using DelvUI.Helpers;
using Newtonsoft.Json;

namespace DelvUI.Interface.Jobs
{
    public class BaseJobsConfig : JobConfig
    {
        public override uint JobId => 0;

        public BaseJobsConfig()
        {
            UseDefaultPrimaryResourceBar = true;
        }
    }

    public class GladiatorConfig : BaseJobsConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.GLA;
    }

    public class MarauderConfig : BaseJobsConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.MRD;
    }

    public class PugilistConfig : BaseJobsConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.PGL;
    }

    public class LancerConfig : BaseJobsConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.LNC;
    }

    public class RogueConfig : BaseJobsConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.ROG;
    }

    public class ArcherConfig : BaseJobsConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.ARC;
    }

    public class ThaumaturgeConfig : BaseJobsConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.THM;
    }

    public class ArcanistConfig : BaseJobsConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.ACN;
    }

    public class ConjurerConfig : BaseJobsConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.CNJ;
    }
}
