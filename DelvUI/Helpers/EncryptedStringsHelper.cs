using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.Interop;
using FFXIVClientStructs.STD;
using System;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace DelvUI.Helpers
{
    public static class EncryptedStringsHelper
    {
        public static unsafe string GetString(string original)
        {
            if (!original.StartsWith("_rsv_"))
            {
                return original;
            }

            StdMap<Utf8String, Pointer<byte>> map = LayoutWorld.Instance()->RsvMap[0];
            Pointer<byte> demangled = map[new Utf8String(original)];
            if (demangled.Value != null && Marshal.PtrToStringUTF8((IntPtr)demangled.Value) is { } result)
            {
                return result;
            }

            return original;
        }
    }
}
