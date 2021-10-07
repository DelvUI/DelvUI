using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.Jobs
{
    public class BlackMageHud : JobHud
    {
        private new BlackMageConfig Config => (BlackMageConfig)_config;
        private PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;

        public BlackMageHud(string id, BlackMageConfig config, string? displayName = null) : base(id, config, displayName)
        {

        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.ManaBar.Enabled)
            {
                positions.Add(Config.Position + Config.ManaBar.Position);
                sizes.Add(Config.ManaBar.Size);
            }

            if (Config.UmbralHeartBar.Enabled)
            {
                positions.Add(Config.Position + Config.UmbralHeartBar.Position);
                sizes.Add(Config.UmbralHeartBar.Size);
            }

            if (Config.TriplecastBar.Enabled)
            {
                positions.Add(Config.Position + Config.TriplecastBar.Position);
                sizes.Add(Config.TriplecastBar.Size);
            }

            if (Config.EnochianBar.Enabled)
            {
                positions.Add(Config.Position + Config.EnochianBar.Position);
                sizes.Add(Config.EnochianBar.Size);
            }

            if (Config.PolyglotBar.Enabled)
            {
                positions.Add(Config.Position + Config.PolyglotBar.Position);
                sizes.Add(Config.PolyglotBar.Size);
            }

            if (Config.AlwaysShowFirestarterProcs)
            {
                positions.Add(Config.Position + Config.FirestarterBarPosition);
                sizes.Add(Config.FirestarterBarSize);
            }

            if (Config.AlwaysShowFirestarterProcs)
            {
                positions.Add(Config.Position + Config.ThundercloudBarPosition);
                sizes.Add(Config.ThundercloudBarSize);
            }

            if (Config.ShowDotBar)
            {
                positions.Add(Config.Position + Config.DoTBarPosition);
                sizes.Add(Config.DoTBarSize);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            Vector2 pos = origin + Config.Position;

            if (Config.ManaBar.Enabled)
            {
                DrawManaBar(pos, player);
            }

            if (Config.UmbralHeartBar.Enabled)
            {
                DrawUmbralHeartBar(pos);
            }

            if (Config.TriplecastBar.Enabled)
            {
                DrawTripleCastBar(pos, player);
            }

            if (Config.EnochianBar.Enabled)
            {
                DrawEnochianBar(pos);
            }

            if (Config.PolyglotBar.Enabled)
            {
                DrawPolyglotBar(pos, player);
            }

            if (Config.ShowFirestarterProcs)
            {
                DrawFirestarterProcs(origin, player);
            }

            if (Config.ShowThundercloudProcs)
            {
                DrawThundercloudProcs(origin, player);
            }

            if (Config.ShowDotBar)
            {
                DrawDotTimer(origin, player);
            }
        }

        protected void DrawManaBar(Vector2 origin, PlayerCharacter player)
        {
            BlackMageManaBarConfig config = Config.ManaBar;
            var gauge = Plugin.JobGauges.Get<BLMGauge>();

            if (config.HideWhenInactive && !gauge.InAstralFire && !gauge.InUmbralIce && player.CurrentMp == player.MaxMp)
            {
                return;
            }

            // value
            config.ValueLabelConfig.SetText($"{player.CurrentMp,0}");

            // element timer
            if (gauge.InAstralFire || gauge.InUmbralIce)
            {
                var time = gauge.ElementTimeRemaining > 10 ? gauge.ElementTimeRemaining / 1000 + 1 : 0;
                config.ElementTimerLabelConfig.SetText($"{time,0}");
            }
            else
            {
                config.ElementTimerLabelConfig.SetText("");
            }

            bool drawTreshold = gauge.InAstralFire || !config.ThresholdConfig.ShowOnlyDuringAstralFire;

            BarHud bar = BarUtilities.GetProgressBar(
                ID + "_manaBar",
                config,
                drawTreshold ? config.ThresholdConfig : null,
                new LabelConfig[] { config.ValueLabelConfig, config.ElementTimerLabelConfig },
                player.CurrentMp,
                player.MaxMp,
                0,
                gauge.InAstralFire ? config.FireColor : gauge.InUmbralIce ? config.IceColor : config.FillColor,
                player,
                gauge.IsEnochianActive && config.GlowConfig.Enabled ? config.GlowConfig : null
            );

            bar.Draw(origin);
        }

        protected void DrawUmbralHeartBar(Vector2 origin)
        {
            var gauge = Plugin.JobGauges.Get<BLMGauge>();
            if (Config.UmbralHeartBar.HideWhenInactive && gauge.UmbralHearts == 0)
            {
                return;
            };

            BarUtilities.GetChunkedProgressBars(ID + "_umbralHeartBar", Config.UmbralHeartBar, 3, gauge.UmbralHearts, 3f)
                .Draw(origin);
        }

        protected void DrawEnochianBar(Vector2 origin)
        {
            var gauge = Plugin.JobGauges.Get<BLMGauge>();

            if (Config.EnochianBar.HideWhenInactive && !gauge.IsEnochianActive)
            {
                return;
            }

            int timer = gauge.IsEnochianActive ? (30000 - gauge.EnochianTimer) / 1000 : 0;
            Config.EnochianBar.Label.SetText($"{timer,0}");
            BarUtilities.GetProgressBar(ID + "_enochianBar", Config.EnochianBar, timer, 30, 0f)
                .Draw(origin);
        }

        protected void DrawPolyglotBar(Vector2 origin, PlayerCharacter player)
        {
            var gauge = Plugin.JobGauges.Get<BLMGauge>();

            if (Config.PolyglotBar.HideWhenInactive && gauge.PolyglotStacks == 0)
            {
                return;
            }

            // only 1 stack before level 80
            if (player.Level < 80)
            {
                var glow = gauge.PolyglotStacks == 1 ? Config.PolyglotBar.GlowConfig : null;
                BarUtilities.GetBar(ID + "_polyglotBar", Config.PolyglotBar, gauge.PolyglotStacks, 1, 0, null, glow)
                    .Draw(origin);
            }
            // 2 stacks for level 80+
            else
            {
                BarUtilities.GetChunkedProgressBars(ID + "_polyglotBar", Config.PolyglotBar, 2, gauge.PolyglotStacks, 2f, 0, null, null, Config.PolyglotBar.GlowConfig)
                    .Draw(origin);
            }
        }


        protected void DrawTripleCastBar(Vector2 origin, PlayerCharacter player)
        {
            byte stackCount = player.StatusList.FirstOrDefault(o => o.StatusId is 1211)?.StackCount ?? 0;

            if (Config.TriplecastBar.HideWhenInactive && stackCount == 0)
            {
                return;
            };

            BarUtilities.GetChunkedProgressBars(ID + "_triplecastBar", Config.TriplecastBar, 3, stackCount, 3f)
                .Draw(origin);
        }

        protected void DrawFirestarterProcs(Vector2 origin, PlayerCharacter player)
        {
            IEnumerable<Status> fireStarterBuff = player.StatusList.Where(o => o.StatusId is 165);
            float firestarterTimer = Config.ShowFirestarterProcs ? fireStarterBuff.Any() ? Math.Abs(fireStarterBuff.First().RemainingTime) : 0f : 0;

            DrawProc(
                origin,
                Config.FirestarterBarPosition,
                Config.FirestarterBarSize,
                firestarterTimer,
                18f,
                Config.InvertFirestarterBar,
                Config.AlwaysShowFirestarterProcs,
                Config.FirestarterColor
            );
        }

        protected void DrawThundercloudProcs(Vector2 origin, PlayerCharacter player)
        {
            IEnumerable<Status> thundercloudBuff = player.StatusList.Where(o => o.StatusId is 164);
            float thundercloudTimer = Config.ShowThundercloudProcs ? thundercloudBuff.Any() ? Math.Abs(thundercloudBuff.First().RemainingTime) : 0f : 0;

            DrawProc(
                origin,
                Config.ThundercloudBarPosition,
                Config.ThundercloudBarSize,
                thundercloudTimer,
                18f,
                Config.InvertThundercloudBar,
                Config.AlwaysShowThundercloudProcs,
                Config.ThundercloudColor
            );
        }

        protected void DrawProc(Vector2 origin, Vector2 position, Vector2 size, float timer, float maxDuration, bool invert, bool alwayShow, PluginConfigColor color)
        {
            if (timer == 0 && !alwayShow)
            {
                return;
            }

            var pos = origin + Config.Position + position - size / 2f;

            var builder = BarBuilder.Create(pos, size)
                .AddInnerBar(timer, 18f, color)
                .SetFlipDrainDirection(invert)
                .SetBackgroundColor(EmptyColor.Base);

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        protected void DrawDotTimer(Vector2 origin, PlayerCharacter player)
        {
            var actor = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            float timer = 0f;
            float maxDuration = 1;

            if (actor is BattleChara target)
            {
                // thunder 1 to 4
                int[] dotIDs = { 161, 162, 163, 1210 };
                float[] dotDurations = { 18, 12, 24, 18 };

                for (var i = 0; i < 4; i++)
                {
                    timer = target.StatusList.FirstOrDefault(o => o.StatusId == dotIDs[i] && o.SourceID == player.ObjectId)?.RemainingTime ?? 0f;

                    if (timer > 0)
                    {
                        maxDuration = dotDurations[i];

                        break;
                    }
                }
            }

            if (Config.OnlyShowDotWhenActive && timer is 0) { return; }

            var position = origin + Config.Position + Config.DoTBarPosition - Config.DoTBarSize / 2f;

            var builder = BarBuilder.Create(position, Config.DoTBarSize)
                .AddInnerBar(timer, maxDuration, Config.DotColor)
                .SetFlipDrainDirection(Config.InvertDoTBar)
                .SetBackgroundColor(EmptyColor.Base);

            if (Config.ShowDoTBarTimer && timer != 0)
            {
                builder.SetTextMode(BarTextMode.Single)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Caster", 0)]
    [SubSection("Black Mage", 1)]
    public class BlackMageConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.BLM;

        public new static BlackMageConfig DefaultConfig()
        {
            var config = new BlackMageConfig();
            config.EnochianBar.Label.FontID = FontsConfig.DefaultMediumFontKey;

            return config;
        }

        [NestedConfig("Mana Bar", 30)]
        public BlackMageManaBarConfig ManaBar = new BlackMageManaBarConfig(
            new Vector2(0, -10),
            new Vector2(254, 20),
            new PluginConfigColor(new Vector4(234f / 255f, 95f / 255f, 155f / 255f, 100f / 100f))
        );

        [NestedConfig("Umbreal Heart Bar", 35)]
        public ChunkedBarConfig UmbralHeartBar = new ChunkedBarConfig(
            new(0, -28),
            new(254, 12),
            new PluginConfigColor(new Vector4(125f / 255f, 195f / 255f, 205f / 255f, 100f / 100f))
        );

        [NestedConfig("Triplecast Bar", 40)]
        public ChunkedBarConfig TriplecastBar = new ChunkedBarConfig(
            new(0, -41),
            new(254, 10),
            new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f))
        );

        [NestedConfig("Enochian Bar", 45)]
        public ProgressBarConfig EnochianBar = new ProgressBarConfig(
            new(0, -52),
            new(254, 8),
            new PluginConfigColor(new Vector4(234f / 255f, 95f / 255f, 155f / 255f, 100f / 100f))
        );

        [NestedConfig("Polyglot Bar", 50)]
        public BlackMagePolyglotBarConfig PolyglotBar = new BlackMagePolyglotBarConfig(
            new(0, -67),
            new(38, 18),
            new PluginConfigColor(new Vector4(234f / 255f, 95f / 255f, 155f / 255f, 100f / 100f))
        );

        #region polyglot
        [Checkbox("Polyglot", separator = true)]
        [Order(70)]
        public bool ShowPolyglot = true;

        [Checkbox("Only Show When Active" + "##Polyglot")]
        [Order(71, collapseWith = nameof(ShowPolyglot))]
        public bool OnlyShowPolyglotWhenActive = false;

        [DragFloat2("Position" + "##Polyglot", min = -2000, max = 2000f)]
        [Order(72, collapseWith = nameof(ShowPolyglot))]
        public Vector2 PolyglotPosition = new Vector2(0, -67);

        [DragFloat2("Size" + "##Polyglot", max = 2000f)]
        [Order(73, collapseWith = nameof(ShowPolyglot))]
        public Vector2 PolyglotSize = new Vector2(38, 18);

        [DragInt("Spacing" + "##Polyglot", min = -100, max = 100)]
        [Order(74, collapseWith = nameof(ShowPolyglot))]
        public int PolyglotPadding = 2;

        [ColorEdit4("Color" + "##Polyglot")]
        [Order(75, collapseWith = nameof(ShowPolyglot))]
        public PluginConfigColor PolyglotColor = new PluginConfigColor(new Vector4(234f / 255f, 95f / 255f, 155f / 255f, 100f / 100f));

        [Checkbox("Show Glow" + "##Polyglot")]
        [Order(76, collapseWith = nameof(ShowPolyglot))]
        public bool ShowPolyglotGlow = true;

        [ColorEdit4("Glow Color" + "##Polyglot")]
        [Order(81, collapseWith = nameof(ShowPolyglotGlow))]
        public PluginConfigColor PolyglotGlowColor = new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 50f / 100f));

        [Checkbox("Show Timer Outside of Stacks" + "##Polyglot")]
        [Order(78, collapseWith = nameof(ShowPolyglot))]
        public bool ShowTimerOutsideOfStacks = true;

        [DragFloat2("Timer Position" + "##Polyglot", min = -2000, max = 2000f)]
        [Order(79, collapseWith = nameof(ShowTimerOutsideOfStacks))]
        public Vector2 PolyglotTimerPosition = new Vector2(0, -52);

        [DragFloat2("Timer Size" + "##Polyglot", max = 2000f)]
        [Order(80, collapseWith = nameof(ShowTimerOutsideOfStacks))]
        public Vector2 PolyglotTimerSize = new Vector2(254, 8);

        [ColorEdit4("Timer Color" + "##Polyglot")]
        [Order(81, collapseWith = nameof(ShowTimerOutsideOfStacks))]
        public PluginConfigColor PolyglotTimerColor = new PluginConfigColor(new Vector4(234f / 255f, 95f / 255f, 155f / 255f, 100f / 100f));
        #endregion

        #region firestarter
        [Checkbox("Firestarter", separator = true)]
        [Order(90)]
        public bool ShowFirestarterProcs = true;

        [Checkbox("Always Show" + "##Firestarter")]
        [Order(91, collapseWith = nameof(ShowFirestarterProcs))]
        public bool AlwaysShowFirestarterProcs = true;

        [DragFloat2("Position" + "##Firestarter", min = -2000, max = 2000f)]
        [Order(92, collapseWith = nameof(ShowFirestarterProcs))]
        public Vector2 FirestarterBarPosition = new Vector2(-74, -72);

        [DragFloat2("Size" + "##Firestarter", max = 2000f)]
        [Order(93, collapseWith = nameof(ShowFirestarterProcs))]
        public Vector2 FirestarterBarSize = new Vector2(106, 8);

        [Checkbox("Inverted" + "##Firestarter")]
        [Order(94, collapseWith = nameof(ShowFirestarterProcs))]
        public bool InvertFirestarterBar = true;

        [ColorEdit4("Color" + "##Firestarter")]
        [Order(95, collapseWith = nameof(ShowFirestarterProcs))]
        public PluginConfigColor FirestarterColor = new PluginConfigColor(new Vector4(255f / 255f, 136f / 255f, 0 / 255f, 90f / 100f));
        #endregion

        #region thundercloud
        [Checkbox("Thundercloud", separator = true)]
        [Order(100)]
        public bool ShowThundercloudProcs = true;

        [Checkbox("Always Show" + "##Thundercloud")]
        [Order(101, collapseWith = nameof(ShowThundercloudProcs))]
        public bool AlwaysShowThundercloudProcs = true;

        [DragFloat2("Position" + "##Thundercloud", min = -2000, max = 2000f)]
        [Order(102, collapseWith = nameof(ShowThundercloudProcs))]
        public Vector2 ThundercloudBarPosition = new Vector2(-74, -62);

        [DragFloat2("Size" + "##Thundercloud", max = 2000f)]
        [Order(103, collapseWith = nameof(ShowThundercloudProcs))]
        public Vector2 ThundercloudBarSize = new Vector2(106, 8);

        [Checkbox("Inverted" + "##Thundercloud")]
        [Order(104, collapseWith = nameof(ShowThundercloudProcs))]
        public bool InvertThundercloudBar = true;

        [ColorEdit4("Color" + "##Thundercloud")]
        [Order(105, collapseWith = nameof(ShowThundercloudProcs))]
        public PluginConfigColor ThundercloudColor = new PluginConfigColor(new Vector4(240f / 255f, 163f / 255f, 255f / 255f, 90f / 100f));
        #endregion

        #region thunder dots
        [Checkbox("Thunder", separator = true)]
        [Order(110)]
        public bool ShowDotBar = true;

        [Checkbox("Only Show When Active" + "##Dot")]
        [Order(111, collapseWith = nameof(ShowDotBar))]
        public bool OnlyShowDotWhenActive = false;

        [Checkbox("Timer" + "##Dot")]
        [Order(112, collapseWith = nameof(ShowDotBar))]
        public bool ShowDoTBarTimer = false;

        [Checkbox("Inverted" + "##Dot")]
        [Order(113, collapseWith = nameof(ShowDotBar))]
        public bool InvertDoTBar = false;

        [DragFloat2("Position" + "##Dot", min = -2000, max = 2000f)]
        [Order(114, collapseWith = nameof(ShowDotBar))]
        public Vector2 DoTBarPosition = new Vector2(74, -67);

        [DragFloat2("Size" + "##Dot", max = 2000f)]
        [Order(115, collapseWith = nameof(ShowDotBar))]
        public Vector2 DoTBarSize = new Vector2(106, 18);

        [ColorEdit4("Color" + "##Dot")]
        [Order(116, collapseWith = nameof(ShowDotBar))]
        public PluginConfigColor DotColor = new PluginConfigColor(new Vector4(67f / 255f, 187 / 255f, 255f / 255f, 90f / 100f));
        #endregion
    }

    [Exportable(false)]
    public class BlackMageManaBarConfig : BarConfig
    {
        [ColorEdit4("Ice Color" + "##MP")]
        [Order(26)]
        public PluginConfigColor IceColor = new PluginConfigColor(new Vector4(69f / 255f, 115f / 255f, 202f / 255f, 100f / 100f));

        [ColorEdit4("Fire Color" + "##MP")]
        [Order(27)]
        public PluginConfigColor FireColor = new PluginConfigColor(new Vector4(204f / 255f, 40f / 255f, 40f / 255f, 100f / 100f));

        [NestedConfig("Value Label", 45, separator = false, spacing = true)]
        public LabelConfig ValueLabelConfig = new LabelConfig(new Vector2(2, 0), "", DrawAnchor.Left, DrawAnchor.Left);

        [NestedConfig("Element Timer Label", 50, separator = false, spacing = true)]
        public LabelConfig ElementTimerLabelConfig = new LabelConfig(Vector2.Zero, "", DrawAnchor.Center, DrawAnchor.Center);

        [NestedConfig("Glow When Enochian Is Active", 55)]
        public BarGlowConfig GlowConfig = new BarGlowConfig();

        [NestedConfig("Threshold", 65, separator = false, spacing = true)]
        public BlackMakeManaBarThresholdConfig ThresholdConfig = new BlackMakeManaBarThresholdConfig();

        public BlackMageManaBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
             : base(position, size, fillColor)
        {
        }
    }

    [Exportable(false)]
    public class BlackMakeManaBarThresholdConfig : ThresholdConfig
    {
        [Checkbox("Show Only During Astral Fire")]
        [Order(5)]
        public bool ShowOnlyDuringAstralFire = true;

        public BlackMakeManaBarThresholdConfig()
        {
            Enabled = true;
            Value = 2400;
            Color = new PluginConfigColor(new Vector4(240f / 255f, 120f / 255f, 10f / 255f, 100f / 100f));
            ShowMarker = true;
            MarkerColor = new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));
        }
    }

    [Exportable(false)]
    public class BlackMagePolyglotBarConfig : ChunkedBarConfig
    {
        [NestedConfig("Show Glow", 60)]
        public BarGlowConfig GlowConfig = new BarGlowConfig();

        public BlackMagePolyglotBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
             : base(position, size, fillColor)
        {
        }
    }
}
