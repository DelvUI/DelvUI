using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Structs;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Interface.Bars;
using ImGuiNET;
using Actor = Dalamud.Game.ClientState.Actors.Types.Actor;

namespace DelvUI.Interface
{
    public class SummonerHudWindow : HudWindow
    {
        private bool _bahamutFinished = true;

        public SummonerHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        public override uint JobId => 27;
        private SummonerHudConfig _config => (SummonerHudConfig)ConfigurationManager.GetInstance().GetConfiguration(new SummonerHudConfig());
        private Vector2 Origin => new(CenterX + _config.Position.X, CenterY + _config.Position.Y);
        private Dictionary<string, uint> EmptyColor => PluginConfiguration.MiscColorMap["empty"];

        protected override void Draw(bool _)
        {
            DrawActiveDots();
            DrawRuinBar();
            DrawAetherBar();
            DrawTranceBar();
            DrawDreadWyrmAether();
        }

        private void DrawTranceBar()
        {
            if (!_config.ShowTrance)
            {
                return;
            }

            SMNGauge gauge = PluginInterface.ClientState.JobGauges.Get<SMNGauge>();

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
                    tranceColor = _config.PhoenixColor;
                    maxDuration = 20000f;

                    break;

                case >= 8:
                    tranceColor = _config.BahamutColor;
                    maxDuration = 20000f;
                    _bahamutFinished = false;

                    break;

                default:
                    // This is needed because as soon as you summon Bahamut the flag goes back to 0-2
                    tranceColor = _bahamutFinished ? _config.DreadwyrmColor : _config.BahamutColor;
                    maxDuration = _bahamutFinished ? 15000f : 20000f;

                    break;
            }

            Vector2 barSize = _config.TranceSize;
            Vector2 position = Origin + _config.TrancePosition - barSize / 2f;

            BarBuilder builder = BarBuilder.Create(position, barSize);

            Bar bar = builder.AddInnerBar(tranceDuration / 1000f, maxDuration / 1000f, tranceColor.Map).SetBackgroundColor(EmptyColor["background"]).Build();

            if (_config.ShowTranceText)
            {
                builder.SetTextMode(BarTextMode.Single)
                       .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        public void DrawDreadWyrmAether()
        {
            if (!_config.ShowDreadwyrmAether)
            {
                return;
            }

            SMNGauge gauge = PluginInterface.ClientState.JobGauges.Get<SMNGauge>();
            var stacks = gauge.NumStacks;
            List<Bar> barDrawList = new();

            if (_config.ShowDemiIndicator)
            {
                Vector2 barSize = _config.IndicatorSize;
                Vector2 position = Origin + _config.IndicatorPosition - barSize / 2f;

                BarBuilder builder = BarBuilder.Create(position, barSize);

                if (stacks >= 8 && stacks < 16)
                {
                    Bar indicatorBar = builder.AddInnerBar(1, 1, _config.BahamutReadyColor.Map)
                                              .SetBackgroundColor(EmptyColor["background"])
                                              .Build();
                    barDrawList.Add(indicatorBar);
                }
                if (stacks >= 16)
                {
                    Bar indicatorBar = builder.AddInnerBar(1, 1, _config.PhoenixReadyColor.Map)
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

            if (_config.ShowDreadwyrmAetherBars)
            {
                Vector2 barSize = _config.DreadwyrmAetherBarSize;
                Vector2 position = Origin + _config.DreadwyrmAetherBarPosition - barSize / 2f;
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
                                                    .SetChunkPadding(_config.DreadwyrmAetherBarPadding)
                                                    .AddInnerBar(filledChunkCount, 2, _config.DreadwyrmAetherBarColor.Map)
                                                    .SetBackgroundColor(EmptyColor["background"])
                                                    .Build();
                barDrawList.Add(DreadwyrmAetherBars);
            }

            if (barDrawList.Count > 0)
            {
                ImDrawListPtr drawList = ImGui.GetWindowDrawList();

                foreach (Bar bar in barDrawList)
                {
                    bar.Draw(drawList, PluginConfiguration);
                }
            }
        }

        protected override void DrawPrimaryResourceBar() { }

        private void DrawActiveDots()
        {
            Actor target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;

            if (!_config.ShowBio && !_config.ShowMiasma)
            {
                return;
            }

            if (target is not Chara)
            {
                return;
            }

            Vector2 barSize = _config.MiasmaSize;
            Vector2 position = Origin + _config.MiasmaPosition - barSize / 2f;

            List<Bar> barDrawList = new();

            if (_config.ShowMiasma)
            {
                StatusEffect miasma = target.StatusEffects.FirstOrDefault(
                    o => o.EffectId == 1215 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                      || o.EffectId == 180 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                );

                float miasmaDuration = Math.Abs(miasma.Duration);
                PluginConfigColor miasmaColor = miasmaDuration > 5 ? _config.MiasmaColor : _config.ExpireColor;
                BarBuilder builder = BarBuilder.Create(position, barSize);

                Bar miasmaBar = builder.AddInnerBar(miasmaDuration, 30f, miasmaColor.Map)
                                       .SetFlipDrainDirection(_config.MiasmaInverted)
                                       .Build();

                barDrawList.Add(miasmaBar);
            }

            if (_config.ShowBio)
            {
                StatusEffect bio = target.StatusEffects.FirstOrDefault(
                    o => o.EffectId == 1214 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                      || o.EffectId == 179 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                      || o.EffectId == 189 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                );

                float bioDuration = Math.Abs(bio.Duration);
                PluginConfigColor bioColor = bioDuration > 5 ? _config.BioColor : _config.ExpireColor;

                barSize = _config.BioSize;
                position = Origin + _config.BioPosition - barSize / 2f;

                BarBuilder builder = BarBuilder.Create(position, barSize);

                Bar bioBar = builder.AddInnerBar(bioDuration, 30f, bioColor.Map)
                                    .SetFlipDrainDirection(_config.BioInverted)
                                    .Build();

                barDrawList.Add(bioBar);
            }

            if (barDrawList.Count > 0)
            {
                ImDrawListPtr drawList = ImGui.GetWindowDrawList();

                foreach (Bar bar in barDrawList)
                {
                    bar.Draw(drawList, PluginConfiguration);
                }
            }
        }

        private void DrawRuinBar()
        {
            StatusEffect ruinBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 1212);
            Vector2 barSize = _config.RuinSize;
            Vector2 position = Origin + _config.RuinPosition - barSize / 2f;

            if (!_config.ShowRuin)
            {
                return;
            }

            Bar bar = BarBuilder.Create(position, barSize)
                                .SetChunks(4)
                                .SetChunkPadding(_config.RuinPadding)
                                .AddInnerBar(ruinBuff.StackCount, 4, _config.RuinColor.Map)
                                .SetBackgroundColor(EmptyColor["background"])
                                .Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawAetherBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            StatusEffect aetherFlowBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 304);
            Vector2 barSize = _config.AetherSize;
            Vector2 position = Origin + _config.AetherPosition - barSize / 2f;

            if (!_config.ShowAether)
            {
                return;
            }

            Bar bar = BarBuilder.Create(position, barSize)
                                .SetChunks(2)
                                .SetChunkPadding(_config.AetherPadding)
                                .AddInnerBar(aetherFlowBuff.StackCount, 2, _config.AetherColor.Map)
                                .SetBackgroundColor(EmptyColor["background"])
                                .Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Caster", 0)]
    [SubSection("Summoner", 1)]
    public class SummonerHudConfig : PluginConfigObject
    {
        [ColorEdit4("Aether Tracker Color")]
        [CollapseWith(15, 1)]
        public PluginConfigColor AetherColor = new(new Vector4(0f / 255f, 255f / 255f, 0f / 255f, 100f / 100f));

        [DragInt("Aether Padding", max = 1000)]
        [CollapseWith(10, 1)]
        public int AetherPadding = 2;

        [DragFloat2("Aether Tracker Position", min = -4000f, max = 4000f)]
        [CollapseWith(5, 1)]
        public Vector2 AetherPosition = new(-67, 454);

        [DragFloat2("Aether Tracker Size", min = 1f, max = 2000f)]
        [CollapseWith(0, 1)]
        public Vector2 AetherSize = new(120, 10);

        [ColorEdit4("Trance Bahamut Color")]
        [CollapseWith(45, 5)]
        public PluginConfigColor BahamutColor = new(new Vector4(128f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [ColorEdit4("Bio Color")]
        [CollapseWith(15, 4)]
        public PluginConfigColor BioColor = new(new Vector4(50f / 255f, 93f / 255f, 37f / 255f, 100f / 100f));

        [Checkbox("Bio Inverted")]
        [CollapseWith(0, 4)]
        public bool BioInverted = false;

        [DragFloat2("Bio Position", min = -4000f, max = 4000f)]
        [CollapseWith(10, 4)]
        public Vector2 BioPosition = new(64, 393);

        [DragFloat2("Bio Size", max = 2000f)]
        [CollapseWith(5, 4)]
        public Vector2 BioSize = new(126, 20);

        [ColorEdit4("Trance Dreadwyrm Color")]
        [CollapseWith(40, 5)]
        public PluginConfigColor DreadwyrmColor = new(new Vector4(255f / 255f, 255f / 255f, 147f / 255f, 100f / 100f));

        [ColorEdit4("DoT Expire Color")]
        [Order(70)]
        public PluginConfigColor ExpireColor = new(new Vector4(230f / 255f, 33f / 255f, 33f / 255f, 53f / 100f));

        [ColorEdit4("Miasma Color")]
        [CollapseWith(15, 3)]
        public PluginConfigColor MiasmaColor = new(new Vector4(106f / 255f, 237f / 255f, 241f / 255f, 100f / 100f));

        [Checkbox("Miasma Inverted")]
        [CollapseWith(0, 3)]
        public bool MiasmaInverted = true;

        [DragFloat2("Miasma Position", min = -4000f, max = 4000f)]
        [CollapseWith(10, 3)]
        public Vector2 MiasmaPosition = new(-64, 393);

        [DragFloat2("Miasma Size", max = 2000f)]
        [CollapseWith(5, 3)]
        public Vector2 MiasmaSize = new(126, 20);

        [ColorEdit4("Trance Phoenix Color")]
        [CollapseWith(50, 5)]
        public PluginConfigColor PhoenixColor = new(new Vector4(255f / 255f, 128f / 255f, 0f / 255f, 100f / 100f));

        [DragFloat2("Base Offset", min = -4000f, max = 4000f)]
        [Order(0)]
        public Vector2 Position = new(0, 0);

        [ColorEdit4(" Ruin Color")]
        [CollapseWith(45, 2)]
        public PluginConfigColor RuinColor = new(new Vector4(94f / 255f, 250f / 255f, 154f / 255f, 100f / 100f));

        [DragInt("Ruin Padding", max = 1000)]
        [CollapseWith(40, 2)]
        public int RuinPadding = 2;

        [DragFloat2("Ruin Position", min = -4000f, max = 4000f)]
        [CollapseWith(35, 2)]
        public Vector2 RuinPosition = new(0, 437);

        [DragFloat2("Ruin Size", min = 1f, max = 2000f)]
        [CollapseWith(30, 2)]
        public Vector2 RuinSize = new(254, 20);

        [Checkbox("Aether Tracker Enabled")]
        [CollapseControl(10, 1)]
        public bool ShowAether = true;

        [Checkbox("Bio Enabled")]
        [CollapseControl(65, 4)]
        public bool ShowBio = true;

        [Checkbox("Miasma Enabled")]
        [CollapseControl(60, 3)]
        public bool ShowMiasma = true;

        [Checkbox("Ruin Enabled")]
        [CollapseControl(15, 2)]
        public bool ShowRuin = true;

        [Checkbox("Trance Enabled")]
        [CollapseControl(20, 5)]
        public bool ShowTrance = true;

        [Checkbox("Trance Gauge Text")]
        [CollapseWith(25, 5)]
        public bool ShowTranceText = true;

        [DragFloat2("Trance Gauge Position", min = -4000f, max = 4000f)]
        [CollapseWith(35, 5)]
        public Vector2 TrancePosition = new(0, 415);

        [DragFloat2("Trance Gauge Size", min = 1f, max = 2000f)]
        [CollapseWith(30, 5)]
        public Vector2 TranceSize = new(254, 20);
        [Checkbox("Dreadwyrm Trance Tracker Enabled")]
        [CollapseControl(25, 6)]
        public bool ShowDreadwyrmAether = true;

        [Checkbox("Demi Status Indicator Enabled")]
        [CollapseWith(0, 6)]
        public bool ShowDemiIndicator = true;

        [DragFloat2("Demi Status Indicator Position", min = -4000f, max = -4000f)]
        [CollapseWith(5, 6)]
        public Vector2 IndicatorPosition = new(0, 454);

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
        public Vector2 DreadwyrmAetherBarPosition = new(67, 454);

        [DragFloat2("Dreadwyrm Aether Bars Size", min = 1f, max = 2000f)]
        [CollapseWith(35, 6)]
        public Vector2 DreadwyrmAetherBarSize = new(120, 10);

        [DragInt("Dreadwyrm Aether Bar Padding", max = 1000)]
        [CollapseWith(40, 6)]
        public int DreadwyrmAetherBarPadding = 2;

        [ColorEdit4("Dreadwyrm Aether Bars Color")]
        [CollapseWith(45, 6)]
        public PluginConfigColor DreadwyrmAetherBarColor = new(new Vector4(128f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));
    }
}
