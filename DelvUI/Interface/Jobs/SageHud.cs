using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using Newtonsoft.Json;

namespace DelvUI.Interface.Jobs
{
    public class SageHud : JobHud
    {
        private new SageConfig Config => (SageConfig)_config;

        public SageHud(JobConfig config, string? displayName = null) : base(config, displayName)
        {
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new();
            List<Vector2> sizes = new();

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            Vector2 pos = origin + Config.Position;
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Healer", 0)]
    [SubSection("Sage", 1)]
    public class SageConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.SGE;

        public new static SageConfig DefaultConfig()
        {
            var config = new SageConfig();

            return config;
        }
    }
}
