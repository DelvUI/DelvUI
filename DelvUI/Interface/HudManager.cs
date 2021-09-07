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

        private UnitFrameHud _playerUnitFrame;
        private UnitFrameHud _targetUnitFrame;
        private UnitFrameHud _targetOfTargetUnitFrame;
        private UnitFrameHud _focusTargetUnitFrame;

        private CastbarHud _playerCastbar;
        private CastbarHud _targetCastbar;

        private StatusEffectsListHud _playerBuffs;
        private StatusEffectsListHud _playerDebuffs;
        private StatusEffectsListHud _targetBuffs;
        private StatusEffectsListHud _targetDebuffs;

        public HudManager(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration)
        {
            _pluginInterface = pluginInterface;
            _pluginConfiguration = pluginConfiguration;

            _hudElements = new List<HudElement>();
            _hudElementsUsingPlayer = new List<IHudElementWithActor>();
            _hudElementsUsingTarget = new List<IHudElementWithActor>();

            CreateUnitFrames();
            CreateCastbars();
            CreateStatusEffectsLists();
        }
        ~HudManager()
        {
            _hudElements.Clear();
            _hudElementsUsingTarget.Clear();
            _hudElementsUsingTarget.Clear();
        }

        private void CreateUnitFrames()
        {
            var playerUnitFrameConfig = DefaultUnitFrames.PlayerUnitFrame();
            _playerUnitFrame = new UnitFrameHud("playerUnitFrame", playerUnitFrameConfig, _pluginConfiguration);
            _hudElements.Add(_playerUnitFrame);
            _hudElementsUsingPlayer.Add(_playerUnitFrame);

            var targetUnitFrameConfig = DefaultUnitFrames.TargetUnitFrame();
            _targetUnitFrame = new UnitFrameHud("targetUnitFrame", targetUnitFrameConfig, _pluginConfiguration);
            _hudElements.Add(_targetUnitFrame);
            _hudElementsUsingTarget.Add(_targetUnitFrame);

            var targetOfTargetUnitFrameConfig = DefaultUnitFrames.TargetOfTargetUnitFrame();
            _targetOfTargetUnitFrame = new UnitFrameHud("targetOfTargetUnitFrame", targetOfTargetUnitFrameConfig, _pluginConfiguration);
            _hudElements.Add(_targetOfTargetUnitFrame);

            var focusTargetUnitFrameConfig = DefaultUnitFrames.FocusTargetUnitFrame();
            _focusTargetUnitFrame = new UnitFrameHud("focusTargetUnitFrame", focusTargetUnitFrameConfig, _pluginConfiguration);
            _hudElements.Add(_focusTargetUnitFrame);
        }

        private void CreateCastbars()
        {
            var playerCastbarConfig = PlayerCastbarConfig.DefaultCastbar();
            _playerCastbar = new PlayerCastbarHud("playerCastbar", playerCastbarConfig, _pluginConfiguration);
            _hudElements.Add(_playerCastbar);
            _hudElementsUsingPlayer.Add(_playerCastbar);

            var targetCastbarConfig = TargetCastbarConfig.DefaultCastbar();
            targetCastbarConfig.Enabled = false;
            _targetCastbar = new TargetCastbarHud("targetCastbar", targetCastbarConfig, _pluginConfiguration);
            _hudElements.Add(_targetCastbar);
            _hudElementsUsingTarget.Add(_targetCastbar);
        }

        private void CreateStatusEffectsLists()
        {
            var playerBuffsConfig = DefaultStatusEffectsLists.PlayerBuffsList();
            _playerBuffs = new StatusEffectsListHud("playerBuffs", playerBuffsConfig);
            _hudElements.Add(_playerBuffs);
            _hudElementsUsingPlayer.Add(_playerBuffs);

            var playerDebuffsConfig = DefaultStatusEffectsLists.PlayerDebuffsList();
            _playerDebuffs = new StatusEffectsListHud("playerDebuffs", playerDebuffsConfig);
            _hudElements.Add(_playerDebuffs);
            _hudElementsUsingPlayer.Add(_playerDebuffs);

            var targetBuffsConfig = DefaultStatusEffectsLists.TargetBuffsList();
            _targetBuffs = new StatusEffectsListHud("targetBuffs", targetBuffsConfig);
            _hudElements.Add(_targetBuffs);
            _hudElementsUsingTarget.Add(_targetBuffs);

            var targetDebuffsConfig = DefaultStatusEffectsLists.TargetDebuffsList();
            _targetDebuffs = new StatusEffectsListHud("targetDebuffs", targetDebuffsConfig);
            _hudElements.Add(_targetDebuffs);
            _hudElementsUsingTarget.Add(_targetDebuffs);
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
