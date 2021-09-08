using System;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Hooking;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace DelvUI.Helpers
{
    public sealed unsafe class SystemMenuHook
    {
        private readonly AtkValueChangeType _atkValueChangeType;

        private readonly AtkValueSetString _atkValueSetString;

        private readonly Hook<AgentHudOpenSystemMenuPrototype> _hookAgentHudOpenSystemMenu;

        private readonly Hook<UiModuleRequestMainCommand> _hookUiModuleRequestMainCommand;
        private readonly DalamudPluginInterface _pluginInterface;

        // This hook overrides Dalamuds one (or comes after so the Dalamud changes don't go through) change this for Dalamud 5 
        // https://github.com/goatcorp/Dalamud/blob/7ac46ed869a5f31ffb21c74eec30f43ca185ac46/Dalamud/Game/Internal/DalamudAtkTweaks.cs#L190

        public SystemMenuHook(DalamudPluginInterface pluginInterface)
        {
            _pluginInterface = pluginInterface;
            IntPtr openSystemMenuAddress = pluginInterface.TargetModuleScanner.ScanText("E8 ?? ?? ?? ?? 32 C0 4C 8B AC 24 ?? ?? ?? ?? 48 8B 8D ?? ?? ?? ??");

            _hookAgentHudOpenSystemMenu = new Hook<AgentHudOpenSystemMenuPrototype>(openSystemMenuAddress, AgentHudOpenSystemMenuDetour);
            _hookAgentHudOpenSystemMenu.Enable();

            IntPtr atkValueChangeTypeAddress =
                pluginInterface.TargetModuleScanner.ScanText("E8 ?? ?? ?? ?? 45 84 F6 48 8D 4C 24 ??");

            _atkValueChangeType =
                Marshal.GetDelegateForFunctionPointer<AtkValueChangeType>(atkValueChangeTypeAddress);

            IntPtr atkValueSetStringAddress =
                pluginInterface.TargetModuleScanner.ScanText("E8 ?? ?? ?? ?? 41 03 ED");

            _atkValueSetString = Marshal.GetDelegateForFunctionPointer<AtkValueSetString>(atkValueSetStringAddress);

            IntPtr uiModuleRequestMainCommandAddress = pluginInterface.TargetModuleScanner.ScanText(
                "40 53 56 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 48 8B 01 8B DA 48 8B F1 FF 90 ?? ?? ?? ??"
            );

            _hookUiModuleRequestMainCommand = new Hook<UiModuleRequestMainCommand>(uiModuleRequestMainCommandAddress, UiModuleRequestMainCommandDetour);
            _hookUiModuleRequestMainCommand.Enable();
        }

        private void AgentHudOpenSystemMenuDetour(void* thisPtr, AtkValue* atkValueArgs, uint menuSize)
        {
            // the max size (hardcoded) is 0xE/15, but the system menu currently uses 0xC/12
            // this is a just in case that doesnt really matter
            // see if we can add 3 entries
            if (menuSize > 12)
            {
                _hookAgentHudOpenSystemMenu.Original(thisPtr, atkValueArgs, menuSize);

                return;
            }

            // atkValueArgs is actually an array of AtkValues used as args. all their UI code works like this.
            // in this case, menu size is stored in atkValueArgs[4], and the next 15 slots are the MainCommand
            // the 15 slots after that, if they exist, are the entry names, but they are otherwise pulled from MainCommand EXD
            // reference the original function for more details :)

            // step 1) move all the current menu items down so we can put Dalamud at the top like it deserves
            // Yeet DelvUI settings button in first
            _atkValueChangeType(&atkValueArgs[menuSize + 5], ValueType.Int); // currently this value has no type, set it to int
            _atkValueChangeType(&atkValueArgs[menuSize + 5 + 1], ValueType.Int);
            _atkValueChangeType(&atkValueArgs[menuSize + 5 + 2], ValueType.Int);

            for (uint i = menuSize + 3; i > 1; i--)
            {
                AtkValue* curEntry = &atkValueArgs[i + 5 - 3];
                AtkValue* nextEntry = &atkValueArgs[i + 5];

                nextEntry->Int = curEntry->Int;
            }

            // step 2) set our new entries to dummy commands
            AtkValue* firstEntry = &atkValueArgs[5];
            firstEntry->Int = 69420;
            AtkValue* secondEntry = &atkValueArgs[6];
            secondEntry->Int = 69421;
            AtkValue* thirdEntry = &atkValueArgs[7];
            thirdEntry->Int = 69422;

            // step 3) create strings for them
            // since the game first checks for strings in the AtkValue argument before pulling them from the exd, if we create strings we dont have to worry
            // about hooking the exd reader, thank god
            AtkValue* firstStringEntry = &atkValueArgs[5 + 15];
            _atkValueChangeType(firstStringEntry, ValueType.String);
            AtkValue* secondStringEntry = &atkValueArgs[6 + 15];
            _atkValueChangeType(secondStringEntry, ValueType.String);
            AtkValue* thirdStringEntry = &atkValueArgs[7 + 15];
            _atkValueChangeType(thirdStringEntry, ValueType.String);

            byte[] strDelvSettings = Encoding.UTF8.GetBytes("DelvUI Settings");
            byte[] strPlugins = Encoding.UTF8.GetBytes("Dalamud Plugins");
            byte[] strSettings = Encoding.UTF8.GetBytes("Dalamud Settings");
            byte* bytes = stackalloc byte[strDelvSettings.Length + 1];
            byte* bytes2 = stackalloc byte[strPlugins.Length + 1];
            byte* bytes3 = stackalloc byte[strSettings.Length + 1];

            Marshal.Copy(strDelvSettings, 0, new IntPtr(bytes), strDelvSettings.Length);
            bytes[strDelvSettings.Length] = 0x0;
            Marshal.Copy(strPlugins, 0, new IntPtr(bytes2), strPlugins.Length);
            bytes2[strPlugins.Length] = 0x0;
            Marshal.Copy(strSettings, 0, new IntPtr(bytes3), strSettings.Length);
            bytes3[strSettings.Length] = 0x0;

            _atkValueSetString(firstStringEntry, bytes); // this allocs the string properly using the game's allocators and copies it, so we dont have to worry about memory fuckups

            _atkValueSetString(
                secondStringEntry,
                bytes2
            ); // this allocs the string properly using the game's allocators and copies it, so we dont have to worry about memory fuckups

            _atkValueSetString(
                thirdStringEntry,
                bytes3
            ); // this allocs the string properly using the game's allocators and copies it, so we dont have to worry about memory fuckups

            // open menu with new size
            AtkValue* sizeEntry = &atkValueArgs[4];
            sizeEntry->UInt = menuSize + 3;

            _hookAgentHudOpenSystemMenu.Original(thisPtr, atkValueArgs, menuSize + 3);
        }

        private void UiModuleRequestMainCommandDetour(void* thisPtr, int commandId)
        {
            switch (commandId)
            {
                case 69420:
                    _pluginInterface.CommandManager.ProcessCommand("/delvui");

                    break;

                case 69421:
                    _pluginInterface.CommandManager.ProcessCommand("/xlplugins");

                    break;

                case 69422:
                    _pluginInterface.CommandManager.ProcessCommand("/xlsettings");

                    break;

                default:
                    _hookUiModuleRequestMainCommand.Original(thisPtr, commandId);

                    break;
            }
        }

        private delegate void AgentHudOpenSystemMenuPrototype(void* thisPtr, AtkValue* atkValueArgs, uint menuSize);

        private delegate void AtkValueChangeType(AtkValue* thisPtr, ValueType type);

        private delegate void AtkValueSetString(AtkValue* thisPtr, byte* bytes);

        private delegate void UiModuleRequestMainCommand(void* thisPtr, int commandId);

        #region IDisposable Support

        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            _hookAgentHudOpenSystemMenu.Disable();
            _hookAgentHudOpenSystemMenu.Dispose();

            _hookUiModuleRequestMainCommand.Disable();
            _hookUiModuleRequestMainCommand.Dispose();

            _pluginInterface.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
