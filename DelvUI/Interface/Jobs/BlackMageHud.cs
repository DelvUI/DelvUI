using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
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
    public class BlackMageHud : JobHud
    {
        private new BlackMageConfig Config => (BlackMageConfig)_config;
        private Dictionary<string, uint> EmptyColor => GlobalColors.Instance.EmptyColor.Map;

        public BlackMageHud(string id, BlackMageConfig config, PluginConfiguration pluginConfiguration) : base(id, config, pluginConfiguration)
        {

        }

        public override void Draw(Vector2 origin)
        {
            DrawManaBar(origin);
            DrawUmbralHeartStacks(origin);
            DrawPolyglot(origin);

            if (Config.ShowTripleCast)
            {
                DrawTripleCast(origin);
            }

            if (Config.ShowFirestarterProcs || Config.ShowThundercloudProcs)
            {
                DrawProcs(origin);
            }

            if (Config.ShowDotBar)
            {
                DrawDotTimer(origin);
            }
        }

        protected virtual void DrawManaBar(Vector2 origin)
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<BLMGauge>();
            var actor = PluginInterface.ClientState.LocalPlayer;

            var position = new Vector2(
                origin.X + Config.Position.X + Config.ManaBarPosition.X - Config.ManaBarSize.X / 2f,
                origin.Y + Config.Position.Y + Config.ManaBarPosition.Y - Config.ManaBarSize.Y / 2f
            );

            var color = gauge.InAstralFire() ? Config.ManaBarFireColor.Map : gauge.InUmbralIce() ? Config.ManaBarIceColor.Map : Config.ManaBarNoElementColor.Map;

            var builder = BarBuilder.Create(position, Config.ManaBarSize)
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
            if (Config.ShowManaThresholdMarker && gauge.InAstralFire())
            {
                var pos = new Vector2(
                    position.X + Config.ManaThresholdValue / 10000f * Config.ManaBarSize.X,
                    position.Y + Config.ManaBarSize.Y
                );
                var size = new Vector2(3, Config.ManaBarSize.Y);

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
            if (Config.ShowManaValue)
            {
                var text = $"{actor.CurrentMp,0}";
                var textSize = ImGui.CalcTextSize(text);
                var textPos = new Vector2(
                    position.X + 2,
                    position.Y + Config.ManaBarSize.Y / 2f - textSize.Y / 2f
                );
                DrawHelper.DrawOutlinedText(text, textPos);
            }
        }

        protected virtual void DrawUmbralHeartStacks(Vector2 origin)
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<BLMGauge>();
            var position = new Vector2(
                origin.X + Config.Position.X + Config.UmbralHeartPosition.X - Config.UmbralHeartSize.X / 2f,
                origin.Y + Config.Position.Y + Config.UmbralHeartPosition.Y - Config.UmbralHeartSize.Y / 2f
            );

            var bar = BarBuilder.Create(position, Config.UmbralHeartSize)
                                .SetChunks(3)
                                .SetChunkPadding(Config.UmbralHeartPadding)
                                .AddInnerBar(gauge.NumUmbralHearts, 3, Config.UmbralHeartColor.Map, EmptyColor)
                                .SetBackgroundColor(EmptyColor["background"])
                                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        protected virtual void DrawPolyglot(Vector2 origin)
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<BLMGauge>();

            var position = new Vector2(
                origin.X + Config.Position.X + Config.PolyglotPosition.X - Config.PolyglotSize.X / 2f,
                origin.Y + Config.Position.Y + Config.PolyglotPosition.Y - Config.PolyglotSize.Y / 2f
            );

            var barWidth = (int)(Config.PolyglotSize.X - Config.PolyglotPadding) / 2;
            var barSize = new Vector2(barWidth, Config.PolyglotSize.Y);

            var scale = 1 - (gauge.IsEnoActive() ? gauge.TimeUntilNextPolyglot / 30000f : 1);
            var drawList = ImGui.GetWindowDrawList();

            // 1
            var builder = BarBuilder.Create(position, barSize)
                                    .AddInnerBar(gauge.NumPolyglotStacks < 1 ? scale : 1, 1, Config.PolyglotColor.Map)
                                    .SetBackgroundColor(EmptyColor["background"]);

            if (gauge.NumPolyglotStacks >= 1)
            {
                builder.SetGlowColor(0x88FFFFFF);
            }

            builder.Build().Draw(drawList, PluginConfiguration);

            // 2
            position.X += barWidth + Config.PolyglotPadding;
            builder = BarBuilder.Create(position, barSize)
                                .AddInnerBar(gauge.NumPolyglotStacks == 1 ? scale : gauge.NumPolyglotStacks == 0 ? 0 : 1, 1, Config.PolyglotColor.Map)
                                .SetBackgroundColor(EmptyColor["background"]);

            if (gauge.NumPolyglotStacks == 2)
            {
                builder.SetGlowColor(0x88FFFFFF);
            }

            builder.Build().Draw(drawList, PluginConfiguration);
        }

        protected virtual void DrawTripleCast(Vector2 origin)
        {
            var tripleStackBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 1211);

            var position = new Vector2(
                origin.X + Config.Position.X + Config.TriplecastPosition.X - Config.TripleCastSize.X / 2f,
                origin.Y + Config.Position.Y + Config.TriplecastPosition.Y - Config.TripleCastSize.Y / 2f
            );

            var bar = BarBuilder.Create(position, Config.TripleCastSize)
                                .SetChunks(3)
                                .SetChunkPadding(Config.TriplecastPadding)
                                .AddInnerBar(tripleStackBuff.StackCount, 3, Config.TriplecastColor.Map, EmptyColor)
                                .SetBackgroundColor(EmptyColor["background"])
                                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        protected virtual void DrawProcs(Vector2 origin)
        {
            var statusEffects = PluginInterface.ClientState.LocalPlayer.StatusEffects;
            var firestarterTimer = Config.ShowFirestarterProcs ? Math.Abs(statusEffects.FirstOrDefault(o => o.EffectId == 165).Duration) : 0;
            var thundercloudTimer = Config.ShowThundercloudProcs ? Math.Abs(statusEffects.FirstOrDefault(o => o.EffectId == 164).Duration) : 0;

            var position = new Vector2(
                origin.X + Config.Position.X + Config.ProcsBarPosition.X,
                origin.Y + Config.Position.Y + Config.ProcsBarPosition.Y - Config.ProcsBarSize.Y / 2f
            );

            var builder = BarBuilder.Create(position, Config.ProcsBarSize);

            // fire starter
            if (Config.ShowFirestarterProcs)
            {
                var scale = firestarterTimer / 18f;
                builder.AddInnerBar(firestarterTimer, 18f, Config.FirestarterColor.Map);
                builder.SetFlipDrainDirection(Config.InvertProcsBar);
            }

            // thundercloud
            if (Config.ShowThundercloudProcs)
            {
                var scale = thundercloudTimer / 18f;
                builder.AddInnerBar(thundercloudTimer, 18f, Config.ThundercloudColor.Map);
                builder.SetFlipDrainDirection(Config.InvertProcsBar);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        protected virtual void DrawDotTimer(Vector2 origin)
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
                origin.X + Config.Position.X + Config.DoTBarPosition.X,
                origin.Y + Config.Position.Y + Config.DoTBarPosition.Y - Config.ProcsBarSize.Y / 2f
            );

            var builder = BarBuilder.Create(position, Config.DoTBarSize)
                .AddInnerBar(timer, maxDuration, Config.DotColor.Map)
                .SetFlipDrainDirection(Config.InvertDoTBar);

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Caster", 0)]
    [SubSection("Black Mage", 1)]
    public class BlackMageConfig : JobConfig
    {
        [JsonIgnore] public new uint JobId = JobIDs.BLM;
        public new static BlackMageConfig DefaultConfig() { return new BlackMageConfig(); }

        #region mana bar
        [DragFloat2("Mana Bar Position", min = -2000, max = 2000f)]
        [Order(30)]
        public Vector2 ManaBarPosition = new Vector2(0, HUDConstants.JobHudsBaseY - 10);

        [DragFloat2("Mana Bar Size", max = 2000f)]
        [Order(35)]
        public Vector2 ManaBarSize = new Vector2(254, 20);

        [Checkbox("Show Mana Value")]
        [Order(40)]
        public bool ShowManaValue = false;

        [Checkbox("Show Mana Threshold Marker During Astral Fire")]
        [CollapseControl(45, 0)]
        public bool ShowManaThresholdMarker = true;

        [DragInt("Mana Threshold Marker Value", max = 10000)]
        [CollapseWith(0, 0)]
        public int ManaThresholdValue = 2400;

        [ColorEdit4("Mana Bar Color")]
        [Order(50)]
        public PluginConfigColor ManaBarNoElementColor = new PluginConfigColor(new Vector4(234f / 255f, 95f / 255f, 155f / 255f, 100f / 100f));

        [ColorEdit4("Mana Bar Ice Color")]
        [Order(55)]
        public PluginConfigColor ManaBarIceColor = new PluginConfigColor(new Vector4(69f / 255f, 115f / 255f, 202f / 255f, 100f / 100f));

        [ColorEdit4("Mana Bar Fire Color")]
        [Order(60)]
        public PluginConfigColor ManaBarFireColor = new PluginConfigColor(new Vector4(204f / 255f, 40f / 255f, 40f / 255f, 100f / 100f));
        #endregion

        #region umbral heart
        [DragFloat2("Umbral Heart Bar Position", min = -2000, max = 2000f)]
        [Order(65)]
        public Vector2 UmbralHeartPosition = new Vector2(0, HUDConstants.JobHudsBaseY - 30);

        [DragFloat2("Umbral Heart Bar Size", max = 2000f)]
        [Order(70)]
        public Vector2 UmbralHeartSize = new Vector2(254, 16);

        [DragInt("Umbral Heart Padding", min = -100, max = 100)]
        [Order(75)]
        public int UmbralHeartPadding = 2;

        [ColorEdit4("Umbral Heart Color")]
        [Order(80)]
        public PluginConfigColor UmbralHeartColor = new PluginConfigColor(new Vector4(125f / 255f, 195f / 255f, 205f / 255f, 100f / 100f));
        #endregion

        #region triple cast
        [Checkbox("Show Triplecast")]
        [CollapseControl(85, 1)]
        public bool ShowTripleCast = true;

        [DragFloat2("Triplecast Position", min = -2000, max = 2000f)]
        [CollapseWith(0, 1)]
        public Vector2 TriplecastPosition = new Vector2(0, HUDConstants.JobHudsBaseY - 48);

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
        [Order(90)]
        public Vector2 PolyglotPosition = new Vector2(0, HUDConstants.JobHudsBaseY - 67);

        [DragFloat2("Polyglot Size", max = 2000f)]
        [Order(95)]
        public Vector2 PolyglotSize = new Vector2(38, 18);

        [DragInt("Polyglot Padding", min = -100, max = 100)]
        [Order(100)]
        public int PolyglotPadding = 2;

        [ColorEdit4("Polyglot Color")]
        [Order(105)]
        public PluginConfigColor PolyglotColor = new PluginConfigColor(new Vector4(234f / 255f, 95f / 255f, 155f / 255f, 100f / 100f));
        #endregion

        #region procs
        [Checkbox("Show Firestarter Procs")]
        [CollapseControl(110, 2)]
        public bool ShowFirestarterProcs = true;

        [Checkbox("Show Thundercloud Procs")]
        [CollapseWith(0, 2)]
        public bool ShowThundercloudProcs = true;

        [Checkbox("Invert Procs Bar")]
        [CollapseWith(5, 2)]
        public bool InvertProcsBar = true;

        [DragFloat2("Procs Bar Position", min = -2000, max = 2000f)]
        [CollapseWith(10, 2)]
        public Vector2 ProcsBarPosition = new Vector2(-127, HUDConstants.JobHudsBaseY - 67);

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
        [CollapseControl(115, 4)]
        public bool ShowDotBar = true;

        [Checkbox("Invert DoT Bar")]
        [CollapseWith(0, 4)]
        public bool InvertDoTBar = false;

        [DragFloat2("DoT Bar Position", min = -2000, max = 2000f)]
        [CollapseWith(5, 4)]
        public Vector2 DoTBarPosition = new Vector2(21, HUDConstants.JobHudsBaseY - 67);

        [DragFloat2("DoT Bar Size", max = 2000f)]
        [CollapseWith(10, 4)]
        public Vector2 DoTBarSize = new Vector2(106, 18);

        [ColorEdit4("DoT Color")]
        [CollapseWith(15, 4)]
        public PluginConfigColor DotColor = new PluginConfigColor(new Vector4(67f / 255f, 187 / 255f, 255f / 255f, 90f / 100f));
        #endregion
    }
}
