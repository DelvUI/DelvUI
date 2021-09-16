using DelvUI.Helpers;
using Newtonsoft.Json;

namespace DelvUI.Interface.Jobs
{
    public class GatherersConfig : JobConfig
    {
        public override uint JobId => 0;

        public GatherersConfig()
        {
            UseDefaultPrimaryResourceBar = true;
            PrimaryResourceType = PrimaryResourceTypes.GP;
        }
    }

    public class MinerConfig : GatherersConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.MIN;
    }

    public class BotanistConfig : GatherersConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.BOT;
    }

    public class FisherConfig : GatherersConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.FSH;
    }
}
