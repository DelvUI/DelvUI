using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Interface.Bars;
using ImGuiNET;
using DelvUI.Config;
using System.Numerics;

namespace DelvUI.Interface {
    public class WarriorHudWindow : HudWindow {
        public override uint JobId => Jobs.WAR;
        
        private readonly WarriorHudConfig _config;

        private Dictionary<string, uint> EmptyColor => PluginConfiguration.MiscColorMap["empty"];

        public WarriorHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration, WarriorHudConfig config) : base(pluginInterface, pluginConfiguration) {
            _config = config;
        }

        protected override void Draw(bool _) {

            if (_config.ShowStormsEye) {
                DrawStormsEyeBar();
            }

            if (_config.ShowBeastGauge) { 
                DrawBeastGauge();
            }
        }

        protected override void DrawPrimaryResourceBar() {
        }

        private void DrawStormsEyeBar() {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var innerReleaseBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1177);
            var stormsEyeBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 90);

            var xPos = (CenterX - _config.StormsEyeSize.X/2f) + _config.Position.X + _config.StormsEyePosition.X;
            var yPos = (CenterY - _config.StormsEyeSize.Y/2f) + _config.Position.Y + _config.StormsEyePosition.Y;

            var builder = BarBuilder.Create(xPos, yPos, _config.StormsEyeSize.Y, _config.StormsEyeSize.X).SetBackgroundColor(EmptyColor["background"]);

            var duration = 0f;
            var maximum = 10f;
            var color = EmptyColor;
            if (innerReleaseBuff.Any()) {
                duration = Math.Abs(innerReleaseBuff.First().Duration);
                color = _config.InnerReleaseColor.Map;
            }
            else if (stormsEyeBuff.Any()) {
                duration = Math.Abs(stormsEyeBuff.First().Duration);
                maximum = 60f;
                color = _config.StormsEyeColor.Map;
            }

            builder.AddInnerBar(duration, maximum, color);

            if (_config.ShowStormsEyeText) {
                builder.SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawBeastGauge() {
            var gauge = PluginInterface.ClientState.JobGauges.Get<WARGauge>();
            var nascentChaosBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1897);

            var xPos = (CenterX - _config.BeastGaugeSize.X / 2f) + _config.Position.X + _config.BeastGaugePosition.X;
            var yPos = (CenterY - _config.BeastGaugeSize.Y / 2f) + _config.Position.Y + _config.BeastGaugePosition.Y;

            var builder = BarBuilder.Create(xPos, yPos, _config.BeastGaugeSize.Y, _config.BeastGaugeSize.X)
                .SetChunks(2)
                .AddInnerBar(gauge.BeastGaugeAmount, 100, _config.BeastGaugeFillColor.Map).SetBackgroundColor(EmptyColor["background"])
                .SetChunkPadding(_config.BeastGaugePadding);

            if (nascentChaosBuff.Any()) {
                builder.SetChunksColors(_config.NascentChaosColor.Map);
            }

            if (_config.ShowBeastGaugeText) {
                builder.SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

    }

    [Serializable]
    public class WarriorHudConfig : PluginConfigObject {
        public Vector2 Position = new(0, 0);

        public bool ShowStormsEye = true;
        public bool ShowStormsEyeText = true;
        public Vector2 StormsEyeSize = new(254, 20);
        public Vector2 StormsEyePosition = new(127, 417);

        public bool ShowBeastGauge = true;
        public bool ShowBeastGaugeText = false;
        public Vector2 BeastGaugeSize = new(254, 20);
        public float BeastGaugePadding = 2.0f;
        public Vector2 BeastGaugePosition = new(127, 439);

        public PluginConfigColor InnerReleaseColor = new(new Vector4(255f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));
        public PluginConfigColor StormsEyeColor = new(new Vector4(255f / 255f, 136f / 255f, 146f / 255f, 100f / 100f));
        public PluginConfigColor BeastGaugeFillColor = new(new Vector4(201f / 255f, 13f / 255f, 13f / 255f, 100f / 100f));
        public PluginConfigColor NascentChaosColor = new(new Vector4(240f / 255f, 176f / 255f, 0f / 255f, 100f / 100f));
      
        public bool Draw() {
            var changed = false;

            ImGui.BeginGroup(); // Begin Configation Group
            {

                ImGui.Text("");
                ImGui.Text("Base Offset");
                
                changed |= ImGui.DragFloat2("##Base Offset", ref Position, 1f, -4000, 4000);

                /* Storm's Eye */
                ImGui.Text("");
                ImGui.Text("Storm's Eye Bar Config");
                ImGui.Separator();

                changed |= ImGui.Checkbox("Show ##stormEye", ref ShowStormsEye);
                changed |= ImGui.Checkbox("Show Text ##stormEye", ref ShowStormsEyeText);
                changed |= ImGui.DragFloat2("Position (X, Y) ##stormEye", ref StormsEyePosition, 1f, -4000, 4000);
                changed |= ImGui.DragFloat2("Size (Width, Height) ##stormEye", ref StormsEyeSize, 1f, 20, 4000);
                changed |= ColorEdit4("Storm's Eye ##color", ref StormsEyeColor);
                changed |= ColorEdit4("Inner Release ##color", ref InnerReleaseColor);

                /* Beast Gauge */
                ImGui.Text("");
                ImGui.Text("Beast Gauge Bar:");
                ImGui.Separator();
                
                changed |= ImGui.Checkbox("Show##bestGauge", ref ShowBeastGauge);
                changed |= ImGui.Checkbox("Show Text ##bestGauge", ref ShowBeastGaugeText);
                changed |= ImGui.DragFloat2("Position (X, Y) ##bestGauge", ref BeastGaugePosition, 1f, -4000, 4000);
                changed |= ImGui.DragFloat2("Size (Width, Height) ##bestGauge", ref BeastGaugeSize, 1f, 20, 4000);
                changed |= ImGui.DragFloat("Chunk Padding ##bestGauge", ref BeastGaugePadding, 1f, 0, 4000);

                changed |= ColorEdit4("Beast Gauge Fill Color ##color", ref BeastGaugeFillColor);
                changed |= ColorEdit4("Nascent Chaos Ready Color ##color", ref NascentChaosColor);
            }
            ImGui.EndGroup(); // End Configation Group

            return changed;
        }

    }

}