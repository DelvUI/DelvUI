using System.Runtime.InteropServices;

namespace DelvUI.GameStructs {
    // DNC Gauge struct, with public stepOrder
    [StructLayout(LayoutKind.Explicit)]
    public struct OpenDNCGauge {
        [FieldOffset(0)] public byte NumFeathers;
        [FieldOffset(1)] public byte Esprit;
        [FieldOffset(2)] public unsafe fixed byte stepOrder[4];
        [FieldOffset(6)] public byte NumCompleteSteps;

        public unsafe ulong NextStep() => (ulong)(15999 + stepOrder[NumCompleteSteps] - 1);

        public unsafe bool IsDancing() => stepOrder[0] > 0;
    }

    public enum DNCStep : byte {
        None,
        Emboite,
        Entrechat,
        Jete,
        Pirouette
    }
}
