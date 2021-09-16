using Dalamud.Game.ClientState.Actors.Types;
using FFXIVClientStructs.FFXIV.Client.Game;
using System.Collections.Generic;

namespace DelvUI.Helpers
{
    internal static class GCDHelper
    {
        private static readonly Dictionary<uint, uint> JobActionIDs = new()
        {
            [JobIDs.GNB] = 16137, // Keen Edge
            [JobIDs.WAR] = 31,    // Heavy Swing
            [JobIDs.MRD] = 31,    // Heavy Swing
            [JobIDs.DRK] = 3617,  // Hard Slash
            [JobIDs.PLD] = 9,     // Fast Blade
            [JobIDs.GLD] = 9,     // Fast Blade

            [JobIDs.SCH] = 163,   // Ruin
            [JobIDs.AST] = 3596,  // Malefic
            [JobIDs.WHM] = 119,   // Stone
            [JobIDs.CNJ] = 119,   // Stone

            [JobIDs.BRD] = 97,    // Heavy Shot
            [JobIDs.ARC] = 97,    // Heavy Shot
            [JobIDs.DNC] = 15989, // Cascade
            [JobIDs.MCH] = 2866,  // Split Shot

            [JobIDs.SMN] = 163,   // Ruin
            [JobIDs.ACN] = 163,   // Ruin
            [JobIDs.RDM] = 7504,  // Riposte
            [JobIDs.BLM] = 142,   // Blizzard
            [JobIDs.THM] = 142,   // Blizzard

            [JobIDs.SAM] = 7477,  // Hakaze
            [JobIDs.NIN] = 2240,  // Spinning Edge
            [JobIDs.MNK] = 53,    // Bootshine
            [JobIDs.DRG] = 75,    // True Thrust

            [JobIDs.BLU] = 11385  // Water Cannon
        };

        public static unsafe bool GetGCDInfo(PlayerCharacter player, out float timeElapsed, out float timeTotal, ActionType actionType = ActionType.Spell)
        {
            if (player is null || !JobActionIDs.TryGetValue(player.ClassJob.Id, out var actionId))
            {
                timeElapsed = 0;
                timeTotal = 0;

                return false;
            }

            var actionManager = ActionManager.Instance();
            var adjustedId = actionManager->GetAdjustedActionId(actionId);
            timeElapsed = actionManager->GetRecastTimeElapsed(actionType, adjustedId);
            timeTotal = actionManager->GetRecastTime(actionType, adjustedId);

            return timeElapsed > 0;
        }
    }
}
