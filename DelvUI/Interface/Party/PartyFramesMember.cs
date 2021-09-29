using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using DelvUI.Helpers;
using ImGuiNET;
using System;

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

        private uint _objectID = 0;
        public uint ObjectId => _partyMember != null ? _partyMember.ObjectId : _objectID;
        public Character? Character { get; private set; }

        public int Order { get; private set; }
        public string Name => _partyMember != null ? _partyMember.Name.ToString() : Character!.Name.ToString();
        public uint Level => _partyMember != null ? _partyMember.Level : Character!.Level;
        public uint JobId => _partyMember != null ? _partyMember.ClassJob.Id : Character!.ClassJob.Id;
        public uint HP => _partyMember != null ? _partyMember.CurrentHP : Character!.CurrentHp;
        public uint MaxHP => _partyMember != null ? _partyMember.MaxHP : Character!.MaxHp;
        public uint MP => _partyMember != null ? _partyMember.CurrentMP : JobsHelper.CurrentPrimaryResource(Character!);
        public uint MaxMP => _partyMember != null ? _partyMember.MaxMP : JobsHelper.MaxPrimaryResource(Character!);
        public float Shield => Utils.ActorShieldValue(Character);
        public EnmityLevel EnmityLevel { get; private set; }

        public PartyFramesMember(PartyMember partyMember, int order, EnmityLevel enmityLevel)
        {
            _partyMember = partyMember;
            Order = order;
            EnmityLevel = enmityLevel;

            var gameObject = partyMember.GameObject;
            if (gameObject is Character character)
            {
                Character = character;
            }
        }

        public PartyFramesMember(Character character, int order, EnmityLevel enmityLevel)
        {
            Order = order;
            EnmityLevel = enmityLevel;

            _objectID = character.ObjectId;
            Character = character;
        }

        public void Update(EnmityLevel enmityLevel)
        {
            EnmityLevel = enmityLevel;

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

        public FakePartyFramesMember(int order)
        {
            Order = order;
            Level = (uint)RNG.Next(1, 80);
            JobId = (uint)RNG.Next(19, 38);
            MaxHP = (uint)RNG.Next(90000, 150000);
            HP = (uint)(MaxHP * RNG.Next(50, 100) / 100f);
            MaxMP = 10000;
            MP = (uint)(MaxMP * RNG.Next(100) / 100f);
            Shield = RNG.Next(30) / 100f;

            byte value = (byte)RNG.Next(1, 3);
            EnmityLevel = value == 3 ? EnmityLevel.Last : (EnmityLevel)value;
        }

        public void Update(EnmityLevel enmityLevel)
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

        public void Update(EnmityLevel enmityLevel);
    }
}
