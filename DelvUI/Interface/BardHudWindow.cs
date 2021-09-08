using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Structs;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using ImGuiNET;
using Actor = Dalamud.Game.ClientState.Actors.Types.Actor;

namespace DelvUI.Interface
{
    public class BardHudWindow : HudWindow
    {
        private readonly SpellHelper _spellHelper = new();

        public BardHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        public override uint JobId => 23;
        private BardHudConfig _config => (BardHudConfig)ConfigurationManager.GetInstance().GetConfiguration(new BardHudConfig());
        private Vector2 Origin => new(CenterX + _config.Position.X, CenterY + _config.Position.Y);
        private Dictionary<string, uint> EmptyColor => PluginConfiguration.MiscColorMap["empty"];

        protected override void Draw(bool _)
        {
            DrawActiveDots();
            HandleCurrentSong();
            DrawSoulVoiceBar();
        }

        protected override void DrawPrimaryResourceBar() { }

        private void DrawActiveDots()
        {
            if (!_config.ShowCB && !_config.ShowSB)
            {
                return;
            }

            Actor target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;

            if (target is not Chara)
            {
                return;
            }

            Vector2 barSize = _config.CBSize;
            Vector2 position = Origin + _config.CBPosition - barSize / 2f;

            List<Bar> barDrawList = new();

            if (_config.ShowCB)
            {
                StatusEffect cb = target.StatusEffects.FirstOrDefault(
                    o => o.EffectId == 1200 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                      || o.EffectId == 124 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                );

                float duration = Math.Abs(cb.Duration);

                PluginConfigColor color = duration <= 5 ? _config.ExpireColor : _config.CBColor;

                BarBuilder builder = BarBuilder.Create(position, barSize);

                Bar cbBar = builder.AddInnerBar(duration, 30f, color.Map).SetFlipDrainDirection(_config.CBInverted).SetBackgroundColor(EmptyColor["background"]).Build();

                barDrawList.Add(cbBar);
            }

            barSize = _config.SBSize;
            position = Origin + _config.SBPosition - barSize / 2f;

            if (_config.ShowSB)
            {
                StatusEffect sb = target.StatusEffects.FirstOrDefault(
                    o => o.EffectId == 1201 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                      || o.EffectId == 129 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                );

                float duration = Math.Abs(sb.Duration);

                PluginConfigColor color = duration <= 5 ? _config.ExpireColor : _config.SBColor;

                BarBuilder builder = BarBuilder.Create(position, barSize);

                Bar sbBar = builder.AddInnerBar(duration, 30f, color.Map).SetFlipDrainDirection(_config.SBInverted).SetBackgroundColor(EmptyColor["background"]).Build();

                barDrawList.Add(sbBar);
            }

            if (barDrawList.Count <= 0)
            {
                return;
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            foreach (Bar bar in barDrawList)
            {
                bar.Draw(drawList, PluginConfiguration);
            }
        }

        private void HandleCurrentSong()
        {
            BRDGauge gauge = PluginInterface.ClientState.JobGauges.Get<BRDGauge>();
            byte songStacks = gauge.NumSongStacks;
            CurrentSong song = gauge.ActiveSong;
            short songTimer = gauge.SongTimer;

            switch (song)
            {
                case CurrentSong.WANDERER:
                    if (_config.ShowWMStacks)
                    {
                        DrawStacks(songStacks, 3, _config.WMStackColor.Map);
                    }

                    DrawSongTimer(songTimer, _config.WMColor.Map);

                    break;

                case CurrentSong.MAGE:
                    if (_config.ShowMBProc)
                    {
                        DrawBloodletterReady(_config.MBProcColor.Map);
                    }

                    DrawSongTimer(songTimer, _config.MBColor.Map);

                    break;

                case CurrentSong.ARMY:
                    if (_config.ShowAPStacks)
                    {
                        DrawStacks(songStacks, 4, _config.APStackColor.Map);
                    }

                    DrawSongTimer(songTimer, _config.APColor.Map);

                    break;

                case CurrentSong.NONE:
                    DrawSongTimer(0, EmptyColor);

                    break;

                default:
                    DrawSongTimer(0, EmptyColor);

                    break;
            }
        }

        private void DrawBloodletterReady(Dictionary<string, uint> color)
        {
            // I want to draw Bloodletter procs here (just color entire bar red to indicate cooldown is ready).
            // But can't find a way yet to accomplish this.

            if (!_config.ShowMBProc)
            {
                return;
            }

            Vector2 barSize = _config.StackSize;
            Vector2 position = Origin + _config.StackPosition - barSize / 2f;

            BarBuilder builder = BarBuilder.Create(position, barSize);

            int active = _spellHelper.GetSpellCooldown(110) == 0 ? 100 : 0;

            Bar bar = builder.AddInnerBar(active, 100, _config.MBProcColor.Map)
                             .SetBackgroundColor(EmptyColor["background"])
                             .Build();

            if (_config.ShowMBProcGlow && active == 100)
            {
                builder.SetGlowSize(2);
                builder.SetGlowColor(0x88FFFFFF);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawSongTimer(short songTimer, Dictionary<string, uint> songColor)
        {
            if (!_config.ShowSongGauge)
            {
                return;
            }

            Vector2 barSize = _config.SongGaugeSize;
            Vector2 position = Origin + _config.SongGaugePosition - barSize / 2f;

            BarBuilder builder = BarBuilder.Create(position, barSize);

            short duration = Math.Abs(songTimer);

            Bar bar = builder.AddInnerBar(duration / 1000f, 30f, songColor)
                             .SetTextMode(BarTextMode.EachChunk)
                             .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                             .SetBackgroundColor(EmptyColor["background"])
                             .Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawSoulVoiceBar()
        {
            if (!_config.ShowSoulGauge)
            {
                return;
            }

            byte soulVoice = PluginInterface.ClientState.JobGauges.Get<BRDGauge>().SoulVoiceValue;

            Vector2 barSize = _config.SoulGaugeSize;
            Vector2 position = Origin + _config.SoulGaugePosition - barSize / 2f;

            BarBuilder builder = BarBuilder.Create(position, barSize);

            Bar bar = builder.AddInnerBar(soulVoice, 100f, _config.SoulGaugeColor.Map).SetBackgroundColor(EmptyColor["background"]).Build();

            if (_config.ShowSoulGaugeGlow && soulVoice == 100)
            {
                builder.SetGlowSize(2);
                builder.SetGlowColor(0x88FFFFFF);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawStacks(int amount, int max, Dictionary<string, uint> stackColor)
        {
            Vector2 barSize = _config.StackSize;
            Vector2 position = Origin + _config.StackPosition - barSize / 2f;

            BarBuilder builder = BarBuilder.Create(position, barSize);

            Bar bar = builder.SetChunks(max)
                             .SetChunkPadding(_config.StackPadding)
                             .AddInnerBar(amount, max, stackColor)
                             .SetBackgroundColor(EmptyColor["background"])
                             .Build();

            if (_config.ShowWMStacksGlow && amount == 3 && max == 3)
            {
                builder.SetGlowSize(2);
                builder.SetGlowColor(0x88FFFFFF);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Ranged", 0)]
    [SubSection("Bard", 1)]
    public class BardHudConfig : PluginConfigObject
    {
        [ColorEdit4("Army's Paeon Color")]
        [CollapseWith(20, 0)]
        public PluginConfigColor APColor = new(new Vector4(207f / 255f, 205f / 255f, 52f / 255f, 100f / 100f));

        [ColorEdit4("Army's Paeon Stack Color")]
        [Order(65)]
        public PluginConfigColor APStackColor = new(new Vector4(0f / 255f, 222f / 255f, 177f / 255f, 100f / 100f));

        [ColorEdit4("Caustic Bite Color")]
        [CollapseWith(15, 2)]
        public PluginConfigColor CBColor = new(new Vector4(182f / 255f, 68f / 255f, 235f / 255f, 100f / 100f));

        [Checkbox("Caustic Bite Inverted")]
        [CollapseWith(0, 2)]
        public bool CBInverted = true;

        [DragFloat2("Caustic Bite Position", min = -4000f, max = 4000f)]
        [CollapseWith(10, 2)]
        public Vector2 CBPosition = new(-64, 408);

        [DragFloat2("Caustic Bite Size", max = 2000f)]
        [CollapseWith(5, 2)]
        public Vector2 CBSize = new(126, 10);

        [ColorEdit4("DoT Expire Color")]
        [Order(80)]
        public PluginConfigColor ExpireColor = new(new Vector4(199f / 255f, 46f / 255f, 46f / 255f, 100f / 100f));

        [ColorEdit4("Mage's Ballad Color")]
        [CollapseWith(15, 0)]
        public PluginConfigColor MBColor = new(new Vector4(143f / 255f, 90f / 255f, 143f / 255f, 100f / 100f));

        [ColorEdit4("Mage's Ballad Proc Color")]
        [Order(60)]
        public PluginConfigColor MBProcColor = new(new Vector4(199f / 255f, 46f / 255f, 46f / 255f, 100f / 100f));

        [DragFloat2("Base Position", min = -4000f, max = 4000f)]
        [Order(0)]
        public Vector2 Position = new(0, 0);

        [ColorEdit4("Stormbite Color")]
        [CollapseWith(15, 3)]
        public PluginConfigColor SBColor = new(new Vector4(72f / 255f, 117f / 255f, 202f / 255f, 100f / 100f));

        [Checkbox("Stormbite Inverted")]
        [CollapseWith(0, 3)]
        public bool SBInverted = false;

        [DragFloat2("Stormbite Position", min = -4000f, max = 4000f)]
        [CollapseWith(10, 3)]
        public Vector2 SBPosition = new(64, 408);

        [DragFloat2("Stormbite Size", max = 2000f)]
        [CollapseWith(5, 3)]
        public Vector2 SBSize = new(126, 10);

        [Checkbox("Army's Paeon Stacks Enabled")]
        [Order(35)]
        public bool ShowAPStacks = true;

        [Checkbox("Caustic Bite Enabled")]
        [CollapseControl(70, 2)]
        public bool ShowCB = true;

        [Checkbox("Mage's Ballad Proc Enabled")]
        [Order(25)]
        public bool ShowMBProc = true;

        [Checkbox("Mage's Ballad Proc Glow Enabled")]
        [Order(30)]
        public bool ShowMBProcGlow = false;

        [Checkbox("Stormbite Enabled")]
        [CollapseControl(75, 3)]
        public bool ShowSB = true;

        [Checkbox("Song Gauge Enabled")]
        [CollapseControl(5, 0)]
        public bool ShowSongGauge = true;

        [Checkbox("Soul Gauge Enabled")]
        [CollapseControl(10, 1)]
        public bool ShowSoulGauge = true;

        [Checkbox("Soul Gauge Full Glow Enabled")]
        [CollapseWith(0, 1)]
        public bool ShowSoulGaugeGlow = false;

        [Checkbox("Wanderer's Minuet Stacks Enabled")]
        [Order(15)]
        public bool ShowWMStacks = true;

        [Checkbox("Wanderer's Minuet Stacks Glow Enabled")]
        [Order(20)]
        public bool ShowWMStacksGlow = false;

        [DragFloat2("Song Gauge Position", min = -4000f, max = 4000f)]
        [CollapseWith(5, 0)]
        public Vector2 SongGaugePosition = new(0, 437);

        [DragFloat2("Song Gauge Size", min = 1f, max = 2000f)]
        [CollapseWith(0, 0)]
        public Vector2 SongGaugeSize = new(254, 20);

        [ColorEdit4("Soul Gauge Color")]
        [CollapseWith(15, 1)]
        public PluginConfigColor SoulGaugeColor = new(new Vector4(248f / 255f, 227f / 255f, 0f / 255f, 100f / 100f));

        [DragFloat2("Soul Gauge Position", min = -4000f, max = 4000f)]
        [CollapseWith(10, 1)]
        public Vector2 SoulGaugePosition = new(0, 454);

        [DragFloat2("Soul Gauge Size", min = 1f, max = 2000f)]
        [CollapseWith(5, 1)]
        public Vector2 SoulGaugeSize = new(254, 10);

        [DragInt("Stack Padding", max = 1000)]
        [Order(50)]
        public int StackPadding = 2;

        [DragFloat2("Stack Position", min = -4000f, max = 4000f)]
        [Order(45)]
        public Vector2 StackPosition = new(0, 420);

        [DragFloat2("Stack Size", min = 1f, max = 2000f)]
        [Order(40)]
        public Vector2 StackSize = new(254, 10);

        [ColorEdit4("Wanderer's Minuet Color")]
        [CollapseWith(10, 0)]
        public PluginConfigColor WMColor = new(new Vector4(158f / 255f, 157f / 255f, 36f / 255f, 100f / 100f));

        [ColorEdit4("Wanderer's Minuet Stack Color")]
        [Order(55)]
        public PluginConfigColor WMStackColor = new(new Vector4(150f / 255f, 215f / 255f, 232f / 255f, 100f / 100f));
    }
}
