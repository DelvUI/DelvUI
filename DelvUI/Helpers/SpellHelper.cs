using FFXIVClientStructs.FFXIV.Client.Game;
using System;

namespace DelvUI.Helpers
{
    internal class SpellHelper
    {
        private readonly unsafe ActionManager* _actionManager;

        public unsafe SpellHelper() { _actionManager = ActionManager.Instance(); }

        public unsafe uint GetSpellActionId(uint actionId) => _actionManager->GetAdjustedActionId(actionId);

        public unsafe float GetRecastTimeElapsed(uint actionId) => _actionManager->GetRecastTimeElapsed(ActionType.Spell, GetSpellActionId(actionId));

        public unsafe float GetRecastTime(uint actionId) => _actionManager->GetRecastTime(ActionType.Spell, GetSpellActionId(actionId));

        public float GetSpellCooldown(uint actionId) => Math.Abs(GetRecastTime(GetSpellActionId(actionId)) - GetRecastTimeElapsed(GetSpellActionId(actionId)));

        public int GetSpellCooldownInt(uint actionId)
        {
            if ((int) Math.Ceiling(GetSpellCooldown(actionId) % GetRecastTime(actionId)) <= 0)
            {
                return 0;
            }

            return (int) Math.Ceiling(GetSpellCooldown(actionId) % GetRecastTime(actionId));
        }

        public int GetStackCount(int maxStacks, uint actionId)
        {
            if (GetSpellCooldownInt(actionId) == 0 || GetSpellCooldownInt(actionId) < 0)
            {
                return maxStacks;
            }

            return maxStacks - (int) Math.Ceiling(GetSpellCooldownInt(actionId) / (GetRecastTime(actionId) / maxStacks));
        }

        /*public unsafe uint CheckActionResources(uint ActionID)
        {
            return _actionManager->CheckActionResources(ActionType.Spell, ActionID);
        }*/
    }
}
