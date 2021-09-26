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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.Jobs
{
    public class SamuraiHud : JobHud
    {
        private new SamuraiConfig Config => (SamuraiConfig)_config;
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
            var gauge = Plugin.JobGauges.Get<SAMGauge>();
            var pos = new Vector2(
                origin.X + Config.Position.X + Config.KenkiBarPosition.X - Config.KenkiBarSize.X / 2f,
                origin.Y + Config.Position.Y + Config.KenkiBarPosition.Y - Config.KenkiBarSize.Y / 2f
            );

            var kenkiBuilder = BarBuilder.Create(pos, Config.KenkiBarSize)
                .SetBackgroundColor(EmptyColor.Base)
                .AddInnerBar(gauge.Kenki, 100, Config.KenkiColor);

            if (Config.ShowKenkiText)
            {
                kenkiBuilder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            var drawList = ImGui.GetWindowDrawList();
            kenkiBuilder.Build().Draw(drawList);
        }

        private void DrawHiganbanaBar(Vector2 origin, PlayerCharacter player)
        {
            var actor = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            if (actor is not BattleChara target)
            {
                return;
            }

            var actorId = player.ObjectId;
            var higanbana = target.StatusList.FirstOrDefault(o => o.StatusId == 1228 && o.SourceID == actorId || o.StatusId == 1319 && o.SourceID == actorId);
            var higanbanaDuration = higanbana?.RemainingTime ?? 0f;

            if (higanbanaDuration == 0)
            {
                return;
            }

            var higanbanaColor = higanbanaDuration > 5 ? Config.HiganbanaColor : Config.HiganbanaExpiryColor;
            var pos = new Vector2(
                origin.X + Config.Position.X + Config.HiganbanaBarPosition.X - Config.HiganbanaBarSize.X / 2f,
                origin.Y + Config.Position.Y + Config.HiganbanaBarPosition.Y - Config.HiganbanaBarSize.Y / 2f
            );

            var higanbanaBuilder = BarBuilder.Create(pos, Config.HiganbanaBarSize)
                .SetBackgroundColor(EmptyColor.Base)
                .AddInnerBar(higanbanaDuration, 60f, higanbanaColor).SetFlipDrainDirection(false);

            if (Config.ShowHiganbanaText)
            {
                higanbanaBuilder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }
            var drawList = ImGui.GetWindowDrawList();
            higanbanaBuilder.Build().Draw(drawList);
        }

        private void DrawActiveBuffs(Vector2 origin, PlayerCharacter player)
        {
            var buffsSize = new Vector2(Config.BuffsBarSize.X / 2f - Config.BuffsPadding / 2f, Config.BuffsBarSize.Y);
            var order = Config.buffOrder;

            // shifu
            var shifu = player.StatusList.FirstOrDefault(o => o.StatusId == 1299);
            var shifuDuration = shifu?.RemainingTime ?? 0f;
            var shifuPos = new Vector2(
                origin.X + Config.Position.X + Config.BuffsBarPosition.X + (2 * order[0] - 1) * Config.BuffsBarSize.X / 2f - order[0] * buffsSize.X,
                origin.Y + Config.Position.Y + Config.BuffsBarPosition.Y - Config.BuffsBarSize.Y / 2f
            );
            var shifuBuilder = BarBuilder.Create(shifuPos, buffsSize)
                .SetBackgroundColor(EmptyColor.Base)
                .AddInnerBar(shifuDuration, 40f, Config.ShifuColor)
                .SetFlipDrainDirection(true);

            // jinpu
            var jinpu = player.StatusList.FirstOrDefault(o => o.StatusId == 1298);
            var jinpuDuration = jinpu?.RemainingTime ?? 0f;
            var jinpuPos = new Vector2(
                origin.X + Config.Position.X + Config.BuffsBarPosition.X + (2 * order[1] - 1) * Config.BuffsBarSize.X / 2f - order[1] * buffsSize.X,
                origin.Y + Config.Position.Y + Config.BuffsBarPosition.Y - Config.BuffsBarSize.Y / 2f
            );
            var jinpuBuilder = BarBuilder.Create(jinpuPos, buffsSize)
                .SetBackgroundColor(EmptyColor.Base)
                .AddInnerBar(jinpuDuration, 40f, Config.JinpuColor)
                .SetFlipDrainDirection(false);

            if (Config.ShowBuffsText)
            {
                shifuBuilder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                jinpuBuilder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            var drawList = ImGui.GetWindowDrawList();
            shifuBuilder.Build().Draw(drawList);
            jinpuBuilder.Build().Draw(drawList);
        }

        private void DrawSenResourceBar(Vector2 origin)
        {
            var gauge = Plugin.JobGauges.Get<SAMGauge>();
            var senBarWidth = (Config.SenBarSize.X - Config.SenBarPadding * 2) / 3f;
            var senBarSize = new Vector2(senBarWidth, Config.SenBarSize.Y);

            var cursorPos = new Vector2(
                origin.X + Config.Position.X + Config.SenBarPosition.X - Config.SenBarSize.X / 2f,
                origin.Y + Config.Position.Y + Config.SenBarPosition.Y - Config.SenBarSize.Y / 2f
            );
            var drawList = ImGui.GetWindowDrawList();

            // setsu, getsu, ka
            var order = Config.senOrder;
            var hasSen = new[] { gauge.HasSetsu ? 1 : 0, gauge.HasGetsu ? 1 : 0, gauge.HasKa ? 1 : 0 };
            var colors = new[] { Config.SetsuColor, Config.GetsuColor, Config.KaColor };

            for (int i = 0; i < 3; i++)
            {
                var builder = BarBuilder.Create(cursorPos, senBarSize).
                    AddInnerBar(hasSen[order[i]], 1, colors[order[i]]);

                builder.Build().Draw(drawList);
                cursorPos.X += senBarWidth + Config.SenBarPadding;
            }
        }

        private void DrawMeditationResourceBar(Vector2 origin)
        {
            var gauge = Plugin.JobGauges.Get<SAMGauge>();

            var pos = new Vector2(
                origin.X + Config.Position.X + Config.MeditationBarPosition.X - Config.MeditationBarSize.X / 2f,
                origin.Y + Config.Position.Y + Config.MeditationBarPosition.Y - Config.MeditationBarSize.Y / 2f
            );

            var meditationBuilder = BarBuilder.Create(pos, Config.MeditationBarSize)
                .SetChunks(3)
                .SetBackgroundColor(EmptyColor.Base)
                .SetChunkPadding(Config.MeditationBarPadding)
                .AddInnerBar(gauge.MeditationStacks, 3, Config.MeditationColor);

            var drawList = ImGui.GetWindowDrawList();
            meditationBuilder.Build().Draw(drawList);
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Melee", 0)]
    [SubSection("Samurai", 1)]
    public class SamuraiConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.SAM;
        public new static SamuraiConfig DefaultConfig() { return new SamuraiConfig(); }

        #region Kenki
        [Checkbox("Show Kenki Bar")]
        [Order(30)]
        public bool ShowKenkiBar = true;

        [DragFloat2("Kenki Bar Size", max = 2000f)]
        [Order(35, collapseWith = nameof(ShowKenkiBar))]
        public Vector2 KenkiBarSize = new Vector2(254, 20);

        [DragFloat2("Kenki Bar Position", min = -2000f, max = 2000f)]
        [Order(40, collapseWith = nameof(ShowKenkiBar))]
        public Vector2 KenkiBarPosition = new Vector2(0, -34);

        [Checkbox("Show Kenki Text")]
        [Order(45, collapseWith = nameof(ShowKenkiBar))]
        public bool ShowKenkiText = true;
        #endregion

        #region Sen
        [Checkbox("Show Sen Bar")]
        [Order(50)]
        public bool ShowSenBar = true;

        [DragInt("Sen Bar Padding", max = 1000)]
        [Order(55, collapseWith = nameof(ShowSenBar))]
        public int SenBarPadding = 2;

        [DragFloat2("Sen Bar Size", max = 2000f)]
        [Order(60, collapseWith = nameof(ShowSenBar))]
        public Vector2 SenBarSize = new Vector2(254, 10);

        [DragFloat2("Sen Bar Position", min = -2000f, max = 2000f)]
        [Order(65, collapseWith = nameof(ShowSenBar))]
        public Vector2 SenBarPosition = new Vector2(0, -17);

        [DragDropHorizontal("Sen Order", "Setsu", "Getsu", "Ka")]
        [Order(70, collapseWith = nameof(ShowSenBar))]
        public int[] senOrder = new int[] { 0, 1, 2 };
        #endregion

        #region Meditation
        [Checkbox("Show Meditation Bar")]
        [Order(75)]
        public bool ShowMeditationBar = true;

        [DragInt("Meditation Bar Padding", max = 1000)]
        [Order(80, collapseWith = nameof(ShowMeditationBar))]
        public int MeditationBarPadding = 2;

        [DragFloat2("Meditation Bar Size", max = 2000f)]
        [Order(85, collapseWith = nameof(ShowMeditationBar))]
        public Vector2 MeditationBarSize = new Vector2(254, 10);

        [DragFloat2("Meditation Bar Position", min = -2000f, max = 2000f)]
        [Order(90, collapseWith = nameof(ShowMeditationBar))]
        public Vector2 MeditationBarPosition = new Vector2(0, -5);
        #endregion

        #region Buffs
        [Checkbox("Show Buffs Bar")]
        [Order(95)]
        public bool ShowBuffsBar = true;

        [DragInt("Buffs Bar Padding", max = 1000)]
        [Order(100, collapseWith = nameof(ShowBuffsBar))]
        public int BuffsPadding = 2;

        [DragFloat2("Buffs Bar Size", max = 2000f)]
        [Order(105, collapseWith = nameof(ShowBuffsBar))]
        public Vector2 BuffsBarSize = new Vector2(254, 20);

        [DragFloat2("Buffs Bar Position", min = -2000f, max = 2000f)]
        [Order(110, collapseWith = nameof(ShowBuffsBar))]
        public Vector2 BuffsBarPosition = new Vector2(0, -56);

        [Checkbox("Show Buffs Bar Text")]
        [Order(115, collapseWith = nameof(ShowBuffsBar))]
        public bool ShowBuffsText = true;

        [DragDropHorizontal("Shifu/Jinpu Order", "Shifu", "Jinpu")]
        [Order(120, collapseWith = nameof(ShowBuffsBar))]
        public int[] buffOrder = new int[] { 0, 1 };

        #endregion

        #region Higanbana
        [Checkbox("Show Higanbana Bar")]
        [Order(125)]
        public bool ShowHiganbanaBar = true;

        [DragFloat2("Higanbana Bar Size", max = 2000f)]
        [Order(130, collapseWith = nameof(ShowHiganbanaBar))]
        public Vector2 HiganbanaBarSize = new Vector2(254, 20);

        [DragFloat2("Higanbana Bar Position", min = -2000f, max = 2000f)]
        [Order(135, collapseWith = nameof(ShowHiganbanaBar))]
        public Vector2 HiganbanaBarPosition = new Vector2(0, -78);

        [Checkbox("Show Higanbana Text")]
        [Order(140, collapseWith = nameof(ShowHiganbanaBar))]
        public bool ShowHiganbanaText = true;
        #endregion

        #region BarOrders



        #endregion

        #region colors
        [ColorEdit4("Kenki Bar Color")]
        [Order(55)]
        public PluginConfigColor KenkiColor = new PluginConfigColor(new(255f / 255f, 82f / 255f, 82f / 255f, 53f / 100f));

        [ColorEdit4("Setsu Color")]
        [Order(60)]
        public PluginConfigColor SetsuColor = new PluginConfigColor(new(89f / 255f, 234f / 255f, 247f / 255f, 100f / 100f));

        [ColorEdit4("Getsu Color")]
        [Order(65)]
        public PluginConfigColor GetsuColor = new PluginConfigColor(new(89f / 255f, 126f / 255f, 247f / 255f, 100f / 100f));

        [ColorEdit4("Ka Color")]
        [Order(70)]
        public PluginConfigColor KaColor = new PluginConfigColor(new(247f / 255f, 89f / 255f, 89f / 255f, 100f / 100f));

        [ColorEdit4("Meditation Color")]
        [Order(75)]
        public PluginConfigColor MeditationColor = new PluginConfigColor(new(247f / 255f, 163f / 255f, 89f / 255f, 100f / 100f));

        [ColorEdit4("Shifu Color")]
        [Order(80)]
        public PluginConfigColor ShifuColor = new PluginConfigColor(new(219f / 255f, 211f / 255f, 136f / 255f, 100f / 100f));

        [ColorEdit4("Jinpu Color")]
        [Order(85)]
        public PluginConfigColor JinpuColor = new PluginConfigColor(new(136f / 255f, 146f / 255f, 219f / 255f, 100f / 100f));

        [ColorEdit4("Higanbana Color")]
        [Order(90)]
        public PluginConfigColor HiganbanaColor = new PluginConfigColor(new(237f / 255f, 141f / 255f, 7f / 255f, 100f / 100f));

        [ColorEdit4("Higanbana Expiry Color")]
        [Order(95)]
        public PluginConfigColor HiganbanaExpiryColor = new PluginConfigColor(new(230f / 255f, 33f / 255f, 33f / 255f, 53f / 100f));
        #endregion
    }
}
