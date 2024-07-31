using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;

namespace DelvUI.Helpers
{
    internal class PetRenamerHelper
    {
        private Dictionary<ulong, string>? PetNicknamesDictionary;

        #region Singleton
        public static void Initialize() { Instance = new PetRenamerHelper(); }

        public static PetRenamerHelper Instance { get; private set; } = null!;

        public PetRenamerHelper()
        {
            AssignShares();
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

            Plugin.PluginInterface.RelinquishData("PetRenamer.GameObjectRenameDict");

            Instance = null!;
        }
        #endregion

        private void AssignShares()
        {
            try
            {
                PetNicknamesDictionary = Plugin.PluginInterface.GetOrCreateData("PetRenamer.GameObjectRenameDict", () => new Dictionary<ulong, string>());
            }
            catch { }
        }

        private string? GetNameForActor(IGameObject actor)
        {
            if (PetNicknamesDictionary == null)
            {
                return null;
            }

            if (PetNicknamesDictionary.TryGetValue(actor.GameObjectId, out string? nickname))
            {
                return nickname;
            }

            return null;
        }

        public string? GetPetName(IGameObject? actor)
        {
            if (actor == null)
            {
                return null;
            }

            if (actor.ObjectKind != ObjectKind.Companion && actor.ObjectKind != ObjectKind.BattleNpc)
            {
                return null;
            }

            return GetNameForActor(actor);
        }
    }
}
