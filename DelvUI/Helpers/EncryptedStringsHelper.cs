using Dalamud;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.Interop;
using FFXIVClientStructs.STD;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DelvUI.Helpers
{
    public static class EncryptedStringsHelper
    {
        public static unsafe string GetString(string original)
        {
            string result = original;

            if (result.StartsWith("_rsv_"))
            {
                StdMap<Utf8String, Pointer<byte>>* map = LayoutWorld.Instance()->RsvMap;
                var demangled = Find(LayoutWorld.Instance()->RsvMap->Head->Parent, original);
                if (demangled.Value != null)
                {
                    result = Marshal.PtrToStringUTF8((IntPtr)demangled.Value) ?? original;
                }
            }

            return result;
        }

        private static unsafe int Compare(Utf8String* param1, string param2)
        {
            var length = Math.Min(param1->BufUsed, param2.Length);
            for (var i = 0; i < length; i++)
            {
                var diff = param1->StringPtr[i] - param2[i];
                if (diff != 0)
                {
                    return diff;
                }
            }

            return 0;
        }

        private static unsafe TVal Find<TVal>(StdMap<Utf8String, TVal>.Node* node, string item) where TVal : unmanaged
        {
            while (!node->IsNil)
            {
                switch (Compare(&node->KeyValuePair.Item1, item))
                {
                    case < 0:
                        node = node->Right;
                        continue;
                    case > 0:
                        node = node->Left;
                        continue;
                    default: return node->KeyValuePair.Item2;
                }
            }

            return default;
        }
    }
}
