﻿using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;

namespace DelvUI.Interface.StatusEffects
{
    public class CustomEffectsListHud : StatusEffectsListHud
    {
        public CustomEffectsListHud(StatusEffectsListConfig config, string displayName) : base(config, displayName)
        {
        }

        public IGameObject? TargetActor { get; set; } = null!;

        protected override List<StatusEffectData> StatusEffectsData()
        {
            var list = StatusEffectDataList(TargetActor);
            list.AddRange(StatusEffectDataList(Actor));

            // cull duplicate statuses from the same source
            list = list.GroupBy(s => new { s.Status.StatusId, s.Status.SourceId })
                .Select(status => status.First())
                .ToList();

            // show mine or permanent first
            if (Config.ShowMineFirst || Config.ShowPermanentFirst)
            {
                return OrderByMineOrPermanentFirst(list);
            }

            return list;
        }
    }
}
