using System;
using System.Collections.Generic;
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
    public class NinjaHudWindow : HudWindow
    {
        public override uint JobId => Jobs.NIN;

        private NinjaHudConfig _config => (NinjaHudConfig)ConfigurationManager.GetInstance().GetConfiguration(new NinjaHudConfig());

        private Dictionary<string, uint> EmptyColor => PluginConfiguration.MiscColorMap["empty"];

        private Dictionary<string, uint> NinkiNotFilledColor => PluginConfiguration.MiscColorMap["partial"];

        public NinjaHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _)
        {
            if (_config.ShowHutonGauge)
            {
                DrawHutonGauge();
            }

            if (_config.ShowNinkiGauge)
            {
                DrawNinkiGauge();
            }

            if (_config.ShowTrickBar)
            {
                DrawTrickAndSuitonGauge();
            }
        }

        protected override void DrawPrimaryResourceBar() { }

        private void DrawHutonGauge()
        {
            NINGauge gauge = PluginInterface.ClientState.JobGauges.Get<NINGauge>();
            var hutonDurationLeft = (int)Math.Ceiling((float)(gauge.HutonTimeLeft / (double)1000));

            var xPos = CenterX - _config.Position.X + _config.HutonGaugeOffset.X;
            var yPos = CenterY + _config.Position.Y + _config.HutonGaugeOffset.Y;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, _config.HutonGaugeSize.Y, _config.HutonGaugeSize.X);
            var maximum = 70f;

            Bar bar = builder.AddInnerBar(Math.Abs(hutonDurationLeft), maximum, _config.HutonGaugeColor.Map)
                             .SetTextMode(BarTextMode.Single)
                             .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                             .SetBackgroundColor(EmptyColor["background"])
                             .Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawNinkiGauge()
        {
            NINGauge gauge = PluginInterface.ClientState.JobGauges.Get<NINGauge>();

            var xPos = CenterX - _config.Position.X + _config.NinkiGaugeOffset.X;
            var yPos = CenterY + _config.Position.Y + _config.NinkiGaugeOffset.Y;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, _config.NinkiGaugeSize.Y, _config.NinkiGaugeSize.X);

            if (_config.ChunkNinkiGauge)
            {
                builder.SetChunks(2).SetChunkPadding(_config.NinkiGaugeChunkPadding).AddInnerBar(gauge.Ninki, 100, _config.NinkiGaugeColor.Map, NinkiNotFilledColor);
            }
            else
            {
                builder.AddInnerBar(gauge.Ninki, 100, _config.NinkiGaugeColor.Map);
            }

            builder.SetBackgroundColor(EmptyColor["background"]);

            if (_config.ShowNinkiGaugeText)
            {
                builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            Bar bar = builder.Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawTrickAndSuitonGauge()
        {
            var xPos = CenterX - _config.Position.X + _config.TrickBarOffset.X;
            var yPos = CenterY + _config.Position.Y + _config.TrickBarOffset.Y;

            Actor target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;
            var trickDuration = 0f;
            const float trickMaxDuration = 15f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, _config.TrickBarSize.Y, _config.TrickBarSize.X);

            if (target is Chara)
            {
                StatusEffect trickStatus = target.StatusEffects.FirstOrDefault(o => o.EffectId == 638 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId);
                trickDuration = Math.Max(trickStatus.Duration, 0);
            }

            builder.AddInnerBar(trickDuration, trickMaxDuration, _config.TrickBarColor.Map);

            if (trickDuration != 0 && _config.ShowTrickBarText)
            {
                builder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            IEnumerable<StatusEffect> suitonBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 507);

            if (suitonBuff.Any() && _config.ShowSuitonBar)
            {
                var suitonDuration = Math.Abs(suitonBuff.First().Duration);
                builder.AddInnerBar(suitonDuration, 20, _config.SuitonBarColor.Map);

                if (_config.ShowSuitonBarText)
                {
                    builder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterRight, BarTextType.Current, _config.SuitonBarColor.Vector, Vector4.UnitW, null);
                }
            }

            Bar bar = builder.Build();
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Melee", 0)]
    [SubSection("Ninja", 1)]
    public class NinjaHudConfig : PluginConfigObject
    {
        [DragFloat2("Base Offset", min = -4000f, max = 4000f)]
        public Vector2 Position = new(127, 417);

        [Checkbox("Show Huton Gauge")]
        public bool ShowHutonGauge = true;

        [DragFloat2("Huton Gauge Size", max = 2000f)]
        public Vector2 HutonGaugeSize = new(254, 20);

        [DragFloat2("Huton Gauge Offset", min = -4000f, max = 4000f)]
        public Vector2 HutonGaugeOffset = new(0, 0);

        [Checkbox("Show Ninki Gauge")]
        public bool ShowNinkiGauge = true;

        [Checkbox("Show Ninki Gauge Text")]
        public bool ShowNinkiGaugeText = true;

        [Checkbox("Chunk Ninki Gauge")]
        public bool ChunkNinkiGauge = true;

        [DragFloat2("Ninki Gauge Size", max = 2000f)]
        public Vector2 NinkiGaugeSize = new(254, 20);

        [DragFloat2("Ninki Gauge Offset", min = -4000f, max = 4000f)]
        public Vector2 NinkiGaugeOffset = new(0, 22);

        [DragFloat("Ninki Gauge Chunk Padding", min = -4000f, max = 4000f)]
        public float NinkiGaugeChunkPadding = 2;

        [Checkbox("Show Trick Bar")]
        public bool ShowTrickBar = false;

        [Checkbox("Show Trick Bar Text")]
        public bool ShowTrickBarText = true;

        [Checkbox("Show Suiton Bar")]
        public bool ShowSuitonBar = true;

        [Checkbox("Show Suiton Bar Text")]
        public bool ShowSuitonBarText = true;

        [DragFloat2("Trick/Suiton Bar Size", max = 2000f)]
        public Vector2 TrickBarSize = new(254, 20);

        [DragFloat2("Trick/Suiton Bar Offset", min = -4000f, max = 4000f)]
        public Vector2 TrickBarOffset = new(0, 44);

        [ColorEdit4("Huton Gauge Color")]
        public PluginConfigColor HutonGaugeColor = new(new Vector4(110f / 255f, 197f / 255f, 207f / 255f, 100f / 100f));

        [ColorEdit4("Ninki Gauge Color")]
        public PluginConfigColor NinkiGaugeColor = new(new Vector4(137f / 255f, 82f / 255f, 236f / 255f, 100f / 100f));

        [ColorEdit4("Trick Bar Color")]
        public PluginConfigColor TrickBarColor = new(new Vector4(191f / 255f, 40f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Suiton Bar Color")]
        public PluginConfigColor SuitonBarColor = new(new Vector4(202f / 255f, 228f / 255f, 246f / 242f, 100f / 100f));
    }
}
