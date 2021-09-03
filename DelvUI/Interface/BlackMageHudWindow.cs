using System;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using DelvUI.Interface.Bars;
using DelvUI.Config;

namespace DelvUI.Interface
{
    public class BlackMageHudWindow : HudWindow
    {
        public override uint JobId => Jobs.BLM;
        private BlackMageHudConfig _config;

        private float OriginX => CenterX + _config.Position.X;
        private float OriginY => CenterY + YOffset + _config.Position.Y;
        private Dictionary<string, uint> EmptyColor => PluginConfiguration.MiscColorMap["empty"];

        public BlackMageHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration, BlackMageHudConfig config) 
            : base(pluginInterface, pluginConfiguration) 
        {

            _config = config;
        }

        protected override void Draw(bool _)
        {
            DrawManaBar();
            DrawUmbralHeartStacks();
            DrawPolyglot();

            if (_config.ShowTripleCast)
            {
                DrawTripleCast();
            }

            if (_config.ShowFirestarterProcs || _config.ShowThundercloudProcs)
            {
                DrawProcs();
            }

            if (_config.ShowDotTimer)
            {
                DrawDotTimer();
            }
        }

        protected override void DrawPrimaryResourceBar()
        {
        }

        protected virtual void DrawManaBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<BLMGauge>();

            var actor = PluginInterface.ClientState.LocalPlayer;
            var barSize = _config.ManaBarSize;
            var cursorPos = new Vector2(OriginX - barSize.X / 2, OriginY - barSize.Y);
            var color = gauge.InAstralFire() ? _config.ManaBarFireColor.Map : (gauge.InUmbralIce() ? 
                _config.ManaBarIceColor.Map : _config.ManaBarNoElementColor.Map);

            var builder = BarBuilder.Create(OriginX - barSize.X / 2, OriginY - barSize.Y, (int)barSize.Y, (int)barSize.X);


            builder.AddInnerBar(actor.CurrentMp, actor.MaxMp, color).SetBackgroundColor(EmptyColor["background"]);

            // element timer
            if (gauge.InAstralFire() || gauge.InUmbralIce())
            {
                var time = gauge.ElementTimeRemaining > 10 ? gauge.ElementTimeRemaining / 1000 + 1 : 0;
                builder.SetTextMode(BarTextMode.Single);
                builder.SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, $"{time,0}");
            }
            
            // enochian
            if (gauge.IsEnoActive())
            {
                builder.SetGlowSize(2);
                builder.SetGlowColor(0x88FFFFFF);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);

            // threshold marker
            if (_config.ShowManaThresholdMarker && gauge.InAstralFire())
            {
                var position = new Vector2(OriginX - barSize.X / 2 + (_config.ManaThresholdValue / 10000f) * barSize.X, cursorPos.Y + barSize.Y);
                var size = new Vector2(3, barSize.Y);
                drawList.AddRectFilledMultiColor(
                    position, position - size,
                    0xFF000000, 0x00000000, 0x00000000, 0xFF000000
                );
            }

            // mana
            if (_config.ShowManaValue)
            {
                var mana = PluginInterface.ClientState.LocalPlayer.CurrentMp;
                var text = $"{mana,0}";
                var textSize = ImGui.CalcTextSize(text);
                DrawOutlinedText(text, new Vector2(OriginX - barSize.X / 2f + 2, OriginY - _config.ManaBarSize.Y / 2f - textSize.Y / 2f));
            }
        }

        protected virtual void DrawUmbralHeartStacks()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<BLMGauge>();

            var totalWidth = _config.UmbralHeartSize.X * 3 + _config.Padding.X * 2;
            var cursorPos = new Vector2(OriginX - totalWidth / 2, OriginY - _config.ManaBarSize.Y - _config.Padding.Y - _config.UmbralHeartSize.Y);

            var bar = BarBuilder.Create(cursorPos.X, cursorPos.Y, _config.UmbralHeartSize.Y, totalWidth)
                .SetChunks(3)
                .SetChunkPadding(_config.Padding.X)
                .AddInnerBar(gauge.NumUmbralHearts, 3, _config.UmbralHeartColor.Map, EmptyColor).SetBackgroundColor(EmptyColor["background"])
                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        protected virtual void DrawPolyglot()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<BLMGauge>();
            var totalWidth = _config.PolyglotSize.X * 2 + _config.Padding.X;
            var y = OriginY - _config.ManaBarSize.Y - _config.Padding.Y - _config.UmbralHeartSize.Y - _config.Padding.Y - _config.PolyglotSize.Y;
            if (_config.ShowTripleCast)
            {
                y = y - _config.Padding.Y - _config.TripleCastSize.Y;
            }

            var scale = 1 - (gauge.IsEnoActive() ? gauge.TimeUntilNextPolyglot / 30000f : 1);
            var drawList = ImGui.GetWindowDrawList();

            // 1
            var builder = BarBuilder.Create(OriginX - totalWidth / 2, y, _config.PolyglotSize.Y, _config.PolyglotSize.X)
                .AddInnerBar(gauge.NumPolyglotStacks < 1 ? scale : 1, 1, _config.PolyglotColor.Map)
                .SetBackgroundColor(EmptyColor["background"]);

            if (gauge.NumPolyglotStacks >= 1)
            {
                builder.SetGlowColor(0x88FFFFFF);
            }

            builder.Build().Draw(drawList, PluginConfiguration);

            // 2
            builder = BarBuilder.Create(OriginX - totalWidth / 2 + _config.PolyglotSize.X + _config.Padding.X, y, _config.PolyglotSize.Y, _config.PolyglotSize.X)
                .AddInnerBar(gauge.NumPolyglotStacks == 1 ? scale : gauge.NumPolyglotStacks == 0 ? 0 : 1, 1, _config.PolyglotColor.Map)
                .SetBackgroundColor(EmptyColor["background"]);

            if (gauge.NumPolyglotStacks == 2)
            {
                builder.SetGlowColor(0x88FFFFFF);
            }

            builder.Build().Draw(drawList, PluginConfiguration);
        }

        protected virtual void DrawTripleCast()
        {
            var tripleStackBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 1211);

            var totalWidth = _config.TripleCastSize.X * 3 + _config.Padding.X * 2;
            var cursorPos = new Vector2(
                OriginX - totalWidth / 2, 
                OriginY - _config.ManaBarSize.Y - _config.Padding.Y - _config.UmbralHeartSize.Y - _config.Padding.Y - _config.TripleCastSize.Y
            );

            var bar = BarBuilder.Create(cursorPos.X, cursorPos.Y, _config.TripleCastSize.Y, totalWidth)
                .SetChunks(3)
                .SetChunkPadding(_config.Padding.X)
                .AddInnerBar(tripleStackBuff.StackCount, 3, _config.TriplecastColor.Map, EmptyColor)
                .SetBackgroundColor(EmptyColor["background"])
                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        protected virtual void DrawProcs()
        {
            var firestarterTimer = _config.ShowFirestarterProcs ? Math.Abs(PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 165).Duration) : 0;
            var thundercloudTimer = _config.ShowThundercloudProcs ? Math.Abs(PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 164).Duration) : 0;

            if (firestarterTimer == 0 && thundercloudTimer == 0)
            {
                return;
            }

            var totalHeight = firestarterTimer > 0 && thundercloudTimer > 0 ? _config.ProcsHeight * 2 + _config.Padding.Y : _config.ProcsHeight;
            var x = OriginX - _config.Padding.X * 2f - _config.PolyglotSize.X;
            var y = OriginY - _config.ManaBarSize.Y - _config.Padding.Y - _config.UmbralHeartSize.Y - _config.Padding.Y - _config.PolyglotSize.Y / 2f + _config.ProcsHeight;
            if (_config.ShowTripleCast)
            {
                y = y - _config.Padding.Y - _config.TripleCastSize.Y;
            }

            // fire starter
            if (firestarterTimer > 0)
            {
                var position = new Vector2(x, y - totalHeight / 2f);
                var scale = firestarterTimer / 18f;

                DrawTimerBar(position, scale, _config.ProcsHeight, _config.FirestarterColor.Map, true);
            }

            // thundercloud
            if (thundercloudTimer > 0)
            {
                var position = new Vector2(x, firestarterTimer == 0 ? y - totalHeight / 2f : y + _config.Padding.Y / 2f);
                var scale = thundercloudTimer / 18f;

                DrawTimerBar(position, scale, _config.ProcsHeight, _config.ThundercloudColor.Map, true);
            }
        }

        protected virtual void DrawDotTimer()
        {
            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;
            if (target is null)
            {
                return;
            }

            // thunder 1 to 4
            int[] dotIDs = new int[] { 161, 162, 163, 1210 };
            float[] dotDurations = new float[] { 12, 18, 24, 18 };

            float timer = 0;
            float maxDuration = 1;

            for (int i = 0; i < 4; i++)
            {
                timer = target.StatusEffects.FirstOrDefault(o => o.EffectId == dotIDs[i] && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId).Duration;
                if (timer > 0)
                {
                    maxDuration = dotDurations[i];
                    break;
                }
            }

            if (timer == 0)
            {
                return;
            }

            var x = OriginX + _config.Padding.X * 2f + _config.PolyglotSize.X;
            var y = OriginY - _config.ManaBarSize.Y - _config.Padding.Y - _config.UmbralHeartSize.Y - _config.Padding.Y - _config.PolyglotSize.Y / 2f - _config.DotTimerHeight;
            if (_config.ShowTripleCast)
            {
                y = y - _config.Padding.Y - _config.TripleCastSize.Y;
            }

            var position = new Vector2(x, y + _config.DotTimerHeight / 2f);
            var scale = timer / maxDuration;
            
            DrawTimerBar(position, scale, _config.DotTimerHeight, _config.DotColor.Map, false);
        }

        private void DrawTimerBar(Vector2 position, float scale, float height, Dictionary<string, uint> colorMap, bool inverted)
        {
            var drawList = ImGui.GetWindowDrawList();
            var size = new Vector2((_config.ManaBarSize.X / 2f - _config.PolyglotSize.X - _config.Padding.X * 2f) * scale, height);
            size.X = Math.Max(1, size.X);

            var startPoint = inverted ? position - size : position;
            var leftColor = inverted ? colorMap["gradientRight"] : colorMap["gradientLeft"];
            var rightColor = inverted ? colorMap["gradientLeft"] : colorMap["gradientRight"];

            drawList.AddRectFilledMultiColor(startPoint, startPoint + size,
                leftColor, rightColor, rightColor, leftColor
            );
        }
    }

    [Serializable]
    public class BlackMageHudConfig: PluginConfigObject
    {
        public Vector2 Position = new Vector2(0, -2);
        public Vector2 Padding = new Vector2(2, 2);
        public Vector2 ManaBarSize = new Vector2(253, 20);
        public Vector2 UmbralHeartSize = new Vector2(83, 16);
        public Vector2 PolyglotSize = new Vector2(18, 18);
        
        public bool ShowManaValue = false;
        public bool ShowManaThresholdMarker = true;
        public int ManaThresholdValue = 2600;

        public bool ShowTripleCast = true;
        public Vector2 TripleCastSize = new Vector2(83, 16);
        
        public bool ShowFirestarterProcs = true;
        public bool ShowThundercloudProcs = true;
        public int ProcsHeight = 7;
        public bool ShowDotTimer = true;
        public int DotTimerHeight = 10;

        public PluginConfigColor ManaBarNoElementColor = new PluginConfigColor(new Vector4(234f / 255f, 95f / 255f, 155f / 255f, 100f / 100f));
        public PluginConfigColor ManaBarIceColor = new PluginConfigColor(new Vector4(69f / 255f, 115f / 255f, 202f / 255f, 100f / 100f));
        public PluginConfigColor ManaBarFireColor = new PluginConfigColor(new Vector4(204f / 255f, 40f / 255f, 40f / 255f, 100f / 100f));
        public PluginConfigColor UmbralHeartColor = new PluginConfigColor(new Vector4(125f / 255f, 195f / 255f, 205f / 255f, 100f / 100f));
        public PluginConfigColor PolyglotColor = new PluginConfigColor(new Vector4(234f / 255f, 95f / 255f, 155f / 255f, 100f / 100f));
        public PluginConfigColor TriplecastColor = new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));
        public PluginConfigColor FirestarterColor = new PluginConfigColor(new Vector4(255f / 255f, 136f / 255f, 0 / 255f, 90f / 100f));
        public PluginConfigColor ThundercloudColor = new PluginConfigColor(new Vector4(240f / 255f, 163f / 255f, 255f / 255f, 90f / 100f));
        public PluginConfigColor DotColor = new PluginConfigColor(new Vector4(67f / 255f, 187 / 255f, 255f / 255f, 90f / 100f));

        public bool Draw()
        {
            var changed = false;

            changed |= ImGui.DragFloat2("Base Offset", ref Position, 1f, -4000, 4000);
            changed |= ImGui.DragFloat2("Padding", ref Padding, 1f, -100, 100);
            changed |= ImGui.DragFloat2("Mana Bar Size", ref ManaBarSize, 1f, 1, 2000);
            changed |= ImGui.DragFloat2("Umbral Heart Size", ref UmbralHeartSize, 1f, 1, 2000);
            changed |= ImGui.DragFloat2("Polyglot Size", ref PolyglotSize, 1f, 1, 2000);

            changed |= ImGui.Checkbox("Show Mana Value", ref ShowManaValue);
            changed |= ImGui.Checkbox("Show Mana Threshold Marker During Astral Fire", ref ShowManaThresholdMarker);
            changed |= ImGui.DragInt("Mana Threshold Marker Value", ref ManaThresholdValue, 1f, 1, 10000);
            
            changed |= ImGui.Checkbox("Show Triplecast", ref ShowTripleCast);
            changed |= ImGui.DragFloat2("Triplecast Size", ref TripleCastSize, 1f, 1, 2000);
            
            changed |= ImGui.Checkbox("Show Firestarter Procs", ref ShowFirestarterProcs);
            changed |= ImGui.Checkbox("Show Thundercloud Procs", ref ShowThundercloudProcs);

            changed |= ImGui.DragInt("Procs Height", ref ProcsHeight, .1f, 1, 2000);

            changed |= ImGui.Checkbox("Show DoT Timer", ref ShowDotTimer);
            changed |= ImGui.DragInt("DoT Timer Height", ref DotTimerHeight, .1f, 1, 2000);

            changed |= ColorEdit4("Mana Bar Color", ref ManaBarNoElementColor);
            changed |= ColorEdit4("Mana Bar Ice Color", ref ManaBarIceColor);
            changed |= ColorEdit4("Mana Bar Fire Color", ref ManaBarFireColor);
            changed |= ColorEdit4("Umbral Heart Color", ref UmbralHeartColor);
            changed |= ColorEdit4("Polyglot Color", ref PolyglotColor);
            changed |= ColorEdit4("Triplecast Color", ref TriplecastColor);
            changed |= ColorEdit4("Firestarter Proc Color", ref FirestarterColor);
            changed |= ColorEdit4("Thundercloud Proc Color", ref ThundercloudColor);
            changed |= ColorEdit4("DoT Timer Color", ref DotColor);

            return changed;
        }
    }
}