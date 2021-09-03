using System;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace DelvUI.Helpers
{
    class SpellHelper
    {
        private readonly unsafe ActionManager* _actionManager;

        public unsafe SpellHelper()
        {
            _actionManager = ActionManager.Instance();
        }

        public unsafe uint GetSpellActionId(uint ActionID)
        {
            return _actionManager->GetAdjustedActionId(ActionID);
        }

        public unsafe float GetRecastTimeElapsed(uint ActionID)
        {
            return _actionManager->GetRecastTimeElapsed(ActionType.Spell, GetSpellActionId(ActionID));
        }

        public unsafe float GetRecastTime(uint ActionID)
        {
            return _actionManager->GetRecastTime(ActionType.Spell, GetSpellActionId(ActionID));
        }

        public float GetSpellCooldown(uint ActionID)
        {
            return Math.Abs(GetRecastTime(GetSpellActionId(ActionID)) - GetRecastTimeElapsed(GetSpellActionId(ActionID)));
        }

        public int GetSpellCooldownInt(uint ActionID)
        {
            if ((int) Math.Ceiling(GetSpellCooldown(ActionID) % GetRecastTime(ActionID)) <= 0)
            {
                return 0;
            }

            return (int)Math.Ceiling(GetSpellCooldown(ActionID) % GetRecastTime(ActionID));
        }

        public int GetStackCount(int maxStacks, uint ActionID)
        {
            if (GetSpellCooldownInt(ActionID) == 0 || GetSpellCooldownInt(ActionID) < 0)
            {
                return maxStacks;
            }

            return maxStacks - (int)Math.Ceiling(GetSpellCooldownInt(ActionID) / (GetRecastTime(ActionID) / maxStacks));
        }

        /*public unsafe uint CheckActionResources(uint ActionID)
        {
            return _actionManager->CheckActionResources(ActionType.Spell, ActionID);
        }*/

    }
}
