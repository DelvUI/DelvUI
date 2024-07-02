using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.Interop;
using System;
using System.Runtime.InteropServices;

namespace DelvUI.Helpers
{
    public static class EncryptedStringsHelper
    {
        public static unsafe string GetString(string original)
        {
            Pointer<byte> demangled = LayoutWorld.Instance()->RsvMap[0][new Utf8String(original)];
            if (demangled.Value != null && Marshal.PtrToStringUTF8((IntPtr)demangled.Value) is { } result)
            {
                return result;
            }

            return original;
        }
    }
}
