﻿using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using DelvUI.Helpers;
using ImGuiNET;
using System;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace DelvUI.Interface.Party
{
    public enum EnmityLevel : byte
    {
        Leader = 1,
        Second = 2,
        Last = 255
    }
    
    public unsafe class PartyFramesMember : IPartyFramesMember
    {
        protected PartyMember? _partyMember = null;
        private string _name = "";
        private uint _jobId = 0;
        private uint _objectID = 0;

        public uint ObjectId => _partyMember != null ? _partyMember.ObjectId : _objectID;
        public Character? Character { get; private set; }

        public int Order { get; private set; }
        public string Name => _partyMember != null ? _partyMember.Name.ToString() : (Character != null ? Character.Name.ToString() : _name);
        public uint Level => _partyMember != null ? _partyMember.Level : (Character != null ? Character.Level : (uint)0);
        public uint JobId => _partyMember != null ? _partyMember.ClassJob.Id : (Character != null ? Character.ClassJob.Id : _jobId);
        public uint HP => _partyMember != null ? _partyMember.CurrentHP : (Character != null ? Character.CurrentHp : (uint)0);
        public uint MaxHP => _partyMember != null ? _partyMember.MaxHP : (Character != null ? Character.MaxHp : (uint)0);
        public uint MP => _partyMember != null ? _partyMember.CurrentMP : JobsHelper.CurrentPrimaryResource(Character);
        public uint MaxMP => _partyMember != null ? _partyMember.MaxMP : JobsHelper.MaxPrimaryResource(Character);
        public float Shield => Utils.ActorShieldValue(Character);
        public EnmityLevel EnmityLevel { get; private set; } = EnmityLevel.Last;
        public bool IsPartyLeader { get; private set; } = false;
        public float? RaiseTime { get; set; }
        public InvulnStatus? InvulnStatus { get; set; }
        public bool HasDispellableDebuff { get; set; } = false;

        public PartyFramesMember(PartyMember partyMember, int order, EnmityLevel enmityLevel, bool isPartyLeader)
        {
            _partyMember = partyMember;
            Order = order;
            EnmityLevel = enmityLevel;
            IsPartyLeader = isPartyLeader;

            var gameObject = partyMember.GameObject;
            if (gameObject is Character character)
            {
                Character = character;
            }
        }

        public PartyFramesMember(Character character, int order, EnmityLevel enmityLevel, bool isPartyLeader)
        {
            Order = order;
            EnmityLevel = enmityLevel;
            IsPartyLeader = isPartyLeader;

            _objectID = character.ObjectId;
            Character = character;
        }

        public PartyFramesMember(string? name, int order, uint jobId, bool isPartyLeader)
        {
            Order = order;
            IsPartyLeader = isPartyLeader;
            _name = name ?? "";
            _jobId = jobId;
        }

        public void Update(EnmityLevel enmityLevel, bool isPartyLeader, uint jobId)
        {
            EnmityLevel = enmityLevel;
            IsPartyLeader = isPartyLeader;
            _jobId = jobId;

            if (ObjectId == 0)
            {
                Character = null;
                return;
            }

            var gameObject = Plugin.ObjectTable.SearchById(ObjectId);
            Character = gameObject is Character ? (Character)gameObject : null;
        }
    }

    public class FakePartyFramesMember : IPartyFramesMember
    {
        private static readonly Random RNG = new Random((int)ImGui.GetTime());

        public uint ObjectId => GameObject.InvalidGameObjectId;
        public Character? Character => null;

        public int Order { get; private set; }
        public string Name => "Fake Name";
        public uint Level { get; private set; }
        public uint JobId { get; private set; }
        public uint HP { get; private set; }
        public uint MaxHP { get; private set; }
        public uint MP { get; private set; }
        public uint MaxMP { get; private set; }
        public float Shield { get; private set; }
        public EnmityLevel EnmityLevel { get; private set; }
        public bool IsPartyLeader { get; }
        public float? RaiseTime { get; set; }
        public InvulnStatus? InvulnStatus { get; set; }
        public bool HasDispellableDebuff { get; set; }

        public FakePartyFramesMember(int order, EnmityLevel enmityLevel, bool isPartyLeader)
        {
            Order = order + 1;
            Level = (uint)RNG.Next(1, 80);
            JobId = (uint)RNG.Next(19, 38);
            MaxHP = (uint)RNG.Next(90000, 150000);
            HP = (uint)(MaxHP * RNG.Next(50, 100) / 100f);
            MaxMP = 10000;
            MP = (uint)(MaxMP * RNG.Next(100) / 100f);
            Shield = RNG.Next(30) / 100f;
            EnmityLevel = enmityLevel;
            IsPartyLeader = isPartyLeader;
            HasDispellableDebuff = RNG.Next(0, 2) == 1;
        }

        public void Update(EnmityLevel enmityLevel, bool isPartyLeader, uint jobId)
        {

        }
    }

    public interface IPartyFramesMember
    {
        public uint ObjectId { get; }
        public Character? Character { get; }

        public int Order { get; }
        public string Name { get; }
        public uint Level { get; }
        public uint JobId { get; }
        public uint HP { get; }
        public uint MaxHP { get; }
        public uint MP { get; }
        public uint MaxMP { get; }
        public float Shield { get; }
        public EnmityLevel EnmityLevel { get; }
        public bool IsPartyLeader { get; }
        public float? RaiseTime { get; set; }
        public InvulnStatus? InvulnStatus { get; set; }
        public bool HasDispellableDebuff { get; set; }

        public void Update(EnmityLevel enmityLevel, bool isPartyLeader, uint jobId);
    }
}
