using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

            if (Config.KenkiBar.Enabled)
            {
                positions.Add(Config.Position + Config.KenkiBar.Position);
                sizes.Add(Config.KenkiBar.Size);
            }

            if (Config.ShifuBar.Enabled)
            {
                positions.Add(Config.Position + Config.ShifuBar.Position);
                sizes.Add(Config.ShifuBar.Size);
            }

            if (Config.JinpuBar.Enabled)
            {
                positions.Add(Config.Position + Config.JinpuBar.Position);
                sizes.Add(Config.JinpuBar.Size);
            }

            if (Config.HiganbanaBar.Enabled)
            {
                positions.Add(Config.Position + Config.HiganbanaBar.Position);
                sizes.Add(Config.HiganbanaBar.Size);
            }

            if (Config.SenBar.Enabled)
            {
                positions.Add(Config.Position + Config.SenBar.Position);
                sizes.Add(Config.SenBar.Size);
            }

            if (Config.MeditationBar.Enabled)
            {
                positions.Add(Config.Position + Config.MeditationBar.Position);
                sizes.Add(Config.MeditationBar.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            if (Config.KenkiBar.Enabled)
            {
                DrawKenkiBar(origin + Config.Position, player);
            }

            if (Config.ShifuBar.Enabled)
            {
                DrawShifuBar(origin + Config.Position, player);
            }

            if (Config.JinpuBar.Enabled)
            {
                DrawJinpuBar(origin + Config.Position, player);
            }

            if (Config.SenBar.Enabled)
            {
                DrawSenBar(origin + Config.Position, player);
            }

            if (Config.MeditationBar.Enabled)
            {
                DrawMeditationBar(origin + Config.Position);
            }

            if (Config.HiganbanaBar.Enabled)
            {
                DrawHiganbanaBar(origin + Config.Position, player);
            }
        }

        private void DrawKenkiBar(Vector2 pos, PlayerCharacter player)
        {
            var gauge = Plugin.JobGauges.Get<SAMGauge>();
            if (!Config.KenkiBar.HideWhenInactive || gauge.Kenki > 0)
            { 
                Config.KenkiBar.Label.SetText(gauge.Kenki.ToString("N0"));
                BarUtilities.GetProgressBar(nameof(Config.KenkiBar), Config.KenkiBar, gauge.Kenki, 100f, 0f, player).Draw(pos);
            }
        }

        private void DrawShifuBar(Vector2 pos, PlayerCharacter player)
        {
            float shifuDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 1299)?.RemainingTime ?? 0f;
            if (!Config.ShifuBar.HideWhenInactive || shifuDuration > 0)
            {
                Config.ShifuBar.Label.SetText(Math.Truncate(shifuDuration).ToString());
                BarUtilities.GetProgressBar(nameof(Config.ShifuBar), Config.ShifuBar, shifuDuration, 40f, 0f, player).Draw(pos);
            }
        }

        private void DrawJinpuBar(Vector2 pos, PlayerCharacter player)
        {
            float jinpuDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 1298)?.RemainingTime ?? 0f;
            if (!Config.JinpuBar.HideWhenInactive || jinpuDuration > 0)
            {
                Config.JinpuBar.Label.SetText(Math.Truncate(jinpuDuration).ToString());
                BarUtilities.GetProgressBar(nameof(Config.JinpuBar), Config.JinpuBar, jinpuDuration, 40f, 0f, player).Draw(pos);
            }
        }

        private void DrawHiganbanaBar(Vector2 pos, PlayerCharacter player)
        {
            var actor = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            if (actor is BattleChara target)
            {
                var higanbanaDuration = target.StatusList.FirstOrDefault(o => o.StatusId is 1228 or 1319 && o.SourceID == player.ObjectId)?.RemainingTime ?? 0f;
                if (!Config.HiganbanaBar.HideWhenInactive || higanbanaDuration > 0)
                {
                    Config.HiganbanaBar.Label.SetText(Math.Truncate(higanbanaDuration).ToString());
                    BarUtilities.GetProgressBar(nameof(Config.HiganbanaBar), Config.HiganbanaBar, higanbanaDuration, 60f, 0f, player).Draw(pos);
                }
            }
        }

        private void DrawSenBar(Vector2 pos, PlayerCharacter player)
        {
            var gauge = Plugin.JobGauges.Get<SAMGauge>();
            if (!Config.SenBar.HideWhenInactive || gauge.HasSetsu || gauge.HasGetsu || gauge.HasKa)
            {
                var order = Config.SenBar.SenOrder;
                var hasSen = new[] { gauge.HasSetsu ? 1 : 0, gauge.HasGetsu ? 1 : 0, gauge.HasKa ? 1 : 0 };
                var colors = new[] { Config.SenBar.SetsuColor, Config.SenBar.GetsuColor, Config.SenBar.KaColor };

                var sen = new Tuple<PluginConfigColor, float, LabelConfig?>[3];
                for (int i = 0; i < 3; i++)
                {
                    sen[i] = new Tuple<PluginConfigColor, float, LabelConfig?>(colors[order[i]], hasSen[order[i]], null);
                }

                BarUtilities.GetChunkedBars(nameof(Config.SenBar), Config.SenBar, player, sen).Draw(pos);
            }
        }

        private void DrawMeditationBar(Vector2 pos)
        {
            var gauge = Plugin.JobGauges.Get<SAMGauge>();
            if (!Config.MeditationBar.HideWhenInactive || gauge.MeditationStacks > 0)
            {
                BarUtilities.GetChunkedProgressBars(nameof(Config.MeditationBar), Config.MeditationBar, 3, gauge.MeditationStacks, 3f).Draw(pos);
            }
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Melee", 0)]
    [SubSection("Samurai", 1)]
    public class SamuraiConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.SAM;

        public SamuraiConfig()
        {
            // Setup initial bar config
            HiganbanaBar.Threshold = true;
        }

        public new static SamuraiConfig DefaultConfig() { return new SamuraiConfig(); }

        [NestedConfig("Sen Bar", 40)]
        public SenBarConfig SenBar = new SenBarConfig(
                                                    new (0, -17),
                                                    new (254, 10),
                                                    new PluginConfigColor(new Vector4(0, 0, 0, 0)));

        [NestedConfig("Shifu Bar", 45)]
        public ProgressBarConfig ShifuBar = new ProgressBarConfig(
                                                    new (-64, -56),
                                                    new (126, 20),
                                                    new PluginConfigColor(new(219f / 255f, 211f / 255f, 136f / 255f, 100f / 100f)));

        [NestedConfig("Jinpu Bar", 50)]
        public ProgressBarConfig JinpuBar = new ProgressBarConfig(
                                                    new(64, -56),
                                                    new(126, 20),
                                                    new PluginConfigColor(new(136f / 255f, 146f / 255f, 219f / 255f, 100f / 100f)));

        [NestedConfig("Kenki Bar", 55)]
        public ProgressBarConfig KenkiBar = new ProgressBarConfig(
                                                    new(0, -34),
                                                    new(254, 20),
                                                    new PluginConfigColor(new(255f / 255f, 82f / 255f, 82f / 255f, 53f / 100f)));


        [NestedConfig("Higanbana Bar", 60)]
        public ProgressBarConfig HiganbanaBar = new ProgressBarConfig(
                                                    new (0, -78),
                                                    new (254, 20),
                                                    new PluginConfigColor(new(237f / 255f, 141f / 255f, 7f / 255f, 100f / 100f)),
                                                    new PluginConfigColor(new(230f / 255f, 33f / 255f, 33f / 255f, 53f / 100f)),
                                                    15f);

        [NestedConfig("Meditation Bar", 65, separator = true)]
        public ChunkedBarConfig MeditationBar = new ChunkedBarConfig(
                                                    new(0, -5),
                                                    new(254, 10),
                                                    new PluginConfigColor(new(247f / 255f, 163f / 255f, 89f / 255f, 100f / 100f)));
    }

    public class SenBarConfig : ChunkedBarConfig
    {
        [ColorEdit4("Setsu", spacing = true)]
        [Order(60)]
        public PluginConfigColor SetsuColor = new PluginConfigColor(new(89f / 255f, 234f / 255f, 247f / 255f, 100f / 100f));

        [ColorEdit4("Getsu")]
        [Order(65)]
        public PluginConfigColor GetsuColor = new PluginConfigColor(new(89f / 255f, 126f / 255f, 247f / 255f, 100f / 100f));

        [ColorEdit4("Ka")]
        [Order(70)]
        public PluginConfigColor KaColor = new PluginConfigColor(new(247f / 255f, 89f / 255f, 89f / 255f, 100f / 100f));

        [DragDropHorizontal("Order", "Setsu", "Getsu", "Ka")]
        [Order(75)]
        public int[] SenOrder = new int[] { 0, 1, 2 };

        public SenBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor) : base(position, size, fillColor, 2) { }
    }
}
