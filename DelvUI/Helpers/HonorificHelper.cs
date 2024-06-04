using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Logging;
using Dalamud.Plugin.Ipc;
using Newtonsoft.Json;
using Lumina.Data.Parsing.Uld;
using System;
using System.Numerics;

namespace DelvUI.Helpers
{

    public class TitleData
    {
        public string Title = "";
        public bool IsPrefix = false;
        public bool IsOriginal = false;
        public Vector3? Color = new Vector3(0f, 0f, 0f);
        public Vector3? Glow = new Vector3(0f, 0f, 0f);
    }

    internal class HonorificHelper
    {
        private ICallGateSubscriber<Character, string>? _getCharacterTitle;

        #region Singleton
        private HonorificHelper()
        {
            _getCharacterTitle = Plugin.PluginInterface.GetIpcSubscriber<Character, string>("Honorific.GetCharacterTitle");
        }

        public static void Initialize() { Instance = new HonorificHelper(); }

        public static HonorificHelper Instance { get; private set; } = null!;

        ~HonorificHelper()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            Instance = null!;
        }
        #endregion

        public TitleData? GetTitle(GameObject? actor)
        {
            if (_getCharacterTitle == null || actor == null || actor.ObjectKind != ObjectKind.Player || actor is not Character character)
            {
                return null;
            }

            try
            {
                string jsonData = _getCharacterTitle.InvokeFunc(character);
                TitleData? titleData = JsonConvert.DeserializeObject<TitleData>(jsonData ?? string.Empty);
                return titleData;
            }
            catch { }

            return null;
        }
    }
}
