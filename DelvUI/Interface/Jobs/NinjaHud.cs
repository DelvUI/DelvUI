using Dalamud.Game.ClientState.Structs;
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
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;

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
            Debug.Assert(Plugin.ClientState.LocalPlayer != null, "Plugin.ClientState.LocalPlayer != null");

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
            foreach (Status statusEffect in Plugin.ClientState.LocalPlayer.StatusList)
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
                float duration = haveTCJBuff ? tcjBuff?.RemainingTime ?? 0f : haveMudraBuff ? mudraBuff?.RemainingTime ?? 0f: haveKassatsuBuff ? kassatsuBuff?.RemainingTime ?? 0f : ninjutsuMaxDuration;

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
            int hutonDurationLeft = (int)Math.Ceiling((float)(gauge.HutonTimer / (double)1000));

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

            GameObject? actor = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            float trickDuration = 0f;
            const float trickMaxDuration = 15f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.TrickBarSize.Y, Config.TrickBarSize.X);

            if (actor is BattleChara target)
            {
                Debug.Assert(Plugin.ClientState.LocalPlayer != null, "Plugin.ClientState.LocalPlayer != null");
                Status trickStatus = target.StatusList.FirstOrDefault(o => o.StatusId == 638 && o.SourceID == Plugin.ClientState.LocalPlayer.ObjectId);
                trickDuration = Math.Max(trickStatus?.RemainingTime ?? 0f, 0);
            }

            if (trickDuration != 0)
            {
                builder.AddInnerBar(trickDuration, trickMaxDuration, Config.TrickBarColor);

                if (Config.ShowTrickBarText)
                {
                    builder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }
            }

            Debug.Assert(Plugin.ClientState.LocalPlayer != null, "Plugin.ClientState.LocalPlayer != null");
            IEnumerable<Status> suitonBuff = Plugin.ClientState.LocalPlayer.StatusList.Where(o => o.StatusId == 507);

            if (suitonBuff.Any() && Config.ShowSuitonBar)
            {
                float suitonDuration = Math.Abs(suitonBuff.First().RemainingTime);
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

    [Section("Job Specific Bars")]
    [SubSection("Melee", 0)]
    [SubSection("Ninja", 1)]
    public class NinjaConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.NIN;
        public new static NinjaConfig DefaultConfig() { return new NinjaConfig(); }

        #region huton gauge
        [Checkbox("Huton" + "##Huton", separator = true)]
        [CollapseControl(30, 0)]
        public bool ShowHutonGauge = true;

        [Checkbox("Timer" + "##Huton")]
        [CollapseWith(0, 0)]
        public bool ShowHutonGaugeText = true;  
        
        [Checkbox("Border" + "##Huton")]
        [CollapseWith(1, 0)]
        public bool ShowHutonGaugeBorder = true;
        
        [DragFloat2("Position" + "##Huton", min = -4000f, max = 4000f)]
        [CollapseWith(2, 0)]
        public Vector2 HutonGaugePosition = new(0, -54);
        
        [DragFloat2("Size" + "##Huton", max = 2000f)]
        [CollapseWith(3, 0)]
        public Vector2 HutonGaugeSize = new(254, 20);
        
        [ColorEdit4("Color" + "##Huton")]
        [CollapseWith(10, 0)]
        public PluginConfigColor HutonGaugeColor = new(new Vector4(110f / 255f, 197f / 255f, 207f / 255f, 100f / 100f));

        [Checkbox("Expire" + "##Huton")]
        [CollapseWith(11, 0)]
        public bool ShowHutonGaugeExpiry = true;

        [DragFloat("Expire Threshold" + "##Huton", min = 1f, max = 70f)]
        [CollapseWith(12, 0)]
        public float HutonGaugeExpiryThreshold = 40f;

        [ColorEdit4("Expire Color" + "##Huton")]
        [CollapseWith(13, 0)]
        public PluginConfigColor HutonGaugeExpiryColor = new(new Vector4(230f / 255f, 33f / 255f, 33f / 255f, 53f / 100f));


        #endregion

        #region ninki gauge
        [Checkbox("Ninki" + "##Ninki", separator = true)]
        [CollapseControl(35, 1)]
        public bool ShowNinkiGauge = true;

        [Checkbox("Text" + "##Ninki")]
        [CollapseWith(0, 1)]
        public bool ShowNinkiGaugeText = true;
        
        [Checkbox("Border" + "##Ninki")]
        [CollapseWith(5, 1)]
        public bool ShowNinkiGaugeBorder = true;
        
        [Checkbox("Split Bar" + "##Ninki")]
        [CollapseWith(10, 1)]
        public bool ChunkNinkiGauge = true;
        
        [DragFloat2("Position" + "##Ninki", min = -4000f, max = 4000f)]
        [CollapseWith(15, 1)]
        public Vector2 NinkiGaugePosition = new(0, -32);
        
        [DragFloat2("Size" + "##Ninki", max = 2000f)]
        [CollapseWith(20, 1)]
        public Vector2 NinkiGaugeSize = new(254, 20);

        [DragFloat("Spacing" + "##Ninki", min = -4000f, max = 4000f)]
        [CollapseWith(25, 1)]
        public float NinkiGaugeChunkPadding = 2;

        [ColorEdit4("Color" + "##Ninki")]
        [CollapseWith(30, 1)]
        public PluginConfigColor NinkiGaugeColor = new(new Vector4(137f / 255f, 82f / 255f, 236f / 255f, 100f / 100f));
        
        #endregion

        #region trick / suiton
        [DragFloat2("Trick Attack & Suiton Position" + "##TnS", min = -4000f, max = 4000f, separator = true)]
        [Order(40)]
        public Vector2 TrickBarPosition = new(0, -10);
        
        [DragFloat2("Trick Attack & Suiton Size" + "##TnS", max = 2000f)]
        [Order(45)]
        public Vector2 TrickBarSize = new(254, 20);

        [Checkbox("Trick Attack" + "##TnS")]
        [CollapseControl(50, 2)]
        public bool ShowTrickBar = false;

        [Checkbox("Timer" + "##TnS")]
        [CollapseWith(0, 2)]
        public bool ShowTrickBarText = true;

        [ColorEdit4("Color" + "##TnS")]
        [CollapseWith(5, 2)]
        public PluginConfigColor TrickBarColor = new(new Vector4(191f / 255f, 40f / 255f, 0f / 255f, 100f / 100f));

        [Checkbox("Suiton" + "##TnS")]
        [CollapseControl(55, 3)]
        public bool ShowSuitonBar = false;

        [Checkbox("Timer" + "##TnS")]
        [CollapseWith(0, 3)]
        public bool ShowSuitonBarText = true;

        [ColorEdit4("Color" + "##TnS")]
        [CollapseWith(5, 3)]
        public PluginConfigColor SuitonBarColor = new(new Vector4(202f / 255f, 228f / 255f, 246f / 242f, 100f / 100f));
        #endregion

        #region mudra
        [Checkbox("Mudra" + "##Mudra", separator = true)]
        [CollapseControl(60, 4)]
        public bool ShowMudraCooldown = true;

        [Checkbox("Timers" + "##Mudra")]
        [CollapseWith(0, 4)]
        public bool ShowMudraBarText = true;

        [Checkbox("Ninjutsu Text" + "##Mudra")]
        [CollapseWith(1, 4)]
        public bool ShowNinjutsuText = true;

        [DragFloat2("Position" + "##Mudra", min = -4000f, max = 4000f)]
        [CollapseWith(5, 4)]
        public Vector2 MudraBarPosition = new(0, -73);
        
        [DragFloat2("Size" + "##Mudra", max = 2000f)]
        [CollapseWith(10, 4)]
        public Vector2 MudraBarSize = new(254, 10);
        
        [DragFloat("Spacing" + "##Mudra", min = -4000f, max = 4000f)]
        [CollapseWith(15, 4)]
        public float MudraBarChunkPadding = 2;

        [ColorEdit4("Mudra" + "##Mudra")]
        [CollapseWith(20, 4)]
        public PluginConfigColor MudraBarColor = new(new Vector4(211 / 255f, 166 / 255f, 75 / 242f, 100f / 100f));

        [ColorEdit4("Kassatsu" + "##Mudra")]
        [CollapseWith(25, 4)]
        public PluginConfigColor KassatsuBarColor = new(new Vector4(239 / 255f, 123 / 255f, 222 / 242f, 100f / 100f));

        [ColorEdit4("Ten Chi Jin" + "##Mudra")]
        [CollapseWith(30, 4)]
        public PluginConfigColor TCJBarColor = new(new Vector4(181 / 255f, 33 / 255f, 41 / 242f, 100f / 100f));
        #endregion
    }
}
