using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using DelvUI.Helpers.PetRenamer;
using Newtonsoft.Json;

namespace DelvUI.Helpers
{
    public static class PetRenamerHelper
    {
        private static ICallGateSubscriber<Character, string>? GetCharacterNickname;
        private const string GetCharacterNicknameIdentifier = "PetRenamer.GetCharacterNickname";

        public static void Initialize(DalamudPluginInterface dalamudPluginInterface)
        {
            GetCharacterNickname = dalamudPluginInterface?.GetIpcSubscriber<Character, string>(GetCharacterNicknameIdentifier);
        }

        public static string GetPetNamesForCharacter(Character character) => GetCharacterNickname?.InvokeFunc(character) ?? string.Empty;
        public static NicknameData? FromString(string? str) => JsonConvert.DeserializeObject<NicknameData>(str ?? string.Empty);
    }
}

namespace DelvUI.Helpers.PetRenamer
{
    public class NicknameData
    {
        public int ID = -1;
        public string? Nickname = string.Empty;
        public int BattleID = -1;
        public string? BattleNickname = string.Empty;

        public NicknameData() { }

        [JsonConstructor]
        public NicknameData(int ID, string? nickname, int BattleID, string? BattleNickname)
        {
            this.ID = ID;
            Nickname = nickname;
            this.BattleID = BattleID;
            this.BattleNickname = BattleNickname;
        }

        public new string ToString() => $"{ID}^{Nickname}^{BattleID}^{BattleNickname}";
        public string ToNormalString() => ToString().Replace("^", ",");

        public bool CompanionValid() => ID != -1 && Nickname != string.Empty;
        public bool BatteValid() => BattleID != -1 && BattleNickname != string.Empty;

        public bool Equals(NicknameData other) => ID == other.ID && Nickname == other.Nickname;
        public bool IDEquals(NicknameData other) => ID == other.ID;
    }
}
