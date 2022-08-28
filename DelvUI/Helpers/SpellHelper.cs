using FFXIVClientStructs.FFXIV.Client.Game;
using System;

namespace DelvUI.Helpers
{
    internal class SpellHelper
    {
        #region Singleton
        private static Lazy<SpellHelper> _lazyInstance = new Lazy<SpellHelper>(() => new SpellHelper());

        public static SpellHelper Instance => _lazyInstance.Value;

        ~SpellHelper()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            _lazyInstance = new Lazy<SpellHelper>(() => new SpellHelper());
        }
        #endregion

        private readonly unsafe ActionManager* _actionManager;

        public unsafe SpellHelper()
        {
            _actionManager = ActionManager.Instance();
        }

        public unsafe uint GetSpellActionId(uint actionId) => _actionManager->GetAdjustedActionId(actionId);

        public unsafe float GetRecastTimeElapsed(uint actionId) => _actionManager->GetRecastTimeElapsed(ActionType.Spell, GetSpellActionId(actionId));
        public unsafe float GetRealRecastTimeElapsed(uint actionId) => _actionManager->GetRecastTimeElapsed(ActionType.Spell, actionId);

        public unsafe float GetRecastTime(uint actionId) => _actionManager->GetRecastTime(ActionType.Spell, GetSpellActionId(actionId));
        public unsafe float GetRealRecastTime(uint actionId) => _actionManager->GetRecastTime(ActionType.Spell, actionId);

        public float GetSpellCooldown(uint actionId) => Math.Abs(GetRecastTime(GetSpellActionId(actionId)) - GetRecastTimeElapsed(GetSpellActionId(actionId)));
        public float GetRealSpellCooldown(uint actionId) => Math.Abs(GetRealRecastTime(actionId) - GetRealRecastTimeElapsed(actionId));

        public int GetSpellCooldownInt(uint actionId)
        {
            int cooldown = (int)Math.Ceiling(GetSpellCooldown(actionId) % GetRecastTime(actionId));
            return Math.Max(0, cooldown);
        }

        public int GetStackCount(int maxStacks, uint actionId)
        {
            int cooldown = GetSpellCooldownInt(actionId);
            float recastTime = GetRecastTime(actionId);

            if (cooldown <= 0 || recastTime == 0)
            {
                return maxStacks;
            }

            return maxStacks - (int)Math.Ceiling(cooldown / (recastTime / maxStacks));
        }
    }
}
