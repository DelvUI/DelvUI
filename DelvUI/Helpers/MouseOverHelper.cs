/*
Copyright(c) 2021 attickdoor (https://github.com/attickdoor/MOActionPlugin)
Modifications Copyright(c) 2021 DelvUI
09/21/2021 - Used original's code hooks and action validations while using 
DelvUI's own logic to select a target.

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using Dalamud.Hooking;
using Lumina.Excel;
using System;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace DelvUI.Helpers
{
    public delegate void OnSetUIMouseoverActorId(long arg1, long arg2);
    public delegate ulong OnRequestAction(long arg1, uint arg2, ulong arg3, long arg4, uint arg5, uint arg6, int arg7);

    public class MouseOverHelper
    {
        #region Singleton
        private MouseOverHelper()
        {
#if DEBUG
            _setUIMouseOverActorId = Plugin.SigScanner.ScanText("48 89 91 ?? ?? ?? ?? C3 CC CC CC CC CC CC CC CC 48 89 5C 24 ?? 55 56 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 48 8D B1 ?? ?? ?? ?? 44 89 44 24 ?? 48 8B EA 48 8B D9 48 8B CE 48 8D 15 ?? ?? ?? ?? 41 B9 ?? ?? ?? ??");
            _uiMouseOverActorIdHook = new Hook<OnSetUIMouseoverActorId>(_setUIMouseOverActorId, new OnSetUIMouseoverActorId(HandleUIMouseOverActorId));
            _uiMouseOverActorIdHook.Enable();
#endif
            _requestAction = Plugin.SigScanner.ScanText("40 53 55 57 41 54 41 57 48 83 EC 60 83 BC 24 ?? ?? ?? ?? ?? 49 8B E9 45 8B E0 44 8B FA 48 8B F9 41 8B D8 74 14 80 79 68 00 74 0E 32 C0 48 83 C4 60 41 5F 41 5C 5F 5D 5B C3");
            _requsetActionHook = new Hook<OnRequestAction>(_requestAction, new OnRequestAction(HandleRequestAction));
            _requsetActionHook.Enable();

            _sheet = Plugin.DataManager.GetExcelSheet<Action>();
        }

        public static void Initialize() { Instance = new MouseOverHelper(); }

        public static MouseOverHelper Instance { get; private set; } = null!;
        #endregion

        private IntPtr _setUIMouseOverActorId;
        private Hook<OnSetUIMouseoverActorId> _uiMouseOverActorIdHook;

        private IntPtr _requestAction;
        private Hook<OnRequestAction> _requsetActionHook;

        private ExcelSheet<Action>? _sheet;
        public GameObject? Target = null;

        private void HandleUIMouseOverActorId(long arg1, long arg2)
        {
            PluginLog.Log("MO: {0} - {1}", arg1, arg2);
            _uiMouseOverActorIdHook.Original(arg1, arg2);
        }

        private ulong HandleRequestAction(long arg1, uint arg2, ulong arg3, long arg4, uint arg5, uint arg6, int arg7)
        {
            //PluginLog.Log("ACTION: {0} - {1} - {2} - {3} - {4} - {5} - {6}}", arg1, arg2, arg3, arg4, arg5, arg6, arg7);

            if (IsActionValid(arg3, Target))
            {
                return _requsetActionHook.Original(arg1, arg2, arg3, Target!.ObjectId, arg5, arg6, arg7);
            }

            return _requsetActionHook.Original(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        private bool IsActionValid(ulong actionID, GameObject? target)
        {
            if (target == null || actionID == 0 || _sheet == null)
            {
                return false;
            }

            var action = _sheet.GetRow((uint)actionID);
            if (action == null)
            {
                return false;
            }

            // friendly player (TODO: pvp? lol)
            if (target is PlayerCharacter)
            {
                return action.CanTargetFriendly || action.CanTargetParty || action.CanTargetSelf;
            }

            // friendly npc
            if (target is BattleNpc npc)
            {
                if (npc.BattleNpcKind != BattleNpcSubKind.Enemy)
                {
                    return action.CanTargetFriendly || action.CanTargetParty || action.CanTargetSelf;
                }
            }

            return action.CanTargetHostile;
        }
    }
}
