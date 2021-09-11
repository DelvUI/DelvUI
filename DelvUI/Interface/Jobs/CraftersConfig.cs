using DelvUI.Helpers;
using Newtonsoft.Json;

namespace DelvUI.Interface.Jobs
{
    public class CraftersConfig : JobConfig
    {
        public override uint JobId => 0;

        public CraftersConfig()
        {
            UseDefaultPrimaryResourceBar = true;
            PrimaryResourceType = PrimaryResourceTypes.CP;
        }
    }

    public class CarpenterConfig : CraftersConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.CRP;
    }

    public class BlacksmithConfig : CraftersConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.BSM;
    }

    public class ArmorerConfig : CraftersConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.ARM;
    }

    public class GoldsmithConfig : CraftersConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.GSM;
    }

    public class LeatherworkerConfig : CraftersConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.LTW;
    }

    public class WeaverConfig : CraftersConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.WVR;
    }

    public class AlchemistConfig : CraftersConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.ALC;
    }

    public class CulinarianConfig : CraftersConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.CUL;
    }
}
