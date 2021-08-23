using System;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;

namespace DelvUI.Helpers
{
    internal static class Utils
    {
        public static unsafe bool IsHostileMemory(BattleNpc npc)
        {
            if (npc == null) return false;

            return (npc.BattleNpcKind == BattleNpcSubKind.Enemy || (int)npc.BattleNpcKind == 1)
                   && *(byte*)(npc.Address + 0x1980) != 0
                   && *(byte*)(npc.Address + 0x193C) != 1;
        }

        public static unsafe float ActorShieldValue(GameObject actor)
        {
            if (actor == null) return 0f;

            return Math.Min(*(int*)(actor.Address + 0x1997), 100) / 100f;
        }

        public static string DurationToString(double duration)
        {
            if (duration == 0) return "";

            TimeSpan t = TimeSpan.FromSeconds(duration);

            if (t.Hours > 1) return t.Hours + "h";
            if (t.Minutes >= 5) return t.Minutes + "m";
            if (t.Minutes >= 1) return t.Minutes + ":" + t.Seconds;

            return t.Seconds.ToString();
        }
    }
}
