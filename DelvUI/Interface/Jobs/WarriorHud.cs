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
using Dalamud.Game.ClientState.Statuses;

namespace DelvUI.Interface.Jobs
{
    public class WarriorHud : JobHud
    {
        private new WarriorConfig Config => (WarriorConfig)_config;
        private PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;

        public WarriorHud(string id, WarriorConfig config, string? displayName = null) : base(id, config, displayName)
        {

        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new();
            List<Vector2> sizes = new();

            if (Config.ShowStormsEye)
            {
                positions.Add(Config.Position + Config.StormsEyePosition);
                sizes.Add(Config.StormsEyeSize);
            }

            if (Config.ShowBeastGauge)
            {
                positions.Add(Config.Position + Config.BeastGaugePosition);
                sizes.Add(Config.BeastGaugeSize);
            }

            return (positions, sizes);
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (Config.ShowStormsEye)
            {
                DrawStormsEyeBar(origin);
            }

            if (Config.ShowBeastGauge)
            {
                DrawBeastGauge(origin);
            }
        }

        private void DrawStormsEyeBar(Vector2 origin)
        {
            Debug.Assert(Plugin.ClientState.LocalPlayer != null, "Plugin.ClientState.LocalPlayer != null");
            IEnumerable<Status>? innerReleaseBuff = Plugin.ClientState.LocalPlayer.StatusList.Where(o => o.StatusId is 1177 or 86);
            IEnumerable<Status>? stormsEyeBuff = Plugin.ClientState.LocalPlayer.StatusList.Where(o => o.StatusId == 90);

            Vector2 position = origin + Config.Position + Config.StormsEyePosition - Config.StormsEyeSize / 2f;

            BarBuilder builder = BarBuilder.Create(position, Config.StormsEyeSize).SetBackgroundColor(EmptyColor.Base);

            var duration = 0f;
            var maximum = 10f;
            PluginConfigColor color = EmptyColor;

            if (innerReleaseBuff.Any())
            {
                duration = Math.Abs(innerReleaseBuff.First().RemainingTime);
                color = Config.InnerReleaseColor;
            }
            else if (stormsEyeBuff.Any())
            {
                duration = Math.Abs(stormsEyeBuff.First().RemainingTime);
                maximum = 60f;
                color = Config.StormsEyeColor;
            }

            builder.AddInnerBar(duration, maximum, color);

            if (Config.ShowStormsEyeText)
            {
                builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawBeastGauge(Vector2 origin)
        {
            Debug.Assert(Plugin.ClientState.LocalPlayer != null, "Plugin.ClientState.LocalPlayer != null");
            WARGauge gauge = Plugin.JobGauges.Get<WARGauge>();
            var nascentChaosBuff = Plugin.ClientState.LocalPlayer.StatusList.Where(o => o.StatusId == 1897);

            Vector2 position = origin + Config.Position + Config.BeastGaugePosition - Config.BeastGaugeSize / 2f;

            BarBuilder builder = BarBuilder.Create(position, Config.BeastGaugeSize)
                                           .SetChunks(2)
                                           .AddInnerBar(gauge.BeastGauge, 100, Config.BeastGaugeFillColor)
                                           .SetBackgroundColor(EmptyColor.Base)
                                           .SetChunkPadding(Config.BeastGaugePadding);

            if (nascentChaosBuff.Any())
            {
                builder.SetChunksColors(Config.NascentChaosColor);
            }

            if (Config.ShowBeastGaugeText)
            {
                builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Tank", 0)]
    [SubSection("Warrior", 1)]
    public class WarriorConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.WAR;
        public new static WarriorConfig DefaultConfig() { return new WarriorConfig(); }

        #region Storm's Eye
        [Checkbox("Storm's Eye", separator = true)]
        [CollapseControl(30, 0)]
        public bool ShowStormsEye = true;

        [Checkbox("Text" + "##StormsEye")]
        [CollapseWith(0, 0)]
        public bool ShowStormsEyeText = true;

        [DragFloat2("Position" + "##StormsEye", min = -4000f, max = 4000f)]
        [CollapseWith(5, 0)]
        public Vector2 StormsEyePosition = new(0, -32);

        [DragFloat2("Size" + "##StormsEye", min = 1f, max = 4000f)]
        [CollapseWith(10, 0)]
        public Vector2 StormsEyeSize = new(254, 20);
        
        [ColorEdit4("Storm's Eye")]
        [CollapseWith(15, 0)]
        public PluginConfigColor StormsEyeColor = new(new Vector4(255f / 255f, 136f / 255f, 146f / 255f, 100f / 100f));
        
        [ColorEdit4("Inner Release")]
        [CollapseWith(20, 0)]
        public PluginConfigColor InnerReleaseColor = new(new Vector4(255f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));


        #endregion

        #region Beast Gauge
        [Checkbox("Beast Gauge",separator = true)]
        [CollapseControl(35, 1)]
        public bool ShowBeastGauge = true;

        [Checkbox("Text" + "##BeastGauge")]
        [CollapseWith(0, 1)]
        public bool ShowBeastGaugeText = false;

        [DragFloat2("Position" + "##BeastGauge", min = -4000f, max = 4000f)]
        [CollapseWith(5, 1)]
        public Vector2 BeastGaugePosition = new(0, -10);

        [DragFloat2("Size" + "##BeastGauge", min = 1f, max = 4000f)]
        [CollapseWith(10, 1)]
        public Vector2 BeastGaugeSize = new(254, 20);

        [DragFloat("Spacing" + "##BeastGauge")]
        [CollapseWith(15, 1)]
        public float BeastGaugePadding = 2.0f;

        [ColorEdit4("Beast Gauge")]
        [CollapseWith(20, 1)]
        public PluginConfigColor BeastGaugeFillColor = new(new Vector4(201f / 255f, 13f / 255f, 13f / 255f, 100f / 100f));

        [ColorEdit4("Nascent Chaos")]
        [CollapseWith(25, 1)]
        public PluginConfigColor NascentChaosColor = new(new Vector4(240f / 255f, 176f / 255f, 0f / 255f, 100f / 100f));
        #endregion
    }
}
