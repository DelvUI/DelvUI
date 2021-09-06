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
using DelvUI.Interface.Bars;
using ImGuiNET;
using Actor = Dalamud.Game.ClientState.Actors.Types.Actor;

namespace DelvUI.Interface
{
    public class BardHudWindow : HudWindow
    {
        public BardHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        public override uint JobId => 23;
        private BardHudConfig _config => (BardHudConfig)ConfigurationManager.GetInstance().GetConfiguration(new BardHudConfig());
        private Vector2 Origin => new(CenterX + _config.Position.X, CenterY + YOffset + _config.Position.Y);
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

                var duration = Math.Abs(cb.Duration);

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

                var duration = Math.Abs(sb.Duration);

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
            var songStacks = gauge.NumSongStacks;
            CurrentSong song = gauge.ActiveSong;
            var songTimer = gauge.SongTimer;

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

            var duration = Math.Abs(songTimer);

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

            var soulVoice = PluginInterface.ClientState.JobGauges.Get<BRDGauge>().SoulVoiceValue;

            Vector2 barSize = _config.SoulGaugeSize;
            Vector2 position = Origin + _config.SoulGaugePosition - barSize / 2f;

            BarBuilder builder = BarBuilder.Create(position, barSize);

            Bar bar = builder.AddInnerBar(soulVoice, 100f, _config.SoulGaugeColor.Map).SetBackgroundColor(EmptyColor["background"]).Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawStacks(int amount, int max, Dictionary<string, uint> stackColor)
        {
            Vector2 barSize = _config.StackSize;
            Vector2 position = Origin + _config.StackPosition - barSize / 2f;

            Bar bar = BarBuilder.Create(position, barSize)
                                .SetChunks(max)
                                .SetChunkPadding(_config.StackPadding)
                                .AddInnerBar(amount, max, stackColor)
                                .SetBackgroundColor(EmptyColor["background"])
                                .Build();

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
        [DragFloat2("Base Offset", min = -4000f, max = 4000f)]
        public Vector2 Position = new(0, 0);

        [Checkbox("Song Gauge Enabled")]
        public bool ShowSongGauge = true;

        [DragFloat2("Song Gauge Size", min = 1f, max = 2000f)]
        public Vector2 SongGaugeSize = new(254, 20);

        [DragFloat2("Song Gauge Position", min = -4000f, max = 4000f)]
        public Vector2 SongGaugePosition = new(0, -23);

        [ColorEdit4("Wanderer's Minuet Color")]
        public PluginConfigColor WMColor = new(new Vector4(158f / 255f, 157f / 255f, 36f / 255f, 100f / 100f));

        [ColorEdit4("Mage's Ballad Color")]
        public PluginConfigColor MBColor = new(new Vector4(143f / 255f, 90f / 255f, 143f / 255f, 100f / 100f));

        [ColorEdit4("Army's Paeon Color")]
        public PluginConfigColor APColor = new(new Vector4(207f / 255f, 205f / 255f, 52f / 255f, 100f / 100f));

        [Checkbox("Soul Gauge Enabled")]
        public bool ShowSoulGauge = true;

        [DragFloat2("Soul Gauge Size", min = 1f, max = 2000f)]
        public Vector2 SoulGaugeSize = new(254, 10);

        [DragFloat2("Soul Gauge Position", min = -4000f, max = 4000f)]
        public Vector2 SoulGaugePosition = new(0, -6);

        [ColorEdit4("Soul Gauge Color")]
        public PluginConfigColor SoulGaugeColor = new(new Vector4(248f / 255f, 227f / 255f, 0f / 255f, 100f / 100f));

        [Checkbox("Wanderer's Minuet Stacks Enabled")]
        public bool ShowWMStacks = true;

        [Checkbox("Mage's Ballad Proc Enabled (n/a yet)")]
        public bool ShowMBProc = true;

        [Checkbox("Army's Paeon Stacks Enabled")]
        public bool ShowAPStacks = true;

        [DragFloat2("Stack Size", min = 1f, max = 2000f)]
        public Vector2 StackSize = new(254, 10);

        [DragFloat2("Stack Position", min = -4000f, max = 4000f)]
        public Vector2 StackPosition = new(0, -40);

        [DragInt("Stack Padding", max = 1000)]
        public int StackPadding = 2;

        [ColorEdit4("Wanderer's Minuet Stack Color")]
        public PluginConfigColor WMStackColor = new(new Vector4(150f / 255f, 215f / 255f, 232f / 255f, 100f / 100f));

        [ColorEdit4("Mage's Ballad Proc Color")]
        public PluginConfigColor MBProcColor = new(new Vector4(199f / 255f, 46f / 255f, 46f / 255f, 100f / 100f));

        [ColorEdit4("Army's Paeon Stack Color")]
        public PluginConfigColor APStackColor = new(new Vector4(0f / 255f, 222f / 255f, 177f / 255f, 100f / 100f));

        [Checkbox("Caustic Bite Enabled")]
        public bool ShowCB = true;

        [Checkbox("Caustic Bite Inverted")]
        public bool CBInverted = true;

        [DragFloat2("Caustic Bite Size", max = 2000f)]
        public Vector2 CBSize = new(126, 10);

        [DragFloat2("Caustic Bite Position", min = -4000f, max = 4000f)]
        public Vector2 CBPosition = new(-64, -52);

        [ColorEdit4("Caustic Bite Color")]
        public PluginConfigColor CBColor = new(new Vector4(182f / 255f, 68f / 255f, 235f / 255f, 100f / 100f));

        [Checkbox("Stormbite Enabled")]
        public bool ShowSB = true;

        [Checkbox("Stormbite Inverted")]
        public bool SBInverted = false;

        [DragFloat2("Stormbite Size", max = 2000f)]
        public Vector2 SBSize = new(126, 10);

        [DragFloat2("Stormbite Position", min = -4000f, max = 4000f)]
        public Vector2 SBPosition = new(64, -52);

        [ColorEdit4("Stormbite Color")]
        public PluginConfigColor SBColor = new(new Vector4(72f / 255f, 117f / 255f, 202f / 255f, 100f / 100f));

        [ColorEdit4("DoT Expire Color")]
        public PluginConfigColor ExpireColor = new(new Vector4(199f / 255f, 46f / 255f, 46f / 255f, 100f / 100f));
    }
}
