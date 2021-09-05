using Dalamud.Game.ClientState.Actors.Types;
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
    public class HudManager
    {
        private DalamudPluginInterface _pluginInterface;
        private PluginConfiguration _pluginConfiguration;
        private Vector2 _origin = ImGui.GetMainViewport().Size / 2f;

        private List<HudElement> _hudElements;
        private UnitFrameHud _playerUnitFrame;
        private UnitFrameHud _targetUnitFrame;
        private UnitFrameHud _targetOfTargetUnitFrame;
        private UnitFrameHud _focusTargetUnitFrame;

        public HudManager(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration)
        {
            _pluginInterface = pluginInterface;
            _pluginConfiguration = pluginConfiguration;

            _hudElements = new List<HudElement>();

            var playerUnitFrameConfig = PlayerUnitFrameConfig.DefaultUnitFrame();
            _playerUnitFrame = new UnitFrameHud(playerUnitFrameConfig, pluginConfiguration);
            _hudElements.Add(_playerUnitFrame);

            var targetUnitFrameConfig = TargetUnitFrameConfig.DefaultUnitFrame();
            _targetUnitFrame = new UnitFrameHud(targetUnitFrameConfig, pluginConfiguration);
            _hudElements.Add(_targetUnitFrame);

            var targetOfTargetUnitFrameConfig = TargetOfTargetUnitFrameConfig.DefaultUnitFrame();
            _targetOfTargetUnitFrame = new UnitFrameHud(targetOfTargetUnitFrameConfig, pluginConfiguration);
            _hudElements.Add(_targetOfTargetUnitFrame);

            var focusTargetUnitFrameConfig = FocusTargetUnitFrameConfig.DefaultUnitFrame();
            _focusTargetUnitFrame = new UnitFrameHud(focusTargetUnitFrameConfig, pluginConfiguration);
            _hudElements.Add(_focusTargetUnitFrame);
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
            _playerUnitFrame.Actor = player;

            // target
            var target = _pluginInterface.ClientState.Targets.SoftTarget ?? _pluginInterface.ClientState.Targets.CurrentTarget;
            _targetUnitFrame.Actor = _pluginInterface.ClientState.Targets.SoftTarget ?? _pluginInterface.ClientState.Targets.CurrentTarget;

            // target of target
            _targetOfTargetUnitFrame.Actor = Utils.FindTargetOfTarget(target, player, _pluginInterface.ClientState.Actors);

            // focus
            _focusTargetUnitFrame.Actor = _pluginInterface.ClientState.Targets.FocusTarget;
        }
    }
}
