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
using System.Linq;
using System.Numerics;

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
        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
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
            var player = Plugin.ClientState.LocalPlayer;
            if (player == null)
            {
                return;
            }

            GameObject? actor = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            if (actor is not BattleChara && !Config.CBNoTarget && !Config.SBNoTarget)
            {
                return;
            }

            Vector2 barSize = Config.CBSize;
            Vector2 position = origin + Config.Position + Config.CBPosition - barSize / 2f;

            List<Bar> barDrawList = new();

            float cbDuration = 0f;
            float sbDuration = 0f;
            
            if (Config.ShowCB)
            {
                if (actor is BattleChara target)
                {
                    var cb = target.StatusList.Where(
                        o => o.StatusId is 1200 or 124 && o.SourceID == player.ObjectId
                    );

                    if (cb.Any())
                    {
                        cbDuration = Math.Abs(cb.First().RemainingTime);
                    }
                }

                PluginConfigColor color = cbDuration <= 5 ? Config.ExpireColor : Config.CBColor;

                BarBuilder builder = BarBuilder.Create(position, barSize);

                Bar cbBar = builder.AddInnerBar(cbDuration, 30f, color).SetFlipDrainDirection(Config.CBInverted).SetBackgroundColor(EmptyColor.Base).Build();

                if (Config.CBValue && cbDuration != 0)
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
                if (actor is BattleChara target)
                {
                    var sb = target.StatusList.Where(
                        o => o.StatusId is 1201 or 129 && o.SourceID == player.ObjectId
                    );

                    if (sb.Any())
                    {
                        sbDuration = Math.Abs(sb.First().RemainingTime);
                    }                    
                }

                PluginConfigColor color = sbDuration <= 5 ? Config.ExpireColor : Config.SBColor;

                BarBuilder builder = BarBuilder.Create(position, barSize);

                Bar sbBar = builder.AddInnerBar(sbDuration, 30f, color).SetFlipDrainDirection(Config.SBInverted).SetBackgroundColor(EmptyColor.Base).Build();

                if (Config.SBValue && sbDuration != 0)
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

            if (barDrawList.Count <= 0 || (Config.OnlyShowDoTsWhenActive && sbDuration == 0 && cbDuration == 0))
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

            if (duration == 0 && Config.OnlyShowSongGaugeWhenActive)
            {
                return;
            }

            builder.AddInnerBar(duration / 1000f, 30f, songColor)
                             .SetBackgroundColor(EmptyColor.Base);

            if (Config.ShowSongTimer && duration != 0)
            {
                builder.SetTextMode(BarTextMode.EachChunk)
                    .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawSoulVoiceBar(Vector2 origin)
        {
            byte soulVoice = Plugin.JobGauges.Get<BRDGauge>().SoulVoice;

            if (soulVoice == 0 && Config.OnlyShowSoulGaugeWhenActive)
            {
                return;
            }

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
        [Checkbox("Song Gauge", separator = true)]
        [Order(30)]
        public bool ShowSongGauge = true;

        [Checkbox("Only Show When Active" + "##Song")]
        [Order(31, collapseWith = nameof(ShowSongGauge))]
        public bool OnlyShowSongGaugeWhenActive = false;

        [Checkbox("Timer" + "##Song")]
        [Order(32, collapseWith = nameof(ShowSongGauge))]
        public bool ShowSongTimer = true;

        [DragFloat2("Position" + "##Song", min = -4000f, max = 4000f)]
        [Order(35, collapseWith = nameof(ShowSongGauge))]
        public Vector2 SongGaugePosition = new(0, -22);

        [DragFloat2("Size" + "##Song", min = 1f, max = 2000f)]
        [Order(40, collapseWith = nameof(ShowSongGauge))]
        public Vector2 SongGaugeSize = new(254, 20);

        [ColorEdit4("Wanderer's Minuet" + "##Song")]
        [Order(45, collapseWith = nameof(ShowSongGauge))]
        public PluginConfigColor WMColor = new(new Vector4(158f / 255f, 157f / 255f, 36f / 255f, 100f / 100f));

        [ColorEdit4("Mage's Ballad" + "##Song")]
        [Order(50, collapseWith = nameof(ShowSongGauge))]
        public PluginConfigColor MBColor = new(new Vector4(143f / 255f, 90f / 255f, 143f / 255f, 100f / 100f));

        [ColorEdit4("Army's Paeon" + "##Song")]
        [Order(55, collapseWith = nameof(ShowSongGauge))]
        public PluginConfigColor APColor = new(new Vector4(207f / 255f, 205f / 255f, 52f / 255f, 100f / 100f));
        #endregion

        #region soul gauge
        [Checkbox("Soul Gauge", separator = true)]
        [Order(60)]
        public bool ShowSoulGauge = true;

        [Checkbox("Only Show When Active" + "##Soul")]
        [Order(61, collapseWith = nameof(ShowSoulGauge))]
        public bool OnlyShowSoulGaugeWhenActive = false;

        [Checkbox("Soul Gauge Full Glow" + "##Soul")]
        [Order(65, collapseWith = nameof(ShowSoulGauge))]
        public bool ShowSoulGaugeGlow = false;

        [DragFloat2("Position" + "##Soul", min = -4000f, max = 4000f)]
        [Order(70, collapseWith = nameof(ShowSoulGauge))]
        public Vector2 SoulGaugePosition = new(0, -5);

        [DragFloat2("Size" + "##Soul", min = 1f, max = 2000f)]
        [Order(75, collapseWith = nameof(ShowSoulGauge))]
        public Vector2 SoulGaugeSize = new(254, 10);

        [ColorEdit4("Color" + "##Soul")]
        [Order(80, collapseWith = nameof(ShowSoulGauge))]
        public PluginConfigColor SoulGaugeColor = new(new Vector4(248f / 255f, 227f / 255f, 0f / 255f, 100f / 100f));
        #endregion

        #region Song Procs / Stacks
        [Checkbox("Wanderer's Minuet Stacks", separator = true)]
        [Order(85)]
        public bool ShowWMStacks = true;

        [Checkbox("Wanderer's Minuet Stacks Glow" + "##Stacks")]
        [Order(90)]
        public bool ShowWMStacksGlow = false;

        [Checkbox("Mage's Ballad Proc" + "##Stacks")]
        [Order(95)]
        public bool ShowMBProc = true;

        [Checkbox("Mage's Ballad Proc Glow" + "##Stacks")]
        [Order(100)]
        public bool ShowMBProcGlow = false;

        [Checkbox("Army's Paeon Stacks" + "##Stacks")]
        [Order(105)]
        public bool ShowAPStacks = true;

        [Checkbox("Show Empty Stack Area" + "##Stacks")]
        [Order(110)]
        public bool ShowEmptyStacks = true;

        [DragFloat2("Position" + "##Stacks", min = -4000f, max = 4000f)]
        [Order(115)]
        public Vector2 StackPosition = new(0, -39);

        [DragFloat2("Size" + "##Stacks", min = 1f, max = 2000f)]
        [Order(120)]
        public Vector2 StackSize = new(254, 10);

        [DragInt("Spacing" + "##Stacks", max = 1000)]
        [Order(125)]
        public int StackPadding = 2;

        [ColorEdit4("Wanderer's Minuet Stack" + "##Stacks")]
        [Order(130)]
        public PluginConfigColor WMStackColor = new(new Vector4(150f / 255f, 215f / 255f, 232f / 255f, 100f / 100f));

        [ColorEdit4("Mage's Ballad Proc" + "##Stacks")]
        [Order(135)]
        public PluginConfigColor MBProcColor = new(new Vector4(199f / 255f, 46f / 255f, 46f / 255f, 100f / 100f));

        [ColorEdit4("Army's Paeon Stack" + "##Stacks")]
        [Order(140)]
        public PluginConfigColor APStackColor = new(new Vector4(0f / 255f, 222f / 255f, 177f / 255f, 100f / 100f));

        [ColorEdit4("DoT Expire" + "##Stacks")]
        [Order(145)]
        public PluginConfigColor ExpireColor = new(new Vector4(199f / 255f, 46f / 255f, 46f / 255f, 100f / 100f));
        #endregion

        #region caustic bite
        [Checkbox("Caustic Bite", separator = true)]
        [Order(150)]
        public bool ShowCB = true;

        [Checkbox("Show On No Target" + "##CB")]
        [Order(155, collapseWith = nameof(ShowCB))]
        public bool CBNoTarget = true;

        [Checkbox("Timer" + "##CB")]
        [Order(160, collapseWith = nameof(ShowCB))]
        public bool CBValue = true;

        [Checkbox("Inverted" + "##CB")]
        [Order(165, collapseWith = nameof(ShowCB))]
        public bool CBInverted = true;

        [DragFloat2("Position" + "##CB", min = -4000f, max = 4000f)]
        [Order(170, collapseWith = nameof(ShowCB))]
        public Vector2 CBPosition = new(-64, -51);

        [DragFloat2("Size" + "##CB", max = 2000f)]
        [Order(175, collapseWith = nameof(ShowCB))]
        public Vector2 CBSize = new(126, 10);

        [ColorEdit4("Color" + "##CB")]
        [Order(180, collapseWith = nameof(ShowCB))]
        public PluginConfigColor CBColor = new(new Vector4(182f / 255f, 68f / 255f, 235f / 255f, 100f / 100f));
        #endregion

        #region stormbite
        [Checkbox("Stormbite", separator = true)]
        [Order(185)]
        public bool ShowSB = true;

        [Checkbox("Show On No Target" + "##SB")]
        [Order(190, collapseWith = nameof(ShowSB))]
        public bool SBNoTarget = true;

        [Checkbox("Timer" + "##SB")]
        [Order(195, collapseWith = nameof(ShowSB))]
        public bool SBValue = true;

        [Checkbox("Inverted" + "##SB")]
        [Order(200, collapseWith = nameof(ShowSB))]
        public bool SBInverted = false;

        [DragFloat2("Position" + "##SB", min = -4000f, max = 4000f)]
        [Order(205, collapseWith = nameof(ShowSB))]
        public Vector2 SBPosition = new(64, -51);

        [DragFloat2("Size" + "##SB", max = 2000f)]
        [Order(210, collapseWith = nameof(ShowSB))]
        public Vector2 SBSize = new(126, 10);

        [ColorEdit4("Color" + "##SB")]
        [Order(215, collapseWith = nameof(ShowSB))]
        public PluginConfigColor SBColor = new(new Vector4(72f / 255f, 117f / 255f, 202f / 255f, 100f / 100f));

        [Checkbox("Only Show DoTs When Active" + "##DoTs", spacing = true)]
        [Order(220)]
        public bool OnlyShowDoTsWhenActive = false;
        #endregion
    }
}
