using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using Newtonsoft.Json;
using System;
using System.Reflection;

namespace DelvUI.Interface.Jobs
{
    [Serializable]
    public class JobConfig : MovablePluginConfigObject
    {
        [JsonIgnore]
        public uint JobId;

        [Checkbox("Use Default Primary Resource Bar")]
        [Order(20)]
        public bool UseDefaulyPrimaryResourceBar = false;

        [JsonIgnore]
        public PrimaryResourceTypes PrimaryResourceType = PrimaryResourceTypes.MP;

        public new static JobConfig DefaultConfig()
        {
            return (JobConfig)Activator.CreateInstance(MethodBase.GetCurrentMethod().DeclaringType);
        }
    }
}
