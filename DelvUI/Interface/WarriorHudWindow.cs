using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Structs;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Interface.Bars;
using ImGuiNET;

namespace DelvUI.Interface
{
    public class WarriorHudWindow : HudWindow
    {
        public override uint JobId => Jobs.WAR;
        private WarriorHudConfig _config => (WarriorHudConfig)ConfigurationManager.GetInstance().GetConfiguration(new WarriorHudConfig());
        private Vector2 Origin => new(CenterX + _config.Position.X, CenterY + _config.Position.Y);
        private Dictionary<string, uint> EmptyColor => PluginConfiguration.MiscColorMap["empty"];
        public WarriorHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _)
        {
            if (_config.ShowStormsEye)
            {
                DrawStormsEyeBar();
            }

            if (_config.ShowBeastGauge)
            {
                DrawBeastGauge();
            }
        }

        protected override void DrawPrimaryResourceBar() { }

        private Vector2 CalculatePosition(Vector2 position, Vector2 size) => Origin + position - size / 2f;

        private void DrawStormsEyeBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            IEnumerable<StatusEffect> innerReleaseBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1177);
            IEnumerable<StatusEffect> stormsEyeBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 90);

            Vector2 position = CalculatePosition(_config.StormsEyePosition, _config.StormsEyeSize);

            BarBuilder builder = BarBuilder.Create(position, _config.StormsEyeSize).SetBackgroundColor(EmptyColor["background"]);

            var duration = 0f;
            var maximum = 10f;
            Dictionary<string, uint> color = EmptyColor;

            if (innerReleaseBuff.Any())
            {
                duration = Math.Abs(innerReleaseBuff.First().Duration);
                color = _config.InnerReleaseColor.Map;
            }
            else if (stormsEyeBuff.Any())
            {
                duration = Math.Abs(stormsEyeBuff.First().Duration);
                maximum = 60f;
                color = _config.StormsEyeColor.Map;
            }

            builder.AddInnerBar(duration, maximum, color);

            if (_config.ShowStormsEyeText)
            {
                builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawBeastGauge()
        {
            WARGauge gauge = PluginInterface.ClientState.JobGauges.Get<WARGauge>();
            IEnumerable<StatusEffect> nascentChaosBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1897);

            Vector2 position = CalculatePosition(_config.BeastGaugePosition, _config.BeastGaugeSize);

            BarBuilder builder = BarBuilder.Create(position, _config.BeastGaugeSize)
                                           .SetChunks(2)
                                           .AddInnerBar(gauge.BeastGaugeAmount, 100, _config.BeastGaugeFillColor.Map)
                                           .SetBackgroundColor(EmptyColor["background"])
                                           .SetChunkPadding(_config.BeastGaugePadding);

            if (nascentChaosBuff.Any())
            {
                builder.SetChunksColors(_config.NascentChaosColor.Map);
            }

            if (_config.ShowBeastGaugeText)
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
    [SubSection("Warrior", 1)]
    public class WarriorHudConfig : PluginConfigObject
    {
        [DragFloat2("Base Position" + "##Warrior", min = -4000f, max = 4000f)]
        [Order(0)]
        public Vector2 Position = new(0, 0);

        /* Storm's Eye */
        #region Storm's Eye
        [Checkbox("Show Storm's Eye")]
        [CollapseControl(5, 0)]
        public bool ShowStormsEye = true;

        [Checkbox("Show Text" + "##StormsEye")]
        [CollapseWith(0, 0)]
        public bool ShowStormsEyeText = true;

        [DragFloat2("Position" + "##StormsEye", min = -4000f, max = 4000f)]
        [CollapseWith(5, 0)]
        public Vector2 StormsEyePosition = new(0, 428);

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

        /* Beast Gauge*/
        #region Beast Gauge
        [Checkbox("Show Beast Gauge")]
        [CollapseControl(10, 1)]
        public bool ShowBeastGauge = true;

        [Checkbox("Show Text" + "##BeastGauge")]
        [CollapseWith(0, 1)]
        public bool ShowBeastGaugeText = false;

        [DragFloat2("Position" + "##BeastGauge", min = -4000f, max = 4000f)]
        [CollapseWith(5, 1)]
        public Vector2 BeastGaugePosition = new(0, 449);

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
