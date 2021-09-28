using Dalamud.Game.ClientState.Structs;
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
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;

namespace DelvUI.Interface.Jobs
{
    public class PaladinHud : JobHud
    {
        private new PaladinConfig Config => (PaladinConfig)_config;
        private PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;
        private PluginConfigColor PartialFillColor => GlobalColors.Instance.PartialFillColor;

        public PaladinHud(string id, PaladinConfig config, string? displayName = null) : base(id, config, displayName)
        {

        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new();
            List<Vector2> sizes = new();

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

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            if (Config.ShowManaBar)
            {
                DrawManaBar(origin, player);
            }

            if (Config.ShowOathGauge)
            {
                DrawOathGauge(origin);
            }

            if (Config.ShowBuffBar)
            {
                DrawBuffBar(origin, player);
            }

            if (Config.ShowAtonementBar)
            {
                DrawAtonementBar(origin, player);
            }

            if (Config.ShowGoringBladeBar)
            {
                DrawDoTBar(origin, player);
            }
        }

        private void DrawManaBar(Vector2 origin, PlayerCharacter player)
        {
            float posX = origin.X + Config.Position.X + Config.ManaBarPosition.X - Config.ManaBarSize.X / 2f;
            float posY = origin.Y + Config.Position.Y + Config.ManaBarPosition.Y - Config.ManaBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(posX, posY, Config.ManaBarSize.Y, Config.ManaBarSize.X).SetBackgroundColor(EmptyColor.Base);

            if (Config.ChunkManaBar)
            {
                builder.SetChunks(5).SetChunkPadding(Config.ManaBarPadding).AddInnerBar(player.CurrentMp, player.MaxMp, Config.ManaBarColor, EmptyColor);
            }
            else
            {
                builder.AddInnerBar(player.CurrentMp, player.MaxMp, Config.ManaBarColor);
            }

            if (Config.ShowManaBarText)
            {
                string formattedManaText = TextTags.GenerateFormattedTextFromTags(player, "[mana:current-short]");

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
                                           .SetBackgroundColor(EmptyColor.Base)
                                           .AddInnerBar(gauge.OathGauge, 100, Config.OathGaugeColor, PartialFillColor);

            if (Config.ShowOathGaugeText)
            {
                builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawBuffBar(Vector2 origin, PlayerCharacter player)
        {
            IEnumerable<Status> fightOrFlightBuff = player.StatusList.Where(o => o.StatusId == 76);
            IEnumerable<Status> requiescatBuff = player.StatusList.Where(o => o.StatusId == 1368);

            float xPos = origin.X + Config.Position.X + Config.BuffBarPosition.X - Config.BuffBarSize.X / 2f;
            float yPos = origin.Y + Config.Position.Y + Config.BuffBarPosition.Y - Config.BuffBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.BuffBarSize.Y, Config.BuffBarSize.X).SetBackgroundColor(EmptyColor.Base);

            if (fightOrFlightBuff.Any())
            {
                float fightOrFlightDuration = Math.Abs(fightOrFlightBuff.First().RemainingTime);
                builder.AddInnerBar(fightOrFlightDuration, 25, Config.FightOrFlightColor);

                if (Config.ShowBuffBarText)
                {
                    builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterLeft, BarTextType.Current, Config.FightOrFlightColor.Vector, Vector4.UnitW, null);
                }
            }

            if (requiescatBuff.Any())
            {
                float requiescatDuration = Math.Abs(requiescatBuff.First().RemainingTime);
                builder.AddInnerBar(requiescatDuration, 12, Config.RequiescatColor);

                if (Config.ShowBuffBarText)
                {
                    builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterRight, BarTextType.Current, Config.RequiescatColor.Vector, Vector4.UnitW, null);
                }
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawAtonementBar(Vector2 origin, PlayerCharacter player)
        {
            IEnumerable<Status> atonementBuff = player.StatusList.Where(o => o.StatusId == 1902);
            int stackCount = atonementBuff.Any() ? atonementBuff.First().StackCount : 0;

            float xPos = origin.X + Config.Position.X + Config.AtonementBarPosition.X - Config.AtonementBarSize.X / 2f;
            float yPos = origin.Y + Config.Position.Y + Config.AtonementBarPosition.Y - Config.AtonementBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.AtonementBarSize.Y, Config.AtonementBarSize.X)
                                           .SetChunks(3)
                                           .SetChunkPadding(Config.AtonementBarPadding)
                                           .SetBackgroundColor(EmptyColor.Base)
                                           .AddInnerBar(stackCount, 3, Config.AtonementColor, null);

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawDoTBar(Vector2 origin, PlayerCharacter player)
        {
            GameObject? actor = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            if (actor is not BattleChara target)
            {
                return;
            }

            Status? goringBlade = target.StatusList.FirstOrDefault(o => o.StatusId == 725 && o.SourceID == player.ObjectId);

            float duration = Math.Abs(goringBlade?.RemainingTime ?? 0f);

            float xPos = origin.X + Config.Position.X + Config.GoringBladeBarPosition.X - Config.GoringBladeBarSize.X / 2f;
            float yPos = origin.Y + Config.Position.Y + Config.GoringBladeBarPosition.Y - Config.GoringBladeBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.GoringBladeBarSize.Y, Config.GoringBladeBarSize.X)
                                           .AddInnerBar(duration, 21, Config.GoringBladeColor)
                                           .SetBackgroundColor(EmptyColor.Base);

            if (Config.ShowGoringBladeBarText)
            {
                builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Tank", 0)]
    [SubSection("Paladin", 1)]
    public class PaladinConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.PLD;
        public new static PaladinConfig DefaultConfig() { return new PaladinConfig(); }

        #region mana bar
        [Checkbox("Mana", separator = true)]
        [Order(30)]
        public bool ShowManaBar = true;

        [Checkbox("Text" + "##MP")]
        [Order(35, collapseWith = nameof(ShowManaBar))]
        public bool ShowManaBarText = true;

        [Checkbox("Split Bar" + "##MP")]
        [Order(40, collapseWith = nameof(ShowManaBar))]
        public bool ChunkManaBar = true;

        [DragFloat2("Position" + "##MP", min = -4000f, max = 4000f)]
        [Order(45, collapseWith = nameof(ShowManaBar))]
        public Vector2 ManaBarPosition = new(0, -76);

        [DragFloat2("Size" + "##MP", max = 2000f)]
        [Order(50, collapseWith = nameof(ShowManaBar))]
        public Vector2 ManaBarSize = new(254, 20);

        [DragInt("Spacing" + "##MP", max = 100)]
        [Order(55, collapseWith = nameof(ShowManaBar))]
        public int ManaBarPadding = 2;

        [ColorEdit4("Color" + "##MP")]
        [Order(60, collapseWith = nameof(ShowManaBar))]
        public PluginConfigColor ManaBarColor = new(new Vector4(0f / 255f, 162f / 255f, 252f / 255f, 100f / 100f));
        #endregion

        #region oath gauge
        [Checkbox("Oath Gauge", separator = true)]
        [Order(65)]
        public bool ShowOathGauge = true;

        [Checkbox("Text" + "##Oath")]
        [Order(70, collapseWith = nameof(ShowOathGauge))]
        public bool ShowOathGaugeText = true;

        [DragFloat2("Position" + "##Oath", min = -4000f, max = 4000f)]
        [Order(75, collapseWith = nameof(ShowOathGauge))]
        public Vector2 OathGaugePosition = new(0, -54);

        [DragFloat2("Size" + "##Oath", min = -4000f, max = 4000f)]
        [Order(80, collapseWith = nameof(ShowOathGauge))]
        public Vector2 OathGaugeSize = new(254, 20);

        [DragInt("Spacing" + "##Oath", max = 100)]
        [Order(85, collapseWith = nameof(ShowOathGauge))]
        public int OathGaugePadding = 2;

        [ColorEdit4("Color" + "##Oath")]
        [Order(90, collapseWith = nameof(ShowOathGauge))]
        public PluginConfigColor OathGaugeColor = new(new Vector4(24f / 255f, 80f / 255f, 175f / 255f, 100f / 100f));
        #endregion

        #region buff
        [Checkbox("Fight or Flight & Requiescat", separator = true)]
        [Order(95)]
        public bool ShowBuffBar = true;

        [Checkbox("Timer" + "##Buff")]
        [Order(100, collapseWith = nameof(ShowBuffBar))]
        public bool ShowBuffBarText = true;

        [DragFloat2("Position" + "##Buff", min = -4000f, max = 4000f)]
        [Order(105, collapseWith = nameof(ShowBuffBar))]
        public Vector2 BuffBarPosition = new(0, -32);

        [DragFloat2("Size" + "##Buff", min = -4000f, max = 4000f)]
        [Order(110, collapseWith = nameof(ShowBuffBar))]
        public Vector2 BuffBarSize = new(254, 20);

        [ColorEdit4("Fight or Flight" + "##Buff")]
        [Order(115, collapseWith = nameof(ShowBuffBar))]
        public PluginConfigColor FightOrFlightColor = new(new Vector4(240f / 255f, 50f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Requiescat" + "##Buff")]
        [Order(120, collapseWith = nameof(ShowBuffBar))]
        public PluginConfigColor RequiescatColor = new(new Vector4(61f / 255f, 61f / 255f, 255f / 255f, 100f / 100f));
        #endregion

        #region atonement
        [Checkbox("Atonement" + "##Atonement", separator = true)]
        [Order(125)]
        public bool ShowAtonementBar = true;

        [DragFloat2("Position" + "##Atonement", min = -4000f, max = 4000f)]
        [Order(130, collapseWith = nameof(ShowAtonementBar))]
        public Vector2 AtonementBarPosition = new(0, -10);

        [DragFloat2("Size" + "##Atonement", min = -4000f, max = 4000f)]
        [Order(135, collapseWith = nameof(ShowAtonementBar))]
        public Vector2 AtonementBarSize = new(254, 20);

        [DragInt("Spacing" + "##Atonement", max = 100)]
        [Order(140, collapseWith = nameof(ShowAtonementBar))]
        public int AtonementBarPadding = 2;

        [ColorEdit4("Color" + "##Atonement")]
        [Order(145, collapseWith = nameof(ShowAtonementBar))]
        public PluginConfigColor AtonementColor = new(new Vector4(240f / 255f, 176f / 255f, 0f / 255f, 100f / 100f));
        #endregion

        #region goring blade
        [Checkbox("Goring Blade" + "##GoringBlade", separator = true)]
        [Order(150)]
        public bool ShowGoringBladeBar = true;

        [Checkbox("Timer" + "##GoringBlade")]
        [Order(155, collapseWith = nameof(ShowGoringBladeBar))]
        public bool ShowGoringBladeBarText = true;

        [DragFloat2("Position" + "##GoringBlade", min = -4000f, max = 4000f)]
        [Order(160, collapseWith = nameof(ShowGoringBladeBar))]
        public Vector2 GoringBladeBarPosition = new(0, -98);

        [DragFloat2("Size" + "##GoringBlade", min = -4000f, max = 4000f)]
        [Order(165, collapseWith = nameof(ShowGoringBladeBar))]
        public Vector2 GoringBladeBarSize = new(254, 20);

        [ColorEdit4("Color" + "##GoringBlade")]
        [Order(170, collapseWith = nameof(ShowGoringBladeBar))]
        public PluginConfigColor GoringBladeColor = new(new Vector4(255f / 255f, 128f / 255f, 0f / 255f, 100f / 100f));
        #endregion
    }
}
