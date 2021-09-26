using DelvUI.Config;
using DelvUI.Interface.GeneralElements;
using System.Diagnostics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.SubKinds;
using DelvUI.Interface.Jobs;

namespace DelvUI.Interface
{
    public class GUIAddon
    {
        public readonly unsafe AtkUnitBase* addonPtr;
        public readonly string name;

        public unsafe GUIAddon(AtkUnitBase* addonPtr, string name)
        {
            this.addonPtr = addonPtr;
            this.name = name;
        }

        public unsafe void VisibilityToggle(bool isHidden)
        {
            addonPtr->IsVisible = !isHidden;
            if (!isHidden)
            {
                addonPtr->UldManager.UpdateDrawNodeList(); // enable
            }
        }

        public unsafe void NodeListVisibilityToggle(bool isHidden)
        {
            if (isHidden)
            {
                addonPtr->UldManager.NodeListCount = 0; // disable
            }
            else
            {
                addonPtr->IsVisible = true;
                addonPtr->UldManager.UpdateDrawNodeList(); // enable
            }
        }
    }

    public class HudHelper
    {
        private HideHudConfig Config => ConfigurationManager.GetInstance().GetConfigObject<HideHudConfig>();

        private bool _previousCombatState = true;
        private bool _isInitial = true;
        private uint[] GoldSaucerIDs = new uint[] { 144, 388, 389, 390, 391, 579, 792, 899, 941 };

        public HudHelper()
        {
            Config.ValueChangeEvent += ConfigValueChanged;
        }

        ~HudHelper()
        {
            Config.ValueChangeEvent -= ConfigValueChanged;
        }

        public unsafe void Configure(bool forceUpdate = false)
        {
            if (!_isInitial && !forceUpdate)
            {
                return;
            }

            ConfigureCombatActionBars(_isInitial || forceUpdate);

            ToggleDefaultComponent(delegate (GUIAddon addon)
            {
                if (addon.name == "_CastBar")
                {
                    addon.NodeListVisibilityToggle(Config.HideDefaultCastbar);
                }
                else if (addon.name.StartsWith("JobHud") && !addon.name.StartsWith("JobHudNotice"))
                {
                    bool isHidden = Config.HideDefaultJobGauges;

                    addon.NodeListVisibilityToggle(isHidden && Config.DisableJobGaugeSounds);
                    if (!Config.DisableJobGaugeSounds)
                    {
                        addon.VisibilityToggle(isHidden);

                    }
                }
            });

            _isInitial = false;
        }

        public bool IsElementHidden(HudElement element)
        {
            if (!ConfigurationManager.GetInstance().LockHUD)
            {
                return ConfigurationManager.GetInstance().LockHUD;
            }

            if (element.GetType() == typeof(PlayerCastbarHud))
            {
                return false;
            }

            // hide in gold saucer
            if (Config.HideInGoldSaucer && GoldSaucerIDs.Where(id => id == Plugin.ClientState.TerritoryType).Count() > 0)
            {
                return true;
            }

            bool isHidden = Config.HideOutsideOfCombat && !IsInCombat();
            if (!isHidden && element is JobHud)
            {
                return Config.HideOnlyJobPackHudOutsideOfCombat && !IsInCombat();
            }

            if (element.GetConfig().GetType() == typeof(PlayerUnitFrameConfig))
            {
                PlayerCharacter? player = Plugin.ClientState.LocalPlayer;
                if (player is not null)
                {
                    isHidden = isHidden && player.CurrentHp == player.MaxHp;
                }
            }

            return isHidden;
        }

        private void ConfigValueChanged(object sender, OnChangeBaseArgs e)
        {
            if (e.PropertyName == "HideDefaultCastbar")
            {
                ConfigureDefaultCastBar();
            }
            else if (e.PropertyName == "HideDefaultJobGauges" || e.PropertyName == "DisableJobGaugeSounds")
            {
                ConfigureDefaultJobGauge();
            }
            else if (e.PropertyName == "EnableCombatActionBars")
            {
                Config.CombatActionBars.ForEach(name => ToggleActionbar(name, Config.EnableCombatActionBars));
            }
            else if (e.PropertyName == "CombatActionBars" && e is OnChangeEventArgs<string> listEvent)
            {
                switch (listEvent.ChangeType)
                {
                    case ChangeType.ListAdd:
                        ToggleActionbar(listEvent.Value, !IsInCombat());
                        break;
                    case ChangeType.ListRemove:
                        ToggleActionbar(listEvent.Value, false);
                        break;
                }
            }
        }

        private void ConfigureCombatActionBars(bool forceUpdate = false)
        {
            if (!Config.EnableCombatActionBars)
            {
                return;
            }

            var currentCombatState = IsInCombat();
            if (_previousCombatState != currentCombatState && Config.CombatActionBars.Count > 0 || forceUpdate)
            {
                Config.CombatActionBars.ForEach(name => ToggleActionbar(name, !currentCombatState));
                _previousCombatState = currentCombatState;
            }
        }

        private unsafe void ToggleActionbar(string targetName, bool isHidden)
        {
            ToggleDefaultComponent(delegate (GUIAddon addon)
            {
                string keyCode = GetActionBarName(targetName);
                if (addon.name == keyCode)
                {
                    if (isHidden)
                    {
                        addon.addonPtr->IsVisible = false;
                        addon.addonPtr->UldManager.NodeListCount = 0; // disable
                    }
                    else
                    {
                        addon.addonPtr->IsVisible = true;
                        addon.addonPtr->UldManager.UpdateDrawNodeList(); // enable
                    }
                }
            });
        }

        private void ConfigureDefaultCastBar()
        {
            ToggleDefaultComponent(delegate (GUIAddon addon)
            {
                if (addon.name == "_CastBar")
                {
                    addon.NodeListVisibilityToggle(Config.HideDefaultCastbar);
                }
            });
        }

        private unsafe void ConfigureDefaultJobGauge()
        {
            ToggleDefaultComponent(delegate (GUIAddon addon)
            {
                if (addon.name.StartsWith("JobHud"))
                {
                    bool isHidden = Config.HideDefaultJobGauges;
                    if (isHidden && Config.DisableJobGaugeSounds)
                    {
                        addon.NodeListVisibilityToggle(isHidden);
                    }
                    else
                    {
                        addon.VisibilityToggle(isHidden);
                    }
                }
            });
        }

        #region Helpers

        private bool IsInCombat()
        {
            return Plugin.Condition[ConditionFlag.InCombat];
        }

        private bool IsInDuty()
        {
            return Plugin.Condition[ConditionFlag.BoundByDuty];
        }

        public static unsafe void ToggleDefaultComponent(Action<GUIAddon> action)
        {
            var stage = AtkStage.GetSingleton();
            if (stage == null)
            {
                return;
            }

            var loadedUnitsList = &stage->RaptureAtkUnitManager->AtkUnitManager.AllLoadedUnitsList;
            if (loadedUnitsList == null)
            {
                return;
            }

            var addonList = &loadedUnitsList->AtkUnitEntries;
            if (addonList == null)
            {
                return;
            }

            for (var i = 0; i < loadedUnitsList->Count; i++)
            {
                AtkUnitBase* addon = addonList[i];
                if (addon == null)
                {
                    continue;
                }

                var name = Marshal.PtrToStringAnsi(new IntPtr(addon->Name));
                if (name == null)
                {
                    continue;
                }

                action?.Invoke(new GUIAddon(addon, name));
            }
        }

        public static unsafe void RestoreToGameDefaults()
        {
            ToggleDefaultComponent(delegate (GUIAddon addon)
            {
                if (addon.name == "_CastBar"
                    || addon.name.StartsWith("JobHud")
                    || addon.name.StartsWith("_ActionBar"))
                {
                    addon.VisibilityToggle(false);
                }
            });
        }

        public static Dictionary<string, string> HotbarMap => new Dictionary<string, string>()
            {
                { "Hotbar 1", "_ActionBar" },
                { "Hotbar 2", "_ActionBar01" },
                { "Hotbar 3", "_ActionBar02" },
                { "Hotbar 4", "_ActionBar03" },
                { "Hotbar 5", "_ActionBar04" },
                { "Hotbar 6", "_ActionBar05" },
                { "Hotbar 7", "_ActionBar06" },
                { "Hotbar 8", "_ActionBar07" },
                { "Hotbar 9", "_ActionBar08" },
                { "Hotbar 10", "_ActionBar09" },
            };

        private string GetActionBarName(string hotbarUserFriendlyName)
        {
            HudHelper.HotbarMap.TryGetValue(hotbarUserFriendlyName, out var result);
            return result ?? hotbarUserFriendlyName;
        }

        #endregion
    }
}
