using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Structs;
using Dalamud.Game.ClientState.Structs.JobGauge;
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
using Actor = Dalamud.Game.ClientState.Actors.Types.Actor;

namespace DelvUI.Interface.Jobs
{
    public class SummonerHud : JobHud
    {
        private bool _bahamutFinished = true;

        private new SummonerConfig Config => (SummonerConfig)_config;
        private Dictionary<string, uint> EmptyColor => GlobalColors.Instance.EmptyColor.Map;

        public SummonerHud(string id, SummonerConfig config) : base(id, config)
        {

        }

        public override void Draw(Vector2 origin)
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

            switch (gauge.NumStacks)
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

            Bar bar = builder.AddInnerBar(tranceDuration / 1000f, maxDuration / 1000f, tranceColor.Map).SetBackgroundColor(EmptyColor["background"]).Build();

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
            var stacks = gauge.NumStacks;
            List<Bar> barDrawList = new();

            if (Config.ShowDemiIndicator)
            {
                Vector2 barSize = Config.IndicatorSize;
                Vector2 position = origin + Config.Position + Config.IndicatorPosition - barSize / 2f;

                BarBuilder builder = BarBuilder.Create(position, barSize);

                if (stacks >= 8 && stacks < 16)
                {
                    Bar indicatorBar = builder.AddInnerBar(1, 1, Config.BahamutReadyColor.Map)
                                              .SetBackgroundColor(EmptyColor["background"])
                                              .Build();
                    barDrawList.Add(indicatorBar);
                }
                if (stacks >= 16)
                {
                    Bar indicatorBar = builder.AddInnerBar(1, 1, Config.PhoenixReadyColor.Map)
                                              .SetBackgroundColor(EmptyColor["background"])
                                              .Build();
                    barDrawList.Add(indicatorBar);
                }
                if (stacks < 8)
                {
                    Bar indicatorBar = builder.SetBackgroundColor(EmptyColor["background"])
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
                                                    .AddInnerBar(filledChunkCount, 2, Config.DreadwyrmAetherBarColor.Map)
                                                    .SetBackgroundColor(EmptyColor["background"])
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
            Actor target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.CurrentTarget;

            if (!Config.ShowBio && !Config.ShowMiasma)
            {
                return;
            }

            if (target is not Chara)
            {
                return;
            }

            Vector2 barSize = Config.MiasmaSize;
            Vector2 position = origin + Config.Position + Config.MiasmaPosition - barSize / 2f;

            List<Bar> barDrawList = new();

            if (Config.ShowMiasma)
            {
                StatusEffect miasma = target.StatusEffects.FirstOrDefault(
                    o => o.EffectId == 1215 && o.OwnerId == Plugin.ClientState.LocalPlayer.ActorId
                      || o.EffectId == 180 && o.OwnerId == Plugin.ClientState.LocalPlayer.ActorId
                );

                float miasmaDuration = Math.Abs(miasma.Duration);
                PluginConfigColor miasmaColor = miasmaDuration > 5 ? Config.MiasmaColor : Config.ExpireColor;
                BarBuilder builder = BarBuilder.Create(position, barSize);

                Bar miasmaBar = builder.AddInnerBar(miasmaDuration, 30f, miasmaColor.Map)
                                       .SetFlipDrainDirection(Config.MiasmaInverted)
                                       .Build();

                barDrawList.Add(miasmaBar);
            }

            if (Config.ShowBio)
            {
                StatusEffect bio = target.StatusEffects.FirstOrDefault(
                    o => o.EffectId == 1214 && o.OwnerId == Plugin.ClientState.LocalPlayer.ActorId
                      || o.EffectId == 179 && o.OwnerId == Plugin.ClientState.LocalPlayer.ActorId
                      || o.EffectId == 189 && o.OwnerId == Plugin.ClientState.LocalPlayer.ActorId
                );

                float bioDuration = Math.Abs(bio.Duration);
                PluginConfigColor bioColor = bioDuration > 5 ? Config.BioColor : Config.ExpireColor;

                barSize = Config.BioSize;
                position = origin + Config.Position + Config.BioPosition - barSize / 2f;

                BarBuilder builder = BarBuilder.Create(position, barSize);

                Bar bioBar = builder.AddInnerBar(bioDuration, 30f, bioColor.Map)
                                    .SetFlipDrainDirection(Config.BioInverted)
                                    .Build();

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
            StatusEffect ruinBuff = Plugin.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 1212);

            Vector2 barSize = Config.RuinSize;
            Vector2 position = origin + Config.Position + Config.RuinPosition - barSize / 2f;

            if (!Config.ShowRuin)
            {
                return;
            }

            Bar bar = BarBuilder.Create(position, barSize)
                                .SetChunks(4)
                                .SetChunkPadding(Config.RuinPadding)
                                .AddInnerBar(ruinBuff.StackCount, 4, Config.RuinColor.Map)
                                .SetBackgroundColor(EmptyColor["background"])
                                .Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        private void DrawAetherBar(Vector2 origin)
        {
            StatusEffect aetherFlowBuff = Plugin.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 304);

            Vector2 barSize = Config.AetherSize;
            Vector2 position = origin + Config.Position + Config.AetherPosition - barSize / 2f;

            if (!Config.ShowAether)
            {
                return;
            }

            Bar bar = BarBuilder.Create(position, barSize)
                                .SetChunks(2)
                                .SetChunkPadding(Config.AetherPadding)
                                .AddInnerBar(aetherFlowBuff.StackCount, 2, Config.AetherColor.Map)
                                .SetBackgroundColor(EmptyColor["background"])
                                .Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Caster", 0)]
    [SubSection("Summoner", 1)]
    public class SummonerConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.SMN;
        public new static SummonerConfig DefaultConfig() { return new SummonerConfig(); }

        #region aether
        [Checkbox("Aether Tracker Enabled")]
        [CollapseControl(30, 1)]
        public bool ShowAether = true;

        [DragFloat2("Aether Tracker Size", min = 1f, max = 2000f)]
        [CollapseWith(0, 1)]
        public Vector2 AetherSize = new(120, 10);

        [DragFloat2("Aether Tracker Position", min = -4000f, max = 4000f)]
        [CollapseWith(5, 1)]
        public Vector2 AetherPosition = new(-67, HUDConstants.JobHudsBaseY - 6);

        [DragInt("Aether Padding", max = 1000)]
        [CollapseWith(10, 1)]
        public int AetherPadding = 2;

        [ColorEdit4("Aether Tracker Color")]
        [CollapseWith(15, 1)]
        public PluginConfigColor AetherColor = new(new Vector4(0f / 255f, 255f / 255f, 0f / 255f, 100f / 100f));
        #endregion

        #region ruin
        [Checkbox("Ruin Enabled")]
        [CollapseControl(35, 2)]
        public bool ShowRuin = true;

        [DragFloat2("Ruin Size", min = 1f, max = 2000f)]
        [CollapseWith(0, 2)]
        public Vector2 RuinSize = new(254, 20);

        [DragFloat2("Ruin Position", min = -4000f, max = 4000f)]
        [CollapseWith(5, 2)]
        public Vector2 RuinPosition = new(0, HUDConstants.JobHudsBaseY - 45);

        [DragInt("Ruin Padding", max = 1000)]
        [CollapseWith(10, 2)]
        public int RuinPadding = 2;

        [ColorEdit4("Ruin Color")]
        [CollapseWith(15, 2)]
        public PluginConfigColor RuinColor = new(new Vector4(94f / 255f, 250f / 255f, 154f / 255f, 100f / 100f));
        #endregion

        #region miasma
        [Checkbox("Miasma Enabled")]
        [CollapseControl(40, 3)]
        public bool ShowMiasma = true;

        [Checkbox("Miasma Inverted")]
        [CollapseWith(0, 3)]
        public bool MiasmaInverted = true;

        [DragFloat2("Miasma Size", max = 2000f)]
        [CollapseWith(5, 3)]
        public Vector2 MiasmaSize = new(126, 20);

        [DragFloat2("Miasma Position", min = -4000f, max = 4000f)]
        [CollapseWith(10, 3)]
        public Vector2 MiasmaPosition = new(-64, HUDConstants.JobHudsBaseY - 67);

        [ColorEdit4("Miasma Color")]
        [CollapseWith(15, 3)]
        public PluginConfigColor MiasmaColor = new(new Vector4(106f / 255f, 237f / 255f, 241f / 255f, 100f / 100f));
        #endregion

        #region bio
        [Checkbox("Bio Enabled")]
        [CollapseControl(45, 4)]
        public bool ShowBio = true;

        [Checkbox("Bio Inverted")]
        [CollapseWith(0, 4)]
        public bool BioInverted = false;

        [DragFloat2("Bio Size", max = 2000f)]
        [CollapseWith(5, 4)]
        public Vector2 BioSize = new(126, 20);

        [DragFloat2("Bio Position", min = -4000f, max = 4000f)]
        [CollapseWith(10, 4)]
        public Vector2 BioPosition = new(64, HUDConstants.JobHudsBaseY - 67);

        [ColorEdit4("Bio Color")]
        [CollapseWith(15, 4)]
        public PluginConfigColor BioColor = new(new Vector4(50f / 255f, 93f / 255f, 37f / 255f, 100f / 100f));
        #endregion

        #region trance
        [Checkbox("Trance Enabled")]
        [CollapseControl(50, 5)]
        public bool ShowTrance = true;

        [Checkbox("Trance Gauge Text")]
        [CollapseWith(0, 5)]
        public bool ShowTranceText = true;

        [DragFloat2("Trance Gauge Position", min = -4000f, max = 4000f)]
        [CollapseWith(5, 5)]
        public Vector2 TrancePosition = new(0, HUDConstants.JobHudsBaseY - 23);

        [DragFloat2("Trance Gauge Size", min = 1f, max = 2000f)]
        [CollapseWith(10, 5)]
        public Vector2 TranceSize = new(254, 20);

        [ColorEdit4("Trance Dreadwyrm Color")]
        [CollapseWith(15, 5)]
        public PluginConfigColor DreadwyrmColor = new(new Vector4(255f / 255f, 255f / 255f, 147f / 255f, 100f / 100f));

        [ColorEdit4("Trance Bahamut Color")]
        [CollapseWith(20, 5)]
        public PluginConfigColor BahamutColor = new(new Vector4(128f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [ColorEdit4("Trance Phoenix Color")]
        [CollapseWith(25, 5)]
        public PluginConfigColor PhoenixColor = new(new Vector4(255f / 255f, 128f / 255f, 0f / 255f, 100f / 100f));
        #endregion

        #region dreadwyrm
        [Checkbox("Dreadwyrm Trance Tracker Enabled")]
        [CollapseControl(55, 6)]
        public bool ShowDreadwyrmAether = true;

        [Checkbox("Demi Status Indicator Enabled")]
        [CollapseWith(0, 6)]
        public bool ShowDemiIndicator = true;

        [DragFloat2("Demi Status Indicator Position", min = -4000f, max = -4000f)]
        [CollapseWith(5, 6)]
        public Vector2 IndicatorPosition = new(0, HUDConstants.JobHudsBaseY - 6);

        [DragFloat2("Demi Status Indicator Size", min = 1f, max = 2000f)]
        [CollapseWith(10, 6)]
        public Vector2 IndicatorSize = new(10, 10);

        [ColorEdit4("Demi Status Indicator Bahamut Color")]
        [CollapseWith(15, 6)]
        public PluginConfigColor BahamutReadyColor = new(new Vector4(128f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [ColorEdit4("Demi Status Indicator Phoenix Color")]
        [CollapseWith(20, 6)]
        public PluginConfigColor PhoenixReadyColor = new(new Vector4(255f / 255f, 128f / 255f, 0f / 255f, 100f / 100f));

        [Checkbox("Dreadwyrm Aether Bars Enabled")]
        [CollapseWith(25, 6)]
        public bool ShowDreadwyrmAetherBars = true;

        [DragFloat2("Dreadwyrm Aether Bars Position", min = -4000f, max = -4000f)]
        [CollapseWith(30, 6)]
        public Vector2 DreadwyrmAetherBarPosition = new(67, HUDConstants.JobHudsBaseY - 6);

        [DragFloat2("Dreadwyrm Aether Bars Size", min = 1f, max = 2000f)]
        [CollapseWith(35, 6)]
        public Vector2 DreadwyrmAetherBarSize = new(120, 10);

        [DragInt("Dreadwyrm Aether Bar Padding", max = 1000)]
        [CollapseWith(40, 6)]
        public int DreadwyrmAetherBarPadding = 2;

        [ColorEdit4("Dreadwyrm Aether Bars Color")]
        [CollapseWith(45, 6)]
        public PluginConfigColor DreadwyrmAetherBarColor = new(new Vector4(128f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));
        #endregion

        [ColorEdit4("DoT Expire Color")]
        [Order(60)]
        public PluginConfigColor ExpireColor = new(new Vector4(230f / 255f, 33f / 255f, 33f / 255f, 53f / 100f));
    }
}
