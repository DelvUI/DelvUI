using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Ipc;
using System;

namespace DelvUI.Helpers
{
    internal class PetRenamerHelper
    {
        private ICallGateSubscriber<nint, string>? GetPetNicknameNint;
        private ICallGateSubscriber<object>? PetNicknameReady;
        private ICallGateSubscriber<object>? PetNicknameDispose;
        private ICallGateSubscriber<bool>? Enabled;

        private bool pluginEnabled = false;

        #region Singleton
        public static void Initialize() { Instance = new PetRenamerHelper(); }

        public static PetRenamerHelper Instance { get; private set; } = null!;

        public PetRenamerHelper()
        {
            AssignIPCs();
            AssignFunctions();
            CheckPluginEnabled();
        }

        ~PetRenamerHelper()
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

            PetNicknameReady?.Unsubscribe(OnPetReady);
            PetNicknameDispose?.Unsubscribe(OnPetDispose);

            Instance = null!;
        }
        #endregion

        private void AssignIPCs()
        {
            GetPetNicknameNint = Plugin.PluginInterface.GetIpcSubscriber<nint, string>("PetRenamer.GetPetNicknameNint");
            PetNicknameReady = Plugin.PluginInterface.GetIpcSubscriber<object>("PetRenamer.Ready");
            PetNicknameDispose = Plugin.PluginInterface.GetIpcSubscriber<object>("PetRenamer.Disposing");
            Enabled = Plugin.PluginInterface.GetIpcSubscriber<bool>("PetRenamer.Enabled");
        }

        private void AssignFunctions()
        {
            PetNicknameReady?.Subscribe(OnPetReady);
            PetNicknameDispose?.Subscribe(OnPetDispose);
        }

        private void CheckPluginEnabled()
        {
            try
            {
                pluginEnabled = Enabled?.InvokeFunc() ?? false;
            }
            catch { }
        }

        private void OnPetReady()
        {
            pluginEnabled = true;
        }

        private void OnPetDispose()
        {
            pluginEnabled = false;
        }

        private string GetPetNamesForCharacter(nint character)
        {
            try
            {
                return GetPetNicknameNint?.InvokeFunc(character) ?? null!;
            }
            catch { }

            return null!;
        }

        public string? GetPetName(IGameObject? actor)
        {
            if (!pluginEnabled)
            {
                return null;
            }

            if (actor == null || (actor.ObjectKind != ObjectKind.Companion && actor.ObjectKind != ObjectKind.BattleNpc))
            {
                return null;
            }

            return GetPetNamesForCharacter(actor.Address);
        }
    }
}
