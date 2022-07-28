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

        [Checkbox("Always show when in duty")]
        [Order(10)]
        public bool ShowInDuty = false;

        [Checkbox("Always show when weapon is drawn")]
        [Order(11)]
        public bool ShowOnWeaponDrawn = false;

        [Checkbox("Always show when crafting")]
        [Order(12)]
        public bool ShowWhileCrafting = false;

        [Checkbox("Always show when gathering")]
        [Order(13)]
        public bool ShowWhileGathering = false;

        [Checkbox("Always show while in a party")]
        [Order(14)]
        public bool ShowInParty = false;


        private bool IsInCombat() => Plugin.Condition[ConditionFlag.InCombat];

        private bool IsInDuty() => Plugin.Condition[ConditionFlag.BoundByDuty];

        private bool IsCrafting() => Plugin.Condition[ConditionFlag.Crafting] || Plugin.Condition[ConditionFlag.Crafting40];

        private bool IsGathering() => Plugin.Condition[ConditionFlag.Gathering] || Plugin.Condition[ConditionFlag.Gathering42];

        private bool HasWeaponDrawn() => (Plugin.ClientState.LocalPlayer != null && Plugin.ClientState.LocalPlayer.StatusFlags.HasFlag(StatusFlags.WeaponOut));

        private readonly uint[] _goldSaucerIDs = { 144, 388, 389, 390, 391, 579, 792, 899, 941 };

        public bool IsElementVisible(HudElement? element = null)
        {
            if (!Enabled) { return true; }
            if (!ConfigurationManager.Instance.LockHUD) { return true; }
            if (element != null && element.GetType() == typeof(PlayerCastbarHud)) { return true; }
            if (element != null && !element.GetConfig().Enabled) { return false; }

            if (ShowInDuty && IsInDuty()) { return true; }

            if (ShowOnWeaponDrawn && HasWeaponDrawn()) { return true; }

            if (ShowWhileCrafting && IsCrafting()) { return true; }

            if (ShowWhileGathering && IsGathering()) { return true; }

            if (ShowInParty && PartyManager.Instance.MemberCount > 1) { return true; }

            if (HideOutsideOfCombat && !IsInCombat()) { return false; }

            if (HideInCombat && IsInCombat()) { return false; }

            if (HideInGoldSaucer && _goldSaucerIDs.Any(id => id == Plugin.ClientState.TerritoryType)) { return false; }

            PlayerCharacter? player = Plugin.ClientState.LocalPlayer;
            if (HideOnFullHP && player != null && player.CurrentHp == player.MaxHp) { return false; }

            return true;
        }

        public void CopyFrom(VisibilityConfig config)
        {
            Enabled = config.Enabled;
            HideOutsideOfCombat = config.HideOutsideOfCombat;
            HideInGoldSaucer = config.HideInGoldSaucer;
            HideOnFullHP = config.HideOnFullHP;
            ShowInDuty = config.ShowInDuty;
            ShowOnWeaponDrawn = config.ShowOnWeaponDrawn;
            ShowWhileCrafting = config.ShowWhileCrafting;
            ShowWhileGathering = config.ShowWhileGathering;
            ShowInParty = config.ShowInParty;
        }

        public VisibilityConfig()
        {
            Enabled = false;
        }
    }
}


