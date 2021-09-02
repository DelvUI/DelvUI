using System;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using DelvUI.Interface.Bars;
using DelvUI.Helpers;

namespace DelvUI.Interface
{
    public class BlackMageHudWindow : HudWindow
    {
        public override uint JobId => Jobs.BLM;

        private float OriginX => CenterX + PluginConfiguration.BLMHorizontalOffset;
        private float OriginY => CenterY + YOffset + PluginConfiguration.BLMVerticalOffset;
        private int VerticalSpaceBetweenBars => PluginConfiguration.BLMVerticalSpaceBetweenBars;
        private int HorizontalSpaceBetweenBars => PluginConfiguration.BLMHorizontalSpaceBetweenBars;
        private int ManaBarWidth => PluginConfiguration.BLMManaBarWidth;
        private int ManaBarHeight => PluginConfiguration.BLMManaBarHeight;
        private int UmbralHeartHeight => PluginConfiguration.BLMUmbralHeartHeight;
        private int UmbralHeartWidth=> PluginConfiguration.BLMUmbralHeartWidth;
        private int PolyglotHeight => PluginConfiguration.BLMPolyglotHeight;
        private int PolyglotWidth => PluginConfiguration.BLMPolyglotWidth;
        private bool ShowManaValue => PluginConfiguration.BLMShowManaValue;
        private bool ShowManaThresholdMarker => PluginConfiguration.BLMShowManaThresholdMarker;
        private int ManaThresholdValue => PluginConfiguration.BLMManaThresholdValue;
        private bool ShowTripleCast => PluginConfiguration.BLMShowTripleCast;
        private int TripleCastHeight => PluginConfiguration.BLMTripleCastHeight;
        private int TripleCastWidth => PluginConfiguration.BLMTripleCastWidth;
        private bool ShowFirestarterProcs => PluginConfiguration.BLMShowFirestarterProcs;
        private bool ShowThundercloudProcs => PluginConfiguration.BLMShowThundercloudProcs;
        private int ProcsHeight => PluginConfiguration.BLMProcsHeight;
        private bool ShowDotTimer => PluginConfiguration.BLMShowDotTimer;
        private int DotTimerHeight => PluginConfiguration.BLMDotTimerHeight;

        private Dictionary<string, uint> ManaBarNoElementColor => PluginConfiguration.JobColorMap[Jobs.BLM * 1000];
        private Dictionary<string, uint> ManaBarIceColor => PluginConfiguration.JobColorMap[Jobs.BLM * 1000 + 1];
        private Dictionary<string, uint> ManaBarFireColor => PluginConfiguration.JobColorMap[Jobs.BLM * 1000 + 2];
        private Dictionary<string, uint> UmbralHeartColor => PluginConfiguration.JobColorMap[Jobs.BLM * 1000 + 3];
        private Dictionary<string, uint> PolyglotColor => PluginConfiguration.JobColorMap[Jobs.BLM * 1000 + 4];
        private Dictionary<string, uint> TriplecastColor => PluginConfiguration.JobColorMap[Jobs.BLM * 1000 + 5];
        private Dictionary<string, uint> FirestarterColor => PluginConfiguration.JobColorMap[Jobs.BLM * 1000 + 6];
        private Dictionary<string, uint> ThundercloudColor => PluginConfiguration.JobColorMap[Jobs.BLM * 1000 + 7];
        private Dictionary<string, uint> DotColor => PluginConfiguration.JobColorMap[Jobs.BLM * 1000 + 8];
        private Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.BLM * 1000 + 9];

        public BlackMageHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _)
        {
            DrawManaBar();
            DrawUmbralHeartStacks();
            DrawPolyglot();

            if (ShowTripleCast)
            {
                DrawTripleCast();
            }

            if (ShowFirestarterProcs || ShowThundercloudProcs)
            {
                DrawProcs();
            }

            if (ShowDotTimer)
            {
                DrawDotTimer();
            }
        }

        protected override void DrawPrimaryResourceBar()
        {
        }

        protected virtual void DrawManaBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");

            var gauge = PluginInterface.ClientState.JobGauges.Get<BLMGauge>();

            var actor = PluginInterface.ClientState.LocalPlayer;
            var barSize = new Vector2(ManaBarWidth, ManaBarHeight);
            var cursorPos = new Vector2(OriginX - barSize.X / 2, OriginY - barSize.Y);
            var color = gauge.InAstralFire() ? ManaBarFireColor : (gauge.InUmbralIce() ? ManaBarIceColor : ManaBarNoElementColor);

            var builder = BarBuilder.Create(OriginX - ManaBarWidth / 2, OriginY - ManaBarHeight, ManaBarHeight, ManaBarWidth);
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
            if (ShowManaThresholdMarker && gauge.InAstralFire())
            {
                var position = new Vector2(OriginX - barSize.X / 2 + (ManaThresholdValue / 10000f) * barSize.X, cursorPos.Y + barSize.Y);
                var size = new Vector2(3, barSize.Y);
                drawList.AddRectFilledMultiColor(
                    position, position - size,
                    0xFF000000, 0x00000000, 0x00000000, 0xFF000000
                );
            }

            // mana
            if (ShowManaValue)
            {
                var mana = PluginInterface.ClientState.LocalPlayer.CurrentMp;
                var text = $"{mana,0}";
                var textSize = ImGui.CalcTextSize(text);
                DrawOutlinedText(text, new Vector2(OriginX - barSize.X / 2f + 2, OriginY - ManaBarHeight / 2f - textSize.Y / 2f));
            }
        }

        protected virtual void DrawUmbralHeartStacks()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");

            var gauge = PluginInterface.ClientState.JobGauges.Get<BLMGauge>();

            var totalWidth = UmbralHeartWidth * 3 + HorizontalSpaceBetweenBars * 2;
            var cursorPos = new Vector2(OriginX - totalWidth / 2, OriginY - ManaBarHeight - VerticalSpaceBetweenBars - UmbralHeartHeight);

            var bar = BarBuilder.Create(cursorPos.X, cursorPos.Y, UmbralHeartHeight, totalWidth)
                .SetChunks(3)
                .SetChunkPadding(HorizontalSpaceBetweenBars)
                .AddInnerBar(gauge.NumUmbralHearts, 3, UmbralHeartColor, EmptyColor).SetBackgroundColor(EmptyColor["background"])
                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        protected virtual void DrawPolyglot()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");

            var gauge = PluginInterface.ClientState.JobGauges.Get<BLMGauge>();
            var totalWidth = PolyglotWidth * 2 + HorizontalSpaceBetweenBars;
            var y = OriginY - ManaBarHeight - VerticalSpaceBetweenBars - UmbralHeartHeight - VerticalSpaceBetweenBars - PolyglotHeight;
            if (ShowTripleCast)
            {
                y = y - VerticalSpaceBetweenBars - TripleCastHeight;
            }

            var scale = 1 - (gauge.IsEnoActive() ? gauge.TimeUntilNextPolyglot / 30000f : 1);
            var drawList = ImGui.GetWindowDrawList();

            // 1
            var builder = BarBuilder.Create(OriginX - totalWidth / 2, y, PolyglotHeight, PolyglotWidth)
                .AddInnerBar(gauge.NumPolyglotStacks < 1 ? scale : 1, 1, PolyglotColor).SetBackgroundColor(
                EmptyColor["background"]);

            if (gauge.NumPolyglotStacks >= 1)
            {
                builder.SetGlowColor(0x88FFFFFF);
            }

            builder.Build().Draw(drawList, PluginConfiguration);

            // 2
            builder = BarBuilder.Create(OriginX - totalWidth / 2 + PolyglotWidth + HorizontalSpaceBetweenBars, y, PolyglotHeight, PolyglotWidth)
                .AddInnerBar(gauge.NumPolyglotStacks == 1 ? scale : gauge.NumPolyglotStacks == 0 ? 0 : 1, 1, PolyglotColor).SetBackgroundColor(EmptyColor["background"]);

            if (gauge.NumPolyglotStacks == 2)
            {
                builder.SetGlowColor(0x88FFFFFF);
            }

            builder.Build().Draw(drawList, PluginConfiguration);
        }

        protected virtual void DrawTripleCast()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");

            var tripleStackBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 1211);

            var totalWidth = TripleCastWidth * 3 + HorizontalSpaceBetweenBars * 2;
            var cursorPos = new Vector2(OriginX - totalWidth / 2, OriginY - ManaBarHeight - VerticalSpaceBetweenBars - UmbralHeartHeight - VerticalSpaceBetweenBars - TripleCastHeight);

            var bar = BarBuilder.Create(cursorPos.X, cursorPos.Y, TripleCastHeight, totalWidth)
                .SetChunks(3)
                .SetChunkPadding(HorizontalSpaceBetweenBars)
                .AddInnerBar(tripleStackBuff.StackCount, 3, TriplecastColor, EmptyColor).SetBackgroundColor(EmptyColor["background"])
                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        protected virtual void DrawProcs()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");

            var firestarterTimer = ShowFirestarterProcs ? Math.Abs(PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 165).Duration) : 0;
            var thundercloudTimer = ShowThundercloudProcs ? Math.Abs(PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 164).Duration) : 0;

            if (firestarterTimer == 0 && thundercloudTimer == 0)
            {
                return;
            }

            var totalHeight = firestarterTimer > 0 && thundercloudTimer > 0 ? ProcsHeight * 2 + VerticalSpaceBetweenBars : ProcsHeight;
            var x = OriginX - HorizontalSpaceBetweenBars * 2f - PolyglotWidth;
            var y = OriginY - ManaBarHeight - VerticalSpaceBetweenBars - UmbralHeartHeight - VerticalSpaceBetweenBars - PolyglotHeight / 2f + ProcsHeight;
            if (ShowTripleCast)
            {
                y = y - VerticalSpaceBetweenBars - TripleCastHeight;
            }

            // fire starter
            if (firestarterTimer > 0)
            {
                var position = new Vector2(x, y - totalHeight / 2f);
                var scale = firestarterTimer / 18f;

                DrawTimerBar(position, scale, ProcsHeight, FirestarterColor, true);
            }

            // thundercloud
            if (thundercloudTimer > 0)
            {
                var position = new Vector2(x, firestarterTimer == 0 ? y - totalHeight / 2f : y + VerticalSpaceBetweenBars / 2f);
                var scale = thundercloudTimer / 18f;

                DrawTimerBar(position, scale, ProcsHeight, ThundercloudColor, true);
            }
        }

        protected virtual void DrawDotTimer()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");

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

            var x = OriginX + HorizontalSpaceBetweenBars * 2f + PolyglotWidth;
            var y = OriginY - ManaBarHeight - VerticalSpaceBetweenBars - UmbralHeartHeight - VerticalSpaceBetweenBars - PolyglotHeight / 2f - DotTimerHeight;
            if (ShowTripleCast)
            {
                y = y - VerticalSpaceBetweenBars - TripleCastHeight;
            }

            var position = new Vector2(x, y + DotTimerHeight / 2f);
            var scale = timer / maxDuration;
            
            DrawTimerBar(position, scale, DotTimerHeight, DotColor, false);
        }

        private void DrawTimerBar(Vector2 position, float scale, float height, Dictionary<string, uint> colorMap, bool inverted)
        {
            var drawList = ImGui.GetWindowDrawList();
            var size = new Vector2((ManaBarWidth / 2f - PolyglotWidth - HorizontalSpaceBetweenBars * 2f) * scale, height);
            size.X = Math.Max(1, size.X);

            var startPoint = inverted ? position - size : position;
            var leftColor = inverted ? colorMap["gradientRight"] : colorMap["gradientLeft"];
            var rightColor = inverted ? colorMap["gradientLeft"] : colorMap["gradientRight"];

            drawList.AddRectFilledMultiColor(startPoint, startPoint + size,
                leftColor, rightColor, rightColor, leftColor
            );
        }
    }
}