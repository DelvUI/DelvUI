using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
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
    public class SamuraiHud : JobHud
    {
        private new SamuraiConfig Config => (SamuraiConfig)Config;
        private PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;

        public SamuraiHud(string id, SamuraiConfig config, string? displayName = null) : base(id, config, displayName)
        {
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.ShowKenkiBar)
            {
                positions.Add(Config.Position + Config.KenkiBarPosition);
                sizes.Add(Config.KenkiBarSize);
            }

            if (Config.ShowSenBar)
            {
                positions.Add(Config.Position + Config.SenBarPosition);
                sizes.Add(Config.SenBarSize);
            }

            if (Config.ShowMeditationBar)
            {
                positions.Add(Config.Position + Config.MeditationBarPosition);
                sizes.Add(Config.MeditationBarSize);
            }

            if (Config.ShowBuffsBar)
            {
                positions.Add(Config.Position + Config.BuffsBarPosition);
                sizes.Add(Config.BuffsBarSize);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            if (Config.ShowKenkiBar)
            {
                DrawKenkiBar(origin);
            }

            if (Config.ShowSenBar)
            {
                DrawSenResourceBar(origin);
            }

            if (Config.ShowMeditationBar)
            {
                DrawMeditationResourceBar(origin);
            }

            if (Config.ShowBuffsBar)
            {
                DrawActiveBuffs(origin, player);
            }

            if (Config.ShowHiganbanaBar)
            {
                DrawHiganbanaBar(origin, player);
            }
        }

        private void DrawKenkiBar(Vector2 origin)
        {
            SAMGauge? gauge = Plugin.JobGauges.Get<SAMGauge>();
            var pos = new Vector2(
                origin.X + Config.Position.X + Config.KenkiBarPosition.X - Config.KenkiBarSize.X / 2f,
                origin.Y + Config.Position.Y + Config.KenkiBarPosition.Y - Config.KenkiBarSize.Y / 2f
            );

            if (gauge.Kenki == 0 && Config.OnlyShowKenkiWhenActive) { return; }

            BarBuilder? kenkiBuilder = BarBuilder.Create(pos, Config.KenkiBarSize)
                .SetBackgroundColor(EmptyColor.Base)
                .AddInnerBar(gauge.Kenki, 100, Config.KenkiColor);

            if (Config.ShowKenkiText && gauge.Kenki != 0)
            {
                kenkiBuilder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            kenkiBuilder.Build().Draw(drawList);
        }

        private void DrawHiganbanaBar(Vector2 origin, PlayerCharacter player)
        {
            GameObject? actor = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            if (actor is not BattleChara target)
            {
                return;
            }

            uint actorId = player.ObjectId;
            IEnumerable<Dalamud.Game.ClientState.Statuses.Status>? higanbana = target.StatusList.Where(o => o.StatusId is 1228 or 1319 && o.SourceID == actorId);
            float higanbanaDuration = higanbana.Any() ? Math.Abs(higanbana.First().RemainingTime) : 0f;

            if (higanbanaDuration == 0 && Config.OnlyShowHiganbanaWhenActive) { return; }

            PluginConfigColor? higanbanaColor = higanbanaDuration > 5 ? Config.HiganbanaColor : Config.HiganbanaExpiryColor;
            var pos = new Vector2(
                origin.X + Config.Position.X + Config.HiganbanaBarPosition.X - Config.HiganbanaBarSize.X / 2f,
                origin.Y + Config.Position.Y + Config.HiganbanaBarPosition.Y - Config.HiganbanaBarSize.Y / 2f
            );

            BarBuilder? higanbanaBuilder = BarBuilder.Create(pos, Config.HiganbanaBarSize)
                .SetBackgroundColor(EmptyColor.Base);

            if (higanbanaDuration > 0)
            {
                higanbanaBuilder.AddInnerBar(higanbanaDuration, 60f, higanbanaColor)
                    .SetFlipDrainDirection(false);

                if (Config.ShowHiganbanaText)
                {
                    higanbanaBuilder.SetTextMode(BarTextMode.Single)
                        .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            higanbanaBuilder.Build().Draw(drawList);
        }

        private void DrawActiveBuffs(Vector2 origin, PlayerCharacter player)
        {
            var buffsSize = new Vector2(Config.BuffsBarSize.X / 2f - Config.BuffsPadding / 2f, Config.BuffsBarSize.Y);
            int[]? order = Config.BuffOrder;

            // shifu
            var shifu = player.StatusList.Where(o => o.StatusId is 1299).ToList();
            float shifuDuration = shifu.Any() ? Math.Abs(shifu.First().RemainingTime) : 0f;

            var shifuPos = new Vector2(
                origin.X + Config.Position.X + Config.BuffsBarPosition.X + (2 * order[0] - 1) * Config.BuffsBarSize.X / 2f - order[0] * buffsSize.X,
                origin.Y + Config.Position.Y + Config.BuffsBarPosition.Y - Config.BuffsBarSize.Y / 2f
            );
            BarBuilder? shifuBuilder = BarBuilder.Create(shifuPos, buffsSize)
                .SetBackgroundColor(EmptyColor.Base);

            if (shifuDuration > 0)
            {
                shifuBuilder.AddInnerBar(shifuDuration, 40f, Config.ShifuColor)
                .SetFlipDrainDirection(true);

                if (Config.ShowBuffsText)
                {
                    shifuBuilder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
            }

            // jinpu
            var jinpu = player.StatusList.Where(o => o.StatusId is 1298).ToList();
            float jinpuDuration = jinpu.Any() ? Math.Abs(jinpu.First().RemainingTime) : 0f;

            var jinpuPos = new Vector2(
                origin.X + Config.Position.X + Config.BuffsBarPosition.X + (2 * order[1] - 1) * Config.BuffsBarSize.X / 2f - order[1] * buffsSize.X,
                origin.Y + Config.Position.Y + Config.BuffsBarPosition.Y - Config.BuffsBarSize.Y / 2f
            );

            BarBuilder? jinpuBuilder = BarBuilder.Create(jinpuPos, buffsSize)
                .SetBackgroundColor(EmptyColor.Base);

            if (jinpuDuration > 0)
            {
                jinpuBuilder.AddInnerBar(jinpuDuration, 40f, Config.JinpuColor)
                    .SetFlipDrainDirection(false);

                if (Config.ShowBuffsText)
                {
                    jinpuBuilder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
            }

            if (Config.OnlyShowBuffsWhenActive && jinpuDuration == 0 && shifuDuration == 0)
            {
                return;
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            shifuBuilder.Build().Draw(drawList);
            jinpuBuilder.Build().Draw(drawList);
        }

        private void DrawSenResourceBar(Vector2 origin)
        {
            SAMGauge? gauge = Plugin.JobGauges.Get<SAMGauge>();

            if (Config.OnlyShowSenWhenActive && !gauge.HasKa && !gauge.HasGetsu && !gauge.HasSetsu) { return; }

            float senBarWidth = (Config.SenBarSize.X - Config.SenBarPadding * 2) / 3f;
            var senBarSize = new Vector2(senBarWidth, Config.SenBarSize.Y);

            var cursorPos = new Vector2(
                origin.X + Config.Position.X + Config.SenBarPosition.X - Config.SenBarSize.X / 2f,
                origin.Y + Config.Position.Y + Config.SenBarPosition.Y - Config.SenBarSize.Y / 2f
            );
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            // setsu, getsu, ka
            int[]? order = Config.SenOrder;
            int[]? hasSen = new[] { gauge.HasSetsu ? 1 : 0, gauge.HasGetsu ? 1 : 0, gauge.HasKa ? 1 : 0 };
            PluginConfigColor[]? colors = new[] { Config.SetsuColor, Config.GetsuColor, Config.KaColor };

            for (int i = 0; i < 3; i++)
            {
                BarBuilder? builder = BarBuilder.Create(cursorPos, senBarSize).
                    AddInnerBar(hasSen[order[i]], 1, colors[order[i]]);

                builder.Build().Draw(drawList);
                cursorPos.X += senBarWidth + Config.SenBarPadding;
            }
        }

        private void DrawMeditationResourceBar(Vector2 origin)
        {
            SAMGauge? gauge = Plugin.JobGauges.Get<SAMGauge>();

            if (Config.OnlyShowMeditationWhenActive && gauge.MeditationStacks == 0) { return; }

            var pos = new Vector2(
                origin.X + Config.Position.X + Config.MeditationBarPosition.X - Config.MeditationBarSize.X / 2f,
                origin.Y + Config.Position.Y + Config.MeditationBarPosition.Y - Config.MeditationBarSize.Y / 2f
            );

            BarBuilder? meditationBuilder = BarBuilder.Create(pos, Config.MeditationBarSize).SetBackgroundColor(EmptyColor.Base)
                .SetChunks(3)
                .SetChunkPadding(Config.MeditationBarPadding);

            if (gauge.MeditationStacks > 0)
            {
                meditationBuilder.AddInnerBar(gauge.MeditationStacks, 3, Config.MeditationColor);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            meditationBuilder.Build().Draw(drawList);
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Melee", 0)]
    [SubSection("Samurai", 1)]
    public class SamuraiConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.SAM;
        public static new SamuraiConfig DefaultConfig() { return new SamuraiConfig(); }

        #region Kenki
        [Checkbox("Kenki", separator = true)]
        [Order(30)]
        public bool ShowKenkiBar = true;

        [Checkbox("Only Show When Active" + "##Kenki")]
        [Order(35, collapseWith = nameof(ShowKenkiBar))]
        public bool OnlyShowKenkiWhenActive = false;

        [Checkbox("Text" + "##Kenki")]
        [Order(40, collapseWith = nameof(ShowKenkiBar))]
        public bool ShowKenkiText = true;

        [DragFloat2("Position" + "##Kenki", min = -2000f, max = 2000f)]
        [Order(45, collapseWith = nameof(ShowKenkiBar))]
        public Vector2 KenkiBarPosition = new(0, -34);

        [DragFloat2("Size" + "##Kenki", max = 2000f)]
        [Order(50, collapseWith = nameof(ShowKenkiBar))]
        public Vector2 KenkiBarSize = new(254, 20);

        [ColorEdit4("Color" + "##Kenki")]
        [Order(55, collapseWith = nameof(ShowKenkiBar))]
        public PluginConfigColor KenkiColor = new(new(255f / 255f, 82f / 255f, 82f / 255f, 53f / 100f));
        #endregion

        #region Sen
        [Checkbox("Sen", separator = true)]
        [Order(60)]
        public bool ShowSenBar = true;

        [Checkbox("Only Show When Active" + "##Sen")]
        [Order(65, collapseWith = nameof(ShowSenBar))]
        public bool OnlyShowSenWhenActive = false;

        [DragFloat2("Position" + "##Sen", min = -2000f, max = 2000f)]
        [Order(70, collapseWith = nameof(ShowSenBar))]
        public Vector2 SenBarPosition = new(0, -17);

        [DragFloat2("Size" + "##Sen", max = 2000f)]
        [Order(75, collapseWith = nameof(ShowSenBar))]
        public Vector2 SenBarSize = new(254, 10);

        [DragInt("Spacing" + "##Sen", max = 1000)]
        [Order(80, collapseWith = nameof(ShowSenBar))]
        public int SenBarPadding = 2;

        [ColorEdit4("Setsu" + "##Sen")]
        [Order(85, collapseWith = nameof(ShowSenBar))]
        public PluginConfigColor SetsuColor = new(new(89f / 255f, 234f / 255f, 247f / 255f, 100f / 100f));

        [ColorEdit4("Getsu" + "##Sen")]
        [Order(90, collapseWith = nameof(ShowSenBar))]
        public PluginConfigColor GetsuColor = new(new(89f / 255f, 126f / 255f, 247f / 255f, 100f / 100f));

        [ColorEdit4("Ka" + "##Sen")]
        [Order(95, collapseWith = nameof(ShowSenBar))]
        public PluginConfigColor KaColor = new(new(247f / 255f, 89f / 255f, 89f / 255f, 100f / 100f));

        [DragDropHorizontal("Order", "Setsu", "Getsu", "Ka" + "##Sen")]
        [Order(100, collapseWith = nameof(ShowSenBar))]
        public int[] SenOrder = new int[] { 0, 1, 2 };
        #endregion

        #region Meditation
        [Checkbox("Meditation", separator = true)]
        [Order(105)]
        public bool ShowMeditationBar = true;

        [Checkbox("Only Show When Active" + "##Meditation")]
        [Order(110, collapseWith = nameof(ShowMeditationBar))]
        public bool OnlyShowMeditationWhenActive = false;

        [DragFloat2("Position" + "##Meditation", min = -2000f, max = 2000f)]
        [Order(115, collapseWith = nameof(ShowMeditationBar))]
        public Vector2 MeditationBarPosition = new(0, -5);

        [DragFloat2("Size" + "##Meditation", max = 2000f)]
        [Order(120, collapseWith = nameof(ShowMeditationBar))]
        public Vector2 MeditationBarSize = new(254, 10);

        [DragInt("Spacing" + "##Meditation", max = 1000)]
        [Order(125, collapseWith = nameof(ShowMeditationBar))]
        public int MeditationBarPadding = 2;

        [ColorEdit4("Color" + "##Meditation")]
        [Order(130, collapseWith = nameof(ShowMeditationBar))]
        public PluginConfigColor MeditationColor = new(new(247f / 255f, 163f / 255f, 89f / 255f, 100f / 100f));
        #endregion

        #region Buffs
        [Checkbox("Buffs", separator = true)]
        [Order(135)]
        public bool ShowBuffsBar = true;

        [Checkbox("Only Show When Active" + "##Buffs")]
        [Order(140, collapseWith = nameof(ShowBuffsBar))]
        public bool OnlyShowBuffsWhenActive = false;

        [Checkbox("Text" + "##Buffs")]
        [Order(145, collapseWith = nameof(ShowBuffsBar))]
        public bool ShowBuffsText = true;

        [DragFloat2("Position" + "##Buffs", min = -2000f, max = 2000f)]
        [Order(150, collapseWith = nameof(ShowBuffsBar))]
        public Vector2 BuffsBarPosition = new(0, -56);

        [DragFloat2("Size" + "##Buffs", max = 2000f)]
        [Order(155, collapseWith = nameof(ShowBuffsBar))]
        public Vector2 BuffsBarSize = new(254, 20);

        [DragInt("Spacing" + "##Buffs", max = 1000)]
        [Order(160, collapseWith = nameof(ShowBuffsBar))]
        public int BuffsPadding = 2;

        [ColorEdit4("Shifu" + "##Buffs")]
        [Order(165, collapseWith = nameof(ShowBuffsBar))]
        public PluginConfigColor ShifuColor = new(new(219f / 255f, 211f / 255f, 136f / 255f, 100f / 100f));

        [ColorEdit4("Jinpu" + "##Buffs")]
        [Order(170, collapseWith = nameof(ShowBuffsBar))]
        public PluginConfigColor JinpuColor = new(new(136f / 255f, 146f / 255f, 219f / 255f, 100f / 100f));

        [DragDropHorizontal("Order", "Shifu", "Jinpu" + "##Buffs")]
        [Order(175, collapseWith = nameof(ShowBuffsBar))]
        public int[] BuffOrder = new int[] { 0, 1 };
        #endregion

        #region Higanbana
        [Checkbox("Higanbana", separator = true)]
        [Order(180)]
        public bool ShowHiganbanaBar = true;

        [Checkbox("Only Show When Active" + "##Higanbana")]
        [Order(185, collapseWith = nameof(ShowHiganbanaBar))]
        public bool OnlyShowHiganbanaWhenActive = false;

        [Checkbox("Timer" + "##Higanbana")]
        [Order(190, collapseWith = nameof(ShowHiganbanaBar))]
        public bool ShowHiganbanaText = true;

        [DragFloat2("Position" + "##Higanbana", min = -2000f, max = 2000f)]
        [Order(195, collapseWith = nameof(ShowHiganbanaBar))]
        public Vector2 HiganbanaBarPosition = new(0, -78);

        [DragFloat2("Size" + "##Higanbana", max = 2000f)]
        [Order(200, collapseWith = nameof(ShowHiganbanaBar))]
        public Vector2 HiganbanaBarSize = new(254, 20);

        [ColorEdit4("Color" + "##Higanbana")]
        [Order(205, collapseWith = nameof(ShowHiganbanaBar))]
        public PluginConfigColor HiganbanaColor = new(new(237f / 255f, 141f / 255f, 7f / 255f, 100f / 100f));

        [ColorEdit4("Expiry Color" + "##Higanbana")]
        [Order(210, collapseWith = nameof(ShowHiganbanaBar))]
        public PluginConfigColor HiganbanaExpiryColor = new(new(230f / 255f, 33f / 255f, 33f / 255f, 53f / 100f));
        #endregion

    }
}
