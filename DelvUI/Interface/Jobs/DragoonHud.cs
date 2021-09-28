using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
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

            if (Config.ShowChaosThrustBar)
            {
                positions.Add(Config.Position + Config.ChaosThrustBarPosition);
                sizes.Add(Config.ChaosThrustBarSize);
            }

            if (Config.ShowDisembowelBar)
            {
                positions.Add(Config.Position + Config.DisembowelBarPosition);
                sizes.Add(Config.DisembowelBarSize);
            }

            if (Config.ShowEyeOfTheDragonBar)
            {
                positions.Add(Config.Position + Config.EyeOfTheDragonBarPosition);
                sizes.Add(Config.EyeOfTheDragonBarSize);
            }

            if (Config.ShowBloodBar)
            {
                positions.Add(Config.Position + Config.BloodBarPosition);
                sizes.Add(Config.BloodBarSize);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            if (Config.ShowChaosThrustBar)
            {
                DrawChaosThrustBar(origin, player);
            }

            if (Config.ShowDisembowelBar)
            {
                DrawDisembowelBar(origin, player);
            }

            if (Config.ShowEyeOfTheDragonBar)
            {
                DrawEyeOfTheDragonBars(origin);
            }

            if (Config.ShowBloodBar)
            {
                DrawBloodOfTheDragonBar(origin);
            }
        }

        private void DrawChaosThrustBar(Vector2 origin, PlayerCharacter player)
        {
            GameObject? actor = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            float duration = 0f;

            if (actor is BattleChara target)
            {
                var chaosThrust = target.StatusList.Where(o => o.StatusId is 1312 or 118 && o.SourceID == player.ObjectId);
                if (chaosThrust.Any())
                {
                    duration = Math.Abs(chaosThrust.First().RemainingTime);
                }
            }

            if (duration == 0f && Config.OnlyShowChaosThrustWhenActive)
            {
                return;
            }

            Vector2 cursorPos = origin + Config.Position + Config.ChaosThrustBarPosition - Config.ChaosThrustBarSize / 2f;
            BarBuilder builder = BarBuilder.Create(cursorPos, Config.ChaosThrustBarSize)
                .SetBackgroundColor(EmptyColor.Base);
            Bar bar = builder.AddInnerBar(duration, 24f, Config.ChaosThrustBarColor)
                             .Build();

            if (Config.ShowChaosThrustBarText && duration > 0f)
            {
                builder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        private void DrawEyeOfTheDragonBars(Vector2 origin)
        {
            DRGGauge gauge = Plugin.JobGauges.Get<DRGGauge>();

            Vector2 cursorPos = origin + Config.Position + Config.EyeOfTheDragonBarPosition - Config.EyeOfTheDragonBarSize / 2f;

            byte eyeCount = gauge.EyeCount;

            if (eyeCount == 0 && Config.OnlyShowEyeOfTheDragonWhenActive)
            {
                return;
            }

            BarBuilder builder = BarBuilder.Create(cursorPos, Config.EyeOfTheDragonBarSize);
            Bar eyeBars = builder.SetChunks(2)
                                 .SetChunkPadding(Config.EyeOfTheDragonBarPadding)
                                 .AddInnerBar(eyeCount, 2, Config.EyeOfTheDragonColor)
                                 .SetBackgroundColor(EmptyColor.Base)
                                 .Build();
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            eyeBars.Draw(drawList);
        }

        private void DrawBloodOfTheDragonBar(Vector2 origin)
        {
            DRGGauge gauge = Plugin.JobGauges.Get<DRGGauge>();

            Vector2 cursorPos = origin + Config.Position + Config.BloodBarPosition - Config.BloodBarSize / 2f;

            int maxTimerMs = 30 * 1000;
            short currTimerMs = gauge.BOTDTimer;
            if (currTimerMs == 0 && Config.OnlyShowBloodBarWhenActive)
            {
                return;
            }

            PluginConfigColor color = gauge.BOTDState == BOTDState.LOTD ? Config.LifeOfTheDragonColor : Config.BloodOfTheDragonColor;
            BarBuilder builder = BarBuilder.Create(cursorPos, Config.BloodBarSize)
                .SetBackgroundColor(EmptyColor.Base);
            Bar bar = builder.AddInnerBar(currTimerMs / 1000f, maxTimerMs / 1000f, color)
                             .Build();

            if (Config.ShowBloodBarText)
            {
                builder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        private void DrawDisembowelBar(Vector2 origin, PlayerCharacter player)
        {
            Vector2 cursorPos = origin + Config.Position + Config.DisembowelBarPosition - Config.DisembowelBarSize / 2f;
            IEnumerable<Status> disembowelBuff = player.StatusList.Where(o => o.StatusId is 1914 or 121);
            float duration = 0f;

            if (disembowelBuff.Any())
            {
                duration = Math.Abs(disembowelBuff.First().RemainingTime);                
            }

            if (duration == 0f && Config.OnlyShowDisembowelWhenActive)
            {
                return;
            }

            BarBuilder builder = BarBuilder.Create(cursorPos, Config.DisembowelBarSize)
                .SetBackgroundColor(EmptyColor.Base);
            Bar bar = builder.AddInnerBar(duration, 30f, Config.DisembowelBarColor)
                             .Build();

            if (Config.ShowDisembowelBarText)
            {
                builder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Melee", 0)]
    [SubSection("Dragoon", 1)]
    public class DragoonConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.DRG;
        public new static DragoonConfig DefaultConfig() { return new DragoonConfig(); }

        #region Chaos Thrust Bar
        [Checkbox("Chaos Thrust" + "##ChaosThrust", separator = true)]
        [Order(30)]
        public bool ShowChaosThrustBar = true;

        [Checkbox("Only Show When Active" + "##ChaosThrust")]
        [Order(35, collapseWith = nameof(ShowChaosThrustBar))]
        public bool OnlyShowChaosThrustWhenActive = false;

        [Checkbox("Timer" + "##ChaosThrust")]
        [Order(40, collapseWith = nameof(ShowChaosThrustBar))]
        public bool ShowChaosThrustBarText = true;

        [DragFloat2("Position" + "##ChaosThrust", min = -4000f, max = 4000f)]
        [Order(45, collapseWith = nameof(ShowChaosThrustBar))]
        public Vector2 ChaosThrustBarPosition = new(0, -76);

        [DragFloat2("Size" + "##ChaosThrust", max = 2000f)]
        [Order(50, collapseWith = nameof(ShowChaosThrustBar))]
        public Vector2 ChaosThrustBarSize = new(254, 20);

        [ColorEdit4("Color" + "##ChaosThrust")]
        [Order(55, collapseWith = nameof(ShowChaosThrustBar))]
        public PluginConfigColor ChaosThrustBarColor = new(new Vector4(106f / 255f, 82f / 255f, 148f / 255f, 100f / 100f));
        #endregion

        #region Disembowel Bar
        [Checkbox("Disembowel" + "##Disembowel", separator = true)]
        [Order(60)]
        public bool ShowDisembowelBar = true;

        [Checkbox("Only Show When Active" + "##Disembowel")]
        [Order(65, collapseWith = nameof(ShowDisembowelBar))]
        public bool OnlyShowDisembowelWhenActive = false;

        [Checkbox("Timer" + "##Disembowel")]
        [Order(70, collapseWith = nameof(ShowDisembowelBar))]
        public bool ShowDisembowelBarText = true;

        [DragFloat2("Position" + "##Disembowel", min = -4000f, max = 4000f)]
        [Order(75, collapseWith = nameof(ShowDisembowelBar))]
        public Vector2 DisembowelBarPosition = new(0, -54);

        [DragFloat2("Size" + "##Disembowel", max = 2000f)]
        [Order(80, collapseWith = nameof(ShowDisembowelBar))]
        public Vector2 DisembowelBarSize = new(254, 20);

        [ColorEdit4("Color" + "##Disembowel")]
        [Order(85, collapseWith = nameof(ShowDisembowelBar))]
        public PluginConfigColor DisembowelBarColor = new(new Vector4(244f / 255f, 206f / 255f, 191f / 255f, 100f / 100f));
        #endregion

        #region Eye Of The Dragon Bar
        [Checkbox("Eye Of The Dragon" + "##EyeOfTheDragon", separator = true)]
        [Order(90)]
        public bool ShowEyeOfTheDragonBar = true;

        [Checkbox("Only Show When Active" + "##EyeOfTheDragon")]
        [Order(95, collapseWith = nameof(ShowEyeOfTheDragonBar))]
        public bool OnlyShowEyeOfTheDragonWhenActive = false;

        [DragFloat2("Position" + "##EyeOfTheDragon", min = -4000f, max = 4000f)]
        [Order(100, collapseWith = nameof(ShowEyeOfTheDragonBar))]
        public Vector2 EyeOfTheDragonBarPosition = new(0, -32);

        [DragFloat2("Size" + "##EyeOfTheDragon", max = 2000f)]
        [Order(105, collapseWith = nameof(ShowEyeOfTheDragonBar))]
        public Vector2 EyeOfTheDragonBarSize = new(254, 20);

        [DragInt("Spacing" + "##EyeOfTheDragon")]
        [Order(110, collapseWith = nameof(ShowEyeOfTheDragonBar))]
        public int EyeOfTheDragonBarPadding = 2;

        [ColorEdit4("Color" + "##EyeOfTheDragon")]
        [Order(115, collapseWith = nameof(ShowEyeOfTheDragonBar))]
        public PluginConfigColor EyeOfTheDragonColor = new(new Vector4(1f, 182f / 255f, 194f / 255f, 100f / 100f));
        #endregion

        #region Blood Bar
        [Checkbox("Show Blood Bar", separator = true)]
        [Order(120)]
        public bool ShowBloodBar = true;

        [Checkbox("Only Show When Active" + "##Blood")]
        [Order(125, collapseWith = nameof(ShowBloodBar))]
        public bool OnlyShowBloodBarWhenActive = false;

        [DragFloat2("Blood Bar Size" + "##Blood", max = 2000f)]
        [Order(130, collapseWith = nameof(ShowBloodBar))]
        public Vector2 BloodBarSize = new(254, 20);

        [DragFloat2("Blood Bar Position" + "##Blood", min = -4000f, max = 4000f)]
        [Order(135, collapseWith = nameof(ShowBloodBar))]
        public Vector2 BloodBarPosition = new(0, -10);

        [Checkbox("Show Blood Bar Text" + "##Blood")]
        [Order(140, collapseWith = nameof(ShowBloodBar))]
        public bool ShowBloodBarText = true;

        [ColorEdit4("Blood Of The Dragon Bar Color" + "##Blood")]
        [Order(145, collapseWith = nameof(ShowBloodBar))]
        public PluginConfigColor BloodOfTheDragonColor = new(new Vector4(78f / 255f, 198f / 255f, 238f / 255f, 100f / 100f));

        [ColorEdit4("Life Of The Dragon Bar Color" + "##Blood")]
        [Order(150, collapseWith = nameof(ShowBloodBar))]
        public PluginConfigColor LifeOfTheDragonColor = new(new Vector4(139f / 255f, 24f / 255f, 24f / 255f, 100f / 100f));
        #endregion
    }
}
