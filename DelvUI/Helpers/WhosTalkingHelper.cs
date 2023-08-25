using Dalamud.Game.ClientState.Party;
using Dalamud.Interface;
using Dalamud.Plugin.Ipc;
using DelvUI.Config;
using DelvUI.Config.Tree;
using DelvUI.Interface.Party;
using ImGuiScene;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using static System.Collections.Specialized.BitVector32;

namespace DelvUI.Helpers
{
    public enum WhosTalkingState : int
    {
        None = 0,
        Speaking = 1,
        Muted = 2,
        Deafened = 3
    }

    public class WhosTalkingHelper
    {
        private readonly ICallGateSubscriber<string, int> _getUserState;
        private Dictionary<string, WhosTalkingState> _cachedStates = new Dictionary<string, WhosTalkingState>();

        private TextureWrap? _speakingTexture = null;
        private TextureWrap? _mutedTexture = null;
        private TextureWrap? _deafenedTexture = null;

        #region Singleton
        private WhosTalkingHelper()
        {
            _getUserState = Plugin.PluginInterface.GetIpcSubscriber<string, int>("WT.GetUserState");

            try
            {
                string imagesPath = Path.Combine(Plugin.AssemblyLocation, "Media", "Images");

                // speaking
                string speakingPath = Path.Combine(imagesPath, "speaking.png");
                _speakingTexture = Plugin.UiBuilder.LoadImage(speakingPath);

                // muted
                string mutedPath = Path.Combine(imagesPath, "muted.png");
                _mutedTexture = Plugin.UiBuilder.LoadImage(mutedPath);
                
                // deafened
                string deafenedPath = Path.Combine(imagesPath, "deafened.png");
                _deafenedTexture = Plugin.UiBuilder.LoadImage(deafenedPath);
            }
            catch { }
        }

        public static void Initialize() { Instance = new WhosTalkingHelper(); }

        public static WhosTalkingHelper Instance { get; private set; } = null!;

        ~WhosTalkingHelper()
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

        public void Update()
        {
            _cachedStates.Clear();

            foreach (IPartyFramesMember member in PartyManager.Instance.GroupMembers)
            {
                if (member.Name.Length <= 0) { continue; }

                WhosTalkingState state = WhosTalkingState.None;

                try
                {
                    state = (WhosTalkingState)_getUserState.InvokeFunc(member.Name);
                }
                catch { }

                if (!_cachedStates.ContainsKey(member.Name))
                {
                    _cachedStates.Add(member.Name, state);
                }
            }
        }

        public WhosTalkingState GetUserState(string name)
        {
            if (_cachedStates.TryGetValue(name, out WhosTalkingState state))
            {
                return state;
            }

            return WhosTalkingState.None;
        }

        public TextureWrap? GetTextureForState(WhosTalkingState state)
        {
            switch (state)
            {
                case WhosTalkingState.Speaking: return _speakingTexture;
                case WhosTalkingState.Muted: return _mutedTexture;
                case WhosTalkingState.Deafened: return _deafenedTexture;
            }

            return null;
        }
    }
}
