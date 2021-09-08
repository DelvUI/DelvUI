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
    public class NinjaHudWindow : HudWindow
    {
        public override uint JobId => Jobs.NIN;

        private NinjaHudConfig _config => (NinjaHudConfig)ConfigurationManager.GetInstance().GetConfiguration(new NinjaHudConfig());

        private Dictionary<string, uint> EmptyColor => PluginConfiguration.MiscColorMap["empty"];

        private Dictionary<string, uint> NinkiNotFilledColor => PluginConfiguration.MiscColorMap["partial"];

        public NinjaHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        private readonly SpellHelper _spellHelper = new();
        private float _oldMudraCooldownInfo;

        protected override void Draw(bool _)
        {
            if (_config.ShowMudraCooldown)
            {
                DrawMudraBars();
            }

            if (_config.ShowHutonGauge)
            {
                DrawHutonGauge();
            }

            if (_config.ShowNinkiGauge)
            {
                DrawNinkiGauge();
            }

            if (_config.ShowTrickBar || _config.ShowSuitonBar)
            {
                DrawTrickAndSuitonGauge();
            }
        }

        protected override void DrawPrimaryResourceBar() { }

        private void DrawMudraBars()
        {
            float xPos = CenterX + _config.Position.X + _config.MudraBarPosition.X - _config.MudraBarSize.X / 2f;
            float yPos = CenterY + _config.Position.Y + _config.MudraBarPosition.Y - _config.MudraBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, _config.MudraBarSize.Y, _config.MudraBarSize.X);

            // each of the 2 mudra charges has a cooldown of 20s
            float maximum = 40f;
            // get the current cooldown and number of charges on mudras
            float mudraCooldownInfo = _spellHelper.GetSpellCooldown(2259);
            int mudraStacks = _spellHelper.GetStackCount(2, 2259);

            // is the player casting ninjutsu or under kassatsu?
            IEnumerable<StatusEffect> ninjutsuBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 496);
            IEnumerable<StatusEffect> kassatsuBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 497);
            IEnumerable<StatusEffect> tcjBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1186);
            bool haveMudraBuff = ninjutsuBuff.Any();
            bool haveKassatsuBuff = kassatsuBuff.Any();
            bool haveTCJBuff = tcjBuff.Any();
            // for some reason (perhaps a slight delay), the mudras may be on cooldown before the "Mudra" buff is applied
            // hence we check for either
            bool inNinjutsu = mudraStacks == -2 || haveMudraBuff;
            // this ensures that if the cooldown suddenly drops to 0.5s because the player has casted a mudra
            // then the depicted cooldown freezes while the ninjutsu is being casted
            // unfortunately I can't quite get this to work for kassatsu
            // this is really only a problem if we wish to keep showing chunked bars during ninjutsu casts
            if (inNinjutsu)
            {
                mudraCooldownInfo = _oldMudraCooldownInfo;
            }
            else
            {
                _oldMudraCooldownInfo = mudraCooldownInfo;
            }
            // if we are casting ninjutsu then show ninjutsu info
            // if we are in kassatsu, simply show "kassatsu" unless we are casting ninjutsu
            // if we are in TCJ, simply show "ten chi jin" unless we are casting ninjutsu (this overrides kassatsu)
            if (inNinjutsu || haveKassatsuBuff || haveTCJBuff)
            {
                string ninjutsuText = haveKassatsuBuff ? "KASSATSU" : "";
                // determine which ninjutsu is being cast
                // thanks to daemitus for pointing me in this direction
                // NOTE: in ClientStructs it seems that StackCount and Param are switched
                // if this ever breaks -- possibly due to a ClientStructs update -- try swapping them
                if (ninjutsuBuff.Any())
                {
                    ninjutsuText = GenerateNinjutsuText(ninjutsuBuff.First().StackCount, haveKassatsuBuff, haveTCJBuff);
                }
                // notice that this approach will never display the third ninjutsu cast under TCJ
                // as TCJ ends before the third ninjutsu is cast
                if (haveTCJBuff)
                {
                    ninjutsuText = GenerateNinjutsuText(tcjBuff.First().StackCount, haveKassatsuBuff, haveTCJBuff);
                }
                PluginConfigColor barColor = haveTCJBuff ? _config.TCJBarColor : (haveKassatsuBuff ? _config.KassatsuBarColor : _config.MudraBarColor);
                builder.AddInnerBar(maximum, maximum, barColor.Map);
                if (_config.ShowNinjutsuText)
                {
                    builder.SetTextMode(BarTextMode.Single)
                           .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, ninjutsuText);
                }
            }
            else
            {
                // if we are neither casting ninjutsu nor in kassatsu nor in TCJ, show the mudra charges and cooldowns
                _oldMudraCooldownInfo = mudraCooldownInfo;
                builder.SetChunks(2)
                   .SetChunkPadding(_config.MudraBarChunkPadding)
                   .AddInnerBar(maximum - mudraCooldownInfo, maximum, _config.MudraBarColor.Map);
                if (_config.ShowMudraBarText)
                {
                    builder.SetTextMode(BarTextMode.EachChunk)
                           .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
            }

            Bar bar = builder.SetBackgroundColor(EmptyColor["background"])
                             .Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private string GenerateNinjutsuText(byte param, bool haveKassatsuBuff, bool haveTCJBuff)
        {
            return param switch
            {
                1 or 2 or 3 => "FUMA SHURIKEN",
                6 or 7 => haveKassatsuBuff ? "GOKA MEKKYAKU" : "KATON",
                9 or 11 => "RAITON",
                13 or 14 => haveKassatsuBuff ? "HYOSHO RANRYU" : "HYOTON",
                27 or 30 => "HUTON",
                39 or 45 => "DOTON",
                54 or 57 => "SUITON",
                _ => haveTCJBuff ? "TEN CHI JIN" : "NINJUTSU",
            };
        }

        private void DrawHutonGauge()
        {
            NINGauge gauge = PluginInterface.ClientState.JobGauges.Get<NINGauge>();
            int hutonDurationLeft = (int)Math.Ceiling((float)(gauge.HutonTimeLeft / (double)1000));

            float xPos = CenterX + _config.Position.X + _config.HutonGaugePosition.X - _config.HutonGaugeSize.X / 2f;
            float yPos = CenterY + _config.Position.Y + _config.HutonGaugePosition.Y - _config.HutonGaugeSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, _config.HutonGaugeSize.Y, _config.HutonGaugeSize.X);
            float maximum = 70f;

            builder.AddInnerBar(Math.Abs(hutonDurationLeft), maximum, _config.HutonGaugeColor.Map)
                   .SetTextMode(BarTextMode.Single)
                   .SetText(BarTextPosition.CenterMiddle, BarTextType.Current)
                   .SetBackgroundColor(EmptyColor["background"]);

            if (!_config.ShowHutonGaugeBorder)
            {
                builder.SetDrawBorder(false);
            }

            Bar bar = builder.Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawNinkiGauge()
        {
            NINGauge gauge = PluginInterface.ClientState.JobGauges.Get<NINGauge>();

            float xPos = CenterX + _config.Position.X + _config.NinkiGaugePosition.X - _config.NinkiGaugeSize.X / 2f;
            float yPos = CenterY + _config.Position.Y + _config.NinkiGaugePosition.Y - _config.NinkiGaugeSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, _config.NinkiGaugeSize.Y, _config.NinkiGaugeSize.X);

            if (_config.ChunkNinkiGauge)
            {
                builder.SetChunks(2).SetChunkPadding(_config.NinkiGaugeChunkPadding).AddInnerBar(gauge.Ninki, 100, _config.NinkiGaugeColor.Map, NinkiNotFilledColor);
            }
            else
            {
                builder.AddInnerBar(gauge.Ninki, 100, _config.NinkiGaugeColor.Map);
            }

            builder.SetBackgroundColor(EmptyColor["background"]);

            if (_config.ShowNinkiGaugeText)
            {
                builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            if (!_config.ShowNinkiGaugeBorder)
            {
                builder.SetDrawBorder(false);
            }

            Bar bar = builder.Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }

        private void DrawTrickAndSuitonGauge()
        {
            float xPos = CenterX + _config.Position.X + _config.TrickBarPosition.X - _config.TrickBarSize.X / 2f;
            float yPos = CenterY + _config.Position.Y + _config.TrickBarPosition.Y - _config.TrickBarSize.Y / 2f;

            Actor target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;
            float trickDuration = 0f;
            const float trickMaxDuration = 15f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, _config.TrickBarSize.Y, _config.TrickBarSize.X);

            if (target is Chara)
            {
                StatusEffect trickStatus = target.StatusEffects.FirstOrDefault(o => o.EffectId == 638 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId);
                trickDuration = Math.Max(trickStatus.Duration, 0);
            }

            if (trickDuration != 0)
            {
                builder.AddInnerBar(trickDuration, trickMaxDuration, _config.TrickBarColor.Map);

                if (_config.ShowTrickBarText)
                {
                    builder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
            }

            IEnumerable<StatusEffect> suitonBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 507);

            if (suitonBuff.Any() && _config.ShowSuitonBar)
            {
                float suitonDuration = Math.Abs(suitonBuff.First().Duration);
                builder.AddInnerBar(suitonDuration, 20, _config.SuitonBarColor.Map);

                if (_config.ShowSuitonBarText)
                {
                    builder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterRight, BarTextType.Current, _config.SuitonBarColor.Vector, Vector4.UnitW, null);
                }
            }

            Bar bar = builder.Build();
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList, PluginConfiguration);
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Melee", 0)]
    [SubSection("Ninja", 1)]
    public class NinjaHudConfig : PluginConfigObject
    {
        [DragFloat2("Base Position", min = -4000f, max = 4000f)]
        [Order(0)]
        public Vector2 Position = new(0, 0);

        [Checkbox("Show Huton Gauge")]
        [CollapseControl(5, 0)]
        public bool ShowHutonGauge = true;

        [DragFloat2("Huton Gauge Size", max = 2000f)]
        [CollapseWith(0, 0)]
        public Vector2 HutonGaugeSize = new(254, 20);

        [DragFloat2("Huton Gauge Position", min = -4000f, max = 4000f)]
        [CollapseWith(5, 0)]
        public Vector2 HutonGaugePosition = new(0, 427);

        [ColorEdit4("Huton Gauge Color")]
        [CollapseWith(10, 0)]
        public PluginConfigColor HutonGaugeColor = new(new Vector4(110f / 255f, 197f / 255f, 207f / 255f, 100f / 100f));

        [Checkbox("Show Huton Gauge Border")]
        [CollapseWith(15, 0)]
        public bool ShowHutonGaugeBorder = true;

        [Checkbox("Show Ninki Gauge")]
        [CollapseControl(10, 1)]
        public bool ShowNinkiGauge = true;

        [Checkbox("Show Ninki Gauge Text")]
        [CollapseWith(0, 1)]
        public bool ShowNinkiGaugeText = true;

        [Checkbox("Chunk Ninki Gauge")]
        [CollapseWith(5, 1)]
        public bool ChunkNinkiGauge = true;

        [DragFloat2("Ninki Gauge Size", max = 2000f)]
        [CollapseWith(10, 1)]
        public Vector2 NinkiGaugeSize = new(254, 20);

        [DragFloat2("Ninki Gauge Position", min = -4000f, max = 4000f)]
        [CollapseWith(15, 1)]
        public Vector2 NinkiGaugePosition = new(0, 449);

        [DragFloat("Ninki Gauge Chunk Padding", min = -4000f, max = 4000f)]
        [CollapseWith(20, 1)]
        public float NinkiGaugeChunkPadding = 2;

        [ColorEdit4("Ninki Gauge Color")]
        [CollapseWith(25, 1)]
        public PluginConfigColor NinkiGaugeColor = new(new Vector4(137f / 255f, 82f / 255f, 236f / 255f, 100f / 100f));

        [Checkbox("Show Ninki Gauge Border")]
        [CollapseWith(30, 1)]
        public bool ShowNinkiGaugeBorder = true;

        [Checkbox("Show Trick Bar")]
        [CollapseControl(15, 2)]
        public bool ShowTrickBar = false;

        [Checkbox("Show Trick Bar Text")]
        [CollapseWith(0, 2)]
        public bool ShowTrickBarText = true;

        [ColorEdit4("Trick Bar Color")]
        [CollapseWith(5, 2)]
        public PluginConfigColor TrickBarColor = new(new Vector4(191f / 255f, 40f / 255f, 0f / 255f, 100f / 100f));

        [Checkbox("Show Suiton Bar")]
        [CollapseControl(20, 3)]
        public bool ShowSuitonBar = false;

        [Checkbox("Show Suiton Bar Text")]
        [CollapseWith(0, 3)]
        public bool ShowSuitonBarText = true;

        [ColorEdit4("Suiton Bar Color")]
        [CollapseWith(5, 3)]
        public PluginConfigColor SuitonBarColor = new(new Vector4(202f / 255f, 228f / 255f, 246f / 242f, 100f / 100f));

        [DragFloat2("Trick/Suiton Bar Size", max = 2000f)]
        [Order(25)]
        public Vector2 TrickBarSize = new(254, 20);

        [DragFloat2("Trick/Suiton Bar Position", min = -4000f, max = 4000f)]
        [Order(30)]
        public Vector2 TrickBarPosition = new(0, 471);

        [Checkbox("Show Mudra Bars")]
        [CollapseControl(35, 4)]
        public bool ShowMudraCooldown = true;

        [Checkbox("Show Mudra Bar Timers")]
        [CollapseWith(0, 4)]
        public bool ShowMudraBarText = true;

        [Checkbox("Show Ninjutsu Text")]
        [CollapseWith(1, 4)]
        public bool ShowNinjutsuText = true;

        [DragFloat2("Mudra Bar Size", max = 2000f)]
        [CollapseWith(5, 4)]
        public Vector2 MudraBarSize = new(254, 10);

        [DragFloat2("Mudra Bar Position", min = -4000f, max = 4000f)]
        [CollapseWith(10, 4)]
        public Vector2 MudraBarPosition = new(0, 407);

        [DragFloat("Mudra Bar Chunk Padding", min = -4000f, max = 4000f)]
        [CollapseWith(15, 4)]
        public float MudraBarChunkPadding = 2;

        [ColorEdit4("Mudra Bar Color")]
        [CollapseWith(20, 4)]
        public PluginConfigColor MudraBarColor = new(new Vector4(211 / 255f, 166 / 255f, 75 / 242f, 100f / 100f));

        [ColorEdit4("Kassatsu Bar Color")]
        [CollapseWith(25, 4)]
        public PluginConfigColor KassatsuBarColor = new(new Vector4(239 / 255f, 123 / 255f, 222 / 242f, 100f / 100f));

        [ColorEdit4("TCJ Bar Color")]
        [CollapseWith(30, 4)]
        public PluginConfigColor TCJBarColor = new(new Vector4(181 / 255f, 33 / 255f, 41 / 242f, 100f / 100f));
    }
}
