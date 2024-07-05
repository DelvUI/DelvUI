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
        private Dictionary<string, Vector2> _jobGaugePos = new();
        private bool _hidingJobGauge = false;
        private Vector2 _pullTimerPos = Vector2.Zero;
        private bool _hidingPullTimer = false;

        private uint _previousJob = 0;
        private double _jobChangeTime = 0;
        private bool _jobChangeUpdated = false;

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
            UpdateCombatActionBars();

            if (_firstUpdate)
            {
                _firstUpdate = false;
                UpdateDefaultCastBar();
                UpdateDefaultPulltimer();
                UpdateJobGauges();
            }
            else
            {
                // detect job change
                IPlayerCharacter? player = Plugin.ClientState.LocalPlayer;
                if (player != null)
                {
                    uint job = player.ClassJob.Id;
                    double now = ImGui.GetTime();

                    if (job != _previousJob)
                    {
                        _previousJob = job;
                        _jobChangeUpdated = false;
                        _jobChangeTime = now;
                        ResetJobGauges();
                    }
                    else if (!_jobChangeUpdated && now - _jobChangeTime > 0.1f)
                    { 
                        _hidingJobGauge = false;
                        ResetJobGauges();
                        UpdateJobGauges();
                    }
                }
            }
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
                case "HideDefaultJobGauges":
                    UpdateJobGauges();
                    break;
            }
        }

        private void UpdateCombatActionBars()
        {
            if (Plugin.Condition[ConditionFlag.OccupiedInEvent] ||
                Plugin.Condition[ConditionFlag.OccupiedInQuestEvent] ||
                Plugin.Condition[ConditionFlag.ChocoboRacing] ||
                Plugin.Condition[ConditionFlag.Occupied] ||
                Plugin.Condition[ConditionFlag.Occupied30] ||
                Plugin.Condition[ConditionFlag.Occupied33] ||
                Plugin.Condition[ConditionFlag.Occupied38] ||
                Plugin.Condition[ConditionFlag.Occupied39] ||
                Plugin.Condition[ConditionFlag.OccupiedInCutSceneEvent] ||
                Plugin.Condition[ConditionFlag.OccupiedInEvent] ||
                Plugin.Condition[ConditionFlag.OccupiedInQuestEvent] ||
                Plugin.Condition[ConditionFlag.WatchingCutscene] ||
                Plugin.Condition[ConditionFlag.WatchingCutscene78] ||
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

        private unsafe void UpdateJobGauges(bool forceVisible = false)
        {
            IPlayerCharacter? player = Plugin.ClientState.LocalPlayer;
            if (player == null) { return; }

            string jobName = JobsHelper.JobNames[player.ClassJob.Id];

            // special case for pictomancer
            if (jobName == "PCT")
            {
                jobName = "RPM";
            }

            if (Config.HideDefaultJobGauges && !_hidingJobGauge)
            {
                int i = 0;
                bool stop = false;

                do
                {
                    string addonName = $"JobHud{jobName}{i}";

                    AtkUnitBase* addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName(addonName, 1);
                    if (addon == null)
                    {
                        stop = true;
                    }
                    else
                    {
                        _jobGaugePos[addonName] = new Vector2(addon->RootNode->GetXFloat(), addon->RootNode->GetYFloat());
                        addon->RootNode->SetPositionFloat(-9999, -9999);

                        Plugin.AddonLifecycle.RegisterListener(AddonEvent.PreDraw, addonName, (addonEvent, args) =>
                        {
                            AtkUnitBase* addon = (AtkUnitBase*)args.Addon;
                            addon->RootNode->SetPositionFloat(-9999.0f, -9999.0f);
                        });
                    }

                    i++;
                } while (!stop);

                _hidingJobGauge = true;
            }
            else if ((forceVisible || !Config.HideDefaultJobGauges) && _hidingJobGauge)
            {
                ResetJobGauges();
                _hidingJobGauge = false;
            }
        }

        private unsafe void ResetJobGauges()
        {
            foreach (KeyValuePair<string, Vector2> entry in _jobGaugePos)
            {
                Plugin.AddonLifecycle.UnregisterListener(AddonEvent.PreDraw, entry.Key);

                AtkUnitBase* addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName(entry.Key, 1);
                if (addon != null)
                {
                    addon->RootNode->SetPositionFloat(entry.Value.X, entry.Value.Y);
                }
            }
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
