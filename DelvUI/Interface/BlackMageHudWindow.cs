using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Interface.Bars;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface
{
    public class BlackMageHudWindow : HudWindow
    {
        public override uint JobId => Jobs.BLM;

        private BlackMageHudConfig _config => (BlackMageHudConfig)ConfigurationManager.GetInstance().GetConfiguration(new BlackMageHudConfig());

        private float OriginX => CenterX + _config.Position.X;
        private float OriginY => CenterY + YOffset + _config.Position.Y;
        private Dictionary<string, uint> EmptyColor => PluginConfiguration.MiscColorMap["empty"];

        public BlackMageHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration)
            : base(pluginInterface, pluginConfiguration) { }

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

        protected override void DrawPrimaryResourceBar() { }

        protected virtual void DrawManaBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<BLMGauge>();

            var actor = PluginInterface.ClientState.LocalPlayer;
            var barSize = _config.ManaBarSize;
            var cursorPos = new Vector2(OriginX - barSize.X / 2, OriginY - barSize.Y);

            var color = gauge.InAstralFire()
                ? _config.ManaBarFireColor.Map
                : gauge.InUmbralIce() ? _config.ManaBarIceColor.Map : _config.ManaBarNoElementColor.Map;

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
                var position = new Vector2(OriginX - barSize.X / 2 + _config.ManaThresholdValue / 10000f * barSize.X, cursorPos.Y + barSize.Y);
                var size = new Vector2(3, barSize.Y);

                drawList.AddRectFilledMultiColor(
                    position,
                    position - size,
                    0xFF000000,
                    0x00000000,
                    0x00000000,
                    0xFF000000
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
                                .AddInnerBar(gauge.NumUmbralHearts, 3, _config.UmbralHeartColor.Map, EmptyColor)
                                .SetBackgroundColor(EmptyColor["background"])
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
                                .AddInnerBar(
                                    gauge.NumPolyglotStacks == 1
                                        ? scale
                                        : gauge.NumPolyglotStacks == 0
                                            ? 0
                                            : 1,
                                    1,
                                    _config.PolyglotColor.Map
                                )
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
            var firestarterTimer = _config.ShowFirestarterProcs
                ? Math.Abs(PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 165).Duration)

                : 0;

            var thundercloudTimer = _config.ShowThundercloudProcs
                ? Math.Abs(PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 164).Duration)
                : 0;

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
            int[] dotIDs = { 161, 162, 163, 1210 };
            float[] dotDurations = { 12, 18, 24, 18 };

            float timer = 0;
            float maxDuration = 1;

            for (var i = 0; i < 4; i++)
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

            drawList.AddRectFilledMultiColor(
                startPoint,
                startPoint + size,
                leftColor,
                rightColor,
                rightColor,
                leftColor
            );
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Caster", 0)]
    [SubSection("Black Mage", 1)]
    public class BlackMageHudConfig : PluginConfigObject
    {
        [DragFloat2("Base Offset", min = -4000f, max = 4000f)]
        [Order(0)]
        public Vector2 Position = new Vector2(0, -2);

        [DragFloat2("Padding", min = -100f, max = 100f)]
        [Order(5)]
        public Vector2 Padding = new Vector2(2, 2);

        [DragFloat2("Mana Bar Size", max = 2000f)]
        [Order(10)]
        public Vector2 ManaBarSize = new Vector2(253, 20);

        [Checkbox("Show Mana Value")]
        [Order(15)]
        public bool ShowManaValue = false;

        [DragFloat2("Umbra Heart Size", max = 2000f)]
        [Order(20)]
        public Vector2 UmbralHeartSize = new Vector2(83, 16);

        [DragFloat2("Polyglot Size", max = 2000f)]
        [Order(25)]
        public Vector2 PolyglotSize = new Vector2(18, 18);

        [Checkbox("Show Mana Threshold Marker During Astral Fire")]
        [CollapseControl(30, 0)]
        public bool ShowManaThresholdMarker = true;

        [DragInt("Mana Threshold Marker Value", max = 10000)]
        [CollapseWith(0, 0)]
        public int ManaThresholdValue = 2600;

        [ColorEdit4("Mana Bar Color")]
        [Order(35)]
        public PluginConfigColor ManaBarNoElementColor = new PluginConfigColor(new Vector4(234f / 255f, 95f / 255f, 155f / 255f, 100f / 100f));

        [ColorEdit4("Mana Bar Ice Color")]
        [Order(40)]
        public PluginConfigColor ManaBarIceColor = new PluginConfigColor(new Vector4(69f / 255f, 115f / 255f, 202f / 255f, 100f / 100f));

        [ColorEdit4("Mana Bar Fire Color")]
        [Order(45)]
        public PluginConfigColor ManaBarFireColor = new PluginConfigColor(new Vector4(204f / 255f, 40f / 255f, 40f / 255f, 100f / 100f));

        [Checkbox("Show Triplecast")] 
        [CollapseControl(50, 1)]
        public bool ShowTripleCast = true;

        [DragFloat2("Triplecast Size", max = 2000)]
        [CollapseWith(0, 1)]
        public Vector2 TripleCastSize = new Vector2(83, 16);

        [ColorEdit4("Triplecast Color")]
        [CollapseWith(5, 1)]
        public PluginConfigColor TriplecastColor = new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [DragInt("Procs Height", max = 2000)]
        [Order(55)]
        public int ProcsHeight = 7;

        [Checkbox("Show Firestarter Procs")]
        [CollapseControl(60, 2)]
        public bool ShowFirestarterProcs = true;

        [ColorEdit4("Firestarter Proc Color")]
        [CollapseWith(0, 2)]
        public PluginConfigColor FirestarterColor = new PluginConfigColor(new Vector4(255f / 255f, 136f / 255f, 0 / 255f, 90f / 100f));

        [Checkbox("Show Thundercloud Procs")]
        [CollapseControl(65, 3)]
        public bool ShowThundercloudProcs = true;

        [ColorEdit4("Thundercloud Proc Color")]
        [CollapseWith(0, 3)]
        public PluginConfigColor ThundercloudColor = new PluginConfigColor(new Vector4(240f / 255f, 163f / 255f, 255f / 255f, 90f / 100f));

        [Checkbox("Show DoT Timer")]
        [CollapseControl(70, 4)]
        public bool ShowDotTimer = true;

        [DragInt("DoT Timer Height", max = 2000)]
        [CollapseWith(0, 4)]
        public int DotTimerHeight = 10;

        [ColorEdit4("DoT Timer Color")]
        [CollapseWith(5, 4)]
        public PluginConfigColor DotColor = new PluginConfigColor(new Vector4(67f / 255f, 187 / 255f, 255f / 255f, 90f / 100f));

        [ColorEdit4("Umbral Heart Color")]
        [Order(75)]
        public PluginConfigColor UmbralHeartColor = new PluginConfigColor(new Vector4(125f / 255f, 195f / 255f, 205f / 255f, 100f / 100f));

        [ColorEdit4("Polyglot Color Color")]
        [Order(80)]
        public PluginConfigColor PolyglotColor = new PluginConfigColor(new Vector4(234f / 255f, 95f / 255f, 155f / 255f, 100f / 100f));
    }
}
