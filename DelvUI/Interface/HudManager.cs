using Dalamud.Interface;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;

namespace DelvUI.Interface
{
    internal static class HUDConstants
    {
        internal static int BaseHUDOffsetY = (int)(ImGui.GetMainViewport().Size.Y * 0.3f);
    }

    public class HudManager
    {
        private DalamudPluginInterface _pluginInterface;
        private PluginConfiguration _pluginConfiguration;
        private Vector2 _origin = ImGui.GetMainViewport().Size / 2f;

        private List<HudElement> _hudElements;
        private List<IHudElementWithActor> _hudElementsUsingPlayer;
        private List<IHudElementWithActor> _hudElementsUsingTarget;

        private UnitFrameHud _playerUnitFrame;
        private UnitFrameHud _targetUnitFrame;
        private UnitFrameHud _targetOfTargetUnitFrame;
        private UnitFrameHud _focusTargetUnitFrame;

        private CastbarHud _playerCastbar;
        private CastbarHud _targetCastbar;

        public HudManager(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration)
        {
            _pluginInterface = pluginInterface;
            _pluginConfiguration = pluginConfiguration;

            _hudElements = new List<HudElement>();
            _hudElementsUsingPlayer = new List<IHudElementWithActor>();
            _hudElementsUsingTarget = new List<IHudElementWithActor>();

            // unit frames
            var playerUnitFrameConfig = PlayerUnitFrameConfig.DefaultUnitFrame();
            _playerUnitFrame = new UnitFrameHud("playerUnitFrame", playerUnitFrameConfig, pluginConfiguration);
            _hudElements.Add(_playerUnitFrame);
            _hudElementsUsingPlayer.Add(_playerUnitFrame);

            var targetUnitFrameConfig = TargetUnitFrameConfig.DefaultUnitFrame();
            _targetUnitFrame = new UnitFrameHud("targetUnitFrame", targetUnitFrameConfig, pluginConfiguration);
            _hudElements.Add(_targetUnitFrame);
            _hudElementsUsingTarget.Add(_targetUnitFrame);

            var targetOfTargetUnitFrameConfig = TargetOfTargetUnitFrameConfig.DefaultUnitFrame();
            _targetOfTargetUnitFrame = new UnitFrameHud("targetOfTargetUnitFrame", targetOfTargetUnitFrameConfig, pluginConfiguration);
            _hudElements.Add(_targetOfTargetUnitFrame);

            var focusTargetUnitFrameConfig = FocusTargetUnitFrameConfig.DefaultUnitFrame();
            _focusTargetUnitFrame = new UnitFrameHud("focusTargetUnitFrame", focusTargetUnitFrameConfig, pluginConfiguration);
            _hudElements.Add(_focusTargetUnitFrame);

            // cast bars
            var playerCastbarConfig = PlayerCastbarConfig.DefaultCastbar();
            _playerCastbar = new PlayerCastbarHud("playerCastbar", playerCastbarConfig, pluginConfiguration);
            _hudElements.Add(_playerCastbar);
            _hudElementsUsingPlayer.Add(_playerCastbar);

            var targetCastbarConfig = TargetCastbarConfig.DefaultCastbar();
            _targetCastbar = new TargetCastbarHud("targetCastbar", targetCastbarConfig, pluginConfiguration);
            _hudElements.Add(_targetCastbar);
            _hudElementsUsingTarget.Add(_targetCastbar);
        }

        ~HudManager()
        {
            _hudElements.Clear();
            _hudElementsUsingTarget.Clear();
            _hudElementsUsingTarget.Clear();
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
            _targetOfTargetUnitFrame.Actor = Utils.FindTargetOfTarget(target, player, _pluginInterface.ClientState.Actors);

            // focus
            _focusTargetUnitFrame.Actor = _pluginInterface.ClientState.Targets.FocusTarget;
        }
    }
}
