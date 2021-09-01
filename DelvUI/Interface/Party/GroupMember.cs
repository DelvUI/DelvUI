using System;
using Dalamud.Plugin;
using Dalamud.Game.Internal;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.Game;
using PartyMember = FFXIVClientStructs.FFXIV.Client.Game.Group.PartyMember;
using Dalamud.Game.ClientState.Actors;
using Dalamud.Game.ClientState.Actors.Types;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Linq;
using DelvUI.Helpers;
using ImGuiNET;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Dalamud.Game.ClientState.Structs;
using Actor = Dalamud.Game.ClientState.Actors.Types.Actor;


namespace DelvUI.Interface.Party
{
    public unsafe class GroupMember : IGroupMember
    {
        private DalamudPluginInterface pluginInterface = null;
        protected PartyMember* partyMember = null;

        public int ActorID => partyMember != null ? (int)partyMember->ObjectID : Actor.ActorId;
        protected string _name;
        public string Name => _name == null ? "???" : _name;
        public uint Level => partyMember != null ? partyMember->Level : BattleCharacter->Character.Level;
        public uint JobId => partyMember != null ? partyMember->ClassJob : BattleCharacter->Character.ClassJob;
        public uint HP => partyMember != null ? partyMember->CurrentHP : BattleCharacter->Character.Health;
        public uint MaxHP => partyMember != null ? partyMember->MaxHP : BattleCharacter->Character.MaxHealth;
        public uint MP => partyMember != null ? partyMember->CurrentMP : BattleCharacter->Character.Mana;
        public uint MaxMP => partyMember != null ? partyMember->MaxMP : BattleCharacter->Character.MaxMana;
        public float Shield => Utils.ActorShieldValue(GetActor());
        public StatusEffect[] StatusEffects
        {
            get
            {
                var actor = GetActor();
                if (actor == null) return new StatusEffect[0];

                return actor.StatusEffects;
            }
        }

        private Actor Actor = null;
        private BattleChara *BattleCharacter => (BattleChara *)GetActor().Address;


        public GroupMember(PartyMember *partyMember, DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
            this.partyMember = partyMember;

            // name
            byte[] nameBytes = new byte[64];
            Marshal.Copy((IntPtr)partyMember->Name, nameBytes, 0, 64);
            var text = System.Text.Encoding.Default.GetString(nameBytes);
            if (text != null)
            {
                _name = Regex.Replace(text, "[^a-zA-Z0-9_. ]+", "", RegexOptions.Compiled);
            } 
        }

        public GroupMember(Actor actor, DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
            Actor = actor;
            _name = Actor.Name;
        }

        public Actor GetActor()
        {
            return Actor ?? pluginInterface.ClientState.Actors.FirstOrDefault(o => o.ActorId == ActorID);
        }
    }

    public class FakeGroupMember : IGroupMember
    {
        static Random RNG = new Random((int)ImGui.GetTime());

        public int ActorID => -1;
        public string Name => "Fake Name";

        private uint _level;
        public uint Level => _level;

        private uint _jobId;
        public uint JobId => _jobId;

        private uint _hp;
        public uint HP => _hp;

        private uint _maxHP;
        public uint MaxHP => _maxHP;

        private uint _mp;
        public uint MP => _mp;

        private uint _maxMP;
        public uint MaxMP => _maxMP;

        private float _shield;
        public float Shield => _shield;

        private StatusEffect _fakeEffect;
        public StatusEffect[] StatusEffects => new StatusEffect[] { _fakeEffect };

        public FakeGroupMember()
        {
            _level = (uint)FakeGroupMember.RNG.Next(1, 80);
            _jobId = (uint)FakeGroupMember.RNG.Next(19, 38);
            _maxHP = (uint)FakeGroupMember.RNG.Next(90000, 150000);
            _hp = (uint)(_maxHP * FakeGroupMember.RNG.Next(100) / 100f);
            _maxMP = 10000;
            _mp = (uint)(_maxMP * FakeGroupMember.RNG.Next(100) / 100f);
            _shield = FakeGroupMember.RNG.Next(100) / 100f;

            _fakeEffect = new StatusEffect();
            _fakeEffect.Duration = FakeGroupMember.RNG.Next(1, 30);
            _fakeEffect.EffectId = (short)FakeGroupMember.RNG.Next(1, 200);
            _fakeEffect.StackCount = (byte)FakeGroupMember.RNG.Next(0, 3);
        }
        public Actor GetActor()
        {
            return null;
        }
    }

    public interface IGroupMember
    {
        public int ActorID { get; }
        public string Name { get; }
        public uint Level { get; }
        public uint JobId { get; }
        public uint HP { get; }
        public uint MaxHP { get; }
        public uint MP { get; }
        public uint MaxMP { get; }
        public float Shield { get; }
        public StatusEffect[] StatusEffects { get; }

        public abstract Actor GetActor();
    }
}
