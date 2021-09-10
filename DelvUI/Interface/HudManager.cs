using Dalamud.Interface;
using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using DelvUI.Interface.Jobs;
using DelvUI.Interface.StatusEffects;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DelvUI.Interface
{
    public class HudManager
    {
        private Vector2 _origin = ImGui.GetMainViewport().Size / 2f;

        private List<HudElement> _hudElements;
        private List<IHudElementWithActor> _hudElementsUsingPlayer;
        private List<IHudElementWithActor> _hudElementsUsingTarget;
        private List<IHudElementWithActor> _hudElementsUsingTargetOfTarget;
        private List<IHudElementWithActor> _hudElementsUsingFocusTarget;

        private PrimaryResourceHud _primaryResourceHud;
        private JobHud _jobHud = null;
        private Dictionary<uint, JobHudTypes> _jobsMap;
        private Dictionary<uint, Type> _unsupportedJobsMap;

        public HudManager()
        {
            _hudElements = new List<HudElement>();
            _hudElementsUsingPlayer = new List<IHudElementWithActor>();
            _hudElementsUsingTarget = new List<IHudElementWithActor>();
            _hudElementsUsingTargetOfTarget = new List<IHudElementWithActor>();
            _hudElementsUsingFocusTarget = new List<IHudElementWithActor>();

            CreateJobsMap();
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
            var playerUnitFrameConfig = ConfigurationManager.GetInstance().GetConfigObject<PlayerUnitFrameConfig>();
            var playerUnitFrame = new UnitFrameHud("playerUnitFrame", playerUnitFrameConfig);
            _hudElements.Add(playerUnitFrame);
            _hudElementsUsingPlayer.Add(playerUnitFrame);

            var targetUnitFrameConfig = ConfigurationManager.GetInstance().GetConfigObject<TargetUnitFrameConfig>();
            var targetUnitFrame = new UnitFrameHud("targetUnitFrame", targetUnitFrameConfig);
            _hudElements.Add(targetUnitFrame);
            _hudElementsUsingTarget.Add(targetUnitFrame);

            var targetOfTargetUnitFrameConfig = ConfigurationManager.GetInstance().GetConfigObject<TargetOfTargetUnitFrameConfig>();
            var targetOfTargetUnitFrame = new UnitFrameHud("targetOfTargetUnitFrame", targetOfTargetUnitFrameConfig);
            _hudElements.Add(targetOfTargetUnitFrame);
            _hudElementsUsingTargetOfTarget.Add(targetOfTargetUnitFrame);

            var focusTargetUnitFrameConfig = ConfigurationManager.GetInstance().GetConfigObject<FocusTargetUnitFrameConfig>();
            var focusTargetUnitFrame = new UnitFrameHud("focusTargetUnitFrame", focusTargetUnitFrameConfig);
            _hudElements.Add(focusTargetUnitFrame);
            _hudElementsUsingFocusTarget.Add(focusTargetUnitFrame);
        }

        private void CreateCastbars()
        {
            var playerCastbarConfig = ConfigurationManager.GetInstance().GetConfigObject<PlayerCastbarConfig>();
            var playerCastbar = new PlayerCastbarHud("playerCastbar", playerCastbarConfig);
            _hudElements.Add(playerCastbar);
            _hudElementsUsingPlayer.Add(playerCastbar);

            var targetCastbarConfig = ConfigurationManager.GetInstance().GetConfigObject<TargetCastbarConfig>();
            var targetCastbar = new TargetCastbarHud("targetCastbar", targetCastbarConfig);
            _hudElements.Add(targetCastbar);
            _hudElementsUsingTarget.Add(targetCastbar);
        }

        private void CreateStatusEffectsLists()
        {
            var playerBuffsConfig = ConfigurationManager.GetInstance().GetConfigObject<PlayerBuffsListConfig>();
            var playerBuffs = new StatusEffectsListHud("playerBuffs", playerBuffsConfig);
            _hudElements.Add(playerBuffs);
            _hudElementsUsingPlayer.Add(playerBuffs);

            var playerDebuffsConfig = ConfigurationManager.GetInstance().GetConfigObject<PlayerDebuffsListConfig>();
            var playerDebuffs = new StatusEffectsListHud("playerDebuffs", playerDebuffsConfig);
            _hudElements.Add(playerDebuffs);
            _hudElementsUsingPlayer.Add(playerDebuffs);

            var targetBuffsConfig = ConfigurationManager.GetInstance().GetConfigObject<TargetBuffsListConfig>();
            var targetBuffs = new StatusEffectsListHud("targetBuffs", targetBuffsConfig);
            _hudElements.Add(targetBuffs);
            _hudElementsUsingTarget.Add(targetBuffs);

            var targetDebuffsConfig = ConfigurationManager.GetInstance().GetConfigObject<TargetDebuffsListConfig>();
            var targetDebuffs = new StatusEffectsListHud("targetDebuffs", targetDebuffsConfig);
            _hudElements.Add(targetDebuffs);
            _hudElementsUsingTarget.Add(targetDebuffs);
        }

        private void CaretMiscElements()
        {
            // primary resource bar
            var primaryResourceConfig = ConfigurationManager.GetInstance().GetConfigObject<PrimaryResourceConfig>();
            _primaryResourceHud = new PrimaryResourceHud("primaryResource", primaryResourceConfig);
            _hudElements.Add(_primaryResourceHud);
            _hudElementsUsingPlayer.Add(_primaryResourceHud);

            // gcd indicator
            var gcdIndicatorConfig = ConfigurationManager.GetInstance().GetConfigObject<GCDIndicatorConfig>();
            var gcdIndicator = new GCDIndicatorHud("gcdIndicator", gcdIndicatorConfig);
            _hudElements.Add(gcdIndicator);
            _hudElementsUsingPlayer.Add(gcdIndicator);

            // mp ticker
            var mpTickerConfig = ConfigurationManager.GetInstance().GetConfigObject<MPTickerConfig>();
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

            UpdateJob();
            AssignActors();

            foreach (var element in _hudElements)
            {
                element.Draw(_origin);
            }

            if (_jobHud != null && _jobHud.Config.Enabled)
            {
                _jobHud.Draw(_origin);
            }

            ImGui.End();
        }

        protected unsafe bool ShouldBeVisible()
        {
            if (!ConfigurationManager.GetInstance().ShowHUD || Plugin.GetPluginInterface().ClientState.LocalPlayer == null)
            {
                return false;
            }

            var parameterWidget = (AtkUnitBase*)Plugin.GetPluginInterface().Framework.Gui.GetUiObjectByName("_ParameterWidget", 1);
            var fadeMiddleWidget = (AtkUnitBase*)Plugin.GetPluginInterface().Framework.Gui.GetUiObjectByName("FadeMiddle", 1);

            return parameterWidget->IsVisible && !fadeMiddleWidget->IsVisible;
        }

        private void UpdateJob()
        {
            var newJobId = Plugin.GetPluginInterface().ClientState.LocalPlayer.ClassJob.Id;
            if (_jobHud != null && _jobHud.Config.JobId == newJobId)
            {
                return;
            }

            JobConfig config = null;

            // unsupported jobs
            if (_unsupportedJobsMap.ContainsKey(newJobId) && _unsupportedJobsMap.TryGetValue(newJobId, out var type))
            {
                config = (JobConfig)Activator.CreateInstance(type);
                _jobHud = new JobHud(type.FullName, config);
            }

            // supported jobs
            if (_jobsMap.TryGetValue(newJobId, out var types))
            {
                config = (JobConfig)ConfigurationManager.GetInstance().GetConfigObjectForType(types.ConfigType);
                _jobHud = (JobHud)Activator.CreateInstance(types.HudType, types.HudType.FullName, config);
            }

            if (config != null)
            {
                _primaryResourceHud.ResourceType = config.UseDefaulyPrimaryResourceBar ? config.PrimaryResourceType : PrimaryResourceTypes.None;
            }
        }

        private void AssignActors()
        {
            var pluginInterface = Plugin.GetPluginInterface();

            // player
            var player = pluginInterface.ClientState.LocalPlayer;
            foreach (var element in _hudElementsUsingPlayer)
            {
                element.Actor = player;

                if (_jobHud != null)
                {
                    _jobHud.Actor = player;
                }
            }

            // target
            var target = pluginInterface.ClientState.Targets.SoftTarget ?? pluginInterface.ClientState.Targets.CurrentTarget;
            foreach (var element in _hudElementsUsingTarget)
            {
                element.Actor = target;
            }

            // target of target
            var targetOfTarget = Utils.FindTargetOfTarget(target, player, pluginInterface.ClientState.Actors);
            foreach (var element in _hudElementsUsingTargetOfTarget)
            {
                element.Actor = targetOfTarget;
            }

            // focus
            var focusTarget = pluginInterface.ClientState.Targets.FocusTarget;
            foreach (var element in _hudElementsUsingFocusTarget)
            {
                element.Actor = focusTarget;
            }
        }

        protected void CreateJobsMap()
        {
            _jobsMap = new Dictionary<uint, JobHudTypes>()
            {
                // tanks
                [JobIDs.DRK] = new JobHudTypes(typeof(DarkKnightHud), typeof(DarkKnightConfig)),
                [JobIDs.PLD] = new JobHudTypes(typeof(PaladinHud), typeof(PaladinConfig)),
                [JobIDs.WAR] = new JobHudTypes(typeof(WarriorHud), typeof(WarriorConfig)),
                [JobIDs.GNB] = new JobHudTypes(typeof(GunbreakerHud), typeof(GunbreakerConfig)),

                // healers
                [JobIDs.SCH] = new JobHudTypes(typeof(ScholarHud), typeof(ScholarConfig)),
                [JobIDs.WHM] = new JobHudTypes(typeof(WhiteMageHud), typeof(WhiteMageConfig)),
                [JobIDs.AST] = new JobHudTypes(typeof(AstrologianHud), typeof(AstrologianConfig)),

                // melee
                [JobIDs.MNK] = new JobHudTypes(typeof(MonkHud), typeof(MonkConfig)),
                [JobIDs.DRG] = new JobHudTypes(typeof(DragoonHud), typeof(DragoonConfig)),
                [JobIDs.NIN] = new JobHudTypes(typeof(NinjaHud), typeof(NinjaConfig)),
                [JobIDs.SAM] = new JobHudTypes(typeof(SamuraiHud), typeof(SamuraiConfig)),

                // ranged
                [JobIDs.MCH] = new JobHudTypes(typeof(MachinistHud), typeof(MachinistConfig)),
                [JobIDs.DNC] = new JobHudTypes(typeof(DancerHud), typeof(DancerConfig)),
                [JobIDs.DNC] = new JobHudTypes(typeof(BardHud), typeof(BardConfig)),

                // casters
                [JobIDs.BLM] = new JobHudTypes(typeof(BlackMageHud), typeof(BlackMageConfig)),
                [JobIDs.RDM] = new JobHudTypes(typeof(RedMageHud), typeof(RedMageConfig)),
                [JobIDs.SMN] = new JobHudTypes(typeof(SummonerHud), typeof(SummonerConfig))
            };

            _unsupportedJobsMap = new Dictionary<uint, Type>()
            {
                [JobIDs.BLU] = typeof(BlueMageConfig),

                // base jobs
                [JobIDs.GLD] = typeof(GladiatorConfig),
                [JobIDs.MRD] = typeof(MarauderConfig),
                [JobIDs.PGL] = typeof(PugilistConfig),
                [JobIDs.LNC] = typeof(LancerConfig),
                [JobIDs.ROG] = typeof(RogueConfig),
                [JobIDs.ARC] = typeof(ArcherConfig),
                [JobIDs.THM] = typeof(ThaumaturgeConfig),
                [JobIDs.ACN] = typeof(ArcanistConfig),
                [JobIDs.CNJ] = typeof(ConjurerConfig),

                // crafters
                [JobIDs.CRP] = typeof(CarpenterConfig),
                [JobIDs.BSM] = typeof(BlacksmithConfig),
                [JobIDs.ARM] = typeof(ArmorerConfig),
                [JobIDs.GSM] = typeof(GoldsmithConfig),
                [JobIDs.LTW] = typeof(LeatherworkerConfig),
                [JobIDs.WVR] = typeof(WeaverConfig),
                [JobIDs.ALC] = typeof(AlchemistConfig),
                [JobIDs.CUL] = typeof(CulinarianConfig),

                // gatherers
                [JobIDs.MIN] = typeof(MinerConfig),
                [JobIDs.BOT] = typeof(BotanistConfig),
                [JobIDs.FSH] = typeof(FisherConfig),
            };
        }
    }

    internal struct JobHudTypes
    {
        public Type HudType;
        public Type ConfigType;

        public JobHudTypes(Type hudType, Type configType)
        {
            HudType = hudType;
            ConfigType = configType;
        }
    }

    internal static class HUDConstants
    {
        internal static int BaseHUDOffsetY = (int)(ImGui.GetMainViewport().Size.Y * 0.3f);
        internal static int UnitFramesOffsetX = 160;
        internal static int PlayerCastbarY = BaseHUDOffsetY - 12;
        internal static int JobHudsBaseY = PlayerCastbarY - 14;
        internal static Vector2 DefaultBigUnitFrameSize = new Vector2(270, 50);
        internal static Vector2 DefaultSmallUnitFrameSize = new Vector2(120, 20);
        internal static Vector2 DefaultStatusEffectsListSize = new Vector2(292, 82);
    }
}
