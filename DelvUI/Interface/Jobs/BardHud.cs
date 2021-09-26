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
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;

namespace DelvUI.Interface.Jobs
{
    public class BardHud : JobHud
    {
        private readonly SpellHelper _spellHelper = new();
        private new BardConfig Config => (BardConfig)_config;
        private PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;

        public BardHud(string id, BardConfig config, string? displayName = null) : base(id, config, displayName)
        {
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.ShowSongGauge)
            {
                positions.Add(Config.Position + Config.SongGaugePosition);
                sizes.Add(Config.SongGaugeSize);
            }

            if (Config.ShowSoulGauge)
            {
                positions.Add(Config.Position + Config.SoulGaugePosition);
                sizes.Add(Config.SoulGaugeSize);
            }

            if (Config.ShowAPStacks || Config.ShowEmptyStacks || Config.ShowMBProc || Config.ShowWMStacks)
            {
                positions.Add(Config.Position + Config.StackPosition);
                sizes.Add(Config.StackSize);
            }

            if (Config.ShowCB)
            {
                positions.Add(Config.Position + Config.CBPosition);
                sizes.Add(Config.CBSize);
            }

            if (Config.ShowSB)
            {
                positions.Add(Config.Position + Config.SBPosition);
                sizes.Add(Config.SBSize);
            }

            return (positions, sizes);
        }
        public override void DrawChildren(Vector2 origin)
        {
            if (Config.ShowCB || Config.ShowSB)
            {
                DrawActiveDots(origin);
            }

            HandleCurrentSong(origin);

            if (Config.ShowSoulGauge)
            {
                DrawSoulVoiceBar(origin);
            }
        }

        private void DrawActiveDots(Vector2 origin)
        {
            Debug.Assert(Plugin.ClientState.LocalPlayer != null, "Plugin.ClientState.LocalPlayer != null");
            GameObject? actor = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;

            if (actor is not BattleChara && !Config.CBNoTarget && !Config.SBNoTarget)
            {
                return;
            }

            Vector2 barSize = Config.CBSize;
            Vector2 position = origin + Config.Position + Config.CBPosition - barSize / 2f;

            List<Bar> barDrawList = new();

            if (Config.ShowCB)
            {
                float duration = 0;

                if (actor is BattleChara target)
                {
                    Status? cb = target.StatusList.FirstOrDefault(
                        o => o.StatusId == 1200 && o.SourceID == Plugin.ClientState.LocalPlayer.ObjectId
                          || o.StatusId == 124 && o.SourceID == Plugin.ClientState.LocalPlayer.ObjectId
                    );

                    duration = Math.Abs(cb?.RemainingTime ?? 0f);
                }

                PluginConfigColor color = duration <= 5 ? Config.ExpireColor : Config.CBColor;

                BarBuilder builder = BarBuilder.Create(position, barSize);

                Bar cbBar = builder.AddInnerBar(duration, 30f, color).SetFlipDrainDirection(Config.CBInverted).SetBackgroundColor(EmptyColor.Base).Build();

                if (Config.CBValue)
                {
                    BarTextPosition textPos = Config.CBInverted ? BarTextPosition.CenterRight : BarTextPosition.CenterLeft;
                    builder.SetTextMode(BarTextMode.Single);
                    builder.SetText(textPos, BarTextType.Current);
                }

                if (actor is BattleChara || Config.CBNoTarget)
                {
                    barDrawList.Add(cbBar);
                }
            }

            barSize = Config.SBSize;
            position = origin + Config.Position + Config.SBPosition - barSize / 2f;

            if (Config.ShowSB)
            {
                float duration = 0;

                if (actor is BattleChara target)
                {
                    Status? sb = target.StatusList.FirstOrDefault(
                        o => o.StatusId == 1201 && o.SourceID == Plugin.ClientState.LocalPlayer.ObjectId
                          || o.StatusId == 129 && o.SourceID == Plugin.ClientState.LocalPlayer.ObjectId
                    );

                    duration = Math.Abs(sb?.RemainingTime ?? 0f);
                }

                PluginConfigColor color = duration <= 5 ? Config.ExpireColor : Config.SBColor;

                BarBuilder builder = BarBuilder.Create(position, barSize);

                Bar sbBar = builder.AddInnerBar(duration, 30f, color).SetFlipDrainDirection(Config.SBInverted).SetBackgroundColor(EmptyColor.Base).Build();

                if (Config.SBValue)
                {
                    BarTextPosition textPos = Config.SBInverted ? BarTextPosition.CenterRight : BarTextPosition.CenterLeft;
                    builder.SetTextMode(BarTextMode.Single);
                    builder.SetText(textPos, BarTextType.Current);
                }

                if (actor is BattleChara || Config.SBNoTarget)
                {
                    barDrawList.Add(sbBar);
                }
            }

            if (barDrawList.Count <= 0)
            {
                return;
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            foreach (Bar bar in barDrawList)
            {
                bar.Draw(drawList);
            }
        }

        private void HandleCurrentSong(Vector2 origin)
        {
            BRDGauge gauge = Plugin.JobGauges.Get<BRDGauge>();
            byte songStacks = gauge.Repertoire;
            Song song = gauge.Song;
            short songTimer = gauge.SongTimer;

            switch (song)
            {
                case Song.WANDERER:
                    if (Config.ShowWMStacks)
                    {
                        DrawStacks(origin, songStacks, 3, Config.WMStackColor);
                    }

                    DrawSongTimer(origin, songTimer, Config.WMColor);

                    break;

                case Song.MAGE:
                    if (Config.ShowMBProc)
                    {
                        DrawBloodletterReady(origin, Config.MBProcColor);
                    }

                    DrawSongTimer(origin, songTimer, Config.MBColor);

                    break;

                case Song.ARMY:
                    if (Config.ShowAPStacks)
                    {
                        DrawStacks(origin, songStacks, 4, Config.APStackColor);
                    }

                    DrawSongTimer(origin, songTimer, Config.APColor);

                    break;

                case Song.NONE:
                    if (Config.ShowEmptyStacks)
                    {
                        DrawStacks(origin, 0, 3, Config.APStackColor);
                    }

                    DrawSongTimer(origin, 0, EmptyColor);

                    break;

                default:
                    if (Config.ShowEmptyStacks)
                    {
                        DrawStacks(origin, 0, 3, Config.APStackColor);
                    }

                    DrawSongTimer(origin, 0, EmptyColor);

                    break;
            }
        }

        private void DrawBloodletterReady(Vector2 origin, PluginConfigColor color)
        {
            // I want to draw Bloodletter procs here (just color entire bar red to indicate cooldown is ready).
            // But can't find a way yet to accomplish this.

            if (!Config.ShowMBProc)
            {
                return;
            }

            Vector2 barSize = Config.StackSize;
            Vector2 position = origin + Config.Position + Config.StackPosition - barSize / 2f;

            BarBuilder builder = BarBuilder.Create(position, barSize);

            int active = _spellHelper.GetSpellCooldown(110) == 0 ? 100 : 0;

            Bar bar = builder.AddInnerBar(active, 100, Config.MBProcColor)
                             .SetBackgroundColor(EmptyColor.Base)
                             .Build();

            if (Config.ShowMBProcGlow && active == 100)
            {
                builder.SetGlowSize(2);
                builder.SetGlowColor(0x88FFFFFF);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        private void DrawSongTimer(Vector2 origin, short songTimer, PluginConfigColor songColor)
        {
            if (!Config.ShowSongGauge)
            {
                return;
            }

            Vector2 barSize = Config.SongGaugeSize;
            Vector2 position = origin + Config.Position + Config.SongGaugePosition - barSize / 2f;

            BarBuilder builder = BarBuilder.Create(position, barSize);

            short duration = Math.Abs(songTimer);

            Bar bar = builder.AddInnerBar(duration / 1000f, 30f, songColor)
                             .SetTextMode(BarTextMode.EachChunk)
                             .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                             .SetBackgroundColor(EmptyColor.Base)
                             .Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        private void DrawSoulVoiceBar(Vector2 origin)
        {
            byte soulVoice = Plugin.JobGauges.Get<BRDGauge>().SoulVoice;

            Vector2 barSize = Config.SoulGaugeSize;
            Vector2 position = origin + Config.Position + Config.SoulGaugePosition - barSize / 2f;

            BarBuilder builder = BarBuilder.Create(position, barSize);

            Bar bar = builder.AddInnerBar(soulVoice, 100f, Config.SoulGaugeColor).SetBackgroundColor(EmptyColor.Base).Build();

            if (Config.ShowSoulGaugeGlow && soulVoice == 100)
            {
                builder.SetGlowSize(2);
                builder.SetGlowColor(0x88FFFFFF);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        private void DrawStacks(Vector2 origin, int amount, int max, PluginConfigColor stackColor)
        {
            Vector2 barSize = Config.StackSize;
            Vector2 position = origin + Config.Position + Config.StackPosition - barSize / 2f;

            BarBuilder builder = BarBuilder.Create(position, barSize);

            Bar bar = builder.SetChunks(max)
                             .SetChunkPadding(Config.StackPadding)
                             .AddInnerBar(amount, max, stackColor)
                             .SetBackgroundColor(EmptyColor.Base)
                             .Build();

            if (Config.ShowWMStacksGlow && amount == 3 && max == 3)
            {
                builder.SetGlowSize(2);
                builder.SetGlowColor(0x88FFFFFF);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Ranged", 0)]
    [SubSection("Bard", 1)]
    public class BardConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.BRD;
        public new static BardConfig DefaultConfig() { return new BardConfig(); }

        #region song gauge
        [Checkbox("Song Gauge Enabled", separator = true)]
        [CollapseControl(30, 0)]
        public bool ShowSongGauge = true;

        [DragFloat2("Song Gauge Size", min = 1f, max = 2000f)]
        [CollapseWith(0, 0)]
        public Vector2 SongGaugeSize = new(254, 20);

        [DragFloat2("Song Gauge Position", min = -4000f, max = 4000f)]
        [CollapseWith(5, 0)]
        public Vector2 SongGaugePosition = new(0, -22);

        [ColorEdit4("Wanderer's Minuet Color")]
        [CollapseWith(10, 0)]
        public PluginConfigColor WMColor = new(new Vector4(158f / 255f, 157f / 255f, 36f / 255f, 100f / 100f));

        [ColorEdit4("Mage's Ballad Color")]
        [CollapseWith(15, 0)]
        public PluginConfigColor MBColor = new(new Vector4(143f / 255f, 90f / 255f, 143f / 255f, 100f / 100f));

        [ColorEdit4("Army's Paeon Color")]
        [CollapseWith(20, 0)]
        public PluginConfigColor APColor = new(new Vector4(207f / 255f, 205f / 255f, 52f / 255f, 100f / 100f));
        #endregion

        #region soul gauge
        [Checkbox("Soul Gauge Enabled", separator = true)]
        [CollapseControl(35, 1)]
        public bool ShowSoulGauge = true;

        [Checkbox("Soul Gauge Full Glow Enabled")]
        [CollapseWith(0, 1)]
        public bool ShowSoulGaugeGlow = false;

        [DragFloat2("Soul Gauge Size", min = 1f, max = 2000f)]
        [CollapseWith(5, 1)]
        public Vector2 SoulGaugeSize = new(254, 10);

        [DragFloat2("Soul Gauge Position", min = -4000f, max = 4000f)]
        [CollapseWith(10, 1)]
        public Vector2 SoulGaugePosition = new(0, -5);

        [ColorEdit4("Soul Gauge Color")]
        [CollapseWith(15, 1)]
        public PluginConfigColor SoulGaugeColor = new(new Vector4(248f / 255f, 227f / 255f, 0f / 255f, 100f / 100f));
        #endregion

        #region Song Procs / Stacks
        [Checkbox("Wanderer's Minuet Stacks Enabled", separator = true)]
        [Order(40)]
        public bool ShowWMStacks = true;

        [Checkbox("Wanderer's Minuet Stacks Glow Enabled")]
        [Order(45)]
        public bool ShowWMStacksGlow = false;

        [Checkbox("Mage's Ballad Proc Enabled")]
        [Order(50)]
        public bool ShowMBProc = true;

        [Checkbox("Mage's Ballad Proc Glow Enabled")]
        [Order(55)]
        public bool ShowMBProcGlow = false;

        [Checkbox("Army's Paeon Stacks Enabled")]
        [Order(60)]
        public bool ShowAPStacks = true;

        [Checkbox("Show Empty Stack Area")]
        [Order(65)]
        public bool ShowEmptyStacks = true;

        [DragFloat2("Stack Size", min = 1f, max = 2000f)]
        [Order(70)]
        public Vector2 StackSize = new(254, 10);

        [DragFloat2("Stack Position", min = -4000f, max = 4000f)]
        [Order(75)]
        public Vector2 StackPosition = new(0, -39);

        [DragInt("Stack Padding", max = 1000)]
        [Order(80)]
        public int StackPadding = 2;

        [ColorEdit4("Wanderer's Minuet Stack Color")]
        [Order(85)]
        public PluginConfigColor WMStackColor = new(new Vector4(150f / 255f, 215f / 255f, 232f / 255f, 100f / 100f));

        [ColorEdit4("Mage's Ballad Proc Color")]
        [Order(90)]
        public PluginConfigColor MBProcColor = new(new Vector4(199f / 255f, 46f / 255f, 46f / 255f, 100f / 100f));

        [ColorEdit4("Army's Paeon Stack Color")]
        [Order(95)]
        public PluginConfigColor APStackColor = new(new Vector4(0f / 255f, 222f / 255f, 177f / 255f, 100f / 100f));

        [ColorEdit4("DoT Expire Color")]
        [Order(100)]
        public PluginConfigColor ExpireColor = new(new Vector4(199f / 255f, 46f / 255f, 46f / 255f, 100f / 100f));
        #endregion

        #region caustic bite
        [Checkbox("Caustic Bite Enabled", separator = true)]
        [CollapseControl(105, 2)]
        public bool ShowCB = true;

        [Checkbox("Show Caustic Bite On No Target")]
        [CollapseWith(0, 2)]
        public bool CBNoTarget = true;

        [Checkbox("Caustic Bite Value")]
        [CollapseWith(5, 2)]
        public bool CBValue = true;

        [Checkbox("Caustic Bite Inverted")]
        [CollapseWith(10, 2)]
        public bool CBInverted = true;

        [DragFloat2("Caustic Bite Size", max = 2000f)]
        [CollapseWith(15, 2)]
        public Vector2 CBSize = new(126, 10);

        [DragFloat2("Caustic Bite Position", min = -4000f, max = 4000f)]
        [CollapseWith(20, 2)]
        public Vector2 CBPosition = new(-64, -51);

        [ColorEdit4("Caustic Bite Color")]
        [CollapseWith(25, 2)]
        public PluginConfigColor CBColor = new(new Vector4(182f / 255f, 68f / 255f, 235f / 255f, 100f / 100f));
        #endregion

        #region stormbite
        [Checkbox("Stormbite Enabled", separator = true)]
        [CollapseControl(110, 3)]
        public bool ShowSB = true;

        [Checkbox("Show Stormbite On No Target")]
        [CollapseWith(0, 3)]
        public bool SBNoTarget = true;

        [Checkbox("Stormbite Value")]
        [CollapseWith(5, 3)]
        public bool SBValue = true;

        [Checkbox("Stormbite Inverted")]
        [CollapseWith(10, 3)]
        public bool SBInverted = false;

        [DragFloat2("Stormbite Size", max = 2000f)]
        [CollapseWith(15, 3)]
        public Vector2 SBSize = new(126, 10);

        [DragFloat2("Stormbite Position", min = -4000f, max = 4000f)]
        [CollapseWith(20, 3)]
        public Vector2 SBPosition = new(64, -51);

        [ColorEdit4("Stormbite Color")]
        [CollapseWith(25, 3)]
        public PluginConfigColor SBColor = new(new Vector4(72f / 255f, 117f / 255f, 202f / 255f, 100f / 100f));
        #endregion
    }
}
