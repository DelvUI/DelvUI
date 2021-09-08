using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Structs;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Interface.Bars;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Actor = Dalamud.Game.ClientState.Actors.Types.Actor;

namespace DelvUI.Interface
{
    public class DragoonHudWindow : HudWindow
    {
        public DragoonHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        public override uint JobId => Jobs.DRG;
        private readonly DragoonHudConfig _config = (DragoonHudConfig)ConfigurationManager.GetInstance().GetConfiguration(new DragoonHudConfig());
        private Vector2 Origin => new(CenterX + _config.BasePosition.X, CenterY + _config.BasePosition.Y);
        private Dictionary<string, uint> EmptyColor => PluginConfiguration.MiscColorMap["empty"];

        protected override List<uint> GetJobSpecificBuffs()
        {
            uint[] ids =
            {
                // Dive Ready
                1243,
                // Life Surge
                116, 2175,
                // Lance Charge
                1864,
                // Right Eye
                1183, 1453, 1910,
                // Disembowel
                121, 1914
            };

            return new List<uint>(ids);
        }

        protected override void Draw(bool _)
        {
            if (_config.ShowChaosThrustBar)
            {
                DrawChaosThrustBar();
            }

            if (_config.ShowDisembowelBar)
            {
                DrawDisembowelBar();
            }

            if (_config.ShowEyeOfTheDragonBar)
            {
                DrawEyeOfTheDragonBars();
            }

            if (_config.ShowBloodBar)
            {
                DrawBloodOfTheDragonBar();
            }
        }

        // Never draw the mana bar for Dragoons as it's useless.
        protected override void DrawPrimaryResourceBar() { }

        private void DrawChaosThrustBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            Actor target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;
            float duration = 0f;

            if (target is Chara)
            {
                StatusEffect chaosThrust = target.StatusEffects.FirstOrDefault(
                    o => (o.EffectId == 1312 || o.EffectId == 118) && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                );
                duration = Math.Max(0f, chaosThrust.Duration);
            }

            Vector2 cursorPos = Origin + _config.ChaosThrustBarPosition - _config.ChaosThrustBarSize / 2f;
            BarBuilder builder = BarBuilder.Create(cursorPos, _config.ChaosThrustBarSize);
            Bar bar = builder.AddInnerBar(duration, 24f, _config.ChaosThrustBarColor.Map)
                             .SetBackgroundColor(EmptyColor["background"])
                             .Build();

            if (_config.ShowChaosThrustBarText && duration > 0f)
            {
                builder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawEyeOfTheDragonBars()
        {
            DRGGauge gauge = PluginInterface.ClientState.JobGauges.Get<DRGGauge>();

            Vector2 cursorPos = Origin + _config.EyeOfTheDragonBarPosition - _config.EyeOfTheDragonBarSize / 2f;

            byte eyeCount = gauge.EyeCount;

            BarBuilder builder = BarBuilder.Create(cursorPos, _config.EyeOfTheDragonBarSize);
            Bar eyeBars = builder.SetChunks(2)
                                 .SetChunkPadding(_config.EyeOfTheDragonBarPadding)
                                 .AddInnerBar(eyeCount, 2, _config.EyeOfTheDragonColor.Map)
                                 .SetBackgroundColor(EmptyColor["background"])
                                 .Build();
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            eyeBars.Draw(drawList, PluginConfiguration);
        }

        private void DrawBloodOfTheDragonBar()
        {
            DRGGauge gauge = PluginInterface.ClientState.JobGauges.Get<DRGGauge>();

            Vector2 cursorPos = Origin + _config.BloodBarPosition - _config.BloodBarSize / 2f;

            int maxTimerMs = 30 * 1000;
            short currTimerMs = gauge.BOTDTimer;
            PluginConfigColor color = gauge.BOTDState == BOTDState.LOTD ? _config.LifeOfTheDragonColor : _config.BloodOfTheDragonColor;
            BarBuilder builder = BarBuilder.Create(cursorPos, _config.BloodBarSize);
            Bar bar = builder.AddInnerBar(currTimerMs / 1000f, maxTimerMs / 1000f, color.Map)
                             .SetBackgroundColor(EmptyColor["background"])
                             .Build();

            if (_config.ShowBloodBarText)
            {
                builder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawDisembowelBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");

            Vector2 cursorPos = Origin + _config.DisembowelBarPosition - _config.DisembowelBarSize / 2f;

            IEnumerable<StatusEffect> disembowelBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId is 1914 or 121);
            float duration = 0f;
            if (disembowelBuff.Any())
            {
                StatusEffect buff = disembowelBuff.First();
                if (buff.Duration <= 0)
                {
                    duration = 0f;
                }
                else
                {
                    duration = buff.Duration;
                }
            }

            BarBuilder builder = BarBuilder.Create(cursorPos, _config.DisembowelBarSize);
            Bar bar = builder.AddInnerBar(duration, 30f, _config.DisembowelBarColor.Map)
                             .SetBackgroundColor(EmptyColor["background"])
                             .Build();

            if (_config.ShowDisembowelBarText)
            {
                builder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Melee", 0)]
    [SubSection("Dragoon", 1)]
    public class DragoonHudConfig : PluginConfigObject
    {
        [DragFloat2("Base Position", min = -4000f, max = 4000f)]
        [Order(0)]
        public Vector2 BasePosition = new(0, 0);

        #region Chaos Thrust Bar

        [Checkbox("Show Chaos Thrust Bar")]
        [CollapseControl(5, 0)]
        public bool ShowChaosThrustBar = true;

        [DragFloat2("Chaos Thrust Bar Size", max = 2000f)]
        [CollapseWith(0, 0)]
        public Vector2 ChaosThrustBarSize = new(254, 20);

        [DragFloat2("Chaos Thrust Bar Position", min = -4000f, max = 4000f)]
        [CollapseWith(5, 0)]
        public Vector2 ChaosThrustBarPosition = new(0, 383);

        [Checkbox("Show Chaos Bar Thrust Text")]
        [CollapseWith(10, 0)]
        public bool ShowChaosThrustBarText = true;

        [ColorEdit4("Chaos Thrust Bar Color")]
        [CollapseWith(15, 0)]
        public PluginConfigColor ChaosThrustBarColor = new(new Vector4(106f / 255f, 82f / 255f, 148f / 255f, 100f / 100f));

        #endregion

        #region Disembowel Bar

        [Checkbox("Show Disembowel Bar")]
        [CollapseControl(10, 1)]
        public bool ShowDisembowelBar = true;

        [DragFloat2("Disembowel Bar Size", max = 2000f)]
        [CollapseWith(0, 1)]
        public Vector2 DisembowelBarSize = new(254, 20);

        [DragFloat2("Disembowel Bar Position", min = -4000f, max = 4000f)]
        [CollapseWith(5, 1)]
        public Vector2 DisembowelBarPosition = new(0, 405);

        [Checkbox("Show Disembowel Bar Text")]
        [CollapseWith(10, 1)]
        public bool ShowDisembowelBarText = true;

        [ColorEdit4("Disembowel Bar Color")]
        [CollapseWith(15, 1)]
        public PluginConfigColor DisembowelBarColor = new(new Vector4(244f / 255f, 206f / 255f, 191f / 255f, 100f / 100f));

        #endregion

        #region Eye Of The Dragon Bar

        [Checkbox("Show Eye Of The Dragon Bar")]
        [CollapseControl(15, 2)]
        public bool ShowEyeOfTheDragonBar = true;

        [DragFloat2("Eye Of The Dragon Bar Size", max = 2000f)]
        [CollapseWith(0, 2)]
        public Vector2 EyeOfTheDragonBarSize = new(254, 20);

        [DragFloat2("Eye Of The Dragon Bar Position", min = -4000f, max = 4000f)]
        [CollapseWith(5, 2)]
        public Vector2 EyeOfTheDragonBarPosition = new(0, 427);

        [DragInt("Eye Of The Dragon Bar Padding")]
        [CollapseWith(10, 2)]
        public int EyeOfTheDragonBarPadding = 2;

        [ColorEdit4("Eye Of The Dragon Bar Color")]
        [CollapseWith(15, 2)]
        public PluginConfigColor EyeOfTheDragonColor = new(new Vector4(1f, 182f / 255f, 194f / 255f, 100f / 100f));

        #endregion

        #region Blood Bar

        [Checkbox("Show Blood Bar")]
        [CollapseControl(20, 3)]
        public bool ShowBloodBar = true;

        [DragFloat2("Blood Bar Size", max = 2000f)]
        [CollapseWith(0, 3)]
        public Vector2 BloodBarSize = new(254, 20);

        [DragFloat2("Blood Bar Position", min = -4000f, max = 4000f)]
        [CollapseWith(5, 3)]
        public Vector2 BloodBarPosition = new(0, 449);

        [Checkbox("Show Blood Bar Text")]
        [CollapseWith(10, 3)]
        public bool ShowBloodBarText = true;

        [ColorEdit4("Blood Of The Dragon Bar Color")]
        [CollapseWith(15, 3)]
        public PluginConfigColor BloodOfTheDragonColor = new(new Vector4(78f / 255f, 198f / 255f, 238f / 255f, 100f / 100f));

        [ColorEdit4("Life Of The Dragon Bar Color")]
        [CollapseWith(20, 3)]
        public PluginConfigColor LifeOfTheDragonColor = new(new Vector4(139f / 255f, 24f / 255f, 24f / 255f, 100f / 100f));

        #endregion
    }
}
