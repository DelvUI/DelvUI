using Dalamud.Game.ClientState.Actors;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Actors.Types.NonPlayer;
using Dalamud.Game.ClientState.Structs;
using Dalamud.Game.Internal.Gui.Addon;
using Dalamud.Interface;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.StatusEffects;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Actor = Dalamud.Game.ClientState.Actors.Types.Actor;

namespace DelvUI.Interface
{
    public abstract class HudWindow
    {
        private readonly OpenContextMenuFromTarget _openContextMenuFromTarget;

        private readonly StatusEffectsListHud _playerBuffList;
        private readonly StatusEffectsListHud _playerDebuffList;
        private readonly StatusEffectsListHud _targetBuffList;
        private readonly StatusEffectsListHud _targetDebuffList;
        private readonly StatusEffectsListHud _raidJobsBuffList;
        protected readonly PluginConfiguration PluginConfiguration;
        protected readonly DalamudPluginInterface PluginInterface;

        private ImGuiWindowFlags _childFlags = 0;

        protected uint[] _raidWideBuffs =
        {
            // See https://external-preview.redd.it/bKacLk4PKav7vdP1ilT66gAtB1t7BTJjxsMrImRHr1k.png?auto=webp&s=cbe6880c34b45e2db20c247c8ab9eef543538e96
            // Left Eye
            1184, 1454,
            // Battle Litany
            786, 1414,
            // Brotherhood
            1185, 2174,
            // Battle Voice
            141,
            // Devilment
            1825,
            // Technical Finish
            1822, 2050,
            // Standard Finish
            1821, 2024, 2105, 2113,
            // Embolden
            1239, 1297, 2282,
            // Devotion
            1213,
            // ------ AST Card Buffs -------
            // The Balance
            829, 1338, 1882,
            // The Bole
            830, 1339, 1883,
            // The Arrow
            831, 1884,
            // The Spear
            832, 1885,
            // The Ewer
            833, 1340, 1886,
            // The Spire
            834, 1341, 1887,
            // Lord of Crowns
            1451, 1876,
            // Lady of Crowns
            1452, 1877,
            // Divination
            1878, 2034,
            // Chain Stratagem
            1221, 1406
        };

        protected List<uint> RaidWideBuffs;
        private readonly List<uint> JobSpecificBuffs;
        protected bool ShowRaidWideBuffIcons => PluginConfiguration.ShowRaidWideBuffIcons;
        protected bool ShowJobSpecificBuffIcons => PluginConfiguration.ShowJobSpecificBuffIcons;

        private LastUsedCast _lastPlayerUsedCast;
        private LastUsedCast _lastTargetUsedCast;

        private MPTickHelper _mpTickHelper;
        public bool IsVisible = true;
        private readonly Vector2 Center = new(CenterX, CenterY);

        protected TankHudConfig ConfigTank => (TankHudConfig)ConfigurationManager.GetInstance().GetConfiguration(new TankHudConfig());
        protected GeneralHudConfig ConfigGeneral => (GeneralHudConfig)ConfigurationManager.GetInstance().GetConfiguration(new GeneralHudConfig());

        protected HudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration)
        {
            PluginInterface = pluginInterface;
            PluginConfiguration = pluginConfiguration;

            _childFlags |= ImGuiWindowFlags.NoTitleBar;
            _childFlags |= ImGuiWindowFlags.NoScrollbar;
            _childFlags |= ImGuiWindowFlags.AlwaysAutoResize;
            _childFlags |= ImGuiWindowFlags.NoBackground;
            _childFlags |= ImGuiWindowFlags.NoBringToFrontOnFocus;

            _openContextMenuFromTarget =
                Marshal.GetDelegateForFunctionPointer<OpenContextMenuFromTarget>(PluginInterface.TargetModuleScanner.ScanText("48 85 D2 74 7F 48 89 5C 24"));

            RaidWideBuffs = new List<uint>(_raidWideBuffs);
            JobSpecificBuffs = GetJobSpecificBuffs();
            //PluginConfiguration.ConfigChangedEvent += OnConfigChanged;

            //_playerBuffList = new StatusEffectsListHud("tmp1", pluginConfiguration.PlayerBuffListConfig);
            //_playerDebuffList = new StatusEffectsListHud("tmp2", pluginConfiguration.PlayerDebuffListConfig);
            //_targetBuffList = new StatusEffectsListHud("tmp3", pluginConfiguration.TargetDebuffListConfig);
            //_targetDebuffList = new StatusEffectsListHud("tmp4", pluginConfiguration.TargetBuffListConfig);
            //_raidJobsBuffList = new StatusEffectsListHud("tmp5", pluginConfiguration.RaidJobBuffListConfig);
        }

        public abstract uint JobId { get; }

        protected static float CenterX => ImGui.GetMainViewport().Size.X / 2f;
        protected static float CenterY => ImGui.GetMainViewport().Size.Y / 2f;
        protected static int XOffset => 160;
        protected static int YOffset => 460;

        protected Vector2 BarSize { get; private set; }

        private void OnConfigChanged(object sender, EventArgs args)
        {
            if (!PluginConfiguration.MPTickerEnabled)
            {
                _mpTickHelper = null;
            }
        }

        protected virtual void DrawHealthBar()
        {

        }

        private Vector2 CalculatePosition(Vector2 position, Vector2 size) => Center + position - size / 2f;

        protected virtual void DrawPrimaryResourceBar() => DrawPrimaryResourceBar(PrimaryResourceType.MP);

        protected virtual void DrawPrimaryResourceBar(PrimaryResourceType type = PrimaryResourceType.MP, PluginConfigColor partialFillColor = null)
        {

        }

        protected virtual void DrawTargetBar()
        {

        }

        protected virtual void DrawFocusBar()
        {
        }

        protected virtual void DrawTargetOfTargetBar(int targetActorId)
        {

        }

        protected virtual unsafe void DrawCastBar()
        {

        }

        protected virtual unsafe void DrawTargetCastBar()
        {

        }

        protected virtual void DrawTargetShield(Actor actor, Vector2 cursorPos, Vector2 targetBar)
        {

        }

        protected virtual void DrawTankStanceIndicator()
        {

        }

        protected virtual void DrawMPTicker()
        {

        }

        protected virtual void DrawGCDIndicator()
        {

        }

        private void DrawPlayerStatusEffects()
        {

        }

        private void DrawTargetStatusEffects()
        {

        }
        protected virtual List<uint> GetJobSpecificBuffs() => new();

        private void DrawRaidJobBuffs()
        {
            if (!(ShowRaidWideBuffIcons || ShowJobSpecificBuffIcons))
            {
                return;
            }

            List<uint> buffIds = new();

            if (ShowJobSpecificBuffIcons)
            {
                buffIds.AddRange(JobSpecificBuffs);
            }

            if (ShowRaidWideBuffIcons)
            {
                buffIds.AddRange(RaidWideBuffs);
            }

            var center = new Vector2(CenterX, CenterY);

            _raidJobsBuffList.Actor = PluginInterface.ClientState.LocalPlayer;
            _raidJobsBuffList.Draw(center, buffIds);
        }

        protected Dictionary<string, uint> DetermineTargetPlateColors(Chara actor)
        {
            //Dictionary<string, uint> colors = PluginConfiguration.NPCColorMap["neutral"];

            //switch (actor.ObjectKind)
            //{
            //    // Still need to figure out the "orange" state; aggroed but not yet attacked.
            //    case ObjectKind.Player:
            //        PluginConfiguration.JobColorMap.TryGetValue(actor.ClassJob.Id, out colors);
            //        colors ??= PluginConfiguration.NPCColorMap["neutral"];

            //        break;

            //    case ObjectKind.BattleNpc when (actor.StatusFlags & StatusFlags.InCombat) == StatusFlags.InCombat:
            //        colors = PluginConfiguration.NPCColorMap["hostile"];

            //        break;

            //    case ObjectKind.BattleNpc:
            //        if (!Utils.IsHostileMemory((BattleNpc)actor))
            //        {
            //            colors = PluginConfiguration.NPCColorMap["friendly"];
            //        }

            //        break;
            //}

            return null;
        }

        private void ClipAround(Addon addon, string windowName, ImDrawListPtr drawList, Action<ImDrawListPtr, string> drawAction)
        {
            if (addon is { Visible: true })
            {
                ClipAround(new Vector2(addon.X + 5, addon.Y + 5), new Vector2(addon.X + addon.Width - 5, addon.Y + addon.Height - 5), windowName, drawList, drawAction);
            }
            else
            {
                drawAction(drawList, windowName);
            }
        }

        private void ClipAround(Vector2 min, Vector2 max, string windowName, ImDrawListPtr drawList, Action<ImDrawListPtr, string> drawAction)
        {
            float maxX = ImGui.GetMainViewport().Size.X;
            float maxY = ImGui.GetMainViewport().Size.Y;
            Vector2 aboveMin = new(0, 0);
            Vector2 aboveMax = new(maxX, min.Y);
            Vector2 leftMin = new(0, min.Y);
            Vector2 leftMax = new(min.X, maxY);

            Vector2 rightMin = new(max.X, min.Y);
            Vector2 rightMax = new(maxX, max.Y);
            Vector2 belowMin = new(min.X, max.Y);
            Vector2 belowMax = new(maxX, maxY);

            for (int i = 0; i < 4; i++)
            {
                Vector2 clipMin;
                Vector2 clipMax;

                switch (i)
                {
                    default:
                        clipMin = aboveMin;
                        clipMax = aboveMax;

                        break;

                    case 1:
                        clipMin = leftMin;
                        clipMax = leftMax;

                        break;

                    case 2:
                        clipMin = rightMin;
                        clipMax = rightMax;

                        break;

                    case 3:
                        clipMin = belowMin;
                        clipMax = belowMax;

                        break;
                }

                ImGui.PushClipRect(clipMin, clipMax, false);
                drawAction(drawList, windowName + "_" + i);
                ImGui.PopClipRect();
            }
        }

        protected void DrawOutlinedText(string text, Vector2 pos) { DrawHelper.DrawOutlinedText(text, pos); }

        protected void DrawOutlinedText(string text, Vector2 pos, Vector4 color, Vector4 outlineColor) { DrawHelper.DrawOutlinedText(text, pos, color, outlineColor); }

        public void Draw()
        {
            return;
            if (!ShouldBeVisible() || PluginInterface.ClientState.LocalPlayer == null)
            {
                return;
            }

            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.SetNextWindowSize(ImGui.GetMainViewport().Size);

            bool begin = ImGui.Begin(
                "DelvUI",
                ImGuiWindowFlags.NoTitleBar
              | ImGuiWindowFlags.NoScrollbar
              | ImGuiWindowFlags.AlwaysAutoResize
              | ImGuiWindowFlags.NoBackground
              | ImGuiWindowFlags.NoInputs
              | ImGuiWindowFlags.NoBringToFrontOnFocus
            );

            if (!begin)
            {
                return;
            }

            DrawGenericElements();

            Draw(true);

            ImGui.End();
        }

        protected void DrawGenericElements()
        {
            DrawHealthBar();
            DrawPrimaryResourceBar();
            DrawTargetBar();
            DrawFocusBar();
            DrawCastBar();
            DrawTargetCastBar();
            DrawMPTicker();
            DrawGCDIndicator();

            DrawPlayerStatusEffects();
            DrawTargetStatusEffects();
            DrawRaidJobBuffs();
        }

        protected abstract void Draw(bool _);

        protected virtual unsafe bool ShouldBeVisible()
        {
            if (PluginConfiguration.HideHud)
            {
                return false;
            }

            if (IsVisible)
            {
                return true;
            }

            AtkUnitBase* parameterWidget = (AtkUnitBase*)PluginInterface.Framework.Gui.GetUiObjectByName("_ParameterWidget", 1);
            AtkUnitBase* fadeMiddleWidget = (AtkUnitBase*)PluginInterface.Framework.Gui.GetUiObjectByName("FadeMiddle", 1);

            // Display HUD only if parameter widget is visible and we're not in a fade event
            return PluginInterface.ClientState.LocalPlayer == null
                || parameterWidget == null
                || fadeMiddleWidget == null
                || !parameterWidget->IsVisible
                || fadeMiddleWidget->IsVisible;
        }

        private delegate void OpenContextMenuFromTarget(IntPtr agentHud, IntPtr gameObject);

        #region configs

        protected int HealthBarHeight => PluginConfiguration.HealthBarHeight;
        protected int HealthBarWidth => PluginConfiguration.HealthBarWidth;
        protected int HealthBarXOffset => PluginConfiguration.HealthBarXOffset;
        protected int HealthBarYOffset => PluginConfiguration.HealthBarYOffset;
        protected int HealthBarTextLeftXOffset => PluginConfiguration.HealthBarTextLeftXOffset;
        protected int HealthBarTextLeftYOffset => PluginConfiguration.HealthBarTextLeftYOffset;
        protected int HealthBarTextRightXOffset => PluginConfiguration.HealthBarTextRightXOffset;
        protected int HealthBarTextRightYOffset => PluginConfiguration.HealthBarTextRightYOffset;

        protected int TargetBarHeight => PluginConfiguration.TargetBarHeight;
        protected int TargetBarWidth => PluginConfiguration.TargetBarWidth;
        protected int TargetBarXOffset => PluginConfiguration.TargetBarXOffset;
        protected int TargetBarYOffset => PluginConfiguration.TargetBarYOffset;
        protected int TargetBarTextLeftXOffset => PluginConfiguration.TargetBarTextLeftXOffset;
        protected int TargetBarTextLeftYOffset => PluginConfiguration.TargetBarTextLeftYOffset;
        protected int TargetBarTextRightXOffset => PluginConfiguration.TargetBarTextRightXOffset;
        protected int TargetBarTextRightYOffset => PluginConfiguration.TargetBarTextRightYOffset;

        protected int ToTBarHeight => PluginConfiguration.ToTBarHeight;
        protected int ToTBarWidth => PluginConfiguration.ToTBarWidth;
        protected int ToTBarXOffset => PluginConfiguration.ToTBarXOffset;
        protected int ToTBarYOffset => PluginConfiguration.ToTBarYOffset;
        protected int ToTBarTextXOffset => PluginConfiguration.ToTBarTextXOffset;
        protected int ToTBarTextYOffset => PluginConfiguration.ToTBarTextYOffset;

        protected int FocusBarHeight => PluginConfiguration.FocusBarHeight;
        protected int FocusBarWidth => PluginConfiguration.FocusBarWidth;
        protected int FocusBarXOffset => PluginConfiguration.FocusBarXOffset;
        protected int FocusBarYOffset => PluginConfiguration.FocusBarYOffset;
        protected int FocusBarTextXOffset => PluginConfiguration.FocusBarTextXOffset;
        protected int FocusBarTextYOffset => PluginConfiguration.FocusBarTextYOffset;

        protected int MPTickerHeight => PluginConfiguration.MPTickerHeight;
        protected int MPTickerWidth => PluginConfiguration.MPTickerWidth;
        protected int MPTickerXOffset => PluginConfiguration.MPTickerXOffset;
        protected int MPTickerYOffset => PluginConfiguration.MPTickerYOffset;
        protected bool MPTickerShowBorder => PluginConfiguration.MPTickerShowBorder;
        protected bool MPTickerHideOnFullMp => PluginConfiguration.MPTickerHideOnFullMp;

        protected int GCDIndicatorHeight => PluginConfiguration.GCDIndicatorHeight;
        protected int GCDIndicatorWidth => PluginConfiguration.GCDIndicatorWidth;
        protected int GCDIndicatorXOffset => PluginConfiguration.GCDIndicatorXOffset;
        protected int GCDIndicatorYOffset => PluginConfiguration.GCDIndicatorYOffset;
        protected bool GCDIndicatorShowBorder => PluginConfiguration.GCDIndicatorShowBorder;
        protected bool GCDIndicatorVertical => PluginConfiguration.GCDIndicatorVertical;

        protected int CastBarWidth => PluginConfiguration.CastBarWidth;
        protected int CastBarHeight => PluginConfiguration.CastBarHeight;
        protected int CastBarXOffset => PluginConfiguration.CastBarXOffset;
        protected int CastBarYOffset => PluginConfiguration.CastBarYOffset;

        protected int TargetCastBarWidth => PluginConfiguration.TargetCastBarWidth;
        protected int TargetCastBarHeight => PluginConfiguration.TargetCastBarHeight;
        protected int TargetCastBarXOffset => PluginConfiguration.TargetCastBarXOffset;
        protected int TargetCastBarYOffset => PluginConfiguration.TargetCastBarYOffset;


        #endregion
    }

    [Serializable]
    [Portable(false)]
    [Section("Job Specific Bars")]
    [SubSection("Tank", 0)]
    [SubSection("General##Tank", 1)]
    public class TankHudConfig : PluginConfigObject
    {
        [Checkbox("Tank Stance Indicator Enabled")]
        public bool TankStanceIndicatorEnabled = true;

        [DragInt("Tank Stance Indicator Width", min = 1, max = 6)]
        public int TankStanceIndicatorWidth = 2;
    }

    public enum PrimaryResourceType
    {
        MP,
        CP,
        GP,
        None
    }

    [Section("Job Specific Bars")]
    [SubSection("General", 0)]
    [SubSection("General##General", 0)]
    public class GeneralHudConfig : PluginConfigObject
    {
        [DragFloat2("Primary Resource Position", min = -4000f, max = 4000f)]
        [Order(0)]
        public Vector2 PrimaryResourcePosition = new(0, 449);

        [DragFloat2("Primary Resource Bar Size", min = 0, max = 4000f)]
        [Order(5)]
        public Vector2 PrimaryResourceSize = new(254, 20);

        #region Primary Resource Value

        [Checkbox("Show Primary Resource Value")]
        [CollapseControl(10, 0)]
        public bool ShowPrimaryResourceBarValue = false;

        [DragFloat2("Primary Resource Text Offset", min = -4000f, max = 4000f)]
        [CollapseWith(0, 0)]
        public Vector2 PrimaryResourceBarTextOffset = new(0, 0);

        #endregion

        #region Primary Resource Threshold

        [Checkbox("Show Primary Resource Threshold Marker")]
        [CollapseControl(15, 1)]
        public bool ShowPrimaryResourceBarThresholdMarker = false;

        [DragInt("Primary Resource Threshold Value", min = 0, max = 10000)]
        [CollapseWith(0, 1)]
        public int PrimaryResourceBarThresholdValue = 7000;

        #endregion

        #region Colors

        [ColorEdit4("Bar Background Color")]
        [Order(20)]
        public PluginConfigColor BarBackgroundColor = new(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 50f / 100f));

        [ColorEdit4("Bar Partial Fill Color")]
        [Order(25)]
        public PluginConfigColor BarPartialFillColor = new(new Vector4(0f / 255f, 205f / 255f, 230f / 255f, 100f / 100f));

        #endregion
    }
}
