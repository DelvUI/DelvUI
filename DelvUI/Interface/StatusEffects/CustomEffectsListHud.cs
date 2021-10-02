using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;

namespace DelvUI.Interface.StatusEffects
{
    public class CustomEffectsListHud : StatusEffectsListHud
    {
        public CustomEffectsListHud(string id, StatusEffectsListConfig config, string displayName) : base(id, config, displayName)
        {
        }

        public GameObject? TargetActor { get; set; } = null!;

        protected override List<StatusEffectData> StatusEffectsData()
        {
            var list = StatusEffectDataList(TargetActor);
            list.AddRange(StatusEffectDataList(Actor));

            // cull duplicate statuses from the same source
            list = list.GroupBy(s => new { s.Status.StatusID, s.Status.SourceID })
                .Select(status => status.First())
                .ToList();

            // show mine first
            if (Config.ShowMineFirst)
            {
                OrderByMineFirst(list);
            }

            return list;
        }
    }
}
