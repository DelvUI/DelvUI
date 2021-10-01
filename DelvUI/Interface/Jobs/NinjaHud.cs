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
    public class NinjaHud : JobHud
    {
        private new NinjaConfig Config => (NinjaConfig)_config;
        private PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;
        private PluginConfigColor PartialFillColor => GlobalColors.Instance.PartialFillColor;

        public NinjaHud(string id, NinjaConfig config, string? displayName = null) : base(id, config, displayName)
        {

        }

        private readonly SpellHelper _spellHelper = new();
        private float _oldMudraCooldownInfo;

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.HutonBarConfig.Enabled)
            {
                positions.Add(Config.Position + Config.HutonBarConfig.Position);
                sizes.Add(Config.HutonBarConfig.Size);
            }

            if (Config.NinkiBarConfig.Enabled)
            {
                positions.Add(Config.Position + Config.NinkiBarConfig.Position);
                sizes.Add(Config.NinkiBarConfig.Size);
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

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            if (Config.ShowMudraCooldown)
            {
                DrawMudraBars(origin, player);
            }

            if (Config.HutonBarConfig.Enabled)
            {
                DrawHutonGauge(origin);
            }

            if (Config.NinkiBarConfig.Enabled)
            {
                DrawNinkiGauge(origin);
            }

            if (Config.EnableTrickSuitonBar)
            {
                DrawTrickAndSuitonGauge(origin, player);
            }
        }

        private void DrawMudraBars(Vector2 origin, PlayerCharacter player)
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
            Status? mudraBuff = null, kassatsuBuff = null, tcjBuff = null;
            foreach (Status statusEffect in player.StatusList)
            {
                if (statusEffect.StatusId == 496) { mudraBuff = statusEffect; }
                if (statusEffect.StatusId == 497) { kassatsuBuff = statusEffect; }
                if (statusEffect.StatusId == 1186) { tcjBuff = statusEffect; }
            }

            bool haveMudraBuff = mudraBuff is not null;
            bool haveKassatsuBuff = kassatsuBuff is not null;
            bool haveTCJBuff = tcjBuff is not null;

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
                    ninjutsuText = GenerateNinjutsuText(mudraBuff?.StackCount ?? 0, haveKassatsuBuff, haveTCJBuff);
                }

                // notice that this approach will never display the third ninjutsu cast under TCJ
                // as TCJ ends before the third ninjutsu is cast
                if (haveTCJBuff)
                {
                    ninjutsuText = GenerateNinjutsuText(tcjBuff?.StackCount ?? 0, haveKassatsuBuff, haveTCJBuff);
                }
                PluginConfigColor barColor = haveTCJBuff ? Config.TCJBarColor : (haveKassatsuBuff ? Config.KassatsuBarColor : Config.MudraBarColor);

                float ninjutsuMaxDuration = haveMudraBuff || haveTCJBuff ? 6f : 15f;
                float duration = haveTCJBuff ? tcjBuff?.RemainingTime ?? 0f : haveMudraBuff ? mudraBuff?.RemainingTime ?? 0f : haveKassatsuBuff ? kassatsuBuff?.RemainingTime ?? 0f : ninjutsuMaxDuration;

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
            else if ((!inNinjutsu || !haveKassatsuBuff || !haveTCJBuff) && Config.OnlyShowMudraWhenActive)
            {
                return;
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
            float hutonDurationLeft = gauge.HutonTimer / 1000f;
            Bar2 hutonBar = new Bar2(Config.HutonBarConfig);
            hutonBar.SetBarText(((int)hutonDurationLeft).ToString());

            hutonBar.Draw(origin + Config.Position, hutonDurationLeft, 70f, 40f);
        }

        private void DrawNinkiGauge(Vector2 origin)
        {
            NINGauge gauge = Plugin.JobGauges.Get<NINGauge>();
            Bar2 ninkiBar = new Bar2(Config.NinkiBarConfig);
            ninkiBar.SetBarText(((int)gauge.Ninki).ToString());

            ninkiBar.Draw(origin + Config.Position, gauge.Ninki, 100f);
        }

        private void DrawTrickAndSuitonGauge(Vector2 origin, PlayerCharacter player)
        {
            float xPos = origin.X + Config.Position.X + Config.TrickBarPosition.X - Config.TrickBarSize.X / 2f;
            float yPos = origin.Y + Config.Position.Y + Config.TrickBarPosition.Y - Config.TrickBarSize.Y / 2f;

            GameObject? actor = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            float trickDuration = 0f;
            float suitonDuration = 0f;
            const float trickMaxDuration = 15f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.TrickBarSize.Y, Config.TrickBarSize.X);

            if (actor is BattleChara target)
            {
                var trickStatus = target.StatusList.Where(o => o.StatusId is 638 && o.SourceID == player.ObjectId);
                if (trickStatus.Any() && Config.ShowTrickBar)
                {
                    trickDuration = Math.Abs(trickStatus.First().RemainingTime);
                }
            }

            if (trickDuration != 0)
            {
                builder.AddInnerBar(trickDuration, trickMaxDuration, Config.TrickBarColor);

                if (Config.ShowTrickBarText)
                {
                    builder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
            }

            IEnumerable<Status> suitonBuff = player.StatusList.Where(o => o.StatusId == 507);
                        
            if (suitonBuff.Any() && Config.ShowSuitonBar)
            {
                suitonDuration = Math.Abs(suitonBuff.First().RemainingTime);

                builder.AddInnerBar(suitonDuration, 20, Config.SuitonBarColor);

                if (Config.ShowSuitonBarText)
                {
                    builder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterRight, BarTextType.Current, Config.SuitonBarColor.Vector, Vector4.UnitW, null);
                }
            }

            if (trickDuration == 0f && suitonDuration == 0f && Config.OnlyShowTnSWhenActive)
            {
                return;
            }

            Bar bar = builder.Build();
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Melee", 0)]
    [SubSection("Ninja", 1)]
    public class NinjaConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.NIN;
        public new static NinjaConfig DefaultConfig() { return new NinjaConfig(); }

        [NestedConfig("Huton Bar", 30, separator = true)]
        public BarConfig HutonBarConfig = new BarConfig(
                                                new(0, -32),
                                                new(254, 20),
                                                new PluginConfigColor(new Vector4(110f / 255f, 197f / 255f, 207f / 255f, 100f / 100f)),
                                                new PluginConfigColor(new Vector4(230f / 255f, 33f / 255f, 33f / 255f, 53f / 100f)));

        //[Checkbox("Expire" + "##Huton")]
        //[Order(60, collapseWith = nameof(ShowHutonGauge))]
        //public bool ShowHutonGaugeExpiry = true;

        //[DragFloat("Expire Threshold" + "##Huton", min = 1f, max = 70f)]
        //[Order(65, collapseWith = nameof(ShowHutonGaugeExpiry))]
        //public float HutonGaugeExpiryThreshold = 40f;

        //[ColorEdit4("Expire Color" + "##Huton")]
        //[Order(70, collapseWith = nameof(ShowHutonGaugeExpiry))]
        //public PluginConfigColor HutonGaugeExpiryColor = new(new Vector4(230f / 255f, 33f / 255f, 33f / 255f, 53f / 100f));

        [NestedConfig("Ninki Bar", 75, separator = true)]
        public BarConfig NinkiBarConfig = new BarConfig(new(0, -32), new(254, 20), new PluginConfigColor(new Vector4(137f / 255f, 82f / 255f, 236f / 255f, 100f / 100f)));

        #region trick / suiton
        [Checkbox("Trick Attack & Suiton Bar" + "##TnS", separator = true)]
        [Order(125)]
        public bool EnableTrickSuitonBar = true;

        [DragFloat2("Trick Attack & Suiton Position" + "##TnS", min = -4000f, max = 4000f)]
        [Order(130, collapseWith = nameof(EnableTrickSuitonBar))]
        public Vector2 TrickBarPosition = new(0, -10);

        [DragFloat2("Trick Attack & Suiton Size" + "##TnS", max = 2000f)]
        [Order(135, collapseWith = nameof(EnableTrickSuitonBar))]
        public Vector2 TrickBarSize = new(254, 20);

        [Checkbox("Only Show When Active" + "##TnS")]
        [Order(140, collapseWith = nameof(EnableTrickSuitonBar))]
        public bool OnlyShowTnSWhenActive = false;

        [Checkbox("Trick Attack" + "##TnS")]
        [Order(145, collapseWith = nameof(EnableTrickSuitonBar))]
        public bool ShowTrickBar = false;

        [Checkbox("Timer" + "##TnS")]
        [Order(150, collapseWith = nameof(ShowTrickBar))]
        public bool ShowTrickBarText = true;

        [ColorEdit4("Color" + "##TnS")]
        [Order(155, collapseWith = nameof(ShowTrickBar))]
        public PluginConfigColor TrickBarColor = new(new Vector4(191f / 255f, 40f / 255f, 0f / 255f, 100f / 100f));

        [Checkbox("Suiton" + "##TnS")]
        [Order(160, collapseWith = nameof(EnableTrickSuitonBar))]
        public bool ShowSuitonBar = false;

        [Checkbox("Timer" + "##TnS")]
        [Order(165, collapseWith = nameof(ShowSuitonBar))]
        public bool ShowSuitonBarText = true;

        [ColorEdit4("Color" + "##TnS")]
        [Order(170, collapseWith = nameof(ShowSuitonBar))]
        public PluginConfigColor SuitonBarColor = new(new Vector4(202f / 255f, 228f / 255f, 246f / 242f, 100f / 100f));
        #endregion

        #region mudra
        [Checkbox("Mudra" + "##Mudra", separator = true)]
        [Order(175)]
        public bool ShowMudraCooldown = true;

        [Checkbox("Only Show When Active" + "##Mudra")]
        [Order(180, collapseWith = nameof(ShowMudraCooldown))]
        public bool OnlyShowMudraWhenActive = false;

        [Checkbox("Timers" + "##Mudra")]
        [Order(185, collapseWith = nameof(ShowMudraCooldown))]
        public bool ShowMudraBarText = true;

        [Checkbox("Ninjutsu Text" + "##Mudra")]
        [Order(190, collapseWith = nameof(ShowMudraCooldown))]
        public bool ShowNinjutsuText = true;

        [DragFloat2("Position" + "##Mudra", min = -4000f, max = 4000f)]
        [Order(195, collapseWith = nameof(ShowMudraCooldown))]
        public Vector2 MudraBarPosition = new(0, -73);

        [DragFloat2("Size" + "##Mudra", max = 2000f)]
        [Order(200, collapseWith = nameof(ShowMudraCooldown))]
        public Vector2 MudraBarSize = new(254, 10);

        [DragFloat("Spacing" + "##Mudra", min = -4000f, max = 4000f)]
        [Order(205, collapseWith = nameof(ShowMudraCooldown))]
        public float MudraBarChunkPadding = 2;

        [ColorEdit4("Mudra" + "##Mudra")]
        [Order(210, collapseWith = nameof(ShowMudraCooldown))]
        public PluginConfigColor MudraBarColor = new(new Vector4(211 / 255f, 166 / 255f, 75 / 242f, 100f / 100f));

        [ColorEdit4("Kassatsu" + "##Mudra")]
        [Order(215, collapseWith = nameof(ShowMudraCooldown))]
        public PluginConfigColor KassatsuBarColor = new(new Vector4(239 / 255f, 123 / 255f, 222 / 242f, 100f / 100f));

        [ColorEdit4("Ten Chi Jin" + "##Mudra")]
        [Order(220, collapseWith = nameof(ShowMudraCooldown))]
        public PluginConfigColor TCJBarColor = new(new Vector4(181 / 255f, 33 / 255f, 41 / 242f, 100f / 100f));
        #endregion
    }
}
