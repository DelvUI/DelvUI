using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface
{
    public class PaladinHudWindow : HudWindow
    {
        public override uint JobId => Jobs.PLD;

        private PaladinHudConfig _config => (PaladinHudConfig)ConfigurationManager.GetInstance().GetConfiguration(new PaladinHudConfig());

        private Dictionary<string, uint> EmptyColor => PluginConfiguration.MiscColorMap["empty"];

        private Dictionary<string, uint> OathGaugeNotFilledColor => PluginConfiguration.MiscColorMap["partial"];

        public PaladinHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration)
            : base(pluginInterface, pluginConfiguration)
        { }

        protected override void Draw(bool _)
        {
            if (_config.ShowManaBar)
            {
                DrawManaBar();
            }

            if (_config.ShowOathGauge)
            {
                DrawOathGauge();
            }

            if (_config.ShowBuffBar)
            {
                DrawBuffBar();
            }

            if (_config.ShowAtonementBar)
            {
                DrawAtonementBar();
            }

            if (_config.ShowGoringBladeBar)
            {
                DrawDoTBar();
            }
        }

        protected override void DrawPrimaryResourceBar() { }

        private void DrawManaBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var actor = PluginInterface.ClientState.LocalPlayer;

            var posX = CenterX + _config.Position.X - _config.ManaBarOffset.X;
            var posY = CenterY + _config.Position.Y + _config.ManaBarOffset.Y;

            var builder = BarBuilder.Create(posX, posY, _config.ManaBarSize.Y, _config.ManaBarSize.X)
                                    .SetBackgroundColor(EmptyColor["background"]);

            if (_config.ChunkManaBar)
            {
                builder.SetChunks(5)
                       .SetChunkPadding(_config.ManaBarPadding)
                       .AddInnerBar(actor.CurrentMp, actor.MaxMp, _config.ManaBarColor.Map, EmptyColor);
            }
            else
            {
                builder.AddInnerBar(actor.CurrentMp, actor.MaxMp, _config.ManaBarColor.Map);
            }

            if (_config.ShowManaBarText)
            {
                var formattedManaText = TextTags.GenerateFormattedTextFromTags(actor, "[mana:current-short]");

                builder.SetTextMode(BarTextMode.Single)
                       .SetText(BarTextPosition.CenterLeft, BarTextType.Custom, formattedManaText);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawOathGauge()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<PLDGauge>();

            var xPos = CenterX + _config.Position.X - _config.OathGaugeOffset.X;
            var yPos = CenterY + _config.Position.Y + _config.OathGaugeOffset.Y;

            var builder = BarBuilder.Create(xPos, yPos, _config.OathGaugeSize.Y, _config.OathGaugeSize.X)
                                    .SetChunks(2)
                                    .SetChunkPadding(_config.OathGaugePadding)
                                    .SetBackgroundColor(EmptyColor["background"])
                                    .AddInnerBar(gauge.GaugeAmount, 100, _config.OathGaugeColor.Map, OathGaugeNotFilledColor);

            if (_config.ShowOathGaugeText)
            {
                builder.SetTextMode(BarTextMode.EachChunk)
                       .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawBuffBar()
        {
            var fightOrFlightBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 76);
            var requiescatBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1368);

            var xPos = CenterX + _config.Position.X - _config.BuffBarOffset.X;
            var yPos = CenterY + _config.Position.Y + _config.BuffBarOffset.Y;

            var builder = BarBuilder.Create(xPos, yPos, _config.BuffBarSize.Y, _config.BuffBarSize.X)
                                    .SetBackgroundColor(EmptyColor["background"]);

            if (fightOrFlightBuff.Any())
            {
                var fightOrFlightDuration = Math.Abs(fightOrFlightBuff.First().Duration);
                builder.AddInnerBar(fightOrFlightDuration, 25, _config.FightOrFlightColor.Map);

                if (_config.ShowBuffBarText)
                {
                    builder.SetTextMode(BarTextMode.EachChunk)
                           .SetText(BarTextPosition.CenterLeft, BarTextType.Current, _config.FightOrFlightColor.Vector, Vector4.UnitW, null);
                }
            }

            if (requiescatBuff.Any())
            {
                var requiescatDuration = Math.Abs(requiescatBuff.First().Duration);
                builder.AddInnerBar(requiescatDuration, 12, _config.RequiescatColor.Map);

                if (_config.ShowBuffBarText)
                {
                    builder.SetTextMode(BarTextMode.EachChunk)
                           .SetText(BarTextPosition.CenterRight, BarTextType.Current, _config.RequiescatColor.Vector, Vector4.UnitW, null);
                }
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawAtonementBar()
        {
            var atonementBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1902);
            var stackCount = atonementBuff.Any() ? atonementBuff.First().StackCount : 0;

            var xPos = CenterX + _config.Position.X - _config.AtonementBarOffset.X;
            var yPos = CenterY + _config.Position.Y + _config.AtonementBarOffset.Y;

            var builder = BarBuilder.Create(xPos, yPos, _config.AtonementBarSize.Y, _config.AtonementBarSize.X)
                                    .SetChunks(3)
                                    .SetChunkPadding(_config.AtonementBarPadding)
                                    .SetBackgroundColor(EmptyColor["background"])
                                    .AddInnerBar(stackCount, 3, _config.AtonementColor.Map, null);

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawDoTBar()
        {
            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;

            if (target is not Chara)
            {
                return;
            }

            var goringBlade = target.StatusEffects.FirstOrDefault(
                o =>
                    o.EffectId == 725 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
            );

            var duration = Math.Abs(goringBlade.Duration);

            var xPos = CenterX + _config.Position.X - _config.GoringBladeBarOffset.X;
            var yPos = CenterY + _config.Position.Y + _config.GoringBladeBarOffset.Y;

            var builder = BarBuilder.Create(xPos, yPos, _config.GoringBladeBarSize.Y, _config.GoringBladeBarSize.X)
                                    .AddInnerBar(duration, 21, _config.GoringBladeColor.Map)
                                    .SetBackgroundColor(EmptyColor["background"]);

            if (_config.ShowGoringBladeBarText)
            {
                builder.SetTextMode(BarTextMode.EachChunk)
                       .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }
    }
    
    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Tank", 0)]
    [SubSection("Paladin", 1)]
    public class PaladinHudConfig : PluginConfigObject
    {
        [DragFloat2("Base Offset", min = -4000f, max = 4000f)]
        public Vector2 Position = new Vector2(0, 0);

        [Checkbox("Show Mana Bar")]
        public bool ShowManaBar = true;

        [Checkbox("Show Mana Bar Text")]
        public bool ShowManaBarText = true;

        [Checkbox("Chunk Mana Bar")]
        public bool ChunkManaBar = true;

        [DragFloat2("Mana Bar Size", max = 2000f)]
        public Vector2 ManaBarSize = new Vector2(254, 20);

        [DragInt("Mana Bar Padding", max = 100)]
        public int ManaBarPadding = 2;

        [DragFloat2("Mana Bar Offset", min = -4000f, max = 4000f)]
        public Vector2 ManaBarOffset = new Vector2(127, 373);

        [Checkbox("Show Oath Gauge")]
        public bool ShowOathGauge = true;

        [Checkbox("Show Oath Gauge Text")]
        public bool ShowOathGaugeText = true;

        [DragFloat2("Oath Gauge Size", min = -4000f, max = 4000f)]
        public Vector2 OathGaugeSize = new Vector2(254, 20);

        [DragInt("Oath Gauge Padding", max = 100)]
        public int OathGaugePadding = 2;

        [DragFloat2("Oath Gauge Offset", min = -4000f, max = 4000f)]
        public Vector2 OathGaugeOffset = new Vector2(127, 395);

        [Checkbox("Show Buff Bar")]
        public bool ShowBuffBar = true;

        [Checkbox("Show Buff Bar Text")]
        public bool ShowBuffBarText = true;

        [DragFloat2("Buff Bar Size", min = -4000f, max = 4000f)]
        public Vector2 BuffBarSize = new Vector2(254, 20);

        [DragFloat2("Buff Bar Offset", min = -4000f, max = 4000f)]
        public Vector2 BuffBarOffset = new Vector2(127, 417);

        [Checkbox("Show Atonement Bar")]
        public bool ShowAtonementBar = true;

        [DragFloat2("Atonement Bar Size", min = -4000f, max = 4000f)]
        public Vector2 AtonementBarSize = new Vector2(254, 20);

        [DragInt("Atonement Bar Padding", max = 100)]
        public int AtonementBarPadding = 2;

        [DragFloat2("Atonement Bar Offset", min = -4000f, max = 4000f)]
        public Vector2 AtonementBarOffset = new Vector2(127, 439);

        [Checkbox("Show Goring Blade Bar")]
        public bool ShowGoringBladeBar = true;

        [Checkbox("Show Goring Blade Bar Text")]
        public bool ShowGoringBladeBarText = true;

        [DragFloat2("Goring Blade Bar Size", min = -4000f, max = 4000f)]
        public Vector2 GoringBladeBarSize = new Vector2(254, 20);

        [DragFloat2("Goring Blade Bar Offset", min = -4000f, max = 4000f)]
        public Vector2 GoringBladeBarOffset = new Vector2(127, 351);

        [ColorEdit4("Mana Bar Color")]
        public PluginConfigColor ManaBarColor = new PluginConfigColor(new Vector4(0f / 255f, 203f / 255f, 230f / 255f, 100f / 100f));

        [ColorEdit4("Oath Gauge Color")]
        public PluginConfigColor OathGaugeColor = new PluginConfigColor(new Vector4(24f / 255f, 80f / 255f, 175f / 255f, 100f / 100f));

        [ColorEdit4("Fight or Flight Bar Color")]
        public PluginConfigColor FightOrFlightColor = new PluginConfigColor(new Vector4(240f / 255f, 50f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Requiescat Bar Color")]
        public PluginConfigColor RequiescatColor = new PluginConfigColor(new Vector4(61f / 255f, 61f / 255f, 255f / 255f, 100f / 100f));

        [ColorEdit4("Atonement Bar Color")]
        public PluginConfigColor AtonementColor = new PluginConfigColor(new Vector4(240f / 255f, 176f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Goring Blade Color")]
        public PluginConfigColor GoringBladeColor = new PluginConfigColor(new Vector4(255f / 255f, 128f / 255f, 0f / 255f, 100f / 100f));
    }
}
