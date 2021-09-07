using Dalamud.Interface;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using DelvUI.Interface.StatusEffects;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;

namespace DelvUI.Interface
{
    internal static class HUDConstants
    {
        internal static int BaseHUDOffsetY = (int)(ImGui.GetMainViewport().Size.Y * 0.3f);
        internal static int UnitFramesOffsetX = 160;
    }

    public class HudManager
    {
        private DalamudPluginInterface _pluginInterface;
        private PluginConfiguration _pluginConfiguration;
        private Vector2 _origin = ImGui.GetMainViewport().Size / 2f;

        private List<HudElement> _hudElements;
        private List<IHudElementWithActor> _hudElementsUsingPlayer;
        private List<IHudElementWithActor> _hudElementsUsingTarget;
        private List<IHudElementWithActor> _hudElementsUsingTargetOfTarget;
        private List<IHudElementWithActor> _hudElementsUsingFocusTarget;

        public HudManager(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration)
        {
            _pluginInterface = pluginInterface;
            _pluginConfiguration = pluginConfiguration;

            _hudElements = new List<HudElement>();
            _hudElementsUsingPlayer = new List<IHudElementWithActor>();
            _hudElementsUsingTarget = new List<IHudElementWithActor>();
            _hudElementsUsingTargetOfTarget = new List<IHudElementWithActor>();
            _hudElementsUsingFocusTarget = new List<IHudElementWithActor>();

            CreateUnitFrames();
            CreateCastbars();
            CreateStatusEffectsLists();
            CaretMiscElements();
        }
        ~HudManager()
        {
            _hudElements.Clear();
            _hudElementsUsingPlayer.Clear();
            _hudElementsUsingTarget.Clear();
            _hudElementsUsingTargetOfTarget.Clear();
            _hudElementsUsingFocusTarget.Clear();
        }

        private void CreateUnitFrames()
        {
            var playerUnitFrameConfig = DefaultHudElements.PlayerUnitFrame();
            var playerUnitFrame = new UnitFrameHud("playerUnitFrame", playerUnitFrameConfig, _pluginConfiguration);
            _hudElements.Add(playerUnitFrame);
            _hudElementsUsingPlayer.Add(playerUnitFrame);

            var targetUnitFrameConfig = DefaultHudElements.TargetUnitFrame();
            var targetUnitFrame = new UnitFrameHud("targetUnitFrame", targetUnitFrameConfig, _pluginConfiguration);
            _hudElements.Add(targetUnitFrame);
            _hudElementsUsingTarget.Add(targetUnitFrame);

            var targetOfTargetUnitFrameConfig = DefaultHudElements.TargetOfTargetUnitFrame();
            var targetOfTargetUnitFrame = new UnitFrameHud("targetOfTargetUnitFrame", targetOfTargetUnitFrameConfig, _pluginConfiguration);
            _hudElements.Add(targetOfTargetUnitFrame);
            _hudElementsUsingTargetOfTarget.Add(targetOfTargetUnitFrame);

            var focusTargetUnitFrameConfig = DefaultHudElements.FocusTargetUnitFrame();
            var focusTargetUnitFrame = new UnitFrameHud("focusTargetUnitFrame", focusTargetUnitFrameConfig, _pluginConfiguration);
            _hudElements.Add(focusTargetUnitFrame);
            _hudElementsUsingFocusTarget.Add(focusTargetUnitFrame);
        }

        private void CreateCastbars()
        {
            var playerCastbarConfig = DefaultHudElements.PlayerCastbar();
            var playerCastbar = new PlayerCastbarHud("playerCastbar", playerCastbarConfig, _pluginConfiguration);
            _hudElements.Add(playerCastbar);
            _hudElementsUsingPlayer.Add(playerCastbar);

            var targetCastbarConfig = DefaultHudElements.TargetCastbar();
            var targetCastbar = new TargetCastbarHud("targetCastbar", targetCastbarConfig, _pluginConfiguration);
            _hudElements.Add(targetCastbar);
            _hudElementsUsingTarget.Add(targetCastbar);
        }

        private void CreateStatusEffectsLists()
        {
            var playerBuffsConfig = DefaultHudElements.PlayerBuffsList();
            var playerBuffs = new StatusEffectsListHud("playerBuffs", playerBuffsConfig);
            _hudElements.Add(playerBuffs);
            _hudElementsUsingPlayer.Add(playerBuffs);

            var playerDebuffsConfig = DefaultHudElements.PlayerDebuffsList();
            var playerDebuffs = new StatusEffectsListHud("playerDebuffs", playerDebuffsConfig);
            _hudElements.Add(playerDebuffs);
            _hudElementsUsingPlayer.Add(playerDebuffs);

            var targetBuffsConfig = DefaultHudElements.TargetBuffsList();
            var targetBuffs = new StatusEffectsListHud("targetBuffs", targetBuffsConfig);
            _hudElements.Add(targetBuffs);
            _hudElementsUsingTarget.Add(targetBuffs);

            var targetDebuffsConfig = DefaultHudElements.TargetDebuffsList();
            var targetDebuffs = new StatusEffectsListHud("targetDebuffs", targetDebuffsConfig);
            _hudElements.Add(targetDebuffs);
            _hudElementsUsingTarget.Add(targetDebuffs);
        }

        private void CaretMiscElements()
        {
            //gcd indicator
            var gcdIndicatorConfig = DefaultHudElements.GCDIndicator();
            var gcdIndicator = new GCDIndicatorHud("gcdIndicator", gcdIndicatorConfig, _pluginConfiguration);
            _hudElements.Add(gcdIndicator);
            _hudElementsUsingPlayer.Add(gcdIndicator);

            // mp ticker
            var mpTickerConfig = DefaultHudElements.MPTicker();
            var mpTicker = new MPTickerHud("mpTicker", mpTickerConfig);
            _hudElements.Add(mpTicker);
            _hudElementsUsingPlayer.Add(mpTicker);
        }

        public void Draw()
        {
            if (!ShouldBeVisible())
            {
                return;
            }

            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.SetNextWindowSize(ImGui.GetMainViewport().Size);

            var begin = ImGui.Begin(
                "DelvUI2",
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

            AssignActors();

            foreach (var element in _hudElements)
            {
                element.Draw(_origin);
            }

            ImGui.End();
        }

        protected unsafe bool ShouldBeVisible()
        {
            if (_pluginConfiguration.HideHud || _pluginInterface.ClientState.LocalPlayer == null)
            {
                return false;
            }

            var parameterWidget = (AtkUnitBase*)_pluginInterface.Framework.Gui.GetUiObjectByName("_ParameterWidget", 1);
            var fadeMiddleWidget = (AtkUnitBase*)_pluginInterface.Framework.Gui.GetUiObjectByName("FadeMiddle", 1);

            return parameterWidget->IsVisible && !fadeMiddleWidget->IsVisible;
        }

        protected void AssignActors()
        {
            // player
            var player = _pluginInterface.ClientState.LocalPlayer;
            foreach (var element in _hudElementsUsingPlayer)
            {
                element.Actor = player;
            }

            // target
            var target = _pluginInterface.ClientState.Targets.SoftTarget ?? _pluginInterface.ClientState.Targets.CurrentTarget;
            foreach (var element in _hudElementsUsingTarget)
            {
                element.Actor = target;
            }

            // target of target
            var targetOfTarget = Utils.FindTargetOfTarget(target, player, _pluginInterface.ClientState.Actors);
            foreach (var element in _hudElementsUsingTargetOfTarget)
            {
                element.Actor = targetOfTarget;
            }

            // focus
            var focusTarget = _pluginInterface.ClientState.Targets.FocusTarget;
            foreach (var element in _hudElementsUsingFocusTarget)
            {
                element.Actor = focusTarget;
            }
        }
    }
}
