using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.Jobs
{
    public class NinjaHud : JobHud
    {
        private new NinjaConfig Config => (NinjaConfig)_config;

        public NinjaHud(NinjaConfig config, string? displayName = null) : base(config, displayName) { }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.KazematoiBar.Enabled)
            {
                positions.Add(Config.Position + Config.KazematoiBar.Position);
                sizes.Add(Config.KazematoiBar.Size);
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

            if (Config.ShadowWalkerBar.Enabled)
            {
                positions.Add(Config.Position + Config.ShadowWalkerBar.Position);
                sizes.Add(Config.ShadowWalkerBar.Size);
            }

            if (Config.MudraBar.Enabled)
            {
                positions.Add(Config.Position + Config.MudraBar.Position);
                sizes.Add(Config.MudraBar.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, IPlayerCharacter player)
        {
            var pos = origin + Config.Position;
            if (Config.MudraBar.Enabled)
            {
                DrawMudraBars(pos, player);
            }

            if (Config.KazematoiBar.Enabled)
            {
                DrawKazematoiBar(pos, player);
            }

            if (Config.NinkiBar.Enabled)
            {
                DrawNinkiGauge(pos, player);
            }

            if (Config.TrickAttackBar.Enabled)
            {
                DrawTrickAttackBar(pos, player);
            }

            if (Config.ShadowWalkerBar.Enabled)
            {
                DrawShadowWalkerBar(pos, player);
            }
        }

        public (bool, bool, bool) GetMudraBuffs(IPlayerCharacter? player, out Status? ninjutsuBuff, out Status? kassatsuBuff, out Status? tcjBuff)
        {
            ninjutsuBuff = null;
            kassatsuBuff = null;
            tcjBuff = null;

            if (player is not null)
            {
                var statusList = Utils.StatusListForBattleChara(player);
                foreach (Status status in statusList)
                {
                    if (status.StatusId == 496) { ninjutsuBuff = status; }
                    if (status.StatusId == 497) { kassatsuBuff = status; }
                    if (status.StatusId == 1186) { tcjBuff = status; }
                }
            }

            return (ninjutsuBuff is not null, kassatsuBuff is not null, tcjBuff is not null);
        }

        private void DrawMudraBars(Vector2 origin, IPlayerCharacter player)
        {
            var (hasNinjutsuBuff, hasKassatsuBuff, hasTCJBuff) = GetMudraBuffs(player, out Status? ninjutsuBuff, out Status? kassatsuBuff, out Status? tcjBuff);

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
                    Config.MudraBar.Label.SetText(GenerateNinjutsuText(tcjBuff?.StackCount ?? 0, hasKassatsuBuff, hasTCJBuff));
                }
                else if (hasKassatsuBuff)
                {
                    max = 15f;
                    current = kassatsuBuff is null || kassatsuBuff.RemainingTime < 0 ? max : kassatsuBuff.RemainingTime;
                    Config.MudraBar.Label.SetText("KASSATSU");
                }

                if (inNinjutsu)
                {
                    max = 6f;
                    current = ninjutsuBuff is null || ninjutsuBuff.RemainingTime < 0 ? max : ninjutsuBuff.RemainingTime;
                    Config.MudraBar.Label.SetText(GenerateNinjutsuText(ninjutsuBuff?.StackCount ?? 0, hasKassatsuBuff, hasTCJBuff));
                }

                PluginConfigColor fillColor = hasTCJBuff ? Config.MudraBar.TCJBarColor : hasKassatsuBuff ? Config.MudraBar.KassatsuBarColor : Config.MudraBar.FillColor;
                Rect foreground = BarUtilities.GetFillRect(Config.MudraBar.Position, Config.MudraBar.Size, Config.MudraBar.FillDirection, fillColor, current, max);

                BarHud bar = new BarHud(Config.MudraBar, player);
                bar.AddForegrounds(foreground);
                bar.AddLabels(Config.MudraBar.Label);

                AddDrawActions(bar.GetDrawActions(origin, Config.MudraBar.StrataLevel));
            }
            else
            {
                max = 40f;
                current = max - mudraCooldown;

                if (!Config.MudraBar.HideWhenInactive || current < max)
                {
                    Config.MudraBar.Label.SetValue((max - current) % 20);

                    BarHud[] bars = BarUtilities.GetChunkedProgressBars(Config.MudraBar, 2, current, max, 0f, player);
                    foreach (BarHud bar in bars)
                    {
                        AddDrawActions(bar.GetDrawActions(origin, Config.MudraBar.StrataLevel));
                    }
                }
            }
        }

        private unsafe void DrawKazematoiBar(Vector2 origin, IPlayerCharacter player)
        {
            NinjaGauge* gauge = (NinjaGauge*)Plugin.JobGauges.Get<NINGauge>().Address;
            int stacks = gauge->Kazematoi;

            if (Config.KazematoiBar.HideWhenInactive && stacks == 0)
            {
                return;
            }

            BarHud[] bars = BarUtilities.GetChunkedBars(Config.KazematoiBar, 5, stacks, 5, 0, player);
            foreach (BarHud bar in bars)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.KazematoiBar.StrataLevel));
            }
        }

        private unsafe void DrawNinkiGauge(Vector2 origin, IPlayerCharacter player)
        {
            NinjaGauge* gauge = (NinjaGauge*)Plugin.JobGauges.Get<NINGauge>().Address;
            int ninki = gauge->Ninki;

            if (Config.NinkiBar.HideWhenInactive && ninki == 0)
            {
                return;
            }

            Config.NinkiBar.Label.SetValue(ninki);

            BarHud[] bars = BarUtilities.GetChunkedProgressBars(Config.NinkiBar, 2, ninki, 100, 0, player);
            foreach (BarHud bar in bars)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.NinkiBar.StrataLevel));
            }
        }

        private void DrawTrickAttackBar(Vector2 origin, IPlayerCharacter player)
        {
            IGameObject? actor = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;

            float trickDuration = Utils.StatusListForActor(actor).FirstOrDefault(
                o => o.StatusId is 3254 or 3906 && o.SourceId == player.GameObjectId && o.RemainingTime > 0
            )?.RemainingTime ?? 0f;

            if (Config.TrickAttackBar.HideWhenInactive && trickDuration == 0)
            {
                return;
            }

            Config.TrickAttackBar.Label.SetValue(trickDuration);

            BarHud bar = BarUtilities.GetProgressBar(Config.TrickAttackBar, trickDuration, 15f, 0f, player);
            AddDrawActions(bar.GetDrawActions(origin, Config.TrickAttackBar.StrataLevel));
        }

        private void DrawShadowWalkerBar(Vector2 origin, IPlayerCharacter player)
        {
            float shadowwalkerDuration = Utils.StatusListForBattleChara(player).FirstOrDefault(
                o => o.StatusId == 3848 && o.RemainingTime > 0
            )?.RemainingTime ?? 0f;

            if (Config.ShadowWalkerBar.HideWhenInactive && shadowwalkerDuration == 0)
            {
                return;
            }

            Config.ShadowWalkerBar.Label.SetValue(shadowwalkerDuration);

            BarHud bar = BarUtilities.GetProgressBar(Config.ShadowWalkerBar, shadowwalkerDuration, 20f, 0f, player);
            AddDrawActions(bar.GetDrawActions(origin, Config.ShadowWalkerBar.StrataLevel));
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

        public new static NinjaConfig DefaultConfig()
        {
            var config = new NinjaConfig();

            config.MudraBar.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.MudraBar.LabelMode = LabelMode.ActiveChunk;

            config.TrickAttackBar.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.TrickAttackBar.Enabled = false;

            config.ShadowWalkerBar.Label.FontID = FontsConfig.DefaultMediumFontKey;
            config.ShadowWalkerBar.Enabled = false;

            config.NinkiBar.UsePartialFillColor = true;

            return config;
        }

        [NestedConfig("Mudra Bar", 30)]
        public NinjaMudraBarConfig MudraBar = new NinjaMudraBarConfig(
            new(0, -50),
            new(254, 10),
            new PluginConfigColor(new Vector4(211f / 255f, 166f / 255f, 75f / 242f, 100f / 100f))
        );

        [NestedConfig("Kazematoi Bar", 35)]
        public ChunkedBarConfig KazematoiBar = new ChunkedBarConfig(
            new(0, -10),
            new(254, 20),
            PluginConfigColor.FromHex(0xFFABB6BD)
        );


        [NestedConfig("Ninki Bar", 40)]
        public ChunkedProgressBarConfig NinkiBar = new ChunkedProgressBarConfig(
            new(0, -32),
            new(254, 20),
            new PluginConfigColor(new Vector4(137f / 255f, 82f / 255f, 236f / 255f, 100f / 100f))
        );

        [NestedConfig("Trick Attack Bar", 45)]
        public ProgressBarConfig TrickAttackBar = new ProgressBarConfig(
            new(0, -63),
            new(254, 10),
            new PluginConfigColor(new Vector4(191f / 255f, 40f / 255f, 0f / 255f, 100f / 100f))
        );

        [NestedConfig("Shadow Walker Bar", 50)]
        public ProgressBarConfig ShadowWalkerBar = new ProgressBarConfig(
            new(0, -75),
            new(254, 10),
            new PluginConfigColor(new Vector4(202f / 255f, 228f / 255f, 246f / 242f, 100f / 100f))
        );
    }

    [Exportable(false)]
    public class NinjaMudraBarConfig : ChunkedProgressBarConfig
    {
        [ColorEdit4("Kassatsu Color", spacing = true)]
        [Order(60)]
        public PluginConfigColor KassatsuBarColor = new(new Vector4(239 / 255f, 123 / 255f, 222 / 242f, 100f / 100f));

        [ColorEdit4("Ten Chi Jin Color")]
        [Order(65)]
        public PluginConfigColor TCJBarColor = new(new Vector4(181 / 255f, 33 / 255f, 41 / 242f, 100f / 100f));

        public NinjaMudraBarConfig(Vector2 position, Vector2 size, PluginConfigColor fillColor) : base(position, size, fillColor, 2)
        {
            Label.Enabled = true;
            UsePartialFillColor = true;
        }
    }
}
