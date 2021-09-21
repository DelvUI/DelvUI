using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using Newtonsoft.Json;
using System;
using System.Reflection;

namespace DelvUI.Interface.Jobs
{
    public abstract class JobConfig : MovablePluginConfigObject
    {
        [JsonIgnore]
        public abstract uint JobId { get; }

        [Checkbox("Use Generic MP Bar")]
        [Order(20)]
        public bool UseDefaultPrimaryResourceBar = false;

        [JsonIgnore]
        public PrimaryResourceTypes PrimaryResourceType = PrimaryResourceTypes.MP;

        public new static JobConfig DefaultConfig()
        {
            return (JobConfig)Activator.CreateInstance(MethodBase.GetCurrentMethod().DeclaringType);
        }

        public JobConfig()
        {
            Position.Y = HUDConstants.JobHudsBaseY;
        }
    }
}
