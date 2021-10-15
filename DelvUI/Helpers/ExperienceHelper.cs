using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Runtime.InteropServices;

namespace DelvUI.Helpers
{
    public unsafe class ExperienceHelper
    {
        #region singleton
        private static Lazy<ExperienceHelper> _lazyInstance = new Lazy<ExperienceHelper>(() => new ExperienceHelper());

        public static ExperienceHelper Instance => _lazyInstance.Value;

        ~ExperienceHelper()
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

            _lazyInstance = new Lazy<ExperienceHelper>(() => new ExperienceHelper());
        }
        #endregion

        private AddonExp* _addonExp = null;

        public ExperienceHelper()
        {
        }

        public AddonExp* GetExpAddon()
        {
            return (AddonExp*)Plugin.GameGui.GetAddonByName("_Exp", 1);
        }

        public uint CurrentExp
        {
            get
            {
                AddonExp* addon = GetExpAddon();
                return addon != null ? addon->CurrentExp : 0;
            }
        }

        public uint RequiredExp
        {
            get
            {
                AddonExp* addon = GetExpAddon();
                return addon != null ? addon->RequiredExp : 0;
            }
        }

        public uint RestedExp
        {
            get
            {
                AddonExp* addon = GetExpAddon();
                return addon != null ? addon->RestedExp : 0;
            }
        }

        public float PercentExp
        {
            get
            {
                AddonExp* addon = GetExpAddon();
                return addon != null ? addon->CurrentExpPercent : 0;
            }
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x290)]
    public struct AddonExp
    {
        [FieldOffset(0x0)] public AtkUnitBase AtkUnitBase;

        [FieldOffset(0x270)] public byte ClassJob;

        [FieldOffset(0x278)] public uint CurrentExp;
        [FieldOffset(0x27C)] public uint RequiredExp;
        [FieldOffset(0x280)] public uint RestedExp;

        public float CurrentExpPercent => (float)CurrentExp / RequiredExp * 100;
    }
}
