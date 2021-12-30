using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.Jobs
{
    public class SummonerHud : JobHud
    {
        private new SummonerConfig Config => (SummonerConfig)_config;

        public SummonerHud(SummonerConfig config, string? displayName = null) : base(config, displayName)
        {
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new();
            List<Vector2> sizes = new();

            if (Config.AetherflowBar.Enabled)
            {
                positions.Add(Config.Position + Config.AetherflowBar.Position);
                sizes.Add(Config.AetherflowBar.Size);
            }

            if (Config.TranceBar.Enabled)
            {
                positions.Add(Config.Position + Config.TranceBar.Position);
                sizes.Add(Config.TranceBar.Size);
            }

            if (Config.IfritBar.Enabled)
            {
                positions.Add(Config.Position + Config.IfritBar.Position);
                sizes.Add(Config.IfritBar.Size);
            }

            if (Config.TitanBar.Enabled)
            {
                positions.Add(Config.Position + Config.TitanBar.Position);
                sizes.Add(Config.TitanBar.Size);
            }

            if (Config.GarudaBar.Enabled)
            {
                positions.Add(Config.Position + Config.GarudaBar.Position);
                sizes.Add(Config.GarudaBar.Size);
            }

            if (Config.StacksBar.Enabled)
            {
                positions.Add(Config.Position + Config.StacksBar.Position);
                sizes.Add(Config.StacksBar.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            Vector2 pos = origin + Config.Position;

            if (Config.AetherflowBar.Enabled)
            {
                DrawAetherBar(pos, player);
            }

            if (Config.TranceBar.Enabled)
            {
                DrawTranceBar(pos, player);
            }

            if (Config.IfritBar.Enabled)
            {
                DrawIfritBar(pos, player);
            }

            if (Config.TitanBar.Enabled)
            {
                DrawTitanBar(pos, player);
            }

            if (Config.GarudaBar.Enabled)
            {
                DrawGarudaBar(pos, player);
            }

            if (Config.StacksBar.Enabled)
            {
                HandleAttunementStacks(pos, player);
            }
        }

        private void DrawIfritBar(Vector2 origin, PlayerCharacter player)
        {
            SMNGauge gauge = Plugin.JobGauges.Get<SMNGauge>();
            int stackCount = gauge.IsIfritReady ? 1 : 0;

            if (!Config.IfritBar.HideWhenInactive || stackCount > 1)
            {
                BarHud[] bars = BarUtilities.GetChunkedBars(Config.IfritBar, 1, stackCount, 1, 0, player);
                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.IfritBar.StrataLevel));
                }
            }
        }

        private void DrawTitanBar(Vector2 origin, PlayerCharacter player)
        {
            SMNGauge gauge = Plugin.JobGauges.Get<SMNGauge>();
            int stackCount = gauge.IsTitanReady ? 1 : 0;

            if (!Config.TitanBar.HideWhenInactive || stackCount > 1)
            {
                BarHud[] bars = BarUtilities.GetChunkedBars(Config.TitanBar, 1, stackCount, 1, 0, player);
                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.TitanBar.StrataLevel));
                }
            }
        }

        private void DrawGarudaBar(Vector2 origin, PlayerCharacter player)
        {
            SMNGauge gauge = Plugin.JobGauges.Get<SMNGauge>();
            int stackCount = gauge.IsGarudaReady ? 1 : 0;

            if (!Config.GarudaBar.HideWhenInactive || stackCount > 1)
            {
                BarHud[] bars = BarUtilities.GetChunkedBars(Config.GarudaBar, 1, stackCount, 1, 0, player);
                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.GarudaBar.StrataLevel));
                }
            }
        }

        private void HandleAttunementStacks(Vector2 origin, PlayerCharacter player)
        {
            SMNGauge gauge = Plugin.JobGauges.Get<SMNGauge>();
            byte attunementStacks = gauge.Attunement;

            if (gauge.IsIfritAttuned && Config.StacksBar.ShowIfritStacks)
            {
                DrawStacksBar(origin, player, attunementStacks, 2, Config.StacksBar.IfritStackColor);
            }
            else if (gauge.IsTitanAttuned && Config.StacksBar.ShowTitanStacks)
            {
                DrawStacksBar(origin, player, attunementStacks, 4, Config.StacksBar.TitanStackColor);
            }
            else if (gauge.IsGarudaAttuned && Config.StacksBar.ShowGarudaStacks)
            {
                DrawStacksBar(origin, player, attunementStacks, 4, Config.StacksBar.GarudaStackColor);
            }
            else if (!Config.StacksBar.HideWhenInactive)
            {
                DrawStacksBar(origin, player, 0, 1, Config.StacksBar.FillColor);
            }
        }

        private void DrawAetherBar(Vector2 origin, PlayerCharacter player)
        {
            byte stackCount = player.StatusList.FirstOrDefault(o => o.StatusId == 304)?.StackCount ?? 0;

            if (Config.AetherflowBar.HideWhenInactive && stackCount == 0)
            {
                return;
            }

            BarHud[] bars = BarUtilities.GetChunkedBars(Config.AetherflowBar, 2, stackCount, 2, 0, player);
            foreach (BarHud bar in bars)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.AetherflowBar.StrataLevel));
            }
        }

        private void DrawTranceBar(Vector2 origin, PlayerCharacter player)
        {
            SMNGauge gauge = Plugin.JobGauges.Get<SMNGauge>();
            PluginConfigColor tranceColor;
            uint spellID = 0;
            float maxDuration = 0f;
            float currentCooldown = 0f;
            float tranceDuration = 0f;
            tranceColor = Config.TranceBar.FillColor;

            if (gauge.IsIfritAttuned || gauge.IsTitanAttuned || gauge.IsGarudaAttuned)
            {
                tranceColor = gauge.IsIfritAttuned ? Config.TranceBar.IfritColor : gauge.IsTitanAttuned ? Config.TranceBar.TitanColor : gauge.IsGarudaAttuned ? Config.TranceBar.GarudaColor : Config.TranceBar.FillColor;
                tranceDuration = gauge.AttunmentTimerRemaining;
                maxDuration = 30f;
            }
            else
            {
                if (gauge.IsBahamutReady)
                {
                    tranceColor = Config.TranceBar.BahamutColor;
                    tranceDuration = gauge.SummonTimerRemaining;
                    spellID = 7427;
                    maxDuration = 15f;
                }
                else if (gauge.IsPhoenixReady)
                {
                    tranceColor = Config.TranceBar.PhoenixColor;
                    tranceDuration = gauge.SummonTimerRemaining;
                    spellID = 25831;
                    maxDuration = 15f;
                }
            }

            if (tranceDuration != 0)
            {
                if (gauge.AttunmentTimerRemaining > 0 && Config.TranceBar.HidePrimals)
                {
                    return;
                }

                Config.TranceBar.Label.SetValue(tranceDuration / 1000f);

                BarHud bar = BarUtilities.GetProgressBar(Config.TranceBar, tranceDuration / 1000f, maxDuration, 0, player, tranceColor);
                AddDrawActions(bar.GetDrawActions(origin, Config.TranceBar.StrataLevel));
            }
            else
            {
                if (!Config.TranceBar.HideWhenInactive)
                {
                    if (gauge.AttunmentTimerRemaining == 0)
                    {
                        maxDuration = SpellHelper.Instance.GetRecastTime(spellID);
                        float tranceCooldown = SpellHelper.Instance.GetSpellCooldown(spellID);
                        currentCooldown = maxDuration - tranceCooldown;

                        Config.TranceBar.Label.SetValue(maxDuration - currentCooldown);
                        if (currentCooldown == maxDuration)
                        {
                            Config.TranceBar.Label.SetText("READY");
                        }

                        BarHud bar = BarUtilities.GetProgressBar(Config.TranceBar, currentCooldown, maxDuration, 0, player, tranceColor);
                        AddDrawActions(bar.GetDrawActions(origin, Config.TranceBar.StrataLevel));
                    }
                }
            }
        }

        private void DrawStacksBar(Vector2 origin, PlayerCharacter player, int amount, int max, PluginConfigColor stackColor, BarGlowConfig? glowConfig = null)
        {
            SummonerStacksBarConfig config = Config.StacksBar;

            config.FillColor = stackColor;

            BarHud[] bars = BarUtilities.GetChunkedBars(Config.StacksBar, max, amount, max, 0f, player, glowConfig: glowConfig);
            foreach (BarHud bar in bars)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.StacksBar.StrataLevel));
            }
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Caster", 0)]
    [SubSection("Summoner", 1)]
    public class SummonerConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.SMN;
        public new static SummonerConfig DefaultConfig()
        {
            var config = new SummonerConfig();

            config.TranceBar.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.UseDefaultPrimaryResourceBar = true;

            return config;
        }

        [NestedConfig("Aetherflow Bar", 40)]
        public ChunkedBarConfig AetherflowBar = new ChunkedBarConfig(
            new(-0, -7),
            new(254, 14),
            new(new Vector4(255f / 255f, 177f / 255f, 255f / 255f, 100f / 100f))
        );

        [NestedConfig("Trance Bar", 45)]
        public SummonerTranceBarConfig TranceBar = new SummonerTranceBarConfig(
            new(0, -23),
            new(254, 14),
            new(new Vector4(128f / 255f, 255f / 255f, 255f / 255f, 100f / 100f))
        );

        [NestedConfig("Ifrit Bar", 50)]
        public ChunkedBarConfig IfritBar = new ChunkedBarConfig(
            new(-85, -39),
            new(84, 14),
            new(new Vector4(200f / 255f, 40f / 255f, 0f / 255f, 100f / 100f))
        );

        [NestedConfig("Titan Bar", 55)]
        public ChunkedBarConfig TitanBar = new ChunkedBarConfig(
            new(0, -39),
            new(84, 14),
            new(new Vector4(210f / 255f, 150f / 255f, 26f / 255f, 100f / 100f))
        );

        [NestedConfig("Garuda Bar", 60)]
        public ChunkedBarConfig GarudaBar = new ChunkedBarConfig(
            new(85, -39),
            new(84, 14),
            new(new Vector4(60f / 255f, 160f / 255f, 100f / 255f, 100f / 100f))
        );

        [NestedConfig("Attunement Stacks Bar", 65)]
        public SummonerStacksBarConfig StacksBar = new SummonerStacksBarConfig(
            new(0, -55),
            new(254, 14),
            new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 0f / 100f))
        );

    }


    [Exportable(false)]
    public class SummonerTranceBarConfig : ProgressBarConfig
    {
        [ColorEdit4("Bahamut Color")]
        [Order(26)]
        public PluginConfigColor BahamutColor = new(new Vector4(128f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [ColorEdit4("Phoenix Color")]
        [Order(27)]
        public PluginConfigColor PhoenixColor = new(new Vector4(240f / 255f, 100f / 255f, 10f / 255f, 100f / 100f));

        [ColorEdit4("Ifrit Color")]
        [Order(28)]
        public PluginConfigColor IfritColor = new(new Vector4(200f / 255f, 40f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Titan Color")]
        [Order(29)]
        public PluginConfigColor TitanColor = new(new Vector4(210f / 255f, 150f / 255f, 26f / 255f, 100f / 100f));

        [ColorEdit4("Garuda Color")]
        [Order(30)]
        public PluginConfigColor GarudaColor = new(new Vector4(60f / 255f, 160f / 255f, 100f / 255f, 100f / 100f));

        [Checkbox("Hide Primals")]
        [Order(45)]
        public bool HidePrimals = false;

        public SummonerTranceBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
            : base(position, size, fillColor)
        {
        }
    }

    [Exportable(false)]
    public class SummonerStacksBarConfig : ChunkedBarConfig
    {
        [Checkbox("Ifrit Stacks", separator = false, spacing = false)]
        [Order(51)]
        public bool ShowIfritStacks = true;

        [Checkbox("Titan Stacks", separator = false, spacing = false)]
        [Order(53)]
        public bool ShowTitanStacks = true;

        [Checkbox("Garuda Stacks", separator = false, spacing = false)]
        [Order(55)]
        public bool ShowGarudaStacks = true;

        [ColorEdit4("Ifrit Stacks Color")]
        [Order(56)]
        public PluginConfigColor IfritStackColor = new(new Vector4(200f / 255f, 40f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Titan Stacks Color")]
        [Order(57)]
        public PluginConfigColor TitanStackColor = new(new Vector4(210f / 255f, 150f / 255f, 26f / 255f, 100f / 100f));

        [ColorEdit4("Garuda Stacks Color")]
        [Order(58)]
        public PluginConfigColor GarudaStackColor = new(new Vector4(60f / 255f, 160f / 255f, 100f / 255f, 100f / 100f));

        public SummonerStacksBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
            : base(position, size, fillColor)
        {
        }
    }
}