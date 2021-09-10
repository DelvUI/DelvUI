using DelvUI.Helpers;
using Newtonsoft.Json;

namespace DelvUI.Interface.Jobs
{
    public class GatherersConfig : JobConfig
    {
        public GatherersConfig()
        {
            UseDefaulyPrimaryResourceBar = true;
            PrimaryResourceType = PrimaryResourceTypes.GP;
        }
    }

    public class MinerConfig : GatherersConfig
    {
        [JsonIgnore] public new uint JobId = JobIDs.MIN;
    }

    public class BotanistConfig : GatherersConfig
    {
        [JsonIgnore] public new uint JobId = JobIDs.BOT;
    }

    public class FisherConfig : GatherersConfig
    {
        [JsonIgnore] public new uint JobId = JobIDs.FSH;
    }
}
