using DelvUI.Helpers;
using Newtonsoft.Json;

namespace DelvUI.Interface.Jobs
{
    public class CraftersConfig : JobConfig
    {
        public CraftersConfig()
        {
            UseDefaulyPrimaryResourceBar = true;
            PrimaryResourceType = PrimaryResourceTypes.CP;
        }
    }

    public class CarpenterConfig : CraftersConfig
    {
        [JsonIgnore] public new uint JobId = JobIDs.CRP;
    }

    public class BlacksmithConfig : CraftersConfig
    {
        [JsonIgnore] public new uint JobId = JobIDs.BSM;
    }

    public class ArmorerConfig : CraftersConfig
    {
        [JsonIgnore] public new uint JobId = JobIDs.ARM;
    }

    public class GoldsmithConfig : CraftersConfig
    {
        [JsonIgnore] public new uint JobId = JobIDs.GSM;
    }

    public class LeatherworkerConfig : CraftersConfig
    {
        [JsonIgnore] public new uint JobId = JobIDs.LTW;
    }

    public class WeaverConfig : CraftersConfig
    {
        [JsonIgnore] public new uint JobId = JobIDs.WVR;
    }

    public class AlchemistConfig : CraftersConfig
    {
        [JsonIgnore] public new uint JobId = JobIDs.ALC;
    }

    public class CulinarianConfig : CraftersConfig
    {
        [JsonIgnore] public new uint JobId = JobIDs.CUL;
    }
}
