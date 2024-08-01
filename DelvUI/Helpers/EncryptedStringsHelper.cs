using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.Interop;
using FFXIVClientStructs.STD;
using System;
using System.Runtime.InteropServices;

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

            try
            {
                TempLayoutWorld* layoutWorld = (TempLayoutWorld*)LayoutWorld.Instance();
                StdMap<Utf8String, Pointer<byte>> map = layoutWorld->RsvMap[0];
                Pointer<byte> demangled = map[new Utf8String(original)];
                if (demangled.Value != null && Marshal.PtrToStringUTF8((IntPtr)demangled.Value) is { } result)
                {
                    return result;
                }
            }
            catch (Exception e)
            {
                Plugin.Logger.Error("Error reading rsv map:\n" + e.StackTrace);
            }

            return original;
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x230)]
    public unsafe struct TempLayoutWorld
    {
        [FieldOffset(0x220)] public StdMap<Utf8String, Pointer<byte>>* RsvMap;
    }
}
