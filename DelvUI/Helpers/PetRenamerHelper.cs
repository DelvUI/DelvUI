using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Ipc;
using Newtonsoft.Json;
using CSCompanion = FFXIVClientStructs.FFXIV.Client.Game.Character.Companion;
using CSGameObjectManager = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObjectManager;

namespace DelvUI.Helpers
{
    public static class PetRenamerHelper
    {
        private static ICallGateSubscriber<Character, string>? GetCharacterNickname;

        public static void Initialize()
        {
            GetCharacterNickname = Plugin.PluginInterface.GetIpcSubscriber<Character, string>("PetRenamer.GetCharacterNickname");
        }

        internal static string GetPetNamesForCharacter(Character character) => GetCharacterNickname?.InvokeFunc(character) ?? string.Empty;
        internal static NicknameData? FromString(string? str) => JsonConvert.DeserializeObject<NicknameData>(str ?? string.Empty);

        public static unsafe string? GetPetName(GameObject? actor)
        {
            if (actor == null || (actor.ObjectKind != ObjectKind.Companion && actor.ObjectKind != ObjectKind.BattleNpc))
            {
                return null;
            }

            // For companions it doesn't work that way due to a missing Dalamud feature.
            // Most Dalamud stuff does NOT work with unnetworked gameObjects so this workaround gets the owner ID of a companion.
            int ownerID = (int)actor.OwnerId;
            if (actor?.ObjectKind == ObjectKind.Companion)
            {
                CSCompanion* gObj = (CSCompanion*)CSGameObjectManager.GetGameObjectByIndex(((Character)actor).ObjectIndex);
                if (gObj == null)
                {
                    return null;
                }
                ownerID = (int)gObj->Character.CompanionOwnerID;
            }

            // We get the Dalamud gameObject of the owner
            GameObject? dalamudObj = Plugin.ObjectTable.SearchById((ulong)ownerID);
            if (dalamudObj == null)
            {
                return null;
            }

            // We get the petnames via IPC endpoints
            // And convert that json data to usable data
            string jsonData = GetPetNamesForCharacter((Character)dalamudObj);
            NicknameData? nicknameData = FromString(jsonData);
            if (nicknameData == null)
            {
                return null;
            }

            // If the object is a BattleNPC and the nickname is valid, apply it!
            if (actor?.ObjectKind == ObjectKind.BattleNpc && nicknameData.BatteValid())
            {
                return nicknameData.BattleNickname;
            }
            // If the object is a Companion and the nickname is valid, apply it!
            else if (actor?.ObjectKind == ObjectKind.Companion && nicknameData.CompanionValid())
            {
                return nicknameData.Nickname;
            }

            return null;
        }
    }

    internal class NicknameData
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
