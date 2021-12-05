using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using Newtonsoft.Json;

namespace DelvUI.Interface.Jobs
{
    public class ReaperHud : JobHud
    {
        private new ReaperConfig Config => (ReaperConfig)_config;

        public ReaperHud(JobConfig config, string? displayName = null) : base(config, displayName)
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
    [SubSection("Melee", 0)]
    [SubSection("Reaper", 1)]
    public class ReaperConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.RPR;

        public new static ReaperConfig DefaultConfig()
        {
            var config = new ReaperConfig();

            return config;
        }
    }
}
