﻿using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Interface.GeneralElements;
using DelvUI.Interface.Party;
using System.Linq;

namespace DelvUI.Interface
{
    [Exportable(false)]
    public class VisibilityConfig : PluginConfigObject
    {
        [Checkbox("Hide outside of combat")]
        [Order(5)]
        public bool HideOutsideOfCombat = false;

        [Checkbox("Hide in combat")]
        [Order(6)]
        public bool HideInCombat = false;

        [Checkbox("Hide in Gold Saucer")]
        [Order(7)]
        public bool HideInGoldSaucer = false;

        [Checkbox("Hide while at full HP")]
        [Order(8)]
        public bool HideOnFullHP = false;

        [Checkbox("Hide when in duty")]
        [Order(9)]
        public bool HideInDuty = false;

        [Checkbox("Hide in Island Sanctuary")]
        [Order(10)]
        public bool HideInIslandSanctuary = false;

        [Checkbox("Hide in PvP")]
        [Order(11)]
        public bool HideInPvP = false;

        [Checkbox("Always show when in duty")]
        [Order(20)]
        public bool ShowInDuty = false;

        [Checkbox("Always show when weapon is drawn")]
        [Order(21)]
        public bool ShowOnWeaponDrawn = false;

        [Checkbox("Always show when crafting")]
        [Order(22)]
        public bool ShowWhileCrafting = false;

        [Checkbox("Always show when gathering")]
        [Order(23)]
        public bool ShowWhileGathering = false;

        [Checkbox("Always show while in a party")]
        [Order(24)]
        public bool ShowInParty = false;

        [Checkbox("Always show while in Island Sanctuary")]
        [Order(25)]
        public bool ShowInIslandSanctuary = false;

        [Checkbox("Always show while in PvP")]
        [Order(26)]
        public bool ShowInPvP = false;

        [Checkbox("Always show while target exists")]
        [Order(27)]
        public bool ShowWhileTargetExists = false;


        private bool IsInCombat() => Plugin.Condition[ConditionFlag.InCombat];

        private bool IsInDuty() => Plugin.Condition[ConditionFlag.BoundByDuty];

        private bool IsCrafting() => Plugin.Condition[ConditionFlag.Crafting] || Plugin.Condition[ConditionFlag.Crafting40];

        private bool IsGathering() => Plugin.Condition[ConditionFlag.Gathering] || Plugin.Condition[ConditionFlag.Gathering42];

        private bool HasWeaponDrawn() => (Plugin.ClientState.LocalPlayer != null && Plugin.ClientState.LocalPlayer.StatusFlags.HasFlag(StatusFlags.WeaponOut));

        private bool IsInGoldSaucer() => _goldSaucerIDs.Any(id => id == Plugin.ClientState.TerritoryType);

        private bool IsInIslandSanctuary() => Plugin.ClientState.TerritoryType == 1055;

        private readonly uint[] _goldSaucerIDs = { 144, 388, 389, 390, 391, 579, 792, 899, 941 };

        public bool IsElementVisible(HudElement? element = null)
        {
            if (!Enabled) { return true; }
            if (!ConfigurationManager.Instance.LockHUD) { return true; }
            if (element != null && element.GetType() == typeof(PlayerCastbarHud)) { return true; }
            if (element != null && !element.GetConfig().Enabled) { return false; }

            bool isInIslandSanctuary = IsInIslandSanctuary();
            bool isInDuty = IsInDuty() && !isInIslandSanctuary;
            IPlayerCharacter? player = Plugin.ClientState.LocalPlayer;

            // show
            if (ShowInDuty && isInDuty) { return true; }

            if (ShowOnWeaponDrawn && HasWeaponDrawn()) { return true; }

            if (ShowWhileCrafting && IsCrafting()) { return true; }

            if (ShowWhileGathering && IsGathering()) { return true; }

            if (ShowInParty && PartyManager.Instance.MemberCount > 1) { return true; }

            if (ShowInIslandSanctuary && isInIslandSanctuary) { return true; }

            if (ShowInPvP && Plugin.ClientState.IsPvP) { return true; }

            if (ShowWhileTargetExists && player != null && player.TargetObject != null) { return true; }


            // hide
            if (HideOutsideOfCombat && !IsInCombat()) { return false; }

            if (HideInCombat && IsInCombat()) { return false; }

            if (HideInGoldSaucer && IsInGoldSaucer()) { return false; }

            if (HideOnFullHP && player != null && player.CurrentHp == player.MaxHp) { return false; }

            if (HideInDuty && isInDuty) { return false; }

            if (HideInIslandSanctuary && isInIslandSanctuary) { return false; }

            if (HideInPvP && Plugin.ClientState.IsPvP) { return false; }

            return true;
        }

        public void CopyFrom(VisibilityConfig config)
        {
            Enabled = config.Enabled;

            HideOutsideOfCombat = config.HideOutsideOfCombat;
            HideInCombat = config.HideInCombat;
            HideInGoldSaucer = config.HideInGoldSaucer;
            HideOnFullHP = config.HideOnFullHP;
            HideInDuty = config.HideInDuty;
            HideInIslandSanctuary = config.HideInIslandSanctuary;
            HideInPvP = config.HideInPvP;

            ShowInDuty = config.ShowInDuty;
            ShowOnWeaponDrawn = config.ShowOnWeaponDrawn;
            ShowWhileCrafting = config.ShowWhileCrafting;
            ShowWhileGathering = config.ShowWhileGathering;
            ShowInParty = config.ShowInParty;
            ShowInIslandSanctuary = config.ShowInIslandSanctuary;
            ShowInPvP = config.ShowInPvP;
            ShowWhileTargetExists = config.ShowWhileTargetExists;
        }

        public VisibilityConfig()
        {
            Enabled = false;
        }
    }
}


