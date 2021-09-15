using Dalamud.Interface;
using DelvUI.Config;
using DelvUI.Config.Attributes;
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

        private GridConfig _gridConfig;
        private DraggableHudElement _selectedElement = null;

        private List<DraggableHudElement> _hudElements;
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
            CreateJobsMap();

            ConfigurationManager.GetInstance().ResetEvent += OnConfigReset;
            ConfigurationManager.GetInstance().LockEvent += OnHUDLockChanged;

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

        private void OnConfigReset(object sender, EventArgs e)
        {
            CreateHudElements();
            _jobHud = null;
        }

        private void OnHUDLockChanged(object sender, EventArgs e)
        {
            var draggingEnabled = !ConfigurationManager.GetInstance().LockHUD;

            foreach (var element in _hudElements)
            {
                element.DraggingEnabled = draggingEnabled;
                element.Selected = false;
            }

            if (_jobHud != null)
            {
                _jobHud.DraggingEnabled = draggingEnabled;
            }

            _selectedElement = null;
        }

        private void OnDraggableElementSelected(object sender, EventArgs e)
        {
            foreach (var element in _hudElements)
            {
                element.Selected = element == sender;
            }

            if (_jobHud != null)
            {
                _jobHud.Selected = _jobHud == sender;
            }

            _selectedElement = (DraggableHudElement)sender;
        }

        private void CreateHudElements()
        {
            _gridConfig = ConfigurationManager.GetInstance().GetConfigObject<GridConfig>();

            _hudElements = new List<DraggableHudElement>();
            _hudElementsUsingPlayer = new List<IHudElementWithActor>();
            _hudElementsUsingTarget = new List<IHudElementWithActor>();
            _hudElementsUsingTargetOfTarget = new List<IHudElementWithActor>();
            _hudElementsUsingFocusTarget = new List<IHudElementWithActor>();

            CreateUnitFrames();
            CreateCastbars();
            CreateStatusEffectsLists();
            CreateMiscElements();

            foreach (var element in _hudElements)
            {
                element.SelectEvent += OnDraggableElementSelected;
            }
        }

        private void CreateUnitFrames()
        {
            var playerUnitFrameConfig = ConfigurationManager.GetInstance().GetConfigObject<PlayerUnitFrameConfig>();
            var playerUnitFrame = new UnitFrameHud("playerUnitFrame", playerUnitFrameConfig, "Player");
            _hudElements.Add(playerUnitFrame);
            _hudElementsUsingPlayer.Add(playerUnitFrame);

            var targetUnitFrameConfig = ConfigurationManager.GetInstance().GetConfigObject<TargetUnitFrameConfig>();
            var targetUnitFrame = new UnitFrameHud("targetUnitFrame", targetUnitFrameConfig, "Target");
            _hudElements.Add(targetUnitFrame);
            _hudElementsUsingTarget.Add(targetUnitFrame);

            var targetOfTargetUnitFrameConfig = ConfigurationManager.GetInstance().GetConfigObject<TargetOfTargetUnitFrameConfig>();
            var targetOfTargetUnitFrame = new UnitFrameHud("targetOfTargetUnitFrame", targetOfTargetUnitFrameConfig, "Target of Target");
            _hudElements.Add(targetOfTargetUnitFrame);
            _hudElementsUsingTargetOfTarget.Add(targetOfTargetUnitFrame);

            var focusTargetUnitFrameConfig = ConfigurationManager.GetInstance().GetConfigObject<FocusTargetUnitFrameConfig>();
            var focusTargetUnitFrame = new UnitFrameHud("focusTargetUnitFrame", focusTargetUnitFrameConfig, "Focus Target");
            _hudElements.Add(focusTargetUnitFrame);
            _hudElementsUsingFocusTarget.Add(focusTargetUnitFrame);
        }

        private void CreateCastbars()
        {
            var playerCastbarConfig = ConfigurationManager.GetInstance().GetConfigObject<PlayerCastbarConfig>();
            var playerCastbar = new PlayerCastbarHud("playerCastbar", playerCastbarConfig, "Player Castbar");
            _hudElements.Add(playerCastbar);
            _hudElementsUsingPlayer.Add(playerCastbar);

            var targetCastbarConfig = ConfigurationManager.GetInstance().GetConfigObject<TargetCastbarConfig>();
            var targetCastbar = new TargetCastbarHud("targetCastbar", targetCastbarConfig, "Target Castbar");
            _hudElements.Add(targetCastbar);
            _hudElementsUsingTarget.Add(targetCastbar);

            var targetOfTargetCastbarConfig = ConfigurationManager.GetInstance().GetConfigObject<TargetOfTargetCastbarConfig>();
            var targetOfTargetCastbar = new CastbarHud("targetOfTargetCastbar", targetOfTargetCastbarConfig, "ToT Castbar");
            _hudElements.Add(targetOfTargetCastbar);
            _hudElementsUsingTargetOfTarget.Add(targetOfTargetCastbar);

            var focusTargetCastbarConfig = ConfigurationManager.GetInstance().GetConfigObject<FocusTargetCastbarConfig>();
            var focusTargetCastbar = new CastbarHud("focusTargetCastbar", focusTargetCastbarConfig, "Focus Castbar");
            _hudElements.Add(focusTargetCastbar);
            _hudElementsUsingFocusTarget.Add(focusTargetCastbar);
        }

        private void CreateStatusEffectsLists()
        {
            var playerBuffsConfig = ConfigurationManager.GetInstance().GetConfigObject<PlayerBuffsListConfig>();
            var playerBuffs = new StatusEffectsListHud("playerBuffs", playerBuffsConfig, "Buffs");
            _hudElements.Add(playerBuffs);
            _hudElementsUsingPlayer.Add(playerBuffs);

            var playerDebuffsConfig = ConfigurationManager.GetInstance().GetConfigObject<PlayerDebuffsListConfig>();
            var playerDebuffs = new StatusEffectsListHud("playerDebuffs", playerDebuffsConfig, "Debufffs");
            _hudElements.Add(playerDebuffs);
            _hudElementsUsingPlayer.Add(playerDebuffs);

            var targetBuffsConfig = ConfigurationManager.GetInstance().GetConfigObject<TargetBuffsListConfig>();
            var targetBuffs = new StatusEffectsListHud("targetBuffs", targetBuffsConfig, "Target Buffs");
            _hudElements.Add(targetBuffs);
            _hudElementsUsingTarget.Add(targetBuffs);

            var targetDebuffsConfig = ConfigurationManager.GetInstance().GetConfigObject<TargetDebuffsListConfig>();
            var targetDebuffs = new StatusEffectsListHud("targetDebuffs", targetDebuffsConfig, "Target Debuffs");
            _hudElements.Add(targetDebuffs);
            _hudElementsUsingTarget.Add(targetDebuffs);
        }

        private void CreateMiscElements()
        {
            // primary resource bar
            var primaryResourceConfig = ConfigurationManager.GetInstance().GetConfigObject<PrimaryResourceConfig>();
            _primaryResourceHud = new PrimaryResourceHud("primaryResource", primaryResourceConfig, "Primary Resource");
            _hudElements.Add(_primaryResourceHud);
            _hudElementsUsingPlayer.Add(_primaryResourceHud);

            // gcd indicator
            var gcdIndicatorConfig = ConfigurationManager.GetInstance().GetConfigObject<GCDIndicatorConfig>();
            var gcdIndicator = new GCDIndicatorHud("gcdIndicator", gcdIndicatorConfig, "GCD Indicator");
            _hudElements.Add(gcdIndicator);
            _hudElementsUsingPlayer.Add(gcdIndicator);

            // mp ticker
            var mpTickerConfig = ConfigurationManager.GetInstance().GetConfigObject<MPTickerConfig>();
            var mpTicker = new MPTickerHud("mpTicker", mpTickerConfig, "MP Ticker");
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

            // grid
            if (_gridConfig.Enabled)
            {
                DraggablesHelper.DrawGrid(_gridConfig, _selectedElement?.GetConfig());
            }

            // general elements
            foreach (var element in _hudElements)
            {
                if (element != _selectedElement)
                {
                    element.Draw(_origin);
                }
            }

            // job hud
            if (_jobHud != null && _jobHud.Config.Enabled && _jobHud != _selectedElement)
            {
                _jobHud.Draw(_origin);
            }

            // selected
            if (_selectedElement != null)
            {
                _selectedElement.Draw(_origin);
            }

            // tooltip
            TooltipsHelper.Instance.Draw();

            ImGui.End();
        }

        protected unsafe bool ShouldBeVisible()
        {
            if (!ConfigurationManager.GetInstance().ShowHUD || Plugin.ClientState.LocalPlayer == null)
            {
                return false;
            }

            var parameterWidget = (AtkUnitBase*)Plugin.GameGui.GetUiObjectByName("_ParameterWidget", 1);
            var fadeMiddleWidget = (AtkUnitBase*)Plugin.GameGui.GetUiObjectByName("FadeMiddle", 1);

            return parameterWidget->IsVisible && !fadeMiddleWidget->IsVisible;
        }

        private void UpdateJob()
        {
            var newJobId = Plugin.ClientState.LocalPlayer.ClassJob.Id;
            if (_jobHud != null && _jobHud.Config.JobId == newJobId)
            {
                _primaryResourceHud.ResourceType = _jobHud.Config.UseDefaultPrimaryResourceBar ? _jobHud.Config.PrimaryResourceType : PrimaryResourceTypes.None;
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
                _jobHud = (JobHud)Activator.CreateInstance(types.HudType, types.HudType.FullName, config, types.DisplayName);
                _jobHud.SelectEvent += OnDraggableElementSelected;
            }

            if (config != null && _primaryResourceHud != null)
            {
                _primaryResourceHud.ResourceType = config.UseDefaultPrimaryResourceBar ? config.PrimaryResourceType : PrimaryResourceTypes.None;
            }
        }

        private void AssignActors()
        {
            var pluginInterface = Plugin.PluginInterface;

            // player
            var player = Plugin.ClientState.LocalPlayer;
            foreach (var element in _hudElementsUsingPlayer)
            {
                element.Actor = player;

                if (_jobHud != null)
                {
                    _jobHud.Actor = player;
                }
            }

            // target
            var target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.CurrentTarget;
            foreach (var element in _hudElementsUsingTarget)
            {
                element.Actor = target;
            }

            // target of target
            var targetOfTarget = Utils.FindTargetOfTarget(target, player, Plugin.ObjectTable);
            foreach (var element in _hudElementsUsingTargetOfTarget)
            {
                element.Actor = targetOfTarget;
            }

            // focus
            var focusTarget = Plugin.TargetManager.FocusTarget;
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
                [JobIDs.PLD] = new JobHudTypes(typeof(PaladinHud), typeof(PaladinConfig), "Paladin HUD"),
                [JobIDs.WAR] = new JobHudTypes(typeof(WarriorHud), typeof(WarriorConfig), "Warrior HUD"),
                [JobIDs.DRK] = new JobHudTypes(typeof(DarkKnightHud), typeof(DarkKnightConfig), "Dark Knight HUD"),
                [JobIDs.GNB] = new JobHudTypes(typeof(GunbreakerHud), typeof(GunbreakerConfig), "Gunbreaker HUD"),

                // healers
                [JobIDs.WHM] = new JobHudTypes(typeof(WhiteMageHud), typeof(WhiteMageConfig), "White Mage HUD"),
                [JobIDs.SCH] = new JobHudTypes(typeof(ScholarHud), typeof(ScholarConfig), "Scholar HUD"),
                [JobIDs.AST] = new JobHudTypes(typeof(AstrologianHud), typeof(AstrologianConfig), "Astrologian HUD"),

                // melee
                [JobIDs.MNK] = new JobHudTypes(typeof(MonkHud), typeof(MonkConfig), "Monk HUD"),
                [JobIDs.DRG] = new JobHudTypes(typeof(DragoonHud), typeof(DragoonConfig), "Dragoon HUD"),
                [JobIDs.NIN] = new JobHudTypes(typeof(NinjaHud), typeof(NinjaConfig), "Ninja HUD"),
                [JobIDs.SAM] = new JobHudTypes(typeof(SamuraiHud), typeof(SamuraiConfig), "Samurai HUD"),

                // ranged
                [JobIDs.BRD] = new JobHudTypes(typeof(BardHud), typeof(BardConfig), "Bard HUD"),
                [JobIDs.MCH] = new JobHudTypes(typeof(MachinistHud), typeof(MachinistConfig), "Mechanic HUD"),
                [JobIDs.DNC] = new JobHudTypes(typeof(DancerHud), typeof(DancerConfig), "Dancer HUD"),

                // casters
                [JobIDs.BLM] = new JobHudTypes(typeof(BlackMageHud), typeof(BlackMageConfig), "Black Mage HUD"),
                [JobIDs.SMN] = new JobHudTypes(typeof(SummonerHud), typeof(SummonerConfig), "Summoner HUD"),
                [JobIDs.RDM] = new JobHudTypes(typeof(RedMageHud), typeof(RedMageConfig), "Red Mage HUD")
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
        public string DisplayName;

        public JobHudTypes(Type hudType, Type configType, string displayName)
        {
            HudType = hudType;
            ConfigType = configType;
            DisplayName = displayName;
        }
    }

    internal static class HUDConstants
    {
        internal static int BaseHUDOffsetY = (int)(ImGui.GetMainViewport().Size.Y * 0.3f);
        internal static int UnitFramesOffsetX = 160;
        internal static int PlayerCastbarY = BaseHUDOffsetY - 13;
        internal static int JobHudsBaseY = PlayerCastbarY - 14;
        internal static Vector2 DefaultBigUnitFrameSize = new Vector2(270, 50);
        internal static Vector2 DefaultSmallUnitFrameSize = new Vector2(120, 20);
        internal static Vector2 DefaultStatusEffectsListSize = new Vector2(292, 82);
    }

    [Portable(false)]
    [Section("Misc")]
    [SubSection("Grid", 0)]
    public class GridConfig : PluginConfigObject
    {
        public new static GridConfig DefaultConfig()
        {
            var config = new GridConfig();
            config.Enabled = false;

            return config;
        }

        [DragFloat("Background Alpha", min = 0, max = 1, velocity = .05f)]
        [Order(10)]
        public float BackgroundAlpha = 0.3f;

        [Checkbox("Show Center Lines")]
        [Order(15)]
        public bool ShowCenterLines = true;

        [Checkbox("Show Grid")]
        [Order(20)]
        public bool ShowGrid = true;

        [DragInt("Divisions Distance", min = 50, max = 500)]
        [Order(25)]
        public int GridDivisionsDistance = 50;

        [DragInt("Subdivision Count", min = 1, max = 10)]
        [Order(30)]
        public int GridSubdivisionCount = 4;

        [Checkbox("Show Anchor Points")]
        [Order(35)]
        public bool ShowAnchorPoints = true;
    }
}
