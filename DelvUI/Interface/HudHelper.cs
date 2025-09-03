using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.SubKinds;
using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Utility.Signatures;
using DelvUI.Enums;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace DelvUI.Interface
{
    public class HudHelper : IDisposable
    {
        private HUDOptionsConfig Config => ConfigurationManager.Instance.GetConfigObject<HUDOptionsConfig>();

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

            if (Plugin.Framework.IsInFrameworkUpdateThread && Plugin.ClientState.LocalPlayer != null)
            {
                UpdateCombatActionBars();
                UpdateDefaultCastBar(true);
                UpdateDefaultPulltimer(true);
                UpdateJobGauges(true);
            }
            else
            {
                try
                {
                    Plugin.Framework.RunOnFrameworkThread(() =>
                    {
                        if (Plugin.ClientState.LocalPlayer == null)
                        {
                            return;
                        }

                        UpdateCombatActionBars();
                        UpdateDefaultCastBar(true);
                        UpdateDefaultPulltimer(true);
                        UpdateJobGauges(true);
                    });
                }
                catch(Exception ex)
                {
                    Plugin.Logger.Error($"Exception during HudHelper.Dispose: {ex}");
                }
            }
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
            if(Plugin.Condition.Any(
                   ConditionFlag.OccupiedInEvent,
                   ConditionFlag.OccupiedInQuestEvent,
                   ConditionFlag.OccupiedInCutSceneEvent,
                   ConditionFlag.OccupiedSummoningBell,
                   ConditionFlag.Occupied,
                   ConditionFlag.Occupied30,
                   ConditionFlag.Occupied33,
                   ConditionFlag.Occupied38,
                   ConditionFlag.Occupied39,
                   ConditionFlag.WatchingCutscene,
                   ConditionFlag.WatchingCutscene78,
                   ConditionFlag.CreatingCharacter,
                   ConditionFlag.BetweenAreas,
                   ConditionFlag.BetweenAreas51,
                   ConditionFlag.BoundByDuty95,
                   ConditionFlag.ChocoboRacing,
                   ConditionFlag.PlayingLordOfVerminion)
                   || Plugin.ClientState.IsPvP
               )
            {
                return;
            }

            HotbarsVisibilityConfig? config = ConfigurationManager.Instance?.GetConfigObject<HotbarsVisibilityConfig>();
            if (config == null) { return; }

            List<VisibilityConfig> hotbarConfigs = config.GetHotbarConfigs();
            SetHotbarsVisibility(hotbarConfigs, config.HotbarConfigCross);
        }

        private unsafe void SetHotbarsVisibility(List<VisibilityConfig> hotbarConfigs, VisibilityConfig crossHotbarConfig)
        {
            AddonConfig* config = AddonConfig.Instance();
            Span<AddonConfigEntry> entries = config->ActiveDataSet->HudLayoutConfigEntries;
            bool hasChanges = false;

            var hashToConfig = new Dictionary<uint, VisibilityConfig>();
            for (int i = 0; i < hotbarConfigs.Count; i++)
            {
                if (hotbarConfigs[i].Enabled)
                {
                    uint hash = (uint)ElementKindHelper.ElementKindByHotBarId(i);
                    hashToConfig[hash] = hotbarConfigs[i];
                }
            }

            if (crossHotbarConfig.Enabled)
            {
                uint crossHash = (uint)ElementKind.CrossHotbar;
                hashToConfig[crossHash] = crossHotbarConfig;
            }

            foreach (ref var entry in entries)
            {
                if (hashToConfig.TryGetValue(entry.AddonNameHash, out var configVal))
                {
                    bool isVisible = configVal.IsElementVisible();
                    byte shouldBeVisible = 1;
                    if (entry.AddonNameHash == (uint)ElementKind.CrossHotbar)
                    {
                        shouldBeVisible = (byte)(isVisible ? 0x2 : 0x0);
                    }
                    else
                    {
                        shouldBeVisible = (byte)(isVisible ? 0x1 : 0x0);
                    }

                    if(entry.ByteValue2 == shouldBeVisible)
                    {
                        continue;
                    }
                    entry.ByteValue2 = shouldBeVisible;
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                config->SaveFile(true);
                config->ApplyHudLayout();
            }
        }

        private unsafe void UpdateDefaultCastBar(bool forceVisible = false)
        {
            if (Config.HideDefaultCastbar && !_hidingCastBar)
            {
                Plugin.AddonLifecycle.RegisterListener(AddonEvent.PreDraw, "_CastBar", (addonEvent, args) =>
                {
                    AtkUnitBase* addon = (AtkUnitBase*)args.Addon.Address;

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

                AtkUnitBase* addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("_CastBar", 1).Address;
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
                    AtkUnitBase* addon = (AtkUnitBase*)args.Addon.Address;

                    if (!_hidingPullTimer)
                    {
                        _pullTimerPos = new Vector2(addon->RootNode->GetXFloat(), addon->RootNode->GetYFloat());
                    }

                    addon->RootNode->SetPositionFloat(-9999.0f, -9999.0f);
                });

                AtkUnitBase* addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("ScreenInfo_CountDown", 1).Address;
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

                AtkUnitBase* addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("ScreenInfo_CountDown", 1).Address;
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

            string jobName = JobsHelper.JobNames[player.ClassJob.RowId];
            int i = 0;
            bool stop = false;

            do
            {
                string addonName = $"JobHud{jobName}{i}";
                if (_specialCases.TryGetValue(addonName, out string? name) && name != null)
                {
                    addonName = name;
                }

                AtkUnitBase* addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName(addonName, 1).Address;
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