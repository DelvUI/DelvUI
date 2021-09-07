using Dalamud.Game.ClientState.Actors.Types;
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
    public class SamuraiHudWindow : HudWindow
    {
        private SamuraiHudConfig _config => (SamuraiHudConfig)ConfigurationManager.GetInstance().GetConfiguration(new SamuraiHudConfig());

        public SamuraiHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        public override uint JobId => Jobs.SAM;

        private int OriginX => (int)(CenterX + _config.Position.X);
        private int OriginY => (int)(CenterY + _config.Position.Y);

        private Dictionary<string, uint> EmptyColor => PluginConfiguration.MiscColorMap["empty"];


        protected override void Draw(bool _)
        {
            if (_config.ShowKenkiBar)
            {
                DrawKenkiBar();
            }

            if (_config.ShowSenBar)
            {
                DrawSenResourceBar();
            }

            if (_config.ShowMeditationBar)
            {
                DrawMeditationResourceBar();
            }

            if (_config.ShowBuffsBar)
            {
                DrawActiveBuffs();
            }

            if (_config.ShowHiganbanaBar)
            {
                DrawHiganbanaBar();
            }
        }

        protected override void DrawPrimaryResourceBar() { }

        private void DrawKenkiBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<SAMGauge>();
            var pos = new Vector2(
                OriginX + _config.KenkiBarPosition.X - _config.KenkiBarSize.X / 2f,
                OriginY + _config.KenkiBarPosition.Y - _config.KenkiBarSize.Y / 2f
            );

            var kenkiBuilder = BarBuilder.Create(pos, _config.KenkiBarSize)
                .SetBackgroundColor(EmptyColor["background"])
                .AddInnerBar(gauge.Kenki, 100, _config.KenkiColor.Map);

            if (_config.ShowKenkiText)
            {
                kenkiBuilder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            var drawList = ImGui.GetWindowDrawList();
            kenkiBuilder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawHiganbanaBar()
        {
            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;
            if (target is not Chara)
            {
                return;
            }

            var actorId = PluginInterface.ClientState.LocalPlayer.ActorId;
            var higanbana = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1228 && o.OwnerId == actorId || o.EffectId == 1319 && o.OwnerId == actorId);
            var higanbanaDuration = higanbana.Duration;
            if (higanbanaDuration == 0)
            {
                return;
            }

            var higanbanaColor = higanbanaDuration > 5 ? _config.HiganbanaColor.Map : _config.HiganbanaExpiryColor.Map;
            var pos = new Vector2(
                OriginX + _config.HiganbanaBarPosition.X - _config.HiganbanaBarSize.X / 2f,
                OriginY + _config.HiganbanaBarPosition.Y - _config.HiganbanaBarSize.Y / 2f
            );

            var higanbanaBuilder = BarBuilder.Create(pos, _config.HiganbanaBarSize)
                .SetBackgroundColor(EmptyColor["background"])
                .AddInnerBar(higanbanaDuration, 60f, higanbanaColor).SetFlipDrainDirection(false);

            if (_config.ShowHiganbanaText)
            {
                higanbanaBuilder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }
            var drawList = ImGui.GetWindowDrawList();
            higanbanaBuilder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawActiveBuffs()
        {
            var target = PluginInterface.ClientState.LocalPlayer;
            var buffsSize = new Vector2(_config.BuffsBarSize.X / 2f - _config.BuffsPadding / 2f, _config.BuffsBarSize.Y);

            // shifu
            var shifu = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1299);
            var shifuDuration = shifu.Duration;
            var shifuPos = new Vector2(
                OriginX + _config.BuffsBarPosition.X - _config.BuffsBarSize.X / 2f,
                OriginY + _config.BuffsBarPosition.Y - _config.BuffsBarSize.Y / 2f
            );
            var shifuBuilder = BarBuilder.Create(shifuPos, buffsSize)
                .SetBackgroundColor(EmptyColor["background"])
                .AddInnerBar(shifuDuration, 40f, _config.ShifuColor.Map)
                .SetFlipDrainDirection(true);

            // jinpu
            var jinpu = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1298);
            var jinpuDuration = jinpu.Duration;
            var jinpuPos = new Vector2(
                OriginX + _config.BuffsBarPosition.X + _config.BuffsBarSize.X / 2f - buffsSize.X,
                OriginY + _config.BuffsBarPosition.Y - _config.BuffsBarSize.Y / 2f
            );
            var jinpuBuilder = BarBuilder.Create(jinpuPos, buffsSize)
                .SetBackgroundColor(EmptyColor["background"])
                .AddInnerBar(jinpuDuration, 40f, _config.JinpuColor.Map)
                .SetFlipDrainDirection(false);

            if (_config.ShowBuffsText)
            {
                shifuBuilder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                jinpuBuilder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            var drawList = ImGui.GetWindowDrawList();
            shifuBuilder.Build().Draw(drawList, PluginConfiguration);
            jinpuBuilder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawSenResourceBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<SAMGauge>();
            var senBarWidth = (_config.SenBarSize.X - _config.SenBarPadding * 2) / 3f;
            var senBarSize = new Vector2(senBarWidth, _config.SenBarSize.Y);

            var cursorPos = new Vector2(
                OriginX + _config.SenBarPosition.X - _config.SenBarSize.X / 2f,
                OriginY + _config.SenBarPosition.Y - _config.SenBarSize.Y / 2f
            );
            var drawList = ImGui.GetWindowDrawList();

            // setsu, getsu, ka
            var hasSen = new int[] { gauge.HasSetsu() ? 1 : 0, gauge.HasGetsu() ? 1 : 0, gauge.HasKa() ? 1 : 0 };
            var colors = new PluginConfigColor[] { _config.SetsuColor, _config.GetsuColor, _config.KaColor };

            for (int i = 0; i < 3; i++)
            {
                var builder = BarBuilder.Create(cursorPos, senBarSize).
                    AddInnerBar(hasSen[i], 1, colors[i].Map);

                builder.Build().Draw(drawList, PluginConfiguration);
                cursorPos.X += senBarWidth + _config.SenBarPadding;
            }
        }

        private void DrawMeditationResourceBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<SAMGauge>();

            var pos = new Vector2(
                OriginX + _config.MeditationBarPosition.X - _config.MeditationBarSize.X / 2f,
                OriginY + _config.MeditationBarPosition.Y - _config.MeditationBarSize.Y / 2f
            );

            var meditationBuilder = BarBuilder.Create(pos, _config.MeditationBarSize)
                .SetChunks(3)
                .SetBackgroundColor(EmptyColor["background"])
                .SetChunkPadding(_config.MeditationBarPadding)
                .AddInnerBar(gauge.MeditationStacks, 3, _config.MeditationColor.Map);

            var drawList = ImGui.GetWindowDrawList();
            meditationBuilder.Build().Draw(drawList, PluginConfiguration);
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Melee", 0)]
    [SubSection("Samurai", 1)]
    public class SamuraiHudConfig : PluginConfigObject
    {
        [DragFloat2("Base Position", min = -4000f, max = 4000f)]
        [Order(0)]
        public Vector2 Position = new(0, 0);

        #region Kenki
        [Checkbox("Show Kenki Bar")]
        [CollapseControl(5, 0)]
        public bool ShowKenkiBar = true;

        [DragFloat2("Kenki Bar Size", max = 2000f)]
        [CollapseWith(0, 0)]
        public Vector2 KenkiBarSize = new Vector2(254, 20);

        [DragFloat2("Kenki Bar Position", min = -2000f, max = 2000f)]
        [CollapseWith(5, 0)]
        public Vector2 KenkiBarPosition = new Vector2(0, 425);

        [Checkbox("Show Kenki Text")]
        [CollapseWith(10, 0)]
        public bool ShowKenkiText = true;
        #endregion

        #region Sen
        [Checkbox("Show Sen Bar")]
        [CollapseControl(10, 1)]
        public bool ShowSenBar = true;

        [DragInt("Sen Bar Padding", max = 1000)]
        [CollapseWith(0, 1)]
        public int SenBarPadding = 2;

        [DragFloat2("Sen Bar Size", max = 2000f)]
        [CollapseWith(5, 1)]
        public Vector2 SenBarSize = new Vector2(254, 10);

        [DragFloat2("Sen Bar Position", min = -2000f, max = 2000f)]
        [CollapseWith(10, 1)]
        public Vector2 SenBarPosition = new Vector2(0, 442);
        #endregion

        #region Meditation
        [Checkbox("Show Meditation Bar")]
        [CollapseControl(15, 2)]
        public bool ShowMeditationBar = true;

        [DragInt("Meditation Bar Padding", max = 1000)]
        [CollapseWith(0, 2)]
        public int MeditationBarPadding = 2;

        [DragFloat2("Meditation Bar Size", max = 2000f)]
        [CollapseWith(5, 2)]
        public Vector2 MeditationBarSize = new Vector2(254, 10);

        [DragFloat2("Meditation Bar Position", min = -2000f, max = 2000f)]
        [CollapseWith(10, 2)]
        public Vector2 MeditationBarPosition = new Vector2(0, 454);
        #endregion

        #region Buffs
        [Checkbox("Show Buffs Bar")]
        [CollapseControl(20, 3)]
        public bool ShowBuffsBar = true;

        [DragInt("Buffs Bar Padding", max = 1000)]
        [CollapseWith(0, 3)]
        public int BuffsPadding = 2;

        [DragFloat2("Buffs Bar Size", max = 2000f)]
        [CollapseWith(5, 3)]
        public Vector2 BuffsBarSize = new Vector2(254, 20);

        [DragFloat2("Buffs Bar Position", min = -2000f, max = 2000f)]
        [CollapseWith(10, 3)]
        public Vector2 BuffsBarPosition = new Vector2(0, 403);

        [Checkbox("Show Buffs Bar Text")]
        [CollapseWith(15, 3)]
        public bool ShowBuffsText = true;
        #endregion

        #region Higanbana
        [Checkbox("Show Higanbana Bar")]
        [CollapseControl(25, 4)]
        public bool ShowHiganbanaBar = true;

        [DragFloat2("Higanbana Bar Size", max = 2000f)]
        [CollapseWith(0, 4)]
        public Vector2 HiganbanaBarSize = new Vector2(254, 20);

        [DragFloat2("Higanbana Bar Position", min = -2000f, max = 2000f)]
        [CollapseWith(5, 4)]
        public Vector2 HiganbanaBarPosition = new Vector2(0, 381);

        [Checkbox("Show Higanbana Text")]
        [CollapseWith(10, 4)]
        public bool ShowHiganbanaText = true;
        #endregion

        #region colors
        [ColorEdit4("Kenki Bar Color")]
        [Order(30)]
        public PluginConfigColor KenkiColor = new PluginConfigColor(new(255f / 255f, 82f / 255f, 82f / 255f, 53f / 100f));

        [ColorEdit4("Setsu Color")]
        [Order(35)]
        public PluginConfigColor SetsuColor = new PluginConfigColor(new(89f / 255f, 234f / 255f, 247f / 255f, 100f / 100f));

        [ColorEdit4("Getsu Color")]
        [Order(40)]
        public PluginConfigColor GetsuColor = new PluginConfigColor(new(89f / 255f, 126f / 255f, 247f / 255f, 100f / 100f));

        [ColorEdit4("Ka Color")]
        [Order(45)]
        public PluginConfigColor KaColor = new PluginConfigColor(new(247f / 255f, 89f / 255f, 89f / 255f, 100f / 100f));

        [ColorEdit4("Meditation Color")]
        [Order(50)]
        public PluginConfigColor MeditationColor = new PluginConfigColor(new(247f / 255f, 163f / 255f, 89f / 255f, 100f / 100f));

        [ColorEdit4("Shifu Color")]
        [Order(55)]
        public PluginConfigColor ShifuColor = new PluginConfigColor(new(219f / 255f, 211f / 255f, 136f / 255f, 100f / 100f));

        [ColorEdit4("Jinpu Color")]
        [Order(60)]
        public PluginConfigColor JinpuColor = new PluginConfigColor(new(136f / 255f, 146f / 255f, 219f / 255f, 100f / 100f));

        [ColorEdit4("Higanbana Color")]
        [Order(65)]
        public PluginConfigColor HiganbanaColor = new PluginConfigColor(new(237f / 255f, 141f / 255f, 7f / 255f, 100f / 100f));

        [ColorEdit4("Higanbana Expiry Color")]
        [Order(70)]
        public PluginConfigColor HiganbanaExpiryColor = new PluginConfigColor(new(230f / 255f, 33f / 255f, 33f / 255f, 53f / 100f));
        #endregion
    }
}
