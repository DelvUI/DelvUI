using System;
using Dalamud.Game.ClientState.Actors;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Actors.Types.NonPlayer;

namespace DelvUI.Helpers
{
    class Utils
    {
        public static unsafe bool IsHostileMemory(BattleNpc npc)
        {
            if (npc == null) return false;

            return (npc.BattleNpcKind == BattleNpcSubKind.Enemy || (int)npc.BattleNpcKind == 1)
                   && *(byte*)(npc.Address + 0x1980) != 0
                   && *(byte*)(npc.Address + 0x193C) != 1;
        }

        public static unsafe float ActorShieldValue(Actor actor)
        {
            if (actor == null) return 0f;

            return Math.Min(*(int*)(actor.Address + 0x1997), 100) / 100f;
        }
    }
}
