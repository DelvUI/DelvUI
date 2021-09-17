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
    public class PaladinHud : JobHud
    {
        private new PaladinConfig Config => (PaladinConfig)_config;
        private Dictionary<string, uint> EmptyColor => GlobalColors.Instance.EmptyColor.Map;
        private Dictionary<string, uint> PartialFillColor => GlobalColors.Instance.PartialFillColor.Map;

        public PaladinHud(string id, PaladinConfig config, string displayName = null) : base(id, config, displayName)
        {

        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.ShowManaBar)
            {
                positions.Add(Config.Position + Config.ManaBarPosition);
                sizes.Add(Config.ManaBarSize);
            }

            if (Config.ShowOathGauge)
            {
                positions.Add(Config.Position + Config.OathGaugePosition);
                sizes.Add(Config.OathGaugeSize);
            }

            if (Config.ShowBuffBar)
            {
                positions.Add(Config.Position + Config.BuffBarPosition);
                sizes.Add(Config.BuffBarSize);
            }

            if (Config.ShowAtonementBar)
            {
                positions.Add(Config.Position + Config.AtonementBarPosition);
                sizes.Add(Config.AtonementBarSize);
            }

            return (positions, sizes);
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (Config.ShowManaBar)
            {
                DrawManaBar(origin);
            }

            if (Config.ShowOathGauge)
            {
                DrawOathGauge(origin);
            }

            if (Config.ShowBuffBar)
            {
                DrawBuffBar(origin);
            }

            if (Config.ShowAtonementBar)
            {
                DrawAtonementBar(origin);
            }

            if (Config.ShowGoringBladeBar)
            {
                DrawDoTBar(origin);
            }
        }

        private void DrawManaBar(Vector2 origin)
        {
            PlayerCharacter actor = Plugin.ClientState.LocalPlayer;

            float posX = origin.X + Config.Position.X + Config.ManaBarPosition.X - Config.ManaBarSize.X / 2f;
            float posY = origin.Y + Config.Position.Y + Config.ManaBarPosition.Y - Config.ManaBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(posX, posY, Config.ManaBarSize.Y, Config.ManaBarSize.X).SetBackgroundColor(EmptyColor["background"]);

            if (Config.ChunkManaBar)
            {
                builder.SetChunks(5).SetChunkPadding(Config.ManaBarPadding).AddInnerBar(actor.CurrentMp, actor.MaxMp, Config.ManaBarColor.Map, EmptyColor);
            }
            else
            {
                builder.AddInnerBar(actor.CurrentMp, actor.MaxMp, Config.ManaBarColor.Map);
            }

            if (Config.ShowManaBarText)
            {
                string formattedManaText = TextTags.GenerateFormattedTextFromTags(actor, "[mana:current-short]");

                builder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterLeft, BarTextType.Custom, formattedManaText);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawOathGauge(Vector2 origin)
        {
            PLDGauge gauge = Plugin.JobGauges.Get<PLDGauge>();

            float xPos = origin.X + Config.Position.X + Config.OathGaugePosition.X - Config.OathGaugeSize.X / 2f;
            float yPos = origin.Y + Config.Position.Y + Config.OathGaugePosition.Y - Config.OathGaugeSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.OathGaugeSize.Y, Config.OathGaugeSize.X)
                                           .SetChunks(2)
                                           .SetChunkPadding(Config.OathGaugePadding)
                                           .SetBackgroundColor(EmptyColor["background"])
                                           .AddInnerBar(gauge.GaugeAmount, 100, Config.OathGaugeColor.Map, PartialFillColor);

            if (Config.ShowOathGaugeText)
            {
                builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawBuffBar(Vector2 origin)
        {
            IEnumerable<StatusEffect> fightOrFlightBuff = Plugin.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 76);
            IEnumerable<StatusEffect> requiescatBuff = Plugin.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1368);

            float xPos = origin.X + Config.Position.X + Config.BuffBarPosition.X - Config.BuffBarSize.X / 2f;
            float yPos = origin.Y + Config.Position.Y + Config.BuffBarPosition.Y - Config.BuffBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.BuffBarSize.Y, Config.BuffBarSize.X).SetBackgroundColor(EmptyColor["background"]);

            if (fightOrFlightBuff.Any())
            {
                float fightOrFlightDuration = Math.Abs(fightOrFlightBuff.First().Duration);
                builder.AddInnerBar(fightOrFlightDuration, 25, Config.FightOrFlightColor.Map);

                if (Config.ShowBuffBarText)
                {
                    builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterLeft, BarTextType.Current, Config.FightOrFlightColor.Vector, Vector4.UnitW, null);
                }
            }

            if (requiescatBuff.Any())
            {
                float requiescatDuration = Math.Abs(requiescatBuff.First().Duration);
                builder.AddInnerBar(requiescatDuration, 12, Config.RequiescatColor.Map);

                if (Config.ShowBuffBarText)
                {
                    builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterRight, BarTextType.Current, Config.RequiescatColor.Vector, Vector4.UnitW, null);
                }
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawAtonementBar(Vector2 origin)
        {
            IEnumerable<StatusEffect> atonementBuff = Plugin.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1902);
            int stackCount = atonementBuff.Any() ? atonementBuff.First().StackCount : 0;

            float xPos = origin.X + Config.Position.X + Config.AtonementBarPosition.X - Config.AtonementBarSize.X / 2f;
            float yPos = origin.Y + Config.Position.Y + Config.AtonementBarPosition.Y - Config.AtonementBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.AtonementBarSize.Y, Config.AtonementBarSize.X)
                                           .SetChunks(3)
                                           .SetChunkPadding(Config.AtonementBarPadding)
                                           .SetBackgroundColor(EmptyColor["background"])
                                           .AddInnerBar(stackCount, 3, Config.AtonementColor.Map, null);

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawDoTBar(Vector2 origin)
        {
            Actor target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.CurrentTarget;

            if (target is not Chara)
            {
                return;
            }

            StatusEffect goringBlade = target.StatusEffects.FirstOrDefault(o => o.EffectId == 725 && o.OwnerId == Plugin.ClientState.LocalPlayer.ActorId);

            float duration = Math.Abs(goringBlade.Duration);

            float xPos = origin.X + Config.Position.X + Config.GoringBladeBarPosition.X - Config.GoringBladeBarSize.X / 2f;
            float yPos = origin.Y + Config.Position.Y + Config.GoringBladeBarPosition.Y - Config.GoringBladeBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.GoringBladeBarSize.Y, Config.GoringBladeBarSize.X)
                                           .AddInnerBar(duration, 21, Config.GoringBladeColor.Map)
                                           .SetBackgroundColor(EmptyColor["background"]);

            if (Config.ShowGoringBladeBarText)
            {
                builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Tank", 0)]
    [SubSection("Paladin", 1)]
    public class PaladinConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.PLD;
        public new static PaladinConfig DefaultConfig() { return new PaladinConfig(); }

        #region mana bar
        [Checkbox("Show Mana Bar")]
        [CollapseControl(30, 0)]
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
        public Vector2 ManaBarPosition = new(0, -76);

        [ColorEdit4("Mana Bar Color")]
        [CollapseWith(25, 0)]
        public PluginConfigColor ManaBarColor = new(new Vector4(0f / 255f, 162f / 255f, 252f / 255f, 100f / 100f));
        #endregion

        #region oath gauge
        [Checkbox("Show Oath Gauge")]
        [CollapseControl(35, 1)]
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
        public Vector2 OathGaugePosition = new(0, -54);

        [ColorEdit4("Oath Gauge Color")]
        [CollapseWith(20, 1)]
        public PluginConfigColor OathGaugeColor = new(new Vector4(24f / 255f, 80f / 255f, 175f / 255f, 100f / 100f));
        #endregion

        #region buff
        [Checkbox("Show Buff Bar")]
        [CollapseControl(40, 2)]
        public bool ShowBuffBar = true;

        [Checkbox("Show Buff Bar Text")]
        [CollapseWith(0, 2)]
        public bool ShowBuffBarText = true;

        [DragFloat2("Buff Bar Size", min = -4000f, max = 4000f)]
        [CollapseWith(5, 2)]
        public Vector2 BuffBarSize = new(254, 20);

        [DragFloat2("Buff Bar Position", min = -4000f, max = 4000f)]
        [CollapseWith(10, 2)]
        public Vector2 BuffBarPosition = new(0, -32);

        [ColorEdit4("Fight or Flight Bar Color")]
        [CollapseWith(15, 2)]
        public PluginConfigColor FightOrFlightColor = new(new Vector4(240f / 255f, 50f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Requiescat Bar Color")]
        [CollapseWith(20, 2)]
        public PluginConfigColor RequiescatColor = new(new Vector4(61f / 255f, 61f / 255f, 255f / 255f, 100f / 100f));
        #endregion

        #region atonement
        [Checkbox("Show Atonement Bar")]
        [CollapseControl(45, 3)]
        public bool ShowAtonementBar = true;

        [DragFloat2("Atonement Bar Size", min = -4000f, max = 4000f)]
        [CollapseWith(0, 3)]
        public Vector2 AtonementBarSize = new(254, 20);

        [DragInt("Atonement Bar Padding", max = 100)]
        [CollapseWith(5, 3)]
        public int AtonementBarPadding = 2;

        [DragFloat2("Atonement Bar Position", min = -4000f, max = 4000f)]
        [CollapseWith(10, 3)]
        public Vector2 AtonementBarPosition = new(0, -10);

        [ColorEdit4("Atonement Bar Color")]
        [CollapseWith(15, 3)]
        public PluginConfigColor AtonementColor = new(new Vector4(240f / 255f, 176f / 255f, 0f / 255f, 100f / 100f));
        #endregion

        #region goring blade
        [Checkbox("Show Goring Blade Bar")]
        [CollapseControl(50, 4)]
        public bool ShowGoringBladeBar = true;

        [Checkbox("Show Goring Blade Bar Text")]
        [CollapseWith(0, 4)]
        public bool ShowGoringBladeBarText = true;

        [DragFloat2("Goring Blade Bar Size", min = -4000f, max = 4000f)]
        [CollapseWith(5, 4)]
        public Vector2 GoringBladeBarSize = new(254, 20);

        [DragFloat2("Goring Blade Bar Position", min = -4000f, max = 4000f)]
        [CollapseWith(10, 4)]
        public Vector2 GoringBladeBarPosition = new(0, -98);

        [ColorEdit4("Goring Blade Color")]
        [CollapseWith(15, 4)]
        public PluginConfigColor GoringBladeColor = new(new Vector4(255f / 255f, 128f / 255f, 0f / 255f, 100f / 100f));
        #endregion
    }
}
