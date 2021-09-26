using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;

namespace DelvUI.Interface.Jobs
{
    public class SummonerHud : JobHud
    {
        private bool _bahamutFinished = true;

        private new SummonerConfig Config => (SummonerConfig)_config;
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

        public override void DrawChildren(Vector2 origin)
        {
            DrawActiveDots(origin);
            DrawRuinBar(origin);
            DrawAetherBar(origin);
            DrawTranceBar(origin);
            DrawDreadWyrmAether(origin);
        }

        private void DrawTranceBar(Vector2 origin)
        {
            if (!Config.ShowTrance)
            {
                return;
            }
            
            SMNGauge gauge = Plugin.JobGauges.Get<SMNGauge>();
            
            PluginConfigColor tranceColor;
            float maxDuration;
            float tranceDuration = gauge.TimerRemaining;
            
            if (!_bahamutFinished && tranceDuration < 1)
            {
                _bahamutFinished = true;
            }
            
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
            
            if (Config.ShowTranceText)
            {
                builder.SetTextMode(BarTextMode.Single)
                       .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }
            
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        public void DrawDreadWyrmAether(Vector2 origin)
        {
            if (!Config.ShowDreadwyrmAether)
            {
                return;
            }
            
            SMNGauge gauge = Plugin.JobGauges.Get<SMNGauge>();
            var stacks = gauge.AetherFlags;
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
            
                var filledChunkCount = 0;
            
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
        private void DrawActiveDots(Vector2 origin)
        {
            Debug.Assert(Plugin.ClientState.LocalPlayer != null, "Plugin.ClientState.LocalPlayer != null");
            GameObject? actor = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;

            if (!Config.ShowBio && !Config.ShowMiasma)
            {
                return;
            }

            if (actor is not BattleChara target)
            {
                return;
            }

            Vector2 barSize = Config.MiasmaSize;
            Vector2 position = origin + Config.Position + Config.MiasmaPosition - barSize / 2f;

            List<Bar> barDrawList = new();

            if (Config.ShowMiasma)
            {
                Status? miasma = target.StatusList.FirstOrDefault(
                    o => o.StatusId == 1215 && o.SourceID == Plugin.ClientState.LocalPlayer.ObjectId
                      || o.StatusId == 180 && o.SourceID == Plugin.ClientState.LocalPlayer.ObjectId
                );

                float miasmaDuration = Math.Abs(miasma?.RemainingTime ?? 0f);
                PluginConfigColor miasmaColor = miasmaDuration > 5 ? Config.MiasmaColor : Config.ExpireColor;
                BarBuilder builder = BarBuilder.Create(position, barSize);

                Bar miasmaBar = builder.AddInnerBar(miasmaDuration, 30f, miasmaColor)
                                       .SetFlipDrainDirection(Config.MiasmaInverted)
                                       .Build();

                if (Config.ShowMiasmaText)
                {
                    builder.SetTextMode(BarTextMode.Single)
                           .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
                barDrawList.Add(miasmaBar);
                
            }

            if (Config.ShowBio)
            {
                Status? bio = target.StatusList.FirstOrDefault(
                    o => o.StatusId == 1214 && o.SourceID == Plugin.ClientState.LocalPlayer.ObjectId
                      || o.StatusId == 179 && o.SourceID == Plugin.ClientState.LocalPlayer.ObjectId
                      || o.StatusId == 189 && o.SourceID == Plugin.ClientState.LocalPlayer.ObjectId
                );

                float bioDuration = Math.Abs(bio?.RemainingTime ?? 0f);
                PluginConfigColor bioColor = bioDuration > 5 ? Config.BioColor : Config.ExpireColor;

                barSize = Config.BioSize;
                position = origin + Config.Position + Config.BioPosition - barSize / 2f;

                BarBuilder builder = BarBuilder.Create(position, barSize);

                Bar bioBar = builder.AddInnerBar(bioDuration, 30f, bioColor)
                                    .SetFlipDrainDirection(Config.BioInverted)
                                    .Build();
                if (Config.ShowBioText)
                {
                    builder.SetTextMode(BarTextMode.Single)
                           .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
                barDrawList.Add(bioBar);
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

        private void DrawRuinBar(Vector2 origin)
        {
            Debug.Assert(Plugin.ClientState.LocalPlayer != null, "Plugin.ClientState.LocalPlayer != null");
            Status? ruinBuff = Plugin.ClientState.LocalPlayer.StatusList.FirstOrDefault(o => o.StatusId == 1212);

            Vector2 barSize = Config.RuinSize;
            Vector2 position = origin + Config.Position + Config.RuinPosition - barSize / 2f;

            if (!Config.ShowRuin)
            {
                return;
            }

            Bar bar = BarBuilder.Create(position, barSize)
                                .SetChunks(4)
                                .SetChunkPadding(Config.RuinPadding)
                                .AddInnerBar(ruinBuff?.StackCount ?? 0, 4, Config.RuinColor)
                                .SetBackgroundColor(EmptyColor.Base)
                                .Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        private void DrawAetherBar(Vector2 origin)
        {
            Debug.Assert(Plugin.ClientState.LocalPlayer != null, "Plugin.ClientState.LocalPlayer != null");
            Status? aetherFlowBuff = Plugin.ClientState.LocalPlayer.StatusList.FirstOrDefault(o => o.StatusId == 304);

            Vector2 barSize = Config.AetherSize;
            Vector2 position = origin + Config.Position + Config.AetherPosition - barSize / 2f;

            if (!Config.ShowAether)
            {
                return;
            }

            Bar bar = BarBuilder.Create(position, barSize)
                                .SetChunks(2)
                                .SetChunkPadding(Config.AetherPadding)
                                .AddInnerBar(aetherFlowBuff?.StackCount ?? 0, 2, Config.AetherColor)
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
        public new static SummonerConfig DefaultConfig() => new SummonerConfig();

        #region aether
        [Checkbox("Aether Tracker Enabled", separator = true)]
        [Order(30)]
        public bool ShowAether = true;

        [DragFloat2("Aether Tracker Size", min = 1f, max = 2000f)]
        [Order(35, collapseWith = nameof(ShowAether))]
        public Vector2 AetherSize = new(120, 10);

        [DragFloat2("Aether Tracker Position", min = -4000f, max = 4000f)]
        [Order(40, collapseWith = nameof(ShowAether))]
        public Vector2 AetherPosition = new(-67, -6);

        [DragInt("Aether Padding", max = 1000)]
        [Order(45, collapseWith = nameof(ShowAether))]
        public int AetherPadding = 2;

        [ColorEdit4("Aether Tracker Color")]
        [Order(50, collapseWith = nameof(ShowAether))]
        public PluginConfigColor AetherColor = new(new Vector4(0f / 255f, 255f / 255f, 0f / 255f, 100f / 100f));
        #endregion

        #region ruin
        [Checkbox("Ruin Enabled", separator = true)]
        [Order(55)]
        public bool ShowRuin = true;

        [DragFloat2("Ruin Size", min = 1f, max = 2000f)]
        [Order(60, collapseWith = nameof(ShowRuin))]
        public Vector2 RuinSize = new(254, 20);

        [DragFloat2("Ruin Position", min = -4000f, max = 4000f)]
        [Order(65, collapseWith = nameof(ShowRuin))]
        public Vector2 RuinPosition = new(0, -45);

        [DragInt("Ruin Padding", max = 1000)]
        [Order(70, collapseWith = nameof(ShowRuin))]
        public int RuinPadding = 2;

        [ColorEdit4("Ruin Color")]
        [Order(75, collapseWith = nameof(ShowRuin))]
        public PluginConfigColor RuinColor = new(new Vector4(94f / 255f, 250f / 255f, 154f / 255f, 100f / 100f));
        #endregion

        #region miasma
        [Checkbox("Miasma Enabled", separator = true)]
        [Order(80)]
        public bool ShowMiasma = true;
        
        [Checkbox("Miasma Text")]
        [Order(85, collapseWith = nameof(ShowMiasma))]
        public bool ShowMiasmaText = true;

        [Checkbox("Miasma Inverted")]
        [Order(90, collapseWith = nameof(ShowMiasma))]
        public bool MiasmaInverted = true;

        [DragFloat2("Miasma Size", max = 2000f)]
        [Order(95, collapseWith = nameof(ShowMiasma))]
        public Vector2 MiasmaSize = new(126, 20);

        [DragFloat2("Miasma Position", min = -4000f, max = 4000f)]
        [Order(100, collapseWith = nameof(ShowMiasma))]
        public Vector2 MiasmaPosition = new(-64, -67);

        [ColorEdit4("Miasma Color")]
        [Order(105, collapseWith = nameof(ShowMiasma))]
        public PluginConfigColor MiasmaColor = new(new Vector4(106f / 255f, 237f / 255f, 241f / 255f, 100f / 100f));
        #endregion

        #region bio
        [Checkbox("Bio Enabled", separator = true)]
        [Order(110)]
        public bool ShowBio = true;

        [Checkbox("Bio Text")]
        [Order(115, collapseWith = nameof(ShowBio))]
        public bool ShowBioText = true;

        [Checkbox("Bio Inverted")]
        [Order(120, collapseWith = nameof(ShowBio))]
        public bool BioInverted = false;

        [DragFloat2("Bio Size", max = 2000f)]
        [Order(125, collapseWith = nameof(ShowBio))]
        public Vector2 BioSize = new(126, 20);

        [DragFloat2("Bio Position", min = -4000f, max = 4000f)]
        [Order(130, collapseWith = nameof(ShowBio))]
        public Vector2 BioPosition = new(64, -67);

        [ColorEdit4("Bio Color")]
        [Order(135, collapseWith = nameof(ShowBio))]
        public PluginConfigColor BioColor = new(new Vector4(50f / 255f, 93f / 255f, 37f / 255f, 100f / 100f));
        #endregion

        #region trance
        [Checkbox("Trance Enabled", separator = true)]
        [Order(140)]
        public bool ShowTrance = true;

        [Checkbox("Trance Gauge Text")]
        [Order(145, collapseWith = nameof(ShowTrance))]
        public bool ShowTranceText = true;

        [DragFloat2("Trance Gauge Position", min = -4000f, max = 4000f)]
        [Order(150, collapseWith = nameof(ShowTrance))]
        public Vector2 TrancePosition = new(0, -23);

        [DragFloat2("Trance Gauge Size", min = 1f, max = 2000f)]
        [Order(155, collapseWith = nameof(ShowTrance))]
        public Vector2 TranceSize = new(254, 20);

        [ColorEdit4("Trance Dreadwyrm Color")]
        [Order(160, collapseWith = nameof(ShowTrance))]
        public PluginConfigColor DreadwyrmColor = new(new Vector4(255f / 255f, 255f / 255f, 147f / 255f, 100f / 100f));

        [ColorEdit4("Trance Bahamut Color")]
        [Order(165, collapseWith = nameof(ShowTrance))]
        public PluginConfigColor BahamutColor = new(new Vector4(128f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [ColorEdit4("Trance Phoenix Color")]
        [Order(170, collapseWith = nameof(ShowTrance))]
        public PluginConfigColor PhoenixColor = new(new Vector4(255f / 255f, 128f / 255f, 0f / 255f, 100f / 100f));
        #endregion

        #region dreadwyrm
        [Checkbox("Dreadwyrm Trance Tracker Enabled", separator = true)]
        [Order(175)]
        public bool ShowDreadwyrmAether = true;

        [Checkbox("Demi Status Indicator Enabled")]
        [Order(180, collapseWith = nameof(ShowDreadwyrmAether))]
        public bool ShowDemiIndicator = true;

        [DragFloat2("Demi Status Indicator Position", min = -4000f, max = -4000f)]
        [Order(185, collapseWith = nameof(ShowDreadwyrmAether))]
        public Vector2 IndicatorPosition = new(0, -6);

        [DragFloat2("Demi Status Indicator Size", min = 1f, max = 2000f)]
        [Order(190, collapseWith = nameof(ShowDreadwyrmAether))]
        public Vector2 IndicatorSize = new(10, 10);

        [ColorEdit4("Demi Status Indicator Bahamut Color")]
        [Order(195, collapseWith = nameof(ShowDreadwyrmAether))]
        public PluginConfigColor BahamutReadyColor = new(new Vector4(128f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [ColorEdit4("Demi Status Indicator Phoenix Color")]
        [Order(200, collapseWith = nameof(ShowDreadwyrmAether))]
        public PluginConfigColor PhoenixReadyColor = new(new Vector4(255f / 255f, 128f / 255f, 0f / 255f, 100f / 100f));

        [Checkbox("Dreadwyrm Aether Bars Enabled")]
        [Order(205, collapseWith = nameof(ShowDreadwyrmAether))]
        public bool ShowDreadwyrmAetherBars = true;

        [DragFloat2("Dreadwyrm Aether Bars Position", min = -4000f, max = -4000f)]
        [Order(210, collapseWith = nameof(ShowDreadwyrmAether))]
        public Vector2 DreadwyrmAetherBarPosition = new(67, -6);

        [DragFloat2("Dreadwyrm Aether Bars Size", min = 1f, max = 2000f)]
        [Order(215, collapseWith = nameof(ShowDreadwyrmAether))]
        public Vector2 DreadwyrmAetherBarSize = new(120, 10);

        [DragInt("Dreadwyrm Aether Bar Padding", max = 1000)]
        [Order(220, collapseWith = nameof(ShowDreadwyrmAether))]
        public int DreadwyrmAetherBarPadding = 2;

        [ColorEdit4("Dreadwyrm Aether Bars Color")]
        [Order(225, collapseWith = nameof(ShowDreadwyrmAether))]
        public PluginConfigColor DreadwyrmAetherBarColor = new(new Vector4(128f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));
        #endregion

        [ColorEdit4("DoT Expire Color")]
        [Order(230)]
        public PluginConfigColor ExpireColor = new(new Vector4(230f / 255f, 33f / 255f, 33f / 255f, 53f / 100f));
    }
}