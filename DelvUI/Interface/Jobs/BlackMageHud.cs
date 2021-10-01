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
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;

namespace DelvUI.Interface.Jobs
{
    public class BlackMageHud : JobHud
    {
        private new BlackMageConfig Config => (BlackMageConfig)Config;
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

            if (Config.AlwaysShowFirestarterProcs)
            {
                positions.Add(Config.Position + Config.FirestarterBarPosition);
                sizes.Add(Config.FirestarterBarSize);
            }

            if (Config.AlwaysShowFirestarterProcs)
            {
                positions.Add(Config.Position + Config.ThundercloudBarPosition);
                sizes.Add(Config.ThundercloudBarSize);
            }

            if (Config.ShowDotBar)
            {
                positions.Add(Config.Position + Config.DoTBarPosition);
                sizes.Add(Config.DoTBarSize);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            if (Config.ShowManaBar)
            {
                DrawManaBar(origin, player);
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
                DrawTripleCast(origin, player);
            }

            if (Config.ShowFirestarterProcs)
            {
                DrawFirestarterProcs(origin, player);
            }

            if (Config.ShowThundercloudProcs)
            {
                DrawThundercloudProcs(origin, player);
            }

            if (Config.ShowDotBar)
            {
                DrawDotTimer(origin, player);
            }
        }

        protected void DrawManaBar(Vector2 origin, PlayerCharacter player)
        {
            BLMGauge? gauge = Plugin.JobGauges.Get<BLMGauge>();

            Vector2 position = origin + Config.Position + Config.ManaBarPosition - Config.ManaBarSize / 2f;

            PluginConfigColor? color = gauge.InAstralFire ? Config.ManaBarFireColor : gauge.InUmbralIce ? Config.ManaBarIceColor : Config.ManaBarNoElementColor;

            BarBuilder? builder = BarBuilder.Create(position, Config.ManaBarSize)
                .AddInnerBar(player.CurrentMp, player.MaxMp, color)
                .SetBackgroundColor(EmptyColor.Base);

            // element timer
            if (gauge.InAstralFire || gauge.InUmbralIce)
            {
                int time = gauge.ElementTimeRemaining > 10 ? gauge.ElementTimeRemaining / 1000 + 1 : 0;
                builder.SetTextMode(BarTextMode.Single);
                builder.SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, $"{time,0}");
            }

            // enochian
            if (gauge.IsEnochianActive)
            {
                builder.SetGlowSize(2);
                builder.SetGlowColor(0x88FFFFFF);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
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
                string? text = $"{player.CurrentMp,0}";
                Vector2 textSize = ImGui.CalcTextSize(text);
                var textPos = new Vector2(
                    position.X + 2,
                    position.Y + Config.ManaBarSize.Y / 2f - textSize.Y / 2f
                );
                DrawHelper.DrawOutlinedText(text, textPos);
            }
        }

        protected void DrawUmbralHeartStacks(Vector2 origin)
        {
            BLMGauge? gauge = Plugin.JobGauges.Get<BLMGauge>();
            Vector2 position = origin + Config.Position + Config.UmbralHeartPosition - Config.UmbralHeartSize / 2f;

            Bar? bar = BarBuilder.Create(position, Config.UmbralHeartSize)
                                .SetChunks(3)
                                .SetChunkPadding(Config.UmbralHeartPadding)
                                .AddInnerBar(gauge.UmbralHearts, 3, Config.UmbralHeartColor, EmptyColor)
                                .SetBackgroundColor(EmptyColor.Base)
                                .Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        protected void DrawPolyglot(Vector2 origin)
        {
            BLMGauge? gauge = Plugin.JobGauges.Get<BLMGauge>();

            Vector2 position = origin + Config.Position + Config.PolyglotPosition - Config.PolyglotSize / 2f;

            int barWidth = (int)(Config.PolyglotSize.X - Config.PolyglotPadding) / 2;
            var barSize = new Vector2(barWidth, Config.PolyglotSize.Y);

            float scale = 1 - (gauge.IsEnochianActive ? gauge.EnochianTimer / 30000f : 1);
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            // 1
            BarBuilder? builder = BarBuilder.Create(position, barSize)
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

        protected void DrawTripleCast(Vector2 origin, PlayerCharacter player)
        {
            Dalamud.Game.ClientState.Statuses.Status? tripleStackBuff = player.StatusList.FirstOrDefault(o => o.StatusId == 1211);

            Vector2 position = origin + Config.Position + Config.TriplecastPosition - Config.TriplecastSize / 2f;

            Bar? bar = BarBuilder.Create(position, Config.TriplecastSize)
                                .SetChunks(3)
                                .SetChunkPadding(Config.TriplecastPadding)
                                .AddInnerBar(tripleStackBuff?.StackCount ?? 0, 3, Config.TriplecastColor, EmptyColor)
                                .SetBackgroundColor(EmptyColor.Base)
                                .Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        protected void DrawFirestarterProcs(Vector2 origin, PlayerCharacter player)
        {
            Dalamud.Game.ClientState.Statuses.StatusList? statusEffects = player.StatusList;
            float firestarterTimer = Config.ShowFirestarterProcs ? Math.Abs(statusEffects.FirstOrDefault(o => o.StatusId == 165)?.RemainingTime ?? 0f) : 0;

            DrawProc(
                origin,
                Config.FirestarterBarPosition,
                Config.FirestarterBarSize,
                firestarterTimer,
                18f,
                Config.InvertFirestarterBar,
                Config.AlwaysShowFirestarterProcs,
                Config.FirestarterColor
            );
        }

        protected void DrawThundercloudProcs(Vector2 origin, PlayerCharacter player)
        {
            Dalamud.Game.ClientState.Statuses.StatusList? statusEffects = player.StatusList;
            float thundercloudTimer = Config.ShowThundercloudProcs ? Math.Abs(statusEffects.FirstOrDefault(o => o.StatusId == 164)?.RemainingTime ?? 0f) : 0;

            DrawProc(
                origin,
                Config.ThundercloudBarPosition,
                Config.ThundercloudBarSize,
                thundercloudTimer,
                18f,
                Config.InvertThundercloudBar,
                Config.AlwaysShowThundercloudProcs,
                Config.ThundercloudColor
            );
        }

        protected void DrawProc(Vector2 origin, Vector2 position, Vector2 size, float timer, float maxDuration, bool invert, bool alwayShow, PluginConfigColor color)
        {
            if (timer == 0 && !alwayShow)
            {
                return;
            }

            Vector2 pos = origin + Config.Position + position - size / 2f;

            BarBuilder? builder = BarBuilder.Create(pos, size)
                .AddInnerBar(timer, 18f, color)
                .SetFlipDrainDirection(invert);

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        protected void DrawDotTimer(Vector2 origin, PlayerCharacter player)
        {
            GameObject? actor = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            float timer = 0;
            float maxDuration = 1;

            if (actor is BattleChara target)
            {
                // thunder 1 to 4
                int[] dotIDs = { 161, 162, 163, 1210 };
                float[] dotDurations = { 12, 18, 24, 18 };

                for (int i = 0; i < 4; i++)
                {
                    timer = target.StatusList.FirstOrDefault(o => o.StatusId == dotIDs[i] && o.SourceID == player.ObjectId)?.RemainingTime ?? 0f;

                    if (timer > 0)
                    {
                        maxDuration = dotDurations[i];

                        break;
                    }
                }
            }

            Vector2 position = origin + Config.Position + Config.DoTBarPosition - Config.DoTBarSize / 2f;

            BarBuilder? builder = BarBuilder.Create(position, Config.DoTBarSize)
                .AddInnerBar(timer, maxDuration, Config.DotColor)
                .SetFlipDrainDirection(Config.InvertDoTBar);

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Caster", 0)]
    [SubSection("Black Mage", 1)]
    public class BlackMageConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.BLM;
        public static new BlackMageConfig DefaultConfig() { return new BlackMageConfig(); }

        #region mana bar
        [Checkbox("Show Mana Bar", separator = true)]
        [Order(30)]
        public bool ShowManaBar = true;

        [DragFloat2("Mana Bar Position", min = -2000, max = 2000f)]
        [Order(35, collapseWith = nameof(ShowManaBar))]
        public Vector2 ManaBarPosition = new(0, -10);

        [DragFloat2("Mana Bar Size", max = 2000f)]
        [Order(40, collapseWith = nameof(ShowManaBar))]
        public Vector2 ManaBarSize = new(254, 20);

        [Checkbox("Show Mana Value")]
        [Order(45, collapseWith = nameof(ShowManaBar))]
        public bool ShowManaValue = false;

        [Checkbox("Show Mana Threshold Marker During Astral Fire")]
        [Order(50, collapseWith = nameof(ShowManaBar))]
        public bool ShowManaThresholdMarker = true;

        [DragInt("Mana Threshold Marker Value", max = 10000)]
        [Order(55, collapseWith = nameof(ShowManaBar))]
        public int ManaThresholdValue = 2400;

        [ColorEdit4("Mana Bar Color")]
        [Order(60, collapseWith = nameof(ShowManaBar))]
        public PluginConfigColor ManaBarNoElementColor = new(new Vector4(234f / 255f, 95f / 255f, 155f / 255f, 100f / 100f));

        [ColorEdit4("Mana Bar Ice Color")]
        [Order(65, collapseWith = nameof(ShowManaBar))]
        public PluginConfigColor ManaBarIceColor = new(new Vector4(69f / 255f, 115f / 255f, 202f / 255f, 100f / 100f));

        [ColorEdit4("Mana Bar Fire Color")]
        [Order(70, collapseWith = nameof(ShowManaBar))]
        public PluginConfigColor ManaBarFireColor = new(new Vector4(204f / 255f, 40f / 255f, 40f / 255f, 100f / 100f));
        #endregion

        #region umbral heart
        [Checkbox("Show Umbral Heart Bar", separator = true)]
        [Order(75)]
        public bool ShowUmbralHeart = true;

        [DragFloat2("Umbral Heart Bar Position", min = -2000, max = 2000f)]
        [Order(80, collapseWith = nameof(ShowUmbralHeart))]
        public Vector2 UmbralHeartPosition = new(0, -30);

        [DragFloat2("Umbral Heart Bar Size", max = 2000f)]
        [Order(85, collapseWith = nameof(ShowUmbralHeart))]
        public Vector2 UmbralHeartSize = new(254, 16);

        [DragInt("Umbral Heart Padding", min = -100, max = 100)]
        [Order(90, collapseWith = nameof(ShowUmbralHeart))]
        public int UmbralHeartPadding = 2;

        [ColorEdit4("Umbral Heart Color")]
        [Order(95, collapseWith = nameof(ShowUmbralHeart))]
        public PluginConfigColor UmbralHeartColor = new(new Vector4(125f / 255f, 195f / 255f, 205f / 255f, 100f / 100f));
        #endregion

        #region triple cast
        [Checkbox("Show Triplecast", separator = true)]
        [Order(100)]
        public bool ShowTriplecast = true;

        [DragFloat2("Triplecast Position", min = -2000, max = 2000f)]
        [Order(105, collapseWith = nameof(ShowTriplecast))]
        public Vector2 TriplecastPosition = new(0, -48);

        [DragFloat2("Triplecast Size", max = 2000)]
        [Order(110, collapseWith = nameof(ShowTriplecast))]
        public Vector2 TriplecastSize = new(254, 16);

        [DragInt("Trioplecast Padding", min = -100, max = 100)]
        [Order(115, collapseWith = nameof(ShowTriplecast))]
        public int TriplecastPadding = 2;

        [ColorEdit4("Triplecast Color")]
        [Order(120, collapseWith = nameof(ShowTriplecast))]
        public PluginConfigColor TriplecastColor = new(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));
        #endregion

        #region polyglot
        [Checkbox("Show Polyglot Stacks", separator = true)]
        [Order(125)]
        public bool ShowPolyglot = true;

        [DragFloat2("Polyglot Position", min = -2000, max = 2000f)]
        [Order(130, collapseWith = nameof(ShowPolyglot))]
        public Vector2 PolyglotPosition = new(0, -67);

        [DragFloat2("Polyglot Size", max = 2000f)]
        [Order(135, collapseWith = nameof(ShowPolyglot))]
        public Vector2 PolyglotSize = new(38, 18);

        [DragInt("Polyglot Padding", min = -100, max = 100)]
        [Order(140, collapseWith = nameof(ShowPolyglot))]
        public int PolyglotPadding = 2;

        [ColorEdit4("Polyglot Color")]
        [Order(145, collapseWith = nameof(ShowPolyglot))]
        public PluginConfigColor PolyglotColor = new(new Vector4(234f / 255f, 95f / 255f, 155f / 255f, 100f / 100f));
        #endregion

        #region firestarter
        [Checkbox("Show Firestarter Proc", separator = true)]
        [Order(150)]
        public bool ShowFirestarterProcs = true;

        [Checkbox("Always Show ##Firestarter")]
        [Order(155, collapseWith = nameof(ShowFirestarterProcs))]
        public bool AlwaysShowFirestarterProcs = true;

        [DragFloat2("Position ##Firestarter", min = -2000, max = 2000f)]
        [Order(160, collapseWith = nameof(ShowFirestarterProcs))]
        public Vector2 FirestarterBarPosition = new(-74, -72);

        [DragFloat2("Size ##Firestarter", max = 2000f)]
        [Order(165, collapseWith = nameof(ShowFirestarterProcs))]
        public Vector2 FirestarterBarSize = new(106, 8);

        [Checkbox("Invert ##Firestarter")]
        [Order(170, collapseWith = nameof(ShowFirestarterProcs))]
        public bool InvertFirestarterBar = true;

        [ColorEdit4("Color ##Firestarter")]
        [Order(175, collapseWith = nameof(ShowFirestarterProcs))]
        public PluginConfigColor FirestarterColor = new(new Vector4(255f / 255f, 136f / 255f, 0 / 255f, 90f / 100f));
        #endregion

        #region thundercloud
        [Checkbox("Show Thundercloud Proc", separator = true)]
        [Order(180)]
        public bool ShowThundercloudProcs = true;

        [Checkbox("Always Show ##Thundercloud")]
        [Order(185, collapseWith = nameof(ShowThundercloudProcs))]
        public bool AlwaysShowThundercloudProcs = true;

        [DragFloat2("Position ##Thundercloud", min = -2000, max = 2000f)]
        [Order(190, collapseWith = nameof(ShowThundercloudProcs))]
        public Vector2 ThundercloudBarPosition = new(-74, -62);

        [DragFloat2("Size ##Thundercloud", max = 2000f)]
        [Order(195, collapseWith = nameof(ShowThundercloudProcs))]
        public Vector2 ThundercloudBarSize = new(106, 8);

        [Checkbox("Invert ##Thundercloud")]
        [Order(200, collapseWith = nameof(ShowThundercloudProcs))]
        public bool InvertThundercloudBar = true;

        [ColorEdit4("Color ##Thundercloud")]
        [Order(205, collapseWith = nameof(ShowThundercloudProcs))]
        public PluginConfigColor ThundercloudColor = new(new Vector4(240f / 255f, 163f / 255f, 255f / 255f, 90f / 100f));
        #endregion

        #region thunder dots
        [Checkbox("Show DoT Bar", separator = true)]
        [Order(210)]
        public bool ShowDotBar = true;

        [Checkbox("Invert DoT Bar")]
        [Order(215, collapseWith = nameof(ShowDotBar))]
        public bool InvertDoTBar = false;

        [DragFloat2("DoT Bar Position", min = -2000, max = 2000f)]
        [Order(220, collapseWith = nameof(ShowDotBar))]
        public Vector2 DoTBarPosition = new(74, -67);

        [DragFloat2("DoT Bar Size", max = 2000f)]
        [Order(225, collapseWith = nameof(ShowDotBar))]
        public Vector2 DoTBarSize = new(106, 18);

        [ColorEdit4("DoT Color")]
        [Order(230, collapseWith = nameof(ShowDotBar))]
        public PluginConfigColor DotColor = new(new Vector4(67f / 255f, 187 / 255f, 255f / 255f, 90f / 100f));
        #endregion
    }
}
