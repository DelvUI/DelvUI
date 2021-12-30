using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.Jobs
{
    public class BlueMageHud : JobHud
    {
        private new BlueMageConfig Config => (BlueMageConfig)_config;

        public BlueMageHud(BlueMageConfig config, string? displayName = null) : base(config, displayName)
        {
        }
        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.BleedBar.Enabled)
            {
                positions.Add(Config.Position + Config.BleedBar.Position);
                sizes.Add(Config.BleedBar.Size);
            }

            if (Config.WindburnBar.Enabled)
            {
                positions.Add(Config.Position + Config.WindburnBar.Position);
                sizes.Add(Config.WindburnBar.Size);
            }

            if (Config.SurpanakhaBar.Enabled)
            {
                positions.Add(Config.Position + Config.SurpanakhaBar.Position);
                sizes.Add(Config.SurpanakhaBar.Size);
            }

            if (Config.OffGuardBar.Enabled)
            {
                positions.Add(Config.Position + Config.OffGuardBar.Position);
                sizes.Add(Config.OffGuardBar.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            Vector2 pos = origin + Config.Position;
            if (Config.BleedBar.Enabled)
            {
                DrawBleedBar(pos, player);
            }

            if (Config.WindburnBar.Enabled)
            {
                DrawWindburnBar(pos, player);
            }

            if (Config.SurpanakhaBar.Enabled)
            {
                DrawSurpanakhaBar(pos, player);
            }

            if (Config.OffGuardBar.Enabled)
            {
                DrawOffGuardBar(pos, player);
            }

            if (Config.MoonFluteBar.Enabled)
            {
                DrawMoonFluteBar(pos, player);
            }

            if (Config.SpellAmpBar.Enabled)
            {
                DrawSpellAmpBar(pos, player);
            }

            if (Config.TingleBar.Enabled)
            {
                DrawTingleBar(pos, player);
            }
        }

        private static List<uint> BleedID = new List<uint> { 1714 };
        private static List<float> BleedDurations = new List<float> { 30, 60 };

        protected void DrawBleedBar(Vector2 origin, PlayerCharacter player)
        {
            var target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            BarHud? bar = BarUtilities.GetDoTBar(Config.BleedBar, player, target, BleedID, BleedDurations);
            if (bar != null)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.BleedBar.StrataLevel));
            }
        }

        protected void DrawWindburnBar(Vector2 origin, PlayerCharacter player)
        {
            GameObject? target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            bool dotExists = false;

            if (target != null && target is BattleChara targetChara)
            {
                dotExists = targetChara.StatusList.FirstOrDefault(o => o.SourceID == player.ObjectId && o.StatusId == 1723) != null;
            }

            if (dotExists)
            {
                BarHud? bar = BarUtilities.GetDoTBar(Config.WindburnBar, player, target, 1723, 6f);
                if (bar != null)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.WindburnBar.StrataLevel));
                }
            }
            else
            {
                float featherRainCD = SpellHelper.Instance.GetSpellCooldown(11426);
                float max = 30f;
                float current = max - featherRainCD;

                if (!Config.WindburnBar.HideWhenInactive || current < max)
                {
                    Config.WindburnBar.Label.SetValue(max - current);
                    if (current == max)
                    {
                        Config.WindburnBar.Label.SetText("Ready");
                    }

                    BarHud bar = BarUtilities.GetProgressBar(Config.WindburnBar, current, max, 0f, player);
                    AddDrawActions(bar.GetDrawActions(origin, Config.WindburnBar.StrataLevel));
                }
            }
        }

        protected void DrawSurpanakhaBar(Vector2 origin, PlayerCharacter player)
        {
            float surpanakhaCD = SpellHelper.Instance.GetSpellCooldown(18323);
            float max = 120f;
            float current = max - surpanakhaCD;

            if (!Config.SurpanakhaBar.HideWhenInactive || current < max)
            {
                Config.SurpanakhaBar.Label.SetValue((max - current) % 30);

                BarHud[] bars = BarUtilities.GetChunkedProgressBars(Config.SurpanakhaBar, 4, current, max, 0f, player);
                foreach (BarHud bar in bars)
                {
                    AddDrawActions(bar.GetDrawActions(origin, Config.SurpanakhaBar.StrataLevel));
                }
            }
        }

        protected void DrawOffGuardBar(Vector2 origin, PlayerCharacter player)
        {
            var target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;

            BarHud? bar = BarUtilities.GetDoTBar(Config.OffGuardBar, player, target, 1717, 15f);
            if (bar != null)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.OffGuardBar.StrataLevel));
            }
        }

        protected void DrawMoonFluteBar(Vector2 origin, PlayerCharacter player)
        {
            Status? buff = player.StatusList.FirstOrDefault(o => o.StatusId is 1718 or 1727 && o.RemainingTime > 0f);
            if (!Config.MoonFluteBar.HideWhenInactive || buff is not null)
            {
                var buffColor = buff is not null ? buff.StatusId switch
                {
                    1718 => Config.MoonFluteBar.WaxingCrescentColor,
                    1727 => Config.MoonFluteBar.WaningCrescentColor,
                    _ => Config.MoonFluteBar.WaxingCrescentColor
                } : Config.MoonFluteBar.WaxingCrescentColor;

                float buffDuration = buff?.RemainingTime ?? 0f;

                Config.MoonFluteBar.Label.SetValue(buffDuration);

                BarHud bar = BarUtilities.GetProgressBar(Config.MoonFluteBar, buffDuration, 15f, 0, player, fillColor: buffColor);
                AddDrawActions(bar.GetDrawActions(origin, Config.MoonFluteBar.StrataLevel));
            }
        }

        protected void DrawSpellAmpBar(Vector2 origin, PlayerCharacter player)
        {
            Status? buff = player.StatusList.FirstOrDefault(o => o.StatusId is 2118 or 1716 && o.RemainingTime > 0f);
            if (!Config.SpellAmpBar.HideWhenInactive || buff is not null)
            {
                var buffColor = buff is not null ? buff.StatusId switch
                {
                    2118 => Config.SpellAmpBar.BristleColor,
                    1716 => Config.SpellAmpBar.WhistleColor,
                    _ => Config.SpellAmpBar.BristleColor
                } : Config.SpellAmpBar.BristleColor;

                float buffDuration = buff?.RemainingTime ?? 0f;

                Config.SpellAmpBar.Label.SetValue(buffDuration);

                BarHud bar = BarUtilities.GetProgressBar(Config.SpellAmpBar, buffDuration, 30f, 0, player, fillColor: buffColor);
                AddDrawActions(bar.GetDrawActions(origin, Config.SpellAmpBar.StrataLevel));
            }
        }

        protected void DrawTingleBar(Vector2 origin, PlayerCharacter player)
        {
            BarHud? bar = BarUtilities.GetProcBar(Config.TingleBar, player, 2492, 15f);
            if (bar != null)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.TingleBar.StrataLevel));
            }
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Caster", 0)]
    [SubSection("Blue Mage", 1)]
    public class BlueMageConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.BLU;
        public new static BlueMageConfig DefaultConfig()
        {
            var config = new BlueMageConfig();
            config.UseDefaultPrimaryResourceBar = true;

            return config;
        }
        [NestedConfig("Bleed Bar", 40)]
        public ProgressBarConfig BleedBar = new ProgressBarConfig(
            new(-64, -55),
            new(126, 14),
            new PluginConfigColor(new Vector4(106f / 255f, 237f / 255f, 241f / 255f, 100f / 100f)),
            BarDirection.Left
        );
        [NestedConfig("Windburn Bar", 45)]
        public ProgressBarConfig WindburnBar = new ProgressBarConfig(
            new(64, -55),
            new(126, 14),
            new PluginConfigColor(new Vector4(50f / 255f, 93f / 255f, 37f / 255f, 100f / 100f))
        );
        [NestedConfig("Surpanakha Bar", 50)]
        public ChunkedProgressBarConfig SurpanakhaBar = new ChunkedProgressBarConfig(
            new(0, -39),
            new(254, 14),
            new PluginConfigColor(new Vector4(202f / 255f, 228f / 255f, 246f / 242f, 100f / 100f))

        );
        [NestedConfig("Off-Guard Bar", 55)]
        public ProgressBarConfig OffGuardBar = new ProgressBarConfig(
            new(0, -23),
            new(254, 14),
            new PluginConfigColor(new Vector4(202f / 255f, 228f / 255f, 246f / 242f, 100f / 100f))
        );

        [NestedConfig("Moon Flute Bar", 60)]
        public MoonFluteBarConfig MoonFluteBar = new MoonFluteBarConfig(
            new(0, -7),
            new(84, 14),
            new(new Vector4(128f / 255f, 255f / 255f, 255f / 255f, 100f / 100f))
        );
        [NestedConfig("Spell Amp Bar", 65)]
        public SpellAmpBarConfig SpellAmpBar = new SpellAmpBarConfig(
            new(-86, -7),
            new(82, 14),
            new(new Vector4(128f / 255f, 255f / 255f, 255f / 255f, 100f / 100f))
        );
        [NestedConfig("Tingle Bar", 70)]
        public ProgressBarConfig TingleBar = new ProgressBarConfig(
            new(86, -7),
            new(82, 14),
            new(new Vector4(128f / 255f, 255f / 255f, 255f / 255f, 100f / 100f))
        );
    }

    [Exportable(false)]
    public class MoonFluteBarConfig : ProgressBarConfig
    {
        [ColorEdit4("Waning Crescent Color")]
        [Order(26)]
        public PluginConfigColor WaxingCrescentColor = new(new Vector4(0f / 255f, 255f / 255f, 0f / 255f, 100f / 100f));
        [ColorEdit4("Waxing Crescent Color")]
        [Order(27)]
        public PluginConfigColor WaningCrescentColor = new(new Vector4(255f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));
        public MoonFluteBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
            : base(position, size, fillColor)
        {
        }
    }
    [Exportable(false)]
    public class SpellAmpBarConfig : ProgressBarConfig
    {
        [ColorEdit4("Waning Crescent Color")]
        [Order(26)]
        public PluginConfigColor BristleColor = new(new Vector4(0f / 255f, 255f / 255f, 0f / 255f, 100f / 100f));
        [ColorEdit4("Waxing Crescent Color")]
        [Order(27)]
        public PluginConfigColor WhistleColor = new(new Vector4(255f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));
        public SpellAmpBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor)
            : base(position, size, fillColor)
        {
        }
    }
}
