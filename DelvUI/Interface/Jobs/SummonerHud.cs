using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using DelvUI.Config;
using DelvUI.Config.Attributes;
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
    public class SummonerHud : JobHud
    {
        private bool _bahamutFinished = true;

        private new SummonerConfig Config => (SummonerConfig)Config;
        private PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;

        public SummonerHud(string id, SummonerConfig config, string? displayName = null) : base(id, config, displayName)
        {

        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new();
            List<Vector2> sizes = new();

            if (Config.ShowAether)
            {
                positions.Add(Config.Position + Config.AetherPosition);
                sizes.Add(Config.AetherSize);
            }

            if (Config.ShowRuin)
            {
                positions.Add(Config.Position + Config.RuinPosition);
                sizes.Add(Config.RuinSize);
            }

            if (Config.ShowTrance)
            {
                positions.Add(Config.Position + Config.TrancePosition);
                sizes.Add(Config.TranceSize);
            }

            if (Config.ShowDreadwyrmAether)
            {
                if (Config.ShowDemiIndicator)
                {
                    positions.Add(Config.Position + Config.IndicatorPosition);
                    sizes.Add(Config.IndicatorSize);
                }

                if (Config.ShowDreadwyrmAetherBars)
                {
                    positions.Add(Config.Position + Config.DreadwyrmAetherBarPosition);
                    sizes.Add(Config.DreadwyrmAetherBarSize);
                }
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            if (Config.ShowBio || Config.ShowMiasma)
            {
                DrawActiveDots(origin, player);
            }

            if (Config.ShowRuin)
            {
                DrawRuinBar(origin, player);
            }

            if (Config.ShowAether)
            {
                DrawAetherBar(origin, player);
            }

            if (Config.ShowTrance)
            {
                DrawTranceBar(origin);
            }

            if (Config.ShowDreadwyrmAether)
            {
                DrawDreadWyrmAether(origin);
            }
        }

        private void DrawTranceBar(Vector2 origin)
        {
            SMNGauge gauge = Plugin.JobGauges.Get<SMNGauge>();

            PluginConfigColor tranceColor;
            float maxDuration;
            float tranceDuration = gauge.TimerRemaining;

            if (!_bahamutFinished && tranceDuration < 1)
            {
                _bahamutFinished = true;
            }

            if (Config.OnlyShowTranceWhenActive && tranceDuration == 0) { return; }

            switch (gauge.AetherFlags)
            {
                case >= 16:
                    tranceColor = Config.PhoenixColor;
                    maxDuration = 20000f;

                    break;

                case >= 8:
                    tranceColor = Config.BahamutColor;
                    maxDuration = 20000f;
                    _bahamutFinished = false;

                    break;

                default:
                    // This is needed because as soon as you summon Bahamut the flag goes back to 0-2
                    tranceColor = _bahamutFinished ? Config.DreadwyrmColor : Config.BahamutColor;
                    maxDuration = _bahamutFinished ? 15000f : 20000f;

                    break;
            }

            Vector2 barSize = Config.TranceSize;
            Vector2 position = origin + Config.Position + Config.TrancePosition - barSize / 2f;

            BarBuilder builder = BarBuilder.Create(position, barSize);

            Bar bar = builder.AddInnerBar(tranceDuration / 1000f, maxDuration / 1000f, tranceColor).SetBackgroundColor(EmptyColor.Base).Build();

            if (Config.ShowTranceText && tranceDuration != 0)
            {
                builder.SetTextMode(BarTextMode.Single)
                       .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        public void DrawDreadWyrmAether(Vector2 origin)
        {
            SMNGauge gauge = Plugin.JobGauges.Get<SMNGauge>();
            byte stacks = gauge.AetherFlags;

            if (Config.OnlyShowDreadwyrmAetherWhenActive && stacks == 0) { return; }

            List<Bar> barDrawList = new();

            if (Config.ShowDemiIndicator)
            {
                Vector2 barSize = Config.IndicatorSize;
                Vector2 position = origin + Config.Position + Config.IndicatorPosition - barSize / 2f;

                BarBuilder builder = BarBuilder.Create(position, barSize);

                if (stacks >= 8 && stacks < 16)
                {
                    Bar indicatorBar = builder.AddInnerBar(1, 1, Config.BahamutReadyColor)
                                              .SetBackgroundColor(EmptyColor.Base)
                                              .Build();
                    barDrawList.Add(indicatorBar);
                }
                if (stacks >= 16)
                {
                    Bar indicatorBar = builder.AddInnerBar(1, 1, Config.PhoenixReadyColor)
                                              .SetBackgroundColor(EmptyColor.Base)
                                              .Build();
                    barDrawList.Add(indicatorBar);
                }
                if (stacks < 8)
                {
                    Bar indicatorBar = builder.SetBackgroundColor(EmptyColor.Base)
                                              .Build();
                    barDrawList.Add(indicatorBar);
                }
            }

            if (Config.ShowDreadwyrmAetherBars)
            {
                Vector2 barSize = Config.DreadwyrmAetherBarSize;
                Vector2 position = origin + Config.Position + Config.DreadwyrmAetherBarPosition - barSize / 2f;

                int filledChunkCount = 0;

                if (stacks >= 4 && stacks < 8)
                {
                    filledChunkCount = 1;
                }
                else if (stacks >= 8 && stacks < 16)
                {
                    filledChunkCount = 2;
                }

                Bar DreadwyrmAetherBars = BarBuilder.Create(position, barSize)
                                                    .SetChunks(2)
                                                    .SetChunkPadding(Config.DreadwyrmAetherBarPadding)
                                                    .AddInnerBar(filledChunkCount, 2, Config.DreadwyrmAetherBarColor)
                                                    .SetBackgroundColor(EmptyColor.Base)
                                                    .Build();
                barDrawList.Add(DreadwyrmAetherBars);
            }

            if (barDrawList.Count > 0)
            {
                ImDrawListPtr drawList = ImGui.GetWindowDrawList();

                foreach (Bar bar in barDrawList)
                {
                    bar.Draw(drawList);
                }
            }
        }
        private void DrawActiveDots(Vector2 origin, PlayerCharacter player)
        {
            GameObject? actor = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            if (actor is not BattleChara && Config.HideBioWhenNoTarget && Config.HideMiasmaWhenNoTarget) { return; }

            Vector2 barSize = Config.MiasmaSize;
            Vector2 position = origin + Config.Position + Config.MiasmaPosition - barSize / 2f;

            List<Bar> barDrawList = new();

            float miasmaDuration = 0f;
            float bioDuration = 0f;

            if (Config.ShowMiasma)
            {
                if (actor is BattleChara target)
                {
                    IEnumerable<Status> miasma = target.StatusList.Where(o => o.StatusId is 1215 or 180 && o.SourceID == player.ObjectId);
                    miasmaDuration = miasma.Any() ? Math.Abs(miasma.First().RemainingTime) : 0;
                }

                PluginConfigColor miasmaColor = miasmaDuration > 5 ? Config.MiasmaColor : Config.MiasmaExpireColor;
                BarBuilder builder = BarBuilder.Create(position, barSize).SetBackgroundColor(EmptyColor.Base);

                Bar miasmaBar = builder.AddInnerBar(miasmaDuration, 30f, miasmaColor)
                                       .SetFlipDrainDirection(Config.MiasmaInverted)
                                       .Build();

                if (Config.ShowMiasmaText && miasmaDuration != 0)
                {
                    builder.SetTextMode(BarTextMode.Single)
                           .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
                barDrawList.Add(miasmaBar);
            }

            if (Config.ShowBio)
            {
                if (actor is BattleChara target)
                {
                    IEnumerable<Status> bio = target.StatusList.Where(o => o.StatusId is 1214 or 179 or 189 && o.SourceID == player.ObjectId);
                    bioDuration = bio.Any() ? Math.Abs(bio.First().RemainingTime) : 0;
                }

                PluginConfigColor bioColor = bioDuration > 5 ? Config.BioColor : Config.BioExpireColor;

                barSize = Config.BioSize;
                position = origin + Config.Position + Config.BioPosition - barSize / 2f;

                BarBuilder builder = BarBuilder.Create(position, barSize).SetBackgroundColor(EmptyColor.Base);

                Bar bioBar = builder.AddInnerBar(bioDuration, 30f, bioColor)
                                    .SetFlipDrainDirection(Config.BioInverted)
                                    .Build();

                if (Config.ShowBioText && bioDuration != 0)
                {
                    builder.SetTextMode(BarTextMode.Single)
                           .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
                barDrawList.Add(bioBar);
            }

            if (Config.OnlyShowDoTsWhenActive && bioDuration is 0 && miasmaDuration is 0) { return; }

            if (barDrawList.Count > 0)
            {
                ImDrawListPtr drawList = ImGui.GetWindowDrawList();

                foreach (Bar bar in barDrawList)
                {
                    bar.Draw(drawList);
                }
            }
        }

        private void DrawRuinBar(Vector2 origin, PlayerCharacter player)
        {
            IEnumerable<Status> ruinBuff = player.StatusList.Where(o => o.StatusId == 1212);
            int stackCount = ruinBuff.Any() ? ruinBuff.First().StackCount : 0;
            if (Config.OnlyShowRuinWhenActive && stackCount is 0) { return; }

            Vector2 barSize = Config.RuinSize;
            Vector2 position = origin + Config.Position + Config.RuinPosition - barSize / 2f;

            Bar bar = BarBuilder.Create(position, barSize)
                                .SetChunks(4)
                                .SetChunkPadding(Config.RuinPadding)
                                .AddInnerBar(stackCount, 4, Config.RuinColor)
                                .SetBackgroundColor(EmptyColor.Base)
                                .Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        private void DrawAetherBar(Vector2 origin, PlayerCharacter player)
        {
            IEnumerable<Status> aetherFlowBuff = player.StatusList.Where(o => o.StatusId == 304);
            int stackCount = aetherFlowBuff.Any() ? aetherFlowBuff.First().StackCount : 0;
            if (Config.OnlyShowAetherWhenActive && stackCount is 0) { return; }

            Vector2 barSize = Config.AetherSize;
            Vector2 position = origin + Config.Position + Config.AetherPosition - barSize / 2f;

            Bar bar = BarBuilder.Create(position, barSize)
                                .SetChunks(2)
                                .SetChunkPadding(Config.AetherPadding)
                                .AddInnerBar(stackCount, 2, Config.AetherColor)
                                .SetBackgroundColor(EmptyColor.Base)
                                .Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Caster", 0)]
    [SubSection("Summoner", 1)]
    public class SummonerConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.SMN;
        public static new SummonerConfig DefaultConfig() => new();

        #region aether
        [Checkbox("Aether", separator = true)]
        [Order(30)]
        public bool ShowAether = true;

        [Checkbox("Only Show When Active" + "##Aether")]
        [Order(31, collapseWith = nameof(ShowAether))]
        public bool OnlyShowAetherWhenActive = false;

        [DragFloat2("Position" + "##Aether", min = -4000f, max = 4000f)]
        [Order(35, collapseWith = nameof(ShowAether))]
        public Vector2 AetherPosition = new(-67, -6);

        [DragFloat2("Size" + "##Aether", min = 1f, max = 2000f)]
        [Order(40, collapseWith = nameof(ShowAether))]
        public Vector2 AetherSize = new(120, 10);

        [DragInt("Spacing" + "##Aether", max = 1000)]
        [Order(45, collapseWith = nameof(ShowAether))]
        public int AetherPadding = 2;

        [ColorEdit4("Color" + "##Aether")]
        [Order(50, collapseWith = nameof(ShowAether))]
        public PluginConfigColor AetherColor = new(new Vector4(0f / 255f, 255f / 255f, 0f / 255f, 100f / 100f));
        #endregion

        #region ruin
        [Checkbox("Ruin", separator = true)]
        [Order(55)]
        public bool ShowRuin = true;

        [Checkbox("Only Show When Active" + "##Ruin")]
        [Order(56, collapseWith = nameof(ShowRuin))]
        public bool OnlyShowRuinWhenActive = false;

        [DragFloat2("Position" + "##Ruin", min = -4000f, max = 4000f)]
        [Order(60, collapseWith = nameof(ShowRuin))]
        public Vector2 RuinPosition = new(0, -45);

        [DragFloat2("Size" + "##Ruin", min = 1f, max = 2000f)]
        [Order(65, collapseWith = nameof(ShowRuin))]
        public Vector2 RuinSize = new(254, 20);

        [DragInt("Spacing" + "##Ruin", max = 1000)]
        [Order(70, collapseWith = nameof(ShowRuin))]
        public int RuinPadding = 2;

        [ColorEdit4("Color" + "##Ruin")]
        [Order(75, collapseWith = nameof(ShowRuin))]
        public PluginConfigColor RuinColor = new(new Vector4(94f / 255f, 250f / 255f, 154f / 255f, 100f / 100f));
        #endregion

        #region miasma
        [Checkbox("Miasma", separator = true)]
        [Order(80)]
        public bool ShowMiasma = true;

        [Checkbox("Hide When No Target" + "##Miasma")]
        [Order(82, collapseWith = nameof(ShowMiasma))]
        public bool HideMiasmaWhenNoTarget = true;

        [Checkbox("Timer" + "##Miasma")]
        [Order(85, collapseWith = nameof(ShowMiasma))]
        public bool ShowMiasmaText = true;

        [Checkbox("Inverted" + "##Miasma")]
        [Order(90, collapseWith = nameof(ShowMiasma))]
        public bool MiasmaInverted = true;

        [DragFloat2("Position" + "##Miasma", min = -4000f, max = 4000f)]
        [Order(95, collapseWith = nameof(ShowMiasma))]
        public Vector2 MiasmaPosition = new(-64, -67);

        [DragFloat2("Size" + "##Miasma", max = 2000f)]
        [Order(100, collapseWith = nameof(ShowMiasma))]
        public Vector2 MiasmaSize = new(126, 20);

        [ColorEdit4("Color" + "##Miasma")]
        [Order(105, collapseWith = nameof(ShowMiasma))]
        public PluginConfigColor MiasmaColor = new(new Vector4(106f / 255f, 237f / 255f, 241f / 255f, 100f / 100f));

        [ColorEdit4("Expire Color" + "##Miasma")]
        [Order(106, collapseWith = nameof(ShowMiasma))]
        public PluginConfigColor MiasmaExpireColor = new(new Vector4(230f / 255f, 33f / 255f, 33f / 255f, 53f / 100f));
        #endregion

        #region bio
        [Checkbox("Bio", separator = true)]
        [Order(110)]
        public bool ShowBio = true;

        [Checkbox("Hide When No Target" + "##Bio")]
        [Order(112, collapseWith = nameof(ShowBio))]
        public bool HideBioWhenNoTarget = true;

        [Checkbox("Timer" + "##Bio")]
        [Order(115, collapseWith = nameof(ShowBio))]
        public bool ShowBioText = true;

        [Checkbox("Inverted" + "##Bio")]
        [Order(120, collapseWith = nameof(ShowBio))]
        public bool BioInverted = false;

        [DragFloat2("Position" + "##Bio", min = -4000f, max = 4000f)]
        [Order(125, collapseWith = nameof(ShowBio))]
        public Vector2 BioPosition = new(64, -67);

        [DragFloat2("Size" + "##Bio", max = 2000f)]
        [Order(130, collapseWith = nameof(ShowBio))]
        public Vector2 BioSize = new(126, 20);

        [ColorEdit4("Color" + "##Bio")]
        [Order(135, collapseWith = nameof(ShowBio))]
        public PluginConfigColor BioColor = new(new Vector4(50f / 255f, 93f / 255f, 37f / 255f, 100f / 100f));

        [ColorEdit4("Expire Color" + "##Bio")]
        [Order(136, collapseWith = nameof(ShowBio))]
        public PluginConfigColor BioExpireColor = new(new Vector4(230f / 255f, 33f / 255f, 33f / 255f, 53f / 100f));

        [Checkbox("Only Show DoTs When Active" + "##DoTs", spacing = true)]
        [Order(137)]
        public bool OnlyShowDoTsWhenActive = false;
        #endregion

        #region trance
        [Checkbox("Trance", separator = true)]
        [Order(140)]
        public bool ShowTrance = true;

        [Checkbox("Only Show When Active" + "##Trance")]
        [Order(141, collapseWith = nameof(ShowTrance))]
        public bool OnlyShowTranceWhenActive = false;

        [Checkbox("Text" + "##Trance")]
        [Order(145, collapseWith = nameof(ShowTrance))]
        public bool ShowTranceText = true;

        [DragFloat2("Position" + "##Trance", min = -4000f, max = 4000f)]
        [Order(150, collapseWith = nameof(ShowTrance))]
        public Vector2 TrancePosition = new(0, -23);

        [DragFloat2("Size" + "##Trance", min = 1f, max = 2000f)]
        [Order(155, collapseWith = nameof(ShowTrance))]
        public Vector2 TranceSize = new(254, 20);

        [ColorEdit4("Dreadwyrm")]
        [Order(160, collapseWith = nameof(ShowTrance))]
        public PluginConfigColor DreadwyrmColor = new(new Vector4(255f / 255f, 255f / 255f, 147f / 255f, 100f / 100f));

        [ColorEdit4("Bahamut" + "##Trance")]
        [Order(165, collapseWith = nameof(ShowTrance))]
        public PluginConfigColor BahamutColor = new(new Vector4(128f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [ColorEdit4("Phoenix" + "##Trance")]
        [Order(170, collapseWith = nameof(ShowTrance))]
        public PluginConfigColor PhoenixColor = new(new Vector4(255f / 255f, 128f / 255f, 0f / 255f, 100f / 100f));
        #endregion

        #region dreadwyrm
        [Checkbox("Dreadwyrm Trance Tracker", separator = true)]
        [Order(175)]
        public bool ShowDreadwyrmAether = true;

        [Checkbox("Only Show When Active" + "##TranceTracker")]
        [Order(178, collapseWith = nameof(ShowDreadwyrmAether))]
        public bool OnlyShowDreadwyrmAetherWhenActive = false;

        [Checkbox("Demi Status Indicator")]
        [Order(180, collapseWith = nameof(ShowDreadwyrmAether))]
        public bool ShowDemiIndicator = true;

        [DragFloat2("Position" + "##DemiIndicator", min = -4000f, max = -4000f)]
        [Order(185, collapseWith = nameof(ShowDemiIndicator))]
        public Vector2 IndicatorPosition = new(0, -6);

        [DragFloat2("Size" + "##DemiIndicator", min = 1f, max = 2000f)]
        [Order(190, collapseWith = nameof(ShowDemiIndicator))]
        public Vector2 IndicatorSize = new(10, 10);

        [ColorEdit4("Bahamut" + "##DemiIndicator")]
        [Order(195, collapseWith = nameof(ShowDemiIndicator))]
        public PluginConfigColor BahamutReadyColor = new(new Vector4(128f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [ColorEdit4("Phoenix" + "##DemiIndicator")]
        [Order(200, collapseWith = nameof(ShowDemiIndicator))]
        public PluginConfigColor PhoenixReadyColor = new(new Vector4(255f / 255f, 128f / 255f, 0f / 255f, 100f / 100f));

        [Checkbox("Dreadwyrm Aether Bars")]
        [Order(205, collapseWith = nameof(ShowDreadwyrmAether))]
        public bool ShowDreadwyrmAetherBars = true;

        [DragFloat2("Position" + "##AetherBars", min = -4000f, max = -4000f)]
        [Order(210, collapseWith = nameof(ShowDreadwyrmAetherBars))]
        public Vector2 DreadwyrmAetherBarPosition = new(67, -6);

        [DragFloat2("Size" + "##AetherBars", min = 1f, max = 2000f)]
        [Order(215, collapseWith = nameof(ShowDreadwyrmAetherBars))]
        public Vector2 DreadwyrmAetherBarSize = new(120, 10);

        [DragInt("Spacing" + "##AetherBars", max = 1000)]
        [Order(220, collapseWith = nameof(ShowDreadwyrmAetherBars))]
        public int DreadwyrmAetherBarPadding = 2;

        [ColorEdit4("Color" + "##AetherBars")]
        [Order(225, collapseWith = nameof(ShowDreadwyrmAetherBars))]
        public PluginConfigColor DreadwyrmAetherBarColor = new(new Vector4(128f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));
        #endregion
    }
}