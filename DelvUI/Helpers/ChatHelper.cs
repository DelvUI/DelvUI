/*
Copyright(c) 2021 Ottermandias (https://github.com/Ottermandias/GatherBuddy)
Modifications Copyright(c) 2021 DelvUI
09/12/2021 - Extracted code to send chat messages and commands.

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

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DelvUI.Helpers
{
    public class ChatHelper : IDisposable
    {
        #region Singleton
        private ChatHelper()
        {
            /*
             Part of chatModule disassembly signature
            .text:00007FF6482AF4B0                   sub_7FF6482AF4B0 proc near
            .text:00007FF6482AF4B0
            .text:00007FF6482AF4B0                   arg_0= qword ptr  8
            .text:00007FF6482AF4B0
            .text:00007FF6482AF4B0 48 89 5C 24 08    mov     [rsp+arg_0], rbx
            .text:00007FF6482AF4B5 57                push    rdi
            .text:00007FF6482AF4B6 48 83 EC 20       sub     rsp, 20h
            .text:00007FF6482AF4BA 48 8B FA          mov     rdi, rdx
            .text:00007FF6482AF4BD 48 8B D9          mov     rbx, rcx
            .text:00007FF6482AF4C0 45 84 C9          test    r9b, r9b
            .text:00007FF6482AF4C3 74 0C             jz      short loc_7FF6482AF4D1
            */
            _chatModulePtr = Plugin.SigScanner.ScanText("48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9");
        }

        public static void Initialize() { Instance = new ChatHelper(); }

        public static ChatHelper Instance { get; private set; } = null!;

        ~ChatHelper()
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

        private IntPtr _chatModulePtr;

        public static void SendChatMessage(string message)
        {
            if (Instance == null)
            {
                return;
            }

            Instance.SendMessage(message);
        }

        private void SendMessage(string message)
        {
            if (message == null || message.Length == 0)
            {
                return;
            }

            // let dalamud process the command first
            if (Plugin.CommandManager.ProcessCommand(message))
            {
                return;
            }

            if (_chatModulePtr == IntPtr.Zero)
            {
                return;
            }

            // encode message
            var (text, length) = EncodeMessage(message);
            var payload = MessagePayload(text, length);

            ChatDelegate chatDelegate = Marshal.GetDelegateForFunctionPointer<ChatDelegate>(_chatModulePtr);
            chatDelegate.Invoke(Plugin.GameGui.GetUIModule(), payload, IntPtr.Zero, (byte)0);

            Marshal.FreeHGlobal(payload);
            Marshal.FreeHGlobal(text);
        }

        private static (IntPtr, long) EncodeMessage(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            var mem = Marshal.AllocHGlobal(bytes.Length + 30);
            Marshal.Copy(bytes, 0, mem, bytes.Length);
            Marshal.WriteByte(mem + bytes.Length, 0);
            return (mem, bytes.Length + 1);
        }

        private static IntPtr MessagePayload(IntPtr message, long length)
        {
            var mem = Marshal.AllocHGlobal(400);
            Marshal.WriteInt64(mem, message.ToInt64());
            Marshal.WriteInt64(mem + 0x8, 64);
            Marshal.WriteInt64(mem + 0x10, length);
            Marshal.WriteInt64(mem + 0x18, 0);
            return mem;
        }
    }

    public delegate IntPtr ChatDelegate(IntPtr uiModulePtr, IntPtr message, IntPtr unknown1, byte unknown2);
}
