using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using DelvUI.Helpers;
using ImGuiNET;
using System;

namespace DelvUI.Interface.Party
{
    public unsafe class PartyFramesMember : IPartyFramesMember
    {
        protected PartyMember? _partyMember = null;

        public uint ObjectId => _partyMember != null ? _partyMember.ObjectId : Character!.ObjectId;
        public Character? Character { get; private set; }

        public string Name => _partyMember != null ? _partyMember.Name.ToString() : Character!.Name.ToString();
        public uint Level => _partyMember != null ? _partyMember.Level : Character!.Level;
        public uint JobId => _partyMember != null ? _partyMember.ClassJob.Id : Character!.ClassJob.Id;
        public uint HP => _partyMember != null ? _partyMember.CurrentHP : Character!.CurrentHp;
        public uint MaxHP => _partyMember != null ? _partyMember.MaxHP : Character!.MaxHp;
        public uint MP => _partyMember != null ? _partyMember.CurrentMP : JobsHelper.CurrentPrimaryResource(Character!);
        public uint MaxMP => _partyMember != null ? _partyMember.MaxMP : JobsHelper.MaxPrimaryResource(Character!);
        public float Shield => Utils.ActorShieldValue(Character);

        public PartyFramesMember(PartyMember partyMember)
        {
            _partyMember = partyMember;

            var gameObject = partyMember.GameObject;
            if (gameObject is Character character)
            {
                Character = character;
            }
        }

        public PartyFramesMember(Character character)
        {
            Character = character;
        }
    }

    public class FakePartyFramesMember : IPartyFramesMember
    {
        private static Random RNG = new Random((int)ImGui.GetTime());

        public uint ObjectId => GameObject.InvalidGameObjectId;
        public Character? Character => null;

        public string Name => "Fake Name";
        public uint Level { get; private set; }
        public uint JobId { get; private set; }
        public uint HP { get; private set; }
        public uint MaxHP { get; private set; }
        public uint MP { get; private set; }
        public uint MaxMP { get; private set; }
        public float Shield { get; private set; }

        public FakePartyFramesMember()
        {
            Level = (uint)RNG.Next(1, 80);
            JobId = (uint)RNG.Next(19, 38);
            MaxHP = (uint)RNG.Next(90000, 150000);
            HP = (uint)(MaxHP * RNG.Next(50, 100) / 100f);
            MaxMP = 10000;
            MP = (uint)(MaxMP * RNG.Next(100) / 100f);
            Shield = RNG.Next(30) / 100f;
        }
    }

    public interface IPartyFramesMember
    {
        public uint ObjectId { get; }
        public Character? Character { get; }

        public string Name { get; }
        public uint Level { get; }
        public uint JobId { get; }
        public uint HP { get; }
        public uint MaxHP { get; }
        public uint MP { get; }
        public uint MaxMP { get; }
        public float Shield { get; }
    }
}
