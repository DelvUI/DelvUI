﻿using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Interface.Bars;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using System.Numerics;

namespace DelvUI.Interface
{
    public class WarriorHudWindow : HudWindow
    {
        public override uint JobId => Jobs.WAR;

        private readonly WarriorHudConfig _config = (WarriorHudConfig) ConfigurationManager.GetInstance().GetConfiguration(new WarriorHudConfig());

        private Vector2 Origin => new Vector2(CenterX + _config.Position.X, CenterY + _config.Position.Y);

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

        protected override void DrawPrimaryResourceBar() {
        }

        private Vector2 CalculatePosition(Vector2 position, Vector2 size)
        {
            return Origin + position - size / 2f;
        }

        private void DrawStormsEyeBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var innerReleaseBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1177);
            var stormsEyeBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 90);

            var position = CalculatePosition(_config.StormsEyePosition, _config.StormsEyeSize);
            var builder = BarBuilder.Create(position, _config.StormsEyeSize)
                .SetBackgroundColor(EmptyColor["background"]);

            var duration = 0f;
            var maximum = 10f;
            var color = EmptyColor;

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
                builder.SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawBeastGauge()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<WARGauge>();
            var nascentChaosBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1897);

            var position = CalculatePosition(_config.BeastGaugePosition, _config.BeastGaugeSize);
            var builder = BarBuilder.Create(position, _config.BeastGaugeSize)
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
    [SubSection("Warrior", 1)]
    public class WarriorHudConfig : PluginConfigObject
    {
        [DragFloat2("Base offset", min = -4000f, max = 4000f)]
        public Vector2 Position = new Vector2(0, 0);

        /* Storm's Eye */
        [Checkbox("Show Storm's Eye")]
        public bool ShowStormsEye = true;
        
        [Checkbox("Show Storm's Eye Text")]
        public bool ShowStormsEyeText = true;

        [DragFloat2("Storm's Eye Position", min = -4000f, max = 4000f)]
        public Vector2 StormsEyePosition = new(0, 428);

        [DragFloat2("Storm's Eye Size", min = 1f, max = 4000f)]
        public Vector2 StormsEyeSize = new(254, 20);

        [ColorEdit4("Inner Release Color")] public PluginConfigColor InnerReleaseColor = new(new Vector4(255f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));
        [ColorEdit4("Storm's Eye Color")] public PluginConfigColor StormsEyeColor = new(new Vector4(255f / 255f, 136f / 255f, 146f / 255f, 100f / 100f));

        /* Beast Gauge*/
        [Checkbox("Show Beast Gauge")]
        public bool ShowBeastGauge = true;
        
        [Checkbox("Show Beast Gauge Text")]
        public bool ShowBeastGaugeText = false;

        [DragFloat2("Beast Gauge Position", min = -4000f, max = 4000f)]
        public Vector2 BeastGaugePosition = new(0, 449);
        
        [DragFloat2("Beast Gauge Size", min = 1f, max = 4000f)]
        public Vector2 BeastGaugeSize = new(254, 20);

        [DragFloat("Beast Gauge Spacing")]
        public float BeastGaugePadding = 2.0f;

        [ColorEdit4("Beast Gauge Fill Color")] public PluginConfigColor BeastGaugeFillColor = new(new Vector4(201f / 255f, 13f / 255f, 13f / 255f, 100f / 100f));
        [ColorEdit4("Nascent Chaos Color")] public PluginConfigColor NascentChaosColor = new(new Vector4(240f / 255f, 176f / 255f, 0f / 255f, 100f / 100f));
    }

    

}