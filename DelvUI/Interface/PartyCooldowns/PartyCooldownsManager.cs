using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.Party;
using ImGuiNET;
using System;
using System.Collections.Generic;

namespace DelvUI.Interface.PartyCooldowns
{
    public class PartyCooldownsManager
    {
        #region Singleton
        public static PartyCooldownsManager Instance { get; private set; } = null!;
        private PartyCooldownsConfig _config = null!;
        private PartyCooldownsDataConfig _dataConfig = null!;

        private PartyCooldownsManager()
        {
            IntPtr funcPtr = Plugin.SigScanner.ScanText("4C 89 44 24 18 53 56 57 41 54 41 57 48 81 EC ?? 00 00 00 8B F9");
            OnActionUsedHook = new Hook<OnActionUsedDelegate>(funcPtr, OnActionUsed);
            OnActionUsedHook.Enable();

            PartyManager.Instance.MembersChangedEvent += OnMembersChanged;
            ConfigurationManager.Instance.ResetEvent += OnConfigReset;
            Plugin.JobChangedEvent += OnJobChanged;
            Plugin.ClientState.TerritoryChanged += OnTerritoryChanged;

            OnConfigReset(ConfigurationManager.Instance);
            UpdatePreview();

            OnMembersChanged(PartyManager.Instance);
        }

        public static void Initialize()
        {
            Instance = new PartyCooldownsManager();
        }

        ~PartyCooldownsManager()
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

            OnActionUsedHook.Disable();
            OnActionUsedHook.Dispose();

            PartyManager.Instance.MembersChangedEvent -= OnMembersChanged;
            Plugin.JobChangedEvent -= OnJobChanged;
            _config.ValueChangeEvent -= OnConfigPropertyChanged;
            _dataConfig.CooldownsDataEnabledChangedEvent -= OnCooldownEnabledChanged;
            Plugin.ClientState.TerritoryChanged -= OnTerritoryChanged;

            Instance = null!;
        }

        private void OnConfigReset(ConfigurationManager sender)
        {
            if (_config != null)
            {
                _config.ValueChangeEvent -= OnConfigPropertyChanged;
            }

            _config = sender.GetConfigObject<PartyCooldownsConfig>();
            _config.ValueChangeEvent += OnConfigPropertyChanged;

            if (_dataConfig != null)
            {
                _dataConfig.CooldownsDataEnabledChangedEvent -= OnCooldownEnabledChanged;
            }

            _dataConfig = sender.GetConfigObject<PartyCooldownsDataConfig>();
            _dataConfig.CooldownsDataEnabledChangedEvent += OnCooldownEnabledChanged;
            _dataConfig.UpdateDataIfNeeded();
        }

        #endregion Singleton

        private delegate void OnActionUsedDelegate(int characterId, IntPtr characterAddress, IntPtr position, IntPtr effect, IntPtr unk1, IntPtr unk2);
        private Hook<OnActionUsedDelegate> OnActionUsedHook;

        private Dictionary<uint, Dictionary<uint, PartyCooldown>>? _oldMap;
        private Dictionary<uint, Dictionary<uint, PartyCooldown>> _cooldownsMap = new Dictionary<uint, Dictionary<uint, PartyCooldown>>();
        public IReadOnlyDictionary<uint, Dictionary<uint, PartyCooldown>> CooldownsMap => _cooldownsMap;

        public delegate void PartyCooldownsChangedEventHandler(PartyCooldownsManager sender);
        public event PartyCooldownsChangedEventHandler? CooldownsChangedEvent;

        private bool _wasInDuty = false;

        private unsafe void OnActionUsed(int characterId, IntPtr characterAddress, IntPtr position, IntPtr effect, IntPtr unk1, IntPtr unk2)
        {
            uint actorId = (uint)characterId;
            bool isAction = *((byte*)effect.ToPointer() + 0x1F) == 1;

            // check if its an action
            if (isAction)
            {
                // check if its a member in the party
                if (!_cooldownsMap.ContainsKey(actorId))
                {
                    // check if its a party member's pet
                    GameObject? actor = Plugin.ObjectTable.SearchById(actorId);

                    if (actor is BattleNpc battleNpc && _cooldownsMap.ContainsKey(battleNpc.OwnerId))
                    {
                        actorId = battleNpc.OwnerId;
                    }
                    else
                    {
                        actorId = 0;
                    }
                }

                // if its a valid actor
                if (actorId > 0)
                {
                    uint actionID = *((uint*)effect.ToPointer() + 0x2);

                    // check if its an action we track
                    if (_cooldownsMap[actorId].TryGetValue(actionID, out PartyCooldown? cooldown) && cooldown != null)
                    {
                        cooldown.LastTimeUsed = ImGui.GetTime() + 1;
                    }
                }
            }

            OnActionUsedHook.Original(characterId, characterAddress, position, effect, unk1, unk2);
        }

        private void OnMembersChanged(PartyManager sender)
        {
            if (sender.Previewing || _config.Preview) { return; }

            _cooldownsMap.Clear();

            if (_config.ShowOnlyInDuties && !Plugin.Condition[ConditionFlag.BoundByDuty])
            {
                CooldownsChangedEvent?.Invoke(this);
                return;
            }

            // show when solo
            if (sender.IsSoloParty() || sender.MemberCount == 0)
            {
                var player = Plugin.ClientState.LocalPlayer;
                if (_config.ShowWhenSolo && player != null)
                {
                    _cooldownsMap.Add(player.ObjectId, CooldownsForMember(player.ObjectId, player.ClassJob.Id, player.Level, null));
                }
            }
            else if (!_config.ShowOnlyInDuties || Plugin.Condition[ConditionFlag.BoundByDuty])
            {
                // add new members
                foreach (PartyFramesMember member in sender.GroupMembers)
                {
                    if (member.ObjectId > 0)
                    {
                        _cooldownsMap.Add(member.ObjectId, CooldownsForMember(member));
                    }
                }
            }

            CooldownsChangedEvent?.Invoke(this);
        }

        private Dictionary<uint, PartyCooldown> CooldownsForMember(PartyFramesMember member)
        {
            return CooldownsForMember(member.ObjectId, member.JobId, member.Level, member);
        }

        private Dictionary<uint, PartyCooldown> CooldownsForMember(uint objectId, uint jobId, uint level, PartyFramesMember? member)
        {
            Dictionary<uint, PartyCooldown> cooldowns = new Dictionary<uint, PartyCooldown>();

            foreach (PartyCooldownData data in _dataConfig.Cooldowns)
            {
                if (data.Enabled && level >= data.RequiredLevel && data.IsUsableBy(jobId))
                {
                    cooldowns.Add(data.ActionId, new PartyCooldown(data, objectId, member));
                }
            }

            return cooldowns;
        }

        #region events
        private void OnConfigPropertyChanged(object sender, OnChangeBaseArgs args)
        {
            if (args.PropertyName == "Preview")
            {
                UpdatePreview();
            }
            else if (args.PropertyName == "ShowWhenSolo" && PartyManager.Instance?.MemberCount == 0)
            {
                OnMembersChanged(PartyManager.Instance);
            }
            else if (args.PropertyName == "ShowOnlyInDuties" && PartyManager.Instance != null)
            {
                OnMembersChanged(PartyManager.Instance);
            }
        }

        private void OnJobChanged(uint jobId)
        {
            OnMembersChanged(PartyManager.Instance);
        }

        private void OnCooldownEnabledChanged(PartyCooldownsDataConfig config)
        {
            OnMembersChanged(PartyManager.Instance);
        }

        private void OnTerritoryChanged(object? sender, ushort territoryId)
        {
            bool isInDuty = Plugin.Condition[ConditionFlag.BoundByDuty];
            if (_config.ShowOnlyInDuties && _wasInDuty != isInDuty)
            {
                OnMembersChanged(PartyManager.Instance);
            }

            _wasInDuty = isInDuty;
        }

        public void UpdatePreview()
        {
            if (!_config.Preview)
            {
                if (_oldMap != null)
                {
                    _cooldownsMap = _oldMap;
                }
                else
                {
                    _cooldownsMap.Clear();
                }

                if (PartyManager.Instance.Previewing)
                {
                    CooldownsChangedEvent?.Invoke(this);
                }
                else
                {
                    OnMembersChanged(PartyManager.Instance);
                }
                return;
            }

            if (PartyManager.Instance?.Previewing == false)
            {
                _oldMap = _cooldownsMap;
            }

            _cooldownsMap.Clear();
            Random RNG = new Random((int)ImGui.GetTime());

            for (uint i = 1; i < 9; i++)
            {
                Dictionary<uint, PartyCooldown> cooldowns = new Dictionary<uint, PartyCooldown>();

                JobRoles role = i < 3 ? JobRoles.Tank : (i < 5 ? JobRoles.Healer : JobRoles.Unknown);
                role = role == JobRoles.Unknown ? JobRoles.DPSMelee + RNG.Next(3) : role;
                int jobCount = JobsHelper.JobsByRole[role].Count;
                int jobIndex = RNG.Next(jobCount);
                uint jobId = JobsHelper.JobsByRole[role][jobIndex];

                _cooldownsMap.Add(i, CooldownsForMember(i, jobId, 90, null));

                foreach (PartyCooldown cooldown in _cooldownsMap[i].Values)
                {
                    int rng = RNG.Next(100);
                    if (rng > 80)
                    {
                        cooldown.LastTimeUsed = ImGui.GetTime() - 30;
                    }
                    else if (rng > 50)
                    {
                        cooldown.LastTimeUsed = ImGui.GetTime() + 1;
                    }
                }
            }

            CooldownsChangedEvent?.Invoke(this);
        }
        #endregion
    }
}
