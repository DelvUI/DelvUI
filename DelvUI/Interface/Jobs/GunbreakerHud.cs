﻿using Dalamud.Game.ClientState.Structs.JobGauge;
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

namespace DelvUI.Interface.Jobs
{
    public class GunbreakerHud : JobHud
    {
        private new GunbreakerConfig Config => (GunbreakerConfig)_config;
        private PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;

        public GunbreakerHud(string id, GunbreakerConfig config, string displayName = null) : base(id, config, displayName)
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

        public override void DrawChildren(Vector2 origin)
        {
            if (Config.ShowPowderGauge)
            {
                DrawPowderGauge(origin);
            }

            if (Config.ShowNoMercyBar)
            {
                DrawNoMercyBar(origin);
            }
        }

        private void DrawPowderGauge(Vector2 origin)
        {
            Vector2 position = origin + Config.Position + Config.PowderGaugeBarPosition - Config.PowderGaugeBarSize / 2f;
            var builder = BarBuilder.Create(position, Config.PowderGaugeBarSize);

            var gauge = Plugin.JobGauges.Get<GNBGauge>();

            builder.SetChunks(2)
                   .SetChunkPadding(Config.PowderGaugeSpacing)
                   .AddInnerBar(gauge.NumAmmo, 2, Config.PowderGaugeFillColor, null)
                   .SetBackgroundColor(EmptyColor.Base);

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }


        private void DrawNoMercyBar(Vector2 origin)
        {
            Vector2 position = origin + Config.Position + Config.NoMercyBarPosition - Config.NoMercyBarSize / 2f;
            var noMercyBuff = Plugin.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1831);

            var builder = BarBuilder.Create(position, Config.NoMercyBarSize)
                .SetBackgroundColor(EmptyColor.Base);

            if (noMercyBuff.Any())
            {
                var duration = noMercyBuff.First().Duration;

                builder.AddInnerBar(duration, 20, Config.NoMercyFillColor, null)
                       .SetTextMode(BarTextMode.EachChunk)
                       .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

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
        [CollapseControl(30, 0)]
        public bool ShowPowderGauge = true;

        [DragFloat2("Position" + "##PowderGauge", min = -4000f, max = 4000f)]
        [CollapseWith(0, 0)]
        public Vector2 PowderGaugeBarPosition = new(0, -32);

        [DragFloat2("Size" + "##PowderGauge", min = 1f, max = 4000f)]
        [CollapseWith(5, 0)]
        public Vector2 PowderGaugeBarSize = new(254, 20);

        [DragFloat("Spacing" + "##PowderGauge", min = 0)]
        [CollapseWith(10, 0)]
        public float PowderGaugeSpacing = 2.0f;

        [ColorEdit4("Color" + "##PowderGauge")]
        [CollapseWith(15, 0)]
        public PluginConfigColor PowderGaugeFillColor = new(new Vector4(0f / 255f, 162f / 255f, 252f / 255f, 1f));
        #endregion

        #region No Mercy
        [Checkbox("No Mercy", separator = true)]
        [CollapseControl(35, 1)]
        public bool ShowNoMercyBar = true;

        [DragFloat2("Position" + "##NoMercy", min = -4000f, max = 4000f)]
        [CollapseWith(0, 1)]
        public Vector2 NoMercyBarPosition = new(0, -10);

        [DragFloat2("Size" + "##NoMercy", min = 1f, max = 4000f)]
        [CollapseWith(5, 1)]
        public Vector2 NoMercyBarSize = new(254, 20);

        [ColorEdit4("Color" + "##NoMercy")]
        [CollapseWith(10, 1)]
        public PluginConfigColor NoMercyFillColor = new(new Vector4(252f / 255f, 204f / 255f, 255f / 255f, 1f));
        #endregion
    }
}
