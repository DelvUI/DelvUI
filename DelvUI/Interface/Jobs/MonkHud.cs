using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
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
    public class MonkHud : JobHud
    {
        private new MonkConfig Config => (MonkConfig)_config;

        public MonkHud(MonkConfig config, string? displayName = null) : base(config, displayName)
        {
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.DemolishBar.Enabled)
            {
                positions.Add(Config.Position + Config.DemolishBar.Position);
                sizes.Add(Config.DemolishBar.Size);
            }

            if (Config.ChakraBar.Enabled)
            {
                positions.Add(Config.Position + Config.ChakraBar.Position);
                sizes.Add(Config.ChakraBar.Size);
            }

            if (Config.LeadenFistBar.Enabled)
            {
                positions.Add(Config.Position + Config.LeadenFistBar.Position);
                sizes.Add(Config.LeadenFistBar.Size);
            }

            if (Config.TwinSnakesBar.Enabled)
            {
                positions.Add(Config.Position + Config.TwinSnakesBar.Position);
                sizes.Add(Config.TwinSnakesBar.Size);
            }

            if (Config.RiddleofEarthBar.Enabled)
            {
                positions.Add(Config.Position + Config.RiddleofEarthBar.Position);
                sizes.Add(Config.RiddleofEarthBar.Size);
            }

            if (Config.PerfectBalanceBar.Enabled)
            {
                positions.Add(Config.Position + Config.PerfectBalanceBar.Position);
                sizes.Add(Config.PerfectBalanceBar.Size);
            }

            if (Config.TrueNorthBar.Enabled)
            {
                positions.Add(Config.Position + Config.TrueNorthBar.Position);
                sizes.Add(Config.TrueNorthBar.Size);
            }

            if (Config.FormsBar.Enabled)
            {
                positions.Add((Config.Position + Config.FormsBar.Position));
                sizes.Add(Config.FormsBar.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            var position = origin + Config.Position;
            if (Config.FormsBar.Enabled)
            {
                DrawFormsBar(position, player);
            }

            if (Config.RiddleofEarthBar.Enabled)
            {
                DrawRiddleOfEarthBar(position, player);
            }

            if (Config.PerfectBalanceBar.Enabled)
            {
                DrawPerfectBalanceBar(position, player);
            }

            if (Config.TrueNorthBar.Enabled)
            {
                DrawTrueNorthBar(position, player);
            }

            //if (Config.ChakraBar.Enabled)
            //{
            //    DrawChakraGauge(position);
            //}

            if (Config.LeadenFistBar.Enabled)
            {
                DrawLeadenFistBar(position, player);
            }

            if (Config.TwinSnakesBar.Enabled)
            {
                DrawTwinSnakesBar(position, player);
            }

            if (Config.DemolishBar.Enabled)
            {
                DrawDemolishBar(position, player);
            }
        }

        private void DrawFormsBar(Vector2 origin, PlayerCharacter player)
        {
            Status? form = player.StatusList.FirstOrDefault(o => o.StatusId is 107 or 108 or 109 or 2513 && o.RemainingTime > 0f);

            if (!Config.FormsBar.HideWhenInactive || form is not null)
            {
                float formDuration = form?.RemainingTime ?? 0f;
                string label = form is not null ? form.StatusId switch
                {
                    107 => "Opo-Opo Form",
                    108 => "Raptor Form",
                    109 => "Coeurl Form",
                    2513 => "Formless Fist",
                    _ => ""
                } : "";

                Config.FormsBar.Label.SetText(label);
                BarUtilities.GetProgressBar(Config.FormsBar, formDuration, 15f, 0, player).Draw(origin);
            }
        }

        private void DrawTrueNorthBar(Vector2 origin, PlayerCharacter player)
        {
            float trueNorthDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 1250 && o.RemainingTime > 0)?.RemainingTime ?? 0f;
            if (!Config.TrueNorthBar.HideWhenInactive || trueNorthDuration > 0)
            {
                Config.TrueNorthBar.Label.SetValue(trueNorthDuration);
                BarUtilities.GetProgressBar(Config.TrueNorthBar, trueNorthDuration, 10f, 0f, player).Draw(origin);
            }
        }

        private void DrawPerfectBalanceBar(Vector2 origin, PlayerCharacter player)
        {
            Status? perfectBalance = player.StatusList.Where(o => o.StatusId is 110 && o.RemainingTime > 0f).FirstOrDefault();
            if (!Config.PerfectBalanceBar.HideWhenInactive || perfectBalance is not null)
            {
                float duration = perfectBalance?.RemainingTime ?? 0f;
                float stacks = perfectBalance?.StackCount ?? 0f;
                Config.PerfectBalanceBar.Label.SetValue(stacks);
                BarUtilities.GetProgressBar(Config.PerfectBalanceBar, duration, 15f, 0, player).Draw(origin);
            }
        }

        private void DrawRiddleOfEarthBar(Vector2 origin, PlayerCharacter player)
        {
            Status? riddleOfEarth = player.StatusList.Where(o => o.StatusId is 1179 && o.RemainingTime > 0f).FirstOrDefault();
            if (!Config.PerfectBalanceBar.HideWhenInactive || riddleOfEarth is not null)
            {
                float duration = riddleOfEarth?.RemainingTime ?? 0f;
                float stacks = riddleOfEarth?.StackCount ?? 0f;
                Config.RiddleofEarthBar.Label.SetValue(stacks);
                BarUtilities.GetProgressBar(Config.RiddleofEarthBar, duration, 10f, 0, player).Draw(origin);
            }
        }

        //private void DrawChakraGauge(Vector2 origin)
        //{
        //    var gauge = Plugin.JobGauges.Get<MNKGauge>();
        //    if (!Config.ChakraBar.HideWhenInactive || gauge.Chakra > 0)
        //    {
        //        BarUtilities.GetChunkedBars(Config.ChakraBar, 5, gauge.Chakra, 5).Draw(origin);
        //    }
        //}

        private void DrawTwinSnakesBar(Vector2 origin, PlayerCharacter player)
        {
            float twinSnakesDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 101 && o.RemainingTime > 0)?.RemainingTime ?? 0f;
            if (!Config.TwinSnakesBar.HideWhenInactive || twinSnakesDuration > 0)
            {
                Config.TwinSnakesBar.Label.SetValue(twinSnakesDuration);
                BarUtilities.GetProgressBar(Config.TwinSnakesBar, twinSnakesDuration, 15f, 0f, player).Draw(origin);
            }
        }

        private void DrawLeadenFistBar(Vector2 origin, PlayerCharacter player)
        {
            float leadenFistDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 1861 && o.RemainingTime > 0)?.RemainingTime ?? 0f;
            if (!Config.LeadenFistBar.HideWhenInactive || leadenFistDuration > 0)
            {
                Config.LeadenFistBar.Label.SetValue(leadenFistDuration);
                BarUtilities.GetProgressBar(Config.LeadenFistBar, leadenFistDuration, 30f, 0f, player).Draw(origin);
            }
        }

        private void DrawDemolishBar(Vector2 origin, PlayerCharacter player)
        {
            GameObject? target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;

            BarUtilities.GetDoTBar(Config.DemolishBar, player, target, 246, 18f)?.
                Draw(origin);
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Melee", 0)]
    [SubSection("Monk", 1)]
    public class MonkConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.MNK;
        public new static MonkConfig DefaultConfig()
        {
            var config = new MonkConfig();

            config.PerfectBalanceBar.FillDirection = BarDirection.Up;
            config.LeadenFistBar.FillDirection = BarDirection.Up;

            return config;
        }

        [NestedConfig("Demolish", 30)]
        public ProgressBarConfig DemolishBar = new ProgressBarConfig(
            new(71, -10),
            new(111, 20),
            new(new Vector4(246f / 255f, 169f / 255f, 255f / 255f, 100f / 100f))
        );

        [NestedConfig("Chakra", 35)]
        public ChunkedBarConfig ChakraBar = new ChunkedBarConfig(
            new(0, -32),
            new(254, 20),
            new(new Vector4(204f / 255f, 115f / 255f, 0f, 100f / 100f))
        );

        [NestedConfig("Leaden Fist", 40)]
        public ProgressBarConfig LeadenFistBar = new ProgressBarConfig(
            new(0, -10),
            new(28, 20),
            new(new Vector4(255f / 255f, 0f, 0f, 100f / 100f))
        );

        [NestedConfig("Twin Snakes", 45)]
        public ProgressBarConfig TwinSnakesBar = new ProgressBarConfig(
            new(-71, -10),
            new(111, 20),
            new(new Vector4(227f / 255f, 255f / 255f, 64f / 255f, 100f / 100f))
        );

        [NestedConfig("Riddle of Earth", 50)]
        public ProgressBarConfig RiddleofEarthBar = new ProgressBarConfig(
            new(-69, -54),
            new(115, 20),
            new(new Vector4(157f / 255f, 59f / 255f, 255f / 255f, 100f / 100f))
        );

        [NestedConfig("Perfect Balance", 55)]
        public ProgressBarConfig PerfectBalanceBar = new ProgressBarConfig(
            new(0, -54),
            new(20, 20),
            new(new Vector4(150f / 255f, 255f / 255f, 255f / 255f, 100f / 100f))
        );

        [NestedConfig("True North", 60)]
        public ProgressBarConfig TrueNorthBar = new ProgressBarConfig(
            new(69, -54),
            new(115, 20),
            new(new Vector4(255f / 255f, 225f / 255f, 189f / 255f, 100f / 100f))
        );

        [NestedConfig("Forms", 65)]
        public ProgressBarConfig FormsBar = new ProgressBarConfig(
            new(0, -76),
            new(254, 20),
            new(new Vector4(36f / 255f, 131f / 255f, 255f / 255f, 100f / 100f))
        );
    }
}
