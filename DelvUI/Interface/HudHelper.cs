using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.SubKinds;
using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DelvUI.Interface
{
    public class HudHelper : IDisposable
    {
        private HUDOptionsConfig Config => ConfigurationManager.Instance.GetConfigObject<HUDOptionsConfig>();

        private readonly string[] _hotbarAddonNames = { "_ActionBar", "_ActionBar01", "_ActionBar02", "_ActionBar03", "_ActionBar04", "_ActionBar05", "_ActionBar06", "_ActionBar07", "_ActionBar08", "_ActionBar09" };
        private bool? _previousHotbarCrossVisible = null;

        private bool _firstUpdate = true;

        private Vector2 _castBarPos = Vector2.Zero;
        private bool _hidingCastBar = false;
        private Vector2 _pullTimerPos = Vector2.Zero;
        private bool _hidingPullTimer = false;

        public HudHelper()
        {
            Config.ValueChangeEvent += ConfigValueChanged;
        }

        ~HudHelper()
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

            Config.ValueChangeEvent -= ConfigValueChanged;

            UpdateCombatActionBars();
            UpdateDefaultCastBar(true);
            UpdateDefaultPulltimer(true);
            UpdateJobGauges(true);
        }

        public void Update()
        {
            if (_firstUpdate)
            {
                _firstUpdate = false;
                UpdateDefaultCastBar();
                UpdateDefaultPulltimer();
            }

            UpdateJobGauges();
        }

        public bool IsElementHidden(HudElement element)
        {
            IHudElementWithVisibilityConfig? e = element as IHudElementWithVisibilityConfig;
            if (e == null || e.VisibilityConfig == null) { return false; }

            return !e.VisibilityConfig.IsElementVisible(element);
        }

        private void ConfigValueChanged(object sender, OnChangeBaseArgs e)
        {
            switch (e.PropertyName)
            {
                case "HideDefaultCastbar":
                    UpdateDefaultCastBar();
                    break;
                case "HideDefaultPulltimer":
                    UpdateDefaultPulltimer();
                    break;
            }
        }

        public void UpdateCombatActionBars()
        {
            if (Plugin.Condition[ConditionFlag.OccupiedInEvent] ||
                Plugin.Condition[ConditionFlag.OccupiedInQuestEvent] ||
                Plugin.Condition[ConditionFlag.OccupiedInCutSceneEvent] ||
                Plugin.Condition[ConditionFlag.OccupiedSummoningBell] ||
                Plugin.Condition[ConditionFlag.Occupied] ||
                Plugin.Condition[ConditionFlag.Occupied30] ||
                Plugin.Condition[ConditionFlag.Occupied33] ||
                Plugin.Condition[ConditionFlag.Occupied38] ||
                Plugin.Condition[ConditionFlag.Occupied39] ||
                Plugin.Condition[ConditionFlag.WatchingCutscene] ||
                Plugin.Condition[ConditionFlag.WatchingCutscene78] ||
                Plugin.Condition[ConditionFlag.CreatingCharacter] ||
                Plugin.Condition[ConditionFlag.BetweenAreas] ||
                Plugin.Condition[ConditionFlag.BetweenAreas51] ||
                Plugin.Condition[ConditionFlag.ChocoboRacing] ||
                Plugin.ClientState.IsPvP)
            {
                return;
            }

            HotbarsVisibilityConfig? config = ConfigurationManager.Instance?.GetConfigObject<HotbarsVisibilityConfig>();
            if (config == null) { return; }

            List<VisibilityConfig> hotbarConfigs = config.GetHotbarConfigs();
            for (int i = 0; i < hotbarConfigs.Count; i++)
            {
                if (hotbarConfigs[i].Enabled)
                {
                    SetHotbarVisible(i, hotbarConfigs[i].IsElementVisible());
                }
            }

            if (config.HotbarConfigCross.Enabled)
            {
                SetCrossHotbarVisible(config.HotbarConfigCross.IsElementVisible());
            }
        }

        private unsafe void SetHotbarVisible(int index, bool visible)
        {
            AtkUnitBase* addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName(_hotbarAddonNames[index], 1);
            if (addon == null || addon->IsVisible == visible) { return; }

            string numberText = (index + 1).ToString();
            string onOffText = visible ? "on" : "off";

            ChatHelper.SendChatMessage("/hotbar display " + numberText + " " + onOffText);
        }

        private unsafe void SetCrossHotbarVisible(bool visible)
        {
            if (_previousHotbarCrossVisible.HasValue && _previousHotbarCrossVisible.Value == visible) { return; }

            _previousHotbarCrossVisible = visible;

            string onOffText = visible ? "on" : "off";
            ChatHelper.SendChatMessage("/crosshotbardisplay" + " " + onOffText);
        }

        private unsafe void UpdateDefaultCastBar(bool forceVisible = false)
        {
            if (Config.HideDefaultCastbar && !_hidingCastBar)
            {
                Plugin.AddonLifecycle.RegisterListener(AddonEvent.PreDraw, "_CastBar", (addonEvent, args) =>
                {
                    AtkUnitBase* addon = (AtkUnitBase*)args.Addon;

                    if (!_hidingCastBar)
                    {
                        _castBarPos = new Vector2(addon->RootNode->GetXFloat(), addon->RootNode->GetYFloat());
                    }

                    addon->RootNode->SetPositionFloat(-9999.0f, -9999.0f);
                });

                _hidingCastBar = true;
            }
            else if ((forceVisible || !Config.HideDefaultCastbar) && _hidingCastBar)
            {
                Plugin.AddonLifecycle.UnregisterListener(AddonEvent.PreDraw, "_CastBar");

                AtkUnitBase* addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("_CastBar", 1);
                if (addon != null)
                {
                    addon->RootNode->SetPositionFloat(_castBarPos.X, _castBarPos.Y);
                }

                _hidingCastBar = false;
            }

            return;
        }

        private unsafe void UpdateDefaultPulltimer(bool forceVisible = false)
        {
            if (Config.HideDefaultPulltimer && !_hidingPullTimer)
            {
                Plugin.AddonLifecycle.RegisterListener(AddonEvent.PreDraw, "ScreenInfo_CountDown", (addonEvent, args) =>
                {
                    AtkUnitBase* addon = (AtkUnitBase*)args.Addon;

                    if (!_hidingPullTimer)
                    {
                        _pullTimerPos = new Vector2(addon->RootNode->GetXFloat(), addon->RootNode->GetYFloat());
                    }

                    addon->RootNode->SetPositionFloat(-9999.0f, -9999.0f);
                });

                AtkUnitBase* addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("ScreenInfo_CountDown", 1);
                if (addon != null)
                {
                    _pullTimerPos = new Vector2(addon->RootNode->GetXFloat(), addon->RootNode->GetYFloat());
                    addon->RootNode->SetPositionFloat(-9999, -9999);
                }

                _hidingPullTimer = true;
            }
            else if ((forceVisible || !Config.HideDefaultPulltimer) && _hidingPullTimer)
            {
                Plugin.AddonLifecycle.UnregisterListener(AddonEvent.PreDraw, "ScreenInfo_CountDown");

                AtkUnitBase* addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("ScreenInfo_CountDown", 1);
                if (addon != null)
                {
                    addon->RootNode->SetPositionFloat(_pullTimerPos.X, _pullTimerPos.Y);
                }

                _hidingPullTimer = false;
            }

            return;
        }

        private static Dictionary<string, string> _specialCases = new()
        {
            ["JobHudPCT0"] = "JobHudRPM0",
            ["JobHudPCT1"] = "JobHudRPM1",

            ["JobHudNIN1"] = "JobHudNIN1v70",
            
            ["JobHudVPR0"] = "JobHudRDB0",
            ["JobHudVPR1"] = "JobHudRDB1"
        };

        private unsafe void UpdateJobGauges(bool forceVisible = false)
        {
            IPlayerCharacter? player = Plugin.ClientState.LocalPlayer;
            if (player == null) { return; }

            string jobName = JobsHelper.JobNames[player.ClassJob.Id];
            int i = 0;
            bool stop = false;

            do
            {
                string addonName = $"JobHud{jobName}{i}";
                if (_specialCases.TryGetValue(addonName, out string? name) && name != null)
                {
                    addonName = name;
                }

                AtkUnitBase* addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName(addonName, 1);
                if (addon == null)
                {
                    stop = true;
                }
                else
                {
                    addon->IsVisible = forceVisible || !Config.HideDefaultJobGauges;
                }

                i++;
            } while (!stop);
        }
    }

    internal class StrataLevelComparer<TKey> : IComparer<TKey> where TKey : PluginConfigObject
    {
        public int Compare(TKey? a, TKey? b)
        {
            MovablePluginConfigObject? configA = a is MovablePluginConfigObject ? a as MovablePluginConfigObject : null;
            MovablePluginConfigObject? configB = b is MovablePluginConfigObject ? b as MovablePluginConfigObject : null;

            if (configA == null && configB == null) { return 0; }
            if (configA == null && configB != null) { return -1; }
            if (configA != null && configB == null) { return 1; }

            if (configA!.StrataLevel == configB!.StrataLevel)
            {
                return configA.ID.CompareTo(configB.ID);
            }

            if (configA.StrataLevel < configB.StrataLevel)
            {
                return -1;
            }

            return 1;
        }
    }
}
