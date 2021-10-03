using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DelvUI.Interface.GeneralElements
{
    public unsafe class ExperienceBarHud : DraggableHudElement, IHudElementWithActor
    {
        private ExperienceBarConfig Config => (ExperienceBarConfig)_config;

        public GameObject? Actor { get; set; } = null;

        private Bar2 ExpBar { get; set; }

        public ExperienceBarHud(string ID, ExperienceBarConfig config, string displayName) : base(ID, config, displayName)
        {
            ExpBar = new Bar2(Config);
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            return (new List<Vector2>() { Config.Position }, new List<Vector2>() { Config.Size });
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (!Config.Enabled)
            {
                return;
            }

            PlayerCharacter? player = Plugin.ClientState.LocalPlayer;
            var addonExp = (AddonExp*)Plugin.GameGui.GetAddonByName("_Exp", 1);

            uint current = addonExp->CurrentExp;
            uint max = addonExp->RequiredExp;
            uint rested = addonExp->RestedExp;
            string level = player?.Level.ToString() ?? "??";
            string jobLabel = player is not null ? JobsHelper.JobNames[player.ClassJob.Id] : "???";

            ExpBar.SetBarText(string.Format("{0}  Lv{1}  {2:n0}/{3:n0}", jobLabel, level, current, max));
            ExpBar.Draw(origin, current, max);
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
