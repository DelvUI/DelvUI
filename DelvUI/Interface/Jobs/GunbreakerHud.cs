﻿using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.Jobs
{
    public class GunbreakerHud : JobHud
    {
        private new GunbreakerConfig Config => (GunbreakerConfig)_config;
        private PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;

        public GunbreakerHud(string id, GunbreakerConfig config, string? displayName = null) : base(id, config, displayName)
        {

        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.ShowPowderGauge)
            {
                positions.Add(Config.Position + Config.PowderGaugeBarPosition);
                sizes.Add(Config.PowderGaugeBarSize);
            }

            if (Config.ShowNoMercyBar)
            {
                positions.Add(Config.Position + Config.NoMercyBarPosition);
                sizes.Add(Config.NoMercyBarSize);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            if (Config.ShowPowderGauge)
            {
                DrawPowderGauge(origin);
            }

            if (Config.ShowNoMercyBar)
            {
                DrawNoMercyBar(origin, player);
            }
        }

        private void DrawPowderGauge(Vector2 origin)
        {
            Vector2 position = origin + Config.Position + Config.PowderGaugeBarPosition - Config.PowderGaugeBarSize / 2f;
            var builder = BarBuilder.Create(position, Config.PowderGaugeBarSize);

            var gauge = Plugin.JobGauges.Get<GNBGauge>();

            if (Config.OnlyShowPowderGaugeWhenActive && gauge.Ammo is 0) { return; }

            builder.SetChunks(2)
                   .SetChunkPadding(Config.PowderGaugeSpacing)
                   .AddInnerBar(gauge.Ammo, 2, Config.PowderGaugeFillColor, null)
                   .SetBackgroundColor(EmptyColor.Base);

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }


        private void DrawNoMercyBar(Vector2 origin, PlayerCharacter player)
        {
            Vector2 position = origin + Config.Position + Config.NoMercyBarPosition - Config.NoMercyBarSize / 2f;
            var noMercyBuff = player.StatusList.Where(o => o.StatusId == 1831);
            float duration = 0f;

            var builder = BarBuilder.Create(position, Config.NoMercyBarSize)
                .SetBackgroundColor(EmptyColor.Base);

            if (noMercyBuff.Any())
            {
                duration = noMercyBuff.First().RemainingTime;

                builder.AddInnerBar(duration, 20, Config.NoMercyFillColor, null)
                       .SetTextMode(BarTextMode.EachChunk)
                       .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }
            if (Config.OnlyShowNoMercyWhenActive && duration is 0) { return; }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Tank", 0)]
    [SubSection("Gunbreaker", 1)]
    public class GunbreakerConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.GNB;
        public new static GunbreakerConfig DefaultConfig() { return new GunbreakerConfig(); }

        #region Powder Gauge
        [Checkbox("Powder Gauge", separator = true)]
        [Order(30)]
        public bool ShowPowderGauge = true;

        [Checkbox("Only Show When Active" + "##PowderGauge")]
        [Order(35, collapseWith = nameof(ShowPowderGauge))]
        public bool OnlyShowPowderGaugeWhenActive = false;

        [DragFloat2("Position" + "##PowderGauge", min = -4000f, max = 4000f)]
        [Order(40, collapseWith = nameof(ShowPowderGauge))]
        public Vector2 PowderGaugeBarPosition = new(0, -32);

        [DragFloat2("Size" + "##PowderGauge", min = 1f, max = 4000f)]
        [Order(45, collapseWith = nameof(ShowPowderGauge))]
        public Vector2 PowderGaugeBarSize = new(254, 20);

        [DragFloat("Spacing" + "##PowderGauge", min = 0)]
        [Order(50, collapseWith = nameof(ShowPowderGauge))]
        public float PowderGaugeSpacing = 2.0f;

        [ColorEdit4("Color" + "##PowderGauge")]
        [Order(55, collapseWith = nameof(ShowPowderGauge))]
        public PluginConfigColor PowderGaugeFillColor = new(new Vector4(0f / 255f, 162f / 255f, 252f / 255f, 1f));
        #endregion

        #region No Mercy
        [Checkbox("No Mercy", separator = true)]
        [Order(60)]
        public bool ShowNoMercyBar = true;

        [Checkbox("Only Show When Active" + "##NoMercy")]
        [Order(65, collapseWith = nameof(ShowNoMercyBar))]
        public bool OnlyShowNoMercyWhenActive = false;

        [DragFloat2("Position" + "##NoMercy", min = -4000f, max = 4000f)]
        [Order(70, collapseWith = nameof(ShowNoMercyBar))]
        public Vector2 NoMercyBarPosition = new(0, -10);

        [DragFloat2("Size" + "##NoMercy", min = 1f, max = 4000f)]
        [Order(75, collapseWith = nameof(ShowNoMercyBar))]
        public Vector2 NoMercyBarSize = new(254, 20);

        [ColorEdit4("Color" + "##NoMercy")]
        [Order(80, collapseWith = nameof(ShowNoMercyBar))]
        public PluginConfigColor NoMercyFillColor = new(new Vector4(252f / 255f, 204f / 255f, 255f / 255f, 1f));
        #endregion
    }
}
