using System.Runtime.InteropServices;

namespace DelvUI.GameStructs
{
    // DNC Gauge struct, with public stepOrder
    [StructLayout(LayoutKind.Explicit)]
    public struct OpenDNCGauge
    {
        [FieldOffset(0)] public byte NumFeathers;
        [FieldOffset(1)] public byte Esprit;
        [FieldOffset(2)] public unsafe fixed byte stepOrder[4];
        [FieldOffset(6)] public byte NumCompleteSteps;

        public unsafe ulong NextStep() => (ulong)(15999 + stepOrder[NumCompleteSteps] - 1);

        public unsafe bool IsDancing() => stepOrder[0] > 0;
    }

    public enum DNCStep : uint
    {
        None = 15998,
        Emboite = 15999,
        Entrechat = 16000,
        Jete = 16001,
        Pirouette = 16002
    }
}
