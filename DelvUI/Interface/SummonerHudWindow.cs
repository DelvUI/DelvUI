using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Interface.Bars;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using DelvUI.Config.Attributes;

namespace DelvUI.Interface
{
    public class SummonerHudWindow : HudWindow
    {
        public SummonerHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        public override uint JobId => 27;
        private SummonerHudConfig _config => (SummonerHudConfig)ConfigurationManager.GetInstance().GetConfiguration(new SummonerHudConfig());
        private Vector2 Origin => new(CenterX + _config.Position.X, CenterY + YOffset + _config.Position.Y);
        private Dictionary<string, uint> EmptyColor => PluginConfiguration.MiscColorMap["empty"];

        protected override void Draw(bool _)
        {
            DrawActiveDots();
            DrawRuinBar();
            DrawAetherBar();
        }

        protected override void DrawPrimaryResourceBar() { }

        private void DrawActiveDots()
        {
            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;

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

            var barDrawList = new List<Bar>();

            if (_config.ShowMiasma)
            {
                var miasma = target.StatusEffects.FirstOrDefault(
                    o => o.EffectId == 1215 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                      || o.EffectId == 180 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                );

                var miasmaDuration = Math.Abs(miasma.Duration);
                var miasmaColor = miasmaDuration > 5 ? _config.MiasmaColor : _config.ExpireColor;
                var builder = BarBuilder.Create(position, barSize);

                var miasmaBar = builder.AddInnerBar(miasmaDuration, 30f, miasmaColor.Map)
                                       .SetFlipDrainDirection(_config.MiasmaInverted)
                                       .Build();

                barDrawList.Add(miasmaBar);
            }

            if (_config.ShowBio)
            {
                var bio = target.StatusEffects.FirstOrDefault(
                    o => o.EffectId == 1214 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                      || o.EffectId == 179 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                      || o.EffectId == 189 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                );

                var bioDuration = Math.Abs(bio.Duration);
                var bioColor = bioDuration > 5 ? _config.BioColor : _config.ExpireColor;
                
                barSize = _config.BioSize;
                position = Origin + _config.BioPosition - barSize / 2f;
                
                var builder = BarBuilder.Create(position, barSize);

                var bioBar = builder.AddInnerBar(bioDuration, 30f, bioColor.Map)
                                    .SetFlipDrainDirection(_config.BioInverted)
                                    .Build();

                barDrawList.Add(bioBar);
            }

            if (barDrawList.Count > 0)
            {
                var drawList = ImGui.GetWindowDrawList();

                foreach (var bar in barDrawList)
                {
                    bar.Draw(drawList, PluginConfiguration);
                }
            }
        }

        private void DrawRuinBar()
        {
            var ruinBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 1212);
            var barSize = _config.RuinSize;
            var position = Origin + _config.RuinPosition - barSize / 2f;

            if (!_config.ShowRuin)
            {
                return;
            }

            var bar = BarBuilder.Create(position, barSize)
                                .SetChunks(4)
                                .SetChunkPadding(_config.RuinPadding)
                                .AddInnerBar(ruinBuff.StackCount, 4, _config.RuinColor.Map)
                                .SetBackgroundColor(EmptyColor["background"])
                                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawAetherBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var aetherFlowBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 304);
            var barSize = _config.AetherSize;
            var position = Origin + _config.AetherPosition - barSize / 2f;

            if (!_config.ShowAether)
            {
                return;
            }

            var bar = BarBuilder.Create(position, barSize)
                                .SetChunks(2)
                                .SetChunkPadding(_config.AetherPadding)
                                .AddInnerBar(aetherFlowBuff.StackCount, 2, _config.AetherColor.Map)
                                .SetBackgroundColor(EmptyColor["background"])
                                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }
    }
    
    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Caster", 0)]
    [SubSection("Summoner", 1)]
    public class SummonerHudConfig : PluginConfigObject
    {
        [DragFloat2("Base Offset", min = -4000f, max = 4000f)]
        [Order(0)]
        public Vector2 Position = new(0, 0);

        [Checkbox("Aether Tracker Enabled")]
        [CollapseControl(10, 1)]
        public bool ShowAether = true;

        [DragFloat2("Aether Tracker Size", min = 1f, max = 2000f)]
        [CollapseWith(0, 1)]
        public Vector2 AetherSize = new(254, 20);

        [DragFloat2("Aether Tracker Position", min = -4000f, max = 4000f)]
        [CollapseWith(5, 1)]
        public Vector2 AetherPosition = new(0, -11);
        
        [DragInt("Aether Padding", max = 1000)]
        [Order(40)]
        public int AetherPadding = 2;

        [ColorEdit4("Aether Tracker Color")]
        [CollapseWith(10, 1)]
        public PluginConfigColor AetherColor = new(new Vector4(0f / 255f, 255f / 255f, 0f / 255f, 100f / 100f));

        [Checkbox("Ruin Enabled")]
        [CollapseControl(15, 2)]
        public bool ShowRuin = true;

        [DragFloat2("Ruin Size", min = 1f, max = 2000f)]
        [CollapseWith(30, 2)]
        public Vector2 RuinSize = new(254, 20);

        [DragFloat2("Ruin Position", min = -4000f, max = 4000f)]
        [CollapseWith(35, 2)]
        public Vector2 RuinPosition = new(0, -33);

        [DragInt("Ruin Padding", max = 1000)]
        [CollapseWith(40, 2)]
        public int RuinPadding = 2;

        [ColorEdit4(" Ruin Color")]
        [CollapseWith(45, 2)]
        public PluginConfigColor RuinColor = new(new Vector4(94f / 255f, 250f / 255f, 154f / 255f, 100f / 100f));

        [Checkbox("Miasma Enabled")]
        [CollapseControl(60, 3)]
        public bool ShowMiasma = true;

        [Checkbox("Miasma Inverted")]
        [CollapseWith(0, 3)]
        public bool MiasmaInverted = true;

        [DragFloat2("Miasma Size", max = 2000f)]
        [CollapseWith(5, 3)]
        public Vector2 MiasmaSize = new(126, 20);

        [DragFloat2("Miasma Position", min = -4000f, max = 4000f)]
        [CollapseWith(10, 3)]
        public Vector2 MiasmaPosition = new(-64, -55);

        [ColorEdit4("Miasma Color")]
        [CollapseWith(15, 3)]
        public PluginConfigColor MiasmaColor = new(new Vector4(106f / 255f, 237f / 255f, 241f / 255f, 100f / 100f));

        [Checkbox("Bio Enabled")]
        [CollapseControl(65, 4)]
        public bool ShowBio = true;

        [Checkbox("Bio Inverted")]
        [CollapseWith(0, 4)]
        public bool BioInverted = false;

        [DragFloat2("Bio Size", max = 2000f)]
        [CollapseWith(5, 4)]
        public Vector2 BioSize = new(126, 20);

        [DragFloat2("Bio Position", min = -4000f, max = 4000f)]
        [CollapseWith(10, 4)]
        public Vector2 BioPosition = new(64, -55);

        [ColorEdit4("Bio Color")]
        [CollapseWith(15, 4)]
        public PluginConfigColor BioColor = new(new Vector4(50f / 255f, 93f / 255f, 37f / 255f, 100f / 100f));

        [ColorEdit4("DoT Expire Color")]
        [Order(70)]
        public PluginConfigColor ExpireColor = new(new Vector4(230f / 255f, 33f / 255f, 33f / 255f, 53f / 100f));
    }
}
