using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Logging;
using DelvUI.Config;
using DelvUI.Helpers;
//using FFXIVClientStructs.FFXIV.Client.Game.Group;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace DelvUI.Interface.Party
{
    public delegate void PartyMembersChangedEventHandler(PartyManager sender);

    public unsafe class PartyManager : IDisposable
    {
        #region Singleton
        public static PartyManager Instance { get; private set; } = null!;
        private PartyFramesConfig _config;

        private IntPtr _hudAgent = IntPtr.Zero;

        private PartyManager(PartyFramesConfig config)
        {
            Plugin.Framework.Update += FrameworkOnOnUpdateEvent;

            _config = config;
            _config.ValueChangeEvent += OnConfigPropertyChanged;

            UpdatePreview();
        }

        public static void Initialize()
        {
            var config = ConfigurationManager.Instance.GetConfigObject<PartyFramesConfig>();
            Instance = new PartyManager(config);
        }

        ~PartyManager()
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

            Plugin.Framework.Update -= FrameworkOnOnUpdateEvent;
            _config.ValueChangeEvent -= OnConfigPropertyChanged;

            Instance = null!;
        }


        #endregion Singleton

        private const int PartyListInfoOffset = 0x0B50;
        private const int PartyListMemberRawInfoSize = 0x18;
        private List<PartyListMemberInfo> _partyMembersInfo = null!;
        private bool _playerOrderChanged = false;

        private List<IPartyFramesMember> _groupMembers = new List<IPartyFramesMember>();
        public IReadOnlyCollection<IPartyFramesMember> GroupMembers => _groupMembers.AsReadOnly();
        public uint MemberCount => (uint)_groupMembers.Count;

        public event PartyMembersChangedEventHandler? MembersChangedEvent;


        private void FrameworkOnOnUpdateEvent(Framework framework)
        {
            // find party list hud agent
            if (_hudAgent == IntPtr.Zero)
            {
                var addon = Plugin.GameGui.GetAddonByName("_PartyList", 1);
                _hudAgent = Plugin.GameGui.FindAgentInterface(addon);

                PluginLog.Log($"_PartyList Hud Angent found at: 0x{_hudAgent.ToInt64():X16}");
            }

            // no need to update on preview mode
            if (_config.Preview)
            {
                return;
            }

            var player = Plugin.ClientState.LocalPlayer;
            if (player is null || player is not PlayerCharacter)
            {
                return;
            }

            // solo
            var memberCount = Plugin.PartyList.Length;
            if (_config.ShowWhenSolo && memberCount == 0)
            {
                UpdateSoloParty(player);
                return;
            }

            // party
            try
            {
                bool partyChanged = _playerOrderChanged || _partyMembersInfo == null || _groupMembers.Count != memberCount;

                // get data from default party list 
                if (_hudAgent != IntPtr.Zero)
                {
                    List<PartyListMemberInfo> newInfo = new List<PartyListMemberInfo>(8);

                    for (int i = 0; i < 8; i++)
                    {
                        PartyListMemberRawInfo* info = (PartyListMemberRawInfo*)(_hudAgent + (PartyListInfoOffset + PartyListMemberRawInfoSize * i));
                        newInfo.Add(new PartyListMemberInfo(info));
                    }

                    if (!partyChanged && _partyMembersInfo != null)
                    {
                        partyChanged = !newInfo.SequenceEqual(_partyMembersInfo);
                    }
                    _partyMembersInfo = newInfo;
                }

                // if party is the same, just update actor references
                if (!partyChanged)
                {
                    foreach (var member in _groupMembers)
                    {
                        member.Update();
                    }

                    return;
                }

                PluginLog.Log("Setup party");

                // create new members array with dalamud's data
                _groupMembers.Clear();

                for (int i = 0; i < memberCount; i++)
                {
                    var partyMember = Plugin.PartyList[i];
                    if (partyMember == null)
                    {
                        continue;
                    }

                    var isPlayer = partyMember.ObjectId == player.ObjectId;

                    // player order override
                    int order;
                    if (isPlayer && _config.PlayerOrderOverrideEnabled)
                    {
                        order = _config.PlayerOrder + 1;
                    }
                    else
                    {
                        order = IndexForPartyMember(partyMember) ?? 9;
                    }

                    var member = new PartyFramesMember(partyMember, order);
                    _groupMembers.Add(member);

                    // player's chocobo (always last)
                    if (_config.ShowChocobo && member.ObjectId == player.ObjectId)
                    {
                        var companion = Utils.GetBattleChocobo(player);
                        if (companion is Character companionCharacter)
                        {
                            _groupMembers.Add(new PartyFramesMember(companionCharacter, 10));
                        }
                    }
                }

                // sort according to default party list
                SortGroupMembers(player);
                _playerOrderChanged = false;

                // fire event
                MembersChangedEvent?.Invoke(this);
            }
            catch (Exception e)
            {
                PluginLog.LogError("ERROR getting party data: " + e.Message);
            }
        }

        private int? IndexForPartyMember(PartyMember member)
        {
            if (_partyMembersInfo == null || _partyMembersInfo.Count == 0)
            {
                return null;
            }

            var name = member.Name.ToString();
            return _partyMembersInfo.FindIndex(o => o.ObjectId == member.ObjectId || o.Name == name) + 1;
        }

        public void OnPlayerOrderChange()
        {
            _playerOrderChanged = true;
        }

        private void SortGroupMembers(PlayerCharacter player)
        {
            _groupMembers.Sort((a, b) =>
            {
                if (a.Order == b.Order)
                {
                    if (a.ObjectId == player.ObjectId)
                    {
                        return 1;
                    }
                    else if (b.ObjectId == player.ObjectId)
                    {
                        return -1;
                    }

                    return a.Name.CompareTo(b.Name);
                }

                if (a.Order < b.Order)
                {
                    return -1;
                }

                return 1;
            });
        }

        private void UpdateSoloParty(PlayerCharacter player)
        {
            List<IPartyFramesMember> newMembers = new List<IPartyFramesMember>();

            newMembers.Add(new PartyFramesMember(player, 1));

            if (_config.ShowChocobo)
            {
                var companion = Utils.GetBattleChocobo(player);
                if (companion is Character companionCharacter)
                {
                    newMembers.Add(new PartyFramesMember(companionCharacter, 2));
                }
            }

            if (newMembers.Count != _groupMembers.Count)
            {
                _groupMembers = newMembers;
                MembersChangedEvent?.Invoke(this);
            }
        }

        private void OnConfigPropertyChanged(object sender, OnChangeBaseArgs args)
        {
            if (args.PropertyName == "Preview")
            {
                UpdatePreview();
            }
            else if (args.PropertyName == "PlayerOrder")
            {
                OnPlayerOrderChange();
            }
        }

        private void UpdatePreview()
        {
            if (!_config.Preview)
            {
                _groupMembers.Clear();
                return;
            }

            // fill list with fake members for UI testing
            _groupMembers.Clear();

            if (_config.Preview)
            {
                for (int i = 0; i < 8; i++)
                {
                    _groupMembers.Add(new FakePartyFramesMember(i));
                }
            }

            MembersChangedEvent?.Invoke(this);
        }

        #region raw party info
        internal unsafe class PartyListMemberInfo : IEquatable<PartyListMemberInfo>
        {
            public readonly string Name;
            public readonly uint ObjectId;
            public readonly byte Type;

            public PartyListMemberInfo(PartyListMemberRawInfo* info)
            {
                Name = Marshal.PtrToStringAnsi(new IntPtr(info->NamePtr)) ?? "";
                ObjectId = info->ObjectId;
                Type = info->Type;
            }

            public bool Equals(PartyListMemberInfo? other)
            {
                return ObjectId == other?.ObjectId && Name == other?.Name;
            }
        }

        [StructLayout(LayoutKind.Explicit, Size = 24)]
        public unsafe struct PartyListMemberRawInfo
        {
            [FieldOffset(0x00)] public byte* NamePtr;
            [FieldOffset(0x08)] public long ContentId;
            [FieldOffset(0x10)] public uint ObjectId;

            // some kind of type
            // 1 = player
            // 2 = party member?
            // 3 = unknown
            // 4 = chocobo
            // 5 = summon?
            [FieldOffset(0x14)] public byte Type;

            public string Name => Marshal.PtrToStringAnsi(new IntPtr(NamePtr)) ?? "";
        }
        #endregion
    }
}
