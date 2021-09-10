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

namespace DelvUI.Interface.Jobs
{
    public class WarriorHud : JobHud
    {
        private new WarriorConfig Config => (WarriorConfig)_config;
        private Dictionary<string, uint> EmptyColor => GlobalColors.Instance.EmptyColor.Map;

        public WarriorHud(string id, WarriorConfig config) : base(id, config)
        {

        }

        public override void Draw(Vector2 origin)
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
            IEnumerable<StatusEffect> innerReleaseBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1177);
            IEnumerable<StatusEffect> stormsEyeBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 90);

            Vector2 position = origin + Config.Position + Config.StormsEyePosition - Config.StormsEyeSize / 2f;

            BarBuilder builder = BarBuilder.Create(position, Config.StormsEyeSize).SetBackgroundColor(EmptyColor["background"]);

            var duration = 0f;
            var maximum = 10f;
            Dictionary<string, uint> color = EmptyColor;

            if (innerReleaseBuff.Any())
            {
                duration = Math.Abs(innerReleaseBuff.First().Duration);
                color = Config.InnerReleaseColor.Map;
            }
            else if (stormsEyeBuff.Any())
            {
                duration = Math.Abs(stormsEyeBuff.First().Duration);
                maximum = 60f;
                color = Config.StormsEyeColor.Map;
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
            WARGauge gauge = PluginInterface.ClientState.JobGauges.Get<WARGauge>();
            IEnumerable<StatusEffect> nascentChaosBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1897);

            Vector2 position = origin + Config.Position + Config.BeastGaugePosition - Config.BeastGaugeSize / 2f;

            BarBuilder builder = BarBuilder.Create(position, Config.BeastGaugeSize)
                                           .SetChunks(2)
                                           .AddInnerBar(gauge.BeastGaugeAmount, 100, Config.BeastGaugeFillColor.Map)
                                           .SetBackgroundColor(EmptyColor["background"])
                                           .SetChunkPadding(Config.BeastGaugePadding);

            if (nascentChaosBuff.Any())
            {
                builder.SetChunksColors(Config.NascentChaosColor.Map);
            }

            if (Config.ShowBeastGaugeText)
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
    [SubSection("Warrior", 1)]
    public class WarriorConfig : JobConfig
    {
        [JsonIgnore] public new uint JobId = JobIDs.WAR;
        public new static WarriorConfig DefaultConfig() { return new WarriorConfig(); }

        #region Storm's Eye
        [Checkbox("Show Storm's Eye")]
        [CollapseControl(30, 0)]
        public bool ShowStormsEye = true;

        [Checkbox("Show Text" + "##StormsEye")]
        [CollapseWith(0, 0)]
        public bool ShowStormsEyeText = true;

        [DragFloat2("Position" + "##StormsEye", min = -4000f, max = 4000f)]
        [CollapseWith(5, 0)]
        public Vector2 StormsEyePosition = new(0, HUDConstants.JobHudsBaseY - 32);

        [DragFloat2("Size" + "##StormsEye", min = 1f, max = 4000f)]
        [CollapseWith(10, 0)]
        public Vector2 StormsEyeSize = new(254, 20);

        [ColorEdit4("Inner Release Color")]
        [CollapseWith(15, 0)]
        public PluginConfigColor InnerReleaseColor = new(new Vector4(255f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Storm's Eye Color")]
        [CollapseWith(20, 0)]
        public PluginConfigColor StormsEyeColor = new(new Vector4(255f / 255f, 136f / 255f, 146f / 255f, 100f / 100f));
        #endregion

        #region Beast Gauge
        [Checkbox("Show Beast Gauge")]
        [CollapseControl(35, 1)]
        public bool ShowBeastGauge = true;

        [Checkbox("Show Text" + "##BeastGauge")]
        [CollapseWith(0, 1)]
        public bool ShowBeastGaugeText = false;

        [DragFloat2("Position" + "##BeastGauge", min = -4000f, max = 4000f)]
        [CollapseWith(5, 1)]
        public Vector2 BeastGaugePosition = new(0, HUDConstants.JobHudsBaseY - 10);

        [DragFloat2("Size" + "##BeastGauge", min = 1f, max = 4000f)]
        [CollapseWith(10, 1)]
        public Vector2 BeastGaugeSize = new(254, 20);

        [DragFloat("Spacing" + "##BeastGauge")]
        [CollapseWith(15, 1)]
        public float BeastGaugePadding = 2.0f;

        [ColorEdit4("Beast Gauge Color")]
        [CollapseWith(20, 1)]
        public PluginConfigColor BeastGaugeFillColor = new(new Vector4(201f / 255f, 13f / 255f, 13f / 255f, 100f / 100f));

        [ColorEdit4("Nascent Chaos Color")]
        [CollapseWith(25, 1)]
        public PluginConfigColor NascentChaosColor = new(new Vector4(240f / 255f, 176f / 255f, 0f / 255f, 100f / 100f));
        #endregion
    }
}
