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
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using ImGuiNET;
using Actor = Dalamud.Game.ClientState.Actors.Types.Actor;

namespace DelvUI.Interface
{
    public class PaladinHudWindow : HudWindow
    {
        public override uint JobId => Jobs.PLD;

        private PaladinHudConfig _config => (PaladinHudConfig)ConfigurationManager.GetInstance().GetConfiguration(new PaladinHudConfig());
        private float OriginX => CenterX + _config.Position.X;
        private float OriginY => CenterY + _config.Position.Y;

        private Dictionary<string, uint> EmptyColor => PluginConfiguration.MiscColorMap["empty"];
        private Dictionary<string, uint> OathGaugeNotFilledColor => PluginConfiguration.MiscColorMap["partial"];

        public PaladinHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

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
            PlayerCharacter actor = PluginInterface.ClientState.LocalPlayer;

            float posX = OriginX + _config.ManaBarPosition.X - _config.ManaBarSize.X / 2f;
            float posY = OriginY + _config.ManaBarPosition.Y - _config.ManaBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(posX, posY, _config.ManaBarSize.Y, _config.ManaBarSize.X).SetBackgroundColor(EmptyColor["background"]);

            if (_config.ChunkManaBar)
            {
                builder.SetChunks(5).SetChunkPadding(_config.ManaBarPadding).AddInnerBar(actor.CurrentMp, actor.MaxMp, _config.ManaBarColor.Map, EmptyColor);
            }
            else
            {
                builder.AddInnerBar(actor.CurrentMp, actor.MaxMp, _config.ManaBarColor.Map);
            }

            if (_config.ShowManaBarText)
            {
                string formattedManaText = TextTags.GenerateFormattedTextFromTags(actor, "[mana:current-short]");

                builder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterLeft, BarTextType.Custom, formattedManaText);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawOathGauge()
        {
            PLDGauge gauge = PluginInterface.ClientState.JobGauges.Get<PLDGauge>();

            float xPos = OriginX + _config.OathGaugePosition.X - _config.OathGaugeSize.X / 2f;
            float yPos = OriginY + _config.OathGaugePosition.Y - _config.OathGaugeSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, _config.OathGaugeSize.Y, _config.OathGaugeSize.X)
                                           .SetChunks(2)
                                           .SetChunkPadding(_config.OathGaugePadding)
                                           .SetBackgroundColor(EmptyColor["background"])
                                           .AddInnerBar(gauge.GaugeAmount, 100, _config.OathGaugeColor.Map, OathGaugeNotFilledColor);

            if (_config.ShowOathGaugeText)
            {
                builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawBuffBar()
        {
            IEnumerable<StatusEffect> fightOrFlightBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 76);
            IEnumerable<StatusEffect> requiescatBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1368);

            float xPos = OriginX + _config.BuffBarPosition.X - _config.BuffBarSize.X / 2f;
            float yPos = OriginY + _config.BuffBarPosition.Y - _config.BuffBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, _config.BuffBarSize.Y, _config.BuffBarSize.X).SetBackgroundColor(EmptyColor["background"]);

            if (fightOrFlightBuff.Any())
            {
                float fightOrFlightDuration = Math.Abs(fightOrFlightBuff.First().Duration);
                builder.AddInnerBar(fightOrFlightDuration, 25, _config.FightOrFlightColor.Map);

                if (_config.ShowBuffBarText)
                {
                    builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterLeft, BarTextType.Current, _config.FightOrFlightColor.Vector, Vector4.UnitW, null);
                }
            }

            if (requiescatBuff.Any())
            {
                float requiescatDuration = Math.Abs(requiescatBuff.First().Duration);
                builder.AddInnerBar(requiescatDuration, 12, _config.RequiescatColor.Map);

                if (_config.ShowBuffBarText)
                {
                    builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterRight, BarTextType.Current, _config.RequiescatColor.Vector, Vector4.UnitW, null);
                }
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawAtonementBar()
        {
            IEnumerable<StatusEffect> atonementBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1902);
            int stackCount = atonementBuff.Any() ? atonementBuff.First().StackCount : 0;

            float xPos = OriginX + _config.AtonementBarPosition.X - _config.AtonementBarSize.X / 2f;
            float yPos = OriginY + _config.AtonementBarPosition.Y - _config.AtonementBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, _config.AtonementBarSize.Y, _config.AtonementBarSize.X)
                                           .SetChunks(3)
                                           .SetChunkPadding(_config.AtonementBarPadding)
                                           .SetBackgroundColor(EmptyColor["background"])
                                           .AddInnerBar(stackCount, 3, _config.AtonementColor.Map, null);

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawDoTBar()
        {
            Actor target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;

            if (target is not Chara)
            {
                return;
            }

            StatusEffect goringBlade = target.StatusEffects.FirstOrDefault(o => o.EffectId == 725 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId);

            float duration = Math.Abs(goringBlade.Duration);

            float xPos = OriginX + _config.GoringBladeBarPosition.X - _config.GoringBladeBarSize.X / 2f;
            float yPos = OriginY + _config.GoringBladeBarPosition.Y - _config.GoringBladeBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, _config.GoringBladeBarSize.Y, _config.GoringBladeBarSize.X)
                                           .AddInnerBar(duration, 21, _config.GoringBladeColor.Map)
                                           .SetBackgroundColor(EmptyColor["background"]);

            if (_config.ShowGoringBladeBarText)
            {
                builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Tank", 0)]
    [SubSection("Paladin", 1)]
    public class PaladinHudConfig : PluginConfigObject
    {
        [DragFloat2("Base Position", min = -4000f, max = 4000f)]
        [Order(0)]
        public Vector2 Position = new(0, 0);

        [Checkbox("Show Mana Bar")]
        [CollapseControl(5, 0)]
        public bool ShowManaBar = true;

        [Checkbox("Show Mana Bar Text")]
        [CollapseWith(0, 0)]
        public bool ShowManaBarText = true;

        [Checkbox("Chunk Mana Bar")]
        [CollapseWith(5, 0)]
        public bool ChunkManaBar = true;

        [DragFloat2("Mana Bar Size", max = 2000f)]
        [CollapseWith(10, 0)]
        public Vector2 ManaBarSize = new(254, 20);

        [DragInt("Mana Bar Padding", max = 100)]
        [CollapseWith(15, 0)]
        public int ManaBarPadding = 2;

        [DragFloat2("Mana Bar Position", min = -4000f, max = 4000f)]
        [CollapseWith(20, 0)]
        public Vector2 ManaBarPosition = new(0, 383);

        [ColorEdit4("Mana Bar Color")]
        [CollapseWith(25, 0)]
        public PluginConfigColor ManaBarColor = new(new Vector4(0f / 255f, 203f / 255f, 230f / 255f, 100f / 100f));

        [Checkbox("Show Oath Gauge")]
        [CollapseControl(10, 1)]
        public bool ShowOathGauge = true;

        [Checkbox("Show Oath Gauge Text")]
        [CollapseWith(0, 1)]
        public bool ShowOathGaugeText = true;

        [DragFloat2("Oath Gauge Size", min = -4000f, max = 4000f)]
        [CollapseWith(5, 1)]
        public Vector2 OathGaugeSize = new(254, 20);

        [DragInt("Oath Gauge Padding", max = 100)]
        [CollapseWith(10, 1)]
        public int OathGaugePadding = 2;

        [DragFloat2("Oath Gauge Position", min = -4000f, max = 4000f)]
        [CollapseWith(15, 1)]
        public Vector2 OathGaugePosition = new(0, 405);

        [ColorEdit4("Oath Gauge Color")]
        [CollapseWith(20, 1)]
        public PluginConfigColor OathGaugeColor = new(new Vector4(24f / 255f, 80f / 255f, 175f / 255f, 100f / 100f));

        [Checkbox("Show Buff Bar")]
        [CollapseControl(15, 2)]
        public bool ShowBuffBar = true;

        [Checkbox("Show Buff Bar Text")]
        [CollapseWith(0, 2)]
        public bool ShowBuffBarText = true;

        [DragFloat2("Buff Bar Size", min = -4000f, max = 4000f)]
        [CollapseWith(5, 2)]
        public Vector2 BuffBarSize = new(254, 20);

        [DragFloat2("Buff Bar Position", min = -4000f, max = 4000f)]
        [CollapseWith(10, 2)]
        public Vector2 BuffBarPosition = new(0, 427);

        [ColorEdit4("Fight or Flight Bar Color")]
        [CollapseWith(15, 2)]
        public PluginConfigColor FightOrFlightColor = new(new Vector4(240f / 255f, 50f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Requiescat Bar Color")]
        [CollapseWith(20, 2)]
        public PluginConfigColor RequiescatColor = new(new Vector4(61f / 255f, 61f / 255f, 255f / 255f, 100f / 100f));

        [Checkbox("Show Atonement Bar")]
        [CollapseControl(20, 3)]
        public bool ShowAtonementBar = true;

        [DragFloat2("Atonement Bar Size", min = -4000f, max = 4000f)]
        [CollapseWith(0, 3)]
        public Vector2 AtonementBarSize = new(254, 20);

        [DragInt("Atonement Bar Padding", max = 100)]
        [CollapseWith(5, 3)]
        public int AtonementBarPadding = 2;

        [DragFloat2("Atonement Bar Position", min = -4000f, max = 4000f)]
        [CollapseWith(10, 3)]
        public Vector2 AtonementBarPosition = new(0, 449);

        [ColorEdit4("Atonement Bar Color")]
        [CollapseWith(15, 3)]
        public PluginConfigColor AtonementColor = new(new Vector4(240f / 255f, 176f / 255f, 0f / 255f, 100f / 100f));

        [Checkbox("Show Goring Blade Bar")]
        [CollapseControl(25, 4)]
        public bool ShowGoringBladeBar = true;

        [Checkbox("Show Goring Blade Bar Text")]
        [CollapseWith(0, 4)]
        public bool ShowGoringBladeBarText = true;

        [DragFloat2("Goring Blade Bar Size", min = -4000f, max = 4000f)]
        [CollapseWith(5, 4)]
        public Vector2 GoringBladeBarSize = new(254, 20);

        [DragFloat2("Goring Blade Bar Position", min = -4000f, max = 4000f)]
        [CollapseWith(10, 4)]
        public Vector2 GoringBladeBarPosition = new(0, 361);

        [ColorEdit4("Goring Blade Color")]
        [CollapseWith(15, 4)]
        public PluginConfigColor GoringBladeColor = new(new Vector4(255f / 255f, 128f / 255f, 0f / 255f, 100f / 100f));
    }
}
