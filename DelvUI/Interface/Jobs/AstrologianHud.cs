using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using FFXIVClientStructs.FFXIV.Client.Game;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.Jobs
{
    public class AstrologianHud : JobHud
    {
        private readonly SpellHelper _spellHelper = new();
        private new AstrologianConfig Config => (AstrologianConfig)_config;
        private static PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;

        private static readonly List<uint> DotIDs = new() { 1881, 843, 838 };
        private static readonly List<float> DotDuration = new() { 30f, 30f, 18f };
        private const float STAR_MAX_DURATION = 10f;
        private const float LIGHTSPEED_MAX_DURATION = 15f;

        public AstrologianHud(JobConfig config, string? displayName = null) : base(config, displayName)
        {
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new();
            List<Vector2> sizes = new();

            if (Config.CardsBar.Enabled)
            {
                positions.Add(Config.Position + Config.CardsBar.Position);
                sizes.Add(Config.CardsBar.Size);
            }

            if (Config.DotBar.Enabled)
            {
                positions.Add(Config.Position + Config.DotBar.Position);
                sizes.Add(Config.DotBar.Size);
            }

            if (Config.StarBar.Enabled)
            {
                positions.Add(Config.Position + Config.StarBar.Position);
                sizes.Add(Config.StarBar.Size);
            }

            if (Config.LightspeedBar.Enabled)
            {
                positions.Add(Config.Position + Config.LightspeedBar.Position);
                sizes.Add(Config.LightspeedBar.Size);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, IPlayerCharacter player)
        {
            Vector2 pos = origin + Config.Position;

            if (Config.CardsBar.Enabled)
            {
                DrawCardsBar(pos, player);
            }
            if (Config.DotBar.Enabled)
            {
                DrawDot(pos, player);
            }

            if (Config.LightspeedBar.Enabled)
            {
                DrawLightspeed(pos, player);
            }

            if (Config.StarBar.Enabled)
            {
                DrawStar(pos, player);
            }
        }

        private unsafe void DrawCardsBar(Vector2 origin, IPlayerCharacter player)
        {
            AstrologianCardsBarConfig config = Config.CardsBar;
            PluginConfigColor emptyColor = PluginConfigColor.Empty;

            List<Tuple<PluginConfigColor, float, LabelConfig?>> chunks = new();
            
            uint play1 = ActionManager.Instance()->GetAdjustedActionId(37019);
            PluginConfigColor play1Color = play1 == 37023 ? config.TheBalanceColor : (play1 == 37026 ? config.TheSpearColor : emptyColor);
            chunks.Add(new(play1Color, 1, null));

            uint play2 = ActionManager.Instance()->GetAdjustedActionId(37020);
            PluginConfigColor play2Color = play2 == 37024 ? config.TheArrowColor : (play2 == 37027 ? config.TheBoleColor : emptyColor);
            chunks.Add(new(play2Color, 1, null));

            uint play3 = ActionManager.Instance()->GetAdjustedActionId(37021);
            PluginConfigColor play3Color = play3 == 37025 ? config.TheSpireColor : (play3 == 37028 ? config.TheEwerColor : emptyColor);
            chunks.Add(new(play3Color, 1, null));

            if (player.Level >= 70)
            {
                uint minorArcana = ActionManager.Instance()->GetAdjustedActionId(37022);
                PluginConfigColor minorArcanaColor = minorArcana == 7444 ? config.TheLordOfCrownsColor : (minorArcana == 7445 ? config.TheLadyOfCrownsColor : emptyColor);
                chunks.Add(new(minorArcanaColor, 1, null));
            }

            BarHud[] bars = BarUtilities.GetChunkedBars(config, chunks.ToArray(), player);
            foreach (BarHud bar in bars)
            {
                AddDrawActions(bar.GetDrawActions(origin, config.StrataLevel));
            }
        }

        private void DrawDot(Vector2 origin, IPlayerCharacter player)
        {
            IGameObject? target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            BarHud? bar = BarUtilities.GetDoTBar(Config.DotBar, player, target, DotIDs, DotDuration);
            if (bar != null)
            {
                AddDrawActions(bar.GetDrawActions(origin, Config.DotBar.StrataLevel));
            }
        }

        private void DrawLightspeed(Vector2 origin, IPlayerCharacter player)
        {
            float lightspeedDuration = Utils.StatusListForBattleChara(player).FirstOrDefault(o => o.StatusId is 841 && o.SourceId == player.GameObjectId)?.RemainingTime ?? 0f;

            if (Config.LightspeedBar.HideWhenInactive && lightspeedDuration <= 0)
            {
                return;
            }

            Config.LightspeedBar.Label.SetValue(lightspeedDuration);

            BarHud bar = BarUtilities.GetProgressBar(Config.LightspeedBar, lightspeedDuration, LIGHTSPEED_MAX_DURATION, 0, player);
            AddDrawActions(bar.GetDrawActions(origin, Config.LightspeedBar.StrataLevel));
        }

        private void DrawStar(Vector2 origin, IPlayerCharacter player)
        {
            float starPreCookingBuff = Utils.StatusListForBattleChara(player).FirstOrDefault(o => o.StatusId is 1224 && o.SourceId == player.GameObjectId)?.RemainingTime ?? 0f;
            float starPostCookingBuff = Utils.StatusListForBattleChara(player).FirstOrDefault(o => o.StatusId is 1248 && o.SourceId == player.GameObjectId)?.RemainingTime ?? 0f;

            if (Config.StarBar.HideWhenInactive && starPostCookingBuff <= 0f && starPreCookingBuff <= 0f)
            {
                return;
            }

            float currentStarDuration = starPreCookingBuff > 0 ? STAR_MAX_DURATION - Math.Abs(starPreCookingBuff) : Math.Abs(starPostCookingBuff);
            PluginConfigColor currentStarColor = starPreCookingBuff > 0 ? Config.StarBar.StarEarthlyColor : Config.StarBar.StarGiantColor;

            Config.StarBar.Label.SetValue(currentStarDuration);

            // Star Countdown after Star is ready 
            BarHud bar = BarUtilities.GetProgressBar(Config.StarBar, currentStarDuration, STAR_MAX_DURATION, 0f, player, currentStarColor, Config.StarBar.StarGlowConfig.Enabled && starPostCookingBuff > 0 ? Config.StarBar.StarGlowConfig : null);
            AddDrawActions(bar.GetDrawActions(origin, Config.StarBar.StrataLevel));
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Healer", 0)]
    [SubSection("Astrologian", 1)]
    public class AstrologianConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.AST;

        public new static AstrologianConfig DefaultConfig()
        {
            var config = new AstrologianConfig();
            config.UseDefaultPrimaryResourceBar = true;

            return config;
        }

        [NestedConfig("Cards Bar", 40)]
        public AstrologianCardsBarConfig CardsBar = new(
            new Vector2(0, 0),
            new Vector2(254, 20)
        );

        [NestedConfig("Dot Bar", 40)]
        public ProgressBarConfig DotBar = new(
            new Vector2(-85, -29),
            new Vector2(84, 14),
            new PluginConfigColor(new Vector4(20f / 255f, 80f / 255f, 168f / 255f, 255f / 100f))
        );

        [NestedConfig("Star Bar", 45)]
        public AstrologianStarBarConfig StarBar = new(
            new Vector2(0, -29),
            new Vector2(84, 14)
        );

        [NestedConfig("Lightspeed Bar", 50)]
        public ProgressBarConfig LightspeedBar = new(
            new Vector2(85, -29),
            new Vector2(84, 14),
            new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 173f / 255f, 100f / 100f))
        );
    }

    [Exportable(false)]
    [DisableParentSettings("FillColor", "UsePartialFillColor", "UseChunks", "PartialFillColor", "LabelMode", "HideWhenInactive")]
    public class AstrologianCardsBarConfig : ChunkedBarConfig
    {
        [ColorEdit4("The Balance Color", spacing = true)]
        [Order(201)]
        public PluginConfigColor TheBalanceColor = PluginConfigColor.FromHex(0xFFBE423F);

        [ColorEdit4("The Arrow Color")]
        [Order(201)]
        public PluginConfigColor TheArrowColor = PluginConfigColor.FromHex(0xFF628AA7);

        [ColorEdit4("The Spire Color")]
        [Order(201)]
        public PluginConfigColor TheSpireColor = PluginConfigColor.FromHex(0xFFC8A348);

        [ColorEdit4("The Spear Color")]
        [Order(201)]
        public PluginConfigColor TheSpearColor = PluginConfigColor.FromHex(0xFF5673DF);

        [ColorEdit4("The Bole Color")]
        [Order(201)]
        public PluginConfigColor TheBoleColor = PluginConfigColor.FromHex(0xFF9ACB77);

        [ColorEdit4("The Ewer Color")]
        [Order(201)]
        public PluginConfigColor TheEwerColor = PluginConfigColor.FromHex(0xFF7FBDFF);

        [ColorEdit4("The Lord of Crowns Color")]
        [Order(201)]
        public PluginConfigColor TheLordOfCrownsColor = PluginConfigColor.FromHex(0xFFCA3640);

        [ColorEdit4("The Lady of Crowns Color")]
        [Order(201)]
        public PluginConfigColor TheLadyOfCrownsColor = PluginConfigColor.FromHex(0xFF974A97);

        public AstrologianCardsBarConfig(Vector2 position, Vector2 size)
            : base(position, size, PluginConfigColor.Empty)
        {
        }
    }

    [Exportable(false)]
    [DisableParentSettings("FillColor")]
    public class AstrologianStarBarConfig : ProgressBarConfig
    {
        [ColorEdit4("Earthly" + "##Star")]
        [Order(402)]
        public PluginConfigColor StarEarthlyColor = new(new Vector4(37f / 255f, 181f / 255f, 177f / 255f, 100f / 100f));

        [ColorEdit4("Giant" + "##Star")]
        [Order(403)]
        public PluginConfigColor StarGiantColor = new(new Vector4(198f / 255f, 154f / 255f, 199f / 255f, 100f / 100f));

        [NestedConfig("Giant Dominance Glow" + "##Star", 404, separator = false, spacing = true)]
        public BarGlowConfig StarGlowConfig = new();
        public AstrologianStarBarConfig(Vector2 position, Vector2 size)
            : base(position, size, PluginConfigColor.Empty)
        {
        }
    }
}
