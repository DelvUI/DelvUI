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

            if (gauge.Kenki == 0 && Config.OnlyShowKenkiWhenActive)
            {
                return;
            }

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
            var higanbana = target.StatusList.Where(o => o.StatusId is 1228 or 1319 && o.SourceID == actorId);
            var higanbanaDuration = 0f;
            if (higanbana.Any())
            {
                higanbanaDuration = Math.Abs(higanbana.First().RemainingTime);
            }

            if (higanbanaDuration == 0 && Config.OnlyShowHiganbanaWhenActive)
            {
                return;
            }

            var higanbanaColor = higanbanaDuration > 5 ? Config.HiganbanaColor : Config.HiganbanaExpiryColor;
            var pos = new Vector2(
                origin.X + Config.Position.X + Config.HiganbanaBarPosition.X - Config.HiganbanaBarSize.X / 2f,
                origin.Y + Config.Position.Y + Config.HiganbanaBarPosition.Y - Config.HiganbanaBarSize.Y / 2f
            );

            var higanbanaBuilder = BarBuilder.Create(pos, Config.HiganbanaBarSize)
                .SetBackgroundColor(EmptyColor.Base);

            if (higanbanaDuration > 0 && Config.ShowHiganbanaText)
            {
                higanbanaBuilder.AddInnerBar(higanbanaDuration, 60f, higanbanaColor).SetFlipDrainDirection(false)
                .SetTextMode(BarTextMode.Single)
                .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }
            else
            {
                higanbanaBuilder.AddInnerBar(higanbanaDuration, 60f, higanbanaColor)
                    .SetFlipDrainDirection(false);
            }

            var drawList = ImGui.GetWindowDrawList();
            higanbanaBuilder.Build().Draw(drawList);
        }

        private void DrawActiveBuffs(Vector2 origin, PlayerCharacter player)
        {
            var buffsSize = new Vector2(Config.BuffsBarSize.X / 2f - Config.BuffsPadding / 2f, Config.BuffsBarSize.Y);
            var order = Config.buffOrder;

            // shifu
            var shifu = player.StatusList.Where(o => o.StatusId is 1299).ToList();
            var shifuDuration = 0f;
            if (shifu.Any())
            {
                shifuDuration = Math.Abs(shifu.First().RemainingTime);
            }
            
            var shifuPos = new Vector2(
                origin.X + Config.Position.X + Config.BuffsBarPosition.X + (2 * order[0] - 1) * Config.BuffsBarSize.X / 2f - order[0] * buffsSize.X,
                origin.Y + Config.Position.Y + Config.BuffsBarPosition.Y - Config.BuffsBarSize.Y / 2f
            );
            var shifuBuilder = BarBuilder.Create(shifuPos, buffsSize)
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
            var jinpuDuration = 0f;
            if (jinpu.Any())
            {
                jinpuDuration = Math.Abs(jinpu.First().RemainingTime);
            }

            var jinpuPos = new Vector2(
                origin.X + Config.Position.X + Config.BuffsBarPosition.X + (2 * order[1] - 1) * Config.BuffsBarSize.X / 2f - order[1] * buffsSize.X,
                origin.Y + Config.Position.Y + Config.BuffsBarPosition.Y - Config.BuffsBarSize.Y / 2f
            );

            var jinpuBuilder = BarBuilder.Create(jinpuPos, buffsSize)
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

            var drawList = ImGui.GetWindowDrawList();
            shifuBuilder.Build().Draw(drawList);
            jinpuBuilder.Build().Draw(drawList);
        }

        private void DrawSenResourceBar(Vector2 origin)
        {
            var gauge = Plugin.JobGauges.Get<SAMGauge>();

            if (Config.OnlyShowSenWhenActive && !gauge.HasKa && !gauge.HasGetsu && !gauge.HasSetsu)
            {
                return;
            }

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

            if (Config.OnlyShowMeditationWhenActive && gauge.MeditationStacks == 0)
            {
                return;
            }

            var pos = new Vector2(
                origin.X + Config.Position.X + Config.MeditationBarPosition.X - Config.MeditationBarSize.X / 2f,
                origin.Y + Config.Position.Y + Config.MeditationBarPosition.Y - Config.MeditationBarSize.Y / 2f
            );

            var meditationBuilder = BarBuilder.Create(pos, Config.MeditationBarSize).SetBackgroundColor(EmptyColor.Base)
                .SetChunks(3)
                .SetChunkPadding(Config.MeditationBarPadding);

            if (gauge.MeditationStacks > 0)
            {
                meditationBuilder.AddInnerBar(gauge.MeditationStacks, 3, Config.MeditationColor);
            }

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
        [CollapseControl(30, 0)]
        public bool ShowKenkiBar = true;

        [Checkbox("Only Show When Active" + "##Kenki")]
        [CollapseWith(0, 0)]
        public bool OnlyShowKenkiWhenActive = false;

        [DragFloat2("Kenki Bar Size" + "##Kenki", max = 2000f)]
        [CollapseWith(5, 0)]
        public Vector2 KenkiBarSize = new Vector2(254, 20);

        [DragFloat2("Kenki Bar Position" + "##Kenki", min = -2000f, max = 2000f)]
        [CollapseWith(10, 0)]
        public Vector2 KenkiBarPosition = new Vector2(0, -34);

        [Checkbox("Show Kenki Text" + "##Kenki")]
        [CollapseWith(15, 0)]
        public bool ShowKenkiText = true;
        #endregion

        #region Sen
        [Checkbox("Show Sen Bar")]
        [CollapseControl(35, 1)]
        public bool ShowSenBar = true;

        [Checkbox("Only Show When Active" + "##Sen")]
        [CollapseWith(0, 1)]
        public bool OnlyShowSenWhenActive = false;

        [DragInt("Sen Bar Padding" + "##Sen", max = 1000)]
        [CollapseWith(5, 1)]
        public int SenBarPadding = 2;

        [DragFloat2("Sen Bar Size" + "##Sen", max = 2000f)]
        [CollapseWith(10, 1)]
        public Vector2 SenBarSize = new Vector2(254, 10);

        [DragFloat2("Sen Bar Position" + "##Sen", min = -2000f, max = 2000f)]
        [CollapseWith(15, 1)]
        public Vector2 SenBarPosition = new Vector2(0, -17);

        [DragDropHorizontal("Sen Order", "Setsu", "Getsu", "Ka" + "##Sen")]
        [CollapseWith(20, 1)]
        public int[] senOrder = new int[] { 0, 1, 2 };
        #endregion

        #region Meditation
        [Checkbox("Show Meditation Bar")]
        [CollapseControl(40, 2)]
        public bool ShowMeditationBar = true;

        [Checkbox("Only Show When Active" + "##Meditation")]
        [CollapseWith(0, 2)]
        public bool OnlyShowMeditationWhenActive = false;

        [DragInt("Meditation Bar Padding" + "##Meditation", max = 1000)]
        [CollapseWith(5, 2)]
        public int MeditationBarPadding = 2;

        [DragFloat2("Meditation Bar Size" + "##Meditation", max = 2000f)]
        [CollapseWith(10, 2)]
        public Vector2 MeditationBarSize = new Vector2(254, 10);

        [DragFloat2("Meditation Bar Position" + "##Meditation", min = -2000f, max = 2000f)]
        [CollapseWith(15, 2)]
        public Vector2 MeditationBarPosition = new Vector2(0, -5);
        #endregion

        #region Buffs
        [Checkbox("Show Buffs Bar")]
        [CollapseControl(45, 3)]
        public bool ShowBuffsBar = true;

        [Checkbox("Only Show When Active" + "##Buffs")]
        [CollapseWith(0, 3)]
        public bool OnlyShowBuffsWhenActive = false;

        [DragInt("Buffs Bar Padding" + "##Buffs", max = 1000)]
        [CollapseWith(5, 3)]
        public int BuffsPadding = 2;

        [DragFloat2("Buffs Bar Size" + "##Buffs", max = 2000f)]
        [CollapseWith(10, 3)]
        public Vector2 BuffsBarSize = new Vector2(254, 20);

        [DragFloat2("Buffs Bar Position" + "##Buffs", min = -2000f, max = 2000f)]
        [CollapseWith(15, 3)]
        public Vector2 BuffsBarPosition = new Vector2(0, -56);

        [Checkbox("Show Buffs Bar Text" + "##Buffs")]
        [CollapseWith(20, 3)]
        public bool ShowBuffsText = true;

        [DragDropHorizontal("Shifu/Jinpu Order", "Shifu", "Jinpu" + "##Buffs")]
        [CollapseWith(25, 3)]
        public int[] buffOrder = new int[] { 0, 1 };
        #endregion

        #region Higanbana
        [Checkbox("Show Higanbana Bar")]
        [CollapseControl(300, 4)]
        public bool ShowHiganbanaBar = true;

        [Checkbox("Only Show When Active" + "##Higanbana")]
        [CollapseWith(0, 4)]
        public bool OnlyShowHiganbanaWhenActive = false;

        [DragFloat2("Higanbana Bar Size" + "##Higanbana", max = 2000f)]
        [CollapseWith(5, 4)]
        public Vector2 HiganbanaBarSize = new Vector2(254, 20);

        [DragFloat2("Higanbana Bar Position" + "##Higanbana", min = -2000f, max = 2000f)]
        [CollapseWith(10, 4)]
        public Vector2 HiganbanaBarPosition = new Vector2(0, -78);

        [Checkbox("Show Higanbana Text" + "##Higanbana")]
        [CollapseWith(15, 4)]
        public bool ShowHiganbanaText = true;
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
