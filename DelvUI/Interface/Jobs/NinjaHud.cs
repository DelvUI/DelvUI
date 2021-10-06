using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.Jobs
{
    public class NinjaHud : JobHud
    {
        private new NinjaConfig Config => (NinjaConfig)_config;

        public NinjaHud(string id, NinjaConfig config, string? displayName = null) : base(id, config, displayName) { }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.HutonBar.Enabled)
            {
                positions.Add(Config.Position + Config.HutonBar.Position);
                sizes.Add(Config.HutonBar.Size);
            }

            if (Config.NinkiBar.Enabled)
            {
                positions.Add(Config.Position + Config.NinkiBar.Position);
                sizes.Add(Config.NinkiBar.Size);
            }

            if (Config.TrickAttackBar.Enabled)
            {
                positions.Add(Config.Position + Config.TrickAttackBar.Position);
                sizes.Add(Config.TrickAttackBar.Size);
            }

            if (Config.SuitonBar.Enabled)
            {
                positions.Add(Config.Position + Config.SuitonBar.Position);
                sizes.Add(Config.SuitonBar.Size);
            }

            if (Config.MudraBar.Enabled)
            {
                positions.Add(Config.Position + Config.MudraBar.Position);
                sizes.Add(Config.MudraBar.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            if (Config.MudraBar.Enabled)
            {
                DrawMudraBars(origin, player);
            }

            if (Config.HutonBar.Enabled)
            {
                DrawHutonGauge(origin, player);
            }

            if (Config.NinkiBar.Enabled)
            {
                DrawNinkiGauge(origin, player);
            }

            if (Config.TrickAttackBar.Enabled)
            {
                DrawTrickAttackBar(origin, player);
            }

            if (Config.SuitonBar.Enabled)
            {
                DrawSuitonBar(origin, player);
            }
        }

        private void DrawMudraBars(Vector2 origin, PlayerCharacter player)
        {
            var (hasNinjutsuBuff, hasKassatsuBuff, hasTCJBuff) =
                Config.MudraBar.GetMudraBuffs(player, out Status? ninjutsuBuff, out Status? kassatsuBuff, out Status? tcjBuff);

            int mudraStacks = SpellHelper.Instance.GetStackCount(2, 2259);
            float mudraCooldown = SpellHelper.Instance.GetSpellCooldown(2259);

            float current = 0f;
            float max = 0f;

            // For some reason, the mudras may be on cooldown before the "Mudra" buff is applied.
            // Mudra stack count is set to -2 when a mudra is in the middle of its re-cast timer, so we can check for that instead.
            bool inNinjutsu = mudraStacks == -2 || hasNinjutsuBuff;

            if (hasTCJBuff || hasKassatsuBuff || inNinjutsu)
            { 
                if (hasTCJBuff)
                {
                    max = 6f;
                    current = tcjBuff is null || tcjBuff.RemainingTime < 0 ? max : tcjBuff.RemainingTime;
                    Config.MudraBar.MudraLabelConfig.SetText(GenerateNinjutsuText(tcjBuff?.StackCount ?? 0, hasKassatsuBuff, hasTCJBuff));
                }
                else if (hasKassatsuBuff)
                {
                    max = 15f;
                    current = kassatsuBuff is null || kassatsuBuff.RemainingTime < 0 ? max : kassatsuBuff.RemainingTime;
                    Config.MudraBar.MudraLabelConfig.SetText("KASSATSU");
                }

                if (inNinjutsu)
                {
                    max = 6f;
                    current = ninjutsuBuff is null || ninjutsuBuff.RemainingTime < 0 ? max : ninjutsuBuff.RemainingTime;
                    Config.MudraBar.MudraLabelConfig.SetText(GenerateNinjutsuText(ninjutsuBuff?.StackCount ?? 0, hasKassatsuBuff, hasTCJBuff));
                }
            }
            else
            {
                max = 40f;
                current = max - mudraCooldown;
                Config.MudraBar.MudraLabelConfig.SetText(((max - current) % 20).ToString("N0"));
            }

            Config.MudraBar.GetBars(current, max, 0f, player).DrawBars(origin + Config.Position);
        }

        private void DrawHutonGauge(Vector2 origin, PlayerCharacter player)
        {
            NINGauge gauge = Plugin.JobGauges.Get<NINGauge>();
            float hutonDurationLeft = gauge.HutonTimer / 1000f;
            Config.HutonBar.LabelConfig.SetText(hutonDurationLeft.ToString("N0"));
            Config.HutonBar.GetBars(hutonDurationLeft, 70f, 0f, player).DrawBars(origin + Config.Position);
        }


        private void DrawNinkiGauge(Vector2 origin, PlayerCharacter player)
        {
            NINGauge gauge = Plugin.JobGauges.Get<NINGauge>();
            Config.NinkiBar.LabelConfig.SetText(gauge.Ninki.ToString("N0"));
            Config.NinkiBar.GetBars(gauge.Ninki, 100f, 0f, player).DrawBars(origin + Config.Position);
        }

        private void DrawTrickAttackBar(Vector2 origin, PlayerCharacter player)
        {
            GameObject? actor = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            float trickDuration = 0f;
            const float trickMaxDuration = 15f;

            if (actor is BattleChara target)
            {
                trickDuration = target.StatusList
                    .Where(o => o.StatusId is 638 && o.SourceID == player.ObjectId)
                    .Select(o => Math.Abs(o.RemainingTime))
                    .FirstOrDefault();
            }

            Config.TrickAttackBar.LabelConfig.SetText(trickDuration.ToString("N0"));
            Config.TrickAttackBar.GetBars(trickDuration, trickMaxDuration, 0f, player).DrawBars(origin + Config.Position);
        }

        private void DrawSuitonBar(Vector2 origin, PlayerCharacter player)
        {
            float suitonDuration = player.StatusList.Where(o => o.StatusId == 507).Select(o => Math.Abs(o.RemainingTime)).FirstOrDefault();
            Config.SuitonBar.LabelConfig.SetText(suitonDuration.ToString("N0"));
            Config.SuitonBar.GetBars(suitonDuration, 20f, 0f, player).DrawBars(origin + Config.Position);
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
                _ => haveTCJBuff ? "TEN CHI JIN" : "",
            };
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Melee", 0)]
    [SubSection("Ninja", 1)]
    public class NinjaConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.NIN;

        public NinjaConfig()
        {
            // Setup initial bar config
            TrickAttackBar.Enabled = false;
            SuitonBar.Enabled = false;
            HutonBar.Threshold = true;
        }

        public new static NinjaConfig DefaultConfig() { return new NinjaConfig(); }


        [NestedConfig("Mudra Bar", 30)]
        public MudraBarConfig MudraBar = new MudraBarConfig(
                                                            new(0, -50),
                                                            new(254, 10),
                                                            new PluginConfigColor(new Vector4(211 / 255f, 166 / 255f, 75 / 242f, 100f / 100f)));

        [NestedConfig("Huton Bar", 35, separator = true)]
        public ThresholdBarConfig HutonBar = new ThresholdBarConfig(
                                                            new(0, -10),
                                                            new(254, 20),
                                                            new PluginConfigColor(new Vector4(110f / 255f, 197f / 255f, 207f / 255f, 100f / 100f)),
                                                            new PluginConfigColor(new Vector4(230f / 255f, 33f / 255f, 33f / 255f, 53f / 100f)),
                                                            40f);

        [NestedConfig("Ninki Bar", 40, separator = true)]
        public ThresholdBarConfig NinkiBar = new ThresholdBarConfig(
                                                            new(0, -32), 
                                                            new(254, 20), 
                                                            new PluginConfigColor(new Vector4(137f / 255f, 82f / 255f, 236f / 255f, 100f / 100f)));

        [NestedConfig("Trick Attack Bar", 45, separator = true)]
        public ThresholdBarConfig TrickAttackBar = new ThresholdBarConfig(
                                                            new(0, -63),
                                                            new(254, 10),
                                                            new PluginConfigColor(new Vector4(191f / 255f, 40f / 255f, 0f / 255f, 100f / 100f)));

        [NestedConfig("Suiton Bar", 50, separator = true)]
        public ThresholdBarConfig SuitonBar = new ThresholdBarConfig(
                                                            new(0, -75),
                                                            new(254, 10),
                                                            new PluginConfigColor(new Vector4(202f / 255f, 228f / 255f, 246f / 242f, 100f / 100f)));
    }

    [Portable(false)]
    public class MudraBarConfig : BarConfigBase
    {
        [DragInt("Split Bar Spacing", min = 0, max = 4000, spacing = true)]
        [Order(50)]
        public int MudraBarChunkPadding = 2;

        [ColorEdit4("Kassatsu Color")]
        [Order(55)]
        public PluginConfigColor KassatsuBarColor = new(new Vector4(239 / 255f, 123 / 255f, 222 / 242f, 100f / 100f));

        [ColorEdit4("Ten Chi Jin Color")]
        [Order(60)]
        public PluginConfigColor TCJBarColor = new(new Vector4(181 / 255f, 33 / 255f, 41 / 242f, 100f / 100f));

        [NestedConfig("Mudra Bar Text", 65, separator = false, spacing = true)]
        public LabelConfig MudraLabelConfig;

        public MudraBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor) : base(position, size, fillColor)
        {
            MudraLabelConfig = new LabelConfig(new Vector2(0, 0), "", DrawAnchor.Center, DrawAnchor.Center);
        }

        public override bool IsActive(float current, float max, float min)
        {
            return current < max && current >= min;
        }

        public override BarHud[] GetBars(float current, float max, float min = 0f, GameObject? actor = null)
        {
            if (actor is not PlayerCharacter player)
            {
                return Array.Empty<BarHud>();
            }

            var (hasNinjutsuBuff, hasKassatsuBuff, hasTCJBuff) = GetMudraBuffs(player, out Status? mudraBuff, out Status? kassatsuBuff, out Status? tcjBuff);
            bool inNinjutsu = SpellHelper.Instance.GetStackCount(2, 2259) == -2 || hasNinjutsuBuff;

            BarHud[] bars;
            if (hasTCJBuff || hasKassatsuBuff || inNinjutsu)
            {
                PluginConfigColor fillColor = hasTCJBuff ? TCJBarColor : hasKassatsuBuff ? KassatsuBarColor : FillColor;
                Rect background = new Rect(Position, Size, BackgroundColor);
                Rect foreground = Rect.GetFillRect(Position, Size, FillDirection, fillColor, current, max, min);
                return new BarHud[] { new BarHud(background, new[] { foreground }, DrawBorder, Anchor, new[] { MudraLabelConfig }, actor) };
            }
            else
            {
                bars = BarHud.GetChunkedBars(
                                    2,
                                    MudraBarChunkPadding,
                                    Size,
                                    current,
                                    max,
                                    min,
                                    DrawBorder,
                                    Anchor,
                                    FillDirection,
                                    MudraLabelConfig,
                                    BackgroundColor,
                                    GlobalColors.Instance.PartialFillColor,
                                    FillColor);
            }

            return bars;
        }

        public (bool, bool, bool) GetMudraBuffs(PlayerCharacter? player, out Status? ninjutsuBuff, out Status? kassatsuBuff, out Status? tcjBuff)
        {
            ninjutsuBuff = null;
            kassatsuBuff = null;
            tcjBuff = null;

            if (player is not null)
            {
                foreach (Status status in player.StatusList)
                {
                    if (status.StatusId == 496) { ninjutsuBuff = status; }
                    if (status.StatusId == 497) { kassatsuBuff = status; }
                    if (status.StatusId == 1186) { tcjBuff = status; }
                }
            }

            return (ninjutsuBuff is not null, kassatsuBuff is not null, tcjBuff is not null);
        }
    }
}
