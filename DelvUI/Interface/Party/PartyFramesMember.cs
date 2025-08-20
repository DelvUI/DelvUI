using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using DelvUI.Helpers;
using Dalamud.Bindings.ImGui;
using System;
using FFXIVClientStructs.FFXIV.Client.UI.Info;

namespace DelvUI.Interface.Party
{
    public enum EnmityLevel : byte
    {
        Leader = 1,
        Second = 2,
        Last = 255
    }

    public enum PartyMemberStatus : byte
    {
        None,
        ViewingCutscene,
        Offline,
        Dead
    }

    public unsafe class PartyFramesMember : IPartyFramesMember
    {
        protected IPartyMember? _partyMember = null;
        private string _name = "";
        private uint _jobId = 0;
        private uint _objectID = 0;

        public uint ObjectId => _partyMember != null ? _partyMember.ObjectId : _objectID;
        public ICharacter? Character { get; private set; }
        public CrossRealmMember? CrossCharacter { get; private set; }

        public int Index { get; set; }
        public int Order { get; set; }
        public string Name => _partyMember != null ? _partyMember.Name.ToString() : (Character != null ? Character.Name.ToString() : _name);
        public uint Level => _partyMember != null ? _partyMember.Level : (Character != null ? Character.Level : (uint)0);
        public uint JobId => _partyMember != null ? _partyMember.ClassJob.RowId : (Character != null ? Character.ClassJob.RowId : _jobId);
        public uint HP => _partyMember != null ? _partyMember.CurrentHP : (Character != null ? Character.CurrentHp : (uint)0);
        public uint MaxHP => _partyMember != null ? _partyMember.MaxHP : (Character != null ? Character.MaxHp : (uint)0);
        public uint MP => _partyMember != null ? _partyMember.CurrentMP : JobsHelper.CurrentPrimaryResource(Character);
        public uint MaxMP => _partyMember != null ? _partyMember.MaxMP : JobsHelper.MaxPrimaryResource(Character);
        public float Shield => Utils.ActorShieldValue(Character);
        public EnmityLevel EnmityLevel { get; private set; } = EnmityLevel.Last;
        public PartyMemberStatus Status { get; private set; } = PartyMemberStatus.None;
        public ReadyCheckStatus ReadyCheckStatus { get; private set; } = ReadyCheckStatus.None;
        public bool IsPartyLeader { get; private set; } = false;
        public bool IsChocobo { get; private set; } = false;
        public float? RaiseTime { get; set; }
        public InvulnStatus? InvulnStatus { get; set; }
        public bool HasDispellableDebuff { get; set; } = false;
        public WhosTalkingState WhosTalkingState => WhosTalkingHelper.Instance?.GetUserState(Name) ?? WhosTalkingState.None;

        public PartyFramesMember(IPartyMember partyMember, int index, int order, EnmityLevel enmityLevel, PartyMemberStatus status, ReadyCheckStatus readyCheckStatus, bool isPartyLeader, bool isChocobo = false)
        {
            _partyMember = partyMember;
            Index = index;
            Order = order;
            EnmityLevel = enmityLevel;
            Status = status;
            ReadyCheckStatus = readyCheckStatus;
            IsPartyLeader = isPartyLeader;
            IsChocobo = isChocobo;

            var gameObject = partyMember.GameObject;
            if (gameObject is ICharacter character)
            {
                Character = character;
            }
        }

        public PartyFramesMember(ICharacter character, int index, int order, EnmityLevel enmityLevel, PartyMemberStatus status, ReadyCheckStatus readyCheckStatus, bool isPartyLeader, bool isChocobo = false)
        {
            Index = index;
            Order = order;
            EnmityLevel = enmityLevel;
            Status = status;
            ReadyCheckStatus = readyCheckStatus;
            IsPartyLeader = isPartyLeader;
            IsChocobo = isChocobo;

            _objectID = (uint)character.GameObjectId;
            Character = character;
        }

        public PartyFramesMember(uint objectId, int index, int order, EnmityLevel enmityLevel, PartyMemberStatus status, ReadyCheckStatus readyCheckStatus, bool isPartyLeader, bool isChocobo = false)
        {
            Index = index;
            Order = order;
            EnmityLevel = enmityLevel;
            Status = status;
            ReadyCheckStatus = readyCheckStatus;
            IsPartyLeader = isPartyLeader;
            IsChocobo = isChocobo;

            _objectID = objectId;
            var gameObject = Plugin.ObjectTable.SearchById(ObjectId);
            Character = gameObject is ICharacter ? (ICharacter)gameObject : null;
        }

        public PartyFramesMember(CrossRealmMember member, int index, int order, PartyMemberStatus status, ReadyCheckStatus readyCheckStatus, bool isPartyLeader, bool isChocobo = false)
        {
            Index = index;
            Order = order;
            Status = status;
            ReadyCheckStatus = readyCheckStatus;
            IsPartyLeader = isPartyLeader;
            IsChocobo = isChocobo;

            _objectID = (uint)member.EntityId;
            CrossCharacter = member;
            _name = member.NameString;
            _jobId = member.ClassJobId;
        }


        public void Update(EnmityLevel enmityLevel, PartyMemberStatus status, ReadyCheckStatus readyCheckStatus, bool isPartyLeader, uint jobId = 0)
        {
            EnmityLevel = enmityLevel;
            Status = status;
            ReadyCheckStatus = readyCheckStatus;
            IsPartyLeader = isPartyLeader;

            if (ObjectId == 0)
            {
                Character = null;
                return;
            }

            var gameObject = Plugin.ObjectTable.SearchById(ObjectId);
            Character = gameObject is ICharacter ? (ICharacter)gameObject : null;

            if (jobId > 0)
            {
                _jobId = jobId;
            } 
            else if (Character != null)
            {
                _jobId = Character.ClassJob.RowId;
            }

            if (status == PartyMemberStatus.None && Character != null && MaxHP > 0 && HP <= 0)
            {
                Status = PartyMemberStatus.Dead;
            }
        }
    }

    public class FakePartyFramesMember : IPartyFramesMember
    {
        public static readonly Random RNG = new Random((int)ImGui.GetTime());

        public uint ObjectId => 0xE0000000;
        public ICharacter? Character => null;

        public int Index { get; set; }
        public int Order { get; set; }
        public string Name { get; private set; }
        public uint Level { get; private set; }
        public uint JobId { get; private set; }
        public uint HP { get; private set; }
        public uint MaxHP { get; private set; }
        public uint MP { get; private set; }
        public uint MaxMP { get; private set; }
        public float Shield { get; private set; }
        public EnmityLevel EnmityLevel { get; private set; }
        public PartyMemberStatus Status { get; private set; }
        public ReadyCheckStatus ReadyCheckStatus { get; private set; }
        public bool IsPartyLeader { get; }
        public bool IsChocobo { get; }
        public float? RaiseTime { get; set; }
        public InvulnStatus? InvulnStatus { get; set; }
        public bool HasDispellableDebuff { get; set; }
        public WhosTalkingState WhosTalkingState { get; set; }

        public FakePartyFramesMember(int order)
        {
            Name = RNG.Next(0, 2) == 1 ? "Fake Name" : "FakeLonger MockedName";
            Index = order;
            Order = order + 1;
            Level = (uint)RNG.Next(1, 80);
            JobId = (uint)RNG.Next(19, 41);
            MaxHP = (uint)RNG.Next(90000, 150000);
            HP = order == 2 || order == 3 ? 0 : (uint)(MaxHP * RNG.Next(50, 100) / 100f);
            MaxMP = 10000;
            MP = order == 2 || order == 3 ? 0 : (uint)(MaxMP * RNG.Next(100) / 100f);
            Shield = order == 2 || order == 3 ? 0 : RNG.Next(30) / 100f;
            EnmityLevel = order <= 1 ? (EnmityLevel)order + 1 : EnmityLevel.Last;
            Status = order < 3 ? PartyMemberStatus.None : (order == 3 ? PartyMemberStatus.Dead : (PartyMemberStatus)RNG.Next(0, 3));
            ReadyCheckStatus = (ReadyCheckStatus)RNG.Next(0, 3);
            IsPartyLeader = order == 0;
            IsChocobo = RNG.Next(0, 8) == 1;
            HasDispellableDebuff = RNG.Next(0, 2) == 1;
            RaiseTime = order == 2 ? RNG.Next(0, 60) : null;
            InvulnStatus = order == 0 ? new InvulnStatus(3077, RNG.Next(0, 10), 810) : null;
            WhosTalkingState = (WhosTalkingState)RNG.Next(0, 4);
        }

        public void Update(EnmityLevel enmityLevel, PartyMemberStatus status, ReadyCheckStatus readyCheckStatus, bool isPartyLeader, uint jobId = 0)
        {

        }
    }

    public interface IPartyFramesMember
    {
        public uint ObjectId { get; }
        public ICharacter? Character { get; }

        public int Index { get; }
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
        public PartyMemberStatus Status { get; }
        public ReadyCheckStatus ReadyCheckStatus { get; }
        public bool IsPartyLeader { get; }
        public bool IsChocobo { get; }
        public float? RaiseTime { get; set; }
        public InvulnStatus? InvulnStatus { get; set; }
        public bool HasDispellableDebuff { get; set; }

        public WhosTalkingState WhosTalkingState { get; }

        public void Update(EnmityLevel enmityLevel, PartyMemberStatus status, ReadyCheckStatus readyCheckStatus, bool isPartyLeader, uint jobId = 0);
    }
}
