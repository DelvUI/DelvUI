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
    public class ScholarHudWindow : HudWindow
    {
        public ScholarHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        public override uint JobId => 28;

        private ScholarHudConfig _config => (ScholarHudConfig)ConfigurationManager.GetInstance().GetConfiguration(new ScholarHudConfig());
        private Vector2 Origin => new(CenterX + _config.Position.X, CenterY + _config.Position.Y);
        private Dictionary<string, uint> EmptyColor => PluginConfiguration.MiscColorMap["empty"];

        protected override void Draw(bool _)
        {
            if (_config.ShowFairy)
            {
                DrawFairyBar();
            }

            if (_config.ShowBio)
            {
                DrawBioBar();
            }

            if (_config.ShowAether)
            {
                DrawAetherBar();
            }
        }

        protected override void DrawPrimaryResourceBar()
        {
            if (!_config.ShowPrimary)
            {
                return;
            }

            base.DrawPrimaryResourceBar();
        }

        private void DrawFairyBar()
        {
            float fairyGauge = PluginInterface.ClientState.JobGauges.Get<SCHGauge>().FairyGaugeAmount;

            Vector2 barSize = _config.FairySize;
            Vector2 position = Origin + _config.FairyPosition - barSize / 2f;

            BarBuilder builder = BarBuilder.Create(position, barSize);

            Bar bar = builder.AddInnerBar(fairyGauge, 100f, _config.FairyColor.Map).SetBackgroundColor(EmptyColor["background"]).Build();

            if (_config.ShowFairyText)
            {
                builder.SetTextMode(BarTextMode.Single)
                       .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

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
                                .SetChunks(3)
                                .SetChunkPadding(_config.AetherPadding)
                                .AddInnerBar(aetherFlowBuff.StackCount, 3, _config.AetherColor.Map)
                                .SetBackgroundColor(EmptyColor["background"])
                                .Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawBioBar()
        {
            Actor target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;

            float bioDuration = 0;

            if (target is Chara)
            {
                StatusEffect bio = target.StatusEffects.FirstOrDefault(
                    o => o.EffectId == 179 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                      || o.EffectId == 189 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                      || o.EffectId == 1895 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                );

                bioDuration = Math.Abs(bio.Duration);
            }

            PluginConfigColor bioColor = bioDuration > 5 ? _config.BioColor : _config.ExpireColor;

            Vector2 barSize = _config.BioSize;
            Vector2 position = Origin + _config.BioPosition - barSize / 2f;

            BarBuilder builder = BarBuilder.Create(position, barSize);

            Bar bioBar = builder.AddInnerBar(bioDuration, 30f, bioColor.Map)
                                .SetFlipDrainDirection(_config.BioInverted)
                                .Build();

            if (_config.ShowBioText && bioDuration != 0)
            {
                builder.SetTextMode(BarTextMode.Single)
                       .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bioBar.Draw(drawList, PluginConfiguration);
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Healer", 0)]
    [SubSection("Scholar", 1)]
    public class ScholarHudConfig : PluginConfigObject
    {
        [ColorEdit4("Aether Tracker Color")]
        [CollapseWith(15, 1)]
        public PluginConfigColor AetherColor = new(new Vector4(0f / 255f, 255f / 255f, 0f / 255f, 100f / 100f));

        [DragInt("Aether Padding", max = 1000)]
        [CollapseWith(10, 1)]
        public int AetherPadding = 2;

        [DragFloat2("Aether Tracker Position", min = -4000f, max = 4000f)]
        [CollapseWith(5, 1)]
        public Vector2 AetherPosition = new(0, 383);

        [DragFloat2("Aether Tracker Size", min = 1f, max = 2000f)]
        [CollapseWith(0, 1)]
        public Vector2 AetherSize = new(254, 20);

        [ColorEdit4("Bio Color")]
        [CollapseWith(15, 3)]
        public PluginConfigColor BioColor = new(new Vector4(50f / 255f, 93f / 255f, 37f / 255f, 1f));

        [Checkbox("Bio Inverted")]
        [CollapseWith(5, 3)]
        public bool BioInverted = false;

        [DragFloat2("Bio Position", min = -4000f, max = 4000f)]
        [CollapseWith(20, 3)]
        public Vector2 BioPosition = new(0, 427);

        [DragFloat2("Bio Size", max = 2000f)]
        [CollapseWith(10, 3)]
        public Vector2 BioSize = new(254, 20);

        [ColorEdit4("DoT Expire Color")]
        [Order(70)]
        public PluginConfigColor ExpireColor = new(new Vector4(230f / 255f, 33f / 255f, 33f / 255f, 53f / 100f));

        [ColorEdit4(" Fairy Gauge Color")]
        [CollapseWith(40, 2)]
        public PluginConfigColor FairyColor = new(new Vector4(94f / 255f, 250f / 255f, 154f / 255f, 100f / 100f));

        [DragFloat2("Fairy Gauge Position", min = -4000f, max = 4000f)]
        [CollapseWith(35, 2)]
        public Vector2 FairyPosition = new(0, 405);

        [DragFloat2("Fairy Gauge Size", min = 1f, max = 2000f)]
        [CollapseWith(30, 2)]
        public Vector2 FairySize = new(254, 20);

        [DragFloat2("Base Position", min = -4000f, max = 4000f)]
        [Order(0)]
        public Vector2 Position = new(0, 0);

        [Checkbox("Aether Tracker Enabled")]
        [CollapseControl(10, 1)]
        public bool ShowAether = true;

        [Checkbox("Bio Enabled")]
        [CollapseControl(65, 3)]
        public bool ShowBio = true;

        [Checkbox("Bio Text")]
        [CollapseWith(0, 3)]
        public bool ShowBioText = true;

        [Checkbox("Fairy Gauge Enabled")]
        [CollapseControl(15, 2)]
        public bool ShowFairy = true;

        [Checkbox("Fairy Gauge Text")]
        [CollapseWith(25, 2)]
        public bool ShowFairyText = true;

        [Checkbox("Primary Resource Enabled")]
        [Order(10)]
        public bool ShowPrimary = true;
    }
}
