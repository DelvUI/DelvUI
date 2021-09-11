using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Interface;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using DelvUI.Interface.Jobs;
using DelvUI.Interface.StatusEffects;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;

namespace DelvUI.Interface
{
    public class HudManager
    {
        private readonly Vector2 _origin = ImGui.GetMainViewport().Size / 2f;

        private List<HudElement> _hudElements;
        private List<IHudElementWithActor> _hudElementsUsingPlayer;
        private List<IHudElementWithActor> _hudElementsUsingTarget;
        private List<IHudElementWithActor> _hudElementsUsingTargetOfTarget;
        private List<IHudElementWithActor> _hudElementsUsingFocusTarget;

        private PrimaryResourceHud _primaryResourceHud;
        private JobHud _jobHud;
        private Dictionary<uint, JobHudTypes> _jobsMap;
        private Dictionary<uint, Type> _unsupportedJobsMap;

        public HudManager()
        {
            CreateJobsMap();

            ConfigurationManager.GetInstance().ResetEvent += OnConfigReset;
            CreateHudElements();
        }

        ~HudManager()
        {
            _hudElements.Clear();
            _hudElementsUsingPlayer.Clear();
            _hudElementsUsingTarget.Clear();
            _hudElementsUsingTargetOfTarget.Clear();
            _hudElementsUsingFocusTarget.Clear();
        }

        private void OnConfigReset(object sender, EventArgs e) { CreateHudElements(); }

        private void CreateHudElements()
        {
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

        private void CreateUnitFrames()
        {
            PlayerUnitFrameConfig playerUnitFrameConfig = ConfigurationManager.GetInstance().GetConfigObject<PlayerUnitFrameConfig>();
            UnitFrameHud playerUnitFrame = new("playerUnitFrame", playerUnitFrameConfig);
            _hudElements.Add(playerUnitFrame);
            _hudElementsUsingPlayer.Add(playerUnitFrame);

            TargetUnitFrameConfig targetUnitFrameConfig = ConfigurationManager.GetInstance().GetConfigObject<TargetUnitFrameConfig>();
            UnitFrameHud targetUnitFrame = new("targetUnitFrame", targetUnitFrameConfig);
            _hudElements.Add(targetUnitFrame);
            _hudElementsUsingTarget.Add(targetUnitFrame);

            TargetOfTargetUnitFrameConfig targetOfTargetUnitFrameConfig = ConfigurationManager.GetInstance().GetConfigObject<TargetOfTargetUnitFrameConfig>();
            UnitFrameHud targetOfTargetUnitFrame = new("targetOfTargetUnitFrame", targetOfTargetUnitFrameConfig);
            _hudElements.Add(targetOfTargetUnitFrame);
            _hudElementsUsingTargetOfTarget.Add(targetOfTargetUnitFrame);

            FocusTargetUnitFrameConfig focusTargetUnitFrameConfig = ConfigurationManager.GetInstance().GetConfigObject<FocusTargetUnitFrameConfig>();
            UnitFrameHud focusTargetUnitFrame = new("focusTargetUnitFrame", focusTargetUnitFrameConfig);
            _hudElements.Add(focusTargetUnitFrame);
            _hudElementsUsingFocusTarget.Add(focusTargetUnitFrame);
        }

        private void CreateCastbars()
        {
            PlayerCastbarConfig playerCastbarConfig = ConfigurationManager.GetInstance().GetConfigObject<PlayerCastbarConfig>();
            PlayerCastbarHud playerCastbar = new("playerCastbar", playerCastbarConfig);
            _hudElements.Add(playerCastbar);
            _hudElementsUsingPlayer.Add(playerCastbar);

            TargetCastbarConfig targetCastbarConfig = ConfigurationManager.GetInstance().GetConfigObject<TargetCastbarConfig>();
            TargetCastbarHud targetCastbar = new("targetCastbar", targetCastbarConfig);
            _hudElements.Add(targetCastbar);
            _hudElementsUsingTarget.Add(targetCastbar);
        }

        private void CreateStatusEffectsLists()
        {
            PlayerBuffsListConfig playerBuffsConfig = ConfigurationManager.GetInstance().GetConfigObject<PlayerBuffsListConfig>();
            StatusEffectsListHud playerBuffs = new("playerBuffs", playerBuffsConfig);
            _hudElements.Add(playerBuffs);
            _hudElementsUsingPlayer.Add(playerBuffs);

            PlayerDebuffsListConfig playerDebuffsConfig = ConfigurationManager.GetInstance().GetConfigObject<PlayerDebuffsListConfig>();
            StatusEffectsListHud playerDebuffs = new("playerDebuffs", playerDebuffsConfig);
            _hudElements.Add(playerDebuffs);
            _hudElementsUsingPlayer.Add(playerDebuffs);

            TargetBuffsListConfig targetBuffsConfig = ConfigurationManager.GetInstance().GetConfigObject<TargetBuffsListConfig>();
            StatusEffectsListHud targetBuffs = new("targetBuffs", targetBuffsConfig);
            _hudElements.Add(targetBuffs);
            _hudElementsUsingTarget.Add(targetBuffs);

            TargetDebuffsListConfig targetDebuffsConfig = ConfigurationManager.GetInstance().GetConfigObject<TargetDebuffsListConfig>();
            StatusEffectsListHud targetDebuffs = new("targetDebuffs", targetDebuffsConfig);
            _hudElements.Add(targetDebuffs);
            _hudElementsUsingTarget.Add(targetDebuffs);
        }

        private void CaretMiscElements()
        {
            // Primary Resource Bar
            PrimaryResourceConfig primaryResourceConfig = ConfigurationManager.GetInstance().GetConfigObject<PrimaryResourceConfig>();
            _primaryResourceHud = new PrimaryResourceHud("primaryResource", primaryResourceConfig);
            _hudElements.Add(_primaryResourceHud);
            _hudElementsUsingPlayer.Add(_primaryResourceHud);

            // GCD Indicator
            GCDIndicatorConfig gcdIndicatorConfig = ConfigurationManager.GetInstance().GetConfigObject<GCDIndicatorConfig>();
            GCDIndicatorHud gcdIndicator = new("gcdIndicator", gcdIndicatorConfig);
            _hudElements.Add(gcdIndicator);
            _hudElementsUsingPlayer.Add(gcdIndicator);

            // MP Ticker
            MPTickerConfig mpTickerConfig = ConfigurationManager.GetInstance().GetConfigObject<MPTickerConfig>();
            MPTickerHud mpTicker = new("mpTicker", mpTickerConfig);
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

            bool begin = ImGui.Begin(
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

            foreach (HudElement element in _hudElements)
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

            AtkUnitBase* parameterWidget = (AtkUnitBase*)Plugin.GetPluginInterface().Framework.Gui.GetUiObjectByName("_ParameterWidget", 1);
            AtkUnitBase* fadeMiddleWidget = (AtkUnitBase*)Plugin.GetPluginInterface().Framework.Gui.GetUiObjectByName("FadeMiddle", 1);

            return parameterWidget->IsVisible && !fadeMiddleWidget->IsVisible;
        }

        private void UpdateJob()
        {
            uint newJobId = Plugin.GetPluginInterface().ClientState.LocalPlayer.ClassJob.Id;

            if (_jobHud != null && _jobHud.Config.JobId == newJobId)
            {
                return;
            }

            JobConfig config = null;

            // Unsupported Jobs
            if (_unsupportedJobsMap.ContainsKey(newJobId) && _unsupportedJobsMap.TryGetValue(newJobId, out Type type))
            {
                config = (JobConfig)Activator.CreateInstance(type);
                _jobHud = new JobHud(type.FullName, config);
            }

            // Supported Jobs
            if (_jobsMap.TryGetValue(newJobId, out JobHudTypes types))
            {
                config = (JobConfig)ConfigurationManager.GetInstance().GetConfigObjectForType(types.ConfigType);
                _jobHud = (JobHud)Activator.CreateInstance(types.HudType, types.HudType.FullName, config);
            }

            if (config != null)
            {
                _primaryResourceHud.ResourceType = config.UseDefaultPrimaryResourceBar ? config.PrimaryResourceType : PrimaryResourceTypes.None;
            }
        }

        private void AssignActors()
        {
            DalamudPluginInterface pluginInterface = Plugin.GetPluginInterface();

            // Player
            PlayerCharacter player = pluginInterface.ClientState.LocalPlayer;

            foreach (IHudElementWithActor element in _hudElementsUsingPlayer)
            {
                element.Actor = player;

                if (_jobHud != null)
                {
                    _jobHud.Actor = player;
                }
            }

            // Target
            Actor target = pluginInterface.ClientState.Targets.SoftTarget ?? pluginInterface.ClientState.Targets.CurrentTarget;

            foreach (IHudElementWithActor element in _hudElementsUsingTarget)
            {
                element.Actor = target;
            }

            // Target of Target
            Actor targetOfTarget = Utils.FindTargetOfTarget(target, player, pluginInterface.ClientState.Actors);

            foreach (IHudElementWithActor element in _hudElementsUsingTargetOfTarget)
            {
                element.Actor = targetOfTarget;
            }

            // Focus
            Actor focusTarget = pluginInterface.ClientState.Targets.FocusTarget;

            foreach (IHudElementWithActor element in _hudElementsUsingFocusTarget)
            {
                element.Actor = focusTarget;
            }
        }

        protected void CreateJobsMap()
        {
            _jobsMap = new Dictionary<uint, JobHudTypes>
            {
                // Tanks
                [JobIDs.PLD] = new(typeof(PaladinHud), typeof(PaladinConfig)),
                [JobIDs.WAR] = new(typeof(WarriorHud), typeof(WarriorConfig)),
                [JobIDs.DRK] = new(typeof(DarkKnightHud), typeof(DarkKnightConfig)),
                [JobIDs.GNB] = new(typeof(GunbreakerHud), typeof(GunbreakerConfig)),

                // Healers
                [JobIDs.WHM] = new(typeof(WhiteMageHud), typeof(WhiteMageConfig)),
                [JobIDs.SCH] = new(typeof(ScholarHud), typeof(ScholarConfig)),
                [JobIDs.AST] = new(typeof(AstrologianHud), typeof(AstrologianConfig)),

                // Melee
                [JobIDs.MNK] = new(typeof(MonkHud), typeof(MonkConfig)),
                [JobIDs.DRG] = new(typeof(DragoonHud), typeof(DragoonConfig)),
                [JobIDs.NIN] = new(typeof(NinjaHud), typeof(NinjaConfig)),
                [JobIDs.SAM] = new(typeof(SamuraiHud), typeof(SamuraiConfig)),

                // Ranged
                [JobIDs.BRD] = new(typeof(BardHud), typeof(BardConfig)),
                [JobIDs.MCH] = new(typeof(MachinistHud), typeof(MachinistConfig)),
                [JobIDs.DNC] = new(typeof(DancerHud), typeof(DancerConfig)),

                // Casters
                [JobIDs.BLM] = new(typeof(BlackMageHud), typeof(BlackMageConfig)),
                [JobIDs.SMN] = new(typeof(SummonerHud), typeof(SummonerConfig)),
                [JobIDs.RDM] = new(typeof(RedMageHud), typeof(RedMageConfig))
            };

            _unsupportedJobsMap = new Dictionary<uint, Type>
            {
                // Casters
                [JobIDs.BLU] = typeof(BlueMageConfig),

                // Base jobs
                [JobIDs.GLD] = typeof(GladiatorConfig),
                [JobIDs.MRD] = typeof(MarauderConfig),
                [JobIDs.PGL] = typeof(PugilistConfig),
                [JobIDs.LNC] = typeof(LancerConfig),
                [JobIDs.ROG] = typeof(RogueConfig),
                [JobIDs.ARC] = typeof(ArcherConfig),
                [JobIDs.THM] = typeof(ThaumaturgeConfig),
                [JobIDs.ACN] = typeof(ArcanistConfig),
                [JobIDs.CNJ] = typeof(ConjurerConfig),

                // Crafters
                [JobIDs.CRP] = typeof(CarpenterConfig),
                [JobIDs.BSM] = typeof(BlacksmithConfig),
                [JobIDs.ARM] = typeof(ArmorerConfig),
                [JobIDs.GSM] = typeof(GoldsmithConfig),
                [JobIDs.LTW] = typeof(LeatherworkerConfig),
                [JobIDs.WVR] = typeof(WeaverConfig),
                [JobIDs.ALC] = typeof(AlchemistConfig),
                [JobIDs.CUL] = typeof(CulinarianConfig),

                // Gatherers
                [JobIDs.MIN] = typeof(MinerConfig),
                [JobIDs.BOT] = typeof(BotanistConfig),
                [JobIDs.FSH] = typeof(FisherConfig)
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
        internal static int PlayerCastbarY = BaseHUDOffsetY - 13;
        internal static int JobHudsBaseY = PlayerCastbarY - 14;
        internal static Vector2 DefaultBigUnitFrameSize = new(270, 50);
        internal static Vector2 DefaultSmallUnitFrameSize = new(120, 20);
        internal static Vector2 DefaultStatusEffectsListSize = new(292, 82);
    }
}
