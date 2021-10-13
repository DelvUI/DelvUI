using Dalamud.Hooking;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Runtime.InteropServices;
using System.Text;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace DelvUI.Helpers
{
    public sealed unsafe class SystemMenuHook
    {
        private readonly AtkValueChangeType _atkValueChangeType;

        private readonly AtkValueSetString _atkValueSetString;

        private readonly Hook<AgentHudOpenSystemMenuPrototype> _hookAgentHudOpenSystemMenu;

        private readonly Hook<UiModuleRequestMainCommand> _hookUiModuleRequestMainCommand;


        // This hook overrides Dalamuds one (or comes after so the Dalamud changes don't go through) change this for Dalamud 5 
        // https://github.com/goatcorp/Dalamud/blob/7ac46ed869a5f31ffb21c74eec30f43ca185ac46/Dalamud/Game/Internal/DalamudAtkTweaks.cs#L190

        public SystemMenuHook(DalamudPluginInterface pluginInterface)
        {
            /*
             Part of openSystemMenu disassembly signature
            .text:00007FF648526B20                   Client__UI__Agent__AgentHUD_OpenSystemMenu proc near
            .text:00007FF648526B20
            .text:00007FF648526B20                   var_48= qword ptr -48h
            .text:00007FF648526B20                   var_40= qword ptr -40h
            .text:00007FF648526B20                   var_38= word ptr -38h
            .text:00007FF648526B20                   var_30= dword ptr -30h
            .text:00007FF648526B20                   arg_0= qword ptr  8
            .text:00007FF648526B20                   arg_8= qword ptr  10h
            .text:00007FF648526B20                   arg_10= dword ptr  18h
            .text:00007FF648526B20                   arg_18= qword ptr  20h
            .text:00007FF648526B20
            .text:00007FF648526B20 48 89 5C 24 08    mov     [rsp+arg_0], rbx
            .text:00007FF648526B25 48 89 6C 24 10    mov     [rsp+arg_8], rbp
            .text:00007FF648526B2A 48 89 74 24 20    mov     [rsp+arg_18], rsi
            .text:00007FF648526B2F 57                push    rdi
            .text:00007FF648526B30 41 54             push    r12
            .text:00007FF648526B32 41 55             push    r13
            .text:00007FF648526B34 41 56             push    r14
            .text:00007FF648526B36 41 57             push    r15
            .text:00007FF648526B38 48 83 EC 40       sub     rsp, 40h
            .text:00007FF648526B3C 46 8D 34 45 07 00+lea     r14d, ds:7[r8*2]
            */
            IntPtr openSystemMenuAddress = Plugin.SigScanner.ScanText("E8 ?? ?? ?? ?? 32 C0 4C 8B AC 24 ?? ?? ?? ?? 48 8B 8D ?? ?? ?? ??");

            _hookAgentHudOpenSystemMenu = new Hook<AgentHudOpenSystemMenuPrototype>(openSystemMenuAddress, AgentHudOpenSystemMenuDetour);
            _hookAgentHudOpenSystemMenu.Enable();

            /*
             Part of atkValueChangeType disassembly signature
            .text:00007FF647D8B7B4 E8 67 4B 40 00    call    Component__GUI__AtkValue_ChangeType
            .text:00007FF647D8B7B9 45 84 F6          test    r14b, r14b
            .text:00007FF647D8B7BC 48 8D 4C 24 50    lea     rcx, [rsp+78h+var_28]
            .text:00007FF647D8B7C1 8B D7             mov     edx, edi
            .text:00007FF647D8B7C3 0F 94 C3          setz    bl
            .text:00007FF647D8B7C6 88 5C 24 48       mov     [rsp+78h+var_30], bl
            */
            IntPtr atkValueChangeTypeAddress =
                Plugin.SigScanner.ScanText("E8 ?? ?? ?? ?? 45 84 F6 48 8D 4C 24 ??");

            _atkValueChangeType =
                Marshal.GetDelegateForFunctionPointer<AtkValueChangeType>(atkValueChangeTypeAddress);

            /*
             Part of atkValueSetString disassembly signature
            .text:00007FF647F252E5 E8 16 AB 26 00    call    Component__GUI__AtkValue_SetString
            .text:00007FF647F252EA 41 03 ED          add     ebp, r13d
            .text:00007FF647F252ED E9 A3 00 00 00    jmp     loc_7FF647F25395
            */
            IntPtr atkValueSetStringAddress =
                Plugin.SigScanner.ScanText("E8 ?? ?? ?? ?? 41 03 ED");

            _atkValueSetString = Marshal.GetDelegateForFunctionPointer<AtkValueSetString>(atkValueSetStringAddress);

            /*
             Part of uiModuleRequestMainCommand disassembly signature
            .text:00007FF6482AE6A0                   Client__UI__UIModule_ExecuteMainCommand proc near
            .text:00007FF6482AE6A0
            .text:00007FF6482AE6A0                   var_A8= byte ptr -0A8h
            .text:00007FF6482AE6A0                   var_98= byte ptr -98h
            .text:00007FF6482AE6A0                   var_28= qword ptr -28h
            .text:00007FF6482AE6A0                   var_18= qword ptr -18h
            .text:00007FF6482AE6A0                   arg_10= qword ptr  18h
            .text:00007FF6482AE6A0
            .text:00007FF6482AE6A0                   ; __unwind { // __GSHandlerCheck
            .text:00007FF6482AE6A0 40 53             push    rbx
            */
            IntPtr uiModuleRequestMainCommandAddress = Plugin.SigScanner.ScanText(
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
                    Plugin.CommandManager.ProcessCommand("/delvui");
                    break;

                case 69421:
                    Plugin.CommandManager.ProcessCommand("/xlplugins");

                    break;

                case 69422:
                    Plugin.CommandManager.ProcessCommand("/xlsettings");

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
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
