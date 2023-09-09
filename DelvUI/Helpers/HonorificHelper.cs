using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Logging;
using Dalamud.Plugin.Ipc;
using Newtonsoft.Json;

namespace DelvUI.Helpers
{

    internal class TitleData
    {
        public string title = string.Empty;
        public bool isPrefix = false;
    }

    internal class HonorificHelper
    {

        private static ICallGateSubscriber<Character, string>? GetCharacterTitle;

        public static void Initialize()
        {
            GetCharacterTitle = Plugin.PluginInterface.GetIpcSubscriber<Character, string>("Honorific.GetCharacterTitle");
        }

        internal static string GetTitleForCharater(Character character)
        {
            return GetCharacterTitle?.InvokeFunc(character) ?? string.Empty;
        }

        public static TitleData? GetTitle(GameObject actor)
        {
            if (actor == null || (actor.ObjectKind != ObjectKind.Player))
            {
                return null;
            }
            string jsonData = GetTitleForCharater((Character)actor);
            TitleData? titleData = JsonConvert.DeserializeObject<TitleData>(jsonData ?? string.Empty);
            if (titleData != null)
            {
                return titleData;
            }
            return null;
        }
    }

}
