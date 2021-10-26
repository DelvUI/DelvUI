using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
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
    public class DarkKnightHud : JobHud
    {
        private new DarkKnightConfig Config => (DarkKnightConfig)_config;

        public DarkKnightHud(DarkKnightConfig config, string? displayName = null) : base(config, displayName)
        {
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new();
            List<Vector2> sizes = new();

            if (Config.ManaBar.Enabled)
            {
                positions.Add(Config.Position + Config.ManaBar.Position);
                sizes.Add(Config.ManaBar.Size);
            }

            if (Config.BloodGauge.Enabled)
            {
                positions.Add(Config.Position + Config.BloodGauge.Position);
                sizes.Add(Config.BloodGauge.Size);
            }

            if (Config.DarksideBar.Enabled)
            {
                positions.Add(Config.Position + Config.DarksideBar.Position);
                sizes.Add(Config.DarksideBar.Size);
            }

            if (Config.BloodWeaponBar.Enabled)
            {
                positions.Add(Config.Position + Config.BloodWeaponBar.Position);
                sizes.Add(Config.BloodWeaponBar.Size);
            }

            if (Config.DeliriumBar.Enabled)
            {
                positions.Add(Config.Position + Config.DeliriumBar.Position);
                sizes.Add(Config.DeliriumBar.Size);
            }

            if (Config.LivingShadowBar.Enabled)
            {
                positions.Add(Config.Position + Config.LivingShadowBar.Position);
                sizes.Add(Config.LivingShadowBar.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            Vector2 pos = origin + Config.Position;

            if (Config.ManaBar.Enabled)
            {
                DrawManaBar(pos, player);
            }

            if (Config.BloodGauge.Enabled)
            {
                DrawBloodGauge(pos, player);
            }

            if (Config.DarksideBar.Enabled)
            {
                DrawDarkside(pos);
            }

            if (Config.BloodWeaponBar.Enabled)
            {
                DrawBloodWeaponBar(pos, player);
            }

            if (Config.DeliriumBar.Enabled)
            {
                DrawDeliriumBar(pos, player);
            }

            if (Config.LivingShadowBar.Enabled)
            {
                DrawLivingShadowBar(pos);
            }
        }

        private void DrawManaBar(Vector2 origin, PlayerCharacter player)
        {
            DRKGauge gauge = Plugin.JobGauges.Get<DRKGauge>();

            if (Config.ManaBar.HideWhenInactive && !gauge.HasDarkArts && player.CurrentMp == player.MaxMp) { return; }

            if (gauge.HasDarkArts)
            {
                Config.ManaBar.UsePartialFillColor = false;
            }
            else { Config.ManaBar.UsePartialFillColor = true; }

            Config.ManaBar.Label.SetValue(player.CurrentMp);

            // hardcoded 9k as maxMP so the chunks are each 3k since that's what a DRK wants to see
            BarUtilities.GetChunkedProgressBars(
                Config.ManaBar,
                gauge.HasDarkArts ? 1 : 3,
                player.CurrentMp,
                Config.ManaBar.UseChunks ? 9000 : player.MaxMp,
                0f,
                player,
                glowConfig: null,
                gauge.HasDarkArts ? Config.ManaBar.DarkArtsColor : Config.ManaBar.FillColor
                ).Draw(origin);
        }

        private void DrawDarkside(Vector2 origin)
        {
            DRKGauge gauge = Plugin.JobGauges.Get<DRKGauge>();
            if (Config.DarksideBar.HideWhenInactive && gauge.DarksideTimeRemaining == 0) { return; };

            float timer = Math.Abs(gauge.DarksideTimeRemaining);
            Config.DarksideBar.Label.SetValue(timer / 1000);
            BarUtilities.GetProgressBar(Config.DarksideBar, timer, 60000f)
                .Draw(origin);
        }

        private void DrawBloodGauge(Vector2 origin, PlayerCharacter player)
        {
            DRKGauge gauge = Plugin.JobGauges.Get<DRKGauge>();
            if (!Config.BloodGauge.HideWhenInactive || gauge.Blood > 0)
            {
                Config.BloodGauge.Label.SetValue(gauge.Blood);
                BarUtilities.GetChunkedProgressBars(Config.BloodGauge, 2, gauge.Blood, 100).Draw(origin);
            }
        }

        private void DrawBloodWeaponBar(Vector2 origin, PlayerCharacter player)
        {
            float bloodWeaponDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 742)?.RemainingTime ?? 0f;

            if (Config.BloodWeaponBar.HideWhenInactive && bloodWeaponDuration is 0) { return; }

            Config.BloodWeaponBar.Label.SetValue(bloodWeaponDuration);
            BarUtilities.GetProgressBar(Config.BloodWeaponBar, bloodWeaponDuration, 10, 0f)
                .Draw(origin);
        }

        private void DrawDeliriumBar(Vector2 origin, PlayerCharacter player)
        {
            float deliriumDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 1972 && o.RemainingTime > 0f)?.RemainingTime ?? 0f;

            if (Config.DeliriumBar.HideWhenInactive && deliriumDuration is 0) { return; }

            Config.DeliriumBar.Label.SetValue(deliriumDuration);
            BarUtilities.GetProgressBar(Config.DeliriumBar, deliriumDuration, 10, 0f)
                .Draw(origin);
        }

        private void DrawLivingShadowBar(Vector2 origin)
        {
            DRKGauge gauge = Plugin.JobGauges.Get<DRKGauge>();
            if (Config.LivingShadowBar.HideWhenInactive && gauge.ShadowTimeRemaining == 0) { return; }

            float timer = Math.Abs(gauge.ShadowTimeRemaining);
            Config.LivingShadowBar.Label.SetValue(timer / 1000);
            BarUtilities.GetProgressBar(Config.LivingShadowBar, timer, 24000f)
                .Draw(origin);
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Tank", 0)]
    [SubSection("Dark Knight", 1)]
    public class DarkKnightConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.DRK;
        public new static DarkKnightConfig DefaultConfig()
        {
            var config = new DarkKnightConfig();

            config.ManaBar.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.ManaBar.UsePartialFillColor = true;

            config.DarksideBar.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.DarksideBar.ThresholdConfig.Enabled = true;

            config.BloodGauge.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.BloodGauge.UsePartialFillColor = true;

            return config;
        }

        [NestedConfig("Mana Bar", 30)]
        public DarkKnightManaBarConfig ManaBar = new DarkKnightManaBarConfig(
            new Vector2(0, -61),
            new Vector2(254, 10),
            new PluginConfigColor(new Vector4(0f / 255f, 162f / 255f, 252f / 255f, 100f / 100f))
        );

        [NestedConfig("Blood Gauge", 35)]
        public ChunkedProgressBarConfig BloodGauge = new ChunkedProgressBarConfig(
            new Vector2(0, -49),
            new Vector2(254, 10),
            new PluginConfigColor(new Vector4(216f / 255f, 0f / 255f, 73f / 255f, 100f / 100f)),
            2,
            new PluginConfigColor(new Vector4(180f / 255f, 180f / 255f, 180f / 255f, 100f / 100f))
        );

        [NestedConfig("Darkside Bar", 40)]
        public ProgressBarConfig DarksideBar = new ProgressBarConfig(
            new Vector2(0, -73),
            new Vector2(254, 10),
            new PluginConfigColor(new Vector4(209f / 255f, 38f / 255f, 73f / 204f, 100f / 100f)),
            BarDirection.Right,
            new PluginConfigColor(new Vector4(160f / 255f, 0f / 255f, 0f / 255f, 100f / 100f)),
            5
        );

        [NestedConfig("Blood Weapon Bar", 45)]
        public ProgressBarConfig BloodWeaponBar = new ProgressBarConfig(
            new Vector2(-64, -32),
            new Vector2(126, 20),
            new PluginConfigColor(new Vector4(160f / 255f, 0f / 255f, 0f / 255f, 100f / 100f))
        );

        [NestedConfig("Delirium Bar", 50)]
        public ProgressBarConfig DeliriumBar = new ProgressBarConfig(
            new Vector2(64, -32),
            new Vector2(126, 20),
            new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f))
        );

        [NestedConfig("Living Shadow Bar", 55)]
        public ProgressBarConfig LivingShadowBar = new ProgressBarConfig(
            new Vector2(0, -10),
            new Vector2(254, 20),
            new PluginConfigColor(new Vector4(255f / 255f, 105f / 255f, 205f / 255f, 100f / 100f))
        );
    }

    [Exportable(false)]
    public class DarkKnightManaBarConfig : ChunkedProgressBarConfig
    {
        [ColorEdit4("Dark Arts Color" + "##MP")]
        [Order(26)]
        public PluginConfigColor DarkArtsColor = new PluginConfigColor(new Vector4(210f / 255f, 33f / 255f, 33f / 255f, 100f / 100f));

        public DarkKnightManaBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
             : base(position, size, fillColor)
        {
        }
    }
}
