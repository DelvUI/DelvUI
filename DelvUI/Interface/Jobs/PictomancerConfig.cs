using DelvUI.Helpers;
using Newtonsoft.Json;

namespace DelvUI.Interface.Jobs
{
    public class PictomancerConfig : BaseJobsConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.PCT;
    }
}
