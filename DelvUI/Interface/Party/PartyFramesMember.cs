using Dalamud.Game.ClientState.Structs;
using Dalamud.Plugin;
using DelvUI.Helpers;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using ImGuiNET;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Actor = Dalamud.Game.ClientState.Actors.Types.Actor;
using PartyMember = FFXIVClientStructs.FFXIV.Client.Game.Group.PartyMember;


namespace DelvUI.Interface.Party
{
    public unsafe class PartyFramesMember : IPartyFramesMember
    {
        protected PartyMember* _partyMember = null;

        public int ActorID => _partyMember != null ? (int)_partyMember->ObjectID : _actorID;
        protected string _name;
        public string Name => _name == null ? "???" : _name;
        public uint Level => _partyMember != null ? _partyMember->Level : BattleCharacter->Character.Level;
        public uint JobId => _partyMember != null ? _partyMember->ClassJob : BattleCharacter->Character.ClassJob;
        public uint HP => _partyMember != null ? _partyMember->CurrentHP : BattleCharacter->Character.Health;
        public uint MaxHP => _partyMember != null ? _partyMember->MaxHP : BattleCharacter->Character.MaxHealth;
        public uint MP => _partyMember != null ? _partyMember->CurrentMP : BattleCharacter->Character.Mana;
        public uint MaxMP => _partyMember != null ? _partyMember->MaxMP : BattleCharacter->Character.MaxMana;
        public float Shield => Utils.ActorShieldValue(GetActor());
        public StatusEffect[] StatusEffects
        {
            get
            {
                var actor = GetActor();
                if (actor == null)
                {
                    return new StatusEffect[0];
                }

                return actor.StatusEffects;
            }
        }

        private int _actorID;
        private BattleChara* BattleCharacter => (BattleChara*)GetActor().Address;


        public PartyFramesMember(PartyMember* partyMember)
        {
            _partyMember = partyMember;

            // name
            byte[] nameBytes = new byte[64];
            Marshal.Copy((IntPtr)partyMember->Name, nameBytes, 0, 64);
            var text = System.Text.Encoding.Default.GetString(nameBytes);
            if (text != null)
            {
                _name = Regex.Replace(text, "[^a-zA-Z0-9_. ]+", "", RegexOptions.Compiled);
            }
        }

        public PartyFramesMember(Actor actor)
        {
            _actorID = actor.ActorId;
            _name = actor.Name;
        }

        public Actor GetActor()
        {
            return Plugin.ClientState.Actors.FirstOrDefault(o => o.ActorId == ActorID);
        }
    }

    public class FakePartyFramesMember : IPartyFramesMember
    {
        private static Random RNG = new Random((int)ImGui.GetTime());

        public int ActorID => -1;
        public string Name => "Fake Name";

        public uint Level { get; private set; }
        public uint JobId { get; private set; }
        public uint HP { get; private set; }
        public uint MaxHP { get; private set; }
        public uint MP { get; private set; }
        public uint MaxMP { get; private set; }
        public float Shield { get; private set; }
        public StatusEffect[] StatusEffects { get; private set; }

        public FakePartyFramesMember()
        {
            Level = (uint)RNG.Next(1, 80);
            JobId = (uint)RNG.Next(19, 38);
            MaxHP = (uint)RNG.Next(90000, 150000);
            HP = (uint)(MaxHP * RNG.Next(50, 100) / 100f);
            MaxMP = 10000;
            MP = (uint)(MaxMP * RNG.Next(100) / 100f);
            Shield = RNG.Next(30) / 100f;

            var statusEffectCount = RNG.Next(1, 5);
            StatusEffects = new StatusEffect[statusEffectCount];

            for (int i = 0; i < statusEffectCount; i++)
            {
                var fakeEffect = new StatusEffect();
                fakeEffect.Duration = RNG.Next(1, 30);
                fakeEffect.EffectId = (short)RNG.Next(1, 200);
                fakeEffect.StackCount = (byte)RNG.Next(0, 3);

                StatusEffects[i] = fakeEffect;
            }
        }

        public Actor GetActor()
        {
            return null;
        }
    }

    public interface IPartyFramesMember
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
