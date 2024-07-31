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
            try
            {
                _onActionUsedHook = Plugin.GameInteropProvider.HookFromSignature<OnActionUsedDelegate>(
                    "40 ?? 56 57 41 ?? 41 ?? 41 ?? 48 ?? ?? ?? ?? ?? ?? ?? 48",
                    OnActionUsed
                );
                _onActionUsedHook?.Enable();
            }
            catch
            {
                Plugin.Logger.Error("PartyCooldowns OnActionUsed Hook failed!!!");
            }

            try
            {
                _actorControlHook = Plugin.GameInteropProvider.HookFromSignature<ActorControlDelegate>(
                    "E8 ?? ?? ?? ?? 0F B7 0B 83 E9 64",
                    OnActorControl
                );
                _actorControlHook?.Enable();
            }
            catch
            {
                Plugin.Logger.Error("PartyCooldowns OnActorControl Hook failed!!!");
            }

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

            _onActionUsedHook?.Disable();
            _onActionUsedHook?.Dispose();

            _actorControlHook?.Disable();
            _actorControlHook?.Dispose();

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

            ForcedUpdate();
        }

        #endregion Singleton

        private delegate void OnActionUsedDelegate(int characterId, IntPtr characterAddress, IntPtr position, IntPtr effect, IntPtr unk1, IntPtr unk2);
        private Hook<OnActionUsedDelegate>? _onActionUsedHook;

        private delegate void ActorControlDelegate(uint entityId, uint id, uint unk1, uint type, uint unk2, uint unk3, uint unk4, uint unk5, UInt64 targetId, byte unk6);
        private Hook<ActorControlDelegate>? _actorControlHook;

        private Dictionary<uint, Dictionary<uint, PartyCooldown>>? _oldMap;
        private Dictionary<uint, Dictionary<uint, PartyCooldown>> _cooldownsMap = new Dictionary<uint, Dictionary<uint, PartyCooldown>>();
        public IReadOnlyDictionary<uint, Dictionary<uint, PartyCooldown>> CooldownsMap => _cooldownsMap;

        private Dictionary<uint, double> _technicalStepMap = new Dictionary<uint, double>();

        public delegate void PartyCooldownsChangedEventHandler(PartyCooldownsManager sender);
        public event PartyCooldownsChangedEventHandler? CooldownsChangedEvent;

        private bool _wasInDuty = false;

        private void OnActorControl(uint entityId, uint id, uint unk1, uint type, uint unk2, uint unk3, uint unk4, uint unk5, UInt64 targetId, byte unk6)
        {
            _actorControlHook?.Original(entityId, id, unk1, type, unk2, unk3, unk4, unk5, targetId, unk6);

            // detect wipe fadeouts (not 100% reliable but good enough)
            if (type == 0x4000000F)
            {
                ResetCooldowns();
            }
        }

        private void ResetCooldowns()
        {
            foreach (uint actorId in _cooldownsMap.Keys)
            {
                foreach (PartyCooldown cooldown in _cooldownsMap[actorId].Values)
                {
                    cooldown.LastTimeUsed = 0;
                }
            }
        }

        private unsafe void OnActionUsed(int characterId, IntPtr characterAddress, IntPtr position, IntPtr effect, IntPtr unk1, IntPtr unk2)
        {
            _onActionUsedHook?.Original(characterId, characterAddress, position, effect, unk1, unk2);

            uint actorId = (uint)characterId;
            bool isAction = *((byte*)effect.ToPointer() + 0x1F) == 1;

            // check if its an action
            if (isAction)
            {
                // check if its a member in the party
                if (!_cooldownsMap.ContainsKey(actorId))
                {
                    // check if its a party member's pet
                    IGameObject? actor = Plugin.ObjectTable.SearchById(actorId);

                    if (actor is IBattleNpc battleNpc && _cooldownsMap.ContainsKey(battleNpc.OwnerId))
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

                    // special case for starry muse > set id to scenic muse
                    if (actionID == 34675)
                    {
                        actionID = 35349;
                    }
                    // special case for mug > set id to dokumori
                    if (actionID == 2248)
                    {
                        actionID = 36957;
                    }

                    // special case for technical step / finish
                    // we detect when technical step is pressed and save the time
                    // so we can properly calculate the cooldown once finish is pressed
                    if (actionID == 16193 || actionID == 16194 || actionID == 16195 || actionID == 16196)
                    {
                        actionID = 16004;
                    }

                    if (actionID == 15998)
                    {
                        _technicalStepMap[actorId] = ImGui.GetTime();
                    }
                    else
                    {
                        // check if its an action we track
                        if (_cooldownsMap[actorId].TryGetValue(actionID, out PartyCooldown? cooldown) && cooldown != null)
                        {
                            // if its technical finish, we set the cooldown start time to
                            // the time when step was pressed
                            if (_technicalStepMap.TryGetValue(actorId, out double stepStartTime) && actionID == 16004)
                            {
                                cooldown.OverridenCooldownStartTime = stepStartTime;
                                _technicalStepMap.Remove(actorId);
                            }

                            cooldown.LastTimeUsed = ImGui.GetTime();
                        }
                    }
                }
            }
        }

        public void ForcedUpdate()
        {
            OnMembersChanged(PartyManager.Instance);
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
                    _cooldownsMap.Add((uint)player.GameObjectId, CooldownsForMember((uint)player.GameObjectId, player.ClassJob.Id, player.Level, null));
                }
            }
            else if (!_config.ShowOnlyInDuties || Plugin.Condition[ConditionFlag.BoundByDuty])
            {
                // add new members
                foreach (IPartyFramesMember member in sender.GroupMembers)
                {
                    if (member.ObjectId > 0)
                    {
                        _cooldownsMap.Add(member.ObjectId, CooldownsForMember(member));
                    }
                }
            }

            CooldownsChangedEvent?.Invoke(this);
        }

        private Dictionary<uint, PartyCooldown> CooldownsForMember(IPartyFramesMember member)
        {
            return CooldownsForMember(member.ObjectId, member.JobId, member.Level, member);
        }

        private Dictionary<uint, PartyCooldown> CooldownsForMember(uint objectId, uint jobId, uint level, IPartyFramesMember? member)
        {
            Dictionary<uint, PartyCooldown> cooldowns = new Dictionary<uint, PartyCooldown>();

            foreach (PartyCooldownData data in _dataConfig.Cooldowns)
            {
                if (data.EnabledV2 != PartyCooldownEnabled.Disabled &&
                    level >= data.RequiredLevel &&
                    data.IsUsableBy(jobId) &&
                    !data.ExcludedJobIds.Contains(jobId))
                {
                    cooldowns.Add(data.ActionId, new PartyCooldown(data, objectId, level, member));
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
            ForcedUpdate();
        }

        private void OnCooldownEnabledChanged(PartyCooldownsDataConfig config)
        {
            ForcedUpdate();
        }

        private void OnTerritoryChanged(ushort territoryId)
        {
            bool isInDuty = Plugin.Condition[ConditionFlag.BoundByDuty];
            if (_config.ShowOnlyInDuties && _wasInDuty != isInDuty)
            {
                ForcedUpdate();
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
