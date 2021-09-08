using Dalamud.Game.ClientState.Actors.Types;
using DelvUI.Config;
using FFXIVClientStructs.FFXIV.Client.Game;
using System.Collections.Generic;

namespace DelvUI.Helpers
{
    internal static class GCDHelper
    {
        private static readonly Dictionary<uint, uint> JobActionIDs = new()
        {
            [Jobs.GNB] = 16137, // Keen Edge
            [Jobs.WAR] = 31,    // Heavy Swing
            [Jobs.MRD] = 31,    // Heavy Swing
            [Jobs.DRK] = 3617,  // Hard Slash
            [Jobs.PLD] = 9,     // Fast Blade
            [Jobs.GLD] = 9,     // Fast Blade

            [Jobs.SCH] = 163,   // Ruin
            [Jobs.AST] = 3596,  // Malefic
            [Jobs.WHM] = 119,   // Stone
            [Jobs.CNJ] = 119,   // Stone

            [Jobs.BRD] = 97,    // Heavy Shot
            [Jobs.ARC] = 97,    // Heavy Shot
            [Jobs.DNC] = 15989, // Cascade
            [Jobs.MCH] = 2866,  // Split Shot

            [Jobs.SMN] = 163,   // Ruin
            [Jobs.ACN] = 163,   // Ruin
            [Jobs.RDM] = 7504,  // Riposte
            [Jobs.BLM] = 142,   // Blizzard
            [Jobs.THM] = 142,   // Blizzard

            [Jobs.SAM] = 7477,  // Hakaze
            [Jobs.NIN] = 2240,  // Spinning Edge
            [Jobs.MNK] = 53,    // Bootshine
            [Jobs.DRG] = 75,    // True Thrust

            [Jobs.BLU] = 11385  // Water Cannon
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
