using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
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

            if (Config.PerfectBalanceBar.Enabled)
            {
                positions.Add(Config.Position + Config.PerfectBalanceBar.Position);
                sizes.Add(Config.PerfectBalanceBar.Size);
            }

            if (Config.StancesBar.Enabled)
            {
                positions.Add((Config.Position + Config.StancesBar.Position));
                sizes.Add(Config.StancesBar.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, IPlayerCharacter player)
        {
            var position = origin + Config.Position;
            if (Config.StancesBar.Enabled)
            {
                DrawFormsBar(position, player);
            }

            if (Config.PerfectBalanceBar.Enabled)
            {
                DrawPerfectBalanceBar(position, player);
            }

            if (Config.ChakraBar.Enabled)
            {
                DrawChakraGauge(position, player);
            }

            if (Config.MastersGauge.Enabled)
            {
                DrawBeastChakraGauge(position, player);
            }

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

        private void DrawFormsBar(Vector2 origin, IPlayerCharacter player)
        {
            // formless fist
            Status? formlessFist = Utils.StatusListForBattleChara(player).FirstOrDefault(o => o.StatusId == 2513);
            if (formlessFist != null)
            {
                float remaining = Math.Abs(formlessFist.RemainingTime);

                BarHud bar = BarUtilities.GetProgressBar(
                    Config.StancesBar,
                    null,
                    new LabelConfig[] { Config.StancesBar.FormlessFistLabel },
                    remaining,
                    30f,
                    0,
                    player,
                    Config.StancesBar.FormlessFistColor
                );

                Config.StancesBar.FormlessFistLabel.SetValue(remaining);

                AddDrawActions(bar.GetDrawActions(origin, Config.StancesBar.StrataLevel));
                return;
            }

            // forms
            Status? form = Utils.StatusListForBattleChara(player).FirstOrDefault(o => o.StatusId is 107 or 108 or 109);

            if (!Config.StancesBar.HideWhenInactive || form is not null)
            {
                int activeFormIndex = form != null ? (int)form.StatusId - 107 : -1;
                PluginConfigColor[] chunkColors = new PluginConfigColor[]
                {
                    Config.StancesBar.OpoOpoColor,
                    Config.StancesBar.RaptorColor,
                    Config.StancesBar.CoeurlColor
                };

                string[] chunkTexts = new string[] { "I", "II", "III" };
                LabelConfig[] chunkLabels = new LabelConfig[]
                {
                    Config.StancesBar.FormLabel.Clone(0),
                    Config.StancesBar.FormLabel.Clone(1),
                    Config.StancesBar.FormLabel.Clone(2)
                };


                var chunks = new Tuple<PluginConfigColor, float, LabelConfig?>[3];

                for (int i = 0; i < chunks.Length; i++)
                {
                    LabelConfig label = chunkLabels[i];
                    label.SetText(chunkTexts[i]);

                    chunks[i] = new(chunkColors[i], activeFormIndex == i ? 1 : 0, label);
                }

                BarHud[] bars = BarUtilities.GetChunkedBars(Config.StancesBar, chunks, player);
                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.StancesBar.StrataLevel));
                }
            }
        }

        private void DrawPerfectBalanceBar(Vector2 origin, IPlayerCharacter player)
        {
            Status? perfectBalance = Utils.StatusListForBattleChara(player).Where(o => o.StatusId is 110 && o.RemainingTime > 0f).FirstOrDefault();
            if (!Config.PerfectBalanceBar.HideWhenInactive || perfectBalance is not null)
            {
                float duration = perfectBalance?.RemainingTime ?? 0f;
                float stacks = perfectBalance?.StackCount ?? 0f;
                var chunks = new Tuple<PluginConfigColor, float, LabelConfig?>[3];

                for (int i = 0; i < chunks.Length; i++)
                {
                    chunks[i] = new(
                        Config.PerfectBalanceBar.FillColor,
                        i < stacks ? 1f : 0,
                        i == 1 ? Config.PerfectBalanceBar.PerfectBalanceLabel : null);
                }

                Config.PerfectBalanceBar.PerfectBalanceLabel.SetValue(duration);

                BarHud[] bars = BarUtilities.GetChunkedBars(Config.PerfectBalanceBar, chunks, player);
                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.PerfectBalanceBar.StrataLevel));
                }
            }
        }

        private void DrawChakraGauge(Vector2 origin, IPlayerCharacter player)
        {
            var gauge = Plugin.JobGauges.Get<MNKGauge>();
            if (!Config.ChakraBar.HideWhenInactive || gauge.Chakra > 0)
            {
                BarHud[] bars = BarUtilities.GetChunkedBars(Config.ChakraBar, 5, gauge.Chakra, 5, 0, player);
                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.ChakraBar.StrataLevel));
                }
            }
        }

        private void DrawBeastChakraGauge(Vector2 origin, IPlayerCharacter player)
        {
            var gauge = Plugin.JobGauges.Get<MNKGauge>();
            if (!Config.MastersGauge.HideWhenInactive ||
                gauge.Nadi != Nadi.NONE ||
                gauge.BeastChakra[0] != BeastChakra.NONE ||
                gauge.BeastChakra[1] != BeastChakra.NONE ||
                gauge.BeastChakra[2] != BeastChakra.NONE)
            {
                var order = Config.MastersGauge.ChakraOrder;
                var hasChakra = new[]
                {
                    (gauge.Nadi & Nadi.LUNAR) != 0 ? 1 : 0,
                    gauge.BeastChakra[0] != BeastChakra.NONE ? 1 : 0,
                    gauge.BeastChakra[0] != BeastChakra.NONE ? 1 : 0,
                    gauge.BeastChakra[0] != BeastChakra.NONE ? 1 : 0,
                    (gauge.Nadi & Nadi.SOLAR) != 0 ? 1 : 0,
                };

                var colors = new[]
                {
                    Config.MastersGauge.LunarNadiColor,
                    GetChakraColor(gauge.BeastChakra[0]),
                    GetChakraColor(gauge.BeastChakra[1]),
                    GetChakraColor(gauge.BeastChakra[2]),
                    Config.MastersGauge.SolarNadiColor
                };

                var chunks = new Tuple<PluginConfigColor, float, LabelConfig?>[5];
                for (int i = 0; i < chunks.Length; i++)
                {
                    chunks[i] = new(colors[order[i]], hasChakra[order[i]], i == 2 ? Config.MastersGauge.BlitzTimerLabel : null);
                }

                Config.MastersGauge.BlitzTimerLabel.SetValue(gauge.BlitzTimeRemaining / 1000);

                BarHud[] bars = BarUtilities.GetChunkedBars(Config.MastersGauge, chunks, player);
                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.MastersGauge.StrataLevel));
                }
            }
        }

        private PluginConfigColor GetChakraColor(BeastChakra chakra) => chakra switch
        {
            BeastChakra.RAPTOR => Config.MastersGauge.RaptorChakraColor,
            BeastChakra.COEURL => Config.MastersGauge.CoeurlChakraColor,
            BeastChakra.OPOOPO => Config.MastersGauge.OpoopoChakraColor,
            _ => new PluginConfigColor(new(0, 0, 0, 0))
        };

        private void DrawTwinSnakesBar(Vector2 origin, IPlayerCharacter player)
        {
            BarHud? bar = BarUtilities.GetProcBar(Config.TwinSnakesBar, player, 3001, 15f);
            if (bar != null)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.TwinSnakesBar.StrataLevel));
            }
        }

        private void DrawLeadenFistBar(Vector2 origin, IPlayerCharacter player)
        {
            BarHud? bar = BarUtilities.GetProcBar(Config.LeadenFistBar, player, 1861, 30f);
            if (bar != null)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.LeadenFistBar.StrataLevel));
            }
        }

        private void DrawDemolishBar(Vector2 origin, IPlayerCharacter player)
        {
            IGameObject? target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;

            BarHud? bar = BarUtilities.GetDoTBar(Config.DemolishBar, player, target, 246, 18f);
            if (bar != null)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.DemolishBar.StrataLevel));
            }
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

            config.StancesBar.Enabled = false;
            config.LeadenFistBar.FillDirection = BarDirection.Up;
            config.MastersGauge.BlitzTimerLabel.HideIfZero = true;
            config.PerfectBalanceBar.PerfectBalanceLabel.HideIfZero = true;

            return config;
        }

        [NestedConfig("Demolish", 30)]
        public ProgressBarConfig DemolishBar = new ProgressBarConfig(
            new(71, -10),
            new(111, 20),
            new(new Vector4(246f / 255f, 169f / 255f, 255f / 255f, 100f / 100f))
        );

        [NestedConfig("Leaden Fist", 31)]
        public ProgressBarConfig LeadenFistBar = new ProgressBarConfig(
            new(0, -10),
            new(28, 20),
            new(new Vector4(255f / 255f, 0f, 0f, 100f / 100f))
        );

        [NestedConfig("Twin Snakes", 32)]
        public ProgressBarConfig TwinSnakesBar = new ProgressBarConfig(
            new(-71, -10),
            new(111, 20),
            new(new Vector4(227f / 255f, 255f / 255f, 64f / 255f, 100f / 100f))
        );

        [NestedConfig("Chakra", 35)]
        public ChunkedBarConfig ChakraBar = new ChunkedBarConfig(
            new(0, -32),
            new(254, 20),
            new(new Vector4(204f / 255f, 115f / 255f, 0f, 100f / 100f))
        );

        [NestedConfig("Masterful Blitz Gauge", 40)]
        public MastersGauge MastersGauge = new MastersGauge(
            new(0, -54),
            new(254, 20),
            new(new Vector4(204f / 255f, 115f / 255f, 0f, 100f / 100f))
        );

        [NestedConfig("Forms", 45)]
        public MonkStancesBarConfig StancesBar = new MonkStancesBarConfig(
            new(0, -98),
            new(254, 20),
            new(new Vector4(36f / 255f, 131f / 255f, 255f / 255f, 100f / 100f))
        );

        [NestedConfig("Perfect Balance", 50)]
        public PerfectBalanceBar PerfectBalanceBar = new PerfectBalanceBar(
            new(0, -76),
            new(254, 20),
            new(new Vector4(150f / 255f, 255f / 255f, 255f / 255f, 100f / 100f))
        );
    }

    public class PerfectBalanceBar : ChunkedBarConfig
    {
        [NestedConfig("Perfect Balance Duration Text", 50, spacing = true)]
        public NumericLabelConfig PerfectBalanceLabel;

        public PerfectBalanceBar(Vector2 position, Vector2 size, PluginConfigColor fillColor, int padding = 2) : base(position, size, fillColor, padding)
        {
            PerfectBalanceLabel = new NumericLabelConfig(Vector2.Zero, "", DrawAnchor.Center, DrawAnchor.Center);
        }
    }

    [DisableParentSettings("FillColor", "FillDirection")]
    public class MastersGauge : ChunkedBarConfig
    {
        [ColorEdit4("Lunar Nadi Color")]
        [Order(19)]
        public PluginConfigColor LunarNadiColor = new PluginConfigColor(new Vector4(240f / 255f, 227f / 255f, 246f / 255f, 100f / 100f));

        [ColorEdit4("Solar Nadi Color")]
        [Order(20)]
        public PluginConfigColor SolarNadiColor = new PluginConfigColor(new Vector4(255f / 255f, 248f / 255f, 141f / 255f, 100f / 100f));

        [ColorEdit4("Raptor Chakra Color")]
        [Order(21)]
        public PluginConfigColor RaptorChakraColor = new PluginConfigColor(new Vector4(92f / 255f, 123f / 255f, 200f / 255f, 100f / 100f));

        [ColorEdit4("Coeurl Chakra Color")]
        [Order(22)]
        public PluginConfigColor CoeurlChakraColor = new PluginConfigColor(new Vector4(142f / 255f, 216f / 255f, 116f / 255f, 100f / 100f));

        [ColorEdit4("Opo-opo Chakra Color")]
        [Order(23)]
        public PluginConfigColor OpoopoChakraColor = new PluginConfigColor(new Vector4(184f / 255f, 107f / 255f, 124f / 255f, 100f / 100f));

        [DragDropHorizontal("Chakra Order", "Lunar Nadi", "Chakra 1", "Chakra 2", "Chakra 3", "Solar Nadi")]
        [Order(24)]
        public int[] ChakraOrder = new int[] { 0, 1, 2, 3, 4 };

        [NestedConfig("Blitz Timer Text", 50, spacing = true)]
        public NumericLabelConfig BlitzTimerLabel;

        public MastersGauge(Vector2 position, Vector2 size, PluginConfigColor fillColor, int padding = 2) : base(position, size, fillColor, padding)
        {
            BlitzTimerLabel = new NumericLabelConfig(Vector2.Zero, "", DrawAnchor.Center, DrawAnchor.Center);
        }
    }

    [DisableParentSettings("FillColor")]
    public class MonkStancesBarConfig : ChunkedBarConfig
    {
        [ColorEdit4("Opo-opo Color")]
        [Order(19)]
        public PluginConfigColor OpoOpoColor = new PluginConfigColor(new Vector4(184f / 255f, 107f / 255f, 124f / 255f, 100f / 100f));

        [ColorEdit4("Raptor Color")]
        [Order(20)]
        public PluginConfigColor RaptorColor = new PluginConfigColor(new Vector4(92f / 255f, 123f / 255f, 200f / 255f, 100f / 100f));

        [ColorEdit4("Coeurl Color")]
        [Order(21)]
        public PluginConfigColor CoeurlColor = new PluginConfigColor(new Vector4(199f / 255f, 123f / 255f, 78f / 255f, 100f / 100f));

        [ColorEdit4("Formless Fist Color")]
        [Order(22)]
        public PluginConfigColor FormlessFistColor = new PluginConfigColor(new Vector4(106f / 255f, 92f / 255f, 191f / 255f, 100f / 100f));

        [NestedConfig("Form Number Text", 500, spacing = true)]
        public LabelConfig FormLabel;

        [NestedConfig("Formless Fist Duration Text", 1000, separator = false, spacing = true)]
        public NumericLabelConfig FormlessFistLabel;

        public MonkStancesBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor, int padding = 2) : base(position, size, fillColor, padding)
        {
            FormLabel = new LabelConfig(Vector2.Zero, "", DrawAnchor.Center, DrawAnchor.Center);

            FormlessFistLabel = new NumericLabelConfig(Vector2.Zero, "", DrawAnchor.Center, DrawAnchor.Center);
            FormlessFistLabel.Enabled = false;
        }
    }
}
