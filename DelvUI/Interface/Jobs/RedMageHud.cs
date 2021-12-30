using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.Jobs
{
    public class RedMageHud : JobHud
    {
        private new RedMageConfig Config => (RedMageConfig)_config;

        public RedMageHud(RedMageConfig config, string? displayName = null) : base(config, displayName)
        {
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.BalanceBar.Enabled)
            {
                positions.Add(Config.Position + Config.BalanceBar.Position);
                sizes.Add(Config.BalanceBar.Size);
            }

            if (Config.WhiteManaBar.Enabled)
            {
                positions.Add(Config.Position + Config.WhiteManaBar.Position);
                sizes.Add(Config.WhiteManaBar.Size);
            }

            if (Config.BlackManaBar.Enabled)
            {
                positions.Add(Config.Position + Config.BlackManaBar.Position);
                sizes.Add(Config.BlackManaBar.Size);
            }

            if (Config.ManaStacksBar.Enabled)
            {
                positions.Add(Config.Position + Config.ManaStacksBar.Position);
                sizes.Add(Config.ManaStacksBar.Size);
            }

            if (Config.DualcastBar.Enabled)
            {
                positions.Add(Config.Position + Config.DualcastBar.Position);
                sizes.Add(Config.DualcastBar.Size);
            }

            if (Config.VerstoneBar.Enabled)
            {
                positions.Add(Config.Position + Config.VerstoneBar.Position);
                sizes.Add(Config.VerstoneBar.Size);
            }

            if (Config.VerfireBar.Enabled)
            {
                positions.Add(Config.Position + Config.VerfireBar.Position);
                sizes.Add(Config.VerfireBar.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            Vector2 pos = origin + Config.Position;

            if (Config.BalanceBar.Enabled)
            {
                DrawBalanceBar(pos, player);
            }

            if (Config.WhiteManaBar.Enabled)
            {
                DrawWhiteManaBar(pos, player);
            }

            if (Config.BlackManaBar.Enabled)
            {
                DrawBlackManaBar(pos, player);
            }

            if (Config.ManaStacksBar.Enabled)
            {
                DrawManaStacksBarBar(pos, player);
            }

            if (Config.DualcastBar.Enabled)
            {
                DrawDualCastBar(pos, player);
            }

            if (Config.VerstoneBar.Enabled)
            {
                DrawVerstoneBar(pos, player);
            }

            if (Config.VerfireBar.Enabled)
            {
                DrawVerfireBar(pos, player);
            }
        }

        private void DrawBalanceBar(Vector2 origin, PlayerCharacter player)
        {
            RDMGauge gauge = Plugin.JobGauges.Get<RDMGauge>();
            float whiteGauge = (float)Plugin.JobGauges.Get<RDMGauge>().WhiteMana;
            float blackGauge = (float)Plugin.JobGauges.Get<RDMGauge>().BlackMana;
            int scale = gauge.WhiteMana - gauge.BlackMana;

            PluginConfigColor color = Config.BalanceBar.FillColor;
            int value = 0;

            if (whiteGauge >= 50 && blackGauge >= 50)
            {
                value = 1;
            }
            else if (scale >= 30)
            {
                color = Config.WhiteManaBar.FillColor;
                value = 1;
            }
            else if (scale <= -30)
            {
                color = Config.BlackManaBar.FillColor;
                value = 1;
            }

            if (Config.BalanceBar.HideWhenInactive && value == 0)
            {
                return;
            }

            BarHud bar = BarUtilities.GetBar(Config.BalanceBar, value, 1, 0, player, color);
            AddDrawActions(bar.GetDrawActions(origin, Config.BalanceBar.StrataLevel));
        }

        private void DrawWhiteManaBar(Vector2 origin, PlayerCharacter player)
        {
            byte mana = Plugin.JobGauges.Get<RDMGauge>().WhiteMana;
            if (Config.WhiteManaBar.HideWhenInactive && mana == 0)
            {
                return;
            }

            Config.WhiteManaBar.Label.SetValue(mana);

            BarHud bar = BarUtilities.GetProgressBar(Config.WhiteManaBar, mana, 100, 0, player);
            AddDrawActions(bar.GetDrawActions(origin, Config.WhiteManaBar.StrataLevel));
        }

        private void DrawBlackManaBar(Vector2 origin, PlayerCharacter player)
        {
            byte mana = Plugin.JobGauges.Get<RDMGauge>().BlackMana;
            if (Config.BlackManaBar.HideWhenInactive && mana == 0)
            {
                return;
            }

            Config.BlackManaBar.Label.SetValue(mana);

            BarHud bar = BarUtilities.GetProgressBar(Config.BlackManaBar, mana, 100, 0, player);
            AddDrawActions(bar.GetDrawActions(origin, Config.BlackManaBar.StrataLevel));
        }

        private void DrawManaStacksBarBar(Vector2 origin, PlayerCharacter player)
        {
            byte manaStacks = Plugin.JobGauges.Get<RDMGauge>().ManaStacks;
            if (Config.ManaStacksBar.HideWhenInactive && manaStacks == 0)
            {
                return;
            }

            BarHud[] bars = BarUtilities.GetChunkedBars(Config.ManaStacksBar, 3, manaStacks, 3f, 0, player);
            foreach (BarHud bar in bars)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.ManaStacksBar.StrataLevel));
            }
        }

        private void DrawDualCastBar(Vector2 origin, PlayerCharacter player)
        {
            float duration = player.StatusList.FirstOrDefault(o => o.StatusId is 1249)?.RemainingTime ?? 0f;

            if (Config.DualcastBar.HideWhenInactive && duration == 0)
            {
                return;
            };

            Config.DualcastBar.Label.SetValue(duration);

            BarHud bar = BarUtilities.GetProgressBar(Config.DualcastBar, duration, 15f, 0, player);
            AddDrawActions(bar.GetDrawActions(origin, Config.DualcastBar.StrataLevel));
        }

        private void DrawVerstoneBar(Vector2 origin, PlayerCharacter player)
        {
            BarHud? bar = BarUtilities.GetProcBar(Config.VerstoneBar, player, 1235, 30);
            if (bar != null)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.VerstoneBar.StrataLevel));
            }
        }

        private void DrawVerfireBar(Vector2 origin, PlayerCharacter player)
        {
            BarHud? bar = BarUtilities.GetProcBar(Config.VerfireBar, player, 1234, 30);
            if (bar != null)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.VerfireBar.StrataLevel));
            }
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Caster", 0)]
    [SubSection("Red Mage", 1)]
    public class RedMageConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.RDM;
        public new static RedMageConfig DefaultConfig()
        {
            var config = new RedMageConfig();
            config.UseDefaultPrimaryResourceBar = true;

            config.WhiteManaBar.ThresholdConfig.Enabled = true;
            config.WhiteManaBar.ThresholdConfig.ChangeColor = false;
            config.WhiteManaBar.ThresholdConfig.ShowMarker = true;
            config.WhiteManaBar.Label.TextAnchor = DrawAnchor.Right;
            config.WhiteManaBar.Label.FrameAnchor = DrawAnchor.Right;
            config.WhiteManaBar.Label.Position = new Vector2(-2, 0);

            config.BlackManaBar.ThresholdConfig.Enabled = true;
            config.BlackManaBar.ThresholdConfig.ChangeColor = false;
            config.BlackManaBar.ThresholdConfig.ShowMarker = true;
            config.BlackManaBar.Label.TextAnchor = DrawAnchor.Left;
            config.BlackManaBar.Label.FrameAnchor = DrawAnchor.Left;
            config.BlackManaBar.Label.Position = new Vector2(2, 0);

            config.DualcastBar.Label.Enabled = false;

            config.VerstoneBar.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.VerstoneBar.Label.TextAnchor = DrawAnchor.Right;
            config.VerstoneBar.Label.FrameAnchor = DrawAnchor.Right;
            config.VerstoneBar.Label.Position = new Vector2(-2, 0);

            config.VerfireBar.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.VerfireBar.Label.TextAnchor = DrawAnchor.Left;
            config.VerfireBar.Label.FrameAnchor = DrawAnchor.Left;
            config.VerfireBar.Label.Position = new Vector2(2, 0);

            return config;
        }

        [NestedConfig("Balance Bar", 30)]
        public BarConfig BalanceBar = new BarConfig(
            new(0, -10),
            new(20, 20),
            new PluginConfigColor(new(195f / 255f, 35f / 255f, 35f / 255f, 100f / 100f))
        );

        [NestedConfig("White Mana Bar", 35)]
        public ProgressBarConfig WhiteManaBar = new ProgressBarConfig(
            new(-69.5f, -10),
            new(115, 20),
            new PluginConfigColor(new(221f / 255f, 212f / 255f, 212f / 255f, 100f / 100f)),
            BarDirection.Left,
            null,
            80
        );

        [NestedConfig("Black Mana Bar", 40)]
        public ProgressBarConfig BlackManaBar = new ProgressBarConfig(
            new(69.5f, -10),
            new(115, 20),
            new PluginConfigColor(new(60f / 255f, 81f / 255f, 197f / 255f, 100f / 100f)),
            threshold: 80
        );

        [NestedConfig("Mana Stacks Bar", 45)]
        public ChunkedBarConfig ManaStacksBar = new ChunkedBarConfig(
            new(0, -27),
            new(254, 10),
            new PluginConfigColor(new(200f / 255f, 45f / 255f, 40f / 255f, 100f / 100f))
        );

        [NestedConfig("Dualcast Bar", 50)]
        public ProgressBarConfig DualcastBar = new ProgressBarConfig(
            new(0, -41),
            new(16, 14),
            new PluginConfigColor(new(204f / 255f, 17f / 255f, 255f / 95f, 100f / 100f))
        );

        [NestedConfig("Verstone Ready Bar", 55)]
        public ProgressBarConfig VerstoneBar = new ProgressBarConfig(
            new(-68.5f, -41),
            new(117, 14),
            new PluginConfigColor(new(228f / 255f, 188f / 255f, 145 / 255f, 90f / 100f)),
            BarDirection.Left
        );

        [NestedConfig("Verfire Ready Bar", 60)]
        public ProgressBarConfig VerfireBar = new ProgressBarConfig(
            new(68.5f, -41),
            new(117, 14),
            new PluginConfigColor(new(238f / 255f, 119f / 255f, 17 / 255f, 90f / 100f))
        );
    }
}
