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


namespace DelvUI.Interface.Party
{
    public unsafe class GroupMember : IGroupMember
    {
        private DalamudPluginInterface pluginInterface = null;
        protected PartyMember* partyMember = null;

        public int ActorID => (int)partyMember->ObjectID;
        protected string _name;
        public string Name => _name == null ? "???" : _name;
        public uint Level => partyMember->Level;
        public uint JobId => partyMember->ClassJob;
        public uint HP => partyMember->CurrentHP;
        public uint MaxHP => partyMember->MaxHP;
        public uint MP => partyMember->CurrentMP;
        public uint MaxMP => partyMember->MaxMP;
        public float Shield => Utils.ActorShieldValue(GetActor());
        
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

        public Actor GetActor()
        {
            return pluginInterface.ClientState.Actors.FirstOrDefault(o => o.ActorId == ActorID);
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

        public FakeGroupMember()
        {
            _level = (uint)FakeGroupMember.RNG.Next(1, 80);
            _jobId = (uint)FakeGroupMember.RNG.Next(19, 38);
            _maxHP = (uint)FakeGroupMember.RNG.Next(90000, 150000);
            _hp = (uint)(_maxHP * FakeGroupMember.RNG.Next(100) / 100f);
            _maxMP = 10000;
            _mp = (uint)(_maxMP * FakeGroupMember.RNG.Next(100) / 100f);
            _shield = FakeGroupMember.RNG.Next(100) / 100f;
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

        public abstract Actor GetActor();
    }
}
