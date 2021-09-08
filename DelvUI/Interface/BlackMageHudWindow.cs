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

            if (_config.ShowDotBar)
            {
                DrawDotTimer();
            }
        }

        protected override void DrawPrimaryResourceBar() { }

        protected virtual void DrawManaBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<BLMGauge>();
            var actor = PluginInterface.ClientState.LocalPlayer;

            var position = new Vector2(
                CenterX + _config.Position.X + _config.ManaBarPosition.X - _config.ManaBarSize.X / 2f,
                CenterY + _config.Position.Y + _config.ManaBarPosition.Y - _config.ManaBarSize.Y / 2f
            );

            var color = gauge.InAstralFire() ? _config.ManaBarFireColor.Map : gauge.InUmbralIce() ? _config.ManaBarIceColor.Map : _config.ManaBarNoElementColor.Map;

            var builder = BarBuilder.Create(position, _config.ManaBarSize)
                .AddInnerBar(actor.CurrentMp, actor.MaxMp, color)
                .SetBackgroundColor(EmptyColor["background"]);

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
                var pos = new Vector2(
                    position.X + _config.ManaThresholdValue / 10000f * _config.ManaBarSize.X,
                    position.Y + _config.ManaBarSize.Y
                );
                var size = new Vector2(3, _config.ManaBarSize.Y);

                drawList.AddRectFilledMultiColor(
                    pos,
                    pos - size,
                    0xFF000000,
                    0x00000000,
                    0x00000000,
                    0xFF000000
                );
            }

            // mana
            if (_config.ShowManaValue)
            {
                var text = $"{actor.CurrentMp,0}";
                var textSize = ImGui.CalcTextSize(text);
                var textPos = new Vector2(
                    position.X + 2,
                    position.Y + _config.ManaBarSize.Y / 2f - textSize.Y / 2f
                );
                DrawOutlinedText(text, textPos);
            }
        }

        protected virtual void DrawUmbralHeartStacks()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<BLMGauge>();
            var position = new Vector2(
                CenterX + _config.Position.X + _config.UmbralHeartPosition.X - _config.UmbralHeartSize.X / 2f,
                CenterY + _config.Position.Y + _config.UmbralHeartPosition.Y - _config.UmbralHeartSize.Y / 2f
            );

            var bar = BarBuilder.Create(position, _config.UmbralHeartSize)
                                .SetChunks(3)
                                .SetChunkPadding(_config.UmbralHeartPadding)
                                .AddInnerBar(gauge.NumUmbralHearts, 3, _config.UmbralHeartColor.Map, EmptyColor)
                                .SetBackgroundColor(EmptyColor["background"])
                                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        protected virtual void DrawPolyglot()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<BLMGauge>();

            var position = new Vector2(
                CenterX + _config.Position.X + _config.PolyglotPosition.X - _config.PolyglotSize.X / 2f,
                CenterY + _config.Position.Y + _config.PolyglotPosition.Y - _config.PolyglotSize.Y / 2f
            );

            var barWidth = (int)(_config.PolyglotSize.X - _config.PolyglotPadding) / 2;
            var barSize = new Vector2(barWidth, _config.PolyglotSize.Y);

            var scale = 1 - (gauge.IsEnoActive() ? gauge.TimeUntilNextPolyglot / 30000f : 1);
            var drawList = ImGui.GetWindowDrawList();

            // 1
            var builder = BarBuilder.Create(position, barSize)
                                    .AddInnerBar(gauge.NumPolyglotStacks < 1 ? scale : 1, 1, _config.PolyglotColor.Map)
                                    .SetBackgroundColor(EmptyColor["background"]);

            if (gauge.NumPolyglotStacks >= 1)
            {
                builder.SetGlowColor(0x88FFFFFF);
            }

            builder.Build().Draw(drawList, PluginConfiguration);

            // 2
            position.X += barWidth + _config.PolyglotPadding;
            builder = BarBuilder.Create(position, barSize)
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

            var position = new Vector2(
                CenterX + _config.Position.X + _config.TriplecastPosition.X - _config.TripleCastSize.X / 2f,
                CenterY + _config.Position.Y + _config.TriplecastPosition.Y - _config.TripleCastSize.Y / 2f
            );

            var bar = BarBuilder.Create(position, _config.TripleCastSize)
                                .SetChunks(3)
                                .SetChunkPadding(_config.TriplecastPadding)
                                .AddInnerBar(tripleStackBuff.StackCount, 3, _config.TriplecastColor.Map, EmptyColor)
                                .SetBackgroundColor(EmptyColor["background"])
                                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        protected virtual void DrawProcs()
        {
            var statusEffects = PluginInterface.ClientState.LocalPlayer.StatusEffects;
            var firestarterTimer = _config.ShowFirestarterProcs ? Math.Abs(statusEffects.FirstOrDefault(o => o.EffectId == 165).Duration) : 0;
            var thundercloudTimer = _config.ShowThundercloudProcs ? Math.Abs(statusEffects.FirstOrDefault(o => o.EffectId == 164).Duration) : 0;

            var position = new Vector2(
                CenterX + _config.Position.X + _config.ProcsBarPosition.X,
                CenterY + _config.Position.Y + _config.ProcsBarPosition.Y - _config.ProcsBarSize.Y / 2f
            );

            var builder = BarBuilder.Create(position, _config.ProcsBarSize);

            // fire starter
            if (_config.ShowFirestarterProcs)
            {
                var scale = firestarterTimer / 18f;
                builder.AddInnerBar(firestarterTimer, 18f, _config.FirestarterColor.Map);
                builder.SetFlipDrainDirection(_config.InvertProcsBar);
            }

            // thundercloud
            if (_config.ShowThundercloudProcs)
            {
                var scale = thundercloudTimer / 18f;
                builder.AddInnerBar(thundercloudTimer, 18f, _config.ThundercloudColor.Map);
                builder.SetFlipDrainDirection(_config.InvertProcsBar);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        protected virtual void DrawDotTimer()
        {
            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;
            float timer = 0;
            float maxDuration = 1;

            if (target != null)
            {
                // thunder 1 to 4
                int[] dotIDs = { 161, 162, 163, 1210 };
                float[] dotDurations = { 12, 18, 24, 18 };
                var player = PluginInterface.ClientState.LocalPlayer;

                for (var i = 0; i < 4; i++)
                {
                    timer = target.StatusEffects.FirstOrDefault(o => o.EffectId == dotIDs[i] && o.OwnerId == player.ActorId).Duration;

                    if (timer > 0)
                    {
                        maxDuration = dotDurations[i];

                        break;
                    }
                }
            }

            var position = new Vector2(
                CenterX + _config.Position.X + _config.DoTBarPosition.X,
                CenterY + _config.Position.Y + _config.DoTBarPosition.Y - _config.ProcsBarSize.Y / 2f
            );

            var builder = BarBuilder.Create(position, _config.DoTBarSize)
                .AddInnerBar(timer, maxDuration, _config.DotColor.Map)
                .SetFlipDrainDirection(_config.InvertDoTBar);

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Caster", 0)]
    [SubSection("Black Mage", 1)]
    public class BlackMageHudConfig : PluginConfigObject
    {
        [DragFloat2("Base Position", min = -4000f, max = 4000f)]
        [Order(0)]
        public Vector2 Position = new Vector2(0, 0);

        #region mana bar
        [DragFloat2("Mana Bar Position", min = -2000, max = 2000f)]
        [Order(5)]
        public Vector2 ManaBarPosition = new Vector2(0, 449);

        [DragFloat2("Mana Bar Size", max = 2000f)]
        [Order(10)]
        public Vector2 ManaBarSize = new Vector2(254, 20);

        [Checkbox("Show Mana Value")]
        [Order(15)]
        public bool ShowManaValue = false;

        [Checkbox("Show Mana Threshold Marker During Astral Fire")]
        [CollapseControl(20, 0)]
        public bool ShowManaThresholdMarker = true;

        [DragInt("Mana Threshold Marker Value", max = 10000)]
        [CollapseWith(0, 0)]
        public int ManaThresholdValue = 2400;

        [ColorEdit4("Mana Bar Color")]
        [Order(25)]
        public PluginConfigColor ManaBarNoElementColor = new PluginConfigColor(new Vector4(234f / 255f, 95f / 255f, 155f / 255f, 100f / 100f));

        [ColorEdit4("Mana Bar Ice Color")]
        [Order(30)]
        public PluginConfigColor ManaBarIceColor = new PluginConfigColor(new Vector4(69f / 255f, 115f / 255f, 202f / 255f, 100f / 100f));

        [ColorEdit4("Mana Bar Fire Color")]
        [Order(35)]
        public PluginConfigColor ManaBarFireColor = new PluginConfigColor(new Vector4(204f / 255f, 40f / 255f, 40f / 255f, 100f / 100f));
        #endregion

        #region umbral heart
        [DragFloat2("Umbral Heart Bar Position", min = -2000, max = 2000f)]
        [Order(40)]
        public Vector2 UmbralHeartPosition = new Vector2(0, 429);

        [DragFloat2("Umbral Heart Bar Size", max = 2000f)]
        [Order(45)]
        public Vector2 UmbralHeartSize = new Vector2(254, 16);

        [DragInt("Umbral Heart Padding", min = -100, max = 100)]
        [Order(50)]
        public int UmbralHeartPadding = 2;

        [ColorEdit4("Umbral Heart Color")]
        [Order(55)]
        public PluginConfigColor UmbralHeartColor = new PluginConfigColor(new Vector4(125f / 255f, 195f / 255f, 205f / 255f, 100f / 100f));
        #endregion

        #region triple cast
        [Checkbox("Show Triplecast")]
        [CollapseControl(60, 1)]
        public bool ShowTripleCast = true;

        [DragFloat2("Triplecast Position", min = -2000, max = 2000f)]
        [CollapseWith(0, 1)]
        public Vector2 TriplecastPosition = new Vector2(0, 411);

        [DragFloat2("Triplecast Size", max = 2000)]
        [CollapseWith(5, 1)]
        public Vector2 TripleCastSize = new Vector2(254, 16);

        [DragInt("Trioplecast Padding", min = -100, max = 100)]
        [CollapseWith(10, 1)]
        public int TriplecastPadding = 2;

        [ColorEdit4("Triplecast Color")]
        [CollapseWith(15, 1)]
        public PluginConfigColor TriplecastColor = new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));
        #endregion

        #region polyglot
        [DragFloat2("Polyglot Position", min = -2000, max = 2000f)]
        [Order(65)]
        public Vector2 PolyglotPosition = new Vector2(0, 392);

        [DragFloat2("Polyglot Size", max = 2000f)]
        [Order(70)]
        public Vector2 PolyglotSize = new Vector2(38, 18);

        [DragInt("Polyglot Padding", min = -100, max = 100)]
        [Order(75)]
        public int PolyglotPadding = 2;

        [ColorEdit4("Polyglot Color")]
        [Order(80)]
        public PluginConfigColor PolyglotColor = new PluginConfigColor(new Vector4(234f / 255f, 95f / 255f, 155f / 255f, 100f / 100f));
        #endregion

        #region procs
        [Checkbox("Show Firestarter Procs")]
        [CollapseControl(85, 2)]
        public bool ShowFirestarterProcs = true;

        [Checkbox("Show Thundercloud Procs")]
        [CollapseWith(0, 2)]
        public bool ShowThundercloudProcs = true;

        [Checkbox("Invert Procs Bar")]
        [CollapseWith(5, 2)]
        public bool InvertProcsBar = true;

        [DragFloat2("Procs Bar Position", min = -2000, max = 2000f)]
        [CollapseWith(10, 2)]
        public Vector2 ProcsBarPosition = new Vector2(-127, 392);

        [DragFloat2("Procs Bar Size", max = 2000f)]
        [CollapseWith(15, 2)]
        public Vector2 ProcsBarSize = new Vector2(106, 18);

        [ColorEdit4("Firestarter Color")]
        [CollapseWith(20, 2)]
        public PluginConfigColor FirestarterColor = new PluginConfigColor(new Vector4(255f / 255f, 136f / 255f, 0 / 255f, 90f / 100f));

        [ColorEdit4("Thundercloud Color")]
        [CollapseWith(25, 2)]
        public PluginConfigColor ThundercloudColor = new PluginConfigColor(new Vector4(240f / 255f, 163f / 255f, 255f / 255f, 90f / 100f));
        #endregion

        #region thunder dots
        [Checkbox("Show DoT Bar")]
        [CollapseControl(90, 4)]
        public bool ShowDotBar = true;

        [Checkbox("Invert DoT Bar")]
        [CollapseWith(0, 4)]
        public bool InvertDoTBar = false;

        [DragFloat2("DoT Bar Position", min = -2000, max = 2000f)]
        [CollapseWith(5, 4)]
        public Vector2 DoTBarPosition = new Vector2(21, 392);

        [DragFloat2("DoT Bar Size", max = 2000f)]
        [CollapseWith(10, 4)]
        public Vector2 DoTBarSize = new Vector2(106, 18);

        [ColorEdit4("DoT Color")]
        [CollapseWith(15, 4)]
        public PluginConfigColor DotColor = new PluginConfigColor(new Vector4(67f / 255f, 187 / 255f, 255f / 255f, 90f / 100f));
        #endregion
    }
}
