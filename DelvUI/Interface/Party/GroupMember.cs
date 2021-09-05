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
                if (actor == null)
                {
                    return new StatusEffect[0];
                }

                return actor.StatusEffects;
            }
        }

        private Actor Actor = null;
        private BattleChara* BattleCharacter => (BattleChara*)GetActor().Address;


        public GroupMember(PartyMember* partyMember, DalamudPluginInterface pluginInterface)
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

        public FakeGroupMember()
        {
            Level = (uint)FakeGroupMember.RNG.Next(1, 80);
            JobId = (uint)FakeGroupMember.RNG.Next(19, 38);
            MaxHP = (uint)FakeGroupMember.RNG.Next(90000, 150000);
            HP = (uint)(MaxHP * FakeGroupMember.RNG.Next(50, 100) / 100f);
            MaxMP = 10000;
            MP = (uint)(MaxMP * FakeGroupMember.RNG.Next(100) / 100f);
            Shield = FakeGroupMember.RNG.Next(30) / 100f;

            var statusEffectCount = FakeGroupMember.RNG.Next(1, 5);
            StatusEffects = new StatusEffect[statusEffectCount];

            for (int i = 0; i < statusEffectCount; i++)
            {
                var fakeEffect = new StatusEffect();
                fakeEffect.Duration = FakeGroupMember.RNG.Next(1, 30);
                fakeEffect.EffectId = (short)FakeGroupMember.RNG.Next(1, 200);
                fakeEffect.StackCount = (byte)FakeGroupMember.RNG.Next(0, 3);

                StatusEffects[i] = fakeEffect;
            }
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
