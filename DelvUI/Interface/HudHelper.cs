using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Actors.Types;
using DelvUI.Config;
using DelvUI.Interface.GeneralElements;
using System.Diagnostics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Runtime.InteropServices;
using System;
using Dalamud.Plugin;
using DelvUI.Config.Attributes;
using System.Linq;
using System.Collections.Generic;
using DelvUI.Interface.Jobs;

namespace DelvUI.Interface
{
    internal class GUIAddon
    {
        public unsafe AtkUnitBase* addonPtr;
        public string name;

        public unsafe void DefaultVisibilityToggle(bool isHidden)
        {
            if (isHidden)
            {
                addonPtr->UldManager.NodeListCount = 0; // disable
            }
            else
            {
                addonPtr->UldManager.UpdateDrawNodeList(); // enable
            }
        }
    }

    public class HudHelper
    {
        private HideHudConfig Config => ConfigurationManager.GetInstance().GetConfigObject<HideHudConfig>();

        private bool _previousCombatState = true;
        public bool UserInterfaceWasHidden = false;

        public HudHelper()
        {
            Config.onValueChanged += ConfigValueChanged;
        }

        ~HudHelper()
        {
            Config.onValueChanged -= ConfigValueChanged;
        }

        #region Event Handlers

        public void ConfigValueChanged(object sender, OnChangeBaseArgs e)
        {
            if (e.PropertyName == "HideDefaultCastbar")
            {
                ConfigureDefaultCastBar();
            }
            else if (e.PropertyName == "HideDefaultJobGauges")
            {
                ConfigureDefaultJobGauge();
            }
            else if (e.PropertyName == "EnableCombatActionBars" && e is OnChangeEventArgs<bool> boolEvent)
            {
                Config.CombatActionBars.ForEach(name => ToggleActionbar(name, boolEvent.Value));
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

        #endregion


        #region Action Bars
        public void ConfigureCombatActionBars()
        {
            if (!Config.EnableCombatActionBars)
            {
                return;
            }

            var currentCombatState = IsInCombat();
            if (_previousCombatState != currentCombatState && Config.CombatActionBars.Count > 0 || UserInterfaceWasHidden)
            {
                Config.CombatActionBars.ForEach(name => ToggleActionbar(name, !currentCombatState));
                _previousCombatState = currentCombatState;
            }
        }

        #endregion

        public bool IsElementHidden(HudElement element)
        {
            if (!ConfigurationManager.GetInstance().LockHUD)
            {
                return ConfigurationManager.GetInstance().LockHUD;
            }

            bool isHidden = Config.HideOutsideOfCombat && !IsInCombat();

            if ( !isHidden && element is JobHud _jobHud)
            {
                return Config.HideOnlyJobPackHudOutsideOfCombat && !IsInCombat();
            }

            if (element != null)
            {
                if (element.GetType() == typeof(PlayerCastbarHud))
                {
                    return false;
                }

                if (element.GetConfig().GetType() == typeof(PlayerUnitFrameConfig))
                {
                    PlayerCharacter player = Plugin.ClientState.LocalPlayer;
                    Debug.Assert(player != null, "HudHelper.LocalPlayer is NULL.");
                    isHidden = isHidden && player.CurrentHp == player.MaxHp;
                }
            }

            return isHidden;
        }

        // executed on login and on job swap
        public void ApplyCurrentConfig()
        {
            ConfigureDefaultCastBar();
            ConfigureDefaultJobGauge();
            ConfigureCombatActionBars();
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
                    addon.DefaultVisibilityToggle(Config.HideDefaultCastbar);
                }
            });
        }

        public unsafe void ConfigureDefaultJobGauge()
        {
            ToggleDefaultComponent(delegate (GUIAddon addon)
            {
                if (addon.name.StartsWith("JobHud"))
                {
                    if (Config.HideDefaultJobGauges)
                    {
                        addon.addonPtr->IsVisible = false;
                    }
                    else
                    {
                        addon.addonPtr->IsVisible = true;
                        addon.addonPtr->UldManager.UpdateDrawNodeList();
                    }
                }
            });
        }

        #region Helpers

        private unsafe void ToggleDefaultComponent(Action<GUIAddon> action)
        {
            var stage = AtkStage.GetSingleton();
            Debug.Assert(stage != null, "stage == null");
            var loadedUnitsList = &stage->RaptureAtkUnitManager->AtkUnitManager.AllLoadedUnitsList;
            Debug.Assert(loadedUnitsList != null, "loadedUnitsList == null");
            var addonList = &loadedUnitsList->AtkUnitEntries;
            Debug.Assert(addonList != null, "addonList == null");

            for (var i = 0; i < loadedUnitsList->Count; i++)
            {
                AtkUnitBase* addon = addonList[i];
                Debug.Assert(addon != null, "addon == null");
                var name = Marshal.PtrToStringAnsi(new IntPtr(addon->Name));
                if (name == null)
                {
                    continue;
                }

                action?.Invoke(new GUIAddon() { addonPtr = addon, name = name });
            }
        }

        private bool IsInCombat()
        {
            return Plugin.ClientState.Condition[ConditionFlag.InCombat];
        }

        private bool IsInDuty()
        {
            return Plugin.ClientState.Condition[ConditionFlag.BoundByDuty];
        }

        private Dictionary<string, string> _hotbarMap => new Dictionary<string, string>()
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
            _hotbarMap.TryGetValue(hotbarUserFriendlyName, out var result);
            return result ?? hotbarUserFriendlyName;
        }

        #endregion
    }
}
