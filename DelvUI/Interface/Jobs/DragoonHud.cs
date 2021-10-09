using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using Newtonsoft.Json;

namespace DelvUI.Interface.Jobs
{
    public class DragoonHud : JobHud
    {
        private new DragoonConfig Config => (DragoonConfig)_config;

        public DragoonHud(string id, DragoonConfig config, string? displayName = null) : base(id, config, displayName)
        {

        }

        private PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;

        //protected List<uint> GetJobSpecificBuffs()
        //{
        //    uint[] ids =
        //    {
        //        // Dive Ready
        //        1243,
        //        // Life Surge
        //        116, 2175,
        //        // Lance Charge
        //        1864,
        //        // Right Eye
        //        1183, 1453, 1910,
        //        // Disembowel
        //        121, 1914
        //    };

        //    return new List<uint>(ids);
        //}

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.ChaosThrustBar.Enabled)
            {
                positions.Add(Config.Position + Config.ChaosThrustBar.Position);
                sizes.Add(Config.ChaosThrustBar.Size);
            }

            if (Config.DisembowelBar.Enabled)
            {
                positions.Add(Config.Position + Config.DisembowelBar.Position);
                sizes.Add(Config.DisembowelBar.Size);
            }

            if (Config.EyeOfTheDragonBar.Enabled)
            {
                positions.Add(Config.Position + Config.EyeOfTheDragonBar.Position);
                sizes.Add(Config.EyeOfTheDragonBar.Size);
            }

            if (Config.BloodOfTheDragonBar.Enabled)
            {
                positions.Add(Config.Position + Config.BloodOfTheDragonBar.Position);
                sizes.Add(Config.BloodOfTheDragonBar.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            var position = origin + Config.Position;
            if (Config.ChaosThrustBar.Enabled)
            {
                DrawChaosThrustBar(position, player);
            }

            if (Config.DisembowelBar.Enabled)
            {
                DrawDisembowelBar(position, player);
            }

            if (Config.EyeOfTheDragonBar.Enabled)
            {
                DrawEyeOfTheDragonBars(position);
            }

            if (Config.BloodOfTheDragonBar.Enabled)
            {
                DrawBloodOfTheDragonBar(position, player);
            }
        }

        private void DrawChaosThrustBar(Vector2 origin, PlayerCharacter player)
        {
            GameObject? actor = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            float chaosThrustDuration = 0f;

            if (actor is BattleChara target)
            {
                chaosThrustDuration = target.StatusList.FirstOrDefault(o => o.StatusId is 1312 or 118 && o.SourceID == player.ObjectId)?.RemainingTime ?? 0f;
            }

            if (!Config.ChaosThrustBar.HideWhenInactive || chaosThrustDuration > 0f)
            {
                Config.ChaosThrustBar.Label.SetText(Math.Truncate(chaosThrustDuration).ToString());
                BarUtilities.GetProgressBar(Config.ChaosThrustBar, chaosThrustDuration, 24f, 0f, player).Draw(origin);
            }
        }

        private void DrawEyeOfTheDragonBars(Vector2 origin)
        {
            DRGGauge gauge = Plugin.JobGauges.Get<DRGGauge>();

            if (!Config.EyeOfTheDragonBar.HideWhenInactive || gauge.EyeCount > 0)
            {
                BarUtilities.GetChunkedProgressBars(Config.EyeOfTheDragonBar, 2, gauge.EyeCount, 2).Draw(origin);
            }
        }

        private void DrawBloodOfTheDragonBar(Vector2 origin, PlayerCharacter player)
        {
            DRGGauge gauge = Plugin.JobGauges.Get<DRGGauge>();
            int maxTimerMs = 30 * 1000;
            short currTimerMs = gauge.BOTDTimer;

            var config = Config.BloodOfTheDragonBar;
            if (!config.HideWhenInactive || currTimerMs > 0f)
            {
                PluginConfigColor color = gauge.BOTDState == BOTDState.LOTD ? config.LifeOfTheDragonColor : config.BloodOfTheDragonColor;
                config.Label.SetText(Math.Truncate(currTimerMs / 1000f).ToString());
                BarUtilities.GetProgressBar(
                    config,
                    null,
                    new[] { config.Label },
                    currTimerMs,
                    maxTimerMs,
                    0f,
                    player,
                    color)
                .Draw(origin);
            }
            
        }

        private void DrawDisembowelBar(Vector2 origin, PlayerCharacter player)
        {
            float disembowelDuration = player.StatusList.FirstOrDefault(o => o.StatusId is 1914 or 121 && o.RemainingTime > 0f)?.RemainingTime ?? 0f;
            if (!Config.DisembowelBar.HideWhenInactive || disembowelDuration > 0f)
            {
                Config.DisembowelBar.Label.SetText(Math.Truncate(disembowelDuration).ToString());
                BarUtilities.GetProgressBar(Config.DisembowelBar, disembowelDuration, 30f, 0f, player).Draw(origin);
            }
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Melee", 0)]
    [SubSection("Dragoon", 1)]
    public class DragoonConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.DRG;
        public new static DragoonConfig DefaultConfig() { return new DragoonConfig(); }

        [NestedConfig("Chaos Thrust", 30)]
        public ProgressBarConfig ChaosThrustBar = new ProgressBarConfig(
            new(0, -76),
            new(254, 20),
            new(new Vector4(106f / 255f, 82f / 255f, 148f / 255f, 100f / 100f))
        );

        [NestedConfig("Disembowel", 35)]
        public ProgressBarConfig DisembowelBar = new ProgressBarConfig(
            new(0, -54),
            new(254, 20),
            new(new Vector4(244f / 255f, 206f / 255f, 191f / 255f, 100f / 100f))
        );

        [NestedConfig("Eye of the Dragon", 40)]
        public ChunkedBarConfig EyeOfTheDragonBar = new ChunkedBarConfig(
            new(0, -32),
            new(254, 20),
            new(new Vector4(1f, 182f / 255f, 194f / 255f, 100f / 100f))
        );

        [NestedConfig("Blood of the Dragon", 45)]
        public BloodOfTheDragonBar BloodOfTheDragonBar = new BloodOfTheDragonBar(
            new(0, -10),
            new(254, 20),
            new(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 100f / 100f))
        );
    }

    [Exportable(false)]
    public class BloodOfTheDragonBar : BarConfig
    {
        [ColorEdit4("Blood of the Dragon")]
        [Order(45)]
        public PluginConfigColor BloodOfTheDragonColor = new(new Vector4(78f / 255f, 198f / 255f, 238f / 255f, 100f / 100f));

        [ColorEdit4("Life of the Dragon")]
        [Order(50)]
        public PluginConfigColor LifeOfTheDragonColor = new(new Vector4(139f / 255f, 24f / 255f, 24f / 255f, 100f / 100f));

        [NestedConfig("Bar Text", 1000, separator = false, spacing = true)]
        public LabelConfig Label;
        
        public BloodOfTheDragonBar(Vector2 pos, Vector2 size, PluginConfigColor fillColor)
            : base(pos, size, fillColor)
        {
            Label = new LabelConfig(Vector2.Zero, "", DrawAnchor.Center, DrawAnchor.Center);
        }
    }
}
