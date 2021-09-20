using Dalamud.Game.ClientState.Actors.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelvUI.Interface.StatusEffects
{
    public class CustomEffectsListHud : StatusEffectsListHud
    {
        public CustomEffectsListHud(string id, StatusEffectsListConfig config, string displayName) : base(id, config, displayName)
        {
        }

        public Actor TargetActor { get; set; } = null;

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
