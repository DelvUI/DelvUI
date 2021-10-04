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

        public NinjaHud(string id, NinjaConfig config, string? displayName = null) : base(id, config, displayName)
        {

        }

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

            if (Config.TrickAttackBarConfig.Enabled)
            {
                positions.Add(Config.Position + Config.TrickAttackBarConfig.Position);
                sizes.Add(Config.TrickAttackBarConfig.Size);
            }

            if (Config.SuitonBarConfig.Enabled)
            {
                positions.Add(Config.Position + Config.SuitonBarConfig.Position);
                sizes.Add(Config.SuitonBarConfig.Size);
            }

            if (Config.MudraBarConfig.Enabled)
            {
                positions.Add(Config.Position + Config.MudraBarConfig.Position);
                sizes.Add(Config.MudraBarConfig.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            if (Config.MudraBarConfig.Enabled)
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

            if (Config.TrickAttackBarConfig.Enabled)
            {
                DrawTrickAttackBar(origin, player);
            }

            if (Config.SuitonBarConfig.Enabled)
            {
                DrawSuitonBar(origin, player);
            }
        }

        private void DrawMudraBars(Vector2 origin, PlayerCharacter player)
        {
            var (hasNinjutsuBuff, hasKassatsuBuff, hasTCJBuff) =
                Config.MudraBarConfig.GetMudraBuffs(player, out Status? ninjutsuBuff, out Status? kassatsuBuff, out Status? tcjBuff);

            BarHud mudraBar = new BarHud(Config.MudraBarConfig, player, Config.MudraBarConfig.MudraLabelConfig);
            float current = 0f;
            float mid = 0f;
            float max = 0f;
            int chunks = 1;

            if (hasTCJBuff || hasKassatsuBuff || hasNinjutsuBuff)
            { 
                if (hasTCJBuff)
                {
                    max = 6f;
                    current = tcjBuff is null || tcjBuff.RemainingTime < 0 ? max : tcjBuff.RemainingTime;
                    Config.MudraBarConfig.MudraLabelConfig.SetText(GenerateNinjutsuText(tcjBuff?.StackCount ?? 0, hasKassatsuBuff, hasTCJBuff));
                }
                else if (hasKassatsuBuff)
                {
                    max = 15f;
                    current = kassatsuBuff is null || kassatsuBuff.RemainingTime < 0 ? max : kassatsuBuff.RemainingTime;
                    Config.MudraBarConfig.MudraLabelConfig.SetText("KASSATSU");
                }

                if (hasNinjutsuBuff)
                {
                    max = 6f;
                    current = ninjutsuBuff is null || ninjutsuBuff.RemainingTime < 0 ? max : ninjutsuBuff.RemainingTime;
                    Config.MudraBarConfig.MudraLabelConfig.SetText(GenerateNinjutsuText(ninjutsuBuff?.StackCount ?? 0, hasKassatsuBuff, hasTCJBuff));
                }
            }
            else
            {
                max = 40f;
                mid = max - SpellHelper.Instance.GetSpellCooldown(2259);
                current = (float)Math.Floor(mid / 20f) * 20f;
                chunks = 2;
                Config.MudraBarConfig.MudraLabelConfig.SetText(((max - mid) % 20).ToString("N0"));
            }

            mudraBar.Draw(origin + Config.Position, current, max, mid, chunks, Config.MudraBarConfig.MudraBarChunkPadding);
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

        private void DrawHutonGauge(Vector2 origin)
        {
            NINGauge gauge = Plugin.JobGauges.Get<NINGauge>();
            float hutonDurationLeft = gauge.HutonTimer / 1000f;
            BarHud hutonBar = new BarHud(Config.HutonBarConfig, Config.HutonBarConfig.LabelConfig);
            Config.HutonBarConfig.LabelConfig.SetText(hutonDurationLeft.ToString("N0"));
            hutonBar.Draw(origin + Config.Position, hutonDurationLeft, 70f);
        }

        private void DrawNinkiGauge(Vector2 origin)
        {
            NINGauge gauge = Plugin.JobGauges.Get<NINGauge>();
            BarHud ninkiBar = new BarHud(Config.NinkiBarConfig, Config.NinkiBarConfig.LabelConfig);
            Config.NinkiBarConfig.LabelConfig.SetText(gauge.Ninki.ToString("N0"));
            ninkiBar.Draw(origin + Config.Position, gauge.Ninki, 100f);
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

            BarHud trickBar = new BarHud(Config.TrickAttackBarConfig, Config.TrickAttackBarConfig.LabelConfig);
            Config.TrickAttackBarConfig.LabelConfig.SetText(trickDuration.ToString("N0"));
            trickBar.Draw(origin + Config.Position, trickDuration, trickMaxDuration);
        }

        private void DrawSuitonBar(Vector2 origin, PlayerCharacter player)
        {
            float suitonDuration = player.StatusList.Where(o => o.StatusId == 507).Select(o => Math.Abs(o.RemainingTime)).FirstOrDefault();
            BarHud suitonBar = new BarHud(Config.SuitonBarConfig, Config.SuitonBarConfig.LabelConfig);
            Config.SuitonBarConfig.LabelConfig.SetText(suitonDuration.ToString("N0"));
            suitonBar.Draw(origin + Config.Position, suitonDuration, 20f);
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
            TrickAttackBarConfig.Enabled = false;
            SuitonBarConfig.Enabled = false;
            HutonBarConfig.Threshold = true;
            HutonBarConfig.UseThresholdColor = true;
        }

        public new static NinjaConfig DefaultConfig() { return new NinjaConfig(); }


        [NestedConfig("Mudra Bar", 30)]
        public MudraBarConfig MudraBarConfig = new MudraBarConfig(
                                                            new(0, -50),
                                                            new(254, 10),
                                                            new PluginConfigColor(new Vector4(211 / 255f, 166 / 255f, 75 / 242f, 100f / 100f)));

        [NestedConfig("Huton Bar", 35, separator = true)]
        public ThresholdBarConfig HutonBarConfig = new ThresholdBarConfig(
                                                            new(0, -10),
                                                            new(254, 20),
                                                            new PluginConfigColor(new Vector4(110f / 255f, 197f / 255f, 207f / 255f, 100f / 100f)),
                                                            new PluginConfigColor(new Vector4(230f / 255f, 33f / 255f, 33f / 255f, 53f / 100f)),
                                                            40f);

        [NestedConfig("Ninki Bar", 40, separator = true)]
        public ThresholdBarConfig NinkiBarConfig = new ThresholdBarConfig(
                                                            new(0, -32), 
                                                            new(254, 20), 
                                                            new PluginConfigColor(new Vector4(137f / 255f, 82f / 255f, 236f / 255f, 100f / 100f)));

        [NestedConfig("Trick Attack Bar", 45, separator = true)]
        public ThresholdBarConfig TrickAttackBarConfig = new ThresholdBarConfig(
                                                            new(0, -63),
                                                            new(254, 10),
                                                            new PluginConfigColor(new Vector4(191f / 255f, 40f / 255f, 0f / 255f, 100f / 100f)));

        [NestedConfig("Suiton Bar", 50, separator = true)]
        public ThresholdBarConfig SuitonBarConfig = new ThresholdBarConfig(
                                                            new(0, -75),
                                                            new(254, 10),
                                                            new PluginConfigColor(new Vector4(202f / 255f, 228f / 255f, 246f / 242f, 100f / 100f)));
    }

    [Portable(false)]
    public class MudraBarConfig : BarConfig
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

        public override PluginConfigColor GetBarColor(float current, GameObject? actor = null)
        {
            if (actor is not PlayerCharacter player)
            {
                return base.GetBarColor(current, actor);
            }

            var (hasNinjutsuBuff, hasKassatsuBuff, hasTCJBuff) = GetMudraBuffs(player, out Status? mudraBuff, out Status? kassatsuBuff, out Status? tcjBuff);

            if (hasTCJBuff)
            {
                return TCJBarColor;
            }

            if (hasKassatsuBuff)
            {
                return KassatsuBarColor;
            }

            if (hasNinjutsuBuff)
            {
                return FillColor;
            }

            return current % 20 == 0 ? FillColor : GlobalColors.Instance.PartialFillColor;
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
