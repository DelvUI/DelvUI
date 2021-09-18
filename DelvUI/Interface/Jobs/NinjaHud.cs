using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Structs;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
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
using Actor = Dalamud.Game.ClientState.Actors.Types.Actor;

namespace DelvUI.Interface.Jobs
{
    public class NinjaHud : JobHud
    {
        private new NinjaConfig Config => (NinjaConfig)_config;
        private PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;
        private PluginConfigColor PartialFillColor => GlobalColors.Instance.PartialFillColor;

        public NinjaHud(string id, NinjaConfig config, string displayName = null) : base(id, config, displayName)
        {

        }

        private readonly SpellHelper _spellHelper = new();
        private float _oldMudraCooldownInfo;

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.ShowHutonGauge)
            {
                positions.Add(Config.Position + Config.HutonGaugePosition);
                sizes.Add(Config.HutonGaugeSize);
            }

            if (Config.ShowNinkiGauge)
            {
                positions.Add(Config.Position + Config.NinkiGaugePosition);
                sizes.Add(Config.NinkiGaugeSize);
            }

            if (Config.ShowTrickBar || Config.ShowSuitonBar)
            {
                positions.Add(Config.Position + Config.TrickBarPosition);
                sizes.Add(Config.TrickBarSize);
            }

            if (Config.ShowMudraCooldown)
            {
                positions.Add(Config.Position + Config.MudraBarPosition);
                sizes.Add(Config.MudraBarSize);
            }

            return (positions, sizes);
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (Config.ShowMudraCooldown)
            {
                DrawMudraBars(origin);
            }

            if (Config.ShowHutonGauge)
            {
                DrawHutonGauge(origin);
            }

            if (Config.ShowNinkiGauge)
            {
                DrawNinkiGauge(origin);
            }

            if (Config.ShowTrickBar || Config.ShowSuitonBar)
            {
                DrawTrickAndSuitonGauge(origin);
            }
        }

        private void DrawMudraBars(Vector2 origin)
        {
            float xPos = origin.X + Config.Position.X + Config.MudraBarPosition.X - Config.MudraBarSize.X / 2f;
            float yPos = origin.Y + Config.Position.Y + Config.MudraBarPosition.Y - Config.MudraBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.MudraBarSize.Y, Config.MudraBarSize.X);

            // each of the 2 mudra charges has a cooldown of 20s
            float maxMudraCooldown = 40f;
            // get the current cooldown and number of charges on mudras
            float mudraCooldownInfo = _spellHelper.GetSpellCooldown(2259);
            int mudraStacks = _spellHelper.GetStackCount(2, 2259);

            // is the player casting ninjutsu or under kassatsu?
            StatusEffect? mudraBuff = null, kassatsuBuff = null, tcjBuff = null;
            foreach (StatusEffect statusEffect in Plugin.ClientState.LocalPlayer.StatusEffects)
            {
                if (statusEffect.EffectId == 496) { mudraBuff = statusEffect; }
                if (statusEffect.EffectId == 497) { kassatsuBuff = statusEffect; }
                if (statusEffect.EffectId == 1186) { tcjBuff = statusEffect; }
            }

            bool haveMudraBuff = mudraBuff.HasValue;
            bool haveKassatsuBuff = kassatsuBuff.HasValue;
            bool haveTCJBuff = tcjBuff.HasValue;

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
                if (haveMudraBuff)
                {
                    ninjutsuText = GenerateNinjutsuText(mudraBuff.Value.StackCount, haveKassatsuBuff, haveTCJBuff);
                }

                // notice that this approach will never display the third ninjutsu cast under TCJ
                // as TCJ ends before the third ninjutsu is cast
                if (haveTCJBuff)
                {
                    ninjutsuText = GenerateNinjutsuText(tcjBuff.Value.StackCount, haveKassatsuBuff, haveTCJBuff);
                }
                PluginConfigColor barColor = haveTCJBuff ? Config.TCJBarColor : (haveKassatsuBuff ? Config.KassatsuBarColor : Config.MudraBarColor);

                float ninjutsuMaxDuration = haveMudraBuff || haveTCJBuff ? 6f : 15f;
                float duration = haveTCJBuff ? tcjBuff.Value.Duration : haveMudraBuff ? mudraBuff.Value.Duration : haveKassatsuBuff ? kassatsuBuff.Value.Duration : ninjutsuMaxDuration;

                // it seems there is some time before the duration is updated after the buff is obtained
                if (duration < 0)
                {
                    duration = ninjutsuMaxDuration;
                }

                builder.AddInnerBar(duration, ninjutsuMaxDuration, barColor);
                if (Config.ShowNinjutsuText)
                {
                    builder.SetTextMode(BarTextMode.Single)
                           .SetText(BarTextPosition.CenterMiddle, BarTextType.Custom, ninjutsuText);
                }
            }
            else
            {
                // if we are neither casting ninjutsu nor in kassatsu nor in TCJ, show the mudra charges and cooldowns
                builder.SetChunks(2)
                   .SetChunkPadding(Config.MudraBarChunkPadding);
                // show the mudra recharge timer on bars that aren't full
                if (Config.ShowMudraBarText)
                {
                    PluginConfigColor[] chunkColors = { Config.MudraBarColor, Config.MudraBarColor };
                    BarText[] charges = new BarText[2];
                    charges[0] = new BarText(BarTextPosition.CenterMiddle, BarTextType.Remaining);
                    charges[1] = new BarText(BarTextPosition.CenterMiddle, BarTextType.Remaining);
                    if (mudraCooldownInfo < 20)
                    {
                        charges[0] = new BarText(BarTextPosition.CenterMiddle, BarTextType.Custom, "");
                    }
                    if (mudraCooldownInfo == 0)
                    {
                        charges[1] = new BarText(BarTextPosition.CenterMiddle, BarTextType.Custom, "");
                    }

                    BarText[] barTexts = { };
                    builder.AddInnerBar(maxMudraCooldown - mudraCooldownInfo, maxMudraCooldown, chunkColors, PartialFillColor,
                        BarTextMode.EachChunk, charges);
                }
                else
                {
                    builder.AddInnerBar(maxMudraCooldown - mudraCooldownInfo, maxMudraCooldown, Config.MudraBarColor);
                }
            }

            Bar bar = builder.SetBackgroundColor(EmptyColor.Base)
                             .Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
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

        private void DrawHutonGauge(Vector2 origin)
        {
            NINGauge gauge = Plugin.JobGauges.Get<NINGauge>();
            int hutonDurationLeft = (int)Math.Ceiling((float)(gauge.HutonTimeLeft / (double)1000));

            float xPos = origin.X + Config.Position.X + Config.HutonGaugePosition.X - Config.HutonGaugeSize.X / 2f;
            float yPos = origin.Y + Config.Position.Y + Config.HutonGaugePosition.Y - Config.HutonGaugeSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.HutonGaugeSize.Y, Config.HutonGaugeSize.X);
            float maximum = 70f;

            builder.AddInnerBar(Math.Abs(hutonDurationLeft), maximum, hutonDurationLeft > Config.HutonGaugeExpiryThreshold ? Config.HutonGaugeColor : Config.HutonGaugeExpiryColor)
                   .SetBackgroundColor(EmptyColor.Base);

            if (Config.ShowHutonGaugeText)
            {
                builder.SetTextMode(BarTextMode.Single)
                       .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            if (!Config.ShowHutonGaugeBorder)
            {
                builder.SetDrawBorder(false);
            }

            Bar bar = builder.Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        private void DrawNinkiGauge(Vector2 origin)
        {
            NINGauge gauge = Plugin.JobGauges.Get<NINGauge>();

            float xPos = origin.X + Config.Position.X + Config.NinkiGaugePosition.X - Config.NinkiGaugeSize.X / 2f;
            float yPos = origin.Y + Config.Position.Y + Config.NinkiGaugePosition.Y - Config.NinkiGaugeSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.NinkiGaugeSize.Y, Config.NinkiGaugeSize.X);

            if (Config.ChunkNinkiGauge)
            {
                builder.SetChunks(2).SetChunkPadding(Config.NinkiGaugeChunkPadding).AddInnerBar(gauge.Ninki, 100, Config.NinkiGaugeColor, PartialFillColor);
            }
            else
            {
                builder.AddInnerBar(gauge.Ninki, 100, Config.NinkiGaugeColor);
            }

            builder.SetBackgroundColor(EmptyColor.Base);

            if (Config.ShowNinkiGaugeText)
            {
                builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            if (!Config.ShowNinkiGaugeBorder)
            {
                builder.SetDrawBorder(false);
            }

            Bar bar = builder.Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        private void DrawTrickAndSuitonGauge(Vector2 origin)
        {
            float xPos = origin.X + Config.Position.X + Config.TrickBarPosition.X - Config.TrickBarSize.X / 2f;
            float yPos = origin.Y + Config.Position.Y + Config.TrickBarPosition.Y - Config.TrickBarSize.Y / 2f;

            Actor target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.CurrentTarget;
            float trickDuration = 0f;
            const float trickMaxDuration = 15f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.TrickBarSize.Y, Config.TrickBarSize.X);

            if (target is Chara)
            {
                StatusEffect trickStatus = target.StatusEffects.FirstOrDefault(o => o.EffectId == 638 && o.OwnerId == Plugin.ClientState.LocalPlayer.ActorId);
                trickDuration = Math.Max(trickStatus.Duration, 0);
            }

            if (trickDuration != 0)
            {
                builder.AddInnerBar(trickDuration, trickMaxDuration, Config.TrickBarColor);

                if (Config.ShowTrickBarText)
                {
                    builder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
            }

            IEnumerable<StatusEffect> suitonBuff = Plugin.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 507);

            if (suitonBuff.Any() && Config.ShowSuitonBar)
            {
                float suitonDuration = Math.Abs(suitonBuff.First().Duration);
                builder.AddInnerBar(suitonDuration, 20, Config.SuitonBarColor);

                if (Config.ShowSuitonBarText)
                {
                    builder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterRight, BarTextType.Current, Config.SuitonBarColor.Vector, Vector4.UnitW, null);
                }
            }

            Bar bar = builder.Build();
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Melee", 0)]
    [SubSection("Ninja", 1)]
    public class NinjaConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.NIN;
        public new static NinjaConfig DefaultConfig() { return new NinjaConfig(); }

        #region huton gauge
        [Checkbox("Show Huton Gauge")]
        [CollapseControl(30, 0)]
        public bool ShowHutonGauge = true;

        [Checkbox("Show Huton Gauge Text")]
        [CollapseWith(0, 0)]
        public bool ShowHutonGaugeText = true;

        [DragFloat2("Huton Gauge Size", max = 2000f)]
        [CollapseWith(1, 0)]
        public Vector2 HutonGaugeSize = new(254, 20);

        [DragFloat2("Huton Gauge Position", min = -4000f, max = 4000f)]
        [CollapseWith(5, 0)]
        public Vector2 HutonGaugePosition = new(0, -54);

        [ColorEdit4("Huton Gauge Color")]
        [CollapseWith(10, 0)]
        public PluginConfigColor HutonGaugeColor = new(new Vector4(110f / 255f, 197f / 255f, 207f / 255f, 100f / 100f));

        [Checkbox("Show Huton Gauge Expiry")]
        [CollapseWith(11, 0)]
        public bool ShowHutonGaugeExpiry = true;

        [DragFloat("Huton Gauge Expiry Threshold", min = 1f, max = 70f)]
        [CollapseWith(12, 0)]
        public float HutonGaugeExpiryThreshold = 40f;

        [ColorEdit4("Huton Gauge Expiry Color")]
        [CollapseWith(13, 0)]
        public PluginConfigColor HutonGaugeExpiryColor = new(new Vector4(230f / 255f, 33f / 255f, 33f / 255f, 53f / 100f));

        [Checkbox("Show Huton Gauge Border")]
        [CollapseWith(15, 0)]
        public bool ShowHutonGaugeBorder = true;
        #endregion

        #region ninki gauge
        [Checkbox("Show Ninki Gauge")]
        [CollapseControl(35, 1)]
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
        public Vector2 NinkiGaugePosition = new(0, -32);

        [DragFloat("Ninki Gauge Chunk Padding", min = -4000f, max = 4000f)]
        [CollapseWith(20, 1)]
        public float NinkiGaugeChunkPadding = 2;

        [ColorEdit4("Ninki Gauge Color")]
        [CollapseWith(25, 1)]
        public PluginConfigColor NinkiGaugeColor = new(new Vector4(137f / 255f, 82f / 255f, 236f / 255f, 100f / 100f));

        [Checkbox("Show Ninki Gauge Border")]
        [CollapseWith(30, 1)]
        public bool ShowNinkiGaugeBorder = true;
        #endregion

        #region trick / suiton
        [Checkbox("Show Trick Bar")]
        [CollapseControl(40, 2)]
        public bool ShowTrickBar = false;

        [Checkbox("Show Trick Bar Text")]
        [CollapseWith(0, 2)]
        public bool ShowTrickBarText = true;

        [ColorEdit4("Trick Bar Color")]
        [CollapseWith(5, 2)]
        public PluginConfigColor TrickBarColor = new(new Vector4(191f / 255f, 40f / 255f, 0f / 255f, 100f / 100f));

        [Checkbox("Show Suiton Bar")]
        [CollapseControl(45, 3)]
        public bool ShowSuitonBar = false;

        [Checkbox("Show Suiton Bar Text")]
        [CollapseWith(0, 3)]
        public bool ShowSuitonBarText = true;

        [ColorEdit4("Suiton Bar Color")]
        [CollapseWith(5, 3)]
        public PluginConfigColor SuitonBarColor = new(new Vector4(202f / 255f, 228f / 255f, 246f / 242f, 100f / 100f));

        [DragFloat2("Trick/Suiton Bar Size", max = 2000f)]
        [Order(50)]
        public Vector2 TrickBarSize = new(254, 20);

        [DragFloat2("Trick/Suiton Bar Position", min = -4000f, max = 4000f)]
        [Order(55)]
        public Vector2 TrickBarPosition = new(0, -10);
        #endregion

        #region mudra
        [Checkbox("Show Mudra Bars")]
        [CollapseControl(60, 4)]
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
        public Vector2 MudraBarPosition = new(0, -73);

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
        #endregion
    }
}
