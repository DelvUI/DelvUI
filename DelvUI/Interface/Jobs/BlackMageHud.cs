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
using Dalamud.Game.ClientState.Objects.Types;

namespace DelvUI.Interface.Jobs
{
    public class BlackMageHud : JobHud
    {
        private new BlackMageConfig Config => (BlackMageConfig)_config;
        private PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;

        public BlackMageHud(string id, BlackMageConfig config, string? displayName = null) : base(id, config, displayName)
        {

        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.ShowManaBar)
            {
                positions.Add(Config.Position + Config.ManaBarPosition);
                sizes.Add(Config.ManaBarSize);
            }

            if (Config.ShowUmbralHeart)
            {
                positions.Add(Config.Position + Config.UmbralHeartPosition);
                sizes.Add(Config.UmbralHeartSize);
            }

            if (Config.ShowPolyglot)
            {
                positions.Add(Config.Position + Config.PolyglotPosition);
                sizes.Add(Config.PolyglotSize);
            }

            if (Config.ShowTriplecast)
            {
                positions.Add(Config.Position + Config.TriplecastPosition);
                sizes.Add(Config.TriplecastSize);
            }

            if (Config.ShowProcs)
            {
                positions.Add(Config.Position + Config.ProcsBarPosition);
                sizes.Add(Config.ProcsBarSize);
            }

            if (Config.ShowDotBar)
            {
                positions.Add(Config.Position + Config.DoTBarPosition);
                sizes.Add(Config.DoTBarSize);
            }

            return (positions, sizes);
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (Config.ShowManaBar)
            {
                DrawManaBar(origin);
            }

            if (Config.ShowUmbralHeart)
            {
                DrawUmbralHeartStacks(origin);
            }

            if (Config.ShowPolyglot)
            {
                DrawPolyglot(origin);
            }

            if (Config.ShowTriplecast)
            {
                DrawTripleCast(origin);
            }

            if (Config.ShowProcs)
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
            var gauge = Plugin.JobGauges.Get<BLMGauge>();
            var actor = Plugin.ClientState.LocalPlayer;
            if (actor == null)
            {
                return;
            }

            var position = origin + Config.Position + Config.ManaBarPosition - Config.ManaBarSize / 2f;

            var color = gauge.InAstralFire ? Config.ManaBarFireColor : gauge.InUmbralIce ? Config.ManaBarIceColor : Config.ManaBarNoElementColor;

            var builder = BarBuilder.Create(position, Config.ManaBarSize)
                .AddInnerBar(actor.CurrentMp, actor.MaxMp, color)
                .SetBackgroundColor(EmptyColor.Base);

            // element timer
            if (gauge.InAstralFire || gauge.InUmbralIce)
            {
                var time = gauge.ElementTimeRemaining > 10 ? gauge.ElementTimeRemaining / 1000 + 1 : 0;
                builder.SetTextMode(BarTextMode.Single);
                builder.SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, $"{time,0}");
            }

            // enochian
            if (gauge.IsEnochianActive)
            {
                builder.SetGlowSize(2);
                builder.SetGlowColor(0x88FFFFFF);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);

            // threshold marker
            if (Config.ShowManaThresholdMarker && gauge.InAstralFire)
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
            var gauge = Plugin.JobGauges.Get<BLMGauge>();
            var position = origin + Config.Position + Config.UmbralHeartPosition - Config.UmbralHeartSize / 2f;

            var bar = BarBuilder.Create(position, Config.UmbralHeartSize)
                                .SetChunks(3)
                                .SetChunkPadding(Config.UmbralHeartPadding)
                                .AddInnerBar(gauge.UmbralHearts, 3, Config.UmbralHeartColor, EmptyColor)
                                .SetBackgroundColor(EmptyColor.Base)
                                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        protected virtual void DrawPolyglot(Vector2 origin)
        {
            var gauge = Plugin.JobGauges.Get<BLMGauge>();

            var position = origin + Config.Position + Config.PolyglotPosition - Config.PolyglotSize / 2f;

            var barWidth = (int)(Config.PolyglotSize.X - Config.PolyglotPadding) / 2;
            var barSize = new Vector2(barWidth, Config.PolyglotSize.Y);

            var scale = 1 - (gauge.IsEnochianActive ? gauge.EnochianTimer / 30000f : 1);
            var drawList = ImGui.GetWindowDrawList();

            // 1
            var builder = BarBuilder.Create(position, barSize)
                                    .AddInnerBar(gauge.PolyglotStacks < 1 ? scale : 1, 1, Config.PolyglotColor)
                                    .SetBackgroundColor(EmptyColor.Base);

            if (gauge.PolyglotStacks >= 1)
            {
                builder.SetGlowColor(0x88FFFFFF);
            }

            builder.Build().Draw(drawList);

            // 2
            position.X += barWidth + Config.PolyglotPadding;
            builder = BarBuilder.Create(position, barSize)
                                .AddInnerBar(gauge.PolyglotStacks == 1 ? scale : gauge.PolyglotStacks == 0 ? 0 : 1, 1, Config.PolyglotColor)
                                .SetBackgroundColor(EmptyColor.Base);

            if (gauge.PolyglotStacks == 2)
            {
                builder.SetGlowColor(0x88FFFFFF);
            }

            builder.Build().Draw(drawList);
        }

        protected virtual void DrawTripleCast(Vector2 origin)
        {
            Debug.Assert(Plugin.ClientState.LocalPlayer != null, "Plugin.ClientState.LocalPlayer != null");
            var tripleStackBuff = Plugin.ClientState.LocalPlayer.StatusList.FirstOrDefault(o => o.StatusId == 1211);

            var position = origin + Config.Position + Config.TriplecastPosition - Config.TriplecastSize / 2f;

            var bar = BarBuilder.Create(position, Config.TriplecastSize)
                                .SetChunks(3)
                                .SetChunkPadding(Config.TriplecastPadding)
                                .AddInnerBar(tripleStackBuff?.StackCount ?? 0, 3, Config.TriplecastColor, EmptyColor)
                                .SetBackgroundColor(EmptyColor.Base)
                                .Build();

            var drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        protected virtual void DrawProcs(Vector2 origin)
        {
            Debug.Assert(Plugin.ClientState.LocalPlayer != null, "Plugin.ClientState.LocalPlayer != null");
            var statusEffects = Plugin.ClientState.LocalPlayer.StatusList;
            var firestarterTimer = Config.ShowFirestarterProcs ? Math.Abs(statusEffects.FirstOrDefault(o => o.StatusId == 165)?.RemainingTime ?? 0f) : 0;
            var thundercloudTimer = Config.ShowThundercloudProcs ? Math.Abs(statusEffects.FirstOrDefault(o => o.StatusId == 164)?.RemainingTime ?? 0f) : 0;

            var position = origin + Config.Position + Config.ProcsBarPosition - Config.ProcsBarSize / 2f;

            var builder = BarBuilder.Create(position, Config.ProcsBarSize);

            // fire starter
            if (Config.ShowFirestarterProcs)
            {
                var scale = firestarterTimer / 18f;
                builder.AddInnerBar(firestarterTimer, 18f, Config.FirestarterColor);
                builder.SetFlipDrainDirection(Config.InvertProcsBar);
            }

            // thundercloud
            if (Config.ShowThundercloudProcs)
            {
                var scale = thundercloudTimer / 18f;
                builder.AddInnerBar(thundercloudTimer, 18f, Config.ThundercloudColor);
                builder.SetFlipDrainDirection(Config.InvertProcsBar);
            }

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        protected virtual void DrawDotTimer(Vector2 origin)
        {
            Debug.Assert(Plugin.ClientState.LocalPlayer != null, "Plugin.ClientState.LocalPlayer != null");
            var actor = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            float timer = 0;
            float maxDuration = 1;

            if (actor is BattleChara target)
            {
                // thunder 1 to 4
                int[] dotIDs = { 161, 162, 163, 1210 };
                float[] dotDurations = { 12, 18, 24, 18 };

                for (var i = 0; i < 4; i++)
                {
                    timer = target.StatusList.FirstOrDefault(o => o.StatusId == dotIDs[i] && o.SourceID == Plugin.ClientState.LocalPlayer.ObjectId)?.RemainingTime ?? 0f;

                    if (timer > 0)
                    {
                        maxDuration = dotDurations[i];

                        break;
                    }
                }
            }

            var position = origin + Config.Position + Config.DoTBarPosition - Config.DoTBarSize / 2f;

            var builder = BarBuilder.Create(position, Config.DoTBarSize)
                .AddInnerBar(timer, maxDuration, Config.DotColor)
                .SetFlipDrainDirection(Config.InvertDoTBar);

            var drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Caster", 0)]
    [SubSection("Black Mage", 1)]
    public class BlackMageConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.BLM;
        public new static BlackMageConfig DefaultConfig() { return new BlackMageConfig(); }

        #region mana bar
        [Checkbox("Show Mana Bar", separator = true)]
        [CollapseControl(30, 0)]
        public bool ShowManaBar = true;

        [DragFloat2("Mana Bar Position", min = -2000, max = 2000f)]
        [CollapseWith(0, 0)]
        public Vector2 ManaBarPosition = new Vector2(0, -10);

        [DragFloat2("Mana Bar Size", max = 2000f)]
        [CollapseWith(5, 0)]
        public Vector2 ManaBarSize = new Vector2(254, 20);

        [Checkbox("Show Mana Value")]
        [CollapseWith(10, 0)]
        public bool ShowManaValue = false;

        [Checkbox("Show Mana Threshold Marker During Astral Fire")]
        [CollapseWith(15, 0)]
        public bool ShowManaThresholdMarker = true;

        [DragInt("Mana Threshold Marker Value", max = 10000)]
        [CollapseWith(20, 0)]
        public int ManaThresholdValue = 2400;

        [ColorEdit4("Mana Bar Color")]
        [CollapseWith(25, 0)]
        public PluginConfigColor ManaBarNoElementColor = new PluginConfigColor(new Vector4(234f / 255f, 95f / 255f, 155f / 255f, 100f / 100f));

        [ColorEdit4("Mana Bar Ice Color")]
        [CollapseWith(30, 0)]
        public PluginConfigColor ManaBarIceColor = new PluginConfigColor(new Vector4(69f / 255f, 115f / 255f, 202f / 255f, 100f / 100f));

        [ColorEdit4("Mana Bar Fire Color")]
        [CollapseWith(35, 0)]
        public PluginConfigColor ManaBarFireColor = new PluginConfigColor(new Vector4(204f / 255f, 40f / 255f, 40f / 255f, 100f / 100f));
        #endregion

        #region umbral heart
        [Checkbox("Show Umbral Heart Bar", separator = true)]
        [CollapseControl(35, 1)]
        public bool ShowUmbralHeart = true;

        [DragFloat2("Umbral Heart Bar Position", min = -2000, max = 2000f)]
        [CollapseWith(0, 1)]
        public Vector2 UmbralHeartPosition = new Vector2(0, -30);

        [DragFloat2("Umbral Heart Bar Size", max = 2000f)]
        [CollapseWith(5, 1)]
        public Vector2 UmbralHeartSize = new Vector2(254, 16);

        [DragInt("Umbral Heart Padding", min = -100, max = 100)]
        [CollapseWith(10, 1)]
        public int UmbralHeartPadding = 2;

        [ColorEdit4("Umbral Heart Color")]
        [CollapseWith(15, 1)]
        public PluginConfigColor UmbralHeartColor = new PluginConfigColor(new Vector4(125f / 255f, 195f / 255f, 205f / 255f, 100f / 100f));
        #endregion

        #region triple cast
        [Checkbox("Show Triplecast", separator = true)]
        [CollapseControl(40, 2)]
        public bool ShowTriplecast = true;

        [DragFloat2("Triplecast Position", min = -2000, max = 2000f)]
        [CollapseWith(0, 2)]
        public Vector2 TriplecastPosition = new Vector2(0, -48);

        [DragFloat2("Triplecast Size", max = 2000)]
        [CollapseWith(5, 2)]
        public Vector2 TriplecastSize = new Vector2(254, 16);

        [DragInt("Trioplecast Padding", min = -100, max = 100)]
        [CollapseWith(10, 2)]
        public int TriplecastPadding = 2;

        [ColorEdit4("Triplecast Color")]
        [CollapseWith(15, 2)]
        public PluginConfigColor TriplecastColor = new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));
        #endregion

        #region polyglot
        [Checkbox("Show Polyglot Stacks", separator = true)]
        [CollapseControl(45, 3)]
        public bool ShowPolyglot = true;

        [DragFloat2("Polyglot Position", min = -2000, max = 2000f)]
        [CollapseWith(0, 3)]
        public Vector2 PolyglotPosition = new Vector2(0, -67);

        [DragFloat2("Polyglot Size", max = 2000f)]
        [CollapseWith(5, 3)]
        public Vector2 PolyglotSize = new Vector2(38, 18);

        [DragInt("Polyglot Padding", min = -100, max = 100)]
        [CollapseWith(10, 3)]
        public int PolyglotPadding = 2;

        [ColorEdit4("Polyglot Color")]
        [CollapseWith(15, 3)]
        public PluginConfigColor PolyglotColor = new PluginConfigColor(new Vector4(234f / 255f, 95f / 255f, 155f / 255f, 100f / 100f));
        #endregion

        #region procs
        [Checkbox("Show Procs", separator = true)]
        [CollapseControl(50, 4)]
        public bool ShowProcs = true;

        [Checkbox("Show Firestarter Procs")]
        [CollapseWith(0, 4)]
        public bool ShowFirestarterProcs = true;

        [Checkbox("Show Thundercloud Procs")]
        [CollapseWith(5, 4)]
        public bool ShowThundercloudProcs = true;

        [Checkbox("Invert Procs Bar")]
        [CollapseWith(10, 4)]
        public bool InvertProcsBar = true;

        [DragFloat2("Procs Bar Position", min = -2000, max = 2000f)]
        [CollapseWith(15, 4)]
        public Vector2 ProcsBarPosition = new Vector2(-74, -67);

        [DragFloat2("Procs Bar Size", max = 2000f)]
        [CollapseWith(20, 4)]
        public Vector2 ProcsBarSize = new Vector2(106, 18);

        [ColorEdit4("Firestarter Color")]
        [CollapseWith(25, 4)]
        public PluginConfigColor FirestarterColor = new PluginConfigColor(new Vector4(255f / 255f, 136f / 255f, 0 / 255f, 90f / 100f));

        [ColorEdit4("Thundercloud Color")]
        [CollapseWith(30, 4)]
        public PluginConfigColor ThundercloudColor = new PluginConfigColor(new Vector4(240f / 255f, 163f / 255f, 255f / 255f, 90f / 100f));
        #endregion

        #region thunder dots
        [Checkbox("Show DoT Bar", separator = true)]
        [CollapseControl(55, 5)]
        public bool ShowDotBar = true;

        [Checkbox("Invert DoT Bar")]
        [CollapseWith(0, 5)]
        public bool InvertDoTBar = false;

        [DragFloat2("DoT Bar Position", min = -2000, max = 2000f)]
        [CollapseWith(5, 5)]
        public Vector2 DoTBarPosition = new Vector2(74, -67);

        [DragFloat2("DoT Bar Size", max = 2000f)]
        [CollapseWith(10, 5)]
        public Vector2 DoTBarSize = new Vector2(106, 18);

        [ColorEdit4("DoT Color")]
        [CollapseWith(15, 5)]
        public PluginConfigColor DotColor = new PluginConfigColor(new Vector4(67f / 255f, 187 / 255f, 255f / 255f, 90f / 100f));
        #endregion
    }
}
