using DelvUI.Helpers;
using Newtonsoft.Json;

namespace DelvUI.Interface.Jobs
{
    public class ViperConfig : BaseJobsConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.VPR;
    }
}
