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

            // show mine first
            if (Config.ShowMineFirst)
            {
                OrderByMineFirst(list);
            }

            return list;
        }
    }
}
